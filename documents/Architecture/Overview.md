# Aktuelle Architektur

Auditarium ist ein strukturierter Monolith. Diese Seiten beschreiben die aktuell beabsichtigte Architektur, nicht den Backlog oder die Änderungshistorie. Code und Tests sind der Nachweis des tatsächlich implementierten Verhaltens; die [ADRs](../ADR/README.md) halten die dauerhaften Entscheidungen samt Begründung fest.

## Komponenten und Grenzen

Web und API sind Präsentations- beziehungsweise Transportschichten. Beide reichen Requests über Mediator an gemeinsame BLL-Use-Cases weiter; die BLL besitzt Geschäftssemantik und fachliche Abläufe. DAL, FAL und weitere Infrastruktur behandeln technische Außenbelange wie Persistenz, Dateispeicher und LDAP. Die Webanwendung ruft Auditariums eigene API nicht als internes Backend auf. Präsentationsspezifisches Verhalten bleibt am Rand.

```text
Web / API
    ↓
Mediator
    ↓
BLL-Use-Cases
    ↓
Persistenz / Datei- / Infrastruktur-Services
```

Der gemeinsame Persistenzkern unterstützt PostgreSQL und SQL Server. Identität, Authentifizierung und RBAC sind zentral in die Use-Case-Ausführung eingebunden. Der Katalog- und Audit-Bereich verwaltet versionierte Regelwerke und reproduzierbare Audit-Snapshots; der Job-Laufzeitkern koordiniert Arbeiten über die gemeinsame Datenbank. Die Weboberfläche ist servergerendert und basiert auf lokal bereitgestellten Frontend-Assets. Observability, Konfiguration, Soft-Delete/Retention, Dateispeicher und Audit-Log sind querschnittliche Belange und werden bei den passenden Grenzen referenziert.

Die Präsentations- und Abhängigkeitsgrenzen folgen [ADR 0001](../ADR/0001-application-architecture-and-presentation-boundaries.md).

`Auditarium.DemoData` ist ein separates, im Quellrepository verbleibendes
Development-Tooling-Projekt. Es gehört weder zur Web-/API-Laufzeit noch zu
Produktionsartefakten und wird nicht automatisch ausgeführt. Die vollständige
Tool- und Distributionsgrenze ist in [ADR 0010](../ADR/0010-development-demo-data-tool-and-distribution-boundary.md) festgelegt.

## Themen

- [Persistenz und Startlaufzeit](Persistence.md)
- [Identität und Zugriff](Identity-and-Access.md)
- [Katalog](Catalog.md)
- [Audit-Engine](Audit-Engine.md)
- [Jobs](Jobs.md)
- [UI](UI.md)
