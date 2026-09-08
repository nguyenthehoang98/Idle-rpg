[CmdletBinding()]
param(
    [switch]$NoRestart
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$python = Join-Path $env:LOCALAPPDATA "Programs\Python\Python311\python.exe"
if (-not (Test-Path $python)) { $python = "python" }
$port = 38471

if (-not $NoRestart) {
    $listeners = @(Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)
    foreach ($listener in $listeners) {
        $process = Get-CimInstance Win32_Process -Filter "ProcessId=$($listener.OwningProcess)" -ErrorAction SilentlyContinue
        if ($process -and $process.CommandLine -match "bridge\.py serve") {
            Stop-Process -Id $listener.OwningProcess -Force
            Write-Host "Stopped existing Figma bridge PID $($listener.OwningProcess)."
        }
    }
}

$log = Join-Path $root "bridge-runtime.log"
$errorLog = Join-Path $root "bridge-runtime-error.log"
Start-Process -FilePath $python -ArgumentList "bridge.py serve" -WorkingDirectory $root -WindowStyle Hidden -RedirectStandardOutput $log -RedirectStandardError $errorLog | Out-Null

$healthy = $false
for ($attempt = 0; $attempt -lt 20; $attempt++) {
    Start-Sleep -Milliseconds 250
    try {
        $health = Invoke-RestMethod -Uri "http://127.0.0.1:$port/health" -TimeoutSec 2
        if ($health.ok) { $healthy = $true; break }
    } catch { }
}

if (-not $healthy) {
    Write-Error "Figma bridge did not become healthy. See $errorLog"
    exit 1
}

$token = (& $python (Join-Path $root "bridge.py") token).Trim()
Write-Host "Figma bridge is ready on http://127.0.0.1:$port"
Write-Host "Token length: $($token.Length) characters"
Write-Host "Plugin URL: http://localhost:$port"
Write-Host "The token is intentionally not printed. Run: python bridge.py token"
