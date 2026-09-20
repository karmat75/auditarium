// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;

namespace Auditarium.Bll.Features.Identity.LocalCredentials;

[AllowAnonymous]
public sealed record AuthenticateInteractiveUserCommand(string Login, string Password, string? ProviderKey)
    : IRequest<Result<AuthenticationSuccess>>;

public sealed class AuthenticateInteractiveUserCommandHandler(
    IAuthenticationRouter router,
    ILocalAuthenticationService localAuthentication,
    ILdapAuthenticationService ldapAuthentication,
    IAuditEventWriter auditEvents)
    : IRequestHandler<AuthenticateInteractiveUserCommand, Result<AuthenticationSuccess>>
{
    public async ValueTask<Result<AuthenticationSuccess>> Handle(
        AuthenticateInteractiveUserCommand message,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Login) || string.IsNullOrWhiteSpace(message.Password))
        {
            return Result<AuthenticationSuccess>.Failure(
                new AppError("AUTHENTICATION.CREDENTIALS_REQUIRED", ErrorType.Validation));
        }

        var providerKey = await router.RouteAsync(message.Login, message.ProviderKey, cancellationToken);
        AuthenticationSuccess? authentication = providerKey switch
        {
            "LOCAL" => await localAuthentication.AuthenticateAsync(
                new AuthenticationAttempt(message.Login, message.Password, providerKey), cancellationToken),
            not null => await ldapAuthentication.AuthenticateAsync(
                providerKey,
                new AuthenticationAttempt(message.Login, message.Password, providerKey),
                cancellationToken),
            _ => null
        };

        if (authentication is null)
        {
            await auditEvents.WriteAsync(new AuditEvent("LOGIN_FAILED", "Authentication"), cancellationToken);
            return Result<AuthenticationSuccess>.Failure(
                new AppError("AUTHENTICATION.FAILED", ErrorType.Unauthorized));
        }

        await auditEvents.WriteAsync(
            new AuditEvent("LOGIN", "User", authentication.UserId, authentication.UserId),
            cancellationToken);
        return Result<AuthenticationSuccess>.Success(authentication);
    }
}
