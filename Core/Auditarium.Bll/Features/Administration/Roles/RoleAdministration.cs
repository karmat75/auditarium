// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Administration.Roles;

public sealed record RoleListItem(long RoleId, string? RoleKey, string Name, string Description, bool IsActive, int UserCount, int PermissionCount, long ConcurrencyVersion);
public sealed record RolePermissionItem(string PermissionKey, string Description, bool IsActive, bool Assigned);
public sealed record RoleDetails(long RoleId, string? RoleKey, string Name, string Description, bool IsActive, IReadOnlyList<RolePermissionItem> Permissions, long ConcurrencyVersion);

[RequiresPermission("Roles.Manage")] public sealed record ListRolesQuery() : IRequest<Result<IReadOnlyList<RoleListItem>>>;
[RequiresPermission("Roles.Manage")] public sealed record GetRoleQuery(long RoleId) : IRequest<Result<RoleDetails>>;
[RequiresPermission("Roles.Manage")] public sealed record CreateRoleCommand(string Name, string? Description, bool IsActive) : IRequest<Result<long>>;
[RequiresPermission("Roles.Manage")] public sealed record UpdateRoleCommand(long RoleId, string Name, string? Description, bool IsActive, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Roles.Manage")] public sealed record AddPermissionToRoleCommand(long RoleId, string PermissionKey) : IRequest<Result>;
[RequiresPermission("Roles.Manage")] public sealed record RemovePermissionFromRoleCommand(long RoleId, string PermissionKey) : IRequest<Result>;

public sealed class RoleAdministrationHandler(IAuditariumDbContext db) :
    IRequestHandler<ListRolesQuery, Result<IReadOnlyList<RoleListItem>>>, IRequestHandler<GetRoleQuery, Result<RoleDetails>>,
    IRequestHandler<CreateRoleCommand, Result<long>>, IRequestHandler<UpdateRoleCommand, Result>,
    IRequestHandler<AddPermissionToRoleCommand, Result>, IRequestHandler<RemovePermissionFromRoleCommand, Result>
{
    public async ValueTask<Result<IReadOnlyList<RoleListItem>>> Handle(ListRolesQuery query, CancellationToken ct)
    {
        var roles = await db.Roles.AsNoTracking().OrderBy(x => x.RoleKey == null).ThenBy(x => x.Name)
            .Select(x => new RoleListItem(x.RoleId, x.RoleKey, x.Name, x.Description, x.IsActive,
                db.UserRoles.Count(link => link.RoleId == x.RoleId), db.RolePermissions.Count(link => link.RoleId == x.RoleId), x.ConcurrencyVersion)).ToListAsync(ct);
        return Result<IReadOnlyList<RoleListItem>>.Success(roles);
    }

    public async ValueTask<Result<RoleDetails>> Handle(GetRoleQuery query, CancellationToken ct)
    {
        var role = await db.Roles.AsNoTracking().SingleOrDefaultAsync(x => x.RoleId == query.RoleId, ct);
        if (role is null) return Failure<RoleDetails>("ROLE.NOT_FOUND", ErrorType.NotFound);
        var assigned = await db.RolePermissions.AsNoTracking().Where(x => x.RoleId == role.RoleId).Select(x => x.PermissionKey).ToListAsync(ct);
        var permissions = await db.Permissions.AsNoTracking().OrderBy(x => x.PermissionKey)
            .Select(x => new RolePermissionItem(x.PermissionKey, x.Description, x.IsActive, assigned.Contains(x.PermissionKey))).ToListAsync(ct);
        return Result<RoleDetails>.Success(new(role.RoleId, role.RoleKey, role.Name, role.Description, role.IsActive, permissions, role.ConcurrencyVersion));
    }

    public async ValueTask<Result<long>> Handle(CreateRoleCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) return Failure<long>("ROLE.NAME_REQUIRED");
        if (command.Name.Trim().Length > 256 || (command.Description?.Trim().Length ?? 0) > 512) return Failure<long>("ROLE.VALUE_TOO_LONG");
        var role = new Role { Name = command.Name.Trim(), Description = command.Description?.Trim() ?? string.Empty, IsActive = command.IsActive };
        db.Roles.Add(role); await db.SaveChangesAsync(ct); return Result<long>.Success(role.RoleId);
    }

    public async ValueTask<Result> Handle(UpdateRoleCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) return Failure("ROLE.NAME_REQUIRED");
        var role = await db.Roles.SingleOrDefaultAsync(x => x.RoleId == command.RoleId, ct);
        if (role is null) return Failure("ROLE.NOT_FOUND", ErrorType.NotFound);
        if (role.RoleKey is not null) return Failure("ROLE.SYSTEM_MANAGED", ErrorType.Forbidden);
        if (role.ConcurrencyVersion != command.ConcurrencyVersion) return Failure("ROLE.CONCURRENCY_CONFLICT", ErrorType.Conflict);
        role.Name = command.Name.Trim(); role.Description = command.Description?.Trim() ?? string.Empty; role.IsActive = command.IsActive;
        return await SaveAsync(ct);
    }

    public async ValueTask<Result> Handle(AddPermissionToRoleCommand command, CancellationToken ct)
    {
        var role = await EditableRoleAsync(command.RoleId, ct); if (!role.IsSuccess) return role;
        if (!await db.Permissions.AnyAsync(x => x.PermissionKey == command.PermissionKey, ct)) return Failure("PERMISSION.NOT_FOUND", ErrorType.NotFound);
        if (!await db.RolePermissions.AnyAsync(x => x.RoleId == command.RoleId && x.PermissionKey == command.PermissionKey, ct))
        { db.RolePermissions.Add(new RolePermission { RoleId = command.RoleId, PermissionKey = command.PermissionKey }); await db.SaveChangesAsync(ct); }
        return Result.Success();
    }

    public async ValueTask<Result> Handle(RemovePermissionFromRoleCommand command, CancellationToken ct)
    {
        var role = await EditableRoleAsync(command.RoleId, ct); if (!role.IsSuccess) return role;
        var link = await db.RolePermissions.SingleOrDefaultAsync(x => x.RoleId == command.RoleId && x.PermissionKey == command.PermissionKey, ct);
        if (link is not null) { db.RolePermissions.Remove(link); await db.SaveChangesAsync(ct); }
        return Result.Success();
    }

    private async Task<Result> EditableRoleAsync(long roleId, CancellationToken ct)
    {
        var role = await db.Roles.AsNoTracking().SingleOrDefaultAsync(x => x.RoleId == roleId, ct);
        if (role is null) return Failure("ROLE.NOT_FOUND", ErrorType.NotFound);
        return role.RoleKey is null ? Result.Success() : Failure("ROLE.SYSTEM_MANAGED", ErrorType.Forbidden);
    }
    private async Task<Result> SaveAsync(CancellationToken ct) { try { await db.SaveChangesAsync(ct); return Result.Success(); } catch (DbUpdateConcurrencyException) { return Failure("ROLE.CONCURRENCY_CONFLICT", ErrorType.Conflict); } }
    private static Result Failure(string code, ErrorType type = ErrorType.Validation) => Result.Failure(new AppError(code, type));
    private static Result<T> Failure<T>(string code, ErrorType type = ErrorType.Validation) => Result<T>.Failure(new AppError(code, type));
}
