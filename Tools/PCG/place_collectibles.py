"""Fix collectible placement on a generated Celeste map.

The stock loenn-mcp generator drops a vanilla `goldenBerry` into the *last*
room, which is backwards on both counts: the end-of-chapter collectible in
Celeste is the crystal heart, and a golden berry belongs at the level start
(it is the reward for a deathless run, so it has to be reachable from the
first frame).

This pass:
  * removes every generator-placed golden berry (and any prior run's heart),
  * puts a heart gem in the last room,
  * puts a golden berry in the first room, next to the player spawn.

Both defaults are entities this mod registers itself, in C# *and* in Lonn,
so they need no external dependency:
  DZ/DZHeartGem       Source/Entities/Entities/HeartGem.cs + Loenn/entities/heartGem.lua
  DZ/Goldenstrawberry Source/.../Goldenstrawberry           + Loenn/entities/goldenstrawberry.lua

    python Tools/PCG/place_collectibles.py Maps/DZ/wip/gen_large_60.dressed.bin

Writes `<map>.collect.bin` unless --in-place is given.
"""

from __future__ import annotations

import argparse
import shutil
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from _mapio import (  # noqa: E402
    CB,
    TILE,
    build_spots,
    ensure_section,
    max_entity_id,
    parse_grid,
    section,
)

REPO = Path(__file__).resolve().parents[2]

HEART_COLORS = ("Blue", "Red", "Gold", "Orange", "Purple", "Pink", "White")

# Attribute defaults come from each entity's Lonn `placements.data`, which is
# what the editor itself would write.
def heart_attrs(color: str) -> dict:
    return {"fake": False, "color": color, "removeCameraTriggers": False}


def berry_attrs(name: str) -> dict:
    # The vanilla berry carries these flags; DZ/Goldenstrawberry declares none.
    return {"winged": False, "moon": False} if name == "goldenBerry" else {}


def entities_of(room: dict) -> list[dict]:
    return (section(room, "entities") or {}).get("__children", [])


def find_spawn(room: dict) -> tuple[int, int] | None:
    for e in entities_of(room):
        if e.get("__name") == "player":
            return int(e.get("x", 0)), int(e.get("y", 0))
    return None


def strip(rooms: list[dict], names: set[str]) -> dict[str, int]:
    """Remove named entities everywhere, so the pass is re-runnable."""
    removed: dict[str, int] = {}
    for room in rooms:
        sec = section(room, "entities")
        if not sec:
            continue
        keep = []
        for e in sec.get("__children", []):
            n = e.get("__name")
            if n in names:
                removed[n] = removed.get(n, 0) + 1
            else:
                keep.append(e)
        sec["__children"] = keep
    return removed


def pick_heart_spot(room: dict) -> tuple[int, int] | None:
    """An open-air tile, biased to the upper middle -- where hearts hang."""
    grid, w, h = parse_grid(room)
    if w == 0 or h == 0:
        return None
    spots = build_spots(grid, w, h)
    pool = spots["air"] or spots["ceiling"] or spots["floor"] or spots["wall"]
    if not pool:
        return None
    tx_goal, ty_goal = w / 2, h * 0.35
    tx, ty = min(pool, key=lambda p: (p[0] - tx_goal) ** 2 + (p[1] - ty_goal) ** 2)
    return tx * TILE + TILE // 2, ty * TILE + TILE // 2


def pick_berry_spot(room: dict, spawn: tuple[int, int] | None) -> tuple[int, int] | None:
    """A floor tile a few tiles from spawn -- visible at once, not overlapping."""
    grid, w, h = parse_grid(room)
    if w == 0 or h == 0:
        return None
    spots = build_spots(grid, w, h)
    pool = spots["floor"] or spots["air"]
    if not pool:
        return None
    if spawn is None:
        tx, ty = pool[len(pool) // 2]
        return tx * TILE + TILE // 2, ty * TILE + TILE // 2

    sx, sy = spawn[0] / TILE, spawn[1] / TILE
    ideal = 3.0  # tiles away

    def cost(p: tuple[int, int]) -> float:
        d = ((p[0] - sx) ** 2 + (p[1] - sy) ** 2) ** 0.5
        return abs(d - ideal) + 0.35 * abs(p[1] - sy)  # prefer the same height

    tx, ty = min(pool, key=cost)
    return tx * TILE + TILE // 2, ty * TILE + TILE // 2


def add_entity(room: dict, name: str, x: int, y: int, attrs: dict, eid: int) -> dict:
    e = {"__name": name, "id": eid, "x": x, "y": y, **attrs, "__children": []}
    ensure_section(room, "entities")["__children"].append(e)
    return e


def main() -> int:
    ap = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter
    )
    ap.add_argument("map", type=Path)
    ap.add_argument("--heart-entity", default="DZ/DZHeartGem")
    ap.add_argument("--heart-color", default="Blue", choices=HEART_COLORS)
    ap.add_argument("--berry-entity", default="DZ/Goldenstrawberry")
    ap.add_argument("--no-berry", action="store_true", help="skip the golden berry entirely")
    ap.add_argument("--first", help="room name to treat as the start (default: the one with a spawn)")
    ap.add_argument("--last", help="room name to treat as the end (default: last in map order)")
    ap.add_argument("--in-place", action="store_true", help="overwrite the map (a .bak is kept)")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    map_path = args.map if args.map.is_absolute() else REPO / args.map
    if not map_path.exists():
        raise SystemExit(f"no such map: {map_path}")

    data = CB.read_map(map_path)
    rooms = CB.get_rooms(data)
    if not rooms:
        raise SystemExit("map has no rooms")
    by_name = {r.get("name", ""): r for r in rooms}

    if args.first:
        if args.first not in by_name:
            raise SystemExit(f"no room named '{args.first}'")
        first = by_name[args.first]
    else:
        first = next((r for r in rooms if find_spawn(r)), rooms[0])

    if args.last:
        if args.last not in by_name:
            raise SystemExit(f"no room named '{args.last}'")
        last = by_name[args.last]
    else:
        last = rooms[-1]

    print(f"map   : {map_path.relative_to(REPO)}  ({len(rooms)} rooms)")
    print(f"first : {first.get('name')}   last: {last.get('name')}")

    # Clear what the generator placed, plus anything we placed before, so
    # running this twice does not stack collectibles.
    removed = strip(rooms, {"goldenBerry", args.heart_entity, args.berry_entity})
    for n, c in sorted(removed.items()):
        print(f"remove: {c}x {n}")

    eid = max_entity_id(data)

    hs = pick_heart_spot(last)
    if hs is None:
        print(f"WARN  : no room geometry in '{last.get('name')}' - heart not placed")
    else:
        eid += 1
        add_entity(last, args.heart_entity, hs[0], hs[1], heart_attrs(args.heart_color), eid)
        print(f"place : {args.heart_entity} ({args.heart_color}) in {last.get('name')} at {hs}")

    if not args.no_berry:
        spawn = find_spawn(first)
        bs = pick_berry_spot(first, spawn)
        if bs is None:
            print(f"WARN  : no room geometry in '{first.get('name')}' - berry not placed")
        else:
            eid += 1
            add_entity(first, args.berry_entity, bs[0], bs[1], berry_attrs(args.berry_entity), eid)
            near = f" (spawn {spawn})" if spawn else " (no spawn found)"
            print(f"place : {args.berry_entity} in {first.get('name')} at {bs}{near}")

    if args.dry_run:
        print("dry run - nothing written")
        return 0

    if args.in_place:
        backup = map_path.with_suffix(".bin.bak")
        shutil.copy2(map_path, backup)
        out = map_path
        print(f"backup: {backup.relative_to(REPO)}")
    else:
        out = map_path.with_suffix(".collect.bin")

    CB.write_map(out, data)
    print(f"written: {out.relative_to(REPO)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
