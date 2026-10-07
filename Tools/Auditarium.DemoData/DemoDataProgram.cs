// SPDX-License-Identifier: MIT
using Auditarium.Dal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auditarium.DemoData;

public static class DemoDataProgram
{
    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var environment = ResolveEnvironment();
        var configuration = BuildConfiguration(environment);
        return await RunAsync(args, environment, configuration, CreateServiceProvider, Console.Out, Console.Error, cancellationToken);
    }

    public static async Task<int> RunAsync(
        string[] args,
        string? environment,
        IConfiguration configuration,
        Func<IConfiguration, IServiceProvider> serviceProviderFactory,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken = default)
    {
        var gate = DemoDataInvocationGate.Evaluate(args, environment, configuration["Auditarium:DemoData:Enabled"]);
        if (!gate.IsAllowed)
        {
            await error.WriteLineAsync(gate.Message);
            await error.WriteLineAsync("Usage: dotnet run --project Tools/Auditarium.DemoData/Auditarium.DemoData.csproj -- apply --confirm");
            return 2;
        }

        try
        {
            await using var provider = serviceProviderFactory(configuration).AsAsyncDisposable();
            await using var scope = provider.ServiceProvider.CreateAsyncScope();
            var provisioner = new DevelopmentAdministratorProvisioner(scope.ServiceProvider.GetRequiredService<AuditariumDbContext>());
            await provisioner.ProvisionAsync(cancellationToken);
            await output.WriteLineAsync("Development administrator roles were provisioned.");
            return 0;
        }
        catch (DemoDataProvisioningException exception)
        {
            await error.WriteLineAsync(exception.Message);
            return 1;
        }
        catch (Exception)
        {
            await error.WriteLineAsync("Development administrator provisioning failed.");
            return 1;
        }
    }

    private static IServiceProvider CreateServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddAuditariumPersistence(configuration);
        return services.BuildServiceProvider();
    }

    private static string? ResolveEnvironment()
    {
        var dotnetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var aspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.IsNullOrWhiteSpace(dotnetEnvironment) ? aspNetCoreEnvironment : dotnetEnvironment;
    }

    private static IConfiguration BuildConfiguration(string? environment)
    {
        var root = FindRepositoryRoot();
        var webConfigurationDirectory = Path.Combine(root, "UI", "Auditarium.Web");
        return new ConfigurationBuilder()
            .SetBasePath(webConfigurationDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string FindRepositoryRoot()
    {
        foreach (var startingDirectory in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            for (var directory = new DirectoryInfo(startingDirectory); directory is not null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "Auditarium.sln"))) return directory.FullName;
        throw new DirectoryNotFoundException("Auditarium.DemoData must be run from a cloned Auditarium repository.");
    }

    private sealed class AsyncDisposableServiceProvider(IServiceProvider serviceProvider) : IAsyncDisposable
    {
        public IServiceProvider ServiceProvider { get; } = serviceProvider;

        public ValueTask DisposeAsync() => ServiceProvider switch
        {
            IAsyncDisposable asyncDisposable => asyncDisposable.DisposeAsync(),
            IDisposable disposable => Dispose(disposable),
            _ => ValueTask.CompletedTask
        };

        private static ValueTask Dispose(IDisposable disposable)
        {
            disposable.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private static AsyncDisposableServiceProvider AsAsyncDisposable(this IServiceProvider serviceProvider) => new(serviceProvider);
}
