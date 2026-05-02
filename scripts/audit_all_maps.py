#!/usr/bin/env python3
"""Audit all .bin map files in the mod for common issues."""

import sys
import shutil
from pathlib import Path
from collections import Counter, defaultdict

VENV_SITE = Path(__file__).parent.parent / ".venv" / "Lib" / "site-packages"
if str(VENV_SITE) not in sys.path:
    sys.path.insert(0, str(VENV_SITE))

from loenn_mcp.celeste_bin import read_map, get_rooms

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
MAPS_ROOT = Path(__file__).parent.parent / "Maps"
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
    "MoreDasheline/MaxDashTrigger",
    "SkinModHelper/SkinSwapTrigger",
    "rubysentities/heightdisplaytrigger",
    "rubysentities/fastoshirotrigger",
    "everest/CustomHeightDisplayTrigger",
    "everest/completeAreaTrigger",
    "MaxHelpingHand/FlagToggleCameraTargetTrigger",
    "ExtendedVariantMode/BooleanVanillaVariantTrigger",
}


def is_broken_decal(texture: str) -> bool:
    return any(texture.startswith(p) for p in BROKEN_DECAL_PREFIXES)


def is_external(name: str) -> bool:
    return name in EXTERNAL_NAMES


def audit_map(bin_path: Path):
    issues = []
    try:
        map_data = read_map(bin_path)
        rooms = get_rooms(map_data)
    except Exception as e:
        return [("ERROR", f"Failed to read map: {e}")]

    broken_decals_fg = 0
    broken_decals_bg = 0
    external_entities = 0
    external_triggers = 0
    empty_music = 0
    music_null = 0
    duplicate_entities = 0
    rooms_with_dupes = 0
    filler_rooms = 0

    for room in rooms:
        name = room.get("name", "")

        # Check music
        music = room.get("music", "")
        if not music:
            empty_music += 1
        if music == "null":
            music_null += 1

        # Check decals
        for child in room.get("__children", []):
            cname = child.get("__name", "")
            if cname == "fgdecals":
                for d in child.get("__children", []):
                    if is_broken_decal(d.get("texture", "")):
                        broken_decals_fg += 1
            elif cname == "bgdecals":
                for d in child.get("__children", []):
                    if is_broken_decal(d.get("texture", "")):
                        broken_decals_bg += 1
            elif cname == "decals":
                for d in child.get("__children", []):
                    if is_broken_decal(d.get("texture", "")):
                        broken_decals_fg += 1
            elif cname == "entities":
                # Check external entities
                for e in child.get("__children", []):
                    if is_external(e.get("__name", "")):
                        external_entities += 1
                # Check duplicates
                seen = set()
                dupes = 0
                for e in child.get("__children", []):
                    key = (e.get("__name", ""), e.get("x", 0), e.get("y", 0))
                    if key in seen:
                        dupes += 1
                    seen.add(key)
                if dupes:
                    duplicate_entities += dupes
                    rooms_with_dupes += 1
            elif cname == "triggers":
                for t in child.get("__children", []):
                    if is_external(t.get("__name", "")):
                        external_triggers += 1

        # Check filler rooms
        if name.startswith("FILLER"):
            filler_rooms += 1

    return {
        "rooms": len(rooms),
        "broken_fg": broken_decals_fg,
        "broken_bg": broken_decals_bg,
        "external_entities": external_entities,
        "external_triggers": external_triggers,
        "empty_music": empty_music,
        "music_null": music_null,
        "duplicate_entities": duplicate_entities,
        "rooms_with_dupes": rooms_with_dupes,
        "filler_rooms": filler_rooms,
    }


def main():
    all_bins = sorted(MAPS_ROOT.rglob("*.bin"))
    print(f"Auditing {len(all_bins)} map files...\n")

    overall = Counter()
    files_with_issues = []

    for bin_path in all_bins:
        rel = str(bin_path.relative_to(MAPS_ROOT))
        result = audit_map(bin_path)
        if isinstance(result, list):  # Error
            print(f"[FAIL] {rel}: {result[0][1]}")
            continue

        has_issues = any(v for k, v in result.items() if k != "rooms")
        if has_issues:
            files_with_issues.append((rel, result))

    # Print summary
    print("=" * 80)
    print("AUDIT SUMMARY - Files with issues")
    print("=" * 80)

    if not files_with_issues:
        print("No issues found in any map files!")
        return

    for rel, result in files_with_issues:
        print(f"\n{rel}")
        print(f"  Rooms: {result['rooms']}")
        if result["broken_fg"]:
            print(f"  Broken FG decals: {result['broken_fg']}")
        if result["broken_bg"]:
            print(f"  Broken BG decals: {result['broken_bg']}")
        if result["external_entities"]:
            print(f"  External entities: {result['external_entities']}")
        if result["external_triggers"]:
            print(f"  External triggers: {result['external_triggers']}")
        if result["empty_music"]:
            print(f"  Empty music: {result['empty_music']}")
        if result["music_null"]:
            print(f"  Music=null: {result['music_null']}")
        if result["duplicate_entities"]:
            print(f"  Duplicate entities: {result['duplicate_entities']} in {result['rooms_with_dupes']} rooms")
        if result["filler_rooms"]:
            print(f"  Filler rooms: {result['filler_rooms']}")

    print("\n" + "=" * 80)
    print(f"Total files with issues: {len(files_with_issues)}")


if __name__ == "__main__":
    main()
