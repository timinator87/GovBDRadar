"""
Document Ingestion Page
Upload and process pitch decks and capability statements
"""

import streamlit as st
import os
import sys
from pathlib import Path
from datetime import datetime
import json
import tempfile

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent.parent))

from parsers.pdf_parser import PDFParser
from parsers.pptx_parser import PPTXParser
from extractors.capability_extractor import CapabilityExtractor
from validators.evidence_scorer import EvidenceScorer
from output_generator import OutputGenerator

st.set_page_config(page_title="Ingest Document", page_icon="📤", layout="wide")

st.title("📤 Ingest Document")
st.markdown("Upload a pitch deck or capability statement to extract company intelligence")

# Check API key
if not os.getenv('ANTHROPIC_API_KEY'):
    st.error("⚠️ Anthropic API key not configured. Please set it on the home page.")
    st.stop()

# Initialize session state
if 'processing' not in st.session_state:
    st.session_state.processing = False
if 'last_result' not in st.session_state:
    st.session_state.last_result = None

# Company Information Form
st.markdown("---")
st.subheader("📋 Company Information")

col1, col2 = st.columns(2)

with col1:
    company_name = st.text_input(
        "Company Name *",
        placeholder="e.g., Thor Dynamics",
        help="Full legal name or common name of the company"
    )

    relationship = st.selectbox(
        "Relationship *",
        options=["investment_target", "portfolio", "partner", "competitor"],
        help="Your relationship to this company"
    )

with col2:
    contact_name = st.text_input(
        "Primary Contact (optional)",
        placeholder="e.g., John Smith",
        help="Main point of contact at the company"
    )

    website = st.text_input(
        "Website (optional)",
        placeholder="e.g., https://company.com",
        help="Company website for additional context"
    )

# Additional context
additional_context = st.text_area(
    "Additional Context (optional)",
    placeholder="Any specific information to guide extraction, e.g., 'Focus on counter-UAS capabilities' or 'They recently won an SBIR Phase II'",
    height=100,
    help="Provide any context that might help with extraction"
)

# Document Upload
st.markdown("---")
st.subheader("📄 Document Upload")

uploaded_file = st.file_uploader(
    "Upload Pitch Deck or Capability Statement",
    type=['pdf', 'pptx'],
    help="Upload a PDF or PowerPoint file (max 50MB)"
)

if uploaded_file:
    st.success(f"✓ File uploaded: {uploaded_file.name} ({uploaded_file.size / 1024 / 1024:.2f} MB)")

    # File preview info
    with st.expander("📊 File Details", expanded=False):
        col1, col2, col3 = st.columns(3)
        with col1:
            st.metric("Filename", uploaded_file.name)
        with col2:
            st.metric("Type", uploaded_file.type)
        with col3:
            st.metric("Size", f"{uploaded_file.size / 1024 / 1024:.2f} MB")

# Processing Options
st.markdown("---")
st.subheader("⚙️ Processing Options")

col1, col2, col3 = st.columns(3)

with col1:
    extract_images = st.checkbox(
        "Extract and analyze images",
        value=True,
        help="Analyze diagrams, charts, and technical schematics"
    )

with col2:
    use_ocr = st.checkbox(
        "Enable OCR for text in images",
        value=True,
        help="Extract text embedded in images (slower)"
    )

with col3:
    verbose_output = st.checkbox(
        "Show detailed processing logs",
        value=False,
        help="Display step-by-step processing information"
    )

# Process Button
st.markdown("---")

process_button = st.button(
    "🚀 Process Document",
    type="primary",
    disabled=not (company_name and uploaded_file),
    use_container_width=True
)

if not company_name:
    st.info("👆 Please enter company name to continue")
if not uploaded_file:
    st.info("👆 Please upload a document to continue")

# Processing
if process_button and company_name and uploaded_file:
    st.session_state.processing = True

    with st.spinner("🔄 Processing document... This may take a few minutes."):
        try:
            # Create progress tracking
            progress_bar = st.progress(0)
            status_text = st.empty()

            # Step 1: Save uploaded file temporarily
            status_text.text("📥 Saving uploaded file...")
            progress_bar.progress(10)

            with tempfile.NamedTemporaryFile(delete=False, suffix=Path(uploaded_file.name).suffix) as tmp_file:
                tmp_file.write(uploaded_file.getbuffer())
                tmp_path = tmp_file.name

            # Step 2: Parse document
            status_text.text("📄 Parsing document...")
            progress_bar.progress(20)

            file_ext = Path(uploaded_file.name).suffix.lower()

            if file_ext == '.pdf':
                parser = PDFParser()
            elif file_ext == '.pptx':
                parser = PPTXParser()
            else:
                st.error(f"Unsupported file type: {file_ext}")
                st.stop()

            parsed_data = parser.parse(
                tmp_path,
                extract_images=extract_images,
                use_ocr=use_ocr
            )

            if verbose_output:
                with st.expander("📋 Parsed Content Preview"):
                    st.text(parsed_data['text'][:1000] + "..." if len(parsed_data['text']) > 1000 else parsed_data['text'])
                    if parsed_data.get('images'):
                        st.write(f"Extracted {len(parsed_data['images'])} images")

            # Step 3: Extract capabilities
            status_text.text("🤖 Extracting capabilities with AI...")
            progress_bar.progress(40)

            extractor = CapabilityExtractor()
            profile = extractor.extract(
                parsed_data=parsed_data,
                company_name=company_name,
                relationship=relationship,
                additional_context=additional_context or None,
                contact_name=contact_name or None,
                website=website or None
            )

            # Step 4: Score evidence
            status_text.text("📊 Scoring evidence and validating...")
            progress_bar.progress(70)

            scorer = EvidenceScorer()
            profile = scorer.score_profile(profile)

            # Step 5: Generate outputs
            status_text.text("💾 Generating output files...")
            progress_bar.progress(90)

            generator = OutputGenerator()
            output_paths = generator.generate(
                profile=profile,
                output_dir="data/pending",
                source_filename=uploaded_file.name
            )

            # Step 6: Complete
            progress_bar.progress(100)
            status_text.text("✅ Processing complete!")

            # Clean up temp file
            os.unlink(tmp_path)

            # Store result
            st.session_state.last_result = {
                'profile': profile,
                'output_paths': output_paths,
                'timestamp': datetime.now().isoformat()
            }

            # Success message
            st.success("🎉 Document processed successfully!")

            # Display results
            st.markdown("---")
            st.subheader("📊 Extraction Results")

            # Summary metrics
            col1, col2, col3, col4 = st.columns(4)

            with col1:
                st.metric(
                    "Capabilities",
                    len(profile.get('core_capabilities', []))
                )

            with col2:
                st.metric(
                    "Domains",
                    len(profile.get('operational_domains', []))
                )

            with col3:
                avg_confidence = sum(
                    cap.get('confidence_score', 0)
                    for cap in profile.get('core_capabilities', [])
                ) / max(len(profile.get('core_capabilities', [])), 1)
                st.metric(
                    "Avg Confidence",
                    f"{avg_confidence:.0f}%"
                )

            with col4:
                st.metric(
                    "Warnings",
                    len(profile.get('validation_warnings', []))
                )

            # Quick preview
            with st.expander("👀 Quick Preview", expanded=True):
                if profile.get('core_capabilities'):
                    st.markdown("**Top Capabilities:**")
                    for cap in profile['core_capabilities'][:3]:
                        confidence = cap.get('confidence_score', 0)
                        confidence_emoji = "🟢" if confidence >= 70 else "🟡" if confidence >= 40 else "🔴"
                        st.markdown(f"{confidence_emoji} **{cap['capability_name']}** ({confidence}% confidence)")
                        st.markdown(f"   - {cap['description'][:200]}...")

                if profile.get('validation_warnings'):
                    st.markdown("**⚠️ Validation Warnings:**")
                    for warning in profile['validation_warnings'][:3]:
                        st.warning(warning)

            # Output files
            st.markdown("---")
            st.subheader("📁 Output Files")

            col1, col2 = st.columns(2)

            with col1:
                st.markdown(f"**JSON Profile:**")
                st.code(output_paths['json'], language="text")

                with open(output_paths['json'], 'r') as f:
                    json_content = f.read()

                st.download_button(
                    "⬇️ Download JSON",
                    data=json_content,
                    file_name=Path(output_paths['json']).name,
                    mime="application/json"
                )

            with col2:
                st.markdown(f"**Markdown Review:**")
                st.code(output_paths['markdown'], language="text")

                with open(output_paths['markdown'], 'r') as f:
                    md_content = f.read()

                st.download_button(
                    "⬇️ Download Markdown",
                    data=md_content,
                    file_name=Path(output_paths['markdown']).name,
                    mime="text/markdown"
                )

            # Next steps
            st.info("👉 Go to the **Review** page to review and edit this extraction before approval.")

        except Exception as e:
            st.error(f"❌ Error processing document: {str(e)}")
            if verbose_output:
                st.exception(e)

    st.session_state.processing = False

# Show last result if available
elif st.session_state.last_result and not process_button:
    st.markdown("---")
    st.info("📋 Previous processing result is shown above. Upload a new document to process again.")
