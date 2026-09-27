#!/usr/bin/env pwsh
# new-service wrapper for PowerShell (Windows, and pwsh elsewhere).
param([Parameter(ValueFromRemainingArguments=$true)][string[]]$Rest)
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
node (Join-Path $here "new-service.mjs") @Rest
exit $LASTEXITCODE
