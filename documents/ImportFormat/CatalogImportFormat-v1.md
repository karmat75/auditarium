# Auditarium Catalog Import Format v1

Import Format v1 is a JSON package for adding catalog content to one existing
catalog version in state `DRAFT`. It does not create documents, catalog
versions, users, roles, credentials, files, or `READY` transitions.

The package is an untrusted external input. Property names are case-sensitive;
unknown and missing properties are rejected. All nullable text properties must
be either a JSON string or `null`.

```json
{
  "import_format_version": 1,
  "catalog_version_id": 17,
  "draft_revision": 4,
  "elements": [
    {
      "id": "E-001",
      "parent_id": null,
      "sort_order": 0,
      "title": "Technische Anforderungen",
      "text": null,
      "notes": null,
      "weight": null,
      "questions": []
    },
    {
      "id": "E-002",
      "parent_id": "E-001",
      "sort_order": 0,
      "title": "Zutritt",
      "text": "Zutrittsberechtigungen müssen dokumentiert sein.",
      "notes": null,
      "weight": 4,
      "questions": [
        {
          "sort_order": 0,
          "text": "Sind alle Zutrittsberechtigungen dokumentiert?",
          "verification_hint": "Berechtigtenliste einsehen.",
          "evidence_hint": "Freigabeprotokoll",
          "notes": null,
          "scope_keys": ["TECHNICAL_AREA"]
        }
      ]
    }
  ]
}
```

`id` is a non-empty package-local identifier. `parent_id` is either `null` or
the `id` of another element in the same package. Element IDs are unique;
hierarchies are acyclic. `sort_order` is a non-negative integer and unique
among siblings. An element needs `title` or `text`; an element with imported
questions requires `text`.

Questions have a non-negative `sort_order`, non-empty `text`, and a non-empty
list of distinct scope keys. Scope keys are the fixed Auditarium keys, for
example `ORGANIZATION`, `SITE`, `BUILDING`, `AREA`, `ROOM`, `TECHNICAL_AREA`,
`NETWORK`, `IT_SYSTEM`, `APPLICATION`, `PROCESS`, `SERVICE`,
`EXTERNAL_PROVIDER`, or `OTHER`.

`weight` is `null` for the effective default `3`, or one of `1`, `2`, `4`, or
`5`; it is permitted only for an element with at least one valid imported
question.

`catalog_version_id` and `draft_revision` bind the package to its target. A
package whose revision no longer matches is rejected with
`IMPORT.BASE_REVISION_MISMATCH`; it must be recreated from the current DRAFT.
Validation and preview do not change the database. Apply validates again,
requires an explicit full or root-subtree selection, and commits all selected
valid content plus the DRAFT revision increment as one transaction.
