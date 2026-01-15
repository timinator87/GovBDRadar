# GovBDRadar Web Interface Guide

A complete guide to using the GovBDRadar web interface for company intelligence extraction.

## Getting Started

### Launch the Web Interface

**On Linux/Mac:**
```bash
./run_web.sh
```

**On Windows:**
```
run_web.bat
```

The interface will automatically open in your browser at `http://localhost:8501`

### First-Time Setup

1. **Configure API Key** (if not already set)
   - Look for the "Configuration" section in the sidebar
   - Enter your Anthropic API key
   - Click "Save API Key"

   Alternatively, set it as an environment variable before launching:
   ```bash
   export ANTHROPIC_API_KEY="your-key-here"
   ./run_web.sh
   ```

## Interface Overview

### Home Page

The home page provides:
- **Statistics Dashboard**: View pending and approved profile counts
- **Feature Cards**: Quick links to main functions
- **Recent Activity**: See recently processed companies
- **Quick Start Guide**: Expandable help section

### Navigation

Use the sidebar to navigate between pages:
- 🏠 **Home** - Dashboard and statistics
- 📤 **Ingest** - Upload and process documents
- 📋 **Review** - Review pending profiles
- 📁 **Browse** - Search and filter all profiles

## Workflow

### 1. Ingest a Document (📤 Ingest Page)

**Step-by-step:**

1. Navigate to the **📤 Ingest** page

2. **Fill in Company Information:**
   - **Company Name*** (required): Full legal or common name
   - **Relationship*** (required): Select from:
     - `investment_target` - Potential investment
     - `portfolio` - Current portfolio company
     - `partner` - Business partner
     - `competitor` - Competitive analysis
   - **Primary Contact** (optional): Main point of contact
   - **Website** (optional): Company website URL
   - **Additional Context** (optional): Any guidance for extraction

3. **Upload Document:**
   - Click "Browse files" or drag-and-drop
   - Supported formats: PDF, PPTX
   - Maximum size: 50MB

4. **Configure Processing Options:**
   - ✓ **Extract and analyze images** - Analyzes diagrams, charts
   - ✓ **Enable OCR** - Extracts text from images
   - ☐ **Show detailed logs** - Displays processing steps

5. **Click "🚀 Process Document"**

6. **Wait for Processing:**
   - Progress bar shows current step
   - Typical processing time: 1-5 minutes
   - Depends on document length and image count

7. **View Results:**
   - See extracted capabilities summary
   - View confidence scores and warnings
   - Download JSON or Markdown files

**What happens:**
- Document is parsed (text + images extracted)
- AI analyzes content and extracts capabilities
- Evidence is scored and validated
- JSON profile and Markdown review are generated
- Files saved to `data/pending/`

### 2. Review Extraction (📋 Review Page)

**Step-by-step:**

1. Navigate to the **📋 Review** page

2. **Select Profile:**
   - Choose from dropdown list
   - Profiles sorted by most recent first
   - Shows company name and processing date

3. **Review Using Tabs:**

   **📊 Summary Tab:**
   - View key metrics (capabilities count, avg confidence)
   - Browse all capabilities with details
   - Review competitive positioning
   - Check contract execution details

   **📝 Markdown Tab:**
   - Read human-formatted review document
   - Easy-to-read summary format
   - Includes all key information

   **🔧 Edit JSON Tab:**
   - View raw JSON profile
   - Edit directly in text area
   - Click "💾 Save JSON Changes" to update
   - Validates JSON format before saving

   **⚠️ Warnings Tab:**
   - See all validation warnings
   - Review common issues checklist
   - No warnings = profile looks good!

4. **Make Corrections:**
   - Edit JSON directly in the "Edit JSON" tab
   - Add notes about corrections
   - Save changes before approving

5. **Approve or Delete:**
   - Click "✅ Approve Profile" to finalize
   - Add approval notes (optional but recommended)
   - Or click "🗑️ Delete Profile" to remove (requires confirmation)

**Approval Actions:**
- Profile moved from `data/pending/` to `data/approved/`
- Approval notes saved to `data/training/` for future improvements
- Profile ready for production use

### 3. Browse Profiles (📁 Browse Page)

**Features:**

1. **Statistics Overview:**
   - Total companies
   - Approved vs Pending counts
   - Total capabilities across all companies

2. **Filters:**
   - **Status**: Show approved, pending, or both
   - **Relationship**: Filter by portfolio, partner, etc.
   - **Domains**: Filter by operational domains
   - **Search**: Full-text search across company names and capabilities

3. **Sort Options:**
   - Name (alphabetical)
   - Last Modified (most recent first)
   - Capabilities Count (most capabilities first)
   - Avg Confidence (highest confidence first)

4. **Profile Cards:**
   - Expandable cards for each company
   - Shows key metrics and top capabilities
   - Color-coded confidence scores:
     - 🟢 Green: 70%+ (high confidence)
     - 🟡 Yellow: 40-69% (medium confidence)
     - 🔴 Red: <40% (low confidence)
   - Warning indicators
   - Quick actions (Download, View, Review)

5. **Export Options:**
   - **Export All as JSON**: Complete profile data
   - **Export Summary as CSV**: Spreadsheet with key metrics
   - Downloads filtered results only

6. **Company Comparison:**
   - Select two companies
   - Click "🔄 Compare"
   - Side-by-side comparison of capabilities

## Tips and Best Practices

### Document Preparation

**For Best Results:**
- Use high-quality PDFs (not scanned images)
- Ensure text is selectable in PDFs
- PowerPoint files extract more reliably than PDFs
- Include all slides/pages (don't remove content)

**Document Types:**
- ✅ Pitch decks (10-30 slides typical)
- ✅ Capability statements (2-10 pages)
- ✅ Technical white papers
- ✅ Product brochures with technical content

### Review Checklist

Before approving a profile, verify:

- [ ] Company name is correct
- [ ] All major capabilities are captured
- [ ] Confidence scores seem reasonable
- [ ] Readiness levels match evidence
- [ ] Competitive claims are accurate
- [ ] Contract vehicles are correct
- [ ] Past performance is captured
- [ ] No obvious extraction errors

### Adding Context

Use the "Additional Context" field to guide extraction:
- "Focus on counter-UAS capabilities"
- "They recently won SBIR Phase II with Air Force"
- "Partnership with L3Harris announced in March 2025"
- "Previous contractor was Raytheon"

This helps the AI extract more relevant information.

### Evidence Confidence Scores

Understanding confidence levels:

- **80-100%** (Excellent):
  - Backed by delivered contracts
  - Published metrics with test data
  - Third-party validation

- **50-79%** (Good):
  - Backed by partnerships
  - Ongoing demonstrations
  - Credible customer pipeline

- **20-49%** (Fair):
  - Mentioned in deck
  - Limited supporting evidence
  - Vague language

- **0-19%** (Poor):
  - Contradicted by other sources
  - Implausible given stage
  - Future tense only ("will be able to")

### Handling Warnings

Common validation warnings and how to address:

**"Capability marked production-ready but no delivery contracts"**
- Verify actual readiness level
- Check for missed evidence in document
- Correct readiness level if needed

**"Competitive claim lacks quantitative data"**
- Add metrics if available in document
- Note in approval comments if claim is qualitative only

**"Timeline shows 'prototype' but deck claims 'operational'"**
- Review source document
- Determine which is accurate
- Correct contradictory field

**"No contract vehicles documented"**
- Check if information is in document but not extracted
- Add manually if known
- Note as limitation if truly unavailable

## Keyboard Shortcuts

- **Ctrl/Cmd + Enter**: Submit forms
- **Escape**: Close modals and popups
- **Tab**: Navigate between form fields

## Troubleshooting

### "Anthropic API key not configured"

**Solution:**
1. Go to sidebar → Configuration section
2. Enter your API key
3. Click "Save API Key"

Or set environment variable before launching:
```bash
export ANTHROPIC_API_KEY="your-key-here"
```

### Document Processing Fails

**Common causes:**
- PDF is password-protected → Remove password
- File is corrupted → Re-download or re-save
- Text in images only → Enable OCR option
- File too large (>50MB) → Compress PDF or split

### "Insufficient content extracted"

**Solutions:**
- Enable OCR if text is in images
- Verify PDF has selectable text
- Check if document is mostly graphics
- Try converting to PPTX format

### Processing Takes Too Long

**Reasons:**
- Large documents (30+ pages)
- Many images to analyze
- OCR processing enabled
- API rate limits

**Solutions:**
- Be patient (can take 5-10 minutes for large docs)
- Disable image extraction if not needed
- Process during off-peak hours

### Profile Not Appearing in Browse

**Check:**
- Status filter (approved vs pending)
- Search/filter criteria
- Profile was successfully created (check `data/pending/`)

## Advanced Features

### Editing JSON Directly

When to edit JSON:
- Fix extraction errors (wrong capability name, etc.)
- Add missing information you know about
- Correct readiness levels
- Update confidence scores based on your knowledge

**Process:**
1. Go to Review page
2. Select profile
3. Switch to "Edit JSON" tab
4. Make changes to JSON
5. Click "Save JSON Changes"
6. Validate JSON is correct
7. Approve profile

### Batch Operations

To process multiple documents:
1. Process first document
2. While it's being reviewed, ingest next document
3. Switch between profiles in Review page
4. Approve as you complete each review

### Exporting for External Use

**Export Formats:**

**JSON** - For:
- Database imports
- API integration
- Further processing
- Backup/archival

**CSV** - For:
- Spreadsheet analysis
- Reporting
- Sharing with non-technical stakeholders
- Quick overviews

**Markdown** - For:
- Human reading
- Documentation
- Sharing as readable reports

## Performance Tips

### Speed Up Processing

1. **Disable image extraction** if:
   - Document has few/no relevant diagrams
   - Only need text-based capabilities
   - Processing speed is priority

2. **Disable OCR** if:
   - PDF has selectable text
   - No embedded images with text
   - Processing speed is priority

3. **Process smaller documents** first:
   - Test with 5-10 page documents
   - Verify extraction quality
   - Then process longer documents

### Reduce API Costs

- Disable image analysis for text-only documents
- Review config.yaml to adjust LLM settings
- Use lower temperature (already set to 0.1)
- Process multiple documents in same session

## Data Management

### File Locations

```
data/
├── pending/          # Unreviewed extractions
│   ├── Company_TIMESTAMP.json
│   └── Company_TIMESTAMP_review.md
├── approved/         # Production-ready profiles
│   ├── Company_TIMESTAMP.json
│   └── Company_TIMESTAMP_review.md
└── training/         # Correction logs
    └── Company_TIMESTAMP_corrections.txt
```

### Backup Profiles

Recommended backup strategy:
```bash
# Backup approved profiles
tar -czf profiles_backup_$(date +%Y%m%d).tar.gz data/approved/

# Backup to cloud storage
rclone copy data/approved/ remote:govbdradar/approved/
```

### Version Control

To track changes over time:
- Each ingestion creates timestamped files
- Multiple versions can coexist
- Compare versions using Browse page
- Git can track approved profiles

## Getting Help

### In-App Help

- Hover over ⓘ icons for field help
- Expand "Quick Start Guide" on home page
- Check "Workflow Overview" diagram

### External Resources

- **README.md** - Complete documentation
- **EXAMPLES.md** - Usage examples
- **config.yaml** - Configuration options
- **GitHub Issues** - Report bugs or request features

### Common Questions

**Q: Can I process multiple documents for one company?**
A: Yes! Process new documents and they'll create new profiles. You can then merge information manually or keep separate versions.

**Q: How do I update an existing profile?**
A: Process the new document, then manually merge information from both profiles, or replace the old profile entirely.

**Q: Can I run this on a server?**
A: Yes! Set `server.headless = true` in `.streamlit/config.toml` and expose port 8501.

**Q: Is my data secure?**
A: All processing is local. Only API calls to Anthropic for extraction. Documents and profiles stay on your machine.

**Q: Can I customize the extraction?**
A: Yes! Edit `extractors/capability_extractor.py` to modify prompts and extraction logic.

## Conclusion

The GovBDRadar web interface makes company intelligence extraction fast and intuitive. The human-in-the-loop workflow ensures high-quality, validated profiles for your government business development needs.

For command-line usage and automation, see the main README.md.

For detailed examples, see EXAMPLES.md.
