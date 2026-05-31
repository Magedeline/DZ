#!/usr/bin/env python3
"""
Build Verification Script for Desolo Zantas
Checks for common build-breaking issues before compilation.
"""

import os
import re
import sys

BASE_DIR = r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS"
SOURCE_DIR = os.path.join(BASE_DIR, "Source")

class BuildVerifier:
    def __init__(self):
        self.issues = []
        self.warnings = []

    def log_issue(self, category, message):
        self.issues.append(f"[{category}] {message}")

    def log_warning(self, category, message):
        self.warnings.append(f"[{category}] {message}")

    def check_namespace_conflicts(self):
        """Check for namespace/type name conflicts that cause ambiguity errors."""
        conflicts = {
            'Audio': ['Celeste.Audio', 'Celeste.Mod.MaggyHelper.Audio'],
            'Trigger': ['Celeste.Entities.Trigger', 'Monocle.Trigger'],
        }
        for filename in self._get_cs_files():
            content = self._read_file(filename)
            for type_name, namespaces in conflicts.items():
                if f'using {namespaces[0]}' in content and f'using {namespaces[1]}' in content:
                    self.log_warning("NAMESPACE", f"Potential conflict in {os.path.basename(filename)}: {type_name}")

    def check_missing_usings(self):
        """Check for common missing using directives."""
        for filename in self._get_cs_files():
            content = self._read_file(filename)
            # Check for Logger.Log without using
            if 'Logger.Log(' in content and 'using static Celeste.Mod.Logger' not in content:
                if 'Logger.Log' in content and 'using Monocle' in content:
                    pass  # Might be resolved via Monocle.Logger
                else:
                    self.log_warning("MISSING_USING", f"{os.path.basename(filename)} uses Logger.Log without obvious using directive")

    def check_hardcoded_paths(self):
        """Check for hardcoded file paths that might break on different systems."""
        path_pattern = re.compile(r'[rR]?"[A-Za-z]:\\\\[^"]+"')
        for filename in self._get_cs_files():
            content = self._read_file(filename)
            matches = path_pattern.findall(content)
            for match in matches:
                # Allow known safe paths
                if 'Program Files' in match or 'Steam' in match:
                    self.log_warning("HARDCODED_PATH", f"{os.path.basename(filename)}: {match[:60]}")

    def check_enum_casts(self):
        """Check for potentially unsafe enum casts."""
        cast_pattern = re.compile(r'\(int\)\s*\w+\s*\w+\s*==')
        for filename in self._get_cs_files():
            content = self._read_file(filename)
            if cast_pattern.search(content):
                self.log_warning("ENUM_CAST", f"{os.path.basename(filename)}: Check enum cast safety")

    def check_dialog_keys(self):
        """Verify all dialog keys referenced in C# exist in English.txt."""
        dialog_pattern = re.compile(r'"([A-Z_][A-Z_0-9]+)"')
        english_txt = self._read_file(os.path.join(BASE_DIR, "Dialog", "English.txt"))

        for filename in self._get_cs_files():
            content = self._read_file(filename)
            matches = dialog_pattern.findall(content)
            for key in matches:
                if len(key) > 10 and key.isupper() and key not in english_txt:
                    if not key.startswith("EVENT_") and not key.startswith("IMAGE_"):
                        self.log_warning("MISSING_DIALOG", f"{os.path.basename(filename)} references '{key}' not found in English.txt")

    def check_event_paths(self):
        """Verify FMOD event paths follow consistent naming."""
        event_pattern = re.compile(r'"(event:/[^"]+)"')
        for filename in self._get_cs_files():
            content = self._read_file(filename)
            matches = event_pattern.findall(content)
            for event in matches:
                if not event.startswith("event:/pusheen/") and not event.startswith("event:/game/") and not event.startswith("event:/ui/"):
                    self.log_warning("EVENT_PATH", f"{os.path.basename(filename)}: Unusual event path '{event}'")

    def check_nullable_issues(self):
        """Check for #nullable mismatches in files."""
        for filename in self._get_cs_files():
            content = self._read_file(filename)
            has_nullable_directive = '#nullable' in content
            has_nullable_annotations = '?' in content and ('string?' in content or 'object?' in content)
            if has_nullable_annotations and not has_nullable_directive:
                self.log_warning("NULLABLE", f"{os.path.basename(filename)}: Uses nullable annotations without #nullable directive")

    def check_unused_fields(self):
        """Check for potentially unused private fields."""
        for filename in self._get_cs_files():
            content = self._read_file(filename)
            field_pattern = re.compile(r'private\s+\w+\s+(\w+)\s*;')
            for match in field_pattern.finditer(content):
                field_name = match.group(1)
                # Simple heuristic: count usages
                usage_count = content.count(field_name) - 1  # subtract declaration
                if usage_count <= 0 and not field_name.startswith('_'):
                    self.log_warning("UNUSED_FIELD", f"{os.path.basename(filename)}: Field '{field_name}' may be unused")

    def run_all_checks(self):
        print("=== DESOLO ZANTAS BUILD VERIFICATION ===\n")
        self.check_namespace_conflicts()
        self.check_missing_usings()
        self.check_hardcoded_paths()
        self.check_enum_casts()
        self.check_dialog_keys()
        self.check_event_paths()
        self.check_nullable_issues()
        self.check_unused_fields()

        print(f"Issues: {len(self.issues)}")
        print(f"Warnings: {len(self.warnings)}\n")

        if self.issues:
            print("--- ISSUES ---")
            for issue in self.issues:
                print(f"  {issue}")

        if self.warnings:
            print("--- WARNINGS ---")
            for warning in self.warnings:
                print(f"  {warning}")

        if not self.issues and not self.warnings:
            print("  No issues found! Build looks clean.")

        # Write report
        report_path = os.path.join(BASE_DIR, "Scripts", "build_verify_report.txt")
        with open(report_path, 'w') as f:
            f.write("=== BUILD VERIFICATION REPORT ===\n\n")
            f.write(f"Issues: {len(self.issues)}\n")
            f.write(f"Warnings: {len(self.warnings)}\n\n")
            for issue in self.issues:
                f.write(f"  {issue}\n")
            for warning in self.warnings:
                f.write(f"  {warning}\n")

        print(f"\nReport saved to: {report_path}")
        return len(self.issues) == 0

    def _get_cs_files(self):
        for root, _, files in os.walk(SOURCE_DIR):
            for f in files:
                if f.endswith('.cs'):
                    yield os.path.join(root, f)

    def _read_file(self, path):
        try:
            with open(path, 'r', encoding='utf-8') as f:
                return f.read()
        except:
            return ""

if __name__ == "__main__":
    verifier = BuildVerifier()
    success = verifier.run_all_checks()
    sys.exit(0 if success else 1)
