# Import Prompt v1

Use this prompt with an external tool after replacing the placeholders. The
tool must return only one JSON object conforming to Catalog Import Format v1.

```text
Create an Auditarium Catalog Import Format v1 package from the supplied source
document. Do not invent requirements. Preserve the document hierarchy where it
helps readers understand the requirements.

Target catalog_version_id: <catalog-version-id>
Target draft_revision: <draft-revision>

Return JSON only, with exactly these package properties:
import_format_version (number, always 1), catalog_version_id (number),
draft_revision (number), elements (array).

Every element has exactly: id (non-empty package-local string), parent_id
(string or null), sort_order (non-negative number), title (string or null),
text (string or null), notes (string or null), weight (null or 1, 2, 4, 5),
questions (array). IDs are unique. parent_id refers to another element ID or is
null. Sibling sort_order values are unique. Do not create cycles. Each element
needs title or text. An element that has questions must have text.

Every question has exactly: sort_order (non-negative number), text (non-empty
string), verification_hint (string or null), evidence_hint (string or null),
notes (string or null), scope_keys (non-empty array of distinct values).
Use only these scope keys: ORGANIZATION, SITE, BUILDING, AREA, ROOM,
TECHNICAL_AREA, NETWORK, IT_SYSTEM, APPLICATION, PROCESS, SERVICE,
EXTERNAL_PROVIDER, OTHER. Formulate each question positively: YES must mean
the desired state is fulfilled. Each question checks exactly one fact.

Use weight null for the standard weight 3. Never include users, roles,
credentials, files, documents, catalog states, IDs from Auditarium other than
the supplied target catalog_version_id, or any explanation outside the JSON.
```
