// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;

namespace Auditarium.Bll.Features.Identity.LocalCredentials;

[AllowPasswordChange]
public sealed record ChangeCurrentPasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Result>;

public sealed class ChangeCurrentPasswordCommandHandler(ICurrentActor currentActor, ILocalAuthenticationService localAuthentication)
    : IRequestHandler<ChangeCurrentPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeCurrentPasswordCommand message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(message.CurrentPassword) || string.IsNullOrEmpty(message.NewPassword))
            return Result.Failure(new AppError("AUTHENTICATION.PASSWORD_REQUIRED", ErrorType.Validation));

        if (currentActor.UserId is not { } userId || !await localAuthentication.ChangePasswordAsync(userId, message.CurrentPassword, message.NewPassword, cancellationToken))
            return Result.Failure(new AppError("AUTHENTICATION.PASSWORD_CHANGE_FAILED", ErrorType.Forbidden));

        return Result.Success();
    }
}
