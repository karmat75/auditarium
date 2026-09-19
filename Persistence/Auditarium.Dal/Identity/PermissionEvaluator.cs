// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Dal.Identity;

internal sealed class PermissionEvaluator(AuditariumDbContext db) : IPermissionEvaluator
{
    public async Task<bool> HasPermissionAsync(long userId, string permission, CancellationToken cancellationToken = default) =>
        (await GetPermissionsAsync(userId, cancellationToken)).Contains(permission);

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(long userId, CancellationToken cancellationToken = default)
    {
        // user_id 0 is an internal actor only; ordinary inactive users always deny.
        if (userId != 0 && !await db.Users.AsNoTracking().AnyAsync(x => x.UserId == userId && x.IsActive, cancellationToken))
            return new HashSet<string>(StringComparer.Ordinal);
        return await (from ur in db.UserRoles.AsNoTracking()
                      join role in db.Roles.AsNoTracking() on ur.RoleId equals role.RoleId
                      join rp in db.RolePermissions.AsNoTracking() on role.RoleId equals rp.RoleId
                      join permission in db.Permissions.AsNoTracking() on rp.PermissionKey equals permission.PermissionKey
                      where ur.UserId == userId && role.IsActive && permission.IsActive
                      select permission.PermissionKey).ToHashSetAsync(StringComparer.Ordinal, cancellationToken);
    }
}
