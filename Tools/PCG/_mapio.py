"""Shared Celeste .bin helpers for the DZ PCG tools.

Thin layer over loenn_mcp.celeste_bin: section lookup, solids-grid parsing,
and bucketing air tiles by the surface they touch. Used by dress_map.py and
place_collectibles.py so the two agree on what "floor" means.
"""

from __future__ import annotations

import sys
from pathlib import Path

TILE = 8
AIR = "0"


def load_celeste_bin():
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


CB = load_celeste_bin()


def section(room: dict, name: str) -> dict | None:
    for child in room.get("__children", []):
        if child.get("__name") == name:
            return child
    return None


def ensure_section(room: dict, name: str) -> dict:
    sec = section(room, name)
    if sec is None:
        sec = {"__name": name, "__children": []}
        room.setdefault("__children", []).append(sec)
    sec.setdefault("__children", [])
    return sec


def parse_grid(room: dict) -> tuple[list[str], int, int]:
    """Return (rows, width_tiles, height_tiles) for the solids layer."""
    w = int(room.get("width", 0)) // TILE
    h = int(room.get("height", 0)) // TILE
    text = (section(room, "solids") or {}).get("innerText", "") or ""
    rows = text.split("\n")
    grid = []
    for y in range(h):
        row = (rows[y] if y < len(rows) else "").replace("\r", "")
        if len(row) < w:
            row += AIR * (w - len(row))
        grid.append(row[:w])
    return grid, w, h


def make_solid(grid: list[str], w: int, h: int):
    def solid(tx: int, ty: int) -> bool:
        if tx < 0 or ty < 0 or tx >= w or ty >= h:
            return False
        return grid[ty][tx] not in (AIR, " ")

    return solid


def build_spots(grid: list[str], w: int, h: int) -> dict[str, list[tuple[int, int]]]:
    """Bucket every air tile by the kind of surface it touches."""
    solid = make_solid(grid, w, h)
    spots: dict[str, list[tuple[int, int]]] = {"floor": [], "ceiling": [], "wall": [], "air": []}
    for ty in range(h):
        for tx in range(w):
            if solid(tx, ty):
                continue
            if solid(tx, ty + 1):
                spots["floor"].append((tx, ty))
            elif solid(tx, ty - 1):
                spots["ceiling"].append((tx, ty))
            elif solid(tx - 1, ty) or solid(tx + 1, ty):
                spots["wall"].append((tx, ty))
            else:
                spots["air"].append((tx, ty))
    return spots


def max_entity_id(map_data: dict) -> int:
    """Highest entity id anywhere in the map, so new entities can extend it."""
    best = 0
    for room in CB.get_rooms(map_data):
        for sec_name in ("entities", "triggers"):
            sec = section(room, sec_name)
            for e in (sec or {}).get("__children", []):
                try:
                    best = max(best, int(e.get("id", 0)))
                except (TypeError, ValueError):
                    continue
    return best
