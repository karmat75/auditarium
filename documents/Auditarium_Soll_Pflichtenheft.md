# Auditarium – Soll- und Pflichtenheft

**Version:** 0.113
**Stand:** 20.09.2026
**Status:** Konsolidierter Sollstand / Implementierungsleitfaden  
**Produkt:** Auditarium  
**Sub-Titel:** *Structured audits. Traceable results.*

Dieses Dokument beschreibt den aktuell abgestimmten **Sollzustand** von Auditarium. Es ist zugleich Pflichtenheft, Architekturleitfaden und strukturierter Bauplan für die Implementierung.

Der Hauptteil enthält ausschließlich den aktuell gewünschten Zielzustand. Historische Entscheidungen und frühere Entwicklungsstände stehen gesammelt im **Anhang A – Änderungshistorie**.

## 0.1 Zweck und Verwendung

Das Dokument soll so gelesen und umgesetzt werden, dass eine neue Implementierung von oben nach unten aufgebaut werden kann:

```text
technische Plattform und Solution
→ Persistenz und Startup
→ Konfiguration und Security
→ Querschnittsfunktionen
→ fachliche Produktgrundlagen
→ fachliches Datenmodell
→ Use Cases und Workflows
→ Web/API/Jobs
→ Tests und Abnahme
```

Für die Umsetzung gilt:

- Anforderungen im Hauptteil bilden den aktuellen Sollzustand.
- Offene Punkte werden nicht stillschweigend durch Implementierungsannahmen ersetzt.
- Wo ein technisches Detail ausdrücklich als „implementierungsnah“ oder „konzeptionell“ markiert ist, darf die konkrete Form angepasst werden, solange die beschriebene Semantik erhalten bleibt.
- Datenmodell, BLL, UI und API müssen dieselben fachlichen Regeln verwenden; alternative fachliche Wahrheiten in einzelnen Schichten sind nicht zulässig.
- Provider-, UI- oder Integrationsdetails dürfen den fachlichen Kern nicht unnötig verengen.

## 0.2 Verbindlichkeit von Formulierungen

Dieses Dokument verwendet bewusst überwiegend direkte Sollformulierungen. Für die Interpretation gelten sinngemäß:

```text
muss / ist / wird
→ verbindlicher Zielzustand

soll
→ erwarteter Zielzustand; Abweichung nur mit begründetem Architekturentscheid

kann / später / bei Bedarf
→ vorgesehene Erweiterungsmöglichkeit, nicht zwingend Bestandteil der ersten Implementierung
```

## 0.3 Sprach- und Naming-Konvention

Für technische Bezeichner gilt grundsätzlich Englisch:

```text
Datenbanktabellen und -spalten
C#-Typen und Member
API-Ressourcen und technische Keys
Permissions
Error Codes
Konfigurationskeys
```

Benutzerseitige Texte der ersten Version sind grundsätzlich Deutsch. Das technische Modell darf eine spätere Lokalisierung nicht verhindern.

## 0.4 Systemkurzprofil

Auditarium ist eine generische Open-Source-Anwendung zur strukturierten Vorbereitung, Durchführung, Dokumentation und Auswertung von Audits gegen versionierte Regelwerke bzw. Dokumentkataloge.

Der fachliche Kern lässt sich kurz zusammenfassen:

```text
Dokument / Regelwerk
→ versionierter Katalog aus Dokumentelementen und Fragen
→ Audit Unit als konkretes Prüfobjekt
→ materialisiertes Audit mit historischen Snapshots
→ nachvollziehbare Antworten und Elementergebnisse
→ Auswertung / Export / API
```

Wesentliche Leitplanken:

- Auditarium ist kein CMDB-, GRC- oder Maßnahmenmanagement-System.
- Auditarium erzeugt keinen verpflichtenden Gesamt-Compliance-Score.
- Kataloge können vollständig manuell gepflegt oder über ein versioniertes externes Importformat vorbefüllt werden.
- Auditarium selbst enthält keine KI-/LLM-Laufzeit.
- Relevante Auditdaten bleiben über UI, Export und API zugänglich.
- Die ausführlichen fachlichen Regeln beginnen in Kapitel 8; die Kapitel 1 bis 7 stellen zunächst die technische Plattform und die Querschnittsfunktionen bereit.

## 0.5 Empfohlene Implementierungsphasen

Die Kapitel sind bereits in einer sinnvollen Bau-Reihenfolge angeordnet. Als grobe Phasen gelten:

| Phase | Schwerpunkt | Ergebnis |
|---|---|---|
| 1 | Dev-Umgebung, Plattform, Solution, Persistenz | reproduzierbarer VS-Code-Dev-Container und lauffähiges Grundgerüst mit DB, Migrationen und Bootstrap |
| 2 | Konfiguration, Security, Querschnitt | sicherer Application-Kern mit RBAC, Fehlern, Concurrency und Observability |
| 3 | Dokument-/Katalogmodell | pflegbare Regelwerke und versionierte Kataloge |
| 4 | Auditmodell | erzeugbare, bearbeitbare und finalisierbare Audits |
| 5 | Import, Auswertung und Datenzugriff | externe Importstrecke, Export und Reports-Grundlage |
| 6 | Web und API | nutzbare Razor-Pages-Oberfläche und versionierte HTTP-API |
| 7 | Jobs und Betriebsfunktionen | Maintenance, Retention, Mehrinstanz-Koordination |
| 8 | Tests und Abnahme | reproduzierbar geprüfter Stand auf PostgreSQL und SQL Server |

## 0.6 Inhaltsübersicht

```text
1  Technische Grundlagen und Solution-Architektur
2  Persistenz, Datenbanken und Concurrency
3  Konfiguration, Secrets, Startup und Mehrinstanzbetrieb
4  Identity, Authentication und Authorization
5  Ergebnis-, Fehler- und Exception-Modell
6  Observability, Health und technische Diagnose
7  System-Audit-Log, Soft Delete und Retention
8  Fachliche Produktgrundlagen und Leitprinzipien
9  Fachmodell: Dokumente, Kataloge und Fragen
10 Dateiablage und File Abstraction Layer
11 Strukturierter Katalogimport
12 Fachmodell: Audit Units
13 Fachmodell: Audits, Materialisierung und Antworten
14 Integritäts-, Constraint- und Indexmatrix
15 Auswertung, Zeitleiste, Export und Datenzugriff
16 Web-Oberfläche mit Razor Pages
17 HTTP-API
18 Background Jobs und Maintenance
19 Tests, Qualität und Abnahmekriterien
20 Empfohlene Implementierungsreihenfolge
21 Noch offene Themen
A  Änderungshistorie
```

## 0.7 Arbeitsregeln für Codex und Implementierung

Für die Nutzung dieses Dokuments als Implementierungsanweisung gelten zusätzlich folgende Regeln:

1. **Der Hauptteil ist normativ.** Die Änderungshistorie im Anhang erklärt die Entwicklung, ist aber keine zweite Quelle des aktuellen Sollzustands.
2. **Kapitel 21 ist bewusst offen.** Daraus dürfen keine weitreichenden Produktentscheidungen stillschweigend erfunden werden.
3. **Explizite Regeln schlagen Beispiele.** Code-, JSON- und Ablaufbeispiele erläutern die Semantik; verbindlich sind die beschriebenen Invarianten, Zustände und Verantwortlichkeiten.
4. **Widersprüche werden nicht geraten.** Falls zwei Anforderungen trotz dieser Konsolidierung widersprüchlich erscheinen, wird der Konflikt sichtbar gemacht und vor einer irreversiblen Implementierungsentscheidung geklärt.
5. **Schichtgrenzen gelten auch für Abkürzungen.** Kein UI-, API-, Job- oder Importpfad darf Security, BLL-Regeln, Concurrency oder Persistenzgrenzen umgehen, nur weil eine direktere Implementierung technisch möglich wäre.
6. **Änderungen am Datenmodell ziehen die zugehörigen Artefakte nach sich.** Dazu gehören Migrationen, Bootstrap/Reconcile, Provider-Tests und betroffene API-/Web-Verträge.
7. **Work Packages sind die bevorzugte Bau-Reihenfolge.** Kapitel 20 beschreibt eine sinnvolle Abfolge; einzelne Pakete dürfen nur vorgezogen werden, wenn ihre Abhängigkeiten bereits erfüllt sind.
8. **Definition of Done umfasst Tests.** Ein Use Case gilt nicht allein deshalb als fertig, weil der Happy Path manuell funktioniert. Relevante Fehler-, Permission-, State- und Concurrency-Pfade gehören zur Implementierung.

Diese Regeln sollen verhindern, dass aus einem ausführlichen Pflichtenheft durch lokale Implementierungsentscheidungen erneut mehrere konkurrierende Wahrheiten entstehen.

---

# 1. Technische Grundlagen und Solution-Architektur

## Technischer Steckbrief

| Bereich | Festlegung |
|---|---|
| Architektur | strukturierter Monolith |
| Runtime | .NET 10 LTS / C# 14 |
| Entwicklungsumgebung | VS Code Dev Container auf Docker + Docker Compose als Referenzplattform |
| Container-Portabilität | bewusst Podman-kompatibel ausgelegt; zunächst keine Support-Garantie |
| Lizenz | MIT License |
| Web | ASP.NET Core Razor Pages, Request/Response |
| API | ASP.NET Core HTTP API, `/api/v1`, OpenAPI |
| CQRS | `martinothamar/Mediator` |
| Validation | FluentValidation |
| ORM | Entity Framework Core |
| Datenbanken | PostgreSQL und Microsoft SQL Server |
| Dateiablage | FAL mit Filesystem/Share als erster Implementierung |
| Authentifizierung | LOCAL, LDAP und API; OIDC/SAML später möglich |
| Autorisierung | permission-basiertes additives RBAC |
| Jobs | ASP.NET Core `BackgroundService` + Cronos |
| Observability | `ILogger<T>`, `Meter`, `ActivitySource`, OpenTelemetry, OTLP |
| DB-Secrets | `ISecretProtector` auf ASP.NET Core Data Protection |
| Mehrinstanz | gemeinsame DB, gemeinsamer StorageRoot, gemeinsamer Data-Protection-Keyring |

Dieser Steckbrief ist eine Zusammenfassung. Die nachfolgenden Abschnitte definieren die verbindliche Semantik.

## Architekturgrundsatz

Auditarium wird als **strukturierter Monolith** umgesetzt.

Ziel ist eine klar gegliederte, erweiterbare und wartbare Anwendung mit eindeutigen Projekt- und Schichtgrenzen, ohne die zusätzliche betriebliche und technische Komplexität einer Microservice-Architektur.

Die Architektur soll insbesondere:

- klare Abhängigkeitsrichtungen erzwingen,
- fachliche Logik von UI und technischer Infrastruktur trennen,
- CQRS und featureorientierte Entwicklung unterstützen,
- Entity Framework Core ohne unnötige zusätzliche Persistenzabstraktionen nutzen,
- gute Testbarkeit sicherstellen,
- spätere funktionale Erweiterungen ermöglichen, ohne früh technische Schulden zu erzeugen.

## Plattform und Sprache

Auditarium wird in **C# auf .NET** umgesetzt.

Für neue Hauptversionen gilt:

```text
aktuelle stabile Major-Version
oder
letzte verfügbare LTS-Version
```

Für den initialen Implementierungsstand ist vorgesehen:

```text
.NET 10 LTS
C# 14
```

Ein Versionswechsel erfolgt bewusst und kontrolliert; Preview-/RC-Versionen sind keine reguläre Produktionsbasis.

## Entwicklungsumgebung und Container-Referenzplattform

Die gemeinsame Entwicklungsumgebung wird als **VS Code Dev Container** bereitgestellt.

Für die initiale Entwicklung und die ersten Einsatzumgebungen gilt verbindlich:

```text
Referenzplattform
→ Docker
→ Docker Compose
→ VS Code Dev Containers
```

Das Repository stellt dafür eine gemeinsame Dev-Container-Definition bereit, mindestens auf Basis von:

```text
.devcontainer/devcontainer.json
Dockerfile
compose.yaml
```

Die konkrete Dateiaufteilung darf implementierungsnah angepasst werden, solange es weiterhin genau eine kanonische Entwicklungsumgebung gibt.

### Docker als Referenz, nicht als Anwendungsabhängigkeit

Docker und Docker Compose sind die **getestete und dokumentierte Referenzplattform** für Entwicklung, lokale Integrationstests und den initialen Containerbetrieb.

Auditarium selbst darf jedoch keine fachliche oder technische Kernabhängigkeit von Docker-spezifischen Laufzeitfunktionen aufbauen.

Der gemeinsame Deployment-Vertrag ist das erzeugte Auditarium-Container-Image mit seinen dokumentierten:

```text
Ports
Environment-/Config-Werten
DB-Verbindungen
StorageRoot
Data-Protection-Keyring
Health-Endpunkten
Entrypoints
```

Die äußere Container-Runtime bzw. Orchestrierung bleibt davon getrennt.

### Podman als Portabilitätsziel

Container- und Compose-Definitionen werden bewusst so geschrieben, dass sie nach Möglichkeit auch mit Podman und einer Compose-kompatiblen Umgebung funktionieren.

Dafür gilt:

- portable OCI-/Container-Mechanismen werden Docker-spezifischen Sonderfunktionen vorgezogen,
- `compose.yaml` bleibt im gemeinsamen, üblichen Compose-Funktionsumfang,
- es werden keine parallelen Docker-/Podman-Konfigurationen gepflegt, solange dafür kein konkreter technischer Bedarf besteht,
- absichtliche Podman-Inkompatibilitäten werden vermieden.

Für den initialen Projektstand gilt Podman jedoch ausdrücklich nur als **Kompatibilitätsziel**, nicht als getestete oder zugesicherte Support-Plattform.

Eine offizielle Podman-Supportaussage setzt voraus, dass Build, Dev-Container und relevante Compose-Szenarien tatsächlich mit Podman automatisiert oder regelmäßig getestet werden.

### Kubernetes und weitere Orchestratoren

Kubernetes ist **kein Ziel der initialen Entwicklungs- oder Deployment-Stufe**.

Es werden daher zunächst keine parallelen Kubernetes-Manifeste, Helm-Charts oder Kubernetes-spezifischen Entwicklungsumgebungen gepflegt.

Die Container- und Anwendungsschnittstellen sollen eine spätere Nutzung auf anderen OCI-/Orchestrierungsplattformen nicht unnötig verhindern. Eine solche Plattform wird aber erst dann offiziell unterstützt, wenn ein konkreter Einsatzbedarf besteht und die dafür notwendigen Deployment-Artefakte sowie Tests tatsächlich gepflegt werden.

Grundsatz:

> Docker + Docker Compose ist die Referenzplattform. Podman wird bei Designentscheidungen mitgedacht, aber zunächst nicht als offiziell getestete Zielplattform behauptet.

## Lizenzierung und Open-Source-Artefakte

Auditarium wird unter der **MIT License** veröffentlicht.

Die Lizenzentscheidung ist Bestandteil der Produkt- und Distributionsanforderungen.

Grundsatz:

> Auditarium soll privat, wissenschaftlich und kommerziell genutzt, verändert, weitergegeben und in andere Lösungen integriert werden können, ohne Copyleft-Pflichten für abgeleitete Werke zu erzwingen.

Die Distribution muss mindestens folgende Artefakte enthalten:

```text
LICENSE
THIRD-PARTY-NOTICES.md
```

Zusätzlich wird die Projektlizenz in den dafür geeigneten Projektmetadaten und Quelltextartefakten über den SPDX-Identifier:

```text
SPDX-License-Identifier: MIT
```

ausgewiesen, soweit dies für den jeweiligen Dateityp sinnvoll und üblich ist.

### `LICENSE`

`LICENSE` enthält den vollständigen Text der MIT License.

Als Copyright-Hinweis ist für den Projektstand vorgesehen:

```text
Copyright (c) 2026 Auditarium contributors
```

Die Lizenzdatei ist Bestandteil jeder Quell- und Binärdistribution, soweit die jeweilige Distributionsform dies erfordert.

### `THIRD-PARTY-NOTICES.md`

`THIRD-PARTY-NOTICES.md` dokumentiert relevante Drittanbieterkomponenten sowie Lizenz-/Copyright-Hinweise, die bei einer Distribution mitgeführt werden müssen.

Die Datei ist kein Ersatz für ein automatisiertes Dependency- und License-Scanning, sondern die für Nutzer sichtbare konsolidierte Übersicht der mitzuliefernden Hinweise.

### Abhängigkeitsregel

Neue Laufzeit-, Build- oder Tooling-Abhängigkeiten werden vor Aufnahme auf Lizenzverträglichkeit geprüft.

Dabei gilt:

> Eine neue Abhängigkeit darf das beabsichtigte MIT-Distributionsmodell von Auditarium nicht unbeabsichtigt einschränken oder zusätzliche Copyleft-/Distributionspflichten für den Auditarium-Kern erzwingen.

Lizenzen von Drittkomponenten werden nicht allein anhand ihrer Bezeichnung bewertet; maßgeblich sind die tatsächlichen Lizenzbedingungen und die konkrete Nutzungs-/Distributionsform.

Abhängigkeiten mit unklarer oder nicht kompatibler Lizenzlage werden nicht aufgenommen.

### Kein Lizenzwechsel durch Abhängigkeiten

Die Lizenz einer verwendeten Bibliothek ändert nicht die Projektlizenz von Auditarium.

Falls eine Abhängigkeit zusätzliche Hinweise oder Bedingungen für die Weitergabe verlangt, werden diese über die dafür vorgesehenen Drittanbieterhinweise erfüllt, sofern dies mit der MIT-lizenzierten Distribution vereinbar ist.

## Solution-Struktur

Die Solution wird grundsätzlich wie folgt gegliedert:

```text
Auditarium.sln
│
├── UI
│   ├── Auditarium.Api
│   └── Auditarium.Web
│
├── Core
│   ├── Auditarium.Bll
│   ├── Auditarium.Common
│   └── Auditarium.Models
│
├── Persistence
│   ├── Auditarium.Dal
│   └── Auditarium.Fal
│
├── Infrastructure
│   ├── Auditarium.Infrastructure.Ldap
│   └── weitere externe Integrationen bei Bedarf
│
└── Tests
    ├── Auditarium.Bll.Tests
    ├── Auditarium.Dal.Tests
    ├── Auditarium.Fal.Tests
    └── weitere Testprojekte nach Bedarf
```

Die Solution-Ordner dienen der strukturellen Gliederung; Projektabhängigkeiten ergeben sich aus den folgenden Regeln und nicht aus der Ordnerstruktur selbst.

## Abhängigkeitsrichtung

Für fachliche Operationen gilt:

```text
UI
→ BLL
```

Die UI darf nicht direkt auf DAL, FAL oder externe Infrastructure-Projekte zugreifen.

Die BLL definiert die Fähigkeiten, die sie von äußeren Schichten benötigt.

Beispiel:

```text
Auditarium.Bll
├── IAuditariumDbContext
├── IFileStorage
└── IUserDirectory

Auditarium.Dal
└── AuditariumDbContext : IAuditariumDbContext

Auditarium.Fal
└── FileStorage : IFileStorage

Auditarium.Infrastructure.Ldap
└── LdapUserDirectory : IUserDirectory
```

Daraus folgt technisch:

```text
DAL → BLL
FAL → BLL
Infrastructure.* → BLL
```

weil die äußeren Projekte die in der BLL definierten Interfaces implementieren.

Grundsatz:

> Ein Interface wird dort definiert, wo die Fähigkeit benötigt wird – nicht dort, wo sie implementiert wird.

## Verantwortlichkeiten der Projekte

### `Auditarium.Bll`

Die BLL enthält:

- fachliche Use Cases,
- CQRS Commands und Queries,
- Handler,
- fachliche Validatoren,
- ViewModels,
- für die BLL erforderliche Architekturabstraktionen.

Für fachliche Operationen ist die BLL die einzige Schnittstelle der UI zum Anwendungskern.

### `Auditarium.Models`

`Auditarium.Models` enthält:

- persistierbare Entity-Klassen,
- fachnahe Enums,
- ausschließlich modellnahe Strukturen ohne UI- oder Provider-spezifische Abhängigkeiten.

EF-Core-spezifische Entity-Konfigurationen gehören nicht in `Models`.

### `Auditarium.Common`

`Auditarium.Common` enthält ausschließlich fachlich neutrale und tatsächlich schichtübergreifend benötigte technische Bausteine.

Beispiele:

```text
IClock / SystemClock
FileItem
gemeinsame technische Result-/Fehlertypen
Pagination-Hilfstypen
technische Konstanten
```

`Common` darf nicht als allgemeiner Ablageort für Klassen verwendet werden, die lediglich keinen offensichtlich besseren Platz besitzen.

Für Zeitabhängigkeiten wird eine abstrahierte Uhr verwendet, beispielsweise:

```csharp
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

Produktiv kann eine `SystemClock`, in Tests eine kontrollierbare Test-/Fake-Implementierung verwendet werden.

Zeitwerte werden bevorzugt als `DateTimeOffset` behandelt; fachlich relevante Persistenzzeiten werden intern in UTC geführt.

### `Auditarium.Dal`

Das DAL enthält:

- `AuditariumDbContext`,
- Entity Framework Core,
- Entity-Konfigurationen,
- Provider-Konfiguration für PostgreSQL und Microsoft SQL Server,
- Migrationen,
- persistenznahe technische Implementierungen.

### `Auditarium.Fal`

Das FAL kapselt Dateioperationen, insbesondere:

- Speichern,
- Lesen,
- Upload,
- Download,
- Erzeugen und Auflösen interner Speicherpfade.

Dateibezogene, schichtübergreifend benötigte Metadaten können über gemeinsame Typen wie `FileItem` abgebildet werden.

### `Infrastructure`

Externe Systeme erhalten eigene Infrastructure-Projekte.

Beispiele:

```text
Auditarium.Infrastructure.Ldap
Auditarium.Infrastructure.<ExternalService>
```

Diese Projekte implementieren die von der BLL definierten Ports für externe Dienste.

## CQRS und Feature-Struktur

Auditarium verwendet CQRS in einer feature-/use-case-orientierten Vertical-Slice-Struktur.

Grundstruktur:

```text
Auditarium.Bll
│
├── Abstractions
│   ├── Persistence
│   │   └── IAuditariumDbContext.cs
│   ├── Files
│   │   └── IFileStorage.cs
│   └── Identity
│       └── IUserDirectory.cs
│
└── Features
    ├── Documents
    │   ├── Create
    │   │   ├── CreateDocumentCommand.cs
    │   │   ├── CreateDocumentHandler.cs
    │   │   └── CreateDocumentValidator.cs
    │   │
    │   ├── Get
    │   │   ├── GetDocumentQuery.cs
    │   │   ├── GetDocumentHandler.cs
    │   │   └── ViewModels
    │   │       └── DocumentDetailsViewModel.cs
    │   │
    │   ├── List
    │   │   ├── ListDocumentsQuery.cs
    │   │   ├── ListDocumentsHandler.cs
    │   │   └── ViewModels
    │   │       └── DocumentListItemViewModel.cs
    │   │
    │   └── ChangeUsageState
    │       ├── ChangeDocumentUsageStateCommand.cs
    │       ├── ChangeDocumentUsageStateHandler.cs
    │       └── ChangeDocumentUsageStateValidator.cs
    │
    ├── Catalogs
    ├── AuditUnits
    ├── Audits
    └── Users
```

Grundsatz:

> Alles, was zu einem Use Case gehört, soll möglichst nah beieinander liegen.

Horizontale Ordnerstrukturen wie

```text
Commands/
Queries/
Contracts/
Implementations/
```

über mehrere Use Cases hinweg werden vermieden.

## Commands und Queries

### Queries

Queries:

- verändern keinen fachlichen Zustand,
- verwenden bevorzugt `AsNoTracking()`,
- projizieren möglichst direkt in das benötigte ViewModel,
- laden keine vollständigen Entities, wenn nur eine Projektion benötigt wird,
- rufen kein `SaveChangesAsync()` auf.

Grundmuster:

```text
Query
→ Handler
→ IAuditariumDbContext
→ LINQ / EF Core
→ ViewModel
```

### Commands

Commands:

- verändern fachlichen Zustand,
- laden die dafür benötigten Entities,
- führen fachliche Validierungen und Zustandsänderungen aus,
- persistieren über `SaveChangesAsync()`.

Ein Command muss kein umfangreiches ViewModel zurückgeben.

Bei Bedarf genügt ein kleines Resultat, beispielsweise:

```text
CreateDocumentResult
→ DocumentId
```

## ViewModels

ViewModels gehören zur BLL und werden nicht aus den Entity-Klassen der Persistence-Schicht abgeleitet oder direkt durch diese ersetzt.

Die UI erhält fachliche Daten über BLL-ViewModels.

Regel:

```text
nur in einem Use Case verwendet
→ <UseCase>/ViewModels

von mehreren Use Cases desselben Features verwendet
→ <Feature>/ViewModels

featureübergreifend benötigt
→ erst dann gezielt abstrahieren
```

ViewModels werden nach ihrem dargestellten Inhalt benannt, beispielsweise:

```text
DocumentDetailsViewModel
DocumentListItemViewModel
AuditDetailsViewModel
```

Dateinamen wie

```text
GetDocumentHandler.ViewModel.cs
```

werden vermieden, weil das Ergebnis nicht unnötig an die konkrete Handler-Implementierung gekoppelt werden soll.

## Interfaces und Handler

Interfaces werden an echten Architekturgrenzen verwendet.

Beispiele:

```text
IAuditariumDbContext
IFileStorage
IUserDirectory
IClock
```

Handler erhalten nicht automatisch eigene Interfaces.

Nicht vorgesehen ist beispielsweise:

```text
ICreateDocumentHandler
CreateDocumentHandler
```

wenn das Interface lediglich exakt die Implementierung widerspiegelt.

Ein zusätzliches Interface wird nur eingeführt, wenn daraus ein konkreter architektonischer oder testtechnischer Nutzen entsteht.

## Dependency Injection

Auditarium verwendet Dependency Injection konsequent für:

- BLL-Abstraktionen,
- DbContext,
- Dateizugriff,
- externe Dienste,
- Clock-/Zeitabstraktionen,
- CQRS-Handler,
- Validatoren,
- weitere technische Services.

Die konkreten Implementierungen werden im jeweiligen Composition Root der UI-Projekte registriert.

UI-Projekte dürfen dabei nur die für den Start erforderlichen äußeren Projekte zur DI-Konfiguration referenzieren; fachliche Nutzung erfolgt weiterhin ausschließlich über die BLL.

## CQRS-Dispatcher

Auditarium verwendet für das Dispatching von Commands und Queries den source-generator-basierten Mediator von `martinothamar/Mediator`.

Grundmuster:

```text
UI / API
→ Mediator
→ Pipeline Behaviors
→ Handler
→ Response
```

Die BLL kennt die Mediator-Abstraktionen direkt.

Es wird keine zusätzliche Auditarium-eigene Dispatcher-Abstraktion wie

```text
IAuditariumMediator
ICommandDispatcher
IQueryDispatcher
```

eingeführt, solange dafür kein konkreter technischer Bedarf besteht.

Commands und Queries bleiben einfache BLL-Typen und werden durch ihre jeweiligen Handler verarbeitet.

## Pipeline Behaviors

Pipeline Behaviors kapseln wiederkehrende Querschnittsfunktionen, die unabhängig vom konkreten fachlichen Use Case ausgeführt werden.

Für den ersten Implementierungsstand ist folgende schlanke Pipeline vorgesehen:

```text
Mediator.Send(...)
      ↓
ObservabilityBehavior
      ↓
AuthorizationBehavior
      ↓
ValidationBehavior
      ↓
Handler
      ↓
Response
```

### `ObservabilityBehavior`

Das technische Logging kann insbesondere erfassen:

- Request-Typ,
- Start und Ende,
- Laufzeit,
- technische Fehler und Exceptions.

Dieses Logging ist ausdrücklich vom fachlichen `system_audit_log` getrennt.

Änderungsprotokolle im `system_audit_log` werden weiterhin auf Persistenzebene aus den tatsächlichen Entity-Änderungen über Entity Framework / `SaveChanges` erzeugt. Ereignisse ohne zugehörige Datenänderung werden ausdrücklich über den Ereignisprotokollierungsweg gemäß Kapitel 7 erfasst.

### `AuthorizationBehavior`

Der Authorization-Behavior prüft allgemeine, deklarative Berechtigungsanforderungen eines Requests, bevor der Handler ausgeführt wird.

Die verbindliche Rollen-/Permission-Modellierung ist im Kapitel zu Identity, Authentication und Authorization definiert.

Fachliche Regeln, die den Zustand konkreter Domänenobjekte betreffen, gehören nicht in einen generischen Authorization-Behavior.

### `ValidationBehavior`

Der Validation-Behavior führt die für einen Request registrierten FluentValidation-Validatoren aus.

Bei ungültigem Request wird der Handler nicht ausgeführt.

### Kein generischer `TransactionBehavior`

Ein generischer Transaction-Behavior ist für die erste Version nicht vorgesehen.

Ein einzelnes `SaveChangesAsync()` wird durch Entity Framework ohnehin transaktional ausgeführt.

Explizite Transaktionen werden für konkrete Use Cases eingesetzt, die mehrere persistente Schritte atomar zusammenfassen müssen. Zusätzlich darf der zentrale auditierte Speichervorgang im DAL eine explizite Transaktion verwenden, wenn Fachänderung und zugehörige Audit-Log-Einträge mehrere Speicherschritte benötigen.

Diese technische Koordination bleibt in der Persistenzschicht gekapselt und begründet keinen generischen `TransactionBehavior`.

## FluentValidation

Auditarium verwendet FluentValidation für die inhaltliche und strukturelle Validierung von Commands und Queries.

FluentValidation übernimmt beispielsweise:

```text
Pflichtfeld vorhanden?
Text nicht leer?
Maximale Länge eingehalten?
Wertebereich korrekt?
Requestinterne Abhängigkeit erfüllt?
```

Beispiel:

```text
usage_state = DEPRECATED
→ usage_state_reason muss vorhanden sein
```

Diese Regel kann vollständig aus dem Request selbst bewertet werden und gehört daher in den Validator.

### Grenze zur Businesslogik

Validatoren prüfen ausschließlich den Request selbst.

Sie greifen **nicht** auf folgende Abstraktionen zu:

```text
IAuditariumDbContext
IFileStorage
IUserDirectory
```

und führen keine fachlichen Datenbankabfragen aus.

Insbesondere gehören Regeln wie diese nicht in FluentValidation:

```text
Existiert die referenzierte Katalogversion?
Ist die Katalogversion READY?
Gehört sie zu einem nutzbaren Dokument?
Darf ein Audit aus seinem aktuellen Zustand in den Zielzustand wechseln?
Gibt es Fragen ohne Scope?
```

Solche Regeln benötigen Kenntnis des aktuellen Systemzustands und werden deshalb in Handlern bzw. fachlichen BLL-Komponenten validiert.

Grundsatz:

> Validatoren prüfen den Request. Businesslogik prüft die Welt, in der dieser Request ausgeführt werden soll.

## Validierungsfehler

Validierungsfehler verwenden stabile technische Fehlercodes.

Beispiel:

```text
DOCUMENT.TITLE.REQUIRED
DOCUMENT.TITLE.MAX_LENGTH
DOCUMENT.USAGE_STATE_REASON.REQUIRED
```

Der technische Fehlercode ist die stabile Identität des Fehlers.

Lokalisierte Fehlermeldungen werden davon getrennt behandelt.

Die UI verwendet zunächst deutsche Texte. Weitere Sprachen können später ergänzt werden, ohne die fachlichen Validatoren oder Fehlercodes zu verändern.

Ein Validierungsfehler kann zusätzlich mindestens enthalten:

```text
error_code
property
parameters
```

Die Transportabbildung ist über `AppError`, `ProblemDetails` sowie die Web-Abbildung auf `ModelState` festgelegt.

## Einsatzgrenzen von Behaviors

Behaviors enthalten keine domänenspezifische Fachlogik.

Nicht in generische Behaviors gehören beispielsweise:

```text
Katalog darf READY werden
Fragen für ein Audit materialisieren
Ergebnis eines Dokumentelements berechnen
CANCELED-Audit wieder öffnen
```

Solche Regeln verbleiben im jeweiligen Feature bzw. in gezielt wiederverwendbaren fachlichen BLL-Komponenten.

Grundsatz:

> Ein Behavior kapselt wiederkehrende Querschnittsfunktionalität. Sobald Code verstehen muss, was ein konkretes fachliches Objekt bedeutet, gehört er in der Regel in die Businesslogik.


## Deployment-Grundannahme

Auditarium wird containerfähig und ohne fachliche Abhängigkeit von einer bestimmten Container-Runtime umgesetzt.

Für den initialen Projektstand gilt:

```text
Entwicklung
→ VS Code Dev Container
→ Docker
→ Docker Compose

Container-Build / lokaler Betrieb
→ Docker als Referenz

Portabilität
→ Podman-kompatible Gestaltung anstreben
→ zunächst keine Support-Garantie

Nicht initial vorgesehen
→ Kubernetes-spezifische Deployment-Artefakte
→ eigene Clusterplattform
→ Microservice-Topologie
```

Docker + Compose ist damit die verbindliche Referenzumgebung, nicht Bestandteil der Business-Architektur.

Andere Container- oder Orchestrierungsplattformen werden erst dann zu offiziell unterstützten Zielplattformen, wenn dafür ein konkreter Bedarf besteht und die entsprechenden Artefakte sowie Tests gepflegt werden.

---

# 2. Persistenz
---

# 2. Persistenz, Datenbanken und Concurrency

## Grundregeln für Pflichtfelder, `NULL` und Defaults

Für das relationale Datenmodell gelten folgende übergreifende Regeln:

- Primärschlüssel sind `NOT NULL` und werden technisch erzeugt, sofern kein ausdrücklich reservierter oder fest vorgegebener Systemwert verwendet wird.
- Ein Fremdschlüssel ist `NOT NULL`, wenn die referenzierte Beziehung fachlich zwingend ist. Nur fachlich optionale Beziehungen dürfen `NULL` sein.
- Pflichttexte müssen nach Trimmen mindestens ein Zeichen enthalten.
- Optionale Textfelder ohne fachlichen Inhalt werden als `NULL` gespeichert und nicht als Leerstring.
- Boolesche Felder sind `NOT NULL`, sofern ausdrücklich kein fachlicher Drei-Zustands-Wert benötigt wird.
- Fachlich eindeutige Initialzustände erhalten einen Default, beispielsweise `ACTIVE` oder `DRAFT`.
- Defaults werden fachlich beschrieben; die konkrete technische Umsetzung erfolgt datenbankneutral über Anwendung, Entity Framework und Migrationen.
- Zeitstempel, die beim Erzeugen eines Datensatzes zwingend entstehen, sind `NOT NULL`. Bedingte Zeitstempel sind nur dann gesetzt, wenn das zugehörige fachliche Ereignis tatsächlich eingetreten ist.
- Zeitangaben werden technisch einheitlich als UTC behandelt; die Darstellung in der UI erfolgt in der jeweils gewünschten lokalen Zeitzone.
- Konditionale Pflichtregeln werden durch Anwendungslogik und, soweit plattformneutral sinnvoll, durch Datenbank-Constraints abgesichert.

Für Soft Delete gelten zusätzlich die bereits definierten Querschnittsfelder:

```text
deleted_at
deleted_by
deletion_reason
```

Dabei gilt:

```text
nicht soft gelöscht
→ deleted_at = NULL
→ deleted_by = NULL
→ deletion_reason = NULL

soft gelöscht
→ deleted_at IS NOT NULL
→ deleted_by IS NOT NULL
→ deletion_reason optional
```

Diese Felder werden nur an den jeweiligen Aggregate Roots geführt, für die Soft Delete vorgesehen ist.

## Schlüssel, Constraints und referenzielle Grundregeln

Auditarium unterscheidet bewusst zwischen:

```text
harte Datenbank-Invariante
→ direkt durch PK, FK, UNIQUE oder CHECK absichern

fachliche / transaktionale Regel
→ durch Anwendungslogik innerhalb einer Transaktion absichern
```

Datenbank-Constraints sollen nur Regeln erzwingen, die auf PostgreSQL und Microsoft SQL Server zuverlässig und verständlich abbildbar sind.

Provider-spezifische Datenbank-Tricks oder Trigger sind für fachliche Kernregeln nicht vorgesehen.

### Allgemeine Schlüsselregeln

- technisch erzeugte Primärschlüssel regulärer Datensätze beginnen bei `1`,
- `users.user_id = 0` ist die ausdrücklich reservierte Ausnahme für den Systembenutzer,
- Fremdschlüssel verweisen immer auf stabile technische IDs,
- fachliche Bezeichnungen, Titel oder Texte werden nicht als Ersatz für technische IDs verwendet.
- Eindeutigkeitsregeln für technische Textschlüssel dürfen nicht von der Standard-Collation der konkreten Datenbankinstallation abhängen.

### Allgemeine Löschregeln für Fremdschlüssel

Für physische Löschvorgänge gilt:

```text
abhängiger Bestandteil desselben Aggregats
→ CASCADE beim physischen Purge des Aggregate Roots

externe / historische fachliche Referenz
→ RESTRICT / NO ACTION
```

Soft Delete löst niemals Datenbank-Cascades aus.

Die konkrete SQL-Syntax kann zwischen PostgreSQL und Microsoft SQL Server abweichen; die fachliche Wirkung muss identisch bleiben.

Strukturierte Felder werden im fachlichen Modell datenbankneutral als JSON-Dokumente beschrieben. Die konkrete physische Speicherung und Typabbildung übernimmt Entity Framework über den jeweiligen Datenbankprovider. PostgreSQL- oder SQL-Server-spezifische JSON-Typen sind daher kein Bestandteil des fachlichen Zielmodells.

Datenmodell, Migrationen und Implementierung müssen beide freigegebenen Provider berücksichtigen. Datenbankspezifische Funktionen dürfen nur verwendet werden, wenn für beide Plattformen eine fachlich gleichwertige Umsetzung vorgesehen ist.

## Entity Framework und `IAuditariumDbContext`

Auditarium abstrahiert den konkreten DbContext, **nicht Entity Framework Core selbst**.

`IAuditariumDbContext` wird in der BLL definiert und vom DAL implementiert.

Konzeptionelles Beispiel:

```csharp
public interface IAuditariumDbContext
{
    DbSet<Document> Documents { get; }
    DbSet<CatalogVersion> CatalogVersions { get; }
    DbSet<DocumentElement> DocumentElements { get; }
    DbSet<Question> Questions { get; }
    DbSet<AuditUnit> AuditUnits { get; }
    DbSet<Audit> Audits { get; }
    DbSet<AuditDocumentElement> AuditDocumentElements { get; }
    DbSet<AuditQuestion> AuditQuestions { get; }
    DbSet<User> Users { get; }
    DbSet<UserIdentity> UserIdentities { get; }
    DbSet<SystemAuditLogEntry> SystemAuditLog { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
```

Die endgültige Liste der `DbSet`-Properties wird aus dem implementierten Datenmodell abgeleitet.

Die BLL darf EF Core als Persistenzwerkzeug verwenden, insbesondere:

```text
IQueryable<T>
DbSet<T>
AsNoTracking()
Where()
Select()
Include()
ThenInclude()
AnyAsync()
SingleAsync()
ToListAsync()
ExecuteUpdateAsync()
ExecuteDeleteAsync()
```

`ExecuteUpdateAsync()` und `ExecuteDeleteAsync()` umgehen den Change Tracker und den normalen `SaveChanges`-Ablauf. Bei protokollierungspflichtigen Änderungen sind sie deshalb nur zulässig, wenn der konkrete Pfad die zugehörigen Audit-Log-Einträge ausdrücklich erzeugt und gemeinsam mit der Änderung in derselben Datenbanktransaktion speichert. Dabei müssen auch die erforderlichen Vorher-/Nachher-Werte, betroffenen Objekt-IDs und Concurrency-Regeln korrekt berücksichtigt werden.

Solange ein solcher abgesicherter Pfad nicht implementiert und geprüft ist, werden protokollierungspflichtige Änderungen über getrackte Entities und den zentralen `SaveChangesAsync()`-Weg ausgeführt.

Nicht in die BLL gehören:

```text
UseNpgsql()
UseSqlServer()
konkrete Connection Strings
DbConnection
EntityTypeConfiguration<T>
Migrationen
provider-spezifisches SQL
AuditariumDbContext als konkrete Klasse
```

Die `DbSet`-Properties des Interfaces sind read-only definiert.

Ein generisches `Set<TEntity>()` muss nicht zusätzlich über das Interface veröffentlicht werden.

## Kein zusätzliches Repository- oder Unit-of-Work-Pattern

Auditarium führt kein zusätzliches generisches Repository-Pattern ein.

Nicht vorgesehen sind beispielsweise:

```text
IRepository<T>
IDocumentRepository
IAuditRepository
GenericRepository<T>
```

solange kein konkreter fachlicher oder technischer Bedarf dafür existiert.

Entity Framework Core stellt bereits die benötigten Query- und Änderungsmechanismen bereit.

Ebenso wird kein zusätzliches `IUnitOfWork` eingeführt.

Der reguläre Unit-of-Work-Mechanismus ist:

```text
AuditariumDbContext
→ SaveChangesAsync()
```

Explizite Datenbanktransaktionen werden für konkrete mehrschrittige Use Cases sowie bei Bedarf innerhalb des zentralen auditierten Speichervorgangs eingesetzt. Sie sichern dort die gemeinsame Persistenz von Fachänderungen und Audit-Log gemäß Kapitel 7.

Normale Handler verwenden weiterhin `SaveChangesAsync()` über `IAuditariumDbContext`. Die technische Koordination des auditierten Speichervorgangs liegt im DAL; dafür wird weder ein zusätzliches Repository noch ein `IUnitOfWork` eingeführt.

## Optimistische Concurrency

Auditarium verwendet für parallel bearbeitbare Daten optimistische Concurrency.

```text
User A liest Version 17
User B liest Version 17

User A speichert
→ Version wird 18

User B speichert auf Basis von Version 17
→ Concurrency Conflict
→ keine stille Überschreibung
```

Auditarium verwendet ausdrücklich kein generelles Last-Write-Wins-Verhalten.

## Providerneutraler Versionstoken

Für relevante bearbeitbare Objekte wird ein eigener providerneutraler Versionstoken verwendet.

```text
concurrency_version BIGINT
NOT NULL
DEFAULT 1
```

Entity Framework behandelt dieses Feld als Concurrency Token.

SQL-Server-spezifisches `rowversion` und PostgreSQL-spezifisches `xmin` werden nicht als gemeinsames fachliches Kernmodell verwendet.

## Update-Semantik

Ein Update darf nur erfolgreich sein, wenn der vom Client gelesene Versionstoken noch aktuell ist.

Semantisch:

```sql
UPDATE audits
SET
    name = ...,
    concurrency_version = 18
WHERE
    audit_id = 4711
    AND concurrency_version = 17;
```

```text
1 Row affected
→ Update erfolgreich

0 Rows affected
→ Concurrency Conflict
```

Der genaue SQL-Ausdruck wird von Entity Framework und dem jeweiligen Provider erzeugt; entscheidend ist die dargestellte Semantik.

## Einsatzbereich

Ein Concurrency Token wird nicht reflexartig jeder Tabelle hinzugefügt.

Typische Kandidaten:

```text
documents
catalog_versions
document_elements
questions
audit_units
audits
audit_questions
roles
users
zentrale App-Settings
```

Reine Zuordnungstabellen wie:

```text
role_permissions
user_roles
question_scope_types
```

erhalten nicht automatisch einen eigenen Concurrency Token.

## Concurrency bei Audit-Antworten

`audit_questions` sind ein besonders wichtiger Concurrency-Fall.

```text
Auditor A liest Antwort
result = NULL
version = 4

Auditor B liest dieselbe Antwort
result = NULL
version = 4

Auditor A speichert JA
→ version = 5

Auditor B versucht NEIN auf Basis von version = 4
→ Concurrency Conflict
```

Die Änderung von Auditor A wird nicht automatisch überschrieben.

## Versionstoken in ViewModels und Commands

Der Concurrency Token wird zwischen Lesen und Schreiben transparent mitgeführt.

Beispiel:

```text
AuditQuestionViewModel
├── ...
└── ConcurrencyVersion
```

```csharp
UpdateAuditQuestionCommand(
    QuestionId: 27,
    Result: ...,
    ConcurrencyVersion: 17);
```

Die UI zeigt den technischen Versionstoken nicht an.

## Concurrency Conflict als erwartbarer Fehler

Eine von Entity Framework ausgelöste `DbUpdateConcurrencyException` ist das technische Signal für einen möglichen Concurrency-Konflikt.

An der passenden Stelle wird sie in einen erwartbaren Application Error übersetzt.

```text
Code = AUDIT_QUESTION.CONCURRENCY_CONFLICT
Type = Conflict
```

Die API bildet diesen Fehler auf `409 Conflict` ab.

Die UI fordert zum Neuladen und erneuten Prüfen des aktuellen Stands auf.

## Keine automatische Zusammenführung

Bei konkurrierenden Benutzeränderungen führt Auditarium kein automatisches Merge durch.

Ebenso gibt es kein blindes Retry nach erneutem Laden des aktuellen Datenstands.

```text
Concurrency Conflict
→ aktuelle Version laden
→ alten Benutzerwert automatisch erneut darüber schreiben
```

ist ausdrücklich nicht zulässig.

## Intent-basierte Commands

Wo möglich, sollen Commands konkrete Benutzerabsichten ausdrücken, statt komplette Collections zu ersetzen.

Bevorzugt:

```text
AddPermissionToRole
RemovePermissionFromRole
```

gegenüber:

```text
ReplaceAllRolePermissions
```

Gezielte Commands reduzieren unnötige Concurrency-Konflikte und machen Businessregeln eindeutiger.

## Trennung von Concurrency und technischen Retries

Fachliche Concurrency-Konflikte und technische transiente Infrastrukturfehler sind getrennte Problemklassen.

```text
Concurrency Conflict
→ kein automatisches Retry
→ Benutzer-/Use-Case-Entscheidung erforderlich
```

Echte transiente technische Fehler dürfen über geeignete Retry-Mechanismen behandelt werden.

Ein technischer Retry darf niemals verwendet werden, um einen erkannten Benutzer-Concurrency-Konflikt automatisch zu überschreiben.

## Datenbankprovider und Migrationsstrategie

Schemaänderungen werden ausschließlich über versionierte Entity-Framework-Core-Migrationen durchgeführt. Manuelle, nicht versionierte Produktionsänderungen am Datenbankschema sind kein regulärer Betriebsweg.

Auditarium unterstützt zunächst:

```text
PostgreSQL
Microsoft SQL Server
```

Die Unterstützung beider DBMS dient der Wahlfreiheit des Betreibers. Pro Installation wird genau ein Datenbankprovider ausgewählt:

```text
Installation A
→ PostgreSQL

Installation B
→ Microsoft SQL Server
```

Alle Auditarium-Instanzen derselben Installation verwenden denselben ausgewählten Provider und dieselbe gemeinsame Datenbank. Ein paralleler Betrieb beider DBMS zur Speicherung der Daten derselben Installation ist nicht vorgesehen.

Beide Provider müssen denselben fachlichen Datenbankzustand und dieselben fachlichen Invarianten abbilden können.

Grundsatz:

> Fachmodell und Geschäftslogik bleiben providerneutral. Provider-spezifische Persistenzkonfiguration und Migrationsartefakte bilden die technischen Unterschiede der DBMS ab.

## Gemeinsames EF-Core-Modell

Auditarium verwendet:

```text
ein AuditariumDbContext
ein gemeinsames EF-Core-Modell
je einen EF-Core-Migrationssatz für PostgreSQL und Microsoft SQL Server
```

Das Datenmodell wird bewusst im gemeinsamen Funktionsumfang von PostgreSQL und Microsoft SQL Server gehalten.

Provider-spezifische Datenbankfeatures werden nicht verwendet, nur weil sie technisch verfügbar sind.

Beispiele für unerwünschte Abhängigkeiten:

```text
proprietäre Datentypen ohne gleichwertige Abbildung
provider-spezifische SQL-Fragmente ohne zwingenden Grund
providergebundene Index-Spezialfunktionen
providergebundene Concurrency-Mechanismen
```

## Anforderungen an unterstützte Datenbankprovider

Ein freigegebener Auditarium-Datenbankprovider muss mindestens die für das definierte Datenmodell benötigten relationalen Fähigkeiten zuverlässig unterstützen.

Dazu gehören insbesondere:

```text
Schemas
Foreign Keys
Unique Constraints
Check Constraints
Transaktionen
geeignete Identity-/Sequence-Erzeugung
BIGINT
große Textwerte
Indizes
EF Core Migrations
optimistische Concurrency mit BIGINT
```

Ein vorhandener EF-Core-Provider allein bedeutet nicht automatisch, dass die jeweilige Datenbank als Auditarium-Backend unterstützt wird.

## Provider-spezifische Migrationen

Die EF-Core-Werkzeuge erzeugen Migrationen für den jeweils aktiven Provider. Auditarium führt deshalb getrennte Migrationssätze einschließlich der zugehörigen Model Snapshots für PostgreSQL und Microsoft SQL Server.

Beide Migrationssätze werden aus demselben gemeinsamen Modell gepflegt:

```text
fachliche Modelländerung
→ Migration für PostgreSQL erzeugen und prüfen
→ Migration für Microsoft SQL Server erzeugen und prüfen
→ denselben fachlichen Zielzustand auf beiden Providern nachweisen
```

Die getrennten Migrationssätze sind technische Artefakte derselben Schemaentwicklung. Sie führen keine unterschiedlichen fachlichen Datenmodelle oder Geschäftsregeln ein.

Beim Start einer Installation wird ausschließlich der Migrationssatz des ausgewählten Providers verwendet. Die Migrationen entwickeln das Schema innerhalb dieses DBMS weiter.

Provider-spezifische Konfiguration und Migrationen bleiben in der Persistenzschicht gekapselt. Zusätzliche manuelle SQL-Sonderbehandlungen müssen technisch begründet werden.

Die technische Grundlage für dieses Vorgehen beschreibt die [EF-Core-Dokumentation zu Migrationen mit mehreren Providern](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers).

## Entwicklungsbetrieb

Im Entwicklungsbetrieb dürfen und sollen die normalen EF-Core-Werkzeuge verwendet werden.

Beispiele:

```powershell
Add-Migration <Name>
Update-Database
```

bzw.:

```text
dotnet ef migrations add <Name>
dotnet ef database update
```

Entwicklung, Tests und späterer Produktivbetrieb verwenden für den jeweils ausgewählten Provider denselben Migrationssatz.

## Automatische Migration im Produktivbetrieb

Im Produktivbetrieb führt Auditarium notwendige Datenbankmigrationen beim Start automatisch aus.

Grundablauf:

```text
Process Start
→ Configuration
→ DB-Verbindung
→ Migrationsstand prüfen
→ ggf. EF-Core-Migrationen anwenden
→ DAL Bootstrap / Reconcile
→ Datenbankzustand gültig?
→ Application starten
```

Eine separate Administratorkonsole oder ein manuell auszuführender Produktivschritt ist für den normalen Updateprozess nicht erforderlich.

## Vergleich von Code- und Datenbankstand

Vor dem normalen Application-Start werden die bekannten und bereits angewendeten EF-Core-Migrationen des ausgewählten Providers verglichen.

Es gibt drei gültige Entscheidungsfälle:

```text
DB == Code
→ keine Migration notwendig
→ Bootstrap / Reconcile
→ Start

DB < Code
→ ausstehende Migrationen anwenden
→ Bootstrap / Reconcile
→ Start

DB > Code
→ Start abbrechen
```

`DB > Code` bedeutet dabei, dass die Datenbank Migrationen enthält, die der laufende Code nicht kennt.

Beispiel:

```text
Datenbank bereits mit Auditarium 1.8 aktualisiert
+
versehentlich Auditarium 1.7 gestartet
→ DATABASE_SCHEMA_NEWER_THAN_APPLICATION
→ CRITICAL
→ Prozess beendet
```

## Kein automatisches Downgrade

Auditarium führt niemals automatisch Datenbank-Downgrades aus.

Ein älterer Application-Build darf nicht versuchen, einen bereits neueren Datenbankzustand rückwärts zu migrieren.

Grundsatz:

> Ein unbekannt neuer Datenbankstand ist ein Startfehler, kein Downgrade-Auftrag.

## Gleichzeitiger Start mehrerer Instanzen

Starten mehrere Instanzen derselben neuen Auditarium-Version gleichzeitig, wird die Migration nicht mehrfach parallel ausgeführt.

Auditarium verwendet hierfür den von EF Core bereitgestellten Migration-Lock.

Konzeptionell:

```text
Instance A v1.8 ─┐
Instance B v1.8 ─┼→ Migrationsprüfung
Instance C v1.8 ─┘
```

Eine Instanz erhält den Migration-Lock und wendet notwendige Migrationen an.

Die übrigen Instanzen erkennen nach Freigabe des Locks den bereits aktuellen Datenbankstand und fahren ohne erneute Migration fort.

Auch der anschließende DAL-Bootstrap/Reconcile wird installationsweit koordiniert. Der Schutz der Migrationen allein ersetzt diese Anforderung nicht. Es darf immer nur eine Instanz gleichzeitig Bootstrap/Reconcile ausführen; weitere startende Instanzen warten und prüfen anschließend den dann aktuellen Datenbankzustand gemäß Kapitel 3.

## Versionsupdate mit mehreren Instanzen

Bei einem Update auf eine neue Auditarium-Version werden laufende Instanzen der alten Version vor dem Start der neuen Version beendet.

Betriebsablauf:

```text
alte Auditarium-Instanzen stoppen
→ neue Codebasis ausrollen
→ erste neue Instanz startet
→ ggf. Migration
→ weitere neue Instanzen starten
→ alle erkennen aktuellen DB-Stand
```

Auditarium verfolgt ausdrücklich kein Zero-Downtime- oder Rolling-Upgrade-Modell.

Schemaänderungen müssen daher nicht gleichzeitig mit mehreren unterschiedlichen Auditarium-Versionen kompatibel bleiben.

## Fehlerverhalten bei Migrationen

Schlägt eine Migration oder die Prüfung des Datenbankstands fehl, startet Auditarium nicht in den normalen Application-Betrieb.

Nicht vorgesehen:

```text
Migration fehlgeschlagen
→ Bootstrap trotzdem versuchen
→ Web/API teilweise starten
```

Stattdessen:

```text
Migration / Schema Check failed
→ CRITICAL Logging / Telemetry
→ Startup abort
```

Erst ein erfolgreich geprüfter und gegebenenfalls migrierter Datenbankzustand darf an den DAL-Bootstrap/Reconcile übergeben werden.

---

# 3. Konfiguration, Secrets, Startup und Mehrinstanzbetrieb

## Konfigurations- und Settings-Modell

Auditarium trennt klar zwischen:

```text
systemdefinierten Setting-Metadaten im Code
UI-editierbaren Setting-Werten in der Datenbank
externen Host-/Deployment-Werten in Config und Environment
```

Grundregel:

> In der Datenbank stehen ausschließlich Settings, die Auditarium selbst über seine UI verändern darf.

Settings, die nicht UI-editierbar sind, werden niemals in der Settings-Tabelle gespeichert.

## Einheitlicher Setting-Namespace

Alle Settings verwenden denselben hierarchischen Key-Namespace.

Kanonische Schreibweise:

```text
Security:LocalPassword:MinimumLength
Security:LocalLockout:FailedAttempts
Security:ApiCredentials:DefaultLifetimeDays

Jobs:Retention:Schedule
Jobs:Retention:TimeZone
Jobs:Retention:RunOnStartup

Files:OriginalDocuments:MaxUploadSize

Observability:Otlp:Endpoint
Storage:RootPath
```

Der Doppelpunkt `:` ist der kanonische logische Separator.

Dadurch können Settings konsistent gruppiert und in der UI strukturiert dargestellt werden.

## Abbildung auf `appsettings` und Environment

`appsettings.json` bildet denselben Namespace hierarchisch ab.

Beispiel:

```json
{
  "Auditarium": {
    "Security": {
      "LocalPassword": {
        "MinimumLength": 15
      }
    }
  }
}
```

Environment Variablen verwenden die übliche .NET-Abbildung mit Doppel-Unterstrich:

```text
AUDITARIUM__Security__LocalPassword__MinimumLength=20
```

Logisch entspricht dies:

```text
Security:LocalPassword:MinimumLength
```

Die gleiche semantische Setting-ID wird damit unabhängig von der Quelle verwendet.

## Setting-Definitionen im Code

Jedes bekannte Setting besitzt eine Definition im Code.

Konzeptionell enthält eine `SettingDefinition` mindestens:

```text
Key
ValueType
DefaultValue
UiEditable
Secret
RestartRequired
Validation
Category
DisplayName
DisplayOrder
```

Die konkrete Typdefinition wird implementierungsnah festgelegt.

Die Code-Definition ist die kanonische Quelle für:

```text
Datentyp
Default
Validierungsregeln
UI-Editierbarkeit
Secret-Eigenschaft
Restart-Verhalten
Gruppierung und Anzeige
```

Die Datenbank definiert keine neuen Setting-Keys.

### Instanzbasierte Settings und `SettingDefinitionTemplate`

Neben statischen Settings darf der Code Templates für wiederholbare benannte Instanzen definieren.

Beispiel:

```text
Authentication:Providers:<provider_key>:Host
Authentication:Providers:<provider_key>:Port
Authentication:Providers:<provider_key>:TlsMode
Authentication:Providers:<provider_key>:BindUser
Authentication:Providers:<provider_key>:BindPassword
```

Der variable Teil `<provider_key>` bezeichnet eine gültige konkrete Instanz.

Der Code definiert weiterhin:

```text
zulässige Properties
Datentyp
Default
Validierung
UI-Editierbarkeit
Secret-Eigenschaft
Darstellung
```

Die konkrete Instanz liefert lediglich den stabilen Instanzschlüssel.

Damit gilt:

> Ein Setting-Key muss entweder einer statischen `SettingDefinition` oder einem code-definierten `SettingDefinitionTemplate` einer gültigen Instanz entsprechen.

Instanzbasierte Settings verwenden dieselbe allgemeine Quellen-, Prioritäts- und Override-Semantik wie alle anderen Auditarium-Settings.

## Priorität für UI-editierbare Settings

Für Settings mit:

```text
UiEditable = true
```

gilt folgende Priorität:

```text
Code Default / Fallback
        ↓
Config
        ↓
DB
        ↓
Environment
        ↓
Effective Value
```

Damit gilt ausdrücklich:

```text
DB überschreibt Config
Environment überschreibt DB
```

Environment besitzt immer die höchste Priorität.

Die Config bleibt als niedrigere Quelle/Fallback vorhanden, wird aber im Normalbetrieb durch einen vorhandenen DB-Wert übersteuert.

Für alle konfigurierbaren Auditarium-Komponenten gilt dieselbe Auflösungslogik.

Die UI zeigt mindestens:

```text
Configured Value
Effective Value
Source
Editable / externally overridden
```

Ein DB-Override bleibt als eigener Layer nachvollziehbar. Wird er entfernt, wird der darunterliegende Config-/Default-Wert wieder wirksam.

Diese allgemeine Regel gilt ausdrücklich auch für Authentication Provider, Jobs, Security Policies und spätere instanzbasierte Konfigurationsobjekte.

## Priorität für nicht UI-editierbare Settings

Für Settings mit:

```text
UiEditable = false
```

gilt:

```text
Code Default / Fallback
        ↓
Config
        ↓
Environment
        ↓
Effective Value
```

Diese Settings besitzen keinen DB-Wert.

Die UI darf sie anzeigen, aber nicht verändern.

Beispiele:

```text
Database:Provider
Database:ConnectionString
Storage:RootPath
Recovery:Enabled
OpenTelemetry:Otlp:Endpoint
```

## Darstellung in der Settings-UI

Die UI zeigt für jedes Setting mindestens:

```text
Setting
Effective Value
Source
Editable / ReadOnly
RestartRequired
```

Für ein UI-editierbares Setting mit Environment-Override beispielsweise:

```text
Security:LocalPassword:MinimumLength

Configured in Auditarium: 18
Effective Value:          24
Source:                   Environment
State:                    externally overridden
```

Ein extern überschriebener Wert kann in der UI nicht wirksam verändert werden, solange die höher priorisierte externe Quelle gesetzt ist.

Nicht UI-editierbare Settings erscheinen ausschließlich als ReadOnly.

## Persistenz UI-editierbarer Settings

Die Datenbank speichert nur die aktuellen Werte UI-editierbarer Settings.

Konzeptionell:

```text
application_settings
├── setting_key
├── serialized_value
└── concurrency_version
```

`serialized_value` enthält ein kleines selbstbeschreibendes JSON-Objekt:

```json
{
  "datatype": "int",
  "value": 14
}
```

Weitere Beispiele:

```json
{
  "datatype": "float",
  "value": 1.5
}
```

```json
{
  "datatype": "string",
  "value": "Europe/Berlin"
}
```

```json
{
  "datatype": "text",
  "value": "Auditarium wird heute ab 22:00 Uhr gewartet.\\nBitte speichern Sie Ihre Arbeit."
}
```

```json
{
  "datatype": "bool",
  "value": true
}
```

```json
{
  "datatype": "secret",
  "value": "audsec:v1:<protected-payload>"
}
```

Der gespeicherte Envelope ist bewusst einfach und datenbankneutral.

Es wird keine breite Tabelle mit separaten Spalten für verschiedene Datentypen oder einzelne Settings eingeführt.

## Generische Setting-Datentypen

Auditarium verwendet für persistierte Settings ausschließlich einen kleinen Satz generischer logischer Datentypen:

```text
int
float
string
text
bool
secret
```

Semantik:

```text
int
→ ganzzahliger Wert

float
→ Fließkommawert; logischer Settings-Typ, CLR-seitig typischerweise double

string
→ einzeilige Zeichenkette

text
→ mehrzeilige Zeichenkette

bool
→ true / false

secret
→ geschützter, wieder entschlüsselbarer Secret-Wert
```

`string` und `text` sind technisch Zeichenketten. Die Unterscheidung steuert insbesondere Darstellung und Editor in der UI.

Numerische Werte werden als echte JSON-Numbers gespeichert und nicht kulturabhängig als formatierte Strings.

## Enums in Settings

Enums sind kein eigener Persistenzdatentyp.

Wenn ein Setting seine zulässigen Werte aus einem Enum erhält, existiert dieses Enum im Code.

Beispiel:

```csharp
public enum MisfirePolicy
{
    Skip,
    RunOnce
}
```

Persistiert wird beispielsweise:

```json
{
  "datatype": "string",
  "value": "Skip"
}
```

Die zugehörige `SettingDefinition` kennt den erwarteten Basistyp und die zulässigen Enum-Werte.

Die UI kann daraus automatisch eine geeignete Auswahl erzeugen.

Die Datenbank definiert oder erweitert keine Enum-Werte.

## `SettingDefinition` bleibt kanonische Wahrheit

Der `datatype` im DB-Envelope macht den gespeicherten Wert selbstbeschreibend, ersetzt aber nicht die `SettingDefinition` im Code.

Beispiel:

```text
Code:
Security:LocalPassword:MinimumLength
datatype = int
```

DB:

```json
{
  "datatype": "bool",
  "value": true
}
```

bedeutet nicht, dass das Setting zu einem Boolean geworden ist.

Es bedeutet:

```text
inkonsistenter DB-Zustand
```

Beim Reconcile und beim Laden eines Settings müssen daher mindestens übereinstimmen:

```text
bekannter setting_key
erwarteter datatype
JSON-Wert zum datatype passend
fachliche Validierungsregeln
ggf. zulässiger Enum-Wert
```

## Nullable Values

Ob `value = null` zulässig ist, wird ausschließlich durch die `SettingDefinition` bestimmt.

Dies ist insbesondere für optionale Feature-Secrets sinnvoll:

```json
{
  "datatype": "secret",
  "value": null
}
```

Bedeutung:

```text
Secret nicht konfiguriert
```

Ein nicht-nullbarer Setting-Wert darf dadurch nicht umgangen werden.

## Seed/Reconcile der DB-Settings

Alle im Code definierten Settings mit:

```text
UiEditable = true
```

werden bei jedem Application-Start im DAL-Bootstrap/Reconcile geprüft.

Geprüft werden mindestens:

```text
Setting vorhanden?
Envelope syntaktisch gültig?
datatype entspricht SettingDefinition?
value ist für datatype deserialisierbar?
fachliche Validierung erfüllt?
ggf. Enum-Wert zulässig?
```

Fehlt ein Setting:

```text
gültiger Config-Wert vorhanden?
→ daraus DB-Initialwert bilden

sonst
→ Code-Default verwenden
```

Environment-Overrides werden nicht in die DB zurückgeschrieben.

Existiert ein gültiger Betreiberwert, bleibt er unverändert.

Ist ein gespeicherter Wert ungültig oder nicht mehr mit der Code-Definition vereinbar, wird er kontrolliert auf einen gültigen Seed-Wert zurückgeführt:

```text
gültige Config
→ verwenden

sonst
→ Code-Default
```

Eine solche Korrektur wird technisch protokolliert und – sobald der normale Audit-Log-Kontext dafür verfügbar ist – als relevante Systemänderung mit `user_id = 0` nachvollziehbar gemacht.

Grundsatz:

> Reconcile stellt Existenz und Gültigkeit systemdefinierter Settings sicher, ohne gültige Betreiberwerte auf Defaults zurückzusetzen.

Nicht mehr im Code definierte Setting-Keys werden nicht als neue gültige Settings behandelt; ihre Migrations-/Bereinigungsstrategie wird kontrolliert festgelegt.

## Concurrency und Audit-Logging für Settings

UI-editierbare Settings verwenden den bestehenden optimistischen Concurrency-Mechanismus.

```text
concurrency_version
```

verhindert stille Überschreibungen durch parallele Administration.

Änderungen werden über das bestehende `system_audit_log` nachvollziehbar protokolliert.

Für Secret-Settings werden dabei niemals Klartextwerte in `before_state` oder `after_state` geschrieben.

Bei normalen Settings darf der serialisierte Envelope protokolliert werden, sofern er keine sensiblen Inhalte enthält.

Bei `datatype = secret` werden höchstens nicht-sensitive Metadaten protokolliert, beispielsweise:

```text
configured_before = false
configured_after  = true
```

Der geschützte Ciphertext selbst wird nicht als fachlich hilfreicher Audit-Inhalt behandelt.

## Secrets in der Datenbank

Secrets sind nicht grundsätzlich aus der Datenbank ausgeschlossen.

Wenn ein Feature bewusst vollständig über die Auditarium-UI administrierbar ist, darf sein Secret ebenfalls als UI-editierbares Setting gespeichert werden.

Beispiele für mögliche spätere Feature-Secrets:

```text
Mail:Smtp:Password
Webhook:Secret
ExternalService:ApiKey
```

Solche Werte werden niemals im Klartext in `serialized_value` gespeichert.

Der Setting-Envelope verwendet:

```json
{
  "datatype": "secret",
  "value": "audsec:v1:<protected-payload>"
}
```

`audsec` kennzeichnet ein von Auditarium geschütztes Secret.

`v1` versioniert den Auditarium-Secret-Envelope unabhängig von der konkreten kryptografischen Implementierung.

## `ISecretProtector`

Auditarium definiert eine kleine Abstraktion zum Schutz UI-administrierbarer Secrets.

Konzeptionell:

```csharp
public interface ISecretProtector
{
    string Protect(
        string plaintext,
        string purpose);

    string Unprotect(
        string protectedValue,
        string purpose);
}
```

Die konkrete Signatur kann implementierungsnah noch um asynchrone Varianten oder Binärdaten erweitert werden.

Der kanonische `setting_key` wird für Setting-Secrets als `purpose` verwendet.

Beispiel:

```text
purpose = Mail:Smtp:Password
```

Dadurch ist ein geschützter Wert kryptografisch an seinen vorgesehenen Verwendungszweck gebunden und kann nicht sinnvoll durch bloßes Kopieren unter einen anderen Setting-Key verschoben werden.

Die Aufgabe des Protectors ist ausschließlich:

```text
Secret schützen
Secret wieder entschlüsseln
Purpose Isolation sicherstellen
```

Die Abstraktion enthält keine Businesslogik und kennt weder `application_settings` noch konkrete Features.

Die Abstraktion wird dort definiert, wo die Fähigkeit benötigt wird, also in der BLL, beispielsweise unter `Auditarium.Bll/Abstractions/Security`. Die erste konkrete Data-Protection-Implementierung liegt in der äußeren Infrastructure-Schicht. Die DAL speichert lediglich den geschützten Wert und übernimmt keine Kryptografie.

## Kryptografisches Modell für DB-Secrets

Für DB-Secrets wird symmetrische, authentifizierte Verschlüsselung verwendet.

Begründung:

```text
Auditarium muss den Wert später wieder lesen können
Schutz und Entschlüsselung erfolgen durch dieselbe vertrauenswürdige Anwendung
hohe Performance
einfachere Schlüsselrotation
keine unnötige asymmetrische Komplexität pro Secret
```

Asymmetrische Verschlüsselung wird nicht für jedes einzelne Secret eingesetzt.

Grundsatz:

> Nutzdaten werden symmetrisch geschützt; asymmetrische Kryptografie kann den Schlüsselbestand schützen.

## ASP.NET Core Data Protection als Zielimplementierung

Die erste Zielimplementierung des `ISecretProtector` verwendet ASP.NET Core Data Protection.

Data Protection stellt insbesondere bereit:

```text
authentifizierte Verschlüsselung
Schlüsselmanagement
Key Rotation
Purpose Isolation
Schutz gegen Manipulation
```

Ein eigener kryptografischer Algorithmus oder eine selbst entwickelte Verschlüsselungsschicht wird nicht implementiert.

Der Protector verwendet einen eigenen Purpose/Namensraum für Auditarium-Settings-Secrets, damit diese kryptografisch von anderen Data-Protection-Verwendungszwecken getrennt bleiben.

Die Entschlüsselung erfolgt bewusst explizit in der Settings-/Security-Schicht.

Nicht vorgesehen:

```text
EF ValueConverter
→ lädt DB-Wert
→ entschlüsselt Secret automatisch
```

Damit bleibt im Anwendungscode sichtbar, wann ein Secret in Klartext vorliegt.

## Data-Protection-Keyring

Der Data-Protection-Keyring muss persistent außerhalb der `application_settings`-Tabelle gespeichert werden.

Er darf nicht durch genau dasselbe Secret-System geschützt werden, das von diesem Keyring abhängig ist.

Mögliche Speicherorte:

```text
persistentes lokales Verzeichnis
gemountetes Volume
geschützter Netzwerkpfad
später geeigneter externer Key-Store
```

Insbesondere bei Containerbetrieb muss der Keyring einen Container-Neustart überleben.

Bei mehreren Auditarium-Instanzen verwenden alle Instanzen denselben persistenten Keyring und denselben Data-Protection-Application-Namensraum.

Dadurch können unter anderem:

```text
geschützte Setting-Secrets
Authentication-Cookies
weitere Data-Protection-Payloads
```

von jeder Instanz derselben Installation verarbeitet werden.

Sticky Sessions sind dadurch für die Cookie-Authentication nicht erforderlich.

Ohne den passenden Keyring dürfen bestehende geschützte DB-Secrets nicht unbemerkt unlesbar werden.

## Schutz des Keyrings

Der Keyring selbst soll nach Möglichkeit gegen unbefugtes Lesen geschützt werden.

Hier kann asymmetrische Kryptografie sinnvoll eingesetzt werden.

Beispiel:

```text
Data Protection Keys
→ symmetrische Nutzdatenverschlüsselung

X.509-Zertifikat / externer Key-Schutz
→ schützt die gespeicherten Data-Protection-Keys
```

Damit wird asymmetrische Kryptografie dort eingesetzt, wo sie tatsächlich einen Mehrwert liefert, ohne jedes einzelne Setting-Secret unnötig asymmetrisch zu verschlüsseln.

## Secrets in der UI

Die UI zeigt bestehende Secret-Werte niemals wieder im Klartext an.

Darstellung:

```text
SMTP Password
Configured: Yes

[ Neues Secret setzen ]
```

Nicht vorgesehen:

```text
aktuelles Secret anzeigen
Secret zurückkopieren
```

Beim Ersetzen eines Secrets wird ausschließlich der neue Wert entgegengenommen, geschützt und gespeichert.

## Infrastruktur-Secrets

Bestimmte Secrets gehören weiterhin ausschließlich in externe Host-/Deployment-Konfiguration.

Beispiele:

```text
Database Connection Secret
Recovery Password
Schlüssel/Zertifikat zum Schutz des Data-Protection-Keyrings
```

Diese Werte dürfen nicht in `application_settings` gespeichert werden.

Grundsatz:

> Auditarium darf Feature-Secrets verwalten, aber nicht die Wurzel seines eigenen kryptografischen Vertrauens in derselben Datenbank ablegen.

## Bootstrap als bewusste RBAC-Ausnahme

Der Datenbank-Bootstrap ist die einzige bewusste Ausnahme vom normalen RBAC.

Er wird nicht als Business-Use-Case behandelt und läuft nicht durch:

```text
Mediator
AuthorizationBehavior
ValidationBehavior
BLL Handler
IAuditariumDbContext
```

Stattdessen ist er Bestandteil der technischen Datenbankinitialisierung im DAL.

Grundsatz:

> Vor Application-Start darf der DAL-Bootstrap das RBAC-System herstellen. Nach Application-Start ist niemand über RBAC erhaben.

## DAL-Bootstrap

Der Bootstrap verwendet direkt den konkreten:

```text
AuditariumDbContext
```

im Projekt:

```text
Auditarium.Dal
```

Eine mögliche Struktur ist:

```text
Auditarium.Dal
├── AuditariumDbContext
├── Configurations
├── Migrations
└── Bootstrap
    ├── DatabaseBootstrapper
    ├── PermissionSeed
    ├── SystemRoleSeed
    ├── SystemUserSeed
    └── ApplicationSettingsSeed
```

Die konkrete Dateiaufteilung ist implementierungsnah und nicht normativ.

Der Architekturpunkt ist:

```text
Bootstrap
→ konkreter DbContext
→ direkte EF-Core-Operationen
→ kein BLL-/Mediator-Umweg
```

## Startup-Reihenfolge

Der technische Startup erfolgt in folgender Reihenfolge:

```text
Process Start
→ Configuration laden
→ DAL initialisieren
→ DB-Migrationsstand prüfen
→ DB älter als Code: ausstehende EF-Core-Migrationen anwenden
→ DB neuer als Code: Startup abbrechen
→ installationsweiten exklusiven Bootstrap-Zugriff über die Datenbank erwerben; ggf. begrenzt warten
→ DAL Bootstrap / Reconcile in einer Transaktion
   ├── Systemuser
   ├── code-definierte Permissions
   ├── System-/Default-Rollen
   ├── RolePermissions
   ├── Default-Admin
   ├── Default-Admin → SYSTEM_ADMIN
   ├── UI-editierbare Application Settings
   │   ├── Existenz
   │   ├── Datentyp/Envelope
   │   ├── Deserialisierung
   │   └── Validierung/Korrektur
   └── optional Recovery-Verarbeitung
→ Bootstrap-Transaktion abschließen und exklusiven Bootstrap-Zugriff freigeben
→ Datenbankzustand prüfen
   ├── ungültig → Startup abbrechen
   └── gültig   → Application starten
→ Mediator / Behaviors / RBAC / API / Web UI
```

Die fachliche Anwendung wird erst freigegeben, wenn die technische Initialisierung vollständig erfolgreich abgeschlossen wurde.

## Koordination des Bootstrap im Mehrinstanzbetrieb

Grundsatz:

> **Pro Installation darf immer nur eine Anwendungsinstanz gleichzeitig Bootstrap/Reconcile ausführen.**

Die gemeinsame Datenbank koordiniert den exklusiven Zugriff. Die Regel gilt für PostgreSQL und Microsoft SQL Server sowie für Erstinstallation und spätere Starts bei bereits aktuellem Schema.

Konzeptionell:

```text
Instanz A erhält exklusiven Bootstrap-Zugriff
→ aktuellen Zustand prüfen
→ Bootstrap/Reconcile in einer Transaktion ausführen
→ Transaktion abschließen
→ exklusiven Zugriff freigeben

Instanz B wartet
→ exklusiven Bootstrap-Zugriff erhalten
→ dann aktuellen Zustand erneut prüfen
→ nur noch erforderliche Korrekturen ausführen
→ Transaktion abschließen und exklusiven Zugriff freigeben
```

Ein vor dem Warten gelesener Zustand darf nicht ungeprüft als Grundlage für Änderungen verwendet werden. Insbesondere darf eine wartende Instanz ein inzwischen vorhandenes Initial-Credential weder erneut erzeugen noch ersetzen.

Jede startende Instanz wird erst nach erfolgreichem Abschluss ihrer eigenen Initialisierung für den Normalbetrieb freigegeben. Während des Wartens werden durch diese Instanz keine regulären Web-/API-Zugriffe oder Background Jobs verarbeitet; sie meldet keine Betriebsbereitschaft für den Normalbetrieb.

Die Wartezeit ist begrenzt. Bei Zeitüberschreitung, fehlgeschlagenem Erwerb des exklusiven Zugriffs oder fehlgeschlagenem Bootstrap bricht die betroffene Instanz ihren Start mit einer eindeutigen technischen Fehlermeldung ab. Eine Fortsetzung ohne die erforderliche Koordination ist nicht zulässig.

Nach einem Prozessabbruch darf keine dauerhaft verwaiste Sperre zurückbleiben. Ein nachfolgender Versuch prüft den tatsächlich vorhandenen Datenbankzustand erneut; nicht abgeschlossene Bootstrap-Änderungen unterliegen dem transaktionalen Rollback.

Die konkrete technische Umsetzung verwendet den EF-Core-Initialisierungsschutz. Die begrenzte Wartezeit ist ein nicht UI-editierbarer Betreiberwert aus Config oder Environment; ihr initialer Default beträgt 180 Sekunden. Eine Einbindung in geschützte EF-Core-Initialisierung oder eine separate Datenbanksperre ist zulässig, sofern die gesamte Bootstrap-/Reconcile-Phase einschließlich ihrer Transaktion geschützt ist. Zusätzliche Infrastruktur-Dienste oder ein allgemeines Cluster-Subsystem werden dafür nicht eingeführt.

## Reconcile-Verhalten

Bootstrap/Reconcile ist kein einmaliger Seed, sondern ein wiederholbarer Soll-/Ist-Abgleich für systemverwaltete Stammdaten.

Der Bootstrap/Reconciler darf erwartbare Abweichungen hart auf den definierten Sollzustand korrigieren.

Beispiele:

```text
fehlender Systemuser
→ INSERT

fehlende Permission
→ INSERT

fehlende Systemrolle
→ INSERT

fehlende RolePermission
→ INSERT

nicht vorgesehene RolePermission einer Systemrolle
→ DELETE

abweichende systemverwaltete Metadaten
→ UPDATE
```

Bei strukturell widersprüchlichen oder nicht eindeutig reparierbaren Zuständen wird nicht geraten.

Beispiele:

```text
mehrere Benutzer mit user_key = SYSTEM
user_id = 0 widerspricht dem erwarteten Systemkonto
mehrere Rollen mit demselben systemverwalteten role_key
```

In solchen Fällen gilt:

```text
CRITICAL
→ Bootstrap fehlgeschlagen
→ Application Startup abbrechen
```

## Transaktionaler Bootstrap

Der Bootstrap/Reconcile wird als atomarer technischer Initialisierungsschritt ausgeführt.

Dafür wird eine explizite Datenbanktransaktion verwendet.

Konzeptionell:

```text
BEGIN
→ Systemuser reconciliieren
→ Permissions reconciliieren
→ Systemrollen reconciliieren
→ RolePermissions reconciliieren
→ Default-Admin reconciliieren
→ Application Settings reconciliieren
→ Recovery ggf. anwenden
COMMIT
```

Schlägt ein notwendiger Schritt fehl:

```text
ROLLBACK
→ Application startet nicht
```

Ein teilweise korrigierter Sicherheits- oder Bootstrap-Zustand darf nicht in den Normalbetrieb gelangen.

## Recovery innerhalb des DAL-Bootstrap

Die Datenbankänderungen des Default-Admin-Recovery-Modus werden ebenfalls in der DAL-Bootstrap-Phase durchgeführt.

Der Ablauf ist:

```text
Migrationen
→ exklusiven Bootstrap-Zugriff erwerben
→ Bootstrap-/Reconcile-Transaktion beginnen
→ systemverwaltete Stammdaten und Settings reconciliieren
→ Recovery-Konfiguration prüfen
→ Default-Admin-Credential ggf. ersetzen
→ LOCAL-Sperre und Fehlversuchszähler zurücksetzen
→ Recovery-Credential als temporär mit erzwungenem Passwortwechsel markieren
→ Default-Admin aktivieren
→ notwendige Systemrolle sicherstellen
→ Bootstrap-/Reconcile-Transaktion committen
→ exklusiven Bootstrap-Zugriff freigeben
→ Bootstrap abschließen
```

Der DAL bringt dabei ausschließlich den Datenbankzustand in den definierten Recovery-Zustand.

Der Host entscheidet anschließend, ob Auditarium:

```text
im Normalbetrieb
```

oder:

```text
im eingeschränkten Recovery-Webmodus
```

gestartet wird.

## Audit-Logging während Bootstrap

Der Bootstrap darf nicht vom normalen fachlichen Audit-Logging abhängig sein, um überhaupt funktionieren zu können.

Die initiale Herstellung von:

```text
user_id = 0
```

ist daher ein technischer Sonderfall.

Sobald der Systembenutzer existiert, können weitere relevante Bootstrap-/Reconcile-Änderungen mit:

```text
system_audit_log.user_id = 0
```

protokolliert werden.

EF-Migrationen selbst gelten nicht als fachliche Audit-Actions.

Technische Startup-, Migration- und Bootstrap-Ereignisse werden über die technische Observability (`ILogger<T>`, OpenTelemetry) protokolliert.

## Einfacher Mehrinstanzbetrieb

Auditarium unterstützt sowohl Einzelinstanz- als auch Mehrinstanzbetrieb.

Das Mehrinstanzmodell bleibt bewusst einfach:

```text
Auditarium Instance A ─┐
Auditarium Instance B ─┼→ gemeinsame Datenbank
Auditarium Instance C ─┘
```

Alle Instanzen derselben Installation verwenden:

```text
dieselbe Datenbank
denselben gemeinsam erreichbaren FileStorage
denselben Data-Protection-Keyring
dieselben fachlichen Konfigurationswerte
```

Nicht erforderlich sind:

```text
Redis
Message Broker
Service Discovery
Distributed Cache
eigener Cluster Coordinator
Sticky Sessions
```

Grundsatz:

> Die gemeinsame Datenbank ist der primäre Koordinationspunkt des Auditarium-Mehrinstanzbetriebs.

## Technische `instance_id`

Jeder laufende Auditarium-Prozess erhält beim Start eine technische `instance_id`.

Sie dient ausschließlich:

```text
Job-Lease-Zuordnung
technischer Diagnose
Observability
```

Sie ist keine fachliche Entity und benötigt keine eigene dauerhafte Instanzverwaltung.

Ein Neustart darf eine neue `instance_id` erzeugen.

## DB-basierte Jobkoordination

Da alle Instanzen dieselbe Datenbank verwenden, erfolgt auch die Jobkoordination über diese Datenbank.

Ablauf bei einem geplanten Lauf:

```text
Instance A erreicht Cron-Termin ─┐
Instance B erreicht Cron-Termin ─┼→ JobCoordinator
Instance C erreicht Cron-Termin ─┘
                                  ↓
                           Lease atomar erwerben
                                  ↓
                    genau eine Instanz erfolgreich
```

Die Gewinnerinstanz führt den Job aus.

Alle anderen Instanzen behandeln eine vorhandene gültige Lease gemäß `SkipIfRunning`.

Damit bleibt der bestehende `JobCoordinator` die einzige zentrale Startlogik unabhängig davon, ob Auditarium mit einer oder mehreren Instanzen betrieben wird.

## Gemeinsam erreichbarer FileStorage

Im Mehrinstanzbetrieb ist keine spezielle verteilte Storage-Technologie erforderlich.

Voraussetzung ist lediglich, dass alle Auditarium-Instanzen denselben `StorageRoot` sehen.

Beispiele:

```text
SMB-Share
NFS-Share
gemeinsam gemountetes Volume
gemeinsamer Host-Pfad
```

Da `FileItem.SaveFilePath` relativ gespeichert wird, bleibt die physische Root-Konfiguration installationsabhängig.

Lokaler, nicht geteilter Storage ist nur für Einzelinstanzbetrieb zulässig.

## Gemeinsamer Data-Protection-Keyring

Mehrere Auditarium-Instanzen derselben Installation verwenden:

```text
denselben persistenten Data-Protection-Keyring
denselben Data-Protection-Application-Namensraum
```

Damit können alle Instanzen dieselben geschützten DB-Secrets und Authentication-Cookies verarbeiten.

Cookie Authentication benötigt daher keine Sticky Sessions.

Der Keyring kann beispielsweise auf demselben gemeinsamen Share/Volume liegen wie andere technische persistente Installationsdaten, sofern Berechtigungen und Schutzanforderungen eingehalten werden.

## Kein zusätzliches Cluster-Subsystem

Auditarium führt für den vorgesehenen Mehrinstanzbetrieb kein eigenes Cluster- oder Membership-System ein.

Nicht modelliert werden:

```text
dauerhafte Instanzregistrierung
Leader Election für die gesamte Anwendung
Service Discovery
Cluster Messaging
Node Membership
```

Koordination wird nur dort eingeführt, wo sie fachlich oder technisch tatsächlich benötigt wird.

Für v1 betrifft dies insbesondere:

```text
EF-Core-Migration-Lock
DB-basierter exklusiver Bootstrap-/Reconcile-Zugriff
DB-basierte Job-Leases
gemeinsamer FileStorage
gemeinsamer Data-Protection-Keyring
```

---

# 4. Identity, Authentication und Authorization

## Fachliches Benutzer- und Identity-Modell

### Architekturprinzip

Benutzerkonto und Authentifizierungsquelle werden voneinander getrennt.

> Ein Benutzer ist immer ein internes Objekt der Anwendung. Die Art der Anmeldung ist davon unabhängig.

### Tabelle `users`

Die Tabelle `users` enthält ausschließlich providerneutrale Benutzerinformationen. Credential- und Provider-spezifische Daten werden in den jeweiligen Identity-/Credential-Strukturen gehalten.

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `user_id` | `NOT NULL` | technisch erzeugt; `0` reserviert | interner Primärschlüssel |
| `user_key` | `NULL` erlaubt; für Systemkonten gesetzt | `NULL` | stabiler technischer Schlüssel für `SYSTEM` bzw. `DEFAULT_ADMIN` |
| `username` | `NOT NULL` | – | eindeutiger, kanonisch gespeicherter technischer Benutzername |
| `display_name` | `NOT NULL` | bei Bedarf aus `username` vorbelegt | Anzeigename |
| `email` | `NULL` erlaubt | `NULL` | optionale E-Mail-Adresse |
| `is_active` | `NOT NULL` | `true` | Benutzer darf sich grundsätzlich anmelden und Auditarium verwenden |

`username` und `display_name` müssen nicht leer sein.

`username` wird vor dem Speichern durch die Anwendung in eine einheitliche technische Schreibweise normalisiert. Für die erste Version wird der Wert getrimmt und in Kleinbuchstaben gespeichert. Die benutzerfreundliche Schreibweise gehört in `display_name`.

Die Eindeutigkeit darf nicht von der zufälligen Standard-Collation des Datenbankservers abhängen. Migrationen müssen für `username` auf PostgreSQL und Microsoft SQL Server eine fachlich gleichwertige, deterministische Vergleichssemantik sicherstellen.


Für Benutzer gilt:

```text
is_active = true
→ Benutzer darf sich grundsätzlich anmelden und Auditarium verwenden

is_active = false
→ Benutzer ist deaktiviert und darf sich nicht anmelden
→ keine effektiven Permissions bei weiteren Berechtigungsprüfungen
→ Logout einer bestehenden Sitzung bleibt möglich
```

Die Änderung von `is_active` ist reversibel und wird im `system_audit_log` protokolliert.

Für interne Systemaktionen existiert ein reservierter Systembenutzer:

```text
user_id = 0
```

Für diesen Datensatz gilt abweichend:

```text
username = system
is_active = false
```

Der Systembenutzer:

- existiert dauerhaft in `users`,
- ist nicht anmeldbar,
- ist nicht löschbar,
- ist nicht über die normale Benutzerverwaltung editierbar,
- wird ausschließlich für intern ausgelöste Aktionen verwendet.

Reguläre Benutzer und technische/API-Benutzer verwenden IDs ab `1`.

API-Zugänge sind keine eigenständigen Akteure. Jeder API-Zugang ist einem Benutzer zugeordnet und wird über dessen Rollen und Berechtigungen autorisiert.


`user_key` ist für gesetzte Werte eindeutig. Normale Benutzer besitzen `user_key = NULL`.
### Tabelle `user_identities`

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `identity_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `user_id` | `NOT NULL` | – | Referenz auf den internen Benutzer |
| `authentication_provider_id` | `NOT NULL` | – | Referenz auf eine konkrete Authentication-Provider-Instanz |
| `external_id` | `NOT NULL` | – | stabiler Identifikator innerhalb dieser Provider-Instanz |

Die Kombination

```text
(authentication_provider_id, external_id)
```

muss eindeutig sein.

Die Authentifizierungsart wird nicht redundant in `user_identities` gespeichert. Sie ergibt sich aus der referenzierten Provider-Instanz und deren code-definiertem Provider-Typ.

Beispiele:

```text
User 17
→ Provider-Instanz LDAP_FOO
→ external_id <stabile Directory-ID>

User 23
→ Provider-Instanz LOCAL
→ external_id defaultadmin

User 42
→ Provider-Instanz API
→ external_id checkmk
```

#### Regeln

- Passwort-Hashes existieren nur für `LOCAL`; API-Secrets werden getrennt als nicht rückrechenbare Secret-Hashes gespeichert.
- LDAP-Passwörter werden niemals gespeichert.
- Externe Identitäten werden über stabile externe IDs zugeordnet.
- Berechtigungen hängen am internen `user_id`.
- Provider-Typ, Provider-Instanz und Rollenmodell sind getrennte Verantwortlichkeiten.
- Spätere Provider wie OIDC oder SAML müssen dasselbe Provider-Instanz-/Identity-Schema verwenden und dürfen keinen parallelen Identity-Pfad einführen.
- Provider-spezifische Credential-Daten werden im technischen Authentifizierungsdesign definiert und gehören nicht in das fachliche Audit-Datenmodell.

---

## Rollenmodell – fachliche Sicht

Das Rollenmodell basiert auf den in diesem Soll- und Pflichtenheft definierten Permissions. Benutzerdefinierte Rollen sind frei konfigurierbare, additive Sammlungen dieser Permissions. Die ausgelieferten System-/Default-Rollen mit gesetztem `role_key` sind davon ausgenommen und werden vom Startup-Reconcile verwaltet. Konkrete Berechtigungsentscheidungen werden nicht dauerhaft an Rollennamen im Code gekoppelt.


Rollen sind **additive Berechtigungsbündel**.

Es gibt:

- keine Hierarchie,
- keine automatische Vererbung,
- keinen impliziten Superuser durch die Bezeichnung „Admin“.

Vorgesehene Rollen:

| Rolle | Schwerpunkt |
|---|---|
| `SYSTEM_ADMIN` | Benutzer, Rollen, Authentifizierung, technische Systemeinstellungen |
| `AUDIT_MANAGER` | Regelwerke, Kataloge, Audit Units, Audits anlegen und organisatorisch verwalten |
| `AUDITOR` | Audits durchführen, Fragen beantworten und finalisieren |
| `REVIEWER` | Ergebnisse lesen, prüfen und auswerten |
| `VIEWER` | reine Leserechte auf abgeschlossene Inhalte und Reports |

Ein Benutzer kann mehrere Rollen besitzen.

---

## Rechte-Matrix – fachliche Ausgangssicht

| Aktion | SYSTEM_ADMIN | AUDIT_MANAGER | AUDITOR | REVIEWER | VIEWER |
|---|:---:|:---:|:---:|:---:|:---:|
| Benutzer anlegen / deaktivieren | ✓ | | | | |
| Rollen zuweisen / entziehen | ✓ | | | | |
| Authentifizierung konfigurieren | ✓ | | | | |
| Systemeinstellungen verwalten | ✓ | | | | |
| Dokumente und Kataloge anlegen / importieren / bearbeiten | | ✓ | | | |
| Dokumente auf `ACTIVE` / `DEPRECATED` setzen | | ✓ | | | |
| `audit_units` anlegen / bearbeiten / deaktivieren | | ✓ | | | |
| Audit als `DRAFT` anlegen | | ✓ | | | |
| Audit-`DRAFT` bearbeiten | | ✓ | | | |
| Audit-`DRAFT` löschen | | ✓ | | | |
| Audit publishen (`DRAFT → READY`) | | ✓ | | | |
| unbeantwortetes `READY`-Audit löschen | | ✓ | | | |
| Auditor-Zuweisung setzen / ändern / lösen | | ✓ | | | |
| freies Audit selbst claimen | | | ✓ | | |
| eigene Auditor-Zuweisung freigeben | | | ✓ | | |
| Audit abbrechen (`READY` / `IN_PROGRESS → CANCELED`) | | ✓ | | | |
| `CANCELED`-Audit wieder öffnen | | ✓ | | | |
| Auditfragen beantworten / ändern / zurücksetzen | | | ✓ | | |
| Audit finalisieren | | | ✓ | | |
| laufende Audits ansehen | | ✓ | ✓ | ✓ | |
| finalisierte Audits ansehen | | ✓ | ✓ | ✓ | ✓ |
| Audit-Ergebnisse fachlich auswerten | | ✓ | | ✓ | |
| Reports erstellen / filtern / exportieren | | ✓ | | ✓ | ✓ |
| System-/Audit-Historie einsehen | ✓ | ✓ | | ✓ | |

Rollen sind additiv. `SYSTEM_ADMIN` erhält durch seine technische Rolle keine automatischen fachlichen Auditrechte.

## Autorisierungsmodell

Auditarium verwendet ein permission-basiertes RBAC-Modell.

Grundprinzip:

```text
Code
→ definiert verfügbare Permissions

DB
→ Rollen bündeln Permissions

DB
→ Benutzer erhalten Rollen

BLL
→ berechnet effektive Permissions

AuthorizationBehavior
→ erzwingt Permissions

UI
→ verwendet effektive Permissions zur Darstellung
```

Permissions beschreiben stabile fachliche Fähigkeiten und sind nicht an konkrete UI-Controller, Methoden oder technische Einstiegspunkte gekoppelt.

Beispiele:

```text
Documents.List
Documents.Read
Documents.Create
Documents.Update
Documents.Delete
Documents.ChangeUsageState

CatalogVersions.Create
CatalogVersions.Publish

Audits.Create
Audits.Read
Audits.ReadFinalized
Audits.Claim
Audits.ReleaseOwn
Audits.Assign
Audits.Answer
Audits.Finalize
Audits.Cancel
Audits.Reopen

Users.Manage
Roles.Manage
```

Die konkrete Permission-Liste wird im Code bedarfsgerecht erweitert.

## Berechtigungsumfang und Auditor-Zuweisung

### Keine individuellen Objekt-ACLs im initialen Sollstand

Auditarium verwendet im initialen Sollstand **keine benutzer-, rollen- oder objektspezifischen ACLs auf einzelne Fachobjekte**.

Insbesondere existieren keine generischen Konstrukte wie:

```text
object_permissions
user_object_grants
role_object_grants
owner_permissions
```

Permissions beschreiben Fähigkeiten auf fachlicher Ebene.

Beispiel:

```text
Audits.Read
Audits.Answer
Audits.Finalize
```

Die API besitzt kein separates Berechtigungsmodell. Web, API und interne Use Cases verwenden dieselben BLL-Requests und dieselben Permission-Prüfungen.

Grundsatz:

> **Permission entscheidet, ob ein Benutzer eine Fähigkeit grundsätzlich besitzt. Fachlicher Objektzustand und Audit-Zuweisung entscheiden zusätzlich, ob diese Fähigkeit auf dem konkreten Audit ausgeführt werden darf.**

Eine spätere organisatorische Einschränkung nach Standort, Organisation oder Audit-Unit darf bei tatsächlichem Bedarf als separates fachliches Scope-Modell ergänzt werden. Individuelle Datensatz-ACLs werden dafür nicht vorsorglich eingeführt.

### Auditor-Zuweisung ist Workflow, keine ACL

Veröffentlichte Audits können genau einem Auditor zur aktiven Bearbeitung zugewiesen sein.

Die Zuweisung ist:

```text
Arbeitsverantwortung
+
exklusive Bearbeitungssperre
```

Sie ist ausdrücklich **keine Berechtigungsquelle**.

Damit gilt:

```text
Permission vorhanden
+
Audit fachlich bearbeitbar
+
Audit dem aktuellen Benutzer zugewiesen
→ fachliche Bearbeitung erlaubt
```

Eine Zuweisung an einen Benutzer ohne die erforderliche Permission verleiht diese Permission nicht.

### Sichtbarkeit

Ein Claim oder eine Zuweisung macht ein Audit für andere berechtigte Benutzer nicht unsichtbar.

Für Benutzer mit entsprechender Leseberechtigung gilt:

```text
freies Audit
→ sichtbar
→ read-only
→ für berechtigten Auditor claimbar

Audit mir zugewiesen
→ sichtbar
→ bei vorhandenen Bearbeitungs-Permissions editierbar

Audit anderem Auditor zugewiesen
→ sichtbar
→ read-only
→ deutlich als fremd in Bearbeitung gekennzeichnet

FINALIZED
→ entsprechend Leseberechtigung sichtbar
→ read-only

CANCELED
→ entsprechend Leseberechtigung sichtbar
→ read-only
```

Grundsatz:

> **Permission entscheidet über Sichtbarkeit. Zuweisung entscheidet bei bearbeitbaren Audits über die exklusive Bearbeitung.**

Die UI zeigt bei einer aktiven Zuweisung den zuständigen Auditor eindeutig an, beispielsweise:

```text
In Bearbeitung durch <Display Name>
```

### Claim

Ein Auditor mit `Audits.Claim` darf ein nicht zugewiesenes Audit selbst übernehmen.

Claim ist nur für fachlich bearbeitbare, veröffentlichte Audits vorgesehen:

```text
READY
IN_PROGRESS
```

Der Claim:

- setzt die Auditor-Zuweisung auf den aktuellen Benutzer,
- verändert den Audit-State nicht,
- verändert keine vorhandenen Antworten,
- wird im `system_audit_log` protokolliert.

Der Vorgang muss atomar sein.

Wenn zwei Auditoren gleichzeitig dasselbe freie Audit claimen, darf genau einer erfolgreich sein.

Der zweite Vorgang endet als erwarteter Konflikt, beispielsweise:

```text
AUDIT.ALREADY_ASSIGNED
Type = Conflict
```

Es erfolgt keine automatische Mehrfachzuweisung.

### Eigene Zuweisung freigeben

Ein zugewiesener Auditor mit `Audits.ReleaseOwn` darf seine eigene Zuweisung jederzeit wieder freigeben.

Dabei gilt:

```text
assigned_auditor_user_id = NULL
```

Die Freigabe:

- verändert den Audit-State nicht,
- löscht oder verändert keine Antworten,
- löscht keine Kommentare oder Nachweise,
- setzt keine `answered_at`-/`answered_by`-Informationen zurück,
- macht ein `READY`- oder `IN_PROGRESS`-Audit wieder claimbar,
- wird im `system_audit_log` protokolliert.

Beispiel:

```text
IN_PROGRESS
assigned to Auditor A
→ ReleaseOwn
→ IN_PROGRESS
→ unassigned
→ anderer Auditor kann claimen und weiterarbeiten
```

### Administrative Zuweisung

Ein Benutzer mit `Audits.Assign`, typischerweise der `AUDIT_MANAGER`, darf:

```text
freies Audit → Auditor A
Auditor A → Auditor B
Auditor A → unassigned
```

Eine Zuweisung darf nur auf einen aktiven Benutzer erfolgen, der grundsätzlich die für die Auditbearbeitung erforderliche Permission besitzt.

Die administrative Zuweisung:

- verändert den Audit-State nicht,
- verändert keine Antworten,
- verleiht keine zusätzlichen Permissions,
- wird vollständig im `system_audit_log` protokolliert.

Ein `AUDIT_MANAGER` darf durch `Audits.Assign` nicht automatisch Auditfragen beantworten. Dafür benötigt auch er die entsprechenden Bearbeitungs-Permissions und eine gültige eigene Zuweisung.

### Exklusive Bearbeitung

Für fachliche Änderungen an einem veröffentlichten Audit gilt:

```text
Audit READY oder IN_PROGRESS
AND
CurrentUser besitzt benötigte Permission
AND
assigned_auditor_user_id = CurrentUser
```

Erst dann dürfen insbesondere folgende Aktionen erfolgen:

```text
Auditfrage beantworten
Antwort ändern
Antwort zurücksetzen
Kommentar ändern
Nachweis ändern
Audit finalisieren
```

Ein nicht zugewiesenes Audit ist für Auditoren zunächst read-only und muss vor der Bearbeitung geclaimt oder administrativ zugewiesen werden.

Ein einem anderen Auditor zugewiesenes Audit bleibt lesbar, ist aber für normale fachliche Bearbeitungsoperationen gesperrt.

### Finalisierung und Zuweisung

Beim erfolgreichen Übergang:

```text
IN_PROGRESS → FINALIZED
```

wird die aktive Auditor-Zuweisung innerhalb derselben fachlichen Transaktion entfernt:

```text
assigned_auditor_user_id = NULL
```

Damit gilt als Invariante:

> **Ein `FINALIZED`-Audit besitzt niemals eine aktive Auditor-Zuweisung.**

Die historische Nachvollziehbarkeit bleibt erhalten durch:

```text
audit_questions.answered_by
audit_questions.answered_at
system_audit_log
```

sowie den protokollierten Finalisierungsvorgang und die vorherigen Assignment-Ereignisse.

Schlägt die Finalisierung fehl, darf auch die Auditor-Zuweisung nicht entfernt werden.

## Permission-Datenmodell

Folgende Entitäten sind vorgesehen:

```text
permissions
roles
role_permissions
user_roles
```

### `permissions`

Konzeptionell mindestens:

```text
permission_key
description
is_active
```

`permission_key` ist der stabile technische Primärschlüssel, beispielsweise:

```text
Documents.Delete
```

Permissions werden durch Auditarium definiert. Betreiber können keine beliebigen neuen Permission-Keys erfinden.

Zusätzliche beschreibende Felder wie Kategorie oder Anzeigename können später ergänzt werden.

### `roles`

Konzeptionell mindestens:

```text
role_id
role_key
name
description
is_active
```

Benutzerdefinierte Rollen (`role_key = NULL`) sind frei konfigurierbare Sammlungen von Permissions. Systemverwaltete Rollen (`role_key != NULL`) folgen dem im Code definierten Sollzustand.

### `role_permissions`

Zuordnung:

```text
role_id
permission_key
```

Die Kombination ist eindeutig.

### `user_roles`

Zuordnung:

```text
user_id
role_id
```

Die Kombination ist eindeutig.

## Additives Rollenmodell

Berechtigungen wirken ausschließlich additiv.

Beispiel:

```text
User
├── Role A
│   ├── Documents.List
│   └── Documents.Read
│
└── Role B
    ├── Documents.Create
    └── Documents.Update
```

Effektive Permissions:

```text
Documents.List
Documents.Read
Documents.Create
Documents.Update
```

Ein explizites `DENY`-Modell ist nicht vorgesehen.

Damit existieren keine Prioritäts- oder Konfliktregeln zwischen Rollen.

Für deaktivierte Objekte gilt:

```text
permission.is_active = false
→ Permission ist installationsweit nicht effektiv

role.is_active = false
→ Rolle liefert keine effektiven Permissions
```

Zuweisungen müssen dafür nicht gelöscht werden.

## Permission-Definition im Code

Die verfügbaren Permissions werden zentral im Code definiert.

Beispiel:

```csharp
public static class PermissionKeys
{
    public static class Documents
    {
        public const string List = "Documents.List";
        public const string Read = "Documents.Read";
        public const string Create = "Documents.Create";
        public const string Update = "Documents.Update";
        public const string Delete = "Documents.Delete";
        public const string ChangeUsageState = "Documents.ChangeUsageState";
    }
}
```

Die Datenbank spiegelt diese im Code vorhandenen Permissions wider.

Grundsatz:

> Der Code definiert, welche Fähigkeiten Auditarium besitzt. Die Datenbank definiert, wie Betreiber diese Fähigkeiten organisatorisch verteilen.

Die Synchronisierung zwischen Code und `permissions`-Tabelle erfolgt beim Startup im DAL-Bootstrap/Reconcile. Code-definierte Permissions werden vor dem Reconcile der systemverwalteten Rollen hergestellt.

## `IPermissionEvaluator`

Die BLL stellt einen zentralen Permission-Evaluator bereit.

Konzeptionell:

```csharp
public interface IPermissionEvaluator
{
    Task<bool> HasPermissionAsync(
        long userId,
        string permission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetPermissionsAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
```

Für reguläre und technische/API-Benutzer mit `user_id >= 1` wird vor der Ermittlung effektiver Permissions der aktuelle Aktivierungszustand geprüft:

```text
users.is_active = false
→ effektive Permissions = leere Menge
→ HasPermissionAsync = false

users.is_active = true
→ effektive Permissions aus den aktiven Rollenzuordnungen ermitteln
```

Ein vorhandenes Authentication-Cookie oder ein noch gültiges API-Credential ersetzt diese Prüfung nicht. Die Deaktivierung muss nach erfolgreichem Speichern spätestens beim nächsten autorisierten Use Case wirksam sein, unabhängig davon, welche Anwendungsinstanz ihn ausführt. Veraltete Cookie-Daten oder zwischengespeicherte Berechtigungen dürfen die Sperre nicht verzögern.

Logout ist eine ausdrücklich erlaubte, permissionunabhängige Sitzungsaktion. Dafür wird einem deaktivierten Benutzer keine fachliche oder administrative Permission hinzugefügt.

Der ausschließlich intern verwendete Systemactor mit `user_id = 0` folgt der gesonderten Regel im Abschnitt „Systemactor und RBAC“. Sein bewusst gesetztes `is_active = false` verhindert Benutzeranmeldungen, sperrt aber nicht die unter System-RBAC autorisierten internen Vorgänge.

Für aktive Benutzer sowie den zulässigen internen Systemactor ergeben sich die effektiven Permissions aus:

```text
User
→ aktive UserRoles
→ aktive Roles
→ RolePermissions
→ aktive Permissions
```

Die Logik zur Ermittlung effektiver Permissions existiert nur an einer zentralen Stelle.

Der `AuthorizationBehavior` und UI-bezogene Permission-Abfragen verwenden denselben Evaluator.

## Autorisierung von CQRS-Requests

Die Berechtigungsprüfung wird an den CQRS-Request gebunden und nicht an einen UI-Endpunkt.

Beispiel:

```csharp
[RequiresPermission(PermissionKeys.Documents.Delete)]
public sealed record DeleteDocumentCommand(...)
    : ICommand;
```

Der Authorization-Behavior ermittelt die deklarierte Permission und prüft sie gegen die effektiven Permissions des aktuellen Benutzers.

Grundmuster:

```text
Request
→ AuthorizationBehavior
→ erforderliche Permission
→ IPermissionEvaluator
→ erlaubt?
   ├── nein → Forbidden
   └── ja   → nächster Behavior / Handler
```

Damit gilt dieselbe Sicherheitsregel unabhängig davon, ob ein Request durch

```text
Auditarium.Api
Auditarium.Web
einen späteren Background-Prozess
einen anderen UI-/Client-Einstiegspunkt
```

ausgelöst wird.

## Default-Deny

Jeder über den Mediator erreichbare Request muss seine Security-Anforderung ausdrücklich deklarieren.

Erlaubte Varianten:

```text
RequiresPermission
```

oder:

```text
AllowAnonymous
```

Beispiel:

```csharp
[RequiresPermission(PermissionKeys.Documents.Create)]
public sealed record CreateDocumentCommand(...)
    : ICommand<CreateDocumentResult>;
```

und für einen bewusst anonym erlaubten Use Case:

```csharp
[AllowAnonymous]
public sealed record LoginCommand(...)
    : ICommand<LoginResult>;
```

Fehlt jede Security-Deklaration, gilt:

```text
Default-Deny
→ Request wird nicht ausgeführt
```

Damit kann ein neu implementierter Use Case nicht versehentlich ungeschützt über den Mediator erreichbar werden.

Für später ist zusätzlich ein Architecture-Test bzw. Roslyn Analyzer vorgesehen, der sicherstellt, dass jeder Mediator-Request genau eine gültige Security-Deklaration besitzt.

## Permissions in der UI

UI-Projekte werten keine Rollen selbst aus.

Sie fragen ausschließlich effektive Permissions des aktuellen Benutzers ab.

Beispiel:

```text
GetCurrentUserPermissionsQuery
→ IPermissionEvaluator
→ Set<string> der effektiven Permission-Keys
```

Die UI kann darauf aufbauend Steuer- und Navigationselemente ausblenden.

Beispiel:

```text
Documents.Delete nicht vorhanden
→ Delete-Button nicht rendern
```

Die UI-Prüfung ist ausschließlich eine Komfort- und Darstellungsfunktion.

Grundsatz:

> Die UI verwendet Permissions zur Darstellung. Die BLL verwendet Permissions zur Sicherheit.

Selbst wenn ein Benutzer einen ausgeblendeten UI-Weg technisch umgeht oder einen API-Aufruf manuell erzeugt, verhindert der `AuthorizationBehavior` die Ausführung des nicht erlaubten Requests.

Für UI-Code kann eine ergonomische Hilfsabstraktion vorgesehen werden, beispielsweise:

```text
PermissionView.Can(PermissionKeys.Documents.Delete)
```

ohne dass dadurch Rollen oder Berechnungslogik in die UI verlagert werden.

## Systemverwaltete Rollen

System- und Default-Rollen besitzen einen stabilen technischen Schlüssel:

```text
role_key
```

Beispiele:

```text
SYSTEM_ADMIN
AUDIT_MANAGER
AUDITOR
REVIEWER
VIEWER
```

Regel:

```text
role_key != NULL
→ systemverwaltete Rolle
→ nicht über normale UI/API editierbar
→ wird bei jedem Start vollständig reconciled

role_key == NULL
→ benutzerdefinierte Rolle
→ vollständig in Betreiberhoheit
```

Für systemverwaltete Rollen wird beim Startup der im Code definierte Sollzustand vollständig hergestellt. Dazu gehören mindestens Existenz, `role_key`, Name, Beschreibung, Aktivierungszustand und die vollständige Permission-Zuordnung.

Fehlende Permission-Zuordnungen werden ergänzt, nicht vorgesehene Zuordnungen werden entfernt. Damit kann insbesondere `SYSTEM_ADMIN` seine erforderlichen Berechtigungen nicht dauerhaft verlieren.

## Benutzer-Schlüssel für Systemkonten

Für besondere systemverwaltete Benutzer wird ein stabiler technischer Schlüssel eingeführt:

```text
user_key
```

Vorgesehene Werte:

```text
SYSTEM
DEFAULT_ADMIN
```

Regeln:

```text
user_key = SYSTEM
→ reservierter Systembenutzer
→ user_id = 0

user_key = DEFAULT_ADMIN
→ reservierter Default-Administrator

user_key = NULL
→ normaler oder technischer Benutzer
```

`user_key` ist für systemverwaltete Benutzer stabil und eindeutig.

## Systembenutzer

Der Systembenutzer wird bei jedem Start vollständig gegen den definierten Sollzustand reconciled.

```text
user_id   = 0
user_key  = SYSTEM
username  = system
is_active = false
```

Der Systembenutzer ist nicht anmeldbar, nicht löschbar und nicht über normale Benutzerverwaltung editierbar. Er besitzt keine regulär verwendbare Login-Identity und dient ausschließlich als technischer Akteur für intern ausgelöste Aktionen.

Sobald der Systembenutzer verfügbar ist, werden weitere Bootstrap-/Reconcile-Aktionen mit

```text
system_audit_log.user_id = 0
```

protokolliert.

Die initiale Erzeugung von `user_id = 0` ist der einmalige technische Bootstrapping-Sonderfall vor der normalen Audit-Log-Zuordnung.

## Default-Administrator im Normalbetrieb

Der Default-Administrator ist ein systemverwaltetes Break-Glass-Konto mit:

```text
user_key = DEFAULT_ADMIN
```

Beim normalen Application Startup wird sichergestellt:

- der Default-Administrator existiert,
- sein technischer `user_key` korrekt ist,
- die vorgeschriebenen systemverwalteten Rollen zugeordnet sind,
- insbesondere die Rolle `SYSTEM_ADMIN` vorhanden ist.

Nach abgeschlossener Erstinitialisierung werden im normalen Startup ausdrücklich nicht automatisch verändert:

```text
is_active
Passwort / Passwort-Hash
must_change_password
```

Damit kann ein Betreiber den Default-Administrator im Normalbetrieb deaktivieren oder sein Passwort ändern, ohne dass der nächste Neustart diese Entscheidung zurücksetzt.

## First Install – initiales Credential des Default-Administrators

Recovery und Erstinbetriebnahme sind getrennte Betriebsfälle.

Der Recovery-Modus wird **nicht** für eine reguläre Neuinstallation verwendet.

Bei einer frischen Installation stellt der Bootstrap fest, dass für den systemverwalteten Default-Administrator noch kein nutzbares LOCAL-Credential existiert.

Nur in diesem Zustand wird einmalig ein initiales Credential erzeugt.

Voraussetzungen:

```text
user_key = DEFAULT_ADMIN
+
LOCAL-Identity vorhanden bzw. im Bootstrap herstellbar
+
noch kein lokales Passwort-Credential vorhanden
```

Dann gilt:

```text
→ kryptographisch zufälliges temporäres Initialpasswort erzeugen
→ Passwort sicher hashen
→ Hash im LOCAL-Credential speichern
→ must_change_password = true
→ Default-Administrator aktivieren
→ initiales Passwort genau einmal auf stdout ausgeben
```

Der erzeugte Klartext wird nicht in der Datenbank, in einer Datei oder in einem separaten Bootstrap-Store persistiert.

Die Prüfung und Erzeugung erfolgen unter dem installationsweiten exklusiven Bootstrap-Zugriff. Nur die Instanz, die das Initial-Credential tatsächlich erzeugt und erfolgreich committed hat, gibt das Initialpasswort aus. Weitere Instanzen übernehmen den vorhandenen Credential-Zustand und erzeugen oder veröffentlichen kein weiteres Initialpasswort.

### Ausgabe des Initial-Credentials

Die einmalige Ausgabe erfolgt bewusst auf dem Standard-Output des Auditarium-Hauptprozesses.

Im Docker-/Compose-Referenzbetrieb wird diese Ausgabe dadurch vom Container-Logging erfasst.

Der dokumentierte Abruf lautet:

```bash
docker compose logs auditarium
```

Die Dokumentation darf deshalb nicht lediglich von einer „Container-Konsole“ sprechen, sondern muss ausdrücklich auf das **Auditarium-Container-Log** und den dazugehörigen Compose-Befehl verweisen.

Die Ausgabe enthält mindestens:

```text
Auditarium – Initial Administrator Credential

Username:         Administrator
Initial password: <generated temporary password>

This is a temporary credential.
You must change it at the first login.

If this credential is no longer available before it was changed,
use the Auditarium Recovery procedure.
```

Das temporäre Passwort wird ausschließlich beim tatsächlichen erstmaligen Erzeugen des Credentials ausgegeben.

Ein späterer Neustart gibt es nicht erneut aus.

Auditarium kann es nicht aus dem gespeicherten Passwort-Hash rekonstruieren.

Ist die ursprüngliche Ausgabe aufgrund von Logrotation, Container-Neuerstellung oder anderer Logbereinigung nicht mehr verfügbar, bevor das Passwort erfolgreich geändert wurde, ist der definierte Recovery-Modus der Wiederherstellungsweg.

### Bewusste Ausnahme von der Secret-Logging-Regel

Das initiale temporäre Default-Admin-Passwort ist die einzige bewusst definierte Ausnahme von der allgemeinen Regel, dass Passwörter niemals in Logs ausgegeben werden.

Diese Ausnahme ist eng begrenzt:

```text
nur First Install
nur bei erstmaliger Credential-Erzeugung
nur das temporäre Default-Admin-Passwort
nur direkte Ausgabe auf stdout
```

Die Ausgabe erfolgt **nicht** über:

```text
ILogger
OpenTelemetry Logging
Traces
Metrics
system_audit_log
ProblemDetails
```

Damit bleibt sie im Referenzbetrieb im lokalen Container-Log verfügbar, wird aber nicht bewusst über die normale Telemetrie-Pipeline an externe Logging-/Observability-Systeme weitergereicht.

Wer Zugriff auf das Container-Log besitzt, kann das Initial-Credential bis zu dessen erstem erfolgreichen Wechsel verwenden. Dies ist eine bewusste Betriebsannahme des First-Install-Verfahrens.

### Erzwungener Passwortwechsel

Das initiale Credential ist ausschließlich ein temporäres Erstzugangs-Credential.

Nach erfolgreicher Authentifizierung mit einem LOCAL-Credential mit:

```text
must_change_password = true
```

darf der Benutzer nicht in die normale Anwendung wechseln.

Serverseitig zulässig sind ausschließlich:

```text
Passwort ändern
Logout
```

Alle anderen fachlichen Web-/API-/BLL-Operationen bleiben gesperrt.

Die Möglichkeit zum Passwortwechsel setzt weiterhin einen aktiven Benutzer voraus. Wird er während dieses Ablaufs deaktiviert, bleibt ausschließlich Logout zulässig.

Die UI leitet unmittelbar auf den Passwortwechsel.

Erst ein erfolgreicher Passwortwechsel:

```text
→ validiert das neue Passwort gegen die aktuelle LOCAL-Password-Policy
→ ersetzt den Passwort-Hash
→ setzt password_changed_at
→ setzt must_change_password = false
→ ermöglicht den normalen Betrieb
```

Der Passwortwechsel ist atomar.

Ohne erfolgreichen Passwortwechsel kann das Onboarding nicht abgeschlossen werden.

Nach dem Passwortwechsel ist das im Container-Log verbliebene Initialpasswort aufgrund des ersetzten Passwort-Hashes nicht mehr verwendbar. Eine nachträgliche Entfernung aus historischen Container-Logs ist für die Funktionsfähigkeit des Verfahrens nicht erforderlich.

### Idempotenz und Restart-Verhalten

Das First-Install-Verfahren ist idempotent.

Existiert bereits ein LOCAL-Credential für den Default-Administrator, erzeugt ein normaler Startup niemals ein weiteres Initialpasswort.

Insbesondere gilt:

```text
Credential existiert
+
must_change_password = true
→ kein neues Passwort erzeugen
→ kein Passwort erneut ausgeben

Credential existiert
+
must_change_password = false
→ normaler Startup
→ Passwort unverändert
```

Damit kann ein verlorenes Initialpasswort nicht durch simples Neustarten neu erzeugt werden.

Dafür ist ausschließlich der Recovery-Modus vorgesehen.

## Recovery-Modus für den Default-Administrator

Auditarium stellt einen expliziten Recovery-Startmodus zur Wiederherstellung des Default-Administrators bereit.

Konzeptionelle Konfiguration:

```text
AUDITARIUM__Recovery__Enabled=true
AUDITARIUM__Recovery__DefaultAdminPassword=<new password>
```

Äquivalente Schlüssel innerhalb der .NET-Configuration-Hierarchie sind zulässig. Das Recovery-Passwort soll bevorzugt über Environment-/Secret-Injection bereitgestellt werden.

Eine persistente Klartextablage des Recovery-Passworts innerhalb der Anwendung oder Datenbank ist nicht zulässig.

Das konfigurierte Recovery-Passwort ist ein temporäres Übergangs-Credential. Nach dem anschließenden Start im Normalbetrieb muss der Default-Administrator es über den bereits vorgesehenen erzwungenen Passwortwechsel durch ein neues Passwort ersetzen.

### Recovery-Ablauf

```text
1. alle Auditarium-Instanzen der Installation herunterfahren
2. Recovery-Konfiguration für genau eine Instanz setzen
3. ausschließlich diese eine Instanz im Recovery-Modus starten
4. Startup erkennt Recovery-Modus
5. Systemdaten reconciliieren
6. Default-Admin-Recovery prüfen und ggf. anwenden
7. ausschließlich Recovery-Statusmodus bereitstellen
8. Recovery-Instanz herunterfahren
9. Recovery-Konfiguration entfernen
10. gewünschte Auditarium-Instanzen normal starten
```

Der Recovery-Modus ist ein separater Betriebszustand und keine normale Laufzeitfunktion.

Recovery setzt einen vollständigen Stillstand der übrigen Installation voraus. Währenddessen dürfen weder weitere Recovery-Instanzen noch Instanzen im Normalbetrieb laufen oder gestartet werden. Diese Reihenfolge ist als verbindlicher Betreiberablauf zu dokumentieren; die Bootstrap-Sperre allein stellt keine Abschaltung bereits laufender Instanzen sicher.

## Idempotente Recovery-Verarbeitung

Bei aktiviertem Recovery-Modus wird geprüft:

```text
Default-Admin existiert
AND
Default-Admin ist aktiv
AND
konfiguriertes Recovery-Passwort wird
gegen den gespeicherten Passwort-Hash erfolgreich verifiziert
AND
SYSTEM_ADMIN-Zuordnung ist korrekt
AND
failed_attempt_count = 0
AND
failed_attempt_window_started_at = NULL
AND
lockout_until = NULL
AND
must_change_password = true
AND
password_changed_at = NULL
```

Ist die Bedingung vollständig erfüllt:

```text
→ Recovery wurde bereits angewendet
→ keine erneute Passwortänderung
→ keine erneute Änderung von is_active
→ keine erneute Änderung der bereits korrekten Credential-Metadaten
```

Andernfalls:

```text
→ bei fehlendem oder abweichendem Credential: Recovery-Passwort sicher hashen und Passwort-Hash ersetzen
→ failed_attempt_count = 0
→ failed_attempt_window_started_at = NULL
→ lockout_until = NULL
→ must_change_password = true
→ password_changed_at = NULL
→ is_active = true
→ SYSTEM_ADMIN-Zuordnung sicherstellen
```

Der vorhandene Hash wird nicht durch erneutes Hashen des Klartextpassworts verglichen. Das konfigurierte Passwort wird mit dem eingesetzten Password-Hasher gegen den vorhandenen Hash verifiziert.

Damit bleibt die Prüfung auch bei gesalzenen Passwort-Hashes korrekt und der Vorgang idempotent.

Ist das konfigurierte Passwort bereits wirksam, aber ein anderer Teil des Recovery-Zielzustands noch nicht hergestellt, wird nur der abweichende Zustand korrigiert. Ein bereits passender Passwort-Hash wird nicht unnötig ersetzt. Alle notwendigen Korrekturen werden gemeinsam in der bestehenden Bootstrap-/Reconcile-Transaktion gespeichert.

Die Rücksetzung der Fehlversuchszähler und von `lockout_until` betrifft ausschließlich das LOCAL-Credential des Default-Administrators. Die normalen Lockout- und Rate-Limit-Regeln gelten nach dem Neustart weiterhin für neue Anmeldeversuche.

### Erzwungener Passwortwechsel nach Recovery

Nach Beendigung des Recovery-Modus und dem anschließenden normalen Start gilt für die Anmeldung mit dem Recovery-Credential derselbe Ablauf wie beim temporären Initial-Credential:

```text
Anmeldung mit temporärem Recovery-Passwort
→ must_change_password = true
→ ausschließlich Passwortwechsel und Logout zulässig
→ neues Passwort gegen LOCAL-Password-Policy prüfen
→ Passwort-Hash ersetzen
→ password_changed_at = Zeitpunkt des erfolgreichen Passwortwechsels
→ must_change_password = false
→ normaler Zugriff gemäß RBAC
```

`password_changed_at = NULL` kennzeichnet bis dahin, dass für dieses temporäre Credential noch kein abschließender Passwortwechsel durch den Administrator stattgefunden hat. Die Recovery bleibt über das bestehende Bootstrap-Audit-Logging nachvollziehbar; Secrets und Passwort-Hashes werden nicht protokolliert.

Der abschließende Passwortwechsel ist atomar und verwendet den vorhandenen LOCAL-Passwortwechselmechanismus. Die Aktivitätsprüfung bleibt wirksam: Wird der Benutzer zwischenzeitlich deaktiviert, bleibt ihm ausschließlich Logout.

## Verhalten während des Recovery-Modus

Solange Recovery aktiviert ist, startet Auditarium **nicht** in den normalen Betriebsmodus.

Nicht bereitgestellt werden insbesondere:

- reguläre Web UI,
- normale API-Endpunkte,
- regulärer Login,
- Background Jobs,
- fachliche Scheduler oder sonstige operative Verarbeitung.

Bereitgestellt werden dürfen ausschließlich:

- eine statische Recovery-Statusseite,
- notwendige technische Health-/Readiness-Endpunkte.

Die Recovery-Seite zeigt keine geheimen Werte und insbesondere niemals das konfigurierte Passwort an.

## Recovery-Konfiguration als bewusste Sperre

Solange

```text
AUDITARIUM__Recovery__Enabled=true
```

gesetzt ist, bleibt Auditarium im Recovery-Modus.

Auch nach erfolgreich angewendeter Recovery wird nicht automatisch in den Normalbetrieb gewechselt.

Der Betreiber muss ausdrücklich:

```text
Recovery-Instanz stoppen; übrige Installation bleibt gestoppt
→ Recovery-Konfiguration entfernen
→ gewünschte Auditarium-Instanzen normal starten
```

Erst dieser anschließende normale Start akzeptiert den neuen Credential-Zustand für den regulären Betrieb.

## Synchronisierung code-definierter Permissions

Die im Code definierten Permission-Keys werden bei jedem Start mit der Datenbank reconciled.

```text
Permission im Code vorhanden, aber nicht in DB
→ anlegen

Permission in Code und DB vorhanden
→ systemverwaltete Metadaten korrigieren

Permission nicht mehr im Code vorhanden
→ nicht automatisch physisch löschen
```

Für nicht mehr im Code vorhandene Permissions wird eine kontrollierte Deaktivierungs- bzw. Migrationsstrategie verwendet.

Die Permission-Synchronisierung erfolgt vor dem vollständigen Reconcile der systemverwalteten Rollen, damit deren Permission-Sets zuverlässig hergestellt werden können.

## Authentifizierungs-Grundmodell

Auditarium trennt Benutzer, Identitäten und Credentials konsequent voneinander.

Grundmodell:

```text
Credentials
   ↓
Authentication Provider
   ↓
user_identity
   ↓
user
   ↓
Roles
   ↓
Permissions
```

Der Authentifizierungs-Provider beantwortet ausschließlich die Frage:

> Wer ist der anfragende Benutzer?

Ab der erfolgreichen Auflösung auf einen internen `user_id` verwendet Auditarium unabhängig vom ursprünglichen Provider das gleiche Rollen- und Permission-Modell.

## Authentication-Provider-Modell

Auditarium trennt drei Ebenen:

```text
Authentication Provider Definition
→ Provider-Typ / Mechanik aus dem Code

Authentication Provider Instance
→ konkrete konfigurierte Instanz

user_identity
→ Zuordnung eines internen Benutzers zu genau dieser Instanz
```

Beispiel:

```text
Provider-Typ LDAP
├── LDAP_FOO
└── LDAP_BAR

Provider-Typ LOCAL
└── LOCAL

Provider-Typ API
└── API
```

### `AuthenticationProviderDefinition`

Jeder unterstützte Provider-Typ wird im Code definiert.

Eine Definition beschreibt mindestens:

```text
ProviderType
AllowMultipleInstances
SystemManaged
ConfigurationDefinitions / SettingDefinitionTemplates
RoutingCapabilities
SupportedProvisioningModes
```

Die genaue technische Typstruktur wird implementierungsnah festgelegt.

Initial verbindlich unterstützt werden:

```text
LOCAL
API
LDAP
```

Dabei gilt:

```text
LOCAL
→ genau eine Instanz
→ systemverwaltet

API
→ genau eine Instanz
→ systemverwaltet

LDAP
→ 0..n Instanzen
→ administrierbar
```

Spätere Provider wie OIDC oder SAML sind Erweiterungen nach demselben Schema und kein eigener Architekturpfad.

### Erweiterungsregel für spätere OIDC-/SAML-Provider

OIDC und SAML sind **nicht Bestandteil des initialen Funktionsumfangs**.

Eine spätere Integration darf ausschließlich als weitere `AuthenticationProviderDefinition` innerhalb des bestehenden Authentication-Provider-Modells erfolgen.

Dabei gilt:

```text
Provider-Typ
→ neue AuthenticationProviderDefinition

Provider-Instanz
→ eine oder mehrere konkrete OIDC-/SAML-Instanzen

Authentication
→ externe Identitätsbestätigung durch den jeweiligen IdP

Identity
→ konkrete Provider-Instanz + stabile external_id

Provisioning
→ bestehende Modi wiederverwenden

Authorization
→ bestehendes user-/roles-/permissions-Modell
```

Für spätere OIDC-/SAML-Implementierungen gilt ausdrücklich:

- mehrere konkrete Provider-Instanzen sind zulässig,
- jede Identity benötigt eine stabile externe ID,
- `EXISTING_ONLY`, `CREATE_INACTIVE` und `CREATE_ACTIVE` werden als bestehende Provisioning-Modi wiederverwendet,
- externe Claims bzw. Assertions dürfen zunächst für Identity- und Profilinformationen wie stabile ID, Anzeigename, E-Mail oder bevorzugten Login-Namen verwendet werden,
- Group-to-Role- oder Claim-to-Role-Mapping ist keine Voraussetzung und wird nicht vorsorglich in das Kernmodell eingebaut,
- es entsteht kein paralleler Benutzer-, Rollen- oder Berechtigungspfad,
- `LOCAL` einschließlich des systemverwalteten Default-Administrators bleibt als unabhängiger Break-Glass-/Recovery-Zugang erhalten.

Der lokale Recovery-/Break-Glass-Pfad darf nicht von OIDC, SAML, DNS, externen Zertifikaten oder der Verfügbarkeit eines externen Identity Providers abhängen.

### Tabelle `authentication_providers`

Jede konkrete Provider-Instanz besitzt einen stabilen technischen Schlüssel und eine persistierte Identität.

Konzeptionell:

```text
authentication_provider_id
provider_key
provider_type
display_name
is_enabled
concurrency_version
```

`provider_key` ist technisch stabil und verbindet Konfiguration aus DB, Config und Environment zu derselben logischen Provider-Instanz.

Beispiele:

```text
LOCAL
API
LDAP_FOO
LDAP_BAR
```

`provider_type` muss zu einer im Code vorhandenen `AuthenticationProviderDefinition` passen.

Für systemverwaltete Provider-Instanzen stellt der Startup-Reconcile Existenz und korrekten Typ sicher.

### Provider-Mechanik

Die technische Provider-Implementierung repräsentiert den Mechanismus und nicht eine einzelne konfigurierte Instanz.

Konzeptionell:

```csharp
public interface IAuthenticationProvider
{
    string ProviderType { get; }

    Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationProviderContext providerContext,
        AuthenticationRequest request,
        CancellationToken cancellationToken = default);
}
```

Die konkrete Signatur wird implementierungsnah festgelegt.

`AuthenticationProviderContext` liefert die effektive Konfiguration der ausgewählten Provider-Instanz.

Damit kann eine einzige LDAP-Provider-Implementierung mehrere Instanzen wie `LDAP_FOO` und `LDAP_BAR` bedienen.

## Provider-Konfiguration und Quellen

Provider-spezifische Eigenschaften verwenden das zentrale Settings-/Override-Modell.

Kanonischer Namespace:

```text
Authentication:Providers:<provider_key>:<property>
```

Beispiele:

```text
Authentication:Providers:LDAP_FOO:Host
Authentication:Providers:LDAP_FOO:Port
Authentication:Providers:LDAP_FOO:TlsMode
Authentication:Providers:LDAP_FOO:BindUser
Authentication:Providers:LDAP_FOO:BindPassword
```

Für UI-editierbare Eigenschaften gilt weiterhin:

```text
Code Default
→ Config
→ DB
→ Environment
→ Effective Value
```

Für nicht UI-editierbare Eigenschaften:

```text
Code Default
→ Config
→ Environment
→ Effective Value
```

`provider_key` verbindet alle Quellen zu derselben Provider-Instanz.

Die UI zeigt je Property mindestens:

```text
konfigurierten DB-Wert
effektiven Wert
Quelle
editierbar / extern überschrieben
```

Eine extern definierte Eigenschaft wird nicht durch eine scheinbare UI-Änderung verschleiert.

Wird ein DB-Override entfernt, wird der darunterliegende Config-/Default-Wert wieder wirksam.

Eine ausschließlich extern definierte Provider-Instanz wird in der UI sichtbar, ihre externe Existenz kann dort aber nicht gelöscht werden. Ein vorhandener DB-Override kann zurückgesetzt werden.

Provider-Konfiguration ist damit kein LDAP-Sonderfall, sondern ein Verbraucher der allgemeinen Auditarium-Konfigurationsmechanik.

## Lifecycle einer Provider-Instanz

Eine normale Provider-Instanz kann aktiviert und deaktiviert werden.

Deaktivieren bedeutet:

```text
keine neue Authentifizierung über diese Instanz
kein automatisches Login-Routing zu dieser Instanz
bestehende user_identities bleiben erhalten
Konfiguration bleibt erhalten
historische Nachvollziehbarkeit bleibt erhalten
```

### Löschen

Die Löschbarkeit richtet sich pragmatisch nach Abhängigkeiten.

```text
Abhängigkeiten vorhanden
→ aktivieren / deaktivieren
→ nicht löschen

keine Abhängigkeiten
→ aktivieren / deaktivieren
→ löschen möglich
```

Als Abhängigkeit gilt mindestens:

```text
eine user_identity referenziert die Provider-Instanz
ODER
die Provider-Instanz wurde nachvollziehbar produktiv verwendet und wird für historische Zuordnung benötigt
```

Eine lediglich angelegte oder getestete Provider-Konfiguration ohne solche Abhängigkeiten darf physisch gelöscht werden.

Systemverwaltete Provider sind unabhängig von Abhängigkeiten nicht löschbar:

```text
LOCAL
API
```

Ein Löschversuch bei vorhandenen Abhängigkeiten endet als erwarteter Konflikt, beispielsweise:

```text
AUTH_PROVIDER.HAS_DEPENDENCIES
Type = Conflict
```

## `LocalAuthenticationProvider`

Lokale Authentifizierung ist ein vollwertiger eigener Provider-Typ.

Es existiert genau eine systemverwaltete Provider-Instanz:

```text
provider_key  = LOCAL
provider_type = LOCAL
```

Lokale Credentials werden in einem eigenen provider-spezifischen Store gehalten.

Konzeptionell:

```text
local_credentials
├── identity_id
├── password_hash
├── password_changed_at
├── must_change_password
├── failed_attempt_count
├── failed_attempt_window_started_at
├── lockout_until
└── weitere lokale Credential-Metadaten bei Bedarf
```

`must_change_password` ist ein serverseitig durchgesetzter Credential-Zustand.

Für das initial erzeugte oder durch Recovery hergestellte temporäre Default-Admin-Credential gilt:

```text
password_changed_at = NULL
must_change_password = true
```

Nach erfolgreichem erzwungenem Passwortwechsel gilt:

```text
password_changed_at = <Zeitpunkt>
must_change_password = false
```

Das Passwort wird niemals dauerhaft im Klartext gespeichert.

Der Default-Administrator verwendet eine Identity der Provider-Instanz `LOCAL`.

Der Recovery-Modus stellt gezielt das LOCAL-Credential des Default-Administrators einschließlich aufgehobener LOCAL-Sperre und erzwungenem Passwortwechsel her. Zusätzlich reaktiviert er den zugehörigen Benutzer über `users.is_active = true` und stellt dessen `SYSTEM_ADMIN`-Zuordnung sicher. Credentials anderer Identities oder Benutzer werden dadurch nicht zurückgesetzt.

## `ApiAuthenticationProvider`

API-Authentifizierung ist ein eigener Provider-Typ.

Es existiert genau eine systemverwaltete Provider-Instanz:

```text
provider_key  = API
provider_type = API
```

Technische API-Benutzer sind normale interne Benutzerobjekte:

```text
user
→ user_identity
   └── authentication_provider_id → API
→ API credential
```

API-Benutzer verwenden dasselbe Rollen- und Permission-Modell wie interaktive Benutzer.

Es gibt kein separates API-Berechtigungsmodell.

## `LdapAuthenticationProvider`

LDAP ist der initiale externe Authentication-Provider-Typ.

LDAP erlaubt mehrere konkrete Instanzen:

```text
LDAP_FOO
LDAP_BAR
LDAP_CTK
...
```

Jede Instanz besitzt eine eigene effektive Konfiguration, eigenes Login-Routing und einen eigenen Identity-Namensraum.

Dadurch können identische `external_id`-Werte in unterschiedlichen LDAP-Instanzen unabhängig voneinander existieren.

Auditarium speichert keine LDAP-Benutzerpasswörter.

### LDAP-Konfigurationsgruppen

Eine LDAP-Provider-Instanz besitzt mindestens folgende fachliche Konfigurationsgruppen.

#### Allgemein

```text
Provider Key
Display Name
Enabled
Provisioning Mode
```

#### Verbindung

```text
Host
Port
TLS Mode
Connection Timeout
```

TLS wird nicht als bloßes Boolean modelliert.

Vorgesehene semantische Werte:

```text
None
StartTls
Ldaps
```

Weitere Zertifikats-/Trust-Einstellungen dürfen ergänzt werden, wenn dies für eine sichere Implementierung notwendig ist.

#### Bind und Suche

```text
Bind User
Bind Password
Base DN
```

`Bind Password` ist ein Secret-Setting und verwendet den zentralen `ISecretProtector`, sofern der effektive Wert aus der DB stammt.

#### Benutzererkennung und Mapping

Mindestens konfigurierbar:

```text
Login / Search Attribute
Search Filter
Stable External ID Attribute
Display Name Attribute
Email Attribute
```

Die dauerhaft verwendete `external_id` soll auf einem stabilen Verzeichnisattribut beruhen und nicht ausschließlich auf einem veränderbaren Login-Namen.

#### Login Routing

Mindestens konfigurierbar:

```text
Domain-/NetBIOS-Präfixe
UPN-Suffixe
Verhalten bei explizit ausgewähltem Provider und unqualifiziertem Login
```

Das automatische Routing unqualifizierter Logins bleibt separat durch den `AuthenticationRouter` definiert.

### Verbindung testen

Die Administration bietet für LDAP-Instanzen eine diagnostische Funktion:

```text
Verbindung testen
```

Der Test prüft soweit konfiguriert mindestens:

```text
Netzwerk / Port erreichbar
TLS-Aufbau
Bind erfolgreich
Base DN erreichbar
Testsuche ausführbar
konfigurierte Mapping-Attribute auflösbar
```

Der Test ist strikt ohne fachliche Nebenwirkungen.

Nicht erlaubt:

```text
User anlegen
user_identity anlegen
Rollen vergeben
Provider-Konfiguration verändern
Provisioning auslösen
```

## LDAP-Provisioning

Jede LDAP-Provider-Instanz besitzt einen konfigurierbaren Provisioning-Modus.

Initial zulässig:

```text
EXISTING_ONLY
CREATE_INACTIVE
CREATE_ACTIVE
```

### `EXISTING_ONLY`

Ein interner Benutzer und die passende `user_identity` müssen bereits existieren.

Ein erfolgreicher LDAP-Credential-Test allein erzeugt keine neuen Auditarium-Objekte und verknüpft keine bestehenden Benutzer automatisch.

### `CREATE_INACTIVE`

Beim ersten erfolgreichen LDAP-Login ohne bestehende Identity werden:

```text
user
+
user_identity
```

angelegt.

Der Benutzer wird mit:

```text
is_active = false
```

erzeugt.

Der Login endet mit einem verständlichen Hinweis, dass das Konto angelegt wurde und auf administrative Freigabe wartet.

Aktivierung und Rollenzuweisung erfolgen anschließend bewusst durch einen berechtigten Administrator.

### `CREATE_ACTIVE`

Beim ersten erfolgreichen LDAP-Login ohne bestehende Identity werden:

```text
user
+
user_identity
```

angelegt und der Benutzer wird direkt aktiviert.

Die Provider-Instanz muss hierfür mindestens eine automatisch zuzuweisende Standardrolle konfigurieren.

Konzeptionell:

```text
ProvisioningMode = CREATE_ACTIVE

AutoProvisionRoles
├── VIEWER
└── <weitere konfigurierte Rolle>
```

Die Rollenzuweisung erfolgt über das normale `user_roles`-/RBAC-Modell.

### Regeln zur Identity-Zuordnung

Für alle Provisioning-Modi gilt:

```text
(authentication_provider_id, stable external_id) vorhanden?
→ bestehende Identity verwenden

nicht vorhanden?
→ ProvisioningMode anwenden
```

Auditarium führt niemals automatisch Accounts allein aufgrund gleicher:

```text
E-Mail-Adresse
Username
Display Name
```

zusammen.

Kollidiert ein für die Neuanlage vorgesehener interner Username mit einem bereits existierenden anderen Benutzer, wird nicht geraten oder automatisch verknüpft.

Stattdessen entsteht ein kontrollierter Provisioning-Konflikt, der administrativ gelöst werden muss.

Die stabile externe ID bleibt die maßgebliche externe Identität.
## Gemeinsame Credential-Prinzipien

LOCAL- und API-Authentifizierung verwenden beide lokal verwaltete Credentials, bleiben aber eigenständige Provider.

Gemeinsam nutzbare technische Bausteine dürfen geteilt werden, ohne beide Provider fachlich zu einem Provider zu verschmelzen.

Grundsatz:

> Gemeinsame Implementierungsbausteine sind erlaubt. Authentifizierungsarten bleiben semantisch getrennt.

Klartext-Credentials werden niemals dauerhaft gespeichert.

Secrets, Passwörter und Credential-Hashes dürfen grundsätzlich nicht in Logs, Traces oder Metrics geschrieben werden.

Einzige definierte Ausnahme ist die einmalige direkte stdout-Ausgabe des automatisch erzeugten temporären Default-Admin-Passworts beim First Install. Diese Ausgabe umgeht bewusst `ILogger` und die OpenTelemetry-Logging-Pipeline und darf weder in Traces, Metrics noch im `system_audit_log` dupliziert werden.

## Authentifizierung und Autorisierung

Authentifizierung und Autorisierung bleiben strikt getrennt.

```text
Authentication Provider
→ Identität
→ interner user_id

IPermissionEvaluator
→ Rollen
→ effektive Permissions

AuthorizationBehavior
→ Zugriff erlauben oder verweigern
```

Damit ist es für die Autorisierung irrelevant, ob ein Benutzer über LOCAL, LDAP, API oder später OIDC/SAML authentifiziert wurde.

Grundsatz:

> Der Authentifizierungs-Provider kennt Credentials und Identitäten. Die BLL kennt anschließend nur noch den internen Benutzer.

## Mehrere Identitäten pro Benutzer

Ein interner Benutzer darf mehrere Authentifizierungs-Identitäten besitzen.

Beispiel:

```text
User 42
├── LOCAL / matthias
├── LDAP_CTK / mkargel
└── OIDC_ENTRA / <subject>
```

Alle Identitäten führen auf:

```text
user_id = 42
```

und damit auf dieselben:

```text
Roles
Permissions
Audit-Historie
```

Die Verknüpfung oder Trennung solcher Identitäten ist eine administrative Funktion und darf nicht automatisch allein aufgrund ähnlicher Namen oder E-Mail-Adressen erfolgen.

## Current Actor

Die BLL verwendet einen schlanken, HTTP-unabhängigen Ausführungskontext:

```csharp
public enum ActorType
{
    Anonymous,
    User,
    System
}

public interface ICurrentActor
{
    ActorType Type { get; }
    long? UserId { get; }
    bool IsAuthenticated { get; }
}
```

Semantik:

| Actor-Typ | `UserId` | `IsAuthenticated` | Bedeutung |
|---|---:|---|---|
| `Anonymous` | `NULL` | `false` | unbekannte externe Person im User-Kontext |
| `User` | `> 0` | `true` | erfolgreich authentifizierter Auditarium-Benutzer |
| `System` | `0` | `false` | interner technischer Ausführungskontext |

`Anonymous` und `System` sind ausdrücklich nicht gleichbedeutend.

`Anonymous` repräsentiert eine unbekannte externe Person, während `System` keinen Benutzer-Login repräsentiert, sondern einen internen technischen Actor mit persistierbarer `user_id = 0`.

Die BLL kennt über `ICurrentActor` weder `HttpContext` noch `ClaimsPrincipal`.

## Auflösung des Current Actor

Bei einem normalen HTTP-Request erfolgt die Auflösung konzeptionell:

```text
HTTP Request
→ ASP.NET Authentication
→ Authentication Provider
→ interne user_id
→ ClaimsPrincipal
→ HTTP-Adapter für ICurrentActor
→ BLL / Mediator
```

Der konkrete HTTP-Adapter lebt außerhalb der BLL.

Die BLL erhält ausschließlich den abstrahierten Actor-Kontext.

Ein anonymer Request wird als:

```text
ActorType = Anonymous
UserId = NULL
IsAuthenticated = false
```

abgebildet.

Ein erfolgreich authentifizierter Benutzer wird als:

```text
ActorType = User
UserId > 0
IsAuthenticated = true
```

abgebildet.

## Systemactor und RBAC

Der Systemactor ist **kein Superuser**.

Nach dem eigentlichen Application-Start unterliegt auch:

```text
ActorType.System
user_id = 0
```

vollständig dem normalen RBAC-Modell.

Grundsatz:

> Der Actor-Typ beschreibt die Herkunft des Ausführungskontexts. Die erlaubten Fähigkeiten bestimmt ausschließlich RBAC.

Der Systembenutzer erhält eine systemverwaltete Rolle mit einem bewusst begrenzten Permission-Set.

Konzeptionell beispielsweise:

```text
SYSTEM_INTERNAL
```

Diese Rolle besitzt nur die Permissions, die interne Systemprozesse tatsächlich benötigen.

Es gilt ausdrücklich nicht:

```text
System → darf alles
```

sondern:

```text
System
→ user_id = 0
→ user_roles
→ systemverwaltete Rolle(n)
→ role_permissions
→ effektive Permissions
```

Damit gilt auch für interne Prozesse das Least-Privilege-Prinzip.

Der reservierte Systembenutzer besitzt bewusst `is_active = false`. Die Aktivitätssperre regulärer und technischer/API-Benutzer wird auf diesen kontrollierten internen Ausführungskontext nicht angewendet. Diese Abgrenzung gilt ausschließlich für `ActorType.System` mit `user_id = 0`; sie hebt keine Permission-Prüfung auf und eröffnet keinen externen Anmeldeweg.

## Schutz vor externem Systemactor

Externe Authentication Provider dürfen niemals einen Systemactor erzeugen.

Dies gilt für:

```text
LOCAL
LDAP
API
OIDC
SAML
```

und alle zukünftigen externen Provider.

Kein externer Authentifizierungsweg darf:

```text
user_id = 0
ActorType.System
user_key = SYSTEM
```

als Ergebnis liefern.

Der Systemactor darf ausschließlich innerhalb kontrollierter interner Einstiegspunkte erzeugt werden.

Beispiele:

```text
interne Background-Prozesse
Maintenance
Retention
interne Scheduler
```

Auch diese Prozesse unterliegen nach Application-Start dem normalen `AuthorizationBehavior`.

## Web-Login und Session-Transport

Interaktive Web-Logins verwenden ASP.NET Cookie Authentication.

Grundablauf:

```text
Browser
→ Login
→ AuthenticationRouter
→ genau ein Authentication Provider
→ erfolgreiche user_identity
→ interner user_id
→ Authentication Cookie
→ weitere Requests
→ ClaimsPrincipal
→ ICurrentActor
```

Das Cookie enthält nur die für die Wiederherstellung des internen Benutzerkontexts notwendigen Daten.

Rollen und effektive Permissions werden nicht als dauerhaft vertrauenswürdige Autorisierungswahrheit im Cookie gespeichert.

Dadurch gilt:

```text
Benutzerdeaktivierung oder Rollen-/Permission-Änderung
→ nächster autorisierter Use Case
→ IPermissionEvaluator liest aktuellen Zustand
→ Änderung wirkt ohne erneuten Login
```

Ein bereits angemeldeter, inzwischen deaktivierter Benutzer kann damit keine fachlichen oder administrativen Use Cases mehr ausführen. Ein noch geöffnetes Formular berechtigt nicht zum Speichern; auch lesende geschützte Zugriffe werden abgewiesen. Die bestehende Sitzung darf weiterhin über Logout beendet werden.

Eine eigene serverseitige Session-Tabelle wird zunächst nicht eingeführt.

Sie wird erst dann ergänzt, wenn konkrete Anforderungen wie zentrale Session-Invalidierung, aktive Session-Übersicht oder Geräteverwaltung dies erforderlich machen.

### Gültigkeit bestehender Browser-Sessions

Bestehende Authentication-Cookies bleiben innerhalb ihrer regulären Gültigkeitsdauer über einen normalen Neustart der Anwendung hinweg gültig. Der gemeinsame Data-Protection-Keyring stellt dies auch im Mehrinstanzbetrieb sicher.

Ein Passwortwechsel oder die Default-Administrator-Recovery widerruft bestehende Browser-Sessions nicht pauschal. Auditarium führt dafür weder eine serverseitige Session-Tabelle noch einen Security-Stamp oder eine vergleichbare zentrale Cookie-Invalidierung ein.

Die aktuelle Aktivitäts- und Berechtigungsprüfung bleibt die maßgebliche Zugriffskontrolle:

```text
Benutzer deaktiviert
→ nächster autorisierter Use Case
→ keine effektiven Permissions
→ Zugriff abgewiesen
```

Damit kann ein Betreiber einem Benutzer unabhängig von bestehenden Cookies, Passwortwechseln oder angesprochener Anwendungsinstanz den Zugriff entziehen. Der bewusste Verzicht auf eine zentrale Sitzungsinvalidierung bedeutet zugleich, dass ein Passwortwechsel bereits bestehende Browser-Sessions nicht beendet.

Bei künftigem Bedarf an gezielter oder zentraler Sitzungsinvalidierung, einer Sitzungsübersicht oder Geräteverwaltung wird diese Fähigkeit als eigenständige Erweiterung einschließlich Datenmodell, Sicherheitsregeln und Tests ergänzt.

### Logout bei deaktiviertem Benutzer

Logout bleibt auch dann erreichbar und ausführbar, wenn der Benutzer nach seiner Anmeldung deaktiviert wurde und keine effektiven Permissions mehr besitzt.

Der Logout-Weg:

- benötigt keine Rollen- oder Permission-Zuweisung und keinen aktiven Benutzerstatus,
- beendet ausschließlich die eigene Browseranmeldung durch Entfernen des Authentication-Cookies,
- bleibt als schreibender Browser-Request durch den bestehenden Antiforgery-Schutz abgesichert,
- wird nach den bestehenden Audit-Log-Regeln protokolliert.

Ein Fehler beim Schreiben des Logout-Ereignisses darf das Beenden der eigenen Browseranmeldung nicht verhindern. Der Protokollierungsfehler wird gemäß Kapitel 7 zusätzlich technisch gemeldet; die Abmeldung wird trotzdem durchgeführt.

Wird Logout als Mediator-Request umgesetzt, erhält er die ausdrückliche Security-Deklaration `AllowAnonymous` für diese begrenzte Sitzungsaktion. Die Default-Deny-Regel für andere Requests bleibt verbindlich.

## LOCAL Login

Ein unqualifizierter klassischer Username/Password-Login wird dem LOCAL-Provider zugeordnet.

Grundablauf:

```text
username + password
→ AuthenticationRouter
→ LocalAuthenticationProvider
→ LOCAL user_identity bestimmen
→ lokales Credential prüfen
→ users.is_active prüfen
→ must_change_password prüfen
→ internen user_id ermitteln
→ Authentication Cookie ausstellen
→ bei Bedarf ausschließlich Passwortwechsel zulassen
```

Das Passwort wird nur im Rahmen des Loginversuchs verarbeitet und niemals dauerhaft im Klartext persistiert.

Ist `must_change_password = true`, ist die Authentifizierung zwar erfolgreich, die resultierende Sitzung bleibt jedoch bis zum erfolgreichen Passwortwechsel serverseitig auf Passwortwechsel und Logout beschränkt.

## LDAP Login

LDAP-Login wird über den `AuthenticationRouter` eindeutig einer konkreten aktiven LDAP-Provider-Instanz zugeordnet.

Grundablauf:

```text
Loginname + Passwort
→ AuthenticationRouter
→ genau eine LDAP-Provider-Instanz
→ effektive Provider-Konfiguration auflösen
→ LDAP Credential-Prüfung
→ stabile externe Identität ermitteln
→ vorhandene user_identity suchen
→ ggf. ProvisioningMode anwenden
→ users.is_active prüfen
→ internen user_id ermitteln
→ Authentication Cookie ausstellen
```

Auditarium speichert keine LDAP-Benutzerpasswörter.

Bei `CREATE_INACTIVE` wird nach erfolgreicher Neuanlage kein Authentication Cookie ausgestellt, solange der erzeugte Benutzer nicht aktiviert wurde.

Die dauerhafte `external_id` einer LDAP-Identity basiert auf dem für diese Provider-Instanz konfigurierten stabilen Verzeichnisattribut.
## API Authentication Transport

API-Clients verwenden keine Browser-Cookies.

API-Authentifizierung erfolgt über ein dediziertes API-Credential im HTTP-Request.

Verbindliches Format:

```http
Authorization: Bearer aud_v1_<keyid>_<base64url-secret>
```

Bedeutung:

```text
aud
→ fester Auditarium-Präfix

v1
→ Formatversion des Credentials

keyid
→ öffentliche, nicht geheime Lookup-ID

base64url-secret
→ Base64url-Darstellung eines kryptographisch zufällig erzeugten Secrets
```

Das vollständige Credential wird nicht zusätzlich Base64-codiert oder anderweitig obfuskiert.

Der Ablauf ist:

```text
Authorization Header
→ ApiAuthenticationProvider
→ key_id extrahieren
→ genau einen Credential-Datensatz laden
→ secret gegen gespeicherten Hash prüfen
→ user_identity bestimmen
→ users.is_active prüfen
→ internen user_id ermitteln
→ ICurrentActor = User
```

## API Credential Aufbau

Ein API-Credential besteht logisch aus:

```text
prefix
format_version
key_id
secret
```

und wird nach außen in folgender Form dargestellt:

```text
aud_v1_<keyid>_<base64url-secret>
```

Dabei gilt:

```text
prefix = aud
format_version = v1
key_id = öffentliche Lookup-ID
secret = mindestens 256 Bit kryptographisch sicherer Zufall
```

`key_id` ist nicht geheim und dient ausschließlich zur effizienten Auswahl des Credential-Datensatzes.

`secret` ist geheim.

Die rohen Secret-Bytes werden ausschließlich für den sicheren und robusten Transport Base64url-codiert. Base64url ist dabei nur Encoding und ausdrücklich keine zusätzliche Sicherheits- oder Obfuskationsschicht.

Der provider-spezifische Store verwendet die Tabelle `api_credentials`.

Persistiert werden mindestens:

```text
api_credential_id
identity_id
key_id
secret_hash
name
created_at
expires_at
revoked_at
last_used_at
```

Nicht persistiert werden:

```text
Klartext-Secret
vollständiges API-Credential
```

Der Secret-Hash ist niemals Bestandteil des ausgegebenen API-Keys.

Das Klartext-Secret wird:

```text
mit kryptographisch sicherem Zufallszahlengenerator erzeugt
→ mindestens 256 Bit Zufall
→ Base64url-codiert
→ genau einmal als Teil des Credentials angezeigt
→ sicher gehasht
→ ausschließlich als Hash gespeichert
```

Eine vollständige lineare Prüfung aller API-Credential-Hashes ist nicht erforderlich, da `key_id` den passenden Datensatz eindeutig bestimmt.

Die Prüfung eines API-Secrets erfolgt ausschließlich gegen den gespeicherten `secret_hash`. Klartext-Secrets werden nach der einmaligen Ausgabe nicht persistiert und stehen für spätere Vergleiche nicht mehr zur Verfügung.


### Format und Versionierung

Das API-Credential-Format ist verbindlich versioniert:

```text
aud_v1_<keyid>_<base64url-secret>
```

Ziele der einzelnen Bestandteile:

```text
aud
→ Auditarium-Credential eindeutig erkennbar
→ erleichtert Secret-Scanning, Redaction und Betrieb

v1
→ explizite Formatversion
→ ermöglicht spätere Formatänderungen ohne Mehrdeutigkeit

keyid
→ direkte Auswahl des Credential-Datensatzes
→ nicht geheim

base64url-secret
→ transportfreundliche Darstellung des geheimen Zufallswerts
```

Ein zukünftiges Format kann beispielsweise als:

```text
aud_v2_...
```

eingeführt werden, ohne vorhandene `v1`-Credentials syntaktisch mehrdeutig zu machen.

Das vollständige Credential wird bewusst nicht noch einmal Base64-codiert. Die erkennbare Struktur ist gewollt und unterstützt Betrieb, Diagnose, Secret-Erkennung und gezielte Redaction.

## Mehrere API-Credentials pro Identity

Eine API-Identity darf mehrere gleichzeitig gültige Credentials besitzen.

Beispiel:

```text
API Identity
├── Checkmk Production
├── PowerShell Automation
└── Rotation Key 2026-09
```

Dadurch ist eine unterbrechungsfreie Rotation möglich:

```text
neues Credential erzeugen
→ Consumer umstellen
→ altes Credential widerrufen
```

Widerruf wirkt über:

```text
revoked_at
```

sofort.

`last_used_at` wird zur betrieblichen Nachvollziehbarkeit erfasst.

## Aktivierungszustand des Benutzers

Ein gültiges Credential allein reicht nicht für eine erfolgreiche Authentifizierung.

Für alle Provider gilt:

```text
Credential gültig
AND
user_identity gültig
AND
users.is_active = true
→ Authentifizierung erfolgreich
```

Ist der Benutzer inaktiv, wird die eigentliche Anmeldung unabhängig von LOCAL, LDAP, API oder einem späteren Provider abgelehnt.

Die Aktivitätsprüfung gilt zusätzlich bei der Anwendung von Berechtigungen auf bereits angemeldete Benutzer. Ein deaktivierter Benutzer erhält keine effektiven Permissions; als einzige Aktion seiner bestehenden Anmeldung bleibt Logout zulässig. Dies gilt auch bei noch ausstehendem erzwungenem Passwortwechsel: `must_change_password` berechtigt einen deaktivierten Benutzer nicht zum Passwortwechsel.

Die Regel wird über die zentrale Berechtigungsprüfung für Web, API und weitere Benutzer-Einstiegspunkte einheitlich angewendet. Der interne Systemactor ist gemäß seiner ausdrücklich getrennten Semantik kein deaktiviertes Benutzerkonto im Sinne dieser Zugriffssperre.

Bei LDAP `CREATE_INACTIVE` darf ein erfolgreicher externer Credential-Test dennoch kontrolliert zur erstmaligen Anlage von `user` und `user_identity` führen; eine Session wird dabei erst nach späterer Aktivierung des internen Benutzers ausgegeben.

## Zentrale Security Policies

Authentifizierungs- und Session-Policies werden zentral konfigurierbar umgesetzt.

Vorgesehene Policy-Bereiche:

```text
LocalPassword
LocalLockout
ApiCredentials
Session
LoginRateLimiting
```

Provider enthalten keine verstreut hart codierten Schwellenwerte.

Grundsatz:

> Authentifizierungs-Provider liefern die Mechanik. Security Policies liefern die konfigurierbaren Regeln.

## LOCAL Password Policy

Für LOCAL gelten sichere, konfigurierbare Defaults.

Initiale Zielwerte:

| Policy | Default |
|---|---:|
| Mindestlänge | 15 Zeichen |
| Maximallänge | 128 Zeichen |
| periodischer Passwortablauf | deaktiviert |
| klassische Komplexitätsregeln | deaktiviert |
| Common-/Compromised-Password-Blocklist | aktiviert |

Passphrases werden ausdrücklich unterstützt.

Passwörter werden nicht allein aufgrund ihres Alters periodisch erzwungen geändert.

Die konkreten Default-Werte können im Rahmen der Implementierung an aktuelle Sicherheitsstandards angepasst werden, ohne das Grundmodell zu ändern.

## LOCAL Lockout

LOCAL verwendet einen eigenen konfigurierbaren Lockout-Mechanismus.

Initiale Zielwerte:

| Policy | Default |
|---|---:|
| erlaubte Fehlversuche | 5 |
| Beobachtungsfenster | 15 Minuten |
| Lockout-Dauer | 15 Minuten |
| Reset nach erfolgreichem Login | ja |

Konzeptionelle Credential-Metadaten:

```text
failed_attempt_count
failed_attempt_window_started_at
lockout_until
```

Ein LOCAL-Lockout betrifft nur die lokale Identity und sperrt nicht automatisch andere Identities desselben internen Benutzers.

## LDAP Policy

Passwort- und Account-Lockout-Regeln eines LDAP-Benutzers werden grundsätzlich durch das externe Verzeichnis bestimmt.

Auditarium implementiert keine zweite LDAP-Account-Sperrlogik.

Zusätzlich darf Auditarium den eigenen Login-Endpunkt rate-limitieren oder drosseln, um Brute-Force-Versuche und unnötige Last auf LDAP zu begrenzen.

## API Credential Policy

API-Credentials verwenden keine klassischen Passwort-Komplexitätsregeln.

Relevante Policies sind insbesondere:

```text
Secret-Entropie
Default-Lifetime
Maximum-Lifetime
maximale aktive Credentials pro Identity
Revocation
Rotation
Rate Limiting bei ungültigen Secrets
```

Initiale Zielwerte:

| Policy | Default |
|---|---:|
| Secret-Entropie | mindestens 256 Bit kryptographisch sicherer Zufall |
| Default-Lifetime | 180 Tage |
| maximale aktive Credentials pro API-Identity | 5 |

Ungültige API-Secrets führen nicht zu einem klassischen Account-Lockout, da dieser gezielt für Denial-of-Service gegen bekannte `key_id`-Werte missbraucht werden könnte.

Stattdessen werden Rate Limiting, Verzögerung und technische Security-Telemetrie verwendet.

## Session Policy

Web-Sessions verwenden ASP.NET Cookie Authentication.

Konfigurierbare Session-Parameter umfassen mindestens:

```text
Session-Lifetime
Sliding Expiration
absolute maximale Session-Lifetime
Cookie-Sicherheitsparameter
```

Die konkreten Default-Werte werden implementierungsnah festgelegt.

Die Session-Policy darf die fachliche Autorisierung nicht ersetzen.

## Default-Admin und Security Policies

Der Default-Administrator verwendet im Normalbetrieb eine normale LOCAL-Identity.

Damit gelten für ihn dieselben:

```text
Password Policies
Lockout Policies
Rate Limits
```

wie für andere LOCAL-Benutzer.

Der bereits definierte Recovery-Modus bleibt der separate Break-Glass-Pfad und umgeht den normalen Login- und Lockout-Mechanismus nur innerhalb der kontrollierten DAL-Bootstrap-Phase.

## AuthenticationRouter

Username/Password-Credentials werden niemals nacheinander an mehrere Authentication Provider weitergereicht.

Stattdessen bestimmt ein `AuthenticationRouter` vor der Credential-Prüfung genau eine zuständige aktive Provider-Instanz.

Der Router prüft keine Passwörter.

Er beantwortet ausschließlich:

> Welche konkrete Authentication-Provider-Instanz ist für diesen Loginversuch zuständig?

Grundsatz:

> Ein Loginversuch wird genau einer Provider-Instanz zugeordnet. Es gibt keinen Provider-Fallback für Credentials.

## Routing über Domain-Präfix

Ein Loginname im Format:

```text
DOMAIN1\username
```

wird anhand der effektiven Routing-Konfiguration einer konkreten LDAP-Provider-Instanz zugeordnet.

Beispiel:

```text
DOMAIN1\mkargel
→ LDAP_DOMAIN1
```

Mehrere Präfixe können auf dieselbe Provider-Instanz zeigen.

## Routing über UPN-Suffix

Ein Loginname im Format:

```text
username@domain1.example
```

wird anhand der effektiven Routing-Konfiguration einer konkreten LDAP-Provider-Instanz zugeordnet.

Beispiel:

```text
mkargel@domain1.example
→ LDAP_DOMAIN1
```

NetBIOS-/Domain-Präfix, AD-DNS-Domain und erlaubte UPN-Suffixe müssen nicht identisch sein.

Deshalb werden Präfixe und UPN-Suffixe unabhängig konfiguriert.

## Routing unqualifizierter Logins

Ein unqualifizierter Loginname:

```text
username
```

wird standardmäßig dem LOCAL-Provider zugeordnet.

Beispiel:

```text
mkargel
→ LOCAL
```

Diese Regel ist bewusst eindeutig und verhindert ein automatisches Durchprobieren mehrerer Provider.

## Verhalten bei unbekanntem Präfix oder Suffix

Ein qualifizierter Login mit unbekanntem Präfix oder unbekanntem Suffix wird abgelehnt.

Beispiele:

```text
UNKNOWN\mkargel
mkargel@unknown.example
```

führen nicht zu:

```text
Fallback → LOCAL
```

sondern zu einem fehlgeschlagenen Loginversuch.

Damit werden Credentials niemals versehentlich an einen unpassenden Provider weitergereicht.

## Optionale Provider-Auswahl in der UI

Die Login-Oberfläche darf optional einen Selector für interaktiv nutzbare Provider-Instanzen anbieten.

Beispiel:

```text
Login Provider:
[ Automatic ▼ ]

Automatic
Local
DOMAIN1
DOMAIN2
```

`Automatic` ist der Standard und verwendet die Routing-Regeln aus Loginname, Domain-Präfix und UPN-Suffix.

Wählt der Benutzer explizit einen Provider, wird der Loginversuch direkt diesem Provider zugeordnet.

Ein explizit ausgewählter LDAP-Provider darf dabei auch einen unqualifizierten Benutzernamen akzeptieren, sofern dieser Provider dies unterstützt.

Die Provider-Auswahl ist eine Komfortfunktion und ändert nichts am Grundsatz, dass pro Loginversuch genau ein Provider die Credentials erhält.

---

# 5. Ergebnis-, Fehler- und Exception-Modell

## Grundmodell

Auditarium unterscheidet drei grundsätzlich verschiedene Ausgänge eines Use Cases:

```text
Ergebnis
→ erwartbares positives Ende

Fehler
→ erwartbares negatives Ende

Exception
→ unerwartetes negatives Ende
```

Diese drei Fälle werden technisch unterschiedlich behandelt.

Grundsatz:

> Erwartbare fachliche Ausgänge werden modelliert. Unerwartete technische Fehler werden als Exceptions behandelt.

## `Result<T>`

Erfolgreiche Use Cases liefern ein `Result<T>` bzw. bei fehlendem Rückgabewert ein äquivalentes nicht-generisches Result.

Konzeptionell:

```text
Result<T>
├── Success
│   └── Value
└── Failure
    └── AppError[]
```

Ein erwartbarer negativer Ausgang ist kein Ausnahmefall und soll nicht über Exceptions transportiert werden.

Beispiele:

```text
Dokument nicht gefunden
Katalog bereits in Verwendung
Audit kann aus aktuellem Zustand nicht finalisiert werden
Objektzustand steht gewünschter Operation entgegen
```

## `AppError`

Erwartbare Fehler verwenden einen stabilen technischen Fehlertyp.

Konzeptionell:

```csharp
public sealed record AppError(
    string Code,
    ErrorType Type,
    string? Target = null,
    IReadOnlyDictionary<string, object?>? Parameters = null);
```

Die konkrete Typdefinition wird implementierungsnah festgelegt.

Der Error Code ist die stabile technische Identität des Fehlers.

Beispiele:

```text
DOCUMENT.TITLE.REQUIRED
DOCUMENT.NOT_FOUND
CATALOG.NOT_READY
AUDIT.INVALID_STATE
AUTHENTICATION.REQUIRED
AUTHORIZATION.FORBIDDEN
SYSTEM.UNEXPECTED_ERROR
```

BLL-Fehler enthalten keine lokalisierten UI-Texte.

## Fehlerkategorien

Fehler werden zusätzlich durch eine kleine technische Kategorie klassifiziert.

Vorgesehen sind mindestens:

```text
Validation
Unauthorized
Forbidden
NotFound
Conflict
Failure
```

Die Kategorie dient insbesondere zur konsistenten Abbildung in äußere Transportprotokolle.

Die BLL kennt dabei weiterhin keine HTTP-Statuscodes.

## FluentValidation und `AppError`

Der `ValidationBehavior` übersetzt FluentValidation-Ergebnisse in dasselbe `AppError`-Modell.

Beispiel:

```text
Code   = DOCUMENT.TITLE.REQUIRED
Type   = Validation
Target = title
```

Dadurch erhalten Validation-Fehler und fachliche Fehler eine einheitliche technische Struktur.

Der Handler wird bei fehlgeschlagener Request-Validierung nicht ausgeführt.

## HTTP-Abbildung über ProblemDetails

Der API-Layer übersetzt `AppError` in standardisierte ASP.NET-Core-`ProblemDetails`.

Typische Abbildung:

| Error Type | HTTP |
|---|---:|
| `Validation` | 400 Bad Request |
| `Unauthorized` | 401 Unauthorized |
| `Forbidden` | 403 Forbidden |
| `NotFound` | 404 Not Found |
| `Conflict` | 409 Conflict |
| `Failure` | abhängig vom konkreten äußeren Fehlerpfad |

Beispiel:

```json
{
  "status": 409,
  "title": "Conflict",
  "code": "AUDIT.INVALID_STATE",
  "errors": [
    {
      "code": "AUDIT.INVALID_STATE",
      "target": "auditState",
      "parameters": {
        "currentState": "FINALIZED"
      }
    }
  ],
  "traceId": "..."
}
```

Die BLL erzeugt keine HTTP-Statuscodes und kennt keine `ProblemDetails`.

## UI-Lokalisierung von Fehlern

Die UI verwendet:

```text
Error Code
Target
Parameters
```

zur lokalisierten Darstellung.

Beispiel:

```text
AUDIT.INVALID_STATE
currentState = FINALIZED
```

kann in deutscher UI zu:

```text
Das Audit kann im Zustand „Abgeschlossen“ nicht mehr geändert werden.
```

und später in einer anderen Sprache zu einem entsprechend lokalisierten Text führen.

Die Übersetzung bleibt außerhalb der BLL.

## Exceptions

Exceptions sind ausschließlich für unerwartete technische oder programmatische Fehler vorgesehen.

Beispiele:

```text
Datenbankverbindung unerwartet abgebrochen
nicht behandelte Provider-Ausnahme
NullReferenceException
inkonsistente technische Konfiguration
Programmierfehler
```

Exceptions werden nicht in fachliche `AppError` umgedeutet, nur um sie künstlich als erwartbaren Use-Case-Ausgang erscheinen zu lassen.

Grundsatz:

> Fehler sind erwartbare negative Ergebnisse. Exceptions sind unerwartete negative Ergebnisse.

## Zentrales Exception Handling

Unerwartete Exceptions werden zentral abgefangen.

Der zentrale Handler:

```text
Exception
→ technische Logs
→ OpenTelemetry / Trace-Korrelation
→ sichere äußere Fehlerantwort
```

Die technische Diagnose enthält mindestens, soweit verfügbar:

```text
Exception-Typ
Message
StackTrace
Inner Exception
TraceId
SpanId
Request-Kontext ohne unnötige sensitive Nutzdaten
```

Die Exception-Diagnose wird nicht an verschiedenen Controllern oder Endpunkten individuell implementiert.

## Production Exception Response

In Production verlassen keine vollständigen technischen Exception-Details den Server.

Der Client erhält bei unerwarteten Fehlern nur eine bereinigte Antwort, beispielsweise:

```json
{
  "status": 500,
  "title": "Internal Server Error",
  "code": "SYSTEM.UNEXPECTED_ERROR",
  "traceId": "..."
}
```

Nicht an den Client ausgegeben werden insbesondere:

```text
StackTrace
interne Dateipfade
SQL-Details
Connection Strings
Secrets
Tokens
interne Konfigurationswerte
```

Die vollständige technische Diagnose bleibt intern in Logging und Tracing verfügbar.

## Development Exception Handling

In der Development-Umgebung darf Auditarium detaillierte Exception-Diagnose anzeigen.

Bevorzugt werden die vorhandenen ASP.NET-Core-Mechanismen, insbesondere die Developer Exception Page.

Development darf insbesondere anzeigen:

```text
Exception-Typ
Message
StackTrace
Inner Exception
Request Path
TraceId
```

Eine eigene Auditarium-Entwickler-Fehleroberfläche wird nicht aufgebaut, solange die integrierten ASP.NET-Core-Mechanismen den Bedarf erfüllen.

Die detaillierte Developer Exception Page darf niemals in Production aktiviert werden.

## Trace-Korrelation

Jede unerwartete Exception soll mit der laufenden technischen Trace-/Logging-Korrelation verbunden werden.

Production zeigt dem Client eine `traceId`.

Der Betreiber kann diese `traceId` verwenden, um in:

```text
Logs
OpenTelemetry Traces
APM-/Observability-Systemen
```

die vollständige technische Diagnose inklusive Stacktrace aufzufinden.

Grundsatz:

> Die Diagnose bleibt vollständig erhalten; nur ihre Sichtbarkeit nach außen hängt vom Environment ab.

## Sensitive Daten in Fehlerpfaden

Auch im Exception-Pfad gilt die bestehende Telemetrie-Hygiene.

Nicht bewusst in Logs, Traces oder Fehlerobjekte geschrieben werden dürfen insbesondere:

```text
Passwörter
Passwort-Hashes
API-Secrets
Tokens
Authorization Header
vollständige Request Bodies
Audit-Kommentare
Evidence-Inhalte
sonstige sensible Freitexte
```

Development erlaubt mehr technische Diagnose, hebt diese Grundregel aber nicht auf.

---

# 6. Observability, Health und technische Diagnose

## Observability-Grundmodell

Auditarium behandelt technische Observability als eigenständige Infrastruktur neben dem fachlichen Audit-Log.

Grundmodell:

```text
Auditarium Code
│
├── ILogger<T>
│   └── strukturierte technische Logs
│
├── Meter
│   └── technische und ausgewählte fachlich sinnvolle Metrics
│
├── ActivitySource
│   └── Traces / Spans
│
└── ASP.NET Health Checks
    ├── /health/live
    └── /health/ready
         │
         ▼
   OpenTelemetry
         │
   ┌─────┴──────────────┐
   │                    │
  OTLP             Prometheus
   │               /metrics
   ▼                    │
OTel Collector       externe Consumer
```

Grundsatz:

> Auditarium erzeugt standardisierte Telemetrie. Welches System sie konsumiert, ist Sache des Betreibers.

`system_audit_log` und technische Observability erfüllen unterschiedliche Aufgaben und werden nicht vermischt.

## Technisches Logging

Technisches Application Logging erfolgt über:

```text
Microsoft.Extensions.Logging
ILogger<T>
```

Auditarium-Code verwendet `ILogger<T>` als primäre Logging-Abstraktion.

Beispiele für technische Logs:

```text
Application started
Database migration completed
Bootstrap/Reconcile completed
LDAP connection failed
Unhandled exception
Recovery mode enabled
CQRS request failed
```

Logs werden strukturiert geschrieben.

Beispiel:

```csharp
_logger.LogInformation(
    "Catalog {CatalogVersionId} published with {QuestionCount} questions",
    catalogVersionId,
    questionCount);
```

String-Interpolation für strukturierte Logdaten soll vermieden werden.

Console-/stdout-Logging bleibt standardmäßig verfügbar und ist insbesondere für Container- und Service-Betrieb geeignet.

OpenTelemetry kann zusätzlich als Logging Provider verwendet werden und strukturierte Logs über OTLP exportieren.

## Metrics

Metrics werden über:

```text
System.Diagnostics.Metrics
Meter
```

erzeugt.

Auditarium nutzt vorhandene ASP.NET-/HTTP-/Runtime-Instrumentierung soweit sinnvoll und ergänzt nur ausgewählte eigene Metriken.

Mögliche Beispiele:

```text
auditarium.cqrs.requests
auditarium.cqrs.request.duration
auditarium.cqrs.errors
auditarium.import.duration
auditarium.import.failures
auditarium.catalog.publish.duration
auditarium.catalog.publish.failures
auditarium.login.failures
```

Die konkrete Metric-Liste wird implementierungsnah und bedarfsgerecht festgelegt.

Metrics dürfen nur kontrollierte, niedrig kardinale Attribute/Labels verwenden.

Geeignet:

```text
request_type = PublishCatalogCommand
status = success
```

Nicht geeignet:

```text
user_id
audit_id
document_id
question_id
```

Grundsatz:

> Einzelobjekt-IDs gehören nicht in Metric-Labels.

## Tracing

Tracing wird über:

```text
System.Diagnostics.ActivitySource
System.Diagnostics.Activity
```

umgesetzt.

OpenTelemetry sammelt und korreliert daraus technische Traces.

Vorhandene Instrumentierungen für ASP.NET Core, HTTP-Clients und Entity Framework sollen möglichst genutzt werden.

Eigene Auditarium-Spans werden nur dort ergänzt, wo sie echten diagnostischen Mehrwert bieten.

Mögliche Beispiele:

```text
Catalog.Publish
Audit.Publish
Audit.Finalize
Catalog.Import
Bootstrap.Reconcile
Permission.Reconcile
```

Ein typischer Trace kann beispielsweise abbilden:

```text
HTTP Request
  └── PublishCatalogCommand
       ├── Authorization
       ├── Validation
       ├── Handler
       │    └── EF Core / SQL
       └── Response
```

Logs und Traces sollen über Trace-/Span-Korrelation miteinander verknüpfbar sein.

## Health Checks

Auditarium stellt mindestens folgende technische Health-Endpunkte bereit:

```text
/health/live
/health/ready
```

`/health/live` prüft ausschließlich, ob der Prozess grundsätzlich lebt und HTTP-Anfragen beantworten kann.

`/health/ready` prüft, ob Auditarium betriebsbereit ist.

Dazu können abhängig von der konkreten Installation insbesondere gehören:

- Startup vollständig abgeschlossen,
- Migrationen erfolgreich,
- Bootstrap/Reconcile erfolgreich,
- Datenbank erreichbar,
- notwendige Kernabhängigkeiten verfügbar.

Optionale Integrationen wie LDAP werden nur dann Bestandteil von Readiness, wenn sie für die konkrete Installation zwingend erforderlich sind.

## OpenTelemetry

OpenTelemetry ist die gemeinsame Observability-Schicht für:

```text
Logs
Metrics
Traces
```

Auditarium bindet sich im Anwendungscode möglichst an die .NET-Standardabstraktionen:

```text
ILogger<T>
Meter
ActivitySource
```

OpenTelemetry übernimmt Sammlung, Korrelation und Export.

Damit bleibt der Anwendungscode unabhängig von konkreten Monitoring-, Logging- oder APM-Produkten.

## OTLP

OTLP ist der primäre standardisierte externe Transport für zentrale Observability.

Konzeptionelle Konfiguration:

```text
OpenTelemetry:Enabled = true
OpenTelemetry:Otlp:Endpoint = http://otel-collector:4317
OpenTelemetry:ServiceName = Auditarium
```

Ist kein OTLP-Ziel konfiguriert, bleibt Auditarium vollständig funktionsfähig.

Observability ist keine Voraussetzung für den fachlichen Betrieb.

Ein OpenTelemetry Collector kann bei Bedarf als zentrale Verteilstelle dienen:

```text
Auditarium
   ↓ OTLP
OpenTelemetry Collector
   ├── Logs
   ├── Metrics
   └── Traces
```

Die konkreten Backends sind Betreiberentscheidung.

## Prometheus-kompatibler Metrics-Endpunkt

Zusätzlich zum OTLP-Export unterstützt Auditarium einen Prometheus-kompatiblen Metrics-Endpunkt:

```text
GET /metrics
```

Dieser Endpunkt ermöglicht direkten Scrape durch geeignete Monitoring-Systeme.

Mögliche Konsumenten sind beispielsweise:

```text
Prometheus
Checkmk
andere Prometheus-kompatible Monitoring-Systeme
```

Der `/metrics`-Endpoint ist ein zusätzlicher Exportweg und ersetzt OTLP nicht.

## ObservabilityBehavior

`ObservabilityBehavior` bündelt zentrale technische CQRS-Telemetrie:

```text
ILogger<T>
→ Request Start / Ende / Fehler

ActivitySource
→ CQRS Span

Meter
→ Request Count / Duration / Failure Count
```

Der Behavior bleibt rein technisch.

Er enthält keine fachliche Businesslogik und erzeugt insbesondere **nicht** das `system_audit_log`.

Die Protokollierung tatsächlicher Entity-Änderungen bleibt an `SaveChanges` gekoppelt. Für Ereignisse ohne zugehörige Datenänderung gilt der ausdrückliche Ereignisprotokollierungsweg aus Kapitel 7; technische Telemetrie ersetzt diesen nicht.

## Datenschutz und Telemetrie-Hygiene

Technische Telemetrie darf nicht zu einer zweiten Datenkopie der fachlichen Audit-Inhalte werden.

Nicht in technische Telemetrie gehören insbesondere:

```text
Passwörter
Passwort-Hashes
Tokens
Secrets
Authorization Header
vollständige API-Credentials
vollständige Request Bodies
Audit-Kommentare
Evidence-Inhalte
sensible Freitexte
```

Objekt-IDs dürfen in Logs und Traces bei begründetem diagnostischem Bedarf verwendet werden.

In Metrics sind hochkardinale IDs und benutzerspezifische Labels zu vermeiden.

Die Telemetrie-Instrumentierung folgt dem Grundsatz:

> So viel technische Information wie nötig, so wenig fachliche oder personenbezogene Nutzdaten wie möglich.

## Unterstützte Consumer

Auditarium implementiert keine direkte Bindung an ein einzelnes Monitoring- oder Observability-Produkt.

Mögliche Consumer sind unter anderem:

```text
OpenTelemetry Collector
Prometheus
Checkmk
Grafana / Loki / Tempo
Azure Monitor
Datadog
Elastic
andere OTLP- oder Prometheus-kompatible Systeme
```

Diese Liste ist nicht normativ.

Grundsatz:

> Auditarium stellt standardisierte Observability bereit; die konkrete Auswertung und Speicherung liegt beim Betreiber.

---

# 7. System-Audit-Log, Soft Delete und Retention

## Audit-Log-Grundsatz

Neben den fachlichen Auditdaten führt Auditarium ein zentrales technisches Ereignisprotokoll:

```text
system_audit_log
```

Es ist ausdrücklich vom fachlichen Objekt `audits` zu unterscheiden.

Das `system_audit_log` ist grundsätzlich **append-only**. Bereits geschriebene Logeinträge werden während ihrer Lebensdauer nicht verändert. Ein kontrollierter physischer Purge abgelaufener und nicht mehr benötigter Logeinträge nach den in diesem Kapitel definierten Retention-Regeln bleibt zulässig.

Das Protokoll dient der nachvollziehbaren Zuordnung relevanter Änderungen und Sicherheitsereignisse zu Zeitpunkt, Akteur und betroffenem technischen Objekt.

## Gemeinsame Persistenz von Änderungen und Audit-Log

Eine protokollierungspflichtige Datenänderung darf nur gemeinsam mit sämtlichen zugehörigen Audit-Log-Einträgen erfolgreich gespeichert werden.

Grundsatz:

> **Datenänderung und Änderungsprotokoll bilden eine atomare Speichereinheit in derselben Datenbanktransaktion.**

Die gemeinsame Transaktion verwendet die Datenbank des für die Installation ausgewählten Providers. Diese Regel gilt gleichermaßen für PostgreSQL und Microsoft SQL Server.

```text
Datenänderung und zugehörige Audit-Log-Einträge erfolgreich
→ gemeinsamer Commit

Datenänderung oder erforderliche Audit-Log-Speicherung schlägt fehl
→ Rollback der gesamten betroffenen Speichereinheit
→ kein erfolgreicher Abschluss der Änderung
```

Das gilt für Anlage, Änderung und Löschung protokollierungspflichtiger Daten einschließlich administrativer Änderungen. Ein Fehler bei der Erstellung oder Speicherung des erforderlichen Änderungsprotokolls darf nicht abgefangen und als erfolgreicher Fachvorgang behandelt werden. Der Aufrufer erhält einen Fehler über das bestehende Fehler-/Exception-Modell.

### Zentraler auditierter Speichervorgang

Das DAL integriert die Erzeugung und Speicherung der Änderungsprotokolle in den zentralen `SaveChangesAsync()`-Weg.

Liegen Fachänderungen und vollständige Audit-Log-Einträge gemeinsam vor, werden sie im selben EF-Core-Speichervorgang persistiert.

Bei datenbankgenerierten Objekt-IDs oder anderen Werten, die erst während des Speicherns feststehen, kann die technische Umsetzung mehrere Schritte benötigen. Ein zulässiger Ablauf ist konzeptionell:

```text
gemeinsame Transaktion beginnen
→ relevante Änderungen und erforderliche Vorher-Werte erfassen
→ Fachänderungen speichern und endgültige Objekt-IDs erhalten
→ vollständige Audit-Log-Einträge speichern
→ gemeinsame Transaktion committen
```

Die beteiligten internen Speicherschritte dürfen nicht einzeln committen. Existiert bereits eine umgebende Transaktion des Use Cases, wird diese verwendet; der zentrale Speichervorgang darf sie nicht eigenständig committen. Bei einem Fehler dürfen bereits ausgeführte Teilschritte nicht anschließend als erfolgreicher Rest des Use Cases committed werden.

Die Erzeugung eines Audit-Log-Eintrags löst nicht ihrerseits erneut die Erzeugung eines Änderungsprotokolls aus. Zulässige technische Wiederholungsversuche müssen die atomare Speichereinheit berücksichtigen und dürfen keine doppelten Protokolle derselben erfolgreich gespeicherten Änderung erzeugen. Die bestehenden Regeln gegen blinde Retries fachlicher Concurrency-Konflikte gelten weiterhin.

Die konkrete Umsetzung, beispielsweise über eine DbContext-Erweiterung oder geeignete EF-Core-Erweiterungspunkte, wird im Persistenz-Arbeitspaket festgelegt. Verbindlich sind die gemeinsame Transaktion, die korrekte Fehlerbehandlung und die Kapselung im DAL. Ein zusätzliches `IUnitOfWork`, Repository oder generischer `TransactionBehavior` ist dafür nicht vorgesehen.

### Abgrenzung

Diese Atomaritätsregel betrifft Datenänderungen und deren Änderungsprotokolle in der Datenbank. Technische Observability und externe Telemetrie-Empfänger sind kein Bestandteil dieser Speichereinheit. Für physische Dateioperationen gelten weiterhin die getrennten FAL- und Fehlerbehandlungsregeln.

Für Ereignisse ohne zugehörige Datenänderung sowie die Protokollierung fehlgeschlagener Vorgänge gelten die nachfolgenden gesonderten Regeln. Die gemeinsame Speicherung erfolgreicher Datenänderungen bleibt davon unberührt.

Die besondere Abgrenzung der technischen Initialisierung und des Bootstrap-Audit-Loggings aus Kapitel 3 bleibt bestehen.

## Ereignisprotokollierung ohne zugehörige Datenänderung

Login, Logout, fehlgeschlagene Anmeldung, verweigerter Zugriff und Export können ohne Änderung einer fachlichen Entity stattfinden. Ihre Protokollierung darf deshalb nicht allein von der automatischen Erkennung von Entity-Änderungen abhängen.

Die jeweils verantwortlichen Authentifizierungs-, Autorisierungs- oder Exportpfade veranlassen diese Ereignisse ausdrücklich. Die Speicherung erfolgt zentral im DAL in das bestehende `system_audit_log` und verwendet dieselben Regeln für Akteur, Ereigniskontext und Maskierung. Der technische `ObservabilityBehavior` übernimmt diese Aufgabe nicht.

### Verhalten bei Protokollierungsfehlern

| Vorgang | Verhalten, wenn der erforderliche Audit-Log-Eintrag nicht gespeichert werden kann |
|---|---|
| erfolgreiche Anmeldung | Anmeldung nicht abschließen; keine neue authentifizierte Sitzung bzw. kein Authentication-Cookie ausstellen |
| Export | Export nicht ausliefern; keine Exportdaten an den Aufrufer übertragen |
| Logout | eigene Browseranmeldung trotzdem beenden; das Entfernen des Authentication-Cookies darf nicht am Protokollierungsfehler scheitern |
| fehlgeschlagene Anmeldung oder verweigerter Zugriff | Ablehnung bleibt bestehen; der Protokollierungsfehler darf keinen Zugriff ermöglichen |

Grundsatz:

> **Anmeldung und Export setzen erfolgreiche Ereignisprotokollierung voraus. Logout und die Ablehnung unzulässiger Zugriffe bleiben auch bei einem Protokollierungsfehler wirksam.**

Bei Anmeldung und Export wird der erforderliche Ereigniseintrag vor Ausgabe des Authentication-Cookies bzw. vor Beginn der Übertragung von Exportdaten gespeichert. Ein bereits gespeicherter Eintrag dokumentiert die serverseitige Freigabe, nicht den nachweislich erfolgreichen Empfang des Cookies oder der vollständigen Exportdatei beim Client.

Ein Protokollierungsfehler wird zusätzlich über die bestehende technische Observability als Fehler gemeldet. Dabei gelten weiterhin die Regeln gegen Secrets und sensible Nutzdaten in Logs, Traces und Metrics. Technische Telemetrie ist kein Ersatz für einen fehlenden Eintrag im `system_audit_log` und hebt insbesondere die Sperre von Anmeldung oder Export nicht auf.

Beim Logout und bei bereits abgelehnten Vorgängen darf auch ein Fehler der technischen Fehlermeldung die Abmeldung bzw. Ablehnung nicht verhindern oder aufheben. Die sonstigen Prüfungen des Logout-Wegs, insbesondere der Antiforgery-Schutz, bleiben bestehen.

### Protokollierung fehlgeschlagener Vorgänge

Ein Fehlerereignis über einen bereits zurückgerollten Vorgang, beispielsweise einen fehlgeschlagenen Import-Apply, wird außerhalb der zurückgerollten Fachtransaktion gespeichert. Es darf weder mit dieser Transaktion verschwinden noch deren Änderungen erneut ausführen.

Schlägt auch diese Ereignisprotokollierung fehl, bleibt der ursprüngliche Vorgang fehlgeschlagen. Der zusätzliche Protokollierungsfehler wird technisch gemeldet; er darf nicht zu einer Erfolgsmeldung oder einer Freigabe des abgelehnten Vorgangs führen.

## Zu protokollierende Ereignisse

Jegliche ändernde und/oder sicherheitsrelevante Aktion wird protokolliert.

Dazu gehören mindestens:

- Login,
- Logout,
- fehlgeschlagene Anmeldung,
- verweigerter Zugriff,
- Benutzeranlage und -deaktivierung,
- Rollenvergabe und Rollenentzug,
- Änderungen an Authentifizierungs- oder Sicherheitseinstellungen,
- Anlage von Fachobjekten,
- Änderungen an Fachobjekten,
- Soft Delete,
- Wiederherstellung,
- physischer Purge,
- Import von Regelwerken und Katalogversionen,
- Anwenden von Importpaketen einschließlich erfolgreicher und fehlgeschlagener Apply-Vorgänge,
- Löschen unvollständiger Katalogversionen,
- Änderungen an Gewichtungen,
- Erstellung von Audits,
- Zustandsänderungen von Audits sowie Änderungen an Auditantworten,
- Finalisierung,
- Exporte,
- spätere Erstellung, Änderung oder Widerruf von API-Zugangsdaten.

Für Soft Delete gilt ausdrücklich eine Ausnahme vom allgemeinen Prinzip, Änderungsmetadaten möglichst nur im `system_audit_log` zu halten.

Die drei Felder

```text
deleted_at
deleted_by
deletion_reason
```

bleiben vollständig direkt am jeweiligen Fachobjekt gespeichert.

Sie beschreiben den aktuellen operativen Löschzustand des Objekts und werden für Sichtbarkeit, Wiederherstellung und Retention benötigt. Das `system_audit_log` protokolliert die Ereignisse `DELETED`, `RESTORED` und `PURGED` zusätzlich historisch, ersetzt diese Felder aber nicht.

## Tabelle `system_audit_log`

Mindestens folgende Felder sind vorgesehen:

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `event_id` | `NOT NULL` | technisch erzeugt | interne eindeutige ID des Ereignisses |
| `occurred_at` | `NOT NULL` | Ereigniszeitpunkt | Zeitpunkt des Ereignisses |
| `user_id` | `NOT NULL` | explizit; System = `0` | Referenz auf `users.user_id` |
| `action` | `NOT NULL` | – | zentral definierter technischer Action-Code |
| `object_type` | `NOT NULL` | – | stabiler technischer Entity-/Klassentyp des betroffenen Objekts bzw. Ereigniskontexts |
| `object_id` | `NULL` erlaubt | `NULL` | ID des betroffenen persistierten Objekts; nur leer, wenn kein solches Zielobjekt existiert |
| `before_state` | `NULL` erlaubt | `NULL` | relevante Werte vor der Änderung als JSON-Dokument |
| `after_state` | `NULL` erlaubt | `NULL` | relevante Werte nach der Änderung als JSON-Dokument |

`action` und `object_type` müssen nicht-leere, technisch validierte Werte enthalten.

`user_id` ist `NOT NULL` und referenziert `users.user_id`.

```text
user_id = 0
→ Aktion durch Auditarium selbst bzw. ein Ereignis ohne authentifizierten Benutzer

user_id >= 1
→ Aktion durch einen regulären oder technischen Benutzer
```

Damit besitzt jedes Audit-Log-Ereignis einen eindeutig bestimmbaren protokollierenden Akteur.

`action` ist kein Freitextfeld. Auditarium verwendet einen zentral definierten und technisch validierten Satz von Action-Codes. Neue Codes werden nur eingeführt, wenn ein neuer fachlich oder technisch unterscheidbarer Ereignistyp tatsächlich benötigt wird.

Der Action-Code bleibt möglichst generisch. Kontext, der bereits durch `object_type`, `object_id` und die Zustandsdaten eindeutig beschrieben wird, wird nicht zusätzlich in den Action-Namen eingebaut.

`object_type` ist ebenfalls kein Freitextfeld. Der Wert entspricht einem zentral definierten technischen Entity-/Klassentyp und muss innerhalb des Auditarium-Modells eindeutig und langfristig stabil sein.

Ein vollqualifizierter Klassenname einschließlich Namespace kann verwendet werden, wenn diese technische Identität bewusst stabil gehalten wird. Reine Code- oder Namespace-Refactorings dürfen bestehende Audit-Log-Einträge nicht semantisch ungültig machen.

Beispielhafte technische Typen:

```text
User
Role
Document
CatalogVersion
DocumentElement
Question
AuditUnit
Audit
AuditDocumentElement
AuditQuestion
SystemSetting
ApiCredential
```

Die tatsächlich verwendeten kanonischen Typbezeichner werden im technischen Modell festgelegt.

Für Zustandswechsel wird kein objekt- oder feldspezifischer Action-Code erzeugt.

Beispiel:

```text
object_type = Document
object_id   = <id>
action      = STATE_CHANGED
```

Dasselbe Muster gilt beispielsweise für `CatalogVersion` und `Audit`.

Für häufige Historienabfragen soll die technische Umsetzung einen geeigneten zusammengesetzten Index vorsehen, konzeptionell beispielsweise auf:

```text
(object_type, object_id, action, occurred_at DESC)
```

Damit kann die Anwendung effizient die Ereignishistorie bzw. den letzten relevanten Zustandswechsel eines konkreten Objekts ermitteln.

## Vorher-/Nachher-Zustand

Bei Änderungen an persistierten Fachobjekten werden die tatsächlich geänderten Properties durch Entity Framework in der Persistenzschicht ermittelt und als Delta über `before_state` und `after_state` protokolliert.

Das `system_audit_log` ist damit ein Änderungsprotokoll und kein fortlaufender Vollsnapshot aller Fachobjekte.

Beispiel:

```text
object_type = AuditUnit
object_id   = 42
action      = UPDATED
```

```json
{
  "before_state": {
    "name": "Serverraum EG"
  },
  "after_state": {
    "name": "Serverraum Erdgeschoss"
  }
}
```

Mehrere im selben Speichervorgang geänderte Properties derselben Entity werden gemeinsam im Delta dieses Ereignisses abgebildet.

Bei nicht ändernden Sicherheitsereignissen wie Login, Logout oder fehlgeschlagener Anmeldung können `before_state` und `after_state` leer bleiben.

Die konkrete Ermittlung der Änderungen und die technische Abbildung der JSON-Dokumente sind Aufgabe der Persistenzschicht und des jeweiligen Entity-Framework-Datenbankproviders.

## Schutz sensibler Informationen

Bestimmte Informationen dürfen niemals im `system_audit_log` gespeichert werden.

Dazu gehören insbesondere:

- Passwörter,
- Passwort-Hashes,
- LDAP-Credentials,
- OAuth-/OIDC-Secrets,
- API-Tokens,
- Session-Tokens,
- private Schlüssel,
- sonstige Secrets.

Vor dem Schreiben in das Audit-Log müssen sensible Felder entfernt oder maskiert werden.

### Positivlisten-Modell für Zustandsdaten

Die Zustandsdaten in `before_state` und `after_state` werden ausschließlich aus einer zentral gepflegten Positivliste je `object_type` und `action` gebildet.

```text
Property ausdrücklich für Objekt und Ereignis freigegeben
→ darf als fachlich relevantes Delta protokolliert werden

Property nicht freigegeben, unbekannt oder später hinzugefügt
→ wird nicht in before_state oder after_state aufgenommen
```

Eine generische Serialisierung aller durch Entity Framework ermittelten Properties ist nicht zulässig. Die Persistenzschicht verwendet die Änderungsinformationen nur, um die für das konkrete Ereignis freigegebenen Werte zu bestimmen.

Die Positivliste enthält nur Werte, die für die Nachvollziehbarkeit der jeweiligen Änderung erforderlich sind. Personenbezogene Angaben wie Anzeigename oder E-Mail-Adresse dürfen nur dann aufgenommen werden, wenn sie für den konkreten Änderungsnachweis notwendig und ausdrücklich freigegeben sind. Andernfalls genügt die stabile technische Referenz, insbesondere `user_id`, `object_type` und `object_id`.

Die in diesem Kapitel genannten Secrets, Credentials, Passwort- und Secret-Hashes, Tokens, Schlüsselmaterial und Recovery-Konfigurationswerte sind ausnahmslos ausgeschlossen. Sie werden weder im Klartext noch maskiert, gehasht oder in abgeleiteter Form in Zustandsdaten aufgenommen. Für Secret-Settings bleiben ausschließlich die bereits definierten nicht-sensitiven Metadaten wie `configured_before` und `configured_after` zulässig.

Die konkrete Positivliste wird zusammen mit jedem persistierten Fachobjekt und neuen protokollierungspflichtigen Ereignis im technischen Modell ergänzt und getestet. Eine neue oder geänderte freigegebene Property ist eine bewusste Änderung des Audit-Log-Vertrags und wird in der technischen Dokumentation nachvollziehbar gemacht.

Dieses Modell unterstützt Datenminimierung und den Schutz sensibler Daten. Die rechtmäßige Verarbeitung personenbezogener Daten, Retention, Betreiberkonfiguration und organisatorische Maßnahmen bleiben davon unabhängige Pflichten.

## Zweistufiges Löschen

Löschvorgänge für fachliche Objekte erfolgen grundsätzlich zweistufig.

Soft Delete ist ausschließlich für fachliche Aggregate Roots vorgesehen. Es wird nicht für Benutzer, Rollen, Permissions, Authentifizierungsprovider, Zugangsdaten, Settings oder andere technische beziehungsweise Security-Konfigurationsobjekte eingesetzt. Deren fachliche Lifecycle-Regeln bleiben maßgeblich.

### Stufe 1: Soft Delete

Das Objekt bleibt physisch in der Datenbank vorhanden, wird jedoch als gelöscht markiert.

Mindestens vorgesehen:

| Feld | Bedeutung |
|---|---|
| `deleted_at` | Zeitpunkt des Soft Deletes |
| `deleted_by` | Benutzer, der die Löschung ausgelöst hat |
| `deletion_reason` | optionale Begründung |

Soft gelöschte Objekte werden:

- in normalen Ansichten ausgeblendet,
- nicht mehr für neue Audits oder operative Prozesse verwendet,
- mindestens bis zum Ablauf der konfigurierten Soft-Delete-Retention aufbewahrt.

### Stufe 2: Purge

Nach Ablauf der Soft-Delete-Retention wird ein Objekt grundsätzlich **purgeberechtigt**.

Der physische Purge erfolgt nur, wenn zusätzlich alle fachlichen und referenziellen Abhängigkeitsbedingungen erfüllt sind.

Der Purge eines Fachobjekts löscht dessen Ereignisse im `system_audit_log` nicht automatisch. Diese unterliegen ihrer eigenen Retention und Abhängigkeitsprüfung.

## Wiederherstellung soft gelöschter Objekte

Soft Delete ist im initialen Sollstand eine technische Schutzschicht und **kein Benutzer- oder Self-Service-Feature**.

Für v1 gilt ausdrücklich:

```text
kein Papierkorb
kein Restore-Dialog
kein Restore-Button
kein allgemeiner Restore-Endpunkt in der API
keine Suche nach gelöschten Objekten in der normalen UI
```

Soft gelöschte Objekte bleiben in normalen Anwendungsansichten ausgeblendet.

Solange ein Aggregate Root nur soft gelöscht und noch nicht physisch gepurged wurde, darf ein Betreiber ihn im Ausnahmefall administrativ direkt in der Datenbank wiederherstellen.

Das Administratorhandbuch muss dafür einen kleinen, geprüften Satz von SQL-Hilfen bereitstellen, mindestens für:

```text
soft gelöschte Aggregate Roots identifizieren
Löschmetadaten eines konkreten Aggregate Roots prüfen
relevante Abhängigkeiten vor dem Restore prüfen
gezielten Restore eines konkreten Aggregate Roots durchführen
```

Der Restore erfolgt am Aggregate Root und umfasst dessen abhängige Bestandteile gemäß dem bestehenden Aggregate-Modell.

Konzeptionell besteht ein Restore aus dem gezielten Zurücksetzen der Soft-Delete-Metadaten:

```text
deleted_at      → NULL
deleted_by      → NULL
deletion_reason → NULL
```

Die konkrete SQL-Syntax und die notwendigen Prüfungen werden im Administratorhandbuch für die unterstützten Datenbankprovider dokumentiert.

Ein erfolgreicher Restore bleibt als Ereignis:

```text
RESTORED
```

im `system_audit_log` nachvollziehbar.

Nach dem physischen Purge ist keine normale Wiederherstellung des Fachobjekts mehr möglich.

> **Soft Delete ersetzt kein Backup.**

## Retention für soft gelöschte Objekte

Die Mindestaufbewahrungsdauer soft gelöschter fachlicher Objekte wird separat konfiguriert.

Beispiel:

```text
AUDITARIUM__Retention__DeletionDays=90
```

Der Wert:

- besitzt einen sinnvollen Default,
- wird beim Start der Anwendung aus Deployment-/Environment-Konfiguration geladen,
- ist nicht über die normale UI veränderbar,
- kann in einer Systeminformationsansicht read-only angezeigt werden.

Nach Ablauf dieser Frist darf ein Objekt nur dann physisch gepurged werden, wenn die für seinen Aggregate Root und seine Referenzen geltenden Abhängigkeitsbedingungen erfüllt sind.

## Retention des System-Audit-Logs

Für das `system_audit_log` existiert eine eigene Mindest-Retention, unabhängig von der Soft-Delete-Retention fachlicher Objekte.

Beispiel:

```text
AUDITARIUM__Retention__AuditLogDays
```

Der Wert:

- wird über Deployment-/Environment-Konfiguration gesetzt,
- ist nicht über die normale UI veränderbar,
- kann read-only angezeigt werden.

Das Erreichen dieser Frist macht einen Audit-Log-Eintrag lediglich **purgeberechtigt**.

Ein Eintrag darf nur physisch gepurged werden, wenn zusätzlich:

- das zugehörige persistierte Ursprungsobjekt nicht mehr existiert,
- das Ursprungsobjekt also insbesondere nicht lediglich soft gelöscht ist,
- keine weiterhin existierenden abhängigen Fachobjekte auf diese Historie angewiesen sind.

Für Ereignisse ohne persistiertes Ursprungsobjekt gelten die eigene Mindest-Retention und die jeweils definierten fachlichen Abhängigkeitsregeln.

Für den ersten Implementierungsstand kann der Audit-Log-Purge vollständig deaktiviert werden; die Einträge bleiben dann unbegrenzt erhalten.

## Verhältnis der beiden Retention-Werte

Die beiden konfigurierten Mindestfristen sind voneinander unabhängig:

```text
DELETION_RETENTION
```

bestimmt, ab wann ein soft gelöschtes Fachobjekt grundsätzlich für einen Purge infrage kommt.

```text
AUDIT_LOG_RETENTION
```

bestimmt, ab wann ein Audit-Log-Eintrag grundsätzlich für einen Purge infrage kommt.

Keine der beiden Fristen erzwingt eine Löschung.

Insbesondere darf das Löschen oder Purgen eines fachlichen Objekts niemals automatisch dessen Historie im `system_audit_log` entfernen.

## Backup und vollständige Wiederherstellung

Auditarium enthält in v1 **keine eigene Backup- oder Disaster-Restore-Funktion**.

Auditarium definiert ausschließlich, welche persistenten Bestandteile für eine vollständige Wiederherstellung verfügbar sein müssen.

Zum notwendigen Sicherungsumfang gehören mindestens:

```text
Datenbank
├── Fach- und Auditdaten
├── Benutzer / Rollen / Identities
├── Application Settings
├── Soft-Delete-Zustände
└── system_audit_log

FileStorage
└── persistierte Quelldokumente und sonstige durch Auditarium gespeicherte Dateien

Data-Protection-Keyring
└── erforderlich für weiterhin entschlüsselbare DB-gespeicherte Secrets

externe Betreiber-Konfiguration und Secrets
└── Config / Environment / Deployment-Werte, die zum Wiederanlauf benötigt werden
```

Insbesondere muss der Data-Protection-Keyring zusammen mit den übrigen persistenten Daten wieder verfügbar sein. Eine wiederhergestellte Datenbank allein genügt nicht, wenn darin geschützte Secrets enthalten sind, deren Schlüsselmaterial fehlt.

Auditarium macht **keine Vorgaben** zu:

```text
Backup-Verfahren
Backup-Produkt
Backup-Zeitplan
Backup-Häufigkeit
Aufbewahrungsdauer der Backups
Speicherort
Medienrotation
Offsite-/Offline-Kopien
Restore-Testintervallen
```

Diese Entscheidungen liegen vollständig in der Verantwortung des Betreibers.

Auditarium:

```text
erstellt keine Backups
plant keine Backups
überwacht keine Backups
prüft kein Backup-Alter
meldet keine fehlenden Backups
validiert keine Backup-Vollständigkeit
führt keinen Disaster-Restore aus
```

Grundsatz:

> **Auditarium beschreibt, was für eine vollständige Wiederherstellung gesichert sein muss. Wie, wann und wie häufig gesichert wird, entscheidet und verantwortet ausschließlich der Betreiber.**

Fehlen für den Wiederanlauf notwendige persistente Bestandteile, kann Auditarium keine vollständige Wiederherstellung gewährleisten.


## Aggregate und abhängige Daten

Delete, Restore und Purge werden nicht beliebig auf einzelne untergeordnete Datensätze angewendet.

Fachliche Aggregate besitzen einen übergeordneten Lebenszyklus.

Beispiel:

```text
catalog_version
└── document_elements
    ├── questions
    │   └── question_scope_types
    └── document_element_weights
```

Innerhalb eines solchen Aggregats gilt:

> **Delete, Restore und Purge erfolgen am Aggregate Root und umfassen dessen abhängige Bestandteile.**

Einzelne prüfbare Dokumentelemente oder Fragen eines Katalogs werden nicht separat gelöscht oder wiederhergestellt.

Bei einem Soft Delete genügt es, den Aggregate Root als gelöscht zu markieren. Abhängige Bestandteile gelten dadurch implizit ebenfalls als gelöscht.

Beim Restore wird das vollständige Aggregate wiederhergestellt.

Beim Purge wird der vollständige abhängige Datenbestand entfernt.

Für rein aggregatinterne Besitzbeziehungen darf der physische Purge über Datenbank-Cascades umgesetzt werden. Externe fachliche Referenzen werden davon ausdrücklich nicht erfasst.


## Externe Referenzen und Löschschutz

Abhängige Bestandteile eines Aggregats sind von externen fachlichen Referenzen zu unterscheiden.

Beispiel:

```text
catalog_version ← audit
```

Ein Audit ist kein Bestandteil des Katalog-Aggregats, sondern eine externe fachliche Referenz.

Grundregel:

> **Ein Objekt darf nicht gelöscht oder gepurged werden, wenn dadurch eine noch existierende schützenswerte externe Referenz ungültig würde.**

Solche Beziehungen werden technisch mit `RESTRICT` bzw. `NO ACTION` behandelt und nicht kaskadiert gelöscht.

Beispiele:

- eine von Audits referenzierte Katalogversion darf nicht gelöscht werden,
- eine Audit Unit mit historischen Audits darf operativ auf `INACTIVE` gesetzt und ggf. soft gelöscht werden, aber nicht physisch gepurged werden, solange schützenswerte Referenzen bestehen.

## Fachliche Außerbetriebnahme statt Löschung

Nicht jedes Objekt, das künftig nicht mehr verwendet werden soll, muss gelöscht werden.

Dafür werden fachlich passende reversible Zustände verwendet:

| Objekt | Zustand | Bedeutung |
|---|---|---|
| `documents` | `ACTIVE` / `DEPRECATED` | Regelwerk für neue Audits nutzbar bzw. bewusst außer Betrieb genommen |
| `audit_units` | `usage_state`: `ACTIVE` / `INACTIVE` | Prüfobjekt aktuell nutzbar bzw. historisch/inaktiv |
| `users` | `is_active = true / false` | Benutzer darf sich anmelden bzw. bleibt nur historisch erhalten |

Es wird bewusst **kein universelles `usage_state`** für alle Tabellen eingeführt.

Andere Objekte behalten ihre fachlich passenden Zustände:

- `catalog_versions`: `DRAFT`, `READY`; technische Importvorgänge besitzen getrennte Zustände
- `audits`: `DRAFT`, `READY`, `IN_PROGRESS`, `FINALIZED`, `CANCELED`
- `audit_questions`: Bearbeitungszustand ergibt sich aus `result` und dem Zustand des Audits
- `document_elements` und `questions`: Bestandteil einer konkreten Katalogversion
- `scope_types`: fest definierte Systemobjekte

## Produktgrundsatz

Für Auditarium gilt:

> **Fachliche Daten dürfen nach definierten Regeln gelöscht werden. Die Nachvollziehbarkeit relevanter Aktionen bleibt davon unabhängig erhalten.**

Zusätzlich gilt:

> **Außerbetriebnahme ist kein Löschen. Historisch relevante Objekte sollen erhalten bleiben, wenn nur ihre zukünftige Nutzung beendet werden soll.**


---

# 8. Fachliche Produktgrundlagen und Leitprinzipien

Dieses Kapitel definiert die fachlichen Leitplanken, die für die anschließenden Fachmodelle verbindlich sind. Die technische Basis und die zentralen Querschnittsfunktionen sind zu diesem Zeitpunkt bereits festgelegt; ab hier wird darauf der eigentliche Auditarium-Fachkern aufgebaut.

## Produktidentität und Markenauftritt

### Produktname

Der Produktname ist verbindlich festgelegt auf:

> **Auditarium**

Der Name ist ein Kunstwort aus `Audit` und dem lateinisch geprägten Suffix `-arium` und steht sinngemäß für einen Ort bzw. eine Sammlung rund um Audits.

### Sub-Titel

Der festgelegte Sub-Titel lautet:

> **Structured audits. Traceable results.**

Der Sub-Titel soll als prägnante englische Produktzeile unter dem Namen verwendet werden.

### Hero-/Startseiten-Konzept

Für die spätere Startseite ist ein prägendes visuelles Element vorgesehen.

Der Text:

> **Ort für**

bleibt statisch stehen.

Daneben bzw. darunter wechseln Begriffe in einer **Split-Flap-/Klapptafel-Animation**, angelehnt an klassische Flughafen- oder Bahnhofsanzeigen.

Vorgesehene Sequenz:

```text
Ort für Regelwerke
Ort für prüfbare Dokumentelemente
Ort für Fragen
Ort für Audits
Ort für Ergebnisse und Nachvollziehbarkeit
```

Die Begriffe werden nacheinander durchgeblättert.

Die letzte Anzeige:

> **Ort für Ergebnisse und Nachvollziehbarkeit**

bleibt stehen und bildet den inhaltlichen Abschluss der Animation.

#### Gestaltungsprinzip

Die Animation soll:

- präzise und technisch wirken,
- nicht verspielt oder dekorativ überladen sein,
- den Charakter eines strukturierten Audit-Werkzeugs unterstützen,
- als wiedererkennbares Markenelement von Auditarium dienen.

---

## Zielbild und fachlicher Kern

Die Anwendung soll strukturierte Audits gegen konkrete Dokumente bzw. Regelwerke ermöglichen.

Der ursprüngliche fachliche Ausgangspunkt sind der BSI IT-Grundschutz sowie Vorgaben und Dokumente im Umfeld von KRITIS und NIS2. Auditarium bleibt zugleich für Regelwerke aus anderen Fachbereichen nutzbar.

Die zugrunde liegende Annahme ist, dass ein Regelwerk Anforderungen in Form von Aussagen beschreibt. Auditarium unterstützt dabei, diese Aussagen in prüfbare Fragen zu überführen und darauf wiederholbare, nachvollziehbare und vergleichbare Audits aufzubauen. Fachbegriffe oder Strukturen eines einzelnen Regelwerks dürfen das generische Fachmodell nicht festlegen.

Im Mittelpunkt stehen dabei:

- nachvollziehbare und wiederholbare Audits,
- eine klare Trennung zwischen Regelwerk, prüfbaren Dokumentelementen, Prüffragen und konkreten Auditobjekten,
- eine generische Nutzbarkeit unabhängig von Branche oder Organisationstyp,
- strukturierte, auswertbare Ergebnisse,
- eine saubere Historie,
- eine spätere Berichtserstellung über Hierarchien von Auditobjekten hinweg,
- eine Importstrecke für extern strukturierte Regelwerke,
- eine Architektur, die spätere Erweiterungen erlaubt, ohne unnötig vorzugreifen.

Die Anwendung soll **keine CMDB**, **keine technische Abhängigkeitsdatenbank** und **kein vollständiges GRC-System** werden.

---

### Grundprinzip des Auditmodells

Ein Audit prüft immer:

> **genau eine konkrete Audit Unit gegen genau eine konkrete Dokument-/Regelwerkversion unter Verwendung einer beim Publish festgelegten `READY`-Katalogversion.**

Die relevanten Fragen werden beim Publish aus folgenden Informationen abgeleitet:

1. der gewählten `audit_unit`,
2. deren `scope_type`,
3. dem gewählten `document`,
4. der im `DRAFT` gewählten und beim Publish validierten `READY`-Katalogversion,
5. den darin enthaltenen Fragen und deren Scope-Zuordnungen.

Die grundsätzliche Reihenfolge lautet:

1. Regelwerke importieren.
2. Auditierbare Objekte anlegen und klassifizieren.
3. Regelwerk + Audit Unit auswählen.
4. Audit erzeugen.
5. Passende Fragen automatisch ermitteln.
6. Konkrete `audit_questions` anlegen.
7. Audit durchführen und finalisieren.

Mehrere einzelne Audits werden **nicht** künstlich zu Auditprojekten zusammengefasst. Über Reports kann später ausgewertet werden, welche Audit Units innerhalb einer Hierarchie gegen welche Regelwerke geprüft wurden und mit welchem Ergebnis.

## Produktgrenzen und Nicht-Ziele

Auditarium strukturiert, führt, dokumentiert und wertet Audits aus. Es zeigt nachvollziehbar, wo Handlungsbedarf bestehen kann, übernimmt aber keine vollständige Maßnahmensteuerung.

Auditarium ist ausdrücklich **nicht**:

```text
CMDB
technische Abhängigkeitsdatenbank
vollständiges GRC-System
Maßnahmenmanagement
allgemeine Workflow-Plattform
KI-/LLM-Laufzeit
```

Es gibt kein verpflichtendes mathematisches Gesamturteil wie `PASSED`, `FAILED`, eine Gesamt-Prozentzahl oder eine universelle Compliance-Ampel. Auditarium stellt Fakten, Einzelergebnisse, deterministisch ableitbare Elementergebnisse und Gewichtungen bereit; die fachliche Interpretation bleibt beim Menschen oder bei externen Auswertungssystemen.

Grundsatz:

> Keine Sackgassen: Alle relevanten Auditdaten müssen über Oberfläche, Datei-Export und API zugänglich sein.

### Open-Source-Grundsatz

Auditarium ist als öffentliches Open-Source-Projekt vorgesehen. Kernfunktionalität darf daher nicht von proprietären Laufzeitlizenzen oder nur kommerziell nutzbaren Bibliotheken abhängig gemacht werden. Drittanbieter-Abhängigkeiten müssen lizenzseitig zum Open-Source-Ziel des Projekts passen. Die konkrete Lizenz des Auditarium-Projekts wird separat festgelegt, sofern sie noch nicht entschieden ist.

## Fachliche Übersicht

```text
documents
└── catalog_versions [DRAFT ⇄ READY solange unbenutzt]
    └── document_elements
        ├── document_element_weights
        └── questions
            └── question_scope_types

scope_types
└── audit_units
    └── audit_units (Parent-Child)

audit [DRAFT]
├── audit_unit_id
├── catalog_version_id
└── audit_settings
        ↓ Publish
Validierung + Scope-Matching
        ↓
audit [READY]
├── audit_unit_context
└── audit_document_elements
    └── audit_questions

system_audit_log
└── Nachvollziehbarkeit relevanter Änderungen und Sicherheitsereignisse
```

Der fachliche Kern folgt den vier Hauptschritten:

```text
Import / Pflege des Katalogs
→ Audit konfigurieren
→ Audit durchführen
→ Auswertung / Export
```

---

# 9. Fachmodell: Dokumente, Kataloge und Fragen

## Dokumente und Regelwerke

### Grundsatz

Jede konkrete Version eines Regelwerks wird als eigenes `document` behandelt.

Ein Fragenkatalog gehört immer genau zu einer konkreten Dokumentversion.

Es gibt **keine regelwerkübergreifende Wiederverwendung oder Deduplizierung von Fragen**.

Dadurch bleibt jede App-Instanz frei darin, nur die für sie gewünschten Regelwerke zu enthalten.

### Tabelle `documents`

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `document_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `title` | `NOT NULL` | – | Titel des Dokuments / Regelwerks |
| `publisher` | `NULL` erlaubt | `NULL` | Herausgeber |
| `version` | `NULL` erlaubt | `NULL` | konkrete Version des Herausgebers |
| `publication_date` | `NULL` erlaubt | `NULL` | Veröffentlichungsdatum |
| `source` | `NULL` erlaubt | `NULL` | Quelle / URL / Herkunft |
| `usage_state` | `NOT NULL` | `ACTIVE` | lokale Nutzbarkeit: `ACTIVE` oder `DEPRECATED` |
| `usage_state_reason` | `NULL` erlaubt; bei `DEPRECATED` fachlich erforderlich | `NULL` | Begründung des aktuellen Nutzungszustands |
| `notes` | `NULL` erlaubt | `NULL` | optionale Hinweise |

Für optionale Textfelder gilt: Ist fachlich kein Wert vorhanden, wird `NULL` gespeichert und kein Leerstring.

`title` muss einen nicht-leeren Wert enthalten.

Für `usage_state` gilt zusätzlich:

```text
ACTIVE
→ usage_state_reason darf NULL sein

DEPRECATED
→ usage_state_reason muss einen nicht-leeren Wert enthalten
```

Aus fachlichen Metadaten wie `title`, `publisher` oder `version` wird kein natürlicher Unique Key gebildet. Die technische Identität eines Dokuments ist ausschließlich `document_id`.

Ein Hash oder eine Prüfsumme des Dokuments ist aktuell **nicht vorgesehen**.

### Bearbeitbarkeit der Dokumentmetadaten

Die beschreibenden Metadaten eines Dokuments bleiben durch berechtigte Benutzer bearbeitbar, auch wenn zugehörige Katalogversionen bereits `READY` sind oder von Audits verwendet werden.

Dies gilt gleichermaßen für:

```text
title
publisher
version
publication_date
source
notes
```

Für diese Felder wird weder eine selektive Bearbeitungssperre aufgrund der Verwendung des Dokuments noch ein besonderer Korrekturmodus eingeführt. Die normalen Regeln für Berechtigungen, Validierung, optimistische Concurrency und Änderungsprotokollierung gelten weiterhin.

Eine Metadatenänderung wird am bestehenden `document` gespeichert. Sie erzeugt weder eine neue Katalogversion noch einen neuen Dokumentdatensatz. Änderungen werden mit den relevanten Vorher-/Nachher-Werten im `system_audit_log` nachvollziehbar protokolliert.

Die Erfassung einer weiteren Herausgeber-Ausgabe als eigenes Dokument bleibt davon unberührt. `document.version` bezeichnet die Herausgeber-Version; `catalog_versions.version_number` bezeichnet den internen Katalogstand.

Auch bestehende Audits verwenden bei Anzeige, Export und API-Ausgabe die aktuellen Metadaten des referenzierten Dokuments. Diese Angaben werden weder pro Katalogversion noch pro Audit zusätzlich eingefroren oder als Snapshot gespeichert.

Grundsatz:

> **Dokumentmetadaten beschreiben das Regelwerk. Die verwendete Katalogversion bildet die unveränderliche fachliche Prüfgrundlage des Audits.**

Die Bearbeitbarkeit von `documents.title` ist damit ausdrücklich von der Unveränderlichkeit der Titel und Texte verwendeter `document_elements` zu unterscheiden. Ebenso ist die bearbeitbare Quellenangabe `documents.source` von der nach Freigabe geschützten Originaldatei der Katalogversion getrennt.

### Nutzungszustand eines Dokuments

Der lokale Nutzungszustand eines Dokuments ist von seiner Katalogversion und seinem Löschzustand getrennt.

Vorgesehene Werte:

```text
ACTIVE
DEPRECATED
```

#### `ACTIVE`

Das Dokument darf für neue Audits verwendet werden.

Bei einem neuen Audit kann die höchste `READY`-Katalogversion dieses Dokuments als Standard vorausgewählt werden. Solange das Audit `DRAFT` ist, bleibt die Auswahl veränderbar.

#### `DEPRECATED`

Das Dokument bleibt vollständig erhalten und historisch referenzierbar, darf aber nicht mehr für neue Audits verwendet werden.

Typische Gründe:

- das Regelwerk soll organisatorisch künftig nicht mehr auditiert werden,
- das Regelwerk besitzt für die konkrete Installation keine Relevanz mehr,
- ein fachlicher oder organisatorischer Beschluss setzt die weitere Nutzung aus.

Eine höhere Katalogversion ist **kein Grund für `DEPRECATED`**. `DEPRECATED` beschreibt ausschließlich die lokale Entscheidung, das Dokument künftig nicht mehr für neue Audits zu verwenden.

Der Wechsel zwischen `ACTIVE` und `DEPRECATED` ist reversibel.

Die fachliche Begründung des aktuellen Nutzungszustands wird in `usage_state_reason` gespeichert. Wer und wann den Zustand geändert hat, wird im `system_audit_log` nachvollzogen.


---

## Katalogversionen

### Warum Katalogversionen benötigt werden

Es wird unterschieden zwischen:

#### Herausgeber-Version

Die offizielle Version des Regelwerks:

```text
document.version
```

#### interne Katalogversion

Der interne Stand der operationalisierten Dokumentelemente, Fragen und Hinweise:

```text
catalog_version
```

Die interne Katalogversion ist unabhängig davon, ob ihre Inhalte:

- vollständig manuell erfasst,
- aus einem extern erzeugten Importpaket importiert,
- oder aus einer Kombination beider Wege erzeugt wurden.

### Tabelle `catalog_versions`

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `catalog_version_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `document_id` | `NOT NULL` | – | Referenz auf das zugehörige Dokument |
| `version_number` | `NOT NULL` | durch Anwendung fortlaufend vergeben | interne Nummer innerhalb des Dokuments |
| `catalog_state` | `NOT NULL` | `DRAFT` | fachlicher Zustand: `DRAFT` oder `READY` |
| `draft_revision` | `NOT NULL` | `1` | technische Revision des DRAFTs für kontrollierte Exporte und Update-Importe |
| `created_at` | `NOT NULL` | Erstellungszeitpunkt | Zeitpunkt der Erstellung |
| `created_by` | `NOT NULL` | – | Benutzer, der die Katalogversion erzeugt hat |
| `notes` | `NULL` erlaubt | `NULL` | optionale Hinweise |
| `source_file_id` | `NULL` erlaubt | `NULL` | optionale Referenz auf das Originaldokument (`FileItem`) |

Die Kombination

```text
(document_id, version_number)
```

muss eindeutig sein.

`version_number` beginnt für jedes Dokument bei `1` und wird innerhalb dieses Dokuments fortlaufend vergeben.

Die Werte müssen positiv sein:

```text
version_number >= 1
draft_revision >= 1
```


`draft_revision` beginnt bei `1` und wird bei relevanten Änderungen am DRAFT erhöht. Importbezogene technische Informationen gehören nicht in `catalog_versions`; für v1 ist kein eigenes persistentes Import-Aggregat vorgeschrieben.

Für die Katalogversion gilt:

```text
Fachtabelle
→ aktueller Zustand + optionale notes

system_audit_log
→ vollständige Historie aller Zustandswechsel und Änderungen
```


---

## Dokumentelemente und Dokumentstruktur

### Grundsatz

Dokumente und Regelwerke können sehr unterschiedlich strukturiert sein, beispielsweise als Kapitel und Unterkapitel, Abschnitte, Paragraphen, Absätze oder rein erklärende Textteile.

Auditarium schreibt deshalb keine feste Dokumenthierarchie vor. Die Struktur wird über frei verschachtelbare `document_elements` abgebildet. Der Benutzer entscheidet selbst, wie viel der Originalstruktur eines Regelwerks er erfassen möchte.

### Hierarchisches Modell

Jedes Dokumentelement gehört genau zu einer `catalog_version` und kann optional ein anderes Dokumentelement derselben Katalogversion als Parent besitzen.

```text
document
└── catalog_version
    ├── document_element
│   ├── document_element
│   │   └── document_element
│   └── document_element
└── document_element
```

Für Elemente auf oberster Ebene gilt:

```text
parent_element_id = NULL
```

Ein Parent darf nur auf ein Element mit derselben `catalog_version_id` verweisen.

Ein Audit speichert keinen redundanten `document_id`. Die Herkunft wird eindeutig über

```text
audit
→ catalog_version
→ document
```

bestimmt.


Circular References sind verboten. Ein Element darf weder sich selbst noch einen seiner Nachfahren als Parent erhalten.

### Tabelle `document_elements`

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `element_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `catalog_version_id` | `NOT NULL` | – | Referenz auf die konkrete Katalogversion |
| `parent_element_id` | `NULL` erlaubt | `NULL` | Referenz auf das übergeordnete Element; `NULL` = oberste Ebene |
| `title` | `NULL` erlaubt; konditional erforderlich | `NULL` | Bezeichnung zur menschlichen Einordnung |
| `text` | `NULL` erlaubt; konditional erforderlich | `NULL` | Original-, Kontext- oder Aussage-/Anforderungstext |
| `sort_order` | `NOT NULL` | durch Anwendung vergeben | Reihenfolge innerhalb desselben Parents |
| `notes` | `NULL` erlaubt | `NULL` | optionale interne Hinweise |

Für `title` und `text` gelten die bei den fachlichen Rollen der Dokumentelemente beschriebenen konditionalen Pflichtregeln. Ein vollständig inhaltsloses Element ist unzulässig.

### Eigentümerschaft und Versionsgrenze

`document_elements` gehören fachlich und technisch zu einer konkreten `catalog_version`.

Die Beziehung lautet:

```text
document_element
    ↓
catalog_version
    ↓
document
```

Ein direktes `document_id` am Dokumentelement wird nicht benötigt.

Dadurch ist jede Dokumentstruktur eindeutig an genau eine Katalogversion gebunden.

Bei einer neuen Katalogversion entsteht ein eigener Satz Dokumentelemente mit eigenen IDs.

Parent-Beziehungen dürfen niemals Katalogversionsgrenzen überschreiten.

### Fachliche Rolle eines Dokumentelements

Die fachliche Rolle eines Elements wird **nicht als eigenes Feld gespeichert**. Sie wird aus den vorhandenen Inhalten und Beziehungen abgeleitet.

#### Prüffähiges Dokumentelement

```text
mindestens eine Frage vorhanden
```

Dann ist das Dokumentelement eine prüfbare Aussage bzw. Anforderung.

Sobald Fragen an einem Element hängen, muss dieses Element einen `text` besitzen.

#### Kontext-Element

```text
keine Fragen
aber text vorhanden
```

Dann dient das Element als einleitender, erklärender oder ergänzender Kontext.

#### Ordnungs-Element

```text
keine Fragen
kein text
```

Dann ist das Element rein strukturierend und enthält typischerweise nur einen Titel.

Diese Einordnung wird programmgesteuert abgeleitet und muss vom Benutzer nicht separat gepflegt werden.


#### Gültigkeitsregeln für `title` und `text`

`title` dient ausschließlich der menschlichen Orientierung.

Es kann beispielsweise enthalten:

```text
§ 12 Abs. 3
3.1.4 Netzwerksegmentierung
Technische Anforderungen
```

Auditarium unterscheidet nicht zwischen Nummerierung, Referenz und Überschrift.

Es gelten folgende Mindestregeln:

```text
Element mit mindestens einer Frage
→ text erforderlich
→ title optional

Element mit text, aber ohne Fragen
→ title optional

Element ohne text und ohne Fragen
→ title erforderlich
```

Unzulässig ist damit ein vollständig inhaltsloses Element:

```text
title IS NULL
text IS NULL
keine Fragen
```


### Reihenfolge

`sort_order` bestimmt die Reihenfolge der Elemente innerhalb desselben Parents.

Auch Root-Elemente mit `parent_element_id = NULL` besitzen untereinander eine definierte Reihenfolge.

Die konkrete technische Nummerierungsstrategie wird nicht fachlich festgeschrieben. Die UI soll die Reihenfolge komfortabel änderbar machen, z. B. per Drag & Drop oder Verschieben nach oben/unten.

### Gewichtung prüfbarer Dokumentelemente

Die Gewichtung erfolgt ausschließlich auf Ebene eines prüfbaren Dokumentelements, also eines Elements, dem mindestens eine Frage zugeordnet ist.

Eine Gewichtung einzelner Fragen ist ausdrücklich nicht vorgesehen.

| Gewicht | Bedeutung |
|---|---|
| `1` | deutlich geringere Priorität |
| `2` | geringere Priorität |
| `3` | neutral / Standard |
| `4` | höhere Priorität |
| `5` | höchste Priorität |

Der Defaultwert ist `3`.

Für den Defaultwert wird kein eigener Datensatz in `document_element_weights` benötigt.

Es gilt:

```text
kein Eintrag in document_element_weights
→ effektives Gewicht = 3

Eintrag vorhanden
→ effektives Gewicht = gespeicherter Wert
```

Ein Datensatz wird damit nur für eine explizite Abweichung vom Default gespeichert.

Wird ein zuvor abweichendes Gewicht wieder auf `3` gesetzt, kann der Datensatz aus `document_element_weights` entfernt werden.


Die Gewichtung ist keine Aussage über Erfüllung, Kritikalität oder Risiko. Sie dient als lokale fachliche Priorisierung und insbesondere zur Sortierung und Hervorhebung in Auswertungen.

#### Tabelle `document_element_weights`

Die lokale Gewichtung wird technisch getrennt vom unveränderlichen Kataloginhalt gespeichert.

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `element_id` | `NOT NULL` | – | Primärschlüssel und Referenz auf das prüfbare Dokumentelement |
| `weight` | `NOT NULL` | kein gespeicherter Default | explizites Gewicht `1`, `2`, `4` oder `5` |

Die Gewichtung darf verändert werden, ohne den Kataloginhalt selbst zu verändern.

Ein Datensatz in `document_element_weights` darf nur für eine explizite Abweichung vom effektiven Default `3` existieren.

```text
kein Datensatz
→ effektives Gewicht = 3

Datensatz vorhanden
→ weight IN (1, 2, 4, 5)
```

Ein gespeicherter Wert `3` ist unzulässig; das Zurücksetzen auf `3` entfernt den Datensatz.


`document_element_weights` speichert ausschließlich den aktuellen fachlichen Wert:

```text
element_id
weight
```

Zeitpunkt, Benutzer und Historie einer Gewichtungsänderung werden nicht redundant in dieser Tabelle geführt, sondern im `system_audit_log`.

Eine eigene fachliche Änderungsbegründung an der Gewichtung ist derzeit nicht vorgesehen.


Beim Publish eines Audits wird für jedes materialisierte `audit_document_element` der zu diesem Zeitpunkt effektive Wert als `weight_snapshot` gespeichert.

Beispiele:

```text
kein Eintrag in document_element_weights
→ weight_snapshot = 3

gespeichertes Gewicht = 5
→ weight_snapshot = 5
```


---

## Fragen

Fragen besitzen keinen eigenen Status.

Ihr Lebenszyklus ergibt sich vollständig aus der zugehörigen Katalogversion:

```text
Katalogversion = DRAFT
→ Fragen anlegen, ändern, löschen

Katalogversion = READY
→ Fragen sind Bestandteil des freigegebenen Katalogs
```

Ein zusätzlicher Fragenstatus wird nicht geführt.

### Tabelle `questions`

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `question_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `element_id` | `NOT NULL` | – | Referenz auf das zugehörige prüfbare Dokumentelement |
| `sort_order` | `NOT NULL` | durch Anwendung vergeben | Reihenfolge innerhalb des zugehörigen Dokumentelements |
| `text` | `NOT NULL` | – | eigentliche Prüffrage / Prüfaussage |
| `verification_hint` | `NULL` erlaubt | `NULL` | optionaler Hinweis zur Prüfung |
| `evidence_hint` | `NULL` erlaubt | `NULL` | optionaler Hinweis auf geeignete Nachweise |
| `notes` | `NULL` erlaubt | `NULL` | optionale interne Hinweise |

`text` muss einen nicht-leeren Wert enthalten.

Ein separates Feld `title` für Fragen wird **nicht** verwendet.

Die Zugehörigkeit zum prüfbaren Dokumentelement ist über `element_id` eindeutig ableitbar.

Die Oberfläche kann bei Bedarf automatisch anzeigen:

- den Titel des zugehörigen Dokumentelements,
- „Frage x von n“,
- einen Hinweis darauf, dass mehrere Fragen demselben prüfbaren Dokumentelement zugeordnet sind.

### Regeln für Fragen

- Eine Frage gehört genau zu einem prüfbaren Dokumentelement.
- Eine Frage prüft genau einen Sachverhalt.
- Fragen werden positiv formuliert.
- `JA` bedeutet immer, dass der gewünschte Zustand erfüllt ist.
- Die Fragen eines importierten und `READY` gesetzten Katalogstands werden nicht nachträglich strukturell verändert.
- Änderungen an Anzahl oder Struktur der Fragen erzeugen eine neue Katalogversion.

---

## Scope Types

### Grundsatz

Scope Types werden fest durch die Anwendung vorgegeben.

Scope Types besitzen stabile technische IDs bzw. Keys. Diese Werte sind `NOT NULL`, nicht benutzerdefiniert und werden durch die Anwendung bzw. deren Seed-/Migrationslogik bereitgestellt. Benutzer können Scope Types weder anlegen noch löschen.


Sie sind **keine frei pflegbaren Stammdaten**, damit:

- Fragenkataloge reproduzierbar bleiben,
- externe Importwerkzeuge immer dieselben Begriffe verwenden,
- unterschiedliche App-Instanzen vergleichbar bleiben,
- keine Synonyme und uneinheitlichen Eigenkreationen entstehen.

Die Definitionen müssen so präzise sein, dass sowohl Menschen als auch externe Importwerkzeuge Auditfragen konsistent zuordnen können.

### Feste Scope Types

| Key | Name | Definition / Zuordnungsregel |
|---|---|---|
| `ORGANIZATION` | Organisation | Rechtlich oder organisatorisch abgegrenzte Gesamteinheit oder wesentliche Organisationseinheit. Typischer oberster Scope. Beispiele: Unternehmen, Behörde, Tochtergesellschaft, Geschäftsbereich. Nicht für einzelne Standorte oder Abteilungen verwenden, wenn diese besser als Standort bzw. Bereich modelliert werden können. |
| `SITE` | Standort | Räumlich zusammenhängende Betriebsstätte an einem geografisch bestimmbaren Ort. Kann Gebäude, Bereiche und Räume enthalten. Beispiele: Werk, Niederlassung, Krankenhausstandort, Bürostandort. |
| `BUILDING` | Gebäude | Einzelnes baulich abgegrenztes Gebäude innerhalb oder außerhalb eines Standortes. Für Anforderungen verwenden, die das Gebäude als Ganzes betreffen. |
| `AREA` | Bereich | Organisatorisch oder funktional abgegrenzter Teil einer Organisation oder eines Standortes, der nicht sinnvoll als einzelner Raum, Gebäude oder eigenes technisches System beschrieben wird. Beispiele: Verwaltung, Produktion, Callcenter, Buchhaltung, Logistikbereich. |
| `ROOM` | Raum | Einzelner räumlich abgegrenzter Raum ohne primär technische Zweckbestimmung. Beispiele: Büro, Archiv, Besprechungsraum, Lagerraum. Für technisch dominierte Räume ist `TECHNICAL_AREA` zu bevorzugen. |
| `TECHNICAL_AREA` | Technikbereich | Räumlich oder funktional abgegrenzter Bereich, dessen Hauptzweck der Betrieb technischer Infrastruktur ist. Beispiele: Serverraum, Rechenzentrumsraum, Netzwerkverteilerraum, Elektro-/Versorgungsraum. |
| `NETWORK` | Netzwerk | Logisch oder technisch abgegrenzte Kommunikationsinfrastruktur. Beispiele: LAN, WAN, WLAN, Managementnetz, OT-Netz, Netzwerksegment oder Netzwerkverbund. Nicht für einzelne Netzwerkgeräte verwenden. |
| `IT_SYSTEM` | IT-System | Konkretes technisches System oder technische Plattform, auf der Daten verarbeitet, gespeichert oder übertragen werden. Beispiele: Server, Cluster, Storage-System, Firewall, Virtualisierungsplattform, Endgerätegruppe. Nicht für die darauf betriebene fachliche Software verwenden. |
| `APPLICATION` | Anwendung | Softwareanwendung oder fachlich nutzbares Softwaresystem unabhängig von der darunterliegenden technischen Plattform. Beispiele: ERP, Krankenhausinformationssystem, CRM, Webanwendung. |
| `PROCESS` | Prozess | Definierter organisatorischer oder technischer Ablauf mit erkennbarem Zweck und mehreren Schritten bzw. Aktivitäten. Beispiele: Benutzer-Onboarding, Patchmanagement, Backup-Prozess, Incident Management. |
| `SERVICE` | Dienst / Service | Bereitgestellte technische oder organisatorische Leistung, deren Betrachtung unabhängig von einer konkreten Implementierung sinnvoll ist. Beispiele: DNS-Dienst, E-Mail-Service, Internetzugang, zentraler Backup-Service, Managed Service. |
| `EXTERNAL_PROVIDER` | Externer Dienstleister | Externe Organisation oder Vertragspartner, der Leistungen für die betrachtete Organisation erbringt oder Zugriff auf deren Informationen/Systeme besitzt. Beispiele: IT-Dienstleister, Wartungsfirma, Cloud-Anbieter. |
| `OTHER` | Sonstiges | Nur verwenden, wenn keiner der definierten Typen sachgerecht passt. Die konkrete Bedeutung muss beschrieben werden. Bei automatischer Zuordnung durch externe Werkzeuge als Ausnahme behandeln und zur manuellen Prüfung markieren. |

### Zuordnung Fragen ↔ Scope Types

Eine Frage darf mehreren Scope Types zugeordnet sein.

Dafür wird die n:m-Zuordnungstabelle `question_scope_types` verwendet:

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `question_id` | `NOT NULL` | – | Referenz auf die Frage |
| `scope_type_id` | `NOT NULL` | – | Referenz auf den fest definierten Scope Type |

Im `DRAFT` darf eine Frage 0..n Scope-Zuordnungen besitzen.

Für den Übergang einer Katalogversion auf `READY` muss jede Frage mindestens einem gültigen Scope Type zugeordnet sein.

Die Kombination:

```text
(question_id, scope_type_id)
```

muss eindeutig sein.

`question_scope_types` besitzt keinen eigenen künstlichen Primärschlüssel. Die Kombination aus

```text
(question_id, scope_type_id)
```

bildet den eindeutigen Schlüssel der Zuordnung. Beide Werte sind verpflichtend.


Weitere Herkunfts-Fremdschlüssel werden nicht gespeichert. Katalogversion und Dokument sind über `question_id → element_id → catalog_version_id → document_id` eindeutig ableitbar.

Scope-Zuordnungen sind Bestandteil des Katalogs und werden nicht zusätzlich in ein Audit kopiert.

Beim Publish eines Audits erfolgt die Filterung über:

```text
audit_unit.scope_type
        ↓
question_scope_types
        ↓
passende questions
```

Nur Fragen mit einer passenden Scope-Zuordnung werden als `audit_questions` materialisiert.

---

## Änderung eines Fragenkatalogs

### Korrektur

Korrekturen sind beispielsweise:

- Schreibfehler,
- verständlichere Formulierung einer Frage,
- Verbesserung von `verification_hint`,
- Verbesserung von `evidence_hint`,
- Korrektur von Notizen,
- Korrektur einer fehlerhaften Scope-Zuordnung.

Die fachliche Menge und Struktur der Fragen bleibt gleich.

Im `DRAFT` können solche Korrekturen direkt vorgenommen werden. Eine noch unbenutzte `READY`-Katalogversion muss dafür zunächst nach den bestehenden Regeln auf `DRAFT` zurückgesetzt werden.

Sobald eine Katalogversion von einem Audit referenziert wird, erfordert auch eine rein redaktionelle Korrektur eine neue Katalogversion. Dafür wird der vorhandene Stand als neue `DRAFT`-Kopie übernommen; die verwendete Version bleibt unverändert.

Diese Regeln betreffen die Inhalte der Katalogversion. Für die Metadaten des übergeordneten Dokuments gilt die separate Bearbeitbarkeitsregel.

### Strukturelle Änderung

Bei einer bereits verwendeten Katalogversion ist eine neue Katalogversion zwingend, wenn:

- Fragen hinzugefügt werden,
- Fragen entfernt werden,
- eine Frage aufgeteilt wird,
- mehrere Fragen zusammengeführt werden,
- der Prüfumfang strukturell verändert wird.

Diese Versionierung ist unabhängig von der Version des Herausgebers.

---

## Auswahl der Katalogversion für Audits

Im Zustand `DRAFT` ist `catalog_version_id` Bestandteil der veränderbaren Auditkonfiguration.

Für ein neu angelegtes Audit kann Auditarium standardmäßig die höchste `READY`-Katalogversion des gewählten `ACTIVE`-Dokuments vorauswählen.

Diese Vorauswahl ist keine unveränderliche Bindung.

Solange das Audit `DRAFT` ist, darf der `AUDIT_MANAGER` eine andere zulässige Katalogversion auswählen oder einen aus einem Vorlagen-Audit übernommenen Wert beibehalten.

Ein zusätzlicher Modus wie `LATEST_READY` oder `FIXED` wird nicht benötigt.

Beim Publish wird geprüft:

- die Katalogversion befindet sich im Zustand `READY`,
- das über die Katalogversion referenzierte Dokument ist für das neue Audit zulässig.

Erst mit erfolgreichem Publish wird `catalog_version_id` für dieses Audit unveränderlich.

## Lebenszyklus einer Katalogversion

Der fachliche Lebenszyklus einer Katalogversion ist bewusst unabhängig vom Befüllungsweg.

```text
DRAFT
  ↓
READY
```

### `DRAFT`

Die Katalogversion befindet sich in fachlicher Bearbeitung.

In diesem Zustand dürfen insbesondere:

- Dokumentelemente angelegt und bearbeitet werden,
- Fragen angelegt und bearbeitet werden,
- Scope Types zugeordnet und geändert werden,
- Prüf- und Nachweishinweise gepflegt werden,
- Gewichtungen gepflegt werden,
- importierte Inhalte manuell geprüft und korrigiert werden.

`DRAFT`-Katalogversionen dürfen nicht für neue Audits verwendet werden.

### `READY`

Die Katalogversion wurde fachlich geprüft und freigegeben.

Ab diesem Zeitpunkt sind die strukturellen Kataloginhalte unveränderlich:

- Dokumentelemente einschließlich Gliederung, Reihenfolge, Titel, Text und inhaltlicher Notizen,
- Fragen einschließlich Formulierung, Reihenfolge und inhaltlicher Notizen,
- Scope-Zuordnungen,
- Prüfhinweise,
- Nachweishinweise.

Der Schutz des freigegebenen Katalogstands sichert die verbindliche Prüfgrundlage gegenüber dem Auftraggeber des Audits. Nach Verwendung gibt es dafür keine Ausnahme für Schreibfehler oder andere redaktionelle Änderungen.

Die lokale Gewichtung eines prüfbaren Dokumentelements bleibt hiervon getrennt und darf durch den `AUDIT_MANAGER` weiterhin angepasst werden.

`READY` ist für unbenutzte Katalogversionen reversibel.

Solange noch kein Audit auf die konkrete Katalogversion verweist, darf ein `AUDIT_MANAGER` sie wieder auf `DRAFT` setzen.

Sobald mindestens ein Audit die Katalogversion referenziert, ist sie historisch eingefroren. Änderungen erfolgen dann ausschließlich über eine neue Katalogversion.

---

## Manueller Katalog-Workflow und DRAFT-Editor

### Grundsatz

Auditarium muss einen vollständigen Fragenkatalog vollständig manuell erstellen und pflegen können.

Der manuelle Workflow ist der fachliche Referenzprozess. Ein strukturierter Import ist lediglich ein alternativer Weg, denselben DRAFT-Zustand vorzubefüllen.

> Der darauf aufbauende strukturierte Import ist in dem Kapitel **„Strukturierter Katalogimport“** beschrieben. Der manuelle Workflow in diesem Abschnitt ist die fachliche Referenz, aus der Importformat und Hilfsprompt abgeleitet werden.

### Workflow

```text
1. Dokument anlegen
      ↓
2. neue Katalogversion als DRAFT erzeugen
      ↓
3. Dokumentstruktur / document_elements erfassen
      ↓
4. prüfbare Elemente mit Fragen versehen
      ↓
5. Scope Types an Fragen zuordnen
      ↓
6. Hinweise und Gewichtungen pflegen
      ↓
7. formale Validierung durchführen
      ↓
8. Vorschau / fachliche Kontrolle
      ↓
9. Katalogversion auf READY setzen
```

### DRAFT-Editor

Der DRAFT-Editor ist der zentrale fachliche Arbeitsplatz für eine Katalogversion.

```text
Dokument
└── Katalogversion [DRAFT]
    └── document_elements
        ├── optionaler Titel
        ├── optionaler Text
        ├── sort_order
        ├── optional weitere document_elements
        ├── Gewichtung  [1 ──●── 5]
        └── Fragen
            ├── Fragetext
            ├── Prüfungshinweis
            ├── Nachweishinweis
            └── Scope Types
```

### Bearbeitungsregeln im DRAFT

Im Zustand `DRAFT` dürfen Dokumentelemente und Fragen frei angelegt, bearbeitet, verschoben und gelöscht werden. Scope-Zuordnungen, Hinweise, Reihenfolgen und Gewichtungen dürfen ebenfalls verändert werden.

Eine eigene Duplizierfunktion wird zunächst **nicht** angeboten.

#### Verschieben

Beim Verschieben eines Elements muss die Anwendung mindestens prüfen:

- Parent und Child gehören zur selben `catalog_version`,
- das Element wird nicht unter sich selbst verschoben,
- das Element wird nicht unter einen eigenen Nachfahren verschoben,
- es entsteht keine Circular Reference.

#### Löschen

Löschungen innerhalb eines `DRAFT` sind zulässig.

Besitzt ein zu löschendes Element abhängige Inhalte, muss Auditarium vor der Ausführung eindeutig anzeigen, welche Daten ebenfalls gelöscht werden.

Beispiel:

```text
betroffenen Teilbaum löschen

Betroffen:
- 4 untergeordnete Dokumentelemente
- 7 Fragen
- 11 Scope-Zuordnungen
```

Nach ausdrücklicher Bestätigung darf die abhängige Struktur kaskadierend gelöscht werden.

### `sort_order`

`sort_order` gilt immer innerhalb desselben Parents.

Wird ein Element verschoben, neu einsortiert, eingefügt oder gelöscht, muss Auditarium die Reihenfolge der betroffenen Geschwister konsistent halten.

Die konkrete technische Nummerierung bleibt Implementierungsdetail.

### Zulässige Zwischenzustände im DRAFT

Ein `DRAFT` darf bewusst fachlich unvollständig sein.

Beispiele:

- Dokumentelement mit Text, aber noch ohne Fragen,
- Frage ohne Scope-Zuordnung,
- noch nicht vollständig gepflegte Hinweise,
- noch nicht endgültig festgelegte Gewichtungen.

Solche Zustände sind während der Bearbeitung erlaubt.

### Formale Mindestbedingungen für `READY`

Auditarium prüft vor dem Übergang zu `READY` nur Bedingungen, die deterministisch feststellbar sind.

Mindestens gilt:

- keine Circular References,
- alle Parent-Beziehungen bleiben innerhalb derselben Katalogversion,
- jede Frage verweist auf ein vorhandenes Dokumentelement,
- jedes Dokumentelement mit Fragen besitzt einen `text`,
- jede Frage besitzt mindestens eine gültige Scope-Zuordnung,
- alle referenzierten Scope Types existieren,
- die hierarchische Reihenfolge ist konsistent,
- alle sonstigen harten Datenmodell- und Validierungsregeln sind erfüllt.

Auditarium bewertet nicht, ob wirklich alle relevanten prüfbaren Dokumentelemente erfasst wurden, genügend Fragen vorhanden sind oder die fachliche Ausgestaltung optimal ist.

> Auditarium prüft formale Konsistenz, nicht die fachliche Vollständigkeit des vom Menschen erstellten Katalogs.

### Rolle der Gewichtung im Editor

Die lokale Gewichtung prüfbarer Dokumentelemente wird im DRAFT-Editor direkt am jeweiligen Element gepflegt.

Default: `3`, Wertebereich: `1..5`.

Die Gewichtung bleibt technisch vom unveränderlichen Kataloginhalt getrennt und darf auch später durch den `AUDIT_MANAGER` angepasst werden. Beim Publish eines Audits wird die zu diesem Zeitpunkt effektive Gewichtung als `weight_snapshot` übernommen.

### Übergang `DRAFT → READY`

Der Übergang wird bewusst durch einen `AUDIT_MANAGER` ausgelöst.

Voraussetzung ist, dass alle formalen Mindestbedingungen erfüllt sind.

Auditarium darf den Übergang nicht aufgrund einer eigenen fachlichen Bewertung verhindern, wenn die formalen Regeln erfüllt sind.

Die Freigabe darf jedoch nicht durch einen einfachen unbestätigten Klick erfolgen.

Vor dem Zustandswechsel zeigt Auditarium einen Bestätigungsdialog mit einem klaren Hinweis, beispielsweise:

> Diese Katalogversion wird für neue Audits freigegeben. Nach erstmaliger Verwendung kann ihr fachlicher Inhalt nicht mehr verändert werden.

Der `AUDIT_MANAGER` muss diese Konsequenz ausdrücklich bestätigen.

Eine GitHub-artige Texteingabe wie bei irreversiblen Audit-Finalisierungen ist hierfür nicht zwingend erforderlich; eine bewusste Bestätigung in einem separaten Dialog ist ausreichend.

Beim Übergang auf `READY` werden die allgemeinen Zustandsmetadaten aktualisiert:

```text
catalog_state
```

Der Zustandswechsel wird vollständig im `system_audit_log` protokolliert.

### Rückkehr `READY → DRAFT`

Ein Wechsel von `READY` zurück zu `DRAFT` ist zulässig, solange **kein Audit auf diese konkrete `catalog_version_id` verweist**.

Der Rückwechsel wird ebenfalls bewusst durch einen `AUDIT_MANAGER` ausgelöst und im `system_audit_log` protokolliert.

```text
DRAFT ⇄ READY
```

gilt damit nur für noch unbenutzte Katalogversionen.

Sobald mindestens ein Audit auf diese Katalogversion verweist:

```text
READY ─X→ DRAFT
```

wird die Rückkehr gesperrt.

Auditarium zeigt dem `AUDIT_MANAGER` den Grund an und bietet stattdessen an:

> **Neue Katalogversion auf Basis dieses Standes erstellen**


### Neue Katalogversion als vollständige DRAFT-Kopie

Wird eine bereits verwendete `READY`-Katalogversion weiterentwickelt oder korrigiert, kann Auditarium eine neue Katalogversion als vollständige Kopie erzeugen.

Als Ausgangsbasis werden übernommen:

- komplette `document_elements`-Hierarchie,
- Fragen,
- Scope-Zuordnungen,
- Prüfhinweise,
- Nachweishinweise,
- Notizen,
- aktuelle Gewichtungen als Ausgangswerte.

Die kopierten `document_elements`, Fragen und Zuordnungen erhalten neue IDs und gehören ausschließlich zur neuen Katalogversion.

Die neue Katalogversion erhält:

```text
version_number = höchste vorhandene Katalogversion + 1
catalog_state  = DRAFT
```

Danach kann der `AUDIT_MANAGER` sie frei bearbeiten.

Eine neue **Dokumentversion** wird dadurch ausdrücklich **nicht** erzeugt. `document.version` bleibt ausschließlich der Version des Herausgebers vorbehalten.

### Einheitlicher Editor für manuelle und importierte Inhalte

Unabhängig von der Herkunft der Daten wird derselbe Editor verwendet.

```text
                    ┌─ manuelle Erfassung
                    │
Dokument/Katalog ───┤
                    │
                    └─ strukturierter Import
                             ↓
                       DRAFT-Editor
                             ↓
                    fachliche Prüfung
                             ↓
                           READY
```

### Leitgedanke

> **Im DRAFT darf gebaut werden. READY bedeutet formal konsistent. Ein verwendetes READY ist historisch eingefroren.**

---

# 10. Dateiablage und File Abstraction Layer

## Dateiablage und FAL

Auditarium speichert dauerhafte Dateiinhalte nicht als BLOB in der relationalen Datenbank.

Stattdessen gilt:

```text
Dateimetadaten
→ Datenbank

Dateiinhalt
→ File Storage über FAL
```

Für die erste Version ist genau ein dauerhafter Datei-Use-Case vorgesehen:

> Zu einer `catalog_version` kann optional das unveränderte Originaldokument gespeichert werden.

## Storage-Ziel

Die erste FAL-Implementierung verwendet einen vom Auditarium-Server erreichbaren Dateisystempfad.

Mögliche Ziele:

```text
lokales Verzeichnis
Netzwerk-Share
eingebundenes Dateisystem
```

Der Storage-Root ist konfigurierbar.

Beispiele:

```text
/var/lib/auditarium/files
/mnt/auditarium
\\fileserver\auditarium
```

In der Datenbank werden keine absoluten physischen Pfade gespeichert.

## `FileItem`

Dateimetadaten werden in einem `FileItem` gespeichert.

Konzeptionell mindestens:

```text
FileId
OriginalFileName
SaveFileName
Extension
SaveFilePath
ContentType
Size
CreatedAt
CreatedBy
Checksum
```

`SaveFilePath` ist immer relativ zum konfigurierten Storage-Root.

Beispiel:

```text
OriginalFileName = BSI_NET_1.1_v3.pdf
SaveFileName     = 7f4e42cb8d34411789c84166cb568887
Extension        = .pdf
SaveFilePath     = documents/originals
Size             = 18472631
```

## Physische Dateinamen

Physische Dateien werden nicht unter ihrem Originalnamen gespeichert.

Beim Upload:

```text
1. technische GUID erzeugen
2. Datei exklusiv unter diesem Namen anlegen
3. bei Kollision neue GUID erzeugen
4. Datei ohne Originalname und ohne Extension speichern
```

Die exklusive Dateierzeugung verhindert eine theoretische Race Condition zwischen Existenzprüfung und Schreiben.

Originaldateiname und Extension bleiben reine Metadaten.

## Verknüpfung mit `catalog_version`

Eine `catalog_version` kann maximal ein Originaldokument referenzieren.

Konzeptionell:

```text
catalog_versions.source_file_id NULL
```

Semantik:

```text
NULL
→ kein Originaldokument hinterlegt

FileId
→ Originaldokument vorhanden
```

Das Originaldokument gehört fachlich zum Stand der jeweiligen Katalogversion.

## Lifecycle des Originaldokuments

Solange eine `catalog_version` den Zustand `DRAFT` besitzt, darf das Originaldokument:

```text
hinzugefügt
ersetzt
entfernt
```

werden.

Mit Übergang zu:

```text
READY
```

wird auch die Dateireferenz historisch eingefroren.

Für `READY` gilt:

```text
Originaldokument
→ nicht ersetzen
→ nicht entfernen
```

Damit bleibt der historische Katalogstand einschließlich seiner optionalen Quelldatei unveränderlich.

## Sicheres Ersetzen einer Datei

Beim Ersetzen eines Originaldokuments wird die alte physische Datei nicht vorzeitig gelöscht.

Ablauf:

```text
1. neuen Upload vollständig empfangen
2. Größe und Dateityp validieren
3. neue Datei über FAL unter neuer Storage-ID speichern
4. neuen FileItem erzeugen
5. catalog_version auf neuen FileItem umstellen
6. DB-Änderung erfolgreich committen
7. alte physische Datei über FAL löschen
8. alten FileItem entfernen bzw. entsprechend bereinigen
```

Grundsatz:

> Die alte physische Datei wird erst aufgegeben, wenn die neue Datei sicher gespeichert und die neue Referenz erfolgreich persistiert ist.

Scheitert das Löschen der alten physischen Datei danach, ist dies ein technischer Cleanup-Fehler.

Die gültige neue Referenz bleibt bestehen.

## Upload-Validierung

Uploads werden mindestens auf folgende Eigenschaften geprüft:

```text
Dateigröße
Extension
Content-Type
Dateisignatur / Magic Header
```

Ein vom Client gelieferter Dateiname oder MIME-Type wird nicht blind vertraut.

Für v1 wird PDF als primär zugelassener Dokumenttyp vorgesehen.

Weitere unveränderliche Formate können später explizit ergänzt werden.

## Größenlimits

Upload- und Download-Größenlimits sind konfigurierbar. Für Originaldokumente gelten initial jeweils **100 MiB**. Die nicht UI-editierbaren Betreiber-Settings `Files:OriginalDocuments:MaxUploadSize` und `Files:OriginalDocuments:MaxDownloadSize` enthalten die jeweiligen Bytewerte; sie werden über Konfiguration oder Environment gesetzt.

Sie werden so gewählt, dass auch große PDF-Regelwerke akzeptiert werden können.

Die Limits müssen mit:

```text
ASP.NET / Kestrel
Reverse Proxy
FAL
```

konsistent abgestimmt werden.

Kleine Web-Framework-Defaults dürfen nicht unbeabsichtigt große legitime Regelwerke blockieren.

## Integritätsprüfung

Für gespeicherte Originaldokumente wird eine SHA-256-Checksumme erfasst.

Die Checksumme dient der Integritätsprüfung und nicht als Identität der Datei.

Damit kann später geprüft werden, ob die physisch gespeicherte Datei noch exakt dem ursprünglich von Auditarium angenommenen Inhalt entspricht.

## `IFileStorage`

Die BLL definiert die benötigte Storage-Fähigkeit.

Konzeptionell:

```csharp
public interface IFileStorage
{
    Task StoreAsync(...);
    Task<Stream> OpenReadAsync(...);
    Task DeleteAsync(...);
    Task<bool> ExistsAsync(...);
}
```

Die konkrete Signatur wird implementierungsnah festgelegt.

Die BLL kennt weder absolute Pfade noch Dateisystemdetails.

## Download

Die FAL ist nicht nur für Upload und Speicherung zuständig, sondern auch für die Bereitstellung der physischen Datei beim Download.

Ablauf:

```text
Download Request
→ Authentication
→ RBAC
→ BLL prüft Zugriff und CatalogVersion
→ FileItem laden
→ FAL.OpenReadAsync(...)
→ Stream
→ Web/API liefert Datei unter OriginalFileName aus
```

Der Benutzer erhält den ursprünglichen Dateinamen.

Die tatsächliche physische Storage-ID bleibt intern.

## Keine öffentlichen Storage-URLs

Originaldokumente werden nicht direkt über einen öffentlich erreichbaren Storage-Pfad ausgeliefert.

Nicht vorgesehen:

```text
https://storage/.../7f4e42...
file://...
direkter SMB-Pfad
```

Downloads laufen kontrolliert durch Auditarium.

Damit greifen:

```text
Authentication
RBAC
Businessregeln
Logging / Observability
```

auch beim Dokumentdownload.

## Verantwortlichkeiten von BLL, DAL und FAL

Die Verantwortlichkeiten sind getrennt:

```text
BLL
→ darf Datei hinzugefügt/ersetzt/entfernt/geladen werden?

DAL
→ FileItem persistieren
→ catalog_version.source_file_id verwalten

FAL
→ Datei physisch speichern
→ Datei physisch öffnen
→ Datei physisch löschen
```

Die FAL enthält keine fachlichen Regeln zum Lifecycle einer `catalog_version`.

## Storage-Unabhängigkeit

Die erste Implementierung ist ein dateisystembasierter Storage.

Konzeptionell:

```text
Auditarium.Fal
└── LocalFileStorage
```

Ein lokales Verzeichnis und ein gemountetes SMB-/NFS-Share sind aus Sicht dieser Implementierung gleichartig.

Bei einer einzelnen Auditarium-Instanz kann `StorageRoot` lokal liegen.

Bei mehreren Auditarium-Instanzen müssen alle Instanzen auf denselben gemeinsam erreichbaren `StorageRoot` zugreifen.

Das kann beispielsweise sein:

```text
gemeinsames SMB-/NFS-Share
gemeinsam gemountetes Volume
anderer gemeinsam erreichbarer Dateisystempfad
```

Die Datenbank speichert weiterhin ausschließlich relative Pfade.

Spätere Implementierungen können beispielsweise sein:

```text
S3FileStorage
AzureBlobFileStorage
```

Das fachliche Modell und die BLL sollen dafür nicht geändert werden müssen.

## Datenbank als Referenz

Die Datenbank ist die Referenz dafür, welche Dateien Auditarium kennt.

Das Dateisystem wird nicht als Datenbank verwendet.

Eine Datei ohne zugehörigen `FileItem` ist aus Auditarium-Sicht eine verwaiste Storage-Datei.

Ein `FileItem` ohne vorhandene physische Datei ist ein Integritätsfehler und muss über technische Diagnose sichtbar werden.

---

# 11. Strukturierter Katalogimport

## Import-Grundmodell

### Grundsatz

Auditarium erzeugt die fachlichen Inhalte eines Importpakets nicht selbst.

Stattdessen definiert Auditarium einen versionierten Importvertrag und stellt dazu einen passenden Hilfsprompt bereit.

Der eigentliche Inhalt eines Importpakets kann außerhalb von Auditarium durch beliebige Werkzeuge erzeugt werden.

Für Auditarium ist die Erzeugungsmethode technisch irrelevant. Das Importpaket kann beispielsweise manuell, per Script oder mit einem externen Analyse-/KI-Werkzeug erzeugt werden. Auditarium selbst enthält dafür keine LLM-/KI-Laufzeit, keine Modell- oder Providerintegration und keine dafür erforderlichen API-Schlüssel.

Grundablauf:

```text
Auditarium
→ Importformat bereitstellen
→ Hilfsprompt bereitstellen

Benutzer / externes Werkzeug
→ Originaldokument verarbeiten
→ Importdatei erzeugen

Auditarium
→ Importdatei hochladen
→ validieren
→ Importbericht / Vorschau
→ kontrolliert anwenden
```

Grundsatz:

> Auditarium automatisiert den Importvertrag, nicht die externe Erzeugung des Importinhalts.

Für v1 ist **kein eigenes persistentes Import-Aggregat** als fachlicher Bestandteil vorgeschrieben. Upload, Validierung, Vorschau und Apply dürfen technisch mit temporären Laufzeitdaten umgesetzt werden. Persistenz wird nur dort eingeführt, wo sie für Nachvollziehbarkeit, Wiederaufnahme oder Betrieb tatsächlich benötigt und ausdrücklich modelliert wird.

### Versioniertes Importformat

Auditarium definiert ein maschinenlesbares, versioniertes Importformat.

Für die erste Version wird ein JSON-basiertes Format vorgesehen.

Konzeptionell:

```text
Auditarium Catalog Import Package
Import Format Version: 1
```

Das Paket kann insbesondere enthalten:

```text
Dokument-/Katalog-Metadaten
Dokumentelemente
Hierarchie
Sortierung
Elementtexte
Fragen
Scope-Zuordnungen
Verification Hints
Evidence Hints
weitere importierbare Katalogdaten
```

Das konkrete technische Schema wird aus dem fachlichen Datenmodell und den DRAFT-Regeln abgeleitet.

Das Paket enthält ausschließlich Daten, die für die Erstellung oder Ergänzung des adressierten Katalog-`DRAFT` benötigt werden. Es darf keine direkten System-, Benutzer-, Rollen-, Credential- oder READY-Zustandsänderungen beschreiben.

### Versionierter Hilfsprompt

Auditarium stellt einen zum jeweiligen Importformat passenden Hilfsprompt bereit.

Der Prompt beschreibt mindestens:

```text
erwartete Aufgabe
zulässige Ausgabe
Import Format Version
Struktur des Importpakets
zulässige Werte
Hierarchieregeln
Regeln für Fragen und Scopes
Anforderung, ausschließlich das Importpaket auszugeben
```

Prompt und Format werden gemeinsam versioniert:

```text
Import Prompt v1
↔ Import Format v1
```

Der Prompt ist eine Hilfestellung für externe Werkzeuge und kein Bestandteil einer internen Inhaltserzeugung.

### Bindung an einen DRAFT

Ein Importpaket bezieht sich auf eine konkrete Katalogversion im Zustand `DRAFT`.

Der Importvertrag führt mindestens:

```text
import_format_version
catalog_version_id
draft_revision
```

mit.

Damit kann Auditarium feststellen, ob das Paket noch auf dem aktuellen DRAFT-Stand basiert.

Beispiel:

```text
Importpaket:
catalog_version_id = 17
draft_revision = 4

aktueller DRAFT:
catalog_version_id = 17
draft_revision = 6

→ IMPORT.BASE_REVISION_MISMATCH
```

Ein veraltetes Paket wird nicht blind auf einen inzwischen geänderten DRAFT angewendet. Der Benutzer muss das Importpaket auf Basis des aktuellen DRAFT-Stands neu erzeugen bzw. erneut erzeugen lassen und anschließend einen neuen Import-/Validierungslauf starten.

### Kein direkter Systemzugriff externer Erzeuger

Das externe Werkzeug schreibt nicht direkt in Auditariums Datenbank oder fachliche Tabellen.

Auditarium akzeptiert ausschließlich den definierten Importvertrag über seine vorgesehenen Importwege.

Damit bleiben:

```text
Authentifizierung
RBAC
Validierung
Audit-Logging
Concurrency
Datenbank-Constraints
```

unter Kontrolle von Auditarium.

---

## Validierung eines Importpakets

### Importdatei als nicht vertrauenswürdige Eingabe

Jede hochgeladene Importdatei wird unabhängig von ihrer Herkunft als nicht vertrauenswürdige externe Eingabe behandelt.

Vor einer Datenänderung erfolgen mindestens:

```text
Syntaxprüfung
Schema-Validierung
fachliche Importvalidierung
Referenzprüfung
Hierarchieprüfung
Constraint-Prüfung
```

Das reine Hochladen und Prüfen verändert den Katalog nicht.

### Importvalidierung und READY-Validierung

Importvalidierung und `READY`-Validierung sind getrennte Prüfungen.

Importvalidierung beantwortet:

> Können die gelieferten Daten sicher und strukturell konsistent in den DRAFT übernommen werden?

READY-Validierung beantwortet:

> Erfüllt der vollständige DRAFT die Voraussetzungen für die Verwendung in Audits?

Ein gültiger Import darf daher bewusst einen noch unvollständigen DRAFT erzeugen.

### Harte Importfehler

Harte strukturelle oder fachliche Invarianten dürfen niemals ignoriert werden.

Beispiele:

```text
ungültiges JSON
unbekannte import_format_version
fehlende Pflichtfelder
ungültige Parent-Referenzen
Zyklen
ungültige Sortierung
unbekannte Scope-Werte
inkonsistente Referenzen
ungültige Ziel-Katalogversion
abweichende draft_revision
Verletzung harter Datenbank-/Modell-Constraints
```

Für solche Fehler gibt es keinen „trotzdem übernehmen“-Schalter. Harte Integritätsverletzungen dürfen niemals in die Datenbank übernommen werden.

### Importstatus

Ein geprüftes Importpaket erhält mindestens einen der Zustände:

```text
VALID
PARTIALLY_VALID
REJECTED
```

Semantik:

```text
VALID
→ vollständig übernehmbar

PARTIALLY_VALID
→ gültige Teilmengen sind kontrolliert übernehmbar

REJECTED
→ strukturell oder fachlich nicht sinnvoll anwendbar
```

---

## Importbericht und Vorschau

Vor einem Apply erzeugt Auditarium einen strukturierten Importbericht.

Dieser enthält mindestens:

```text
Importstatus
Anzahl erkannter Elemente
Anzahl erkannter Fragen
Fehler
Warnungen
übernehmbare Inhalte
nicht übernehmbare Inhalte
```

Beispiel:

```text
Status: PARTIALLY_VALID

132 Elemente erkannt
287 Fragen erkannt

Fehler:
- Element E-047 referenziert unbekannten Parent E-999
- Frage Q-082 enthält keinen gültigen Scope

Warnungen:
- Verification Hint bei Q-114 fehlt
```

Der Bericht dient sowohl der Kontrolle vor dem Import als auch als Rückmeldung zur externen Korrektur des Pakets.

Bei Änderungen an einem bereits bearbeiteten DRAFT kann die Vorschau zusätzlich die konkret vorgesehenen Änderungen sichtbar machen.

---

## Kontrollierter und teilweiser Import

### Getrennter Apply-Schritt

Validierung und Datenänderung sind getrennte Operationen.

```text
Importdatei hochladen
→ validieren
→ Bericht / Vorschau anzeigen
→ Benutzer entscheidet
→ Apply
→ Datenbankänderung
```

### Teilimport

Bei `PARTIALLY_VALID` dürfen valide Inhalte kontrolliert übernommen werden.

Dabei gilt:

```text
valide Inhalte
→ dürfen übernommen werden

harte Fehler
→ werden nicht übernommen

ungültige abhängige Teilbäume
→ werden gemeinsam verworfen
```

Welche Teile übernehmbar sind, muss deterministisch aus dem Validierungsergebnis hervorgehen.

Harte Integritätsregeln können durch einen Teilimport nicht umgangen werden.

### Atomarer Apply-Umfang

Der vom Benutzer gewählte Apply-Umfang wird transaktional angewendet.

```text
alle für diesen Apply vorgesehenen Änderungen erfolgreich
→ Commit

eine vorgesehene Änderung schlägt fehl
→ Rollback
```

Dadurch entstehen keine halb angewendeten Teilzustände innerhalb eines bestätigten Apply-Laufs.

Nach erfolgreicher Änderung des DRAFTs wird dessen `draft_revision` entsprechend fortgeschrieben.

---

## Abgebrochene oder fehlgeschlagene Importvorgänge

Ein Upload, eine Validierung oder eine Vorschau verändert die Katalogversion nicht.

Schlägt der Apply-Schritt fehl, wird seine Transaktion zurückgerollt.

Die Ziel-Katalogversion bleibt fachlich `DRAFT` und behält den Zustand vor dem fehlgeschlagenen Apply.

Technische Importartefakte und temporäre Dateien dürfen nach den dafür definierten Retention-/Cleanup-Regeln entfernt werden.

Ein Import-/Apply-Lauf erhält keine Sonderrechte, um einen anderweitig bearbeiteten DRAFT zu löschen oder harte fachliche Regeln zu umgehen.

---

# 12. Fachmodell: Audit Units

## Begriff

Die konkrete auditierbare Einheit heißt fachlich **Prüfeinheit**.

Technische Bezeichnung:

```text
audit_unit
```

Tabelle:

```text
audit_units
```

## Tabelle `audit_units`

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `audit_unit_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `parent_audit_unit_id` | `NULL` erlaubt | `NULL` | optionale Referenz auf übergeordnete Audit Unit |
| `scope_type_id` | `NOT NULL` | – | genau ein fest definierter Scope Type |
| `name` | `NOT NULL` | – | frei vergebener Name |
| `description` | `NULL` erlaubt | `NULL` | optionale nähere Beschreibung |
| `usage_state` | `NOT NULL` | `ACTIVE` | lokale Nutzbarkeit: `ACTIVE` oder `INACTIVE` |
| `usage_state_reason` | `NULL` erlaubt; bei `INACTIVE` fachlich erforderlich | `NULL` | Begründung des aktuellen Nutzungszustands |
| `notes` | `NULL` erlaubt | `NULL` | optionale interne Hinweise |

`name` muss einen nicht-leeren Wert enthalten.

Für `usage_state` gilt:

```text
ACTIVE
→ usage_state_reason darf NULL sein

INACTIVE
→ usage_state_reason muss einen nicht-leeren Wert enthalten
```

### Grundregeln

- Jede Audit Unit besitzt genau einen Scope Type.
- Jede Audit Unit besitzt höchstens einen Parent.
- Parent-Child bildet nur die Audit-Hierarchie ab.
- Es werden keine beliebigen technischen oder organisatorischen Abhängigkeiten modelliert.
- Es gibt keine `audit_unit_relations`.

`OTHER` erfordert zusätzlich eine nicht-leere `description`, damit die konkrete Bedeutung der Prüfeinheit nachvollziehbar bleibt.

## Parent-Child-Regeln

Parent-Child bedeutet:

> Eine Prüfeinheit ist im Audit-Kontext organisatorisch, räumlich oder strukturell Bestandteil der übergeordneten Prüfeinheit.

Circular Dependencies sind verboten.

Die Anwendung muss:

1. direkte und indirekte Zyklen verhindern,
2. zulässige Parent-/Child-Kombinationen anhand der Scope Types validieren.

### Vorgesehene Parent-Typen

| Child | zulässige / sinnvolle Parent-Typen |
|---|---|
| `ORGANIZATION` | keiner oder `ORGANIZATION` |
| `SITE` | `ORGANIZATION` |
| `BUILDING` | `SITE`, ggf. `ORGANIZATION` |
| `AREA` | `ORGANIZATION`, `SITE`, `BUILDING`, `AREA` |
| `ROOM` | `BUILDING`, `AREA`, ggf. `SITE` |
| `TECHNICAL_AREA` | `BUILDING`, `AREA`, `SITE` |
| `NETWORK` | `ORGANIZATION`, `SITE`, `AREA`, ggf. `TECHNICAL_AREA` |
| `IT_SYSTEM` | `ORGANIZATION`, `SITE`, `AREA`, `TECHNICAL_AREA`, ggf. `NETWORK` |
| `APPLICATION` | `ORGANIZATION`, `AREA`, `IT_SYSTEM`, ggf. `SERVICE` |
| `PROCESS` | `ORGANIZATION`, `AREA` |
| `SERVICE` | `ORGANIZATION`, `AREA` |
| `EXTERNAL_PROVIDER` | `ORGANIZATION` |
| `OTHER` | nur nach manueller Prüfung |

Diese Regeln sind Bestandteil der Anwendung und nicht frei durch Benutzer erweiterbar.

## Nutzungszustand einer Audit Unit

Audit Units verwenden für ihre lokale Nutzbarkeit das Feld `usage_state`.

Für sie gilt:

```text
ACTIVE
INACTIVE
```

`ACTIVE` bedeutet, dass die Audit Unit für neue Audits ausgewählt werden darf.

`INACTIVE` bedeutet, dass die Audit Unit historisch erhalten bleibt, aber nicht mehr für neue Audits verwendet werden soll.

`usage_state_reason` beschreibt den fachlichen Grund des aktuell gesetzten Nutzungszustands.

Typische Beispiele für `INACTIVE`:

```text
Standort geschlossen
Serverraum außer Betrieb
Anwendung abgelöst
IT-System stillgelegt
```

Wer und wann `usage_state` geändert hat, wird nicht redundant an der Prüfeinheit gespeichert, sondern über das `system_audit_log` nachvollzogen.


Beispiele für `INACTIVE`:

- aufgelöster Standort,
- außer Betrieb genommener Serverraum,
- abgeschaltete Anwendung,
- nicht mehr verwendetes IT-System.

Der Wechsel ist reversibel und wird im `system_audit_log` protokolliert.

---

# 13. Fachmodell: Audits, Materialisierung und Antworten

## Audits und Lifecycle

### Definition

Ein Audit ist:

> **die Prüfung genau einer Audit Unit gegen genau eine konkrete Dokumentversion mit einer beim Abschluss der Auditkonfiguration festgelegten `READY`-Katalogversion.**

### Tabelle `audits`

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `audit_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `origin_audit_id` | `NULL` erlaubt | `NULL` | Referenz auf das ursprüngliche Basis-Audit bei Wiederholungen |
| `name` | `NOT NULL` | – | frei vergebene Bezeichnung des Audits |
| `description` | `NULL` erlaubt | `NULL` | optionale Beschreibung von Anlass, Ziel oder Besonderheiten |
| `audit_unit_id` | `NOT NULL` | – | Referenz auf die gewählte Prüfeinheit |
| `catalog_version_id` | `NOT NULL` | – | im `DRAFT` veränderbare, beim Publish festgelegte Katalogversion |
| `audit_unit_context` | konditional; `NULL` nur im `DRAFT` | `NULL` | kompakter historischer Kontext der Prüfeinheit; ab `READY` verpflichtend |
| `audit_state` | `NOT NULL` | `DRAFT` | aktueller Zustand |
| `state_reason` | `NULL` erlaubt; zustands-/aktionsabhängig erforderlich | `NULL` | fachliche Begründung des aktuellen Audit-Zustands |
| `assigned_auditor_user_id` | `NULL` erlaubt | `NULL` | aktuell exklusiv für die Auditbearbeitung zugewiesener interner Benutzer |
| `audit_settings` | `NOT NULL` | gültige Standardkonfiguration | Audit-Einstellungen einschließlich Response-Policy-Regeln |
| `created_at` | `NOT NULL` | Erstellungszeitpunkt | Zeitpunkt der Erstellung |
| `notes` | `NULL` erlaubt | `NULL` | optionale interne bzw. administrative Hinweise |

`name` muss einen nicht-leeren Wert enthalten.

Für `audit_unit_context` gilt:

```text
DRAFT
→ audit_unit_context = NULL

READY / IN_PROGRESS / FINALIZED / CANCELED
→ audit_unit_context IS NOT NULL
```

Für `state_reason` gilt:

- bei `CANCELED` ist eine nicht-leere Begründung verpflichtend,
- beim bewussten Wiederöffnen eines `CANCELED`-Audits ist ebenfalls eine Begründung verpflichtend,
- automatische Zustandswechsel `READY ↔ IN_PROGRESS` benötigen keine Begründung.

Wer und wann den Zustand geändert hat, wird nicht redundant am Audit gespeichert, sondern über das `system_audit_log` nachvollzogen.

Für `assigned_auditor_user_id` gilt:

```text
DRAFT
→ NULL

READY / IN_PROGRESS
→ NULL oder genau ein aktiver, grundsätzlich zur Auditbearbeitung berechtigter Benutzer

FINALIZED
→ NULL
```

Eine Auditor-Zuweisung ist eine operative Arbeitszuweisung und keine historische Autoreninformation. Historische Bearbeitung wird über `answered_by`, `answered_at` und das `system_audit_log` nachvollzogen.

### Audit-Unit-Kontext

Audit Units sind veränderlich.

Deshalb wird beim Abschluss der Auditkonfiguration nur der für das spätere Verständnis relevante historische Kontext der Audit Unit gespeichert.

Damit bleibt ein altes Audit auch dann nachvollziehbar, wenn das Auditobjekt später umbenannt, verschoben oder umklassifiziert wird.

### Name und Beschreibung eines Audits

Jedes Audit erhält einen frei vergebenen, verpflichtenden `name`.

Beispiele:

```text
Voraudit Netzwerksegmentierung Standort Cottbus
Master-Audit BSI NET.1.1 – Serverraum F47
```

Die optionale `description` dient dazu, Anlass, Ziel oder besondere Rahmenbedingungen des Audits zu beschreiben.

Beispiel:

> Schnelles Voraudit zur Vorbereitung auf die formale Prüfung. Kommentare und Nachweise sind weitgehend optional.

Es wird bewusst **kein festes Feld `audit_type`** mit Werten wie `PRE_AUDIT`, `MASTER_AUDIT` oder ähnlichen Kategorien eingeführt.

Der fachliche Kontext wird über `name`, `description` und die konkreten Audit-Einstellungen einschließlich der Response-Policy-Regeln beschrieben.

---


### Lebenszyklus eines Audits

Der fachliche Lebenszyklus lautet:

```text
DRAFT
  ↓
READY
  ↕
IN_PROGRESS
  ↓
FINALIZED

READY ───────→ CANCELED
IN_PROGRESS ─→ CANCELED
CANCELED ────→ READY oder IN_PROGRESS
               abhängig von vorhandenen Antworten
```

`CANCELED` ist ausschließlich für bereits veröffentlichte Audits vorgesehen, die nicht weitergeführt werden.

Bedeutung:

- `DRAFT`: Konfiguration noch veränderbar; keine materialisierten Auditfragen.
- `READY`: Konfiguration abgeschlossen; Audit vollständig materialisiert und bearbeitbar.
- `IN_PROGRESS`: mindestens eine Auditfrage wurde beantwortet.
- `FINALIZED`: Audit vollständig und irreversibel abgeschlossen.
- `CANCELED`: bereits veröffentlichtes Audit bewusst beendet, ohne reguläre Finalisierung; kann durch einen `AUDIT_MANAGER` wieder geöffnet werden.

Zulässige Abbruchpfade:

```text
READY ───────→ CANCELED
IN_PROGRESS ─→ CANCELED
```

Ein `DRAFT` kann nicht auf `CANCELED` gesetzt werden.

Wird ein noch nicht veröffentlichtes Audit nicht mehr benötigt, kann der `DRAFT` gemäß den allgemeinen Löschregeln gelöscht bzw. soft-deleted werden.

Der Übergang `DRAFT → READY` wird durch den `AUDIT_MANAGER` ausgelöst.

Der Übergang `READY → IN_PROGRESS` erfolgt automatisch mit der ersten beantworteten Auditfrage. Werden sämtliche Antworten wieder zurückgesetzt, erfolgt automatisch `IN_PROGRESS → READY`.

Beim Übergang auf `CANCELED` bleiben sämtliche bereits materialisierten Inhalte und vorhandenen Antworten unverändert erhalten.

Der Abbruch wird über die einheitlichen Zustandsmetadaten des Audits dokumentiert:

```text
audit_state = CANCELED
state_reason
```

Der Zeitpunkt der ersten Bearbeitung wird nicht als eigenes Feld am Audit gespeichert. Er ergibt sich aus dem ersten protokollierten Zustandswechsel von `READY` nach `IN_PROGRESS` im `system_audit_log`.


`state_reason` enthält dabei die Abbruchbegründung.

`CANCELED` ist ein Abbruchzustand und kann bewusst wieder geöffnet werden.

Für alle Audit-Zustände gelten einheitliche Zustandsmetadaten:

```text
audit_state
state_reason
```

Diese Felder beschreiben nur den jeweils letzten Zustandswechsel.

Die vollständige Folge aller Zustandsänderungen wird nicht redundant am Audit gespeichert, sondern im `system_audit_log`.

#### Löschbarkeit eines Audits

Die Löschbarkeit hängt vom fachlichen Zustand und davon ab, ob bereits Auditfragen beantwortet wurden.

```text
DRAFT
→ löschbar / soft-delete

READY
→ löschbar / soft-delete,
  solange für alle audit_questions gilt:
  result IS NULL

IN_PROGRESS
FINALIZED
CANCELED
→ nicht löschbar
```

Ein Audit im Zustand `READY` ist zwar bereits materialisiert, enthält aber noch kein fachliches Prüfungsergebnis, solange keine Auditfrage beantwortet wurde.

Sobald mindestens eine Auditfrage beantwortet wird:

```text
READY → IN_PROGRESS
```

und eine Löschung ist zunächst nicht mehr zulässig.

Werden sämtliche Antworten eines noch nicht abgeschlossenen Audits wieder zurückgesetzt, erfolgt automatisch:

```text
IN_PROGRESS → READY
```

Danach ist das Audit wieder löschbar, sofern weiterhin für alle `audit_questions` `result IS NULL` gilt.

Soll ein bereits bearbeitetes Audit nicht fortgeführt werden, wird es stattdessen auf `CANCELED` gesetzt.


---

### Finalisierung eines Audits

Ein Audit darf nur finalisiert werden, wenn sämtliche `audit_questions` beantwortet wurden.

Für jede Auditfrage muss gelten:

```text
result IS NOT NULL
```

Dabei sind alle zulässigen Antwortwerte vollständig gleichwertig im Sinne von „beantwortet“:

```text
JA
NEIN
NICHT_ANWENDBAR
NICHT_FESTSTELLBAR
```

Zusätzlich müssen alle durch die Audit-Einstellungen geforderten Kommentare und Nachweise vorhanden sein.

Erst dann darf ein `AUDITOR` die Finalisierung bewusst auslösen.

Der zulässige Übergang lautet:

```text
IN_PROGRESS → FINALIZED
```

Die Finalisierung ist eine fachliche Bearbeitungsoperation und erfordert neben `Audits.Finalize` eine gültige Zuweisung an den aktuellen Benutzer.

Beim erfolgreichen Übergang wird die aktive Auditor-Zuweisung atomar aufgehoben:

```text
IN_PROGRESS
assigned_auditor_user_id = CurrentUser

→ Finalize

FINALIZED
assigned_auditor_user_id = NULL
```

Schlägt die Finalisierung fehl, bleibt die Zuweisung unverändert.

Ein Audit mit mindestens einer unbeantworteten Frage kann nicht finalisiert werden.

Wird ein unvollständig bearbeitetes Audit nicht weitergeführt, ist dies kein regulärer Abschluss, sondern ein Abbruch:

```text
READY → CANCELED
IN_PROGRESS → CANCELED
```

Ein `CANCELED`-Audit besitzt kein gültiges fachliches Abschlussresultat.

Bereits vorhandene Antworten, Kommentare und Nachweise bleiben jedoch aus Gründen der Nachvollziehbarkeit erhalten. „Ergebnis verwerfen“ bedeutet daher nicht, die vorhandenen Bearbeitungsdaten zu löschen, sondern das Audit nicht als regulär abgeschlossenes Ergebnis zu werten.

`FINALIZED` ist irreversibel. `CANCELED` kann über die ausdrücklich protokollierte Aktion „Audit wieder öffnen“ verlassen werden.

---

### Abgebrochenes Audit wieder öffnen

Ein Audit im Zustand `CANCELED` kann durch einen `AUDIT_MANAGER` bewusst wieder geöffnet werden.

Die Aktion lautet fachlich:

> **Audit wieder öffnen**

Der Benutzer wählt dabei keinen Zielstatus.

Auditarium bestimmt den Zielzustand automatisch anhand der vorhandenen Antworten:

```text
CANCELED
   ↓ Audit wieder öffnen

alle audit_questions.result IS NULL
→ READY

mindestens eine audit_question.result IS NOT NULL
→ IN_PROGRESS
```

Beim Wiederöffnen gilt:

- es erfolgt keine erneute Materialisierung,
- es erfolgt keine neue Scope-Filterung,
- `catalog_version_id` bleibt unverändert,
- `audit_unit_context` bleibt unverändert,
- die Audit-Einstellungen bleiben unverändert,
- `audit_document_elements` und `audit_questions` bleiben unverändert,
- vorhandene Antworten, Kommentare und Nachweise bleiben erhalten.

Das Wiederöffnen erfordert eine Begründung.

Der Zustandswechsel wird über:

```text
audit_state
state_reason
```

dokumentiert und vollständig im `system_audit_log` protokolliert.

`FINALIZED` bleibt davon ausdrücklich ausgeschlossen und kann nicht wieder geöffnet werden.

---

### Audit erneut durchführen / Audit als Vorlage verwenden

Ein bestehendes Audit kann jederzeit als Vorlage für ein neues Audit verwendet werden.

Diese Funktion ist **nicht** auf finalisierte oder abgeschlossene Audits beschränkt.

Als Vorlage zulässig sind Audits in jedem vorhandenen Zustand:

```text
DRAFT
READY
IN_PROGRESS
FINALIZED
CANCELED
```

Die UI stellt dafür eine Funktion bereit, z. B.:

> **Nochmal auditieren**

bzw. fachlich eindeutiger:

> **Neues Audit auf Basis dieses Audits erstellen**

#### Grundprinzip

Die Funktion kopiert nicht den Bearbeitungsstand, sondern ausschließlich die Auditdefinition bzw. ausgewählte Ausgangsbedingungen.

Das neue Audit:

- erhält immer eine neue `audit_id`,
- startet immer mit `audit_state = DRAFT`,
- besitzt keine übernommenen Antworten,
- besitzt keine übernommenen Kommentare,
- besitzt keine übernommenen Nachweise,
- besitzt keine übernommenen `answered_at`-/`answered_by`-Werte,

`answered_at` und `answered_by` sind Bestandteil der aktuell gültigen Antwort. Sie werden direkt an `audit_questions` gespeichert. Das `system_audit_log` kann frühere Änderungen zusätzlich historisch nachvollziehbar machen, ersetzt diese beiden Felder jedoch nicht.


Das neue Audit ist damit fachlich vollständig eigenständig. Zunächst wird nur seine Konfiguration als `DRAFT` angelegt; die prüfbaren Inhalte werden erst beim Abschluss dieser Konfiguration materialisiert.

#### Wiederholungsdialog

Der `AUDIT_MANAGER` erhält beim Erzeugen einer Wiederholung die bisherige Konfiguration als Ausgangspunkt und kann auswählen, welche Bedingungen übernommen oder aktualisiert werden sollen.

Mindestens vorgesehen:

##### Audit Unit

- gleiche Audit Unit verwenden,
- andere Audit Unit auswählen.

##### Katalogversion

Die bisherige Katalogversion kann als Ausgangswert übernommen werden. Alternativ kann die aktuell höchste `READY`-Katalogversion des gewählten `ACTIVE`-Dokuments vorausgewählt werden.

Solange das neue Audit `DRAFT` ist, bleibt `catalog_version_id` frei konfigurierbar. Erst beim Publish wird die Auswahl validiert und festgeschrieben.

##### Audit-Einstellungen

- bisherige Audit-Einstellungen als Ausgangspunkt übernehmen,
- Audit-Einstellungen im `DRAFT` anpassen.

##### Gewichtungen

Beim Publish werden die zu diesem Zeitpunkt effektiven Gewichtungen der ausgewählten Katalogversion als `weight_snapshot` übernommen.

##### Name und Beschreibung

- werden vorbelegt,
- können vor der Erstellung angepasst werden.

Beim erneuten Durchführen eines Audits dürfen die bisherigen Konfigurationswerte in den neuen `DRAFT` übernommen werden. Dort bleiben sie vollständig veränderbar. `audit_document_elements` und `audit_questions` werden erst beim Publish erzeugt.

#### Referenz auf das Ursprungs-Audit

Für die Herkunftsbeziehung wird in `audits` verwendet:

```text
origin_audit_id
```

Beim ursprünglich angelegten Audit gilt:

```text
origin_audit_id = NULL
```

Bei allen daraus erzeugten Wiederholungen verweist `origin_audit_id` direkt auf dieses ursprüngliche Basis-Audit.

Beispiel:

```text
Audit 100
├── Audit 147 → origin_audit_id = 100
├── Audit 163 → origin_audit_id = 100
└── Audit 208 → origin_audit_id = 100
```

Wird aus Audit 147 erneut eine Wiederholung erzeugt, gilt ebenfalls:

```text
neues Audit → origin_audit_id = 100
```

und ausdrücklich nicht:

```text
neues Audit → origin_audit_id = 147
```

Damit entstehen **keine Kopierketten**.

Zusätzlich gilt als harte Mindestregel:

```text
origin_audit_id IS NULL
oder
origin_audit_id != audit_id
```

Dass die Referenz bei Wiederholungen tatsächlich direkt auf das Basis-Audit zeigt, wird durch die Anwendungslogik sichergestellt.


#### Nutzen für Historie und Zeitleiste

Über `origin_audit_id` können Audit-Familien und Wiederholungsreihen eindeutig erkannt werden.

Beispiel:

```text
Basis-Audit 100

2026-03-12  Audit 100
2026-09-12  Audit 147
2027-03-20  Audit 163
2027-10-01  Audit 208
```

Diese Beziehung kann in Zeitleisten und Auswertungen genutzt werden, ohne andere freie Auswertungsmöglichkeiten einzuschränken.

#### Bestehende Nutzungsregeln bleiben gültig

Die Funktion „Nochmal auditieren“ umgeht keine fachlichen Nutzungsregeln.

Ein neues Audit darf nur mit aktuell zulässigen Objekten erzeugt werden.

Beispiele:

- ein `DEPRECATED`-Dokument muss vor erneuter Nutzung wieder auf `ACTIVE` gesetzt werden,
- eine nicht nutzbare Audit Unit muss vor der Verwendung wieder in einen zulässigen Zustand überführt werden.

---

## Materialisierung eines Audits

### Audit im Zustand `DRAFT`

Ein neu angelegtes Audit startet im Zustand:

```text
DRAFT
```

In diesem Zustand wird ausschließlich die Konfiguration des Audits gepflegt.

Dazu gehören insbesondere:

- `name`,
- `description`,
- `audit_unit_id`,
- `catalog_version_id`,
- die konfigurierbaren Audit-Einstellungen einschließlich der Response-Policy-Regeln,
- gegebenenfalls weitere grundlegende Konfigurationsangaben.

Alle diese Angaben dürfen im `DRAFT` verändert oder aus einem bestehenden Audit übernommen werden.

Sie sind zu diesem Zeitpunkt noch keine unveränderlichen fachlichen Abhängigkeiten.

Im `DRAFT` existieren noch keine:

```text
audit_document_elements
audit_questions
weight_snapshot
audit_unit_context
```

Ein Audit im Zustand `DRAFT` kann noch nicht durch einen Auditor bearbeitet werden.


### Dynamische Vorschau

Während der Konfiguration darf Auditarium jederzeit dynamisch berechnen, welche Inhalte sich aus dem aktuellen Stand ergeben würden.

Vereinfacht:

```text
audit_unit
   ↓
scope_type bestimmen

document + catalog_version
   ↓
aktuelle DRAFT-Auswahl verwenden

questions
   ↓
über question_scope_types filtern

relevante document_elements + questions
   ↓
Vorschau
```

Beispielsweise kann die UI anzeigen:

```text
Aktuelle Konfiguration würde erzeugen:
- 68 prüfbare Dokumentelemente
- 143 Auditfragen
```

Diese Vorschau wird nicht persistiert.

Ändert der `AUDIT_MANAGER` im `DRAFT` die Prüfeinheit, das Dokument oder andere prüfungsrelevante Einstellungen, wird die Vorschau einfach neu berechnet.


### Audit-Einstellungen

Die fachlichen Einstellungen eines Audits werden direkt am Audit gespeichert.

Dazu gehören auch die Response-Policy-Regeln, beispielsweise ob Kommentare oder Nachweise für bestimmte Antwortwerte verpflichtend sind.

Diese Einstellungen sind:

```text
im DRAFT
→ veränderbar

ab READY
→ unveränderlich
```

Ein separates fachliches Objekt oder ein zusätzlicher Snapshot für die Response Policy ist nicht vorgesehen.


### Historischer Kontext der Prüfeinheit

Das Audit behält die technische Referenz auf die gewählte Prüfeinheit über:

```text
audit_unit_id
```

Da eine Prüfeinheit später umbenannt, verschoben oder einem anderen `scope_type` zugeordnet werden kann, wird beim Übergang auf `READY` zusätzlich ein kompakter historischer Kontext gespeichert.

```text
audit_unit_context
```

Dieser enthält mindestens:

```text
name
scope_type_id
hierarchy_path
```

Beispiel für `hierarchy_path`:

```text
Organisation
└── Standort Cottbus
    └── Gebäude F47
        └── Serverraum F47-11A
```

Nicht gespeichert werden Angaben, die für die historische Interpretation des Audits nicht erforderlich sind, beispielsweise:

- Status der Prüfeinheit,
- Notizen,
- allgemeine technische Metadaten,
- sonstige veränderliche Eigenschaften ohne fachliche Relevanz für das Audit.

Grundsatz:

> **Auditarium speichert nicht die komplette Prüfeinheit redundant, sondern nur den historischen Kontext, der für das spätere Verständnis des Audits erforderlich ist.**


### Abschluss der Konfiguration

Der `AUDIT_MANAGER` schließt die Auditkonfiguration bewusst über einen Publish-Schritt ab.

Bis zu diesem Zeitpunkt bleibt die gesamte Konfiguration veränderbar.

Beim Publish werden die aktuell im `DRAFT` gesetzten Abhängigkeiten validiert und anschließend materialisiert.

Der Vorgang erfolgt atomar:

```text
Auditkonfiguration [DRAFT]
        ↓
Publish
        ↓
Prüfeinheit validieren
        ↓
Dokument und catalog_version_id validieren
        ↓
prüfen, ob catalog_version READY ist und ihr Dokument für neue Audits zulässig ist
        ↓
Audit-Einstellungen validieren
        ↓
Scope-Filter anwenden
        ↓
audit_document_elements erzeugen
        ↓
audit_questions erzeugen
        ↓
weight_snapshot erzeugen
        ↓
audit_unit_context erzeugen
        ↓
audit_state = READY
```

Beim Publish muss Auditarium mindestens sicherstellen:

- `audit_unit_id` verweist auf eine für neue Audits zulässige Prüfeinheit,
- `catalog_version_id` verweist auf eine Katalogversion, deren zugehöriges Dokument für neue Audits zulässig ist,
- die gewählte Katalogversion befindet sich im Zustand `READY`,
- die Audit-Einstellungen sind formal vollständig,
- der resultierende Fragenbestand lässt sich konsistent erzeugen,
- nach allen Filtern verbleibt mindestens eine `audit_question`.

Es gilt:

> **Im `DRAFT` ist alles Konfiguration. Erst mit Publish wird daraus ein konkretes, unveränderliches Audit.**

Ein `DRAFT` darf auch dann gespeichert werden, wenn die aktuelle Konfiguration zu keinem prüfbaren Inhalt führt.

Die Vorschau kann in diesem Fall beispielsweise anzeigen:

```text
0 prüfbare Dokumentelemente
0 Auditfragen
```

Ein Publish ist dann nicht zulässig.

> **Ein Audit ohne mindestens eine prüfbare Frage darf nicht auf `READY` gesetzt werden.**


Die Materialisierung ist transaktional:

> **Entweder das Audit wird vollständig und konsistent materialisiert und auf `READY` gesetzt, oder es bleibt unverändert im Zustand `DRAFT`.**


### Zustand `READY`

`READY` bedeutet:

> **Das Audit ist vollständig konfiguriert, materialisiert und kann durch einen Auditor bearbeitet werden.**

Ab diesem Zeitpunkt sind insbesondere nicht mehr veränderbar:

- `audit_unit_id`,
- `catalog_version_id`,
- die materialisierte Zusammensetzung aus `audit_document_elements` und `audit_questions`,
- die Audit-Einstellungen einschließlich der Response-Policy-Regeln,
- die zu diesem Audit gehörenden Snapshots.

Die Zusammensetzung des Audits wird nachträglich nicht erneut dynamisch berechnet.

Spätere Änderungen an:

- der Audit Unit,
- deren Scope Type,
- Gewichtungen,
- neueren Katalogversionen

verändern dieses Audit nicht.


### Zustand `IN_PROGRESS`

Die erste beantwortete `audit_question` führt automatisch zu:

```text
READY → IN_PROGRESS
```

Damit ist eindeutig erkennbar, dass die fachliche Bearbeitung des Audits begonnen hat.

## Antwortlogik

### Zulässige Ergebnisse

| Wert | Bedeutung |
|---|---|
| `JA` | geforderter Zustand ist erfüllt |
| `NEIN` | geforderter Zustand ist nicht oder nicht vollständig erfüllt |
| `NICHT_ANWENDBAR` | Frage ist im konkreten Einzelfall sachlich nicht anwendbar |
| `NICHT_FESTSTELLBAR` | Frage ist anwendbar, Zustand konnte aber nicht belastbar festgestellt werden |
| `NULL` | noch nicht beantwortet |

Es gibt keinen separaten Ergebniswert `OFFEN`.

Ob zu einer Antwort zusätzlich ein Kommentar oder Nachweis erforderlich ist, wird **nicht global durch den Antwortwert festgelegt**, sondern durch die Response-Policy-Regeln in den Audit-Einstellungen.

### Fachliche Regeln

- `NEIN` bedeutet immer Abweichung.
- Risikobewertung erfolgt nicht durch den Antwortwert selbst.
- `NICHT_ANWENDBAR` und `NICHT_FESTSTELLBAR` bleiben eigenständige fachliche Ergebnisse.
- Kommentar- und Nachweispflichten werden ausschließlich durch die Response-Policy-Regeln in den Audit-Einstellungen bestimmt.
- `answered_at` und `answered_by` werden automatisch gesetzt bzw. aktualisiert.

### Response-Policy-Regeln innerhalb der Audit-Einstellungen

Die Response-Policy-Regeln sind Bestandteil der `audit_settings` des konkreten Audits.

Der `AUDIT_MANAGER` legt im Zustand `DRAFT` für jeden Antwortwert fest, ob zusätzlich ein Kommentar und/oder ein Nachweis erforderlich ist.

Beispiel:

| Ergebnis | Kommentar | Nachweis |
|---|---|---|
| `JA` | optional | optional |
| `NEIN` | optional | optional |
| `NICHT_ANWENDBAR` | erforderlich | optional |
| `NICHT_FESTSTELLBAR` | erforderlich | optional |

Die Regeln dürfen zusätzlich von der Gewichtung des zugrunde liegenden prüfbaren Dokumentelements abhängen.

Beispiel:

```text
Gewicht 1–3:
  NEIN → Kommentar optional

Ab Gewicht >= 4:
  NEIN → Kommentar erforderlich
```

Es gibt keine global fest verdrahtete Kommentar- oder Nachweispflicht.

Die `audit_settings` sind im `DRAFT` veränderbar und werden mit dem Publish unveränderlich. Ein separates Response-Policy-Objekt oder ein eigener Response-Policy-Snapshot ist nicht vorgesehen.

### Ermittlung des Ergebnisses eines prüfbaren Dokumentelements

Das Ergebnis eines prüfbaren Dokumentelements wird deterministisch aus allen zugehörigen `audit_questions` berechnet.

| Zustand der zugehörigen Fragen | Ergebnis des prüfbaren Dokumentelements |
|---|---|
| mindestens eine Frage `NEIN` | `NICHT_ERFÜLLT` |
| kein `NEIN`, aber mindestens eine Frage `NICHT_FESTSTELLBAR` | `NICHT_FESTSTELLBAR` |
| mindestens eine anwendbare Frage `JA` und alle übrigen Fragen `JA` oder `NICHT_ANWENDBAR` | `ERFÜLLT` |
| alle Fragen `NICHT_ANWENDBAR` | `NICHT_ANWENDBAR` |

`NICHT_ANWENDBAR`-Fragen werden bei der Beurteilung der anwendbaren Teile eines prüfbaren Dokumentelements nicht als negative Feststellung gewertet.

Beispiel:

```text
JA
JA
NICHT_ANWENDBAR
```

führt zum Ergebnis des prüfbaren Dokumentelements:

```text
ERFÜLLT
```

Die Gewichtung eines prüfbaren Dokumentelements beeinflusst dieses Ergebnis **nicht**.

---

## Bearbeitungszustand und Konsistenz einer Auditfrage

Eine `audit_question` besitzt keinen eigenen persistierten Lebenszyklusstatus.

Der Bearbeitungszustand ergibt sich direkt aus ihren Daten und dem Zustand des übergeordneten Audits.

### Unbeantwortete Auditfrage

Für eine unbeantwortete Auditfrage gilt konsistent:

```text
result IS NULL
comment IS NULL
evidence IS NULL
answered_at IS NULL
answered_by IS NULL
```

Damit bedeutet `result IS NULL` eindeutig:

> Diese Auditfrage enthält noch keine fachliche Antwort.

### Beantwortete Auditfrage

Sobald `result` gesetzt wird, werden automatisch:

```text
answered_at
answered_by
```

gesetzt.

Wird die Antwort später innerhalb eines bearbeitbaren Audits geändert, werden diese beiden Felder auf die letzte Änderung aktualisiert.

`comment` und `evidence` richten sich nach den Audit-Einstellungen. Sie können abhängig vom Antwortwert und den dort definierten Regeln optional oder verpflichtend sein.

### Zurücksetzen einer Antwort

Eine beantwortete Auditfrage darf innerhalb eines noch bearbeitbaren Audits zurückgesetzt werden.

Dabei werden gemeinsam geleert:

```text
result
comment
evidence
answered_at
answered_by
```

Es bleiben keine halbfertigen Antwortdaten an einer unbeantworteten Frage zurück.

### Bearbeitbarkeit über den Auditstatus

Auditfragen dürfen bearbeitet oder zurückgesetzt werden, solange sich das Audit in einem bearbeitbaren Zustand befindet:

```text
READY
IN_PROGRESS
```

Bei:

```text
FINALIZED
CANCELED
```

sind Auditfragen unveränderlich.

Ein separates Finalisieren oder Sperren einzelner Auditfragen ist nicht vorgesehen.

### Automatische Rückkehr `IN_PROGRESS → READY`

Die erste beantwortete Auditfrage führt automatisch zu:

```text
READY → IN_PROGRESS
```

Werden später alle beantworteten Auditfragen wieder zurückgesetzt, sodass für alle Fragen gilt:

```text
result IS NULL
```

wechselt das Audit automatisch zurück:

```text
IN_PROGRESS → READY
```

Damit beschreibt `IN_PROGRESS` tatsächlich nur Audits, die mindestens eine fachliche Antwort enthalten.

Ein vollständig zurückgesetztes Audit im Zustand `READY` kann gemäß den Löschregeln wieder gelöscht bzw. soft-deleted werden.

Die Zustandsänderung wird vollständig im `system_audit_log` dokumentiert. Da es sich um einen automatischen Zustandswechsel handelt, ist kein `state_reason` erforderlich.

---

## Auditbezogene Instanzen prüfbarer Kataloginhalte

### `audit_document_elements`

Ein `audit_document_element` repräsentiert ein tatsächlich prüfbares `document_element` innerhalb eines konkreten Audits.

Nur Dokumentelemente, denen mindestens eine Frage zugeordnet ist, werden für ein Audit instanziiert.

Reine Struktur- und Kontext-Elemente werden nicht redundant in das Audit übernommen.

Grundstruktur:

```text
audit
└── audit_document_elements
    └── audit_questions
```

Vorgesehene Kerndaten:

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `audit_document_element_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `audit_id` | `NOT NULL` | – | Referenz auf das konkrete Audit |
| `element_id` | `NOT NULL` | – | Referenz auf das prüfbare `document_element` |
| `weight_snapshot` | `NOT NULL` | effektives Gewicht beim Publish | festgeschriebene Gewichtung `1..5` |

`title`, `text`, Parent-Struktur und andere Kataloginhalte werden **nicht** erneut im Audit gespeichert.

Sie werden über `element_id` und die am Audit fixierte `catalog_version_id` aus dem unveränderlichen Katalog bezogen.


### `audit_questions`

Eine `audit_question` repräsentiert die konkrete Bearbeitung einer Katalogfrage innerhalb eines Audits.

Vorgesehene Kerndaten:

| Feld | Pflicht / Nullability | Default | Bedeutung |
|---|---|---|---|
| `audit_question_id` | `NOT NULL` | technisch erzeugt | interner Primärschlüssel |
| `audit_document_element_id` | `NOT NULL` | – | Referenz auf das zugehörige Audit-Dokumentelement |
| `question_id` | `NOT NULL` | – | Referenz auf die Katalogfrage |
| `result` | `NULL` erlaubt | `NULL` | Antwortwert; `NULL` = unbeantwortet |
| `comment` | `NULL` erlaubt; konditional erforderlich | `NULL` | Kommentar gemäß Response-Policy-Regeln |
| `evidence` | `NULL` erlaubt; konditional erforderlich | `NULL` | Nachweis gemäß Response-Policy-Regeln |
| `answered_at` | konditional | `NULL` | Zeitpunkt der letzten Antwortänderung |
| `answered_by` | konditional | `NULL` | Benutzer der letzten Antwortänderung |

Der Fragetext, Prüfungshinweis und Nachweishinweis werden nicht als zusätzliche Snapshots gespeichert, da die referenzierte Katalogversion nach ihrer Verwendung historisch unveränderlich ist.


### Scope-Filterung bei der Audit-Erzeugung

Ein Audit bezieht sich auf genau eine `audit_unit`.

Jede `audit_unit` besitzt genau einen `scope_type`.

Fragen besitzen über ihre Scope-Zuordnungen einen oder mehrere gültige Scope Types.

Beim Erzeugen eines Audits werden ausschließlich diejenigen Fragen übernommen, deren Scope-Zuordnung zum `scope_type` der gewählten Prüfeinheit passt.

Vereinfacht:

```text
audit_unit.scope_type
        ↓
question_scope_types
        ↓
passende questions
        ↓
audit_document_elements
        ↓
audit_questions
```

Ein `audit_document_element` wird nur dann erzeugt, wenn nach dieser Scope-Filterung mindestens eine zugehörige Frage verbleibt.

Dadurch werden nicht in das Audit übernommen:

- reine Struktur-Elemente,
- reine Kontext-Elemente,
- prüfbare Dokumentelemente, deren Fragen für den Scope der Prüfeinheit nicht relevant sind.

Beispiel:

```text
audit_unit.scope_type = TECHNICAL_AREA

Frage A → TECHNICAL_AREA, ROOM
Frage B → APPLICATION
Frage C → TECHNICAL_AREA
```

Für dieses Audit werden Frage A und Frage C übernommen. Frage B wird nicht übernommen.


### Referenzielle Konsistenz innerhalb eines Audits

Die Beziehungen zwischen Audit, Katalogversion, Dokumentelement und Frage müssen technisch konsistent bleiben.

Für jede `audit_question` gilt:

- `question_id` muss zu genau dem `element_id` gehören, das durch das übergeordnete `audit_document_element` referenziert wird.
- `element_id` muss Bestandteil der `catalog_version_id` sein, an die das Audit gebunden ist.
- `question_id` muss Bestandteil derselben `catalog_version_id` sein.
- Eine Frage aus einem anderen Dokumentelement darf nicht mit einem `audit_document_element` kombiniert werden.
- Eine Frage oder ein Dokumentelement aus einer anderen Katalogversion darf nicht in das Audit eingebunden werden.

Beispiel einer unzulässigen Kombination:

```text
audit_document_element.element_id = Element A

audit_question.question_id = Frage aus Element B
```

Auch wenn Element A und Element B derselben Katalogversion angehören, ist diese Kombination ungültig.

Diese Konsistenz ist durch geeignete Datenbank-Constraints und/oder transaktionale Anwendungslogik sicherzustellen.


### Kardinalitäten und Reihenfolge im Audit

Für die im Audit instanziierten Kataloginhalte gelten eindeutige Kardinalitäten.

#### `audit_document_elements`

Ein `audit_document_element`:

- gehört genau zu einem `audit`,
- referenziert genau ein prüfbares `document_element`,
- darf für dieselbe Kombination aus `audit_id` und `element_id` nur einmal existieren.

Konzeptionell entspricht dies einem Unique Constraint auf:

```text
(audit_id, element_id)
```

#### `audit_questions`

Eine `audit_question`:

- gehört genau zu einem `audit_document_element`,
- referenziert genau eine Katalogfrage,
- darf innerhalb desselben `audit_document_element` für dieselbe `question_id` nur einmal existieren.

Konzeptionell entspricht dies einem Unique Constraint auf:

```text
(audit_document_element_id, question_id)
```

#### Reihenfolge

Auditarium speichert keine zusätzliche Reihenfolge für:

```text
audit_document_elements
audit_questions
```

Die Reihenfolge wird aus dem referenzierten Katalog abgeleitet:

```text
document_elements.sort_order
questions.sort_order
```

Da die verwendete Katalogversion nach ihrer erstmaligen Verwendung historisch unveränderlich ist, bleibt auch diese Reihenfolge für historische Audits stabil.

Dadurch werden redundante `sort_order`-Felder auf Audit-Ebene vermieden.


### Grundsatz für Snapshots und Redundanz

Auditarium vermeidet redundante Kopien historischer Katalogdaten, wenn deren fachliche Bedeutung bereits durch stabile Referenzen und unveränderliche Versionen gesichert ist.

Grundsatz:

> **Snapshots werden nur dort gespeichert, wo sich referenzierte Daten nach Audit-Erstellung noch ändern dürfen und die historische Bedeutung des Audits sonst verloren ginge.**

Für Fremdschlüssel gilt ergänzend:

> **Übergeordnete IDs werden nur dort gespeichert, wo sie eine eigene fachliche Beziehung ausdrücken. Eindeutig ableitbare Parent-IDs werden nicht redundant mitgeführt.**


Daraus folgt:

| Information | Snapshot im Audit? | Begründung |
|---|---:|---|
| Dokumentmetadaten: Titel, Herausgeber, Herausgeber-Version, Veröffentlichungsdatum, Quelle, Notizen | Nein | bewusst bearbeitbare Angaben am `document`; auch bestehende Audits zeigen die aktuellen Werte |
| Dokumentelement: Titel, Text | Nein | Bestandteil der fixierten, historisch unveränderlichen Katalogversion |
| Dokumentstruktur | Nein | über `catalog_version_id` und `document_elements` rekonstruierbar |
| Fragetext | Nein | Bestandteil der fixierten Katalogversion |
| Prüf-/Nachweishinweise | Nein | Bestandteil der fixierten Katalogversion |
| Gewichtung | Ja | darf später durch den `AUDIT_MANAGER` geändert werden |
| Prüfeinheit | Teilweise | Nur `audit_unit_context` mit Name, `scope_type_id` und Hierarchiepfad; die `audit_unit_id` bleibt Referenz |
| Audit-Einstellungen einschließlich Response-Policy-Regeln | Nein, kein separater Snapshot | Bestandteil des Audits; im `DRAFT` veränderbar und ab `READY` unveränderlich |

Historische Stabilität wird damit bevorzugt durch technische Referenzen auf unveränderliche Katalogdaten hergestellt und nicht durch redundante Kopien derselben Inhalte.

Die Unveränderlichkeit der fachlichen Prüfgrundlage umfasst die verwendeten Kataloginhalte und die für das Audit festgelegten Snapshots. Die beschreibenden Dokumentmetadaten sind davon ausdrücklich ausgenommen; ihr aktueller Stand wird über die bestehende Dokumentreferenz gelesen. Das `system_audit_log` dokumentiert deren Änderungen, wird aber nicht zur Rekonstruktion alter Metadaten für die reguläre Auditdarstellung verwendet.

Diese Entscheidung erzeugt eine bewusste technische Abhängigkeit zwischen historischen Audits und den von ihnen referenzierten Katalogdaten.

Daher dürfen verwendete Kataloginhalte nicht physisch gelöscht oder strukturell verändert werden, solange historische Audits darauf verweisen.

---

# 14. Integritäts-, Constraint- und Indexmatrix

## Constraint-Matrix des fachlichen Kerns

Diese Matrix fasst die harten Integritätsregeln der fachlichen Kernentitäten zusammen. Querschnittstabellen für Identity/RBAC, Credentials, Application Settings, Jobs und andere technische Belange werden in ihren jeweiligen Fachkapiteln definiert und sind bewusst nicht Teil dieser Matrix.

| Entität | Harte Schlüssel / Unique Constraints | Harte Werte-/Konsistenzregeln | Referenzielle Regeln |
|---|---|---|---|
| `documents` | PK `document_id`; bewusst kein natürlicher Unique Key | `usage_state ∈ {ACTIVE, DEPRECATED}`; bei `DEPRECATED` nicht-leerer `usage_state_reason` | abhängige `catalog_versions` gehören zum Dokument-Aggregat; externe Referenzen verhindern Purge |
| `catalog_versions` | PK `catalog_version_id`; `UNIQUE(document_id, version_number)` | `version_number >= 1`; `draft_revision >= 1`; `catalog_state ∈ {DRAFT, READY}` | `document_id` gehört zum Aggregate und darf beim physischen Dokument-Purge kaskadieren; `created_by` → `users` mit `RESTRICT`; optionales `source_file_id` referenziert `FileItem`; Referenzen aus `audits` blockieren Purge |
| `document_elements` | PK `element_id`; eindeutige Reihenfolge innerhalb derselben Geschwistergruppe | `sort_order >= 0`; `parent_element_id != element_id`; `title` und `text` dürfen nicht beide leer sein | `catalog_version_id` → `catalog_versions` mit `CASCADE`; `parent_element_id` → `document_elements` mit `RESTRICT/NO ACTION` |
| `document_element_weights` | PK und FK `element_id` | `weight ∈ {1,2,4,5}`; gespeicherter Wert `3` ist unzulässig | `element_id` → `document_elements` mit `CASCADE` |
| `questions` | PK `question_id`; `UNIQUE(element_id, sort_order)` | `sort_order >= 0`; `text` nicht leer | `element_id` → `document_elements` mit `CASCADE` |
| `question_scope_types` | zusammengesetzter PK `(question_id, scope_type_id)` | keine doppelten Zuordnungen | `question_id` → `questions` mit `CASCADE`; `scope_type_id` → feste Scope Types mit `RESTRICT` |
| `scope_types` | stabiler technischer Key/ID eindeutig | ausschließlich die fest definierten Scope Types | keine benutzerseitige Anlage oder Löschung |
| `audit_units` | PK `audit_unit_id`; bewusst kein Unique Constraint auf `name` | `parent_audit_unit_id != audit_unit_id`; `usage_state ∈ {ACTIVE, INACTIVE}`; bei `INACTIVE` nicht-leerer `usage_state_reason` | Parent und Scope Type mit `RESTRICT`; bestehende Children oder Audits blockieren Purge |
| `audits` | PK `audit_id`; bewusst kein Unique Constraint auf `name` | `origin_audit_id != audit_id`; `audit_state ∈ {DRAFT, READY, IN_PROGRESS, FINALIZED, CANCELED}`; `DRAFT → audit_unit_context = NULL`; alle veröffentlichten Zustände → `audit_unit_context IS NOT NULL`; `DRAFT → assigned_auditor_user_id IS NULL`; `FINALIZED → assigned_auditor_user_id IS NULL`; `CANCELED` benötigt nicht-leeren `state_reason` | `audit_unit_id`, `catalog_version_id` und `origin_audit_id` mit `RESTRICT`; `assigned_auditor_user_id` → `users` mit `RESTRICT` |
| `audit_document_elements` | PK `audit_document_element_id`; `UNIQUE(audit_id, element_id)` | `weight_snapshot BETWEEN 1 AND 5` | `audit_id` → `audits` mit `CASCADE`; `element_id` → `document_elements` mit `RESTRICT` |
| `audit_questions` | PK `audit_question_id`; `UNIQUE(audit_document_element_id, question_id)` | `result` ist `NULL` oder einer der definierten Antwortwerte; Antwortmetadaten müssen konsistent sein | `audit_document_element_id` → `audit_document_elements` mit `CASCADE`; `question_id` und `answered_by` mit `RESTRICT` |
| `users` | PK `user_id`; `UNIQUE(username)`; `user_key` eindeutig, sofern gesetzt | `user_id >= 0`; `username` kanonisch und nicht leer; Systemuser `0` ist nicht aktiv und nicht löschbar | historische Referenzen aus Auditdaten und Audit-Log blockieren Purge |
| `authentication_providers` | PK `authentication_provider_id`; `UNIQUE(provider_key)` | `provider_key`, `provider_type` und `display_name` nicht leer; `provider_type` muss einer code-definierten ProviderDefinition entsprechen | bestehende `user_identities` sowie historische Abhängigkeiten blockieren Löschung |
| `user_identities` | PK `identity_id`; `UNIQUE(authentication_provider_id, external_id)` | `external_id` nicht leer | `user_id` → `users` mit `CASCADE` beim zulässigen physischen Benutzer-Purge; `authentication_provider_id` → `authentication_providers` mit `RESTRICT` |
| `system_audit_log` | PK `event_id`; keine fachlichen Unique Constraints | `action` und `object_type` müssen gültige kanonische technische Werte sein | `user_id` → `users` mit `RESTRICT`; `object_type + object_id` ist bewusst eine polymorphe Referenz ohne FK |

## Geschwisterreihenfolge bei `document_elements`

Die fachliche Reihenfolge muss innerhalb einer Geschwistergruppe eindeutig sein.

Für Elemente mit Parent gilt logisch:

```text
UNIQUE(parent_element_id, sort_order)
```

Für Root-Elemente gilt logisch:

```text
UNIQUE(catalog_version_id, sort_order)
WHERE parent_element_id IS NULL
```

Da PostgreSQL und Microsoft SQL Server `NULL` in Unique-Indizes unterschiedlich behandeln, wird diese fachlich identische Regel über providergeeignete partielle/gefilterte Indizes bzw. entsprechende Migrationen umgesetzt.

`sort_order` muss nicht lückenlos sein.

## Antwortkonsistenz bei `audit_questions`

Harte Mindestregel:

```text
result IS NULL
→ comment IS NULL
→ evidence IS NULL
→ answered_at IS NULL
→ answered_by IS NULL
```

und:

```text
result IS NOT NULL
→ answered_at IS NOT NULL
→ answered_by IS NOT NULL
```

Ob `comment` oder `evidence` bei einer konkreten Antwort zusätzlich verpflichtend sind, ergibt sich aus `audit_settings` und wird transaktional in der Anwendung validiert.

## Soft-Delete-Konsistenz

Für jeden Aggregate Root mit Soft Delete gilt als harte Konsistenzregel:

```text
deleted_at IS NULL
↔ deleted_by IS NULL
```

Bei nicht gelöschten Objekten muss zusätzlich gelten:

```text
deletion_reason IS NULL
```

`deleted_by` referenziert `users.user_id` mit `RESTRICT`.

## Regeln, die bewusst nicht als Datenbank-Constraint umgesetzt werden

Folgende Regeln benötigen Kontext über mehrere Datensätze oder fachliche Zustandslogik und werden deshalb in der Anwendung innerhalb derselben Transaktion validiert:

- keine Zyklen in `document_elements`,
- Parent eines `document_element` gehört zur selben `catalog_version`,
- Parent-/Child-Scope-Kombinationen bei `audit_units`,
- bei `audit_units` mit Scope `OTHER` muss `description` fachlich befüllt sein,
- keine Zyklen in `audit_units`,
- Dokumentelement mit mindestens einer Frage benötigt `text`,
- Gewichtung darf nur für tatsächlich prüfbare Dokumentelemente gepflegt werden,
- jede Frage benötigt für `DRAFT → READY` mindestens einen Scope Type,
- `origin_audit_id` verweist immer direkt auf das ursprüngliche Basis-Audit und bildet keine Kette,
- zulässige Audit-State-Transitions,
- Löschbarkeit eines Audits abhängig von Zustand und vorhandenen Antworten,
- `audit_document_elements.element_id` und `audit_questions.question_id` müssen zur am Audit fixierten Katalogversion gehören,
- `audit_questions.question_id` muss zum `element_id` des übergeordneten `audit_document_element` gehören,
- Response-Policy-Regeln aus `audit_settings`,
- Publish-, READY- und Importvalidierungen.

Diese Regeln dürfen nicht durch redundante Fremdschlüssel nur zum Zweck leichterer Constraints in das Modell zurückgebracht werden.

## Index-Grundsätze

Indizes werden aus realen Zugriffs- und Integritätsmustern abgeleitet.

Für die erste Version gelten folgende Grundregeln:

- jeder Primärschlüssel und jeder Unique Constraint besitzt den zugehörigen eindeutigen Index,
- jeder Fremdschlüssel erhält einen Index, sofern er nicht bereits als führender Bestandteil eines geeigneten bestehenden Indexes abgedeckt ist,
- `system_audit_log` erhält mindestens einen Index für die Objekthistorie:

```text
(object_type, object_id, action, occurred_at DESC)
```

- für Audit-Log-Retention wird zusätzlich ein Index auf `occurred_at` vorgesehen,
- für häufige Auditlisten werden `audit_unit_id`, `catalog_version_id` und `audit_state` indexierbar gehalten; die endgültigen zusammengesetzten Reporting-Indizes werden anhand realer Abfragen festgelegt,
- es werden keine vorsorglichen Volltext- oder JSON-Indizes ohne konkreten Abfragebedarf angelegt.

---

# 15. Auswertung, Zeitleiste, Export und Datenzugriff

## Grundsätze

### Keine rein mathematische Gesamtbewertung

Die Anwendung soll aus den Auditdaten **keinen scheinbar objektiven Gesamtscore** ableiten, der eine fachliche Gesamtbewertung ersetzt.

Insbesondere ist keine pauschale Gesamtkennzahl wie z. B.

```text
83,7 % compliant
```

als alleinige Bewertung des Sicherheits- oder Erfüllungszustands vorgesehen.

Die Gesamtbewertung einer Lage muss durch verantwortliche Personen unter Einbeziehung ihrer fachlichen Expertise erfolgen.


### Rolle der Gewichtung

Die Gewichtung dient in Auswertungen primär dazu:

- prüfbare Dokumentelemente zu sortieren,
- wichtige Abweichungen hervorzuheben,
- Verantwortlichen eine priorisierte Sicht auf Feststellungen zu geben.

Beispiel:

```text
NICHT ERFÜLLT

[5] prüfbares Dokumentelement A
[5] prüfbares Dokumentelement B
[4] prüfbares Dokumentelement C
[3] prüfbares Dokumentelement D
[1] prüfbares Dokumentelement E
```

Die Gewichtung ist damit eine **Priorisierungshilfe**, kein mathematischer Multiplikator für einen Gesamtscore.


### Trennung von Feststellung und Bewertung

Die Anwendung trennt bewusst:

1. den festgestellten Sachverhalt,
2. das daraus abgeleitete Ergebnis eines prüfbaren Dokumentelements,
3. die lokale Gewichtung/Priorisierung,
4. die fachliche Gesamtbewertung durch verantwortliche Personen.

Diese Trennung soll verhindern, dass numerische Kennzahlen fachliche Entscheidungen vortäuschen oder ersetzen.


### Freie Auswertung statt technischer Vergleichssperren

Auditarium entscheidet nicht paternalistisch, welche Audits miteinander betrachtet oder verglichen werden dürfen.

Auch fachlich ungewöhnliche Kombinationen dürfen in Auswertungen gemeinsam dargestellt werden.

Beispiele:

- gleiche Audit Unit über mehrere Zeitpunkte,
- unterschiedliche Audit Units mit gleichem Katalog,
- unterschiedliche Scope Types,
- unterschiedliche Katalogversionen,
- unterschiedliche Dokumentversionen.

Die Anwendung soll Unterschiede sichtbar machen, aber die Betrachtung nicht technisch verhindern.

Vergleichbarkeit kann als Information dargestellt werden, darf aber nicht als Sperre wirken.


### Zeitleistenansicht

Es ist eine flexible Zeitleistenansicht vorgesehen, auf der Audits über die Zeit dargestellt werden können.

Mögliche Unterscheidungs- und Gruppierungsmerkmale:

- `scope_type`,
- `audit_unit`,
- `document`,
- `catalog_version`,
- `audit_state`,
- Zeitraum.

Die Darstellung kann über:

- Farbe,
- Gruppen/Lanes,
- Symbole,
- Zusatzinformationen,
- Tooltips oder Audit-Karten

erfolgen.

Eine Farbe darf nicht gleichzeitig mehrere unterschiedliche Bedeutungen tragen. Sinnvoll ist daher eine auswählbare Farblogik, z. B.:

```text
Farbe nach:
- Scope Type
- Audit Unit
- Katalogversion
- Audit State
```

Die Zeitleiste ist eine Darstellungsform, keine fachliche Einschränkung der zugrunde liegenden Daten.


### Tabellen- und Rohdatenansicht

Neben der Zeitleiste sollen Auditdaten mindestens auch tabellarisch und als Rohdaten verfügbar sein.

Vorgesehene Ebenen:

#### Audit-Ebene

Ein Datensatz pro Audit.

#### Ebene der prüfbaren Dokumentelemente

Ein Datensatz pro `audit_document_element`.

#### Frage-Ebene

Ein Datensatz pro `audit_question`.

Dadurch können Nutzer je nach Anwendungsfall aggregierte oder detaillierte Daten weiterverarbeiten.


### Export als Kernfunktion

### Exportumfang der initialen Version

Für die erste Version gilt bewusst ein kleiner Exportumfang:

```text
Dateibasierter Export
→ CSV

Strukturierte Weiterverarbeitung / Integration
→ API
```

CSV ist das einzige initial unterstützte dateibasierte Exportformat.

Die API ist die primäre Schnittstelle für maschinenlesbare Weiterverarbeitung, Integrationen und externe Auswertungen.

Nicht Bestandteil des initialen Funktionsumfangs sind insbesondere:

```text
XLSX
PDF
formatierte Berichte
Drucklayouts
Diagramm- oder Präsentationsexporte
Management-Summaries
Report-Engines
Template-basierte Reporting-Infrastruktur
```

Auditarium erzeugt in der ersten Version keine präsentationsfertigen Management- oder Compliance-Berichte.

Grundsatz:

> **Auditarium stellt strukturierte Auditdaten bereit. Aufbereitung, Reporting und weiterführende Auswertung dürfen extern erfolgen.**

Komfort-Exportformate wie XLSX oder PDF werden erst ergänzt, wenn dafür ein konkreter fachlicher Bedarf entsteht.


Export ist keine Nebenfunktion, sondern eine der zentralen Produktsäulen von Auditarium:

```text
Import
Audit
Auswertung
Export
```

Grundsatz:

> **Auditarium sammelt, strukturiert und dokumentiert Auditdaten, bindet diese Daten aber nicht exklusiv an die Anwendung.**

Alle für Auswertungen relevanten Daten müssen auch außerhalb von Auditarium nutzbar sein.

Für die initiale Version stehen dafür der CSV-Dateiexport und der strukturierte Datenzugriff über die API gemäß dem oben festgelegten Exportumfang bereit. Ein zusätzlicher JSON-Dateiexport von Auditdaten ist für die erste Version nicht vorgesehen.

Vor der Auslieferung eines Exports muss dessen erforderlicher Eintrag im `system_audit_log` erfolgreich gespeichert sein. Schlägt die Protokollierung fehl, wird der Export mit einem Fehler abgebrochen; es werden auch keine Teile der Exportdaten übertragen. Maßgeblich sind die Ereignis- und Fehlerregeln aus Kapitel 7.


### Exportierte Kontextinformationen

Exporte dürfen nicht nur interne IDs enthalten.

Sie müssen genug lesbaren Kontext mitgeben, damit die Daten auch außerhalb von Auditarium verständlich bleiben.

Beispielhafte Felder:

```text
audit_id
audit_name
audit_date
audit_state

audit_unit_id
audit_unit_name
audit_unit_scope_type
document_title
document_version
catalog_version

element_id
element_title
element_weight
element_result

question_id
question_text
result
comment
evidence
answered_at
answered_by
```

Interne IDs werden zusätzlich mit exportiert, damit Datensätze eindeutig verknüpft und technisch weiterverarbeitet werden können.

Dokumentmetadaten wie `document_title` und `document_version` stammen aus dem aktuellen Stand des referenzierten `document`. Dies gilt auch für Ausgaben bereits abgeschlossener Audits. Die Werte aus der verwendeten Katalogversion und die Audit-Snapshots folgen dagegen ihren bestehenden Unveränderlichkeitsregeln.

Eine erneute Ausgabe desselben Audits kann deshalb nach einer Metadatenänderung aktualisierte Dokumentangaben enthalten. Oberfläche, Export und API verwenden hierfür dieselbe Semantik.


### Einheitliche Filterlogik

Interne Auswertung, Zeitleiste, Datei-Export und API sollen möglichst dieselbe Filterlogik verwenden.

Wenn ein Benutzer beispielsweise folgende Daten betrachtet:

```text
Scope Type = TECHNICAL_AREA
Dokument = X
Zeitraum = 2026–2028
```

soll genau diese Auswahl auch exportiert bzw. über die API abgefragt werden können.


### Gleichwertigkeit von API, Export und interner Auswertung

Die API soll keine künstlich reduzierte Sonderansicht liefern.

Sie soll dieselben fachlichen Daten bereitstellen, die auch:

- in der internen Auswertung,
- in Tabellenansichten,
- im CSV-Export

zur Verfügung stehen.

Lesbare Kontextinformationen und interne IDs gehören gleichermaßen zum Datenmodell der Ausgabe.


### Produktgrundsatz „Keine Sackgassen“

Für Auditarium gilt:

> **Keine Sackgassen: Alle relevanten Auditdaten müssen über Oberfläche, Datei-Export und API zugänglich sein.**

Auditarium soll Nutzer beim strukturierten Audit unterstützen, nicht sie an eine bestimmte Auswertungsform oder an das Produkt selbst binden.

---

# 16. Web-Oberfläche mit Razor Pages

## UI-/UX-Konzept

Auditarium ist ein administratives Arbeitswerkzeug und keine Dashboard-Spielwiese.

Die Oberfläche orientiert sich visuell an klassischen Admin-Templates wie **Notika** und **Adminator**, ohne eines dieser Templates als verpflichtende technische oder architektonische Grundlage festzuschreiben.

Grundsatz:

> **Notika und Adminator dienen als visuelle Referenz, nicht als Architektur- oder Framework-Abhängigkeit.**

Falls später konkrete Assets, Styles, Komponenten oder Quellcode aus Drittanbieter-Templates übernommen werden, müssen Lizenz- und Attribution-Pflichten im bestehenden `THIRD-PARTY-NOTICES.md` berücksichtigt werden.

### Grundcharakter

Die UI soll:

```text
ruhig
funktional
konsistent
klar strukturiert
hinreichend informationsdicht
ohne unnötige Dekoration
```

sein.

Nicht vorgesehen sind:

```text
Charts ohne fachlichen Bedarf
KPI-Dashboards nur aus optischen Gründen
Animationen ohne funktionalen Mehrwert
unnötige Cards
überladene Startseiten
```

Die bereits definierte Split-Flap-/Airport-Board-Idee für die öffentliche bzw. initiale Startseite darf als bewusstes Marken-/Charakterelement bestehen bleiben. Innerhalb der eigentlichen Arbeitsoberfläche gilt dagegen funktionale Zurückhaltung.

### Grundlayout

Die Anwendung verwendet ein klassisches Admin-Layout:

```text
Desktop-first
├── linke, einklappbare Hauptnavigation
├── optionale obere Kopfzeile für globale Funktionen
├── zentrale Content-Fläche
├── Seitentitel
├── Breadcrumbs, wo sie Orientierung schaffen
└── konsistente Aktionsbereiche
```

Die Navigation soll wenige stabile Hauptbereiche enthalten und keine tief verschachtelte Menüstruktur erzeugen.

Konzeptionell:

```text
Auditarium
├── Audits
├── Audit Units
├── Regelwerke
│   ├── Dokumente
│   └── Kataloge
└── Administration
    ├── Benutzer
    ├── Rollen
    ├── Authentication Provider
    ├── Einstellungen
    ├── Jobs
    └── System
```

Die tatsächlich sichtbaren Navigationseinträge richten sich nach den Berechtigungen des aktuellen Benutzers.

Das Ausblenden nicht erlaubter Funktionen in der UI ersetzt niemals die serverseitige Autorisierung in BLL und Authorization Pipeline.

### Seitenmuster

Auditarium verwendet wiederkehrende, konsistente Seitentypen.

#### Listen

```text
Seitentitel
Filter / Suche
Tabelle
Pagination
primäre Aktion klar erkennbar
```

Listen verwenden bevorzugt Tabellen und keine dekorativen Card-Grids, wenn tabellarische Darstellung fachlich passender ist.

Serverseitige Pagination, Filterung und Sortierung bleiben die technische Grundlage.

#### Detailseiten

```text
Titel
Status
wichtige Metadaten
zulässige Aktionen
fachlicher Inhalt
```

#### Formulare

```text
sichtbare Labels
erkennbare Pflichtfelder
direkte Feldvalidierung
verständliche Fehlerzusammenfassung
Speichern / Abbrechen klar getrennt
```

Secret-Felder zeigen niemals vorhandene Secret-Werte im Klartext.

Extern überschriebene Settings zeigen Effective Value, Source und Editierbarkeit gemäß dem bestehenden Settings-Modell.

#### Gefährliche Aktionen

Destruktive oder irreversible Aktionen benötigen eine eindeutige Bestätigung und eine kurze Beschreibung der Konsequenz.

Modals werden sparsam eingesetzt. Eine normale Seite oder ein normaler POST/Redirect/GET-Flow ist vorzuziehen, wenn kein echter Modal-Mehrwert besteht.

### Tabellen und Informationsdichte

Auditarium wird zahlreiche tabellarische Verwaltungs- und Auditansichten enthalten.

Dafür gilt:

```text
klare Spalten
sinnvolle Standard-Sortierung
Filter dort, wo sie echten Nutzen bringen
verständliche Empty States
keine unnötige Anzeige technischer IDs
keine Icon-only-Aktionsleisten mit unklarer Bedeutung
```

Technische IDs dürfen verfügbar sein, sollen aber nicht prominent dargestellt werden, sofern sie für die fachliche Arbeit nicht relevant sind.

### Statusdarstellung

Zustände werden in der gesamten Anwendung konsistent dargestellt.

Beispiele:

```text
DRAFT
READY
IN_PROGRESS
FINALIZED
CANCELED

ACTIVE
INACTIVE
DEPRECATED
```

Status-Badges dürfen Farbe verwenden, aber Farbe ist niemals die einzige Information.

Text, Symbol oder Label muss die Bedeutung zusätzlich eindeutig ausdrücken.

### Audit-spezifische Bearbeitung

Die UI bildet die bestehende Assignment-Logik sichtbar ab.

Beispiele:

```text
nicht zugewiesen
→ sichtbar
→ read-only
→ bei Berechtigung: Claim-Aktion

mir zugewiesen
→ sichtbar
→ bearbeitbar entsprechend Permissions

anderem Auditor zugewiesen
→ sichtbar
→ read-only
→ eindeutige Markierung "In Bearbeitung durch <Display Name>"

FINALIZED / CANCELED
→ read-only entsprechend fachlichem Zustand
```

Die UI soll dem Benutzer nach Möglichkeit keine Aktion anbieten, von der bereits bekannt ist, dass sie fachlich oder aufgrund fehlender Permission nicht zulässig ist.

Die verbindliche Entscheidung verbleibt trotzdem immer auf Server-/BLL-Ebene.

### Responsive Verhalten

Auditarium ist **desktop-first**, aber nicht desktop-only.

Ziel:

```text
Desktop / Notebook
→ vollständige Arbeitsfähigkeit

Tablet
→ sinnvoll nutzbar, insbesondere für Auditdurchführung

kleine Smartphone-Viewports
→ grundlegende Bedienbarkeit
→ keine Verpflichtung zu vollständiger Mobile-First-Optimierung
```

Tabellen dürfen bei kleineren Viewports kontrolliert scrollen oder in geeignete responsive Darstellungen wechseln.

### Accessibility

Die UI verwendet bevorzugt semantisches HTML und native Browser-/HTML-Funktionalität.

Mindestens zu berücksichtigen:

```text
Tastaturbedienbarkeit
sichtbarer Focus
zugeordnete Labels
ausreichender Kontrast
Status nicht nur über Farbe
verständliche Fehlermeldungen
ARIA nur dort, wo natives HTML nicht ausreicht
```

Accessibility ist Bestandteil der normalen UI-Qualität und kein separates optionales Theme.

### Technische Umsetzung

Auditarium Web bleibt bei ASP.NET Core Razor Pages.

Die UI soll keine unnötige SPA-/Blazor-/SignalR-Abhängigkeit einführen.

Lokales JavaScript ist für kleine UX-Verbesserungen erlaubt, sofern die fachliche Funktion ohne clientseitigen Sonderzustand nachvollziehbar bleibt.


## Web UI

`Auditarium.Web` wird als klassische ASP.NET Core Web-Anwendung mit **Razor Pages** umgesetzt.

Die Web-Oberfläche verwendet bewusst ein einfaches Request-/Response-Modell.

Grundmuster:

```text
Browser
→ HTTP GET / POST
→ Razor Page / PageModel
→ Mediator
→ BLL
→ Result
→ HTML Response / Redirect
```

### Request/Response statt Live-UI

Auditarium benötigt für seine fachlichen Anwendungsfälle keine dauerhafte Live-Verbindung zwischen Browser und Server.

Typische Oberflächen sind:

```text
Listen
Detailansichten
Formulare
Auditbearbeitung
Import
Export
Benutzer- und Rollenverwaltung
Settings
Jobs
Reports
```

Diese Anwendungsfälle werden mit normalem HTTP Request/Response umgesetzt.

Nicht vorgesehen sind:

```text
Blazor Server / Interactive Server
serverseitige UI-Circuits
SignalR als Grundlage der Web-Oberfläche
SPA-Framework als Architekturvoraussetzung
serverseitiger UI-Session-State
```

### Post/Redirect/Get

Schreibende Formularaktionen verwenden bevorzugt das Post/Redirect/Get-Muster:

```text
GET
→ Formular anzeigen

POST
→ Command ausführen
→ bei Erfolg Redirect

GET
→ aktualisierten Zustand anzeigen
```

Damit werden unbeabsichtigte Wiederholungen schreibender Requests durch Browser-Refresh vermieden.

### Dünne `PageModel`s

Razor-`PageModel`s sind HTTP-/UI-Adapter.

Sie übernehmen insbesondere:

```text
Model Binding
Aufruf von Commands/Queries über Mediator
Abbildung erwarteter Fehler auf die UI
Auswahl von Page / Redirect / HTTP-Ergebnis
```

Sie enthalten keine Businesslogik und greifen nicht direkt auf DAL, FAL oder Infrastructure zu.

Grundsatz:

```text
Auditarium.Web
→ Mediator
→ BLL
```

### Web verwendet nicht die eigene API

`Auditarium.Web` ruft für interne fachliche Operationen nicht `Auditarium.Api` per HTTP auf.

Nicht vorgesehen:

```text
Browser
→ Auditarium.Web
→ HTTP zu Auditarium.Api
→ BLL
```

Stattdessen greifen beide UI-Einstiegspunkte unabhängig auf dieselbe BLL zu:

```text
Auditarium.Web ─┐
                ├→ Mediator → BLL
Auditarium.Api ─┘
```

Die API bleibt der externe HTTP-Zugang für API-Clients und Integrationen.

### JavaScript

JavaScript wird gezielt für lokale Komfortfunktionen eingesetzt.

Beispiele:

```text
Bestätigungsdialoge
Copy-to-Clipboard
Upload-Fortschritt
Cron-Vorschau
kleine dynamische Filter
Split-Flap-Animation
weitere klar abgegrenzte UX-Funktionen
```

Grundsatz:

> JavaScript verbessert die Oberfläche, trägt aber nicht die Anwendungsarchitektur.

Wesentliche Auditarium-Funktionen sollen weiterhin über normales Request-/Response-Verhalten funktionieren.

### Mehrinstanzbetrieb

Die Web-Oberfläche hält keinen instanzgebundenen UI-Circuit oder serverseitigen Session-State zwischen Requests.

Damit dürfen aufeinanderfolgende Requests desselben Browsers von unterschiedlichen Auditarium-Instanzen bearbeitet werden:

```text
Request 1 → Instance A
Request 2 → Instance C
Request 3 → Instance B
```

Alle Instanzen derselben Installation verwenden den bereits festgelegten gemeinsamen Data-Protection-Keyring und denselben Application-Namensraum.

Dadurch können alle Instanzen dieselben Authentication-Cookies verarbeiten.

Sticky Sessions sind für `Auditarium.Web` nicht erforderlich.

---

## Web-Formulare und InputModels

Schreibende Razor-Formulare binden nicht direkt auf BLL-Commands.

Stattdessen verwendet `Auditarium.Web` eigene Web-`InputModel`s.

Grundmuster:

```text
HTML Form
→ Razor Model Binding
→ Web InputModel
→ PageModel
→ explizites Mapping
→ BLL Command
→ Mediator
```

Beispielhaft:

```text
EditDocumentInputModel
→ UpdateDocumentCommand
```

Damit können UI-spezifische Repräsentationen verwendet werden, ohne die BLL-Verträge an HTML-Formulare zu koppeln.

## Rolle der `InputModel`s

Ein `InputModel` darf insbesondere enthalten:

```text
Textrepräsentationen von Datums-/Zahlenwerten
Checkbox-Zustände
Select-Werte
Bestätigungsfelder
temporäre Upload-Informationen
weitere reine UI-Eingabefelder
ConcurrencyVersion
```

Die BLL erhält erst nach erfolgreicher Web-Eingabeverarbeitung einen fachlich typisierten Command.

`InputModel`s sind keine persistierten Entities und keine BLL-Modelle.

## Read-only ViewModels

Für lesende Seiten dürfen Query-ViewModels aus der BLL direkt verwendet werden, wenn kein zusätzlicher Web-spezifischer Typ benötigt wird.

Beispiel:

```text
GetDocumentDetailsQuery
→ DocumentDetailsViewModel
→ Razor Page
```

Es wird kein zweites identisches Web-ViewModel nur aus formalen Gründen eingeführt.

Grundsatz:

```text
Entity
≠ Query ViewModel
≠ Form InputModel
≠ Command
```

Zusätzliche Typen entstehen nur dort, wo ihre Rollen tatsächlich unterschiedlich sind.

## Getrennte Validation-Verantwortung

Validation erfolgt in mehreren klar getrennten Stufen.

Web-Schicht:

```text
Model Binding
Bindbarkeit
Formular-/HTTP-nahe Eingabeprobleme
Parsing UI-spezifischer Repräsentationen
```

BLL-Pipeline:

```text
FluentValidation
→ Request-Struktur und Request-Inhalt
```

Handler / fachliche BLL:

```text
Zustand der Welt
Datenbankabhängige Regeln
fachliche Konflikte
```

Die bereits festgelegte Regel bleibt bestehen:

> Validatoren prüfen den Request. Businesslogik prüft die Welt, in der dieser Request ausgeführt werden soll.

## Fehlerabbildung im Web

Erwartete `AppError`s der BLL werden auf die Web-Oberfläche abgebildet.

Feldbezogener Fehler:

```text
Code   = DOCUMENT.TITLE_REQUIRED
Target = Title
```

kann auf den passenden `ModelState`-Eintrag des `InputModel`s gemappt werden.

Nicht feldbezogene Fehler werden formularweit dargestellt.

Beispiel:

```text
DOCUMENT.CONCURRENCY_CONFLICT
→ formularweite Konfliktmeldung
→ Benutzer lädt aktuellen Zustand neu
```

Erwartete Validierungs- oder Konfliktfehler werden nicht als Exceptions behandelt.

## GET-/POST-Trennung

GET und POST verwenden unterschiedliche Use Cases.

Beispiel Bearbeitungsseite:

```text
GET /Documents/Edit/17
→ GetDocumentForEditQuery
→ ViewModel
→ InputModel befüllen
→ Seite rendern
```

Schreibender Request:

```text
POST /Documents/Edit/17
→ InputModel binden
→ Web-Eingabe prüfen
→ UpdateDocumentCommand
→ BLL
```

Bei Erfolg:

```text
→ RedirectToPage(...)
```

Bei erwartetem Fehler:

```text
→ Fehler auf ModelState/UI abbilden
→ erforderliche Auswahl-/Hilfsdaten erneut laden
→ gleiche Page rendern
```

## Antiforgery

Browserbasierte schreibende Requests der Razor-Pages-Oberfläche verwenden den ASP.NET-Core-Antiforgery-Schutz.

Dies betrifft insbesondere formularbasierte Änderungen über die Cookie-authentifizierte Web-Oberfläche.

Die API verwendet ein separates Authentifizierungsmodell mit Bearer-/API-Credentials und wird nicht an das browserbasierte Antiforgery-Modell gekoppelt.

Grundsatz:

```text
Auditarium.Web
→ Cookie Authentication
→ Antiforgery für schreibende Browser-Requests

Auditarium.Api
→ API Authentication
→ kein Razor-Form-Antiforgery-Modell
```

## Cookie-/Consent-UI

Falls Auditarium optionale Cookies oder vergleichbare optionale Browser-Speicherfunktionen verwendet, kann der dafür notwendige Consent-Hinweis als normale Razor-UI-Komponente umgesetzt werden.

Beispielhaft:

```text
Shared Partial / Layout Component
→ Hinweis anzeigen
→ Auswahl per normalem Request speichern
→ Darstellung anschließend an Consent-State anpassen
```

Dafür wird kein eigenes Frontend-Framework oder separates Consent-Subsystem benötigt.

Technisch notwendige Cookies und optionale Cookies werden in der Implementierung klar getrennt behandelt.

---

# 17. HTTP-API

## API-Grundmodell

`Auditarium.Api` stellt den externen HTTP-Zugang zu Auditarium bereit.

Grundpfad:

```text
/api/v1/...
```

Die API:

- greift fachlich ausschließlich über Mediator/BLL zu,
- verwendet dieselben Commands, Queries, Permissions und fachlichen Regeln wie andere Einstiegspunkte,
- enthält keine eigene Business- oder Persistenzlogik,
- wird aus der tatsächlichen Implementierung mit OpenAPI/Swagger dokumentiert,
- unterstützt abhängig vom jeweiligen Use Case lesende und schreibende Operationen.

Grundsatz:

```text
HTTP Request
→ Authentication
→ RBAC
→ API Endpoint
→ Mediator
→ BLL
→ Result
→ HTTP Response
```

## API-Routing und Versionierung

API-Versionen werden explizit im URL-Pfad geführt.

```text
/api/v1/...
```

Breaking Changes erhalten eine neue Major-Version:

```text
/api/v2/...
```

Header-basierte oder Content-Type-basierte API-Versionierung wird für v1 nicht verwendet.

Mehrere Major-Versionen dürfen während einer kontrollierten Übergangszeit parallel existieren.

## Ressourcenorientierte URLs

Routen benennen fachliche Ressourcen als Substantive.

Beispiele:

```text
/api/v1/documents
/api/v1/documents/{documentId}

/api/v1/catalog-versions
/api/v1/catalog-versions/{catalogVersionId}

/api/v1/audits
/api/v1/audits/{auditId}

/api/v1/audit-units
/api/v1/users
/api/v1/roles
```

Controller-, Handler- oder Methodennamen werden nicht Bestandteil der öffentlichen URL.

Technische IDs dienen als eindeutige Ressourcen-IDs.

## Fachliche Action-Endpunkte

Nicht jeder Use Case wird künstlich auf CRUD reduziert.

Echte fachliche Aktionen erhalten explizite Action-Endpunkte.

Beispiele:

```text
POST /api/v1/audits/{auditId}/finalize
POST /api/v1/audits/{auditId}/cancel
POST /api/v1/audits/{auditId}/reopen

POST /api/v1/jobs/{jobKey}/run
```

Der Action-Name beschreibt einen stabilen fachlichen Use Case und keinen internen Methodennamen.

## API und CQRS

Lesende API-Endpunkte senden BLL-Queries über Mediator.

```text
GET /api/v1/documents/{documentId}
→ GetDocumentQuery
→ Mediator
→ BLL
```

Schreibende API-Endpunkte senden BLL-Commands.

```text
PUT /api/v1/documents/{documentId}
→ UpdateDocumentCommand
→ Mediator
→ BLL
```

Der API-Layer übernimmt HTTP-spezifische Aufgaben wie Binding, Statuscodes und Response-Formate.

Fachliche Entscheidungen bleiben in der BLL.

## Pagination

Listenendpunkte verwenden ein einheitliches seitenbasiertes Pagination-Modell.

Beispiel:

```text
GET /api/v1/audits?page=1&pageSize=50
```

Konzeptionelle Antwort:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 50,
  "totalCount": 0
}
```

Die maximale `pageSize` wird zentral begrenzt.

Für v1 ist als Obergrenze vorgesehen:

```text
pageSize <= 200
```

Cursor-Pagination wird erst eingeführt, wenn dafür ein konkreter fachlicher oder technischer Bedarf entsteht.

## Filter und Sortierung

Filter- und Sortiermöglichkeiten werden pro Endpoint explizit definiert.

Beispiele:

```text
?state=ACTIVE
?search=network
?sort=name
?sort=-createdAt
```

Ein führendes `-` kennzeichnet absteigende Sortierung.

Nicht vorgesehen sind frei formulierbare:

```text
LINQ-Ausdrücke
SQL-Fragmente
OData-Ausdrücke
beliebige Property-Pfade
```

Unbekannte oder nicht erlaubte Filter-/Sortierfelder werden kontrolliert abgewiesen.

## Optimistische Concurrency in der API

Schreibende API-Verträge führen die für den jeweiligen Datensatz bekannte `concurrencyVersion` mit.

Beispiel:

```json
{
  "title": "Neuer Titel",
  "concurrencyVersion": 12
}
```

Wurde der Datensatz zwischenzeitlich verändert:

```text
bekannte Version = 12
aktuelle Version = 13
→ keine stille Überschreibung
→ AppError Type = Conflict
→ HTTP 409
```

Die API führt keine automatische Zusammenführung und keinen blinden Retry bei Concurrency-Konflikten durch.

## Einheitliche API-Fehler

Erwartete Fehler verwenden die zentrale `AppError`-zu-`ProblemDetails`-Abbildung.

Grundzuordnung:

```text
Validation   → 400 Bad Request
Unauthorized → 401 Unauthorized
Forbidden    → 403 Forbidden
NotFound     → 404 Not Found
Conflict     → 409 Conflict
Unexpected   → 500 Internal Server Error
```

Die BLL kennt weiterhin keine HTTP-Statuscodes.

Stabile Error-Codes und `traceId` bleiben Bestandteil der externen Fehlerdiagnose.

## OpenAPI / Swagger

OpenAPI/Swagger ist Bestandteil des öffentlichen API-Vertrags.

Die Beschreibung wird aus der tatsächlichen Implementierung generiert und enthält mindestens:

```text
Routen
HTTP-Methoden
Request-Modelle
Response-Modelle
Statuscodes
Authentication-Anforderungen
Versionierung
```

Eine separate manuell gepflegte, potenziell abweichende API-Spezifikation wird nicht als zweite Wahrheit eingeführt.

## Interne Web-Oberfläche verwendet die API nicht

`Auditarium.Web` verwendet `Auditarium.Api` nicht als internes Backend.

Stattdessen gilt:

```text
Auditarium.Web ─┐
                ├→ Mediator → BLL
Auditarium.Api ─┘
```

Die API ist für externe HTTP-Clients bestimmt.

---

# 18. Background Jobs und Maintenance

## Job-Grundmodell

Interne Wartungsprozesse werden für v1 über ASP.NET Core `BackgroundService` ausgeführt.

Für die Berechnung zeitplanbasierter Ausführungen wird Cronos verwendet.

Auditarium führt zunächst kein zusätzliches Scheduler-Framework wie Hangfire oder Quartz ein.

Grundsatz:

> Zeitplanung, Trigger, Ausführung und Businesslogik bleiben getrennte Verantwortlichkeiten.

## Cron-basierte Zeitplanung

Jobs verwenden echte Zeitpläne statt rein relativer Intervalle.

Beispiel:

```text
Retention
→ täglich 03:00 in der konfigurierten Job-Zeitzone

FileIntegrity
→ sonntags 03:15 Europe/Berlin
```

Konzeptionelle Job-Konfiguration:

```text
Enabled
Schedule
TimeZone
RunOnStartup
MisfirePolicy
ConcurrencyPolicy
```

Damit hängt der reguläre Startzeitpunkt eines Jobs nicht davon ab, wann Auditarium zuletzt neu gestartet wurde.

Für v1 verwendet Auditarium klassische **5-Feld-Cron-Ausdrücke** ohne Sekunden:

```text
Minute Hour Day-of-Month Month Day-of-Week
```

Damit lassen sich sowohl feste Startzeiten als auch wiederkehrende Takte an definierte Uhrzeiten koppeln. Rein relative Timer wie „24 Stunden seit Prozessstart“ werden für reguläre Maintenance-Zeitpläne nicht verwendet.

## Zeitzonen

Cron-Schedules werden immer zusammen mit einer expliziten Zeitzone interpretiert.

Beispiel:

```text
Schedule = "30 2 * * *"
TimeZone = "Europe/Berlin"
```

Die Interpretation des Schedules erfolgt in der konfigurierten Zeitzone.

Interne Zeitstempel und Persistenz verwenden weiterhin die bereits festgelegten UTC-/`DateTimeOffset`-Grundsätze.

## Misfire Policy

Für normale Maintenance gilt standardmäßig:

```text
MisfirePolicy = Skip
```

Beispiel:

```text
geplanter Lauf = 02:30
Auditarium war von 01:00 bis 08:00 offline
→ Lauf wird nicht um 08:00 ungefragt nachgeholt
→ nächster regulärer Termin wird berechnet
```

Damit wird verhindert, dass verpasste Wartungsjobs nach einem Neustart zur ungünstigen Betriebszeit unerwartet Last erzeugen.

Eine spätere explizite Policy wie `RunOnce` kann ergänzt werden, wenn ein konkreter Job zwingend nachgeholt werden muss.

## Concurrency Policy für Jobs

Ein Job darf nicht mehrfach parallel laufen – weder innerhalb derselben Instanz noch über mehrere Auditarium-Instanzen hinweg.

Standard:

```text
ConcurrencyPolicy = SkipIfRunning
```

Beispiele:

```text
Cron-Lauf aktiv
+ Admin klickt „Jetzt starten“
→ zweiter Lauf wird nicht gestartet

RunOnStartup aktiv
+ regulärer Cron-Termin erreicht
→ zweiter Lauf wird nicht gestartet

Instance A führt Retention aus
+ Instance B erreicht denselben Cron-Termin
→ Instance B startet keinen zweiten Lauf
```

Die Prüfung erfolgt zentral im `JobCoordinator`.

Die instanzübergreifende Koordination verwendet den in der gemeinsamen Datenbank gespeicherten Job-Laufzeitstatus und eine zeitlich begrenzte Lease.

## Triggerarten

Jobs können grundsätzlich durch drei Triggerarten gestartet werden:

```csharp
[Flags]
public enum JobTrigger
{
    None      = 0,
    Scheduled = 1,
    Startup   = 2,
    Manual    = 4
}
```

Jeder Job definiert im Code, welche Triggerarten grundsätzlich erlaubt sind.

Beispiele:

```text
FileIntegrity
AllowedTriggers = Scheduled | Startup

Retention
AllowedTriggers = Scheduled | Manual

ManualMaintenance
AllowedTriggers = Manual
```

Damit ist auch ein rein manueller Job ohne Cron-Schedule möglich.

### Default für den Retention-Job

Der bestehende Retention-/Purge-Job verwendet im initialen Sollstand:

```text
JobKey             = Retention
AllowedTriggers    = Scheduled | Manual
Default Schedule   = 0 3 * * *
RunOnStartup       = false
MisfirePolicy      = Skip
ConcurrencyPolicy  = SkipIfRunning
```

Damit wird der Job standardmäßig einmal täglich um 03:00 Uhr in der für diesen Job konfigurierten Zeitzone angeboten.

Eine häufigere Ausführung ist für tagebasierte Retention nicht erforderlich.

Der Job wendet die jeweils konfigurierten Retention- und Purge-Regeln an. Sind einzelne Purge-Funktionen deaktiviert, führt der Job für diese Bereiche keine physische Löschung aus.

Der Job darf weiterhin manuell durch einen entsprechend berechtigten Administrator ausgelöst werden.

## Erlaubte Trigger sind Code-Eigenschaft

Die erlaubten Triggerarten sind eine Eigenschaft des Job-Use-Cases und werden im Code festgelegt.

Sie sind keine frei aktivierbare Betreiberkonfiguration.

Grundsatz:

> Die Jobdefinition legt fest, welche Trigger sicher und fachlich zulässig sind. Die Konfiguration legt nur fest, welche davon tatsächlich genutzt werden.

Ein Betreiber darf daher nicht durch Konfiguration aus:

```text
AllowedTriggers = Scheduled | Startup
```

einen manuell startbaren Job machen.

## Konfigurationsvalidierung

Betriebskonfiguration muss mit den im Code erlaubten Triggerarten konsistent sein.

Beispiele:

```text
RunOnStartup = true
aber Startup nicht in AllowedTriggers
→ Konfigurationsfehler

Schedule gesetzt
aber Scheduled nicht in AllowedTriggers
→ Konfigurationsfehler
```

Solche widersprüchlichen Konfigurationen werden nicht still ignoriert.

Sie werden beim Start erkannt und deutlich gemeldet.

## `RunOnStartup`

Ein Job kann optional nach Freigabe des normalen Application-Starts einmalig für die aktuelle Auditarium-Anwendungsversion ausgelöst werden.

Dies geschieht ausdrücklich erst nach:

```text
Process Start
→ Migrationen
→ DAL Bootstrap/Reconcile
→ Datenbankzustand gültig
→ normaler Application-Start freigegeben
→ Background Services starten
→ RunOnStartup-Prüfung
```

`RunOnStartup` gehört nicht zum privilegierten DAL-Bootstrap.

Der Job läuft bereits als normaler Systemactor unter RBAC.

Im Mehrinstanzbetrieb bedeutet `RunOnStartup` ausdrücklich nicht „einmal pro Prozessstart“.

Stattdessen gilt:

```text
Job erlaubt Startup
AND
RunOnStartup = true
AND
last_startup_version != aktuelle Auditarium-Version
→ Startup-Lauf darf versucht werden
```

Die erste Instanz, die erfolgreich die Job-Lease erwirbt, führt den Startup-Lauf aus.

Nach erfolgreicher Übernahme wird die aktuelle Anwendungsversion als `last_startup_version` gespeichert.

Weitere Instanzen derselben Version überspringen den Startup-Lauf.

Damit wird ein Startup-Job pro Job und Auditarium-Version höchstens einmal ausgeführt.

## Manueller Jobstart

Jobs, die `Manual` als erlaubten Trigger besitzen, können durch einen berechtigten Administrator ad hoc gestartet werden.

Beispiel:

```text
Retention
Status: Idle
Next run: 15.09.2026 02:30

[ Jetzt starten ]
```

Der manuelle Start ist selbst ein normal autorisierter Use Case.

Konzeptionell:

```text
RunMaintenanceJobCommand(JobKey)
→ RequiresPermission(Maintenance.Jobs.Execute)
```

Die genaue Permission-Struktur kann später weiter verfeinert werden.

## Manueller Trigger und Systemausführung

Bei einem manuellen Start sind Anforderung und Ausführung getrennt.

```text
Admin User
→ darf den Job manuell triggern?
→ RBAC

bei Erfolg:
→ JobCoordinator
→ neuer System-Ausführungskontext
→ ActorType.System
→ user_id = 0
→ System-RBAC
→ eigentlicher Job
```

Damit hängt die effektive Jobausführung nicht von den individuellen Permissions des auslösenden Administrators ab.

Der Admin darf den Start anfordern; der Job selbst besitzt nur die systemseitig vorgesehenen Fähigkeiten.

## Nachvollziehbarkeit manueller Starts

Ein manueller Trigger soll den anfordernden Benutzer nachvollziehbar machen.

Konzeptionell:

```text
user_id = <Admin User>
action = JOB_TRIGGERED
object_type = MaintenanceJob
after_state:
  job_key = Retention
  trigger = MANUAL
```

Die tatsächlichen Änderungen des anschließend laufenden Jobs werden als Systemactor mit:

```text
user_id = 0
```

protokolliert.

Damit bleibt unterscheidbar:

```text
Wer hat den Lauf angefordert?
```

und:

```text
Wer hat die internen Jobänderungen ausgeführt?
```

## `JobCoordinator`

Alle Triggerarten verwenden einen zentralen `JobCoordinator`.

Aufgaben:

```text
Job anhand JobKey auflösen
Enabled prüfen
Trigger-Zulässigkeit prüfen
Runtime-State laden
instanzübergreifende Job-Lease atomar erwerben
Doppelstart verhindern
bei langen Jobs Lease per Heartbeat verlängern
eigenen DI Scope erzeugen
System-Ausführungskontext herstellen
Mediator/BLL-Use-Case starten
Runtime-State bei Ende aktualisieren und Lease freigeben
technische Telemetrie erzeugen
```

Konzeptioneller Ablauf:

```text
Cron ───────────┐
                │
Startup ────────┼→ JobCoordinator
                │
Manual ─────────┘
                     ↓
                Validierung
                     ↓
                System Actor
                     ↓
                  Mediator
                     ↓
                    BLL
```

## Job-Fehler

Der `JobCoordinator` verwendet das bestehende Fehler-/Result-Modell.

Mögliche stabile Fehlercodes:

```text
JOB.NOT_FOUND
JOB.DISABLED
JOB.TRIGGER_NOT_ALLOWED
JOB.ALREADY_RUNNING
```

Ein manueller Start eines bereits laufenden Jobs kann beispielsweise als:

```text
Type = Conflict
→ HTTP 409
```

abgebildet werden.

## Businesslogik von Jobs

`BackgroundService`, Cronos und `JobCoordinator` enthalten keine eigentliche fachliche Maintenance-Logik.

Ein Job löst einen normalen BLL-Use-Case über Mediator aus.

Beispiel:

```text
Retention Background Trigger
→ JobCoordinator
→ System Actor
→ RunRetentionCommand
→ AuthorizationBehavior
→ Handler
```

Nach Application-Start gilt damit auch für Background Jobs die normale Architektur:

```text
System Actor
→ RBAC
→ Mediator
→ BLL
→ DAL/FAL
```

Es gibt keinen direkten `DbContext`-Bypass für Maintenance-Jobs.

## Job-Laufzeitstatus

Für v1 wird keine persistente Job-Historie mit einem Datensatz pro Ausführung eingeführt.

Der für Mehrinstanzbetrieb notwendige aktuelle bzw. letzte Laufzeitstatus wird jedoch in einer kleinen technischen Tabelle persistiert.

Konzeptionell:

```text
job_runtime_state
├── job_key
├── last_started_at
├── last_completed_at
├── last_result
├── running_instance_id
├── lease_until
├── last_startup_version
└── concurrency_version
```

`job_key` ist der stabile technische Schlüssel der im Code bekannten Jobdefinition.

Die Job-Konfiguration selbst bleibt in den zentralen App-Settings.

`job_runtime_state` ist keine Konfiguration und gehört daher nicht in `application_settings`.

### Abgeleiteter Running-State

Ein nacktes persistiertes Boolean-Feld `IsRunning` wird bewusst nicht verwendet.

Der Status wird abgeleitet:

```text
IsRunning =
    running_instance_id != NULL
    AND lease_until != NULL
    AND lease_until > UtcNow
```

Damit kann ein Prozessabsturz keinen Job dauerhaft auf `running` stehen lassen.

### Lease-Erwerb

Ein Jobstart versucht atomar eine zeitlich begrenzte Lease in der gemeinsamen Datenbank zu erwerben.

Konzeptionell:

```text
Job nicht geleast
ODER vorhandene Lease abgelaufen
→ diese Instanz darf Lease übernehmen

gültige fremde Lease vorhanden
→ kein zweiter Start
```

Der Erwerb erfolgt providerneutral über EF Core und einen bedingten Datenbank-Write.

Native PostgreSQL-Advisory-Locks oder SQL-Server-spezifische Application Locks werden dafür nicht benötigt.

### Heartbeat

Länger laufende Jobs verlängern ihre Lease in einem kurzen, konfigurationsnah festgelegten Abstand.

Beispiel:

```text
Lease Duration = 5 Minuten
Heartbeat       = 1 Minute
```

Solange die ausführende Instanz lebt, wird `lease_until` regelmäßig verlängert.

Stirbt die Instanz oder verliert dauerhaft die DB-Verbindung:

```text
kein Heartbeat
→ Lease läuft aus
→ Job wird wieder startbar
```

### Normales Jobende

Bei normalem Abschluss werden mindestens:

```text
last_completed_at
last_result
running_instance_id = NULL
lease_until = NULL
```

gesetzt.

`last_started_at` bleibt als Information über den letzten Lauf erhalten.

Die UI kann damit beispielsweise anzeigen:

```text
Status
Last start
Last completion
Last result
Running instance
Next run
```

Langfristige technische Historie und Diagnose bleiben weiterhin Aufgabe von:

```text
ILogger<T>
OpenTelemetry Logs
Metrics
Traces
```

## UI-Regeln für manuellen Start

Die UI zeigt einen manuellen Start nur, wenn beide Bedingungen erfüllt sind:

```text
Job erlaubt Trigger Manual
AND
aktueller Benutzer besitzt notwendige Permission
```

Ist `Manual` nicht erlaubt, existiert für diesen Job kein manueller Startweg.

Das Ausblenden des UI-Steuerelements ist lediglich Darstellungskomfort; die serverseitige Prüfung im `JobCoordinator` und RBAC bleibt verbindlich.

## Spätere Scheduler-Erweiterung

Ein schwereres Scheduler-/Queue-System wird erst eingeführt, wenn konkrete Anforderungen entstehen, beispielsweise:

```text
persistente Job-Queue
persistierte Trigger mit eigener Queue-Semantik
Retries über Prozessneustarts hinweg
Job-Dashboard mit langfristiger Run-Historie
benutzerdefinierte beliebige Jobs
```

Solange diese Anforderungen nicht bestehen, bleibt:

```text
BackgroundService + Cronos + JobCoordinator
```

die bevorzugte Lösung.

---

# 19. Tests, Qualität und Abnahmekriterien

## Tests

Auditarium wird durch automatisierte Tests abgesichert.

Mindestens vorgesehen sind:

- Unit Tests für fachliche BLL-Logik,
- Handler-/Feature-Tests,
- Persistenztests für EF-Konfigurationen, Constraints und Migrationen,
- Tests für FAL und externe Abstraktionen,
- Integrationstests für kritische Use Cases,
- Tests gegen beide unterstützten Datenbankprovider für providerabhängige Persistenzregeln.

Mocks/Fakes werden gezielt an Architekturgrenzen eingesetzt.

Der DbContext wird nicht durch ein eigenes Repository nur zum Zweck des Mockings versteckt.

Für Persistenztests werden reale relationale Provider bzw. geeignete Testinstanzen verwendet; der EF-InMemory-Provider ist kein Ersatz für Tests relationaler Constraints und SQL-Semantik.

## Fachlicher Referenzfall

Der erste durchgängige Referenzfall verwendet ein eigenes kleines Beispielregelwerk und einen fiktiven Serverraum. Damit werden die Abbildung von Aussagen auf Fragen, die Auditdurchführung und eine spätere Wiederholung an einem überschaubaren, reproduzierbaren Beispiel geprüft.

Das Beispielregelwerk ist eigenständig formuliert. Es ist kein offizieller BSI-Katalog und trifft keine Aussage über die vollständige Erfüllung eines externen Regelwerks.

Die nachfolgenden Festlegungen beschreiben den Referenzfall. Sie sind keine allgemeinen Produktdefaults und verpflichten nicht zur Vorbefüllung produktiver Installationen mit Beispieldaten.

### Prüfeinheit und Ausgangslage

```text
Musterorganisation [ORGANIZATION]
└── Standort Musterstadt [SITE]
    └── Gebäude A [BUILDING]
        └── Serverraum R-01 [TECHNICAL_AREA]
```

Auditiert wird ausschließlich `Serverraum R-01`. Die übergeordneten Einheiten bilden seinen Hierarchiekontext.

Der Raum besitzt zwei abschließbare Racks, eine abschließbare Zugangstür mit dokumentierter Schlüsselausgabe und einen Temperatursensor. Eine zusätzliche Sensorik für Wassereintritt ist nicht installiert. Raumverantwortliche, Zutrittsberechtigungen, Wartungsunterlagen und Kontrollnachweise werden als fiktive Beispieldaten beschrieben.

Die Prüfeinheit und das zugehörige Dokument sind `ACTIVE`.

### Beispielregelwerk und Katalog

```text
Dokument
→ Betriebsregeln für technische Räume – Referenzregelwerk

Dokumentversion
→ 1.0

Katalogversion
→ 1

Umfang
→ 5 Ordnungs-Elemente
→ 10 prüfbare Dokumentelemente
→ 2 Fragen je prüfbarem Dokumentelement
→ insgesamt 20 Fragen
```

Alle Fragen werden dem Scope Type `TECHNICAL_AREA` zugeordnet. Jedes prüfbare Dokumentelement enthält den folgenden Anforderungstext; die beiden zugehörigen Fragen prüfen jeweils einen einzelnen Sachverhalt.

| Themenbereich | Referenz | Anforderungstext |
|---|---|---|
| Zuständigkeit und Dokumentation | R01 | Für den Raum müssen eine verantwortliche Person und eine Vertretung benannt sein. |
| Zuständigkeit und Dokumentation | R02 | Der Raum muss durch eine aktuelle Beschreibung seiner Nutzung und einen aktuellen Aufstellungsplan dokumentiert sein. |
| Zutritt und Schlüssel | R03 | Die Zutrittsberechtigten müssen vollständig erfasst und ihre Berechtigungen durch die verantwortliche Person freigegeben sein. |
| Zutritt und Schlüssel | R04 | Ausgegebene Raumschlüssel müssen ihren empfangenden Personen zugeordnet und die Rückgabe bei Wegfall der Berechtigung geregelt sein. |
| Fremdzutritt und Arbeiten | R05 | Externe Besucher müssen im Raum begleitet werden; ihre Besuche müssen dokumentiert sein. |
| Fremdzutritt und Arbeiten | R06 | Wartungsarbeiten im Raum müssen vor Beginn freigegeben und ihr ordnungsgemäßer Abschluss dokumentiert werden. |
| Raumzustand | R07 | Die im Aufstellungsplan bezeichneten Zugänge zu den Racks müssen frei und vor Ort erkennbar markiert sein. |
| Raumzustand | R08 | Die Racktüren müssen abschließbar sein; im Raum dürfen keine Gegenstände ohne vorgesehenen betrieblichen Zweck gelagert werden. |
| Kontrollen und Ausstattung | R09 | Die Raumtemperatur muss ablesbar und der für diesen Raum festgelegte zulässige Temperaturbereich dokumentiert sein. |
| Kontrollen und Ausstattung | R10 | Falls Sensorik für Wassereintritt installiert ist, müssen eine dokumentierte Funktionsprüfung und ein dokumentierter Alarmierungsweg vorhanden sein. |

Alle zehn prüfbaren Dokumentelemente erhalten für diesen Referenzfall das Gewicht `3`. Die Gewichtung verändert die Ableitung ihrer Ergebnisse nicht.

### Prüffragen und erwartete Antworten

Die Tabelle beschreibt vollständig beantwortete Prüfungen an zwei unterschiedlichen Zeitpunkten. Bei jeder neuen Materialisierung beginnen sämtliche Antworten zunächst mit `NULL`.

| Frage | Prüffrage | Beispielbefund bei der Erstprüfung | Erstprüfung | Wiederholung |
|---|---|---|---|---|
| R01.1 | Ist eine verantwortliche Person für den Raum benannt? | Verantwortliche Person ist benannt. | `JA` | `JA` |
| R01.2 | Ist eine Vertretung für die verantwortliche Person benannt? | Eine Vertretung ist nachweislich nicht benannt. | `NEIN` | `JA` |
| R02.1 | Beschreibt die Raumdokumentation die aktuelle Nutzung des Raums? | Beschreibung entspricht der aktuellen Nutzung. | `JA` | `JA` |
| R02.2 | Entspricht der Aufstellungsplan der aktuellen Aufstellung der Racks? | Beide Racks sind im Plan richtig dargestellt. | `JA` | `JA` |
| R03.1 | Sind alle zum Raum zutrittsberechtigten Personen in der Berechtigtenliste erfasst? | Die Liste enthält alle Berechtigten. | `JA` | `JA` |
| R03.2 | Sind alle aufgeführten Zutrittsberechtigungen durch die verantwortliche Person freigegeben? | Für eine Berechtigung wurde nachweislich noch keine Freigabe erteilt. | `NEIN` | `JA` |
| R04.1 | Ist jeder ausgegebene Raumschlüssel einer empfangenden Person zugeordnet? | Die Schlüsselliste enthält alle ausgegebenen Schlüssel mit Zuordnung. | `JA` | `JA` |
| R04.2 | Ist die Rückgabe des Raumschlüssels bei Wegfall der Berechtigung geregelt? | Eine entsprechende Rückgaberegel ist dokumentiert. | `JA` | `JA` |
| R05.1 | Wurden externe Besucher bei den dokumentierten Besuchen im Raum begleitet? | Die Besucheraufzeichnungen und die Auskunft der Begleitpersonen bestätigen dies. | `JA` | `JA` |
| R05.2 | Sind die im Prüfzeitraum erfolgten externen Besuche dokumentiert? | Die bekannten Besuche sind vollständig dokumentiert. | `JA` | `JA` |
| R06.1 | Wurden die Wartungsarbeiten im Prüfzeitraum jeweils vor Beginn freigegeben? | Die Freigabeunterlagen sind zum Prüfzeitpunkt nicht zugänglich; der Sachverhalt lässt sich nicht verlässlich feststellen. | `NICHT_FESTSTELLBAR` | `JA` |
| R06.2 | Ist der ordnungsgemäße Abschluss der Wartungsarbeiten im Prüfzeitraum dokumentiert? | Die Abschlussprotokolle liegen vollständig vor. | `JA` | `JA` |
| R07.1 | Sind die im Aufstellungsplan bezeichneten Zugänge zu den Racks frei? | Ein abgestellter Wartungswagen blockiert einen bezeichneten Zugang. | `NEIN` | `JA` |
| R07.2 | Sind die im Aufstellungsplan bezeichneten Zugänge vor Ort erkennbar markiert? | Die Markierungen sind vorhanden und erkennbar. | `JA` | `JA` |
| R08.1 | Sind die Türen beider Racks abschließbar? | Die Schließfunktion beider Racks wurde geprüft. | `JA` | `JA` |
| R08.2 | Werden im Raum ausschließlich Gegenstände mit vorgesehenem betrieblichem Zweck aufbewahrt? | Die vorhandenen Gegenstände einschließlich des Wartungswagens haben einen betrieblichen Zweck. | `JA` | `JA` |
| R09.1 | Ist die aktuelle Raumtemperatur am vorhandenen Sensor ablesbar? | Die Temperaturanzeige lässt sich ablesen. | `JA` | `JA` |
| R09.2 | Ist der für diesen Raum festgelegte zulässige Temperaturbereich dokumentiert? | Der Temperaturbereich ist in der Raumdokumentation angegeben. | `JA` | `JA` |
| R10.1 | Liegt für die installierte Sensorik für Wassereintritt eine dokumentierte Funktionsprüfung vor? | Im Raum ist keine solche Sensorik installiert; die bedingte Anforderung greift nicht. | `NICHT_ANWENDBAR` | `NICHT_ANWENDBAR` |
| R10.2 | Ist für die installierte Sensorik für Wassereintritt der Alarmierungsweg dokumentiert? | Im Raum ist keine solche Sensorik installiert; die bedingte Anforderung greift nicht. | `NICHT_ANWENDBAR` | `NICHT_ANWENDBAR` |

Für die Wiederholung werden genau vier Befunde geändert:

- R01.2: Eine Vertretung ist inzwischen benannt.
- R03.2: Alle Zutrittsberechtigungen sind inzwischen freigegeben.
- R06.1: Die für den betrachteten Prüfzeitraum maßgeblichen Freigabeunterlagen sind verfügbar und bestätigen die Freigabe vor Arbeitsbeginn.
- R07.1: Der Wartungswagen wurde auf einen vorgesehenen Stellplatz umgesetzt; die bezeichneten Zugänge sind frei.

Die übrigen Befunde bleiben gleich. Die Beispieldaten unterscheiden damit eine nachgewiesene Abweichung, einen nicht feststellbaren Sachverhalt und eine nicht anwendbare bedingte Anforderung.

Für beide Audits wird im `DRAFT` dieselbe Response Policy festgelegt:

| Ergebnis | Kommentar | Nachweis |
|---|---|---|
| `JA` | optional | optional |
| `NEIN` | erforderlich | optional |
| `NICHT_ANWENDBAR` | erforderlich | optional |
| `NICHT_FESTSTELLBAR` | erforderlich | optional |

Pflichtkommentare enthalten mindestens die jeweilige Begründung aus dem Beispielbefund. Kommentare und Nachweise verwenden die bereits definierten Felder der Auditantwort; der Referenzfall führt keine zusätzlichen Dateianhänge ein.

### Durchgängiger Ablauf und Abnahme

1. Das Beispielregelwerk wird mit seinen Aussagen, Fragen und Scope-Zuordnungen vollständig über den manuellen Katalog-Workflow angelegt und als Katalogversion `1` auf `READY` gesetzt.
2. Ein erstes Audit für `Serverraum R-01` wird als `DRAFT` konfiguriert. Die Vorschau enthält zehn prüfbare Dokumentelemente und 20 Fragen.
3. Publish erzeugt diese zehn Audit-Dokumentelemente und 20 Auditfragen vollständig. Alle Fragen sind zunächst unbeantwortet; das Audit befindet sich in `READY`.
4. Ein berechtigter Auditor übernimmt das Audit, trägt die Erstbefunde ein und finalisiert es nach vollständiger Beantwortung und Erfüllung der Response Policy.
5. Über den bestehenden Wiederholungs-Workflow wird für dieselbe Prüfeinheit und dieselbe Katalogversion ein neues Audit angelegt. Es verweist auf das Ursprungs-Audit; Antworten, Kommentare und Nachweise beginnen leer.
6. Das neue Audit wird mit derselben Konfiguration veröffentlicht, von einem berechtigten Auditor übernommen, anhand der Wiederholungsbefunde beantwortet und finalisiert.
7. Beide abgeschlossenen Audits bleiben getrennt nachvollziehbar und liefern ihre jeweiligen Daten über Oberfläche, CSV-Export und API.

Die erwarteten Elementergebnisse folgen ausschließlich der bereits festgelegten Ableitungsregel:

| Ergebnis der Dokumentelemente | Erstprüfung | Wiederholung |
|---|---|---|
| `ERFÜLLT` | R02, R04, R05, R08, R09 | R01 bis R09 |
| `NICHT_ERFÜLLT` | R01, R03, R07 | keine |
| `NICHT_FESTSTELLBAR` | R06 | keine |
| `NICHT_ANWENDBAR` | R10 | R10 |

Aus diesen Ergebnissen wird kein Gesamt-Compliance-Score gebildet. Die späteren Antworten dürfen die Erstprüfung nicht verändern. Eine spezielle Vergleichs- oder Reportansicht ist für den Nachweis dieses Referenzfalls nicht erforderlich; die Ausgestaltung dieser Ansichten bleibt dem jeweiligen Umsetzungsschritt vorbehalten.

## Provider-Testmatrix

Jede Schemaänderung soll automatisiert gegen beide freigegebenen Provider validiert werden.

Zielablauf:

```text
PostgreSQL
→ leere DB
→ alle PostgreSQL-Migrationen anwenden
→ Bootstrap / Reconcile
→ Integration Tests

Microsoft SQL Server
→ leere DB
→ alle SQL-Server-Migrationen anwenden
→ Bootstrap / Reconcile
→ Integration Tests
```

Damit ist die Unterstützung beider Datenbanken eine kontinuierlich geprüfte Eigenschaft und keine reine Dokumentationsaussage.

## Zusätzliche Qualitätsregeln

Für die Implementierung gelten zusätzlich folgende Qualitätsziele:

- Jeder fachliche Use Case erhält mindestens Tests für Erfolgsfall und relevante erwartete Fehlerpfade.
- Zustandsübergänge, Permission-Prüfungen, Concurrency-Konflikte und Bootstrap-/Reconcile-Verhalten werden automatisiert getestet.
- Die Deaktivierung eines bereits angemeldeten Benutzers wird für lesende und schreibende Zugriffe sowie instanzübergreifend geprüft: Beim nächsten autorisierten Use Case bestehen keine effektiven Permissions mehr; Logout bleibt möglich. Ein ausstehender erzwungener Passwortwechsel darf diese Sperre nicht umgehen.
- Bestehende Browser-Sessions bleiben über Neustart, Passwortwechsel und Default-Administrator-Recovery innerhalb ihrer regulären Cookie-Gültigkeit erhalten. Ein bereits ausgestelltes Cookie wird nicht zentral invalidiert; die Deaktivierung eines Benutzers sperrt dennoch beim nächsten autorisierten Use Case wirksam und instanzübergreifend.
- Der interne Systemactor mit `user_id = 0` und `is_active = false` kann weiterhin ausschließlich die durch seine Systemrollen erlaubten Use Cases ausführen.
- Die gemeinsame Persistenz protokollierungspflichtiger Datenänderungen und ihrer Audit-Log-Einträge wird gegen beide Datenbankprovider geprüft. Ein gezielt ausgelöster Audit-Log-Schreibfehler muss die gesamte betroffene Datenänderung zurückrollen; ebenso darf bei fehlgeschlagener Fachänderung kein erfolgreiches Änderungsprotokoll bestehen bleiben.
- Tests des auditierten Speichervorgangs berücksichtigen datenbankgenerierte Objekt-IDs, umgebende Use-Case-Transaktionen und zulässige technische Wiederholungen ohne doppelte Änderungsprotokolle. Eingesetzte direkte Massenänderungspfade müssen dieselben Atomaritäts- und Protokollierungsregeln nachweisen.
- Für beide Datenbankprovider wird das Fehlerverhalten der Ereignisprotokollierung geprüft: Ohne erforderlichen Audit-Log-Eintrag werden weder Anmeldung abgeschlossen noch Exportdaten ausgeliefert. Logout bleibt auch bei Protokollierungsfehlern ausführbar; fehlgeschlagene Anmeldungen und verweigerte Zugriffe bleiben abgelehnt. Zusätzliche technische Fehlermeldungen enthalten keine Secrets oder sensiblen Nutzdaten.
- Fehlerereignisse zu zurückgerollten Vorgängen bleiben bei erfolgreicher Ereignisprotokollierung erhalten. Ein weiterer Protokollierungsfehler darf weder die Fachänderung wiederholen noch den ursprünglichen Fehler in einen Erfolg umdeuten.
- Zustandsdaten im `system_audit_log` werden gegen die zentrale Positivliste je Objekt- und Ereignistyp geprüft. Nicht freigegebene oder neu hinzugekommene Properties sowie Secrets, Credentials, Hashes, Tokens, Schlüsselmaterial und Recovery-Konfigurationswerte dürfen nicht in `before_state` oder `after_state` erscheinen. Personenbezogene Angaben werden nur bei ausdrücklich begründeter Freigabe aufgenommen.
- Migrationen werden auf einer leeren PostgreSQL- und SQL-Server-Datenbank vollständig angewendet und anschließend mit Bootstrap/Reconcile geprüft.
- Gleichzeitige Starts mehrerer Instanzen werden auf beiden Datenbankprovidern für Erstinstallation und bereits aktuelles Schema geprüft: Bootstrap/Reconcile läuft exklusiv, wartende Instanzen prüfen anschließend den aktuellen Zustand erneut, und das Initial-Credential wird nur einmal erzeugt und von der erzeugenden Instanz nach erfolgreichem Commit ausgegeben.
- Wartezeitüberschreitung, Bootstrap-Fehler und Prozessabbruch werden geprüft: Die betroffene Instanz wird nicht für den Normalbetrieb freigegeben, Änderungen bleiben atomar und eine abgebrochene Instanz blockiert spätere Starts nicht dauerhaft.
- Der dokumentierte Recovery-Ablauf setzt alle übrigen Instanzen außer Betrieb und startet genau eine Recovery-Instanz. Der Normalbetrieb wird erst nach deren Beendigung und Entfernung der Recovery-Konfiguration wieder aufgenommen.
- Recovery wird auf beiden Datenbankprovidern mit deaktiviertem Default-Administrator, bestehender LOCAL-Sperre und unvollständigem Recovery-Zielzustand geprüft: Benutzer und Administratorrolle werden wiederhergestellt, Fehlversuchszähler und LOCAL-Sperre zurückgesetzt und der Passwortwechsel erzwungen. Ein wiederholter Recovery-Start mit bereits erreichtem Zielzustand verändert weder Passwort-Hash noch Credential-Metadaten erneut.
- Nach Recovery erlaubt die Anmeldung mit dem temporären Credential ausschließlich Passwortwechsel und Logout. Erst der erfolgreiche atomare Passwortwechsel ermöglicht normalen Zugriff; eine zwischenzeitliche Benutzerdeaktivierung lässt weiterhin nur Logout zu.
- Providerneutralität wird als Testeigenschaft behandelt; eine Änderung gilt nicht als fertig, solange sie nur auf einem der beiden freigegebenen Provider funktioniert.
- Security-relevante Pfade dürfen keine Secrets, Passwörter, Tokens oder sensible Freitexte in Logs, Traces oder Fehlermeldungen ausgeben; einzige definierte Ausnahme ist die einmalige direkte stdout-Ausgabe des temporären First-Install-Credentials des Default-Administrators.
- Architekturregeln wie Default-Deny, erlaubte Abhängigkeitsrichtungen und Security-Deklaration von Mediator-Requests sollen durch Architecture Tests bzw. Analyzer abgesichert werden.

---

# 20. Empfohlene Implementierungsreihenfolge

Dieses Kapitel ist kein zusätzliches Fachmodell, sondern ein Arbeitsplan, mit dem Codex oder ein Entwicklerteam den beschriebenen Sollzustand schrittweise herstellen kann.

## 20.1 Work Package 1 – Dev-Umgebung, Solution und lauffähiger Host

Ziel:

```text
VS Code
→ Repository öffnen
→ Dev Container auf Docker + Docker Compose starten
→ reproduzierbare Toolchain verfügbar
→ Auditarium.sln bauen
→ Auditarium.Api startet
→ Auditarium.Web startet
→ DI funktioniert
→ Health-Endpunkte vorhanden
```

Umsetzen:

1. `.devcontainer/devcontainer.json` als gemeinsame Entwicklungsumgebung anlegen.
2. Docker-basierte Containerdefinition und `compose.yaml` als Referenzumgebung bereitstellen.
3. Container-/Compose-Definitionen ohne unnötige Docker-Spezialitäten gestalten, damit Podman-Kompatibilität grundsätzlich erhalten bleibt.
4. `LICENSE` mit MIT License und `THIRD-PARTY-NOTICES.md` anlegen; Projektmetadaten mit `SPDX-License-Identifier: MIT` kennzeichnen, soweit sinnvoll.
6. Solution- und Projektstruktur anlegen.
5. .NET-/C#-Version und benötigte Entwicklungswerkzeuge im Dev Container festlegen.
7. Referenzen entsprechend der Abhängigkeitsregeln setzen.
8. Mediator, FluentValidation und zentrale Pipeline-Grundstruktur integrieren.
9. `Result<T>`, `AppError`, Exception Handling und `IClock` bereitstellen.
10. minimale Health-/Observability-Grundstruktur anlegen.

Abnahme:

- ein frischer Checkout kann mit VS Code, Docker und Docker Compose in der dokumentierten Dev-Container-Umgebung gestartet werden.
- die Entwicklungs-Toolchain befindet sich reproduzierbar im Dev Container und muss nicht manuell auf dem Host nachgebaut werden.
- Solution baut reproduzierbar.
- verbotene Projektabhängigkeiten existieren nicht.
- Web und API starten ohne fachliche Features.
- Podman-Kompatibilität wird durch die gewählte Definition nicht absichtlich verhindert; eine Podman-Support-Garantie ist für diesen Stand nicht erforderlich.
- `LICENSE` enthält die MIT License.
- `THIRD-PARTY-NOTICES.md` ist vorhanden.
- Projektmetadaten verwenden, soweit sinnvoll, `SPDX-License-Identifier: MIT`.
- die initialen direkten Dependencies wurden auf Lizenzverträglichkeit mit der MIT-Distribution geprüft.

## 20.2 Work Package 2 – Persistenz, Provider und Bootstrap

1. `AuditariumDbContext` und `IAuditariumDbContext` anlegen.
2. PostgreSQL und SQL Server als freigegebene Provider konfigurieren.
3. getrennte EF-Core-Migrationssätze für PostgreSQL und Microsoft SQL Server aus dem gemeinsamen Modell sowie die Auswahl des passenden Migrationssatzes beim Start herstellen.
4. Startup-Schema-Prüfung und automatische Produktionsmigration implementieren.
5. Bootstrap-/Reconcile-Infrastruktur anlegen.
6. First-Install-Erkennung und einmalige Initial-Credential-Erzeugung für den Default-Administrator integrieren.
7. Application Settings einschließlich Seed/Reconcile implementieren.
8. Data-Protection-Keyring und `ISecretProtector` integrieren.
9. installationsweit exklusiven Bootstrap-/Reconcile-Zugriff über die Datenbank einschließlich begrenzter Wartezeit, Fehlerbehandlung und Freigabe bei Abbruch herstellen.
10. Recovery-Betreiberablauf mit vollständig gestoppter Installation und genau einer Recovery-Instanz dokumentieren.
11. vollständigen idempotenten Recovery-Zielzustand einschließlich LOCAL-Entsperrung, zurückgesetzter Fehlversuchszähler und `must_change_password` herstellen.

Abnahme:

- frische DB kann auf beiden Providern vollständig aufgebaut werden.
- eine frische Installation erzeugt genau einmal ein temporäres Initial-Credential für den Default-Administrator.
- das Initial-Credential wird ausschließlich direkt auf stdout ausgegeben und ist über `docker compose logs auditarium` im Referenzbetrieb auffindbar.
- ein Neustart erzeugt oder protokolliert kein zweites Initialpasswort.
- zweiter Startup ist idempotent.
- gleichzeitig startende Instanzen führen Bootstrap/Reconcile nacheinander aus und prüfen nach dem Warten den aktuellen Zustand erneut; dies gilt auch ohne ausstehende Migrationen.
- eine wartende oder bei der Initialisierung gescheiterte Instanz wird nicht für den Normalbetrieb freigegeben; ein Prozessabbruch hinterlässt keine dauerhaft blockierende Bootstrap-Sperre.
- der Recovery-Ablauf beschreibt den vollständigen Stillstand der übrigen Installation und den anschließenden kontrollierten Neustart.
- Recovery stellt den vollständigen Zielzustand atomar her und verändert ihn bei wiederholtem Start nicht erneut; bereits passende Passwort-Hashes bleiben auch bei Korrektur anderer Recovery-Metadaten erhalten.
- ältere Anwendung verweigert Start gegen eine neuere DB.

## 20.3 Work Package 3 – Identity, Authentication Provider und RBAC

Ziel:

```text
users
→ authentication_providers
→ user_identities
→ LOCAL / API / LDAP
→ roles
→ permissions
→ authorization
```

Umsetzen:

1. `users`, `authentication_providers` und `user_identities` entsprechend dem kanonischen Modell anlegen.
2. `AuthenticationProviderDefinition` mit initialen Typen `LOCAL`, `API` und `LDAP` implementieren.
3. systemverwaltete Einzelinstanzen `LOCAL` und `API` im Startup-Reconcile sicherstellen.
4. instanzbasierte Provider-Settings über den gemeinsamen Config-/DB-/Environment-Resolver anbinden.
5. LDAP-Provider-Instanzen mit Connection-, Bind-/Search-, Mapping-, Routing- und Provisioning-Konfiguration implementieren.
6. diagnostischen LDAP-Verbindungstest ohne fachliche Nebenwirkungen bereitstellen.
7. die drei initialen LDAP-Provisioning-Modi `EXISTING_ONLY`, `CREATE_INACTIVE` und `CREATE_ACTIVE` implementieren.
8. Rollen, Permissions, `user_roles` und `role_permissions` anlegen.
9. code-definierte Permissions und systemverwaltete Rollen reconciliieren.
10. `IPermissionEvaluator`, `ICurrentActor` und `AuthorizationBehavior` integrieren.
11. LOCAL- und API-Credential-Stores entsprechend den festgelegten Regeln implementieren.
12. `must_change_password` einschließlich serverseitig erzwungenem Passwortwechsel für temporäre LOCAL-Credentials implementieren.
13. `AuthenticationRouter` gegen konkrete Provider-Instanzen und deren effektive Routing-Konfiguration aufbauen.

Abnahme:

- LOCAL und API existieren jeweils genau einmal und sind systemverwaltet.
- mehrere LDAP-Provider-Instanzen können parallel konfiguriert werden.
- `user_identities` referenzieren eine konkrete Provider-Instanz und eine stabile `external_id`.
- Config-, DB- und Environment-Werte ergeben deterministisch dieselbe Effective-Configuration-Semantik wie alle anderen Settings.
- Provider mit Abhängigkeiten können deaktiviert, aber nicht gelöscht werden.
- LDAP-Verbindungstest erzeugt keine Benutzer, Identities oder Rollen.
- Provisioning erzeugt niemals automatische Account-Merges anhand Username, Mail oder Display Name.
- ein aktiver LOCAL-Benutzer mit `must_change_password = true` kann serverseitig ausschließlich Passwortwechsel und Logout verwenden.
- erfolgreicher erzwungener Passwortwechsel setzt `must_change_password = false` und ersetzt den bisherigen Passwort-Hash.
- temporäre Recovery-Credentials verwenden denselben erzwungenen Passwortwechsel wie Initial-Credentials; ein alter LOCAL-Lockout verhindert den wiederhergestellten Zugang nicht.
- ein Passwortwechsel oder Recovery invalidiert bestehende Browser-Sessions nicht pauschal; zentraler Sitzungswiderruf ist nicht Teil des initialen Modells.
- Authorization erfolgt ausschließlich über das bestehende Permission-/RBAC-Modell.
- die Deaktivierung eines bereits angemeldeten Benutzers entzieht beim nächsten autorisierten Use Case alle effektiven Permissions, auch bei Zugriff über eine andere Anwendungsinstanz; Logout bleibt ausführbar.
- die Aktivitätssperre regulärer und technischer/API-Benutzer beeinträchtigt nicht die weiterhin auf System-Permissions begrenzte interne Ausführung als `user_id = 0`.
## 20.4 Work Package 4 – System-Audit-Log und Querschnitt

1. `system_audit_log` implementieren.
2. relevante Entity-Änderungen als Delta protokollieren.
3. Secret-/Sensitive-Data-Maskierung umsetzen.
4. Soft-Delete-Grundmechanismus und Purge-Schutz implementieren.
5. administrativen SQL-basierten Soft-Delete-Restore als dokumentierten Betreiberweg vorsehen; keine Restore-UI/API für v1 implementieren.
6. optimistische Concurrency in den vorgesehenen Aggregate Roots integrieren.
7. gemeinsame Persistenz von Datenänderung und erforderlichen Audit-Log-Einträgen im zentralen DAL-Speichervorgang herstellen; mehrschrittige Abläufe bei Bedarf durch eine gemeinsame Transaktion absichern.
8. ausdrücklichen Ereignisprotokollierungsweg für Login, Logout, abgelehnte Zugriffe, Exporte und fehlgeschlagene Vorgänge einschließlich des Fehlerverhaltens gemäß Kapitel 7 bereitstellen und in die jeweiligen Abläufe integrieren.
9. zentrale Positivlisten für die als Zustandsdaten zulässigen Properties je Objekt- und Ereignistyp anlegen und deren Einhaltung automatisiert prüfen.

Abnahme:

- relevante Änderungen sind nachvollziehbar.
- protokollierungspflichtige Datenänderungen und ihre Audit-Log-Einträge werden gemeinsam committed oder gemeinsam zurückgerollt; ein Protokollierungsfehler darf keinen erfolgreichen Abschluss der Änderung hinterlassen.
- normale Handler verwenden weiterhin `SaveChangesAsync()` über `IAuditariumDbContext`; ein zusätzliches `IUnitOfWork` oder Repository ist nicht erforderlich.
- ein Audit-Log-Schreibfehler verhindert den Abschluss einer Anmeldung und die Auslieferung eines Exports; Logout bleibt möglich und abgelehnte Zugriffe bleiben abgelehnt. Protokollierungsfehler werden zusätzlich technisch gemeldet.
- Zustandsdaten enthalten ausschließlich ausdrücklich freigegebene fachlich relevante Properties; sensible Werte und nicht freigegebene, neue Properties werden nicht protokolliert.
- keine Secret-Werte erscheinen im Audit-Log.
- Soft-Delete-Restore ist kein normaler UI-/API-Use-Case; das Administratorhandbuch beschreibt den gezielten DB-basierten Ausnahmeweg.
- Concurrency-Konflikte überschreiben keine Fremdänderungen.

## 20.5 Work Package 5 – Katalogkern

In dieser Reihenfolge implementieren:

```text
documents
→ catalog_versions
→ document_elements
→ questions
→ scope_types / question_scope_types
→ document_element_weights
→ DRAFT-Editor
→ READY-Validierung
```

Danach Originaldateiablage über FAL anbinden.

Abnahme:

- ein Dokument kann angelegt und seine Metadaten können unabhängig von der Verwendung zugehöriger Katalogversionen bearbeitet werden.
- die zugehörigen Katalogversionen können als vollständige DRAFTs gepflegt werden.
- ein gültiger DRAFT kann `READY` werden.
- eine benutzte Katalogversion bleibt historisch stabil.
- auch redaktionelle Änderungen an verwendeten Kataloginhalten erfolgen ausschließlich in einer neuen DRAFT-Kopie.

## 20.6 Work Package 6 – Audit Units und Audit Engine

Reihenfolge:

```text
audit_units
→ audit DRAFT
→ Vorschau / Scope-Matching
→ Publish
→ audit_document_elements
→ audit_questions
→ Auditor-Zuweisung / Claim / Release
→ exklusive Antwortbearbeitung
→ READY ↔ IN_PROGRESS
→ FINALIZED / CANCELED / Reopen
```

Abnahme:

- ein Audit kann gegen eine Audit Unit und eine READY-Katalogversion erzeugt werden.
- Materialisierung und Snapshot-Regeln entsprechen diesem Dokument.
- veröffentlichte Audits bleiben für leseberechtigte Benutzer sichtbar, auch wenn sie einem anderen Auditor zugewiesen sind.
- ein freies `READY`-/`IN_PROGRESS`-Audit kann atomar geclaimt werden.
- nur der zugewiesene Auditor kann bei vorhandenen Bearbeitungs-Permissions fachliche Auditinhalte ändern.
- der zugewiesene Auditor kann seine eigene Zuweisung lösen, ohne Audit-State oder Antworten zu verändern.
- `AUDIT_MANAGER` kann Zuweisungen setzen, ändern und lösen.
- `FINALIZED` besitzt niemals eine aktive Auditor-Zuweisung.
- alle Antwort-, Assignment- und Zustandsregeln werden erzwungen.

## 20.7 Work Package 7 – Import

1. versioniertes Importformat definieren.
2. dazugehörigen Hilfsprompt bereitstellen.
3. Upload, Parser und Importvalidierung implementieren.
4. Importbericht/Vorschau implementieren.
5. kontrollierten vollständigen und teilweisen Apply implementieren.
6. `draft_revision`-Schutz testen.

Abnahme:

- Upload oder Validierung verändert keinen DRAFT.
- fehlerhafte Teilbäume gelangen nicht in die DB.
- Apply ist transaktional.

## 20.8 Work Package 8 – Web und API

Web:

```text
Razor Pages
→ Listen / Details
→ InputModels / Formulare
→ Permission-gesteuerte UI
→ PRG / Antiforgery
```

API:

```text
/api/v1
→ Resources
→ Queries / Commands
→ Pagination / Filter / Sort
→ ProblemDetails
→ OpenAPI
```

Abnahme:

- Web verwendet niemals die eigene API als Backend.
- API und Web führen dieselben BLL-Use-Cases aus.

## 20.9 Work Package 9 – Jobs und Betriebsfunktionen

1. `BackgroundService` + Cronos integrieren.
2. Jobdefinitionen und erlaubte Trigger implementieren.
3. `JobCoordinator` und DB-basierte Lease implementieren.
4. Startup-/Scheduled-/Manual-Trigger integrieren.
5. Maintenance-Jobs für tatsächlich benötigte Retention-/Cleanup-Aufgaben ergänzen; der Retention-Job besitzt standardmäßig den täglichen Schedule `0 3 * * *` und bleibt manuell startbar.

Abnahme:

- derselbe Job kann instanzübergreifend nicht parallel laufen.
- abgestürzte Instanz blockiert einen Job nicht dauerhaft.
- `RunOnStartup` läuft pro Job und Anwendungsversion höchstens einmal.

## 20.10 Work Package 10 – Auswertung, Export und Hardening

1. CSV-Export umsetzen und strukturierte Weiterverarbeitung über die API sicherstellen.
2. Tabellen-/Zeitleistenansichten gemäß fachlichen Regeln aufbauen.
3. Provider-Testmatrix und Integrationstests vervollständigen.
4. Security-, Telemetrie- und Retention-Regeln prüfen.
5. offene Punkte nur nach bewusster Entscheidung in den Sollzustand übernehmen.

Abnahme:

- kein künstlicher Gesamtscore wird erzeugt.
- relevante Auditdaten sind über UI, Export und API zugänglich.
- PostgreSQL und SQL Server bestehen dieselben fachlichen Integrationstests.
- der fachliche Referenzfall aus Kapitel 19 kann von der manuellen Katalogpflege bis zur finalisierten Erstprüfung und Wiederholung einschließlich CSV-Export und API-Datenzugriff durchlaufen werden.

---

# 21. Noch offene Themen

Folgende Themen sind für die weitere Konzeption offen:


- Detailgestaltung der Reports, Zeitleiste und Visualisierungen
- Managementsicht / Kennzahlen unter Beachtung des Verbots einer rein mathematischen Gesamtbewertung
- fachliche Bewertung von Abweichungen durch verantwortliche Personen
- Audit-Historie und Vergleich über Zeit
- abschließender fachlicher Review des Zustands-, Soft-Delete-, Aggregate- und Retention-Modells
- Detailausgestaltung von Audit-Log-Eventtypen und Purge-Jobs
- vollständiges versioniertes Import-Schema und Hilfsprompt für externe Werkzeuge, abgeleitet aus dem manuellen DRAFT-Workflow

---

## Umgang mit offenen Punkten

Offene Punkte sind bewusst noch nicht Teil des verbindlichen Sollzustands. Codex oder eine Implementierung darf hierfür keine weitreichende Produktentscheidung stillschweigend erfinden. Für die erste Implementierung ist entweder eine konservative, leicht austauschbare technische Zwischenlösung zu wählen oder die Entscheidung vor Umsetzung explizit nachzuholen.

---

# Anhang A – Änderungshistorie

## Änderungen in Version 0.113

- Upload und Download von Originaldokumenten sind initial jeweils auf 100 MiB begrenzt. Die nicht UI-editierbaren Betreiber-Settings `Files:OriginalDocuments:MaxUploadSize` und `Files:OriginalDocuments:MaxDownloadSize` verwenden Bytewerte und können über Konfiguration oder Environment angepasst werden.

## Änderungen in Version 0.112

- Soft Delete ist auf fachliche Aggregate Roots ab Work Package 5 beschränkt. Identity-, RBAC-, Credential-, Setting- und sonstige technische Konfigurationsobjekte verwenden ihre jeweils eigenen Lifecycle-Mechanismen und werden nicht soft gelöscht.

## Änderungen in Version 0.111

Die Entscheidungen für Work Package 2 wurden verbindlich festgelegt:

- Der installationsweite Bootstrap-/Reconcile-Schutz verwendet den EF-Core-Initialisierungsschutz. Der nicht UI-editierbare Betreiberwert `Auditarium:Database:BootstrapTimeoutSeconds` hat initial 180 Sekunden und kann über Config oder Environment angepasst werden.
- Der Default-Administrator erhält den initialen Benutzernamen `Administrator`; technisch wird er kanonisch als `administrator` gespeichert.


## Änderungen in Version 0.110

Gegenüber Version 0.109 wurden die Maskierungsregeln für Zustandsdaten im System-Audit-Log verbindlich festgelegt:

- `before_state` und `after_state` folgen einer zentralen Positivliste je Objekt- und Ereignistyp. Nicht ausdrücklich freigegebene, unbekannte oder später ergänzte Properties werden nicht protokolliert.
- Die automatische Änderungsfeststellung durch Entity Framework darf nicht zu einer generischen Vollserialisierung von Entities führen.
- Freigegeben werden nur Werte, die für den konkreten Änderungsnachweis erforderlich sind. Personenbezogene Angaben benötigen hierfür eine ausdrückliche, fachlich begründete Freigabe; technische Referenzen werden bevorzugt.
- Secrets, Credentials, Passwort- und Secret-Hashes, Tokens, Schlüsselmaterial und Recovery-Konfigurationswerte sind ausnahmslos ausgeschlossen. Sie erscheinen auch nicht maskiert, gehasht oder in abgeleiteter Form im Audit-Log.
- Jede Freigabe oder Änderung einer Zustandsproperty wird zusammen mit dem technischen Modell dokumentiert und automatisiert geprüft.
- Die Regel unterstützt Datenminimierung und den Schutz sensibler Daten. Rechtmäßige Verarbeitung, Retention, Betreiberkonfiguration und organisatorische Maßnahmen bleiben eigenständige Anforderungen.
- Qualitätsziele und Work Package 4 wurden ergänzt; der offene Punkt zu Maskierungsregeln wurde aus Kapitel 21 entfernt.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.109

Gegenüber Version 0.108 wurde die Gültigkeit bestehender Browser-Sessions festgelegt:

- Authentication-Cookies bleiben innerhalb ihrer regulären Gültigkeitsdauer über normale Anwendungsneustarts hinweg gültig; der gemeinsame Data-Protection-Keyring ermöglicht dies auch im Mehrinstanzbetrieb.
- Passwortwechsel und Default-Administrator-Recovery widerrufen bestehende Browser-Sessions nicht pauschal.
- Auditarium führt für den initialen Stand keine serverseitige Session-Tabelle, keinen Security-Stamp und keine zentrale Cookie-Invalidierung ein.
- Die zentrale Aktivitäts- und Berechtigungsprüfung bleibt die wirksame Zugriffskontrolle. Eine Benutzerdeaktivierung sperrt beim nächsten autorisierten Use Case unabhängig von Cookie, Neustart oder Anwendungsinstanz.
- Der Verzicht auf Sitzungswiderruf ist bewusst: Ein Passwortwechsel beendet bereits ausgestellte Browser-Sessions nicht. Bei künftigem Bedarf wird eine zentrale Sitzungsinvalidierung als eigenständige Erweiterung ergänzt.
- Session-Regeln, Qualitätsziele und Work Package 3 wurden entsprechend ergänzt.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.108

Gegenüber Version 0.107 wurde der vollständige Zielzustand der Default-Administrator-Recovery festgelegt:

- Recovery setzt den LOCAL-Fehlversuchszähler sowie Beobachtungsfenster und zeitliche Anmeldesperre zurück. Nach dem Neustart gelten die normalen Schutzregeln für neue Anmeldeversuche weiterhin.
- Das Recovery-Passwort ist ein temporäres Übergangs-Credential mit `must_change_password = true` und `password_changed_at = NULL`. Nach der Anmeldung sind bis zum erfolgreichen Passwortwechsel ausschließlich Passwortwechsel und Logout zulässig.
- Der bestehende Mechanismus für temporäre Initial-Credentials wird wiederverwendet. Der erfolgreiche Passwortwechsel ersetzt den Hash, setzt den Änderungszeitpunkt und hebt die Passwortwechselpflicht atomar auf.
- Die Idempotenzprüfung umfasst den vollständigen Recovery-Zielzustand. Bereits passende Passwort-Hashes werden auch dann nicht erneut erzeugt, wenn noch andere Recovery-Metadaten korrigiert werden müssen.
- Die widersprüchliche Beschränkung der Recovery auf das Credential wurde bereinigt: Zum bestehenden Umfang gehören auch die Reaktivierung des Default-Administrators und die Wiederherstellung seiner `SYSTEM_ADMIN`-Zuordnung. Andere Credentials werden nicht zurückgesetzt.
- Bootstrap-Ablauf, Qualitätsregeln und Work Packages 2 und 3 wurden entsprechend ergänzt.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.107

Gegenüber Version 0.106 wurden der gleichzeitige Start mehrerer Anwendungsinstanzen und der Recovery-Betrieb präzisiert:

- Bootstrap/Reconcile wird zusätzlich zur Migrationskoordination installationsweit exklusiv über die gemeinsame Datenbank ausgeführt. Dies gilt auf beiden Providern auch bei bereits aktuellem Schema.
- Weitere startende Instanzen warten begrenzt und prüfen nach Erwerb des exklusiven Zugriffs den dann aktuellen Zustand erneut. Dadurch werden insbesondere konkurrierende Erzeugungen oder Ersetzungen des initialen Administrator-Credentials verhindert.
- Eine Instanz wird erst nach erfolgreicher eigener Initialisierung betriebsbereit. Zeitüberschreitung oder Initialisierungsfehler führen zum Startabbruch; Prozessabbrüche dürfen keine dauerhaft verwaiste Bootstrap-Sperre hinterlassen.
- Nur die Instanz, die das Initial-Credential erfolgreich committed hat, gibt das temporäre Initialpasswort aus.
- Recovery findet bei vollständig gestoppter übriger Installation mit genau einer Recovery-Instanz statt. Erst nach deren Beendigung und Entfernung der Recovery-Konfiguration werden die gewünschten Instanzen wieder im Normalbetrieb gestartet.
- Die Datenbankkoordination sichert den Bootstrap; der vollständige Stillstand während Recovery bleibt ein verbindlicher Betreiberablauf. Die Bootstrap-Sperre ersetzt keine Abschaltung bereits laufender Instanzen.
- Die konkrete Sperrtechnik und Wartezeit werden im Persistenz-Arbeitspaket bestimmt. Ein zusätzlicher Infrastruktur-Dienst oder ein allgemeines Cluster-Subsystem ist nicht erforderlich.
- Startup-Reihenfolge, First-Install-Regeln, Qualitätsziele und Work Package 2 wurden entsprechend ergänzt.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.106

Gegenüber Version 0.105 wurde das Fehlerverhalten der Ereignisprotokollierung ohne zugehörige Datenänderung festgelegt:

- Eine erfolgreiche Anmeldung wird erst nach Speicherung des erforderlichen Audit-Log-Eintrags abgeschlossen. Bei einem Protokollierungsfehler wird keine neue authentifizierte Sitzung bzw. kein Authentication-Cookie ausgegeben.
- Ein Export wird nur nach erfolgreicher Ereignisprotokollierung ausgeliefert; bei einem Protokollierungsfehler werden auch keine Teile der Exportdaten übertragen.
- Logout beendet die eigene Browseranmeldung auch bei einem Audit-Log-Schreibfehler. Fehlgeschlagene Anmeldungen und verweigerte Zugriffe bleiben unabhängig vom Erfolg ihrer Protokollierung abgelehnt.
- Die Entscheidung stellt Nachvollziehbarkeit vor die Freigabe von Zugang und Exportdaten, erhält aber jederzeit die Möglichkeit zur Abmeldung und die Wirksamkeit einer Zugriffsverweigerung.
- Ereignisse ohne Entity-Änderung werden ausdrücklich erfasst und zentral im DAL gespeichert. Die Abgrenzung zur automatischen Änderungsprotokollierung und zur technischen Observability wurde entsprechend präzisiert.
- Protokollierungsfehler werden zusätzlich technisch gemeldet, ohne Secrets oder sensible Nutzdaten offenzulegen. Technische Telemetrie ersetzt keinen erforderlichen Audit-Log-Eintrag.
- Fehlerereignisse zu zurückgerollten Vorgängen werden außerhalb der zurückgerollten Fachtransaktion gespeichert; ein weiterer Protokollierungsfehler ändert deren ursprüngliches Fehlerergebnis nicht.
- Logout- und Exportregeln, Qualitätsziele und Work Package 4 wurden um das beschlossene Verhalten ergänzt.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.105

Gegenüber Version 0.104 wurde die gemeinsame Speicherung protokollierungspflichtiger Datenänderungen und ihrer Audit-Log-Einträge verbindlich festgelegt:

- Datenänderung und zugehöriges Änderungsprotokoll werden in derselben Datenbanktransaktion gespeichert. Schlägt einer der erforderlichen Schritte fehl, wird die gesamte betroffene Speichereinheit zurückgerollt; eine unprotokollierte Änderung darf nicht als erfolgreich abgeschlossen gelten.
- Die zugesagte Nachvollziehbarkeit rechtfertigt den zusätzlichen Aufwand für die zentrale Persistenzkoordination und deren Tests.
- Normale Handler verwenden weiterhin `SaveChangesAsync()` über `IAuditariumDbContext`. Das DAL kapselt erforderliche zusätzliche Speicherschritte, etwa zur Aufnahme datenbankgenerierter Objekt-IDs in das Protokoll, und berücksichtigt vorhandene Use-Case-Transaktionen.
- Die Transaktionsregeln wurden um diesen zentralen auditierten Speichervorgang präzisiert. Die Entscheidungen gegen ein zusätzliches Repository, `IUnitOfWork` und einen generischen `TransactionBehavior` bleiben bestehen.
- Direkte Massenänderungen über `ExecuteUpdateAsync()` oder `ExecuteDeleteAsync()` sind für protokollierungspflichtige Daten nur mit ausdrücklich abgesicherter Protokollierung und gemeinsamer Transaktion zulässig. Andernfalls wird der getrackte Speicherweg verwendet.
- Qualitätsregeln und Work Package 4 wurden um Fehler-, Transaktions-, ID- und Wiederholungsprüfungen auf beiden Datenbankprovidern ergänzt.
- Das Fehlerverhalten für Ereignisse ohne zugehörige Datenänderung und für die Protokollierung fehlgeschlagener Vorgänge bleibt einer gesonderten Festlegung vorbehalten. Technische Telemetrie, physische Dateioperationen und die bestehende Bootstrap-Abgrenzung werden durch diese Entscheidung nicht neu geregelt.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.104

Gegenüber Version 0.103 wurde die Wirkung einer Benutzerdeaktivierung auf bestehende Anmeldungen festgelegt:

- Die zentrale Berechtigungsprüfung berücksichtigt bei regulären und technischen/API-Benutzern den aktuellen Aktivierungszustand. Inaktive Benutzer besitzen keine effektiven Permissions.
- Eine gespeicherte Deaktivierung wirkt spätestens beim nächsten autorisierten Use Case für lesende und schreibende Zugriffe, unabhängig von Anmeldeverfahren und angesprochener Anwendungsinstanz. Ein noch gültiges Cookie oder API-Credential erhält keine Zugriffsrechte.
- Logout bleibt als ausdrücklich erlaubte, permissionunabhängige Aktion der eigenen Browseranmeldung möglich. Antiforgery-Schutz und Audit-Protokollierung gelten weiterhin.
- Ein noch ausstehender erzwungener Passwortwechsel hebt die Aktivitätssperre nicht auf; deaktivierten Benutzern bleibt auch in diesem Fall ausschließlich Logout.
- Der bewusst nicht anmeldbare interne Systembenutzer mit `user_id = 0` und `is_active = false` bleibt von der Aktivitätssperre für Benutzerzugriffe ausgenommen. Seine internen Vorgänge unterliegen weiterhin dem begrenzten System-RBAC.
- Session-Beschreibung, Qualitätsregeln und Abnahmekriterien des Identity-/RBAC-Arbeitspakets wurden entsprechend ergänzt. Eine zusätzliche serverseitige Session-Tabelle ist dafür nicht erforderlich.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.103

Gegenüber Version 0.102 wurden verbliebene widersprüchliche Angaben zum Exportumfang in Kapitel 15 bereinigt:

- Die ältere Aufzählung von CSV und JSON als vorgesehenen Dateiexportformaten wurde an die bereits in Version 0.97 getroffene Entscheidung angepasst: CSV ist das einzige initiale Dateiexportformat; strukturierte Weiterverarbeitung erfolgt über die API.
- Ein zusätzlicher JSON-Dateiexport von Auditdaten ist für die erste Version nicht vorgesehen. Die frühere Nennung wurde auch aus der Gleichwertigkeitsregel für API, Export und interne Auswertung entfernt.
- Der doppelte Hinweis auf einen späteren XLSX-Export wurde entfernt. Die bestehende Regel, zusätzliche Komfortformate erst bei konkretem fachlichem Bedarf zu ergänzen, bleibt bestehen.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen; bereinigt wurden redaktionelle Überreste früherer Exportvorgaben.

## Änderungen in Version 0.102

Gegenüber Version 0.101 wurde die Bearbeitbarkeit von Dokumentmetadaten von der Unveränderlichkeit verwendeter Kataloginhalte ausdrücklich abgegrenzt:

- Die Dokumentmetadaten `title`, `publisher`, `version`, `publication_date`, `source` und `notes` bleiben auch nach Freigabe oder Verwendung zugehöriger Katalogversionen bearbeitbar. Einzelne dieser Felder erhalten keine verwendungsabhängige Bearbeitungssperre.
- Metadatenänderungen werden am bestehenden Dokument unter den normalen Berechtigungs-, Validierungs-, Concurrency- und Audit-Log-Regeln gespeichert. Sie erzeugen weder eine neue Katalogversion noch einen neuen Dokumentdatensatz.
- Bestehende Audits verwenden in Oberfläche, Export und API die aktuellen Dokumentmetadaten. Zusätzliche Metadaten-Snapshots pro Katalogversion oder Audit werden nicht eingeführt.
- Die Herausgeber-Version eines Dokuments und die interne Katalogversion bleiben getrennte Begriffe.
- Die Gliederung und Inhalte einer freigegebenen Katalogversion bilden die verbindliche Prüfgrundlage gegenüber dem Auftraggeber. Eine verwendete Katalogversion bleibt einschließlich Titeln, Texten, Fragen und Hinweisen unveränderlich; auch Schreibfehler werden ausschließlich in einer neuen DRAFT-Kopie korrigiert.
- Die bestehende Rückkehrmöglichkeit einer noch unbenutzten Katalogversion von `READY` nach `DRAFT` wurde mit den Korrekturregeln konsolidiert.
- Snapshot-Übersicht, Ausgabesemantik und Abnahmekriterien für den Katalogkern wurden entsprechend präzisiert.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.101

Gegenüber Version 0.100 wurden die Betreiberwahl des DBMS, die zugehörige Migrationsstrategie, der fachliche Ausgangspunkt und ein erster durchgängiger Referenzfall festgelegt bzw. präzisiert:

- PostgreSQL und Microsoft SQL Server bleiben von Beginn an unterstützte Alternativen, damit Betreiber das für ihre Installation passende DBMS wählen können.
- Pro Installation wird genau ein Provider mit einer gemeinsamen Datenbank verwendet. Mehrere Auditarium-Instanzen derselben Installation teilen diese Auswahl; ein paralleler Betrieb beider DBMS innerhalb derselben Installation ist nicht vorgesehen.
- Das gemeinsame Fachmodell und der gemeinsame `AuditariumDbContext` bleiben erhalten.
- Die bisherige Annahme eines gemeinsamen Migrationssatzes wurde durch je einen Migrationssatz mit Model Snapshot pro Provider ersetzt. Damit wird berücksichtigt, dass die EF-Core-Werkzeuge Migrationen für den aktiven Provider erzeugen.
- Jede fachliche Modelländerung wird für beide Provider migriert und auf denselben fachlichen Zielzustand geprüft. Provider-Testmatrix und Work Package 2 wurden entsprechend präzisiert.
- BSI IT-Grundschutz sowie Vorgaben und Dokumente im Umfeld von KRITIS und NIS2 wurden als ursprünglicher fachlicher Ausgangspunkt festgehalten. Die generische Nutzbarkeit für Regelwerke anderer Fachbereiche bleibt eine verbindliche Leitplanke.
- Anforderungen eines Regelwerks werden über prüfbare Fragen für wiederholbare, nachvollziehbare und vergleichbare Audits nutzbar gemacht.
- Als erster Referenzfall wurde ein eigenes kleines Beispielregelwerk für einen fiktiven Serverraum gewählt. Zehn Anforderungen mit insgesamt 20 Fragen bilden einen überschaubaren Katalog, ohne an einen offiziellen BSI-Katalog gebunden zu sein.
- Kapitel 19 beschreibt Prüfeinheit, Anforderungen, Fragen, Beispielbefunde, Response Policy und erwartete Ergebnisse einer Erstprüfung und einer Wiederholung mit derselben Katalogversion. Work Package 10 greift diesen durchgängigen Ablauf als Abnahmekriterium auf.
- Der Referenzfall deckt alle vier fachlichen Antwortwerte ab und prüft die unveränderte Nachvollziehbarkeit des ersten Audits nach einer Wiederholung. Die spätere Detailgestaltung von Reports und Vergleichen bleibt offen.

Die übrigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.100

Gegenüber Version 0.99 wurde der Beginn der aktiven Dokumentarbeit mit Codex als Meilenstein festgehalten:

- Der bis Version 0.99 gemeinsam mit ChatGPT erarbeitete Sollstand bildet die Grundlage für die weitere Verfeinerung mit Codex.
- Die weitere Klärung erfolgt iterativ in kleinen Themenbündeln. Offene Fragen werden gemeinsam entschieden und anschließend in den aktuellen Sollzustand eingearbeitet.
- Die Arbeit beschränkt sich zunächst auf `documents/Auditarium_Soll_Pflichtenheft.md`; Struktur, Schreibstil, Begriffe und Formatierung werden fortgeführt.
- Bei jeder eingearbeiteten Entscheidung werden die betroffenen Stellen innerhalb des Dokuments auf Konsistenz geprüft.
- Pro abgeschlossenem, eingearbeitetem Themenbündel wird die Dokumentversion erhöht, das Standdatum bei Bedarf aktualisiert und die Änderungshistorie um die Entscheidungen und ihre Begründungen ergänzt.
- Die Versionszählung wird mit `0.100` nach `0.99` fortgeführt. Dieser Versionsstand markiert den Beginn der gemeinsamen Dokumentarbeit mit Codex.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.99

Gegenüber Version 0.98 wurde das UI-/UX-Konzept auf Leitplanken-Niveau festgelegt:

- Auditarium wird als funktionales administratives Arbeitswerkzeug gestaltet und nicht als Dashboard-Spielwiese.
- Notika und Adminator dienen als visuelle Referenz, aber nicht als verpflichtende technische oder architektonische Abhängigkeit.
- Bei späterer Übernahme konkreter Drittanbieter-Assets oder Komponenten sind Lizenz- und Attribution-Pflichten in `THIRD-PARTY-NOTICES.md` zu berücksichtigen.
- Das Grundlayout ist desktop-first mit linker einklappbarer Navigation, zentraler Content-Fläche, klaren Seitentiteln und Breadcrumbs, wo sie Orientierung schaffen.
- Die Navigation verwendet wenige stabile Hauptbereiche und blendet nicht erlaubte Funktionen aus, ohne die serverseitige Autorisierung zu ersetzen.
- Listen, Detailseiten, Formulare und gefährliche Aktionen erhalten konsistente wiederverwendbare Seitenmuster.
- Tabellen sind das bevorzugte Muster für listenorientierte Verwaltungsansichten; unnötige Card-Grids und Icon-only-Aktionsleisten werden vermieden.
- Statusdarstellungen werden anwendungsweit konsistent und niemals ausschließlich über Farbe kommuniziert.
- Die bestehende Audit-Assignment-Logik wird sichtbar in der UI abgebildet; fremd zugewiesene Audits bleiben lesbar, aber read-only.
- Auditarium ist desktop-first, soll aber auf Tablets sinnvoll nutzbar bleiben; vollständige Mobile-First-Optimierung für Smartphones ist kein Ziel der ersten Version.
- Semantisches HTML, Tastaturbedienbarkeit, sichtbarer Fokus, Labels, Kontrast und verständliche Fehlermeldungen gelten als normale UI-Qualitätsanforderungen.
- ASP.NET Core Razor Pages bleibt die technische Web-Basis; unnötige SPA-/Blazor-/SignalR-Abhängigkeiten werden nicht eingeführt.
- Der bisher offene Punkt „UI-/UX-Konzept“ wurde aus Kapitel 21 entfernt und gilt für den initialen Sollstand als entschieden.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.98

Gegenüber Version 0.97 wurde das Backup-/Restore-Konzept einschließlich Soft-Delete-Recovery und Retention-Job für den initialen Sollstand abgeschlossen:

- Soft Delete bleibt eine technische Schutzschicht und wird nicht als Papierkorb- oder Self-Service-Funktion in UI oder API umgesetzt.
- Für v1 gibt es keinen Restore-Dialog, keinen Restore-Button und keinen allgemeinen Restore-Endpunkt für soft gelöschte Fachobjekte.
- Das Administratorhandbuch muss geprüfte SQL-Hilfen zum Identifizieren, Prüfen und gezielten Wiederherstellen noch nicht gepurgter Aggregate Roots bereitstellen.
- Der Restore erfolgt am Aggregate Root durch kontrolliertes Zurücksetzen der Soft-Delete-Metadaten; Abhängigkeiten sind vorher zu prüfen.
- Soft Delete ersetzt ausdrücklich kein Backup.
- Auditarium implementiert in v1 keine eigene Backup- oder Disaster-Restore-Funktion.
- Als notwendiger Sicherungsumfang wurden Datenbank, FileStorage, Data-Protection-Keyring sowie die für den Wiederanlauf erforderliche externe Betreiber-Konfiguration und Secrets festgelegt.
- Auditarium gibt weder Backup-Verfahren noch Produkt, Zeitplan, Häufigkeit, Aufbewahrung, Speicherort oder Restore-Testintervalle vor.
- Auditarium erstellt, plant, überwacht oder validiert keine Backups und prüft auch nicht deren Alter oder Vollständigkeit.
- Backup und vollständige Wiederherstellbarkeit liegen vollständig in der Verantwortung des Betreibers.
- Der bestehende Retention-Job wird standardmäßig einmal täglich mit `0 3 * * *` in der konfigurierten Job-Zeitzone geplant.
- Der Retention-Job verwendet `Scheduled | Manual`, `RunOnStartup = false`, `MisfirePolicy = Skip` und `ConcurrencyPolicy = SkipIfRunning`.
- Deaktivierte Purge-Bereiche bleiben auch bei laufendem Retention-Job unangetastet.
- Work Package 4 und Work Package 9 wurden entsprechend ergänzt.
- Eine aus v0.97 verbliebene Formulierung „CSV- und JSON-Export“ in Work Package 10 wurde auf den beschlossenen Sollstand CSV + API korrigiert.
- Der bisher offene Punkt „Backup-/Restore-Konzept inklusive Verhalten bei Soft Delete und Audit-Log-Retention“ wurde aus Kapitel 21 entfernt und gilt als entschieden.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.97

Gegenüber Version 0.96 wurde der Exportumfang der initialen Version bewusst begrenzt und der offene Punkt zu Komfort-Exportformaten geschlossen:

- CSV ist das einzige initial unterstützte dateibasierte Exportformat.
- Die API ist die primäre Schnittstelle für strukturierte Weiterverarbeitung, Integrationen und externe Auswertungen.
- XLSX und PDF sind nicht Bestandteil des initialen Funktionsumfangs.
- Ebenso werden zunächst keine formatierten Berichte, Drucklayouts, Management-Summaries, Diagramm-/Präsentationsexporte oder allgemeine Report-Engine implementiert.
- Auditarium stellt strukturierte Auditdaten bereit; weiterführende Aufbereitung und Reporting dürfen extern erfolgen.
- Zusätzliche Komfortformate werden erst bei konkretem fachlichem Bedarf ergänzt.
- Der bisher offene Punkt „zusätzliche Komfort-Exportformate wie XLSX oder PDF“ wurde aus Kapitel 21 entfernt und gilt für die erste Version als entschieden.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.96

Gegenüber Version 0.95 wurde die spätere OIDC-/SAML-Integration als reine Erweiterungsregel des bestehenden Authentication-Provider-Modells festgelegt:

- OIDC und SAML sind nicht Bestandteil des initialen Funktionsumfangs.
- Eine spätere Integration erfolgt ausschließlich als weitere `AuthenticationProviderDefinition`.
- Mehrere konkrete OIDC-/SAML-Provider-Instanzen sind zulässig.
- Jede externe Identity wird weiterhin über eine konkrete Provider-Instanz und eine stabile `external_id` auf einen internen Benutzer abgebildet.
- Die bestehenden Provisioning-Modi `EXISTING_ONLY`, `CREATE_INACTIVE` und `CREATE_ACTIVE` werden wiederverwendet.
- Externe Claims bzw. Assertions dienen zunächst Identity- und Profilinformationen.
- Group-to-Role- oder Claim-to-Role-Mapping ist keine Voraussetzung und wird nicht vorsorglich in das Kernmodell eingebaut.
- OIDC/SAML dürfen keinen parallelen Benutzer-, Rollen- oder Berechtigungspfad einführen.
- `LOCAL` einschließlich des systemverwalteten Default-Administrators bleibt als unabhängiger Break-Glass-/Recovery-Zugang erhalten.
- Der lokale Recovery-/Break-Glass-Pfad darf nicht von der Verfügbarkeit externer Identity Provider oder deren Infrastruktur abhängen.
- Der bisher offene Punkt zur späteren OIDC-/SAML-Integration wurde aus Kapitel 21 entfernt und gilt architektonisch als entschieden.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.95

Gegenüber Version 0.94 wurde das First-Install-Verfahren für das initiale Credential des systemverwalteten Default-Administrators festgelegt:

- First Install und Recovery sind getrennte Betriebsfälle; eine reguläre Neuinstallation verwendet nicht den Recovery-Modus.
- Erkennt der Bootstrap bei einer frischen Installation, dass der Default-Administrator noch kein LOCAL-Credential besitzt, wird genau einmal ein kryptographisch zufälliges temporäres Initialpasswort erzeugt.
- Der Passwort-Hash wird persistiert; der Klartext wird nicht dauerhaft in DB, Datei oder separatem Bootstrap-Store gespeichert.
- Das temporäre Initialpasswort wird genau einmal direkt auf stdout des Auditarium-Hauptprozesses ausgegeben.
- Im Docker-/Compose-Referenzbetrieb ist die Ausgabe dadurch über `docker compose logs auditarium` abrufbar.
- Die Dokumentation verweist ausdrücklich auf das Auditarium-Container-Log und nicht irreführend auf eine nachträglich geöffnete Container-Shell.
- Die Initial-Credential-Ausgabe erfolgt nicht über `ILogger`, OpenTelemetry Logging, Traces, Metrics, `system_audit_log` oder Fehlerobjekte und wird damit nicht bewusst über die normale Telemetrie-Pipeline exportiert.
- Diese einmalige stdout-Ausgabe ist die einzige definierte Ausnahme von der allgemeinen Secret-/Passwort-Logging-Regel.
- Das LOCAL-Credential-Modell erhält `must_change_password`.
- Das initial erzeugte Default-Admin-Credential startet mit `must_change_password = true` und `password_changed_at = NULL`.
- Nach erfolgreichem Login mit diesem Credential sind serverseitig ausschließlich Passwortwechsel und Logout zulässig.
- Der Passwortwechsel validiert das neue Passwort gegen die normale LOCAL-Password-Policy, ersetzt den Hash, setzt `password_changed_at` und `must_change_password = false`.
- Ohne erfolgreichen Passwortwechsel ist kein normaler Betrieb mit diesem Benutzer möglich.
- Nach dem Passwortwechsel ist das im historischen Container-Log verbliebene Initialpasswort nicht mehr nutzbar.
- Ein normaler Neustart erzeugt und protokolliert kein neues Initialpasswort, auch wenn `must_change_password = true` noch gesetzt ist.
- Ist die ursprüngliche Container-Log-Ausgabe verloren, bevor das Initialpasswort geändert wurde, ist der definierte Recovery-Modus der Wiederherstellungsweg.
- Work Packages 2 und 3 wurden um First-Install-Credential, stdout-Ausgabe und erzwungenen Passwortwechsel ergänzt.
- Der bisher offene First-Install-Punkt wurde aus Kapitel 21 entfernt und gilt als entschieden.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.94

Gegenüber Version 0.93 wurde das initiale Berechtigungsmodell auf API-/Objektebene abgeschlossen und um eine exklusive Auditor-Zuweisung ergänzt:

- Auditarium verwendet im initialen Sollstand keine individuellen Objekt-ACLs, Owner-Grants oder benutzer-/rollenbezogenen Datensatzberechtigungen.
- Web, API und interne Use Cases verwenden dasselbe permission-basierte BLL-Autorisierungsmodell.
- Eine spätere organisatorische Einschränkung nach Standort, Organisation oder Audit Unit bleibt als separates mögliches Scope-Modell offen, wird aber nicht vorsorglich implementiert.
- Auditor-Zuweisungen sind ausdrücklich Workflow- und Arbeitsverantwortung, keine Berechtigungsquelle.
- `audits` erhält das nullable Feld `assigned_auditor_user_id`.
- Ein veröffentlichtes Audit kann höchstens einem Auditor gleichzeitig zur aktiven Bearbeitung zugewiesen sein.
- Freie, selbst geclaimte und fremd zugewiesene Audits bleiben für entsprechend leseberechtigte Benutzer sichtbar.
- Fremd zugewiesene Audits werden read-only dargestellt und eindeutig mit dem zuständigen Auditor gekennzeichnet.
- Ein freies `READY`- oder `IN_PROGRESS`-Audit ist read-only, bis ein berechtigter Auditor es claimt oder ein Audit Manager es zuweist.
- `Audits.Claim` erlaubt das atomare Selbst-Claimen eines freien Audits.
- Gleichzeitige Claims werden deterministisch auf genau einen erfolgreichen Claim begrenzt; weitere Versuche erhalten einen Conflict.
- `Audits.ReleaseOwn` erlaubt dem aktuell zugewiesenen Auditor, seine eigene Zuweisung zu lösen, ohne Audit-State, Antworten, Kommentare oder Nachweise zu verändern.
- `Audits.Assign` erlaubt dem Audit Manager, Zuweisungen zu setzen, umzuhängen oder zu lösen.
- Eine Zuweisung verleiht niemals zusätzliche Permissions.
- Fachliche Bearbeitung eines veröffentlichten Audits erfordert die passende Permission und die Zuweisung an den aktuellen Benutzer.
- Dazu gehören insbesondere Antworten, Änderungen, Zurücksetzen, Kommentare, Nachweise und Finalisierung.
- Beim erfolgreichen Übergang `IN_PROGRESS → FINALIZED` wird die aktive Auditor-Zuweisung in derselben Transaktion entfernt.
- `FINALIZED`-Audits besitzen als Invariante niemals eine aktive Auditor-Zuweisung.
- Historische Bearbeitung bleibt über `answered_by`, `answered_at` und `system_audit_log` nachvollziehbar.
- Rechte-Matrix, Permission-Beispiele, Constraint-Matrix und Work Package 6 wurden entsprechend aktualisiert.
- Der bisher offene Punkt „detailliertes Berechtigungskonzept auf API-/Objektebene“ wurde aus Kapitel 21 entfernt und gilt für den initialen Sollstand als entschieden.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.93

Gegenüber Version 0.92 wurde das Authentication-Provider-, LDAP- und Provisioning-Modell als verbindliche Grundlage konsolidiert:

- Provider-Typ bzw. Mechanik, konkrete Provider-Instanz und `user_identity` sind getrennte Ebenen.
- `AuthenticationProviderDefinition` wird im Code definiert und beschreibt Provider-Typ, Multiplizität, Systemverwaltung, Konfigurationsdefinitionen, Routing-Fähigkeiten und unterstützte Provisioning-Modi.
- Initial verbindliche Provider-Typen sind `LOCAL`, `API` und `LDAP`.
- `LOCAL` und `API` besitzen jeweils genau eine systemverwaltete Instanz.
- `LDAP` erlaubt mehrere administrierbare Instanzen wie `LDAP_FOO` und `LDAP_BAR`.
- Spätere Provider wie OIDC oder SAML müssen demselben Provider-Instanz-/Identity-Schema folgen.
- `user_identities` speichern nicht länger redundant `auth_type` und `provider`, sondern referenzieren `authentication_provider_id` plus stabile `external_id`.
- `(authentication_provider_id, external_id)` ist die maßgebliche Eindeutigkeitsregel.
- `authentication_providers` wurde als persistiertes Instanzmodell mit `provider_key`, `provider_type`, `display_name`, `is_enabled` und `concurrency_version` festgelegt.
- Die technische Provider-Implementierung repräsentiert den Mechanismus; die effektive konkrete Instanzkonfiguration wird über einen Provider-Kontext übergeben.
- Instanzbasierte Konfiguration verwendet `SettingDefinitionTemplate`s und den Namespace `Authentication:Providers:<provider_key>:...`.
- Authentication Provider verwenden dieselbe Config-/DB-/Environment-Prioritäts- und Quellenlogik wie alle anderen Auditarium-Settings.
- Die UI zeigt effektive Werte und Quellen und behandelt externe Overrides nach denselben Regeln wie andere Settings.
- Provider-Instanzen mit bestehenden oder historisch relevanten Abhängigkeiten können aktiviert/deaktiviert, aber nicht gelöscht werden.
- Provider-Instanzen ohne Abhängigkeiten dürfen gelöscht werden.
- `LOCAL` und `API` sind systemverwaltet und unabhängig von Abhängigkeiten nicht löschbar.
- Für LDAP wurden die Konfigurationsgruppen Allgemein, Verbindung, Bind/Suche, Benutzer-Mapping und Login-Routing als Grundlage festgelegt.
- TLS wird semantisch mindestens über `None`, `StartTls` und `Ldaps` modelliert und nicht als bloßes Boolean.
- LDAP-Provider erhalten eine diagnostische Funktion „Verbindung testen“, die Netzwerk/TLS, Bind, Base DN, Suche und Mapping prüft, aber keinerlei Benutzer-, Identity-, Rollen- oder Konfigurationsänderungen ausführt.
- LDAP-Provisioning unterstützt initial `EXISTING_ONLY`, `CREATE_INACTIVE` und `CREATE_ACTIVE`.
- `CREATE_ACTIVE` benötigt mindestens eine konfigurierte Auto-Provisioning-Rolle.
- Accounts werden niemals allein anhand gleicher E-Mail-Adresse, Username oder Display Name automatisch zusammengeführt.
- Die stabile externe ID ist die maßgebliche Identity-Zuordnung.
- Kollisionen bei der Provisionierung werden als kontrollierter Konflikt behandelt und nicht automatisch aufgelöst.
- Die Constraint-Matrix wurde auf `authentication_providers` und `UNIQUE(authentication_provider_id, external_id)` angepasst.
- Der bisher offene Punkt „LDAP-Konfigurationsdetails“ wurde aus Kapitel 21 entfernt.
- Work Package 3 wurde auf das neue Provider-/Identity-/LDAP-Modell aktualisiert.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.92

Gegenüber Version 0.91 wurde die Open-Source-Lizenzierung verbindlich festgelegt:

- Auditarium wird unter der MIT License veröffentlicht.
- `LICENSE` mit vollständigem MIT-Lizenztext ist verpflichtendes Distributionsartefakt.
- `THIRD-PARTY-NOTICES.md` ist als konsolidierte Übersicht relevanter Drittanbieter-Lizenz- und Copyright-Hinweise verpflichtend.
- `SPDX-License-Identifier: MIT` wird in geeigneten Projektmetadaten und Quelltextartefakten verwendet.
- Als Copyright-Hinweis ist zunächst `Copyright (c) 2026 Auditarium contributors` vorgesehen.
- Neue Laufzeit-, Build- und Tooling-Abhängigkeiten müssen vor Aufnahme auf Lizenzverträglichkeit mit dem MIT-Distributionsmodell geprüft werden.
- Abhängigkeiten mit unklarer oder nicht kompatibler Lizenzlage werden nicht aufgenommen.
- Zusätzliche Drittanbieter-Hinweispflichten werden über die vorgesehenen Notices erfüllt, sofern sie mit der MIT-Distribution vereinbar sind.
- Work Package 1 enthält nun ausdrücklich die Erstellung der Lizenzartefakte und die initiale License-Prüfung direkter Dependencies.
- Der bisher offene Lizenzpunkt wurde aus dem Kapitel der offenen Themen entfernt.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.91

Gegenüber Version 0.90 wurde die gemeinsame Entwicklungs- und Container-Referenzumgebung als verbindliche Leitplanke festgelegt:

- VS Code Dev Containers ist die gemeinsame reproduzierbare Entwicklungsumgebung.
- Docker und Docker Compose sind die getestete und dokumentierte Referenzplattform für die initiale Entwicklung und die ersten Einsatzumgebungen.
- Das Repository stellt eine gemeinsame `.devcontainer/devcontainer.json`, eine Docker-basierte Containerdefinition und ein `compose.yaml` bereit.
- Es werden keine parallelen Docker-/Podman-Dev-Konfigurationen gepflegt, solange kein konkreter technischer Bedarf dafür besteht.
- Container- und Compose-Definitionen sollen portable OCI-/Compose-Mechanismen bevorzugen und absichtliche Podman-Inkompatibilitäten vermeiden.
- Podman ist zunächst ein Portabilitätsziel, aber keine offiziell getestete oder zugesicherte Support-Plattform.
- Eine spätere offizielle Podman-Unterstützung setzt reale Build-/Dev-Container-/Compose-Tests voraus.
- Kubernetes ist kein Ziel der initialen Entwicklungs- oder Deployment-Stufe; Kubernetes-Manifeste oder Helm-Charts werden daher zunächst nicht parallel gepflegt.
- Das Auditarium-Container-Image und seine dokumentierten Laufzeitschnittstellen bilden den gemeinsamen Deployment-Vertrag; Docker selbst ist keine Abhängigkeit der Business-Architektur.
- Work Package 1 beginnt nun ausdrücklich mit dem Aufbau der reproduzierbaren Dev-Container-Umgebung.
- Der bisher offene Punkt „technische Zielarchitektur / Containerstruktur“ wurde aus Kapitel 21 entfernt.

Die bisherigen fachlichen und technischen Festlegungen bleiben bestehen.

## Änderungen in Version 0.90

Version 0.90 ist eine vollständige strukturelle Konsolidierung des bisherigen Konzeptstands 0.86.

- Der Dokumenttitel wurde von „Konzept- und Datenmodell“ auf „Soll- und Pflichtenheft“ umgestellt.
- Der Hauptteil wurde vollständig nach einer implementierungsorientierten Lesereihenfolge neu aufgebaut.
- Die endgültige Lesereihenfolge beginnt nach der Dokumentorientierung mit Solution und technischen Grundlagen; die ausführlichen fachlichen Produktleitplanken stehen unmittelbar vor dem Fachmodell.
- Die Dateiablage folgt nun bewusst erst auf das Katalog-Fachmodell, damit `catalog_version` und dessen Lifecycle vor `source_file_id` und FAL-Verhalten definiert sind.
- Ein kompaktes Systemkurzprofil im Dokumentvorspann liefert den fachlichen Kontext, ohne die technische Bau-Reihenfolge zu unterbrechen.
- Technische Grundlagen, Solution, Persistenz, Startup, Security und Querschnittsfunktionen stehen nun vor dem detaillierten Fachmodell.
- Fachliche und technische Beschreibungen desselben Themas wurden an jeweils einer kanonischen Stelle zusammengeführt.
- Die bisher sehr große Sammelkategorie „Technischer Architekturrahmen“ wurde auf fachlich zusammenhängende Kapitel verteilt.
- Die doppelte bzw. leere CQRS-Dispatcher-Überschrift wurde entfernt.
- Import, Web, API, Settings, Migrationen, Mehrinstanzbetrieb und Jobs wurden in eigenständige Kapitel bzw. eindeutige Querschnittskapitel einsortiert.
- Ein neues Kapitel mit einer empfohlenen Implementierungsreihenfolge und Work Packages wurde ergänzt, damit das Dokument unmittelbar als Bauplan für Codex oder ein Entwicklerteam verwendet werden kann.
- Eine kurze Dokumentkonvention für Verbindlichkeit, technische Benennung und Lesereihenfolge wurde ergänzt.
- Die vollständige bisherige Änderungshistorie bleibt erhalten, wurde aber aus dem Dokumentkopf in den Anhang verschoben.
- Die Umstrukturierung beabsichtigt keine fachliche Funktionsänderung; verbindliche Entscheidungen aus Version 0.86 bleiben erhalten, soweit sie nicht bereits dort als offen gekennzeichnet waren.
- Veraltete Restformulierungen eines persistenten Importvorgangs wurden entfernt; v1 definiert keinen eigenen fachlichen Import-Aggregate-Root.
- Im Zuge der semantischen Abschlussprüfung wurden providerneutrale JSON-Abbildung, ausschließlich migrationsbasierte Schemaänderungen, providerneutrale Benutzerstammdaten und Hash-only-Prüfung von API-Secrets wieder explizit formuliert.
- Import-Sicherheitsregeln für harte Integritätsfehler und veraltete `draft_revision` wurden im finalen Solltext nochmals ausdrücklich formuliert.
- Startup-/Recovery-Reihenfolge und transaktionaler Bootstrap wurden zu einer eindeutigen, widerspruchsfreien Sequenz konsolidiert.
- Die bislang nicht entschiedene Erzeugung des ersten Default-Admin-Credentials wurde als expliziter First-Install-Offenpunkt sichtbar gemacht, statt eine Implementierungsannahme vorzugeben.
- Die bereits beschlossene zeitplanbasierte Jobausführung wurde auf klassische 5-Feld-Cron-Schedules präzisiert; relative Prozessstart-Intervalle sind kein regulärer Maintenance-Zeitplan.
- Die finale semantische Abdeckungsprüfung hat alle technischen Identifier des bisherigen Sollteils gegen den konsolidierten Hauptteil abgeglichen; der einzige entfallene Identifier `credential_hash` war ein durch das spätere `key_id`/`secret_hash`-Modell überholter Zwischenstand.
- Beispielblöcke, Überschriftenhierarchie, Code-Fences, JSON-Syntax, Kapitelverweise und bekannte obsolete Architekturbegriffe wurden in einer abschließenden formalen QA geprüft.
- Der Status wurde nach Abschluss der Konsolidierungs- und QA-Arbeiten von Arbeitsstand auf konsolidierten Sollstand gesetzt.

## Änderungen in Version 0.86

Gegenüber Version 0.85 wurden die API-Konventionen festgelegt und ältere Read-only-Einschränkungen aus dem aktuellen Zielzustand entfernt:

- `Auditarium.Api` ist der externe HTTP-Zugang für lesende und schreibende Integrationen.
- Die API greift ausschließlich über Mediator/BLL auf fachliche Funktionen zu und enthält keine eigene Business- oder Persistenzlogik.
- API-Routen werden explizit im Pfad versioniert; die erste Version verwendet `/api/v1/...`.
- Breaking Changes führen zu einer neuen Major-API-Version, z. B. `/api/v2/...`.
- Ressourcen werden als Substantive benannt; Controller-/Handler-/Methodennamen erscheinen nicht in der URL.
- Technische IDs werden als Ressourcen-IDs verwendet.
- Echte fachliche Aktionen wie `finalize`, `cancel`, `reopen` oder ein manueller Jobstart erhalten explizite Action-Endpunkte, statt künstlich als CRUD modelliert zu werden.
- GET-Endpunkte senden BLL-Queries, schreibende Endpunkte BLL-Commands über Mediator.
- Listenendpunkte verwenden ein einheitliches seitenbasiertes Pagination-Modell mit begrenzter maximaler `pageSize`.
- Filter und Sortierfelder werden pro Endpoint explizit freigegeben; es gibt keine frei formulierbaren Query-/LINQ-/OData-Ausdrücke.
- Optimistische Concurrency wird in schreibenden API-Verträgen über `concurrencyVersion` mitgeführt.
- Concurrency-Konflikte werden als `409 Conflict` über das bestehende `ProblemDetails`-/`AppError`-Modell zurückgegeben.
- Erwartete API-Fehler verwenden konsistent die bereits festgelegte `ProblemDetails`-Abbildung.
- OpenAPI/Swagger wird aus der tatsächlichen Implementierung generiert und gilt als Teil des öffentlichen API-Vertrags.
- `Auditarium.Web` verwendet die API weiterhin nicht als internen Backend-Kanal, sondern greift direkt über Mediator auf die BLL zu.
- Die ältere Festlegung einer ausschließlich read-only API wurde im aktuellen Zielzustand entfernt.
- Die Nummerierung der Abschnitte im technischen Architekturkapitel wurde konsolidiert, nachdem durch mehrere Ergänzungen doppelte Abschnittsnummern entstanden waren.
- Die älteren KI-zentrierten Importkapitel im aktuellen Haupttext wurden auf das bereits beschlossene neutrale Modell aus versioniertem Importformat, Hilfsprompt, externer Erzeugung, Validierung und kontrolliertem Apply konsolidiert.
- Der versehentlich hinter dem Dokumentende platzierte Mehrinstanz-Block wurde wieder in den technischen Architekturrahmen einsortiert.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.85

Gegenüber Version 0.84 wurde das Formular-, ViewModel- und Validation-Modell für `Auditarium.Web` festgelegt:

- Razor-Formulare binden nicht direkt auf BLL-Commands.
- Schreibende Web-Formulare verwenden eigene Web-`InputModel`s.
- `InputModel`s dürfen UI-/HTTP-spezifische Repräsentationen enthalten, ohne BLL-Verträge zu verunreinigen.
- Der `PageModel` mappt ein gültiges `InputModel` explizit auf den jeweiligen BLL-Command.
- Read-only Query-ViewModels aus der BLL dürfen direkt für Razor-Ausgaben verwendet werden, wenn kein zusätzlicher Web-spezifischer Typ notwendig ist.
- Persistente Entities werden weder als Web-ViewModel noch als Formularmodell verwendet.
- Web-Eingabevalidierung und BLL-Validierung bleiben getrennte Verantwortlichkeiten.
- Die Web-Schicht prüft Bindbarkeit und UI-/HTTP-nahe Eingabeprobleme.
- FluentValidation prüft weiterhin die Struktur und den Inhalt des BLL-Requests.
- Fachliche Zustandsregeln bleiben im Handler bzw. in der BLL.
- Erwartete `AppError`s werden im Web auf `ModelState` bzw. formularweite Fehlermeldungen abgebildet.
- `AppError.Target` kann zur feldbezogenen Fehlerzuordnung verwendet werden.
- Erfolgreiche schreibende Formularaktionen verwenden Post/Redirect/Get.
- GET und POST verwenden getrennte Query-/Command-Pfade.
- Browserbasierte schreibende Requests verwenden ASP.NET-Core-Antiforgery-Schutz.
- Die API bleibt vom browserbasierten Antiforgery-Modell getrennt.
- Ein einfacher Cookie-/Consent-Hinweis für optionale Cookies kann als normale Razor-UI-Komponente bzw. Partial ergänzt werden; dafür wird kein eigenes Frontend-Subsystem benötigt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.84

Gegenüber Version 0.83 wurde die Web-UI-Architektur festgelegt:

- `Auditarium.Web` wird als klassische ASP.NET Core Web-Anwendung mit Razor Pages umgesetzt.
- Auditarium verwendet für die eigene Web-Oberfläche bewusst kein Blazor-Server-/Interactive-Server-Modell und keine SPA-Architektur.
- Das UI-Modell basiert auf klassischem HTTP Request/Response.
- Schreibende Formularaktionen verwenden bevorzugt das Post/Redirect/Get-Muster.
- Razor-`PageModel`s bleiben dünne HTTP-/UI-Adapter und enthalten keine Businesslogik.
- Fachliche Aktionen der Web-Oberfläche laufen direkt über `Mediator` in die BLL.
- `Auditarium.Web` ruft für eigene fachliche Operationen nicht die eigene HTTP-API auf.
- `Auditarium.Api` bleibt der externe HTTP-Zugang für API-Clients.
- Die Web-Oberfläche hält keinen serverseitigen UI-Session-/Circuit-State zwischen Requests.
- Dadurch bleiben Requests im Mehrinstanzbetrieb frei zwischen Instanzen verteilbar.
- Cookie Authentication funktioniert instanzübergreifend über den bereits festgelegten gemeinsamen Data-Protection-Keyring.
- Sticky Sessions werden für die Web-Oberfläche nicht benötigt.
- JavaScript wird gezielt für Komfortfunktionen eingesetzt, ist aber nicht Träger der Anwendungsarchitektur.
- Wesentliche Auditarium-Funktionen bleiben über normales Request/Response-Verhalten nutzbar.
- Die bisher im Zielzustand offene Blazor-Frage wurde entfernt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.83

Gegenüber Version 0.82 wurde ein bewusst einfacher Mehrinstanzbetrieb festgelegt und die Jobkoordination entsprechend erweitert:

- Auditarium unterstützt eine oder mehrere gleichartige Application-Instanzen, die auf dieselbe Datenbank zugreifen.
- Mehrinstanzbetrieb benötigt keine zusätzliche Cluster-, Cache-, Broker- oder Service-Discovery-Infrastruktur.
- Alle Instanzen verwenden denselben fachlichen Datenbestand in derselben Datenbank.
- Der dateibasierte Storage verwendet bei Mehrinstanzbetrieb denselben gemeinsam erreichbaren `StorageRoot`, z. B. ein gemeinsames Share oder gemountetes Volume.
- Alle Instanzen verwenden denselben persistenten ASP.NET-Core-Data-Protection-Keyring und denselben Application-Namensraum.
- Cookie Authentication benötigt dadurch keine Sticky Sessions.
- Die bisher nur pro Prozess gedachte Job-Concurrency wird auf eine DB-basierte instanzübergreifende Koordination erweitert.
- Job-Konfiguration bleibt in den zentralen App-Settings; technischer Job-Laufzeitstatus wird getrennt in `job_runtime_state` persistiert.
- `job_runtime_state` enthält pro `job_key` mindestens Start-/Endzeit, letzte Ausführung, laufende Instanz, Lease und Startup-Version.
- Ein nacktes persistiertes `IsRunning` wird bewusst nicht verwendet.
- `IsRunning` wird aus gültigem `running_instance_id` und nicht abgelaufener `lease_until` abgeleitet.
- Ein Jobstart erwirbt seine Lease atomar über die gemeinsame Datenbank.
- Nur eine Instanz kann die Lease eines Jobs gleichzeitig erfolgreich übernehmen; weitere Trigger werden gemäß `SkipIfRunning` übersprungen bzw. als `JOB.ALREADY_RUNNING` behandelt.
- Lange laufende Jobs verlängern ihre Lease regelmäßig per Heartbeat.
- Stirbt eine Instanz, läuft ihre Lease aus und der Job wird wieder startbar.
- Bei normalem Jobende werden Laufzeitstatus und Lease kontrolliert abgeschlossen bzw. freigegeben.
- `RunOnStartup` bedeutet im Mehrinstanzbetrieb einmalige Ausführung pro Auditarium-Anwendungsversion und Job, nicht einmal pro Prozessstart.
- `last_startup_version` verhindert, dass jede weitere Instanz denselben Startup-Job erneut ausführt.
- Eine beim Prozessstart erzeugte technische `instance_id` dient ausschließlich der Laufzeitkoordination und Diagnose.
- Es wird weiterhin keine persistente Job-Historie mit einzelnen Run-Datensätzen eingeführt; gespeichert wird nur der aktuelle bzw. letzte Runtime-Zustand.
- Die bestehenden Job-, FileStorage- und Data-Protection-Abschnitte wurden an den Mehrinstanzbetrieb angepasst.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.82

Gegenüber Version 0.81 wurde die Datenbank-Migrationsstrategie für PostgreSQL und Microsoft SQL Server festgelegt:

- Auditarium unterstützt zunächst PostgreSQL und Microsoft SQL Server als freigegebene relationale Datenbankprovider.
- Es gibt einen gemeinsamen `AuditariumDbContext`, ein gemeinsames EF-Core-Modell und zunächst einen gemeinsamen EF-Core-Migrationssatz.
- Das Datenmodell und die Migrationen werden bewusst auf den gemeinsamen Funktionsumfang der unterstützten Provider beschränkt.
- Provider-spezifische Migrationen, proprietäre SQL-Fragmente und providergebundene Spezialfunktionen gelten als Ausnahme und müssen technisch begründet werden.
- Bei jeder Schemaänderung sollen Migration und Integrationstests automatisiert gegen PostgreSQL und SQL Server ausgeführt werden.
- Im Entwicklungsbetrieb bleiben die normalen EF-Core-Werkzeuge wie `Add-Migration`, `Update-Database` und die entsprechenden `dotnet ef`-Befehle ausdrücklich erlaubt.
- Im Produktivbetrieb werden notwendige Migrationen automatisch beim Application-Start ausgeführt.
- Vor dem normalen Application-Start wird der Migrationsstand der Datenbank mit dem Migrationsstand des laufenden Codes verglichen.
- Ist die Datenbank älter als der Code, werden ausstehende Migrationen automatisch angewendet.
- Ist die Datenbank auf demselben Stand wie der Code, wird keine Migration ausgeführt.
- Enthält die Datenbank bereits Migrationen, die der laufende Code nicht kennt, wird der Start mit einem klaren Fehler abgebrochen.
- Auditarium führt niemals automatisch ein Downgrade der Datenbank aus.
- Mehrere gleichzeitig startende Instanzen derselben neuen Version verlassen sich auf den von EF Core bereitgestellten Migration-Lock; genau eine Instanz führt notwendige Migrationen aus, die übrigen erkennen anschließend den aktuellen Stand.
- Bei einem Versionsupdate werden laufende Instanzen der alten Auditarium-Version vor dem Start der neuen Version beendet.
- Zero-Downtime- oder Rolling-Upgrade-Kompatibilität ist kein Ziel des Projekts.
- Erst nach erfolgreicher Versionsprüfung und ggf. Migration wird der bestehende DAL-Bootstrap/Reconcile ausgeführt.
- Schlägt Migration oder Versionsprüfung fehl, startet die Anwendung nicht.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.81

Gegenüber Version 0.80 wurde die Persistenz und Typisierung zentraler App-Settings konkretisiert und mit bestehenden Architekturabschnitten konsolidiert:

- `application_settings.serialized_value` enthält einen kleinen selbstbeschreibenden JSON-Envelope aus `datatype` und `value`.
- Die zulässigen generischen Setting-Datentypen sind bewusst auf `int`, `float`, `string`, `text`, `bool` und `secret` begrenzt.
- `string` und `text` werden technisch beide als Zeichenkette behandelt; `text` signalisiert der UI eine mehrzeilige Eingabe.
- `float` ist ein logischer Settings-Datentyp; die CLR-Repräsentation darf als `double` erfolgen.
- Numerische Werte werden als echte JSON-Numbers kulturunabhängig gespeichert.
- Enums werden nicht als eigener Persistenzdatentyp eingeführt.
- Enum-basierte Settings werden mit einem generischen Basistyp, typischerweise `string`, gespeichert; die zulässigen Enum-Werte stammen ausschließlich aus dem Code.
- Die `SettingDefinition` im Code bleibt die kanonische Wahrheit für erwarteten Datentyp, Default, Validierung und ggf. Enum-Werte.
- Ein abweichender `datatype` im gespeicherten Envelope ändert nicht den Typ des Settings, sondern gilt als inkonsistenter DB-Zustand.
- `secret` verwendet als `value` einen geschützten Payload im versionierten Auditarium-Envelope `audsec:v1:<protected-payload>`.
- Der kanonische Setting-Key wird beim `ISecretProtector` als kryptografischer Purpose verwendet.
- Die Secret-Protection-Implementierung bleibt explizit in der Settings-/Security-Schicht; EF entschlüsselt Secrets nicht automatisch über einen ValueConverter.
- Beim Startup-Reconcile werden Envelope, Datentyp, Deserialisierbarkeit und fachliche Validierung geprüft.
- Fehlende UI-editierbare Settings werden aus gültiger Config oder sonst aus dem Code-Default initialisiert.
- Ungültige DB-Settings werden kontrolliert auf einen gültigen Seed-Wert zurückgeführt und als technische bzw. Audit-relevante Korrektur protokolliert.
- Environment-Overrides werden nicht in die DB zurückgeschrieben.
- Die ältere allgemeine Prioritätsbeschreibung der Security Policies wurde an das neue Settings-Modell angepasst.
- `ApplicationSettingsSeed/Reconcile` wurde ausdrücklich in die technische Startup-Reihenfolge aufgenommen.
- Concurrency- und Audit-Logging-Regeln für Settings wurden auf den neuen Envelope und Secret-Schutz präzisiert.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.80

Gegenüber Version 0.79 wurde das zentrale Konfigurations-, Settings- und Secret-Modell festgelegt:

- Die Datenbank enthält ausschließlich Settings, die über die Auditarium-UI administrierbar sein dürfen.
- Settings, die nicht UI-editierbar sind, werden niemals in `application_settings` gespeichert.
- Für UI-editierbare Settings gilt die Priorität: Code-Default/Fallback → Config → DB → Environment.
- Für nicht UI-editierbare Settings gilt: Code-Default/Fallback → Config → Environment.
- Environment Values besitzen damit immer die höchste Priorität.
- Nicht UI-editierbare Settings werden in der UI nur angezeigt und als extern verwaltet gekennzeichnet.
- Alle Settings verwenden einen einheitlichen hierarchischen Key-Namespace mit `:` als kanonischem Separator.
- Environment Variablen bilden denselben Namespace über die übliche .NET-Doppel-Unterstrich-Schreibweise `__` ab.
- Setting-Definitionen leben im Code und enthalten Typ, Default, UI-Editierbarkeit, Secret-Markierung, Validierung, Restart-Verhalten und Darstellungsmetadaten.
- Die Datenbank speichert für UI-editierbare Settings nur `setting_key`, einen serialisierten Wert und technische Metadaten wie `concurrency_version`.
- UI-editierbare Setting-Defaults werden bei jedem Start im DAL-Bootstrap verifiziert.
- Fehlende Settings werden angelegt; vorhandene gültige Betreiberwerte werden nicht auf den Code-Default zurückgesetzt.
- Ungültige oder nicht deserialisierbare systemdefinierte Settings werden als Bootstrap-/Konfigurationsfehler behandelt bzw. nur nach klar definierter Policy korrigiert.
- Die UI zeigt neben dem konfigurierten DB-Wert immer auch den effektiven Wert und dessen Quelle an.
- Secrets sind nicht grundsätzlich aus der DB ausgeschlossen: UI-administrierbare Feature-Secrets dürfen dort gespeichert werden.
- Secrets werden in der DB jedoch niemals im Klartext gespeichert.
- Ein `ISecretProtector` kapselt Schutz und Entschlüsselung solcher DB-Secrets.
- Für DB-Secrets wird symmetrische, authentifizierte Verschlüsselung verwendet.
- Der erste technische Zielpfad ist ASP.NET Core Data Protection mit persistiertem Keyring und Key-Rotation.
- Der Keyring bzw. sein Schutzschlüssel liegt außerhalb der `application_settings`-Tabelle und darf nicht durch dasselbe DB-Secret-System geschützt werden.
- Asymmetrische Kryptografie ist nicht für jedes Secret vorgesehen, kann aber zum Schutz/Wrap des Data-Protection-Keyrings verwendet werden, z. B. über ein X.509-Zertifikat.
- UI-administrierbare Secrets werden in der UI nur als gesetzt/nicht gesetzt dargestellt und nicht wieder im Klartext angezeigt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.79

Gegenüber Version 0.78 wurde die Architektur für interne Maintenance-/Background-Jobs festgelegt:

- Auditarium verwendet für v1 ASP.NET Core `BackgroundService` in Kombination mit Cronos.
- Cronos berechnet echte Zeitpläne; reine Startzeitpunkt-abhängige Intervalle werden vermieden.
- Jobs besitzen konfigurierbare Cron-Schedules und Zeitzonen.
- Für normale Maintenance ist `MisfirePolicy = Skip` der Default; verpasste Läufe werden nicht automatisch zur nächsten ungünstigen Zeit nachgeholt.
- Derselbe Job darf nicht parallel mehrfach laufen; zentral gilt `ConcurrencyPolicy = SkipIfRunning`.
- Ein zentraler `JobCoordinator` vereinheitlicht alle Triggerarten und verhindert Doppelstarts.
- Ein Job kann durch Zeitplan, Application Startup oder manuellen Admin-Trigger gestartet werden.
- Alle Trigger führen in dieselbe Job-Implementierung; es gibt keine getrennte Cron-/Startup-/Manual-Logik.
- `RunOnStartup` wird erst nach erfolgreichem DAL-Bootstrap und nach Freigabe des normalen Application-Starts ausgelöst.
- Manuell gestartete Jobs benötigen zusätzlich eine passende RBAC-Permission.
- Der eigentliche Job läuft unabhängig vom Auslöser immer als `ActorType.System` und unterliegt dessen RBAC.
- Ein manueller Admin-Trigger dokumentiert den anfordernden User; die eigentliche Jobausführung erfolgt als Systemactor.
- Pro Job wird im Code festgelegt, welche Triggerarten grundsätzlich erlaubt sind.
- Vorgesehene Triggerarten sind `Scheduled`, `Startup` und `Manual`.
- Betreiberkonfiguration darf nur innerhalb der im Code erlaubten Triggerarten wirken.
- Ein Job, der `Manual` nicht erlaubt, kann weder über UI noch API manuell gestartet werden.
- Ein Job, der `Startup` nicht erlaubt, darf nicht per Konfiguration auf `RunOnStartup = true` gesetzt werden.
- Ein rein manueller Job ist möglich und benötigt keinen Schedule.
- Die UI zeigt manuelle Startmöglichkeiten nur, wenn sowohl der Job `Manual` erlaubt als auch der aktuelle Benutzer die notwendige Permission besitzt.
- Für v1 wird keine zusätzliche Job-Historien-Datenbank und kein schweres Scheduler-Framework eingeführt.
- Jobstatus und technische Laufzeitinformationen werden über In-Memory-Status sowie `ILogger<T>` / OpenTelemetry sichtbar gemacht.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.78

Gegenüber Version 0.77 wurde die technische Import-Architektur konkretisiert:

- Auditarium enthält in der aktuellen Produktarchitektur keine eigene KI-Funktionalität.
- Auditarium definiert ein versioniertes Importformat für Kataloginhalte.
- Auditarium stellt dazu einen passenden, versionierten Hilfsprompt bereit.
- Benutzer können Originaldokument und Prompt außerhalb von Auditarium mit einem beliebigen externen Werkzeug verarbeiten.
- Auditarium verarbeitet ausschließlich die daraus zurückgelieferte Importdatei.
- Importdateien werden grundsätzlich als nicht vertrauenswürdige externe Eingaben behandelt.
- Vor jeder Datenänderung erfolgen Syntax-, Schema- und fachliche Importvalidierung.
- Der Import verändert die Datenbank erst nach erfolgreicher Validierung und ausdrücklicher Übernahmeentscheidung.
- Importergebnisse unterscheiden mindestens `VALID`, `PARTIALLY_VALID` und `REJECTED`.
- Harte Integritätsverletzungen dürfen niemals übernommen werden.
- Bei teilweise gültigen Paketen können valide Inhalte kontrolliert übernommen werden.
- Validierungsfehler und Warnungen werden als strukturierter Importbericht ausgegeben.
- Der Importbericht kann vom Benutzer zur Korrektur der extern erzeugten Importdatei verwendet werden.
- `import_format_version`, `catalog_version_id` und `draft_revision` werden im Importvertrag mitgeführt.
- Ein Importpaket mit veralteter `draft_revision` wird nicht blind auf einen inzwischen geänderten DRAFT angewendet.
- Import und Apply bleiben technisch deterministisch und unabhängig davon, mit welchem externen Werkzeug die Datei erzeugt wurde.
- Die technische Terminologie verwendet `Import Package`, `Import Format`, `Import Prompt`, `Import Validation` und `Import Apply` statt eine KI als Bestandteil des Systems zu modellieren.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.77

Gegenüber Version 0.76 wurde das Dateiablage-/FAL-Modell für Originaldokumente konkretisiert:

- Persistente Dateien werden nicht als BLOB in PostgreSQL/MSSQL gespeichert, sondern über die FAL in einem Dateispeicher abgelegt.
- Für Auditarium ist zunächst genau ein dauerhafter Datei-Use-Case vorgesehen: eine optionale Originaldatei je `catalog_version`.
- Eine `catalog_version` darf in `DRAFT` ein Originaldokument hinzufügen, ersetzen oder entfernen.
- Beim Übergang zu `READY` wird auch die Dateireferenz historisch eingefroren; Ersetzen oder Entfernen ist danach nicht mehr zulässig.
- Der physische Storage kann zunächst ein lokales Verzeichnis oder ein Netzwerk-Share sein.
- Der Storage-Root ist konfigurierbar; in der DB werden nur relative Pfade gespeichert.
- Dateien werden unter einem technisch generierten GUID-basierten Namen ohne Original-Dateiname und ohne Extension abgelegt.
- Eine neue Storage-ID wird race-condition-frei durch exklusives Anlegen abgesichert.
- Dateimetadaten werden in einem `FileItem` in der Datenbank gespeichert.
- Vorgesehene Metadaten sind mindestens OriginalFileName, SaveFileName, Extension, SaveFilePath, ContentType, Size, CreatedAt, CreatedBy und Checksum.
- Für Integritätsprüfung wird SHA-256 als Checksumme vorgesehen.
- Uploads werden mindestens anhand Dateigröße, Extension, Content-Type und Dateisignatur plausibilisiert.
- Für v1 wird PDF als primär zugelassener Dokumenttyp vorgesehen; weitere unveränderliche Formate können später ergänzt werden.
- Größenlimits für Upload und Download sind konfigurierbar und sollen große PDF-Dokumente unterstützen.
- Beim Ersetzen wird zuerst die neue Datei vollständig gespeichert und referenziert; die alte physische Datei wird erst danach gelöscht.
- Scheitert das physische Löschen der alten Datei nach erfolgreichem DB-Commit, bleibt höchstens eine verwaiste Datei zurück; eine gültige neue Referenz darf dadurch nicht verloren gehen.
- Die FAL ist sowohl für Speichern als auch für Lesen und Löschen der physischen Datei zuständig.
- Downloads laufen kontrolliert durch Auditarium und die FAL; direkte öffentliche Storage-Pfade oder statische URLs werden nicht verwendet.
- Die BLL entscheidet, ob ein Originaldokument hinzugefügt, ersetzt, entfernt oder geladen werden darf.
- Die DAL verwaltet `FileItem` und die Referenz der `catalog_version`; die FAL verwaltet ausschließlich den physischen Dateiinhalt.
- Das Modell bleibt offen für spätere Storage-Implementierungen wie S3 oder Azure Blob Storage.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.76

Gegenüber Version 0.75 wurde das Concurrency-Modell für paralleles Arbeiten festgelegt:

- Auditarium verwendet optimistische Concurrency.
- Relevante bearbeitbare Aggregate erhalten einen providerneutralen numerischen Concurrency-Token, z. B. `concurrency_version BIGINT`.
- SQL-Server-spezifisches `rowversion` und PostgreSQL-spezifisches `xmin` werden nicht als fachliches Kernmodell verwendet.
- Entity Framework behandelt den Versionstoken als Concurrency Token.
- Updates erfolgen semantisch nur, wenn der beim Lesen bekannte Versionstoken noch aktuell ist.
- Ein Update mit veraltetem Versionstoken führt zu einem kontrollierten Concurrency Conflict und überschreibt keine zwischenzeitlichen Änderungen.
- Concurrency-Tokens werden nur dort eingesetzt, wo Objekte tatsächlich unabhängig geladen und bearbeitet werden können; reine Zuordnungstabellen erhalten nicht automatisch eigene Tokens.
- ViewModels und Commands transportieren den Versionstoken transparent zwischen Lesen und Schreiben.
- EF-`DbUpdateConcurrencyException` wird an geeigneter Stelle in einen erwartbaren `AppError` vom Typ `Conflict` übersetzt.
- Die API bildet Concurrency-Konflikte auf HTTP 409 ab.
- Die UI fordert bei Konflikten zum Neuladen bzw. erneuten Prüfen des aktuellen Stands auf.
- Automatisches Merge oder blindes Retry mit neu geladenen Daten ist bei Benutzeränderungen nicht vorgesehen.
- Commands sollen nach Möglichkeit konkrete Absichten ausdrücken, z. B. `AddPermissionToRole` statt komplette Collections blind zu ersetzen.
- Technische Retry-Mechanismen für transiente Infrastrukturfehler bleiben von fachlichen Concurrency-Konflikten getrennt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.75

Gegenüber Version 0.74 wurde das einheitliche Fehler-, Result- und Exception-Modell festgelegt:

- Auditarium unterscheidet strikt zwischen Ergebnis, Fehler und Ausnahme.
- Erfolgreiche erwartbare Use-Case-Ausgänge werden als `Result<T>`-Erfolg behandelt.
- Erwartbare negative Use-Case-Ausgänge werden als stabile `AppError`-Ergebnisse behandelt.
- Unerwartete technische oder programmatische Fehler bleiben echte Exceptions.
- BLL-Fehler verwenden stabile technische Error Codes und keine UI-Texte.
- Die BLL kennt keine HTTP-Statuscodes.
- Fehler werden über kleine technische Kategorien wie `Validation`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict` und `Failure` klassifiziert.
- FluentValidation-Fehler werden zentral in dasselbe `AppError`-Format überführt.
- Der API-Layer übersetzt BLL-Fehler in standardisierte ASP.NET-Core-`ProblemDetails`.
- Erwartbare HTTP-Abbildungen sind insbesondere 400, 401, 403, 404 und 409.
- Unerwartete Exceptions werden zentral abgefangen, vollständig technisch geloggt und mit OpenTelemetry korreliert.
- Production liefert bei Exceptions ausschließlich ein bereinigtes Fehlerobjekt mit stabilem Fehlercode und `traceId`.
- Stacktrace, Exception-Typ, Inner Exception und technische Details bleiben in Production intern über Logs/Tracing verfügbar.
- Development darf detaillierte Exception-Diagnose einschließlich Stacktrace über die ASP.NET-Core-Developer-Mechanismen anzeigen.
- Die eingebaute Developer Exception Page wird gegenüber einer eigenen Entwickler-Fehleroberfläche bevorzugt.
- Sensitive Daten und Secrets dürfen weder durch eigene Logs noch durch Fehlerobjekte unnötig exponiert werden.
- UI-Lokalisierung erfolgt anhand stabiler Error Codes und Parameter und bleibt von der BLL getrennt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.74

Gegenüber Version 0.73 wurde das Format der Auditarium-API-Credentials verbindlich festgelegt:

- API-Credentials verwenden das Format `aud_v1_<keyid>_<base64url-secret>`.
- `aud` ist der feste Auditarium-Präfix und erleichtert Erkennung, Redaction und Secret-Scanning.
- `v1` ist eine explizite Formatversion und ermöglicht zukünftige kompatible Änderungen am Credential-Format.
- `keyid` ist nicht geheim und dient ausschließlich zur direkten Auswahl des zugehörigen Credential-Datensatzes.
- Das eigentliche Secret wird aus mindestens 256 Bit kryptographisch sicherem Zufall erzeugt.
- Nur die zufälligen Secret-Bytes werden für den Transport Base64url-codiert.
- Das vollständige Credential wird nicht zusätzlich Base64-codiert oder anderweitig obfuskiert.
- Der Client erhält das Klartext-Credential nur einmal bei der Erzeugung.
- In der Datenbank werden `keyid` und ein sicherer Hash des Secrets gespeichert; der Secret-Hash ist niemals Bestandteil des ausgegebenen API-Keys.
- Logs, Traces, Fehlermeldungen und UI-Ausgaben dürfen vollständige API-Credentials nicht ausgeben; der erkennbare Präfix soll gezielte Redaction erleichtern.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.73

Gegenüber Version 0.72 wurden Login-Transport, Sessions, API-Credentials, Security Policies und Provider-Routing konkretisiert:

- Interaktive Web-Logins verwenden ASP.NET Cookie Authentication.
- Das Authentifizierungs-Cookie transportiert nur minimale interne Identität; Rollen und Permissions werden nicht als dauerhafte Autorisierungswahrheit im Cookie behandelt.
- Rollen- und Permission-Änderungen wirken dadurch beim nächsten autorisierten Use Case ohne erneuten Login.
- Eine eigene serverseitige Session-Tabelle wird zunächst nicht eingeführt.
- API-Authentifizierung erfolgt über dedizierte API-Credentials und nicht über Browser-Cookies.
- API-Credentials bestehen aus einer öffentlich identifizierbaren Key-ID und einem geheimen Secret.
- Gespeichert werden Key-ID und Secret-Hash; das Klartext-Secret wird nur einmal ausgegeben.
- Eine API-Identity darf mehrere gleichzeitig gültige Credentials besitzen, um Rotation ohne Downtime zu ermöglichen.
- API-Credentials können benannt, widerrufen und zeitlich begrenzt werden; `last_used_at` wird erfasst.
- Ein inaktiver Benutzer kann sich unabhängig vom verwendeten Provider nicht erfolgreich authentifizieren.
- Security Policies für LOCAL, API und Sessions besitzen sichere Defaults und sind zentral konfigurierbar.
- Konfiguration kann aus Built-in Defaults, zentralen App-Settings, Anwendungs-Konfiguration und Environment Variablen stammen.
- Extern vorgegebene Werte werden in der Settings-Oberfläche nur angezeigt und als extern verwaltet gekennzeichnet.
- LOCAL verwendet Mindestlänge, Lockout- und Rate-Limit-Regeln; LDAP übernimmt Passwort- und Account-Policy grundsätzlich vom externen Verzeichnis.
- API-Credentials verwenden keine klassischen Passwortregeln, sondern Entropie-, Ablauf-, Rotations-, Revocation- und Rate-Limit-Regeln.
- Der Default-Admin unterliegt im Normalbetrieb denselben LOCAL-Policies wie andere lokale Benutzer; Recovery bleibt der separate Wiederherstellungspfad.
- Ein `AuthenticationRouter` bestimmt bei Username/Password-Logins genau einen zuständigen Provider.
- Credentials werden niemals per Fallback nacheinander an mehrere Provider weitergereicht.
- `DOMAIN\username` wird anhand eines konfigurierten NetBIOS-/Domain-Präfixes einem LDAP-Provider zugeordnet.
- `username@domain.example` wird anhand eines konfigurierten UPN-Suffixes einem LDAP-Provider zugeordnet.
- Ein unqualifizierter `username` wird standardmäßig dem LOCAL-Provider zugeordnet.
- Unbekannte Präfixe oder Suffixe führen zu einer Ablehnung und nicht zu einem Fallback auf LOCAL.
- Optional kann die Login-Oberfläche einen Provider-Selector anbieten; Standard bleibt `Automatic`.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.72

Gegenüber Version 0.71 wurden Ausführungskontext, Systemautorisation und die harte Bootstrap-Grenze konkretisiert:

- Die BLL erhält mit `ICurrentActor` einen schlanken, HTTP-unabhängigen Ausführungskontext.
- Es werden die Actor-Typen `Anonymous`, `User` und `System` unterschieden.
- `Anonymous` repräsentiert eine unbekannte externe Person im User-Kontext.
- `User` repräsentiert einen erfolgreich authentifizierten internen Benutzer.
- `System` repräsentiert einen internen technischen Ausführungskontext.
- Der Systemactor verwendet `user_id = 0`, gilt aber ausdrücklich nicht als authentifiziert.
- Auch der Systemactor unterliegt nach Application-Start vollständig dem normalen RBAC-Modell.
- `System` ist kein Superuser und besitzt nur die Permissions seiner systemverwalteten Rolle(n).
- Externe Authentication Provider dürfen niemals `ActorType.System` bzw. `user_id = 0` erzeugen.
- Der Datenbank-Bootstrap ist die einzige bewusste Ausnahme vom normalen RBAC.
- Migrationen sowie Bootstrap/Reconcile laufen vor dem eigentlichen Application-Start direkt im DAL.
- Der Bootstrap verwendet den konkreten `AuditariumDbContext` und nicht Mediator, BLL-Handler oder `IAuditariumDbContext`.
- Systemuser, code-definierte Permissions, Systemrollen, RolePermissions, Default-Admin und dessen notwendige Rollenzuordnung werden dabei direkt gegen den Sollzustand reconciled.
- Der Reconcile darf erwartbare Abweichungen hart korrigieren; strukturell widersprüchliche oder nicht eindeutig reparierbare Zustände brechen den Startup ab.
- Der Bootstrap/Reconcile wird als atomarer technischer Initialisierungsschritt mit expliziter Datenbanktransaktion ausgeführt.
- Recovery-Änderungen am Default-Admin werden ebenfalls in dieser DAL-Bootstrap-Phase vorgenommen.
- Erst nach erfolgreichem Bootstrap startet die eigentliche Anwendung mit Mediator, Behaviors, RBAC, API und Web UI.
- Nach Application-Start existiert kein allgemeiner System-/RBAC-Bypass.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.71

Gegenüber Version 0.70 wurde das Authentifizierungsmodell konkretisiert:

- `users` bleibt die providerneutrale Benutzer-Hülse.
- Ein Benutzer kann 0..n Authentifizierungs-Identitäten besitzen.
- Authentifizierung wird über eigenständige Provider abstrahiert.
- Vorgesehene Provider sind mindestens LOCAL, LDAP und API; OIDC/SAML bleiben offen.
- Die Authentifizierung liefert immer auf eine interne `user_identity` und damit auf genau einen internen `user_id`.
- Autorisierung bleibt vollständig von der verwendeten Authentifizierungsmethode entkoppelt.
- `LocalAuthenticationProvider` verwaltet lokale Benutzer-Credentials.
- `LdapAuthenticationProvider` prüft Credentials extern und speichert keine LDAP-Passwörter.
- `ApiAuthenticationProvider` behandelt technische API-Credentials als eigenständige Authentifizierungsart.
- API-Benutzer verwenden dieselben Rollen und Permissions wie interaktive Benutzer; es gibt kein separates API-Berechtigungsmodell.
- Lokale und API-Credentials werden nicht in `users` gespeichert, sondern in provider-spezifischen Credential-Stores.
- Klartext-Passwörter und API-Secrets werden niemals persistent gespeichert.
- API-Secrets werden nur einmal ausgegeben und anschließend ausschließlich gehasht gespeichert.
- Provider liefern eine stabile externe Identität; die Zuordnung erfolgt über `(auth_type, provider, external_id)`.
- Der Default-Admin verwendet eine LOCAL-Identity; der Recovery-Modus wirkt gezielt auf deren lokales Credential.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.70

Gegenüber Version 0.69 wurde die technische Observability-Architektur festgelegt:

- `system_audit_log` bleibt vollständig von technischer Observability getrennt und dient ausschließlich der fachlichen, nachvollziehbaren Änderungshistorie.
- Technisches Application Logging erfolgt über `Microsoft.Extensions.Logging` / `ILogger<T>`.
- Metrics werden über `System.Diagnostics.Metrics` / `Meter` erzeugt.
- Tracing wird über `System.Diagnostics.ActivitySource` / `Activity` umgesetzt.
- Health Checks werden über ASP.NET Core Health Checks bereitgestellt.
- OpenTelemetry bildet die gemeinsame Sammlung-, Korrelation- und Export-Schicht für Logs, Metrics und Traces.
- OTLP ist der primäre standardisierte externe Transport für zentrale Observability.
- Zusätzlich wird ein Prometheus-kompatibler `/metrics`-Endpoint unterstützt.
- Auditarium bleibt vollständig funktionsfähig, wenn kein OpenTelemetry-/OTLP-Ziel konfiguriert ist.
- Console-/stdout-Logging bleibt als lokaler und containerfreundlicher Standardpfad bestehen.
- Konsumenten wie OpenTelemetry Collector, Prometheus, Checkmk, Grafana-Stacks, Azure Monitor oder andere Systeme sind reine Betreiberentscheidung.
- Eigene Auditarium-Instrumentierung wird sparsam und fachlich sinnvoll ergänzt; vorhandene ASP.NET-/HTTP-/EF-Instrumentierung soll möglichst genutzt werden.
- CQRS-Telemetrie wird zentral über einen `ObservabilityBehavior` erfasst, der Logging, Tracing und geeignete technische Metrics bündeln kann.
- Das `system_audit_log` wird ausdrücklich nicht durch den `ObservabilityBehavior` erzeugt.
- Telemetrie darf keine Secrets, Passwörter, Tokens, Authorization Header, vollständige Request Bodies oder sensible Audit-Inhalte enthalten.
- IDs können in Logs/Traces bei begründetem Bedarf vorkommen, aber nicht als hochkardinale Metric-Labels.
- Metrics verwenden nur kontrollierte, niedrig kardinale Labels wie Request-Typ oder Status.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.69

Gegenüber Version 0.68 wurde das Bootstrap-, Reconcile- und Recovery-Konzept für systemverwaltete Benutzer, Rollen und Permissions festgelegt:

- Code-definierte Permissions werden bei jedem Anwendungsstart mit der Datenbank abgeglichen.
- System-/Default-Rollen besitzen einen stabilen `role_key` und sind nicht über normale UI/API-Verwaltung editierbar.
- Rollen mit `role_key` werden bei jedem Start vollständig gegen den im Code definierten Sollzustand reconciled, einschließlich Permission-Zuordnungen und Aktivierungszustand.
- Benutzerdefinierte Rollen besitzen keinen `role_key` und bleiben vollständig in Betreiberhoheit.
- Der reservierte Systembenutzer erhält einen stabilen `user_key = SYSTEM`.
- Der Default-Administrator erhält einen stabilen `user_key = DEFAULT_ADMIN`.
- Normale Benutzer besitzen keinen `user_key`.
- Der Systembenutzer wird bei jedem Start vollständig auf den definierten Sollzustand korrigiert.
- Beim Default-Administrator werden im Normalbetrieb Existenz und notwendige Systemrollen sichergestellt; Passwort und Aktivierungszustand werden dabei nicht verändert.
- Für den Default-Administrator wird ein expliziter Recovery-Startmodus vorgesehen.
- Im Recovery-Modus kann der Default-Administrator reaktiviert und sein Passwort kontrolliert überschrieben werden.
- Die Recovery-Verarbeitung ist idempotent: Das konfigurierte Recovery-Passwort wird gegen den gespeicherten Passwort-Hash verifiziert; ist Recovery bereits wirksam, wird nichts erneut verändert.
- Solange Recovery aktiviert ist, startet Auditarium nicht in den normalen Betriebsmodus.
- Im Recovery-Modus werden nur eine statische Recovery-Statusseite und gegebenenfalls technische Health-Endpunkte bereitgestellt.
- Nach erfolgreicher Recovery muss Auditarium heruntergefahren, die Recovery-Konfiguration entfernt und anschließend normal neu gestartet werden.
- Recovery-Passwörter sollen bevorzugt über Environment-/Secret-Injection bereitgestellt werden.
- Alle Reconcile- und Recovery-Aktionen werden, sobald der Systembenutzer verfügbar ist, mit `user_id = 0` im `system_audit_log` protokolliert.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.68

Gegenüber Version 0.67 wurde das Autorisierungsmodell für Auditarium konkretisiert:

- Auditarium verwendet ein permission-basiertes RBAC-Modell.
- Permissions sind stabile technische Fähigkeiten und werden im Code definiert.
- Rollen sind frei konfigurierbare Sammlungen von Permissions.
- Benutzer können mehreren Rollen angehören; die effektiven Permissions ergeben sich additiv aus der Vereinigungsmenge aller aktiven Rollen.
- Ein explizites Deny-Modell ist nicht vorgesehen.
- Die Datenbank enthält `permissions`, `roles`, `role_permissions` und `user_roles`.
- Permission-Keys werden nicht an Controller- oder Methodennamen gekoppelt, sondern fachlich stabil benannt, z. B. `Documents.Read`, `Documents.Create` oder `Audits.Finalize`.
- Betreiber dürfen Rollen, Rollenzuordnungen und Permission-Zuordnungen konfigurieren, aber keine beliebigen neuen Permission-Keys erfinden.
- Ein zentraler `IPermissionEvaluator` in der BLL berechnet die effektiven Permissions eines Benutzers.
- Der `AuthorizationBehavior` verwendet denselben Permission-Evaluator wie die UI-Abfragen.
- UI-Projekte fragen ausschließlich effektive Permissions ab und werten keine Rollen selbst aus.
- Die UI nutzt Permissions nur zur Darstellung und blendet nicht erlaubte Funktionen aus; die eigentliche Sicherheitsentscheidung bleibt in der BLL.
- Jeder über den Mediator erreichbare Request muss explizit entweder eine benötigte Permission deklarieren oder als anonym erlaubt markiert sein.
- Fehlt eine solche Deklaration, gilt Default-Deny.
- Für später ist ein Architecture-Test bzw. Roslyn Analyzer vorgesehen, der fehlende Security-Deklarationen bereits beim Build bzw. Test erkennt.
- Auditarium liefert initiale Standardrollen aus; Betreiber können diese anpassen und weitere Rollen anlegen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.67

Gegenüber Version 0.66 wurden CQRS-Dispatching, Pipeline Behaviors und Request-Validierung konkretisiert:

- Auditarium verwendet `martinothamar/Mediator` als CQRS-Dispatcher.
- Es wird keine zusätzliche Auditarium-eigene Dispatcher-Abstraktion eingeführt.
- Commands und Queries werden in der BLL definiert und über den Mediator dispatcht.
- Cross-Cutting Concerns werden über schlanke Pipeline Behaviors umgesetzt.
- Für den ersten Implementierungsstand sind `TechnicalLoggingBehavior`, `AuthorizationBehavior` und `ValidationBehavior` vorgesehen.
- Ein generischer `TransactionBehavior` ist zunächst nicht vorgesehen; einzelne Use Cases verwenden nur bei echtem Bedarf explizite Transaktionen.
- Das fachliche `system_audit_log` bleibt an Entity Framework / `SaveChanges` gekoppelt und wird nicht über CQRS-Behaviors erzeugt.
- Für Request-Validierung wird FluentValidation verwendet.
- FluentValidation prüft ausschließlich inhaltliche, strukturelle und requestinterne Konsistenz.
- Validatoren greifen nicht auf `IAuditariumDbContext` oder andere Persistenzabstraktionen zu.
- Regeln, die Persistenzzustand, fachliche Zusammenhänge oder Zustandsautomaten betreffen, bleiben in Handlern bzw. fachlicher BLL-Logik.
- Validierungsfehler erhalten stabile technische Fehlercodes; lokalisierte UI-Texte werden davon getrennt.
- Die Validierung wird zentral über den `ValidationBehavior` ausgeführt und ist damit unabhängig davon, ob der Request aus API, Web UI oder einem späteren anderen Einstiegspunkt stammt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.66

Gegenüber Version 0.65 wurde der grundlegende technische Architekturrahmen für Auditarium festgelegt:

- Auditarium wird als strukturierter Monolith auf Basis von .NET / C# umgesetzt.
- Als Zielplattform wird die jeweils aktuelle stabile Major-Version verwendet, bevorzugt die letzte LTS-Version; für den initialen Stand ist .NET 10 LTS vorgesehen.
- Die Solution wird in die Bereiche `UI`, `Core`, `Persistence`, `Infrastructure` und `Tests` gegliedert.
- Die UI greift für fachliche Operationen ausschließlich auf die BLL zu.
- `Auditarium.Bll` enthält die fachlichen Use Cases, CQRS-Features, ViewModels und die von der BLL benötigten Abstraktionen.
- `Auditarium.Models` enthält die persistierbaren Entity-Klassen und fachnahen Enums.
- `Auditarium.Common` enthält ausschließlich fachlich neutrale, schichtübergreifend benötigte technische Bausteine.
- `Auditarium.Dal` implementiert den Datenzugriff mit Entity Framework Core einschließlich DbContext, Entity-Konfigurationen und Migrationen.
- `Auditarium.Fal` kapselt den Dateizugriff.
- Externe Systeme wie LDAP werden in eigenen Infrastructure-Projekten angebunden.
- Interfaces werden grundsätzlich dort definiert, wo die Fähigkeit benötigt wird; Implementierungen liegen in den äußeren Projekten.
- `IAuditariumDbContext`, `IFileStorage` und `IUserDirectory` werden in der BLL definiert und von DAL, FAL bzw. Infrastructure implementiert.
- Entity Framework Core darf innerhalb der BLL über `IAuditariumDbContext` direkt genutzt werden; Auditarium führt kein zusätzliches Repository- oder Unit-of-Work-Pattern ein.
- Queries projizieren bevorzugt direkt per LINQ/EF auf ViewModels und verwenden `AsNoTracking()`.
- Commands laden und verändern die benötigten Entities und persistieren über `SaveChangesAsync()`.
- CQRS wird feature-/use-case-orientiert als Vertical-Slice-Struktur organisiert.
- ViewModels liegen innerhalb des jeweiligen Features bzw. Use Cases und werden nur bei echter Mehrfachnutzung eine Ebene nach oben gezogen.
- Handler erhalten nicht automatisch eigene Interfaces; Interfaces werden nur an echten Architekturgrenzen eingeführt.
- Dependency Injection, Migrationen, automatisierte Tests und Swagger/OpenAPI für die API sind verbindliche Bestandteile der Architektur.
- Die konkrete Web-UI-Technologie, insbesondere eine mögliche Umsetzung mit Blazor, bleibt zunächst offen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.65

Gegenüber Version 0.64 wurden Schlüssel, Unique Constraints, Check Constraints, Fremdschlüsselregeln und referenzielle Löschregeln für das bestehende Datenmodell festgelegt:

- harte Datenbank-Invarianten werden von fachlichen/transaktionalen Regeln getrennt,
- physische Cascades sind nur innerhalb echter Aggregate zulässig,
- externe und historische Referenzen verwenden `RESTRICT` / `NO ACTION`,
- `documents` erhält bewusst keinen natürlichen Unique Key,
- `(document_id, version_number)` ist für `catalog_versions` eindeutig,
- `document_elements.sort_order` ist innerhalb derselben Geschwistergruppe eindeutig,
- `(element_id, sort_order)` ist für `questions` eindeutig,
- `document_element_weights.weight` darf nur `1`, `2`, `4` oder `5` enthalten,
- `(question_id, scope_type_id)` bleibt der zusammengesetzte Primärschlüssel von `question_scope_types`,
- `(audit_id, element_id)` und `(audit_document_element_id, question_id)` sind harte Unique Constraints,
- Antwortkonsistenz von `audit_questions` wird als Constraint festgelegt,
- `users.username` wird als kanonischer technischer Benutzername gespeichert und eindeutig gehalten,
- `(auth_type, provider, external_id)` bleibt für `user_identities` eindeutig,
- Scope-, Audit-, Katalog- und Nutzungszustände werden auf die jeweils definierten Wertemengen begrenzt,
- Zyklen, Parent-Typ-Kompatibilität, Audit-State-Transitions, direkte Root-Referenz von `origin_audit_id` und katalogübergreifende Konsistenz bleiben bewusst in transaktionaler Anwendungslogik,
- jeder Fremdschlüssel erhält einen Index, sofern er nicht bereits durch einen Primär-/Unique-/Composite-Index abgedeckt ist.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.64

Gegenüber Version 0.63 wurden Pflichtfelder, Nullability und Defaults für die übrigen bereits definierten Entitäten festgelegt und bestehende Tabellen konsolidiert:

- allgemeine Regeln für `NULL`, Leerstrings, Defaults, Primärschlüssel, Fremdschlüssel und Zeitstempel wurden ergänzt,
- `catalog_versions.document_id` wurde als zwingende Referenz wieder in die Tabelle aufgenommen,
- `audit_units.usage_state_reason` wurde in der Tabelle ergänzt,
- die beschädigte Tabellenstruktur von `audits` wurde bereinigt,
- `document_element_weights` wurde an die bereits festgelegte Sparse-Speicherung angepasst: Ein Datensatz existiert nur für explizite Gewichte `1`, `2`, `4` oder `5`; der effektive Default `3` wird durch das Fehlen eines Datensatzes dargestellt,
- für `questions`, `question_scope_types`, `audit_units`, `audits`, `audit_document_elements`, `audit_questions`, `users`, `user_identities`, `catalog_versions` und `system_audit_log` wurden Pflichtfelder und Defaults festgelegt,
- konditionale Pflichtfelder wie `audit_unit_context`, `answered_at` / `answered_by`, `usage_state_reason` und `state_reason` wurden ausdrücklich beschrieben,
- optionale Textfelder werden konsistent als `NULL` und nicht als Leerstring gespeichert.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.63

Gegenüber Version 0.62 wurden Pflichtfelder, Nullability und Defaults für `documents` für den ersten Implementierungsstand festgelegt:

- `document_id` ist verpflichtender, technisch erzeugter Primärschlüssel.
- `title` ist verpflichtend.
- `publisher`, `version`, `publication_date` und `source` sind optional, da diese Metadaten nicht für jedes Regelwerk zuverlässig vorhanden sein müssen.
- `usage_state` ist verpflichtend und erhält beim Anlegen den Default `ACTIVE`.
- `usage_state_reason` ist grundsätzlich optional, muss bei `usage_state = DEPRECATED` jedoch fachlich befüllt sein.
- `notes` ist optional.
- Optionale Textfelder werden bei fehlendem Inhalt als `NULL` gespeichert, nicht als Leerstring.
- Aus `title`, `publisher`, `version` oder anderen fachlichen Metadaten wird kein natürlicher Unique Key gebildet.
- Eine veraltete, falsch platzierte Aussage zu `catalog_state` im Dokumentabschnitt wurde entfernt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.62

Gegenüber Version 0.61 wurde das Modell des `system_audit_log` vollständig auf Konsistenz geprüft und bereinigt:

- `system_audit_log.user_id` ist in der eigentlichen Felddefinition nun korrekt als verpflichtende Referenz auf `users.user_id` beschrieben; `user_id = 0` kennzeichnet den reservierten Systembenutzer.
- Die Tabelle `system_audit_log` wurde strukturell repariert; `before_state` und `after_state` sind wieder reguläre Felder derselben Tabelle.
- `object_type` wird in Beispielen als technischer Entity-/Klassentyp dargestellt und nicht mehr als separater Großbuchstaben-Code.
- Veraltete Verweise auf ein separates `attribute` wurden aus dem aktuellen Zielmodell entfernt.
- Der zusammengesetzte Index für Historienabfragen basiert auf `object_type`, `object_id`, `action` und `occurred_at`.
- Nicht ändernde Sicherheitsereignisse wie Login, Logout oder fehlgeschlagene Anmeldung dürfen `before_state` und `after_state` leer lassen.
- Für Ereignisse ohne eindeutig vorhandenes persistiertes Zielobjekt darf `object_id` leer bleiben; `object_type` beschreibt weiterhin den technischen Ereignis-/Objektkontext.
- `RESTORE` wurde auf den festgelegten Action-Code `RESTORED` vereinheitlicht.
- Soft-Delete-Retention und Audit-Log-Retention wurden widerspruchsfrei formuliert: Retention ist jeweils eine Mindestfrist bzw. Purge-Voraussetzung, keine automatische Löschgarantie.
- Audit-Log-Einträge dürfen erst nach physischem Purge des Ursprungsobjekts, Ablauf ihrer eigenen Mindest-Retention und bestandener Abhängigkeitsprüfung gepurged werden.
- `append-only` bedeutet unveränderlich während der Lebensdauer eines Logeintrags; kontrollierter physischer Purge nach den Retention-Regeln bleibt zulässig.
- Veraltete `audits.document_id`-Verweise im aktuellen Audit-Zielmodell wurden entfernt. Das Dokument wird weiterhin über `catalog_version_id → document_id` abgeleitet.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.61

Gegenüber Version 0.60 wurde `system_audit_log.attribute` entfernt:

- `attribute` ist redundant und entfällt.
- `object_type` und `object_id` identifizieren das betroffene Fachobjekt eindeutig.
- `before_state` und `after_state` enthalten die tatsächlich geänderten Properties und liefern damit bereits die benötigte Änderungsinformation.
- Welche Properties geändert wurden, wird durch Entity Framework in der Persistenzschicht ermittelt.
- Ein Audit-Log-Ereignis beschreibt damit eine Änderung an genau einer Entity; mehrere gleichzeitig geänderte Properties werden gemeinsam im Delta dieses Ereignisses abgebildet.
- Abfragen nach Zustandsänderungen erfolgen über `object_type`, `object_id`, `action` und `occurred_at`.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.60

Gegenüber Version 0.59 wurde die Regel für `system_audit_log.object_type` präzisiert:

- `object_type` ist kein Freitextfeld.
- Der Wert entspricht einem zentral definierten technischen Entity-/Klassentyp.
- Die zulässigen Werte werden aus dem Domain-/Entity-Modell abgeleitet und technisch validiert.
- Der Bezeichner muss innerhalb des Auditarium-Datenmodells eindeutig und langfristig stabil sein.
- Ein vollqualifizierter Klassenname einschließlich Namespace kann verwendet werden, wenn diese technische Identität bewusst stabil gehalten wird.
- Reine Code- oder Namespace-Refactorings dürfen bestehende Audit-Log-Einträge nicht semantisch ungültig machen.
- Neue `object_type`-Werte entstehen nur mit neuen tatsächlich protokollierbaren Entity-Typen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.59

Gegenüber Version 0.58 wurde die Beschreibung von JSON-Feldern datenbankneutral formuliert:

- Physische Datenbanktypen wie `JSONB` werden im fachlichen Datenmodell nicht mehr festgelegt.
- Felder wie `before_state`, `after_state`, `audit_settings` oder andere strukturierte Inhalte werden konzeptionell als JSON-Dokument bzw. JSON-Struktur beschrieben.
- Die konkrete physische Abbildung erfolgt durch Entity Framework und den jeweiligen Datenbankprovider für PostgreSQL bzw. Microsoft SQL Server.
- Datenbankspezifische Typdetails gehören damit in Implementierung, Migrationen und Coding Guidelines, nicht in das fachliche Zielmodell.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.58

Gegenüber Version 0.57 wurden Benutzerzuordnung und Datenbankplattform präzisiert:

- `system_audit_log.user_id` ist verpflichtend und nicht `NULL`.
- `users.user_id = 0` ist dauerhaft für den internen Systembenutzer reserviert.
- Der Systembenutzer existiert als realer Datensatz in `users`, ist nicht anmeldbar, nicht löschbar und nicht über die normale Benutzerverwaltung editierbar.
- Systemseitig ausgelöste Audit-Log-Ereignisse werden mit `user_id = 0` protokolliert.
- Reguläre Benutzer sowie technische/API-Benutzer verwenden IDs ab `1`.
- API-Zugänge werden immer einem Benutzer zugeordnet und über dessen Rollen und Berechtigungen autorisiert.
- Auditarium unterstützt PostgreSQL und Microsoft SQL Server als Datenbankplattformen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.57

Gegenüber Version 0.56 wurde das Feld `system_audit_log.context` entfernt:

- `context` ist im aktuellen Audit-Log-Modell nicht mehr erforderlich.
- Der Bezug eines Ereignisses wird über `object_type` und `object_id` eindeutig hergestellt.
- Bei feldbezogenen Ereignissen ergänzt `attribute` den fachlichen Kontext.
- `before_state` und `after_state` enthalten die tatsächlich geänderten Werte.
- Zusätzliche unspezifische Kontextdaten werden nicht in einem generischen JSON-Feld gesammelt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.56

Gegenüber Version 0.55 wurde die Regel für `system_audit_log.action` präzisiert:

- `action` ist ein definierter technischer Action-Code und kein Freitextfeld.
- Die Menge zulässiger Action-Codes wird zentral vorgegeben und technisch validiert.
- Neue Action-Codes werden nur eingeführt, wenn tatsächlich ein neuer fachlich oder technisch unterscheidbarer Ereignistyp benötigt wird.
- Objekt- oder feldspezifische Varianten werden vermieden, wenn `object_type`, `object_id` und gegebenenfalls `attribute` den Kontext bereits eindeutig liefern.
- Ein vollständiger Action-Katalog wird im Konzeptstand nicht vorab festgelegt, sondern bedarfsgerecht während der Implementierung ergänzt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.55

Gegenüber Version 0.54 wurde die Retention des `system_audit_log` präzisiert:

- Audit-Log-Retention führt nicht zu einer bedingungslosen zeitbasierten Löschung.
- Eine Retention-Frist bestimmt nur, ab wann ein Audit-Log-Eintrag grundsätzlich für einen Purge infrage kommt.
- Solange das zugehörige Ursprungsobjekt noch existiert, dürfen dessen Audit-Log-Einträge nicht gelöscht werden.
- Dies gilt ausdrücklich auch für soft-gelöschte Objekte, da diese innerhalb ihrer Retention wiederhergestellt werden können.
- Audit-Log-Einträge dürfen erst dann purgeberechtigt werden, wenn das Ursprungsobjekt physisch entfernt wurde und keine weiterhin existierenden abhängigen Fachobjekte auf diese historische Information angewiesen sind.
- Damit bleiben alle Informationen erhalten, auf die Auditarium für die historische Darstellung und Wiederherstellbarkeit eines noch existierenden Fachobjekts angewiesen ist.

Zusätzlich wurde für `system_audit_log` festgelegt:

- `before_state` und `after_state` bilden bei Änderungen grundsätzlich nur die tatsächlich geänderten Attribute ab und sind keine Vollsnapshots des Fachobjekts.
- Das Audit-Log bleibt append-only; eine spätere physische Bereinigung erfolgt ausschließlich über den kontrollierten Retention-/Purge-Prozess.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.54

Gegenüber Version 0.53 wurde eine redundante Beziehung aus `audits` entfernt:

- `audits.document_id` entfällt.
- Ein Audit referenziert ausschließlich die konkrete `catalog_version_id`.
- Das zugehörige Dokument ergibt sich eindeutig über `catalog_version.document_id`.
- Damit wird die bereits an anderen Stellen verwendete Regel fortgeführt, redundante übergeordnete Fremdschlüssel nicht zusätzlich zu speichern.
- Auch im DRAFT muss eine konkrete Katalogversion gewählt sein; die Dokumentzuordnung wird daraus abgeleitet.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.53

Gegenüber Version 0.52 wurde die Behandlung der Antwortmetadaten ausdrücklich festgelegt:

- `audit_questions.answered_at` bleibt direkt am Antwortobjekt erhalten.
- `audit_questions.answered_by` bleibt direkt am Antwortobjekt erhalten.
- Beide Felder gehören zur aktuell gültigen Antwort und sind keine bloß redundanten Historienmetadaten.
- Das `system_audit_log` kann zusätzlich frühere Änderungen an Antworten protokollieren, ersetzt `answered_at` und `answered_by` jedoch nicht.
- Dasselbe Grundprinzip gilt für originäre Erstellungsmetadaten wie `created_at` und `created_by`: Sie bleiben am jeweiligen Fachobjekt, soweit sie fachlich vorgesehen sind.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.52

Gegenüber Version 0.51 wurden die Zustandsmetadaten von `catalog_versions` konsolidiert:

- `catalog_versions.state_changed_at` entfällt.
- `catalog_versions.state_changed_by` entfällt.
- `catalog_versions.catalog_state` bleibt als aktueller fachlicher Zustand erhalten.
- Wer und wann den Katalogzustand geändert hat, wird ausschließlich im `system_audit_log` nachvollzogen.
- Ein eigener `state_reason` ist für Katalogversionen weiterhin nicht vorgesehen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.51

Gegenüber Version 0.50 wurde das Soft-Delete-Modell ausdrücklich festgelegt:

- Soft-Delete-Metadaten bleiben vollständig direkt am jeweiligen Fachobjekt.
- Dazu gehören `deleted_at`, `deleted_by` und `deletion_reason`.
- Diese Felder sind Teil des aktuellen operativen Löschzustands und werden nicht ausschließlich aus dem `system_audit_log` abgeleitet.
- `deleted_at` wird insbesondere für Sichtbarkeit, Wiederherstellung und Retention benötigt.
- `deleted_by` und `deletion_reason` bleiben ebenfalls unmittelbar am Objekt verfügbar.
- Das `system_audit_log` protokolliert Lösch-, Wiederherstellungs- und Purge-Ereignisse zusätzlich als Historie, ersetzt die Soft-Delete-Metadaten jedoch nicht.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.50

Gegenüber Version 0.49 wurden zwei verbliebene Inkonsistenzen aus den vorherigen Konsolidierungen bereinigt:

- In der Feldliste von `users` wurde der veraltete Eintrag `status` durch das bereits festgelegte boolesche Feld `is_active` ersetzt.
- `system_audit_log.attribute` wurde in die Felddefinition des Audit-Logs aufgenommen. Das Feld wird bereits für generische feldbezogene Ereignisse wie `STATE_CHANGED` verwendet.
- Die Beschreibung der protokollierten Audit-Aktivitäten wurde an das aktuelle Modell ohne eigenen Zustand von `audit_questions` angepasst.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.49

Gegenüber Version 0.48 wurden die Änderungsmetadaten des Audit-Zustands weiter konsolidiert:

- `audits.state_changed_at` entfällt.
- `audits.state_changed_by` entfällt.
- `audits.state_reason` bleibt als fachliche Begründung des aktuellen Audit-Zustands erhalten.
- Wer und wann den Audit-Zustand geändert hat, wird ausschließlich im `system_audit_log` nachvollzogen.
- Damit wird das bereits für andere Fachobjekte verwendete Prinzip konsequent fortgeführt: fachliche Begründung am Objekt, Änderungsverlauf im Audit-Log.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.48

Gegenüber Version 0.47 wurde `audits.started_at` entfernt:

- `started_at` wird nicht mehr direkt am Audit gespeichert.
- Der Beginn der Bearbeitung entspricht dem ersten Zustandswechsel von `READY` nach `IN_PROGRESS`.
- Dieser Zeitpunkt wird bei Bedarf aus dem ersten passenden `STATE_CHANGED`-Ereignis im `system_audit_log` abgeleitet.
- Damit wird eine weitere redundante historische Zeitangabe aus dem Fachobjekt entfernt.
- `created_at` bleibt erhalten, da der Erstellungszeitpunkt eine originäre Eigenschaft des Audits ist.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.47

Gegenüber Version 0.46 wurde der Nutzungszustand von `audit_units` um eine fachliche Begründung ergänzt:

- `audit_units.usage_state_reason` wurde eingeführt.
- Das Feld beschreibt, warum eine Prüfeinheit aktuell `ACTIVE` oder `INACTIVE` ist.
- Insbesondere bei `INACTIVE` kann damit der fachliche Hintergrund direkt am Objekt nachvollzogen werden, beispielsweise Stilllegung, Schließung oder Ablösung.
- Wer und wann `usage_state` geändert hat, wird weiterhin ausschließlich im `system_audit_log` protokolliert.
- Zusätzliche Änderungsmetadaten wie `usage_state_changed_at` oder `usage_state_changed_by` werden nicht in `audit_units` gespeichert.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.46

Gegenüber Version 0.45 wurde der Aktivierungszustand von Benutzern vereinfacht:

- `users.status` entfällt.
- Stattdessen wird das boolesche Feld `users.is_active` verwendet.
- `is_active = true` bedeutet: Der Benutzer darf sich grundsätzlich anmelden und Auditarium verwenden.
- `is_active = false` bedeutet: Der Benutzer ist deaktiviert und darf sich nicht anmelden.
- Weitere fachliche Benutzerzustände sind nicht vorgesehen.
- Änderungen an `is_active` werden im `system_audit_log` nachvollzogen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.45

Gegenüber Version 0.44 wurde der Nutzungszustand von `audit_units` an die bestehende Namenskonvention angeglichen:

- `audit_units.status` wurde in `audit_units.usage_state` umbenannt.
- Die zulässigen Werte bleiben `ACTIVE` und `INACTIVE`.
- `usage_state` beschreibt auch bei Prüfeinheiten ausschließlich die lokale Nutzbarkeit für neue Audits.
- Dokumente und Prüfeinheiten verwenden damit dasselbe Feldkonzept:
  - `documents.usage_state`
  - `audit_units.usage_state`
- Die konkreten Zustandswerte bleiben fachlich objektspezifisch (`DEPRECATED` bei Dokumenten, `INACTIVE` bei Prüfeinheiten).

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.44

Gegenüber Version 0.43 wurde `documents.validity_state` vollständig entfernt:

- `validity_state` besitzt keinen eigenständigen fachlichen Nutzen mehr.
- Der Lebenszyklus und die lokale Nutzbarkeit eines Dokuments werden vollständig über `usage_state` (`ACTIVE` / `DEPRECATED`) abgebildet.
- Zusätzliche Gültigkeitszustände auf Dokumentebene werden nicht geführt.
- Audit-Log-Beispiele und Beschreibungen wurden entsprechend bereinigt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.43

Gegenüber Version 0.42 wurden zwei offene Konsolidierungspunkte umgesetzt:

- `documents.validity_status` wurde in `documents.validity_state` umbenannt.
- Für technische Bezeichner wird damit die bestehende `*_state`-Terminologie konsistent fortgeführt.
- Zustandswechsel werden im `system_audit_log` generisch über `action = STATE_CHANGED` protokolliert.
- Der betroffene fachliche Zustand wird über ein optionales Feld `attribute` angegeben, beispielsweise `usage_state`, `validity_state`, `catalog_state` oder `audit_state`.
- Objektbezogene Spezialaktionen wie `DOCUMENT_USAGE_STATE_CHANGED` oder `CATALOG_STATE_CHANGED` sind nicht vorgesehen.
- `object_type`, `object_id`, `action` und `attribute` liefern gemeinsam den fachlichen Kontext eines Zustandswechsels.
- Für häufige objektbezogene Historienabfragen wird ein geeigneter zusammengesetzter Index vorgesehen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.42

Gegenüber Version 0.41 wurde der Nutzungszustand von `documents` konsolidiert:

- `usage_state_changed_at` entfällt.
- `usage_state_changed_by` entfällt.
- `usage_state` bleibt als aktueller lokaler Nutzungszustand erhalten.
- `usage_state_reason` bleibt als fachliche Begründung des aktuellen Nutzungszustands erhalten.
- Wer und wann einen Zustandswechsel durchgeführt hat, wird ausschließlich im `system_audit_log` nachvollzogen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.41

Gegenüber Version 0.40 wurde `documents` weiter vereinfacht:

- `processed_at` entfällt.
- Der Zeitpunkt der erstmaligen Aufbereitung eines Dokuments wird nicht als eigener fachlicher Wert gespeichert.
- Entstehung und Bearbeitung der zugehörigen Katalogversionen bleiben über deren Metadaten und das `system_audit_log` nachvollziehbar.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.40

Gegenüber Version 0.39 wurde `catalog_versions` weiter konsolidiert:

- `ready_at` und `ready_by` entfallen.
- `change_reason` entfällt.
- Der aktuelle Zustandswechsel wird weiterhin durch `catalog_state`, `state_changed_at` und `state_changed_by` beschrieben.
- Frühere Zustandswechsel und Freigaben werden ausschließlich im `system_audit_log` nachvollzogen.
- `notes` bleibt als einziges optionales Freitextfeld für zusätzliche fachliche oder administrative Hinweise zur Katalogversion bestehen.
- Es wird keine zusätzliche Begründungspflicht für das Anlegen oder Ändern einer Katalogversion eingeführt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.39

Gegenüber Version 0.38 wurde das gesamte Dokument strukturell und redaktionell konsolidiert:

- die versehentlich unterhalb der Versionshistorie platzierte Berechtigungsmatrix wurde entfernt und die gültigen Audit-Lebenszyklusrechte in die reguläre Rechte-Matrix integriert,
- Kapitel- und Unterkapitelnummerierung wurden vollständig neu geprüft und konsistent fortgeführt,
- beschädigte Formulierungen rund um `document_elements` und prüfbare Dokumentelemente wurden korrigiert,
- veraltete Einzel-Finalisierung von Auditfragen wurde vollständig entfernt,
- die Tabellen `documents`, `audits` und `catalog_versions` wurden um versehentlich verlorene Pflicht-IDs bzw. Beziehungen ergänzt,
- die Response-Policy wurde überall auf das aktuelle Modell als Bestandteil der Audit-Einstellungen vereinheitlicht,
- die Auswahl der Katalogversion wurde auf die aktuelle DRAFT-/Publish-Logik angepasst,
- Importvalidierung, Importvorschau und Wiederholungslogik wurden auf das aktuelle iterative DRAFT-/Revision-Modell vereinheitlicht,
- veraltete natürliche Schlüssel und implizite Vollständigkeitsanforderungen beim KI-Import wurden entfernt,
- Scope-Zuordnungen wurden an einer einzigen fachlich passenden Stelle konsolidiert,
- der aktuelle fachliche Kern am Dokumentende wurde auf die heutige Publish-, Materialisierungs- und Redundanzlogik aktualisiert,
- offensichtliche redaktionelle Altlasten, doppelte Aussagen und widersprüchliche Begriffe wurden entfernt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.38

Gegenüber Version 0.37 wurden redaktionelle und strukturelle Inkonsistenzen bereinigt:

- Veraltete Hinweise auf eine separate Finalisierung einzelner Auditfragen wurden entfernt.
- Verbliebene Felder wie `item_state` auf Ebene von `audit_questions` wurden entfernt.
- Doppelte oder widersprüchliche Zustandsmetadaten in `audits` wurden bereinigt.
- Veraltete Bezeichnungen rund um einen vollständigen Audit-Unit-Kontext wurden auf das aktuelle Modell `audit_unit_context` vereinheitlicht.
- Mehrfach beschriebene Tabellen und Lebenszyklusregeln wurden auf den aktuell gültigen Sollzustand konsolidiert.
- Veraltete Importbeispiele mit `statements`, Fragenstatus und Pflicht-Vollständigkeit wurden durch das aktuelle `document_elements`- und DRAFT-Modell ersetzt.
- Aggregate- und Retention-Beispiele wurden auf `document_elements`, `audit_questions` und die aktuelle Lebenszykluslogik aktualisiert.
- Der Haupttext beschreibt ausschließlich das aktuelle Modell; ältere Entwicklungsstände bleiben nur über die Dokumenthistorie nachvollziehbar.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.37

Gegenüber Version 0.36 wurde `document_element_weights` weiter vereinfacht:

- `changed_at` und `changed_by` entfallen aus `document_element_weights`.
- Die Tabelle enthält nur noch `element_id` und `weight`.
- Der aktuelle fachliche Zustand wird in der Fachtabelle gespeichert.
- Zeitpunkt, Benutzer und Änderungshistorie werden ausschließlich im `system_audit_log` geführt.
- Eine zusätzliche Begründung direkt an der Gewichtung ist derzeit nicht vorgesehen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.36

Gegenüber Version 0.35 wurden redundante Fremdschlüssel in den Katalogtabellen ausgeschlossen:

- `questions` referenziert ausschließlich das zugehörige `document_element` über `element_id`.
- `questions` erhält weder `catalog_version_id` noch `document_id`.
- Die Herkunft einer Frage wird über die Beziehungskette `question → document_element → catalog_version → document` bestimmt.
- `document_element_weights` referenziert ausschließlich `element_id`.
- `question_scope_types` enthält ausschließlich `question_id` und `scope_type_id`.
- Übergeordnete IDs werden nicht zusätzlich gespeichert, wenn sie über bestehende Beziehungen eindeutig ableitbar sind.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.35

Gegenüber Version 0.34 wurde das Datenmodell der `document_elements` vereinfacht:

- Das Feld `reference` entfällt vollständig.
- Für die menschliche Einordnung wird ausschließlich `title` verwendet.
- Nummern, Paragraphenangaben, Kapitelnummern oder Überschriften können gemeinsam im Feld `title` stehen.
- `title` ist nicht grundsätzlich verpflichtend.
- Für ein Dokumentelement mit Fragen ist `text` verpflichtend; `title` bleibt optional.
- Für ein Kontext-Element mit `text`, aber ohne Fragen, bleibt `title` optional.
- Für ein reines Struktur-Element ohne `text` und ohne Fragen ist `title` verpflichtend.
- Ein Dokumentelement ohne `title`, ohne `text` und ohne Fragen ist unzulässig.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.34

Gegenüber Version 0.33 wurde die Eigentümerschaft der Dokumentstruktur präzisiert:

- `document_elements` gehören nicht direkt zu einem `document`, sondern zu einer konkreten `catalog_version`.
- `document_elements.document_id` entfällt.
- `document_elements.catalog_version_id` wird als Pflichtreferenz eingeführt.
- Die Zugehörigkeit zum Dokument ergibt sich über:
  - `document_element → catalog_version → document`
- `parent_element_id` darf nur auf ein Element derselben `catalog_version_id` verweisen.
- Beim Erzeugen einer neuen Katalogversion als DRAFT-Kopie entsteht ein eigener vollständiger Satz `document_elements` mit eigenen IDs.
- Unterschiedliche Katalogversionen können sich dadurch strukturell nicht vermischen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.33

Gegenüber Version 0.32 wurde der eigene Status von Katalogfragen entfernt:

- Fragen besitzen keinen eigenen fachlichen Lebenszyklus.
- Im Zustand `DRAFT` der Katalogversion können Fragen angelegt, geändert und gelöscht werden.
- Im Zustand `READY` sind die Fragen Bestandteil des freigegebenen Katalogs.
- Änderungen an einer bereits verwendeten `READY`-Katalogversion erfolgen ausschließlich über eine neue Katalogversion.
- Zusätzliche Zustände wie `ENTWURF`, `AKTIV` oder  werden nicht benötigt.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.32

Gegenüber Version 0.31 wurde die Speicherung von Gewichtungen vereinfacht:

- Das fachliche Default-Gewicht eines prüfbaren Dokumentelements beträgt weiterhin `3`.
- Für den Defaultwert wird kein Datensatz in `document_element_weights` benötigt.
- Ein Datensatz wird nur gespeichert, wenn der `AUDIT_MANAGER` explizit ein von `3` abweichendes Gewicht festlegt.
- Wird ein abweichendes Gewicht wieder auf `3` gesetzt, kann der entsprechende Datensatz entfernt werden.
- Beim Publish eines Audits wird immer der effektive Gewichtungswert als `weight_snapshot` gespeichert.
- Fehlt ein Eintrag in `document_element_weights`, wird dabei `weight_snapshot = 3` verwendet.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.31

Gegenüber Version 0.30 wurden die Regeln für Scope-Zuordnungen von Fragen präzisiert:

- Eine Frage darf im Zustand `DRAFT` 0..n Scope-Zuordnungen besitzen.
- Für den Übergang einer Katalogversion auf `READY` muss jede Frage mindestens einem gültigen Scope Type zugeordnet sein.
- Die Kombination aus `question_id` und `scope_type_id` darf nur einmal vorkommen.
- Scope-Zuordnungen bleiben Bestandteil des Katalogs und werden nicht redundant in ein Audit kopiert.
- Beim Publish eines Audits entscheidet ausschließlich die Kombination aus `audit_unit.scope_type` und den `question_scope_types`, ob eine Frage in das Audit aufgenommen wird.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.30

Gegenüber Version 0.29 wurden die Berechtigungen für den Audit-Lebenszyklus konsolidiert:

- `AUDIT_MANAGER` darf Audits im Zustand `DRAFT` anlegen, bearbeiten und löschen.
- `AUDIT_MANAGER` darf ein Audit veröffentlichen (`DRAFT → READY`).
- `AUDIT_MANAGER` darf ein unbeantwortetes `READY`-Audit löschen.
- `AUDIT_MANAGER` darf veröffentlichte Audits auf `CANCELED` setzen.
- `AUDIT_MANAGER` darf ein `CANCELED`-Audit über die Aktion „Audit wieder öffnen“ reaktivieren.
- `AUDITOR` darf Auditfragen in `READY` und `IN_PROGRESS` beantworten, ändern und zurücksetzen.
- `AUDITOR` darf ein vollständig beantwortetes Audit finalisieren.
- `REVIEWER` und `VIEWER` besitzen im Audit-Lebenszyklus ausschließlich lesenden Zugriff.
- Rollen bleiben additiv; ein Benutzer kann mehrere Rollen besitzen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.29

Gegenüber Version 0.28 wurden folgende Festlegungen ergänzt bzw. präzisiert:

- Ein `FINALIZED`-Audit erhält kein separates Gesamturteil wie `PASSED`, `FAILED`, Prozentwert oder Ampel.
- Auditarium speichert und zeigt technisch ableitbare Fakten; eine übergreifende fachliche Bewertung wird nicht erzwungen.
- `FINALIZED` bedeutet ausschließlich, dass alle Auditfragen vollständig beantwortet und die formalen Abschlussbedingungen erfüllt wurden.
- `CANCELED` ist nicht mehr grundsätzlich irreversibel.
- Ein `CANCELED`-Audit kann durch einen `AUDIT_MANAGER` über die fachliche Aktion „Audit wieder öffnen“ reaktiviert werden.
- Der Zielstatus wird automatisch aus den vorhandenen Antworten bestimmt:
  - keine beantwortete Frage → `READY`
  - mindestens eine beantwortete Frage → `IN_PROGRESS`
- Beim Wiederöffnen erfolgt keine erneute Materialisierung oder Scope-Berechnung.
- Katalogversion, Prüfeinheit-Kontext, Audit-Einstellungen, materialisierte Fragen und vorhandene Antworten bleiben unverändert.
- Das Wiederöffnen erfordert eine Begründung und wird vollständig im `system_audit_log` protokolliert.
- `FINALIZED` bleibt ein irreversibler Abschlusszustand.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.28

Gegenüber Version 0.27 wurden die Bedingungen zur Finalisierung eines Audits präzisiert:

- Ein Audit darf nur finalisiert werden, wenn jede `audit_question` beantwortet wurde.
- Jeder zulässige Antwortwert gilt als beantwortet, einschließlich `NICHT_ANWENDBAR` und `NICHT_FESTSTELLBAR`.
- Zusätzlich müssen alle durch die Audit-Einstellungen geforderten Kommentare und Nachweise vorhanden sein.
- Die Finalisierung wird bewusst durch einen `AUDITOR` ausgelöst.
- Ein unvollständig beantwortetes Audit kann nicht finalisiert werden.
- Wird ein solches Audit nicht weitergeführt, muss es auf `CANCELED` gesetzt werden.
- Ein `CANCELED`-Audit besitzt kein gültiges Abschlussresultat.
- Bereits vorhandene Antworten bleiben dennoch aus Gründen der Nachvollziehbarkeit erhalten und werden nicht gelöscht.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.27

Gegenüber Version 0.26 wurden die Konsistenzregeln für Antworten sowie die Rückkehr von `IN_PROGRESS` nach `READY` präzisiert:

- Für eine unbeantwortete `audit_question` gilt:
  - `result IS NULL`
  - `comment IS NULL`
  - `evidence IS NULL`
  - `answered_at IS NULL`
  - `answered_by IS NULL`
- Sobald `result` gesetzt wird, werden `answered_at` und `answered_by` automatisch gesetzt bzw. bei einer späteren Änderung aktualisiert.
- `comment` und `evidence` richten sich bei beantworteten Fragen nach den Audit-Einstellungen und können dort optional oder verpflichtend sein.
- Beim Zurücksetzen einer Antwort auf `NULL` werden `comment`, `evidence`, `answered_at` und `answered_by` ebenfalls geleert.
- Werden in einem Audit im Zustand `IN_PROGRESS` alle Antworten wieder zurückgesetzt, wechselt das Audit automatisch zurück auf `READY`.
- Damit ist ein vollständig zurückgesetztes, noch nicht abgeschlossenes Audit wieder löschbar.
- `FINALIZED` und `CANCELED` bleiben unveränderliche Abschlusszustände und können nicht auf diese Weise zurückgesetzt werden.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.26

Gegenüber Version 0.25 wurden die Löschregeln für Audits präzisiert:

- Ein Audit im Zustand `DRAFT` darf gemäß den allgemeinen Löschregeln gelöscht bzw. soft-deleted werden.
- Ein Audit im Zustand `READY` darf ebenfalls gelöscht bzw. soft-deleted werden, solange noch keine Auditfrage beantwortet wurde.
- Für ein löschbares `READY`-Audit muss für alle `audit_questions` gelten: `result IS NULL`.
- Sobald mindestens eine Auditfrage beantwortet wurde, wechselt das Audit automatisch auf `IN_PROGRESS` und ist nicht mehr löschbar.
- Audits in den Zuständen `IN_PROGRESS`, `FINALIZED` und `CANCELED` bleiben dauerhaft Bestandteil der Audit-Historie.
- Soll ein bereits bearbeitetes Audit nicht fortgeführt werden, ist `CANCELED` statt Löschung zu verwenden.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.25

Gegenüber Version 0.24 wurden die Metadaten der Audit-Zustandswechsel vereinheitlicht:

- Separate Felder wie `state_changed_at`, `state_changed_by`, `state_changed_at` oder `state_changed_by` werden nicht benötigt.
- Das Audit verwendet einheitlich:
  - `audit_state`
  - `state_changed_at`
  - `state_changed_by`
  - `state_reason`
- Diese Felder beschreiben jeweils den aktuellen letzten Zustandswechsel.
- Bei `CANCELED` enthält `state_reason` die Abbruchbegründung.
- Bei `FINALIZED` kann `state_reason` optional eine Abschlussbegründung enthalten.
- Die vollständige Historie aller Zustandswechsel wird ausschließlich im `system_audit_log` geführt.
- Dadurch werden redundante Zustandsmetadaten vermieden.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.24

Gegenüber Version 0.23 wurde der Zustand `CANCELED` präzisiert:

- Ein Audit im Zustand `DRAFT` kann nicht auf `CANCELED` gesetzt werden.
- Ein nicht mehr benötigter `DRAFT` kann gemäß den allgemeinen Löschregeln gelöscht bzw. soft-deleted werden.
- `CANCELED` ist ausschließlich für bereits veröffentlichte Audits vorgesehen.
- `READY → CANCELED` und `IN_PROGRESS → CANCELED` sind zulässige Übergänge.
- `CANCELED` ist ein protokollierter Abbruchzustand.
- Bereits materialisierte Inhalte und vorhandene Antworten bleiben bei einem Abbruch vollständig erhalten.
- Ein Abbruch speichert mindestens Zeitpunkt, Benutzer und Begründung.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.23

Gegenüber Version 0.22 wurde eine zusätzliche Publish-Bedingung festgelegt:

- Ein Audit im Zustand `DRAFT` darf eine Konfiguration besitzen, aus der aktuell keine passende Auditfrage resultiert.
- Die Vorschau darf entsprechend 0 prüfbare Dokumentelemente und 0 Auditfragen anzeigen.
- Ein Publish ist jedoch nur zulässig, wenn nach allen Filtern mindestens eine `audit_question` verbleibt.
- Ein Audit ohne prüfbaren Inhalt darf nicht auf `READY` gesetzt und damit nicht fachlich manifestiert werden.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.22

Gegenüber Version 0.21 wurde die Behandlung von Audit-Abhängigkeiten im Zustand `DRAFT` vereinfacht:

- `document_id`, `catalog_version_id`, `audit_unit_id` und weitere Audit-Einstellungen dürfen bereits im `DRAFT` gespeichert und beliebig verändert werden.
- Es wird kein zusätzlicher Modus wie `LATEST_READY` oder `FIXED` benötigt.
- Auch bei einem erneut durchgeführten Audit können bestehende Werte einfach in den neuen `DRAFT` übernommen und dort geändert werden.
- Erst der bewusste Publish-Schritt schließt die Konfiguration ab.
- Beim Publish werden alle fachlichen Abhängigkeiten und Voraussetzungen validiert.
- Erst nach erfolgreicher Validierung wird das Audit materialisiert und auf `READY` gesetzt.
- Ab `READY` sind die festgelegten Abhängigkeiten unveränderlich.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.21

Gegenüber Version 0.20 wurde die historische Abbildung der Prüfeinheit vereinfacht:

- Ein vollständiger `audit_unit_context` entfällt.
- Stattdessen wird nur ein kompakter `audit_unit_context` gespeichert.
- Dieser enthält ausschließlich historisch relevante und später veränderbare Angaben zur Prüfeinheit.
- Vorgesehen sind mindestens:
  - Name der Prüfeinheit zum Zeitpunkt des Übergangs auf `READY`,
  - `scope_type_id` zu diesem Zeitpunkt,
  - verständlicher Hierarchiepfad der Prüfeinheit zu diesem Zeitpunkt.
- Die eigentliche `audit_unit_id` bleibt als technische Referenz bestehen.
- Nicht fachlich notwendige Angaben wie Status, Notizen oder sonstige Metadaten werden nicht redundant gespeichert.
- Grundsatz: Es wird nur der historische Kontext persistiert, der für das spätere Verständnis des Audits erforderlich ist.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.20

Gegenüber Version 0.19 wurde die Response-Policy-Regeln vereinfacht:

- Die Response-Policy-Regeln wird nicht als separates fachliches Objekt behandelt.
- Ihre Einstellungen sind Bestandteil der Konfiguration des jeweiligen Audits.
- Im Zustand `DRAFT` können diese Einstellungen durch den `AUDIT_MANAGER` verändert werden.
- Mit dem Übergang `DRAFT → READY` werden sämtliche Audit-Einstellungen unveränderlich.
- Ein separates `audit_settings` ist damit nicht erforderlich.
- Komfortvorlagen oder eigenständige Response-Policy-Objekte sind nicht Bestandteil des aktuellen Modells.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.19

Gegenüber Version 0.18 wurde der Lebenszyklus einzelner Auditfragen vereinfacht:

- Ein eigener Status pro `audit_question` entfällt.
- Felder wie `question_state`, `state_changed_at`, `state_changed_by` und `state_reason` auf Fragenebene werden nicht benötigt.
- `result = NULL` bedeutet unbeantwortet.
- `result != NULL` bedeutet beantwortet.
- Ob eine Auditfrage bearbeitet werden darf, ergibt sich ausschließlich aus dem Zustand des übergeordneten Audits.
- Bei `READY` und `IN_PROGRESS` dürfen Auditfragen bearbeitet werden.
- Bei `FINALIZED` und `CANCELED` sind Auditfragen unveränderlich.
- Ein separates Finalisieren einzelner Auditfragen ist nicht vorgesehen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.18

Gegenüber Version 0.17 wurde der Lebenszyklus eines Audits präzisiert:

- `DRAFT` beschreibt ausschließlich die noch veränderbare Konfiguration eines Audits.
- Im Audit-Zustand `DRAFT` werden nur grundlegende Auditdaten und konfigurierbare Angaben gespeichert.
- Im `DRAFT` existieren noch keine `audit_document_elements`, `audit_questions` oder Audit-Snapshots.
- Eine Vorschau des späteren Prüfumfangs wird im `DRAFT` dynamisch aus der aktuellen Konfiguration berechnet und nicht persistiert.
- Der `AUDIT_MANAGER` schließt die Konfiguration bewusst ab.
- Erst dabei wird die konkrete `catalog_version_id` festgelegt und das Audit atomar materialisiert.
- Dabei entstehen die Scope-gefilterten `audit_document_elements`, `audit_questions`, `weight_snapshot`, `audit_unit_context` und `audit_settings`.
- Nach erfolgreicher Materialisierung wechselt das Audit in den Zustand `READY`.
- `READY` bedeutet: vollständig konfiguriert, unveränderlich materialisiert und für Auditoren bearbeitbar.
- Ab `READY` sind Prüfeinheit, Dokument, Katalogversion, Audit-Einstellungen und die Zusammensetzung des Audits nicht mehr veränderbar.
- Die erste beantwortete Auditfrage führt automatisch zu `READY → IN_PROGRESS`.
- Die Materialisierung erfolgt transaktional: vollständig oder gar nicht.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.17

Gegenüber Version 0.16 wurden folgende Festlegungen ergänzt:

- Ein bestimmtes `element_id` darf innerhalb desselben Audits höchstens einmal als `audit_document_element` vorkommen.
- Eine bestimmte `question_id` darf innerhalb desselben `audit_document_element` höchstens einmal als `audit_question` vorkommen.
- Ein `audit_document_element` gehört genau zu einem Audit.
- Eine `audit_question` gehört genau zu einem `audit_document_element`.
- Für `audit_document_elements` und `audit_questions` wird keine eigene `sort_order` gespeichert.
- Die Reihenfolge im Audit wird aus `document_elements.sort_order` und `questions.sort_order` der fest gebundenen Katalogversion abgeleitet.
- Dadurch werden redundante Reihenfolgendaten im Audit vermieden.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.16

Gegenüber Version 0.15 wurden folgende Festlegungen ergänzt:

- Beim Erzeugen eines Audits werden ausschließlich Fragen übernommen, deren Scope-Zuordnung zum `scope_type` der gewählten Prüfeinheit passt.
- Ein `audit_document_element` wird nur erzeugt, wenn nach der Scope-Filterung mindestens eine zugehörige Frage für das konkrete Audit verbleibt.
- Reine Struktur- und Kontext-Elemente sowie prüfbare Dokumentelemente ohne passende Fragen werden nicht in das Audit übernommen.
- Jede `audit_question` muss auf eine Frage verweisen, die tatsächlich zum `element_id` ihres übergeordneten `audit_document_element` gehört.
- `element_id` und `question_id` eines Audits müssen aus genau der `catalog_version_id` stammen, an die das Audit gebunden ist.
- Inkonsistente Kombinationen aus Audit-Dokumentelementen und fremden Fragen sind technisch auszuschließen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.15

Gegenüber Version 0.14 wurden redaktionelle und terminologische Bereinigungen vorgenommen:

- Überschrift „Lebenszyklus einer Auditfrage“ wurde zu „Lebenszyklus einer Auditfrage“ korrigiert.
- Historische bzw. inzwischen verworfene Modellbegriffe werden im aktuellen Konzept nicht mehr erwähnt.
- Das Dokument beschreibt ausschließlich das aktuell gültige Datenmodell und keine Umbenennungs- oder Migrationsgeschichte.
- Historische Änderungen bleiben bei Bedarf über ältere Konzeptversionen nachvollziehbar.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.14

Gegenüber Version 0.13 wurden folgende Konsolidierungen und Modellierungsgrundsätze festgelegt:

- Prüffähige Aussagen werden ausschließlich durch `document_elements` mit mindestens einer zugeordneten Frage repräsentiert.
- `questions.element_id` wird durch `questions.element_id` ersetzt.
- `document_element_weights` wird in `document_element_weights` umbenannt.
- Gewichtungen existieren nur für tatsächlich prüfbare Dokumentelemente.
- `audit_document_elements` wird in `audit_document_elements` umbenannt.
- `audit_document_element_id` wird in `audit_document_element_id` umbenannt.
- `audit_questions` wird in `audit_questions` umbenannt.
- `audit_document_elements` enthält ausschließlich die im konkreten Audit tatsächlich relevanten prüfbaren Dokumentelemente.
- Reine Struktur- und Kontext-Elemente werden nicht redundant in das Audit übernommen.
- Für Dokumentelemente, Fragetexte und Katalogstruktur werden keine zusätzlichen Inhalts-Snapshots im Audit gespeichert.
- Historische Stabilität wird durch die unveränderliche referenzierte `catalog_version` und deren geschützte Inhalte sichergestellt.
- Snapshots werden nur dort verwendet, wo sich referenzierte Daten nach Audit-Erstellung noch ändern dürfen und dadurch sonst die historische Bedeutung verloren ginge.
- `weight_snapshot`, `audit_unit_context` und `audit_settings` bleiben deshalb fachlich sinnvoll.
- Grundsatz: historische Nachvollziehbarkeit bevorzugt durch stabile Referenzen und unveränderliche Katalogdaten statt durch redundante Kopien.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.13

Gegenüber Version 0.12 wurden folgende Festlegungen ergänzt bzw. präzisiert:

- Die KI-Unterstützung erhält einen festen, wiederkehrenden Ort im DRAFT-Workflow.
- Der Ort bleibt gleich, unabhängig davon, ob der DRAFT leer oder bereits teilweise bearbeitet ist.
- Der Inhalt des dort angebotenen KI-Arbeitspakets richtet sich nach dem aktuellen Zustand des DRAFTs.
- Bei einem leeren DRAFT wird ein Initial-Paket für die erste Strukturierung und optionale Fragenerstellung bereitgestellt.
- Bei einem bereits bearbeiteten DRAFT wird ein Update-Paket bereitgestellt, das zusätzlich den aktuellen Arbeitsstand, stabile IDs und die DRAFT-Revision enthält.
- Der Benutzer muss nicht selbst entscheiden, welches Prompt-/Schema-Paket benötigt wird; Auditarium stellt kontextabhängig das passende Paket bereit.
- Im Dialog zum Anlegen eines neuen Dokuments wird ein zusätzlicher Einstieg in die KI-Unterstützung angeboten.
- Dieser Einstieg erzeugt keinen separaten KI-Workflow, sondern öffnet nach dem Anlegen denselben zentralen KI-Bereich der neuen DRAFT-Katalogversion.
- KI-Unterstützung bleibt eine externe Vorbereitungs- und Importhilfe; Auditarium selbst benötigt dafür kein eingebettetes LLM.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.12

Gegenüber Version 0.11 wurden folgende Festlegungen ergänzt:

- Der Übergang `DRAFT → READY` erfolgt nicht durch einen einfachen unbestätigten Klick.
- Vor der Freigabe muss ein Bestätigungsdialog klar auf die Folgen hinweisen.
- Der `AUDIT_MANAGER` muss die Freigabe bewusst bestätigen.
- Die Katalogversion speichert mindestens `ready_at` und `ready_by`.
- Der allgemeine aktuelle Zustandswechsel wird zusätzlich über `state_changed_at` und `state_changed_by` nachvollzogen.
- Wird eine noch unbenutzte `READY`-Katalogversion wieder auf `DRAFT` gesetzt, bleiben historische Freigabeereignisse im `system_audit_log` erhalten.
- Punkt 3 der Konzeptliste – Übergang `DRAFT → READY` – gilt damit fachlich als abgeschlossen.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.11

Gegenüber Version 0.10 wurden folgende Festlegungen ergänzt bzw. präzisiert:

- `document_elements` bilden die komplette frei verschachtelbare Struktur eines Dokuments ab.
- `document_elements` besitzen `document_id`, optional `parent_element_id`, `reference`, `title`, optional `text`, `sort_order` und `notes`.
- `parent_element_id = NULL` kennzeichnet Elemente auf oberster Ebene.
- Parent-Elemente müssen zum selben `document_id` gehören; Circular References sind unzulässig.
- Die fachliche Rolle eines Dokumentelements wird nicht gespeichert, sondern aus Inhalt und Fragen abgeleitet: Fragen vorhanden = prüfbare Aussage; keine Fragen, aber Text = Kontext; keine Fragen und kein Text = Ordnung.
- Sobald Fragen an einem Dokumentelement hängen, muss dieses Element einen `text` besitzen.
- `sort_order` legt die Reihenfolge unter demselben Parent fest und ist nachträglich veränderbar.
- Im `DRAFT` dürfen Dokumentelemente und Fragen frei angelegt, verschoben und gelöscht werden.
- Duplizieren wird zunächst nicht als eigene Funktion vorgesehen.
- Beim Verschieben sind Dokumentzugehörigkeit und Circular References zu validieren.
- Beim Löschen abhängiger Strukturen muss vorab eindeutig angezeigt werden, welche Daten mit gelöscht werden; Cascade Delete ist im `DRAFT` nach bewusster Bestätigung zulässig.
- Fragen ohne Scope-Zuordnung dürfen im `DRAFT` existieren, verhindern aber den Übergang auf `READY`.
- Auditarium prüft vor `READY` nur deterministisch feststellbare formale Konsistenz, nicht die fachliche Vollständigkeit eines Katalogs.
- `READY → DRAFT` ist zulässig, solange noch kein Audit auf diese konkrete Katalogversion verweist.
- Sobald eine Katalogversion von mindestens einem Audit referenziert wird, ist `READY → DRAFT` gesperrt.
- In diesem Fall kann der `AUDIT_MANAGER` eine neue Katalogversion als vollständige Kopie des aktuellen Standes erzeugen; diese startet im Zustand `DRAFT`.
- Eine neue Dokumentversion wird dadurch nicht erzeugt. `document.version` bleibt ausschließlich der Version des Herausgebers vorbehalten.
- Grundsatz: Im `DRAFT` darf gebaut werden; `READY` bedeutet formal konsistent; ein verwendetes `READY` ist historisch eingefroren.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.10

Gegenüber Version 0.9 wurden folgende Festlegungen ergänzt:

- Jedes bestehende Audit kann als Vorlage für ein neues Audit verwendet werden, unabhängig von seinem aktuellen `audit_state`.
- Die UI stellt dafür eine Funktion wie **„Nochmal auditieren“** bzw. **„Neues Audit auf Basis dieses Audits erstellen“** bereit.
- Das neue Audit erhält immer eine neue `audit_id` und startet im Zustand `DRAFT`.
- Antworten, Kommentare, Nachweise, Bearbeitungszeiten und Item-Zustände werden niemals übernommen.
- Ein neues Audit kann wahlweise exakt mit den bisherigen Bedingungen oder mit angepassten Bedingungen erzeugt werden.
- Anpassbar sind insbesondere Audit Unit, Katalogversion, Antwort-Policy und die zu verwendenden Gewichtungen.
- Jede Wiederholung verweist über `origin_audit_id` direkt auf das ursprüngliche Basis-Audit.
- Es gibt keine Kopierketten: Wird eine Wiederholung aus einer Wiederholung erzeugt, referenziert auch sie das ursprüngliche Basis-Audit.
- Für ein neues Audit gelten weiterhin alle aktuellen Nutzbarkeitsregeln, z. B. `ACTIVE`-Dokument und zulässige Audit Unit.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.9

Gegenüber Version 0.8 wurde die Dokumentstruktur fachlich neu geordnet:

- Der manuelle Katalog-Workflow ist ausdrücklich der fachliche Referenzprozess.
- Der DRAFT-Editor steht vor dem KI-Import und definiert, welche Daten überhaupt benötigt werden.
- Der KI-/JSON-Import folgt erst danach als alternativer Befüllungsweg.
- Der bisher früh platzierte Abschnitt „KI-gestützter Regelwerkimport“ wurde hinter den manuellen DRAFT-Workflow verschoben.
- Manueller Workflow und KI-Import verweisen ausdrücklich aufeinander.
- Die bisherigen zwei teilweise redundanten KI-Importabschnitte wurden zu einem konsistenten Abschnitt zusammengeführt.
- Validierung, Importvorschau, idempotenter Import und Abbruch unvollständiger Importe folgen nun direkt auf den KI-Import.
- Abschnitts- und Unterabschnittsnummern wurden entsprechend bereinigt.

Die fachlichen Inhalte bleiben bestehen; geändert wurde vor allem ihre logische Reihenfolge und gegenseitige Referenzierung.

## Änderungen in Version 0.8

Gegenüber Version 0.7 wurden folgende Festlegungen ergänzt:

- Der Fragenkatalog wird nicht mehr primär vom KI-Import her gedacht.
- Auditarium muss einen vollständigen Katalog unabhängig von seiner Herkunft manuell erstellen und bearbeiten können.
- Einführung eines zentralen DRAFT-Editors für Katalogversionen.
- Fachlicher Lebenszyklus einer Katalogversion: `DRAFT → READY`.
- `DRAFT` ist bearbeitbar; `READY` ist inhaltlich unveränderlich.
- KI-/JSON-Import ist künftig nur ein alternativer Befüllungsweg für denselben DRAFT-Editor.
- Importstatus und fachlicher Katalogstatus werden konzeptionell voneinander getrennt.
- Aussagen, Fragen, Scope-Zuordnungen und Hinweise werden im DRAFT-Editor bearbeitet.
- Die Gewichtung einer Aussage erhält im DRAFT-Editor ihren fachlichen Platz.
- Bei `READY` bleiben Aussagen, Fragen und Scope-Zuordnungen unveränderlich; die lokale Gewichtung bleibt weiterhin durch den `AUDIT_MANAGER` pflegbar.
- Ein Audit übernimmt weiterhin die zum Erstellungszeitpunkt gültige Gewichtung als Snapshot.
- Das endgültige KI-JSON-Schema wird erst aus dem manuellen Fachworkflow abgeleitet und nicht vorab als primäres Datenmodell festgelegt.

Die bisherigen fachlichen Festlegungen bleiben bestehen, soweit sie diesen Punkten nicht widersprechen.

## Änderungen in Version 0.7

Gegenüber Version 0.6 wurden folgende Festlegungen ergänzt:

- Fachliche Außerbetriebnahme und Löschung werden klar voneinander getrennt.
- `documents` erhalten einen reversiblen Nutzungszustand `ACTIVE` / `DEPRECATED`.
- Ein `DEPRECATED`-Dokument bleibt vollständig erhalten und historisch referenzierbar, darf aber nicht für neue Audits verwendet werden.
- Die Reaktivierung eines `DEPRECATED`-Dokuments ist erlaubt und wird vollständig protokolliert.
- Bei `audit_units` wird derselbe fachliche Zweck mit `ACTIVE` / `INACTIVE` abgebildet.
- Bei `users` wird derselbe Zweck mit `ACTIVE` / `DISABLED` abgebildet.
- Es wird bewusst kein universelles `usage_state` über alle Tabellen erzwungen.
- `catalog_versions`, `statements`, `questions`, `audits`, `audit_document_elements`, `audit_questions` und `scope_types` behalten ihre jeweils fachlich passenden Lebenszyklen.
- Für neue Audits gilt: Das gewählte Dokument muss `ACTIVE` sein; verwendet wird anschließend automatisch die höchste `READY`-Katalogversion.
- Der Themenbereich Zustände, Außerbetriebnahme, Löschung und Retention bleibt als fachlicher Review-Punkt ausdrücklich offen, da das Modell noch weiter geschärft werden kann.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.6

Gegenüber Version 0.5 wurden folgende Festlegungen ergänzt:

- Einführung eines zentralen unveränderlichen `system_audit_log`.
- Jegliche ändernde und/oder sicherheitsrelevante Aktion wird mit Zeit, Benutzer und Kontext protokolliert.
- Bei Änderungen von Datenbankobjekten werden Vorher- und Nachher-Zustand als JSON/JSONB erfasst.
- Login, Logout, fehlgeschlagene Anmeldungen, Rollenänderungen, Importe, Finalisierungen, Exporte und Löschvorgänge werden ebenfalls protokolliert.
- Secrets, Passwörter, Tokens und vergleichbare sensible Werte dürfen niemals im Audit-Log gespeichert werden.
- Löschungen erfolgen zweistufig über Soft Delete und späteres physisches Purging.
- Soft gelöschte Objekte erhalten mindestens `deleted_at` und `deleted_by`, optional `deletion_reason`.
- Innerhalb der Retention-Zeit ist eine Wiederherstellung vorgesehen.
- Für Soft-Delete-Retention und Audit-Log-Retention existieren getrennte Schwellenwerte.
- Diese Retention-Werte werden nicht über die normale UI konfiguriert, sondern über Deployment-/Umgebungsparameter.
- Das `system_audit_log` wird beim Löschen fachlicher Objekte niemals mit gelöscht.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.5

Gegenüber Version 0.4 wurden folgende Festlegungen ergänzt:

- Auditarium schränkt fachlich ungewöhnliche Vergleiche nicht technisch ein.
- Einführung einer freien Zeitleistenansicht für Audits.
- Scope, Audit Unit, Dokument/Katalog und weitere Merkmale werden in Auswertungen über Farbe, Gruppierung oder Zusatzinformationen sichtbar gemacht.
- Dieselben Auditdaten sollen zusätzlich tabellarisch und als Rohdaten verfügbar sein.
- Export wird als Kernfunktion gleichwertig neben Import, Audit und interner Auswertung behandelt.
- Mindestens CSV- und JSON-Export sind vorgesehen.
- Zusätzlich wird von Anfang an eine versionierte HTTP-API vorgesehen.
- Die API ist zunächst ausschließlich read-only.
- Die API soll dieselben fachlichen Daten und Filtermöglichkeiten bereitstellen wie interne Auswertung und Export.
- API-Versionierung beginnt mit `/api/v1/...`.
- Exportierte Daten enthalten sowohl interne IDs als auch lesbare Kontextinformationen.
- Grundsatz: Keine Sackgassen – alle relevanten Auditdaten müssen über Oberfläche, Datei-Export und API zugänglich sein.

Die bisherigen fachlichen Festlegungen bleiben bestehen.

## Änderungen in Version 0.4

Gegenüber Version 0.3 wurden folgende Festlegungen ergänzt:

- Jedes Audit erhält einen verpflichtenden frei vergebenen `name`.
- Jedes Audit kann eine optionale `description` für Anlass, Ziel und Besonderheiten enthalten.
- Es wird bewusst kein festes `audit_type` wie `PRE_AUDIT` oder `MASTER_AUDIT` eingeführt.
- Der `AUDIT_MANAGER` definiert bei der Erstellung eines Audits eine Antwort-Policy.
- Die Antwort-Policy legt je Antwortwert fest, ob Kommentar und Nachweis optional oder erforderlich sind.
- Die Antwort-Policy kann zusätzlich abhängig von der Gewichtung einer Aussage strengere Anforderungen vorgeben.
- Es gibt keine global fest verdrahtete Kommentar- oder Nachweispflicht, auch nicht bei `NEIN`.
- Die Antwort-Policy wird mit dem Audit eingefroren und bleibt damit historisch nachvollziehbar.

Die bisherigen fachlichen Festlegungen bleiben bestehen, soweit sie diesen Regeln nicht widersprechen.

## Änderungen in Version 0.3

Gegenüber Version 0.2 wurden folgende Festlegungen ergänzt:

- Produktname: **Auditarium**
- Sub-Titel: **Structured audits. Traceable results.**
- Branding-Idee für die Startseite: statischer Text **„Ort für“** mit einer Split-Flap-/Klapptafel-Animation für wechselnde Begriffe.
- Die Animation endet bewusst auf **„Ergebnisse und Nachvollziehbarkeit“**.

Die fachlichen Festlegungen aus Version 0.2 bleiben unverändert bestehen.

## Änderungen in Version 0.2

Gegenüber Version 0.1 wurden folgende fachliche Festlegungen ergänzt:

- Gewichtung erfolgt auf Ebene der Aussagen/Anforderungen, nicht auf Ebene einzelner Fragen.
- Gewichtungsskala `1..5`, Default `3`.
- Gewichtungen sind lokale Priorisierungen und kein Bestandteil des unveränderlichen Regelwerk-Katalogs.
- Pflege der Gewichtungen durch den `AUDIT_MANAGER` in einem eigenen Editor.
- Jedes Audit speichert die zum Erstellungszeitpunkt gültige Gewichtung einer Aussage als Snapshot.
- Einführung der Audit-Ebene `audit_document_elements` zwischen Audit und Auditfragen.
- Das Ergebnis einer Aussage wird deterministisch aus den zugehörigen Fragen abgeleitet.
- Gewichtungen dienen in Reports primär der Sortierung und Priorisierung.
- Es wird bewusst kein rein mathematischer Gesamtscore zur fachlichen Gesamtbewertung verwendet.

---
