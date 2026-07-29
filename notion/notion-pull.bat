@echo off
setlocal

REM This file is in: C:\Users\Hoang PC\Documents\Idle-rpg\notion
cd /d "%~dp0\.."

if not exist "notion\tools\notion-sync\.env" (
  echo [ERROR] Missing notion\tools\notion-sync\.env
  echo Run notion\notion-push.bat first to create .env.
  pause
  exit /b 1
)

where node >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Node.js is not installed or not in PATH.
  pause
  exit /b 1
)

echo Checking Notion connection...
node notion/tools/notion-sync/sync.js check
if errorlevel 1 (
  echo [ERROR] Notion check failed.
  pause
  exit /b 1
)

echo.
echo Pulling Notion child pages to ai/DesignDocs/_notion_pull...
node notion/tools/notion-sync/sync.js pull
if errorlevel 1 (
  echo [ERROR] Pull failed.
  pause
  exit /b 1
)

echo.
echo [DONE] Pull completed.
pause
