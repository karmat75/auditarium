// SPDX-License-Identifier: MIT
using System.Data.Common;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

/// <summary>Provider-neutral security and audit-log quality gates required by work package 20.10.5.</summary>
public sealed class FinalHardeningIntegrationTests
{
    [Fact]
    public async Task PostgreSql_audit_log_failure_rolls_back_the_change_and_sensitive_state_is_allowlisted()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await VerifyAuditLogAndPermissionHardeningAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task SqlServer_audit_log_failure_rolls_back_the_change_and_sensitive_state_is_allowlisted()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await VerifyAuditLogAndPermissionHardeningAsync("SqlServer", container.GetConnectionString());
    }

    private static async Task VerifyAuditLogAndPermissionHardeningAsync(string provider, string connectionString)
    {
        await using var services = CreateServiceProvider(provider, connectionString);
        await services.InitializeAuditariumDatabaseAsync();

        await using (var permissionScope = services.CreateAsyncScope())
        {
            var evaluator = permissionScope.ServiceProvider.GetRequiredService<IPermissionEvaluator>();
            var db = permissionScope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
            var activeAdministratorId = await db.Users.Where(user => user.UserKey == "DEFAULT_ADMIN").Select(user => user.UserId).SingleAsync();
            Assert.Contains("Users.Manage", await evaluator.GetPermissionsAsync(activeAdministratorId));
            var systemPermissions = await evaluator.GetPermissionsAsync(0);
            Assert.Contains("Maintenance.Retention.Execute", systemPermissions);
            Assert.DoesNotContain("Users.Manage", systemPermissions);
        }

        var interceptor = new FailAuditLogInsertInterceptor();
        await using (var faulting = new AuditariumDbContext(CreateOptions(provider, connectionString, interceptor)))
        {
            var administrator = await faulting.Users.SingleAsync(user => user.UserKey == "DEFAULT_ADMIN");
            administrator.IsActive = false;
            interceptor.Enabled = true;
            Assert.NotNull(await Record.ExceptionAsync(() => faulting.SaveChangesAsync()));
        }

        const string sensitiveEmail = "sensitive-user@example.invalid";
        const string sensitiveDisplayName = "Sensitive User Name";
        const string sensitivePasswordHash = "sensitive-password-hash";
        long administratorId;
        await using (var verification = new AuditariumDbContext(CreateOptions(provider, connectionString)))
        {
            var administrator = await verification.Users.SingleAsync(user => user.UserKey == "DEFAULT_ADMIN");
            administratorId = administrator.UserId;
            Assert.True(administrator.IsActive);
            Assert.False(await verification.SystemAuditLogs.AnyAsync(entry =>
                entry.ObjectType == "User" && entry.ObjectId == administrator.UserId && entry.Action == "UPDATED"));

            var credential = await verification.LocalCredentials.SingleAsync();
            administrator.Email = sensitiveEmail;
            administrator.DisplayName = sensitiveDisplayName;
            administrator.IsActive = false;
            credential.PasswordHash = sensitivePasswordHash;
            await verification.SaveChangesAsync();

            var states = await verification.SystemAuditLogs
                .Where(entry => entry.ObjectId == administrator.UserId || entry.ObjectType == "LocalCredential")
                .Select(entry => (entry.BeforeState ?? string.Empty) + (entry.AfterState ?? string.Empty))
                .ToListAsync();
            Assert.Contains(states, state => state.Contains("is_active", StringComparison.Ordinal));
            Assert.DoesNotContain(states, state =>
                state.Contains(sensitiveEmail, StringComparison.Ordinal) ||
                state.Contains(sensitiveDisplayName, StringComparison.Ordinal) ||
                state.Contains(sensitivePasswordHash, StringComparison.Ordinal) ||
                state.Contains("password_hash", StringComparison.OrdinalIgnoreCase));
        }

        // A second service provider represents another application instance and must see deactivation immediately.
        await using var secondInstance = CreateServiceProvider(provider, connectionString);
        await using var secondScope = secondInstance.CreateAsyncScope();
        var secondEvaluator = secondScope.ServiceProvider.GetRequiredService<IPermissionEvaluator>();
        Assert.Empty(await secondEvaluator.GetPermissionsAsync(administratorId));
        Assert.False(await secondEvaluator.HasPermissionAsync(administratorId, "Users.Manage"));
    }

    private static DbContextOptions<AuditariumDbContext> CreateOptions(string provider, string connectionString, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<AuditariumDbContext>();
        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            options.UseNpgsql(connectionString, sql => sql.MigrationsAssembly("Auditarium.Dal.PostgreSql.Migrations").MigrationsHistoryTable("__EFMigrationsHistory", "auditarium"));
        else
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("Auditarium.Dal.SqlServer.Migrations").MigrationsHistoryTable("__EFMigrationsHistory", "auditarium"));
        options.AddInterceptors(interceptors);
        return options.Options;
    }

    private static ServiceProvider CreateServiceProvider(string provider, string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auditarium:Database:Provider"] = provider,
            ["Auditarium:Database:ConnectionString"] = connectionString,
            ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180"
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddAuditariumPersistence(configuration);
        return services.BuildServiceProvider();
    }

    private sealed class FailAuditLogInsertInterceptor : DbCommandInterceptor
    {
        public bool Enabled { get; set; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (Enabled && command.CommandText.Contains("INSERT", StringComparison.OrdinalIgnoreCase) &&
                command.CommandText.Contains("system_audit_log", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Injected audit-log write failure.");
            return ValueTask.FromResult(result);
        }
    }
}
