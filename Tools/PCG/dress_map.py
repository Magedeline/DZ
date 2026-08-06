"""Dress a generated Celeste map using vocabulary harvested from hand-built maps.

Replaces the stock loenn-mcp decoration pass, which writes six invented
texture names into `decalsFg` / `decalsBg` -- sections Celeste never reads,
so the decals are invisible in game. This writes real textures, sampled
from the maps in this mod, into the correct `fgdecals` / `bgdecals`
sections, and honours the tile context each decal was observed in
(floor / wall / ceiling / air) plus its median offset from that surface.

    python Tools/PCG/harvest_vocabulary.py          # build PCG/vocabulary.json
    python Tools/PCG/dress_map.py Maps/DZ/wip/gen_large_60.bin --theme summit

Writes `<map>.dressed.bin` unless --in-place is given.
"""

from __future__ import annotations

import argparse
import copy
import json
import random
import shutil
import sys
from fnmatch import fnmatch
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from _mapio import (  # noqa: E402
    CB,
    TILE,
    build_spots,
    ensure_section,
    make_solid,
    parse_grid,
)

AIR = "0"

REPO = Path(__file__).resolve().parents[2]
VOCAB = REPO / "PCG" / "vocabulary.json"

# Sections the stock PCG writes decals into. Celeste ignores them; we strip
# them so a dressed map does not carry dead weight.
BOGUS_SECTIONS = ("decalsFg", "decalsBg")

# Themes with no harvested decals of their own borrow from a built-out theme
# with a compatible look.
FALLBACK = {
    "snow": "summit",
    "ruins": "city",
    "castle": "stronghold",
    "digital": "truth",
    "fire": "inferno",
    "corruption": "shadow",
    "prologue": "city",
    "water": "heart",
}


def place_xy(ctx: str, tx: int, ty: int, dy: int | None, rng: random.Random) -> tuple[int, int]:
    """Invert the harvester's offset measurement back into a position."""
    px = tx * TILE + rng.randint(0, TILE - 1)
    if ctx == "floor":
        return px, (ty + 1) * TILE + (dy if dy is not None else 0)
    if ctx == "ceiling":
        return px, (ty - 1) * TILE + (dy if dy is not None else 0)
    return px, ty * TILE + TILE // 2


def weighted_pick(entries: list[dict], rng: random.Random) -> dict:
    total = sum(e["count"] for e in entries)
    r = rng.uniform(0, total)
    acc = 0.0
    for e in entries:
        acc += e["count"]
        if r <= acc:
            return e
    return entries[-1]


def dress_room(
    room: dict,
    theme: dict,
    rng: random.Random,
    scale_density: float,
    clear: bool,
) -> tuple[int, int]:
    grid, w, h = parse_grid(room)
    if w == 0 or h == 0:
        return 0, 0

    spots = build_spots(grid, w, h)
    air_tiles = sum(len(v) for v in spots.values())
    if air_tiles == 0:
        return 0, 0

    placed = {"bg": 0, "fg": 0}
    for layer in ("bg", "fg"):
        entries = theme["decals"][layer]
        if not entries:
            continue
        density = theme["density"][f"{layer}_per_air_tile"] * scale_density
        target = int(air_tiles * density)
        if target <= 0:
            continue

        sec = ensure_section(room, f"{layer}decals")
        if clear:
            sec["__children"] = []
        kids = sec["__children"]

        # Group by context so we only sample decals we have somewhere to put.
        by_ctx: dict[str, list[dict]] = {}
        for e in entries:
            if spots.get(e["context"]):
                by_ctx.setdefault(e["context"], []).append(e)
        if not by_ctx:
            continue

        used: set[tuple[str, int, int]] = set()
        attempts = 0
        while placed[layer] < target and attempts < target * 12:
            attempts += 1
            ctx = rng.choice(list(by_ctx))
            entry = weighted_pick(by_ctx[ctx], rng)
            pool = spots[ctx]
            tx, ty = pool[rng.randrange(len(pool))]
            key = (ctx, tx, ty)
            if key in used:
                continue
            used.add(key)

            px, py = place_xy(ctx, tx, ty, entry.get("dy_median"), rng)
            sx, _, sy = entry.get("scale", "1,1").partition(",")
            kids.append({
                "__name": "decal",
                "texture": entry["texture"],
                "x": px,
                "y": py,
                "scaleX": float(sx or 1),
                "scaleY": float(sy or 1),
                "color": entry.get("color", "ffffffff"),
                "__children": [],
            })
            placed[layer] += 1

    return placed["bg"], placed["fg"]


def apply_stylegrounds(data: dict, theme: dict) -> int:
    """Copy a whole working styleground stack onto the map.

    Without this a generated room is solid tiles against a black void, which
    dominates how "plain" it looks far more than decals do. Stylegrounds are an
    ordered composition (bg0 behind bg1 behind bg2, weather on top), so the
    harvester keeps one complete stack per theme and it is copied wholesale
    rather than sampled layer by layer.
    """
    node = theme.get("style_node")
    if not node:
        return 0
    kids = data.setdefault("__children", [])
    for i, child in enumerate(kids):
        if child.get("__name") == "Style":
            kids[i] = copy.deepcopy(node)
            break
    else:
        kids.append(copy.deepcopy(node))
    return theme.get("style_layers", 0)


def fill_bg_tiles(room: dict, char: str, radius: int) -> int:
    """Paint the background tile layer behind and around the geometry.

    Hand-built rooms cover roughly 60% of the room: a back wall hugging the
    playable pocket, with open sky left bare so the parallax shows through.
    Filling within `radius` tiles of any solid approximates that.
    """
    grid, w, h = parse_grid(room)
    if w == 0 or h == 0:
        return 0
    solid = make_solid(grid, w, h)
    offsets = [
        (dx, dy)
        for dy in range(-radius, radius + 1)
        for dx in range(-radius, radius + 1)
        if dx * dx + dy * dy <= radius * radius
    ]
    rows = []
    filled = 0
    for y in range(h):
        row = []
        for x in range(w):
            if any(solid(x + dx, y + dy) for dx, dy in offsets):
                row.append(char)
                filled += 1
            else:
                row.append(AIR)
        rows.append("".join(row))
    ensure_section(room, "bg")["innerText"] = "\n".join(rows)
    return filled


def strip_bogus(room: dict) -> int:
    kids = room.get("__children", [])
    before = len(kids)
    room["__children"] = [c for c in kids if c.get("__name") not in BOGUS_SECTIONS]
    return before - len(room["__children"])


def prefix_mix(theme: dict) -> list[tuple[str, int, float]]:
    """Which asset families a theme's decals are drawn from, most-used first.

    A theme harvested from rooms that borrowed decals from many chapters
    produces incoherent output (resort bookshelves in a summit room), so
    surface this and let --palette narrow it.
    """
    from collections import Counter

    c: Counter = Counter()
    for layer in ("bg", "fg"):
        for d in theme["decals"][layer]:
            c[d["texture"].split("/")[0]] += d["count"]
    total = sum(c.values()) or 1
    return [(p, n, 100 * n / total) for p, n in c.most_common()]


def apply_palette(theme: dict, palette: list[str]) -> dict:
    """Restrict a theme to decals whose texture starts with an allowed prefix."""
    out = dict(theme)
    out["decals"] = {
        layer: [d for d in theme["decals"][layer] if d["texture"].split("/")[0] in palette]
        for layer in ("bg", "fg")
    }
    return out


def resolve_theme(vocab: dict, requested: str | None, map_path: Path) -> tuple[str, dict]:
    themes = vocab["themes"]

    def usable(name: str) -> bool:
        t = themes.get(name)
        return bool(t and (t["decals"]["bg"] or t["decals"]["fg"]))

    if requested:
        if requested not in themes:
            raise SystemExit(
                f"unknown theme '{requested}'. available: {', '.join(sorted(themes))}"
            )
        if not usable(requested):
            fb = FALLBACK.get(requested)
            if fb and usable(fb):
                print(f"  theme '{requested}' has no harvested decals; borrowing '{fb}'")
                return fb, themes[fb]
            raise SystemExit(f"theme '{requested}' has no decals and no usable fallback")
        return requested, themes[requested]

    import re

    guess = re.sub(r"^\d+[_-]?", "", map_path.stem).lower()
    for cand in (guess, FALLBACK.get(guess, "")):
        if cand and usable(cand):
            return cand, themes[cand]
    raise SystemExit(
        f"could not infer a theme from '{map_path.name}'; pass --theme "
        f"(available: {', '.join(sorted(t for t in themes if usable(t)))})"
    )


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("map", type=Path, help="path to the .bin map to dress")
    ap.add_argument("--theme", help="vocabulary theme to dress with (default: infer from filename)")
    ap.add_argument(
        "--palette",
        help="comma-separated asset families to restrict to, e.g. '7-summit,generic'. "
             "Use --list-palette to see what a theme offers.",
    )
    ap.add_argument("--list-palette", action="store_true", help="show the theme's asset families and exit")
    ap.add_argument("--rooms", default="*", help="glob of room names to dress (default: all)")
    ap.add_argument("--density", type=float, default=1.0, help="multiplier on harvested density")
    ap.add_argument("--seed", type=int, default=-1, help="rng seed (-1 = random)")
    ap.add_argument("--clear", action="store_true", help="wipe existing decals before dressing")
    ap.add_argument("--no-style", action="store_true", help="do not apply stylegrounds")
    ap.add_argument("--no-bg-tiles", action="store_true", help="do not paint the background tile layer")
    ap.add_argument("--bg-radius", type=int, default=4, help="how far bg tiles spread from geometry")
    ap.add_argument("--in-place", action="store_true", help="overwrite the map (a .bak is kept)")
    ap.add_argument("--dry-run", action="store_true", help="report what would change, write nothing")
    args = ap.parse_args()

    if not VOCAB.exists():
        raise SystemExit(f"missing {VOCAB}. Run Tools/PCG/harvest_vocabulary.py first.")
    vocab = json.loads(VOCAB.read_text(encoding="utf-8"))

    map_path = args.map if args.map.is_absolute() else REPO / args.map
    if not map_path.exists():
        raise SystemExit(f"no such map: {map_path}")

    name, theme = resolve_theme(vocab, args.theme, map_path)

    mix = prefix_mix(theme)
    if args.list_palette:
        print(f"theme '{name}' asset families:")
        for p, n, pct in mix:
            print(f"  {p:24} {n:6} placements  ({pct:.0f}%)")
        return 0

    if args.palette:
        allowed = [p.strip() for p in args.palette.split(",") if p.strip()]
        theme = apply_palette(theme, allowed)
        if not (theme["decals"]["bg"] or theme["decals"]["fg"]):
            raise SystemExit(
                f"palette {allowed} matched no decals in theme '{name}'. "
                f"Available: {', '.join(p for p, _, _ in mix)}"
            )

    seed = random.randrange(1 << 30) if args.seed < 0 else args.seed
    rng = random.Random(seed)

    data = CB.read_map(map_path)
    rooms = CB.get_rooms(data)

    print(f"map    : {map_path.relative_to(REPO)}")
    print(f"theme  : {name}  ({len(theme['decals']['bg'])} bg / {len(theme['decals']['fg'])} fg decals, "
          f"from {theme['rooms']} hand-built rooms)")
    if args.palette:
        print(f"palette: {', '.join(p.strip() for p in args.palette.split(','))}")
    elif mix and mix[0][2] < 60:
        top = ", ".join(f"{p} {pct:.0f}%" for p, _, pct in mix[:3])
        print(f"  note : mixed asset families ({top}) - output will look eclectic.")
        print(f"         narrow it with --palette, or inspect with --list-palette")
    print(f"seed   : {seed}")

    if not args.no_style:
        n = apply_stylegrounds(data, theme)
        print(f"style : {n} styleground layers applied" if n
              else "style : theme has no styleground stack")

    bg_char = next(iter(theme.get("tilesets", {}).get("bg", {})), "1")
    tot_bgtiles = 0
    tot_bg = tot_fg = tot_strip = 0
    touched = 0
    for room in rooms:
        rname = room.get("name", "")
        if not fnmatch(rname, args.rooms):
            continue
        touched += 1
        tot_strip += strip_bogus(room)
        if not args.no_bg_tiles:
            tot_bgtiles += fill_bg_tiles(room, bg_char, args.bg_radius)
        bg, fg = dress_room(room, theme, rng, args.density, args.clear)
        tot_bg += bg
        tot_fg += fg

    print(f"rooms  : {touched} dressed")
    print(f"decals : +{tot_bg} bg, +{tot_fg} fg")
    if tot_bgtiles:
        print(f"bgtile : {tot_bgtiles} background tiles painted (char '{bg_char}', radius {args.bg_radius})")
    if tot_strip:
        print(f"cleaned: {tot_strip} dead decalsFg/decalsBg sections removed")

    if args.dry_run:
        print("dry run - nothing written")
        return 0

    if args.in_place:
        backup = map_path.with_suffix(".bin.bak")
        shutil.copy2(map_path, backup)
        out = map_path
        print(f"backup : {backup.relative_to(REPO)}")
    else:
        out = map_path.with_suffix(".dressed.bin")

    CB.write_map(out, data)
    print(f"written: {out.relative_to(REPO)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
