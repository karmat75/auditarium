// SPDX-License-Identifier: MIT
using System.Security.Cryptography;
using Auditarium.Bll.Abstractions.Identity;
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

internal sealed class LocalAuthenticationService(AuditariumDbContext db, IPasswordHasher<LocalCredential> hasher) : ILocalAuthenticationService
{
    public async Task<AuthenticationSuccess?> AuthenticateAsync(AuthenticationAttempt attempt, CancellationToken cancellationToken = default)
    {
        var username = attempt.Login.Trim().ToLowerInvariant();
        var result = await (from identity in db.UserIdentities.Include(x => x.LocalCredential)
                            join provider in db.AuthenticationProviders on identity.AuthenticationProviderId equals provider.AuthenticationProviderId
                            join user in db.Users on identity.UserId equals user.UserId
                            where provider.ProviderKey == "LOCAL" && identity.ExternalId == username
                            select new { identity, user }).SingleOrDefaultAsync(cancellationToken);
        if (result?.identity.LocalCredential is not { } credential || !result.user.IsActive || credential.LockoutUntil > DateTimeOffset.UtcNow) return null;
        if (hasher.VerifyHashedPassword(credential, credential.PasswordHash, attempt.Secret) == PasswordVerificationResult.Failed)
        {
            var now = DateTimeOffset.UtcNow;
            if (credential.FailedAttemptWindowStartedAt is null || credential.FailedAttemptWindowStartedAt < now.AddMinutes(-15)) { credential.FailedAttemptWindowStartedAt = now; credential.FailedAttemptCount = 0; }
            credential.FailedAttemptCount++;
            if (credential.FailedAttemptCount >= 5) credential.LockoutUntil = now.AddMinutes(15);
            await db.SaveChangesAsync(cancellationToken); return null;
        }
        credential.FailedAttemptCount = 0; credential.FailedAttemptWindowStartedAt = null; credential.LockoutUntil = null;
        await db.SaveChangesAsync(cancellationToken);
        return new AuthenticationSuccess(result.user.UserId, credential.MustChangePassword);
    }
    public async Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        if (newPassword.Length is < 15 or > 128) return false;
        var row = await (from identity in db.UserIdentities.Include(x => x.LocalCredential)
                         join provider in db.AuthenticationProviders on identity.AuthenticationProviderId equals provider.AuthenticationProviderId
                         join user in db.Users on identity.UserId equals user.UserId
                         where user.UserId == userId && user.IsActive && provider.ProviderKey == "LOCAL"
                         select identity).SingleOrDefaultAsync(cancellationToken);
        if (row?.LocalCredential is not { } credential || hasher.VerifyHashedPassword(credential, credential.PasswordHash, currentPassword) == PasswordVerificationResult.Failed) return false;
        credential.PasswordHash = hasher.HashPassword(credential, newPassword); credential.PasswordChangedAt = DateTimeOffset.UtcNow; credential.MustChangePassword = false;
        credential.FailedAttemptCount = 0; credential.FailedAttemptWindowStartedAt = null; credential.LockoutUntil = null;
        await db.SaveChangesAsync(cancellationToken); return true;
    }
}

internal sealed class ApiCredentialService(AuditariumDbContext db, IPasswordHasher<ApiCredential> hasher) : IApiCredentialService
{
    public async Task<(string Credential, long CredentialId)> CreateAsync(long identityId, string name, DateTimeOffset? expiresAt, CancellationToken cancellationToken = default)
    {
        var identity = await db.UserIdentities.Include(x => x.AuthenticationProvider).SingleAsync(x => x.IdentityId == identityId, cancellationToken);
        if (identity.AuthenticationProvider!.ProviderKey != "API") throw new InvalidOperationException("API credentials require an API identity.");
        var active = await db.ApiCredentials.CountAsync(x => x.IdentityId == identityId && x.RevokedAt == null && (x.ExpiresAt == null || x.ExpiresAt > DateTimeOffset.UtcNow), cancellationToken);
        if (active >= 5) throw new InvalidOperationException("API_CREDENTIAL.MAXIMUM_ACTIVE_REACHED");
        var keyId = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
        var secret = Base64Url(RandomNumberGenerator.GetBytes(32));
        var credential = new ApiCredential { IdentityId = identityId, KeyId = keyId, SecretHash = "", Name = name, CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = expiresAt };
        credential.SecretHash = hasher.HashPassword(credential, secret); db.ApiCredentials.Add(credential); await db.SaveChangesAsync(cancellationToken);
        return ($"aud_v1_{keyId}_{secret}", credential.ApiCredentialId);
    }
    public async Task<long?> AuthenticateAsync(string credentialText, CancellationToken cancellationToken = default)
    {
        var parts = credentialText.Split('_'); if (parts.Length != 4 || parts[0] != "aud" || parts[1] != "v1" || string.IsNullOrWhiteSpace(parts[2]) || string.IsNullOrWhiteSpace(parts[3])) return null;
        var credential = await db.ApiCredentials.Include(x => x.Identity).ThenInclude(x => x!.User).SingleOrDefaultAsync(x => x.KeyId == parts[2], cancellationToken);
        if (credential?.Identity?.User is not { IsActive: true } user || credential.RevokedAt is not null || credential.ExpiresAt <= DateTimeOffset.UtcNow || hasher.VerifyHashedPassword(credential, credential.SecretHash, parts[3]) == PasswordVerificationResult.Failed) return null;
        credential.LastUsedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(cancellationToken); return user.UserId;
    }
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
