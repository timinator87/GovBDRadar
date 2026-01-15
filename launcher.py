"""
GovBDRadar Launcher
Simple launcher script that starts the Streamlit application
"""
import os
import sys
import subprocess
import webbrowser
import time
from pathlib import Path

def main():
    # Get the application directory
    if getattr(sys, 'frozen', False):
        # Running as compiled executable
        app_dir = Path(sys.executable).parent
    else:
        # Running as script
        app_dir = Path(__file__).parent

    # Change to app directory
    os.chdir(app_dir)

    # Check for API key
    if not os.getenv('ANTHROPIC_API_KEY'):
        print("=" * 60)
        print("  GovBDRadar - Company Intelligence Ingestion Tool")
        print("=" * 60)
        print()
        print("⚠️  ANTHROPIC_API_KEY not found")
        print()
        print("You can either:")
        print("1. Set it as a system environment variable")
        print("2. Configure it in the web interface when it opens")
        print()
        print("The application will launch in 5 seconds...")
        time.sleep(5)

    # Start Streamlit
    print()
    print("🚀 Starting GovBDRadar...")
    print("=" * 60)
    print()
    print("The web interface will open at: http://localhost:8501")
    print()
    print("⚠️  DO NOT CLOSE THIS WINDOW")
    print("    Closing this window will stop the application.")
    print()
    print("=" * 60)
    print()

    # Open browser after a short delay
    def open_browser():
        time.sleep(3)
        webbrowser.open('http://localhost:8501')

    # Start browser opener in background
    import threading
    browser_thread = threading.Thread(target=open_browser)
    browser_thread.daemon = True
    browser_thread.start()

    # Launch Streamlit
    try:
        subprocess.run([
            sys.executable,
            '-m',
            'streamlit',
            'run',
            str(app_dir / 'app.py'),
            '--server.headless=true',
            '--server.port=8501',
            '--browser.gatherUsageStats=false'
        ])
    except KeyboardInterrupt:
        print()
        print("Shutting down GovBDRadar...")
        sys.exit(0)
    except Exception as e:
        print(f"Error starting application: {e}")
        print()
        print("Press Enter to exit...")
        input()
        sys.exit(1)

if __name__ == "__main__":
    main()
