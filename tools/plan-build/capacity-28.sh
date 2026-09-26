#!/usr/bin/env bash
# capacity-28 wrapper for bash (Linux, macOS, Git Bash on Windows): TC-PERF-800.
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec node "$here/capacity-28.mjs" "$@"
