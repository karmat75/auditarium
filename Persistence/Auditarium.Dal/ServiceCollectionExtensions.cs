// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Dal.Settings;
using Auditarium.Dal.Bootstrap;
using Auditarium.Dal.Configuration;
using Auditarium.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Auditarium.Dal;
public static class ServiceCollectionExtensions
{
 public static IServiceCollection AddAuditariumPersistence(this IServiceCollection services,IConfiguration configuration)
 {
   var database=configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? throw new InvalidOperationException("Auditarium:Database configuration is required.");
   if(string.IsNullOrWhiteSpace(database.ConnectionString)) throw new InvalidOperationException("Auditarium:Database:ConnectionString is required.");
   if(database.BootstrapTimeoutSeconds is < 1 or > 1800) throw new InvalidOperationException("Auditarium:Database:BootstrapTimeoutSeconds must be between 1 and 1800.");
   services.AddSingleton(database); var recovery = configuration.GetSection(RecoveryOptions.SectionName).Get<RecoveryOptions>() ?? new RecoveryOptions(); services.AddSingleton(recovery);
   services.AddScoped<IPasswordHasher<LocalCredential>,PasswordHasher<LocalCredential>>();
   services.AddDbContext<AuditariumDbContext>((_, o) => { if(database.Provider.Equals("PostgreSQL",StringComparison.OrdinalIgnoreCase)) o.UseNpgsql(database.ConnectionString, x => x.MigrationsAssembly("Auditarium.Dal.PostgreSql.Migrations").CommandTimeout(database.BootstrapTimeoutSeconds)); else if(database.Provider.Equals("SqlServer",StringComparison.OrdinalIgnoreCase)) o.UseSqlServer(database.ConnectionString, x => x.MigrationsAssembly("Auditarium.Dal.SqlServer.Migrations").CommandTimeout(database.BootstrapTimeoutSeconds)); else throw new InvalidOperationException("Auditarium:Database:Provider must be PostgreSQL or SqlServer."); });
   services.AddScoped<IAuditariumDbContext>(p=>p.GetRequiredService<AuditariumDbContext>());
   services.AddScoped<IApplicationSettingResolver, ApplicationSettingResolver>();
   return services;
 }
 public static async Task InitializeAuditariumDatabaseAsync(this IServiceProvider services,CancellationToken ct=default)
 {
   await using var scope=services.CreateAsyncScope(); var db=scope.ServiceProvider.GetRequiredService<AuditariumDbContext>(); var database=scope.ServiceProvider.GetRequiredService<DatabaseOptions>(); var recovery=scope.ServiceProvider.GetRequiredService<RecoveryOptions>(); var configuration=scope.ServiceProvider.GetRequiredService<IConfiguration>();
   var applied=await db.Database.GetAppliedMigrationsAsync(ct); var known=db.Database.GetMigrations(); if(applied.Except(known).Any()) throw new InvalidOperationException("DATABASE_SCHEMA_NEWER_THAN_APPLICATION");
   await db.Database.MigrateAsync(ct);
   await using var bootstrapLock=await BootstrapLock.AcquireAsync(db,database.Provider,TimeSpan.FromSeconds(database.BootstrapTimeoutSeconds),ct);
   var password=await new DatabaseBootstrapper(db,new PasswordHasher<LocalCredential>(),recovery,configuration).InitializeAsync(ct);
   if(password is not null) await DatabaseBootstrapper.WriteInitialCredentialAsync(password);
 }
}
