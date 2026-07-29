@echo off
setlocal

REM This file is in: C:\Users\Hoang PC\Documents\Idle-rpg\notion
cd /d "%~dp0\.."

if not exist "notion\tools\notion-sync\sync.js" (
  echo [ERROR] Missing notion\tools\notion-sync\sync.js
  pause
  exit /b 1
)

where node >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Node.js is not installed or not in PATH.
  pause
  exit /b 1
)

if not exist "notion\tools\notion-sync\.env" (
  echo [SETUP] Missing notion\tools\notion-sync\.env
  echo [SETUP] Creating it from .env.example...
  copy /Y "notion\tools\notion-sync\.env.example" "notion\tools\notion-sync\.env" >nul
  echo.
  echo [ACTION REQUIRED]
  echo Notepad will open notion\tools\notion-sync\.env
  echo Replace NOTION_TOKEN with your real Notion token, then SAVE and CLOSE Notepad.
  echo DOCS_DIR is already set to ai/DesignDocs
  echo.
  pause
  notepad "notion\tools\notion-sync\.env"
)

findstr /B /C:"NOTION_TOKEN=secret_or_ntn_" "notion\tools\notion-sync\.env" >nul 2>nul
if not errorlevel 1 (
  echo [ERROR] .env still contains the example token.
  notepad "notion\tools\notion-sync\.env"
  pause
  exit /b 1
)

echo.
echo Checking Notion connection...
node notion/tools/notion-sync/sync.js check
if errorlevel 1 (
  echo.
  echo [ERROR] Notion check failed.
  echo Make sure NOTION_TOKEN is correct and page is shared with integration.
  pause
  exit /b 1
)

echo.
echo Pushing AI design docs to Notion...
node notion/tools/notion-sync/sync.js push
if errorlevel 1 (
  echo.
  echo [ERROR] Push failed.
  pause
  exit /b 1
)

echo.
echo [DONE] Push completed.
pause
