#!/usr/bin/env pwsh
# capacity-28 wrapper for PowerShell (Windows, and pwsh elsewhere): TC-PERF-800.
param([Parameter(ValueFromRemainingArguments=$true)][string[]]$Rest)
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
node (Join-Path $here "capacity-28.mjs") @Rest
exit $LASTEXITCODE
