# Auditarium

SPDX-License-Identifier: MIT

Auditarium ist eine Open-Source-Anwendung für die strukturierte Vorbereitung,
Durchführung, Dokumentation und Auswertung von Audits gegen versionierte
Regelwerke und Dokumentkataloge.

Das Projekt wird als strukturierter Monolith entwickelt: Fachlogik,
Persistenz und technische Infrastruktur sind klar von der Weboberfläche und
den API-Transportgrenzen getrennt. Die gepflegte aktuelle Architektur ist in der
[Architekturdokumentation](documents/Architecture/Overview.md) beschrieben.
Das [Soll- und Pflichtenheft](documents/Auditarium_Soll_Pflichtenheft.md) dient
als historische fachliche und technische Provenienz.

> **Projektstatus:** Geplante Arbeit und Änderungen werden über
> [GitHub Issues](https://github.com/karmat75/auditarium/issues) und das
> GitHub Project verfolgt.

## Technischer Überblick

| Bereich | Technologie |
| --- | --- |
| Runtime | .NET 10 / C# 14 |
| Web | ASP.NET Core Razor Pages |
| API | ASP.NET Core HTTP API unter `/api/v1` |
| Architektur | Strukturierter Monolith, CQRS mit Mediator |
| Validierung | FluentValidation |
| Persistenz | Entity Framework Core; PostgreSQL und Microsoft SQL Server als Zielprovider |
| Observability | `ILogger<T>`, OpenTelemetry, Health Checks und Prometheus-Metriken |
| Entwicklungsumgebung | VS Code, Visual Studio, Docker und Docker Compose |
| Lizenz | [MIT](LICENSE) |

## Projektstruktur

```text
Core/
  Auditarium.Bll/        Fachlogik, Use Cases und CQRS-Handler
  Auditarium.Common/     gemeinsame technische und fachliche Bausteine
  Auditarium.Models/     Domänenmodelle
Persistence/
  Auditarium.Dal/        Datenzugriff und EF-Core-Persistenz
  Auditarium.Fal/        Dateiablage-Abstraktion und -Implementierung
Infrastructure/
  Auditarium.Infrastructure.Ldap/  LDAP-Integration
UI/
  Auditarium.Web/        Razor-Pages-Webanwendung
  Auditarium.Api/        versionierte HTTP-API
Tests/                   automatisierte Tests
documents/               Architektur-, Entscheidungs-, Betriebs- und historische Dokumentation
```

Die Abhängigkeitsrichtung verläuft von den UI-Hosts über die BLL zu Common und
Models. Persistenz und Infrastruktur implementieren technische Details, ohne
fachliche Regeln in die UI zu verlagern.

## Schnellstart

Die gepflegten lokalen Entwicklungswege orientieren sich an den tatsächlich
verwendeten Umgebungen:

| Umgebung | Anwendung | Datenbank |
| --- | --- | --- |
| Windows + VS Code | Docker Compose | PostgreSQL |
| Windows + Visual Studio | native .NET | SQL Server LocalDB |
| Linux + VS Code | Docker Compose | PostgreSQL |

Die kurzen Rezepte für Start, DemoData, Stop und vollständigen Reset stehen in
[Local Development](documents/Development/LocalDevelopment.md).

Compose unter Windows:

```powershell
.\scripts\dev.ps1 up
```

Compose unter Linux:

```sh
sh scripts/dev.sh up
```

Für native Windows-Entwicklung zuerst die vollständigen
Development-Beispielkonfigurationen kopieren und anschließend
`Auditarium.sln` in Visual Studio öffnen. Die
[Konfigurationsreferenz](documents/Development/Configuration.md) enthält auch
Connection Strings für LocalDB, SQL Server sowie lokales und externes
PostgreSQL.

### Build und Tests

Unabhängig vom Entwicklungsweg:

```sh
dotnet restore Auditarium.sln --locked-mode
dotnet build Auditarium.sln --configuration Release --no-restore
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
```

Der vorhandene Dev Container bleibt als zusätzliche VS-Code-Umgebung nutzbar,
ist aber nicht Voraussetzung für die oben beschriebenen Workflows.

## Starten und Debuggen in VS Code

Die Repository-Konfiguration bietet diese F5-Profile im Bereich **Run and
Debug**:

| Profil | Zweck | Adresse |
| --- | --- | --- |
| `Auditarium Web` | Startet und debuggt die Razor-Pages-Anwendung | <http://localhost:5000> |
| `Auditarium API` | Startet und debuggt die HTTP-API | <http://localhost:5001> |
| `Auditarium Web + API` | Startet beide Hosts gemeinsam | beide Adressen |
| `Auditarium Demo Data` | Lädt die Demo-Fixture in die native Development-Datenbank | keine |

Das gewählte Profil baut das jeweilige Projekt einschließlich seiner
Projektverweise vor dem Debug-Start. Der Browser wird nach erfolgreichem Start
automatisch geöffnet.

## Lokaler Containerbetrieb

Compose verwendet den festen Development-Projektnamen `auditarium`. Die
plattformgerechten Helper kapseln nur die üblichen Compose-Befehle und bieten
`up`, `status`, `demo`, `down`, `reset` und `rebuild`.

Web und API sind nach dem Start erreichbar unter:

| Host | Adresse |
| --- | --- |
| Web | <http://localhost:8081> |
| API | <http://localhost:8080> |
| API-Status | <http://localhost:8080/api/v1/system/status> |

Das temporäre Initial-Credential einer frischen Compose-Installation steht im
API-Log:

```sh
docker compose logs auditarium
```

Unter Linux verwenden die Helper intern `scripts/compose.sh`, damit die
UID/GID-Abbildung des Workspace erhalten bleibt. Reset und Ressourcen gehören
immer nur zum Compose-Projekt `auditarium`; externe Datenbanken werden nicht
gelöscht. Details stehen in
[Local Development](documents/Development/LocalDevelopment.md).

## Konfiguration und Secrets

Keine Zugangsdaten, Tokens oder maschinenspezifischen Einstellungen einchecken.
Die vollständigen Development-Beispiele, alle relevanten Optionen,
Konfigurationsreihenfolge und Connection-String-Beispiele stehen in der
[Development-Konfigurationsreferenz](documents/Development/Configuration.md).

`appsettings.Development.json`, `appsettings.*.local.json`, `.auditarium/`
und `.env` sind absichtlich von Git ausgeschlossen. Die versionierten
Beispielkonfigurationen enthalten keine echten Secrets.

## Qualität und Zusammenarbeit

Die CI prüft Pull Requests auf `main` mit Locked-Mode-Restore, Release-Build,
Tests und einer Formatprüfung. Ein erfolgreicher Merge löst nicht noch einmal
dieselbe vollständige CI aus. Vor einem Pull Request kannst du dieselbe Prüfung
lokal ausführen:

```sh
dotnet restore Auditarium.sln --locked-mode
dotnet build Auditarium.sln --configuration Release --no-restore
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
dotnet format Auditarium.sln --verify-no-changes --no-restore
```

Damit ein Commit bereits bei einer nicht formatierten Änderung abgebrochen
wird, die versionierten Repository-Hooks einmal pro lokaler Clone aktivieren:

```sh
sh scripts/setup-git-hooks.sh
```

Der Pre-Commit-Hook führt dieselbe Formatprüfung wie die CI aus und stellt
Abhängigkeiten bei Bedarf wieder her. Er verwendet den lokal installierten
.NET SDK oder, falls dieser nicht vorhanden ist, das offizielle .NET-SDK-
Docker-Image. Git aktiviert Hooks aus Sicherheitsgründen nicht automatisch
beim Klonen; die CI bleibt der verbindliche Schutz, da Hooks mit
`git commit --no-verify` umgangen werden können.

Die verbindlichen Editor- und Zeilenendekonventionen stehen in
[.editorconfig](.editorconfig) und [.gitattributes](.gitattributes).
Hinweise zu Branches, Pull Requests, lokalen Secrets und dem Entwicklungsablauf
enthält [CONTRIBUTING.md](CONTRIBUTING.md).

## Lizenz und Drittanbieterhinweise

Auditarium steht unter der [MIT-Lizenz](LICENSE). Hinweise zu verwendeten
Drittanbieterkomponenten und ihren Lizenzen befinden sich in
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Datenbank-Bootstrap und Recovery

Die ausführbaren Betriebsverfahren für Datenbank-Initialisierung und die
Wiederherstellung des Default-Administrators stehen im
[Bootstrap- und Recovery-Runbook](documents/Operations/BootstrapAndRecovery.md).
Weitere Dokumentationsrollen und Navigation beschreibt
[documents/README.md](documents/README.md).
