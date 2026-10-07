// SPDX-License-Identifier: MIT
using Auditarium.Bll.Settings;
using Auditarium.Dal;
using Auditarium.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.DemoData;

public sealed class DemoDataProvisioningException(string message) : InvalidOperationException(message);

public sealed class DevelopmentAdministratorProvisioner(AuditariumDbContext db)
{
    private static readonly string[] RequiredRoleKeys = ["SYSTEM_ADMIN", "AUDIT_MANAGER", "AUDITOR", "REVIEWER", "VIEWER"];

    public async Task ProvisionAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var administrator = await db.Users.SingleOrDefaultAsync(user => user.UserKey == "DEFAULT_ADMIN", cancellationToken)
            ?? throw new DemoDataProvisioningException("DEFAULT_ADMIN is missing. Initialize the Development database through normal Auditarium startup first.");

        var expectedRoleKeys = SystemRoleDefinitions.All.Select(definition => definition.Key).ToHashSet(StringComparer.Ordinal);
        var roles = await db.Roles.Where(role => role.RoleKey != null && expectedRoleKeys.Contains(role.RoleKey)).ToListAsync(cancellationToken);
        if (roles.Count != expectedRoleKeys.Count || roles.Any(role => !role.IsActive))
            throw new DemoDataProvisioningException("Required active system roles are missing. Initialize the Development database through normal Auditarium startup first.");

        var assignedRoleKeys = await (from userRole in db.UserRoles
                                      join role in db.Roles on userRole.RoleId equals role.RoleId
                                      where userRole.UserId == administrator.UserId
                                      select role.RoleKey).ToListAsync(cancellationToken);
        if (assignedRoleKeys.Contains("SYSTEM_INTERNAL", StringComparer.Ordinal))
            throw new DemoDataProvisioningException("DEFAULT_ADMIN has SYSTEM_INTERNAL assigned. DemoData will not modify this unsafe state.");

        foreach (var role in roles.Where(role => role.RoleKey is not null && RequiredRoleKeys.Contains(role.RoleKey, StringComparer.Ordinal)))
            if (!assignedRoleKeys.Contains(role.RoleKey!, StringComparer.Ordinal))
                db.UserRoles.Add(new UserRole { UserId = administrator.UserId, RoleId = role.RoleId });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
