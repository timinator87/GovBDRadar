@echo off
REM GovMatch - Build and Run (Batch Launcher)
REM This batch file launches the PowerShell script properly

echo ==========================================
echo   GovMatch - Build and Run
echo ==========================================
echo.

REM Check if PowerShell is available
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell not found
    echo Please ensure PowerShell is installed
    pause
    exit /b 1
)

REM Run the PowerShell script with execution policy bypass
echo Running build script...
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0build-and-run.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build or run failed!
    pause
    exit /b 1
)

exit /b 0
