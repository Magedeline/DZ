#!/usr/bin/env python3
"""
Portrait Asset Pipeline for Desolo Zantas
Generates an accurate manifest of all portrait assets and identifies gaps.
"""

import os
import re
import json
from collections import defaultdict

DIALOG_FILE = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Dialog\English.txt"
PORTRAIT_DIR = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Graphics\Atlases\Portraits"
OUTPUT_DIR = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Scripts"


def scan_portrait_directories():
    """Scan all portrait directories and their expressions."""
    characters = {}
    
    for item in os.listdir(PORTRAIT_DIR):
        item_path = os.path.join(PORTRAIT_DIR, item)
        if os.path.isdir(item_path):
            expressions = defaultdict(int)
            for f in os.listdir(item_path):
                if f.endswith('.png'):
                    # Extract expression name (e.g., "angry00.png" -> "angry")
                    match = re.match(r'^([a-zA-Z_]+)\d+\.png$', f)
                    if match:
                        expressions[match.group(1).lower()] += 1
            if expressions:
                characters[item.lower()] = dict(expressions)
    
    return characters


def extract_dialogue_characters():
    """Extract all unique character names used in portrait tags from English.txt."""
    with open(DIALOG_FILE, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Match portrait tags like [MADELINE left angry] or [Flowey center normal]
    pattern = re.compile(r'\[([A-Z][a-zA-Z_0-9]*)\s+(left|right|up|down|center|none)\s+([a-zA-Z_0-9]+)\]')
    matches = pattern.findall(content)
    
    characters = defaultdict(lambda: defaultdict(int))
    
    for char_name, position, expression in matches:
        char_lower = char_name.lower()
        expr_lower = expression.lower()
        characters[char_lower][expr_lower] += 1
    
    return dict(characters)


def find_character_mapping_issues(portrait_chars, dialog_chars):
    """Find mapping issues between dialogue characters and portrait directories."""
    issues = {
        'missing_directories': [],      # Characters in dialog but no portrait dir
        'missing_expressions': {},      # Characters with dirs but missing expressions
        'case_mismatches': [],          # Case sensitivity issues
        'empty_directories': [],        # Portrait dirs with no PNGs
    }
    
    # Check for characters in dialog without portrait dirs
    for char_name in sorted(dialog_chars.keys()):
        if char_name not in portrait_chars:
            # Check for case-insensitive match
            found = False
            for pc in portrait_chars:
                if pc.lower() == char_name:
                    issues['case_mismatches'].append((char_name, pc))
                    found = True
                    break
            if not found:
                issues['missing_directories'].append(char_name)
    
    # Check for missing expressions in existing portrait dirs
    for char_name, expressions in dialog_chars.items():
        if char_name in portrait_chars:
            available = set(portrait_chars[char_name].keys())
            missing = [expr for expr in expressions if expr not in available]
            if missing:
                issues['missing_expressions'][char_name] = missing
    
    return issues


def generate_manifest():
    """Generate the complete portrait asset manifest."""
    print("=== DESOLO ZANTAS PORTRAIT ASSET MANIFEST ===\n")
    
    portrait_chars = scan_portrait_directories()
    dialog_chars = extract_dialogue_characters()
    issues = find_character_mapping_issues(portrait_chars, dialog_chars)
    
    # Summary
    total_portrait_dirs = len(portrait_chars)
    total_dialog_chars = len(dialog_chars)
    missing_dirs = len(issues['missing_directories'])
    missing_exprs = sum(len(v) for v in issues['missing_expressions'].values())
    
    print(f"Portrait directories found: {total_portrait_dirs}")
    print(f"Dialogue characters found: {total_dialog_chars}")
    print(f"Missing directories: {missing_dirs}")
    print(f"Missing expressions: {missing_exprs}")
    print(f"Case mismatches: {len(issues['case_mismatches'])}\n")
    
    # Generate JSON manifest
    manifest = {
        'summary': {
            'portrait_directories': total_portrait_dirs,
            'dialogue_characters': total_dialog_chars,
            'missing_directories': missing_dirs,
            'missing_expressions': missing_exprs,
            'case_mismatches': len(issues['case_mismatches'])
        },
        'portrait_inventory': portrait_chars,
        'dialogue_requirements': dialog_chars,
        'issues': {
            'missing_directories': issues['missing_directories'],
            'missing_expressions': issues['missing_expressions'],
            'case_mismatches': issues['case_mismatches']
        }
    }
    
    manifest_path = os.path.join(OUTPUT_DIR, 'portrait_manifest.json')
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=2)
    
    print(f"Full manifest saved to: {manifest_path}\n")
    
    # Generate artist report
    report_path = os.path.join(OUTPUT_DIR, 'portrait_artist_report.txt')
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write("=== PORTRAIT ASSET ARTIST REPORT ===\n\n")
        f.write(f"Total portrait directories: {total_portrait_dirs}\n")
        f.write(f"Total dialogue characters: {total_dialog_chars}\n\n")
        
        if issues['missing_directories']:
            f.write("\n=== CHARACTERS NEEDING NEW PORTRAIT DIRECTORIES ===\n")
            f.write("(Create a folder under Graphics/Atlases/Portraits/ with these names)\n\n")
            for char_name in sorted(issues['missing_directories']):
                required_exprs = sorted(dialog_chars[char_name].keys())
                f.write(f"  {char_name}/\n")
                f.write(f"    Required expressions: {', '.join(required_exprs)}\n")
        
        if issues['missing_expressions']:
            f.write("\n=== EXISTING CHARACTERS NEEDING ADDITIONAL EXPRESSIONS ===\n\n")
            for char_name, missing in sorted(issues['missing_expressions'].items()):
                f.write(f"  {char_name}/: missing {', '.join(sorted(missing))}\n")
        
        if issues['case_mismatches']:
            f.write("\n=== CASE MISMATCHES (fix directory name to match dialog tag) ===\n\n")
            for dialog_name, dir_name in sorted(issues['case_mismatches']):
                f.write(f"  Dialog uses: {dialog_name}, Directory is: {dir_name}\n")
        
        if not issues['missing_directories'] and not issues['missing_expressions']:
            f.write("\nAll portrait assets are complete!\n")
    
    print(f"Artist report saved to: {report_path}\n")
    
    # Print details
    if issues['missing_directories']:
        print("--- CHARACTERS NEEDING NEW DIRECTORIES ---")
        for char_name in sorted(issues['missing_directories'])[:20]:
            print(f"  {char_name}")
        if len(issues['missing_directories']) > 20:
            print(f"  ... and {len(issues['missing_directories']) - 20} more")
        print()
    
    return manifest


def create_placeholder_portraits():
    """Create placeholder portrait directories for missing characters."""
    manifest = generate_manifest()
    missing = manifest['issues']['missing_directories']
    
    if not missing:
        print("No missing directories to create!")
        return
    
    print("=== CREATING PLACEHOLDER PORTRAIT DIRECTORIES ===\n")
    
    created = 0
    for char_name in sorted(missing):
        dir_path = os.path.join(PORTRAIT_DIR, char_name)
        if not os.path.exists(dir_path):
            os.makedirs(dir_path, exist_ok=True)
            
            # Create a placeholder README
            readme_path = os.path.join(dir_path, '_PLACEHOLDER.txt')
            with open(readme_path, 'w') as f:
                f.write(f"PLACEHOLDER: Portrait assets for character '{char_name.upper()}'\n")
                f.write(f"Required expressions: {', '.join(sorted(manifest['dialogue_requirements'][char_name].keys()))}\n")
                f.write("\nAdd PNG files in format: {expression}{frame}.png\n")
                f.write("Example: normal00.png, angry01.png, happy02.png\n")
                f.write("\nEach expression typically needs 1-12 frames for animation.\n")
            
            created += 1
    
    print(f"Created {created} placeholder directories with READMEs")
    print(f"See: {PORTRAIT_DIR}")


if __name__ == "__main__":
    print("Choose action:")
    print("  1. Generate manifest only")
    print("  2. Generate manifest + create placeholder directories")
    
    import sys
    if len(sys.argv) > 1 and sys.argv[1] == '--create':
        create_placeholder_portraits()
    else:
        generate_manifest()
