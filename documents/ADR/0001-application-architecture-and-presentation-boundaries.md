# ADR 0001 – Application Architecture & Presentation Boundaries

## Status

Accepted

## Context

Auditarium provides both a server-rendered Web application and an HTTP API while preserving one business implementation and testable dependency boundaries.

## Decision

Auditarium is a structured monolith. Web and API are presentation/transport layers that invoke BLL use cases through Mediator. Business rules belong in the BLL; DAL, FAL, and infrastructure implement outer technical concerns. Web does not call Auditarium's own `/api/v1` API as its internal application backend. Web and API may expose the same BLL use cases without duplicating business semantics.

## Consequences

Presentation-specific concerns remain at the edge, while use-case behavior is shared and independently testable. Dependency and architecture tests are enforcement evidence; bypassing the BLL or introducing a Web-to-own-API path violates this boundary.

## References

- [Web composition root](../../UI/Auditarium.Web/Program.cs) and [API composition root](../../UI/Auditarium.Api/Program.cs)
- [BLL registration](../../Core/Auditarium.Bll/ServiceCollectionExtensions.cs) and [authorization pipeline](../../Core/Auditarium.Bll/Pipeline/AuthorizationBehavior.cs)
- [architecture hardening tests](../../Tests/Auditarium.Infrastructure.Security.Tests/ArchitectureHardeningTests.cs) and [presentation foundation tests](../../Tests/Auditarium.Infrastructure.Security.Tests/PresentationFoundationTests.cs)
