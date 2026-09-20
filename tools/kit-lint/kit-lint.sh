#!/usr/bin/env bash
# kit-lint wrapper for bash (Linux, macOS, Git Bash on Windows).
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
root="${1:-$(cd "$here/../.." && pwd)}"
exec node "$here/kit-lint.mjs" "$root" "${@:2}"
