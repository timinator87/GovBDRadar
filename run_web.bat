@echo off
REM GovBDRadar Web Interface Launcher (Windows)

echo.
echo ====================================================
echo    GovBDRadar - Company Intelligence Ingestion Tool
echo ====================================================
echo.

REM Check if Python is installed
where python >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Error: Python is not installed or not in PATH
    echo Please install Python 3.8 or higher from python.org
    pause
    exit /b 1
)

REM Check if virtual environment exists
if not exist "venv\" (
    echo Creating virtual environment...
    python -m venv venv
)

REM Activate virtual environment
echo Activating virtual environment...
call venv\Scripts\activate.bat

REM Install/update dependencies
echo Installing dependencies...
pip install -q --upgrade pip
pip install -q -r requirements.txt

REM Check for API key
if "%ANTHROPIC_API_KEY%"=="" (
    echo.
    echo Warning: ANTHROPIC_API_KEY environment variable not set
    echo You can set it in the web interface or via command:
    echo   set ANTHROPIC_API_KEY=your-key-here
    echo.
)

REM Launch Streamlit
echo.
echo Launching web interface...
echo ====================================================
echo.
echo The interface will open in your browser at:
echo   http://localhost:8501
echo.
echo Press Ctrl+C to stop the server
echo.

streamlit run app.py
