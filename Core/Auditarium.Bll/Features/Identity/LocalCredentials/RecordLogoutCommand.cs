// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;

namespace Auditarium.Bll.Features.Identity.LocalCredentials;

[AllowAnonymous]
public sealed record RecordLogoutCommand : IRequest<Result>;

public sealed class RecordLogoutCommandHandler(ICurrentActor currentActor, IAuditEventWriter auditEvents)
    : IRequestHandler<RecordLogoutCommand, Result>
{
    public async ValueTask<Result> Handle(RecordLogoutCommand message, CancellationToken cancellationToken)
    {
        await auditEvents.WriteAsync(new AuditEvent("LOGOUT", "User", currentActor.UserId), cancellationToken);
        return Result.Success();
    }
}
