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

echo [1/3] Upgrading pip in workspace .venv...
"%VENV_PY%" -m pip install --upgrade pip
if errorlevel 1 exit /b %errorlevel%

echo [2/3] Installing/upgrading loenn-mcp...
"%VENV_PY%" -m pip install --upgrade loenn-mcp
if errorlevel 1 exit /b %errorlevel%

echo [3/3] Installed loenn-mcp version:
"%VENV_PY%" -m pip show loenn-mcp | findstr /B /C:"Version:"
if errorlevel 1 exit /b %errorlevel%

echo.
echo Done. If needed, set mcp.json server command to:
echo   .venv\Scripts\python.exe

exit /b 0