@echo off
setlocal EnableExtensions

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "REPO_ROOT=%%~fI"
set "VENV_PY=%REPO_ROOT%\.venv\Scripts\python.exe"

if not exist "%VENV_PY%" (
  echo [ERROR] Workspace virtual environment not found:
  echo         "%VENV_PY%"
  echo.
  echo Create it from repo root with:
  echo   py -3 -m venv .venv
  exit /b 1
)

if "%~1"=="" (
  echo Usage: %~nx0 ^<path-to-map.bin^> [room-prefix]
  exit /b 1
)

if "%~2"=="" (
  "%VENV_PY%" -m loenn_mcp.preview_map "%~1"
) else (
  "%VENV_PY%" -m loenn_mcp.preview_map "%~1" "%~2"
)

exit /b %errorlevel%
