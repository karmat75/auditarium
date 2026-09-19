#!/usr/bin/env sh
# SPDX-License-Identifier: MIT

set -eu

repository_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$repository_dir"

git config --local core.hooksPath .githooks
printf '%s\n' 'Repository-local pre-commit hooks are enabled.'
