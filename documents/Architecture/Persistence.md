# Persistenz und Startlaufzeit

`AuditariumDbContext` ist das gemeinsame EF-Core-Modell. PostgreSQL und Microsoft SQL Server sind die unterstützten Provider; die Providerwahl erfolgt über Konfiguration. Provider-spezifische Migrationsassemblies kapseln providerabhängige Migrationstechnik, während das Anwendungsmodell providerneutral bleibt. Ein Programmstand verweigert den Betrieb gegen ein Schema, dessen angewendete Migrationen neuer sind als der ausführbare Stand.

Beim Start werden Migration, Bootstrap und Reconcile datenbankkoordiniert ausgeführt. PostgreSQL verwendet Advisory Locks, SQL Server `sp_getapplock`. Die Wartezeit ist durch `BootstrapTimeoutSeconds` begrenzt. Nach dem jeweiligen Lock-Erwerb wird der Zustand erneut bewertet; Bootstrap und Reconcile sind idempotent. Bei Timeout oder fehlgeschlagener Initialisierung wird die Instanz nicht für den Normalbetrieb freigegeben.

Der Data-Protection-Keyring muss persistent sein und bei mehreren Instanzen gemeinsam nutzbar bleiben. Die konkrete Konfiguration und die Operatorabfolge stehen im [Bootstrap- und Recovery-Runbook](../Operations/BootstrapAndRecovery.md).

Das System-Audit-Log wird zusammen mit erforderlichen Datenänderungen transaktional gespeichert; seine kontrollierte Retention folgt [ADR 0007](../ADR/0007-transactional-system-audit-log-and-controlled-retention.md). Soft Delete und eine mögliche Betreiber-Wiederherstellung sind im [Soft-Delete-Restore-Runbook](../Operations/SoftDeleteRestore.md) beschrieben.

Grundlagen: [ADR 0002](../ADR/0002-database-providers-and-migration-strategy.md) und [ADR 0003](../ADR/0003-database-coordinated-startup-bootstrap-and-reconcile.md).
