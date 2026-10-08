# ADR 0012 – Standardized Tabulator Data Grid & Tree Grid Integration

## Status

Accepted

## Context

Auditarium's server-rendered Web UI contains non-trivial tabular collections and
hierarchical data that need consistent sorting, filtering, actions, responsive
behavior, accessibility, and export semantics.

Tabulator is already vendored locally as a frontend dependency, but its role has
not been defined as an architectural standard. The initial Audit Units UI
demonstrated the resulting ambiguity: a simple hierarchy and a separate flat
table represented the same objects, while paging, search, sorting, toolbar, and
footer behavior were implemented as page-specific concerns.

The reproducible Development DemoData fixture makes these issues visible across
realistic application states and motivates a durable integration boundary.

## Decision

Tabulator is Auditarium's standard interactive data-grid and tree-grid component
for non-trivial collections in the Razor Pages Web application.

This decision complements, and does not supersede, ADR 0001 and ADR 0009.

Bootstrap 5.3+ and AdminLTE 4 remain the technical shell and theme foundation.
Tabulator is a locally vendored, unmodified frontend component styled and
integrated through Auditarium-owned CSS and JavaScript. No additional JavaScript
application framework or frontend build pipeline is introduced.

Razor Pages remain responsible for page composition, routing, authorization
integration, forms, and server-rendered fallbacks. Tabulator is a scoped
enhancement inside a page, not a replacement for the server-rendered
presentation architecture.

The BLL must remain independent of Tabulator. BLL queries expose
presentation-agnostic application contracts such as search criteria, sort
semantics, paging where appropriate, and hierarchical results. Tabulator-specific
request parameters, JSON response shapes, and frontend state belong to the Web
layer, which translates between the grid and the BLL.

Flat collections may use server-side paging, sorting, and filtering when the
dataset warrants it. Reusable grid integration must provide a consistent
toolbar, compact search placement, responsive behavior, and footer/paging
presentation.

Hierarchical collections such as Audit Units use a tree-grid representation.
Parent/child relationships must never be split across pages, so the Audit Unit
tree is unpaged. Sorting must preserve the hierarchy and operate within sibling
sets instead of flattening the tree. Search must retain the ancestor path needed
to understand the context of matching nodes.

Scope-type icons are presentation metadata. The Web layer maps stable
scope-type keys to icons; the BLL and database do not store Bootstrap icon class
names. Human-readable scope-type text remains visible so meaning never depends
on iconography alone.

Grid toolbars may expose create, export, and print actions when the underlying
use case supports them. Export and print behavior must have explicit complete-
data semantics; a remotely paged grid must never silently export only the
currently loaded page while presenting the action as a complete export.

## Consequences

Auditarium gains one reusable interaction model for complex flat tables and
hierarchical tree grids instead of page-specific table implementations.

The existing visible combination of a separate Audit Unit hierarchy and a flat
Audit Unit table is transitional and should be replaced by one tree grid.

Shared Web-layer integration code should own generic Tabulator behavior while
page-specific formatters or actions remain small extensions. Backend changes may
be required to expose suitable generic BLL query contracts, but those contracts
must not reference Tabulator terminology or transport shapes.

The project keeps its Razor Pages architecture, structured-monolith boundaries,
local vendor strategy, AdminLTE/Bootstrap foundation, and no-Node build
decision.

## References

- [ADR 0001 – Application Architecture & Presentation Boundaries](0001-application-architecture-and-presentation-boundaries.md)
- [ADR 0009 – Local Bootstrap/AdminLTE Vendoring & Auditarium Theme Layer](0009-local-bootstrap-adminlte-vendoring-and-auditarium-theme-layer.md)
- [UI architecture](../Architecture/UI.md)
- [frontend vendor asset manifest](../VendorAssets.md)
- [Audit Unit page](../../UI/Auditarium.Web/Pages/AuditUnits/Index.cshtml)
- [Tabulator integration](../../UI/Auditarium.Web/wwwroot/js/tables.js)
- [Issue #66](https://github.com/karmat75/auditarium/issues/66)
