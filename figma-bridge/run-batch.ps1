[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$BatchFile,
    [int]$TimeoutSeconds = 120
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$python = Join-Path $env:LOCALAPPDATA "Programs\Python\Python311\python.exe"
if (-not (Test-Path $python)) { $python = "python" }
& $python (Join-Path $root "run-batch.py") (Resolve-Path $BatchFile).Path --timeout $TimeoutSeconds
exit $LASTEXITCODE
