#!/usr/bin/env python3
"""
Portrait Fix Application Script for Desolo Zantas
Reads portrait_fixes.txt and applies all suggested replacements to English.txt
"""

import os
import re

# Base paths
DIALOG_FILE = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Dialog\English.txt"
FIXES_FILE = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Scripts\portrait_fixes.txt"
BACKUP_FILE = DIALOG_FILE + ".backup"

def parse_fixes():
    """Parse portrait_fixes.txt and extract replacements"""
    fixes = []
    
    with open(FIXES_FILE, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Pattern to match: "Replace 'X'\n  With: 'Y'"
    pattern = r"Replace '(.+?)'\n\s*With: '(.+?)'"
    matches = re.findall(pattern, content, re.MULTILINE)
    
    for old_text, new_text in matches:
        fixes.append((old_text, new_text))
    
    return fixes

def apply_fixes():
    """Apply all fixes to English.txt"""
    if not os.path.exists(FIXES_FILE):
        print(f"ERROR: Fixes file not found: {FIXES_FILE}")
        return False
    
    if not os.path.exists(DIALOG_FILE):
        print(f"ERROR: Dialog file not found: {DIALOG_FILE}")
        return False
    
    # Create backup
    print("Creating backup of English.txt...")
    with open(DIALOG_FILE, 'r', encoding='utf-8') as f:
        original_content = f.read()
    
    with open(BACKUP_FILE, 'w', encoding='utf-8') as f:
        f.write(original_content)
    print(f"Backup saved to: {BACKUP_FILE}")
    
    # Parse and apply fixes
    fixes = parse_fixes()
    print(f"\nFound {len(fixes)} fixes to apply\n")
    
    content = original_content
    applied_count = 0
    failed_count = 0
    
    for old_text, new_text in fixes:
        if old_text in content:
            # Count occurrences
            occurrences = content.count(old_text)
            content = content.replace(old_text, new_text)
            applied_count += occurrences
            print(f"  [OK] Replaced '{old_text}' -> '{new_text}' ({occurrences}x)")
        else:
            failed_count += 1
            print(f"  [FAIL] '{old_text}' not found in file")
    
    # Write modified content
    with open(DIALOG_FILE, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print(f"\n{'='*50}")
    print(f"Applied {applied_count} replacements")
    print(f"Failed to apply {failed_count} fixes")
    print(f"Original file backed up to: {BACKUP_FILE}")
    print(f"\nDone! Please review changes with git diff.")
    
    return True

if __name__ == "__main__":
    print("=== DESOLO ZANTAS PORTRAIT FIX APPLICATOR ===\n")
    success = apply_fixes()
    if not success:
        print("\nFix application failed!")
