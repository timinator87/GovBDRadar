#!/bin/bash
# Setup script for GovBDRadar

set -e

echo "=================================="
echo "GovBDRadar Setup"
echo "=================================="
echo ""

# Check Python version
echo "Checking Python version..."
python_version=$(python3 --version 2>&1 | grep -oP '\d+\.\d+')
required_version="3.8"

if [ "$(printf '%s\n' "$required_version" "$python_version" | sort -V | head -n1)" != "$required_version" ]; then
    echo "Error: Python 3.8 or higher required. Found: $python_version"
    exit 1
fi

echo "✓ Python $python_version detected"
echo ""

# Create virtual environment (optional but recommended)
if [ ! -d "venv" ]; then
    echo "Creating virtual environment..."
    python3 -m venv venv
    echo "✓ Virtual environment created"
else
    echo "✓ Virtual environment already exists"
fi

echo ""
echo "Activating virtual environment..."
source venv/bin/activate

# Install dependencies
echo ""
echo "Installing dependencies..."
pip install --upgrade pip
pip install -r requirements.txt

echo ""
echo "✓ All dependencies installed"

# Check for API key
echo ""
echo "Checking for Anthropic API key..."
if [ -z "$ANTHROPIC_API_KEY" ]; then
    echo "⚠  ANTHROPIC_API_KEY not set"
    echo ""
    echo "To set your API key, run:"
    echo "  export ANTHROPIC_API_KEY='your-api-key-here'"
    echo ""
    echo "Or add it directly to config.yaml"
else
    echo "✓ ANTHROPIC_API_KEY is set"
fi

# Test CLI
echo ""
echo "Testing CLI..."
python ingest.py --help > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "✓ CLI is working"
else
    echo "✗ CLI test failed"
    exit 1
fi

echo ""
echo "=================================="
echo "Setup Complete!"
echo "=================================="
echo ""
echo "To use the tool:"
echo "  1. Activate virtual environment: source venv/bin/activate"
echo "  2. Set API key: export ANTHROPIC_API_KEY='your-key'"
echo "  3. Run: python ingest.py ingest --file your_file.pdf --company 'Company Name'"
echo ""
echo "See README.md for full documentation"
echo "See EXAMPLES.md for usage examples"
