// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Identity;

public sealed record AuthenticationAttempt(string Login, string Secret, string? ProviderKey = null);
public sealed record AuthenticationSuccess(long UserId, bool MustChangePassword);
public interface IAuthenticationRouter
{
    Task<string?> RouteAsync(string login, string? explicitlySelectedProvider, CancellationToken cancellationToken = default);
}
public interface ILocalAuthenticationService
{
    Task<AuthenticationSuccess?> AuthenticateAsync(AuthenticationAttempt attempt, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
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
