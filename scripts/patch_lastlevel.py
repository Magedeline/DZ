#!/usr/bin/env python3
"""
Patch lastlevel.bin for MaggyHelper mod.

Actions:
1. Backup original map
2. Remove unused FILLER* rooms (not referenced by any transitions)
3. Remove broken/missing decals (nameguy*, monikadsidespack*, monidsidespack*)
4. Remove external mod entities and triggers to make map "as original as possible"
5. Organize remaining rooms alphabetically
6. Save patched map
"""

import shutil
from pathlib import Path
from collections import Counter

# Add loenn_mcp to path (in case running outside venv)
import sys
VENV_SITE = Path(__file__).parent.parent / ".venv" / "Lib" / "site-packages"
if str(VENV_SITE) not in sys.path:
    sys.path.insert(0, str(VENV_SITE))

from loenn_mcp.celeste_bin import read_map, write_map, get_rooms

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
MAP_PATH = Path(__file__).parent.parent / "Maps" / "Maggy" / "ASide" / "lastlevel.bin"
BACKUP_PATH = MAP_PATH.with_suffix(".bin.backup")

# Decal prefixes to remove entirely (textures missing from mod)
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

# External entity/trigger names to remove
EXTERNAL_NAMES = {
    # Entities
    "Ingeste/CharaBoost",
    "triggerSpikesOriginalRight",
    "triggerSpikesOriginalDown",
    # Triggers
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


def remove_fillers(map_data: dict) -> int:
    """Remove FILLER* rooms that are unreferenced."""
    rooms = get_rooms(map_data)
    room_names = {r.get("name", "") for r in rooms}

    # Collect all room name references in entities and triggers
    referenced = set()
    for room in rooms:
        for child in room.get("__children", []):
            cname = child.get("__name", "")
            if cname in ("entities", "triggers"):
                for item in child.get("__children", []):
                    for k, v in item.items():
                        if isinstance(v, str) and v in room_names:
                            referenced.add(v)

    filler_names = {n for n in room_names if n.startswith("FILLER")}
    safe_to_remove = filler_names - referenced
    removed = 0

    # Find the 'levels' child in map_data and filter its children
    for map_child in map_data.get("__children", []):
        if map_child.get("__name") == "levels":
            new_children = []
            for lvl in map_child.get("__children", []):
                lvl_name = lvl.get("name", "")
                if lvl_name in safe_to_remove:
                    removed += 1
                else:
                    new_children.append(lvl)
            map_child["__children"] = new_children
            break

    return removed


def patch_decals(room: dict) -> tuple[int, int]:
    """Remove broken decals from a room. Returns (removed_fg, removed_bg)."""
    removed_fg = 0
    removed_bg = 0
    for child in room.get("__children", []):
        cname = child.get("__name", "")
        if cname == "fgdecals":
            before = len(child.get("__children", []))
            child["__children"] = [d for d in child.get("__children", []) if not is_broken_decal(d.get("texture", ""))]
            removed_fg = before - len(child["__children"])
        elif cname == "bgdecals":
            before = len(child.get("__children", []))
            child["__children"] = [d for d in child.get("__children", []) if not is_broken_decal(d.get("texture", ""))]
            removed_bg = before - len(child["__children"])
        elif cname == "decals":
            before = len(child.get("__children", []))
            child["__children"] = [d for d in child.get("__children", []) if not is_broken_decal(d.get("texture", ""))]
            # 'decals' generic - count as fg for simplicity
            removed_fg += before - len(child["__children"])
    return removed_fg, removed_bg


def patch_entities_and_triggers(room: dict) -> tuple[int, int]:
    """Remove external entities/triggers. Returns (removed_entities, removed_triggers)."""
    removed_entities = 0
    removed_triggers = 0
    for child in room.get("__children", []):
        cname = child.get("__name", "")
        if cname == "entities":
            before = len(child.get("__children", []))
            child["__children"] = [e for e in child.get("__children", []) if not is_external(e.get("__name", ""))]
            removed_entities = before - len(child["__children"])
        elif cname == "triggers":
            before = len(child.get("__children", []))
            child["__children"] = [t for t in child.get("__children", []) if not is_external(t.get("__name", ""))]
            removed_triggers = before - len(child["__children"])
    return removed_entities, removed_triggers


def sort_rooms(map_data: dict) -> None:
    """Sort rooms alphabetically by name within the levels container."""
    for map_child in map_data.get("__children", []):
        if map_child.get("__name") == "levels":
            children = map_child.get("__children", [])
            children.sort(key=lambda r: r.get("name", ""))
            map_child["__children"] = children
            break


def main():
    print("=" * 60)
    print("Patching lastlevel.bin")
    print("=" * 60)

    # 1. Backup
    if not BACKUP_PATH.exists():
        shutil.copy2(MAP_PATH, BACKUP_PATH)
        print(f"[OK] Backed up original to {BACKUP_PATH.name}")
    else:
        print(f"[SKIP] Backup already exists: {BACKUP_PATH.name}")

    # 2. Load
    print(f"[INFO] Loading {MAP_PATH} ...")
    map_data = read_map(MAP_PATH)
    rooms_before = len(get_rooms(map_data))
    print(f"[INFO] Rooms before: {rooms_before}")

    # 3. Remove FILLER rooms
    removed_rooms = remove_fillers(map_data)
    print(f"[OK] Removed {removed_rooms} FILLER rooms")

    # 4. Patch decals & entities/triggers per room
    rooms = get_rooms(map_data)
    total_removed_fg = 0
    total_removed_bg = 0
    total_removed_entities = 0
    total_removed_triggers = 0

    for room in rooms:
        fg, bg = patch_decals(room)
        total_removed_fg += fg
        total_removed_bg += bg
        re, rt = patch_entities_and_triggers(room)
        total_removed_entities += re
        total_removed_triggers += rt

    print(f"[OK] Removed {total_removed_fg} broken fg decals")
    print(f"[OK] Removed {total_removed_bg} broken bg decals")
    print(f"[OK] Removed {total_removed_entities} external entities")
    print(f"[OK] Removed {total_removed_triggers} external triggers")

    # 5. Sort rooms
    sort_rooms(map_data)
    print("[OK] Rooms sorted alphabetically")

    # 6. Save
    print(f"[INFO] Saving patched map to {MAP_PATH} ...")
    write_map(MAP_PATH, map_data)

    rooms_after = len(get_rooms(map_data))
    print("=" * 60)
    print("SUMMARY")
    print("=" * 60)
    print(f"  Rooms:       {rooms_before} -> {rooms_after} ({rooms_before - rooms_after} removed)")
    print(f"  FG decals:   {total_removed_fg} removed")
    print(f"  BG decals:   {total_removed_bg} removed")
    print(f"  Entities:    {total_removed_entities} removed")
    print(f"  Triggers:    {total_removed_triggers} removed")
    print("=" * 60)
    print("[DONE] lastlevel.bin patched successfully!")
    print("[NOTE] If anything breaks, restore from .bin.backup")


if __name__ == "__main__":
    main()
