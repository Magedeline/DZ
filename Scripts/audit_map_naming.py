#!/usr/bin/env python3
"""
Map Naming Schema Auditor for Desolo Zantas
Validates that map files follow the expected naming convention and align with Everest hooks.
Expected format: Maps/Maggy/{Side}/{NN}_{Name}.bin
  where Side = ASide, BSide, CSide, DSide, DXSide
        NN = two-digit chapter number
        Name = chapter name (e.g., City, Nightmare, Stars)
"""

import os
import re
from collections import defaultdict

MAPS_DIR = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS\Maps\Maggy"
EXPECTED_SIDES = ["ASide", "BSide", "CSide", "DSide", "DXSide"]
EXPECTED_CHAPTERS = [
    ("00", "Prologue"),
    ("01", "City"),
    ("02", "Nightmare"),
    ("03", "Stars"),
    ("04", "Legend"),
    ("05", "Restore"),
    ("06", "Stronghold"),
    ("07", "Hell"),
    ("08", "Truth"),
    ("09", "Summit"),
    ("10", "Ruins"),
    ("11", "Snow"),
    ("12", "Water"),
    ("13", "Fire"),
    ("14", "Digital"),
    ("15", "Castle"),
    ("16", "Corruption"),
    ("17", "Epilogue"),
    ("18", "Heart"),
    ("19", "Space"),
    ("20", "TheEnd"),
    ("21", "LastLevel"),
]

VALID_PATTERN = re.compile(r'^(\d{2})_([A-Za-z]+)\.bin$')

def audit_maps():
    issues = []
    stats = defaultdict(lambda: defaultdict(int))
    found_files = defaultdict(list)

    # Check all side folders
    for side in EXPECTED_SIDES:
        side_dir = os.path.join(MAPS_DIR, side)
        if not os.path.exists(side_dir):
            issues.append(f"MISSING: Side folder '{side}' does not exist")
            continue

        for filename in os.listdir(side_dir):
            if not filename.endswith('.bin'):
                continue

            match = VALID_PATTERN.match(filename)
            if not match:
                issues.append(f"INVALID_NAME: {side}/{filename} does not match expected pattern NN_Name.bin")
                continue

            num, name = match.groups()
            found_files[side].append((num, name))
            stats[side][num] += 1

    # Check consistency across sides
    print("=== DESOLO ZANTAS MAP NAMING AUDIT ===\n")

    # Check chapter completeness per side
    for side in EXPECTED_SIDES:
        if side not in found_files:
            continue

        found_nums = {num for num, _ in found_files[side]}
        expected_nums = {num for num, _ in EXPECTED_CHAPTERS}

        missing = expected_nums - found_nums
        extra = found_nums - expected_nums

        if missing:
            for num in sorted(missing):
                chapter_name = dict(EXPECTED_CHAPTERS).get(num, "Unknown")
                issues.append(f"MISSING_MAP: {side}/{num}_{chapter_name}.bin not found")

        if extra:
            for num in sorted(extra):
                names = [n for n, m in found_files[side] if n == num]
                issues.append(f"EXTRA_MAP: {side} has unexpected chapter number {num} ({', '.join(names)})")

    # Check naming consistency: same chapter number should have same name across all sides
    chapter_names = defaultdict(lambda: defaultdict(set))
    for side in EXPECTED_SIDES:
        for num, name in found_files.get(side, []):
            chapter_names[num][side].add(name)

    for num, side_names in chapter_names.items():
        all_names = set()
        for side, names in side_names.items():
            all_names.update(names)
        if len(all_names) > 1:
            issues.append(f"NAME_MISMATCH: Chapter {num} has inconsistent names across sides: {dict(side_names)}")

    # Check for irregular files
    aside_dir = os.path.join(MAPS_DIR, "ASide")
    if os.path.exists(aside_dir):
        for filename in os.listdir(aside_dir):
            if filename.endswith('.bin') and not VALID_PATTERN.match(filename):
                issues.append(f"IRREGULAR_ASIDE: ASide/{filename} does not match NN_Name.bin pattern")

    # Summary
    total_maps = sum(len(files) for files in found_files.values())
    print(f"Total map files found: {total_maps}")
    print(f"Side breakdown:")
    for side in EXPECTED_SIDES:
        count = len(found_files.get(side, []))
        print(f"  {side}: {count} maps")

    print(f"\n{'='*50}")
    print(f"Issues found: {len(issues)}")
    print(f"{'='*50}\n")

    if issues:
        for issue in sorted(issues):
            category = issue.split(':')[0]
            detail = issue.split(':', 1)[1] if ':' in issue else issue
            print(f"  [{category}] {detail}")
    else:
        print("  No issues found! Map naming schema is clean.")

    # Generate audit report
    report_path = os.path.join(os.path.dirname(__file__), 'map_audit_report.txt')
    with open(report_path, 'w') as f:
        f.write("=== DESOLO ZANTAS MAP NAMING AUDIT REPORT ===\n\n")
        f.write(f"Total map files: {total_maps}\n")
        for side in EXPECTED_SIDES:
            count = len(found_files.get(side, []))
            f.write(f"  {side}: {count} maps\n")
        f.write(f"\nIssues: {len(issues)}\n")
        for issue in sorted(issues):
            f.write(f"  {issue}\n")

    print(f"\nAudit report saved to: {report_path}")

if __name__ == "__main__":
    audit_maps()
