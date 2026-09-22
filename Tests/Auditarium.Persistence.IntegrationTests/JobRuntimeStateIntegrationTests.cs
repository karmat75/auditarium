// SPDX-License-Identifier: MIT
using Auditarium.Bll.Jobs;
using Auditarium.Common.Time;
using Auditarium.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class JobRuntimeStateIntegrationTests
{
    [Fact]
    public async Task PostgreSql_runtime_lease_is_atomic_recovers_and_records_startup_once()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await VerifyRuntimeLeaseSemanticsAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task SqlServer_runtime_lease_is_atomic_recovers_and_records_startup_once()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await VerifyRuntimeLeaseSemanticsAsync("SqlServer", container.GetConnectionString());
    }

    private static async Task VerifyRuntimeLeaseSemanticsAsync(string provider, string connectionString)
    {
        var clock = new MutableClock(new DateTimeOffset(2026, 9, 22, 8, 0, 0, TimeSpan.Zero));
        await using var services = CreateServiceProvider(provider, connectionString, clock);
        await services.InitializeAuditariumDatabaseAsync();

        var concurrentAcquisitions = await Task.WhenAll(
            AcquireAsync(services, "Atomic", "instance-a", JobTrigger.Scheduled, "1.0.0"),
            AcquireAsync(services, "Atomic", "instance-b", JobTrigger.Scheduled, "1.0.0"));
        Assert.Equal(1, concurrentAcquisitions.Count(acquired => acquired));

        Assert.True(await AcquireAsync(services, "Retention", "instance-a", JobTrigger.Scheduled, "1.0.0"));
        Assert.False(await AcquireAsync(services, "Retention", "instance-b", JobTrigger.Scheduled, "1.0.0"));

        clock.UtcNow = clock.UtcNow.AddMinutes(4);
        Assert.True(await HeartbeatAsync(services, "Retention", "instance-a"));
        clock.UtcNow = clock.UtcNow.AddMinutes(2);
        Assert.False(await AcquireAsync(services, "Retention", "instance-b", JobTrigger.Scheduled, "1.0.0"));

        await CompleteAsync(services, "Retention", "instance-a", JobTrigger.Scheduled, "1.0.0", true, "Succeeded");
        await using (var scope = services.CreateAsyncScope())
        {
            var state = await scope.ServiceProvider.GetRequiredService<AuditariumDbContext>().JobRuntimeStates.SingleAsync(x => x.JobKey == "Retention");
            Assert.Null(state.RunningInstanceId);
            Assert.Null(state.LeaseUntil);
            Assert.Equal("Succeeded", state.LastResult);
            Assert.NotNull(state.LastCompletedAt);
        }
        Assert.True(await AcquireAsync(services, "Retention", "instance-a", JobTrigger.Startup, "1.0.0"));
        await CompleteAsync(services, "Retention", "instance-a", JobTrigger.Startup, "1.0.0", true, "Succeeded");
        Assert.False(await AcquireAsync(services, "Retention", "instance-b", JobTrigger.Startup, "1.0.0"));

        Assert.True(await AcquireAsync(services, "CrashRecovery", "instance-a", JobTrigger.Scheduled, "1.0.0"));
        clock.UtcNow = clock.UtcNow.Add(JobRuntimeStateStore.LeaseDuration).AddSeconds(1);
        Assert.True(await AcquireAsync(services, "CrashRecovery", "instance-b", JobTrigger.Scheduled, "1.0.0"));

        Assert.True(await AcquireAsync(services, "Startup", "instance-a", JobTrigger.Startup, "1.0.0"));
        await CompleteAsync(services, "Startup", "instance-a", JobTrigger.Startup, "1.0.0", true, "Succeeded");
        Assert.False(await AcquireAsync(services, "Startup", "instance-b", JobTrigger.Startup, "1.0.0"));
        Assert.True(await AcquireAsync(services, "Startup", "instance-b", JobTrigger.Startup, "1.1.0"));
    }

    private static async Task<bool> AcquireAsync(IServiceProvider services, string jobKey, string instanceId, JobTrigger trigger, string version)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<JobRuntimeStateStore>().TryAcquireAsync(jobKey, instanceId, trigger, version, CancellationToken.None);
    }

    private static async Task<bool> HeartbeatAsync(IServiceProvider services, string jobKey, string instanceId)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<JobRuntimeStateStore>().HeartbeatAsync(jobKey, instanceId, CancellationToken.None);
    }

    private static async Task CompleteAsync(IServiceProvider services, string jobKey, string instanceId, JobTrigger trigger, string version, bool succeeded, string result)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<JobRuntimeStateStore>().CompleteAsync(jobKey, instanceId, trigger, version, succeeded, result, CancellationToken.None);
    }

    private static ServiceProvider CreateServiceProvider(string provider, string connectionString, IClock clock)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auditarium:Database:Provider"] = provider,
            ["Auditarium:Database:ConnectionString"] = connectionString,
            ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180"
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(clock);
        services.AddAuditariumPersistence(configuration);
        services.AddScoped<JobRuntimeStateStore>();
        return services.BuildServiceProvider();
    }

    private sealed class MutableClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;
    }
}
