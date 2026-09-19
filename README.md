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

Für den `workspace`-Entwicklungscontainer auf einem Linux-Host steht ein
Compose-Wrapper bereit:

```sh
sh scripts/compose.sh up --build workspace
```

Der Wrapper erstellt oder ergänzt automatisch die nicht versionierte Datei
`.env` um die UID und GID des jeweiligen Host-Benutzers, lädt die Linux-spezifische
Compose-Ergänzung und führt dann den übergebenen `docker compose`-Befehl aus.
Dadurch gehören Dateien, die der Container im
eingebundenen Repository erzeugt, dem richtigen Entwickler – unabhängig von
dessen Benutzername oder UID. Die Datei darf nicht ins Repository committed
werden.

Unter macOS und Windows genügt der normale `docker compose`-Befehl. Docker
Desktop virtualisiert dort die eingebundenen Host-Dateisysteme; eine Linux-
UID/GID-Abbildung ist weder sinnvoll noch notwendig. Wer unter Windows in WSL
entwickelt, verwendet den Linux-Wrapper und legt das Repository im
WSL-Dateisystem ab.

Für den referenzierten lokalen Containerbetrieb:

```sh
docker compose up --build auditarium web
```

Danach sind die Hosts erreichbar unter:

| Host | Adresse |
| --- | --- |
| Web | <http://localhost:8081> |
| API | <http://localhost:8080> |
| API-Status | <http://localhost:8080/api/v1/system/status> |

Das bei einer frischen Installation erzeugte temporäre Initial-Credential ist ausschließlich im API-Container-Log sichtbar:

```sh
docker compose logs auditarium
```

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

## Datenbank-Bootstrap und Recovery

Der Datenbankprovider wird ausschließlich extern konfiguriert. Erlaubte Werte
sind `PostgreSQL` und `SqlServer`:

```text
AUDITARIUM__Database__Provider=PostgreSQL
AUDITARIUM__Database__ConnectionString=...
AUDITARIUM__Database__BootstrapTimeoutSeconds=180
AUDITARIUM__DataProtection__KeyRingPath=/persisted/auditarium-keys
AUDITARIUM__DataProtection__ApplicationName=Auditarium
```

Der Keyring muss persistent sein und bei mehreren Instanzen gemeinsam erreichbar
bleiben. Niemals den Keyring in `application_settings` oder zusammen mit seinem
Schutz-Secret speichern.

Jeder Start wendet zunächst die Migrationen an und erwirbt anschließend einen
installationsweiten Datenbank-Lock für Bootstrap und Reconcile. Weitere
Instanzen warten höchstens bis `BootstrapTimeoutSeconds`; bei Timeout, Fehler
oder Prozessabbruch wird kein Normalbetrieb freigegeben. PostgreSQL verwendet
einen sessiongebundenen Advisory Lock, SQL Server `sp_getapplock`; das Schließen
der Datenbankverbindung gibt beide Sperren zuverlässig frei.

### Default-Administrator wiederherstellen

1. Alle Auditarium-Instanzen anhalten.
2. Für genau eine Instanz `AUDITARIUM__Recovery__Enabled=true` und
   `AUDITARIUM__Recovery__DefaultAdminPassword=<temporäres Passwort>` setzen.
3. Ausschließlich diese Instanz starten. Sie stellt nur `/recovery` sowie die
   technischen Health-Endpunkte bereit; UI, API und Jobs bleiben gesperrt.
4. Die Recovery-Instanz beenden, beide Recovery-Variablen entfernen und erst
   dann die gewünschte Anzahl normaler Instanzen starten.

Das Recovery-Passwort ist ein temporäres LOCAL-Credential. Beim nächsten
normalen Login muss es geändert werden. Es darf nicht in Dateien, Datenbank,
Logs oder Telemetrie abgelegt werden.
