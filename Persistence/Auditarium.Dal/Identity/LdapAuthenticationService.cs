// SPDX-License-Identifier: MIT
using System.Text.Json;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Security;
using Auditarium.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Auditarium.Dal.Identity;

internal sealed class LdapAuthenticationService(AuditariumDbContext db, IConfiguration configuration, IUserDirectory directory, ISecretProtector secretProtector) : ILdapAuthenticationService
{
    public async Task ValidateConnectionAsync(string providerKey, CancellationToken cancellationToken = default)
    {
        var provider = await ProviderAsync(providerKey, cancellationToken);
        await directory.ValidateConnectionAsync(await SettingsAsync(provider, cancellationToken), cancellationToken);
    }

    public async Task<AuthenticationSuccess?> AuthenticateAsync(string providerKey, AuthenticationAttempt attempt, CancellationToken cancellationToken = default)
    {
        var provider = await ProviderAsync(providerKey, cancellationToken);
        var settings = await SettingsAsync(provider, cancellationToken);
        var directoryUser = await directory.AuthenticateAsync(settings, attempt.Login, attempt.Secret, cancellationToken);
        if (directoryUser is null) return null;
        var identity = await db.UserIdentities.Include(x => x.User).SingleOrDefaultAsync(x => x.AuthenticationProviderId == provider.AuthenticationProviderId && x.ExternalId == directoryUser.ExternalId, cancellationToken);
        if (identity is not null) return identity.User!.IsActive ? new AuthenticationSuccess(identity.UserId, false) : null;

        var mode = (await ValueAsync(provider.ProviderKey, "ProvisioningMode", false, cancellationToken) ?? "EXISTING_ONLY").ToUpperInvariant();
        if (mode == "EXISTING_ONLY") return null;
        if (mode is not "CREATE_INACTIVE" and not "CREATE_ACTIVE") throw new InvalidOperationException("LDAP.PROVISIONING.MODE_INVALID");
        var username = directoryUser.Username.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Username == username, cancellationToken)) throw new InvalidOperationException("LDAP.PROVISIONING.USERNAME_CONFLICT");
        var roles = new List<Role>();
        if (mode == "CREATE_ACTIVE")
        {
            var roleKeys = (await ValueAsync(provider.ProviderKey, "AutoProvisionRoles", false, cancellationToken) ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            roles = await db.Roles.Where(x => x.IsActive && x.RoleKey != null && roleKeys.Contains(x.RoleKey!)).ToListAsync(cancellationToken);
            if (roles.Count == 0) throw new InvalidOperationException("LDAP.PROVISIONING.ACTIVE_REQUIRES_ROLE");
        }
        var user = new User { Username = username, DisplayName = directoryUser.DisplayName, Email = directoryUser.Email, IsActive = mode == "CREATE_ACTIVE" };
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        db.UserIdentities.Add(new UserIdentity { UserId = user.UserId, AuthenticationProviderId = provider.AuthenticationProviderId, ExternalId = directoryUser.ExternalId });
        if (mode == "CREATE_ACTIVE")
        {
            foreach (var role in roles) db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
        }
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return user.IsActive ? new AuthenticationSuccess(user.UserId, false) : null;
    }

    private async Task<AuthenticationProvider> ProviderAsync(string providerKey, CancellationToken ct) =>
        await db.AuthenticationProviders.SingleOrDefaultAsync(x => x.ProviderKey == providerKey && x.ProviderType == "LDAP" && x.IsEnabled, ct) ?? throw new InvalidOperationException("LDAP.PROVIDER.NOT_AVAILABLE");

    private async Task<LdapProviderConnectionSettings> SettingsAsync(AuthenticationProvider provider, CancellationToken ct) => new(
        Required(await ValueAsync(provider.ProviderKey, "Host", false, ct), "Host"),
        int.TryParse(await ValueAsync(provider.ProviderKey, "Port", false, ct), out var port) ? port : 389,
        Enum.TryParse<LdapTlsMode>(await ValueAsync(provider.ProviderKey, "TlsMode", false, ct), true, out var tls) ? tls : LdapTlsMode.None,
        TimeSpan.FromSeconds(int.TryParse(await ValueAsync(provider.ProviderKey, "ConnectionTimeoutSeconds", false, ct), out var timeout) ? timeout : 15),
        await ValueAsync(provider.ProviderKey, "BindUser", false, ct), await ValueAsync(provider.ProviderKey, "BindPassword", true, ct),
        Required(await ValueAsync(provider.ProviderKey, "BaseDn", false, ct), "BaseDn"),
        await ValueAsync(provider.ProviderKey, "LoginAttribute", false, ct) ?? "uid", await ValueAsync(provider.ProviderKey, "SearchFilter", false, ct) ?? "(objectClass=person)",
        await ValueAsync(provider.ProviderKey, "StableExternalIdAttribute", false, ct) ?? "entryUUID", await ValueAsync(provider.ProviderKey, "DisplayNameAttribute", false, ct) ?? "cn", await ValueAsync(provider.ProviderKey, "EmailAttribute", false, ct) ?? "mail");

    private async Task<string?> ValueAsync(string providerKey, string property, bool secret, CancellationToken ct)
    {
        var key = $"Authentication:Providers:{providerKey}:{property}";
        var environment = Environment.GetEnvironmentVariable("AUDITARIUM__" + key.Replace(":", "__", StringComparison.Ordinal));
        if (environment is not null) return environment;
        var stored = await db.ApplicationSettings.AsNoTracking().SingleOrDefaultAsync(x => x.SettingKey == key, ct);
        if (stored is not null)
        {
            using var document = JsonDocument.Parse(stored.SerializedValue);
            var value = document.RootElement.GetProperty("value").GetString();
            if (value is not null) return secret ? secretProtector.Unprotect(value, key) : value;
        }
        return configuration["Auditarium:" + key];
    }
    private static string Required(string? value, string property) => !string.IsNullOrWhiteSpace(value) ? value : throw new InvalidOperationException($"LDAP provider configuration '{property}' is required.");
}
