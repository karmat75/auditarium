# Auditarium

SPDX-License-Identifier: MIT

Auditarium ist eine Open-Source-Anwendung für die strukturierte Vorbereitung,
Durchführung, Dokumentation und Auswertung von Audits gegen versionierte
Regelwerke und Dokumentkataloge.

Das Projekt wird als strukturierter Monolith entwickelt: Fachlogik,
Persistenz und technische Infrastruktur sind klar von der Weboberfläche und
der HTTP-API getrennt. Das ausführliche fachliche und technische Zielbild ist
im [Soll- und Pflichtenheft](documents/Auditarium_Soll_Pflichtenheft.md)
dokumentiert.

> **Projektstatus:** Der aktuelle Repository-Stand enthält die technische
> Grundlage und erste Host-Endpunkte. Die im Pflichtenheft beschriebene
> Fachfunktionalität wird schrittweise umgesetzt.

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
| Entwicklungsumgebung | VS Code Dev Container, Docker und Docker Compose |
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
documents/               Soll- und Pflichtenheft
```

Die Abhängigkeitsrichtung verläuft von den UI-Hosts über die BLL zu Common und
Models. Persistenz und Infrastruktur implementieren technische Details, ohne
fachliche Regeln in die UI zu verlagern.

## Schnellstart

Die kanonische Entwicklungsumgebung ist der Dev Container. Benötigt werden nur
VS Code, die Erweiterung **Dev Containers** sowie Docker und Docker Compose.

1. Repository in VS Code öffnen.
2. In der Befehlspalette **Dev Containers: Reopen in Container** auswählen.
3. Warten, bis `dotnet restore` beim Erstellen des Containers abgeschlossen ist.
4. Anschließend die Anwendung über F5 oder die Kommandozeile starten.

Der Container verwendet die in [global.json](global.json) festgelegte
.NET-SDK-Version und installiert die empfohlenen VS-Code-Erweiterungen.

### Build und Tests

Im Dev Container:

```sh
dotnet restore Auditarium.sln --locked-mode
dotnet build Auditarium.sln --configuration Release --no-restore
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
```

Die Paket-Lockdateien sind Teil des Repositories. Verwende bei Restore und CI
`--locked-mode`, damit Abhängigkeiten reproduzierbar bleiben.

## Starten und Debuggen in VS Code

Die Repository-Konfiguration bietet diese F5-Profile im Bereich **Run and
Debug**:

| Profil | Zweck | Adresse |
| --- | --- | --- |
| `Auditarium Web` | Startet und debuggt die Razor-Pages-Anwendung | <http://localhost:5000> |
| `Auditarium API` | Startet und debuggt die HTTP-API | <http://localhost:5001> |
| `Auditarium Web + API` | Startet beide Hosts gemeinsam | beide Adressen |

Das gewählte Profil baut das jeweilige Projekt einschließlich seiner
Projektverweise vor dem Debug-Start. Der Browser wird nach erfolgreichem Start
automatisch geöffnet.

## Lokaler Containerbetrieb

Für den referenzierten lokalen Containerbetrieb:

```sh
docker compose up --build api web
```

Danach sind die Hosts erreichbar unter:

| Host | Adresse |
| --- | --- |
| Web | <http://localhost:8081> |
| API | <http://localhost:8080> |
| API-Status | <http://localhost:8080/api/v1/system/status> |

Beide Hosts stellen diese Betriebsendpunkte bereit:

- `/health/live` für die Liveness-Prüfung
- `/health/ready` für die Readiness-Prüfung
- `/metrics` für Prometheus-kompatible Metriken

Docker Compose ist die getestete Referenzplattform. Die Konfiguration nutzt
portable OCI-/Compose-Mechanismen und berücksichtigt Podman als
Kompatibilitätsziel, ohne dafür derzeit eine Support-Garantie abzugeben.

## Konfiguration und Secrets

Keine Zugangsdaten, Tokens oder maschinenspezifischen Einstellungen einchecken.
Falls lokale Konfiguration benötigt wird, die versionierten Vorlagen kopieren:

```sh
cp UI/Auditarium.Api/appsettings.Development.example.json UI/Auditarium.Api/appsettings.Development.json
cp UI/Auditarium.Web/appsettings.Development.example.json UI/Auditarium.Web/appsettings.Development.json
```

`appsettings.Development.json`, `appsettings.*.local.json` und `.env` sind
absichtlich von Git ausgeschlossen. Die Beispielkonfigurationen müssen stets
ohne schützenswerte Werte bleiben.

## Qualität und Zusammenarbeit

Die CI prüft jeden Pull Request und jeden Push auf `main` mit einem
Locked-Mode-Restore, Release-Build, Tests und einer Formatprüfung. Vor einem
Pull Request kannst du dieselbe Prüfung lokal ausführen:

```sh
dotnet restore Auditarium.sln --locked-mode
dotnet build Auditarium.sln --configuration Release --no-restore
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
dotnet format Auditarium.sln --verify-no-changes --no-restore
```

Die verbindlichen Editor- und Zeilenendekonventionen stehen in
[.editorconfig](.editorconfig) und [.gitattributes](.gitattributes).
Hinweise zu Branches, Pull Requests, lokalen Secrets und dem Entwicklungsablauf
enthält [CONTRIBUTING.md](CONTRIBUTING.md).

## Lizenz und Drittanbieterhinweise

Auditarium steht unter der [MIT-Lizenz](LICENSE). Hinweise zu verwendeten
Drittanbieterkomponenten und ihren Lizenzen befinden sich in
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
