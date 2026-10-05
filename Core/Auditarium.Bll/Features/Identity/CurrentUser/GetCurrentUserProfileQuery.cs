// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Identity.CurrentUser;

public sealed record CurrentUserProfile(string DisplayName);

[AllowPasswordChange]
public sealed record GetCurrentUserProfileQuery : IRequest<Result<CurrentUserProfile>>;

public sealed class GetCurrentUserProfileQueryHandler(IAuditariumDbContext db, ICurrentActor currentActor)
    : IRequestHandler<GetCurrentUserProfileQuery, Result<CurrentUserProfile>>
{
    public async ValueTask<Result<CurrentUserProfile>> Handle(GetCurrentUserProfileQuery message, CancellationToken cancellationToken)
    {
        if (currentActor.UserId is not { } userId)
        {
            return Result<CurrentUserProfile>.Failure(new AppError("AUTHENTICATION.REQUIRED", ErrorType.Unauthorized));
        }

        var displayName = await db.Users.AsNoTracking()
            .Where(user => user.UserId == userId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(displayName)
            ? Result<CurrentUserProfile>.Failure(new AppError("AUTHENTICATION.USER_NOT_FOUND", ErrorType.Unauthorized))
            : Result<CurrentUserProfile>.Success(new CurrentUserProfile(displayName));
    }
}
