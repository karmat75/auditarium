// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;

namespace Auditarium.Bll.Pipeline;

public sealed class AuthorizationBehavior<TMessage, TResponse>(ICurrentActor currentActor, IPermissionEvaluator permissionEvaluator)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : IAppResult
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TMessage);
        var anonymous = requestType.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: false);
        var passwordChange = requestType.GetCustomAttributes(typeof(AllowPasswordChangeAttribute), inherit: false);
        var required = requestType.GetCustomAttributes(typeof(RequiresPermissionAttribute), inherit: false)
            .Cast<RequiresPermissionAttribute>().SingleOrDefault();

        if (anonymous.Length == 1 && required is null)
        {
            return await next(message, cancellationToken);
        }

        if (passwordChange.Length == 1 && anonymous.Length == 0 && required is null)
        {
            if (currentActor.Type == ActorType.User && currentActor.IsAuthenticated && currentActor.UserId is not null)
                return await next(message, cancellationToken);

            return ResultFactory.Failure<TResponse>(new AppError("AUTHENTICATION.REQUIRED", ErrorType.Unauthorized));
        }

        if (anonymous.Length != 0 || required is null)
        {
            return ResultFactory.Failure<TResponse>(new AppError("AUTHORIZATION.SECURITY_DECLARATION_REQUIRED", ErrorType.Forbidden));
        }

        if (currentActor.Type == ActorType.User && currentActor.MustChangePassword)
        {
            return ResultFactory.Failure<TResponse>(new AppError("AUTHENTICATION.PASSWORD_CHANGE_REQUIRED", ErrorType.Forbidden));
        }

        if ((currentActor.Type != ActorType.System && !currentActor.IsAuthenticated) || currentActor.UserId is null)
        {
            return ResultFactory.Failure<TResponse>(new AppError("AUTHENTICATION.REQUIRED", ErrorType.Unauthorized));
        }

        if (!await permissionEvaluator.HasPermissionAsync(currentActor.UserId.Value, required.Permission, cancellationToken))
        {
            return ResultFactory.Failure<TResponse>(new AppError("AUTHORIZATION.FORBIDDEN", ErrorType.Forbidden));
        }

        return await next(message, cancellationToken);
    }
}
