# -*- mode: python ; coding: utf-8 -*-
"""
PyInstaller spec file for GovBDRadar
This creates a standalone Windows executable with all dependencies
"""

import sys
from pathlib import Path

# Get the app directory
app_dir = Path('.')

# Collect all data files
datas = [
    ('app.py', '.'),
    ('pages', 'pages'),
    ('templates', 'templates'),
    ('parsers', 'parsers'),
    ('extractors', 'extractors'),
    ('validators', 'validators'),
    ('config.yaml', '.'),
    ('output_generator.py', '.'),
    ('README.md', '.'),
    ('WEB_INTERFACE_GUIDE.md', '.'),
    ('EXAMPLES.md', '.'),
]

# Create data directories
data_dirs = [
    ('data/pending', 'data/pending'),
    ('data/approved', 'data/approved'),
    ('data/training', 'data/training'),
]

# Add data directories with .gitkeep files
for src, dst in data_dirs:
    datas.append((src, dst))

# Hidden imports that PyInstaller might miss
hiddenimports = [
    'streamlit',
    'streamlit.web.cli',
    'streamlit.runtime.scriptrunner.magic_funcs',
    'anthropic',
    'click',
    'pydantic',
    'yaml',
    'PIL',
    'pandas',
    'json',
    'pathlib',
    'datetime',
    'tempfile',
    'shutil',
]

block_cipher = None

a = Analysis(
    ['launcher.py'],
    pathex=[],
    binaries=[],
    datas=datas,
    hiddenimports=hiddenimports,
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[
        'matplotlib',
        'numpy',  # Exclude if not needed to reduce size
        'scipy',
        'IPython',
        'jupyter',
    ],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    [],
    exclude_binaries=True,
    name='GovBDRadar',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    console=True,  # Keep console visible
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    icon='icon.ico' if Path('icon.ico').exists() else None,
)

coll = COLLECT(
    exe,
    a.binaries,
    a.zipfiles,
    a.datas,
    strip=False,
    upx=True,
    upx_exclude=[],
    name='GovBDRadar',
)
