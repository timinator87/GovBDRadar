# GovMatch - Build and Run Script for Windows
# This script automates the build and launch process

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  GovMatch - Build and Run" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 8 SDK is installed
Write-Host "[1/5] Checking .NET 8 SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✓ Found .NET SDK version: $dotnetVersion" -ForegroundColor Green

    $majorVersion = [int]($dotnetVersion.Split('.')[0])
    if ($majorVersion -lt 8) {
        Write-Host "   ✗ ERROR: .NET 8 or higher required. Found: $dotnetVersion" -ForegroundColor Red
        Write-Host "   Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "   ✗ ERROR: .NET SDK not found" -ForegroundColor Red
    Write-Host "   Download .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Restore NuGet packages
Write-Host "[2/5] Restoring NuGet packages..." -ForegroundColor Yellow
try {
    dotnet restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Packages restored successfully" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Package restore failed" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "   ✗ Package restore failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Build solution
Write-Host "[3/5] Building solution (Release)..." -ForegroundColor Yellow
try {
    dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Build succeeded" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Build failed" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "   ✗ Build failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Run tests
Write-Host "[4/5] Running tests..." -ForegroundColor Yellow
try {
    dotnet test --configuration Release --no-build --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ All tests passed" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Some tests failed (continuing anyway)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "   ⚠ Tests failed (continuing anyway): $_" -ForegroundColor Yellow
}

Write-Host ""

# Launch application
Write-Host "[5/5] Launching GovMatch..." -ForegroundColor Yellow
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Starting GovMatch Application" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

try {
    dotnet run --project GovMatch.App --configuration Release --no-build
}
catch {
    Write-Host ""
    Write-Host "Application exited or failed to start: $_" -ForegroundColor Red
    exit 1
}
