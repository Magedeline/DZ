"""Gravity-aware reachability for Celeste rooms.

`loenn_mcp.pcg_helper._bfs_reachable` floods 4-directionally through air, which
treats the room as water: a 20-tile shaft reads as reachable when Madeline can
only jump about 3. Everything it calls "playable" is therefore unverified.

This models movement instead. A *foothold* is an air tile with a solid tile
directly beneath it -- somewhere you can stand. From one foothold you can reach
another if it falls inside the movement envelope AND some plausible path between
them is clear of solids.

The movement numbers are deliberately approximate and tunable (see Params).
They are not extracted from Celeste's source. What matters is that the same
model is applied to hand-built and generated rooms, so the *comparison* is
meaningful even though the absolute percentage is not a physics claim.

The model covers walking, jumping, and falling. It does NOT model dashes,
springs, wall-climb stamina, or moving platforms unless `dash` is enabled --
so a hand-built room scoring low means "needs mechanics", not "broken".
"""

from __future__ import annotations

from collections import deque
from dataclasses import dataclass

from _mapio import TILE, make_solid, parse_grid, section


@dataclass(frozen=True)
class Params:
    """Movement envelope, in tiles."""

    jump: int = 3          # how far straight up a jump carries you
    run: int = 5           # horizontal reach while rising
    fall: int = 6          # how far down to consider (drop + drift)
    drift: int = 6         # horizontal drift available while falling
    dash: int = 0          # extra horizontal reach, 0 = dashes not modelled


DEFAULT = Params()


def footholds(grid: list[str], w: int, h: int) -> set[tuple[int, int]]:
    """Air tiles you can stand on."""
    solid = make_solid(grid, w, h)
    return {
        (x, y)
        for y in range(h)
        for x in range(w)
        if not solid(x, y) and solid(x, y + 1)
    }


def _clear_line(solid, x1: int, y1: int, x2: int, y2: int) -> bool:
    """True if every tile on the axis-aligned run between the points is air."""
    if x1 == x2:
        for y in range(min(y1, y2), max(y1, y2) + 1):
            if solid(x1, y):
                return False
        return True
    if y1 == y2:
        for x in range(min(x1, x2), max(x1, x2) + 1):
            if solid(x, y1):
                return False
        return True
    raise ValueError("expected an axis-aligned segment")


def _path_clear(solid, a: tuple[int, int], b: tuple[int, int], p: Params) -> bool:
    """Is there a plausible jump/fall arc from a to b through open air?

    Tries several corridor shapes and accepts if any is clear: go up then across
    then down (the classic jump arc), across then down (a run-off), and down
    then across (a drop). Real arcs are parabolic; these bound them closely
    enough to separate open caves from segmented rooms.
    """
    (x1, y1), (x2, y2) = a, b
    apexes = {min(y1, y2)}
    if y2 >= y1:
        apexes.add(max(y1 - p.jump, min(y1, y2)))  # hop up before dropping
    for apex in apexes:
        if (
            _clear_line(solid, x1, y1, x1, apex)
            and _clear_line(solid, x1, apex, x2, apex)
            and _clear_line(solid, x2, apex, x2, y2)
        ):
            return True
    # Straight across at the landing height, then settle.
    if _clear_line(solid, x1, y1, x1, y2) and _clear_line(solid, x1, y2, x2, y2):
        return True
    return False


def _envelope(p: Params) -> list[tuple[int, int]]:
    """Offsets a single move could plausibly cover, nearest first."""
    out = []
    for dy in range(-p.jump, p.fall + 1):
        span = (p.run if dy <= 0 else p.drift) + p.dash
        for dx in range(-span, span + 1):
            if dx or dy:
                out.append((dx, dy))
    out.sort(key=lambda d: d[0] * d[0] + d[1] * d[1])
    return out


def reachable(
    grid: list[str],
    w: int,
    h: int,
    start: tuple[int, int],
    stand: set[tuple[int, int]] | None = None,
    p: Params = DEFAULT,
) -> set[tuple[int, int]]:
    """Footholds reachable from `start` under the movement model."""
    solid = make_solid(grid, w, h)
    stand = footholds(grid, w, h) if stand is None else stand
    if start not in stand:
        return set()
    env = _envelope(p)
    seen = {start}
    q = deque([start])
    while q:
        x, y = q.popleft()
        for dx, dy in env:
            t = (x + dx, y + dy)
            if t in stand and t not in seen and _path_clear(solid, (x, y), t, p):
                seen.add(t)
                q.append(t)
    return seen


def components(
    grid: list[str], w: int, h: int, stand: set[tuple[int, int]] | None = None, p: Params = DEFAULT
) -> list[set[tuple[int, int]]]:
    """Footholds grouped into mutually reachable islands, largest first.

    Reachability here is not symmetric (you can drop somewhere you cannot climb
    back from), so these are one-way closures, not true components -- good
    enough to spot a room split into disconnected pockets.
    """
    stand = footholds(grid, w, h) if stand is None else stand
    todo = set(stand)
    out: list[set[tuple[int, int]]] = []
    while todo:
        seed = min(todo, key=lambda s: (s[1], s[0]))
        got = reachable(grid, w, h, seed, stand, p) & todo
        got.add(seed)
        out.append(got)
        todo -= got
    out.sort(key=len, reverse=True)
    return out


def entry_point(room: dict, stand: set[tuple[int, int]]) -> tuple[int, int] | None:
    """Where the player starts: the spawn if there is one, else top-left stand."""
    if not stand:
        return None
    for e in (section(room, "entities") or {}).get("__children", []):
        if e.get("__name") == "player":
            px, py = int(e.get("x", 0)) // TILE, int(e.get("y", 0)) // TILE
            if (px, py) in stand:
                return (px, py)
            return min(stand, key=lambda s: (s[0] - px) ** 2 + (s[1] - py) ** 2)
    return min(stand, key=lambda s: (s[1], s[0]))


def score_room(room: dict, p: Params = DEFAULT) -> dict | None:
    """Connectivity summary for one room."""
    grid, w, h = parse_grid(room)
    if w < 4 or h < 4:
        return None
    stand = footholds(grid, w, h)
    if not stand:
        return None
    start = entry_point(room, stand)
    if start is None:
        return None
    got = reachable(grid, w, h, start, stand, p)
    comps = components(grid, w, h, stand, p)
    return {
        "name": room.get("name", ""),
        "footholds": len(stand),
        "reachable": len(got),
        "pct": 100.0 * len(got) / len(stand),
        "islands": len(comps),
        "largest_island_pct": 100.0 * len(comps[0]) / len(stand) if comps else 0.0,
    }
