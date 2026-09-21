// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Administration.Users;

public sealed record UserListItem(long UserId, string? UserKey, string Username, string DisplayName, string? Email, bool IsActive, long ConcurrencyVersion);
public sealed record UserPage(IReadOnlyList<UserListItem> Items, long TotalCount);
public sealed record UserRoleItem(long RoleId, string? RoleKey, string Name, bool IsActive, bool Assigned);
public sealed record UserIdentityItem(long IdentityId, long ProviderId, string ProviderKey, string ProviderType, string ProviderDisplayName, string ExternalId);
public sealed record UserDetails(long UserId, string? UserKey, string Username, string DisplayName, string? Email, bool IsActive, IReadOnlyList<UserRoleItem> Roles, IReadOnlyList<UserIdentityItem> Identities, long ConcurrencyVersion);
public sealed record IdentityProviderOption(long ProviderId, string ProviderKey, string ProviderType, string DisplayName, bool IsEnabled);

[RequiresPermission("Users.Manage")]
public sealed record ListUsersQuery(string? Search, bool? IsActive, int Skip, int Take, string Sort, bool Descending) : IRequest<Result<UserPage>>;
[RequiresPermission("Users.Manage")]
public sealed record GetUserQuery(long UserId) : IRequest<Result<UserDetails>>;
[RequiresPermission("Users.Manage")]
public sealed record ListIdentityProviderOptionsQuery() : IRequest<Result<IReadOnlyList<IdentityProviderOption>>>;
[RequiresPermission("Users.Manage")]
public sealed record CreateUserCommand(string Username, string DisplayName, string? Email, bool IsActive) : IRequest<Result<long>>;
[RequiresPermission("Users.Manage")]
public sealed record UpdateUserCommand(long UserId, string Username, string DisplayName, string? Email, bool IsActive, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Users.Manage")]
public sealed record AddUserRoleCommand(long UserId, long RoleId) : IRequest<Result>;
[RequiresPermission("Users.Manage")]
public sealed record RemoveUserRoleCommand(long UserId, long RoleId) : IRequest<Result>;
[RequiresPermission("Users.Manage")]
public sealed record AddUserIdentityCommand(long UserId, long ProviderId, string ExternalId) : IRequest<Result<long>>;

public sealed class UserAdministrationHandler(IAuditariumDbContext db) :
    IRequestHandler<ListUsersQuery, Result<UserPage>>,
    IRequestHandler<GetUserQuery, Result<UserDetails>>,
    IRequestHandler<ListIdentityProviderOptionsQuery, Result<IReadOnlyList<IdentityProviderOption>>>,
    IRequestHandler<CreateUserCommand, Result<long>>,
    IRequestHandler<UpdateUserCommand, Result>,
    IRequestHandler<AddUserRoleCommand, Result>,
    IRequestHandler<RemoveUserRoleCommand, Result>,
    IRequestHandler<AddUserIdentityCommand, Result<long>>
{
    public async ValueTask<Result<UserPage>> Handle(ListUsersQuery query, CancellationToken ct)
    {
        if (query.Skip < 0 || query.Take is < 1 or > 200 || query.Sort is not ("username" or "displayName" or "email" or "isActive"))
            return Failure<UserPage>("USER.LIST_ARGUMENT_INVALID");
        var users = db.Users.AsNoTracking().Where(x => x.UserKey != "SYSTEM");
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            users = users.Where(x => x.Username.Contains(search) || x.DisplayName.Contains(search) || (x.Email != null && x.Email.Contains(search)));
        }
        if (query.IsActive is { } active) users = users.Where(x => x.IsActive == active);
        var total = await users.LongCountAsync(ct);
        var ordered = (query.Sort, query.Descending) switch
        {
            ("displayName", false) => users.OrderBy(x => x.DisplayName).ThenBy(x => x.Username),
            ("displayName", true) => users.OrderByDescending(x => x.DisplayName).ThenBy(x => x.Username),
            ("email", false) => users.OrderBy(x => x.Email).ThenBy(x => x.Username),
            ("email", true) => users.OrderByDescending(x => x.Email).ThenBy(x => x.Username),
            ("isActive", false) => users.OrderBy(x => x.IsActive).ThenBy(x => x.Username),
            ("isActive", true) => users.OrderByDescending(x => x.IsActive).ThenBy(x => x.Username),
            ("username", true) => users.OrderByDescending(x => x.Username),
            _ => users.OrderBy(x => x.Username)
        };
        var items = await ordered.Skip(query.Skip).Take(query.Take)
            .Select(x => new UserListItem(x.UserId, x.UserKey, x.Username, x.DisplayName, x.Email, x.IsActive, x.ConcurrencyVersion)).ToListAsync(ct);
        return Result<UserPage>.Success(new(items, total));
    }

    public async ValueTask<Result<UserDetails>> Handle(GetUserQuery query, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == query.UserId && x.UserKey != "SYSTEM", ct);
        if (user is null) return Failure<UserDetails>("USER.NOT_FOUND", ErrorType.NotFound);
        var assigned = await db.UserRoles.AsNoTracking().Where(x => x.UserId == user.UserId).Select(x => x.RoleId).ToListAsync(ct);
        var roles = await db.Roles.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new UserRoleItem(x.RoleId, x.RoleKey, x.Name, x.IsActive, assigned.Contains(x.RoleId))).ToListAsync(ct);
        var identities = await (from identity in db.UserIdentities.AsNoTracking()
                                join provider in db.AuthenticationProviders.AsNoTracking() on identity.AuthenticationProviderId equals provider.AuthenticationProviderId
                                where identity.UserId == user.UserId
                                orderby provider.ProviderKey
                                select new UserIdentityItem(identity.IdentityId, provider.AuthenticationProviderId, provider.ProviderKey, provider.ProviderType, provider.DisplayName, identity.ExternalId)).ToListAsync(ct);
        return Result<UserDetails>.Success(new(user.UserId, user.UserKey, user.Username, user.DisplayName, user.Email, user.IsActive, roles, identities, user.ConcurrencyVersion));
    }

    public async ValueTask<Result<IReadOnlyList<IdentityProviderOption>>> Handle(ListIdentityProviderOptionsQuery query, CancellationToken ct)
    {
        var providers = await db.AuthenticationProviders.AsNoTracking().Where(x => x.ProviderKey != "LOCAL").OrderBy(x => x.ProviderKey)
            .Select(x => new IdentityProviderOption(x.AuthenticationProviderId, x.ProviderKey, x.ProviderType, x.DisplayName, x.IsEnabled)).ToListAsync(ct);
        return Result<IReadOnlyList<IdentityProviderOption>>.Success(providers);
    }

    public async ValueTask<Result<long>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var validation = Validate(command.Username, command.DisplayName, command.Email);
        if (validation is not null) return Failure<long>(validation);
        var username = NormalizeUsername(command.Username);
        if (await db.Users.AnyAsync(x => x.Username == username, ct)) return Failure<long>("USER.USERNAME_EXISTS", ErrorType.Conflict);
        var user = new User { Username = username, DisplayName = command.DisplayName.Trim(), Email = Null(command.Email), IsActive = command.IsActive };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return Result<long>.Success(user.UserId);
    }

    public async ValueTask<Result> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var validation = Validate(command.Username, command.DisplayName, command.Email);
        if (validation is not null) return Failure(validation);
        var user = await db.Users.SingleOrDefaultAsync(x => x.UserId == command.UserId, ct);
        if (user is null || user.UserKey == "SYSTEM") return Failure("USER.NOT_FOUND", ErrorType.NotFound);
        if (user.ConcurrencyVersion != command.ConcurrencyVersion) return Failure("USER.CONCURRENCY_CONFLICT", ErrorType.Conflict);
        var username = NormalizeUsername(command.Username);
        if (user.UserKey is not null && !string.Equals(user.Username, username, StringComparison.Ordinal)) return Failure("USER.SYSTEM_MANAGED_USERNAME", ErrorType.Forbidden);
        if (await db.Users.AnyAsync(x => x.UserId != user.UserId && x.Username == username, ct)) return Failure("USER.USERNAME_EXISTS", ErrorType.Conflict);
        user.Username = username; user.DisplayName = command.DisplayName.Trim(); user.Email = Null(command.Email); user.IsActive = command.IsActive;
        return await SaveAsync("USER.CONCURRENCY_CONFLICT", ct);
    }

    public async ValueTask<Result> Handle(AddUserRoleCommand command, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == command.UserId, ct);
        if (user is null || user.UserKey == "SYSTEM") return Failure("USER.NOT_FOUND", ErrorType.NotFound);
        if (user.UserKey is not null) return Failure("USER.SYSTEM_MANAGED_ROLES", ErrorType.Forbidden);
        if (!await db.Roles.AnyAsync(x => x.RoleId == command.RoleId, ct)) return Failure("ROLE.NOT_FOUND", ErrorType.NotFound);
        if (!await db.UserRoles.AnyAsync(x => x.UserId == command.UserId && x.RoleId == command.RoleId, ct))
        {
            db.UserRoles.Add(new UserRole { UserId = command.UserId, RoleId = command.RoleId });
            await db.SaveChangesAsync(ct);
        }
        return Result.Success();
    }

    public async ValueTask<Result> Handle(RemoveUserRoleCommand command, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == command.UserId, ct);
        if (user is null || user.UserKey == "SYSTEM") return Failure("USER.NOT_FOUND", ErrorType.NotFound);
        if (user.UserKey is not null) return Failure("USER.SYSTEM_MANAGED_ROLES", ErrorType.Forbidden);
        var assignment = await db.UserRoles.SingleOrDefaultAsync(x => x.UserId == command.UserId && x.RoleId == command.RoleId, ct);
        if (assignment is not null) { db.UserRoles.Remove(assignment); await db.SaveChangesAsync(ct); }
        return Result.Success();
    }

    public async ValueTask<Result<long>> Handle(AddUserIdentityCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.ExternalId)) return Failure<long>("USER_IDENTITY.EXTERNAL_ID_REQUIRED");
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == command.UserId && x.UserKey == null, ct);
        if (user is null) return Failure<long>("USER.NOT_FOUND", ErrorType.NotFound);
        var provider = await db.AuthenticationProviders.AsNoTracking().SingleOrDefaultAsync(x => x.AuthenticationProviderId == command.ProviderId, ct);
        if (provider is null) return Failure<long>("AUTH_PROVIDER.NOT_FOUND", ErrorType.NotFound);
        if (provider.ProviderKey == "LOCAL") return Failure<long>("USER_IDENTITY.LOCAL_NOT_ADMINISTRABLE", ErrorType.Forbidden);
        var externalId = command.ExternalId.Trim();
        if (await db.UserIdentities.AnyAsync(x => x.AuthenticationProviderId == command.ProviderId && x.ExternalId == externalId, ct)) return Failure<long>("USER_IDENTITY.ALREADY_EXISTS", ErrorType.Conflict);
        var identity = new UserIdentity { UserId = command.UserId, AuthenticationProviderId = command.ProviderId, ExternalId = externalId };
        db.UserIdentities.Add(identity); await db.SaveChangesAsync(ct);
        return Result<long>.Success(identity.IdentityId);
    }

    private async Task<Result> SaveAsync(string code, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); return Result.Success(); }
        catch (DbUpdateConcurrencyException) { return Failure(code, ErrorType.Conflict); }
    }
    private static string? Validate(string username, string displayName, string? email)
    {
        if (string.IsNullOrWhiteSpace(username)) return "USER.USERNAME_REQUIRED";
        if (string.IsNullOrWhiteSpace(displayName)) return "USER.DISPLAY_NAME_REQUIRED";
        if (username.Trim().Length > 256 || displayName.Trim().Length > 256 || (email?.Trim().Length ?? 0) > 320) return "USER.VALUE_TOO_LONG";
        return null;
    }
    private static string NormalizeUsername(string value) => value.Trim().ToLowerInvariant();
    private static string? Null(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static Result Failure(string code, ErrorType type = ErrorType.Validation) => Result.Failure(new AppError(code, type));
    private static Result<T> Failure<T>(string code, ErrorType type = ErrorType.Validation) => Result<T>.Failure(new AppError(code, type));
}
