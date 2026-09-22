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
        var localAuthentication = scope.ServiceProvider.GetRequiredService<ILocalAuthenticationService>();
        var users = new UserAdministrationHandler(db, localAuthentication);
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

        var local = await users.Handle(new ProvisionLocalIdentityCommand(user.UserId), CancellationToken.None);
        Assert.True(local.IsSuccess);
        Assert.Equal(user.Username, local.Value!.LoginName);
        Assert.True((await localAuthentication.AuthenticateAsync(new AuthenticationAttempt(local.Value.LoginName, local.Value.TemporaryPassword)))!.MustChangePassword);
        Assert.Equal("LOCAL_CREDENTIAL.IDENTITY_ALREADY_EXISTS", Assert.Single((await users.Handle(new ProvisionLocalIdentityCommand(user.UserId), CancellationToken.None)).Errors).Code);
        Assert.Equal(LocalPasswordChangeStatus.PolicyViolation, await localAuthentication.ChangePasswordAsync(user.UserId, local.Value.TemporaryPassword, "administrator123"));
        const string permanentPassword = "A-Long-New-Passphrase-2026!";
        Assert.Equal(LocalPasswordChangeStatus.Success, await localAuthentication.ChangePasswordAsync(user.UserId, local.Value.TemporaryPassword, permanentPassword));
        Assert.NotNull(await localAuthentication.AuthenticateAsync(new AuthenticationAttempt(local.Value.LoginName, permanentPassword)));

        var localDetails = (await users.Handle(new GetUserQuery(user.UserId), CancellationToken.None)).Value!.Identities.Single(x => x.ProviderType == "LOCAL");
        var lockedCredential = await db.LocalCredentials.SingleAsync(x => x.IdentityId == localDetails.IdentityId);
        lockedCredential.FailedAttemptCount = 4;
        lockedCredential.FailedAttemptWindowStartedAt = DateTimeOffset.UtcNow;
        lockedCredential.LockoutUntil = DateTimeOffset.UtcNow.AddMinutes(15);
        await db.SaveChangesAsync();
        var reset = await users.Handle(new ResetLocalCredentialCommand(localDetails.IdentityId, localDetails.CredentialConcurrencyVersion!.Value), CancellationToken.None);
        Assert.True(reset.IsSuccess);
        await db.Entry(lockedCredential).ReloadAsync();
        Assert.Equal(0, lockedCredential.FailedAttemptCount);
        Assert.Null(lockedCredential.FailedAttemptWindowStartedAt);
        Assert.Null(lockedCredential.LockoutUntil);
        var staleReset = await users.Handle(new ResetLocalCredentialCommand(localDetails.IdentityId, localDetails.CredentialConcurrencyVersion.Value), CancellationToken.None);
        Assert.Equal("LOCAL_CREDENTIAL.CONCURRENCY_CONFLICT", Assert.Single(staleReset.Errors).Code);

        await using var parallelScopeOne = services.CreateAsyncScope();
        await using var parallelScopeTwo = services.CreateAsyncScope();
        var parallelDbOne = parallelScopeOne.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var parallelDbTwo = parallelScopeTwo.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var parallelOne = new UserAdministrationHandler(parallelDbOne, parallelScopeOne.ServiceProvider.GetRequiredService<ILocalAuthenticationService>());
        var parallelTwo = new UserAdministrationHandler(parallelDbTwo, parallelScopeTwo.ServiceProvider.GetRequiredService<ILocalAuthenticationService>());
        var parallelResults = await Task.WhenAll(
            parallelOne.Handle(new ResetLocalCredentialCommand(localDetails.IdentityId, reset.Value!.ConcurrencyVersion), CancellationToken.None).AsTask(),
            parallelTwo.Handle(new ResetLocalCredentialCommand(localDetails.IdentityId, reset.Value.ConcurrencyVersion), CancellationToken.None).AsTask());
        var effectiveReset = Assert.Single(parallelResults, result => result.IsSuccess).Value!;
        Assert.Equal("LOCAL_CREDENTIAL.CONCURRENCY_CONFLICT", Assert.Single(Assert.Single(parallelResults, result => !result.IsSuccess).Errors).Code);

        db.ChangeTracker.Clear();
        Assert.Null(await localAuthentication.AuthenticateAsync(new AuthenticationAttempt(local.Value.LoginName, permanentPassword)));
        Assert.Null(await localAuthentication.AuthenticateAsync(new AuthenticationAttempt(local.Value.LoginName, reset.Value.TemporaryPassword)));
        Assert.True((await localAuthentication.AuthenticateAsync(new AuthenticationAttempt(local.Value.LoginName, effectiveReset.TemporaryPassword)))!.MustChangePassword);
        var auditStates = await db.SystemAuditLogs.AsNoTracking().Select(x => (x.BeforeState ?? string.Empty) + (x.AfterState ?? string.Empty)).ToListAsync();
        Assert.DoesNotContain(auditStates, state => state.Contains(local.Value.TemporaryPassword, StringComparison.Ordinal) || state.Contains(reset.Value.TemporaryPassword, StringComparison.Ordinal) || state.Contains(effectiveReset.TemporaryPassword, StringComparison.Ordinal));
        Assert.DoesNotContain(auditStates, state => state.Contains("password_hash", StringComparison.OrdinalIgnoreCase));

        var defaultAdminIdentity = await (from candidate in db.UserIdentities.AsNoTracking()
                                          join owner in db.Users.AsNoTracking() on candidate.UserId equals owner.UserId
                                          join provider in db.AuthenticationProviders.AsNoTracking() on candidate.AuthenticationProviderId equals provider.AuthenticationProviderId
                                          join credential in db.LocalCredentials.AsNoTracking() on candidate.IdentityId equals credential.IdentityId
                                          where owner.UserKey == "DEFAULT_ADMIN" && provider.ProviderKey == "LOCAL"
                                          select new { candidate.IdentityId, credential.ConcurrencyVersion }).SingleAsync();
        var protectedLocalReset = await users.Handle(new ResetLocalCredentialCommand(defaultAdminIdentity.IdentityId, defaultAdminIdentity.ConcurrencyVersion), CancellationToken.None);
        Assert.Equal("LOCAL_CREDENTIAL.SYSTEM_MANAGED", Assert.Single(protectedLocalReset.Errors).Code);

        var beforeRename = (await users.Handle(new GetUserQuery(user.UserId), CancellationToken.None)).Value!;
        Assert.True((await users.Handle(new UpdateUserCommand(user.UserId, "renamed.api.service", beforeRename.DisplayName, beforeRename.Email, true, beforeRename.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.Null(await localAuthentication.AuthenticateAsync(new AuthenticationAttempt(local.Value.LoginName, effectiveReset.TemporaryPassword)));
        Assert.NotNull(await localAuthentication.AuthenticateAsync(new AuthenticationAttempt("renamed.api.service", effectiveReset.TemporaryPassword)));

        var currentUser = (await users.Handle(new GetUserQuery(user.UserId), CancellationToken.None)).Value!;
        Assert.True((await users.Handle(new UpdateUserCommand(user.UserId, currentUser.Username, currentUser.DisplayName, currentUser.Email, false, currentUser.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.Null(await credentialService.AuthenticateAsync(rotated.Value.Credential));
        Assert.Null(await localAuthentication.AuthenticateAsync(new AuthenticationAttempt("renamed.api.service", effectiveReset.TemporaryPassword)));

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
