#!/usr/bin/env python3
import sys
from pathlib import Path

VENV_SITE = Path(__file__).parent.parent / ".venv" / "Lib" / "site-packages"
if str(VENV_SITE) not in sys.path:
    sys.path.insert(0, str(VENV_SITE))

from loenn_mcp.celeste_bin import read_map, get_rooms

MAP_PATH = Path("e:/Celeste Desolo Zantas/Mods/MaggyHelper/Maps/Maggy/ASide/lastlevel.bin")
map_data = read_map(MAP_PATH)
rooms = get_rooms(map_data)

k_rooms = [r for r in rooms if r.get("name", "").startswith("k-")]
print("Total k- rooms:", len(k_rooms))
for r in sorted(k_rooms, key=lambda x: x.get("name", "")):
    print(f"  {r.get('name')}: x={r.get('x')} y={r.get('y')} w={r.get('width')} h={r.get('height')}")

print(f"\nTotal rooms in map: {len(rooms)}")
