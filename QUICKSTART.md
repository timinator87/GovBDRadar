# GovMatch - Quick Start Guide

Get up and running in **5 minutes**!

## Step 1: Install .NET 8 SDK (2 minutes)

1. Go to: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download **".NET 8.0 SDK x64"** installer
3. Run installer (accept defaults)
4. Verify installation:
   ```powershell
   dotnet --version
   ```
   Should show: `8.0.x`

## Step 2: Get the Code

**Option A: Clone repository**
```powershell
git clone https://github.com/timinator87/GovBDRadar.git
cd GovBDRadar
```

**Option B: Download ZIP**
1. Download ZIP from GitHub
2. Extract to a folder
3. Open PowerShell in that folder

## Step 3: Build and Run (1 minute)

```powershell
.\build-and-run.ps1
```

That's it! The application will:
1. ✓ Check .NET version
2. ✓ Restore packages
3. ✓ Build solution
4. ✓ Run tests
5. ✓ Launch GovMatch

## Step 4: First-Time Setup (2 minutes)

When GovMatch launches:

### 4.1 Configure API Key
1. Click **Settings** (left sidebar)
2. Paste your SAM.gov API key
3. Select rate limit tier (default: 10/day)
4. Click **Save**

**Don't have an API key?**
- Get one free at: https://sam.gov/data-services
- Requires SAM.gov account (also free)

### 4.2 Setup Company Profile
1. Click **Company Profile**
2. Fill in:
   - **Company Name**
   - **Capabilities** (what you do - be detailed!)
   - **Keywords** (comma-separated)
   - **NAICS Codes** (e.g., `541330,541512`)
   - **PSC Codes** (e.g., `R425,D302`)
3. Click **Save Profile**

### 4.3 Create Retrieval Rule
1. Click **Retrieval Rules**
2. Click **Add New**
3. Configure:
   - **Name**: "My First Rule"
   - **Days Back**: 3
   - **NAICS**: Your primary NAICS code
   - **Max Pages**: 5
   - **Request Budget**: 25
4. Click **Save**

### 4.4 Run First Sync
1. Click **Sync / Logs**
2. Click **Run Sync Now**
3. Wait 10-30 seconds
4. Check the log output

### 4.5 View Opportunities
1. Click **Inbox**
2. Browse opportunities sorted by match score
3. Click any opportunity to view details
4. Click **Pursue**, **Save**, or **Ignore**

## Daily Use

**Automatic Syncs:**
- Runs daily at 6:00 AM (configurable)
- Check **Inbox** for new opportunities
- Check **Digest** for daily summary

**Manual Sync:**
- Click **Sync / Logs** → **Run Sync Now**

**Actions:**
- **Pursue**: Actively pursuing this opportunity
- **Save**: Bookmark for later review
- **Ignore**: Hide from inbox

## Creating Standalone .exe

To create a portable executable:

```powershell
.\publish-standalone.ps1
```

Output: `publish\GovMatch.App.exe` (~80-100 MB)

This .exe includes .NET runtime and can run on any Windows 10/11 machine without installing .NET.

## Troubleshooting

**"dotnet command not found"**
- Install .NET 8 SDK from link above
- Restart PowerShell after installation

**"Build failed"**
```powershell
dotnet nuget locals all --clear
dotnet restore --force
.\build-and-run.ps1
```

**"Rate limit exceeded"**
- Wait 24 hours, or
- Upgrade SAM.gov API key tier, or
- Reduce Request Budget in retrieval rules

**"No opportunities found"**
- Check your retrieval rule filters
- Try removing NAICS/PSC filters first
- Check SAM.gov is accessible

**"Low match scores"**
- Add more detail to Company Profile
- Add more keywords
- Ensure NAICS/PSC codes are correct

## Data Location

All data stored in:
```
%AppData%\GovMatch\govmatch.db
```

To backup:
```powershell
Copy-Item "$env:APPDATA\GovMatch" "C:\Backup\GovMatch_$(Get-Date -Format 'yyyyMMdd')" -Recurse
```

To reset:
```powershell
Remove-Item "$env:APPDATA\GovMatch\govmatch.db"
```

## Next Steps

- **[INSTALL.md](INSTALL.md)** - Detailed installation guide
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Complete user manual
- **[BUILD.md](BUILD.md)** - Advanced build options

## Quick Commands

**Build only:**
```powershell
dotnet build --configuration Release
```

**Run only:**
```powershell
dotnet run --project GovMatch.App --configuration Release
```

**Tests only:**
```powershell
dotnet test --configuration Release
```

**Clean build:**
```powershell
dotnet clean
dotnet restore
dotnet build --configuration Release
```

## Support

**Issues?** Check:
1. .NET 8 SDK installed: `dotnet --version`
2. All packages restored: `dotnet restore`
3. SAM.gov API key is valid
4. Internet connection active

**Still stuck?**
- See [INSTALL.md](INSTALL.md) for troubleshooting
- GitHub Issues: https://github.com/timinator87/GovBDRadar/issues

---

**Happy opportunity hunting!** 🎯
