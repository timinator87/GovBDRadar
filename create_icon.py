"""
Create a simple application icon for GovBDRadar
Generates a 256x256 icon with the app logo
"""

from PIL import Image, ImageDraw, ImageFont

def create_icon():
    """Create a simple icon for the application"""

    # Create a 256x256 image
    size = 256
    img = Image.new('RGB', (size, size), color='#1f77b4')  # Blue background
    draw = ImageDraw.Draw(img)

    # Draw a circular background
    circle_margin = 20
    draw.ellipse(
        [circle_margin, circle_margin, size - circle_margin, size - circle_margin],
        fill='#2196F3',
        outline='#0D47A1',
        width=5
    )

    # Draw a target/radar symbol
    center = size // 2

    # Outer rings
    for radius in [90, 70, 50]:
        draw.ellipse(
            [center - radius, center - radius, center + radius, center + radius],
            outline='white',
            width=3
        )

    # Crosshairs
    draw.line([center, 40, center, size - 40], fill='white', width=3)
    draw.line([40, center, size - 40, center], fill='white', width=3)

    # Center dot
    center_dot_radius = 10
    draw.ellipse(
        [center - center_dot_radius, center - center_dot_radius,
         center + center_dot_radius, center + center_dot_radius],
        fill='#FFC107',
        outline='white',
        width=2
    )

    # Try to add text (may fail if font not available)
    try:
        # Try to use a bold font
        font = ImageFont.truetype("arial.ttf", 32)
        text = "GBR"

        # Get text bounding box
        bbox = draw.textbbox((0, 0), text, font=font)
        text_width = bbox[2] - bbox[0]
        text_height = bbox[3] - bbox[1]

        text_x = (size - text_width) // 2
        text_y = size - 60

        # Draw text with outline
        for offset in [(0,1), (1,0), (0,-1), (-1,0)]:
            draw.text(
                (text_x + offset[0], text_y + offset[1]),
                text,
                font=font,
                fill='#0D47A1'
            )
        draw.text((text_x, text_y), text, font=font, fill='white')
    except:
        # Font not available, skip text
        pass

    # Save in multiple formats
    img.save('icon.png', 'PNG')

    # Create ICO file (Windows icon)
    # ICO files support multiple resolutions
    icon_sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)]
    icons = []
    for size_tuple in icon_sizes:
        icons.append(img.resize(size_tuple, Image.Resampling.LANCZOS))

    icons[0].save('icon.ico', format='ICO', sizes=[s for s in icon_sizes])

    print("✓ Created icon.png")
    print("✓ Created icon.ico")
    print()
    print("Icon files created successfully!")

if __name__ == "__main__":
    create_icon()
