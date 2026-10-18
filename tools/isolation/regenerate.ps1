#!/usr/bin/env pwsh
# Regenerates the tenant-isolation tests (document 12 part 4.4) for PowerShell: Windows, and pwsh elsewhere.
# Usage: tools/isolation/regenerate.ps1
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
node (Join-Path $here "regenerate.mjs") @args
exit $LASTEXITCODE
