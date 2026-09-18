# Auditarium

SPDX-License-Identifier: MIT

## Development

Open this repository in VS Code and select **Dev Containers: Reopen in
Container**. The canonical environment uses Docker and Docker Compose; it
provides the .NET 10 SDK and restores the solution during container creation.

Inside the container, run:

```sh
dotnet build Auditarium.sln
dotnet test Auditarium.sln
```

For local reference hosting, run `docker compose up --build api web`.
The API listens on `http://localhost:8080`, the web host on
`http://localhost:8081`. Both expose `/health/live`, `/health/ready`, and
`/metrics`; the API's initial versioned endpoint is `/api/v1/system/status`.

Docker Compose is the tested reference platform. The Compose definition uses
only ordinary Compose/OCI mechanisms and intentionally has no Podman-specific
or Docker-only runtime dependency; Podman is a design target, not a support
guarantee at this stage.
