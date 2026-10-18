#!/usr/bin/env bash
# Regenerates the tenant-isolation tests (document 12 part 4.4) for bash: Linux, macOS, Git Bash on Windows.
# Usage: tools/isolation/regenerate.sh
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec node "$here/regenerate.mjs" "$@"
