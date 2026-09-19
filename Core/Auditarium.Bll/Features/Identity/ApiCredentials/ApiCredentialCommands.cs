// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Identity.ApiCredentials;

public sealed record ApiCredentialCreated(long CredentialId, string Credential);

[RequiresPermission("Authentication.Manage")]
public sealed record CreateApiCredentialCommand(long IdentityId, string Name, DateTimeOffset? ExpiresAt) : IRequest<Result<ApiCredentialCreated>>;

[RequiresPermission("Authentication.Manage")]
public sealed record RevokeApiCredentialCommand(long CredentialId) : IRequest<Result>;

public sealed class ApiCredentialCommandHandler(IApiCredentialService credentials, IAuditariumDbContext db)
    : IRequestHandler<CreateApiCredentialCommand, Result<ApiCredentialCreated>>, IRequestHandler<RevokeApiCredentialCommand, Result>
{
    public async ValueTask<Result<ApiCredentialCreated>> Handle(CreateApiCredentialCommand message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Name)) return Result<ApiCredentialCreated>.Failure(new AppError("API_CREDENTIAL.NAME_REQUIRED", ErrorType.Validation));
        if (message.ExpiresAt is { } expiry && expiry <= DateTimeOffset.UtcNow) return Result<ApiCredentialCreated>.Failure(new AppError("API_CREDENTIAL.INVALID_EXPIRY", ErrorType.Validation));
        try
        {
            var created = await credentials.CreateAsync(message.IdentityId, message.Name.Trim(), message.ExpiresAt, cancellationToken);
            return Result<ApiCredentialCreated>.Success(new(created.CredentialId, created.Credential));
        }
        catch (InvalidOperationException exception) when (exception.Message == "API_CREDENTIAL.MAXIMUM_ACTIVE_REACHED") { return Result<ApiCredentialCreated>.Failure(new AppError(exception.Message, ErrorType.Conflict)); }
        catch (InvalidOperationException) { return Result<ApiCredentialCreated>.Failure(new AppError("API_CREDENTIAL.INVALID_IDENTITY", ErrorType.Validation)); }
    }

    public async ValueTask<Result> Handle(RevokeApiCredentialCommand message, CancellationToken cancellationToken)
    {
        var credential = await db.ApiCredentials.SingleOrDefaultAsync(x => x.ApiCredentialId == message.CredentialId, cancellationToken);
        if (credential is null) return Result.Failure(new AppError("API_CREDENTIAL.NOT_FOUND", ErrorType.NotFound));
        if (credential.RevokedAt is null) { credential.RevokedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(cancellationToken); }
        return Result.Success();
    }
}
