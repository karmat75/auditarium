// SPDX-License-Identifier: MIT
using System.Text.Json;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auditarium.Dal;

public sealed class AuditariumDbContext : DbContext, IAuditariumDbContext
{
    private readonly ICurrentActor _currentActor;
    private bool _writingAuditLog;
    public AuditariumDbContext(DbContextOptions<AuditariumDbContext> options) : this(options, SystemCurrentActor.Instance) { }
    public AuditariumDbContext(DbContextOptions<AuditariumDbContext> options, ICurrentActor currentActor) : base(options) => _currentActor = currentActor;

    public DbSet<User> Users => Set<User>(); public DbSet<AuthenticationProvider> AuthenticationProviders => Set<AuthenticationProvider>(); public DbSet<UserIdentity> UserIdentities => Set<UserIdentity>(); public DbSet<LocalCredential> LocalCredentials => Set<LocalCredential>(); public DbSet<ApiCredential> ApiCredentials => Set<ApiCredential>(); public DbSet<Permission> Permissions => Set<Permission>(); public DbSet<Role> Roles => Set<Role>(); public DbSet<RolePermission> RolePermissions => Set<RolePermission>(); public DbSet<UserRole> UserRoles => Set<UserRole>(); public DbSet<ApplicationSetting> ApplicationSettings => Set<ApplicationSetting>(); public DbSet<SystemAuditLog> SystemAuditLogs => Set<SystemAuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("auditarium");
        b.Entity<User>(e => { e.ToTable("users"); e.HasKey(x => x.UserId); e.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedOnAdd(); e.Property(x => x.UserKey).HasColumnName("user_key").HasMaxLength(64); e.HasIndex(x => x.UserKey).IsUnique().HasFilter("user_key IS NOT NULL"); e.Property(x => x.Username).HasColumnName("username").HasMaxLength(256); e.HasIndex(x => x.Username).IsUnique(); e.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(256); e.Property(x => x.Email).HasColumnName("email").HasMaxLength(320); Version(e.Property(x => x.ConcurrencyVersion)); });
        b.Entity<AuthenticationProvider>(e => { e.ToTable("authentication_providers"); e.HasKey(x => x.AuthenticationProviderId); e.Property(x => x.AuthenticationProviderId).HasColumnName("authentication_provider_id"); e.Property(x => x.ProviderKey).HasColumnName("provider_key").HasMaxLength(64); e.HasIndex(x => x.ProviderKey).IsUnique(); e.Property(x => x.ProviderType).HasColumnName("provider_type").HasMaxLength(64); e.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(256); Version(e.Property(x => x.ConcurrencyVersion)); });
        b.Entity<UserIdentity>(e => { e.ToTable("user_identities"); e.HasKey(x => x.IdentityId); e.Property(x => x.IdentityId).HasColumnName("identity_id"); e.Property(x => x.UserId).HasColumnName("user_id"); e.Property(x => x.AuthenticationProviderId).HasColumnName("authentication_provider_id"); e.Property(x => x.ExternalId).HasColumnName("external_id").HasMaxLength(256); e.HasIndex(x => new { x.AuthenticationProviderId, x.ExternalId }).IsUnique(); e.HasOne(x => x.User).WithMany(x => x.Identities).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); e.HasOne(x => x.AuthenticationProvider).WithMany().HasForeignKey(x => x.AuthenticationProviderId).OnDelete(DeleteBehavior.Restrict); });
        b.Entity<LocalCredential>(e => { e.ToTable("local_credentials"); e.HasKey(x => x.IdentityId); e.Property(x => x.IdentityId).HasColumnName("identity_id"); e.Property(x => x.PasswordHash).HasColumnName("password_hash"); e.Property(x => x.PasswordChangedAt).HasColumnName("password_changed_at"); e.Property(x => x.MustChangePassword).HasColumnName("must_change_password"); e.Property(x => x.FailedAttemptCount).HasColumnName("failed_attempt_count"); e.Property(x => x.FailedAttemptWindowStartedAt).HasColumnName("failed_attempt_window_started_at"); e.Property(x => x.LockoutUntil).HasColumnName("lockout_until"); e.HasOne(x => x.Identity).WithOne(x => x.LocalCredential).HasForeignKey<LocalCredential>(x => x.IdentityId).OnDelete(DeleteBehavior.Cascade); });
        b.Entity<ApiCredential>(e => { e.ToTable("api_credentials"); e.HasKey(x => x.ApiCredentialId); e.Property(x => x.ApiCredentialId).HasColumnName("api_credential_id"); e.Property(x => x.IdentityId).HasColumnName("identity_id"); e.Property(x => x.KeyId).HasColumnName("key_id").HasMaxLength(128); e.HasIndex(x => x.KeyId).IsUnique(); e.Property(x => x.SecretHash).HasColumnName("secret_hash"); e.Property(x => x.Name).HasColumnName("name").HasMaxLength(256); e.Property(x => x.CreatedAt).HasColumnName("created_at"); e.Property(x => x.ExpiresAt).HasColumnName("expires_at"); e.Property(x => x.RevokedAt).HasColumnName("revoked_at"); e.Property(x => x.LastUsedAt).HasColumnName("last_used_at"); e.HasOne(x => x.Identity).WithMany().HasForeignKey(x => x.IdentityId).OnDelete(DeleteBehavior.Cascade); });
        b.Entity<Permission>(e => { e.ToTable("permissions"); e.HasKey(x => x.PermissionKey); e.Property(x => x.PermissionKey).HasColumnName("permission_key").HasMaxLength(256); e.Property(x => x.Description).HasColumnName("description").HasMaxLength(512); });
        b.Entity<Role>(e => { e.ToTable("roles"); e.HasKey(x => x.RoleId); e.Property(x => x.RoleId).HasColumnName("role_id"); e.Property(x => x.RoleKey).HasColumnName("role_key").HasMaxLength(64); e.HasIndex(x => x.RoleKey).IsUnique().HasFilter("role_key IS NOT NULL"); e.Property(x => x.Name).HasColumnName("name").HasMaxLength(256); e.Property(x => x.Description).HasColumnName("description").HasMaxLength(512); Version(e.Property(x => x.ConcurrencyVersion)); });
        b.Entity<RolePermission>(e => { e.ToTable("role_permissions"); e.HasKey(x => new { x.RoleId, x.PermissionKey }); e.Property(x => x.RoleId).HasColumnName("role_id"); e.Property(x => x.PermissionKey).HasColumnName("permission_key").HasMaxLength(256); e.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade); e.HasOne<Permission>().WithMany().HasForeignKey(x => x.PermissionKey).OnDelete(DeleteBehavior.Cascade); });
        b.Entity<UserRole>(e => { e.ToTable("user_roles"); e.HasKey(x => new { x.UserId, x.RoleId }); e.Property(x => x.UserId).HasColumnName("user_id"); e.Property(x => x.RoleId).HasColumnName("role_id"); e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); e.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict); });
        b.Entity<ApplicationSetting>(e => { e.ToTable("application_settings"); e.HasKey(x => x.SettingKey); e.Property(x => x.SettingKey).HasColumnName("setting_key").HasMaxLength(256); e.Property(x => x.SerializedValue).HasColumnName("serialized_value"); Version(e.Property(x => x.ConcurrencyVersion)); });
        b.Entity<SystemAuditLog>(e => { e.ToTable("system_audit_log"); e.HasKey(x => x.EventId); e.Property(x => x.EventId).HasColumnName("event_id").ValueGeneratedOnAdd(); e.Property(x => x.OccurredAt).HasColumnName("occurred_at"); e.Property(x => x.UserId).HasColumnName("user_id"); e.Property(x => x.Action).HasColumnName("action").HasMaxLength(64); e.Property(x => x.ObjectType).HasColumnName("object_type").HasMaxLength(128); e.Property(x => x.ObjectId).HasColumnName("object_id"); e.Property(x => x.BeforeState).HasColumnName("before_state"); e.Property(x => x.AfterState).HasColumnName("after_state"); e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); e.HasIndex(x => new { x.ObjectType, x.ObjectId, x.Action, x.OccurredAt }); });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_writingAuditLog) return await base.SaveChangesAsync(cancellationToken);
        foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Modified)) { var version = entry.Metadata.FindProperty("ConcurrencyVersion"); if (version is not null) entry.Property("ConcurrencyVersion").CurrentValue = ((long)entry.Property("ConcurrencyVersion").OriginalValue!) + 1; }
        var changes = ChangeTracker.Entries().Where(e => e.Entity is not SystemAuditLog && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).Select(AuditChange.Create).ToList();
        if (changes.Count == 0 || !await Users.AsNoTracking().AnyAsync(x => x.UserId == 0, cancellationToken)) return await base.SaveChangesAsync(cancellationToken);
        var transaction = Database.CurrentTransaction;
        var ownsTransaction = transaction is null;
        if (ownsTransaction) transaction = await Database.BeginTransactionAsync(cancellationToken);
        else await transaction!.CreateSavepointAsync("auditarium_save_changes", cancellationToken);
        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            foreach (var change in changes) SystemAuditLogs.Add(change.ToLog(_currentActor.UserId ?? 0));
            _writingAuditLog = true; await base.SaveChangesAsync(cancellationToken); _writingAuditLog = false;
            if (ownsTransaction) await transaction!.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            _writingAuditLog = false;
            if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
            else await transaction!.RollbackToSavepointAsync("auditarium_save_changes", cancellationToken);
            throw;
        }
        finally { if (ownsTransaction && transaction is not null) await transaction.DisposeAsync(); }
    }

    private static void Version(PropertyBuilder<long> p) => p.HasColumnName("concurrency_version").IsConcurrencyToken().HasDefaultValue(1);

    private sealed record AuditChange(EntityEntry Entry, string Action, string ObjectType, Dictionary<string, object?> Before, Dictionary<string, object?> After)
    {
        private static readonly IReadOnlyDictionary<string, string[]> Allowed = new Dictionary<string, string[]>(StringComparer.Ordinal) { [nameof(User)] = [nameof(User.IsActive)], [nameof(AuthenticationProvider)] = [nameof(AuthenticationProvider.ProviderKey), nameof(AuthenticationProvider.ProviderType), nameof(AuthenticationProvider.IsEnabled)], [nameof(Role)] = [nameof(Role.RoleKey), nameof(Role.Name), nameof(Role.Description), nameof(Role.IsActive)], [nameof(Permission)] = [nameof(Permission.PermissionKey), nameof(Permission.Description), nameof(Permission.IsActive)], [nameof(ApiCredential)] = [nameof(ApiCredential.Name), nameof(ApiCredential.CreatedAt), nameof(ApiCredential.ExpiresAt), nameof(ApiCredential.RevokedAt)], [nameof(ApplicationSetting)] = [nameof(ApplicationSetting.SettingKey)] };
        public static AuditChange Create(EntityEntry entry)
        {
            var before = new Dictionary<string, object?>(); var after = new Dictionary<string, object?>(); var type = entry.Metadata.ClrType.Name;
            if (Allowed.TryGetValue(type, out var allowed)) foreach (var p in entry.Properties.Where(p => allowed.Contains(p.Metadata.Name, StringComparer.Ordinal) && (entry.State != EntityState.Modified || p.IsModified))) { if (entry.State is EntityState.Modified or EntityState.Deleted) before[AuditPropertyName(p.Metadata.Name)] = p.OriginalValue; if (entry.State is EntityState.Modified or EntityState.Added) after[AuditPropertyName(p.Metadata.Name)] = p.CurrentValue; }
            return new(entry, entry.State == EntityState.Added ? "CREATED" : entry.State == EntityState.Deleted ? "PURGED" : "UPDATED", type, before, after);
        }
        public SystemAuditLog ToLog(long actor) { AuditLogContract.Validate(Action, ObjectType); return new() { OccurredAt = DateTimeOffset.UtcNow, UserId = actor, Action = Action, ObjectType = ObjectType, ObjectId = Id(Entry), BeforeState = Before.Count == 0 ? null : JsonSerializer.Serialize(Before), AfterState = After.Count == 0 ? null : JsonSerializer.Serialize(After) }; }
        private static long? Id(EntityEntry entry) { var key = entry.Metadata.FindPrimaryKey(); return key?.Properties.Count == 1 && entry.Property(key.Properties[0].Name).CurrentValue is long id ? id : null; }
        private static string AuditPropertyName(string name) => string.Concat(name.Select((character, index) => index > 0 && char.IsUpper(character) ? "_" + char.ToLowerInvariant(character) : char.ToLowerInvariant(character).ToString()));
    }
    private sealed class SystemCurrentActor : ICurrentActor { public static readonly SystemCurrentActor Instance = new(); public ActorType Type => ActorType.System; public long? UserId => 0; public bool IsAuthenticated => true; }
}
