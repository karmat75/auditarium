# Soft-Delete-Restore – Betreiberweg

Soft Delete ist kein UI-, API- oder Self-Service-Feature. Ein Restore erfolgt ausschließlich gezielt durch einen Datenbankbetreiber und nur vor einem physischen Purge.

Restorefähige Aggregate Roots sind:

| Aggregate Root | Tabelle | Schlüssel |
|---|---|---|
| Dokument einschließlich seiner Katalogversionen und Kataloginhalte | documents | document_id |
| Audit Unit | audit_units | audit_unit_id |
| Audit einschließlich materialisierter Elemente und Fragen | audits | audit_id |

Vor jedem Eingriff müssen ein getestetes Backup, die konkrete Root-ID und die ID des ausführenden internen Betreibers vorliegen. Der Betreiber muss als users.user_id existieren. Nach einem physischen Purge ist kein Restore möglich.

## Vorprüfung

Für den betroffenen Root werden zuerst deleted_at, deleted_by und deletion_reason gelesen. Ein Restore ist nur erforderlich, wenn deleted_at und deleted_by beide gesetzt sind.

Zusätzlich gilt:

- documents: Katalogversionen und Kataloginhalte sind noch vorhanden; ein Restore verändert sie nicht.
- audit_units: Ein vorhandener Parent darf nicht selbst soft gelöscht sein.
- audits: Die referenzierte Audit Unit und das Dokument der Katalogversion dürfen nicht soft gelöscht sein.

PostgreSQL:

~~~sql
SELECT document_id, deleted_at, deleted_by, deletion_reason
FROM auditarium.documents WHERE document_id = :root_id;

SELECT audit_unit_id, parent_audit_unit_id, deleted_at, deleted_by, deletion_reason
FROM auditarium.audit_units WHERE audit_unit_id = :root_id;

SELECT a.audit_id, a.deleted_at, a.deleted_by, a.deletion_reason,
       u.deleted_at AS audit_unit_deleted_at,
       d.deleted_at AS document_deleted_at
FROM auditarium.audits a
JOIN auditarium.audit_units u ON u.audit_unit_id = a.audit_unit_id
JOIN auditarium.catalog_versions cv ON cv.catalog_version_id = a.catalog_version_id
JOIN auditarium.documents d ON d.document_id = cv.document_id
WHERE a.audit_id = :root_id;
~~~

SQL Server:

~~~sql
SELECT document_id, deleted_at, deleted_by, deletion_reason
FROM auditarium.documents WHERE document_id = @root_id;

SELECT audit_unit_id, parent_audit_unit_id, deleted_at, deleted_by, deletion_reason
FROM auditarium.audit_units WHERE audit_unit_id = @root_id;

SELECT a.audit_id, a.deleted_at, a.deleted_by, a.deletion_reason,
       u.deleted_at AS audit_unit_deleted_at,
       d.deleted_at AS document_deleted_at
FROM auditarium.audits a
JOIN auditarium.audit_units u ON u.audit_unit_id = a.audit_unit_id
JOIN auditarium.catalog_versions cv ON cv.catalog_version_id = a.catalog_version_id
JOIN auditarium.documents d ON d.document_id = cv.document_id
WHERE a.audit_id = @root_id;
~~~

## PostgreSQL-Restore

Das folgende Muster wird mit genau einer der drei freigegebenen Tabellen und ihrem Schlüssel ausgeführt. Tabellen- und Schlüsselnamen dürfen nicht aus unvalidierter Eingabe übernommen werden.

~~~sql
BEGIN;

WITH restored AS (
    UPDATE auditarium.documents
       SET deleted_at = NULL, deleted_by = NULL, deletion_reason = NULL
     WHERE document_id = :root_id
       AND deleted_at IS NOT NULL
       AND deleted_by IS NOT NULL
    RETURNING document_id
)
INSERT INTO auditarium.system_audit_log
    (occurred_at, user_id, action, object_type, object_id, before_state, after_state)
SELECT CURRENT_TIMESTAMP, :operator_user_id, 'RESTORED', 'Document', document_id,
       '{"deleted_at":"set","deleted_by":"set","deletion_reason":"redacted"}',
       '{"deleted_at":null,"deleted_by":null,"deletion_reason":null}'
FROM restored;

COMMIT;
~~~

Für Audit Units werden im selben Muster documents/document_id/Document durch audit_units/audit_unit_id/AuditUnit ersetzt. Vorher muss die Parent-Prüfung erfolgreich sein. Für Audits werden sie durch audits/audit_id/Audit ersetzt; vorher müssen Audit Unit und Dokument aktiv, also nicht soft gelöscht, sein.

## SQL-Server-Restore

~~~sql
BEGIN TRANSACTION;

DECLARE @restored TABLE (object_id bigint NOT NULL);

UPDATE auditarium.documents
   SET deleted_at = NULL, deleted_by = NULL, deletion_reason = NULL
OUTPUT inserted.document_id INTO @restored(object_id)
 WHERE document_id = @root_id
   AND deleted_at IS NOT NULL
   AND deleted_by IS NOT NULL;

INSERT INTO auditarium.system_audit_log
    (occurred_at, user_id, action, object_type, object_id, before_state, after_state)
SELECT SYSDATETIMEOFFSET(), @operator_user_id, 'RESTORED', 'Document', object_id,
       '{"deleted_at":"set","deleted_by":"set","deletion_reason":"redacted"}',
       '{"deleted_at":null,"deleted_by":null,"deletion_reason":null}'
FROM @restored;

COMMIT TRANSACTION;
~~~

Für Audit Units werden documents/document_id/Document durch audit_units/audit_unit_id/AuditUnit ersetzt. Für Audits werden sie durch audits/audit_id/Audit ersetzt. Die jeweiligen Vorprüfungen bleiben verpflichtend.

Die Zustandsdaten des RESTORED-Ereignisses enthalten absichtlich keine Löschbegründung und keine personenbezogenen Werte. Der betroffene Root wird über object_type und object_id, der ausführende Betreiber über user_id nachvollziehbar.
