#!/usr/bin/env python3
"""
Portrait Validation Script for Desolo Zantas
Validates all character portrait references in English.txt against actual portrait files
"""

import os
import re
from pathlib import Path
from collections import defaultdict

# Base paths
DIALOG_FILE = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Dialog\English.txt"
PORTRAITS_DIR = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Graphics\Atlases\Portraits"

# Pattern to match portrait references: [CHARACTER position expression]
PORTRAIT_PATTERN = re.compile(r'\[([A-Za-z]+)\s+(left|right)\s+([A-Za-z0-9_]+)\]')

def get_available_portraits():
    """Scan portrait directory and return available expressions for each character"""
    portraits = defaultdict(set)
    
    if not os.path.exists(PORTRAITS_DIR):
        print(f"ERROR: Portraits directory not found: {PORTRAITS_DIR}")
        return portraits
    
    for char_dir in os.listdir(PORTRAITS_DIR):
        char_path = os.path.join(PORTRAITS_DIR, char_dir)
        if os.path.isdir(char_path):
            for file in os.listdir(char_path):
                if file.endswith('.png'):
                    # Extract expression name (remove numbers and .png)
                    expression = re.sub(r'\d+\.png$', '', file)
                    portraits[char_dir.lower()].add(expression.lower())
    
    return portraits

def validate_dialog_file():
    """Parse dialog file and check all portrait references"""
    available_portraits = get_available_portraits()
    issues = []
    portrait_usage = defaultdict(lambda: defaultdict(int))
    
    with open(DIALOG_FILE, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    for line_num, line in enumerate(lines, 1):
        matches = PORTRAIT_PATTERN.findall(line)
        for match in matches:
            character, position, expression = match
            char_lower = character.lower()
            expr_lower = expression.lower()
            
            # Track usage
            portrait_usage[char_lower][expr_lower] += 1
            
            # Check if character exists
            if char_lower not in available_portraits:
                issues.append({
                    'line': line_num,
                    'text': line.strip(),
                    'issue': f"Character '{character}' not found in portraits directory",
                    'severity': 'ERROR'
                })
                continue
            
            # Check if expression exists
            if expr_lower not in available_portraits[char_lower]:
                # Special handling for common expression mappings
                expression_mappings = {
                    'blinded': ['sad', 'worried', 'upset'],
                    'remembering': ['happy', 'normal'],
                    'deadpan': ['normal', 'annoyed'],
                    'angryalt': ['angry', 'upset'],
                    'worriedalt': ['worried', 'concerned']
                }
                
                suggested = expression_mappings.get(expr_lower, list(available_portraits[char_lower])[:3])
                
                issues.append({
                    'line': line_num,
                    'text': line.strip(),
                    'issue': f"Expression '{expression}' not found for character '{character}'",
                    'severity': 'ERROR',
                    'available': sorted(available_portraits[char_lower]),
                    'suggested': suggested
                })
    
    return issues, portrait_usage

def generate_report():
    """Generate comprehensive validation report"""
    print("=== DESOLO ZANTAS PORTRAIT VALIDATION REPORT ===\n")
    
    issues, usage = validate_dialog_file()
    
    # Summary statistics
    print(f"Total issues found: {len(issues)}")
    error_count = sum(1 for i in issues if i['severity'] == 'ERROR')
    print(f"Errors: {error_count}")
    print(f"Warnings: {len(issues) - error_count}\n")
    
    # Character usage summary
    print("=== CHARACTER USAGE SUMMARY ===")
    for character in sorted(usage.keys()):
        total_uses = sum(usage[character].values())
        print(f"\n{character.upper()}: {total_uses} total uses")
        for expression in sorted(usage[character].keys(), key=usage[character].get, reverse=True)[:5]:
            print(f"  - {expression}: {usage[character][expression]} times")
    
    # Detailed issues
    if issues:
        print("\n\n=== DETAILED ISSUES ===")
        for issue in sorted(issues, key=lambda x: x['line']):
            print(f"\nLine {issue['line']}: {issue['issue']}")
            print(f"  Text: {issue['text']}")
            if 'available' in issue:
                print(f"  Available expressions: {', '.join(issue['available'][:10])}")
            if 'suggested' in issue:
                print(f"  Suggested replacements: {', '.join(issue['suggested'])}")
    
    # Generate fix suggestions file
    with open('portrait_fixes.txt', 'w') as f:
        f.write("=== AUTOMATED FIX SUGGESTIONS ===\n\n")
        for issue in issues:
            if 'suggested' in issue and issue['suggested']:
                f.write(f"Line {issue['line']}: Replace '{issue['text']}'\n")
                suggested_fix = issue['text']
                for suggestion in issue['suggested']:
                    if suggestion in issue.get('available', []):
                        # Replace the expression in the text
                        pattern = r'\[(\w+)\s+(left|right)\s+\w+\]'
                        replacement = fr'[\1 \2 {suggestion}]'
                        suggested_fix = re.sub(pattern, replacement, issue['text'])
                        break
                f.write(f"  With: '{suggested_fix}'\n\n")
    
    print("\n\nFix suggestions saved to 'portrait_fixes.txt'")

if __name__ == "__main__":
    generate_report()
