#!/usr/bin/env python3
"""Reposition k-02 through k-11 into a snake pattern and add challenge obstacles."""

import sys
from pathlib import Path

VENV_SITE = Path(__file__).parent.parent / ".venv" / "Lib" / "site-packages"
if str(VENV_SITE) not in sys.path:
    sys.path.insert(0, str(VENV_SITE))

from loenn_mcp.celeste_bin import read_map, get_rooms, write_map

MAP_PATH = Path("e:/Celeste Desolo Zantas/Mods/MaggyHelper/Maps/Maggy/ASide/lastlevel.bin")
map_data = read_map(MAP_PATH)
rooms = get_rooms(map_data)

# Find k-01 and k-12 positions
k_rooms = {r.get("name", ""): r for r in rooms if r.get("name", "").startswith("k-")}
k01 = k_rooms.get("k-01")
k12 = k_rooms.get("k-12")

x_start = k01.get("x", 0) + k01.get("width", 0)  # end of k-01
y_start = k01.get("y", 0)
x_end = k12.get("x", 0)
y_end = k12.get("y", 0)

print(f"k-01 end: x={x_start}, y={y_start}")
print(f"k-12 start: x={x_end}, y={y_end}")

# Snake layout parameters
amplitude = 1600       # horizontal zigzag width
row_height = 340       # vertical spacing between rows
right_drift = 350      # how much the whole pattern shifts right per row
base_x = x_start + 200 # start a bit right of k-01
base_y = y_start - 200 # start a bit below k-01

# Generate snake positions for k-02 to k-11
snake_positions = []
for i in range(10):
    row = i // 2
    col = i % 2
    # Snake alternates direction each row
    if row % 2 == 0:
        # Even row: left to right
        x_offset = 0 if col == 0 else amplitude
    else:
        # Odd row: right to left
        x_offset = amplitude if col == 0 else 0
    
    x = base_x + row * right_drift + x_offset
    y = base_y - row * row_height
    snake_positions.append((x, y))

# Show planned positions
names = [f"k-{i:02d}" for i in range(2, 12)]
print("\nPlanned snake positions:")
for name, (x, y) in zip(names, snake_positions):
    print(f"  {name}: x={x}, y={y}")

# Apply positions and add challenge entities
for room in rooms:
    name = room.get("name", "")
    if name in names:
        idx = names.index(name)
        x, y = snake_positions[idx]
        room["x"] = x
        room["y"] = y
        room["width"] = 320
        room["height"] = 184
        
        # Find entities child
        for child in room.get("__children", []):
            if child.get("__name") == "entities":
                ents = child.get("__children", [])
                # Keep player, add challenge entities
                w, h = 320, 184
                new_ents = [e for e in ents if e.get("__name") == "player"]
                
                # Ensure player exists
                if not new_ents:
                    new_ents.append({
                        "__name": "player",
                        "x": 40,
                        "y": h - 16,
                        "width": 8,
                        "height": 8
                    })
                
                # Add challenge entities based on room index (progressive difficulty)
                if idx == 0:
                    # k-02: Simple spinners and a spring
                    new_ents.append({"__name": "spinner", "x": 120, "y": h - 24, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spinner", "x": 160, "y": h - 24, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spring", "x": 200, "y": h - 16, "width": 8, "height": 8, "playerCanUse": True})
                elif idx == 1:
                    # k-03: Moving platform + spinner
                    new_ents.append({"__name": "moveBlock", "x": 100, "y": h - 40, "width": 32, "height": 16, "direction": "Right", "fast": False, "canSteer": False})
                    new_ents.append({"__name": "spinner", "x": 200, "y": h - 40, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spinner", "x": 240, "y": h - 40, "width": 16, "height": 16, "attachToSolid": False})
                elif idx == 2:
                    # k-04: Jump-thrus and spinners
                    new_ents.append({"__name": "jumpThru", "x": 80, "y": h - 48, "width": 32, "height": 8, "surfaceSoundIndex": 0})
                    new_ents.append({"__name": "jumpThru", "x": 160, "y": h - 72, "width": 32, "height": 8, "surfaceSoundIndex": 0})
                    new_ents.append({"__name": "spinner", "x": 200, "y": h - 24, "width": 16, "height": 16, "attachToSolid": False})
                elif idx == 3:
                    # k-05: Springs + spinners (timing challenge)
                    new_ents.append({"__name": "spring", "x": 80, "y": h - 16, "width": 8, "height": 8, "playerCanUse": True})
                    new_ents.append({"__name": "spring", "x": 120, "y": h - 16, "width": 8, "height": 8, "playerCanUse": True})
                    new_ents.append({"__name": "spinner", "x": 180, "y": h - 48, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spinner", "x": 220, "y": h - 48, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spinner", "x": 260, "y": h - 48, "width": 16, "height": 16, "attachToSolid": False})
                elif idx == 4:
                    # k-06: Moving platform gauntlet
                    new_ents.append({"__name": "moveBlock", "x": 80, "y": h - 48, "width": 32, "height": 16, "direction": "Right", "fast": True, "canSteer": False})
                    new_ents.append({"__name": "moveBlock", "x": 200, "y": h - 80, "width": 32, "height": 16, "direction": "Left", "fast": True, "canSteer": False})
                    new_ents.append({"__name": "refill", "x": 150, "y": h - 32, "width": 16, "height": 16, "oneUse": False})
                elif idx == 5:
                    # k-07: Tight spinner corridor
                    for sx in range(80, 280, 32):
                        new_ents.append({"__name": "spinner", "x": sx, "y": h - 32, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spring", "x": 300, "y": h - 16, "width": 8, "height": 8, "playerCanUse": True})
                elif idx == 6:
                    # k-08: Dash blocks + refill
                    new_ents.append({"__name": "dashBlock", "x": 100, "y": h - 32, "width": 32, "height": 16, "canDash": True})
                    new_ents.append({"__name": "refill", "x": 160, "y": h - 32, "width": 16, "height": 16, "oneUse": False})
                    new_ents.append({"__name": "spinner", "x": 240, "y": h - 40, "width": 16, "height": 16, "attachToSolid": False})
                elif idx == 7:
                    # k-09: Track spinners (circling)
                    new_ents.append({"__name": "trackSpinner", "x": 120, "y": h - 48, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "trackSpinner", "x": 200, "y": h - 48, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spring", "x": 280, "y": h - 16, "width": 8, "height": 8, "playerCanUse": True})
                elif idx == 8:
                    # k-10: Swap block puzzle element
                    new_ents.append({"__name": "swapBlock", "x": 80, "y": h - 48, "width": 32, "height": 16, "direction": "Right", "speed": 360})
                    new_ents.append({"__name": "swapBlock", "x": 200, "y": h - 80, "width": 32, "height": 16, "direction": "Left", "speed": 360})
                    new_ents.append({"__name": "refill", "x": 150, "y": h - 32, "width": 16, "height": 16, "oneUse": False})
                elif idx == 9:
                    # k-11: Final challenge before k-12 (combo)
                    new_ents.append({"__name": "moveBlock", "x": 60, "y": h - 48, "width": 24, "height": 16, "direction": "Right", "fast": True, "canSteer": False})
                    new_ents.append({"__name": "spinner", "x": 120, "y": h - 32, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spinner", "x": 150, "y": h - 32, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "spring", "x": 200, "y": h - 16, "width": 8, "height": 8, "playerCanUse": True})
                    new_ents.append({"__name": "spinner", "x": 260, "y": h - 40, "width": 16, "height": 16, "attachToSolid": False})
                    new_ents.append({"__name": "refill", "x": 290, "y": h - 32, "width": 16, "height": 16, "oneUse": False})
                
                child["__children"] = new_ents
                break
        
        print(f"Updated {name}: x={x}, y={y}, added challenge entities")

# Also add transition triggers between k- rooms
for room in rooms:
    name = room.get("name", "")
    if name in names:
        idx = names.index(name)
        w = room.get("width", 320)
        h = room.get("height", 184)
        
        # Find triggers child or create
        triggers_child = None
        for child in room.get("__children", []):
            if child.get("__name") == "triggers":
                triggers_child = child
                break
        
        if not triggers_child:
            triggers_child = {"__name": "triggers", "__children": []}
            room["__children"].append(triggers_child)
        
        # Add transition to next room (right edge exit)
        if idx < 9:  # k-02 through k-10
            next_name = f"k-{idx+3:02d}"
            triggers_child["__children"].append({
                "__name": "transitionTrigger",
                "x": w - 16,
                "y": 0,
                "width": 16,
                "height": h,
                "toRoom": next_name,
                "toX": 40,
                "toY": h - 48
            })
        
        # Add return transition from left edge (except first)
        if idx > 0:
            prev_name = f"k-{idx+1:02d}"
            triggers_child["__children"].append({
                "__name": "transitionTrigger",
                "x": 0,
                "y": 0,
                "width": 16,
                "height": h,
                "toRoom": prev_name,
                "toX": prev_w - 56 if 'prev_w' in dir() else 264,
                "toY": 48
            })
        
        # Add transition from k-11 to k-12 (final room)
        if name == "k-11":
            triggers_child["__children"].append({
                "__name": "transitionTrigger",
                "x": w - 16,
                "y": 0,
                "width": 16,
                "height": h,
                "toRoom": "k-12",
                "toX": 40,
                "toY": 100
            })

# Sort rooms alphabetically
for map_child in map_data.get("__children", []):
    if map_child.get("__name") == "levels":
        map_child["__children"].sort(key=lambda r: r.get("name", ""))
        break

write_map(MAP_PATH, map_data)
print(f"\nSaved snake layout with challenge obstacles to {MAP_PATH.name}")
