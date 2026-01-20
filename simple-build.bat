@echo off
echo ==========================================
echo   GovMatch - Simple Build and Run
echo ==========================================
echo.

echo [1/5] Checking .NET 8 SDK...
dotnet --version
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo.

echo [2/5] Restoring packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Package restore failed
    pause
    exit /b 1
)
echo.

echo [3/5] Building solution...
dotnet build --configuration Release --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

echo [4/5] Running tests...
dotnet test --configuration Release --no-build --verbosity quiet
echo.

echo [5/5] Launching GovMatch...
echo.
dotnet run --project GovMatch.App --configuration Release --no-build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Application failed to start
    pause
    exit /b 1
)
