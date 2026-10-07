// SPDX-License-Identifier: MIT
using Auditarium.Dal;
using Auditarium.DemoData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class DemoDataInvocationAndProvisioningTests
{
    [Theory]
    [InlineData("Production", "true", "apply", "--confirm", false)]
    [InlineData("Staging", "true", "apply", "--confirm", false)]
    [InlineData("Development", "false", "apply", "--confirm", false)]
    [InlineData("Development", null, "apply", "--confirm", false)]
    [InlineData("Development", "true", "", "", false)]
    [InlineData("Development", "true", "apply", "", false)]
    [InlineData("Development", "true", "apply", "--confirm", true)]
    public void Invocation_gate_requires_each_explicit_condition(string environment, string? enabled, string operation, string confirmation, bool expected)
    {
        string[] args = string.IsNullOrEmpty(operation) ? [] : string.IsNullOrEmpty(confirmation) ? [operation] : [operation, confirmation];

        var result = DemoDataInvocationGate.Evaluate(args, environment, enabled);

        Assert.Equal(expected, result.IsAllowed);
    }

    [Theory]
    [InlineData("Production", "true", "apply", "--confirm")]
    [InlineData("Staging", "true", "apply", "--confirm")]
    [InlineData("Development", "false", "apply", "--confirm")]
    [InlineData("Development", null, "apply", "--confirm")]
    [InlineData("Development", "true", "", "")]
    [InlineData("Development", "true", "apply", "")]
    public async Task Refused_invocations_do_not_construct_the_persistence_service_provider(string environment, string? enabled, string operation, string confirmation)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auditarium:DemoData:Enabled"] = enabled,
            ["Auditarium:Database:ConnectionString"] = "Host=unreachable.invalid"
        }).Build();
        var serviceProviderConstructed = false;
        string[] args = string.IsNullOrEmpty(operation) ? [] : string.IsNullOrEmpty(confirmation) ? [operation] : [operation, confirmation];

        var exitCode = await DemoDataProgram.RunAsync(
            args,
            environment,
            configuration,
            _ =>
            {
                serviceProviderConstructed = true;
                throw new InvalidOperationException("Persistence must not be constructed for a refused invocation.");
            },
            TextWriter.Null,
            TextWriter.Null);

        Assert.Equal(2, exitCode);
        Assert.False(serviceProviderConstructed);
    }

    [Fact]
    public async Task Provisioning_adds_only_missing_regular_roles_is_idempotent_and_audited()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var services = CreateServiceProvider(container.GetConnectionString());
        await services.InitializeAuditariumDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var administrator = await db.Users.SingleAsync(user => user.UserKey == "DEFAULT_ADMIN");
        var viewer = await db.Roles.SingleAsync(role => role.RoleKey == "VIEWER");
        db.UserRoles.Add(new Auditarium.Models.Identity.UserRole { UserId = administrator.UserId, RoleId = viewer.RoleId });
        await db.SaveChangesAsync();
        var userRoleAuditEntriesBefore = await db.SystemAuditLogs.CountAsync(entry => entry.ObjectType == "UserRole" && entry.Action == "CREATED");

        await new DevelopmentAdministratorProvisioner(db).ProvisionAsync();
        db.ChangeTracker.Clear();

        var assignedRoles = await RoleKeysForDefaultAdministratorAsync(db);
        Assert.Equal(["AUDITOR", "AUDIT_MANAGER", "REVIEWER", "SYSTEM_ADMIN", "VIEWER"], assignedRoles);
        Assert.DoesNotContain("SYSTEM_INTERNAL", assignedRoles);
        Assert.Equal(5, await db.UserRoles.CountAsync(userRole => userRole.UserId == administrator.UserId));
        Assert.Equal(3, await db.SystemAuditLogs.CountAsync(entry => entry.ObjectType == "UserRole" && entry.Action == "CREATED") - userRoleAuditEntriesBefore);
        Assert.Empty(await db.Documents.ToListAsync());
        Assert.Empty(await db.AuditUnits.ToListAsync());
        Assert.Empty(await db.Audits.ToListAsync());

        await new DevelopmentAdministratorProvisioner(db).ProvisionAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(5, await db.UserRoles.CountAsync(userRole => userRole.UserId == administrator.UserId));
    }

    [Fact]
    public async Task Provisioning_refuses_missing_default_administrator_without_role_changes()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var services = CreateServiceProvider(container.GetConnectionString());
        await services.InitializeAuditariumDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var administrator = await db.Users.SingleAsync(user => user.UserKey == "DEFAULT_ADMIN");
        administrator.UserKey = "FORMER_DEFAULT_ADMIN";
        await db.SaveChangesAsync();
        var userRoleCount = await db.UserRoles.CountAsync();

        var exception = await Assert.ThrowsAsync<DemoDataProvisioningException>(() => new DevelopmentAdministratorProvisioner(db).ProvisionAsync());

        Assert.Contains("DEFAULT_ADMIN is missing", exception.Message, StringComparison.Ordinal);
        Assert.Equal(userRoleCount, await db.UserRoles.CountAsync());
    }

    [Fact]
    public async Task Provisioning_refuses_missing_system_role_without_role_changes()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var services = CreateServiceProvider(container.GetConnectionString());
        await services.InitializeAuditariumDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var reviewer = await db.Roles.SingleAsync(role => role.RoleKey == "REVIEWER");
        reviewer.RoleKey = "MISSING_REVIEWER";
        await db.SaveChangesAsync();
        var userRoleCount = await db.UserRoles.CountAsync();

        var exception = await Assert.ThrowsAsync<DemoDataProvisioningException>(() => new DevelopmentAdministratorProvisioner(db).ProvisionAsync());

        Assert.Contains("Required active system roles are missing", exception.Message, StringComparison.Ordinal);
        Assert.Equal(userRoleCount, await db.UserRoles.CountAsync());
    }

    [Fact]
    public async Task Provisioning_refuses_default_administrator_with_system_internal_without_role_changes()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var services = CreateServiceProvider(container.GetConnectionString());
        await services.InitializeAuditariumDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var administrator = await db.Users.SingleAsync(user => user.UserKey == "DEFAULT_ADMIN");
        var internalRole = await db.Roles.SingleAsync(role => role.RoleKey == "SYSTEM_INTERNAL");
        db.UserRoles.Add(new Auditarium.Models.Identity.UserRole { UserId = administrator.UserId, RoleId = internalRole.RoleId });
        await db.SaveChangesAsync();
        var userRoleCount = await db.UserRoles.CountAsync();

        var exception = await Assert.ThrowsAsync<DemoDataProvisioningException>(() => new DevelopmentAdministratorProvisioner(db).ProvisionAsync());

        Assert.Contains("SYSTEM_INTERNAL", exception.Message, StringComparison.Ordinal);
        Assert.Equal(userRoleCount, await db.UserRoles.CountAsync());
    }

    private static async Task<string[]> RoleKeysForDefaultAdministratorAsync(AuditariumDbContext db) =>
        await (from userRole in db.UserRoles.AsNoTracking()
               join user in db.Users.AsNoTracking() on userRole.UserId equals user.UserId
               join role in db.Roles.AsNoTracking() on userRole.RoleId equals role.RoleId
               where user.UserKey == "DEFAULT_ADMIN"
               orderby role.RoleKey
               select role.RoleKey!).ToArrayAsync();

    private static ServiceProvider CreateServiceProvider(string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auditarium:Database:Provider"] = "PostgreSQL",
            ["Auditarium:Database:ConnectionString"] = connectionString,
            ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180"
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddAuditariumPersistence(configuration);
        return services.BuildServiceProvider();
    }
}
