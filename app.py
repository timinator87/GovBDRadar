"""
GovBDRadar - Company Intelligence Ingestion Tool
Web Interface powered by Streamlit
"""

import streamlit as st
import os
from pathlib import Path

# Page configuration
st.set_page_config(
    page_title="GovBDRadar",
    page_icon="🎯",
    layout="wide",
    initial_sidebar_state="expanded"
)

# Custom CSS
st.markdown("""
<style>
    .main-header {
        font-size: 2.5rem;
        font-weight: bold;
        color: #1f77b4;
        margin-bottom: 0.5rem;
    }
    .sub-header {
        font-size: 1.2rem;
        color: #666;
        margin-bottom: 2rem;
    }
    .card {
        padding: 1.5rem;
        border-radius: 0.5rem;
        background-color: #f0f2f6;
        margin-bottom: 1rem;
    }
    .metric-card {
        padding: 1rem;
        border-radius: 0.5rem;
        background-color: #e8f4f8;
        border-left: 4px solid #1f77b4;
    }
    .stButton>button {
        width: 100%;
    }
</style>
""", unsafe_allow_html=True)

# Initialize session state
if 'api_key_set' not in st.session_state:
    st.session_state.api_key_set = bool(os.getenv('ANTHROPIC_API_KEY'))

def main():
    # Sidebar
    with st.sidebar:
        st.image("https://via.placeholder.com/200x80/1f77b4/ffffff?text=GovBDRadar", use_container_width=True)
        st.markdown("---")

        # API Key Configuration
        st.subheader("⚙️ Configuration")

        if not st.session_state.api_key_set:
            st.warning("Anthropic API key not configured!")
            api_key = st.text_input("Anthropic API Key", type="password", key="api_key_input")
            if st.button("Save API Key"):
                if api_key:
                    os.environ['ANTHROPIC_API_KEY'] = api_key
                    st.session_state.api_key_set = True
                    st.success("API key saved!")
                    st.rerun()
        else:
            st.success("✓ API Key Configured")
            if st.button("Clear API Key"):
                if 'ANTHROPIC_API_KEY' in os.environ:
                    del os.environ['ANTHROPIC_API_KEY']
                st.session_state.api_key_set = False
                st.rerun()

        st.markdown("---")

        # Statistics
        st.subheader("📊 Statistics")
        pending_count = len(list(Path("data/pending").glob("*.json")))
        approved_count = len(list(Path("data/approved").glob("*.json")))

        col1, col2 = st.columns(2)
        with col1:
            st.metric("Pending", pending_count)
        with col2:
            st.metric("Approved", approved_count)

        st.markdown("---")
        st.caption("Version 1.0.0")

    # Main content
    st.markdown('<div class="main-header">🎯 GovBDRadar</div>', unsafe_allow_html=True)
    st.markdown('<div class="sub-header">Company Intelligence Ingestion Tool</div>', unsafe_allow_html=True)

    # Welcome message
    st.markdown("""
    Welcome to **GovBDRadar**, your tool for extracting structured capability intelligence
    from pitch decks and capability statements.
    """)

    st.markdown("---")

    # Feature cards
    col1, col2, col3, col4 = st.columns(4)

    with col1:
        st.markdown('<div class="card">', unsafe_allow_html=True)
        st.markdown("### 📤 Ingest")
        st.markdown("Upload and process pitch decks or capability statements")
        st.markdown("</div>", unsafe_allow_html=True)

    with col2:
        st.markdown('<div class="card">', unsafe_allow_html=True)
        st.markdown("### 📋 Review")
        st.markdown("Review extracted capabilities and add notes")
        st.markdown("</div>", unsafe_allow_html=True)

    with col3:
        st.markdown('<div class="card">', unsafe_allow_html=True)
        st.markdown("### ✅ Approve")
        st.markdown("Approve profiles and move to production")
        st.markdown("</div>", unsafe_allow_html=True)

    with col4:
        st.markdown('<div class="card">', unsafe_allow_html=True)
        st.markdown("### 📁 Browse")
        st.markdown("Browse and search company profiles")
        st.markdown("</div>", unsafe_allow_html=True)

    st.markdown("---")

    # Quick Start Guide
    with st.expander("📖 Quick Start Guide", expanded=False):
        st.markdown("""
        ### Getting Started

        1. **Configure API Key** (if not already set)
           - Add your Anthropic API key in the sidebar
           - Or set the `ANTHROPIC_API_KEY` environment variable

        2. **Ingest a Document**
           - Go to the 📤 **Ingest** page
           - Upload a PDF or PPTX file
           - Enter company details
           - Click "Process Document"

        3. **Review Extraction**
           - Go to the 📋 **Review** page
           - Select a pending profile
           - Review capabilities and warnings
           - Edit JSON or add notes

        4. **Approve Profile**
           - On the review page, click "Approve Profile"
           - Add approval notes
           - Profile moves to approved directory

        5. **Browse Profiles**
           - Go to the 📁 **Browse** page
           - Search and filter profiles
           - Export or compare companies
        """)

    # Workflow diagram
    with st.expander("🔄 Workflow Overview", expanded=False):
        st.markdown("""
        ```
        ┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
        │   Upload    │ →   │   Extract   │ →   │   Review    │ →   │   Approve   │
        │  Document   │     │ Capabilities│     │  & Edit     │     │   Profile   │
        └─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
        ```

        - **Upload**: Provide company documents (PDF/PPTX)
        - **Extract**: AI analyzes and structures capabilities
        - **Review**: Human validates and corrects
        - **Approve**: Finalize and store for production use
        """)

    # Recent Activity
    st.markdown("---")
    st.subheader("📅 Recent Activity")

    # Get recent files
    pending_files = sorted(
        Path("data/pending").glob("*.json"),
        key=lambda x: x.stat().st_mtime,
        reverse=True
    )[:5]

    approved_files = sorted(
        Path("data/approved").glob("*.json"),
        key=lambda x: x.stat().st_mtime,
        reverse=True
    )[:5]

    col1, col2 = st.columns(2)

    with col1:
        st.markdown("**Recent Pending Profiles**")
        if pending_files:
            for f in pending_files:
                company_name = f.stem.rsplit('_', 1)[0].replace('_', ' ')
                st.markdown(f"- 🟡 {company_name}")
        else:
            st.info("No pending profiles")

    with col2:
        st.markdown("**Recently Approved**")
        if approved_files:
            for f in approved_files:
                company_name = f.stem.rsplit('_', 1)[0].replace('_', ' ')
                st.markdown(f"- 🟢 {company_name}")
        else:
            st.info("No approved profiles")

    st.markdown("---")

    # Help section
    st.subheader("💡 Need Help?")

    col1, col2, col3 = st.columns(3)

    with col1:
        st.markdown("**Documentation**")
        st.markdown("[View README](README.md)")
        st.markdown("[View Examples](EXAMPLES.md)")

    with col2:
        st.markdown("**Support**")
        st.markdown("[GitHub Issues](https://github.com/timinator87/GovBDRadar/issues)")
        st.markdown("[Report Bug](https://github.com/timinator87/GovBDRadar/issues/new)")

    with col3:
        st.markdown("**Configuration**")
        st.markdown("Edit `config.yaml` for advanced settings")
        st.markdown("Customize evidence weights and extraction prompts")

if __name__ == "__main__":
    main()
