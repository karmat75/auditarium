# SPDX-License-Identifier: MIT

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Command = "help"
)

$ErrorActionPreference = "Stop"
$repositoryDir = Split-Path -Parent $PSScriptRoot

function Invoke-Compose {
    param([string[]]$ComposeArgs)

    & docker compose @ComposeArgs
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose failed with exit code $LASTEXITCODE."
    }
}

function Invoke-NativeDemo {
    $oldEnvironment = [Environment]::GetEnvironmentVariable("DOTNET_ENVIRONMENT", "Process")
    $oldDemoData = [Environment]::GetEnvironmentVariable("AUDITARIUM__DemoData__Enabled", "Process")

    try {
        [Environment]::SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development", "Process")
        [Environment]::SetEnvironmentVariable("AUDITARIUM__DemoData__Enabled", "true", "Process")
        & dotnet run --project Tools/Auditarium.DemoData/Auditarium.DemoData.csproj -- apply --confirm
        if ($LASTEXITCODE -ne 0) {
            throw "Auditarium.DemoData failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        [Environment]::SetEnvironmentVariable("DOTNET_ENVIRONMENT", $oldEnvironment, "Process")
        [Environment]::SetEnvironmentVariable("AUDITARIUM__DemoData__Enabled", $oldDemoData, "Process")
    }
}

function Reset-LocalDb {
    if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
        throw "sqlcmd was not found. Delete database 'Auditarium' from (localdb)\MSSQLLocalDB in Visual Studio SQL Server Object Explorer, then remove .auditarium/."
    }

    Write-Host "Resetting only database 'Auditarium' in (localdb)\MSSQLLocalDB and repository-local .auditarium/ state."
    $query = "IF DB_ID(N'Auditarium') IS NOT NULL BEGIN ALTER DATABASE [Auditarium] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Auditarium]; END"
    & sqlcmd -b -S "(localdb)\MSSQLLocalDB" -Q $query
    if ($LASTEXITCODE -ne 0) {
        throw "LocalDB reset failed with exit code $LASTEXITCODE."
    }

    $localState = Join-Path $repositoryDir ".auditarium"
    if (Test-Path $localState) {
        Remove-Item -Recurse -Force $localState
    }
}

Push-Location $repositoryDir
try {
    switch ($Command.ToLowerInvariant()) {
        { $_ -in @("up", "start") } {
            Invoke-Compose -ComposeArgs @("up", "--build", "-d", "auditarium", "web")
            break
        }
        { $_ -in @("down", "stop") } {
            Invoke-Compose -ComposeArgs @("down")
            break
        }
        "status" {
            Invoke-Compose -ComposeArgs @("ps")
            & docker volume ls --filter "label=com.docker.compose.project=auditarium"
            if ($LASTEXITCODE -ne 0) {
                throw "docker volume ls failed with exit code $LASTEXITCODE."
            }
            Write-Host "Web: http://localhost:8081  API: http://localhost:8080"
            break
        }
        "reset" {
            Write-Host "Resetting only the Auditarium Compose project (database, storage and Data Protection volumes)."
            Invoke-Compose -ComposeArgs @("down", "--volumes", "--remove-orphans")
            break
        }
        "rebuild" {
            Invoke-Compose -ComposeArgs @("build", "--no-cache", "auditarium", "web")
            Invoke-Compose -ComposeArgs @("up", "-d", "auditarium", "web")
            break
        }
        "demo" {
            $running = @(Invoke-Compose -ComposeArgs @("ps", "--status", "running", "--services"))
            if ($running -notcontains "auditarium") {
                throw "Auditarium Compose is not running. Run: .\scripts\dev.ps1 up"
            }

            Invoke-Compose -ComposeArgs @(
                "run", "--rm", "--no-deps",
                "-e", "DOTNET_ENVIRONMENT=Development",
                "-e", "AUDITARIUM__DemoData__Enabled=true",
                "-e", "AUDITARIUM__Database__Provider=PostgreSQL",
                "-e", "AUDITARIUM__Database__ConnectionString=Host=postgres;Database=auditarium;Username=auditarium;Password=auditarium-development-only",
                "workspace",
                "dotnet", "run",
                "--project", "Tools/Auditarium.DemoData/Auditarium.DemoData.csproj",
                "--", "apply", "--confirm"
            )
            break
        }
        "demo-native" {
            Invoke-NativeDemo
            break
        }
        "reset-localdb" {
            Reset-LocalDb
            break
        }
        { $_ -in @("help", "-h", "--help") } {
            @"
Auditarium development helper

Compose commands:
  .\scripts\dev.ps1 up        Start/build API + Web + PostgreSQL
  .\scripts\dev.ps1 status    Show Auditarium containers and volumes
  .\scripts\dev.ps1 demo      Apply DemoData to the Compose PostgreSQL database
  .\scripts\dev.ps1 down      Stop the Compose project
  .\scripts\dev.ps1 reset     Remove only Auditarium Compose data/keys/storage
  .\scripts\dev.ps1 rebuild   Rebuild API + Web without image cache and start

Native Windows commands:
  .\scripts\dev.ps1 demo-native
      Apply DemoData to the database configured in Web appsettings.Development.json
      or matching environment variables.

  .\scripts\dev.ps1 reset-localdb
      Drop only database 'Auditarium' from (localdb)\MSSQLLocalDB and remove
      repository-local .auditarium/ state. Requires sqlcmd.

External databases are never deleted by this helper.
"@ | Write-Host
            break
        }
        default {
            throw "Unknown command '$Command'. Run .\scripts\dev.ps1 help."
        }
    }
}
finally {
    Pop-Location
}
