# GovMatch - Build and Run Script for Windows
# This script automates the build and launch process

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  GovMatch - Build and Run" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 8 SDK is installed
Write-Host "[1/5] Checking .NET 8 SDK..." -ForegroundColor Yellow
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

# Restore NuGet packages
Write-Host "[2/5] Restoring NuGet packages..." -ForegroundColor Yellow
& dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ✗ Package restore failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}
Write-Host "   ✓ Packages restored successfully" -ForegroundColor Green

Write-Host ""

# Build solution
Write-Host "[3/5] Building solution (Release)..." -ForegroundColor Yellow
& dotnet build --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ✗ Build failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}
Write-Host "   ✓ Build succeeded" -ForegroundColor Green

Write-Host ""

# Run tests
Write-Host "[4/5] Running tests..." -ForegroundColor Yellow
& dotnet test --configuration Release --no-build --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ All tests passed" -ForegroundColor Green
} else {
    Write-Host "   ⚠ Some tests failed (continuing anyway)" -ForegroundColor Yellow
}

Write-Host ""

# Launch application
Write-Host "[5/5] Launching GovMatch..." -ForegroundColor Yellow
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Starting GovMatch Application" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

& dotnet run --project GovMatch.App --configuration Release --no-build

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Application failed to start" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}
