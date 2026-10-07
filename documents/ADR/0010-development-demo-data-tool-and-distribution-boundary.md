# ADR 0010 – Development Demo Data Tool & Distribution Boundary

## Status

Accepted

## Context

Development demo data must be available without becoming part of Auditarium's
normal runtime, technical bootstrap, or production distribution.

## Decision

`Auditarium.DemoData` is development tooling in a separate console project
that remains as source in this repository. It is not part of the Auditarium Web
or API runtime: it exposes no endpoints, registers no jobs, and is not run
automatically. It is not a regular database seed.

Future demo business data is created through the regular BLL use cases and
therefore their validation, state-transition, concurrency, and audit-logging
rules.

Production artifacts must not contain the DemoData executable or assembly,
DemoData scripts, a dedicated demo container image, or demo-specific production
configuration. A later release pipeline must enforce this boundary; that
pipeline is not introduced by this decision.

## Consequences

Development tooling remains separately executable from a cloned repository,
while Web, API, bootstrap, and current production outputs remain unaffected.

## References

- [DemoData console project](../../Tools/Auditarium.DemoData/)
- [architecture hardening tests](../../Tests/Auditarium.Infrastructure.Security.Tests/ArchitectureHardeningTests.cs)
- [current product container build](../../Dockerfile)
