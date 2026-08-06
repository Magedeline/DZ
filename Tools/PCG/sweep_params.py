"""Search loenn-mcp generator settings for ones that produce Celeste-shaped rooms.

score_rooms.py established that hand-built DZ rooms sit at a 32% median
connectivity with an 18.2% cave rate (cave = >=90% of the room reachable by
plain walking and jumping), while the shipped generator settings produce a 100%
median and a 73% cave rate -- open blobs that gate nothing.

This drives `pcg_helper.run_pipeline` directly, in memory, across a grid of
settings and scores each result against that band. Nothing is written to a map;
the point is to find which knob actually moves the number.

The training map is treated as a variable too. MdMC learns tile transitions
from whatever rooms it is given, so training on a hand-built chapter may matter
more than any tuning parameter.

    python Tools/PCG/sweep_params.py --rooms 12
    python Tools/PCG/sweep_params.py --rooms 12 --quick
"""

from __future__ import annotations

import argparse
import itertools
import json
import statistics as st
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from _mapio import CB  # noqa: E402
from reachability import DEFAULT as MOVE  # noqa: E402
from reachability import score_room  # noqa: E402

REPO = Path(__file__).resolve().parents[2]
BAND = REPO / "PCG" / "connectivity_band.json"
OUT = REPO / "PCG" / "sweep_results.json"

from loenn_mcp import pcg_helper as ph  # noqa: E402

MIN_FOOTHOLDS = 20

TRAINERS = {
    "city": "Maps/DZ/0/01_City.bin",
    "summit": "Maps/DZ/0/09_Summit.bin",
    "fractured": "Maps/DZ/3/05_Fractured.bin",
}

CONFIGS = ["000011012", "001001112", "010111010", "111101111"]
MODES = ["mdmc", "wfc", "hybrid"]
PROBAS = [0.0, 0.5, 1.0]


def evaluate(rooms: list[dict]) -> dict | None:
    scores = [
        s for r in rooms if (s := score_room(r, MOVE)) and s["footholds"] >= MIN_FOOTHOLDS
    ]
    if not scores:
        return None
    pct = sorted(s["pct"] for s in scores)
    return {
        "scored": len(scores),
        "median": round(st.median(pct), 1),
        "cave_rate": round(sum(1 for v in pct if v >= 90.0) / len(pct), 3),
    }


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--rooms", type=int, default=12, help="rooms generated per configuration")
    ap.add_argument("--seed", type=int, default=7)
    ap.add_argument("--quick", action="store_true", help="one config/mode, sweep trainers + proba only")
    ap.add_argument("--top", type=int, default=15)
    args = ap.parse_args()

    if not BAND.exists():
        raise SystemExit("run: python Tools/PCG/score_rooms.py --calibrate")
    band = json.loads(BAND.read_text(encoding="utf-8"))
    target = band["cave_rate"]
    print(f"target: cave rate {target:.1%}, median connectivity {band['median']:.0f}%"
          f"  (from {band['rooms']} hand-built rooms)\n")

    trained: dict[str, list[dict]] = {}
    for name, rel in TRAINERS.items():
        p = REPO / rel
        if p.exists():
            trained[name] = CB.get_rooms(CB.read_map(p))
        else:
            print(f"  (skipping trainer {name}: {rel} not found)")

    configs = CONFIGS[:1] if args.quick else CONFIGS
    modes = MODES[:1] if args.quick else MODES
    grid = list(itertools.product(trained, configs, modes, PROBAS))
    print(f"evaluating {len(grid)} configurations x {args.rooms} rooms ...\n")

    results = []
    t0 = time.time()
    for i, (tname, cfg, mode, proba) in enumerate(grid, 1):
        try:
            out = ph.run_pipeline(
                training_rooms=trained[tname],
                room_count=args.rooms,
                proba=proba,
                configuration=cfg,
                generation_mode=mode,
                seed=args.seed,
                place_decals_flag=False,
                place_triggers_flag=False,
                place_entities_flag=False,
            )
            ev = evaluate(out.get("rooms", []))
        except Exception as exc:  # noqa: BLE001 - a bad combo should not kill the sweep
            print(f"  [{i}/{len(grid)}] {tname}/{cfg}/{mode}/p={proba} FAILED: {exc!r}")
            continue
        if not ev:
            continue
        ev.update(trainer=tname, config=cfg, mode=mode, proba=proba)
        ev["miss"] = abs(ev["cave_rate"] - target)
        results.append(ev)
        print(f"  [{i}/{len(grid)}] {tname:9} {cfg} {mode:6} p={proba:<4} "
              f"-> median {ev['median']:5.1f}%  cave {ev['cave_rate']:5.1%}")

    if not results:
        print("\nno configuration produced scorable rooms")
        return 1

    results.sort(key=lambda r: r["miss"])
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps({"target": target, "results": results}, indent=2), encoding="utf-8")

    print(f"\n=== closest to hand-built (cave rate {target:.1%}) ===")
    print(f"{'trainer':10}{'config':11}{'mode':8}{'proba':7}{'median':>8}{'cave':>8}")
    for r in results[: args.top]:
        print(f"{r['trainer']:10}{r['config']:11}{r['mode']:8}{r['proba']:<7}"
              f"{r['median']:>7.1f}%{r['cave_rate']:>7.1%}")

    best = results[0]
    print(f"\nbest: trainer={best['trainer']} config={best['config']} "
          f"mode={best['mode']} proba={best['proba']}")
    print(f"      cave rate {best['cave_rate']:.1%} vs {target:.1%} target "
          f"(baseline shipped settings: 73%)")
    print(f"\nelapsed {time.time()-t0:.0f}s   full results: {OUT.relative_to(REPO)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
