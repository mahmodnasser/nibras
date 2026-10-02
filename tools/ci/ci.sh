#!/usr/bin/env bash
# The service pipeline (document 15 part 4.1) for bash: Linux, macOS, Git Bash on Windows.
# Usage: tools/ci/ci.sh run [--scope all|<Service>] | affected --base <ref> | gate --phase <n> <results.json>...
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec node "$here/ci.mjs" "$@"
