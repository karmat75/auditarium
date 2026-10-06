# ADR 0006 – Catalog Version Immutability & Audit Materialization

## Status

Accepted

## Context

Audits need reproducible meaning even as the source documents and catalog content evolve.

## Decision

Catalog versions have an explicit lifecycle: DRAFT content is editable and a valid DRAFT may become READY. Once a version is used by an audit, its historical meaning remains stable; editorial evolution uses a new or copied DRAFT instead of mutating used content. Publishing an audit materializes the applicable document/question structure and snapshot data into that audit. Later catalog edits cannot retroactively redefine a published audit.

## Consequences

Catalog immutability and audit materialization are one historical-integrity decision: the first preserves the source version, and the second preserves the audit's interpretation and execution data. This makes published results reproducible while allowing controlled editorial evolution.

## References

- [catalog commands and lifecycle guards](../../Core/Auditarium.Bll/Features/Catalog/CatalogCommands.cs) and [catalog entities](../../Core/Auditarium.Models/Catalog/CatalogEntities.cs)
- [audit publishing/materialization commands](../../Core/Auditarium.Bll/Features/Audits/AuditCommands.cs) and [audit data queries](../../Core/Auditarium.Bll/Features/Audits/AuditDataQueries.cs)
- [catalog workflow integration tests](../../Tests/Auditarium.Persistence.IntegrationTests/CatalogWorkflowIntegrationTests.cs) and [audit workflow integration tests](../../Tests/Auditarium.Persistence.IntegrationTests/AuditWorkflowIntegrationTests.cs)
