# Build Instructions for GovMatch

## System Requirements

- **Operating System**: Windows 10 version 1809 or later, Windows 11
- **Development Tools**: .NET 8 SDK
- **Optional**: Visual Studio 2022 (17.8 or later) or JetBrains Rider

## Prerequisites Installation

### Install .NET 8 SDK

1. **Download** .NET 8 SDK from:
   - https://dotnet.microsoft.com/download/dotnet/8.0

2. **Verify installation**:
   ```bash
   dotnet --version
   ```
   Should output: `8.0.x` or higher

## Building the Solution

### Command Line Build

1. **Navigate to solution directory**:
   ```bash
   cd GovBDRadar
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Build in Debug mode**:
   ```bash
   dotnet build
   ```

4. **Build in Release mode**:
   ```bash
   dotnet build --configuration Release
   ```

5. **Run the application**:
   ```bash
   dotnet run --project GovMatch.App
   ```

### Visual Studio Build

1. **Open** `GovMatch.sln` in Visual Studio 2022

2. **Restore packages**:
   - Right-click solution → Restore NuGet Packages

3. **Build**:
   - Press `Ctrl+Shift+B`
   - Or: Build → Build Solution

4. **Run**:
   - Press `F5` (Debug) or `Ctrl+F5` (Release)
   - Or: Set `GovMatch.App` as startup project and click Start

## Running Tests

### Command Line

```bash
dotnet test
```

### With Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Visual Studio

1. Open **Test Explorer** (Test → Test Explorer)
2. Click **Run All Tests**
3. View results in Test Explorer window

## Publishing

### Self-Contained Executable (Recommended for Distribution)

Creates a standalone .exe that doesn't require .NET runtime on target machine:

```bash
dotnet publish GovMatch.App/GovMatch.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

**Output location**:
```
GovMatch.App/bin/Release/net8.0-windows/win-x64/publish/GovMatch.App.exe
```

**Size**: ~80-100 MB (includes .NET runtime)

### Framework-Dependent Executable

Smaller but requires .NET 8 runtime on target machine:

```bash
dotnet publish GovMatch.App/GovMatch.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -p:PublishSingleFile=true
```

**Output location**:
```
GovMatch.App/bin/Release/net8.0-windows/win-x64/publish/GovMatch.App.exe
```

**Size**: ~5-10 MB

### Publish for Different Platforms

**Windows x64**:
```bash
-r win-x64
```

**Windows ARM64**:
```bash
-r win-arm64
```

## MSIX Packaging (Optional)

For Microsoft Store or enterprise distribution:

### Prerequisites

- Visual Studio 2022 with "Windows Application Packaging" workload
- Windows 10 SDK (10.0.19041.0 or later)

### Steps

1. **Add Windows Application Packaging Project**:
   - File → Add → New Project
   - Search: "Windows Application Packaging Project"
   - Name: `GovMatch.Package`

2. **Reference the WPF app**:
   - Right-click `GovMatch.Package` → Add → Reference
   - Select `GovMatch.App`

3. **Configure package**:
   - Open `Package.appxmanifest`
   - Set application details (name, publisher, version)
   - Add application icon
   - Configure capabilities

4. **Build MSIX**:
   - Right-click `GovMatch.Package` → Publish → Create App Packages
   - Follow wizard

5. **Output**:
   - MSIX file in `GovMatch.Package/AppPackages/`

## Build Configurations

### Debug

- Includes debugging symbols
- No optimizations
- Detailed error messages
- Use for development

**Command**:
```bash
dotnet build --configuration Debug
```

### Release

- Optimized for performance
- Smaller binary size
- Production-ready
- Use for distribution

**Command**:
```bash
dotnet build --configuration Release
```

## Project Structure

```
GovBDRadar/
├── GovMatch.sln                    # Solution file
├── GovMatch.Core/
│   ├── GovMatch.Core.csproj       # Class library
│   ├── Models/                     # Domain models
│   ├── Interfaces/                 # Abstractions
│   └── Services/                   # Business logic
├── GovMatch.Data/
│   ├── GovMatch.Data.csproj       # Data access layer
│   ├── DatabaseContext.cs          # SQLite context
│   ├── Repositories/               # Data repositories
│   └── Services/                   # Data services
├── GovMatch.Integrations.SamGov/
│   ├── GovMatch.Integrations.SamGov.csproj  # SAM.gov integration
│   ├── DTOs/                       # API response models
│   ├── Mappers/                    # Data mapping
│   └── Services/                   # API client
├── GovMatch.App/
│   ├── GovMatch.App.csproj        # WPF application
│   ├── App.xaml                    # Application entry
│   ├── MainWindow.xaml             # Main window
│   ├── ViewModels/                 # MVVM ViewModels
│   ├── Views/                      # XAML views
│   ├── Commands/                   # UI commands
│   ├── Services/                   # UI services
│   └── Jobs/                       # Background jobs
└── GovMatch.Tests/
    ├── GovMatch.Tests.csproj      # Test project
    └── *Tests.cs                   # Test files
```

## Dependencies

### NuGet Packages

**GovMatch.Core**: None (pure .NET 8)

**GovMatch.Data**:
- Microsoft.Data.Sqlite (8.0.0)
- Dapper (2.1.28)

**GovMatch.Integrations.SamGov**:
- Polly (8.2.1)

**GovMatch.App**:
- Microsoft.Extensions.DependencyInjection (8.0.0)
- Microsoft.Extensions.Hosting (8.0.0)
- Quartz (3.8.0)
- Quartz.Extensions.Hosting (3.8.0)

**GovMatch.Tests**:
- Microsoft.NET.Test.Sdk (17.8.0)
- xunit (2.6.5)
- xunit.runner.visualstudio (2.5.6)
- coverlet.collector (6.0.0)
- Moq (4.20.70)

## Troubleshooting Build Issues

### "SDK version not found"

**Problem**: .NET 8 SDK not installed or wrong version

**Solution**:
```bash
dotnet --list-sdks
```
Ensure `8.0.x` is listed. If not, reinstall .NET 8 SDK.

### "Package restore failed"

**Problem**: NuGet package download issues

**Solution**:
```bash
dotnet nuget locals all --clear
dotnet restore --force
```

### "Project not supported"

**Problem**: Trying to build on non-Windows platform

**Solution**:
- WPF requires Windows. Use Windows 10/11 or Windows Server.
- For cross-platform, consider Avalonia UI or MAUI (requires rewrite)

### "MSB3644: Reference assemblies not found"

**Problem**: Missing Windows SDK

**Solution**:
1. Install Visual Studio 2022
2. Select ".NET desktop development" workload
3. Ensure Windows 10 SDK is checked

### Build warnings about nullable reference types

**Expected**: The projects use nullable reference types (`<Nullable>enable</Nullable>`)

**To suppress**:
- These are informational warnings
- Add `#nullable disable` to top of files if needed
- Or fix by adding `?` to nullable types

## Performance Optimization

### Build Performance

**Parallel builds**:
```bash
dotnet build -m
```

**Incremental builds**:
```bash
dotnet build --no-restore
```

**Skip tests during build**:
```bash
dotnet build --no-restore /p:RunTests=false
```

### Runtime Performance

Published Release builds are ~2-3x faster than Debug builds due to:
- Dead code elimination
- Inlining
- Optimized IL code
- Trimming (if enabled)

## Clean Build

To ensure a clean build:

```bash
# Clean solution
dotnet clean

# Remove bin/obj folders
Remove-Item -Recurse -Force */bin,*/obj

# Restore and rebuild
dotnet restore
dotnet build --no-incremental
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Test
      run: dotnet test --no-restore --verbosity normal
```

## Distribution Checklist

Before distributing the application:

- [ ] Build in **Release** configuration
- [ ] Run all tests: `dotnet test`
- [ ] Publish as self-contained: `dotnet publish -c Release -r win-x64 --self-contained`
- [ ] Test published executable on clean Windows VM
- [ ] Verify database creates successfully in `%AppData%\GovMatch`
- [ ] Test API key encryption/decryption
- [ ] Verify scheduled jobs run correctly
- [ ] Include README.md and USAGE_GUIDE.md in distribution
- [ ] Sign executable (optional, for enterprise)
- [ ] Create installer with WiX or Inno Setup (optional)

## Support

For build issues:
- Check .NET 8 SDK is installed: `dotnet --version`
- Verify NuGet packages restored: `dotnet restore --force`
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Check Windows SDK installed (for WPF)

---

**Build successfully tested on**:
- Windows 11 22H2
- Windows 10 22H2
- .NET 8.0.0 SDK
- Visual Studio 2022 17.8+
