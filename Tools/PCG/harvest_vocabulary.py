"""Harvest a decoration vocabulary from hand-built DZ maps.

The stock loenn-mcp PCG dresses generated rooms from six hardcoded decal
strings that (a) are not real textures and (b) get written into sections
Celeste does not read. This scans the maps that were actually built by
hand and records what decals, entities and stylegrounds really appear,
per theme, along with the tile context each one sits in.

Output: PCG/vocabulary.json, consumed by dress_map.py.

    python Tools/PCG/harvest_vocabulary.py
"""

from __future__ import annotations

import copy
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path
from statistics import median

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "PCG" / "vocabulary.json"

TILE = 8
AIR = "0"

# How far to search for a supporting surface before calling a decal airborne.
SURFACE_SEARCH_TILES = 6


def _load_celeste_bin():
    """Import celeste_bin from the installed loenn_mcp package."""
    try:
        from loenn_mcp import celeste_bin  # noqa: PLC0415
    except ImportError:
        for p in sys.path:
            cand = Path(p) / "loenn_mcp"
            if cand.is_dir():
                sys.path.insert(0, str(cand.parent))
                break
        from loenn_mcp import celeste_bin  # noqa: PLC0415
    return celeste_bin


CB = _load_celeste_bin()


def theme_of(map_path: Path) -> str:
    """`Maps/DZ/0/01_City.bin` -> `city`."""
    return re.sub(r"^\d+[_-]?", "", map_path.stem).lower() or map_path.stem.lower()


def side_of(map_path: Path) -> str:
    """Parent dir is the side index (0=A, 1=B, ...)."""
    parent = map_path.parent.name
    return parent if parent.isdigit() else "?"


def section(room: dict, name: str) -> dict | None:
    for child in room.get("__children", []):
        if child.get("__name") == name:
            return child
    return None


def parse_grid(room: dict) -> tuple[list[str], int, int]:
    """Return (rows, width_tiles, height_tiles) for the solids layer.

    Rows are padded so every row is width_tiles long; air is '0'.
    """
    w = int(room.get("width", 0)) // TILE
    h = int(room.get("height", 0)) // TILE
    solids = section(room, "solids")
    text = (solids or {}).get("innerText", "") or ""
    rows = text.split("\n")
    grid = []
    for y in range(h):
        row = rows[y] if y < len(rows) else ""
        row = row.replace("\r", "")
        if len(row) < w:
            row = row + AIR * (w - len(row))
        grid.append(row[:w])
    return grid, w, h


def solid_at(grid: list[str], w: int, h: int, tx: int, ty: int) -> bool:
    if tx < 0 or ty < 0 or tx >= w or ty >= h:
        return False
    ch = grid[ty][tx]
    return ch != AIR and ch != " "


def classify(grid: list[str], w: int, h: int, px: float, py: float) -> tuple[str, int | None]:
    """Classify a room-local pixel position against nearby geometry.

    Returns (context, dy_to_surface_px). Context is one of
    floor / ceiling / wall / air.

    Anything sitting inside a solid tile is classified by whichever face is
    exposed to air, so wall-mounted things (spikesLeft/Right, wire) do not
    collapse into "floor" just because the wall continues downward.
    """
    tx, ty = int(px) // TILE, int(py) // TILE

    at = lambda dx, dy: solid_at(grid, w, h, tx + dx, ty + dy)  # noqa: E731

    if at(0, 0):
        if not at(0, -1):
            return "floor", int(py - ty * TILE)
        if not at(-1, 0) or not at(1, 0):
            return "wall", 0
        if not at(0, 1):
            return "ceiling", int(py - ty * TILE)
        return "air", None

    # Resting directly on a surface takes priority over a nearby wall.
    if at(0, 1):
        return "floor", int(py - (ty + 1) * TILE)

    if at(-1, 0) or at(1, 0):
        return "wall", 0

    for d in range(2, SURFACE_SEARCH_TILES + 1):
        if at(0, d):
            return "floor", int(py - (ty + d) * TILE)

    for d in range(1, SURFACE_SEARCH_TILES + 1):
        if at(0, -d):
            return "ceiling", int(py - (ty - d) * TILE)

    return "air", None


def blank(theme: str) -> dict:
    return {
        "theme": theme,
        "maps": [],
        "sides": Counter(),
        "rooms": 0,
        "tiles": 0,
        "air_tiles": 0,
        "decals": {"bg": defaultdict(lambda: _decal_rec()), "fg": defaultdict(lambda: _decal_rec())},
        "entities": defaultdict(lambda: _entity_rec()),
        "stylegrounds": {"fg": Counter(), "bg": Counter()},
        "tilesets": {"fg": Counter(), "bg": Counter()},
        "room_flags": Counter(),
        "style_node": None,
        "_style_layers": 0,
    }


def _decal_rec() -> dict:
    return {"count": 0, "contexts": Counter(), "dy": [], "scales": Counter(), "colors": Counter()}


def _entity_rec() -> dict:
    return {"count": 0, "contexts": Counter(), "widths": Counter(), "heights": Counter(), "attrs": Counter()}


def harvest_room(bucket: dict, room: dict) -> None:
    grid, w, h = parse_grid(room)
    bucket["rooms"] += 1
    bucket["tiles"] += w * h
    bucket["air_tiles"] += sum(row.count(AIR) for row in grid)

    for flag in ("dark", "space", "underwater", "whisper"):
        if room.get(flag):
            bucket["room_flags"][flag] += 1
    wind = room.get("windPattern")
    if wind and wind != "None":
        bucket["room_flags"][f"wind:{wind}"] += 1

    for layer_name, key in (("bgdecals", "bg"), ("fgdecals", "fg")):
        sec = section(room, layer_name)
        for d in (sec or {}).get("__children", []):
            tex = d.get("texture")
            if not tex:
                continue
            rec = bucket["decals"][key][tex]
            rec["count"] += 1
            ctx, dy = classify(grid, w, h, d.get("x", 0), d.get("y", 0))
            rec["contexts"][ctx] += 1
            if dy is not None:
                rec["dy"].append(dy)
            sx, sy = d.get("scaleX", 1), d.get("scaleY", 1)
            rec["scales"][f"{sx},{sy}"] += 1
            rec["colors"][d.get("color", "ffffffff")] += 1

    sec = section(room, "entities")
    for e in (sec or {}).get("__children", []):
        name = e.get("__name")
        if not name:
            continue
        rec = bucket["entities"][name]
        rec["count"] += 1
        ctx, _ = classify(grid, w, h, e.get("x", 0), e.get("y", 0))
        rec["contexts"][ctx] += 1
        if "width" in e:
            rec["widths"][int(e["width"])] += 1
        if "height" in e:
            rec["heights"][int(e["height"])] += 1
        for k in e:
            if k not in ("__name", "__children", "id", "x", "y", "width", "height", "originX", "originY"):
                rec["attrs"][k] += 1

    # Tileset characters actually used, so the dresser can theme tiles too.
    # The background tile layer is `bg`, not `bgtiles` -- `bgtiles` is empty in
    # every hand-built room.
    for layer, key in (("solids", "fg"), ("bg", "bg")):
        s = section(room, layer)
        for ch in (s or {}).get("innerText", "") or "":
            if ch not in (AIR, "\n", "\r", " "):
                bucket["tilesets"][key][ch] += 1


def harvest_style(bucket: dict, map_data: dict, rooms: int) -> None:
    style = CB.find_child(map_data, "Style")
    if not style:
        return

    # Keep the richest complete Style node we see for this theme. Stylegrounds
    # are an ordered composition (bg0 behind bg1 behind bg2, snow on top), so
    # sampling individual layers across maps would produce nonsense -- copy a
    # whole working stack instead.
    layers = 0
    for group in ("Foregrounds", "Backgrounds"):
        node = CB.find_child(style, group)
        layers += len((node or {}).get("__children", []))
    if layers > bucket.get("_style_layers", 0):
        bucket["_style_layers"] = layers
        bucket["style_node"] = copy.deepcopy(style)
        bucket["style_rooms"] = rooms
    for group, key in (("Foregrounds", "fg"), ("Backgrounds", "bg")):
        node = CB.find_child(style, group)
        if not node:
            continue
        stack = list(node.get("__children", []))
        while stack:
            el = stack.pop()
            nm = el.get("__name")
            if nm == "apply":
                stack.extend(el.get("__children", []))
                continue
            if nm:
                tex = el.get("texture") or el.get("atlas") or ""
                bucket["stylegrounds"][key][f"{nm}|{tex}" if tex else nm] += 1


def finalize(bucket: dict) -> dict:
    def top_decals(d: dict) -> list[dict]:
        out = []
        for tex, rec in sorted(d.items(), key=lambda kv: -kv[1]["count"]):
            ctx = rec["contexts"]
            out.append({
                "texture": tex,
                "count": rec["count"],
                "context": ctx.most_common(1)[0][0] if ctx else "air",
                "contexts": dict(ctx),
                "dy_median": int(median(rec["dy"])) if rec["dy"] else None,
                "scale": rec["scales"].most_common(1)[0][0] if rec["scales"] else "1,1",
                "color": rec["colors"].most_common(1)[0][0] if rec["colors"] else "ffffffff",
            })
        return out

    ents = []
    for name, rec in sorted(bucket["entities"].items(), key=lambda kv: -kv[1]["count"]):
        ctx = rec["contexts"]
        ents.append({
            "name": name,
            "count": rec["count"],
            "context": ctx.most_common(1)[0][0] if ctx else "air",
            "contexts": dict(ctx),
            "common_width": rec["widths"].most_common(1)[0][0] if rec["widths"] else None,
            "common_height": rec["heights"].most_common(1)[0][0] if rec["heights"] else None,
            "attrs": [k for k, _ in rec["attrs"].most_common(12)],
        })

    bg_total = sum(r["count"] for r in bucket["decals"]["bg"].values())
    fg_total = sum(r["count"] for r in bucket["decals"]["fg"].values())
    air = bucket["air_tiles"] or 1

    return {
        "theme": bucket["theme"],
        "maps": bucket["maps"],
        "sides": dict(bucket["sides"]),
        "rooms": bucket["rooms"],
        "tiles": bucket["tiles"],
        "air_tiles": bucket["air_tiles"],
        # Decals per air tile, so the dresser can match hand-built density
        # regardless of how big the target room is.
        "density": {
            "bg_per_air_tile": round(bg_total / air, 6),
            "fg_per_air_tile": round(fg_total / air, 6),
        },
        "room_flags": dict(bucket["room_flags"]),
        "decals": {"bg": top_decals(bucket["decals"]["bg"]), "fg": top_decals(bucket["decals"]["fg"])},
        "entities": ents,
        "stylegrounds": {k: dict(v) for k, v in bucket["stylegrounds"].items()},
        "style_node": bucket.get("style_node"),
        "style_layers": bucket.get("_style_layers", 0),
        "tilesets": {k: dict(v.most_common(12)) for k, v in bucket["tilesets"].items()},
    }


def main() -> int:
    maps_root = REPO / "Maps"
    paths = sorted(p for p in maps_root.rglob("*.bin") if "wip" not in p.parts)
    if not paths:
        print(f"no maps found under {maps_root}", file=sys.stderr)
        return 1

    buckets: dict[str, dict] = {}
    skipped = []

    for p in paths:
        theme = theme_of(p)
        try:
            data = CB.read_map(p)
            rooms = CB.get_rooms(data)
        except Exception as exc:  # noqa: BLE001 - report and continue
            skipped.append((p, repr(exc)))
            continue

        b = buckets.setdefault(theme, blank(theme))
        b["maps"].append(str(p.relative_to(REPO)).replace("\\", "/"))
        b["sides"][side_of(p)] += 1
        harvest_style(b, data, len(rooms))
        for room in rooms:
            harvest_room(b, room)

    themes = {t: finalize(b) for t, b in sorted(buckets.items())}
    total_decals = sum(
        d["count"] for t in themes.values() for layer in t["decals"].values() for d in layer
    )
    total_ents = sum(e["count"] for t in themes.values() for e in t["entities"])

    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(
        json.dumps(
            {
                "version": 1,
                "source_maps": len(paths) - len(skipped),
                "themes": themes,
            },
            indent=2,
        ),
        encoding="utf-8",
    )

    print(f"maps scanned      : {len(paths) - len(skipped)}")
    print(f"themes            : {len(themes)}")
    print(f"rooms             : {sum(t['rooms'] for t in themes.values())}")
    print(f"decal placements  : {total_decals}")
    print(f"entity placements : {total_ents}")
    print(f"unique decals     : {sum(len(t['decals']['bg']) + len(t['decals']['fg']) for t in themes.values())}")
    print(f"unique entities   : {len({e['name'] for t in themes.values() for e in t['entities']})}")
    print(f"written           : {OUT.relative_to(REPO)}")
    for p, err in skipped:
        print(f"  SKIPPED {p}: {err}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
