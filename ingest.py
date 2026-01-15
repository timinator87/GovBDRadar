#!/usr/bin/env python3
"""
Company Intelligence Ingestion Tool

Main CLI for extracting capability intelligence from pitch decks and capability statements.
"""

import json
import logging
import os
import sys
from datetime import datetime
from pathlib import Path
from typing import Optional

import click
import yaml
from rich.console import Console
from rich.logging import RichHandler
from rich.panel import Panel
from rich.table import Table

# Import local modules
from parsers import PDFParser, PPTXParser
from extractors import CapabilityExtractor
from validators import EvidenceScorer
from output_generator import OutputGenerator

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format="%(message)s",
    handlers=[RichHandler(rich_tracebacks=True)]
)
logger = logging.getLogger(__name__)
console = Console()


def load_config(config_path: Path = Path("config.yaml")) -> dict:
    """Load configuration from YAML file."""
    if not config_path.exists():
        logger.error(f"Configuration file not found: {config_path}")
        sys.exit(1)

    with open(config_path, 'r') as f:
        config = yaml.safe_load(f)

    # Expand environment variables in API keys
    api_keys = config.get("api_keys", {})
    for key, value in api_keys.items():
        if isinstance(value, str) and value.startswith("${") and value.endswith("}"):
            env_var = value[2:-1]
            api_keys[key] = os.environ.get(env_var, "")

    return config


def parse_document(file_path: Path, config: dict):
    """Parse PDF or PPTX document."""
    suffix = file_path.suffix.lower()

    if suffix == ".pdf":
        parser = PDFParser(
            ocr_enabled=config.get("document_processing", {}).get("ocr_enabled", True),
            extract_images=config.get("document_processing", {}).get("extract_images", True)
        )
    elif suffix == ".pptx":
        parser = PPTXParser(
            extract_images=config.get("document_processing", {}).get("extract_images", True)
        )
    else:
        raise ValueError(f"Unsupported file format: {suffix}. Use .pdf or .pptx")

    console.print(f"[cyan]Parsing {file_path.name}...[/cyan]")
    parsed_doc = parser.parse(file_path)

    # Validate document
    min_words = config.get("document_processing", {}).get("min_text_length", 100)
    is_valid, error_msg = parser.validate_document(parsed_doc, min_words)

    if not is_valid:
        raise ValueError(error_msg)

    console.print(f"[green]✓[/green] Extracted {parsed_doc.word_count} words from {len(parsed_doc.pages)} pages/slides")

    return parsed_doc


def extract_capabilities(parsed_doc, company_name: str, relationship: str, config: dict):
    """Extract capability profile using LLM."""
    api_key = config.get("api_keys", {}).get("anthropic")
    if not api_key:
        raise ValueError("Anthropic API key not configured. Set ANTHROPIC_API_KEY environment variable.")

    llm_config = config.get("llm", {})
    extractor = CapabilityExtractor(
        api_key=api_key,
        model=llm_config.get("model", "claude-sonnet-4-5-20250929"),
        temperature=llm_config.get("temperature", 0.1),
        max_tokens=llm_config.get("max_tokens", 4096)
    )

    console.print(f"[cyan]Extracting capabilities for {company_name}...[/cyan]")
    profile = extractor.extract(
        document_text=parsed_doc.text,
        company_name=company_name,
        relationship=relationship,
        document_metadata=parsed_doc.metadata
    )

    # Analyze images if available
    if parsed_doc.images and config.get("document_processing", {}).get("extract_images", True):
        console.print(f"[cyan]Analyzing {len(parsed_doc.images)} images...[/cyan]")
        try:
            parsed_doc.images = extractor.analyze_images(parsed_doc.images, api_key)
        except Exception as e:
            logger.warning(f"Image analysis failed: {e}")

    console.print(f"[green]✓[/green] Extracted {len(profile.get('core_capabilities', []))} capabilities")

    return profile, parsed_doc


def validate_and_score(profile: dict, config: dict):
    """Validate profile and score evidence."""
    evidence_weights = config.get("extraction", {}).get("evidence_weights", {})
    scorer = EvidenceScorer(evidence_weights=evidence_weights)

    console.print("[cyan]Validating profile and scoring evidence...[/cyan]")

    # Update confidence scores
    profile = scorer.update_confidence_scores(profile)

    # Validate and get warnings
    warnings = scorer.validate_profile(profile)
    profile["validation_warnings"] = warnings

    if warnings:
        console.print(f"[yellow]⚠[/yellow]  {len(warnings)} validation warnings")
    else:
        console.print("[green]✓[/green] No validation warnings")

    return profile


def generate_outputs(profile: dict, parsed_doc, company_name: str, config: dict):
    """Generate JSON and Markdown outputs."""
    paths = config.get("paths", {})
    pending_dir = Path(paths.get("pending", "data/pending"))
    templates_dir = Path(paths.get("templates", "templates"))

    # Create timestamped filenames
    timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
    safe_name = company_name.replace(" ", "_").replace("/", "_")

    json_path = pending_dir / f"{safe_name}_{timestamp}.json"
    md_path = pending_dir / f"{safe_name}_{timestamp}_review.md"

    # Generate outputs
    generator = OutputGenerator(template_path=templates_dir / "review_template.md")

    console.print("[cyan]Generating outputs...[/cyan]")

    generator.generate_json(profile, json_path)
    generator.generate_markdown(profile, md_path, parsed_doc.text)

    console.print(f"[green]✓[/green] JSON profile: {json_path}")
    console.print(f"[green]✓[/green] Markdown review: {md_path}")

    return json_path, md_path


@click.group()
def cli():
    """Company Intelligence Ingestion Tool"""
    pass


@cli.command()
@click.option('--file', required=True, type=click.Path(exists=True), help='Path to PDF or PPTX file')
@click.option('--company', required=True, help='Company name')
@click.option('--relationship', type=click.Choice(['portfolio', 'investment_target', 'partner', 'competitor']),
              default='investment_target', help='Relationship type')
@click.option('--config', type=click.Path(), default='config.yaml', help='Config file path')
def ingest(file: str, company: str, relationship: str, config: str):
    """Ingest a new company document and extract capabilities."""

    console.print(Panel.fit(
        f"[bold cyan]Company Intelligence Ingestion[/bold cyan]\n\n"
        f"Company: {company}\n"
        f"File: {file}\n"
        f"Relationship: {relationship}",
        border_style="cyan"
    ))

    try:
        # Load configuration
        cfg = load_config(Path(config))

        # Parse document
        file_path = Path(file)
        parsed_doc = parse_document(file_path, cfg)

        # Extract capabilities
        profile, parsed_doc = extract_capabilities(parsed_doc, company, relationship, cfg)

        # Validate and score
        profile = validate_and_score(profile, cfg)

        # Generate outputs
        json_path, md_path = generate_outputs(profile, parsed_doc, company, cfg)

        # Show summary
        console.print("\n" + "="*60)
        console.print("[bold green]Extraction Complete![/bold green]\n")

        # Display validation report
        scorer = EvidenceScorer()
        report = scorer.generate_validation_report(profile)
        console.print(report)

        console.print(f"\n[bold]Next Steps:[/bold]")
        console.print(f"1. Review the markdown file: {md_path}")
        console.print(f"2. Edit JSON if needed: {json_path}")
        console.print(f"3. Approve profile: [cyan]python ingest.py approve --company \"{company}\"[/cyan]")

    except Exception as e:
        console.print(f"[bold red]Error:[/bold red] {e}", style="red")
        logger.exception("Ingestion failed")
        sys.exit(1)


@cli.command()
@click.option('--company', required=True, help='Company name to review')
@click.option('--config', type=click.Path(), default='config.yaml', help='Config file path')
def review(company: str, config: str):
    """Review pending extraction for a company."""

    cfg = load_config(Path(config))
    pending_dir = Path(cfg.get("paths", {}).get("pending", "data/pending"))

    # Find latest extraction for company
    safe_name = company.replace(" ", "_").replace("/", "_")
    json_files = list(pending_dir.glob(f"{safe_name}_*.json"))

    if not json_files:
        console.print(f"[red]No pending extractions found for: {company}[/red]")
        sys.exit(1)

    # Get most recent
    latest_json = sorted(json_files)[-1]
    latest_md = latest_json.with_suffix("").with_suffix("_review.md")

    console.print(f"[cyan]Latest extraction:[/cyan] {latest_json.name}\n")

    # Load and display profile
    with open(latest_json, 'r') as f:
        profile = json.load(f)

    # Display summary table
    table = Table(title=f"Capability Profile: {company}")
    table.add_column("Field", style="cyan")
    table.add_column("Value", style="white")

    table.add_row("Last Updated", profile.get("last_updated", "Unknown"))
    table.add_row("Relationship", profile.get("relationship", "Unknown"))
    table.add_row("Capabilities", str(len(profile.get("core_capabilities", []))))
    table.add_row("Warnings", str(len(profile.get("validation_warnings", []))))

    console.print(table)

    # Show markdown path
    console.print(f"\n[bold]Review document:[/bold] {latest_md}")
    console.print(f"[bold]JSON profile:[/bold] {latest_json}")


@cli.command()
@click.option('--company', required=True, help='Company name to approve')
@click.option('--notes', help='User notes/corrections')
@click.option('--config', type=click.Path(), default='config.yaml', help='Config file path')
def approve(company: str, notes: str, config: str):
    """Approve a capability profile and move to approved directory."""

    cfg = load_config(Path(config))
    pending_dir = Path(cfg.get("paths", {}).get("pending", "data/pending"))
    approved_dir = Path(cfg.get("paths", {}).get("approved", "data/approved"))
    training_dir = Path(cfg.get("paths", {}).get("training", "data/training"))

    # Find latest extraction
    safe_name = company.replace(" ", "_").replace("/", "_")
    json_files = list(pending_dir.glob(f"{safe_name}_*.json"))

    if not json_files:
        console.print(f"[red]No pending extractions found for: {company}[/red]")
        sys.exit(1)

    latest_json = sorted(json_files)[-1]
    latest_md = latest_json.with_suffix("").name + "_review.md"
    latest_md_path = pending_dir / latest_md

    # Load profile
    with open(latest_json, 'r') as f:
        profile = json.load(f)

    # Add user notes if provided
    if notes:
        profile["user_notes"] = notes

    # Move to approved
    approved_json = approved_dir / latest_json.name
    approved_md = approved_dir / latest_md

    generator = OutputGenerator(template_path=Path(cfg.get("paths", {}).get("templates", "templates")) / "review_template.md")
    generator.generate_json(profile, approved_json)

    if latest_md_path.exists():
        import shutil
        shutil.copy(latest_md_path, approved_md)

    # Log corrections to training data
    if notes:
        training_log = training_dir / "corrections.jsonl"
        training_log.parent.mkdir(parents=True, exist_ok=True)

        correction_entry = {
            "company": company,
            "timestamp": datetime.utcnow().isoformat(),
            "notes": notes,
            "profile": profile
        }

        with open(training_log, 'a') as f:
            f.write(json.dumps(correction_entry) + "\n")

    console.print(f"[green]✓[/green] Profile approved and moved to: {approved_json}")
    console.print(f"[green]✓[/green] Review document: {approved_md}")

    if notes:
        console.print(f"[green]✓[/green] Corrections logged for training")


@cli.command()
@click.option('--company', help='Company name (optional, lists all if not provided)')
@click.option('--config', type=click.Path(), default='config.yaml', help='Config file path')
def list(company: str, config: str):
    """List pending and approved profiles."""

    cfg = load_config(Path(config))
    pending_dir = Path(cfg.get("paths", {}).get("pending", "data/pending"))
    approved_dir = Path(cfg.get("paths", {}).get("approved", "data/approved"))

    # Pending profiles
    pending_files = list(pending_dir.glob("*.json"))
    approved_files = list(approved_dir.glob("*.json"))

    if company:
        safe_name = company.replace(" ", "_").replace("/", "_")
        pending_files = [f for f in pending_files if safe_name in f.name]
        approved_files = [f for f in approved_files if safe_name in f.name]

    console.print(f"\n[bold]Pending Profiles:[/bold] {len(pending_files)}")
    for f in sorted(pending_files):
        console.print(f"  - {f.name}")

    console.print(f"\n[bold]Approved Profiles:[/bold] {len(approved_files)}")
    for f in sorted(approved_files):
        console.print(f"  - {f.name}")


if __name__ == "__main__":
    cli()
