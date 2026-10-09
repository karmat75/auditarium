# UI-/UX-Bausteinkatalog

Lebende Sammlung bewährter **Muster und Referenzen**. Ein Muster ist nicht automatisch schon eine allgemein wiederverwendbare Code-Komponente. Einsatz nur, wenn der fachliche Kontext passt; keine pauschale Übertragung vom Audit-Unit-Tree-Grid.

**Reifegrade:** *Etabliert* = vorhandene Standard-/Referenzlösung; *Erprobt* = validiert, aber Übertragbarkeit noch zu prüfen; *Offen* = erst bei passender Komponente entwerfen.

| Baustein / Muster | Reifegrad | Einsatz / Referenz |
| --- | --- | --- |
| Seitenrahmen, Navigation, Titel | Etabliert | Gemeinsames Razor-/AdminLTE-Layout |
| Theme und Design-Tokens | Etabliert | Bootstrap/AdminLTE, LIGHT/DARK/SYSTEM; Vendor-Dateien unverändert |
| Tabulator Data Grid | Etabliert | Flache nichttriviale Sammlungen, ggf. Paging/Sortieren/Filtern |
| Tabulator Tree Grid | Etabliert | Nur echte Hierarchien; Parent-/Child-Kontext und Such-/Sortierverhalten erhalten; Audit Units |
| Flexible Hauptspalte / kompakte Nebenfelder | Erprobt | Audit-Unit-Tree-Grid; horizontal scrollen statt abschneiden |
| Direkte Zeilenaktionen | Erprobt | Details, Bearbeiten, Löschen; zugänglich und berechtigungsgesteuert |
| Read-only Details und getrenntes Edit | Erprobt | Audit Units; pro Fachablauf auf Eignung prüfen |
| Formularaktionen und Navigation | Erprobt | Speichern, Abbrechen ohne POST, Zurück; Ziel pro Workflow festlegen |
| Status-Badges | Erprobt | **Sichtbarer Text** zusätzlich zur Farbe, barrierearm und themefähig; `aud-status` |
| Tastaturbedienbare Hierarchie | Erprobt | Fokus, Enter/Leertaste, korrektes `aria-expanded` |
| Baumzustand erhalten | Erprobt | Expand/Collapse und Scrollposition bei Sortierung |
| Export und Print | Erprobt | CSV/JSON/Druck bei geeigneten Daten und klarer Semantik |
| Dokumentstruktur-Editor | Offen | Erst bei Dokumentstruktur konkret entwerfen |

## Pflege und Entscheidungen

Bei jeder bearbeiteten Komponente fragen:
1. Welches bestehende Muster passt fachlich?
2. Ist es bereits geteilter Code oder bislang nur eine Seitenreferenz?
3. Welche Erkenntnisse sind allgemein übertragbar, welche bleiben lokal?
4. Sind Berechtigungen, Accessibility, Themes, Navigation und Rückmeldungen konsistent?
5. Muss ein Muster ergänzt, verfeinert, ersetzt oder im Reifegrad angepasst werden?

Änderungen nach jedem Komponenten-Durchgang in diesem Katalog und der [Komponenten-Roadmap](ComponentRoadmap.md) nachführen. Umsetzung/Abnahme in Issues/PRs; verbindliche Architekturentscheidungen in [Architecture/UI.md](../Architecture/UI.md) bzw. ADRs. Startreferenz: [Audit Units #78](https://github.com/karmat75/auditarium/issues/78).
