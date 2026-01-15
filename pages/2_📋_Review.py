"""
Review Page
Review pending extractions and approve profiles
"""

import streamlit as st
import os
import sys
import json
import shutil
from pathlib import Path
from datetime import datetime

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent.parent))

st.set_page_config(page_title="Review Profiles", page_icon="📋", layout="wide")

st.title("📋 Review Pending Profiles")
st.markdown("Review and approve company capability extractions")

# Get pending profiles
pending_dir = Path("data/pending")
pending_files = sorted(
    list(pending_dir.glob("*.json")),
    key=lambda x: x.stat().st_mtime,
    reverse=True
)

if not pending_files:
    st.info("🎉 No pending profiles! All extractions have been reviewed.")
    st.markdown("---")
    st.markdown("👉 Go to the **Ingest** page to process new documents")
    st.stop()

# Profile selector
st.markdown("---")
st.subheader("📁 Select Profile to Review")

# Create profile options with metadata
profile_options = []
for f in pending_files:
    # Extract company name from filename
    company_name = f.stem.rsplit('_', 1)[0].replace('_', ' ')
    timestamp = datetime.fromtimestamp(f.stat().st_mtime)
    profile_options.append(f"{company_name} - {timestamp.strftime('%Y-%m-%d %H:%M')}")

selected_idx = st.selectbox(
    "Select a profile",
    range(len(profile_options)),
    format_func=lambda i: profile_options[i]
)

selected_file = pending_files[selected_idx]
selected_md = selected_file.with_suffix('.md').with_name(selected_file.stem.replace('.json', '') + '_review.md')

# Load profile
with open(selected_file, 'r') as f:
    profile = json.load(f)

# Load markdown if exists
markdown_content = ""
if selected_md.exists():
    with open(selected_md, 'r') as f:
        markdown_content = f.read()

# Display profile
st.markdown("---")

# Header with company info
col1, col2, col3 = st.columns([2, 1, 1])

with col1:
    st.markdown(f"## {profile.get('company_name', 'Unknown')}")
    st.caption(f"Relationship: {profile.get('relationship', 'Unknown')}")

with col2:
    st.metric("Last Updated", profile.get('last_updated', 'N/A')[:10])

with col3:
    st.metric("Source Docs", len(profile.get('source_documents', [])))

# Tabs for different views
tab1, tab2, tab3, tab4 = st.tabs(["📊 Summary", "📝 Markdown", "🔧 Edit JSON", "⚠️ Warnings"])

# Tab 1: Summary View
with tab1:
    st.subheader("Quick Assessment")

    # Key metrics
    col1, col2, col3, col4 = st.columns(4)

    with col1:
        st.metric("Capabilities", len(profile.get('core_capabilities', [])))

    with col2:
        st.metric("Domains", len(profile.get('operational_domains', [])))

    with col3:
        avg_confidence = 0
        if profile.get('core_capabilities'):
            avg_confidence = sum(
                cap.get('confidence_score', 0)
                for cap in profile['core_capabilities']
            ) / len(profile['core_capabilities'])
        st.metric("Avg Confidence", f"{avg_confidence:.0f}%")

    with col4:
        st.metric("Warnings", len(profile.get('validation_warnings', [])))

    st.markdown("---")

    # Core Capabilities
    st.subheader("🎯 Core Capabilities")

    if profile.get('core_capabilities'):
        for idx, cap in enumerate(profile['core_capabilities']):
            with st.expander(f"{cap['capability_name']}", expanded=(idx == 0)):
                col1, col2 = st.columns([3, 1])

                with col1:
                    st.markdown(f"**Description:** {cap.get('description', 'N/A')}")
                    st.markdown(f"**Category:** {cap.get('technical_category', 'N/A')}")
                    st.markdown(f"**Domains:** {', '.join(cap.get('domains', []))}")
                    st.markdown(f"**Readiness:** {cap.get('readiness_level', 'N/A')}")

                    if cap.get('evidence'):
                        st.markdown(f"**Evidence:** {', '.join(cap['evidence'])}")

                with col2:
                    confidence = cap.get('confidence_score', 0)
                    if confidence >= 70:
                        st.success(f"🟢 {confidence}%")
                    elif confidence >= 40:
                        st.warning(f"🟡 {confidence}%")
                    else:
                        st.error(f"🔴 {confidence}%")

                    st.caption(f"Confidence Score")
    else:
        st.info("No capabilities extracted")

    st.markdown("---")

    # Competitive Positioning
    if profile.get('competitive_positioning'):
        st.subheader("🏆 Competitive Positioning")
        comp = profile['competitive_positioning']

        col1, col2 = st.columns(2)

        with col1:
            if comp.get('primary_competitors'):
                st.markdown("**Primary Competitors:**")
                for competitor in comp['primary_competitors']:
                    st.markdown(f"- {competitor}")

            if comp.get('differentiation'):
                st.markdown("**Differentiation:**")
                st.info(comp['differentiation'])

        with col2:
            if comp.get('cost_advantage'):
                st.markdown("**Cost Advantage:**")
                st.success(comp['cost_advantage'])

            if comp.get('performance_advantage'):
                st.markdown("**Performance Advantage:**")
                st.success(comp['performance_advantage'])

        st.markdown("---")

    # Contract Execution
    if profile.get('contract_execution'):
        st.subheader("📋 Contract Execution")
        contract = profile['contract_execution']

        col1, col2 = st.columns(2)

        with col1:
            if contract.get('vehicles'):
                st.markdown("**Contract Vehicles:**")
                for vehicle in contract['vehicles']:
                    st.markdown(f"- {vehicle}")

            if contract.get('past_performance'):
                st.markdown("**Past Performance:**")
                for perf in contract['past_performance']:
                    st.markdown(f"- {perf}")

        with col2:
            if contract.get('cage_code'):
                st.metric("CAGE Code", contract['cage_code'])

            if contract.get('duns'):
                st.metric("DUNS", contract['duns'])

            if contract.get('security_clearances'):
                st.markdown("**Security Clearances:**")
                for clearance in contract['security_clearances']:
                    st.markdown(f"- {clearance}")

# Tab 2: Markdown View
with tab2:
    if markdown_content:
        st.markdown(markdown_content)
    else:
        st.info("No markdown review file found")

# Tab 3: JSON Editor
with tab3:
    st.markdown("Edit the JSON profile directly. Changes will be saved when you approve.")

    edited_json = st.text_area(
        "JSON Content",
        value=json.dumps(profile, indent=2),
        height=600,
        key="json_editor"
    )

    if st.button("💾 Save JSON Changes"):
        try:
            # Validate JSON
            updated_profile = json.loads(edited_json)

            # Save back to file
            with open(selected_file, 'w') as f:
                json.dump(updated_profile, f, indent=2)

            st.success("✅ JSON changes saved!")
            st.rerun()

        except json.JSONDecodeError as e:
            st.error(f"❌ Invalid JSON: {str(e)}")

# Tab 4: Warnings
with tab4:
    st.subheader("⚠️ Validation Warnings")

    if profile.get('validation_warnings'):
        for idx, warning in enumerate(profile['validation_warnings'], 1):
            st.warning(f"**{idx}.** {warning}")
    else:
        st.success("✅ No validation warnings! This profile looks good.")

    st.markdown("---")
    st.markdown("**Common Issues to Check:**")
    st.markdown("""
    - ✓ Capability readiness levels match evidence
    - ✓ Competitive claims are backed by data
    - ✓ Timeline claims are realistic
    - ✓ Contract vehicles and past performance are documented
    - ✓ Technical specifications are complete
    """)

# Approval Section
st.markdown("---")
st.subheader("✅ Approve Profile")

col1, col2 = st.columns([2, 1])

with col1:
    approval_notes = st.text_area(
        "Approval Notes (optional)",
        placeholder="Add any corrections, observations, or notes about this extraction...",
        height=100,
        help="These notes will be saved for training data"
    )

with col2:
    st.markdown("### Actions")

    if st.button("✅ Approve Profile", type="primary", use_container_width=True):
        try:
            # Create approved directory if it doesn't exist
            approved_dir = Path("data/approved")
            approved_dir.mkdir(parents=True, exist_ok=True)

            training_dir = Path("data/training")
            training_dir.mkdir(parents=True, exist_ok=True)

            # Move files to approved
            approved_json = approved_dir / selected_file.name
            shutil.move(str(selected_file), str(approved_json))

            if selected_md.exists():
                approved_md = approved_dir / selected_md.name
                shutil.move(str(selected_md), str(approved_md))

            # Save approval notes if provided
            if approval_notes:
                training_file = training_dir / f"{selected_file.stem}_corrections.txt"
                with open(training_file, 'w') as f:
                    f.write(f"Company: {profile.get('company_name')}\n")
                    f.write(f"Approved: {datetime.now().isoformat()}\n")
                    f.write(f"Notes:\n{approval_notes}\n")

            st.success(f"✅ Profile approved and moved to {approved_json}")
            st.balloons()

            # Wait a moment then rerun
            import time
            time.sleep(1)
            st.rerun()

        except Exception as e:
            st.error(f"❌ Error approving profile: {str(e)}")

    if st.button("🗑️ Delete Profile", use_container_width=True):
        if st.session_state.get('confirm_delete'):
            try:
                selected_file.unlink()
                if selected_md.exists():
                    selected_md.unlink()

                st.success("🗑️ Profile deleted")
                st.rerun()

            except Exception as e:
                st.error(f"❌ Error deleting profile: {str(e)}")

            st.session_state.confirm_delete = False
        else:
            st.session_state.confirm_delete = True
            st.warning("⚠️ Click again to confirm deletion")

# Download buttons
st.markdown("---")
col1, col2 = st.columns(2)

with col1:
    st.download_button(
        "⬇️ Download JSON",
        data=json.dumps(profile, indent=2),
        file_name=selected_file.name,
        mime="application/json",
        use_container_width=True
    )

with col2:
    if markdown_content:
        st.download_button(
            "⬇️ Download Markdown",
            data=markdown_content,
            file_name=selected_md.name,
            mime="text/markdown",
            use_container_width=True
        )
