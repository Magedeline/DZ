@echo off
REM ============================================================================
REM Desolo Zantas - Secure Release Build Script
REM Nintendo-style obfuscation and asset protection for Celeste mod
REM ============================================================================

setlocal EnableDelayedExpansion

echo ==========================================
echo Desolo Zantas - Secure Release Builder
echo ==========================================

REM Configuration
set SOLUTION=MaggyHelper.sln
set CONFIGURATION=Release
set OUTPUT_DIR=bin
set OBFSUCAR_CONFIG=obfuscar.xml

REM Step 1: Clean previous builds
echo [1/6] Cleaning previous builds...
if exist %OUTPUT_DIR% rmdir /S /Q %OUTPUT_DIR%
dotnet clean %SOLUTION% -c %CONFIGURATION% -v quiet

REM Step 2: Restore packages
echo [2/6] Restoring NuGet packages...
dotnet restore %SOLUTION% --verbosity quiet

REM Step 3: Build Release
echo [3/6] Building Release configuration...
dotnet build %SOLUTION% -c %CONFIGURATION% --no-restore -v minimal
if errorlevel 1 (
    echo ERROR: Build failed!
    exit /b 1
)

REM Step 4: Run Obfuscar (integrated via MSBuild targets - see csproj)
echo [4/6] Obfuscation applied via MSBuild targets...
echo         (Obfuscar renames methods/fields, encrypts strings)

REM Step 5: Strip PDB files from release
echo [5/6] Stripping PDB debug symbols...
if exist %OUTPUT_DIR%\*.pdb (
    del /Q %OUTPUT_DIR%\*.pdb
    echo         PDB files removed from release folder
) else (
    echo         No PDB files found
)

REM Step 6: Generate checksums for integrity verification
echo [6/6] Generating integrity checksums...
if exist %OUTPUT_DIR%\MaggyHelper.dll (
    for /f "skip=1 tokens=*" %%a in ('certutil -hashfile %OUTPUT_DIR%\MaggyHelper.dll SHA256') do (
        if not defined HASH (
            set HASH=%%a
            set HASH=!HASH: =!
            goto :checksum_done
        )
    )
    :checksum_done
    echo         Assembly SHA256: !HASH!
    
    REM Save to checksum file (not in git)
    echo !HASH! > checksum_sha256.txt
    echo         Saved to checksum_sha256.txt
)

echo.
echo ==========================================
echo Secure Build Complete!
echo ==========================================
echo Output: %OUTPUT_DIR%\MaggyHelper.dll
echo.
echo Security measures applied:
echo   - Obfuscated via Obfuscar (renaming + string encryption)
echo   - Debug symbols stripped (PDB removed)
echo   - Checksum: !HASH!
echo.
echo Ready for distribution: MaggyHelper.zip
echo ==========================================

endlocal
