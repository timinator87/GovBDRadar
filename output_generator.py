"""Output generation for capability profiles (JSON and Markdown)."""

import json
import logging
from datetime import datetime
from pathlib import Path
from typing import Dict, List

logger = logging.getLogger(__name__)


class OutputGenerator:
    """
    Generates human-readable markdown summaries and structured JSON profiles.
    """

    def __init__(self, template_path: Path):
        """
        Initialize output generator.

        Args:
            template_path: Path to markdown template file
        """
        self.template_path = template_path
        with open(template_path, 'r') as f:
            self.template = f.read()

    def generate_json(self, profile: Dict, output_path: Path) -> Path:
        """
        Save profile as JSON file.

        Args:
            profile: Capability profile dict
            output_path: Path to save JSON file

        Returns:
            Path to saved file
        """
        output_path.parent.mkdir(parents=True, exist_ok=True)

        with open(output_path, 'w') as f:
            json.dump(profile, f, indent=2)

        logger.info(f"JSON profile saved to {output_path}")
        return output_path

    def generate_markdown(self, profile: Dict, output_path: Path, source_text: str = None) -> Path:
        """
        Generate markdown review document from profile.

        Args:
            profile: Capability profile dict
            output_path: Path to save markdown file
            source_text: Original document text for source extracts

        Returns:
            Path to saved file
        """
        output_path.parent.mkdir(parents=True, exist_ok=True)

        # Fill template with profile data
        markdown = self._populate_template(profile, source_text)

        with open(output_path, 'w') as f:
            f.write(markdown)

        logger.info(f"Markdown review saved to {output_path}")
        return output_path

    def _populate_template(self, profile: Dict, source_text: str = None) -> str:
        """Populate markdown template with profile data."""

        # Header info
        company_name = profile.get("company_name", "Unknown")
        timestamp = datetime.fromisoformat(profile.get("last_updated", datetime.utcnow().isoformat()))
        source_docs = profile.get("source_documents", [])
        source_file = source_docs[0].get("filename", "Unknown") if source_docs else "Unknown"

        # Quick assessment
        core_capability_summary = self._generate_capability_summary(profile)
        overall_readiness = self._assess_overall_readiness(profile)
        best_fit_opportunities = self._suggest_opportunities(profile)

        # Detailed sections
        capabilities_section = self._format_capabilities(profile.get("core_capabilities", []))
        operational_domains_section = self._format_operational_domains(profile.get("operational_domains", []))
        technical_specs_section = self._format_technical_specs(profile.get("technical_specifications", {}))
        competitive_positioning_section = self._format_competitive_positioning(profile.get("competitive_positioning", {}))

        # Execution readiness
        contract_exec = profile.get("contract_execution", {})
        contract_vehicles = ", ".join(contract_exec.get("vehicles", [])) or "Not specified"
        past_performance = self._format_past_performance(contract_exec.get("past_performance", []))
        security_clearances = self._format_security_clearances(contract_exec.get("security_clearances", {}))

        # Development trajectory
        dev_traj = profile.get("development_trajectory", {})
        current_stage = dev_traj.get("current_stage", "Not specified")
        twelve_month_outlook = dev_traj.get("12_month_outlook", "Not specified")
        key_milestones = self._format_list(dev_traj.get("key_milestones", []))
        technical_risks = self._format_list(dev_traj.get("technical_risks", []))
        deployment_timeline = profile.get("technical_specifications", {}).get("deployment_timeline", "Not specified")

        # Validation warnings
        warnings = profile.get("validation_warnings", [])
        validation_warnings = self._format_warnings(warnings)

        # User notes
        user_notes = profile.get("user_notes", "")

        # Source extracts (if source text provided)
        source_extracts = self._generate_source_extracts(profile, source_text) if source_text else "Source text not available"

        # Fill template
        markdown = self.template.format(
            company_name=company_name,
            timestamp=timestamp.strftime("%Y-%m-%d %H:%M UTC"),
            source_file=source_file,
            core_capability_summary=core_capability_summary,
            overall_readiness=overall_readiness,
            best_fit_opportunities=best_fit_opportunities,
            capabilities_section=capabilities_section,
            operational_domains_section=operational_domains_section,
            technical_specs_section=technical_specs_section,
            competitive_positioning_section=competitive_positioning_section,
            contract_vehicles=contract_vehicles,
            past_performance=past_performance,
            security_clearances=security_clearances,
            deployment_timeline=deployment_timeline,
            current_stage=current_stage,
            twelve_month_outlook=twelve_month_outlook,
            key_milestones=key_milestones,
            technical_risks=technical_risks,
            validation_warnings=validation_warnings,
            user_notes=user_notes,
            source_extracts=source_extracts
        )

        return markdown

    def _generate_capability_summary(self, profile: Dict) -> str:
        """Generate one-sentence capability summary."""
        capabilities = profile.get("core_capabilities", [])

        if not capabilities:
            return "No capabilities extracted"

        # Get top 2-3 capabilities by confidence
        top_caps = sorted(
            capabilities,
            key=lambda c: c.get("confidence_score", 0),
            reverse=True
        )[:3]

        cap_names = [c.get("capability_name") for c in top_caps if c.get("capability_name")]

        if len(cap_names) == 1:
            return cap_names[0]
        elif len(cap_names) == 2:
            return f"{cap_names[0]} and {cap_names[1]}"
        else:
            return f"{', '.join(cap_names[:-1])}, and {cap_names[-1]}"

    def _assess_overall_readiness(self, profile: Dict) -> str:
        """Assess overall readiness level."""
        capabilities = profile.get("core_capabilities", [])

        if not capabilities:
            return "Unknown"

        readiness_levels = [c.get("readiness_level") for c in capabilities]

        if "fielded" in readiness_levels:
            return "Fielded (some capabilities operational)"
        elif "production_ready" in readiness_levels:
            return "Production Ready"
        elif "prototype" in readiness_levels:
            return "Prototype Stage"
        else:
            return "Concept/Early Development"

    def _suggest_opportunities(self, profile: Dict) -> str:
        """Suggest types of opportunities this company can pursue."""
        domains = profile.get("operational_domains", [])
        vehicles = profile.get("contract_execution", {}).get("vehicles", [])

        if not domains:
            return "Unknown - review operational domains"

        domain_names = [d.get("domain") for d in domains if d.get("domain")]
        vehicle_names = vehicles if vehicles else ["traditional FAR-based contracts"]

        opp_text = f"{', '.join(domain_names[:3])} opportunities via {', '.join(vehicle_names[:2])}"
        return opp_text

    def _format_capabilities(self, capabilities: List[Dict]) -> str:
        """Format capabilities section."""
        if not capabilities:
            return "*No capabilities extracted*"

        sections = []
        for cap in capabilities:
            name = cap.get("capability_name", "Unnamed")
            desc = cap.get("description", "No description")
            category = cap.get("technical_category", "unknown")
            domains = ", ".join(cap.get("domains", []))
            readiness = cap.get("readiness_level", "unknown")
            evidence = ", ".join(cap.get("evidence", []))
            confidence = cap.get("confidence_score", 0)

            section = f"### {name}\n\n"
            section += f"**Confidence**: {confidence}/100 | **Category**: {category} | **Readiness**: {readiness}\n\n"
            section += f"{desc}\n\n"
            section += f"**Domains**: {domains or 'Not specified'}\n\n"
            section += f"**Evidence**: {evidence or 'Claimed only'}\n"

            sections.append(section)

        return "\n".join(sections)

    def _format_operational_domains(self, domains: List[Dict]) -> str:
        """Format operational domains section."""
        if not domains:
            return "*No operational domains specified*"

        sections = []
        for domain in domains:
            name = domain.get("domain", "Unnamed")
            use_cases = domain.get("use_cases", [])
            customers = domain.get("customer_segments", [])

            section = f"### {name}\n\n"
            if use_cases:
                section += "**Use Cases**:\n"
                for uc in use_cases:
                    section += f"- {uc}\n"
                section += "\n"

            if customers:
                section += f"**Customer Segments**: {', '.join(customers)}\n"

            sections.append(section)

        return "\n".join(sections)

    def _format_technical_specs(self, specs: Dict) -> str:
        """Format technical specifications section."""
        if not specs:
            return "*No technical specifications provided*"

        output = ""

        metrics = specs.get("key_metrics", [])
        if metrics:
            output += "**Key Metrics**:\n"
            for metric in metrics:
                m = metric.get("metric")
                v = metric.get("value")
                u = metric.get("unit", "")
                output += f"- {m}: {v} {u}\n"
            output += "\n"

        integration = specs.get("integration_requirements")
        if integration:
            output += f"**Integration**: {integration}\n\n"

        timeline = specs.get("deployment_timeline")
        if timeline:
            output += f"**Deployment Timeline**: {timeline}\n"

        return output or "*No technical specifications provided*"

    def _format_competitive_positioning(self, comp: Dict) -> str:
        """Format competitive positioning section."""
        if not comp:
            return "*No competitive positioning information*"

        output = ""

        competitors = comp.get("primary_competitors", [])
        if competitors:
            output += f"**Primary Competitors**: {', '.join(competitors)}\n\n"

        diff = comp.get("differentiation")
        if diff:
            output += f"**Differentiation**: {diff}\n\n"

        cost = comp.get("cost_advantage")
        if cost:
            output += f"**Cost Advantage**: {cost}\n\n"

        perf = comp.get("performance_advantage")
        if perf:
            output += f"**Performance Advantage**: {perf}\n"

        return output or "*No competitive positioning information*"

    def _format_past_performance(self, past_perf: List[str]) -> str:
        """Format past performance list."""
        if not past_perf:
            return "None specified"

        return "\n".join(f"- {p}" for p in past_perf)

    def _format_security_clearances(self, clearances: Dict) -> str:
        """Format security clearances."""
        if not clearances:
            return "Not specified"

        facility = clearances.get("facility_clearance_level", "None")
        personnel = clearances.get("personnel_cleared_count", 0)

        return f"Facility: {facility}, Personnel: {personnel}"

    def _format_list(self, items: List[str]) -> str:
        """Format a list of items."""
        if not items:
            return "None specified"

        return "\n".join(f"- {item}" for item in items)

    def _format_warnings(self, warnings: List[str]) -> str:
        """Format validation warnings."""
        if not warnings:
            return "✓ No validation warnings"

        output = f"⚠️  {len(warnings)} warning(s):\n\n"
        for i, warning in enumerate(warnings, 1):
            output += f"{i}. {warning}\n"

        return output

    def _generate_source_extracts(self, profile: Dict, source_text: str) -> str:
        """Generate source extracts showing key claims."""
        # This is a simplified version - could be enhanced to show actual quotes
        output = "*Key claims from source document:*\n\n"

        capabilities = profile.get("core_capabilities", [])[:3]
        for cap in capabilities:
            name = cap.get("capability_name")
            output += f"**{name}**: See source document for details\n\n"

        return output
