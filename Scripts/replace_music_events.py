#!/usr/bin/env python3
"""
Replace old GUID-based music events with new event:/ paths in Celeste .bin map files.
Also handles Lua files for Loenn entities.

Usage:
    python replace_music_events.py [--dry-run]
"""

import os
import re
from pathlib import Path
from typing import Dict, List, Tuple

# Mapping of old GUIDs to new event paths
# Based on the FMOD Studio GUID export
GUID_TO_EVENT = {
    # Badeline
    "ad25a031-880b-4f88-ac12-82d6c52fbdea": "event:/char/badeline/appear",
    "7a71fc61-b688-4d82-a2b7-781c2434e942": "event:/char/badeline/booster_begin",
    "b2ee45d8-e1b0-43f1-baaa-f6e77d37ddb5": "event:/char/badeline/booster_final",
    "207a212a-2beb-4022-a8da-11ac987d3097": "event:/char/badeline/booster_reappear",
    "7e3c6b4e-e41a-4e9a-9d24-22737c66593b": "event:/char/badeline/booster_relocate",
    "2592d638-f3c5-4f6f-81e0-888a04affa40": "event:/char/badeline/booster_throw",
    "bbe7d0c8-45f9-4671-b49e-8e38e357bb81": "event:/char/badeline/boss_bullet",
    "898fc1ea-250b-4d03-8f37-c0c35799a915": "event:/char/badeline/boss_hug",
    "b4002010-ff11-4311-87c7-f87028220f78": "event:/char/badeline/boss_idle_air",
    "e7a2ddd6-091a-44ab-aac7-e57bd13c009c": "event:/char/badeline/boss_laser_charge",
    "58b5d825-ebcf-457b-b493-9be82640b9eb": "event:/char/badeline/boss_laser_fire",
    "646b0a01-2101-424f-804b-18842f72a62d": "event:/char/badeline/boss_prefight_getup",
    "33df1e8a-7605-4b2d-8235-ee78fbaa8c55": "event:/char/badeline/climb_ledge",
    "b54ae334-409b-4bba-82e2-62753f764f90": "event:/char/badeline/dash_red_left",
    "50356124-434a-4dca-aeb1-52462846fb8a": "event:/char/badeline/dash_red_right",
    "16b40879-0a79-4e42-8c91-fe419a8e186c": "event:/char/badeline/disappear",
    "937c3941-eeb8-4f1f-8451-765b33545202": "event:/char/badeline/dreamblock_enter",
    "3b142d62-975a-41a3-9b59-8e5ba4f6cbdb": "event:/char/badeline/dreamblock_exit",
    "697102c2-9978-42b7-a3b0-0c2b99c9032e": "event:/char/badeline/dreamblock_travel",
    "d879f9dd-98d0-479e-9f08-a1848f4a0f5c": "event:/char/badeline/duck",
    "ff6442f1-66c2-4dba-8d57-038dda8fd296": "event:/char/badeline/footstep",
    "8824489f-1d0b-4670-beb2-08d67cfd3c3b": "event:/char/badeline/grab",
    "c122d904-fb7a-4bfe-99a1-f5a4f8701d17": "event:/char/badeline/grab_letgo",
    "c2cd2f17-045c-4312-b7f6-8e7b750577c0": "event:/char/badeline/handhold",
    "95ce0255-9a27-47b4-a902-fb25c0f2a2e2": "event:/char/badeline/jump",
    "64424649-d82c-4efc-90a9-f54e8411d9b7": "event:/char/badeline/jump_assisted",
    "58e689a2-1717-434f-9fa7-dffcd72ab27e": "event:/char/badeline/jump_climb_left",
    "d9a77828-0390-434a-8cbc-e3df56a40cc4": "event:/char/badeline/jump_climb_right",
    "6faecb61-f82e-400e-9785-4d407a82c838": "event:/char/badeline/jump_dreamblock",
    "bc8a6051-399a-4f76-a865-02b53c5fe8ab": "event:/char/badeline/jump_special",
    "73a21bdf-62c2-46d8-ae04-68e6c0c26b7f": "event:/char/badeline/jump_super",
    "7ca46073-e6b8-4f74-b9a8-ac54a8966c5b": "event:/char/badeline/jump_superslide",
    "2336b17f-5769-4e2e-a636-aee1ddef9086": "event:/char/badeline/jump_superwall",
    "ab3be2a7-36d0-43cb-990a-2c49ba5d5c60": "event:/char/badeline/jump_wall_left",
    "3e5b18db-8f99-4ab7-ad08-8529fd257b6b": "event:/char/badeline/jump_wall_right",
    "8f924592-8b14-40d6-81af-d42fab0b6da1": "event:/char/badeline/landing",
    "25294859-6bfd-434e-9528-8433245fe3b5": "event:/char/badeline/level_entry",
    "38557af4-adf9-4328-9f2c-167b12ff9f8e": "event:/char/badeline/maddy_join",
    "450fb5b3-e9e3-45d8-9f34-ba05e292958f": "event:/char/badeline/maddy_split",
    "1a114663-4b93-4aab-ba8c-ca8793f2831e": "event:/char/badeline/stand",
    "97d0d2e4-92f5-48cf-948f-4fc85b0a0791": "event:/char/badeline/temple_move_chats",
    "a5ceb7fd-8a03-4c79-8d4f-81ad37400f43": "event:/char/badeline/temple_move_first",
    "a2443155-5af1-4e19-80d5-a81a3d9cf06d": "event:/char/badeline/wallslide",
    
    # Madeline
    "75ab6aef-3f69-4776-b60f-5700451cc9a2": "event:/char/madeline/backpack_drop",
    "99373af6-beba-488d-b516-ff48fd95e8b8": "event:/char/madeline/campfire_sit",
    "9435a742-7595-48a4-b2af-2757c4f927da": "event:/char/madeline/campfire_stand",
    "9457c17c-688b-4701-b2ae-b89d821d54f7": "event:/char/madeline/climb_ledge",
    "82f8448b-9452-4b12-838c-dcd81098476e": "event:/char/madeline/core_hair_charged",
    "315ac151-5eb6-4e6b-b1b7-0531f3572c44": "event:/char/madeline/crystaltheo_lift",
    "eb1249df-06cf-434d-991b-0b73464eef06": "event:/char/madeline/crystaltheo_throw",
    "cb220baa-d2cb-4a87-9907-a6595dba6735": "event:/char/madeline/dash_pink_left",
    "8ec31009-771f-4473-a3fc-45b209d6aa87": "event:/char/madeline/dash_pink_right",
    "b9d3a7c5-3d49-4b8a-aad1-fcfaf87293af": "event:/char/madeline/dash_red_left",
    "066cd550-a394-4bed-a4cf-c71905a160ed": "event:/char/madeline/dash_red_right",
    "ae79fcef-d9b4-44c5-91e5-e0348229e9d5": "event:/char/madeline/death",
    "06d7cbaf-9d9e-4f67-8adb-37a599dd4c37": "event:/char/madeline/dreamblock_enter",
    "0a9527ad-360b-4cd2-b585-92b066720b34": "event:/char/madeline/dreamblock_exit",
    "47bc2416-fd95-4a75-8952-7c43757e0e66": "event:/char/madeline/dreamblock_travel",
    "0d6cb459-91af-4842-8d2b-0f0e103313ef": "event:/char/madeline/duck",
    "73ada323-e77f-43f7-b0df-a8facdef03b7": "event:/char/madeline/footstep",
    "8aa6ab7b-3104-4a06-8e7e-b1141ac0d23d": "event:/char/madeline/grab",
    "0e0bb484-9840-47f8-a697-773134e4bedb": "event:/char/madeline/grab_letgo",
    "7f8c23d1-bd9d-4a10-b8af-fd4a5cb12244": "event:/char/madeline/handhold",
    "421fe5d0-9d4b-40a5-ab8e-23b1ec4bf1b3": "event:/char/madeline/idle_crackknuckles",
    "852ea39f-c182-4385-9aa3-1d95cb02040b": "event:/char/madeline/idle_scratch",
    "ae4aea88-f499-49e8-9536-78c4bad21743": "event:/char/madeline/idle_sneeze",
    "eeede5f5-3691-4cd9-8b2c-91e02d3d41ed": "event:/char/madeline/jump",
    "73ba5692-b6bd-4c78-a392-e41fa7425ead": "event:/char/madeline/jump_assisted",
    "faac7cb1-bc6a-4e0d-8dc5-97f3c94363e6": "event:/char/madeline/jump_climb_left",
    "1a77f0d4-81d8-4f97-bdf1-8e1e39cc69ef": "event:/char/madeline/jump_climb_right",
    "5e5a1f06-5cf9-4daf-a7ed-3b36554dd44b": "event:/char/madeline/jump_dreamblock",
    "25759b99-ad2a-4483-bb7e-0c4eac782c53": "event:/char/madeline/jump_special",
    "4fc69f31-fc0f-42d3-af4c-20424c74d6a4": "event:/char/madeline/jump_super",
    "8f5b7d80-8ca1-4415-942c-f1338b8a79f1": "event:/char/madeline/jump_superslide",
    "67bb774a-41bd-4375-a4e7-53031c7f75cd": "event:/char/madeline/jump_superwall",
    "f63c2b0a-f708-44f5-af13-6bba7e3af2cb": "event:/char/madeline/jump_wall_left",
    "3b128250-55a7-4a1b-8d8e-edf325d80192": "event:/char/madeline/jump_wall_right",
    "a7289b79-f525-4762-943e-98cf5c94151a": "event:/char/madeline/landing",
    "60e29264-3711-498c-a7ed-c58ecefb5543": "event:/char/madeline/mirrortemple_big_landing",
    "65659bc0-3d2b-429c-9bdc-42e7a2d29a94": "event:/char/madeline/predeath",
    "14d582cb-0c50-4837-bcc5-e7e03ff23687": "event:/char/madeline/revive",
    "73265a98-2cb3-4d98-9643-cf592c2b9131": "event:/char/madeline/stand",
    "7d18d617-475b-4d12-ad45-885c619e7540": "event:/char/madeline/summit_areastart",
    "72eb7f2d-c3e9-41b2-afc0-b4cfee087389": "event:/char/madeline/summit_flytonext",
    "f5babe6c-1ec1-4d4e-8934-a1809b5e26cb": "event:/char/madeline/summit_sit",
    "dae064c8-86f9-458b-83af-fbd8b699a3b9": "event:/char/madeline/theo_collapse",
    "cad411d1-889d-4fb6-85e7-210db19d1e11": "event:/char/madeline/wallslide",
    "cab0df89-b0d1-439c-8ac7-37db9d43e3a1": "event:/char/madeline/water_dash_gen",
    "fb5d35c5-47c8-439d-8d52-ec6daf91887b": "event:/char/madeline/water_dash_in",
    "5ce79651-4adc-4592-b018-38aa212d6ca6": "event:/char/madeline/water_dash_out",
    "4016ea56-842c-4649-99c4-7057db18c835": "event:/char/madeline/water_in",
    "5cf06f89-cf18-4dfa-bc69-5e4b58095cf3": "event:/char/madeline/water_move_general",
    "7d656ffa-050f-4cab-bbc9-03863fa14e4f": "event:/char/madeline/water_move_shallow",
    "3b90c629-7dcf-467b-a438-75e3287c8ae5": "event:/char/madeline/water_out",
}


def process_lua_file(filepath: Path, dry_run: bool = False) -> Tuple[bool, List[str]]:
    """Process a Lua file and replace GUIDs with event paths."""
    try:
        content = filepath.read_text(encoding='utf-8')
    except Exception as e:
        return False, [f"Failed to read: {e}"]
    
    original = content
    changes = []
    
    # Pattern to match GUID in various contexts (with or without braces)
    guid_pattern = re.compile(
        r'\{?([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\}?',
        re.IGNORECASE
    )
    
    def replace_guid(match):
        guid = match.group(1).lower()
        if guid in GUID_TO_EVENT:
            event_path = GUID_TO_EVENT[guid]
            changes.append(f"{guid} -> {event_path}")
            return f'"{event_path}"'
        return match.group(0)
    
    content = guid_pattern.sub(replace_guid, content)
    
    if content != original and not dry_run:
        filepath.write_text(content, encoding='utf-8')
    
    return len(changes) > 0, changes


def process_bin_file(filepath: Path, dry_run: bool = False) -> Tuple[bool, List[str]]:
    """Process a binary .bin map file and replace GUIDs with event paths."""
    try:
        data = filepath.read_bytes()
    except Exception as e:
        return False, [f"Failed to read: {e}"]
    
    original = data
    changes = []
    
    # In Celeste .bin files, strings are prefixed with their length
    # We need to handle the .NET 7-bit encoded int format
    
    for guid, event_path in GUID_TO_EVENT.items():
        # Try different encodings that might appear in bin files
        # 1. Plain GUID with braces
        guid_bytes_braced = f"{{{guid}}}".encode('utf-8')
        # 2. Plain GUID without braces
        guid_bytes = guid.encode('utf-8')
        
        if guid_bytes_braced in data:
            # Calculate new length prefix for event path
            event_bytes = event_path.encode('utf-8')
            new_len = len(event_bytes)
            
            if new_len < 128:
                # Single byte length prefix
                replacement = bytes([new_len]) + event_bytes
            else:
                # Multi-byte length prefix (7-bit encoded)
                len_bytes = []
                val = new_len
                while val >= 128:
                    len_bytes.append((val & 0x7F) | 0x80)
                    val >>= 7
                len_bytes.append(val)
                replacement = bytes(len_bytes) + event_bytes
            
            # Replace old length+GUID with new length+event_path
            # We need to find the length prefix of the old GUID
            idx = 0
            while True:
                idx = data.find(guid_bytes_braced, idx)
                if idx == -1:
                    break
                
                # Look backwards to find length prefix
                if idx > 0:
                    # Try to determine the length prefix
                    prefix_start = max(0, idx - 4)
                    prefix_data = data[prefix_start:idx]
                    
                    # Simple case: single byte prefix
                    if len(prefix_data) >= 1 and prefix_data[-1] == len(guid_bytes_braced):
                        old_full = bytes([len(guid_bytes_braced)]) + guid_bytes_braced
                        data = data.replace(old_full, replacement, 1)
                        changes.append(f"{guid} -> {event_path}")
                
                idx += 1
    
    if data != original and not dry_run:
        filepath.write_bytes(data)
    
    return len(changes) > 0, changes


def process_cs_file(filepath: Path, dry_run: bool = False) -> Tuple[bool, List[str]]:
    """Process a C# file and replace GUIDs with event paths."""
    try:
        content = filepath.read_text(encoding='utf-8')
    except Exception as e:
        return False, [f"Failed to read: {e}"]
    
    original = content
    changes = []
    
    # Pattern to match GUID in string contexts
    guid_pattern = re.compile(
        r'"\{?([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\}?"',
        re.IGNORECASE
    )
    
    def replace_guid(match):
        guid = match.group(1).lower()
        if guid in GUID_TO_EVENT:
            event_path = GUID_TO_EVENT[guid]
            changes.append(f"{guid} -> {event_path}")
            return f'"{event_path}"'
        return match.group(0)
    
    content = guid_pattern.sub(replace_guid, content)
    
    if content != original and not dry_run:
        filepath.write_text(content, encoding='utf-8')
    
    return len(changes) > 0, changes


def main():
    import argparse
    
    parser = argparse.ArgumentParser(
        description="Replace old GUID-based music events with event:/ paths"
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Show what would be changed without making changes"
    )
    parser.add_argument(
        "--lua-only",
        action="store_true",
        help="Only process Lua files"
    )
    parser.add_argument(
        "--cs-only",
        action="store_true",
        help="Only process C# files"
    )
    parser.add_argument(
        "--bin-only",
        action="store_true",
        help="Only process .bin map files"
    )
    args = parser.parse_args()
    
    root = Path(r"e:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\CELESTE_DESOLO_ZANTAS")
    
    total_files = 0
    total_changes = 0
    
    print(f"{'[DRY RUN] ' if args.dry_run else ''}Scanning for files...")
    print(f"Loaded {len(GUID_TO_EVENT)} GUID to event path mappings\n")
    
    # Process Lua files
    if not args.cs_only and not args.bin_only:
        lua_files = list(root.rglob("*.lua"))
        lua_files = [f for f in lua_files if 'Libraries' not in str(f)]  # Exclude library files
        
        print(f"Found {len(lua_files)} Lua files")
        for f in lua_files:
            modified, changes = process_lua_file(f, args.dry_run)
            if modified:
                print(f"  {f.name}: {len(changes)} changes")
                for c in changes[:3]:  # Show first 3 changes
                    print(f"    - {c}")
                if len(changes) > 3:
                    print(f"    ... and {len(changes) - 3} more")
                total_files += 1
                total_changes += len(changes)
    
    # Process C# files
    if not args.lua_only and not args.bin_only:
        cs_files = list(root.rglob("*.cs"))
        
        print(f"\nFound {len(cs_files)} C# files")
        for f in cs_files:
            modified, changes = process_cs_file(f, args.dry_run)
            if modified:
                print(f"  {f.name}: {len(changes)} changes")
                for c in changes[:3]:
                    print(f"    - {c}")
                if len(changes) > 3:
                    print(f"    ... and {len(changes) - 3} more")
                total_files += 1
                total_changes += len(changes)
    
    # Process .bin map files
    if not args.lua_only and not args.cs_only:
        bin_files = list((root / "Maps").rglob("*.bin")) if (root / "Maps").exists() else []
        
        print(f"\nFound {len(bin_files)} .bin map files")
        for f in bin_files:
            modified, changes = process_bin_file(f, args.dry_run)
            if modified:
                print(f"  {f.name}: {len(changes)} changes")
                total_files += 1
                total_changes += len(changes)
    
    print(f"\n{'='*60}")
    print(f"Summary:")
    print(f"  Files modified: {total_files}")
    print(f"  Total changes: {total_changes}")
    print(f"{'='*60}")


if __name__ == '__main__':
    main()
