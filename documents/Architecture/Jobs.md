# Jobs

Jobdefinitionen und ihre Planung sind Anwendungskonzepte. Manuelle, geplante und beim Start ausgelöste Jobs münden in denselben Ausführungs- und Koordinationspfad. Der gemeinsame Datenbankzustand `job_runtime_state` koordiniert Instanzen: Die Lease wird atomar erworben, ein laufender Job aktualisiert Heartbeat und Abschlusszustand, und eine abgelaufene Lease erlaubt die Wiederaufnahme nach einem Absturz.

Damit gelten Concurrency- und Startup-Semantik instanzübergreifend, ohne einen externen verteilten Scheduler oder Lock-Service. Jobs laufen als System Actor und bleiben auf dessen ausdrücklich zugewiesene Berechtigungen beschränkt. Retention- und Wartungsjobs nutzen diesen Laufzeitkern; die konkrete Jobregistrierung ist kein Architekturkanon. Für die Retention des Audit-Logs gilt [ADR 0007](../ADR/0007-transactional-system-audit-log-and-controlled-retention.md).

Die Laufzeitentscheidung ist in [ADR 0008](../ADR/0008-database-lease-coordination-for-distributed-jobs.md) dokumentiert.
