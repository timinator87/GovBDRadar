# GovMatch - Installation Guide for Windows

This guide will walk you through installing and running GovMatch on your Windows 10 or Windows 11 machine.

## Prerequisites

Before you begin, ensure you have:

1. **Windows 10 (version 1809 or later) or Windows 11**
2. **.NET 8 SDK** - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Download the "SDK x64" installer
   - Run the installer and follow the prompts
   - After installation, verify by opening PowerShell and running: `dotnet --version`
   - You should see: `8.0.x` or higher

3. **SAM.gov API Key**
   - Go to: https://sam.gov/data-services
   - Register for a free API key (requires SAM.gov account)
   - Save your API key - you'll need it when you first run the application

## Installation Steps

### Option 1: Build from Source (Recommended for Development)

#### Step 1: Clone or Download the Repository

If you have the code in a local directory already, skip to Step 2.

Otherwise, clone the repository:
```powershell
git clone https://github.com/timinator87/GovBDRadar.git
cd GovBDRadar
```

#### Step 2: Verify .NET Installation

Open PowerShell or Command Prompt and run:
```powershell
dotnet --version
```

You should see `8.0.x` or higher. If not, install the .NET 8 SDK from step 1 above.

#### Step 3: Restore NuGet Packages

From the `GovBDRadar` directory:
```powershell
dotnet restore
```

This will download all required dependencies (~50-100 MB).

#### Step 4: Build the Solution

```powershell
dotnet build --configuration Release
```

If the build succeeds, you'll see:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**If you encounter errors:**
- Verify .NET 8 SDK is installed: `dotnet --list-sdks`
- Clear NuGet cache and retry:
  ```powershell
  dotnet nuget locals all --clear
  dotnet restore --force
  dotnet build --configuration Release
  ```

#### Step 5: Run the Application

```powershell
dotnet run --project GovMatch.App --configuration Release
```

The WPF window should launch within 5-10 seconds.

### Option 2: Build as Self-Contained Executable (Recommended for Distribution)

This creates a standalone .exe that doesn't require .NET runtime installation.

#### Step 1-3: Same as Option 1

Follow Steps 1-3 from Option 1 above.

#### Step 4: Publish as Self-Contained

```powershell
dotnet publish GovMatch.App/GovMatch.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

**Output location:**
```
GovMatch.App\bin\Release\net8.0-windows\win-x64\publish\GovMatch.App.exe
```

#### Step 5: Run the Executable

Navigate to the publish folder:
```powershell
cd GovMatch.App\bin\Release\net8.0-windows\win-x64\publish
.\GovMatch.App.exe
```

**To install permanently:**
1. Copy the entire `publish` folder to `C:\Program Files\GovMatch\`
2. Create a desktop shortcut to `C:\Program Files\GovMatch\GovMatch.App.exe`
3. (Optional) Pin to Start Menu

## First-Time Setup

When you first launch GovMatch:

### 1. Configure SAM.gov API Key

1. Click **Settings** in the left navigation
2. Paste your SAM.gov API key in the "API Key" field
3. Select your rate limit tier:
   - **10/day** - Free tier (default)
   - **1,000/day** - Registered tier
   - **10,000/day** - Premium tier
4. Click **Save Settings**

The API key is encrypted using Windows DPAPI and stored securely in:
```
%AppData%\GovMatch\appsettings.json
```

### 2. Configure Company Profile

1. Click **Company Profile** in the left navigation
2. Fill in your company's information:
   - **Display Name**: Your company name
   - **Capabilities**: Describe what your company does (be detailed!)
   - **Keywords**: Comma-separated keywords relevant to your work
   - **NAICS Codes**: Comma-separated NAICS codes (e.g., `541330,541512,541519`)
   - **PSC Codes**: Comma-separated Product Service Codes (e.g., `R425,D302`)
   - **Target Agencies**: Agency codes you want to target (e.g., `DOD,DHS,NASA`)
   - **Set-Aside Preferences**: e.g., `SBA,8AN,SDVOSBC` (leave empty if not applicable)
   - **Past Performance**: Optional - describe your past government work
3. Click **Save Profile**

**Profile is automatically versioned** - every save creates a snapshot.

### 3. Create a Retrieval Rule

1. Click **Retrieval Rules** in the left navigation
2. Click **Add New Rule**
3. Configure the rule:
   - **Name**: e.g., "IT Services - Last 3 Days"
   - **Enabled**: ✓ (checked)
   - **Days Back**: 3 (how far back to search)
   - **Filters** (optional but recommended):
     - **NAICS Codes**: Your primary NAICS (e.g., `541512`)
     - **Keywords**: Search terms (e.g., "cybersecurity,cloud,software")
     - **Agency**: Leave blank to search all, or specify (e.g., `DOD`)
     - **Set-Aside**: Your set-aside type if applicable
   - **Max Pages**: 10 (100 opportunities per page)
   - **Request Budget**: 50 (max API calls for this rule)
4. Click **Save Rule**

**Important:** Start with narrow filters to stay within your rate limit!

### 4. Run Your First Sync

1. Click **Sync / Logs** in the left navigation
2. Click **Run Sync Now**
3. Watch the log output:
   ```
   [14:23:15] Starting sync...
   [14:23:17] Sync completed!
     - Opportunities fetched: 142
     - New opportunities: 142
     - Requests made: 2
   ```

Opportunities will now appear in the **Inbox**.

### 5. Review Opportunities in Inbox

1. Click **Inbox** in the left navigation
2. You'll see a list of opportunities sorted by **Match Score** (0-100)
3. Click any opportunity to view details:
   - Full description (fetched on-demand)
   - Match breakdown (why it scored the way it did)
   - Point of Contact
   - Attachments (download on demand)
   - Actions: **Save**, **Pursue**, **Ignore**

**Filters available:**
- Score threshold slider (e.g., show only ≥70)
- Date range
- Active opportunities only
- NAICS code
- Set-aside type
- Search text

### 6. Take Action on Opportunities

For opportunities you're interested in:
1. Click **Pursue** to mark as actively pursuing
2. Or click **Save** to bookmark for later
3. Or click **Ignore** to hide from inbox

These actions **improve future scoring** when you enable auto-tuning.

### 7. View Daily Digest

Every morning at 6:00 AM (configurable), GovMatch generates a digest:
- **New opportunities** (newly posted)
- **Updated opportunities** (SAM.gov modified them)
- **Newly relevant** (score jumped ≥70 after profile change)

1. Click **Digest** in the left navigation
2. View the latest digest with top matches

**Notification:** If enabled in Settings, you'll get a Windows Toast notification.

## Scheduled Syncs

By default, GovMatch automatically syncs **daily at 6:00 AM**.

To change the schedule:
1. Open `GovMatch.App/Jobs/SyncJob.cs`
2. Find the cron expression: `"0 0 6 * * ?"`
3. Modify using standard cron syntax:
   - `"0 0 6 * * ?"` = 6:00 AM daily
   - `"0 0 */6 * * ?"` = Every 6 hours
   - `"0 0 12 * * ?"` = 12:00 PM (noon) daily
4. Rebuild the application

## Data Storage

All data is stored locally in SQLite:

**Database location:**
```
%AppData%\GovMatch\govmatch.db
```

**Full path (typically):**
```
C:\Users\YourUsername\AppData\Roaming\GovMatch\govmatch.db
```

**Database includes:**
- Opportunities (with full SAM.gov data)
- Match scores and breakdowns
- Company profile and version history
- User actions (Pursue/Ignore/Saved)
- Retrieval rules
- Sync history
- Rate limit tracking
- Daily digests

**To backup your data:**
Copy the entire `%AppData%\GovMatch\` folder.

**To reset and start fresh:**
Delete `%AppData%\GovMatch\govmatch.db` and restart the app.

## Troubleshooting

### Application won't start

**Error: "The application requires .NET 8.0"**
- Install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
- Choose "Run desktop apps" runtime if you only want to run the app
- Choose "SDK" if you want to build from source

**Error: "govmatch.db is locked"**
- Close all instances of GovMatch
- Restart your computer
- Run GovMatch again

### Build errors

**Error: "SDK version not found"**
```powershell
dotnet --list-sdks
```
Ensure `8.0.x` is listed. If not, reinstall .NET 8 SDK.

**Error: "Package restore failed"**
```powershell
dotnet nuget locals all --clear
dotnet restore --force
```

### Rate limit issues

**Error: "Rate limit exceeded"**
- Wait 24 hours for the rolling window to reset
- Or upgrade your SAM.gov API key tier
- Reduce `Request Budget` in your retrieval rules
- Reduce `Max Pages` in your retrieval rules

**Check remaining requests:**
1. Go to **Sync / Logs**
2. View "Remaining Requests (24h window)"

### Database issues

**Error: "Table doesn't exist"**
The database schema may need to be migrated:
1. Close GovMatch
2. Delete `%AppData%\GovMatch\govmatch.db`
3. Restart GovMatch (database will be recreated)

**Note:** This deletes all data. Backup first if needed.

### Scoring issues

**All scores are 0 or very low**
- Ensure your **Company Profile** is filled out
- Add detailed capability text and keywords
- Add relevant NAICS and PSC codes
- The more detailed your profile, the better the matching

**Opportunities not showing up**
- Check your **Retrieval Rules** filters
- They may be too restrictive (narrow NAICS, specific agencies, etc.)
- Try a rule with **no filters** first to see if opportunities are being fetched

## Upgrading

To upgrade to a new version:

1. **Backup your data:**
   ```powershell
   Copy-Item "$env:APPDATA\GovMatch" "$env:USERPROFILE\Desktop\GovMatch_Backup_$(Get-Date -Format 'yyyyMMdd')" -Recurse
   ```

2. **Pull latest code:**
   ```powershell
   git pull origin main
   ```

3. **Rebuild:**
   ```powershell
   dotnet clean
   dotnet restore
   dotnet build --configuration Release
   ```

4. **Run:**
   ```powershell
   dotnet run --project GovMatch.App --configuration Release
   ```

The database migration system will automatically update your schema if needed.

## Advanced Configuration

### Change Sync Schedule

Edit `GovMatch.App/Jobs/SyncJob.cs`:
```csharp
.WithCronSchedule("0 0 6 * * ?")  // Change this line
```

Cron examples:
- `"0 0 6 * * ?"` - 6:00 AM daily
- `"0 0 */4 * * ?"` - Every 4 hours
- `"0 30 8 * * MON-FRI"` - 8:30 AM weekdays only

### Change Database Location

Edit `GovMatch.Data/DatabaseContext.cs`:
```csharp
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var dbFolder = Path.Combine(appDataPath, "GovMatch");
```

Change `"GovMatch"` to your desired folder name.

### Adjust Scoring Weights

Edit `GovMatch.App/App.xaml.cs` DI registration:
```csharp
services.AddSingleton<IScoringEngine>(sp =>
    new TfIdfScoringEngine(
        structuredWeight: 0.4,  // 40% structured
        textWeight: 0.6,        // 60% text similarity
        agencyWeight: 1.0       // Agency boost
    ));
```

Adjust weights as needed (they should sum to 1.0 for structured + text).

### Enable Debug Logging

Edit `GovMatch.App/App.xaml.cs` in `OnStartup`:
```csharp
// Add at the top of OnStartup
Console.SetOut(new StreamWriter("govmatch_debug.log") { AutoFlush = true });
```

Logs will be written to `govmatch_debug.log` in the app directory.

## Uninstalling

### If installed from build:

1. Delete the application folder (e.g., `C:\Program Files\GovMatch\`)
2. Delete data folder: `%AppData%\GovMatch\`
3. Remove any shortcuts

### If running from source:

1. Delete the repository folder
2. Delete data folder: `%AppData%\GovMatch\`

## Getting Help

- **Documentation**: See `USAGE_GUIDE.md` for detailed usage scenarios
- **Build Issues**: See `BUILD.md` for detailed build instructions
- **GitHub Issues**: https://github.com/timinator87/GovBDRadar/issues

## Quick Reference

**Build:**
```powershell
dotnet build --configuration Release
```

**Run:**
```powershell
dotnet run --project GovMatch.App --configuration Release
```

**Test:**
```powershell
dotnet test
```

**Publish:**
```powershell
dotnet publish GovMatch.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

**Database location:**
```
%AppData%\GovMatch\govmatch.db
```

**Default sync time:**
```
6:00 AM daily (configurable)
```

---

**You're now ready to discover government opportunities!** 🚀
