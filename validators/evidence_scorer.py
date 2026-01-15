"""Evidence scoring and validation logic for capability claims."""

import logging
import re
from typing import Dict, List, Tuple

logger = logging.getLogger(__name__)


class EvidenceScorer:
    """
    Scores evidence strength for capability claims and validates profiles.

    Applies automated checks to flag potential issues:
    - Capability-evidence mismatches
    - Timeline red flags
    - Vague competitive claims
    - Missing critical information
    - Contradictory claims
    """

    # Evidence weights for confidence scoring
    DEFAULT_EVIDENCE_WEIGHTS = {
        "contract_delivered": 40,
        "patent_granted": 30,
        "third_party_validated": 20,
        "demo_completed": 15,
        "partnership_announced": 10,
        "claimed_only": 0
    }

    # Red flag phrases that reduce confidence
    HEDGING_PHRASES = [
        "up to", "as much as", "can achieve", "targeting", "planning to",
        "will be able to", "roadmap", "potential", "expected to"
    ]

    # Confidence phrases that increase trust
    CONFIDENCE_PHRASES = [
        "delivered to", "under contract", "fielded by", "operational with",
        "deployed at", "in production"
    ]

    def __init__(self, evidence_weights: Dict[str, int] = None):
        """
        Initialize evidence scorer.

        Args:
            evidence_weights: Custom weights for evidence types
        """
        self.evidence_weights = evidence_weights or self.DEFAULT_EVIDENCE_WEIGHTS

    def score_capability(self, capability: Dict) -> int:
        """
        Calculate confidence score for a capability based on evidence.

        Args:
            capability: Capability dict with 'evidence' field

        Returns:
            Confidence score (0-100)
        """
        evidence_list = capability.get("evidence", [])

        if not evidence_list:
            return 0

        # Sum evidence weights
        base_score = sum(
            self.evidence_weights.get(e, 0) for e in evidence_list
        )

        # Cap at 100
        score = min(base_score, 100)

        # Adjust based on readiness level vs evidence match
        readiness = capability.get("readiness_level", "")
        if readiness in ["production_ready", "fielded"]:
            # High readiness should have strong evidence
            if "contract_delivered" not in evidence_list:
                score = max(score - 20, 0)  # Penalty for missing delivery evidence

        # Adjust based on description language
        description = capability.get("description", "")
        if self._contains_hedging_language(description):
            score = max(score - 10, 0)

        if self._contains_confidence_language(description):
            score = min(score + 10, 100)

        return score

    def validate_profile(self, profile: Dict) -> List[str]:
        """
        Validate capability profile and return list of warnings.

        Args:
            profile: Complete capability profile

        Returns:
            List of validation warning messages
        """
        warnings = []

        # Get existing warnings from extraction
        existing_warnings = profile.get("validation_warnings", [])

        # Check 1: Capability-Evidence Mismatch
        capabilities = profile.get("core_capabilities", [])
        for cap in capabilities:
            readiness = cap.get("readiness_level", "")
            evidence = cap.get("evidence", [])

            if readiness in ["production_ready", "fielded"]:
                if "contract_delivered" not in evidence:
                    warnings.append(
                        f"Capability '{cap.get('capability_name')}' marked as {readiness}, "
                        f"but no delivery contracts found"
                    )

        # Check 2: Timeline Red Flags
        dev_trajectory = profile.get("development_trajectory", {})
        current_stage = dev_trajectory.get("current_stage", "")

        if "prototype" in current_stage.lower() or "development" in current_stage.lower():
            for cap in capabilities:
                if cap.get("readiness_level") == "fielded":
                    warnings.append(
                        f"Development stage shows 'prototype/development' but capability "
                        f"'{cap.get('capability_name')}' marked as 'fielded'"
                    )

        # Check 3: Competitive Claims Without Data
        comp_pos = profile.get("competitive_positioning", {})
        differentiation = comp_pos.get("differentiation", "")

        if differentiation and len(differentiation) > 20:  # Non-empty
            has_cost_data = bool(comp_pos.get("cost_advantage"))
            has_perf_data = bool(comp_pos.get("performance_advantage"))
            has_metrics = bool(profile.get("technical_specifications", {}).get("key_metrics"))

            if not (has_cost_data or has_perf_data or has_metrics):
                warnings.append(
                    "Competitive differentiation claimed but no quantitative data provided"
                )

        # Check 4: Missing Critical Information
        contract_exec = profile.get("contract_execution", {})

        if not contract_exec.get("vehicles"):
            warnings.append("No contract vehicles specified")

        if not contract_exec.get("past_performance"):
            warnings.append("No past performance or references provided")

        # Check 5: Low Confidence Capabilities
        low_confidence_caps = [
            cap.get("capability_name")
            for cap in capabilities
            if cap.get("confidence_score", 0) < 30
        ]

        if low_confidence_caps:
            warnings.append(
                f"Low confidence (<30) capabilities: {', '.join(low_confidence_caps)}"
            )

        # Combine with existing warnings (deduplicate)
        all_warnings = list(set(existing_warnings + warnings))

        return all_warnings

    def _contains_hedging_language(self, text: str) -> bool:
        """Check if text contains hedging/uncertain language."""
        text_lower = text.lower()
        return any(phrase in text_lower for phrase in self.HEDGING_PHRASES)

    def _contains_confidence_language(self, text: str) -> bool:
        """Check if text contains confident/proven language."""
        text_lower = text.lower()
        return any(phrase in text_lower for phrase in self.CONFIDENCE_PHRASES)

    def update_confidence_scores(self, profile: Dict) -> Dict:
        """
        Update confidence scores for all capabilities in profile.

        Args:
            profile: Capability profile

        Returns:
            Updated profile with recalculated confidence scores
        """
        capabilities = profile.get("core_capabilities", [])

        for cap in capabilities:
            score = self.score_capability(cap)
            cap["confidence_score"] = score

        profile["core_capabilities"] = capabilities
        return profile

    def generate_validation_report(self, profile: Dict) -> str:
        """
        Generate human-readable validation report.

        Args:
            profile: Capability profile

        Returns:
            Formatted validation report
        """
        warnings = self.validate_profile(profile)

        if not warnings:
            return "✓ No validation warnings"

        report = f"⚠️  {len(warnings)} Validation Warning(s) for {profile.get('company_name')}:\n\n"
        for i, warning in enumerate(warnings, 1):
            report += f"{i}. {warning}\n"

        report += "\nReview these discrepancies before approving profile."

        return report
