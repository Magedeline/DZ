namespace DZ.RysyPlugin.Pcg;

public enum ExitSide { Left, Right, Top, Bottom }

/// <summary>A run of air along a room border wide enough to serve as a doorway.</summary>
public sealed record RoomExit(ExitSide Side, IReadOnlyList<Poi> Cells);

/// <summary>
/// Exit detection and the playability gate from pcg_toolkit.lua (paper section 4.1).
///
/// Unlike <see cref="Playability"/>, which models the Celeste moveset to decide whether a
/// room can be *traversed*, this is the cheaper structural gate: are the doorways
/// connected through air at all, and is there enough open space to be worth playing.
/// </summary>
public static class Connectivity {
    /// <summary>Minimum air ratio of the interior, as a percentage, for a room to pass.</summary>
    public const int MinAirPercent = 15;

    /// <summary>Air runs of at least <paramref name="minRun"/> tiles on each border.</summary>
    public static List<RoomExit> FindExits(PcgGrid grid, int minRun = 2) {
        var w = grid.Width;
        var h = grid.Height;
        var exits = new List<RoomExit>();

        ScanLine(h, y => grid[0, y], y => new Poi(0, y), ExitSide.Left);
        ScanLine(h, y => grid[w - 1, y], y => new Poi(w - 1, y), ExitSide.Right);
        ScanLine(w, x => grid[x, 0], x => new Poi(x, 0), ExitSide.Top);
        ScanLine(w, x => grid[x, h - 1], x => new Poi(x, h - 1), ExitSide.Bottom);

        return exits;

        void ScanLine(int len, Func<int, char> getCell, Func<int, Poi> makeCell, ExitSide side) {
            int? runStart = null;
            for (var k = 0; k <= len; k++) {
                var isAir = k < len && getCell(k) == '0';
                if (isAir) {
                    runStart ??= k;
                    continue;
                }
                if (runStart is { } start && k - start >= minRun) {
                    var cells = new List<Poi>();
                    for (var i = start; i < k; i++)
                        cells.Add(makeCell(i));
                    exits.Add(new RoomExit(side, cells));
                }
                runStart = null;
            }
        }
    }

    /// <summary>
    /// Multi-source BFS through air from one exit's cells to another's.
    /// Returns the distance, or null when the two are disconnected.
    /// </summary>
    public static int? BfsExitToExit(PcgGrid grid, IReadOnlyList<Poi> fromCells, IReadOnlyList<Poi> toCells)
        => BfsExitToExitWithPath(grid, fromCells, toCells)?.Dist;

    /// <summary>
    /// As <see cref="BfsExitToExit"/>, but also returns the cells along the route. Scoring
    /// uses the path to build the area of interest.
    /// </summary>
    public static (int Dist, List<Poi> Path)? BfsExitToExitWithPath(PcgGrid grid,
        IReadOnlyList<Poi> fromCells, IReadOnlyList<Poi> toCells) {
        var w = grid.Width;
        var h = grid.Height;

        var target = new HashSet<int>();
        foreach (var c in toCells)
            target.Add(c.Y * w + c.X);

        var visited = new HashSet<int>();
        var parent = new Dictionary<int, int>();
        var q = new Queue<(int x, int y, int dist)>();

        foreach (var c in fromCells) {
            if (c.X < 0 || c.Y < 0 || c.X >= w || c.Y >= h || grid[c.X, c.Y] != '0')
                continue;
            if (visited.Add(c.Y * w + c.X))
                q.Enqueue((c.X, c.Y, 0));
        }

        ReadOnlySpan<(int dx, int dy)> dirs = [(-1, 0), (1, 0), (0, -1), (0, 1)];

        while (q.Count > 0) {
            var (x, y, dist) = q.Dequeue();
            var key = y * w + x;

            if (target.Contains(key)) {
                var path = new List<Poi>();
                var k = key;
                while (true) {
                    path.Add(new Poi(k % w, k / w));
                    if (!parent.TryGetValue(k, out var p))
                        break;
                    k = p;
                }
                return (dist, path);
            }

            foreach (var (dx, dy) in dirs) {
                var nx = x + dx;
                var ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || grid[nx, ny] != '0')
                    continue;
                var nk = ny * w + nx;
                if (!visited.Add(nk))
                    continue;
                parent[nk] = key;
                q.Enqueue((nx, ny, dist + 1));
            }
        }

        return null;
    }

    /// <summary>
    /// Horizontal sweep fallback: BFS through air from the leftmost opening on a random
    /// lower row to the right edge. Returns -1 when no route exists.
    /// </summary>
    public static int BfsPathLength(PcgGrid grid, Random rng) {
        var w = grid.Width;
        var h = grid.Height;
        if (w < 4 || h < 4)
            return -1;

        var startY = rng.NextRangeInclusive(h / 2, h - 3);
        var sx = -1;
        for (var x = 1; x < w - 1; x++) {
            if (grid[x, startY] != '0')
                continue;
            sx = x;
            break;
        }
        if (sx < 0)
            return -1;

        var visited = new bool[w, h];
        var q = new Queue<(int x, int y, int dist)>();
        q.Enqueue((sx, startY, 0));
        visited[sx, startY] = true;

        ReadOnlySpan<(int dx, int dy)> dirs = [(-1, 0), (1, 0), (0, -1), (0, 1)];

        while (q.Count > 0) {
            var (x, y, dist) = q.Dequeue();
            if (x == w - 2)
                return dist;
            foreach (var (dx, dy) in dirs) {
                var nx = x + dx;
                var ny = y + dy;
                if (nx < 1 || ny < 1 || nx >= w - 1 || ny >= h - 1)
                    continue;
                if (grid[nx, ny] != '0' || visited[nx, ny])
                    continue;
                visited[nx, ny] = true;
                q.Enqueue((nx, ny, dist + 1));
            }
        }

        return -1;
    }

    /// <summary>
    /// Structural fallback for rooms without two border exits: needs a few floor surfaces
    /// at varying heights and enough open interior.
    /// </summary>
    public static bool InteriorLooksPlayable(PcgGrid grid) {
        var w = grid.Width;
        var h = grid.Height;

        var floorY = new List<int>();
        for (var x = 1; x < w - 1; x++)
        for (var y = h - 2; y >= 1; y--) {
            if (grid[x, y] != '0' || grid[x, y + 1] == '0')
                continue;
            floorY.Add(y);
            break;
        }

        var minFloors = Math.Min(3, w / 4);
        if (floorY.Count < minFloors)
            return false;

        // A room whose floors are all at one height is a flat corridor, not a level.
        if (floorY.Count >= 3 && floorY.Max() - floorY.Min() < 2)
            return false;

        return HasEnoughAir(grid);
    }

    /// <summary>
    /// The playability gate. With two or more exits, every exit must be reachable from the
    /// first through air; otherwise falls back to <see cref="InteriorLooksPlayable"/>.
    /// </summary>
    public static bool IsPlayable(PcgGrid grid) {
        var exits = FindExits(grid);
        if (exits.Count < 2)
            return InteriorLooksPlayable(grid);

        for (var i = 1; i < exits.Count; i++) {
            if (BfsExitToExit(grid, exits[0].Cells, exits[i].Cells) is null)
                return false;
        }

        return HasEnoughAir(grid);
    }

    private static bool HasEnoughAir(PcgGrid grid) {
        var w = grid.Width;
        var h = grid.Height;
        if (w < 3 || h < 3)
            return false;

        var air = 0;
        for (var x = 1; x < w - 1; x++)
        for (var y = 1; y < h - 1; y++) {
            if (grid[x, y] == '0')
                air++;
        }

        return air >= (w - 2) * (h - 2) * MinAirPercent / 100;
    }
}
