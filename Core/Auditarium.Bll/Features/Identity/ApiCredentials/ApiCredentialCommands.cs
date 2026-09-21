// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Identity.ApiCredentials;

public sealed record ApiCredentialCreated(long CredentialId, string Credential);
public sealed record ApiCredentialMetadata(long CredentialId, long IdentityId, string KeyId, string Name, DateTimeOffset CreatedAt, DateTimeOffset? ExpiresAt, DateTimeOffset? RevokedAt, DateTimeOffset? LastUsedAt);

[RequiresPermission("Authentication.Manage")]
public sealed record CreateApiCredentialCommand(long IdentityId, string Name, DateTimeOffset? ExpiresAt) : IRequest<Result<ApiCredentialCreated>>;

[RequiresPermission("Authentication.Manage")]
public sealed record RevokeApiCredentialCommand(long CredentialId) : IRequest<Result>;

[RequiresPermission("Authentication.Manage")]
public sealed record RotateApiCredentialCommand(long CredentialId, string Name, DateTimeOffset? ExpiresAt) : IRequest<Result<ApiCredentialCreated>>;

[RequiresPermission("Authentication.Manage")]
public sealed record ListApiCredentialsQuery(long UserId) : IRequest<Result<IReadOnlyList<ApiCredentialMetadata>>>;

public sealed class ApiCredentialCommandHandler(IApiCredentialService credentials, IAuditariumDbContext db)
    : IRequestHandler<CreateApiCredentialCommand, Result<ApiCredentialCreated>>, IRequestHandler<RevokeApiCredentialCommand, Result>,
      IRequestHandler<RotateApiCredentialCommand, Result<ApiCredentialCreated>>, IRequestHandler<ListApiCredentialsQuery, Result<IReadOnlyList<ApiCredentialMetadata>>>
{
    public async ValueTask<Result<IReadOnlyList<ApiCredentialMetadata>>> Handle(ListApiCredentialsQuery message, CancellationToken cancellationToken)
    {
        if (!await db.Users.AnyAsync(x => x.UserId == message.UserId && x.UserKey != "SYSTEM", cancellationToken))
            return Result<IReadOnlyList<ApiCredentialMetadata>>.Failure(new AppError("USER.NOT_FOUND", ErrorType.NotFound));
        var items = await (from credential in db.ApiCredentials.AsNoTracking()
                           join identity in db.UserIdentities.AsNoTracking() on credential.IdentityId equals identity.IdentityId
                           where identity.UserId == message.UserId
                           orderby credential.CreatedAt descending
                           select new ApiCredentialMetadata(credential.ApiCredentialId, credential.IdentityId, credential.KeyId, credential.Name,
                               credential.CreatedAt, credential.ExpiresAt, credential.RevokedAt, credential.LastUsedAt)).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<ApiCredentialMetadata>>.Success(items);
    }

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

    public async ValueTask<Result<ApiCredentialCreated>> Handle(RotateApiCredentialCommand message, CancellationToken cancellationToken)
    {
        var old = await db.ApiCredentials.AsNoTracking().SingleOrDefaultAsync(x => x.ApiCredentialId == message.CredentialId, cancellationToken);
        if (old is null) return Result<ApiCredentialCreated>.Failure(new AppError("API_CREDENTIAL.NOT_FOUND", ErrorType.NotFound));
        if (old.RevokedAt is not null) return Result<ApiCredentialCreated>.Failure(new AppError("API_CREDENTIAL.ALREADY_REVOKED", ErrorType.Conflict));
        if (string.IsNullOrWhiteSpace(message.Name)) return Result<ApiCredentialCreated>.Failure(new AppError("API_CREDENTIAL.NAME_REQUIRED", ErrorType.Validation));
        if (message.ExpiresAt is { } expiry && expiry <= DateTimeOffset.UtcNow) return Result<ApiCredentialCreated>.Failure(new AppError("API_CREDENTIAL.INVALID_EXPIRY", ErrorType.Validation));
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        try
        {
            var tracked = await db.ApiCredentials.SingleAsync(x => x.ApiCredentialId == old.ApiCredentialId, cancellationToken);
            tracked.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            var created = await credentials.CreateAsync(old.IdentityId, message.Name.Trim(), message.ExpiresAt, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<ApiCredentialCreated>.Success(new(created.CredentialId, created.Credential));
        }
        catch (InvalidOperationException exception) when (exception.Message == "API_CREDENTIAL.MAXIMUM_ACTIVE_REACHED")
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<ApiCredentialCreated>.Failure(new AppError(exception.Message, ErrorType.Conflict));
        }
    }
}
