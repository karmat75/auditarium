# Dokumentation und Governance

Diese Seite ordnet die Projektdokumentation ein. Sie verhindert, dass dieselbe
Information in mehreren Artefakten als konkurrierende Wahrheit gepflegt wird.

## Dokumentationskarte

| Bereich | Zweck |
| --- | --- |
| Code und automatisierte Tests | Nachweis des tatsächlich implementierten Verhaltens |
| Architektur-Dokumentation | Gepflegte Beschreibung der aktuell beabsichtigten Architektur; wird in WP13.3 ergänzt |
| ADRs | Dauerhafte Entscheidungen zu wesentlichen Architekturfragen und deren Begründung; werden in WP13.2 ergänzt |
| [GitHub Issues](https://github.com/karmat75/auditarium/issues) | Aktiver Backlog für geplante Arbeit und Änderungen |
| GitHub Project | Workflow-Status und Priorisierung, nicht dauerhafte Architektur-Dokumentation |
| [README.md](../README.md) | Einstieg und Navigation für das Repository, nicht der Architekturkanon |
| [CONTRIBUTING.md](../CONTRIBUTING.md) | Verbindlicher Ablauf für Mitwirkende |
| [Operations](Operations/) | Ausführbare Betriebs- und Runbook-Verfahren |
| [ImportFormat](ImportFormat/) | Versionierte externe Verträge, etwa das Katalog-Importformat |
| [Prompts](Prompts/) | Werkzeughinweise; bei Workflow-Bezug gilt der aktive Issue-basierte Ablauf |
| [Auditarium_Soll_Pflichtenheft.md](Auditarium_Soll_Pflichtenheft.md) | Historisches Material, kein Backlog für künftige Arbeit; die formelle Freeze-Kennzeichnung folgt erst in WP13.4 |

## Änderungen richtig einordnen

Code und Tests reichen aus, wenn eine Änderung keine dokumentierte
Schnittstelle, Architekturentscheidung oder Betriebsanweisung verändert. Für
geplante Arbeit und Änderungen wird ein GitHub Issue verwendet. Ändert sich die
beabsichtigte Architektur, wird die Architektur-Dokumentation aktualisiert.
Ändert sich ein ausführbares Betriebsverfahren, wird das zugehörige Runbook
aktualisiert. Änderungen versionierter externer Formate aktualisieren deren
Formatdokumentation.

## Wann ist ein ADR erforderlich?

Ein ADR dokumentiert eine dauerhafte, bereichsübergreifende
Architekturentscheidung, die zukünftige Implementierungen bewusst bewahren
müssen. Dies betrifft insbesondere:

- System- und Anwendungsgrenzen;
- Sicherheits- oder Berechtigungsmodelle;
- Persistenz- oder Deployment-Strategien;
- Erweiterungs- oder Provider-Modelle.

Kein ADR ist normalerweise nötig für gewöhnliche Implementierungsdetails,
lokale Refactorings, einzelne Handler oder Queries, Bibliotheksnutzung ohne
Architekturvorgabe oder noch unentschiedene Produkt- und Backlog-Fragen.

Akzeptierte ADRs werden nicht umgeschrieben, damit die Historie aktueller
wirkt. Eine spätere Entscheidung ersetzt ein früheres ADR durch ein neues ADR;
beide Entscheidungen verweisen aufeinander.
