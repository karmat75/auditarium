# ADR 0008 – Database-Lease Coordination for Distributed Jobs

## Status

Accepted

## Context

Jobs may be triggered manually, on a schedule, or at startup on more than one application instance without requiring an external scheduler or lock service.

## Decision

The initial job runtime uses no external distributed scheduler or lock service. Job definitions and scheduling are application-level concepts; shared-database `job_runtime_state` coordinates execution. Lease acquisition is atomic, heartbeats and completion state support a running execution, and expired leases enable crash recovery. Concurrency and startup semantics are enforced across instances. Manual, scheduled, and startup triggers converge through the common job coordination path. The System Actor executes jobs only through its explicitly assigned permissions.

## Consequences

The database provides cross-instance execution coordination and recovery while the application retains scheduling policy. Job execution remains subject to the central authorization model; a failed completion write recovers through lease expiry.

## References

- [job scheduling and lease coordinator](../../Core/Auditarium.Bll/Jobs/JobScheduling.cs), [runtime-state model](../../Core/Auditarium.Models/Jobs/JobRuntimeState.cs), and [job operations use cases](../../Core/Auditarium.Bll/Features/Jobs/JobOperations.cs)
- [job runtime-state integration tests](../../Tests/Auditarium.Persistence.IntegrationTests/JobRuntimeStateIntegrationTests.cs) and [job scheduling unit tests](../../Tests/Auditarium.Common.Tests/JobSchedulingTests.cs)
