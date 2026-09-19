// SPDX-License-Identifier: MIT
using System.Text.RegularExpressions;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Identity.AuthenticationProviders;

[RequiresPermission("Authentication.Manage")]
public sealed record CreateLdapProviderCommand(string ProviderKey, string DisplayName) : IRequest<Result<long>>;

[RequiresPermission("Authentication.Manage")]
public sealed record SetAuthenticationProviderEnabledCommand(long ProviderId, bool IsEnabled) : IRequest<Result>;

[RequiresPermission("Authentication.Manage")]
public sealed record DeleteAuthenticationProviderCommand(long ProviderId) : IRequest<Result>;

public sealed class AuthenticationProviderCommandHandler(IAuditariumDbContext db)
    : IRequestHandler<CreateLdapProviderCommand, Result<long>>, IRequestHandler<SetAuthenticationProviderEnabledCommand, Result>, IRequestHandler<DeleteAuthenticationProviderCommand, Result>
{
    public async ValueTask<Result<long>> Handle(CreateLdapProviderCommand message, CancellationToken cancellationToken)
    {
        var key = message.ProviderKey.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(key, "^[A-Z][A-Z0-9_]{1,62}$") || key is "LOCAL" or "API") return Result<long>.Failure(new AppError("AUTH_PROVIDER.INVALID_KEY", ErrorType.Validation));
        if (string.IsNullOrWhiteSpace(message.DisplayName)) return Result<long>.Failure(new AppError("AUTH_PROVIDER.DISPLAY_NAME_REQUIRED", ErrorType.Validation));
        if (await db.AuthenticationProviders.AnyAsync(x => x.ProviderKey == key, cancellationToken)) return Result<long>.Failure(new AppError("AUTH_PROVIDER.KEY_EXISTS", ErrorType.Conflict));
        var provider = new AuthenticationProvider { ProviderKey = key, ProviderType = "LDAP", DisplayName = message.DisplayName.Trim(), IsEnabled = true };
        db.AuthenticationProviders.Add(provider); await db.SaveChangesAsync(cancellationToken);
        return Result<long>.Success(provider.AuthenticationProviderId);
    }

    public async ValueTask<Result> Handle(SetAuthenticationProviderEnabledCommand message, CancellationToken cancellationToken)
    {
        var provider = await db.AuthenticationProviders.SingleOrDefaultAsync(x => x.AuthenticationProviderId == message.ProviderId, cancellationToken);
        if (provider is null) return Result.Failure(new AppError("AUTH_PROVIDER.NOT_FOUND", ErrorType.NotFound));
        if (provider.ProviderKey is "LOCAL" or "API") return Result.Failure(new AppError("AUTH_PROVIDER.SYSTEM_MANAGED", ErrorType.Forbidden));
        provider.IsEnabled = message.IsEnabled; await db.SaveChangesAsync(cancellationToken); return Result.Success();
    }

    public async ValueTask<Result> Handle(DeleteAuthenticationProviderCommand message, CancellationToken cancellationToken)
    {
        var provider = await db.AuthenticationProviders.SingleOrDefaultAsync(x => x.AuthenticationProviderId == message.ProviderId, cancellationToken);
        if (provider is null) return Result.Failure(new AppError("AUTH_PROVIDER.NOT_FOUND", ErrorType.NotFound));
        if (provider.ProviderKey is "LOCAL" or "API") return Result.Failure(new AppError("AUTH_PROVIDER.SYSTEM_MANAGED", ErrorType.Forbidden));
        if (await db.UserIdentities.AnyAsync(x => x.AuthenticationProviderId == provider.AuthenticationProviderId, cancellationToken)) return Result.Failure(new AppError("AUTH_PROVIDER.HAS_DEPENDENCIES", ErrorType.Conflict));
        db.AuthenticationProviders.Remove(provider); await db.SaveChangesAsync(cancellationToken); return Result.Success();
    }
}
