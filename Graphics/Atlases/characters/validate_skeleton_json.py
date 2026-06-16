#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Skeleton JSON Validator for KIRBY_CELESTE
Validates skeleton JSON files for format compliance and completeness
"""

import json
import sys
from pathlib import Path

# Fix encoding for Windows
if sys.platform == 'win32':
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')


def validate_skeleton_json(filepath):
    """Validate a skeleton JSON file"""
    print(f"\n{'='*60}")
    print(f"Validating: {filepath}")
    print('='*60)

    # Check file exists
    path = Path(filepath)
    if not path.exists():
        print(f"❌ ERROR: File does not exist: {filepath}")
        return False

    if not path.is_file():
        print(f"❌ ERROR: Not a file: {filepath}")
        return False

    # Check file size
    size = path.stat().st_size
    print(f"✓ File exists ({size} bytes)")

    if size == 0:
        print(f"❌ ERROR: File is empty")
        return False

    # Try to load JSON
    try:
        with open(filepath, 'r') as f:
            data = json.load(f)
        print(f"✓ Valid JSON format")
    except json.JSONDecodeError as e:
        print(f"❌ ERROR: Invalid JSON - {e}")
        return False
    except Exception as e:
        print(f"❌ ERROR: Cannot read file - {e}")
        return False

    # Validate structure
    required_fields = ['sprite_width', 'sprite_height', 'name', 'x', 'y', 'children']
    missing = []

    for field in required_fields:
        if field not in data:
            missing.append(field)

    if missing:
        print(f"❌ ERROR: Missing required fields: {missing}")
        return False

    print(f"✓ Has required root fields")

    # Validate dimensions
    width = data.get('sprite_width')
    height = data.get('sprite_height')

    if not isinstance(width, int) or not isinstance(height, int):
        print(f"❌ ERROR: sprite_width and sprite_height must be integers")
        return False

    if width <= 0 or height <= 0:
        print(f"❌ ERROR: sprite dimensions must be positive")
        return False

    print(f"✓ Canvas: {width}x{height}")

    # Validate bone structure
    def validate_bone(bone, parent_name=""):
        issues = []

        bone_name = bone.get('name', '?')
        bone_x = bone.get('x')
        bone_y = bone.get('y')

        # Check required fields
        if not bone_name:
            issues.append(f"Bone has no name")
            return issues

        if bone_x is None or bone_y is None:
            issues.append(f"Bone '{bone_name}' missing x or y coordinate")
            return issues

        if not isinstance(bone_x, (int, float)) or not isinstance(bone_y, (int, float)):
            issues.append(f"Bone '{bone_name}' coordinates must be numbers")
            return issues

        # Check bounds
        if bone_x < 0 or bone_x >= width:
            issues.append(f"Bone '{bone_name}' X={bone_x} out of bounds (0-{width-1})")

        if bone_y < 0 or bone_y >= height:
            issues.append(f"Bone '{bone_name}' Y={bone_y} out of bounds (0-{height-1})")

        # Check children
        children = bone.get('children', [])
        if not isinstance(children, list):
            issues.append(f"Bone '{bone_name}' children must be a list")
            return issues

        for child in children:
            child_issues = validate_bone(child, bone_name)
            issues.extend(child_issues)

        return issues

    # Validate root bone
    root_issues = validate_bone(data)

    if root_issues:
        print(f"❌ ISSUES FOUND ({len(root_issues)}):")
        for issue in root_issues:
            print(f"   - {issue}")
        return False

    # Count bones
    def count_bones(bone):
        count = 1
        for child in bone.get('children', []):
            count += count_bones(child)
        return count

    total_bones = count_bones(data)
    print(f"✓ Total bones: {total_bones}")

    # Print bone tree
    def print_tree(bone, indent=0):
        name = bone.get('name', '?')
        x = bone.get('x', '?')
        y = bone.get('y', '?')
        print(f"{'  ' * indent}- {name} ({x}, {y})")
        for child in bone.get('children', []):
            print_tree(child, indent + 1)

    print(f"\n✓ Bone hierarchy:")
    print_tree(data)

    print(f"\n{'='*60}")
    print(f"✅ VALIDATION PASSED")
    print('='*60)
    return True


def validate_all_skeletons(directory):
    """Validate all skeleton JSON files in directory"""
    dir_path = Path(directory)

    if not dir_path.is_dir():
        print(f"Directory not found: {directory}")
        return

    json_files = list(dir_path.glob("*_skeleton.json"))

    if not json_files:
        print(f"No skeleton JSON files found in {directory}")
        return

    print(f"\nFound {len(json_files)} skeleton file(s)")

    passed = 0
    failed = 0

    for json_file in sorted(json_files):
        if validate_skeleton_json(str(json_file)):
            passed += 1
        else:
            failed += 1

    print(f"\n{'='*60}")
    print(f"SUMMARY: {passed} passed, {failed} failed")
    print('='*60)


def fix_skeleton_json(filepath, output_path=None):
    """Attempt to fix common JSON issues"""
    print(f"\nAttempting to fix: {filepath}")

    try:
        with open(filepath, 'r') as f:
            content = f.read()

        # Try to parse
        data = json.loads(content)

        # Re-save with proper formatting
        output = output_path or filepath
        with open(output, 'w') as f:
            json.dump(data, f, indent=2)

        print(f"✓ Fixed and saved to: {output}")
        return True

    except Exception as e:
        print(f"❌ Cannot fix: {e}")
        return False


if __name__ == "__main__":
    import sys

    if len(sys.argv) > 1:
        arg = sys.argv[1]

        if arg == "--all":
            # Validate all in current directory
            validate_all_skeletons(".")
        elif arg == "--fix":
            # Fix a file
            if len(sys.argv) > 2:
                fix_skeleton_json(sys.argv[2])
            else:
                print("Usage: python validate_skeleton_json.py --fix <filepath>")
        else:
            # Validate single file
            validate_skeleton_json(arg)
    else:
        print("Skeleton JSON Validator")
        print("\nUsage:")
        print("  python validate_skeleton_json.py <filepath>  - Validate single file")
        print("  python validate_skeleton_json.py --all       - Validate all *_skeleton.json")
        print("  python validate_skeleton_json.py --fix <file> - Attempt to fix issues")
        print("\nExample:")
        print("  python validate_skeleton_json.py sans_npc_skeleton.json")
        print("  python validate_skeleton_json.py --all")
