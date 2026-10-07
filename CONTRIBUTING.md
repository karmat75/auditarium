# Contributing to Auditarium

Thank you for contributing. This document defines the shared local workflow and
the checks required before opening a pull request.

## Development environment

The maintained local workflows are documented in
[Local Development](documents/Development/LocalDevelopment.md):

- Windows + VS Code + Docker Compose + PostgreSQL
- Windows + Visual Studio + native .NET + SQL Server LocalDB
- Linux + VS Code + Docker Compose + PostgreSQL
- optional native development against an external PostgreSQL database

The Dev Container remains available as an additional VS Code environment, but
it is not required for these workflows.

For Compose, prefer the small repository helpers:

```text
Windows: .\scripts\dev.ps1 up
Linux:   sh scripts/dev.sh up
```

Use `status`, `demo`, `down`, `reset`, or `rebuild` with the same
helper. `reset` is intentionally destructive only for the Auditarium Compose
project and its development volumes.

For native debugging, VS Code provides `Auditarium Web`, `Auditarium API`,
`Auditarium Web + API`, and `Auditarium Demo Data` profiles. The DemoData
profile runs `apply --confirm` in Development and uses the Web Development
configuration.

DemoData always targets an already migrated and normally bootstrapped
Development database. It rejects unknown business data and never resets the
database itself. For Compose use the `demo` helper; for native configuration
use `demo-native`.

## Local configuration and secrets

Do not commit credentials, tokens, or machine-specific settings. The complete
Development examples, configuration precedence, supported database providers,
and copyable connection-string examples are maintained in the
[Development configuration reference](documents/Development/Configuration.md).

`appsettings.Development.json`, `appsettings.*.local.json`, `.auditarium/`,
and `.env` are ignored by Git. Keep the versioned examples safe to publish and
update the reference when configuration behavior changes.

## Required checks

Run these commands before opening a pull request:

```sh
dotnet restore Auditarium.sln --locked-mode
dotnet build Auditarium.sln --configuration Release --no-restore
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
dotnet format Auditarium.sln --verify-no-changes --no-restore
```

The CI workflow runs the same checks automatically for pull requests targeting
`main`. The full suite is not repeated solely because an already-green pull
request was merged. A manual `workflow_dispatch` run remains available when an
explicit re-check of `main` is useful. New commits on the same pull request
supersede and cancel an older in-progress CI run.

For a focused check of maintained documentation, run:

```sh
dotnet test Tests/Auditarium.Infrastructure.Security.Tests/Auditarium.Infrastructure.Security.Tests.csproj --configuration Release --filter FullyQualifiedName~DocumentationQualityTests
```

The normal solution-wide test command above runs these checks as well, so they
are enforced by CI without a separate documentation toolchain.

## Development lifecycle

Planned work starts with a GitHub Issue. Its scope and acceptance criteria
define the selected work. A GitHub Issue is the unit of planned work; the
GitHub Project is its workflow and prioritization view.

The maintainer-verified Project flow is:

```text
Backlog → Ready → In Progress → Review → Done
```

- **Backlog:** captured or planned work that is not yet selected or ready.
- **Ready:** scope is understood and approved; relevant dependencies and
  decisions are resolved sufficiently to start.
- **In Progress:** implementation is active. Contributors normally assign the
  Issue to themselves and move it here.
- **Review:** a pull request represents the implementation and is under review
  and CI; Project automation may move linked work here.
- **Done:** work has been accepted and the Issue is completed or closed,
  normally after review, successful CI, and merge.

Work on one selected Issue scope at a time. The implementation pull request
references that Issue and normally uses `Closes #<issue>` when it completes the
Issue. Tests must cover relevant changed behavior. Put documentation updates in
the same pull request when behavior, interfaces, architecture, operator
procedures, or external formats change.

Architecture documentation is maintained in `documents/Architecture/`.
Durable cross-cutting decisions normally need an ADR: application or system
boundaries, security or authorization models, persistence or deployment
strategies, and provider or extensibility models are typical examples. Ordinary
implementation details, local refactorings, individual handlers or queries,
library use without an architectural constraint, and undecided backlog or
product questions normally do not need an ADR. Follow the governance in
[documents/README.md](documents/README.md) and the conventions in
[documents/ADR/README.md](documents/ADR/README.md); ADR decision history is not
casually rewritten. Update `documents/Operations/` when an operational
procedure changes, and update the applicable versioned contract documentation
when an external contract changes.

The historical Soll-/Pflichtenheft and its Work Packages provide provenance and
traceability, not the active backlog. Completed work is accepted only when its
acceptance criteria are satisfied, relevant tests pass, required documentation
and ADRs are updated, CI is green, review is complete, and the pull request is
merged or the Issue is completed.

## Code and pull requests

- Follow the repository `.editorconfig`; do not mix unrelated formatting
  changes with functional work.
- Keep package lockfiles up to date when changing package dependencies.
- Use focused branches and pull requests. The pull request title must exactly
  match the selected GitHub Issue title. Describe the behavior change, tests,
  and any configuration or migration impact.
- `main` is pull-request-only. Do not push or commit directly to `main`.
- Required CI must be green before merging to `main`.
- Address CI failures before requesting review.
