# Local development

Auditarium hat drei gepflegte lokale Entwicklungswege. Die Details der
Konfiguration und Connection Strings stehen in
[Configuration.md](Configuration.md).

## Befehle für den Compose-Stack

Die Helper sind absichtlich dünn. Sie rufen nur die dokumentierten
`docker compose`-Operationen für den festen Projektkontext `auditarium` auf.

| Aktion | Windows | Linux |
| --- | --- | --- |
| Start / Build | `.\scripts\dev.ps1 up` | `sh scripts/dev.sh up` |
| Status | `.\scripts\dev.ps1 status` | `sh scripts/dev.sh status` |
| DemoData | `.\scripts\dev.ps1 demo` | `sh scripts/dev.sh demo` |
| Stop | `.\scripts\dev.ps1 down` | `sh scripts/dev.sh down` |
| Reset | `.\scripts\dev.ps1 reset` | `sh scripts/dev.sh reset` |
| Rebuild ohne Image-Cache | `.\scripts\dev.ps1 rebuild` | `sh scripts/dev.sh rebuild` |

`reset` ist bewusst destruktiv und entfernt ausschließlich die
Compose-Ressourcen von Auditarium: PostgreSQL-Daten, Data-Protection-Keys und
Auditarium-Storage. Externe Datenbanken oder fremde Docker-Ressourcen werden
nicht gelöscht.

`rebuild` baut die App-Images ohne Cache neu, behält aber Datenbank und
Volumes.

Der Compose-Projektname ist fest `auditarium`. Die wichtigsten Ressourcen sind
damit stabil benannt:

- `auditarium_postgres-data`
- `auditarium_data-protection-keys`
- `auditarium_storage-data`
- `auditarium_default`

## Windows + VS Code + Docker Compose + PostgreSQL

Voraussetzungen: Docker Desktop, Git und VS Code.

1. Repository in VS Code öffnen.
2. Stack starten:

   ```powershell
   .\scripts\dev.ps1 up
   ```

3. Web öffnen: <http://localhost:8081>
4. API öffnen: <http://localhost:8080>

Beim ersten Start liegt das temporäre Administrator-Credential im API-Log:

```powershell
docker compose logs auditarium
```

Demo-Daten in die frische Compose-Datenbank laden:

```powershell
.\scripts\dev.ps1 demo
```

Komplett neu anfangen:

```powershell
.\scripts\dev.ps1 reset
.\scripts\dev.ps1 up
```

## Linux + VS Code + Docker Compose + PostgreSQL

Voraussetzungen: Docker Engine mit Compose Plugin, Git und VS Code. Der
Linux-Helper verwendet intern `scripts/compose.sh` und behält damit die
UID/GID-Abbildung für den Workspace bei.

Start:

```sh
sh scripts/dev.sh up
```

Demo-Daten:

```sh
sh scripts/dev.sh demo
```

Komplett neu anfangen:

```sh
sh scripts/dev.sh reset
sh scripts/dev.sh up
```

Web und API liegen ebenfalls auf <http://localhost:8081> und
<http://localhost:8080>.

## Windows + Visual Studio + native .NET + SQL Server LocalDB

Voraussetzungen: Visual Studio mit .NET 10 SDK und SQL Server LocalDB.

Einmalig die vollständigen Development-Beispiele kopieren:

```powershell
Copy-Item UI/Auditarium.Web/appsettings.Development.example.json UI/Auditarium.Web/appsettings.Development.json
Copy-Item UI/Auditarium.Api/appsettings.Development.example.json UI/Auditarium.Api/appsettings.Development.json
```

Die Beispiele sind bereits auf `SqlServer` und
`(localdb)\MSSQLLocalDB` eingestellt. Web und API teilen sich die
repository-lokalen Verzeichnisse `.auditarium/storage` und
`.auditarium/keys`.

Dann `Auditarium.sln` in Visual Studio öffnen und Web/API mit den vorhandenen
Development-Profilen starten. Beim ersten Bootstrap wird das temporäre
Administrator-Credential in der Ausgabe des Hosts angezeigt, der die Datenbank
initialisiert.

Demo-Daten gegen die konfigurierte LocalDB laden:

```powershell
.\scripts\dev.ps1 demo-native
```

Für einen vollständigen LocalDB-Neustart zuerst Web/API stoppen und dann:

```powershell
.\scripts\dev.ps1 reset-localdb
```

Der Befehl löscht ausschließlich die Datenbank `Auditarium` aus
`(localdb)\MSSQLLocalDB` sowie `.auditarium/`. Er benötigt `sqlcmd`.
Falls `sqlcmd` nicht installiert ist, die Datenbank `Auditarium` im Visual
Studio **SQL Server Object Explorer** löschen und danach `.auditarium/`
entfernen.

Beim nächsten Start werden Migrationen und Bootstrap wieder normal ausgeführt.

## Externes PostgreSQL

Für eine externe Development-Datenbank Web/API nativ starten und in den lokalen
`appsettings.Development.json`-Dateien oder per Environment-Variablen
`Provider=PostgreSQL` und den externen Connection String setzen. Beispiele
inklusive TLS stehen in [Configuration.md](Configuration.md).

DemoData kann anschließend mit dem nativen Helper ausgeführt werden:

```powershell
.\scripts\dev.ps1 demo-native
```

```sh
sh scripts/dev.sh demo-native
```

DemoData akzeptiert nur eine technisch gebootstrappte Development-Datenbank
ohne unbekannte Business-Daten. Die Helper löschen oder resetten externe
Datenbanken niemals.

## VS Code DemoData-Profil

Das Profil **Auditarium Demo Data** startet das Tool mit `apply --confirm`,
setzt `DOTNET_ENVIRONMENT=Development` und aktiviert DemoData für diesen
Prozess. Es verwendet die Datenbankkonfiguration aus
`UI/Auditarium.Web/appsettings.Development.json` und ist damit für native
Development-Konfigurationen gedacht.

Für den Compose-PostgreSQL-Stack stattdessen immer `dev.ps1 demo` bzw.
`dev.sh demo` verwenden; diese Befehle führen DemoData innerhalb des
Compose-Netzes gegen den Service `postgres` aus.

## Falls Docker noch alte Ressourcen zeigt

Ressourcen aus der Zeit vor dem festen Compose-Projektnamen werden nicht
automatisch gelöscht. Die aktuell verwendeten Volumes zeigt:

```text
docker volume ls --filter label=com.docker.compose.project=auditarium
```

Andere alte Compose-Projekte zuerst anhand ihres
`com.docker.compose.project`-Labels eindeutig identifizieren und dann gezielt
entfernen. Kein globales `docker system prune` für einen Auditarium-Reset
verwenden.
