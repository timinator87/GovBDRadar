@echo off
REM GovMatch - Build and Run (Pure Batch Version)
REM No PowerShell required

echo ==========================================
echo   GovMatch - Build and Run
echo ==========================================
echo.

echo [1/5] Checking .NET 8 SDK...
dotnet --version 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found
    echo.
    echo Please install .NET 8 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)
echo   OK - .NET SDK found
echo.

echo [2/5] Restoring NuGet packages...
echo   This may take a minute...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Package restore failed
    pause
    exit /b 1
)
echo   OK - Packages restored
echo.

echo [3/5] Building solution (Release)...
echo   This may take 1-2 minutes...
dotnet build --configuration Release --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed - see errors above
    pause
    exit /b 1
)
echo   OK - Build succeeded
echo.

echo [4/5] Running tests...
dotnet test --configuration Release --no-build --verbosity quiet
if %ERRORLEVEL% EQU 0 (
    echo   OK - All tests passed
) else (
    echo   WARNING - Some tests failed ^(continuing anyway^)
)
echo.

echo [5/5] Launching GovMatch...
echo.
echo ==========================================
echo   Starting Application
echo ==========================================
echo.

dotnet run --project GovMatch.App --configuration Release --no-build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Application failed to start
    pause
    exit /b 1
)
