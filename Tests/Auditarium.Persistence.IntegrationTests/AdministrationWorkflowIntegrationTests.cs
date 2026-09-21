// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Security;
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Bll.Features.Administration.Roles;
using Auditarium.Bll.Features.Administration.Settings;
using Auditarium.Bll.Features.Administration.Users;
using Auditarium.Bll.Features.Identity.ApiCredentials;
using Auditarium.Bll.Features.Identity.AuthenticationProviders;
using Auditarium.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class AdministrationWorkflowIntegrationTests
{
    [Fact]
    public async Task Administration_enforces_system_guards_concurrency_secret_masking_and_credential_lifecycle()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var services = CreateProvider(container.GetConnectionString());
        await services.InitializeAuditariumDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var users = new UserAdministrationHandler(db);
        var roles = new RoleAdministrationHandler(db);

        var createdUser = await users.Handle(new CreateUserCommand("  API.Service  ", "API Service", "api@example.test", true), CancellationToken.None);
        Assert.True(createdUser.IsSuccess);
        var user = (await users.Handle(new GetUserQuery(createdUser.Value!), CancellationToken.None)).Value!;
        Assert.Equal("api.service", user.Username);

        Assert.True((await users.Handle(new UpdateUserCommand(user.UserId, user.Username, "API Service A", user.Email, true, user.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        var staleUpdate = await users.Handle(new UpdateUserCommand(user.UserId, user.Username, "Stale overwrite", user.Email, true, user.ConcurrencyVersion), CancellationToken.None);
        Assert.Equal("USER.CONCURRENCY_CONFLICT", Assert.Single(staleUpdate.Errors).Code);

        var customRole = await roles.Handle(new CreateRoleCommand("Automation", "API automation", true), CancellationToken.None);
        Assert.True((await roles.Handle(new AddPermissionToRoleCommand(customRole.Value!, "Audits.Read"), CancellationToken.None)).IsSuccess);
        Assert.True((await users.Handle(new AddUserRoleCommand(user.UserId, customRole.Value!), CancellationToken.None)).IsSuccess);
        var systemRole = await db.Roles.AsNoTracking().SingleAsync(x => x.RoleKey == "SYSTEM_ADMIN");
        var protectedRole = await roles.Handle(new UpdateRoleCommand(systemRole.RoleId, "Changed", "Changed", false, systemRole.ConcurrencyVersion), CancellationToken.None);
        Assert.Equal("ROLE.SYSTEM_MANAGED", Assert.Single(protectedRole.Errors).Code);
        var defaultAdmin = await db.Users.AsNoTracking().SingleAsync(x => x.UserKey == "DEFAULT_ADMIN");
        var protectedAssignment = await users.Handle(new RemoveUserRoleCommand(defaultAdmin.UserId, systemRole.RoleId), CancellationToken.None);
        Assert.Equal("USER.SYSTEM_MANAGED_ROLES", Assert.Single(protectedAssignment.Errors).Code);

        var resolver = scope.ServiceProvider.GetRequiredService<IApplicationSettingResolver>();
        var protector = scope.ServiceProvider.GetRequiredService<ISecretProtector>();
        var ldap = new SideEffectFreeLdapService();
        var providers = new AuthenticationProviderCommandHandler(db, ldap, resolver, protector);
        var firstProvider = await providers.Handle(new CreateLdapProviderCommand("LDAP_ONE", "LDAP One"), CancellationToken.None);
        var secondProvider = await providers.Handle(new CreateLdapProviderCommand("LDAP_TWO", "LDAP Two"), CancellationToken.None);
        Assert.True(firstProvider.IsSuccess); Assert.True(secondProvider.IsSuccess);
        var first = (await providers.Handle(new GetAuthenticationProviderQuery(firstProvider.Value!), CancellationToken.None)).Value!;
        var update = await providers.Handle(new UpdateLdapProviderCommand(first.ProviderId, first.DisplayName,
            new Dictionary<string, string?> { ["Host"] = "ldap-one.example", ["BaseDn"] = "dc=one,dc=example", ["BindPassword"] = "bind-secret" },
            new Dictionary<string, long> { ["Host"] = 0, ["BaseDn"] = 0, ["BindPassword"] = 0 }, first.ConcurrencyVersion), CancellationToken.None);
        Assert.True(update.IsSuccess);
        var second = (await providers.Handle(new GetAuthenticationProviderQuery(secondProvider.Value!), CancellationToken.None)).Value!;
        Assert.True((await providers.Handle(new UpdateLdapProviderCommand(second.ProviderId, second.DisplayName,
            new Dictionary<string, string?> { ["Host"] = "ldap-two.example", ["BaseDn"] = "dc=two,dc=example" },
            new Dictionary<string, long> { ["Host"] = 0, ["BaseDn"] = 0 }, second.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        first = (await providers.Handle(new GetAuthenticationProviderQuery(first.ProviderId), CancellationToken.None)).Value!;
        second = (await providers.Handle(new GetAuthenticationProviderQuery(second.ProviderId), CancellationToken.None)).Value!;
        Assert.Equal("ldap-one.example", first.Settings.Single(x => x.Property == "Host").EffectiveValue);
        Assert.Equal("ldap-two.example", second.Settings.Single(x => x.Property == "Host").EffectiveValue);
        var secret = first.Settings.Single(x => x.Property == "BindPassword");
        Assert.True(secret.IsConfigured); Assert.Null(secret.ConfiguredValue); Assert.Null(secret.EffectiveValue);
        var storedSecret = await db.ApplicationSettings.AsNoTracking().SingleAsync(x => x.SettingKey.EndsWith(":BindPassword"));
        Assert.DoesNotContain("bind-secret", storedSecret.SerializedValue, StringComparison.Ordinal);
        Assert.DoesNotContain(await db.SystemAuditLogs.Select(x => x.AfterState ?? string.Empty).ToListAsync(), state => state.Contains("bind-secret", StringComparison.Ordinal));

        var beforeTest = (Users: await db.Users.CountAsync(), Identities: await db.UserIdentities.CountAsync(), Roles: await db.UserRoles.CountAsync());
        Assert.True((await providers.Handle(new TestLdapProviderConnectionCommand(first.ProviderId), CancellationToken.None)).IsSuccess);
        Assert.Equal(1, ldap.ConnectionTests);
        Assert.Equal(beforeTest, (await db.Users.CountAsync(), await db.UserIdentities.CountAsync(), await db.UserRoles.CountAsync()));

        var apiProvider = await db.AuthenticationProviders.AsNoTracking().SingleAsync(x => x.ProviderKey == "API");
        var identity = await users.Handle(new AddUserIdentityCommand(user.UserId, apiProvider.AuthenticationProviderId, "api.service"), CancellationToken.None);
        Assert.True(identity.IsSuccess);
        var credentialService = scope.ServiceProvider.GetRequiredService<IApiCredentialService>();
        var credentialHandler = new ApiCredentialCommandHandler(credentialService, db);
        var createdCredential = await credentialHandler.Handle(new CreateApiCredentialCommand(identity.Value!, "Production", null), CancellationToken.None);
        Assert.True(createdCredential.IsSuccess); Assert.StartsWith("aud_v1_", createdCredential.Value!.Credential, StringComparison.Ordinal);
        Assert.DoesNotContain(createdCredential.Value.Credential, (await db.ApiCredentials.AsNoTracking().SingleAsync(x => x.ApiCredentialId == createdCredential.Value.CredentialId)).SecretHash, StringComparison.Ordinal);
        Assert.Equal(user.UserId, await credentialService.AuthenticateAsync(createdCredential.Value.Credential));
        var metadata = (await credentialHandler.Handle(new ListApiCredentialsQuery(user.UserId), CancellationToken.None)).Value!;
        Assert.Single(metadata);
        var rotated = await credentialHandler.Handle(new RotateApiCredentialCommand(createdCredential.Value.CredentialId, "Rotation", null), CancellationToken.None);
        Assert.True(rotated.IsSuccess); Assert.NotEqual(createdCredential.Value.Credential, rotated.Value!.Credential);
        Assert.NotNull((await db.ApiCredentials.AsNoTracking().SingleAsync(x => x.ApiCredentialId == createdCredential.Value.CredentialId)).RevokedAt);

        var currentUser = (await users.Handle(new GetUserQuery(user.UserId), CancellationToken.None)).Value!;
        Assert.True((await users.Handle(new UpdateUserCommand(user.UserId, currentUser.Username, currentUser.DisplayName, currentUser.Email, false, currentUser.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.Null(await credentialService.AuthenticateAsync(rotated.Value.Credential));

        var settings = new SettingAdministrationHandler(db, resolver, protector);
        var settingsBefore = (await settings.Handle(new ListSettingsQuery(), CancellationToken.None)).Value!;
        var lifetime = settingsBefore.Single(x => x.Key == "Security:ApiCredentials:DefaultLifetimeDays");
        Assert.True((await settings.Handle(new UpdateSettingCommand(lifetime.Key, "90", lifetime.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        var settingsAfter = (await settings.Handle(new ListSettingsQuery(), CancellationToken.None)).Value!;
        var updatedLifetime = settingsAfter.Single(x => x.Key == lifetime.Key);
        Assert.Equal("90", updatedLifetime.ConfiguredValue); Assert.Equal("90", updatedLifetime.EffectiveValue); Assert.Equal(SettingValueSource.Database, updatedLifetime.Source);

        const string environmentKey = "AUDITARIUM__Security__ApiCredentials__DefaultLifetimeDays";
        Environment.SetEnvironmentVariable(environmentKey, "365");
        try
        {
            var overridden = (await settings.Handle(new ListSettingsQuery(), CancellationToken.None)).Value!.Single(x => x.Key == lifetime.Key);
            Assert.True(overridden.IsExternallyOverridden); Assert.Equal("365", overridden.EffectiveValue); Assert.Equal(SettingValueSource.Environment, overridden.Source);
            var blocked = await settings.Handle(new UpdateSettingCommand(lifetime.Key, "120", overridden.ConcurrencyVersion), CancellationToken.None);
            Assert.Equal("SETTING.EXTERNALLY_OVERRIDDEN", Assert.Single(blocked.Errors).Code);
        }
        finally { Environment.SetEnvironmentVariable(environmentKey, null); }

        Assert.True((await users.Handle(new AddUserIdentityCommand(user.UserId, first.ProviderId, "directory-object-42"), CancellationToken.None)).IsSuccess);
        var dependency = await providers.Handle(new DeleteAuthenticationProviderCommand(first.ProviderId, first.ConcurrencyVersion), CancellationToken.None);
        Assert.Equal("AUTH_PROVIDER.HAS_DEPENDENCIES", Assert.Single(dependency.Errors).Code);
        var systemProviderDelete = await providers.Handle(new DeleteAuthenticationProviderCommand(apiProvider.AuthenticationProviderId, apiProvider.ConcurrencyVersion), CancellationToken.None);
        Assert.Equal("AUTH_PROVIDER.SYSTEM_MANAGED", Assert.Single(systemProviderDelete.Errors).Code);
    }

    private static ServiceProvider CreateProvider(string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auditarium:Database:Provider"] = "PostgreSQL",
            ["Auditarium:Database:ConnectionString"] = connectionString,
            ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180"
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ISecretProtector, TestSecretProtector>();
        services.AddAuditariumPersistence(configuration);
        return services.BuildServiceProvider();
    }

    private sealed class TestSecretProtector : ISecretProtector
    {
        public string Protect(string plaintext, string purpose) => "audsec:v1:" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(purpose + "\n" + plaintext));
        public string Unprotect(string protectedValue, string purpose)
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(protectedValue[10..]));
            var prefix = purpose + "\n";
            return decoded.StartsWith(prefix, StringComparison.Ordinal) ? decoded[prefix.Length..] : throw new InvalidOperationException("Purpose mismatch.");
        }
    }

    private sealed class SideEffectFreeLdapService : ILdapAuthenticationService
    {
        public int ConnectionTests { get; private set; }
        public Task<AuthenticationSuccess?> AuthenticateAsync(string providerKey, AuthenticationAttempt attempt, CancellationToken cancellationToken = default) => Task.FromResult<AuthenticationSuccess?>(null);
        public Task ValidateConnectionAsync(string providerKey, CancellationToken cancellationToken = default) { ConnectionTests++; return Task.CompletedTask; }
    }
}
