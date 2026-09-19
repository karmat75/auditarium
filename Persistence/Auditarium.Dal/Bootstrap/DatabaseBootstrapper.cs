// SPDX-License-Identifier: MIT
using Auditarium.Dal.Configuration;
using Auditarium.Bll.Settings;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Auditarium.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Auditarium.Dal.Bootstrap;

internal sealed class DatabaseBootstrapper(AuditariumDbContext db, IPasswordHasher<LocalCredential> passwordHasher, RecoveryOptions recovery, IConfiguration configuration)
{
    private const string DefaultAdminUsername = "administrator";
    public async Task<string?> InitializeAsync(CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await SystemUserAsync(ct);
        var local = await ProviderAsync("LOCAL", "LOCAL", "Local", ct);
        await ProviderAsync("API", "API", "API", ct);
        await ReconcilePermissionsAsync(ct);
        await ReconcileRolesAsync(ct);
        var systemAdmin = await db.Roles.SingleAsync(role => role.RoleKey == "SYSTEM_ADMIN", ct);
        var admin = await AdminAsync(local, ct);
        if (!await db.UserRoles.AnyAsync(x => x.UserId == admin.UserId && x.RoleId == systemAdmin.RoleId, ct)) db.UserRoles.Add(new UserRole { UserId = admin.UserId, RoleId = systemAdmin.RoleId });
        await ReconcileSettingsAsync(ct);
        var password = recovery.Enabled ? await RecoverAsync(admin, local, ct) : await FirstInstallAsync(admin, local, ct);
        await db.SaveChangesAsync(ct);
        await ValidateBootstrapAsync(ct);
        await transaction.CommitAsync(ct);
        return password;
    }
    private async Task ValidateBootstrapAsync(CancellationToken ct)
    {
        var system = await db.Users.SingleOrDefaultAsync(x => x.UserKey == "SYSTEM", ct);
        if (system is null) throw new InvalidOperationException("Bootstrap validation failed: SYSTEM user is missing.");
        if (system.UserId != 0) throw new InvalidOperationException($"Bootstrap validation failed: SYSTEM user has id {system.UserId} instead of 0.");
        if (system.IsActive) throw new InvalidOperationException("Bootstrap validation failed: SYSTEM user is active.");
        var administrator = await db.Users.SingleOrDefaultAsync(x => x.UserKey == "DEFAULT_ADMIN", ct);
        if (administrator is null) throw new InvalidOperationException("Bootstrap validation failed: default administrator is missing.");
        foreach (var definition in PermissionDefinitions.All) if (!await db.Permissions.AnyAsync(x => x.PermissionKey == definition.Key && x.IsActive, ct)) throw new InvalidOperationException($"Bootstrap validation failed: permission {definition.Key} is missing.");
        foreach (var definition in SystemRoleDefinitions.All)
        {
            var role = await db.Roles.SingleOrDefaultAsync(x => x.RoleKey == definition.Key && x.IsActive, ct) ?? throw new InvalidOperationException($"Bootstrap validation failed: role {definition.Key} is missing.");
            var actual = await db.RolePermissions.Where(x => x.RoleId == role.RoleId).Select(x => x.PermissionKey).ToListAsync(ct);
            if (!actual.ToHashSet(StringComparer.Ordinal).SetEquals(definition.Permissions)) throw new InvalidOperationException($"Bootstrap validation failed: role {definition.Key} has an invalid permission set.");
        }
    }
    private async Task SystemUserAsync(CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.UserKey == "SYSTEM", ct);
        if (user is null)
        {
            if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                await db.Database.ExecuteSqlInterpolatedAsync($"SET IDENTITY_INSERT auditarium.users ON; INSERT INTO auditarium.users (user_id, user_key, username, display_name, IsActive, concurrency_version) VALUES (0, {"SYSTEM"}, {"system"}, {"System"}, 0, 1); SET IDENTITY_INSERT auditarium.users OFF;", ct);
            }
            else
            {
                user = new User { UserKey = "SYSTEM", Username = "system", DisplayName = "System", IsActive = false };
                db.Users.Add(user);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE auditarium.users SET user_id = 0 WHERE user_key = {user.UserKey}", ct);
            }
            db.ChangeTracker.Clear();
        }
        else if (user.UserId != 0) throw new InvalidOperationException("Bootstrap state invalid: SYSTEM user must have user_id 0.");
        else { user.Username = "system"; user.DisplayName = "System"; user.IsActive = false; }
    }
    private async Task ReconcilePermissionsAsync(CancellationToken ct)
    {
        foreach (var definition in PermissionDefinitions.All)
        {
            var permission = await db.Permissions.SingleOrDefaultAsync(x => x.PermissionKey == definition.Key, ct);
            if (permission is null) db.Permissions.Add(new Permission { PermissionKey = definition.Key, Description = definition.Description, IsActive = true });
            else { permission.Description = definition.Description; permission.IsActive = true; }
        }
    }
    private async Task ReconcileRolesAsync(CancellationToken ct)
    {
        foreach (var definition in SystemRoleDefinitions.All)
        {
            var role = await RoleAsync(definition.Key, definition.Name, definition.Description, ct);
            var existing = await db.RolePermissions.Where(x => x.RoleId == role.RoleId).ToListAsync(ct);
            db.RolePermissions.RemoveRange(existing.Where(x => !definition.Permissions.Contains(x.PermissionKey)));
            foreach (var key in definition.Permissions.Except(existing.Select(x => x.PermissionKey))) db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionKey = key });
        }
    }
    private async Task ReconcileSettingsAsync(CancellationToken ct)
    {
        foreach (var definition in SettingDefinitions.All)
        {
            var setting = await db.ApplicationSettings.SingleOrDefaultAsync(x => x.SettingKey == definition.Key, ct);
            if (setting is not null && IsValid(setting.SerializedValue, definition)) continue;
            var configured = configuration["Auditarium:" + definition.Key];
            var value = configured is not null && definition.IsValid(configured) ? configured : definition.DefaultJson;
            var envelope = JsonSerializer.Serialize(new { datatype = definition.DataType, value = JsonDocument.Parse(value).RootElement });
            if (setting is null) db.ApplicationSettings.Add(new ApplicationSetting { SettingKey = definition.Key, SerializedValue = envelope });
            else setting.SerializedValue = envelope;
        }
    }
    private static bool IsValid(string serialized, SettingDefinition definition)
    {
        try { using var document = JsonDocument.Parse(serialized); return document.RootElement.GetProperty("datatype").GetString() == definition.DataType && definition.IsValid(document.RootElement.GetProperty("value").ToString()); } catch (JsonException) { return false; } catch (KeyNotFoundException) { return false; }
    }
    private async Task<AuthenticationProvider> ProviderAsync(string key, string type, string name, CancellationToken ct)
    {
        var provider = await db.AuthenticationProviders.SingleOrDefaultAsync(x => x.ProviderKey == key, ct);
        if (provider is null) { provider = new AuthenticationProvider { ProviderKey = key, ProviderType = type, DisplayName = name }; db.AuthenticationProviders.Add(provider); await db.SaveChangesAsync(ct); }
        else { if (provider.ProviderType != type) throw new InvalidOperationException($"Bootstrap state invalid: provider '{key}' has an unexpected type."); provider.DisplayName = name; provider.IsEnabled = true; }
        return provider;
    }
    private async Task<Role> RoleAsync(string key, string name, string description, CancellationToken ct)
    {
        var role = await db.Roles.SingleOrDefaultAsync(x => x.RoleKey == key, ct);
        if (role is null) { role = new Role { RoleKey = key, Name = name, Description = description }; db.Roles.Add(role); await db.SaveChangesAsync(ct); }
        else { role.Name = name; role.Description = description; role.IsActive = true; }
        return role;
    }
    private async Task<User> AdminAsync(AuthenticationProvider local, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.UserKey == "DEFAULT_ADMIN", ct);
        if (user is null)
        {
            user = new User { UserKey = "DEFAULT_ADMIN", Username = DefaultAdminUsername, DisplayName = "Administrator", IsActive = true };
            db.Users.Add(user); await db.SaveChangesAsync(ct);
        }
        if (!await db.UserIdentities.AnyAsync(x => x.UserId == user.UserId && x.AuthenticationProviderId == local.AuthenticationProviderId, ct))
        {
            db.UserIdentities.Add(new UserIdentity { UserId = user.UserId, AuthenticationProviderId = local.AuthenticationProviderId, ExternalId = DefaultAdminUsername });
            await db.SaveChangesAsync(ct);
        }
        return user;
    }
    private async Task<string?> FirstInstallAsync(User admin, AuthenticationProvider local, CancellationToken ct)
    {
        var identity = await db.UserIdentities.SingleAsync(x => x.UserId == admin.UserId && x.AuthenticationProviderId == local.AuthenticationProviderId, ct);
        if (await db.LocalCredentials.AnyAsync(x => x.IdentityId == identity.IdentityId, ct)) return null;
        var password = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var credential = new LocalCredential { IdentityId = identity.IdentityId, PasswordHash = "", MustChangePassword = true };
        credential.PasswordHash = passwordHasher.HashPassword(credential, password);
        db.LocalCredentials.Add(credential);
        return password;
    }
    private async Task<string?> RecoverAsync(User admin, AuthenticationProvider local, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(recovery.DefaultAdminPassword)) throw new InvalidOperationException("Recovery is enabled but Auditarium:Recovery:DefaultAdminPassword is missing.");
        var identity = await db.UserIdentities.SingleAsync(x => x.UserId == admin.UserId && x.AuthenticationProviderId == local.AuthenticationProviderId, ct);
        var credential = await db.LocalCredentials.SingleOrDefaultAsync(x => x.IdentityId == identity.IdentityId, ct);
        if (credential is null) { credential = new LocalCredential { IdentityId = identity.IdentityId, PasswordHash = "" }; db.LocalCredentials.Add(credential); }
        var valid = !string.IsNullOrWhiteSpace(credential.PasswordHash) && passwordHasher.VerifyHashedPassword(credential, credential.PasswordHash, recovery.DefaultAdminPassword) != PasswordVerificationResult.Failed;
        if (!valid) credential.PasswordHash = passwordHasher.HashPassword(credential, recovery.DefaultAdminPassword);
        credential.FailedAttemptCount = 0; credential.FailedAttemptWindowStartedAt = null; credential.LockoutUntil = null; credential.MustChangePassword = true; credential.PasswordChangedAt = null; admin.IsActive = true;
        return null;
    }
    public static Task WriteInitialCredentialAsync(string password) => Console.Out.WriteLineAsync($"Auditarium – Initial Administrator Credential\n\nUsername:         Administrator\nInitial password: {password}\n\nThis is a temporary credential.\nYou must change it at the first login.\n\nIf this credential is no longer available before it was changed, use the Auditarium Recovery procedure.");
}
