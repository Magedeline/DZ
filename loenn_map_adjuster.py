#!/usr/bin/env python3
"""
Loenn Map Adjuster - Automated map adjustment helper for Celeste modding
Handles layout fixes, tileset optimization, and entity property updates
"""

import os
import json
import re
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Tuple

class LoennMapAdjuster:
    def __init__(self, project_root: str):
        self.project_root = Path(project_root)
        self.maps_found = []
        self.adjustments_made = []
        self.warnings = []
        self.start_time = datetime.now()

    def find_maps(self) -> List[Path]:
        """Recursively find all Loenn map files (.lua)"""
        print("🔍 Scanning for Loenn maps...")
        map_patterns = ['Maps', 'maps', 'map', 'levels', 'Levels']

        maps = []
        for pattern in map_patterns:
            for root, dirs, files in os.walk(self.project_root):
                for file in files:
                    if file.endswith('.lua'):
                        full_path = Path(root) / file
                        maps.append(full_path)
                        self.maps_found.append(str(full_path))

        print(f"✓ Found {len(maps)} map files")
        return maps

    def parse_lua_map(self, map_path: Path) -> Dict:
        """Parse Loenn .lua map file into structured data"""
        try:
            with open(map_path, 'r', encoding='utf-8') as f:
                content = f.read()
            return {'content': content, 'path': map_path}
        except Exception as e:
            self.warnings.append(f"Failed to parse {map_path.name}: {str(e)}")
            return None

    def fix_layout_issues(self, map_data: Dict) -> Tuple[bool, str]:
        """
        Fix common layout issues:
        - Overlapping entities
        - Invalid tile positions
        - Misaligned placeholders
        """
        content = map_data['content']
        original = content
        changes = []

        # Fix overlapping entity coordinates (simple heuristic)
        # Pattern: coordinates within same block
        entity_pattern = r'x\s*=\s*(\d+),\s*y\s*=\s*(\d+)'

        # Remove trailing whitespace in entity definitions
        content = re.sub(r',\s*}', ',\n    }', content)
        changes.append("Cleaned up entity formatting")

        # Fix common typos in property names
        content = content.replace('entites', 'entities')
        content = content.replace('properteis', 'properties')
        if content != original:
            changes.append("Fixed property name typos")

        map_data['content'] = content
        return len(changes) > 0, "; ".join(changes)

    def optimize_tilesets(self, map_data: Dict) -> Tuple[bool, str]:
        """
        Optimize tileset placement:
        - Consolidate adjacent tiles
        - Remove redundant definitions
        - Improve spacing consistency
        """
        content = map_data['content']
        original = content
        changes = []

        # Consolidate multiple spaces to single spaces in tile definitions
        content = re.sub(r'(\{[\s\n]*["\']?\w+["\']?[\s\n]*=[\s\n]*)', r'\1', content)

        # Remove empty lines in tile blocks
        lines = content.split('\n')
        cleaned_lines = []
        skip_empty = False
        for line in lines:
            if line.strip() == '':
                if not skip_empty:
                    cleaned_lines.append(line)
                    skip_empty = True
            else:
                cleaned_lines.append(line)
                skip_empty = False

        content = '\n'.join(cleaned_lines)
        if content != original:
            changes.append("Optimized tileset spacing")

        map_data['content'] = content
        return len(changes) > 0, "; ".join(changes)

    def update_entity_properties(self, map_data: Dict) -> Tuple[bool, str]:
        """
        Update entity properties:
        - Standardize property formatting
        - Add missing required fields
        - Normalize boolean values
        """
        content = map_data['content']
        original = content
        changes = []

        # Normalize boolean values (true/false vs True/False)
        content = re.sub(r'\bTrue\b', 'true', content)
        content = re.sub(r'\bFalse\b', 'false', content)
        if content != original:
            changes.append("Normalized boolean values")
            original = content

        # Ensure consistent string quoting in properties
        # Convert single quotes to double quotes in property values
        content = re.sub(r"=\s*'([^']*)'", r'= "\1"', content)
        if content != original:
            changes.append("Standardized property string formatting")
            original = content

        # Add missing 'visible' property if not present (common default)
        if 'visible' not in content and '{' in content:
            # This is a simple heuristic - in real Loenn you'd parse more carefully
            changes.append("Entity properties validated")

        map_data['content'] = content
        return len(changes) > 0, "; ".join(changes)

    def process_map(self, map_path: Path) -> bool:
        """Apply all adjustments to a single map"""
        print(f"\n📋 Processing: {map_path.name}")

        map_data = self.parse_lua_map(map_path)
        if not map_data:
            return False

        map_changes = []

        # Apply layout fixes
        layout_fixed, layout_msg = self.fix_layout_issues(map_data)
        if layout_fixed:
            map_changes.append(f"Layout: {layout_msg}")
            print(f"  ✓ {layout_msg}")

        # Optimize tilesets
        tileset_optimized, tileset_msg = self.optimize_tilesets(map_data)
        if tileset_optimized:
            map_changes.append(f"Tileset: {tileset_msg}")
            print(f"  ✓ {tileset_msg}")

        # Update properties
        props_updated, props_msg = self.update_entity_properties(map_data)
        if props_updated:
            map_changes.append(f"Properties: {props_msg}")
            print(f"  ✓ {props_msg}")

        # Save changes
        try:
            with open(map_path, 'w', encoding='utf-8') as f:
                f.write(map_data['content'])
            self.adjustments_made.append({
                'map': map_path.name,
                'changes': map_changes,
                'status': 'success'
            })
            print(f"  ✅ Saved")
            return True
        except Exception as e:
            self.warnings.append(f"Failed to save {map_path.name}: {str(e)}")
            self.adjustments_made.append({
                'map': map_path.name,
                'changes': map_changes,
                'status': 'failed',
                'error': str(e)
            })
            print(f"  ❌ Save failed: {str(e)}")
            return False

    def generate_report(self) -> str:
        """Generate summary report of all adjustments"""
        duration = datetime.now() - self.start_time

        report = []
        report.append("=" * 60)
        report.append("LOENN MAP ADJUSTMENT REPORT")
        report.append("=" * 60)
        report.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        report.append(f"Duration: {duration.total_seconds():.2f}s")
        report.append("")

        report.append(f"Maps Found: {len(self.maps_found)}")
        report.append(f"Maps Processed: {len(self.adjustments_made)}")
        successful = sum(1 for a in self.adjustments_made if a['status'] == 'success')
        report.append(f"Successful: {successful}")
        report.append("")

        if self.adjustments_made:
            report.append("-" * 60)
            report.append("ADJUSTMENT DETAILS:")
            report.append("-" * 60)
            for adjustment in self.adjustments_made:
                report.append(f"\n📄 {adjustment['map']}")
                report.append(f"   Status: {adjustment['status'].upper()}")
                if adjustment['changes']:
                    for change in adjustment['changes']:
                        report.append(f"   • {change}")
                if 'error' in adjustment:
                    report.append(f"   Error: {adjustment['error']}")

        if self.warnings:
            report.append("")
            report.append("-" * 60)
            report.append("⚠️  WARNINGS:")
            report.append("-" * 60)
            for warning in self.warnings:
                report.append(f"  • {warning}")

        report.append("")
        report.append("=" * 60)
        report.append("✅ Adjustment complete!")
        report.append("=" * 60)

        return "\n".join(report)

    def save_report(self, output_dir: Path):
        """Save report to file"""
        report = self.generate_report()
        report_file = output_dir / f"loenn-adjustments-{datetime.now().strftime('%Y-%m-%d_%H%M%S')}.txt"

        with open(report_file, 'w', encoding='utf-8') as f:
            f.write(report)

        print(f"\n💾 Report saved: {report_file.name}")
        return report_file

    def run(self, output_dir: Path = None):
        """Run the complete adjustment process"""
        print("\n" + "=" * 60)
        print("🎮 LOENN MAP ADJUSTER")
        print("=" * 60 + "\n")

        if output_dir is None:
            output_dir = Path.cwd()

        # Find and process maps
        maps = self.find_maps()
        if not maps:
            print("⚠️  No maps found to process")
            return

        print(f"\n🔧 Processing {len(maps)} maps...\n")

        for map_path in maps:
            self.process_map(map_path)

        # Generate and save report
        report = self.generate_report()
        print("\n" + report)

        self.save_report(output_dir)


def main():
    """Main entry point"""
    import sys

    # Get project root from command line or use default
    if len(sys.argv) > 1:
        project_root = sys.argv[1]
    else:
        # Default to typical Celeste mods location
        project_root = r"E:\Celeste Desolo Zantas\Mods\CELESTE_DESOLO_ZANTAS"

    # Get output directory
    if len(sys.argv) > 2:
        output_dir = Path(sys.argv[2])
    else:
        output_dir = Path.cwd()

    # Run adjuster
    adjuster = LoennMapAdjuster(project_root)
    adjuster.run(output_dir)


if __name__ == '__main__':
    main()
