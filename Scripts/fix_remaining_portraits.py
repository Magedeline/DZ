#!/usr/bin/env python3
"""
Targeted Portrait Fix for remaining 1026 issues.
Creates expression mappings for characters that exist but are missing specific expressions.
Characters without any portrait directory are flagged for asset creation.
"""

import os
import re

DIALOG_FILE = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Dialog\English.txt"
PORTRAIT_DIR = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Graphics\Atlases\Portraits"
FIXED_DIALOG_FILE = DIALOG_FILE + ".fixed"

# Expression mapping: if an expression doesn't exist, try these fallbacks in order
EXPRESSION_FALLBACKS = {
    # Madeline variants
    "terrified": ["angry", "normal"],
    "desperate": ["angry", "normal"],
    "weak": ["angry", "normal"],
    "surpriesed": ["angry", "normal"],  # typo in original
    "surprised": ["angry", "normal"],
    "concerned": ["angry", "normal"],
    "distracted": ["angry", "normal"],
    
    # Ralsei
    "determinedclosed": ["angry", "normal"],
    "shocked": ["angry", "normal"],
    "panicked": ["angry", "normal"],
    
    # Magolor
    "surprised": ["happy", "normal"],
    "alarmed": ["happy", "normal"],
    "bewildered": ["happy", "normal"],
    "uneasy": ["happy", "normal"],
    "determined": ["happy", "normal"],
    "wtf": ["happy", "normal"],
    "worried": ["happy", "normal"],
    
    # Chara
    "disapointed": ["laughing", "normal"],  # typo in original
    "disappointed": ["laughing", "normal"],
    "shocked": ["laughing", "normal"],
    "remembering": ["happy", "normal"],
    "blinded": ["sad", "normal"],
    
    # Asriel
    "panic": ["sad", "normal"],
    "awkard": ["sad", "normal"],  # typo
    "awkward": ["sad", "normal"],
    
    # Flowey
    "shocked": ["makeadeal", "normal"],
    "scream": ["makeadeal", "normal"],
    "determined": ["makeadeal", "normal"],
    
    # Els
    "pissed": ["normal"],
    "none": ["normal"],
    
    # Kirby
    "surpried": ["angry", "normal"],  # typo
    
    # Generic fallbacks for any character
    "happy": ["normal"],
    "sad": ["normal"],
    "angry": ["normal"],
    "worried": ["normal"],
    "scared": ["normal"],
    "excited": ["happy", "normal"],
    "confused": ["normal"],
    "thinking": ["normal"],
    "tired": ["normal"],
    "smiling": ["happy", "normal"],
    "laughing": ["happy", "normal"],
    "crying": ["sad", "normal"],
    "shouting": ["angry", "normal"],
    "whispering": ["normal"],
    "yelling": ["angry", "normal"],
}

def get_available_expressions(character_dir):
    """Get list of available expressions for a character."""
    expressions = set()
    char_path = os.path.join(PORTRAIT_DIR, character_dir.lower())
    if not os.path.exists(char_path):
        return expressions
    for f in os.listdir(char_path):
        if f.endswith('.png'):
            # Extract expression name from file (e.g., "angry00.png" -> "angry")
            expr = re.match(r'([a-zA-Z]+)\d+\.png', f)
            if expr:
                expressions.add(expr.group(1).lower())
    return expressions

def fix_portraits():
    # Build character -> expressions map
    print("=== PORTRAIT TARGETED FIX ===\n")
    
    character_expressions = {}
    missing_characters = set()
    
    if os.path.exists(PORTRAIT_DIR):
        for char_dir in os.listdir(PORTRAIT_DIR):
            char_path = os.path.join(PORTRAIT_DIR, char_dir)
            if os.path.isdir(char_path):
                expressions = get_available_expressions(char_dir)
                if expressions:
                    character_expressions[char_dir.lower()] = expressions
    
    # Read dialog and find all portrait tags
    with open(DIALOG_FILE, 'r', encoding='utf-8') as f:
        content = f.read()
    
    portrait_pattern = re.compile(r'\[([A-Z][a-zA-Z_0-9]*)\s+(left|right|up|down|center|none)\s+([a-zA-Z_0-9]+)\]')
    matches = portrait_pattern.findall(content)
    
    fixes_applied = 0
    characters_needing_assets = set()
    
    for char_name, position, expression in matches:
        char_lower = char_name.lower()
        expr_lower = expression.lower()
        
        if char_lower not in character_expressions:
            characters_needing_assets.add(char_name)
            continue
        
        available = character_expressions[char_lower]
        if expr_lower in available:
            continue  # Already valid
        
        # Try to find a fallback expression
        fallback_found = None
        
        # Check specific fallbacks
        if expr_lower in EXPRESSION_FALLBACKS:
            for fallback in EXPRESSION_FALLBACKS[expr_lower]:
                if fallback in available:
                    fallback_found = fallback
                    break
        
        # Generic fallback: use any available expression
        if not fallback_found and available:
            # Prefer "normal" if available
            if "normal" in available:
                fallback_found = "normal"
            else:
                fallback_found = sorted(available)[0]
        
        if fallback_found:
            old_tag = f"[{char_name} {position} {expression}]"
            new_tag = f"[{char_name} {position} {fallback_found}]"
            content = content.replace(old_tag, new_tag)
            fixes_applied += content.count(new_tag)  # Approximate count
            print(f"  Fixed: {old_tag} -> {new_tag}")
    
    # Save fixed content
    with open(FIXED_DIALOG_FILE, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print(f"\n{'='*50}")
    print(f"Fixes applied: {fixes_applied}")
    print(f"Characters needing new portrait assets: {len(characters_needing_assets)}")
    
    if characters_needing_assets:
        print(f"\nCharacters without portrait directories:")
        for char_name in sorted(characters_needing_assets):
            print(f"  - {char_name}")
    
    print(f"\nFixed dialog saved to: {FIXED_DIALOG_FILE}")
    print(f"Review changes and rename to English.txt when ready.")

if __name__ == "__main__":
    fix_portraits()
