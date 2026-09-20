#!/usr/bin/env pwsh
# Licence scan wrapper for PowerShell.
param([string]$Root, [Parameter(ValueFromRemainingArguments=$true)][string[]]$Rest)
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $Root) { $Root = (Resolve-Path (Join-Path $here "..\..")).Path }
node (Join-Path $here "run.mjs") $Root @Rest
exit $LASTEXITCODE
