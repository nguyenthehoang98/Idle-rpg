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
$batchPath = (Resolve-Path $BatchFile).Path
$commands = Get-Content $batchPath -Raw | ConvertFrom-Json
if ($commands -isnot [array]) { $commands = @($commands) }
if ($commands.Count -lt 1 -or $commands.Count -gt 50) { throw "Batch must contain 1-50 command objects." }

$token = (& $python (Join-Path $root "bridge.py") token).Trim()
$headers = @{ "X-Bridge-Token" = $token }
$payload = @{ op = "batch"; args = @{ commands = $commands } } | ConvertTo-Json -Depth 20 -Compress
$accepted = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:38471/command" -Headers $headers -ContentType "application/json" -Body $payload
$id = $accepted.id
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)

while ((Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 500
    $events = Invoke-RestMethod -Uri "http://127.0.0.1:38471/events" -Headers $headers
    $event = @($events.events) | Where-Object { $_.id -eq $id } | Select-Object -First 1
    if ($null -ne $event) {
        $event.result | ConvertTo-Json -Depth 30
        if (-not $event.result.ok) { exit 1 }
        exit 0
    }
}

Write-Error "Timed out waiting for Figma batch $id. Keep the plugin open and connected."
exit 1
