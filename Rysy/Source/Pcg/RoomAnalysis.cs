namespace DZ.RysyPlugin.Pcg;

public enum WallSide { Left, Right }

public readonly record struct WallCell(int X, int Y, WallSide Side);

/// <summary>
/// Single-pass spatial analysis of a room's tile grid, ported from pcg_toolkit.lua's
/// analyzeRoom. Every smart-placement routine (entities, decals, triggers) runs off this
/// instead of re-scanning the grid, so it only needs to be computed once per room.
/// </summary>
public sealed class RoomAnalysis {
    public required int Width { get; init; }
    public required int Height { get; init; }

    public required List<Poi> Floors { get; init; }
    public required List<Poi> Ceilings { get; init; }
    public required List<WallCell> Walls { get; init; }
    public required List<Poi> AirCells { get; init; }
    public required List<Poi> SolidCells { get; init; }

    /// <summary>Floors with open space on one side at head height -- droppable-off edges.</summary>
    public required List<Poi> Ledges { get; init; }

    /// <summary>Air cells in the upper third of the room with solid support below.</summary>
    public required List<Poi> HighPoints { get; init; }

    /// <summary>Connected-air-region id per air cell, 0 where not computed (solid tiles).</summary>
    public required int[,] RegionId { get; init; }

    public required Dictionary<int, List<Poi>> Regions { get; init; }

    /// <summary>Best spawn candidate: reachable floor near the left side, central height.</summary>
    public required Poi? Spawn { get; init; }

    /// <summary>Best goal candidate: reachable floor near the right side, preferring a different region than spawn.</summary>
    public required Poi? Goal { get; init; }

    public int RegionAt(Poi p) => RegionId[p.X, p.Y];

    public static RoomAnalysis Analyze(PcgGrid tiles) {
        var w = tiles.Width;
        var h = tiles.Height;

        var floors = new List<Poi>();
        var ceilings = new List<Poi>();
        var walls = new List<WallCell>();
        var airCells = new List<Poi>();
        var solidCells = new List<Poi>();
        var regionId = new int[w, h];

        bool IsAir(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && tiles[x, y] == '0';

        for (var y = 1; y < h - 1; y++)
        for (var x = 1; x < w - 1; x++) {
            if (IsAir(x, y)) {
                airCells.Add(new Poi(x, y));
                if (tiles[x, y + 1] != '0')
                    floors.Add(new Poi(x, y));
            } else {
                solidCells.Add(new Poi(x, y));
                if (tiles[x, y - 1] == '0')
                    ceilings.Add(new Poi(x, y));
                if (IsAir(x - 1, y))
                    walls.Add(new WallCell(x, y, WallSide.Left));
                if (IsAir(x + 1, y))
                    walls.Add(new WallCell(x, y, WallSide.Right));
            }
        }

        var ledges = new List<Poi>();
        foreach (var f in floors) {
            var openLeft = IsAir(f.X - 1, f.Y) && IsAir(f.X - 1, f.Y - 1);
            var openRight = IsAir(f.X + 1, f.Y) && IsAir(f.X + 1, f.Y - 1);
            if (openLeft || openRight)
                ledges.Add(f);
        }

        // Connected air regions, flood-filled.
        var regions = new Dictionary<int, List<Poi>>();
        ReadOnlySpan<(int dx, int dy)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
        var nextId = 0;
        var q = new Queue<Poi>();
        for (var y = 1; y < h - 1; y++)
        for (var x = 1; x < w - 1; x++) {
            if (!IsAir(x, y) || regionId[x, y] != 0)
                continue;

            nextId++;
            var region = new List<Poi>();
            regions[nextId] = region;
            q.Clear();
            q.Enqueue(new Poi(x, y));

            while (q.Count > 0) {
                var c = q.Dequeue();
                if (regionId[c.X, c.Y] != 0)
                    continue;
                regionId[c.X, c.Y] = nextId;
                region.Add(c);
                foreach (var (dx, dy) in dirs) {
                    var nx = c.X + dx;
                    var ny = c.Y + dy;
                    if (nx >= 1 && ny >= 1 && nx < w - 1 && ny < h - 1 && IsAir(nx, ny) && regionId[nx, ny] == 0)
                        q.Enqueue(new Poi(nx, ny));
                }
            }
        }

        Poi? spawn = null;
        if (floors.Count > 0) {
            var bestScore = double.PositiveInfinity;
            foreach (var f in floors) {
                var score = f.X + Math.Abs(f.Y - h / 2) * 3;
                if (score < bestScore) {
                    bestScore = score;
                    spawn = f;
                }
            }
        }

        Poi? goal = null;
        if (floors.Count > 0) {
            var spawnRegion = spawn is { } sp ? regionId[sp.X, sp.Y] : 0;
            var bestScore = double.NegativeInfinity;
            foreach (var f in floors) {
                var region = regionId[f.X, f.Y];
                var diffBonus = region != spawnRegion ? 200 : 0;
                var score = f.X * 2 + diffBonus - Math.Abs(f.Y - h / 2) * 2;
                if (score > bestScore) {
                    bestScore = score;
                    goal = f;
                }
            }
        }

        var highPoints = new List<Poi>();
        foreach (var c in airCells) {
            if (c.Y >= h / 3)
                continue;
            var supported = false;
            for (var dy = 1; dy <= 5; dy++) {
                if (c.Y + dy < h && tiles[c.X, c.Y + dy] != '0') {
                    supported = true;
                    break;
                }
            }
            if (supported)
                highPoints.Add(c);
        }

        return new RoomAnalysis {
            Width = w, Height = h,
            Floors = floors, Ceilings = ceilings, Walls = walls,
            AirCells = airCells, SolidCells = solidCells,
            Ledges = ledges, HighPoints = highPoints,
            RegionId = regionId, Regions = regions,
            Spawn = spawn, Goal = goal,
        };
    }

    /// <summary>Floor cells: air with a solid tile directly below. Cheaper than a full analysis.</summary>
    public static List<Poi> FindFloorSurfaces(PcgGrid tiles) {
        var w = tiles.Width;
        var h = tiles.Height;
        var result = new List<Poi>();
        for (var x = 1; x < w - 1; x++)
        for (var y = 1; y < h - 1; y++) {
            if (tiles[x, y] == '0' && tiles[x, y + 1] != '0')
                result.Add(new Poi(x, y));
        }
        return result;
    }

    /// <summary>Solid cells with air directly above -- ceilings, where spikes hang from.</summary>
    public static List<Poi> FindExposedTopSurfaces(PcgGrid tiles) {
        var w = tiles.Width;
        var h = tiles.Height;
        var result = new List<Poi>();
        for (var x = 1; x < w - 1; x++)
        for (var y = 1; y < h - 1; y++) {
            if (tiles[x, y] != '0' && tiles[x, y - 1] == '0')
                result.Add(new Poi(x, y));
        }
        return result;
    }

    /// <summary>
    /// BFS distance from any of <paramref name="sources"/> through cells where
    /// <paramref name="passable"/> holds. Cells with no route stay at int.MaxValue.
    /// </summary>
    public static int[,] DistanceTransform(PcgGrid tiles, IReadOnlyList<Poi> sources,
        Func<int, int, bool>? passable = null) {
        var w = tiles.Width;
        var h = tiles.Height;
        passable ??= (x, y) => tiles[x, y] == '0';

        var dist = new int[w, h];
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            dist[x, y] = int.MaxValue;

        var q = new Queue<Poi>();
        foreach (var s in sources) {
            if (s.X < 0 || s.Y < 0 || s.X >= w || s.Y >= h || !passable(s.X, s.Y))
                continue;
            dist[s.X, s.Y] = 0;
            q.Enqueue(s);
        }

        ReadOnlySpan<(int dx, int dy)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
        while (q.Count > 0) {
            var c = q.Dequeue();
            foreach (var (dx, dy) in dirs) {
                var nx = c.X + dx;
                var ny = c.Y + dy;
                if (nx < 1 || ny < 1 || nx >= w - 1 || ny >= h - 1 || !passable(nx, ny))
                    continue;
                if (dist[nx, ny] > dist[c.X, c.Y] + 1) {
                    dist[nx, ny] = dist[c.X, c.Y] + 1;
                    q.Enqueue(new Poi(nx, ny));
                }
            }
        }

        return dist;
    }
}
