#!/usr/bin/env python3
"""Batch patch all .bin map files: remove broken decals, external entities, dupes, fillers, fix music=null."""

import sys
import shutil
from pathlib import Path
from collections import Counter

VENV_SITE = Path(__file__).parent.parent / ".venv" / "Lib" / "site-packages"
if str(VENV_SITE) not in sys.path:
    sys.path.insert(0, str(VENV_SITE))

from loenn_mcp.celeste_bin import read_map, write_map, get_rooms

MAPS_ROOT = Path(__file__).parent.parent / "Maps"
BACKUP_SUFFIX = ".bin.backup"

BROKEN_DECAL_PREFIXES = (
    "nameguy/",
    "nameguy_farewell_celeste/",
    "nameguy_farewell/",
    "nameguy_floatystuff/",
    "nameguy_farewell_glitch/",
    "nameguy_farewell_touchswitchthing/",
    "monikadsidespack_",
    "monidsidespack_",
)

EXTERNAL_NAMES = {
    "Ingeste/CharaBoost",
    "triggerSpikesOriginalRight",
    "triggerSpikesOriginalDown",
    "triggerSpikesOriginalLeft",
    "triggerSpikesOriginalUp",
    "MoreDasheline/MaxDashTrigger",
    "SkinModHelper/SkinSwapTrigger",
    "rubysentities/heightdisplaytrigger",
    "rubysentities/fastoshirotrigger",
    "everest/CustomHeightDisplayTrigger",
    "everest/completeAreaTrigger",
    "MaxHelpingHand/FlagToggleCameraTargetTrigger",
    "ExtendedVariantMode/BooleanVanillaVariantTrigger",
}

EXTERNAL_PREFIXES = (
    "Ingeste/",
    "rubysentities/",
)


def is_broken_decal(texture: str) -> bool:
    return any(texture.startswith(p) for p in BROKEN_DECAL_PREFIXES)


def is_external(name: str) -> bool:
    return name in EXTERNAL_NAMES or any(name.startswith(p) for p in EXTERNAL_PREFIXES)


def patch_map(bin_path: Path, dry_run: bool = False):
    backup_path = bin_path.with_suffix(BACKUP_SUFFIX)
    if not backup_path.exists() and not dry_run:
        shutil.copy2(bin_path, backup_path)

    map_data = read_map(bin_path)
    rooms = get_rooms(map_data)
    room_names = {r.get("name", "") for r in rooms}

    # Collect referenced rooms
    referenced = set()
    for room in rooms:
        for child in room.get("__children", []):
            cname = child.get("__name", "")
            if cname in ("entities", "triggers"):
                for item in child.get("__children", []):
                    for k, v in item.items():
                        if isinstance(v, str) and v in room_names:
                            referenced.add(v)

    stats = Counter()

    # 1. Remove FILLER rooms (unreferenced)
    for map_child in map_data.get("__children", []):
        if map_child.get("__name") == "levels":
            new_children = []
            for lvl in map_child.get("__children", []):
                name = lvl.get("name", "")
                if name.startswith("FILLER") and name not in referenced:
                    stats["filler_removed"] += 1
                else:
                    new_children.append(lvl)
            map_child["__children"] = new_children
            break

    # Re-fetch rooms after filler removal
    rooms = get_rooms(map_data)

    for room in rooms:
        # 2. Fix music=null
        if room.get("music") == "null":
            if not dry_run:
                room["music"] = ""
            stats["music_null_fixed"] += 1

        for child in room.get("__children", []):
            cname = child.get("__name", "")

            # 3. Remove broken decals
            if cname == "fgdecals":
                before = len(child.get("__children", []))
                new = [d for d in child.get("__children", []) if not is_broken_decal(d.get("texture", ""))]
                removed = before - len(new)
                if removed:
                    if not dry_run:
                        child["__children"] = new
                    stats["broken_fg"] += removed
            elif cname == "bgdecals":
                before = len(child.get("__children", []))
                new = [d for d in child.get("__children", []) if not is_broken_decal(d.get("texture", ""))]
                removed = before - len(new)
                if removed:
                    if not dry_run:
                        child["__children"] = new
                    stats["broken_bg"] += removed
            elif cname == "decals":
                before = len(child.get("__children", []))
                new = [d for d in child.get("__children", []) if not is_broken_decal(d.get("texture", ""))]
                removed = before - len(new)
                if removed:
                    if not dry_run:
                        child["__children"] = new
                    stats["broken_fg"] += removed

            # 4. Remove external entities/triggers
            elif cname == "entities":
                before = len(child.get("__children", []))
                new = [e for e in child.get("__children", []) if not is_external(e.get("__name", ""))]
                removed = before - len(new)
                if removed:
                    if not dry_run:
                        child["__children"] = new
                    stats["external_entities"] += removed

                # 5. Remove duplicate entities (same name + position)
                seen = set()
                deduped = []
                dupes = 0
                for e in new:
                    key = (e.get("__name", ""), e.get("x", 0), e.get("y", 0))
                    if key in seen:
                        dupes += 1
                    else:
                        seen.add(key)
                        deduped.append(e)
                if dupes:
                    if not dry_run:
                        child["__children"] = deduped
                    stats["duplicates"] += dupes

            elif cname == "triggers":
                before = len(child.get("__children", []))
                new = [t for t in child.get("__children", []) if not is_external(t.get("__name", ""))]
                removed = before - len(new)
                if removed:
                    if not dry_run:
                        child["__children"] = new
                    stats["external_triggers"] += removed

    if not dry_run and any(stats.values()):
        try:
            write_map(bin_path, map_data)
        except PermissionError:
            print(f"  [SKIP] Permission denied: {bin_path.name}")
            return Counter()

    return stats


def main():
    dry_run = "--dry-run" in sys.argv
    all_bins = sorted(MAPS_ROOT.rglob("*.bin"))

    if dry_run:
        print("DRY RUN MODE - No changes will be saved")

    total_stats = Counter()
    patched_files = []

    for bin_path in all_bins:
        rel = str(bin_path.relative_to(MAPS_ROOT))
        stats = patch_map(bin_path, dry_run=dry_run)
        if any(stats.values()):
            patched_files.append((rel, stats))
            total_stats.update(stats)

    if not patched_files:
        print("No issues found in any map files.")
        return

    print(f"\n{'='*80}")
    print(f"Patched {len(patched_files)} files")
    print(f"{'='*80}")
    for rel, stats in patched_files:
        items = ", ".join(f"{k}={v}" for k, v in stats.items() if v)
        print(f"  {rel}: {items}")

    print(f"\n{'='*80}")
    print("TOTAL CHANGES")
    print(f"{'='*80}")
    for k, v in total_stats.most_common():
        print(f"  {k}: {v}")

    if not dry_run:
        print(f"\nBackups saved as *.bin.backup")


if __name__ == "__main__":
    main()
