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

The pre-launch tasks build the selected project and its project references.

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

## Code and pull requests

- Follow the repository `.editorconfig`; do not mix unrelated formatting
  changes with functional work.
- Keep package lockfiles up to date when changing package dependencies.
- Use focused branches and pull requests. Describe the behavior change, tests,
  and any configuration or migration impact.
- Address CI failures before requesting review.
