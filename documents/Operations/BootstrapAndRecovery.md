# Datenbank-Bootstrap und Recovery

Dieses Runbook beschreibt die Betreiberkonfiguration, den Startlauf und die Wiederherstellung des Default-Administrators.

## Konfiguration und Startlauf

Der Datenbankprovider wird extern konfiguriert. Unterstützte Werte sind `PostgreSQL` und `SqlServer`:

```text
AUDITARIUM__Database__Provider=PostgreSQL
AUDITARIUM__Database__ConnectionString=...
AUDITARIUM__Database__BootstrapTimeoutSeconds=180
AUDITARIUM__DataProtection__KeyRingPath=/persisted/auditarium-keys
AUDITARIUM__DataProtection__ApplicationName=Auditarium
```

Der Data-Protection-Keyring muss persistent sein und für mehrere Instanzen gemeinsam erreichbar bleiben. Er darf nicht in `application_settings` oder zusammen mit seinem Schutz-Secret gespeichert werden.

Jeder Start wendet Migrationen an und führt anschließend Bootstrap und Reconcile unter installationsweiter Datenbanksperre aus. Weitere Instanzen warten höchstens `BootstrapTimeoutSeconds`. PostgreSQL nutzt einen sessiongebundenen Advisory Lock, SQL Server `sp_getapplock`; beim Schließen der Datenbankverbindung werden die Sperren freigegeben. Bei Timeout, Fehler oder Prozessabbruch wird kein Normalbetrieb freigegeben.

## Default-Administrator wiederherstellen

1. Alle normalen Auditarium-Instanzen anhalten.
2. Für genau eine Recovery-Instanz `AUDITARIUM__Recovery__Enabled=true` und `AUDITARIUM__Recovery__DefaultAdminPassword=<temporäres Passwort>` setzen.
3. Nur diese Instanz starten. Sie stellt ausschließlich `/recovery` sowie die technischen Health-Endpunkte bereit; UI, API und Jobs bleiben gesperrt.
4. Die Recovery-Instanz nach Abschluss beenden und beide Recovery-Variablen entfernen, bevor normale Instanzen erneut gestartet werden.

Das Recovery-Passwort ist ein temporäres `LOCAL`-Credential und muss beim nächsten normalen Login geändert werden. Es darf nie in Dateien, Datenbank, Logs oder Telemetrie gespeichert werden.

Architekturhintergrund: [Persistenz](../Architecture/Persistence.md), [ADR 0002](../ADR/0002-database-providers-and-migration-strategy.md) und [ADR 0003](../ADR/0003-database-coordinated-startup-bootstrap-and-reconcile.md).
