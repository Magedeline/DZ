#!/usr/bin/env python3
"""
Replace event paths in Celeste .bin map files.
Handles .NET-style string encoding (7-bit length prefix + UTF-8 bytes).
"""

import os
from pathlib import Path

# The old strings and their replacements
# Both old strings are 21 chars, so 1-byte prefix = 0x15
# New string is 15 chars, so 1-byte prefix = 0x0F
REPLACEMENTS = {
    b'\x15event:/desolozantas/': b'\x0fevent:/pusheen/',
    b'\x15event:/desolo_zantas/': b'\x0fevent:/pusheen/',
}

def process_file(filepath: Path):
    data = filepath.read_bytes()
    original = data
    modified = False
    
    for old_bytes, new_bytes in REPLACEMENTS.items():
        if old_bytes in data:
            data = data.replace(old_bytes, new_bytes)
            modified = True
    
    if modified:
        filepath.write_bytes(data)
        print(f"Updated: {filepath}")
    
    return modified

def main():
    root = Path(r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Maps")
    bin_files = list(root.rglob("*.bin"))
    
    print(f"Found {len(bin_files)} .bin files")
    
    updated = 0
    for f in bin_files:
        if process_file(f):
            updated += 1
    
    print(f"\nDone! Updated {updated} files.")

if __name__ == '__main__':
    main()
