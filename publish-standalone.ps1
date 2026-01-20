# GovMatch - Publish Standalone Executable
# Creates a self-contained .exe that doesn't require .NET runtime

param(
    [string]$OutputPath = "publish",
    [switch]$OpenFolder
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  GovMatch - Publish Standalone" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 8 SDK is installed
Write-Host "[1/4] Checking .NET 8 SDK..." -ForegroundColor Yellow
$dotnetVersion = & dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ✗ ERROR: .NET SDK not found" -ForegroundColor Red
    Write-Host "   Download .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "   ✓ Found .NET SDK version: $dotnetVersion" -ForegroundColor Green

$majorVersion = [int]($dotnetVersion.ToString().Split('.')[0])
if ($majorVersion -lt 8) {
    Write-Host "   ✗ ERROR: .NET 8 or higher required. Found: $dotnetVersion" -ForegroundColor Red
    Write-Host "   Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host ""

# Clean previous builds
Write-Host "[2/4] Cleaning previous builds..." -ForegroundColor Yellow
& dotnet clean --configuration Release
Write-Host "   ✓ Clean completed" -ForegroundColor Green

Write-Host ""

# Restore packages
Write-Host "[3/4] Restoring packages..." -ForegroundColor Yellow
& dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ✗ Package restore failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}
Write-Host "   ✓ Packages restored" -ForegroundColor Green

Write-Host ""

# Publish
Write-Host "[4/4] Publishing self-contained executable..." -ForegroundColor Yellow
Write-Host "   This may take 1-2 minutes..." -ForegroundColor Gray

$publishPath = Join-Path $PSScriptRoot $OutputPath

& dotnet publish GovMatch.App/GovMatch.App.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishPath `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "   ✗ Publish failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "   ✓ Publish succeeded!" -ForegroundColor Green

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "  SUCCESS!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

$exePath = Join-Path $publishPath "GovMatch.App.exe"
if (Test-Path $exePath) {
    $exeSize = (Get-Item $exePath).Length / 1MB
    Write-Host "Executable created:" -ForegroundColor Cyan
    Write-Host "  Location: $exePath" -ForegroundColor White
    Write-Host "  Size: $($exeSize.ToString('F1')) MB" -ForegroundColor White
} else {
    Write-Host "Warning: Could not find GovMatch.App.exe in output folder" -ForegroundColor Yellow
}

Write-Host ""

Write-Host "To run the application:" -ForegroundColor Yellow
Write-Host "  cd $publishPath" -ForegroundColor White
Write-Host "  .\GovMatch.App.exe" -ForegroundColor White
Write-Host ""

Write-Host "To install permanently:" -ForegroundColor Yellow
Write-Host "  1. Copy the entire '$OutputPath' folder to C:\Program Files\GovMatch\" -ForegroundColor White
Write-Host "  2. Create a desktop shortcut to GovMatch.App.exe" -ForegroundColor White
Write-Host "  3. (Optional) Pin to Start Menu" -ForegroundColor White
Write-Host ""

# Open folder if requested
if ($OpenFolder) {
    Write-Host "Opening publish folder..." -ForegroundColor Gray
    if (Test-Path $publishPath) {
        explorer.exe $publishPath
    }
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
