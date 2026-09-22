// SPDX-License-Identifier: MIT
using System.Security.Cryptography;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Auditarium.Dal.Identity;

internal sealed class AuthenticationRouter(AuditariumDbContext db, IConfiguration configuration) : IAuthenticationRouter
{
    public async Task<string?> RouteAsync(string login, string? explicitlySelectedProvider, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(explicitlySelectedProvider))
            return await db.AuthenticationProviders.AsNoTracking().AnyAsync(p => p.ProviderKey == explicitlySelectedProvider && p.IsEnabled, cancellationToken) ? explicitlySelectedProvider : null;
        if (!login.Contains('\\') && !login.Contains('@')) return "LOCAL";
        var isPrefix = login.Contains('\\');
        var part = isPrefix ? login[..login.IndexOf('\\')] : login[(login.LastIndexOf('@') + 1)..];
        foreach (var provider in await db.AuthenticationProviders.AsNoTracking().Where(p => p.ProviderType == "LDAP" && p.IsEnabled).ToListAsync(cancellationToken))
        {
            var property = isPrefix ? "DomainPrefixes" : "UpnSuffixes";
            var configured = Environment.GetEnvironmentVariable($"AUDITARIUM__Authentication__Providers__{provider.ProviderKey}__{property}")
                ?? configuration[$"Auditarium:Authentication:Providers:{provider.ProviderKey}:{property}"];
            if ((configured ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(x => x.Equals(part, StringComparison.OrdinalIgnoreCase))) return provider.ProviderKey;
        }
        return null;
    }
}

internal sealed class LocalAuthenticationService(AuditariumDbContext db, IPasswordHasher<LocalCredential> hasher, IApplicationSettingResolver settings) : ILocalAuthenticationService
{
    public async Task<AuthenticationSuccess?> AuthenticateAsync(AuthenticationAttempt attempt, CancellationToken cancellationToken = default)
    {
        var username = attempt.Login.Trim().ToLowerInvariant();
        var result = await (from identity in db.UserIdentities.Include(x => x.LocalCredential)
                            join provider in db.AuthenticationProviders on identity.AuthenticationProviderId equals provider.AuthenticationProviderId
                            join user in db.Users on identity.UserId equals user.UserId
                            where provider.ProviderKey == "LOCAL" && provider.IsEnabled && identity.ExternalId == username
                            select new { identity, user }).SingleOrDefaultAsync(cancellationToken);
        if (result?.identity.LocalCredential is not { } credential || !result.user.IsActive || credential.LockoutUntil > DateTimeOffset.UtcNow) return null;
        if (hasher.VerifyHashedPassword(credential, credential.PasswordHash, attempt.Secret) == PasswordVerificationResult.Failed)
        {
            var now = DateTimeOffset.UtcNow;
            var window = await IntSettingAsync("Security:LocalLockout:WindowMinutes", cancellationToken);
            var attempts = await IntSettingAsync("Security:LocalLockout:FailedAttempts", cancellationToken);
            if (credential.FailedAttemptWindowStartedAt is null || credential.FailedAttemptWindowStartedAt < now.AddMinutes(-window)) { credential.FailedAttemptWindowStartedAt = now; credential.FailedAttemptCount = 0; }
            credential.FailedAttemptCount++;
            if (credential.FailedAttemptCount >= attempts) credential.LockoutUntil = now.AddMinutes(await IntSettingAsync("Security:LocalLockout:DurationMinutes", cancellationToken));
            try { await db.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateConcurrencyException) { }
            return null;
        }
        credential.FailedAttemptCount = 0; credential.FailedAttemptWindowStartedAt = null; credential.LockoutUntil = null;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return null; }
        return new AuthenticationSuccess(result.user.UserId, credential.MustChangePassword);
    }
    public async Task<LocalPasswordChangeStatus> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var minimumLength = await IntSettingAsync("Security:LocalPassword:MinimumLength", cancellationToken);
        var maximumLength = await IntSettingAsync("Security:LocalPassword:MaximumLength", cancellationToken);
        if (newPassword.Length < minimumLength || newPassword.Length > maximumLength || CommonPasswordBlocklist.Contains(newPassword)) return LocalPasswordChangeStatus.PolicyViolation;
        var row = await (from identity in db.UserIdentities.Include(x => x.LocalCredential)
                         join provider in db.AuthenticationProviders on identity.AuthenticationProviderId equals provider.AuthenticationProviderId
                         join user in db.Users on identity.UserId equals user.UserId
                         where user.UserId == userId && user.IsActive && provider.ProviderKey == "LOCAL"
                         select identity).SingleOrDefaultAsync(cancellationToken);
        if (row?.LocalCredential is not { } credential || hasher.VerifyHashedPassword(credential, credential.PasswordHash, currentPassword) == PasswordVerificationResult.Failed) return LocalPasswordChangeStatus.InvalidCredential;
        credential.PasswordHash = hasher.HashPassword(credential, newPassword); credential.PasswordChangedAt = DateTimeOffset.UtcNow; credential.MustChangePassword = false;
        credential.FailedAttemptCount = 0; credential.FailedAttemptWindowStartedAt = null; credential.LockoutUntil = null;
        try { await db.SaveChangesAsync(cancellationToken); return LocalPasswordChangeStatus.Success; }
        catch (DbUpdateConcurrencyException) { return LocalPasswordChangeStatus.Conflict; }
    }

    public async Task<TemporaryLocalCredential> CreateTemporaryCredentialAsync(long identityId, CancellationToken cancellationToken = default)
    {
        var password = await GenerateTemporaryPasswordAsync(cancellationToken);
        var credential = new LocalCredential { IdentityId = identityId, PasswordHash = "", MustChangePassword = true };
        credential.PasswordHash = hasher.HashPassword(credential, password);
        db.LocalCredentials.Add(credential);
        await db.SaveChangesAsync(cancellationToken);
        return new TemporaryLocalCredential(password, credential.ConcurrencyVersion);
    }

    public async Task<LocalCredentialResetResult> ResetTemporaryCredentialAsync(long identityId, long concurrencyVersion, CancellationToken cancellationToken = default)
    {
        var credential = await db.LocalCredentials.SingleOrDefaultAsync(x => x.IdentityId == identityId, cancellationToken);
        if (credential is null) return new(LocalCredentialResetStatus.NotFound);
        if (credential.ConcurrencyVersion != concurrencyVersion) return new(LocalCredentialResetStatus.Conflict);

        var password = await GenerateTemporaryPasswordAsync(cancellationToken);
        credential.PasswordHash = hasher.HashPassword(credential, password);
        credential.PasswordChangedAt = null;
        credential.MustChangePassword = true;
        credential.FailedAttemptCount = 0;
        credential.FailedAttemptWindowStartedAt = null;
        credential.LockoutUntil = null;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return new(LocalCredentialResetStatus.Success, new TemporaryLocalCredential(password, credential.ConcurrencyVersion));
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(LocalCredentialResetStatus.Conflict);
        }
    }

    private async Task<int> IntSettingAsync(string key, CancellationToken cancellationToken) =>
        int.Parse((await settings.GetAsync(key, cancellationToken)).Value, System.Globalization.CultureInfo.InvariantCulture);

    private async Task<string> GenerateTemporaryPasswordAsync(CancellationToken cancellationToken)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        var minimumLength = await IntSettingAsync("Security:LocalPassword:MinimumLength", cancellationToken);
        var maximumLength = await IntSettingAsync("Security:LocalPassword:MaximumLength", cancellationToken);
        if (maximumLength < minimumLength) throw new InvalidOperationException("LOCAL_PASSWORD_POLICY_INVALID");
        var length = Math.Min(maximumLength, Math.Max(minimumLength, 24));
        var bytes = RandomNumberGenerator.GetBytes(length);
        return new string(bytes.Select(value => alphabet[value & 63]).ToArray());
    }
}

internal sealed class ApiCredentialService(AuditariumDbContext db, IPasswordHasher<ApiCredential> hasher, IApplicationSettingResolver settings) : IApiCredentialService
{
    public async Task<(string Credential, long CredentialId)> CreateAsync(long identityId, string name, DateTimeOffset? expiresAt, CancellationToken cancellationToken = default)
    {
        var identity = await db.UserIdentities.Include(x => x.AuthenticationProvider).SingleAsync(x => x.IdentityId == identityId, cancellationToken);
        if (identity.AuthenticationProvider!.ProviderKey != "API") throw new InvalidOperationException("API credentials require an API identity.");
        var active = await db.ApiCredentials.CountAsync(x => x.IdentityId == identityId && x.RevokedAt == null && (x.ExpiresAt == null || x.ExpiresAt > DateTimeOffset.UtcNow), cancellationToken);
        var maximumActiveCredentials = int.Parse((await settings.GetAsync("Security:ApiCredentials:MaximumActiveCredentials", cancellationToken)).Value, System.Globalization.CultureInfo.InvariantCulture);
        if (active >= maximumActiveCredentials) throw new InvalidOperationException("API_CREDENTIAL.MAXIMUM_ACTIVE_REACHED");
        var effectiveExpiry = expiresAt ?? DateTimeOffset.UtcNow.AddDays(int.Parse((await settings.GetAsync("Security:ApiCredentials:DefaultLifetimeDays", cancellationToken)).Value, System.Globalization.CultureInfo.InvariantCulture));
        var keyId = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
        var secret = Base64Url(RandomNumberGenerator.GetBytes(32));
        var credential = new ApiCredential { IdentityId = identityId, KeyId = keyId, SecretHash = "", Name = name, CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = effectiveExpiry };
        credential.SecretHash = hasher.HashPassword(credential, secret); db.ApiCredentials.Add(credential); await db.SaveChangesAsync(cancellationToken);
        return ($"aud_v1_{keyId}_{secret}", credential.ApiCredentialId);
    }
    public async Task<long?> AuthenticateAsync(string credentialText, CancellationToken cancellationToken = default)
    {
        var parts = credentialText.Split('_', 4, StringSplitOptions.None); if (parts.Length != 4 || parts[0] != "aud" || parts[1] != "v1" || string.IsNullOrWhiteSpace(parts[2]) || string.IsNullOrWhiteSpace(parts[3])) return null;
        var credential = await db.ApiCredentials.Include(x => x.Identity).ThenInclude(x => x!.User).SingleOrDefaultAsync(x => x.KeyId == parts[2], cancellationToken);
        if (credential?.Identity?.User is not { IsActive: true } user || credential.RevokedAt is not null || credential.ExpiresAt <= DateTimeOffset.UtcNow || hasher.VerifyHashedPassword(credential, credential.SecretHash, parts[3]) == PasswordVerificationResult.Failed) return null;
        credential.LastUsedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(cancellationToken); return user.UserId;
    }
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
