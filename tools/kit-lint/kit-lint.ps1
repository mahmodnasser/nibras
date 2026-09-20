#!/usr/bin/env pwsh
# kit-lint wrapper for PowerShell (Windows, and pwsh elsewhere).
param([string]$Root, [Parameter(ValueFromRemainingArguments=$true)][string[]]$Rest)
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $Root) { $Root = (Resolve-Path (Join-Path $here "..\..")).Path }
node (Join-Path $here "kit-lint.mjs") $Root @Rest
exit $LASTEXITCODE
