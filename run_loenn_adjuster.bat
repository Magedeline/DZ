@echo off
REM Loenn Map Adjuster - Quick Launch Script
REM This script runs the automated map adjustment helper

setlocal enabledelayedexpansion

echo.
echo ================================================================
echo   LOENN MAP ADJUSTER - Automated Map Optimization
echo ================================================================
echo.

REM Define paths
set PROJECT_ROOT=E:\Celeste Desolo Zantas\Mods\CELESTE_DESOLO_ZANTAS
set SCRIPT_DIR=%~dp0
set PYTHON_SCRIPT=%SCRIPT_DIR%loenn_map_adjuster.py
set OUTPUT_DIR=%SCRIPT_DIR%

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8+ from https://www.python.org/
    echo Make sure to check "Add Python to PATH" during installation
    pause
    exit /b 1
)

REM Check if script exists
if not exist "%PYTHON_SCRIPT%" (
    echo ERROR: Script not found: %PYTHON_SCRIPT%
    pause
    exit /b 1
)

REM Check if project root exists
if not exist "%PROJECT_ROOT%" (
    echo WARNING: Default project directory not found:
    echo %PROJECT_ROOT%
    echo.
    set /p PROJECT_ROOT="Enter custom project path (or press Enter to skip): "
    if "!PROJECT_ROOT!"=="" (
        echo Aborting...
        pause
        exit /b 1
    )
)

echo Project Root: %PROJECT_ROOT%
echo Output Directory: %OUTPUT_DIR%
echo.
echo Starting map adjustments...
echo.

REM Run the Python script
python "%PYTHON_SCRIPT%" "%PROJECT_ROOT%" "%OUTPUT_DIR%"

REM Check result
if errorlevel 1 (
    echo.
    echo ERROR: Script execution failed with exit code %errorlevel%
    pause
    exit /b %errorlevel%
)

echo.
echo ================================================================
echo   ADJUSTMENTS COMPLETE!
echo   Check the output report for details.
echo ================================================================
echo.
pause
