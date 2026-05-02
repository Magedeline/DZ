#!/usr/bin/env python3
"""Add k-02 through k-11 rooms to lastlevel.bin."""

import sys
from pathlib import Path

VENV_SITE = Path(__file__).parent.parent / ".venv" / "Lib" / "site-packages"
if str(VENV_SITE) not in sys.path:
    sys.path.insert(0, str(VENV_SITE))

from loenn_mcp.celeste_bin import read_map, get_rooms, write_map, find_children

MAP_PATH = Path("e:/Celeste Desolo Zantas/Mods/MaggyHelper/Maps/Maggy/ASide/lastlevel.bin")

map_data = read_map(MAP_PATH)
rooms = get_rooms(map_data)

# Find existing k- rooms and their positions
k_rooms = {r.get("name", ""): r for r in rooms if r.get("name", "").startswith("k-")}
print("Existing k- rooms:")
for name in sorted(k_rooms.keys()):
    r = k_rooms[name]
    print(f"  {name}: x={r.get('x')} y={r.get('y')} w={r.get('width')} h={r.get('height')}")

# Get k-01 and k-12 positions
k01 = k_rooms.get("k-01")
k12 = k_rooms.get("k-12")

if not k01 or not k12:
    print("ERROR: k-01 or k-12 not found!")
    sys.exit(1)

x1 = k01.get("x", 0) + k01.get("width", 0)
y1 = k01.get("y", 0)
x2 = k12.get("x", 0)
y2 = k12.get("y", 0)
w2 = k12.get("width", 320)
h2 = k12.get("height", 184)

print(f"\nk-01 end: x={x1}, y={y1}")
print(f"k-12 start: x={x2}, y={y2}")

# We need to create k-02 through k-11 (10 rooms)
# Distribute them between k-01 and k-12
num_new = 10
# Linear interpolation with some vertical offset pattern
dx = (x2 - x1) / (num_new + 1)
dy_total = y2 - y1

# Find the 'levels' container
levels_child = None
for child in map_data.get("__children", []):
    if child.get("__name") == "levels":
        levels_child = child
        break

if not levels_child:
    print("ERROR: No levels container found!")
    sys.exit(1)

new_rooms = []
for i in range(1, num_new + 1):
    room_num = i + 1  # k-02, k-03, ... k-11
    name = f"k-{room_num:02d}"
    t = i / (num_new + 1)
    rx = int(x1 + dx * i)
    ry = int(y1 + dy_total * t)
    # Alternate heights slightly for variety
    rh = 184 if i % 2 == 0 else 200
    rw = 320
    
    room = {
        "__name": "level",
        "name": name,
        "x": rx,
        "y": ry,
        "width": rw,
        "height": rh,
        "c": 0,
        "music": "event:/desolozantas/final_content/music/lvl21/climb",
        "__children": [
            {
                "__name": "fgtiles",
                "innerText": "",
                "__children": []
            },
            {
                "__name": "bgtiles",
                "innerText": "",
                "__children": []
            },
            {
                "__name": "solids",
                "innerText": "0",
                "__children": []
            },
            {
                "__name": "bg",
                "innerText": "0",
                "__children": []
            },
            {
                "__name": "entities",
                "__children": [
                    {
                        "__name": "player",
                        "x": 40,
                        "y": rh - 16,
                        "width": 8,
                        "height": 8
                    }
                ]
            },
            {
                "__name": "triggers",
                "__children": []
            }
        ]
    }
    new_rooms.append(room)
    print(f"  Created {name}: x={rx} y={ry} w={rw} h={rh}")

# Add new rooms to levels container
levels_child["__children"].extend(new_rooms)

# Sort levels alphabetically by name
levels_child["__children"].sort(key=lambda r: r.get("name", ""))

write_map(MAP_PATH, map_data)
print(f"\nAdded {len(new_rooms)} new k- rooms. Total k- rooms now: {len(k_rooms) + len(new_rooms)}")
print("Saved.")
