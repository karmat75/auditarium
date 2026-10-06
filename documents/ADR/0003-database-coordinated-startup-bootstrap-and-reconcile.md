# ADR 0003 – Database-Coordinated Startup, Bootstrap & Reconcile

## Status

Accepted

## Context

Multiple application instances can start against one database. Migration and bootstrap/reconcile must be safe without an additional cluster-coordination service.

## Decision

Startup coordination uses the shared database. PostgreSQL uses advisory locks and SQL Server uses `sp_getapplock`, with bounded waiting configured by the application. Migration and bootstrap/reconcile use independent coordination resources where required. Each instance re-evaluates migration state after acquiring the migration lock, then runs idempotent bootstrap/reconcile even when no migration is pending. An instance that cannot complete initialization is not released into normal operation. Locks are session/connection-scoped so process termination does not leave a permanent orphan lock.

## Consequences

The database is the sole coordination dependency for initialization. Startup can fail or time out predictably rather than running partially initialized; bootstrap can safely repair expected system-managed state on subsequent starts.

## References

- [initialization flow](../../Persistence/Auditarium.Dal/ServiceCollectionExtensions.cs), [lock implementation](../../Persistence/Auditarium.Dal/Bootstrap/BootstrapLock.cs), and [bootstrap/reconcile implementation](../../Persistence/Auditarium.Dal/Bootstrap/DatabaseBootstrapper.cs)
- [database bootstrap integration tests](../../Tests/Auditarium.Persistence.IntegrationTests/DatabaseBootstrapIntegrationTests.cs)
