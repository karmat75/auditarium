// SPDX-License-Identifier: MIT
namespace Auditarium.Models.Identity;

public sealed class User
{
    public long UserId { get; set; }
    public string? UserKey { get; set; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public long ConcurrencyVersion { get; set; } = 1;
    public List<UserIdentity> Identities { get; } = [];
}
public sealed class AuthenticationProvider
{
    public long AuthenticationProviderId { get; set; }
    public required string ProviderKey { get; set; }
    public required string ProviderType { get; set; }
    public required string DisplayName { get; set; }
    public bool IsEnabled { get; set; } = true;
    public long ConcurrencyVersion { get; set; } = 1;
}
public sealed class UserIdentity
{
    public long IdentityId { get; set; }
    public long UserId { get; set; }
    public long AuthenticationProviderId { get; set; }
    public required string ExternalId { get; set; }
    public User? User { get; set; }
    public AuthenticationProvider? AuthenticationProvider { get; set; }
    public LocalCredential? LocalCredential { get; set; }
}
public sealed class LocalCredential
{
    public long IdentityId { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset? PasswordChangedAt { get; set; }
    public bool MustChangePassword { get; set; }
    public int FailedAttemptCount { get; set; }
    public DateTimeOffset? FailedAttemptWindowStartedAt { get; set; }
    public DateTimeOffset? LockoutUntil { get; set; }
    public UserIdentity? Identity { get; set; }
}
public sealed class ApiCredential
{
    public long ApiCredentialId { get; set; }
    public long IdentityId { get; set; }
    public required string KeyId { get; set; }
    public required string SecretHash { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public UserIdentity? Identity { get; set; }
}
public sealed class Permission { public required string PermissionKey { get; set; } public required string Description { get; set; } public bool IsActive { get; set; } = true; }
public sealed class Role { public long RoleId { get; set; } public string? RoleKey { get; set; } public required string Name { get; set; } public required string Description { get; set; } public bool IsActive { get; set; } = true; public long ConcurrencyVersion { get; set; } = 1; }
public sealed class RolePermission { public long RoleId { get; set; } public required string PermissionKey { get; set; } }
public sealed class UserRole { public long UserId { get; set; } public long RoleId { get; set; } }
public sealed class ApplicationSetting { public required string SettingKey { get; set; } public required string SerializedValue { get; set; } public long ConcurrencyVersion { get; set; } = 1; }
