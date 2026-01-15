# GovBDRadar - Company Intelligence Ingestion Tool

A powerful tool for extracting structured capability intelligence from pitch decks and capability statements, designed specifically for government business development and investment analysis.

## Overview

GovBDRadar automatically analyzes pitch decks and capability statements to extract:

- **Technical Capabilities** with readiness levels and evidence scoring
- **Operational Domains** and specific use cases
- **Competitive Positioning** against incumbents
- **Contract Execution** capabilities and past performance
- **Development Trajectory** and 12-month outlook
- **Validation Warnings** for claims requiring verification

The tool uses Claude (Anthropic's LLM) to extract structured data and generates both JSON profiles and human-readable markdown summaries for review.

## Key Features

- **Multi-Format Support**: Processes PDF and PowerPoint (PPTX) files
- **Image Analysis**: Extracts and analyzes diagrams, technical schematics, and charts using vision models
- **OCR Support**: Handles text embedded in images
- **Evidence-Based Scoring**: Assigns confidence scores based on claim evidence
- **Human-in-the-Loop**: Review and edit extractions before approval
- **Training Data Collection**: Captures user corrections to improve future extractions
- **Validation Warnings**: Flags mismatches, missing evidence, and red flags

## Installation

### Prerequisites

- Python 3.8 or higher
- Anthropic API key (Claude)

### Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/timinator87/GovBDRadar.git
   cd GovBDRadar
   ```

2. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

3. **Configure API keys**:
   ```bash
   export ANTHROPIC_API_KEY="your-api-key-here"
   ```

   Alternatively, edit `config.yaml` and set your API key directly.

4. **Verify installation**:
   ```bash
   python ingest.py --help
   ```

## Quick Start

### 1. Ingest a Document

Extract capabilities from a pitch deck or capability statement:

```bash
python ingest.py ingest \
  --file "path/to/ThorDynamics_Deck.pdf" \
  --company "Thor Dynamics" \
  --relationship "portfolio"
```

**Parameters**:
- `--file`: Path to PDF or PPTX file (required)
- `--company`: Company name (required)
- `--relationship`: One of: `portfolio`, `investment_target`, `partner`, `competitor` (default: `investment_target`)

**Output**:
- JSON profile: `data/pending/Thor_Dynamics_TIMESTAMP.json`
- Markdown review: `data/pending/Thor_Dynamics_TIMESTAMP_review.md`

### 2. Review Extraction

Review the generated profile:

```bash
python ingest.py review --company "Thor Dynamics"
```

This displays a summary and shows the paths to the JSON and Markdown files.

Open the markdown file in your editor to review:
```bash
vim data/pending/Thor_Dynamics_TIMESTAMP_review.md
```

### 3. Approve Profile

After reviewing and making any corrections, approve the profile:

```bash
python ingest.py approve \
  --company "Thor Dynamics" \
  --notes "Corrected readiness level from 'production ready' to 'prototype testing'. Verified L3Harris partnership."
```

The profile is moved to `data/approved/` and corrections are logged for training.

### 4. List Profiles

View all pending and approved profiles:

```bash
# List all profiles
python ingest.py list

# List profiles for specific company
python ingest.py list --company "Thor Dynamics"
```

## Configuration

Edit `config.yaml` to customize behavior:

### API Keys

```yaml
api_keys:
  anthropic: "${ANTHROPIC_API_KEY}"  # Or set directly
```

### LLM Settings

```yaml
llm:
  provider: "anthropic"
  model: "claude-sonnet-4-5-20250929"
  temperature: 0.1  # Low for consistency
  max_tokens: 4096
```

### Document Processing

```yaml
document_processing:
  ocr_enabled: true  # Extract text from images
  extract_images: true  # Analyze diagrams/charts
  min_text_length: 100  # Minimum words required
```

### Evidence Scoring Weights

```yaml
extraction:
  evidence_weights:
    contract_delivered: 40
    patent_granted: 30
    third_party_validated: 20
    demo_completed: 15
    partnership_announced: 10
    claimed_only: 0
```

## Capability Taxonomy

The tool extracts capabilities into a structured JSON schema:

```json
{
  "company_name": "Thor Dynamics",
  "last_updated": "2025-01-15T10:30:00Z",
  "relationship": "portfolio",

  "core_capabilities": [
    {
      "capability_name": "High-Energy Laser Defense System",
      "description": "Mobile directed energy weapon for counter-UAS...",
      "technical_category": "hardware",
      "domains": ["counter_uas", "air_defense"],
      "readiness_level": "prototype",
      "evidence": ["demo_completed", "partnership_announced"],
      "confidence_score": 65
    }
  ],

  "operational_domains": [...],
  "technical_specifications": {...},
  "competitive_positioning": {...},
  "contract_execution": {...},
  "development_trajectory": {...},
  "validation_warnings": [...]
}
```

See `templates/schema.json` for the complete schema definition.

## Validation Rules

The tool automatically flags:

1. **Capability-Evidence Mismatch**: High readiness level without delivery evidence
2. **Timeline Red Flags**: Unrealistic timelines for early-stage tech
3. **Vague Competitive Claims**: "10x better" without metrics
4. **Missing Critical Info**: No contract vehicles or past performance
5. **Contradictory Claims**: Deck says "fielded" but trajectory shows "prototype"

Warnings are shown in the markdown review and included in the JSON profile.

## Output Files

### JSON Profile

Structured data suitable for:
- Database ingestion
- Automated matching against opportunities
- API integration
- Further processing

### Markdown Review

Human-readable summary with:
- Quick assessment and readiness level
- Detailed capability descriptions
- Competitive positioning analysis
- Validation warnings
- Space for user review notes

## Workflow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. INGEST                                                   │
│    Parse PDF/PPTX → Extract text & images                  │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. EXTRACT                                                  │
│    LLM analyzes content → Structured capability profile     │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. VALIDATE & SCORE                                         │
│    Evidence scoring → Confidence scores → Warning flags     │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. REVIEW                                                   │
│    Human reviews → Edits JSON/Markdown → Adds notes        │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. APPROVE                                                  │
│    Move to approved/ → Log corrections → Training data      │
└─────────────────────────────────────────────────────────────┘
```

## Directory Structure

```
GovBDRadar/
├── ingest.py                  # Main CLI entry point
├── config.yaml                # Configuration
├── requirements.txt           # Python dependencies
│
├── parsers/                   # Document parsing
│   ├── pdf_parser.py          # PDF extraction
│   └── pptx_parser.py         # PowerPoint extraction
│
├── extractors/                # LLM-based extraction
│   └── capability_extractor.py
│
├── validators/                # Evidence scoring & validation
│   └── evidence_scorer.py
│
├── output_generator.py        # JSON & Markdown generation
│
├── templates/                 # Templates
│   ├── schema.json            # Capability taxonomy
│   └── review_template.md     # Markdown template
│
└── data/                      # Data storage
    ├── pending/               # Unreviewed extractions
    ├── approved/              # Approved profiles
    └── training/              # Corrections for training
```

## Advanced Usage

### Update Existing Profile

Add a new document to an existing company profile:

```bash
python ingest.py ingest \
  --file "Thor_Capability_Statement_2025.pdf" \
  --company "Thor Dynamics" \
  --relationship "portfolio"
```

Review and approve to create a versioned update.

### Custom Evidence Weights

Adjust evidence scoring in `config.yaml`:

```yaml
extraction:
  evidence_weights:
    contract_delivered: 50  # Higher weight
    claimed_only: -10       # Negative weight for unsupported claims
```

### Disable Image Analysis

To speed up processing or reduce API costs:

```yaml
document_processing:
  extract_images: false
```

### Custom Templates

Edit `templates/review_template.md` to customize the markdown output format.

## Troubleshooting

### "Insufficient content" Error

**Cause**: PDF may be image-based or corrupted.

**Solution**:
- Enable OCR: Set `ocr_enabled: true` in config
- Install Tesseract: `sudo apt-get install tesseract-ocr`
- Verify PDF is not password-protected

### "No text content extracted"

**Cause**: Document is all images or has text extraction issues.

**Solution**:
- Check document manually
- Try converting to a different format
- Use OCR-enabled scanning

### "Anthropic API key not configured"

**Solution**:
```bash
export ANTHROPIC_API_KEY="your-key-here"
```

Or set directly in `config.yaml`.

## Development

### Running Tests

```bash
pytest
```

### Code Formatting

```bash
black .
```

### Adding New Evidence Types

1. Add to schema: `templates/schema.json`
2. Update weights: `config.yaml`
3. Update extraction prompt: `extractors/capability_extractor.py`

## Roadmap

- [ ] Web interface for review workflow
- [ ] Batch processing for multiple documents
- [ ] Integration with CRM systems
- [ ] Fine-tuned models using correction data
- [ ] SAM.gov integration for CAGE/DUNS lookup
- [ ] Opportunity matching engine
- [ ] Version comparison and diff views

## Contributing

Contributions welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## License

MIT License - See LICENSE file for details

## Support

For issues, questions, or feature requests:
- GitHub Issues: https://github.com/timinator87/GovBDRadar/issues
- Email: [your-email]

## Acknowledgments

Built with:
- [Claude by Anthropic](https://anthropic.com) - LLM extraction
- [PyMuPDF](https://pymupdf.readthedocs.io/) - PDF parsing
- [python-pptx](https://python-pptx.readthedocs.io/) - PowerPoint parsing
- [Click](https://click.palletsprojects.com/) - CLI framework
- [Rich](https://rich.readthedocs.io/) - Beautiful terminal output
