@echo off
REM GovMatch - Publish Standalone Executable (Pure Batch Version)
REM Creates self-contained .exe in the "publish" folder
REM No PowerShell required

echo ==========================================
echo   GovMatch - Publish Standalone EXE
echo ==========================================
echo.

echo [1/4] Checking .NET 8 SDK...
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

echo [2/4] Cleaning previous builds...
dotnet clean --configuration Release >nul 2>&1
echo   OK - Cleaned
echo.

echo [3/4] Restoring packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Package restore failed
    pause
    exit /b 1
)
echo   OK - Packages restored
echo.

echo [4/4] Publishing standalone executable...
echo   This will take 2-3 minutes...
echo   Output folder: %CD%\publish
echo.

dotnet publish GovMatch.App\GovMatch.App.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "%CD%\publish" ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:DebugType=None ^
    -p:DebugSymbols=false

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Publish failed
    pause
    exit /b 1
)

echo.
echo ==========================================
echo   SUCCESS!
echo ==========================================
echo.

REM Check if the exe was created
if exist "%CD%\publish\GovMatch.App.exe" (
    echo Executable created successfully!
    echo.
    echo Location: %CD%\publish\GovMatch.App.exe

    REM Get file size
    for %%F in ("%CD%\publish\GovMatch.App.exe") do echo Size: %%~zF bytes
    echo.
    echo To run:
    echo   cd publish
    echo   GovMatch.App.exe
    echo.
    echo Or just double-click: publish\GovMatch.App.exe
    echo.

    REM Open the publish folder in Explorer
    echo Opening publish folder...
    start "" explorer "%CD%\publish"
) else (
    echo WARNING: Could not find GovMatch.App.exe in publish folder
    echo.
    echo Expected location: %CD%\publish\GovMatch.App.exe
)

echo.
pause
