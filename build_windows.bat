@echo off
REM GovBDRadar Windows Build Script
REM This script builds the Windows installer

echo.
echo ====================================================
echo    GovBDRadar - Windows Installer Build Script
echo ====================================================
echo.

REM Check if Python is installed
where python >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Error: Python is not installed or not in PATH
    echo Please install Python 3.8 or higher
    pause
    exit /b 1
)

echo [1/6] Installing build dependencies...
pip install pyinstaller

echo.
echo [2/6] Cleaning previous builds...
if exist "build" rmdir /s /q build
if exist "dist" rmdir /s /q dist
if exist "installer_output" rmdir /s /q installer_output

echo.
echo [3/6] Installing application dependencies...
pip install -r requirements.txt

echo.
echo [4/6] Building executable with PyInstaller...
pyinstaller GovBDRadar.spec

if %ERRORLEVEL% NEQ 0 (
    echo Error: PyInstaller build failed
    pause
    exit /b 1
)

echo.
echo [5/6] Testing executable...
echo Checking if executable was created...
if not exist "dist\GovBDRadar\GovBDRadar.exe" (
    echo Error: Executable not found at dist\GovBDRadar\GovBDRadar.exe
    pause
    exit /b 1
)

echo Executable created successfully!
echo.

echo [6/6] Creating installer with Inno Setup...
echo.
echo To create the installer:
echo   1. Download and install Inno Setup from: https://jrsoftware.org/isdl.php
echo   2. Open installer.iss with Inno Setup
echo   3. Click Build -^> Compile
echo   4. The installer will be created in installer_output\
echo.

REM Try to run Inno Setup if it's installed
set INNO_PATH="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if exist %INNO_PATH% (
    echo Inno Setup found! Building installer...
    %INNO_PATH% installer.iss

    if %ERRORLEVEL% EQU 0 (
        echo.
        echo ====================================================
        echo    Build Complete!
        echo ====================================================
        echo.
        echo Executable: dist\GovBDRadar\GovBDRadar.exe
        echo Installer: installer_output\GovBDRadar-Setup-v1.0.0.exe
        echo.
        echo You can now distribute the installer to users!
        echo ====================================================
    )
) else (
    echo.
    echo ====================================================
    echo    Executable Build Complete!
    echo ====================================================
    echo.
    echo Location: dist\GovBDRadar\GovBDRadar.exe
    echo.
    echo To create installer:
    echo   1. Install Inno Setup
    echo   2. Run: build_windows.bat
    echo ====================================================
)

echo.
pause
