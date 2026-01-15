"""PDF document parser using PyMuPDF and pdfplumber."""

import io
import logging
from pathlib import Path
from typing import Dict, List, Optional, Tuple
from dataclasses import dataclass

try:
    import fitz  # PyMuPDF
    import pdfplumber
    from PIL import Image
except ImportError as e:
    raise ImportError(
        f"Required libraries not installed: {e}. "
        "Run: pip install PyMuPDF pdfplumber Pillow"
    )

logger = logging.getLogger(__name__)


@dataclass
class ParsedDocument:
    """Container for parsed document content."""
    text: str
    pages: List[Dict]
    images: List[Dict]
    metadata: Dict
    word_count: int


class PDFParser:
    """
    Extracts text and images from PDF documents.

    Uses PyMuPDF for image extraction and pdfplumber for robust text extraction.
    """

    def __init__(self, ocr_enabled: bool = False, extract_images: bool = True):
        """
        Initialize PDF parser.

        Args:
            ocr_enabled: Enable OCR for text embedded in images (requires pytesseract)
            extract_images: Extract images from PDF for analysis
        """
        self.ocr_enabled = ocr_enabled
        self.extract_images = extract_images

        if ocr_enabled:
            try:
                import pytesseract
                self.pytesseract = pytesseract
            except ImportError:
                logger.warning("pytesseract not installed, OCR disabled")
                self.ocr_enabled = False

    def parse(self, file_path: Path) -> ParsedDocument:
        """
        Parse PDF document and extract text and images.

        Args:
            file_path: Path to PDF file

        Returns:
            ParsedDocument with extracted content

        Raises:
            FileNotFoundError: If file doesn't exist
            ValueError: If file is not a valid PDF or is password-protected
        """
        if not file_path.exists():
            raise FileNotFoundError(f"File not found: {file_path}")

        logger.info(f"Parsing PDF: {file_path}")

        # Extract text using pdfplumber (better text extraction)
        pages_data = []
        full_text = []

        try:
            with pdfplumber.open(file_path) as pdf:
                for page_num, page in enumerate(pdf.pages, start=1):
                    page_text = page.extract_text() or ""
                    full_text.append(page_text)

                    pages_data.append({
                        "page_number": page_num,
                        "text": page_text,
                        "word_count": len(page_text.split())
                    })
        except Exception as e:
            if "password" in str(e).lower():
                raise ValueError(
                    f"PDF is password-protected: {file_path}. "
                    "Please provide an unlocked version."
                )
            raise ValueError(f"Failed to parse PDF: {e}")

        combined_text = "\n\n".join(full_text)
        word_count = len(combined_text.split())

        # Extract images using PyMuPDF
        images = []
        if self.extract_images:
            images = self._extract_images(file_path)

        # Get metadata
        metadata = self._extract_metadata(file_path)

        logger.info(
            f"Extracted {len(pages_data)} pages, {word_count} words, "
            f"{len(images)} images from {file_path.name}"
        )

        return ParsedDocument(
            text=combined_text,
            pages=pages_data,
            images=images,
            metadata=metadata,
            word_count=word_count
        )

    def _extract_images(self, file_path: Path) -> List[Dict]:
        """Extract images from PDF."""
        images = []

        try:
            doc = fitz.open(file_path)

            for page_num in range(len(doc)):
                page = doc[page_num]
                image_list = page.get_images()

                for img_index, img in enumerate(image_list):
                    xref = img[0]
                    try:
                        base_image = doc.extract_image(xref)
                        image_bytes = base_image["image"]
                        image_ext = base_image["ext"]

                        # Convert to PIL Image
                        pil_image = Image.open(io.BytesIO(image_bytes))

                        # Store image info
                        images.append({
                            "page": page_num + 1,
                            "index": img_index,
                            "format": image_ext,
                            "size": pil_image.size,
                            "bytes": image_bytes,
                            "description": None  # To be filled by vision model
                        })

                        # OCR if enabled and image is large enough
                        if self.ocr_enabled and min(pil_image.size) > 100:
                            try:
                                ocr_text = self.pytesseract.image_to_string(pil_image)
                                if ocr_text.strip():
                                    images[-1]["ocr_text"] = ocr_text
                            except Exception as e:
                                logger.debug(f"OCR failed for image: {e}")

                    except Exception as e:
                        logger.debug(f"Failed to extract image {img_index} on page {page_num + 1}: {e}")

            doc.close()

        except Exception as e:
            logger.warning(f"Image extraction failed: {e}")

        return images

    def _extract_metadata(self, file_path: Path) -> Dict:
        """Extract PDF metadata."""
        metadata = {
            "filename": file_path.name,
            "file_size_bytes": file_path.stat().st_size
        }

        try:
            doc = fitz.open(file_path)
            pdf_metadata = doc.metadata

            metadata.update({
                "title": pdf_metadata.get("title", ""),
                "author": pdf_metadata.get("author", ""),
                "subject": pdf_metadata.get("subject", ""),
                "creator": pdf_metadata.get("creator", ""),
                "page_count": len(doc)
            })

            doc.close()
        except Exception as e:
            logger.debug(f"Failed to extract metadata: {e}")

        return metadata

    def validate_document(self, parsed_doc: ParsedDocument, min_words: int = 100) -> Tuple[bool, Optional[str]]:
        """
        Validate that document has sufficient content.

        Args:
            parsed_doc: Parsed document to validate
            min_words: Minimum word count required

        Returns:
            Tuple of (is_valid, error_message)
        """
        if parsed_doc.word_count < min_words:
            return False, (
                f"Insufficient content: only {parsed_doc.word_count} words extracted "
                f"(minimum: {min_words}). The document may be image-based or corrupted."
            )

        if not parsed_doc.text.strip():
            return False, "No text content extracted from document."

        return True, None
