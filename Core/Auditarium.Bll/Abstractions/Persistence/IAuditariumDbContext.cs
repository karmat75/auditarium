// SPDX-License-Identifier: MIT
using Auditarium.Models.Identity;
using Microsoft.EntityFrameworkCore;
namespace Auditarium.Bll.Abstractions.Persistence;

public interface IAuditariumDbContext
{
    DbSet<User> Users { get; }
    DbSet<AuthenticationProvider> AuthenticationProviders { get; }
    DbSet<UserIdentity> UserIdentities { get; }
    DbSet<LocalCredential> LocalCredentials { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<ApplicationSetting> ApplicationSettings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
