# Audit-Engine

Audit Units repräsentieren den prüfbaren organisatorischen Kontext. Sie können hierarchisch gegliedert sein und besitzen einen Scope Type. Ein Audit wird aus einer Audit Unit und einer Katalogversion vorbereitet und beginnt als `DRAFT`. Die Vorschau prüft die vor dem Veröffentlichen relevante Struktur.

Beim Publish wird die anwendbare Dokument- und Fragenstruktur einschließlich der erforderlichen Snapshotdaten materialisiert. Auch die Antwortregeln-Konfiguration wird für das Audit festgelegt. Spätere Katalogänderungen können damit ein veröffentlichtes Audit nicht neu definieren. Historische Reproduzierbarkeit ist der architektonische Grund für diese Snapshots.

Audits können Auditoren zugewiesen sowie von berechtigten Bearbeitern geclaimt und wieder freigegeben werden. Antworten werden an den materialisierten Fragen erfasst; ihr Ablauf folgt dem Audit-Lebenszyklus. Neben `DRAFT` kennt dieser unter anderem vorbereitete/in Bearbeitung befindliche, finalisierte und abgebrochene Zustände. Reporting- und Vergleichsthemen liegen außerhalb dieser Komponentengrenze.

Die gemeinsame Entscheidung zu Katalog-Unveränderlichkeit und Audit-Materialisierung ist [ADR 0006](../ADR/0006-catalog-version-immutability-and-audit-materialization.md).
