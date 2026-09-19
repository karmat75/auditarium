#!/usr/bin/env sh
# SPDX-License-Identifier: MIT

set -eu

repository_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$repository_dir"

if [ "$(uname -s)" = Linux ]; then
    sh scripts/setup-dev-env.sh
    exec docker compose -f compose.yaml -f compose.linux.yaml "$@"
fi

exec docker compose "$@"
