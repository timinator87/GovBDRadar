"""LLM-based capability extraction from unstructured documents."""

import json
import logging
from datetime import datetime
from typing import Dict, List, Optional

try:
    import anthropic
except ImportError:
    raise ImportError("anthropic library not installed. Run: pip install anthropic")

logger = logging.getLogger(__name__)


class CapabilityExtractor:
    """
    Extracts structured capability intelligence from document text using LLMs.

    Uses Claude to analyze pitch decks and capability statements, extracting:
    - Technical capabilities and readiness levels
    - Operational domains and use cases
    - Competitive positioning
    - Contract execution capabilities
    - Development trajectory
    """

    def __init__(
        self,
        api_key: str,
        model: str = "claude-sonnet-4-5-20250929",
        temperature: float = 0.1,
        max_tokens: int = 4096
    ):
        """
        Initialize capability extractor.

        Args:
            api_key: Anthropic API key
            model: Claude model to use
            temperature: Sampling temperature (lower = more consistent)
            max_tokens: Maximum tokens in response
        """
        self.client = anthropic.Anthropic(api_key=api_key)
        self.model = model
        self.temperature = temperature
        self.max_tokens = max_tokens

    def extract(
        self,
        document_text: str,
        company_name: str,
        relationship: str,
        document_metadata: Dict,
        previous_extractions: Optional[List[Dict]] = None
    ) -> Dict:
        """
        Extract capability profile from document text.

        Args:
            document_text: Extracted text from document
            company_name: Name of the company
            relationship: Relationship type (portfolio, investment_target, etc.)
            document_metadata: Metadata about the source document
            previous_extractions: Previous extractions for this company (for learning)

        Returns:
            Structured capability profile as dict
        """
        logger.info(f"Extracting capabilities for {company_name}")

        prompt = self._build_extraction_prompt(
            document_text,
            company_name,
            relationship,
            previous_extractions
        )

        try:
            response = self.client.messages.create(
                model=self.model,
                max_tokens=self.max_tokens,
                temperature=self.temperature,
                messages=[
                    {"role": "user", "content": prompt}
                ]
            )

            # Extract JSON from response
            response_text = response.content[0].text
            profile = self._parse_json_response(response_text)

            # Add metadata
            profile["company_name"] = company_name
            profile["last_updated"] = datetime.utcnow().isoformat()
            profile["relationship"] = relationship
            profile["source_documents"] = [{
                "filename": document_metadata.get("filename", "unknown"),
                "date": datetime.utcnow().isoformat(),
                "type": self._infer_document_type(document_metadata)
            }]

            logger.info(f"Successfully extracted profile for {company_name}")
            return profile

        except Exception as e:
            logger.error(f"Extraction failed: {e}")
            raise

    def _build_extraction_prompt(
        self,
        document_text: str,
        company_name: str,
        relationship: str,
        previous_extractions: Optional[List[Dict]] = None
    ) -> str:
        """Build the extraction prompt for Claude."""

        context = ""
        if previous_extractions:
            context = "\n\n**Previous Extraction Context:**\n"
            context += "You previously extracted capabilities for this company. "
            context += "User corrections from previous extractions:\n"
            for prev in previous_extractions[-2:]:  # Last 2 extractions
                if "user_notes" in prev and prev["user_notes"]:
                    context += f"- {prev['user_notes']}\n"

        prompt = f"""You are analyzing a document for **{company_name}**, a company with relationship type: {relationship}.

Your task is to extract structured capability intelligence from their pitch deck or capability statement.

**Document Content:**
{document_text}

{context}

**EXTRACTION INSTRUCTIONS:**

Analyze this document and extract the following information in a structured JSON format:

1. **Core Capabilities**: List all technical capabilities mentioned. For each:
   - capability_name: Clear, concise name
   - description: 2-3 sentence explanation
   - technical_category: "hardware", "software", "integration", or "service"
   - domains: Array of relevant domains (air_defense, counter_uas, maritime, cyber, space, logistics, etc.)
   - readiness_level: "concept", "prototype", "production_ready", or "fielded"
   - evidence: Array from [contract_delivered, patent_granted, third_party_validated, demo_completed, partnership_announced, claimed_only]
   - confidence_score: 0-100 based on evidence strength

2. **Operational Domains**: Where do they operate?
   - domain: Specific operational area
   - use_cases: Concrete scenarios where capability applies
   - customer_segments: ["DoD", "allied_nations", "federal_civilian", "commercial"]

3. **Technical Specifications**:
   - key_metrics: Array of {{metric, value, unit}} for quantitative claims
   - integration_requirements: "standalone", "requires_platform", "software_only", or "turnkey_system"
   - deployment_timeline: Time from contract to deployment

4. **Competitive Positioning**:
   - primary_competitors: Companies they compare against
   - differentiation: How they claim to be different
   - cost_advantage: Specific cost claims
   - performance_advantage: Specific performance claims

5. **Contract Execution**:
   - vehicles: Contract vehicles they can use (SBIR, OTA, FAR_Part_12, IDIQ, GSA_Schedule, etc.)
   - past_performance: Specific contracts/awards mentioned
   - security_clearances: Facility clearance level and personnel count
   - cage_code, duns, uei: If mentioned

6. **Development Trajectory**:
   - current_stage: Where they are now
   - 12_month_outlook: Where they could be with support
   - key_milestones: Upcoming milestones
   - technical_risks: Identified risks or unknowns

**EVIDENCE SCORING GUIDELINES:**

- **High Confidence (80-100)**: Backed by delivered contracts, published metrics, third-party validation
- **Medium Confidence (50-79)**: Backed by partnerships, ongoing demos, credible customer pipeline
- **Low Confidence (20-49)**: Mentioned but no supporting evidence, vague language
- **Very Low (0-19)**: Contradicted by other info or implausible given stage

**RED FLAGS** (note these but still extract the capability):
- Future tense: "targeting", "planning to", "will be able to"
- Hedging: "up to", "as much as", "can achieve"
- Vague claims without metrics
- High readiness claims without delivery evidence

**OUTPUT FORMAT:**

Return ONLY valid JSON matching this structure (no markdown formatting, no code blocks):

{{
  "core_capabilities": [...],
  "operational_domains": [...],
  "technical_specifications": {{}},
  "competitive_positioning": {{}},
  "contract_execution": {{}},
  "development_trajectory": {{}},
  "validation_warnings": []
}}

If information is not available for a section, use empty arrays [] or objects {{}}.

Add any validation warnings to the "validation_warnings" array.

**CRITICAL**: Your response must be ONLY the JSON object, starting with {{ and ending with }}. No other text.
"""

        return prompt

    def _parse_json_response(self, response_text: str) -> Dict:
        """Parse JSON from Claude's response."""
        # Try to extract JSON from response
        response_text = response_text.strip()

        # Remove markdown code blocks if present
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            # Remove first line (```json or ```)
            lines = lines[1:]
            # Remove last line (```)
            if lines[-1].strip() == "```":
                lines = lines[:-1]
            response_text = "\n".join(lines)

        try:
            return json.loads(response_text)
        except json.JSONDecodeError as e:
            logger.error(f"Failed to parse JSON response: {e}")
            logger.debug(f"Response text: {response_text[:500]}...")
            raise ValueError(f"LLM returned invalid JSON: {e}")

    def _infer_document_type(self, metadata: Dict) -> str:
        """Infer document type from metadata."""
        filename = metadata.get("filename", "").lower()

        if "pitch" in filename or "deck" in filename:
            return "pitch_deck"
        elif "capability" in filename or "statement" in filename:
            return "capability_statement"
        elif "white" in filename or "paper" in filename:
            return "white_paper"
        elif metadata.get("slide_count"):
            return "pitch_deck"
        else:
            return "capability_statement"

    def analyze_images(
        self,
        images: List[Dict],
        api_key: Optional[str] = None
    ) -> List[Dict]:
        """
        Analyze images using Claude's vision capabilities.

        Args:
            images: List of image dicts with 'bytes' field
            api_key: Optional separate API key for vision

        Returns:
            Images with 'description' field populated
        """
        import base64

        client = anthropic.Anthropic(api_key=api_key or self.client.api_key)

        for i, img in enumerate(images):
            if not img.get("bytes"):
                continue

            try:
                logger.debug(f"Analyzing image {i+1}/{len(images)}")

                # Convert image bytes to base64
                image_b64 = base64.b64encode(img["bytes"]).decode("utf-8")

                # Determine media type
                media_type = f"image/{img.get('format', 'png')}"
                if media_type == "image/jpg":
                    media_type = "image/jpeg"

                response = client.messages.create(
                    model=self.model,
                    max_tokens=300,
                    messages=[{
                        "role": "user",
                        "content": [
                            {
                                "type": "image",
                                "source": {
                                    "type": "base64",
                                    "media_type": media_type,
                                    "data": image_b64
                                }
                            },
                            {
                                "type": "text",
                                "text": (
                                    "Describe this image in 2-3 sentences. "
                                    "Focus on: technical diagrams, product photos, "
                                    "system architectures, performance charts, or key data. "
                                    "If it's just decorative or generic, say 'decorative image'."
                                )
                            }
                        ]
                    }]
                )

                img["description"] = response.content[0].text

            except Exception as e:
                logger.warning(f"Failed to analyze image {i}: {e}")
                img["description"] = "Failed to analyze"

        return images
