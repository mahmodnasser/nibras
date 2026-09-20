#!/usr/bin/env bash
# Developer setup check for bash (Linux, macOS, Git Bash on Windows).
set -uo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
root="${1:-$(cd "$here/../.." && pwd)}"
exec node "$here/verify-setup.mjs" "$root" "${@:2}"
