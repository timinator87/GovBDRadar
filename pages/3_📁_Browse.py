"""
Browse Page
Browse and search all company profiles (approved and pending)
"""

import streamlit as st
import os
import sys
import json
import pandas as pd
from pathlib import Path
from datetime import datetime

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent.parent))

st.set_page_config(page_title="Browse Profiles", page_icon="📁", layout="wide")

st.title("📁 Browse Company Profiles")
st.markdown("Search and view all company intelligence profiles")

# Load all profiles
def load_profiles(directory):
    """Load all JSON profiles from a directory"""
    profiles = []
    json_files = Path(directory).glob("*.json")

    for json_file in json_files:
        try:
            with open(json_file, 'r') as f:
                profile = json.load(f)
                profile['_source_file'] = str(json_file)
                profile['_status'] = 'approved' if 'approved' in str(json_file) else 'pending'
                profile['_modified'] = datetime.fromtimestamp(json_file.stat().st_mtime)
                profiles.append(profile)
        except Exception as e:
            st.warning(f"Error loading {json_file.name}: {str(e)}")

    return profiles

# Load all profiles
pending_profiles = load_profiles("data/pending")
approved_profiles = load_profiles("data/approved")
all_profiles = pending_profiles + approved_profiles

if not all_profiles:
    st.info("📭 No profiles found. Process some documents first!")
    st.stop()

# Statistics
st.markdown("---")
col1, col2, col3, col4 = st.columns(4)

with col1:
    st.metric("Total Companies", len(all_profiles))

with col2:
    st.metric("Approved", len(approved_profiles))

with col3:
    st.metric("Pending", len(pending_profiles))

with col4:
    total_capabilities = sum(len(p.get('core_capabilities', [])) for p in all_profiles)
    st.metric("Total Capabilities", total_capabilities)

# Filters
st.markdown("---")
st.subheader("🔍 Filters")

col1, col2, col3, col4 = st.columns(4)

with col1:
    status_filter = st.multiselect(
        "Status",
        options=['approved', 'pending'],
        default=['approved', 'pending']
    )

with col2:
    # Get all unique relationships
    relationships = list(set(p.get('relationship', 'Unknown') for p in all_profiles))
    relationship_filter = st.multiselect(
        "Relationship",
        options=relationships,
        default=relationships
    )

with col3:
    # Get all unique domains
    all_domains = set()
    for p in all_profiles:
        for cap in p.get('core_capabilities', []):
            all_domains.update(cap.get('domains', []))
    all_domains = sorted(list(all_domains))

    domain_filter = st.multiselect(
        "Domains",
        options=all_domains
    )

with col4:
    search_query = st.text_input(
        "🔍 Search",
        placeholder="Search company name or capabilities..."
    )

# Apply filters
filtered_profiles = all_profiles

# Status filter
if status_filter:
    filtered_profiles = [p for p in filtered_profiles if p['_status'] in status_filter]

# Relationship filter
if relationship_filter:
    filtered_profiles = [p for p in filtered_profiles if p.get('relationship') in relationship_filter]

# Domain filter
if domain_filter:
    filtered_profiles = [
        p for p in filtered_profiles
        if any(
            domain in cap.get('domains', [])
            for cap in p.get('core_capabilities', [])
            for domain in domain_filter
        )
    ]

# Search filter
if search_query:
    query = search_query.lower()
    filtered_profiles = [
        p for p in filtered_profiles
        if query in p.get('company_name', '').lower() or
        any(query in cap.get('capability_name', '').lower() or
            query in cap.get('description', '').lower()
            for cap in p.get('core_capabilities', []))
    ]

# Sort options
st.markdown("---")
col1, col2 = st.columns([3, 1])

with col1:
    st.subheader(f"📊 Results ({len(filtered_profiles)} companies)")

with col2:
    sort_by = st.selectbox(
        "Sort by",
        options=['Name', 'Last Modified', 'Capabilities Count', 'Avg Confidence']
    )

# Sort profiles
if sort_by == 'Name':
    filtered_profiles.sort(key=lambda p: p.get('company_name', ''))
elif sort_by == 'Last Modified':
    filtered_profiles.sort(key=lambda p: p['_modified'], reverse=True)
elif sort_by == 'Capabilities Count':
    filtered_profiles.sort(key=lambda p: len(p.get('core_capabilities', [])), reverse=True)
elif sort_by == 'Avg Confidence':
    def avg_conf(p):
        caps = p.get('core_capabilities', [])
        if not caps:
            return 0
        return sum(c.get('confidence_score', 0) for c in caps) / len(caps)
    filtered_profiles.sort(key=avg_conf, reverse=True)

# Display profiles
if filtered_profiles:
    for profile in filtered_profiles:
        with st.expander(
            f"{'🟢' if profile['_status'] == 'approved' else '🟡'} "
            f"{profile.get('company_name', 'Unknown')} - "
            f"{len(profile.get('core_capabilities', []))} capabilities"
        ):
            # Header
            col1, col2, col3, col4 = st.columns(4)

            with col1:
                st.markdown(f"**Status:** {profile['_status'].upper()}")

            with col2:
                st.markdown(f"**Relationship:** {profile.get('relationship', 'N/A')}")

            with col3:
                caps = profile.get('core_capabilities', [])
                if caps:
                    avg_conf = sum(c.get('confidence_score', 0) for c in caps) / len(caps)
                    st.markdown(f"**Avg Confidence:** {avg_conf:.0f}%")
                else:
                    st.markdown(f"**Avg Confidence:** N/A")

            with col4:
                st.markdown(f"**Last Updated:** {profile['_modified'].strftime('%Y-%m-%d')}")

            st.markdown("---")

            # Capabilities summary
            if profile.get('core_capabilities'):
                st.markdown("**🎯 Core Capabilities:**")
                for cap in profile['core_capabilities'][:5]:  # Show top 5
                    confidence = cap.get('confidence_score', 0)
                    emoji = "🟢" if confidence >= 70 else "🟡" if confidence >= 40 else "🔴"
                    st.markdown(
                        f"{emoji} **{cap['capability_name']}** ({confidence}% confidence) - "
                        f"{cap.get('readiness_level', 'N/A')}"
                    )
                    st.caption(f"Domains: {', '.join(cap.get('domains', []))}")

                if len(profile['core_capabilities']) > 5:
                    st.caption(f"... and {len(profile['core_capabilities']) - 5} more")

            # Competitive positioning
            if profile.get('competitive_positioning', {}).get('differentiation'):
                st.markdown("**🏆 Differentiation:**")
                st.info(profile['competitive_positioning']['differentiation'])

            # Contract execution
            if profile.get('contract_execution', {}).get('vehicles'):
                st.markdown("**📋 Contract Vehicles:**")
                st.markdown(", ".join(profile['contract_execution']['vehicles']))

            # Warnings
            if profile.get('validation_warnings'):
                st.markdown("**⚠️ Warnings:**")
                for warning in profile['validation_warnings'][:3]:
                    st.warning(warning, icon="⚠️")

            # Actions
            col1, col2, col3 = st.columns(3)

            with col1:
                # Download JSON
                st.download_button(
                    "⬇️ Download JSON",
                    data=json.dumps(profile, indent=2, default=str),
                    file_name=f"{profile.get('company_name', 'company').replace(' ', '_')}.json",
                    mime="application/json",
                    key=f"json_{profile['_source_file']}"
                )

            with col2:
                # View full profile
                if st.button("👁️ View Full Profile", key=f"view_{profile['_source_file']}"):
                    st.session_state.viewing_profile = profile['_source_file']

            with col3:
                # Open in review (if pending)
                if profile['_status'] == 'pending':
                    st.markdown("[📋 Review →](../Review)")

else:
    st.info("No profiles match your filters. Try adjusting the criteria.")

# Export all filtered profiles
if filtered_profiles:
    st.markdown("---")
    st.subheader("📤 Export")

    col1, col2 = st.columns(2)

    with col1:
        # Export as JSON
        export_data = json.dumps(filtered_profiles, indent=2, default=str)
        st.download_button(
            "⬇️ Export All as JSON",
            data=export_data,
            file_name=f"company_profiles_{datetime.now().strftime('%Y%m%d')}.json",
            mime="application/json",
            use_container_width=True
        )

    with col2:
        # Export as CSV (summary)
        summary_data = []
        for p in filtered_profiles:
            caps = p.get('core_capabilities', [])
            avg_conf = sum(c.get('confidence_score', 0) for c in caps) / len(caps) if caps else 0

            summary_data.append({
                'Company': p.get('company_name', ''),
                'Status': p['_status'],
                'Relationship': p.get('relationship', ''),
                'Capabilities': len(caps),
                'Avg Confidence': f"{avg_conf:.0f}%",
                'Warnings': len(p.get('validation_warnings', [])),
                'Last Updated': p['_modified'].strftime('%Y-%m-%d')
            })

        df = pd.DataFrame(summary_data)
        csv = df.to_csv(index=False)

        st.download_button(
            "⬇️ Export Summary as CSV",
            data=csv,
            file_name=f"company_summary_{datetime.now().strftime('%Y%m%d')}.csv",
            mime="text/csv",
            use_container_width=True
        )

# Comparison tool
if len(filtered_profiles) >= 2:
    st.markdown("---")
    st.subheader("⚖️ Compare Companies")

    col1, col2 = st.columns(2)

    with col1:
        company1_idx = st.selectbox(
            "Company 1",
            range(len(filtered_profiles)),
            format_func=lambda i: filtered_profiles[i].get('company_name', 'Unknown')
        )

    with col2:
        company2_idx = st.selectbox(
            "Company 2",
            range(len(filtered_profiles)),
            format_func=lambda i: filtered_profiles[i].get('company_name', 'Unknown'),
            index=min(1, len(filtered_profiles) - 1)
        )

    if st.button("🔄 Compare", use_container_width=True):
        c1 = filtered_profiles[company1_idx]
        c2 = filtered_profiles[company2_idx]

        st.markdown("### Comparison")

        col1, col2 = st.columns(2)

        with col1:
            st.markdown(f"#### {c1.get('company_name')}")
            st.markdown(f"**Capabilities:** {len(c1.get('core_capabilities', []))}")

            caps1 = c1.get('core_capabilities', [])
            if caps1:
                avg1 = sum(c.get('confidence_score', 0) for c in caps1) / len(caps1)
                st.markdown(f"**Avg Confidence:** {avg1:.0f}%")

            st.markdown(f"**Relationship:** {c1.get('relationship', 'N/A')}")

            if c1.get('core_capabilities'):
                st.markdown("**Top Capabilities:**")
                for cap in c1['core_capabilities'][:5]:
                    st.markdown(f"- {cap['capability_name']}")

        with col2:
            st.markdown(f"#### {c2.get('company_name')}")
            st.markdown(f"**Capabilities:** {len(c2.get('core_capabilities', []))}")

            caps2 = c2.get('core_capabilities', [])
            if caps2:
                avg2 = sum(c.get('confidence_score', 0) for c in caps2) / len(caps2)
                st.markdown(f"**Avg Confidence:** {avg2:.0f}%")

            st.markdown(f"**Relationship:** {c2.get('relationship', 'N/A')}")

            if c2.get('core_capabilities'):
                st.markdown("**Top Capabilities:**")
                for cap in c2['core_capabilities'][:5]:
                    st.markdown(f"- {cap['capability_name']}")
