# ADR 0005 – Permission-Based RBAC & Default-Deny Authorization

## Status

Accepted

## Context

Authorization must be consistent across Web, API, and internal execution, including when a user's active state or assignments change.

## Decision

Authorization is permission-based: code-defined permissions are aggregated by roles and roles are assigned to users. System-managed roles and permissions are reconciled. BLL use cases declare their requirements and `AuthorizationBehavior` enforces them centrally. Authentication alone grants no ordinary permission; inactive users lose effective permissions on subsequent authorized use cases. Internal execution uses `user_id = 0` (System Actor) and only explicitly assigned `SYSTEM_INTERNAL` permissions. Presentation layers do not implement a competing authorization model.

## Consequences

Every protected use case has one authoritative authorization path, with default denial for missing or unsatisfied declarations. System automation remains constrained by the same permission vocabulary rather than receiving unrestricted access.

## References

- [permission definitions](../../Core/Auditarium.Bll/Settings/PermissionDefinitions.cs), [system role definitions](../../Core/Auditarium.Bll/Settings/SystemRoleDefinitions.cs), and [permission evaluator abstraction](../../Core/Auditarium.Bll/Abstractions/Identity/IPermissionEvaluator.cs)
- [current actor abstraction](../../Core/Auditarium.Bll/Abstractions/Identity/ICurrentActor.cs), [central behavior](../../Core/Auditarium.Bll/Pipeline/AuthorizationBehavior.cs), and [permission evaluator](../../Persistence/Auditarium.Dal/Identity/PermissionEvaluator.cs)
- [security hardening tests](../../Tests/Auditarium.Infrastructure.Security.Tests/ArchitectureHardeningTests.cs) and [final hardening integration tests](../../Tests/Auditarium.Persistence.IntegrationTests/FinalHardeningIntegrationTests.cs)
