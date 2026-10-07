# UI

Die servergerenderte Razor-Webanwendung nutzt Bootstrap 5.3+ und AdminLTE 4 als technische UI-Grundlage. Adminator und Patchmon sind ausschließlich visuelle Referenzen, keine technischen Abhängigkeiten oder pixelgenauen Ziele.

Offizielle Distributionsassets werden lokal vendort, unverändert gehalten und ohne CDN- oder Runtime-Internetabhängigkeit ausgeliefert. Die Auditarium-spezifische Theme- und Anpassungsschicht bleibt von den Vendor-Dateien getrennt. Die vorhandenen `DARK`, `LIGHT` und `SYSTEM`-Konzepte verwenden semantische Designtokens und Bootstrap-Farbmodi.

Die aktuelle Architektur benötigt keine Node-, npm-, Vite- oder Sass-Buildkette. Eine künftige Einführung wäre eine ausdrückliche Architekturänderung. Quelle für Herkunft, Version, Lizenz und Integrität der Dateien ist [VendorAssets.md](../VendorAssets.md), nicht diese Seite.

Die Grundlage ist [ADR 0009](../ADR/0009-local-bootstrap-adminlte-vendoring-and-auditarium-theme-layer.md).
