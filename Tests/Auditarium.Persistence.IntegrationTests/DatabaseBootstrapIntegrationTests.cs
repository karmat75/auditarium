// SPDX-License-Identifier: MIT
using Auditarium.Dal;
using System.Data.Common;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class DatabaseBootstrapIntegrationTests
{
    [Fact]
    public async Task PostgreSql_fresh_database_applies_migrations_and_bootstraps_once()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await VerifyFreshInstallationAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task SqlServer_fresh_database_applies_migrations_and_bootstraps_once()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await VerifyFreshInstallationAsync("SqlServer", container.GetConnectionString());
    }

    [Fact]
    public async Task PostgreSql_parallel_starts_create_one_administrator_credential()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        var first = CreateServiceProvider("PostgreSQL", container.GetConnectionString());
        var second = CreateServiceProvider("PostgreSQL", container.GetConnectionString());
        await using (first)
        await using (second)
        {
            await Task.WhenAll(first.InitializeAuditariumDatabaseAsync(), second.InitializeAuditariumDatabaseAsync());
            await using var scope = first.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
            Assert.Single(await db.Users.Where(user => user.UserKey == "DEFAULT_ADMIN").ToListAsync());
            Assert.Single(await db.LocalCredentials.ToListAsync());
        }
    }

    [Fact]
    public async Task SqlServer_parallel_starts_create_one_administrator_credential()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        var first = CreateServiceProvider("SqlServer", container.GetConnectionString());
        var second = CreateServiceProvider("SqlServer", container.GetConnectionString());
        await using (first)
        await using (second)
        {
            await Task.WhenAll(first.InitializeAuditariumDatabaseAsync(), second.InitializeAuditariumDatabaseAsync());
            await using var scope = first.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
            Assert.Single(await db.Users.Where(user => user.UserKey == "DEFAULT_ADMIN").ToListAsync());
            Assert.Single(await db.LocalCredentials.ToListAsync());
        }
    }

    [Fact]
    public async Task PostgreSql_cancelled_wait_for_bootstrap_lock_does_not_block_later_start()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await VerifyCancelledLockWaitAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task SqlServer_cancelled_wait_for_bootstrap_lock_does_not_block_later_start()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await VerifyCancelledLockWaitAsync("SqlServer", container.GetConnectionString());
    }

    [Fact]
    public async Task PostgreSql_newer_database_schema_blocks_startup()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await VerifyNewerSchemaBlocksStartupAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task SqlServer_newer_database_schema_blocks_startup()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await VerifyNewerSchemaBlocksStartupAsync("SqlServer", container.GetConnectionString());
    }

    [Fact]
    public async Task PostgreSql_recovery_restores_local_administrator_without_rehashing_matching_password()
    {
        const string recoveryPassword = "Temporary-Recovery-Password-123!";
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        var normal = CreateServiceProvider("PostgreSQL", container.GetConnectionString());
        await using (normal)
        {
            await normal.InitializeAuditariumDatabaseAsync();
        }

        var recovery = CreateServiceProvider("PostgreSQL", container.GetConnectionString(), recoveryPassword);
        await using (recovery)
        {
            await recovery.InitializeAuditariumDatabaseAsync();
            await using var scope = recovery.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
            var admin = await db.Users.SingleAsync(user => user.UserKey == "DEFAULT_ADMIN");
            var credential = await db.LocalCredentials.SingleAsync();
            var hash = credential.PasswordHash;
            admin.IsActive = false;
            credential.FailedAttemptCount = 3;
            credential.LockoutUntil = DateTimeOffset.UtcNow.AddHours(1);
            await db.SaveChangesAsync();

            await recovery.InitializeAuditariumDatabaseAsync();
            await db.Entry(admin).ReloadAsync();
            await db.Entry(credential).ReloadAsync();
            Assert.True(admin.IsActive);
            Assert.Equal(0, credential.FailedAttemptCount);
            Assert.Null(credential.LockoutUntil);
            Assert.Equal(hash, credential.PasswordHash);
            Assert.True(credential.MustChangePassword);
        }
    }

    [Fact]
    public async Task SqlServer_recovery_restores_local_administrator_without_rehashing_matching_password()
    {
        const string recoveryPassword = "Temporary-Recovery-Password-123!";
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        var normal = CreateServiceProvider("SqlServer", container.GetConnectionString());
        await using (normal)
        {
            await normal.InitializeAuditariumDatabaseAsync();
        }

        var recovery = CreateServiceProvider("SqlServer", container.GetConnectionString(), recoveryPassword);
        await using (recovery)
        {
            await recovery.InitializeAuditariumDatabaseAsync();
            await using var scope = recovery.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
            var admin = await db.Users.SingleAsync(user => user.UserKey == "DEFAULT_ADMIN");
            var credential = await db.LocalCredentials.SingleAsync();
            var hash = credential.PasswordHash;
            admin.IsActive = false;
            credential.FailedAttemptCount = 3;
            credential.LockoutUntil = DateTimeOffset.UtcNow.AddHours(1);
            await db.SaveChangesAsync();

            await recovery.InitializeAuditariumDatabaseAsync();
            await db.Entry(admin).ReloadAsync();
            await db.Entry(credential).ReloadAsync();
            Assert.True(admin.IsActive);
            Assert.Equal(0, credential.FailedAttemptCount);
            Assert.Null(credential.LockoutUntil);
            Assert.Equal(hash, credential.PasswordHash);
            Assert.True(credential.MustChangePassword);
        }
    }

    private static async Task VerifyFreshInstallationAsync(string provider, string connectionString)
    {
        await using var serviceProvider = CreateServiceProvider(provider, connectionString);
        await serviceProvider.InitializeAuditariumDatabaseAsync();
        await serviceProvider.InitializeAuditariumDatabaseAsync();

        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        Assert.NotEmpty(await db.Database.GetAppliedMigrationsAsync());
        var administrator = await db.Users.SingleAsync(user => user.UserKey == "DEFAULT_ADMIN");
        Assert.Equal("administrator", administrator.Username);
        Assert.Single(await db.Users.Where(user => user.UserKey == "DEFAULT_ADMIN").ToListAsync());
        Assert.Equal(7, await db.ApplicationSettings.CountAsync());
        var system = await db.Users.SingleAsync(user => user.UserKey == "SYSTEM");
        Assert.Equal(0, system.UserId);
        Assert.False(system.IsActive);
        Assert.Equal(21, await db.Permissions.CountAsync());
        Assert.Equal(6, await db.Roles.CountAsync());
        var systemAdmin = await db.Roles.SingleAsync(role => role.RoleKey == "SYSTEM_ADMIN");
        Assert.Equal(5, await db.RolePermissions.CountAsync(x => x.RoleId == systemAdmin.RoleId));

        administrator.IsActive = false;
        await db.SaveChangesAsync();
        var change = await db.SystemAuditLogs.OrderByDescending(entry => entry.EventId).FirstAsync();
        Assert.Equal("UPDATED", change.Action);
        Assert.Equal("User", change.ObjectType);
        Assert.Equal(administrator.UserId, change.ObjectId);
        Assert.NotNull(change.BeforeState);
        Assert.NotNull(change.AfterState);
        Assert.Contains("is_active", change.BeforeState, StringComparison.Ordinal);
        Assert.Contains("is_active", change.AfterState, StringComparison.Ordinal);
        Assert.DoesNotContain("username", change.AfterState, StringComparison.Ordinal);
        Assert.DoesNotContain("display_name", change.AfterState, StringComparison.Ordinal);
    }

    private static async Task VerifyCancelledLockWaitAsync(string provider, string connectionString)
    {
        await using var first = CreateServiceProvider(provider, connectionString);
        await first.InitializeAuditariumDatabaseAsync();
        await using var lockScope = first.CreateAsyncScope();
        var lockContext = lockScope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var heldLock = await AcquireBootstrapLockAsync(lockContext, provider);

        await using var waiting = CreateServiceProvider(provider, connectionString);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting.InitializeAuditariumDatabaseAsync(cancellation.Token));

        await heldLock.CloseAsync();
        await waiting.InitializeAuditariumDatabaseAsync();
    }

    private static async Task<DbConnection> AcquireBootstrapLockAsync(AuditariumDbContext db, string provider)
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            command.CommandText = "SELECT pg_advisory_lock(hashtext('Auditarium.DatabaseBootstrap'));";
        }
        else
        {
            command.CommandText = "DECLARE @result int; EXEC @result = sp_getapplock @Resource = 'Auditarium.DatabaseBootstrap', @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0; SELECT @result;";
        }
        await command.ExecuteNonQueryAsync();
        return connection;
    }

    private static async Task VerifyNewerSchemaBlocksStartupAsync(string provider, string connectionString)
    {
        await using var current = CreateServiceProvider(provider, connectionString);
        await current.InitializeAuditariumDatabaseAsync();
        await using (var scope = current.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
            await db.Database.ExecuteSqlRawAsync("INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('99999999999999_NewerSchema', '10.0.0');");
        }

        await using var older = CreateServiceProvider(provider, connectionString);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => older.InitializeAuditariumDatabaseAsync());
        Assert.Equal("DATABASE_SCHEMA_NEWER_THAN_APPLICATION", exception.Message);
    }

    private static ServiceProvider CreateServiceProvider(string provider, string connectionString, string? recoveryPassword = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["Auditarium:Database:Provider"] = provider,
            ["Auditarium:Database:ConnectionString"] = connectionString,
            ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180"
        };
        if (recoveryPassword is not null)
        {
            values["Auditarium:Recovery:Enabled"] = "true";
            values["Auditarium:Recovery:DefaultAdminPassword"] = recoveryPassword;
        }
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddAuditariumPersistence(configuration);
        return services.BuildServiceProvider();
    }
}
