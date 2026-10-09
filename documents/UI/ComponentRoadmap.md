# UI/UX-Komponenten-Roadmap

Lebende **Bearbeitungsreihenfolge**, keine vollständige UI-Inventur und kein Ersatz für Issues. Wir analysieren und gestalten **eine fachliche Komponente nach der anderen**. Seitenangaben sind zunächst Prüfkandidaten und bedeuten nicht, dass sämtliche Seiten bereits existieren.

| Reihenfolge | Komponente | Zugehörige Seiten / Arbeitsbereiche zur Prüfung | Stand |
| --- | --- | --- | --- |
| Referenz | Audit Units | Übersicht/Tree Grid, Anlegen, Details, Bearbeiten, Aktionen | Abgeschlossen ([#78](https://github.com/karmat75/auditarium/issues/78)) |
| 1 | Dokumente | Übersicht, Anlegen, Details, Bearbeiten, Aktionen | **Als Nächstes** |
| 2 | Dokumentstruktur | Verschachtelte Elemente, Strukturansicht, Elementverwaltung und -bearbeitung | Geplant |
| 3 | Kataloge und Versionen | Übersicht, Versionen, Details, Status/Freigabe | Geplant |
| 4 | Aussagen und Prüffragen | Aussagen, Fragen, Zuordnung, Bearbeitung | Geplant |
| 5 | Audits | Übersicht, Anlegen, Details, Zuordnung, Status | Geplant |
| 6 | Prüfungsdurchführung | Fragen/Antworten, Kommentare, Fortschritt, Abschluss | Geplant |
| 7 | Auswertung und Vergleiche | Ergebnisse, Filter, Zeitvergleiche, vorhandene Berichte | Geplant |
| Danach | Systemische Komponenten | Benutzer, Rollen/Berechtigungen, Provider, Einstellungen – Reihenfolge später festlegen | Später |

Die Abfolge orientiert sich an fachlichen Abhängigkeiten und darf anhand konkreter Erkenntnisse angepasst werden. **Kein vollständiger Vorab-Rundgang** durch alle Seiten.

## Arbeitszyklus

1. Die **nächste** Komponente und ihre tatsächlich vorhandenen Seiten/Workflows untersuchen.
2. Geeignete Muster aus dem [UI-/UX-Bausteinkatalog](ComponentCatalog.md) übernehmen; neue Anforderungen und Lösungen gemeinsam entscheiden.
3. Abgegrenzte GitHub Issues und PRs, technische Prüfungen, Review sowie Browserabnahme umsetzen.
4. Erkenntnisse, neue oder überholte Muster und den Stand in **beiden** Übersichten nachführen.
5. Erst danach die nächste Komponente beginnen.

Fachliche Erweiterungen nicht ungeplant mit UI-Überarbeitung vermischen. Das [UI Quality Gate #18](https://github.com/karmat75/auditarium/issues/18) begleitet und bündelt die übergreifende Qualitätsprüfung. Architekturregeln: [Architecture/UI.md](../Architecture/UI.md).
