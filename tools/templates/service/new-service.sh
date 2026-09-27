#!/usr/bin/env bash
# new-service wrapper for bash (Linux, macOS, Git Bash on Windows).
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec node "$here/new-service.mjs" "$@"
