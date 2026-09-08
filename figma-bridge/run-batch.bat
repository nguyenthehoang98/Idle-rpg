@echo off
setlocal
if "%~1"=="" (
  echo Usage: run-batch.bat path\to\batch.json
  exit /b 2
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-batch.ps1" -BatchFile "%~1"
set code=%ERRORLEVEL%
endlocal & exit /b %code%
