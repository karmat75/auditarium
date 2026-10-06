# ADR 0007 – Transactional System Audit Log & Controlled Retention

## Status

Accepted

## Context

Auditarium requires trustworthy technical history for relevant changes and events, but must also support explicit operational retention of that history.

## Decision

Relevant entity changes and required audit-log entries commit atomically; a required audit write failure prevents the associated successful data change from being committed. Relevant non-entity events, including successful login and export, have explicitly defined audit behavior. Sensitive state is governed by allowlists and secrets are excluded. Normal application behavior does not arbitrarily edit historical entries. Controlled retention/purge is an explicit maintenance capability. The durable decision is transactional audit logging with controlled retention, not infinite immutable storage.

## Consequences

Required audit evidence is coupled to its data change, while retention settings and the authorized maintenance job govern deletion. Consumers must not infer permanent append-only storage from the ordinary no-edit behavior.

## References

- [DbContext audit interception](../../Persistence/Auditarium.Dal/AuditariumDbContext.cs), [event writer](../../Persistence/Auditarium.Dal/AuditEventWriter.cs), and [audit log contract](../../Persistence/Auditarium.Dal/AuditLogContract.cs)
- [retention job](../../Core/Auditarium.Bll/Features/Jobs/RetentionJob.cs)
- [observability hardening tests](../../Tests/Auditarium.Infrastructure.Security.Tests/ObservabilityHardeningTests.cs), [soft-delete/retention integration tests](../../Tests/Auditarium.Persistence.IntegrationTests/SoftDeleteRetentionIntegrationTests.cs), and [final hardening tests](../../Tests/Auditarium.Persistence.IntegrationTests/FinalHardeningIntegrationTests.cs)
