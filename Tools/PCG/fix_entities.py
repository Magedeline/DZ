"""Repair the entity layer of a generated Celeste map.

The stock loenn-mcp generator gets three things wrong, all measurable against
the hand-built maps in this mod (see PCG/vocabulary.json):

  1. It emits `spikes`, which is not a Celeste entity. The real ones are
     `spikesUp` / `spikesDown` / `spikesLeft` / `spikesRight`, and they carry
     `width` (up/down) or `height` (left/right) plus direction-specific origins.
     The generated entity already knows its own `direction`, so this is a rename
     plus the missing fields.

  2. Collectible density is ~100x too high: 12.97 strawberries per room against
     0.11 hand-built, 23.30 springs against 0.21. Anything overshooting the
     hand-built rate is thinned, keeping a spatially spread subset rather than
     an arbitrary prefix.

  3. It writes a player spawn into the first room only. Celeste uses `player`
     entities as respawn points, and hand-built maps carry one per room
     (01_City: 45/45), so dying anywhere else is unrecoverable.

    python Tools/PCG/fix_entities.py Maps/DZ/wip/gen_large_60.dressed.collect.bin

Writes `<map>.fixed.bin` unless --in-place is given.
"""

from __future__ import annotations

import argparse
import json
import random
import shutil
import sys
from collections import Counter
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
VOCAB = REPO / "PCG" / "vocabulary.json"

# Field layout per direction, copied from hand-built placements.
SPIKE_SHAPE = {
    "up": {"name": "spikesUp", "span": "width", "originX": 0, "originY": 4},
    "down": {"name": "spikesDown", "span": "width", "originX": 0, "originY": 0},
    "left": {"name": "spikesLeft", "span": "height", "originX": 4, "originY": 0},
    "right": {"name": "spikesRight", "span": "height", "originX": 0, "originY": 0},
}

# Never thin these, however dense they look.
PROTECTED = {"player", "spikesUp", "spikesDown", "spikesLeft", "spikesRight"}


def entities(room: dict) -> list[dict]:
    return (section(room, "entities") or {}).get("__children", [])


def hand_built_rates() -> dict[str, float]:
    """Per-room placement rate for every entity in the hand-built corpus."""
    v = json.loads(VOCAB.read_text(encoding="utf-8"))
    counts: Counter = Counter()
    rooms = 0
    for t in v["themes"].values():
        rooms += t["rooms"]
        for e in t["entities"]:
            counts[e["name"]] += e["count"]
    return {n: c / max(rooms, 1) for n, c in counts.items()}


def fix_spikes(rooms: list[dict]) -> tuple[int, Counter]:
    """Rename bare `spikes` to its directional entity and add missing fields."""
    fixed = 0
    kinds: Counter = Counter()
    for room in rooms:
        for e in entities(room):
            if e.get("__name") != "spikes":
                continue
            shape = SPIKE_SHAPE.get(str(e.get("direction", "up")).lower())
            if shape is None:
                continue
            e["__name"] = shape["name"]
            e.pop("direction", None)
            e.setdefault("type", "default")
            e[shape["span"]] = int(e.get(shape["span"], TILE))
            e["originX"] = shape["originX"]
            e["originY"] = shape["originY"]
            kinds[shape["name"]] += 1
            fixed += 1
    return fixed, kinds


def thin(
    rooms: list[dict],
    rates: dict[str, float],
    overshoot: float,
    keep: set[str],
    min_count: int,
    rng: random.Random,
) -> list[tuple[str, int, int, float, float]]:
    """Cut entity types that far exceed their hand-built per-room rate.

    Selection is global rather than per-room, so the kept total always hits the
    target exactly. Survivors are chosen shuffled-then-spaced, so what remains
    is spread out instead of clumped. Types with only a handful of placements
    are left alone -- a lone heart gem is not a density problem.
    """
    n_rooms = max(len(rooms), 1)
    present: Counter = Counter()
    for room in rooms:
        for e in entities(room):
            present[e["__name"]] += 1

    min_sep_sq = (6 * TILE) ** 2
    report = []

    for name, have in present.items():
        if name in PROTECTED or name in keep or have < min_count:
            continue
        target_rate = rates.get(name, 0.0)
        have_rate = have / n_rooms
        if target_rate <= 0 or have_rate <= target_rate * overshoot:
            continue
        target = max(1, round(target_rate * n_rooms))
        if target >= have:
            continue

        cands = [(ri, e) for ri, room in enumerate(rooms) for e in entities(room) if e["__name"] == name]
        rng.shuffle(cands)

        kept: list[tuple[int, dict]] = []
        kept_ids: set[int] = set()
        for ri, e in cands:
            if len(kept) >= target:
                break
            if all(
                ri != kri
                or (e.get("x", 0) - k.get("x", 0)) ** 2 + (e.get("y", 0) - k.get("y", 0)) ** 2 >= min_sep_sq
                for kri, k in kept
            ):
                kept.append((ri, e))
                kept_ids.add(id(e))
        # Spacing is a preference, not a quota -- top up if it was too strict.
        for _, e in cands:
            if len(kept_ids) >= target:
                break
            kept_ids.add(id(e))

        for room in rooms:
            sec = section(room, "entities")
            if sec:
                sec["__children"] = [
                    e for e in sec["__children"] if e["__name"] != name or id(e) in kept_ids
                ]
        report.append((name, have, len(kept_ids), have_rate, target_rate))
    return report


def add_spawns(rooms: list[dict], start_id: int) -> tuple[int, int]:
    """Give every room a respawn point, matching hand-built convention."""
    eid = start_id
    added = 0
    for room in rooms:
        if any(e.get("__name") == "player" for e in entities(room)):
            continue
        grid, w, h = parse_grid(room)
        if w == 0 or h == 0:
            continue
        spots = build_spots(grid, w, h)
        pool = spots["floor"]
        if not pool:
            continue
        # Low and to the left reads as a room entrance.
        tx, ty = min(pool, key=lambda p: (-p[1], p[0]))
        eid += 1
        ensure_section(room, "entities")["__children"].append({
            "__name": "player",
            "id": eid,
            "x": tx * TILE + TILE // 4,
            "y": (ty + 1) * TILE,
            "originX": 0,
            "originY": 0,
            "__children": [],
        })
        added += 1
    return added, eid


def main() -> int:
    ap = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter
    )
    ap.add_argument("map", type=Path)
    ap.add_argument("--overshoot", type=float, default=3.0,
                    help="thin an entity once it exceeds this multiple of the hand-built rate")
    ap.add_argument("--keep", default="", help="comma-separated entity names to never thin")
    ap.add_argument("--min-count", type=int, default=10,
                    help="leave entity types with fewer placements than this alone")
    ap.add_argument("--no-thin", action="store_true")
    ap.add_argument("--no-spawns", action="store_true")
    ap.add_argument("--no-spikes", action="store_true")
    ap.add_argument("--seed", type=int, default=-1)
    ap.add_argument("--in-place", action="store_true", help="overwrite the map (a .bak is kept)")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    if not VOCAB.exists():
        raise SystemExit(f"missing {VOCAB}. Run Tools/PCG/harvest_vocabulary.py first.")

    map_path = args.map if args.map.is_absolute() else REPO / args.map
    if not map_path.exists():
        raise SystemExit(f"no such map: {map_path}")

    seed = random.randrange(1 << 30) if args.seed < 0 else args.seed
    rng = random.Random(seed)
    data = CB.read_map(map_path)
    rooms = CB.get_rooms(data)
    rates = hand_built_rates()

    print(f"map   : {map_path.relative_to(REPO)}  ({len(rooms)} rooms)")
    print(f"seed  : {seed}")

    if not args.no_spikes:
        n, kinds = fix_spikes(rooms)
        if n:
            detail = ", ".join(f"{k}x{v}" for k, v in sorted(kinds.items()))
            print(f"spikes: renamed {n} invalid 'spikes' -> {detail}")
        else:
            print("spikes: none to fix")

    if not args.no_thin:
        keep = {s.strip() for s in args.keep.split(",") if s.strip()}
        rep = thin(rooms, rates, args.overshoot, keep, args.min_count, rng)
        if rep:
            print(f"{'thin':<14}{'was':>7}{'now':>7}   {'per-room now':>12}  {'hand-built':>10}")
            for name, have, kept, have_rate, target in sorted(rep, key=lambda r: -r[1]):
                print(f"  {name:<12}{have:>7}{kept:>7}   "
                      f"{kept/max(len(rooms),1):>12.2f}  {target:>10.2f}")
        else:
            print("thin  : nothing over threshold")

    if not args.no_spawns:
        added, _ = add_spawns(rooms, max_entity_id(data))
        have = sum(1 for r in rooms if any(e.get("__name") == "player" for e in entities(r)))
        print(f"spawns: added {added}  ->  {have}/{len(rooms)} rooms have a respawn point")

    if args.dry_run:
        print("dry run - nothing written")
        return 0

    if args.in_place:
        backup = map_path.with_suffix(".bin.bak")
        shutil.copy2(map_path, backup)
        out = map_path
        print(f"backup: {backup.relative_to(REPO)}")
    else:
        out = map_path.with_suffix(".fixed.bin")

    CB.write_map(out, data)
    print(f"written: {out.relative_to(REPO)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
