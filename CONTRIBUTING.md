# Contributing to Auditarium

Thank you for contributing. This document defines the shared local workflow and
the checks required before opening a pull request.

## Development environment

Use VS Code with the repository's Dev Container:

1. Open the repository in VS Code.
2. Run **Dev Containers: Reopen in Container**.
3. Wait for the post-create restore to finish.

The Dev Container provides the .NET SDK version pinned in `global.json` and
installs the repository's recommended VS Code extensions.

## Run and debug

Use the **Run and Debug** view in VS Code:

- `Auditarium Web` starts the web application with the debugger on port 5000.
- `Auditarium API` starts the API with the debugger on port 5001.
- `Auditarium Web + API` starts both processes.
- `Auditarium Demo Data` starts the development-only console-tool boundary. It
  performs no data changes in the current work package.

The pre-launch tasks build the selected project and its project references.

The tool can also be run from the repository root, including the Development
Container:

```sh
dotnet run --project Tools/Auditarium.DemoData/Auditarium.DemoData.csproj
```

## Local configuration and secrets

Do not commit credentials, tokens, or machine-specific settings. Create local
configuration from the versioned examples when needed:

```sh
cp UI/Auditarium.Api/appsettings.Development.example.json UI/Auditarium.Api/appsettings.Development.json
cp UI/Auditarium.Web/appsettings.Development.example.json UI/Auditarium.Web/appsettings.Development.json
```

`appsettings.Development.json`, `appsettings.*.local.json`, and `.env` files
are ignored by Git. Keep the example files safe to publish and update them when
new configuration is required.

## Required checks

Run these commands before opening a pull request:

```sh
dotnet restore Auditarium.sln --locked-mode
dotnet build Auditarium.sln --configuration Release --no-restore
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
dotnet format Auditarium.sln --verify-no-changes --no-restore
```

The CI workflow runs the same checks on pull requests and on `main`.

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
- Use focused branches and pull requests. Describe the behavior change, tests,
  and any configuration or migration impact.
- Address CI failures before requesting review.
