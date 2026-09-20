# Soft-Delete-Restore – Betreiberweg

Soft Delete ist kein UI-, API- oder Self-Service-Feature. Ein Restore erfolgt ausschließlich gezielt durch einen Datenbankbetreiber und nur vor einem physischen Purge.

Im Stand von Work Package 4 existiert noch kein soft-löschbares fachliches Aggregate Root. Benutzer, Rollen, Berechtigungen, Authentifizierungsprovider, Zugangsdaten und Settings sind ausdrücklich nicht restorefähig, da sie nicht soft gelöscht werden. Deshalb gibt es derzeit keinen ausführbaren Restore-Befehl.

Mit jedem soft-löschbaren Aggregate Root ab Work Package 5 ergänzt dessen technische Dokumentation verbindlich beide provider-spezifischen Hilfen:

- Identifikation ausschließlich über den konkreten Root-Schlüssel und Prüfung von `deleted_at`, `deleted_by` und `deletion_reason`.
- Prüfung aller externen Referenzen und des vollständigen aggregatinternen Bestands vor dem Restore.
- Eine einzelne Transaktion, die ausschließlich die drei Löschmetadaten am Root auf `NULL` setzt; abhängige Bestandteile werden nur gemäß dessen Aggregatmodell einbezogen.
- Ein `RESTORED`-Ereignis im `system_audit_log` mit dem ausführenden Betreiber als Akteur.

Nach einem physischen Purge ist kein Restore möglich. Vor jedem direkten Datenbankeingriff sind ein getestetes Backup und die provider- sowie aggregatspezifische SQL-Hilfe des betreffenden Roots erforderlich.
