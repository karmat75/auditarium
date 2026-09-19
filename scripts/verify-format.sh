#!/usr/bin/env sh
# SPDX-License-Identifier: MIT

set -eu

output_file=$(mktemp)
trap 'rm -f "$output_file"' EXIT HUP INT TERM

if command -v dotnet >/dev/null 2>&1; then
    if dotnet format Auditarium.sln --verify-no-changes >"$output_file" 2>&1; then
        exit 0
    fi
elif command -v docker >/dev/null 2>&1; then
    repository_dir=$(git rev-parse --show-toplevel)
    if docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp \
        -v "$repository_dir:/workspace" -w /workspace \
        mcr.microsoft.com/dotnet/sdk:10.0 \
        dotnet format Auditarium.sln --verify-no-changes >"$output_file" 2>&1; then
        exit 0
    fi
else
    printf '%s\n' 'Install the .NET SDK or Docker to verify formatting.' >&2
    exit 1
fi

cat "$output_file" >&2
exit 1
