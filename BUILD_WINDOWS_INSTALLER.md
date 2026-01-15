# Building GovBDRadar Windows Installer

This guide explains how to build a standalone Windows installer for GovBDRadar that users can download and install with a simple double-click.

## Overview

The build process creates:
1. **Standalone Executable**: `GovBDRadar.exe` - No Python installation required
2. **Windows Installer**: `GovBDRadar-Setup-v1.0.0.exe` - Professional installer with Start Menu shortcuts

## Prerequisites

### Required Software

1. **Python 3.8 or higher**
   - Download from: https://www.python.org/downloads/
   - During installation, check "Add Python to PATH"

2. **PyInstaller** (automatically installed by build script)
   - Packages Python apps as standalone executables

3. **Inno Setup** (for creating the installer)
   - Download from: https://jrsoftware.org/isdl.php
   - Free, open-source Windows installer creator
   - Install with default options

### Optional

4. **PIL/Pillow** (for creating custom icon)
   - Automatically installed with requirements.txt

## Build Process

### Quick Build (Automated)

The easiest way to build everything:

```cmd
build_windows.bat
```

This script will:
1. Install PyInstaller
2. Clean previous builds
3. Install dependencies
4. Build the executable
5. Attempt to create the installer (if Inno Setup is installed)

### Step-by-Step Build

If you prefer manual control or troubleshooting:

#### Step 1: Prepare Environment

```cmd
# Install dependencies
pip install -r requirements.txt

# Install PyInstaller
pip install pyinstaller
```

#### Step 2: Create Application Icon (Optional)

```cmd
python create_icon.py
```

This creates `icon.ico` for the application. You can also:
- Use your own icon (must be .ico format, 256x256 recommended)
- Skip this step (app will use default icon)

#### Step 3: Build Executable

```cmd
pyinstaller GovBDRadar.spec
```

**What this does:**
- Analyzes all dependencies
- Bundles Python interpreter, libraries, and your code
- Creates standalone executable in `dist/GovBDRadar/`
- No Python installation required on target machines

**Output location:**
```
dist/GovBDRadar/
├── GovBDRadar.exe          # Main executable
├── app.py                   # Streamlit app
├── pages/                   # UI pages
├── templates/               # Data templates
├── data/                    # Data directories
└── [many .dll and .pyd files]  # Dependencies
```

#### Step 4: Test Executable

Before creating installer, test the executable:

```cmd
cd dist\GovBDRadar
GovBDRadar.exe
```

**Expected behavior:**
- Console window opens
- "Starting GovBDRadar..." message appears
- Browser opens to http://localhost:8501
- Web interface loads

**If it fails:**
- Check console for error messages
- Verify all dependencies are installed
- Check `GovBDRadar.spec` for missing files

#### Step 5: Create Installer

Open Inno Setup and:
1. File → Open → Select `installer.iss`
2. Build → Compile
3. Wait for compilation to complete

**Output location:**
```
installer_output/
└── GovBDRadar-Setup-v1.0.0.exe
```

This installer includes:
- All application files
- Start Menu shortcuts
- Desktop shortcut (optional)
- Uninstaller
- Creates data directories automatically

## Customization

### Change Application Name

Edit `installer.iss`:
```iss
#define MyAppName "Your App Name"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Your Company"
```

### Change Icon

Replace `icon.ico` with your custom icon, then rebuild.

### Modify Included Files

Edit `GovBDRadar.spec`:
```python
datas = [
    ('app.py', '.'),
    ('your_new_file.txt', '.'),  # Add custom files
    # ...
]
```

### Exclude Unnecessary Packages

To reduce file size, edit `GovBDRadar.spec`:
```python
excludes=[
    'matplotlib',
    'numpy',
    'scipy',
    # Add packages you don't need
]
```

## Distribution

### Installer Distribution

**Recommended**: Distribute the installer file:
- `GovBDRadar-Setup-v1.0.0.exe` (typically 150-300 MB)

**Advantages:**
- Professional installation experience
- Start Menu shortcuts
- Proper uninstallation
- Creates data directories
- Single file to distribute

**Distribution methods:**
- GitHub Releases
- Company website download
- Email to clients
- Cloud storage (Dropbox, Google Drive)

### Portable Distribution (Alternative)

Alternatively, you can zip the entire `dist/GovBDRadar/` folder:

```cmd
# Create portable version
cd dist
tar -czf GovBDRadar-Portable-v1.0.0.zip GovBDRadar/
```

**Advantages:**
- No installation required
- Can run from USB drive
- Multiple versions can coexist

**Disadvantages:**
- Larger download (all files uncompressed in zip)
- No Start Menu shortcuts
- User must manually create shortcuts

## User Installation

### Using the Installer

Users simply:
1. Download `GovBDRadar-Setup-v1.0.0.exe`
2. Double-click to run
3. Click "Next" through the wizard
4. Choose installation location (default: `C:\Program Files\GovBDRadar`)
5. Optionally create desktop shortcut
6. Click "Install"
7. Launch from Start Menu or desktop

### First-Time Setup for Users

After installation:

1. **Configure API Key** (two options):

   **Option A: System Environment Variable** (Recommended)
   - Press `Win + R`, type `sysdm.cpl`, press Enter
   - Go to "Advanced" tab → "Environment Variables"
   - Under "User variables", click "New"
   - Variable name: `ANTHROPIC_API_KEY`
   - Variable value: `your-api-key-here`
   - Click OK
   - Restart GovBDRadar

   **Option B: In the Web Interface**
   - Launch GovBDRadar
   - Enter API key in the sidebar
   - Click "Save API Key"

2. **Start Using**
   - Click Start Menu → GovBDRadar
   - Or double-click desktop shortcut (if created)
   - Web interface opens automatically

## Troubleshooting Build Issues

### PyInstaller Errors

**"Module not found" errors:**
```cmd
# Add to hiddenimports in GovBDRadar.spec
hiddenimports = [
    'your_missing_module',
]
```

**"RecursionError: maximum recursion depth exceeded":**
```cmd
# Increase recursion limit
import sys
sys.setrecursionlimit(5000)
```

**Executable too large:**
- Remove unnecessary packages in `excludes`
- Use UPX compression (already enabled)
- Consider excluding test files

### Inno Setup Errors

**"File not found" errors:**
- Verify `dist/GovBDRadar/` exists
- Check paths in `installer.iss` are correct
- Ensure PyInstaller completed successfully

**Icon not showing:**
- Verify `icon.ico` exists
- Check it's a valid ICO file (not renamed PNG)
- Rebuild with icon

### Runtime Errors

**"DLL load failed":**
- Missing Visual C++ Redistributable
- Include redistributable in installer
- Or add note in documentation

**"Port 8501 already in use":**
- Close other Streamlit instances
- Change port in launcher.py

**Application won't start:**
- Run from command line to see errors
- Check Windows Event Viewer
- Verify all dependencies bundled

## Advanced Configuration

### Code Signing (Recommended for Production)

Purchase a code signing certificate and sign the executable:

```cmd
signtool sign /f "certificate.pfx" /p "password" /t "http://timestamp.digicert.com" "dist\GovBDRadar\GovBDRadar.exe"
```

**Benefits:**
- Removes "Unknown Publisher" warning
- Builds trust with users
- Required for some enterprises

### Auto-Update Feature

Consider adding update checking:
1. Host version info on your server
2. Add update check in launcher.py
3. Prompt user to download new version

### Multiple Configurations

Create different builds:
- **Standard**: Full features
- **Lite**: Fewer dependencies, smaller size
- **Offline**: Bundled models for air-gapped systems

## Testing Checklist

Before distributing, test:

- [ ] Installer runs without errors
- [ ] Application launches successfully
- [ ] Web interface loads at localhost:8501
- [ ] Document upload works (PDF and PPTX)
- [ ] Processing completes successfully
- [ ] Review page displays correctly
- [ ] Approval workflow functions
- [ ] Browse page shows profiles
- [ ] Export features work (JSON, CSV, Markdown)
- [ ] Application uninstalls cleanly
- [ ] No leftover files after uninstall

**Test on clean Windows VM:**
- Fresh Windows 10/11 installation
- No Python installed
- No development tools
- This ensures truly standalone operation

## Build Artifacts

After successful build:

```
├── dist/
│   └── GovBDRadar/              # Portable version
│       └── GovBDRadar.exe       # Main executable
├── build/                        # Temporary build files (can delete)
├── installer_output/
│   └── GovBDRadar-Setup-v1.0.0.exe  # Installer (distribute this)
├── icon.ico                      # Application icon
└── icon.png                      # Icon source
```

## File Sizes

Typical sizes:
- **Executable folder**: 150-250 MB
- **Installer**: 150-300 MB (compressed)
- **Portable ZIP**: 200-350 MB

Sizes depend on:
- Number of dependencies
- Compression settings
- Excluded packages

## Continuous Integration

Automate builds with GitHub Actions:

```yaml
name: Build Windows Installer

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-python@v4
        with:
          python-version: '3.10'
      - name: Build
        run: build_windows.bat
      - name: Upload Installer
        uses: actions/upload-artifact@v3
        with:
          name: installer
          path: installer_output/*.exe
```

## Support

For build issues:
- Check PyInstaller documentation: https://pyinstaller.org/
- Check Inno Setup documentation: https://jrsoftware.org/ishelp/
- Open GitHub issue with build log

## Summary

Building the Windows installer:
1. Run `build_windows.bat` (automated)
2. Test `dist/GovBDRadar/GovBDRadar.exe`
3. Distribute `installer_output/GovBDRadar-Setup-v1.0.0.exe`

Users simply download and double-click the installer - no Python, no command line, no technical knowledge required!
