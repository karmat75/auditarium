# ADR 0011 – Development Demo Data Invocation & Admin Provisioning

## Status

Accepted

## Context

Development demo tooling needs a controlled way to prepare the existing
`DEFAULT_ADMIN` account for operating a demonstration environment without
changing the normal RBAC or bootstrap paths.

## Decision

`Auditarium.DemoData` may apply this operation only when the host environment
is exactly `Development`, `Auditarium:DemoData:Enabled` is explicitly `true`,
and the invocation explicitly supplies `apply --confirm`. These gates are
evaluated before the tool registers or resolves persistence services.

The tool targets an already migrated and normally bootstrapped Development
database. It never runs database migrations, bootstrap, or role reconciliation.
It validates the existing `DEFAULT_ADMIN` and active system roles, then
transactionally adds only the missing regular roles `SYSTEM_ADMIN`,
`AUDIT_MANAGER`, `AUDITOR`, `REVIEWER`, and `VIEWER`.

`SYSTEM_INTERNAL` is never assigned to an interactive user. If it is already
assigned to `DEFAULT_ADMIN`, the operation fails without interpreting or
repairing that state. Normal BLL, Web, and API role administration remains
unchanged, including its protection for system-managed users. Normal bootstrap
continues to assign `DEFAULT_ADMIN` only `SYSTEM_ADMIN`.

## Consequences

The narrowly scoped Development operation can use the regular EF persistence
and system audit-log behavior while remaining outside product administration.
It does not establish a superuser mechanism or create business demo fixtures.

## References

- [development DemoData boundary](0010-development-demo-data-tool-and-distribution-boundary.md)
- [identity and access architecture](../Architecture/Identity-and-Access.md)
- [DemoData tool](../../Tools/Auditarium.DemoData/)
