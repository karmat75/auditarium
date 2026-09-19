using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Design; using Auditarium.Dal;
namespace Auditarium.Dal.PostgreSql.Migrations.Design;
public sealed class Factory : IDesignTimeDbContextFactory<AuditariumDbContext> { public AuditariumDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AuditariumDbContext>().UseNpgsql("Host=localhost;Database=auditarium;Username=auditarium;Password=design", x => x.MigrationsAssembly("Auditarium.Dal.PostgreSql.Migrations")).Options); }
