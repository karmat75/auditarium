# Local development

Diese Seite beschreibt den lokalen Entwicklungsbetrieb. Die ausführliche
Konfigurationsreferenz steht in [Configuration.md](Configuration.md).

## Docker Compose: stabiler Projektkontext

`compose.yaml` setzt den festen Compose-Projektnamen `auditarium`. Dadurch
adressieren Windows und Linux dieselben logisch benannten Development-Ressourcen,
solange der Projektname nicht mit `-p` oder `COMPOSE_PROJECT_NAME` überschrieben
wird.

Zum Projekt gehören insbesondere:

- PostgreSQL-Volume: `auditarium_postgres-data`
- Data-Protection-Volume: `auditarium_data-protection-keys`
- Storage-Volume: `auditarium_storage-data`
- Standardnetz: `auditarium_default`

Container-Namen können je nach Compose-Version leicht unterschiedlich gerendert
werden. Für Befehle deshalb immer die Service-Namen `postgres`, `auditarium`
und `web` verwenden.

### Windows

Start:

```powershell
docker compose up --build -d auditarium web
```

Status:

```powershell
docker compose ps
docker volume ls --filter label=com.docker.compose.project=auditarium
```

Stop:

```powershell
docker compose down
```

Vollständiger Development-Reset:

```powershell
docker compose down --volumes --remove-orphans
```

Rebuild ohne Image-Cache:

```powershell
docker compose build --no-cache auditarium web
docker compose up -d auditarium web
```

### Linux

Unter Linux den vorhandenen Wrapper verwenden, damit die UID/GID-Abbildung für
den Workspace erhalten bleibt.

Start:

```sh
sh scripts/compose.sh up --build -d auditarium web
```

Status:

```sh
sh scripts/compose.sh ps
docker volume ls --filter label=com.docker.compose.project=auditarium
```

Stop:

```sh
sh scripts/compose.sh down
```

Vollständiger Development-Reset:

```sh
sh scripts/compose.sh down --volumes --remove-orphans
```

Rebuild ohne Image-Cache:

```sh
sh scripts/compose.sh build --no-cache auditarium web
sh scripts/compose.sh up -d auditarium web
```

Der Reset entfernt ausschließlich Ressourcen des Compose-Projekts
`auditarium`, einschließlich Development-Datenbank, Data-Protection-Keys und
Auditarium-Storage. Er löscht keine externen Datenbanken und verwendet bewusst
kein globales `docker system prune`.

Falls vor Einführung des festen Projektnamens bereits Compose-Ressourcen unter
einem anderen Projektnamen erzeugt wurden, werden diese nicht automatisch
übernommen oder gelöscht. Solche Altressourcen zuerst anhand ihrer
`com.docker.compose.project`-Labels identifizieren und nur gezielt entfernen.
