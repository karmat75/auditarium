// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Identity;

public sealed record AuthenticationAttempt(string Login, string Secret, string? ProviderKey = null);
public sealed record AuthenticationSuccess(long UserId, bool MustChangePassword);
public sealed record TemporaryLocalCredential(string Password, long ConcurrencyVersion);
public enum LocalPasswordChangeStatus { Success, InvalidCredential, PolicyViolation, Conflict }
public enum LocalCredentialResetStatus { Success, NotFound, Conflict }
public sealed record LocalCredentialResetResult(LocalCredentialResetStatus Status, TemporaryLocalCredential? Credential = null);
public interface IAuthenticationRouter
{
    Task<string?> RouteAsync(string login, string? explicitlySelectedProvider, CancellationToken cancellationToken = default);
}
public interface ILocalAuthenticationService
{
    Task<AuthenticationSuccess?> AuthenticateAsync(AuthenticationAttempt attempt, CancellationToken cancellationToken = default);
    Task<LocalPasswordChangeStatus> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<TemporaryLocalCredential> CreateTemporaryCredentialAsync(long identityId, CancellationToken cancellationToken = default);
    Task<LocalCredentialResetResult> ResetTemporaryCredentialAsync(long identityId, long concurrencyVersion, CancellationToken cancellationToken = default);
}
public interface IApiCredentialService
{
    Task<(string Credential, long CredentialId)> CreateAsync(long identityId, string name, DateTimeOffset? expiresAt, CancellationToken cancellationToken = default);
    Task<long?> AuthenticateAsync(string credential, CancellationToken cancellationToken = default);
}
public interface ILdapAuthenticationService
{
    Task<AuthenticationSuccess?> AuthenticateAsync(string providerKey, AuthenticationAttempt attempt, CancellationToken cancellationToken = default);
    Task ValidateConnectionAsync(string providerKey, CancellationToken cancellationToken = default);
}
