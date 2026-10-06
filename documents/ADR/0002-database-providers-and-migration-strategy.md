# ADR 0002 – Database Providers & Migration Strategy

## Status

Accepted

## Context

Auditarium must support PostgreSQL and Microsoft SQL Server without creating separate application data models or allowing a running application to operate against an unknown newer schema.

## Decision

PostgreSQL and Microsoft SQL Server are supported relational providers using the shared `AuditariumDbContext` EF Core model. Provider-specific migration assemblies are maintained separately, and startup selects the matching assembly from the configured provider. Application-model schema evolution remains provider-neutral while migrations may use provider-specific mechanics. Startup rejects a database whose applied migrations are newer than those known to the running application.

## Consequences

Provider behavior is deliberately isolated to persistence configuration and migration projects. New model changes require compatible migrations for both providers; a newer database cannot silently run with an older executable.

## References

- [DbContext](../../Persistence/Auditarium.Dal/AuditariumDbContext.cs) and [provider registration/startup check](../../Persistence/Auditarium.Dal/ServiceCollectionExtensions.cs)
- [PostgreSQL migrations](../../Persistence/Auditarium.Dal.PostgreSql.Migrations/) and [SQL Server migrations](../../Persistence/Auditarium.Dal.SqlServer.Migrations/)
- [design-time factories](../../Persistence/Auditarium.Dal.PostgreSql.Migrations/Design/Factory.cs) and [SQL Server factory](../../Persistence/Auditarium.Dal.SqlServer.Migrations/Design/Factory.cs)
- [provider matrix integration tests](../../Tests/Auditarium.Persistence.IntegrationTests/ProviderMatrixReferenceScenarioIntegrationTests.cs)
