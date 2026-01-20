@echo off
REM GovMatch - Publish Standalone (Batch Launcher)
REM This batch file launches the PowerShell publish script properly

echo ==========================================
echo   GovMatch - Publish Standalone
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
echo Running publish script...
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0publish-standalone.ps1" -OpenFolder

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Publish failed!
    pause
    exit /b 1
)

exit /b 0
