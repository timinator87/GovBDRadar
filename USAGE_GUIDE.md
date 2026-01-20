# GovMatch Usage Guide

## Quick Start (5 Minutes)

### Step 1: Get a SAM.gov API Key
1. Visit https://sam.gov/data-services
2. Create a free account (if needed)
3. Request an API key
4. Copy your API key

### Step 2: Launch GovMatch
1. Run `GovMatch.App.exe` (or use `dotnet run`)
2. The application window opens

### Step 3: Configure Settings
1. Click **Settings** in the left navigation
2. Paste your API key
3. Select **Production** environment
4. Choose your rate limit tier (start with **10** for free tier)
5. Click **Save Settings**

### Step 4: Set Up Your Company Profile
1. Click **Company Profile** in the left navigation
2. Fill in:
   - **Company Name**: Your organization
   - **Capabilities**: "Software development, cloud migration, IT consulting"
   - **Keywords**: "software,cloud,agile,devops,security"
   - **NAICS Codes**: "541511,541512" (Custom Computer Programming + Systems Design)
   - **PSC Codes**: "D302,R408" (IT/Telecom + Professional Services)
3. Click **Save Profile**

### Step 5: Create a Retrieval Rule
1. Click **Retrieval Rules** in the left navigation
2. Click **Add**
3. Configure:
   - **Name**: "IT Services - Last 7 Days"
   - **Enabled**: ✓ (checked)
   - **Days Back**: 7
   - **NAICS Codes**: "541511,541512,541519"
   - **Max Pages**: 3
   - **Request Budget**: 5
4. Click **Save Rule**

### Step 6: Run Your First Sync
1. Click **Sync / Logs** in the left navigation
2. Click **Run Sync Now**
3. Watch the logs populate
4. Wait for "Sync completed!" message

### Step 7: Review Opportunities
1. Click **Inbox** in the left navigation
2. See opportunities sorted by match score (highest first)
3. Use the score slider to filter (e.g., only show 50+)
4. Click on any opportunity to select it

### Step 8: Take Actions
1. Select a high-scoring opportunity
2. Mark it as **Pursue**, **Saved**, or **Ignore**
3. Export selected opportunities to CSV for analysis

---

## Understanding Match Scores

### Score Ranges
- **90-100**: Excellent match - NAICS/PSC aligned + strong text similarity
- **70-89**: Good match - Multiple matching criteria
- **50-69**: Moderate match - Some alignment
- **30-49**: Weak match - Limited relevance
- **0-29**: Poor match - Consider ignoring or refining profile

### What Affects Your Score?

**Structured Factors (40% weight)**:
- ✅ **NAICS Code Match**: Your NAICS codes overlap with opportunity NAICS
- ✅ **PSC Code Match**: Your PSC codes match opportunity classification
- ✅ **Set-Aside Match**: Opportunity set-aside matches your preferences
- ✅ **Agency Match**: Opportunity agency is in your target list

**Text Similarity (60% weight)**:
- ✅ **Keyword Overlap**: Keywords appear in opportunity title/description
- ✅ **Capability Match**: Your capability text relates to opportunity description
- ✅ **Past Performance**: Relevant experience keywords match

### Example: Why Did This Match?

**Opportunity**: "Cloud Infrastructure Migration - VA"
**Your Profile**: Capabilities include "cloud migration services"
**Score**: 82/100

**Breakdown**:
- NAICS Match: 15/15 (541511 matched)
- PSC Match: 10/10 (D302 matched)
- Set-Aside: 0/10 (none specified)
- Agency: 0/5 (VA not in target list)
- Text Similarity: 57/60 (strong keyword overlap: "cloud", "migration", "infrastructure")

**Total**: 82/100

---

## Best Practices

### Optimize Your Company Profile

**✅ DO**:
- Write detailed capability text (200-500 words)
- Include specific technologies, methodologies, certifications
- List 10-20 relevant keywords
- Add all applicable NAICS codes (not just primary)
- Include PSC codes for all service categories
- Update past performance with recent contract types

**❌ DON'T**:
- Use vague descriptions ("We do IT stuff")
- Repeat the same keyword multiple times
- Include irrelevant NAICS codes (lowers precision)
- Leave fields empty

### Create Effective Retrieval Rules

**Strategy 1: Broad Discovery**
- Name: "All IT - Last 30 Days"
- Days Back: 30
- NAICS: "5415" (catches all 5415xx codes)
- Max Pages: 10
- Use for: Finding new opportunity types

**Strategy 2: Focused Targeting**
- Name: "Cloud Services - DoD - Active"
- Days Back: 7
- NAICS: "541511,541512"
- PSC: "D302,D307"
- Agency Codes: "9700"
- Title Keywords: "cloud"
- Use for: High-relevance opportunities

**Strategy 3: Set-Aside Only**
- Name: "8(a) Opportunities"
- Days Back: 14
- Set-Aside Codes: "8A"
- Max Pages: 5
- Use for: Targeted certifications

### Manage API Rate Limits

**Free Tier (10/day)**:
- Run sync **once per day** manually
- Use 1-2 highly focused retrieval rules
- Set `Request Budget` to 3-5 per rule
- Set `Max Pages` to 1-3

**Registered Tier (1,000/day)**:
- Enable automatic daily sync at 6 AM
- Use 5-10 retrieval rules
- Set `Request Budget` to 20-50 per rule
- Set `Max Pages` to 5-10
- Enable **Prefetch** for top matches (score > 75)

**Premium Tier (10,000/day)**:
- Run sync multiple times per day
- Use broad discovery rules
- Set `Request Budget` to 100+ per rule
- Enable prefetch with lower threshold (score > 50)

### Workflow Tips

**Daily Routine**:
1. Open Inbox
2. Filter: Min Score = 70, Active Only
3. Review top 10 opportunities
4. Mark as Pursue/Ignore
5. Export Pursue list to CSV
6. Review with team

**Weekly Review**:
1. Lower score filter to 50
2. Review medium-scoring opportunities
3. Update Company Profile with new keywords from good matches
4. Adjust retrieval rules based on results
5. Check rate limit usage in Sync / Logs

**Monthly Optimization**:
1. Analyze which rules find the most relevant opportunities
2. Disable low-value rules
3. Update NAICS/PSC codes based on awarded contracts
4. Refine capability text with recent project descriptions

---

## Common Scenarios

### Scenario 1: Too Many Irrelevant Results

**Problem**: Inbox filled with low-scoring, irrelevant opportunities

**Solution**:
1. Increase minimum score filter to 60+
2. Review retrieval rules - remove overly broad filters
3. Add more specific keywords to profile
4. Use PSC codes to narrow down service categories
5. Add title keywords to rules (e.g., "software", "cloud")

### Scenario 2: No Results

**Problem**: Sync completes but Inbox is empty

**Solution**:
1. Check Sync Logs for errors
2. Increase "Days Back" to 14 or 30
3. Remove restrictive filters from retrieval rules
4. Try a broad rule (just NAICS category, no other filters)
5. Verify API key is working (check remaining requests)

### Scenario 3: Hitting Rate Limits Too Fast

**Problem**: "Rate limit exceeded" message after first sync

**Solution**:
1. Reduce number of enabled retrieval rules to 1-2
2. Lower "Max Pages" to 1-2
3. Lower "Request Budget" to 3-5
4. Disable "Prefetch" in Settings
5. Consider upgrading SAM.gov API tier

### Scenario 4: Scores All Very Low

**Problem**: All opportunities scoring 20-40, nothing actionable

**Solution**:
1. Expand capability text - be more descriptive
2. Add more keywords (aim for 15-20)
3. Verify NAICS codes are correct for your services
4. Add PSC codes that match your offerings
5. Check that retrieval rules target the right NAICS/PSC

### Scenario 5: Want to Focus on Specific Agency

**Problem**: Only interested in Department of Defense opportunities

**Solution**:
1. Create a rule with Agency Code: "9700"
2. Add to Company Profile: Target Agency Codes: "9700"
3. Title Keywords: "dod,defense,army,navy,air force"
4. This will boost DoD opportunities in scoring

---

## Advanced Features

### Lazy Description Fetching

By default, full descriptions are **not** fetched during sync (saves API quota).

**When Descriptions Are Fetched**:
1. You manually open/select an opportunity
2. Opportunity score ≥ Prefetch Threshold (if enabled)

**To Enable Prefetch**:
1. Settings → Enable Prefetch for Top Matches ✓
2. Set Prefetch Threshold (e.g., 75)
3. Save Settings
4. Now, opportunities scoring 75+ will auto-fetch descriptions

**Why Use It**: Better text similarity scoring for high-value opportunities

### Export and Analysis

**CSV Export**:
- Includes: NoticeId, Title, Score, Agency, Dates, NAICS, PSC, Status, URL
- Use in Excel for pivot tables, charts, filtering
- Share with team for collaborative review

**Markdown Export**:
- Detailed opportunity report
- Match score breakdown
- Points of contact
- Links to SAM.gov
- Use for bid/no-bid decision documentation

### Scheduled Syncs

**Default**: Daily at 6:00 AM

**To Change Schedule**:
1. Open `GovMatch.App/App.xaml.cs`
2. Find Quartz configuration:
   ```csharp
   .WithCronSchedule("0 0 6 * * ?")
   ```
3. Change to desired schedule:
   - `0 0 8 * * ?` = 8:00 AM daily
   - `0 0 6,18 * * ?` = 6:00 AM and 6:00 PM daily
   - `0 0 6 * * MON-FRI` = 6:00 AM weekdays only
4. Rebuild application

**Cron Syntax**: `second minute hour day month dayOfWeek`

---

## Keyboard Shortcuts

(Note: These would need to be implemented in the WPF app)

**Suggested shortcuts**:
- `Ctrl+F`: Focus search box
- `Ctrl+R`: Refresh inbox
- `Ctrl+E`: Export to CSV
- `Ctrl+S`: Save current view/settings
- `F5`: Run sync now
- `Ctrl+1` through `Ctrl+5`: Navigate to different views

---

## Data Management

### Backup Your Data

**Location**: `%AppData%\GovMatch\govmatch.db`

**To Backup**:
1. Close GovMatch
2. Navigate to `C:\Users\<YourName>\AppData\Roaming\GovMatch`
3. Copy `govmatch.db` to safe location (OneDrive, USB drive, etc.)

**To Restore**:
1. Close GovMatch
2. Replace `govmatch.db` with backup copy
3. Restart GovMatch

### Reset Everything

**To Start Fresh**:
1. Close GovMatch
2. Delete entire `%AppData%\GovMatch` folder
3. Restart GovMatch (database recreates automatically)
4. Re-enter API key and profile

### View Database Directly

Use a SQLite browser like [DB Browser for SQLite](https://sqlitebrowser.org/):
1. Download and install DB Browser
2. Open `govmatch.db`
3. Browse tables: Opportunities, MatchResults, CompanyProfile, etc.
4. Run custom SQL queries
5. Export data to other formats

---

## Troubleshooting by Error Message

### "API key is required"
- Go to Settings
- Enter your SAM.gov API key
- Save Settings
- Restart application

### "Rate limit exceeded"
- Check Sync / Logs → Requests Remaining
- Wait for 24-hour rolling window reset
- Reduce retrieval rule budgets

### "Unauthorized (401)"
- API key is incorrect or expired
- Verify at sam.gov/data-services
- Re-enter in Settings

### "Not Found (404)"
- Endpoint URL may have changed (rare)
- Verify Environment is set to "Production"
- Check SAM.gov API status

### "Database is locked"
- Another instance of GovMatch is running
- Close all instances
- Restart application

---

## Getting Help

**Resources**:
- README.md - Technical documentation
- SAM.gov API Docs: https://open.gsa.gov/api/opportunities-api/
- Federal Hierarchy API: https://open.gsa.gov/api/fh-public-api/
- NAICS Codes: https://www.census.gov/naics/
- PSC Codes: https://www.acquisition.gov/PSC_Manual

**Community**:
- Submit issues on GitHub (if open sourced)
- SAM.gov Help Desk: https://www.fsd.gov/gsafsd_sp

---

**Happy opportunity hunting! 🎯**
