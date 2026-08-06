"""Score a map's rooms on traversal structure, against the hand-built corpus.

Celeste rooms earn their difficulty by being *segmented*: most of the room is
out of reach until you spend a dash, a spring, or a wall-jump. Measured over
the 1,696 hand-built DZ rooms with >=20 footholds, the median room is only 32%
reachable by plain walking and jumping, and just 18% of rooms are >90%
reachable -- those are the transitions and rest beats.

Generated rooms sit at a 100% median with 73% of rooms >90%. They are open
caves: you can walk almost anywhere, so nothing gates anything. That is a
different failure from the one PCG literature usually warns about (unreachable
exits), and the generator cannot see it, because its own check floods
4-directionally through air with no gravity.

    python Tools/PCG/score_rooms.py --calibrate          # refresh the band
    python Tools/PCG/score_rooms.py <map.bin>            # score against it
    python Tools/PCG/score_rooms.py <map.bin> --max-cave-rate 0.30

Exits 1 when the cave rate exceeds --max-cave-rate, so it works as a gate in a
generate-and-check loop.
"""

from __future__ import annotations

import argparse
import json
import statistics as st
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from _mapio import CB  # noqa: E402
from reachability import DEFAULT, Params, score_room  # noqa: E402

REPO = Path(__file__).resolve().parents[2]
BAND = REPO / "PCG" / "connectivity_band.json"

CAVE_PCT = 90.0      # at or above this, the room imposes no traversal cost
MIN_FOOTHOLDS = 20   # tiny rooms are noise


def collect(paths: list[Path], p: Params) -> list[dict]:
    out = []
    for path in paths:
        try:
            rooms = CB.get_rooms(CB.read_map(path))
        except Exception:  # noqa: BLE001 - a corrupt map should not abort the sweep
            continue
        for room in rooms:
            s = score_room(room, p)
            if s and s["footholds"] >= MIN_FOOTHOLDS:
                s["map"] = str(path.relative_to(REPO)).replace("\\", "/")
                out.append(s)
    return out


def calibrate(p: Params) -> dict:
    paths = sorted(x for x in (REPO / "Maps").rglob("*.bin") if "wip" not in x.parts)
    scores = collect(paths, p)
    if not scores:
        raise SystemExit("no hand-built rooms found under Maps/")
    pct = sorted(s["pct"] for s in scores)

    def q(f: float) -> float:
        return pct[min(int(len(pct) * f), len(pct) - 1)]

    band = {
        "rooms": len(scores),
        "maps": len(paths),
        "params": vars(p),
        "p10": round(q(0.10), 1),
        "p25": round(q(0.25), 1),
        "median": round(st.median(pct), 1),
        "p75": round(q(0.75), 1),
        "p90": round(q(0.90), 1),
        "cave_rate": round(sum(1 for v in pct if v >= CAVE_PCT) / len(pct), 4),
    }
    BAND.parent.mkdir(parents=True, exist_ok=True)
    BAND.write_text(json.dumps(band, indent=2), encoding="utf-8")
    return band


def main() -> int:
    ap = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter
    )
    ap.add_argument("map", nargs="?", type=Path)
    ap.add_argument("--calibrate", action="store_true", help="rebuild the band from hand-built maps")
    ap.add_argument("--max-cave-rate", type=float, help="exit 1 if the cave rate exceeds this")
    ap.add_argument("--list", type=int, default=0, metavar="N", help="show the N most open rooms")
    ap.add_argument("--dash", type=int, default=0, help="model a dash worth this many extra tiles")
    args = ap.parse_args()

    p = Params(dash=args.dash) if args.dash else DEFAULT

    if args.calibrate:
        band = calibrate(p)
        print(f"calibrated on {band['rooms']} rooms from {band['maps']} hand-built maps")
        print(f"  connectivity: p10={band['p10']}  median={band['median']}  p90={band['p90']}")
        print(f"  cave rate   : {band['cave_rate']:.1%}  (rooms >={CAVE_PCT:.0f}% reachable)")
        print(f"  written     : {BAND.relative_to(REPO)}")
        if not args.map:
            return 0

    if not args.map:
        ap.error("give a map to score, or --calibrate")

    if not BAND.exists():
        raise SystemExit(f"missing {BAND.relative_to(REPO)} - run with --calibrate first")
    band = json.loads(BAND.read_text(encoding="utf-8"))

    map_path = args.map if args.map.is_absolute() else REPO / args.map
    if not map_path.exists():
        raise SystemExit(f"no such map: {map_path}")

    scores = collect([map_path], p)
    if not scores:
        raise SystemExit(f"no rooms with >={MIN_FOOTHOLDS} footholds in {map_path.name}")

    pct = sorted(s["pct"] for s in scores)
    caves = [s for s in scores if s["pct"] >= CAVE_PCT]
    rate = len(caves) / len(scores)

    print(f"map   : {map_path.relative_to(REPO)}")
    print(f"rooms : {len(scores)} scored (>={MIN_FOOTHOLDS} footholds)")
    print()
    print(f"{'':16}{'this map':>10}{'hand-built':>12}")
    print(f"{'median conn.':16}{st.median(pct):>9.0f}%{band['median']:>11.0f}%")
    print(f"{'p10 conn.':16}{pct[len(pct)//10]:>9.0f}%{band['p10']:>11.0f}%")
    print(f"{'cave rate':16}{rate:>9.0%}{band['cave_rate']:>11.0%}")

    verdict = "OK"
    if rate > band["cave_rate"] * 2:
        verdict = "TOO OPEN - rooms impose almost no traversal cost"
    elif rate > band["cave_rate"] * 1.3:
        verdict = "drifting open"
    print(f"\nverdict: {verdict}")

    if args.list and caves:
        print(f"\nmost open rooms (regenerate or add segmentation):")
        for s in sorted(caves, key=lambda s: (-s["pct"], -s["footholds"]))[: args.list]:
            print(f"  {s['name']:14} {s['pct']:5.0f}% reachable  ({s['footholds']} footholds)")

    if args.max_cave_rate is not None and rate > args.max_cave_rate:
        print(f"\nFAIL: cave rate {rate:.0%} exceeds --max-cave-rate {args.max_cave_rate:.0%}")
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
