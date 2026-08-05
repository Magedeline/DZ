namespace DZ.RysyPlugin.Pcg;

public readonly record struct Poi(int X, int Y);

/// <summary>
/// Traversal analysis and repair, ported from pcg_toolkit.lua's PlatformerRepair section.
///
/// Reachability is a budgeted BFS that stands in for Celeste's moveset: falling is free,
/// horizontal movement is cheap, climbing costs more, and the budget refills whenever the
/// player is standing on ground or hugging a wall. Rooms whose exits are not mutually
/// reachable get corridors carved between them.
/// </summary>
public static class Playability {
    /// <summary>Air tiles traversable before needing ground or a wall again.</summary>
    public const int AirBudget = 8;

    /// <summary>Budget cost of moving up one tile. Left/right cost 1, down costs 0.</summary>
    public const int UpCost = 2;

    /// <summary>Unsupported path tiles tolerated before a stepping stone is inserted.</summary>
    public const int MaxUnsupportedRun = 4;

    /// <summary>
    /// Points of interest: the midpoints of air runs along each edge, moved one tile inward.
    /// These stand in for the room's doorways.
    ///
    /// DIVERGENCE FROM LUA: the original's air test is
    ///   (horizontal and tiles[k][edge] == "0") or (tiles[edge][k] == "0")
    /// Because Lua binds 'and' tighter than 'or', the horizontal branch also evaluates the
    /// transposed lookup tiles[edge][k]. That both ORs in a meaningless second test and
    /// throws outright for rooms taller than they are wide, where edge = h-1 exceeds the
    /// grid's x range. Ported here as the evidently intended single test per orientation.
    /// </summary>
    public static List<Poi> FindPois(PcgGrid grid) {
        var w = grid.Width;
        var h = grid.Height;
        var pois = new List<Poi>();

        EdgeRuns(horizontal: true, edge: 0, inward: 1);
        EdgeRuns(horizontal: true, edge: h - 1, inward: h - 2);
        EdgeRuns(horizontal: false, edge: 0, inward: 1);
        EdgeRuns(horizontal: false, edge: w - 1, inward: w - 2);

        if (pois.Count < 2) {
            // Fallback: any air tile sitting on solid ground, first and last found.
            Poi? left = null, right = null;
            for (var x = 1; x < w - 1; x++)
            for (var y = 1; y < h - 1; y++) {
                if (grid[x, y] != '0' || grid[x, y + 1] == '0')
                    continue;
                left ??= new Poi(x, y);
                right = new Poi(x, y);
            }
            if (left is { } l && right is { } r && l != r) {
                pois.Add(l);
                pois.Add(r);
            }
        }

        return pois;

        void EdgeRuns(bool horizontal, int edge, int inward) {
            var len = horizontal ? w : h;
            var runStart = -1;
            for (var k = 0; k <= len; k++) {
                var air = k < len && (horizontal ? grid[k, edge] : grid[edge, k]) == '0';
                if (air) {
                    if (runStart < 0)
                        runStart = k;
                    continue;
                }
                // Runs shorter than 2 tiles are too narrow to be a door.
                if (runStart >= 0 && k - runStart >= 2) {
                    var mid = (runStart + k - 1) / 2;
                    pois.Add(horizontal ? new Poi(mid, inward) : new Poi(inward, mid));
                }
                runStart = -1;
            }
        }
    }

    /// <summary>
    /// Budgeted flood fill from <paramref name="start"/>. Returns which air tiles the
    /// player could plausibly reach.
    /// </summary>
    public static bool[,] ComputeReachable(PcgGrid grid, Poi start) {
        var w = grid.Width;
        var h = grid.Height;
        var reach = new bool[w, h];
        if (grid[start.X, start.Y] != '0')
            return reach;

        var bestBudget = new int[w, h];
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            bestBudget[x, y] = -1;

        var q = new Queue<(int x, int y, int b)>();
        q.Enqueue((start.X, start.Y, AirBudget));
        bestBudget[start.X, start.Y] = AirBudget;
        reach[start.X, start.Y] = true;

        ReadOnlySpan<(int dx, int dy, int cost)> moves =
            [(-1, 0, 1), (1, 0, 1), (0, 1, 0), (0, -1, UpCost)];

        while (q.Count > 0) {
            var (x, y, b) = q.Dequeue();

            var standing = y + 1 < h && grid[x, y + 1] != '0';
            var climbing = (x > 0 && grid[x - 1, y] != '0') || (x + 1 < w && grid[x + 1, y] != '0');
            // Touching ground or a wall refills the budget.
            var b0 = standing || climbing ? AirBudget : b;

            foreach (var (dx, dy, cost) in moves) {
                var nx = x + dx;
                var ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || grid[nx, ny] != '0')
                    continue;
                var nb = b0 - cost;
                if (nb < 0 || nb <= bestBudget[nx, ny])
                    continue;
                bestBudget[nx, ny] = nb;
                reach[nx, ny] = true;
                q.Enqueue((nx, ny, nb));
            }
        }

        return reach;
    }

    /// <summary>
    /// Carves a corridor from the reachable region to <paramref name="goal"/>, choosing the
    /// route that breaks the fewest solid tiles (0-1 BFS: air costs 0, solid costs 1).
    /// Mutates <paramref name="grid"/>.
    /// </summary>
    public static void BridgeTo(PcgGrid grid, bool[,] reach, Poi goal, char solid) {
        var w = grid.Width;
        var h = grid.Height;

        var dist = new int[w, h];
        var parent = new Poi[w, h];
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++) {
            dist[x, y] = int.MaxValue;
            parent[x, y] = new Poi(-1, -1);
        }

        // Two FIFO queues with front taking priority, mirroring the Lua's front/back lists.
        // Not a strict deque, but the "nd < dist" relaxation makes the result correct either way.
        var front = new Queue<Poi>();
        var back = new Queue<Poi>();

        for (var x = 1; x < w - 1; x++)
        for (var y = 1; y < h - 1; y++) {
            if (!reach[x, y])
                continue;
            dist[x, y] = 0;
            back.Enqueue(new Poi(x, y));
        }
        if (back.Count == 0)
            return;

        ReadOnlySpan<(int dx, int dy)> dirs = [(-1, 0), (1, 0), (0, -1), (0, 1)];

        while (front.Count > 0 || back.Count > 0) {
            var cur = front.Count > 0 ? front.Dequeue() : back.Dequeue();
            foreach (var (dx, dy) in dirs) {
                var nx = cur.X + dx;
                var ny = cur.Y + dy;
                if (nx < 1 || ny < 1 || nx >= w - 1 || ny >= h - 1)
                    continue;
                var cost = grid[nx, ny] == '0' ? 0 : 1;
                var nd = dist[cur.X, cur.Y] + cost;
                if (nd >= dist[nx, ny])
                    continue;
                dist[nx, ny] = nd;
                parent[nx, ny] = cur;
                if (cost == 0)
                    front.Enqueue(new Poi(nx, ny));
                else
                    back.Enqueue(new Poi(nx, ny));
            }
        }

        if (dist[goal.X, goal.Y] == int.MaxValue)
            return;

        // Walk back from the goal to the reachable frontier.
        var path = new List<Poi>();
        var node = goal;
        while (node.X >= 0) {
            path.Add(node);
            if (dist[node.X, node.Y] == 0)
                break;
            node = parent[node.X, node.Y];
        }
        path.Reverse();

        var onPath = new HashSet<Poi>(path);

        // Carve the corridor two tiles tall so the player fits.
        foreach (var p in path) {
            if (grid[p.X, p.Y] != '0')
                grid[p.X, p.Y] = '0';
            if (p.Y - 1 >= 1 && grid[p.X, p.Y - 1] != '0' && !onPath.Contains(new Poi(p.X, p.Y - 1)))
                grid[p.X, p.Y - 1] = '0';
        }

        // Drop stepping stones under long unsupported stretches so the corridor is walkable.
        var sinceSupport = 0;
        foreach (var p in path) {
            var (px, py) = (p.X, p.Y);
            var supported =
                (py + 1 < h && grid[px, py + 1] != '0') ||
                (py + 2 < h && grid[px, py + 2] != '0') ||
                (px > 0 && grid[px - 1, py] != '0') ||
                (px + 1 < w && grid[px + 1, py] != '0');

            if (supported) {
                sinceSupport = 0;
                continue;
            }

            sinceSupport++;
            if (sinceSupport < MaxUnsupportedRun)
                continue;

            if (py + 2 <= h - 1 && !onPath.Contains(new Poi(px, py + 2)) && grid[px, py + 2] == '0')
                grid[px, py + 2] = solid;
            else if (py + 1 <= h - 1 && !onPath.Contains(new Poi(px, py + 1)) && grid[px, py + 1] == '0')
                grid[px, py + 1] = solid;
            sinceSupport = 0;
        }
    }

    /// <summary>
    /// Repairs the room until every point of interest is reachable from the first one.
    /// Returns whether the room ended up traversable. Mutates <paramref name="grid"/>.
    /// </summary>
    public static bool EnsurePlayable(PcgGrid grid, char solid, int maxRepairs = 3,
        Action<string>? log = null) {
        if (grid.Width < 5 || grid.Height < 5)
            return true;

        var pois = FindPois(grid);
        // Rooms with fewer than two doors have nothing to connect.
        if (pois.Count < 2)
            return true;

        Poi? start = null;
        foreach (var p in pois) {
            if (grid[p.X, p.Y] != '0')
                continue;
            start = p;
            break;
        }
        if (start is not { } from)
            return true;

        for (var attempt = 0; ; attempt++) {
            var reach = ComputeReachable(grid, from);
            var unreachable = pois.Where(p => !reach[p.X, p.Y]).ToList();

            if (unreachable.Count == 0) {
                if (attempt > 0)
                    log?.Invoke($"Repaired room traversal in {attempt} pass(es)");
                return true;
            }

            if (attempt >= maxRepairs) {
                log?.Invoke($"{unreachable.Count} exit(s) still unreachable after {maxRepairs} repair passes");
                return false;
            }

            foreach (var goal in unreachable)
                BridgeTo(grid, reach, goal, solid);
        }
    }
}
