# Usage Examples

## Example 1: Basic Ingestion

Ingest a pitch deck for a portfolio company:

```bash
python ingest.py ingest \
  --file "pitch_decks/AcmeTech_Pitch_2025.pdf" \
  --company "Acme Technologies" \
  --relationship "portfolio"
```

**Expected Output**:
```
Company Intelligence Ingestion
Company: Acme Technologies
File: pitch_decks/AcmeTech_Pitch_2025.pdf
Relationship: portfolio

Parsing AcmeTech_Pitch_2025.pdf...
✓ Extracted 1847 words from 24 pages/slides
Extracting capabilities for Acme Technologies...
✓ Extracted 5 capabilities
Validating profile and scoring evidence...
⚠  3 validation warnings
Generating outputs...
✓ JSON profile: data/pending/Acme_Technologies_20250115_143022.json
✓ Markdown review: data/pending/Acme_Technologies_20250115_143022_review.md

Extraction Complete!

⚠️  3 Validation Warning(s) for Acme Technologies:

1. Capability 'AI-Powered Threat Detection' marked as production_ready, but no delivery contracts found
2. No past performance or references provided
3. Competitive differentiation claimed but no quantitative data provided

Review these discrepancies before approving profile.

Next Steps:
1. Review the markdown file: data/pending/Acme_Technologies_20250115_143022_review.md
2. Edit JSON if needed: data/pending/Acme_Technologies_20250115_143022.json
3. Approve profile: python ingest.py approve --company "Acme Technologies"
```

## Example 2: Review and Approve

After ingestion, review the extracted profile:

```bash
python ingest.py review --company "Acme Technologies"
```

**Output**:
```
Latest extraction: Acme_Technologies_20250115_143022.json

┏━━━━━━━━━━━━━━━━━━━┳━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃ Field             ┃ Value                         ┃
┡━━━━━━━━━━━━━━━━━━━╇━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┩
│ Last Updated      │ 2025-01-15T14:30:22Z          │
│ Relationship      │ portfolio                     │
│ Capabilities      │ 5                             │
│ Warnings          │ 3                             │
└───────────────────┴───────────────────────────────┘

Review document: data/pending/Acme_Technologies_20250115_143022_review.md
JSON profile: data/pending/Acme_Technologies_20250115_143022.json
```

Open the markdown file, review it, and add any corrections:

```bash
vim data/pending/Acme_Technologies_20250115_143022_review.md
```

Edit the "Review Notes" section at the bottom:

```markdown
## Review Notes

*[Add your observations, corrections, and additional context here]*

- Confirmed with team: AI threat detection system IS deployed at USAF test range
- Updated readiness level from "production_ready" to "fielded"
- Missing from extraction: Recent $2M AFWERX contract award (announced last week)
- Contact: John Smith (CTO) mentioned L3Harris partnership talks in progress
```

Now approve with notes:

```bash
python ingest.py approve \
  --company "Acme Technologies" \
  --notes "Confirmed AI threat detection is fielded at USAF. Added missing $2M AFWERX award. Updated readiness level."
```

**Output**:
```
✓ Profile approved and moved to: data/approved/Acme_Technologies_20250115_143022.json
✓ Review document: data/approved/Acme_Technologies_20250115_143022_review.md
✓ Corrections logged for training
```

## Example 3: Investment Target Analysis

Analyzing a company you're considering investing in:

```bash
python ingest.py ingest \
  --file "due_diligence/DefenseCo_CapStatement.pdf" \
  --company "DefenseCo Systems" \
  --relationship "investment_target"
```

This extracts their capabilities and flags areas needing validation during due diligence.

## Example 4: Competitive Intelligence

Track a competitor's capabilities:

```bash
python ingest.py ingest \
  --file "competitor_materials/ShieldTech_Deck.pdf" \
  --company "ShieldTech Industries" \
  --relationship "competitor"
```

## Example 5: PowerPoint Ingestion

The tool works with both PDF and PPTX files:

```bash
python ingest.py ingest \
  --file "presentations/RocketSystems_Pitch.pptx" \
  --company "Rocket Systems Inc" \
  --relationship "partner"
```

## Example 6: Listing All Profiles

View all companies in your database:

```bash
python ingest.py list
```

**Output**:
```
Pending Profiles: 2
  - Acme_Technologies_20250115_143022.json
  - DefenseCo_Systems_20250115_150445.json

Approved Profiles: 12
  - Rocket_Systems_Inc_20250110_092134.json
  - ShieldTech_Industries_20250112_141523.json
  - Thor_Dynamics_20250114_103045.json
  ...
```

Filter by company:

```bash
python ingest.py list --company "Thor Dynamics"
```

**Output**:
```
Pending Profiles: 0

Approved Profiles: 2
  - Thor_Dynamics_20250114_103045.json
  - Thor_Dynamics_20250115_161208.json  # Updated with new capability statement
```

## Example 7: Updating an Existing Profile

When you receive a new document from a company:

```bash
# First ingestion (pitch deck)
python ingest.py ingest \
  --file "thor_pitch_deck_2024.pdf" \
  --company "Thor Dynamics" \
  --relationship "portfolio"

# Later, new capability statement arrives
python ingest.py ingest \
  --file "thor_capability_statement_2025.pdf" \
  --company "Thor Dynamics" \
  --relationship "portfolio"
```

Each creates a new profile with a timestamp. Compare them to track capability evolution.

## Example 8: Batch Processing

Process multiple files for different companies:

```bash
# Company A
python ingest.py ingest --file "docs/company_a.pdf" --company "Company A" --relationship "portfolio"

# Company B
python ingest.py ingest --file "docs/company_b.pdf" --company "Company B" --relationship "investment_target"

# Company C
python ingest.py ingest --file "docs/company_c.pptx" --company "Company C" --relationship "partner"

# Review all
python ingest.py list
```

## Example 9: Custom Configuration

Create a custom config for high-confidence extraction:

```bash
# Edit config.yaml
vim config.yaml
```

Set stricter evidence requirements:

```yaml
extraction:
  confidence_threshold: 70  # Only include high-confidence capabilities
  evidence_weights:
    contract_delivered: 50
    patent_granted: 35
    third_party_validated: 25
    demo_completed: 15
    partnership_announced: 10
    claimed_only: -10  # Penalize unsupported claims
```

Then run ingestion:

```bash
python ingest.py ingest --file "example.pdf" --company "Example Co" --config config.yaml
```

## Example 10: Image-Heavy Documents

For presentations with lots of diagrams:

```bash
# Ensure image analysis is enabled in config.yaml
# document_processing:
#   extract_images: true
#   ocr_enabled: true

python ingest.py ingest \
  --file "technical_diagrams_presentation.pdf" \
  --company "DiagramCo" \
  --relationship "partner"
```

The tool will extract and describe technical diagrams, system architectures, and performance charts.

## Common Workflows

### Workflow 1: Portfolio Company Tracking

```bash
# Initial ingestion
python ingest.py ingest --file "company_deck.pdf" --company "PortfolioCo" --relationship "portfolio"

# Review and approve
python ingest.py review --company "PortfolioCo"
python ingest.py approve --company "PortfolioCo" --notes "Baseline profile established"

# Quarterly update (3 months later)
python ingest.py ingest --file "company_deck_q2.pdf" --company "PortfolioCo" --relationship "portfolio"
python ingest.py review --company "PortfolioCo"
python ingest.py approve --company "PortfolioCo" --notes "Q2 update: New SBIR Phase II, moved laser system to production-ready"
```

### Workflow 2: Due Diligence

```bash
# Initial analysis
python ingest.py ingest --file "target_company.pdf" --company "TargetCo" --relationship "investment_target"

# Review output, identify gaps
python ingest.py review --company "TargetCo"

# Note validation warnings:
# - No past performance
# - High readiness claims without evidence
# - Missing contract vehicles

# Use these to create due diligence checklist for investor meeting
```

### Workflow 3: Competitive Monitoring

```bash
# Ingest competitor materials as you find them
python ingest.py ingest --file "competitor_a.pdf" --company "CompetitorA" --relationship "competitor"
python ingest.py ingest --file "competitor_b.pdf" --company "CompetitorB" --relationship "competitor"
python ingest.py ingest --file "competitor_c.pptx" --company "CompetitorC" --relationship "competitor"

# Approve all
python ingest.py approve --company "CompetitorA" --notes "Baseline competitive profile"
python ingest.py approve --company "CompetitorB" --notes "Baseline competitive profile"
python ingest.py approve --company "CompetitorC" --notes "Baseline competitive profile"

# List all to see competitive landscape
python ingest.py list
```

## Tips & Best Practices

1. **Always Review Before Approving**: LLM extraction isn't perfect. Review the markdown and JSON before approval.

2. **Add Detailed Notes**: Your notes become training data. Be specific about what the tool got wrong or missed.

3. **Use Descriptive Company Names**: Use full legal names for consistency across updates.

4. **Track Source Documents**: Keep original PDFs organized with clear naming (company_name_date.pdf).

5. **Regular Updates**: Re-ingest when companies release new materials to track capability evolution.

6. **Leverage Warnings**: Validation warnings highlight areas to verify during due diligence or BD calls.

7. **Compare Versions**: Keep multiple versions to see how company positioning evolves over time.

8. **Export Approved Profiles**: Approved JSON files can be imported into CRM systems or databases for opportunity matching.
