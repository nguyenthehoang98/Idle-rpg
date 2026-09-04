[CmdletBinding()]
param(
    [ValidateSet('compile', 'editmode', 'playmode', 'all')]
    [string]$Mode = 'all',
    [string]$ProjectPath = '',
    [string]$UnityPath = '',
    [string]$ResultsDirectory = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}
else {
    $ProjectPath = (Resolve-Path $ProjectPath).Path
}

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $UnityPath = $env:UNITY_PATH
}

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $unityCandidates = @(
        Get-ChildItem 'C:\Program Files\Unity\Hub\Editor' -Filter Unity.exe -Recurse -ErrorAction SilentlyContinue
    ) | Sort-Object FullName -Descending

    if ($unityCandidates.Count -gt 0) {
        $UnityPath = $unityCandidates[0].FullName
    }
}

if ([string]::IsNullOrWhiteSpace($UnityPath) -or -not (Test-Path $UnityPath)) {
    throw 'Unity editor not found. Set UNITY_PATH or pass -UnityPath.'
}

if ([string]::IsNullOrWhiteSpace($ResultsDirectory)) {
    $ResultsDirectory = Join-Path $ProjectPath 'Temp\Verification'
}

New-Item -ItemType Directory -Force -Path $ResultsDirectory | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'

function Invoke-UnityStep {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string[]]$Arguments
    )

    $logPath = Join-Path $ResultsDirectory "$stamp-$Name.log"
    Write-Host "==> Unity $Name"
    Write-Host "    $UnityPath $($Arguments -join ' ')"

    $stdoutPath = "$logPath.stdout"
    $stderrPath = "$logPath.stderr"
    $argumentString = ($Arguments | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + $_.Replace('"', '\\"') + '"'
        }
        else {
            $_
        }
    }) -join ' '

    $process = Start-Process -FilePath $UnityPath `
        -ArgumentList $argumentString `
        -Wait `
        -PassThru `
        -NoNewWindow `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath
    $exitCode = $process.ExitCode

    if (Test-Path $stdoutPath) {
        Get-Content $stdoutPath | Set-Content $logPath
    }
    if (Test-Path $stderrPath) {
        Get-Content $stderrPath | Add-Content $logPath
    }
    Get-Content $logPath

    if ($exitCode -ne 0) {
        throw "Unity $Name failed with exit code $exitCode. Log: $logPath"
    }

    $compileErrors = Select-String -Path $logPath -Pattern 'error CS\d+|Compilation failed|Script compilation failed' -Quiet
    if ($compileErrors) {
        throw "Unity $Name reported compilation errors. Log: $logPath"
    }

    $script:LastUnityLogPath = $logPath
}

function Assert-TestResult {
    param(
        [Parameter(Mandatory = $true)][string]$TestPlatform,
        [Parameter(Mandatory = $true)][string]$LogPath
    )

    $resultPath = Join-Path $ResultsDirectory "$stamp-$TestPlatform.xml"
    $fallbackPath = Join-Path $env:USERPROFILE 'AppData\LocalLow\DefaultCompany\eoe\TestResults.xml'

    if (Test-Path $resultPath) {
        $xml = [xml](Get-Content $resultPath -Raw)
    }
    elseif (Test-Path $fallbackPath) {
        $xml = [xml](Get-Content $fallbackPath -Raw)
        Copy-Item $fallbackPath $resultPath -Force
    }
    else {
        throw "EditMode test result XML not found. Log: $LogPath"
    }

    $testRun = $xml.'test-run'
    if ($null -eq $testRun -or $testRun.result -ne 'Passed' -or [int]$testRun.failed -ne 0) {
        throw "$TestPlatform tests failed. Result: $resultPath"
    }

    Write-Host "    $TestPlatform tests passed: $($testRun.passed)"
}

if ($Mode -eq 'compile' -or $Mode -eq 'all') {
    Invoke-UnityStep -Name 'compile' -Arguments @(
        '-batchmode',
        '-nographics',
        '-quit',
        '-projectPath',
        $ProjectPath,
        '-logFile',
        '-'
    )
    Write-Host '    Compile passed.'
}

if ($Mode -eq 'editmode' -or $Mode -eq 'all') {
    Invoke-UnityStep -Name 'editmode' -Arguments @(
        '-batchmode',
        '-nographics',
        '-projectPath',
        $ProjectPath,
        '-runTests',
        '-testPlatform',
        'editmode',
        '-testResults',
        (Join-Path $ResultsDirectory "$stamp-editmode.xml"),
        '-logFile',
        '-'
    )
    Assert-TestResult -TestPlatform 'editmode' -LogPath (Join-Path $ResultsDirectory "$stamp-editmode.log")
}

if ($Mode -eq 'playmode') {
    Invoke-UnityStep -Name 'playmode' -Arguments @(
        '-batchmode',
        '-nographics',
        '-projectPath',
        $ProjectPath,
        '-runTests',
        '-testPlatform',
        'playmode',
        '-testResults',
        (Join-Path $ResultsDirectory "$stamp-playmode.xml"),
        '-logFile',
        '-'
    )
    Assert-TestResult -TestPlatform 'playmode' -LogPath (Join-Path $ResultsDirectory "$stamp-playmode.log")
}

Write-Host "Verification passed. Artifacts: $ResultsDirectory"
