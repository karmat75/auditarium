using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Auditarium.Dal.Design;

public sealed class AuditariumDbContextFactory : IDesignTimeDbContextFactory<AuditariumDbContext> { public AuditariumDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AuditariumDbContext>().UseNpgsql("Host=localhost;Database=auditarium;Username=auditarium;Password=design").Options); }
