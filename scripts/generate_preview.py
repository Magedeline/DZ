#!/usr/bin/env python3
import sys
from pathlib import Path

VENV_SITE = Path(__file__).parent.parent / ".venv" / "Lib" / "site-packages"
if str(VENV_SITE) not in sys.path:
    sys.path.insert(0, str(VENV_SITE))

from loenn_mcp.celeste_bin import read_map, get_rooms

map_path = Path(__file__).parent.parent / "Maps" / "Maggy" / "ASide" / "lastlevel.bin"
map_data = read_map(map_path)
rooms = get_rooms(map_data)

html = """<html><head><title>lastlevel.bin Preview</title>
<style>
body { font-family: sans-serif; background: #1a1a2e; color: #eee; margin: 20px; }
h1 { margin-bottom: 10px; }
.summary { margin-bottom: 20px; padding: 10px; background: #16213e; border-radius: 4px; }
.room { display: inline-block; border: 1px solid #444; margin: 2px; padding: 4px; font-size: 10px; min-width: 60px; text-align: center; }
.a { background: #16213e; } .b { background: #1a1a2e; } .c { background: #0f3460; }
.d { background: #533483; } .e { background: #e94560; } .f { background: #16213e; }
.g { background: #1a1a2e; } .h { background: #0f3460; } .i { background: #533483; }
.j { background: #e94560; } .k { background: #16213e; } .l { background: #533483; }
.end { background: #ff6b6b; }
.intro { background: #533483; }
</style></head><body>
<h1>lastlevel.bin Preview</h1>
<div class="summary">
Total rooms: """ + str(len(rooms)) + """<br>
</div>
<div style="display:flex;flex-wrap:wrap;">
"""

for room in rooms:
    name = room.get("name", "")
    prefix = name.split("-")[0] if "-" in name else name.split("_")[0]
    w = room.get("width", 0)
    h = room.get("height", 0)
    entities = 0
    triggers = 0
    for child in room.get("__children", []):
        if child.get("__name") == "entities":
            entities = len(child.get("__children", []))
        if child.get("__name") == "triggers":
            triggers = len(child.get("__children", []))
    html += f'<div class="room {prefix}">{name}<br>{w}x{h}<br>{entities}e {triggers}t</div>\n'

html += "</div></body></html>"

out = Path(__file__).parent.parent / "lastlevel_preview.html"
out.write_text(html, encoding="utf-8")
print(f"Preview saved to {out}")
