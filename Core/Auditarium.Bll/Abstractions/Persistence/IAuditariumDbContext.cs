// SPDX-License-Identifier: MIT
using Auditarium.Models.Identity;
using Auditarium.Models.Catalog;
using Microsoft.EntityFrameworkCore;
namespace Auditarium.Bll.Abstractions.Persistence;

public interface IAuditariumDbContext
{
    DbSet<User> Users { get; }
    DbSet<AuthenticationProvider> AuthenticationProviders { get; }
    DbSet<UserIdentity> UserIdentities { get; }
    DbSet<LocalCredential> LocalCredentials { get; }
    DbSet<ApiCredential> ApiCredentials { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<ApplicationSetting> ApplicationSettings { get; }
    DbSet<SystemAuditLog> SystemAuditLogs { get; }
    DbSet<Document> Documents { get; }
    DbSet<CatalogVersion> CatalogVersions { get; }
    DbSet<DocumentElement> DocumentElements { get; }
    DbSet<DocumentElementWeight> DocumentElementWeights { get; }
    DbSet<Question> Questions { get; }
    DbSet<ScopeType> ScopeTypes { get; }
    DbSet<QuestionScopeType> QuestionScopeTypes { get; }
    DbSet<FileItem> FileItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
