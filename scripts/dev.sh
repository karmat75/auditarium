#!/usr/bin/env sh
# SPDX-License-Identifier: MIT

set -eu

repository_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$repository_dir"

command=${1:-help}

compose() {
    sh scripts/compose.sh "$@"
}

require_compose_stack() {
    if ! compose ps --status running --services | grep -qx 'auditarium'; then
        printf '%s\n' 'Auditarium Compose is not running. Run: sh scripts/dev.sh up' >&2
        exit 1
    fi
}

case "$command" in
    up|start)
        compose up --build -d auditarium web
        ;;
    down|stop)
        compose down
        ;;
    status)
        compose ps
        docker volume ls --filter label=com.docker.compose.project=auditarium
        printf '%s\n' 'Web: http://localhost:8081  API: http://localhost:8080'
        ;;
    reset)
        printf '%s\n' 'Resetting only the Auditarium Compose project (database, storage and Data Protection volumes).'
        compose down --volumes --remove-orphans
        ;;
    rebuild)
        compose build --no-cache auditarium web
        compose up -d auditarium web
        ;;
    demo)
        require_compose_stack
        compose run --rm --no-deps \
            -e DOTNET_ENVIRONMENT=Development \
            -e AUDITARIUM__DemoData__Enabled=true \
            -e AUDITARIUM__Database__Provider=PostgreSQL \
            -e "AUDITARIUM__Database__ConnectionString=Host=postgres;Database=auditarium;Username=auditarium;Password=auditarium-development-only" \
            workspace \
            dotnet run --project Tools/Auditarium.DemoData/Auditarium.DemoData.csproj -- apply --confirm
        ;;
    demo-native)
        DOTNET_ENVIRONMENT=Development AUDITARIUM__DemoData__Enabled=true \
            dotnet run --project Tools/Auditarium.DemoData/Auditarium.DemoData.csproj -- apply --confirm
        ;;
    help|-h|--help)
        cat <<'EOF'
Auditarium development helper

Compose commands:
  sh scripts/dev.sh up        Start/build API + Web + PostgreSQL
  sh scripts/dev.sh status    Show Auditarium containers and volumes
  sh scripts/dev.sh demo      Apply DemoData to the Compose PostgreSQL database
  sh scripts/dev.sh down      Stop the Compose project
  sh scripts/dev.sh reset     Remove only Auditarium Compose data/keys/storage
  sh scripts/dev.sh rebuild   Rebuild API + Web without image cache and start

Native command:
  sh scripts/dev.sh demo-native
      Apply DemoData to the database configured in Web appsettings.Development.json
      or matching environment variables. No native/external database is reset.
EOF
        ;;
    *)
        printf 'Unknown command: %s\n\n' "$command" >&2
        sh "$0" help >&2
        exit 2
        ;;
esac
