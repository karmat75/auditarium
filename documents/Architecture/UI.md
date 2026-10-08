# UI

Die servergerenderte Razor-Webanwendung nutzt Bootstrap 5.3+ und AdminLTE 4 als technische UI-Grundlage. Adminator und Patchmon sind ausschließlich visuelle Referenzen, keine technischen Abhängigkeiten oder pixelgenauen Ziele.

Offizielle Distributionsassets werden lokal vendort, unverändert gehalten und ohne CDN- oder Runtime-Internetabhängigkeit ausgeliefert. Die Auditarium-spezifische Theme- und Anpassungsschicht bleibt von den Vendor-Dateien getrennt. Die vorhandenen `DARK`, `LIGHT` und `SYSTEM`-Konzepte verwenden semantische Designtokens und Bootstrap-Farbmodi.

Tabulator ist das standardisierte interaktive Data-Grid- und Tree-Grid-Element für nicht-triviale Sammlungen in der Weboberfläche. Es bleibt ein lokal vendortes Frontend-Element innerhalb einzelner Razor Pages und ersetzt weder Razor Pages noch die gemeinsame BLL. Tabulator-spezifische Request-Parameter und JSON-Formate verbleiben in der Web-Schicht; die BLL stellt präsentationsneutrale Such-, Sortier-, Paging- und Hierarchie-Verträge bereit.

Flache Datenmengen können serverseitiges Paging, Sortieren und Filtern verwenden. Hierarchische Daten wie Audit Units werden als ungepagter Tree Grid dargestellt, damit Parent-/Child-Beziehungen nicht über Seitengrenzen getrennt werden. Suche und Sortierung müssen den hierarchischen Kontext erhalten. Scope-Type-Symbole werden ausschließlich in der Web-Schicht anhand stabiler Scope-Type-Keys zugeordnet und ergänzen, ersetzen aber nicht, die textuelle Bezeichnung.

Die aktuelle Architektur benötigt keine Node-, npm-, Vite- oder Sass-Buildkette. Eine künftige Einführung wäre eine ausdrückliche Architekturänderung. Quelle für Herkunft, Version, Lizenz und Integrität der Dateien ist [VendorAssets.md](../VendorAssets.md), nicht diese Seite.

Die Grundlagen sind [ADR 0009](../ADR/0009-local-bootstrap-adminlte-vendoring-and-auditarium-theme-layer.md) für die UI-/Vendor-Basis und [ADR 0012](../ADR/0012-standardized-tabulator-data-grid-and-tree-grid-integration.md) für Data-Grid- und Tree-Grid-Integration.
