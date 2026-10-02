#!/usr/bin/env pwsh
# The service pipeline (document 15 part 4.1) for PowerShell: Windows, and pwsh elsewhere.
# Usage: tools/ci/ci.ps1 run [--scope all|<Service>] | affected --base <ref> | gate --phase <n> <results.json>...
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
node (Join-Path $here "ci.mjs") @args
exit $LASTEXITCODE
