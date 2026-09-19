#!/usr/bin/env sh
# SPDX-License-Identifier: MIT

set -eu

repository_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$repository_dir"

umask 077

if [ ! -e .env ]; then
    : > .env
fi

if ! grep -q '^LOCAL_UID=' .env; then
    printf 'LOCAL_UID=%s\n' "$(id -u)" >> .env
fi

if ! grep -q '^LOCAL_GID=' .env; then
    printf 'LOCAL_GID=%s\n' "$(id -g)" >> .env
fi

printf '%s\n' 'Configured LOCAL_UID and LOCAL_GID in .env.'
