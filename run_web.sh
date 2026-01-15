#!/bin/bash

# GovBDRadar Web Interface Launcher
# This script launches the Streamlit web interface

echo "🎯 GovBDRadar - Company Intelligence Ingestion Tool"
echo "=================================================="
echo ""

# Check if Python is installed
if ! command -v python3 &> /dev/null; then
    echo "❌ Error: Python 3 is not installed"
    echo "Please install Python 3.8 or higher"
    exit 1
fi

# Check if virtual environment exists
if [ ! -d "venv" ]; then
    echo "📦 Creating virtual environment..."
    python3 -m venv venv
fi

# Activate virtual environment
echo "🔌 Activating virtual environment..."
source venv/bin/activate

# Install/update dependencies
echo "📦 Installing dependencies..."
pip install -q --upgrade pip
pip install -q -r requirements.txt

# Check for API key
if [ -z "$ANTHROPIC_API_KEY" ]; then
    echo ""
    echo "⚠️  Warning: ANTHROPIC_API_KEY environment variable not set"
    echo "You can set it in the web interface or export it:"
    echo "  export ANTHROPIC_API_KEY='your-key-here'"
    echo ""
fi

# Launch Streamlit
echo ""
echo "🚀 Launching web interface..."
echo "=================================================="
echo ""
echo "The interface will open in your browser at:"
echo "  http://localhost:8501"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""

streamlit run app.py
