# GovBDRadar - Installation Guide for Windows

Quick start guide for installing and using GovBDRadar on your Windows PC.

## System Requirements

- **Operating System**: Windows 10 or Windows 11 (64-bit)
- **RAM**: 4 GB minimum, 8 GB recommended
- **Disk Space**: 500 MB for application + space for data
- **Internet**: Required for document processing (API calls to Claude)

**Note**: No Python installation required! Everything is bundled.

## Installation

### Step 1: Download

Download the installer:
- **GovBDRadar-Setup-v1.0.0.exe** (~200 MB)

Download from:
- GitHub Releases: [Your GitHub URL]/releases
- Company website: [Your URL]

### Step 2: Run Installer

1. **Locate the downloaded file** in your Downloads folder
2. **Double-click** `GovBDRadar-Setup-v1.0.0.exe`

### Step 3: Windows SmartScreen Warning

If you see "Windows protected your PC":
1. Click **"More info"**
2. Click **"Run anyway"**

*Note: This appears because the app isn't code-signed. It's safe to run.*

### Step 4: Follow Installation Wizard

1. **Welcome Screen**: Click "Next"
2. **License Agreement**: Read and accept, click "Next"
3. **Installation Location**:
   - Default: `C:\Program Files\GovBDRadar`
   - Click "Next" (or "Browse" to change)
4. **Start Menu Folder**: Click "Next" (use default)
5. **Additional Tasks**:
   - ☑ Create desktop shortcut (optional)
   - Click "Next"
6. **Ready to Install**: Click "Install"
7. **Installation Progress**: Wait for files to copy (~30 seconds)
8. **Completing Setup**:
   - ☑ Launch GovBDRadar (optional)
   - Click "Finish"

## First-Time Setup

### Configure Your API Key

You need an Anthropic API key to process documents.

**Option 1: Set as Environment Variable (Recommended)**

This saves your API key permanently:

1. Press `Win + R` on your keyboard
2. Type `sysdm.cpl` and press Enter
3. Click the **"Advanced"** tab
4. Click **"Environment Variables"** button
5. Under "User variables", click **"New"**
6. Set:
   - **Variable name**: `ANTHROPIC_API_KEY`
   - **Variable value**: `your-api-key-here` (paste your key)
7. Click **OK** on all windows
8. **Restart GovBDRadar** for changes to take effect

**Option 2: Set in Web Interface**

This sets the key for the current session only:

1. Launch GovBDRadar
2. Look for the **"Configuration"** section in the left sidebar
3. Click in the **"Anthropic API Key"** field
4. Paste your API key
5. Click **"Save API Key"**

**Don't have an API key?**
- Get one from: https://console.anthropic.com/
- Sign up for an Anthropic account
- Create an API key in your account settings

## How to Use

### Launching the Application

**Method 1: Start Menu**
1. Click Windows Start button
2. Type "GovBDRadar"
3. Click the app icon

**Method 2: Desktop Shortcut** (if created during installation)
1. Double-click the GovBDRadar icon on your desktop

**Method 3: Directly**
1. Navigate to installation folder: `C:\Program Files\GovBDRadar`
2. Double-click `GovBDRadar.exe`

### What Happens When You Launch

1. A **console window** opens (black window with text)
   - **⚠️ DO NOT CLOSE THIS WINDOW**
   - Closing it will stop the application

2. Your **web browser** opens automatically
   - Opens to: http://localhost:8501
   - The application interface loads

3. If browser doesn't open automatically:
   - Manually open: http://localhost:8501

### Quick Start Workflow

**1. Upload a Document**
- Click **"📤 Ingest"** in the sidebar
- Click **"Browse files"** or drag-and-drop your PDF/PPTX
- Fill in company information
- Click **"🚀 Process Document"**
- Wait 2-5 minutes for processing

**2. Review Results**
- Click **"📋 Review"** in the sidebar
- Select your company from the dropdown
- Review capabilities and warnings
- Edit JSON if needed (click "Edit JSON" tab)

**3. Approve Profile**
- After reviewing, click **"✅ Approve Profile"**
- Add any notes
- Profile moves to approved folder

**4. Browse All Profiles**
- Click **"📁 Browse"** in the sidebar
- Search and filter companies
- Compare capabilities
- Export to CSV or JSON

### Stopping the Application

To close GovBDRadar:
1. **Close your web browser** (or just the tab)
2. **Go to the console window** (black window)
3. Press **Ctrl + C** on your keyboard
4. Or simply **close the console window**

## Data Location

All your data is stored in:
```
C:\Program Files\GovBDRadar\data\
├── pending\      # Unreviewed profiles
├── approved\     # Approved profiles
└── training\     # Your corrections for improvement
```

**Backup Your Data:**
Simply copy the entire `data` folder to a backup location.

## Troubleshooting

### Application Won't Start

**Issue**: Double-clicking does nothing

**Solutions**:
- Right-click → "Run as administrator"
- Check if another instance is already running (look for console window)
- Restart your computer

---

**Issue**: "API key not configured" error

**Solutions**:
- Set API key using Option 1 or Option 2 above
- Verify you copied the key correctly (no extra spaces)
- Make sure you have an active Anthropic account

---

**Issue**: "Port 8501 already in use"

**Solutions**:
- Close other GovBDRadar instances
- Close other Streamlit applications
- Restart your computer

### Browser Issues

**Issue**: Browser doesn't open automatically

**Solutions**:
- Manually type in browser: `http://localhost:8501`
- Try a different browser (Chrome, Firefox, Edge)
- Check if your antivirus is blocking it

---

**Issue**: "This site can't be reached"

**Solutions**:
- Check if the console window is still open
- Wait a few more seconds for the app to start
- Try refreshing the page (F5)

### Processing Issues

**Issue**: Document upload fails

**Solutions**:
- Check file size (<50MB)
- Verify file format (PDF or PPTX only)
- Try re-saving the document
- Check if file is password-protected

---

**Issue**: Processing takes too long (>10 minutes)

**Solutions**:
- Large documents take longer (be patient)
- Disable image extraction for faster processing
- Check your internet connection
- Try a smaller document first

### Uninstallation Issues

**Issue**: Want to completely remove GovBDRadar

**Steps**:
1. **Uninstall normally**:
   - Start Menu → Settings → Apps
   - Find "GovBDRadar"
   - Click "Uninstall"

2. **Remove data** (if you want to delete profiles):
   - Navigate to: `C:\Program Files\GovBDRadar\data\`
   - Delete the `data` folder

3. **Remove environment variable** (if set):
   - Press `Win + R`, type `sysdm.cpl`
   - Advanced → Environment Variables
   - Delete `ANTHROPIC_API_KEY` from User variables

## Tips for Best Results

### Document Preparation
- Use high-quality PDFs with selectable text
- PowerPoint files work better than scanned PDFs
- Ensure text isn't all in images (unless OCR enabled)

### API Key Security
- Never share your API key
- Don't commit it to version control
- Treat it like a password

### Performance
- Close other applications for faster processing
- Disable image extraction if not needed
- Process documents during off-peak hours if on shared internet

### Regular Maintenance
- Backup approved profiles regularly
- Clean up old pending profiles
- Export to CSV for archival

## Getting Help

### Documentation
- **Full User Guide**: In installation folder → `WEB_INTERFACE_GUIDE.md`
- **Examples**: In installation folder → `EXAMPLES.md`
- **README**: In installation folder → `README.md`

### Online Help
- **GitHub Issues**: Report bugs or ask questions
- **Email Support**: [your-support-email]
- **Documentation**: [your-documentation-url]

### In-App Help
- Hover over **ⓘ** icons for tooltips
- Expand **"Quick Start Guide"** on home page
- Check **"Workflow Overview"** section

## Video Tutorial

[Link to video tutorial if available]

## FAQ

**Q: Do I need Python installed?**
A: No! Everything is bundled. Just install and run.

**Q: Is my data sent to the cloud?**
A: Only API calls to Anthropic for processing. Documents and profiles stay on your PC.

**Q: Can I use this offline?**
A: No, it requires internet for document processing (API calls).

**Q: How much does API usage cost?**
A: Check Anthropic's pricing. Typical document costs $0.10-$1.00 depending on length.

**Q: Can I install on multiple computers?**
A: Yes! Install on as many PCs as you need.

**Q: Can I process multiple documents at once?**
A: Process one at a time. Each takes 2-10 minutes.

**Q: Where are my documents stored?**
A: Not stored! Only the extracted intelligence profiles are saved.

**Q: Can I edit profiles after approval?**
A: Yes, edit the JSON files directly or re-process the document.

## Support Contact

For technical support:
- **Email**: [support-email]
- **GitHub**: [github-url]/issues
- **Documentation**: [docs-url]

---

**You're all set!** 🎉

Launch GovBDRadar from your Start Menu and start processing documents!
