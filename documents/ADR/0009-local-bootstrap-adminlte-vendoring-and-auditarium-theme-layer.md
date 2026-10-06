# ADR 0009 – Local Bootstrap/AdminLTE Vendoring & Auditarium Theme Layer

## Status

Accepted

## Context

Auditarium needs a stable, self-contained UI foundation without making production availability depend on third-party frontend CDNs or a JavaScript build toolchain.

## Decision

Bootstrap 5.3+ and AdminLTE 4 are Auditarium's technical UI foundation. Adminator and Patchmon may be visual references only, not technical dependencies or pixel-perfect targets. Official vendor distribution assets are stored locally, unmodified, and served without a production CDN dependency. Auditarium-specific customization is kept in a separate local theme layer. Vendor version, source, license, and integrity information is tracked in `documents/VendorAssets.md`. The initial architecture requires no Node/npm/Vite/Sass build pipeline; materially changing that choice requires a future explicit decision.

## Consequences

Production UI assets are reviewable and deployable with the application, and vendor upgrades can be verified independently from Auditarium styling. The project accepts manual local asset management until a later architectural decision changes it.

## References

- [vendor asset manifest](../VendorAssets.md), [local vendor tree](../../UI/Auditarium.Web/wwwroot/vendor/), and [Auditarium CSS/theme scripts](../../UI/Auditarium.Web/wwwroot/css/site.css)
- [shared Web layout](../../UI/Auditarium.Web/Pages/Shared/_Layout.cshtml) and [presentation foundation tests](../../Tests/Auditarium.Infrastructure.Security.Tests/PresentationFoundationTests.cs)
- [historical UI provenance](../Auditarium_Soll_Pflichtenheft.md)
