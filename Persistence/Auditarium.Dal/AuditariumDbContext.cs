// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Auditarium.Dal;
public sealed class AuditariumDbContext(DbContextOptions<AuditariumDbContext> options) : DbContext(options), IAuditariumDbContext
{
    public DbSet<User> Users => Set<User>(); public DbSet<AuthenticationProvider> AuthenticationProviders => Set<AuthenticationProvider>(); public DbSet<UserIdentity> UserIdentities => Set<UserIdentity>(); public DbSet<LocalCredential> LocalCredentials => Set<LocalCredential>(); public DbSet<Permission> Permissions => Set<Permission>(); public DbSet<Role> Roles => Set<Role>(); public DbSet<RolePermission> RolePermissions => Set<RolePermission>(); public DbSet<UserRole> UserRoles => Set<UserRole>(); public DbSet<ApplicationSetting> ApplicationSettings => Set<ApplicationSetting>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("auditarium");
        b.Entity<User>(e => { e.ToTable("users"); e.HasKey(x=>x.UserId); e.Property(x=>x.UserId).HasColumnName("user_id").ValueGeneratedOnAdd(); e.Property(x=>x.UserKey).HasColumnName("user_key").HasMaxLength(64); e.HasIndex(x=>x.UserKey).IsUnique().HasFilter("user_key IS NOT NULL"); e.Property(x=>x.Username).HasColumnName("username").HasMaxLength(256); e.HasIndex(x=>x.Username).IsUnique(); e.Property(x=>x.DisplayName).HasColumnName("display_name").HasMaxLength(256); e.Property(x=>x.Email).HasColumnName("email").HasMaxLength(320); Version(e.Property(x=>x.ConcurrencyVersion)); });
        b.Entity<AuthenticationProvider>(e => { e.ToTable("authentication_providers"); e.HasKey(x=>x.AuthenticationProviderId); e.Property(x=>x.AuthenticationProviderId).HasColumnName("authentication_provider_id"); e.Property(x=>x.ProviderKey).HasColumnName("provider_key").HasMaxLength(64); e.HasIndex(x=>x.ProviderKey).IsUnique(); e.Property(x=>x.ProviderType).HasColumnName("provider_type").HasMaxLength(64); e.Property(x=>x.DisplayName).HasColumnName("display_name").HasMaxLength(256); Version(e.Property(x=>x.ConcurrencyVersion)); });
        b.Entity<UserIdentity>(e => { e.ToTable("user_identities"); e.HasKey(x=>x.IdentityId); e.Property(x=>x.IdentityId).HasColumnName("identity_id"); e.Property(x=>x.UserId).HasColumnName("user_id"); e.Property(x=>x.AuthenticationProviderId).HasColumnName("authentication_provider_id"); e.Property(x=>x.ExternalId).HasColumnName("external_id").HasMaxLength(256); e.HasIndex(x=>new{x.AuthenticationProviderId,x.ExternalId}).IsUnique(); e.HasOne(x=>x.User).WithMany(x=>x.Identities).HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict); e.HasOne(x=>x.AuthenticationProvider).WithMany().HasForeignKey(x=>x.AuthenticationProviderId).OnDelete(DeleteBehavior.Restrict); });
        b.Entity<LocalCredential>(e => { e.ToTable("local_credentials"); e.HasKey(x=>x.IdentityId); e.Property(x=>x.IdentityId).HasColumnName("identity_id"); e.Property(x=>x.PasswordHash).HasColumnName("password_hash"); e.Property(x=>x.PasswordChangedAt).HasColumnName("password_changed_at"); e.Property(x=>x.MustChangePassword).HasColumnName("must_change_password"); e.Property(x=>x.FailedAttemptCount).HasColumnName("failed_attempt_count"); e.Property(x=>x.FailedAttemptWindowStartedAt).HasColumnName("failed_attempt_window_started_at"); e.Property(x=>x.LockoutUntil).HasColumnName("lockout_until"); e.HasOne(x=>x.Identity).WithOne(x=>x.LocalCredential).HasForeignKey<LocalCredential>(x=>x.IdentityId).OnDelete(DeleteBehavior.Cascade); });
        b.Entity<Permission>(e => { e.ToTable("permissions"); e.HasKey(x=>x.PermissionKey); e.Property(x=>x.PermissionKey).HasColumnName("permission_key").HasMaxLength(256); e.Property(x=>x.Description).HasColumnName("description").HasMaxLength(512); });
        b.Entity<Role>(e => { e.ToTable("roles"); e.HasKey(x=>x.RoleId); e.Property(x=>x.RoleId).HasColumnName("role_id"); e.Property(x=>x.RoleKey).HasColumnName("role_key").HasMaxLength(64); e.HasIndex(x=>x.RoleKey).IsUnique().HasFilter("role_key IS NOT NULL"); e.Property(x=>x.Name).HasColumnName("name").HasMaxLength(256); e.Property(x=>x.Description).HasColumnName("description").HasMaxLength(512); Version(e.Property(x=>x.ConcurrencyVersion)); });
        b.Entity<RolePermission>(e => { e.ToTable("role_permissions"); e.HasKey(x=>new{x.RoleId,x.PermissionKey}); e.Property(x=>x.RoleId).HasColumnName("role_id"); e.Property(x=>x.PermissionKey).HasColumnName("permission_key").HasMaxLength(256); e.HasOne<Role>().WithMany().HasForeignKey(x=>x.RoleId).OnDelete(DeleteBehavior.Cascade); e.HasOne<Permission>().WithMany().HasForeignKey(x=>x.PermissionKey).OnDelete(DeleteBehavior.Cascade); });
        b.Entity<UserRole>(e => { e.ToTable("user_roles"); e.HasKey(x=>new{x.UserId,x.RoleId}); e.Property(x=>x.UserId).HasColumnName("user_id"); e.Property(x=>x.RoleId).HasColumnName("role_id"); e.HasOne<User>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict); e.HasOne<Role>().WithMany().HasForeignKey(x=>x.RoleId).OnDelete(DeleteBehavior.Restrict); });
        b.Entity<ApplicationSetting>(e => { e.ToTable("application_settings"); e.HasKey(x=>x.SettingKey); e.Property(x=>x.SettingKey).HasColumnName("setting_key").HasMaxLength(256); e.Property(x=>x.SerializedValue).HasColumnName("serialized_value"); Version(e.Property(x=>x.ConcurrencyVersion)); });
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Modified))
        {
            var version = entry.Metadata.FindProperty("ConcurrencyVersion");
            if (version is not null) entry.Property("ConcurrencyVersion").CurrentValue = ((long)entry.Property("ConcurrencyVersion").OriginalValue!)+1;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
    private static void Version(PropertyBuilder<long> p) => p.HasColumnName("concurrency_version").IsConcurrencyToken().HasDefaultValue(1);
}
