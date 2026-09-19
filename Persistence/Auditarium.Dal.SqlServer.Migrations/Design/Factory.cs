using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Design; using Auditarium.Dal;
namespace Auditarium.Dal.SqlServer.Migrations.Design;
public sealed class Factory : IDesignTimeDbContextFactory<AuditariumDbContext> { public AuditariumDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AuditariumDbContext>().UseSqlServer("Server=localhost;Database=Auditarium;User Id=sa;Password=DesignOnly123!;TrustServerCertificate=True", x => x.MigrationsAssembly("Auditarium.Dal.SqlServer.Migrations")).Options); }
