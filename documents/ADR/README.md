# Architecture Decision Records

This directory contains the durable decision history for Auditarium's significant architecture decisions. Current implementation and tests remain the evidence of implemented behavior; ADRs preserve the decision and its rationale.

## Convention

- ADRs use four-digit, sequential numbers and kebab-case filenames.
- Accepted ADR decision content is immutable. Status/reference metadata may be amended solely to record supersession; the decision itself is not rewritten merely to describe a later state.
- A changed decision is recorded in a new ADR. The new ADR links back to the ADR it supersedes, and the earlier ADR gains an explicit forward reference.
- Status is stated explicitly. This initial baseline records accepted decisions only.

## Index

1. [ADR 0001 – Application Architecture & Presentation Boundaries](0001-application-architecture-and-presentation-boundaries.md)
2. [ADR 0002 – Database Providers & Migration Strategy](0002-database-providers-and-migration-strategy.md)
3. [ADR 0003 – Database-Coordinated Startup, Bootstrap & Reconcile](0003-database-coordinated-startup-bootstrap-and-reconcile.md)
4. [ADR 0004 – Authentication Provider Instance Model](0004-authentication-provider-instance-model.md)
5. [ADR 0005 – Permission-Based RBAC & Default-Deny Authorization](0005-permission-based-rbac-and-default-deny-authorization.md)
6. [ADR 0006 – Catalog Version Immutability & Audit Materialization](0006-catalog-version-immutability-and-audit-materialization.md)
7. [ADR 0007 – Transactional System Audit Log & Controlled Retention](0007-transactional-system-audit-log-and-controlled-retention.md)
8. [ADR 0008 – Database-Lease Coordination for Distributed Jobs](0008-database-lease-coordination-for-distributed-jobs.md)
9. [ADR 0009 – Local Bootstrap/AdminLTE Vendoring & Auditarium Theme Layer](0009-local-bootstrap-adminlte-vendoring-and-auditarium-theme-layer.md)
10. [ADR 0010 – Development Demo Data Tool & Distribution Boundary](0010-development-demo-data-tool-and-distribution-boundary.md)
11. [ADR 0011 – Development Demo Data Invocation & Admin Provisioning](0011-development-demo-data-invocation-and-admin-provisioning.md)
12. [ADR 0012 – Standardized Tabulator Data Grid & Tree Grid Integration](0012-standardized-tabulator-data-grid-and-tree-grid-integration.md)
