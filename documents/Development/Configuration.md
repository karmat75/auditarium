# Development configuration

Diese Seite ist die kurze Referenz für die lokale Auditarium-Konfiguration. Die
versionierten `appsettings.Development.example.json`-Dateien in Web und API sind
vollständige, lauffähige Entwicklungsbeispiele ohne echte Secrets.

## Welche Datei gilt?

ASP.NET Core lädt die Konfiguration in dieser Reihenfolge; spätere Quellen
überschreiben frühere:

1. `appsettings.json`
2. `appsettings.Development.json`
3. Environment-Variablen

Für lokale native Entwicklung die jeweilige Vorlage kopieren:

```powershell
Copy-Item UI/Auditarium.Web/appsettings.Development.example.json UI/Auditarium.Web/appsettings.Development.json
Copy-Item UI/Auditarium.Api/appsettings.Development.example.json UI/Auditarium.Api/appsettings.Development.json
```

```sh
cp UI/Auditarium.Web/appsettings.Development.example.json UI/Auditarium.Web/appsettings.Development.json
cp UI/Auditarium.Api/appsettings.Development.example.json UI/Auditarium.Api/appsettings.Development.json
```

Die lokalen Dateien sowie `.auditarium/` sind absichtlich nicht versioniert.
Die Beispiele verwenden `../../.auditarium/storage` und
`../../.auditarium/keys`, sodass Web und API dieselben lokalen Daten- und
Data-Protection-Verzeichnisse nutzen.

## Host- und Startup-Einstellungen

| Einstellung | Bedeutung | Default / Vorgabe |
| --- | --- | --- |
| `Auditarium:Database:Provider` | EF-Core-Datenbankprovider | Pflicht: `PostgreSQL` oder `SqlServer` |
| `Auditarium:Database:ConnectionString` | Verbindung zur Auditarium-Datenbank | Pflicht |
| `Auditarium:Database:BootstrapTimeoutSeconds` | Timeout für Migration/Bootstrap-Locks | `180`, gültig 1–1800 |
| `Auditarium:Storage:RootPath` | Lokale Ablage importierter Originaldateien | Pflicht |
| `Auditarium:DataProtection:KeyRingPath` | Persistenter ASP.NET-Data-Protection-Keyring | Pflicht |
| `Auditarium:DataProtection:ApplicationName` | Gemeinsamer Data-Protection-Kontext | Pflicht; lokal `Auditarium` |
| `Auditarium:Retention:DeletionDays` | Mindestalter vor physischem Purge gelöschter Fachdaten | `90`, gültig 1–36500 |
| `Auditarium:Retention:DeletionPurgeEnabled` | Physisches Purging gelöschter Fachdaten | `true` |
| `Auditarium:Retention:AuditLogDays` | Mindestalter für Audit-Log-Purge | `365`, gültig 1–36500 |
| `Auditarium:Retention:AuditLogPurgeEnabled` | Audit-Log-Purge aktivieren | `false` |
| `Auditarium:Recovery:Enabled` | eingeschränkten Recovery-Modus aktivieren | `false` |
| `Auditarium:Recovery:DefaultAdminPassword` | temporäres Recovery-Passwort | nur bei aktiviertem Recovery erforderlich; Secret |
| `Auditarium:DemoData:Enabled` | DemoData-Tool freischalten | `false`; nur Development, vom Tool aus der Web-Konfiguration gelesen |
| `OpenTelemetry:Enabled` | OTLP-Export aktivieren | `false` |
| `OpenTelemetry:ServiceName` | OpenTelemetry Service Name | `Auditarium` |
| `OpenTelemetry:Otlp:Endpoint` | OTLP-Endpunkt | optional; z. B. `http://localhost:4317` |

Recovery wird nur nach dem
[Bootstrap- und Recovery-Runbook](../Operations/BootstrapAndRecovery.md)
verwendet. Ein Recovery-Passwort gehört nie in eine versionierte Datei.

## Datenbankbeispiele

Der Providername und der Connection String müssen zusammenpassen.

### SQL Server LocalDB

Geeignet für Visual Studio unter Windows:

```text
Provider: SqlServer
Server=(localdb)\MSSQLLocalDB;Database=Auditarium;Trusted_Connection=True;TrustServerCertificate=True
```

In JSON muss der Backslash escaped werden:

```json
"ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=Auditarium;Trusted_Connection=True;TrustServerCertificate=True"
```

### Lokaler SQL Server

Windows-Authentifizierung:

```text
Provider: SqlServer
Server=localhost;Database=Auditarium;Trusted_Connection=True;TrustServerCertificate=True
```

SQL-Authentifizierung:

```text
Provider: SqlServer
Server=localhost;Database=Auditarium;User Id=auditarium;Password=<secret>;TrustServerCertificate=True
```

`TrustServerCertificate=True` ist für lokale Entwicklung gedacht, nicht als
allgemeine Empfehlung für externe produktionsnahe Verbindungen.

### Lokales PostgreSQL

```text
Provider: PostgreSQL
Host=localhost;Port=5432;Database=auditarium;Username=auditarium;Password=<secret>
```

### Externes PostgreSQL

```text
Provider: PostgreSQL
Host=db.example.net;Port=5432;Database=auditarium;Username=auditarium;Password=<secret>;SSL Mode=VerifyFull;Root Certificate=/path/to/root-ca.crt
```

Für externe Datenbanken bleiben Datenbank, Benutzer, TLS-Vertrauen und
Erreichbarkeit Aufgabe der jeweiligen Entwicklungsumgebung. Auditarium führt
beim Start seine normalen Migrationen und den technischen Bootstrap aus.

## Environment-Variablen

Für Schlüssel unter `Auditarium` wird `:` durch `__` ersetzt:

```text
Auditarium:Database:Provider
AUDITARIUM__Database__Provider
```

Beispiele:

```text
AUDITARIUM__Database__Provider=PostgreSQL
AUDITARIUM__Database__ConnectionString=Host=localhost;Database=auditarium;Username=auditarium;Password=...
AUDITARIUM__DemoData__Enabled=true
```

Top-Level-Konfiguration folgt demselben ASP.NET-Core-Schema, z. B.
`OpenTelemetry__Enabled=true`.

## Laufzeit-Einstellungen

Einige fachnahe Einstellungen können über die Administration in der Datenbank
gepflegt werden. Für diese gilt effektiv:

```text
Environment > Datenbank > appsettings > eingebauter Default
```

Environment-Variablen sind damit der bewusste harte Override. Die
Entwicklungsbeispiele zeigen die eingebauten Defaults zur Orientierung.

| Einstellung | Default | Gültig |
| --- | ---: | --- |
| `Auditarium:Security:LocalPassword:MinimumLength` | 15 | 12–128 |
| `Auditarium:Security:LocalPassword:MaximumLength` | 128 | 12–1024 |
| `Auditarium:Security:LocalLockout:FailedAttempts` | 5 | 1–20 |
| `Auditarium:Security:LocalLockout:WindowMinutes` | 15 | 1–1440 |
| `Auditarium:Security:LocalLockout:DurationMinutes` | 15 | 1–1440 |
| `Auditarium:Security:ApiCredentials:DefaultLifetimeDays` | 180 | 1–3650 |
| `Auditarium:Security:ApiCredentials:MaximumActiveCredentials` | 5 | 1–50 |
| `Auditarium:Files:OriginalDocuments:MaxUploadSize` | 104857600 | 1 MiB–1 GiB; Restart erforderlich |
| `Auditarium:Files:OriginalDocuments:MaxDownloadSize` | 104857600 | 1 MiB–1 GiB; Restart erforderlich |
| `Auditarium:Jobs:Retention:Enabled` | `true` | Boolean |
| `Auditarium:Jobs:Retention:Schedule` | `0 3 * * *` | Standard-Cron oder leer |
| `Auditarium:Jobs:Retention:TimeZone` | `Europe/Berlin` | gültige System-Zeitzone |
| `Auditarium:Jobs:Retention:RunOnStartup` | `false` | Boolean |
| `Auditarium:Jobs:Retention:MisfirePolicy` | `Skip` | `Skip` |
| `Auditarium:Jobs:Retention:ConcurrencyPolicy` | `SkipIfRunning` | `SkipIfRunning` |

LDAP-Provider besitzen instanzabhängige Einstellungen und werden über die
Auditarium-Administration gepflegt. Sie gehören deshalb bewusst nicht in die
allgemeinen Development-Beispieldateien. Ein gezielter Environment-Override
verwendet das Muster
`AUDITARIUM__Authentication__Providers__<ProviderKey>__<Property>`.
