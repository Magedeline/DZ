namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Port of PCGHelper's pcg_unity_generators.lua -- the Unity tilemap procedural
/// generation patterns, adapted to Rysy's tile grids.
///
/// The Lua RNG documents itself as mirroring System.Random, so the calls map directly:
///   rng:nextDouble()      -> Random.NextDouble()     [0, 1)
///   rng:next(hi)          -> Random.Next(hi)         [0, hi)
///   rng:nextRange(lo, hi) -> Random.Next(lo, hi + 1) [lo, hi] inclusive
/// The number sequences still differ (Lua uses an LCG, .NET does not), so a given seed
/// does not reproduce the same map across the two implementations.
///
/// Row 0 is the TOP of the room. The "top layer" generators fill downward from row 0,
/// which is inherited from Unity's y-up convention and therefore produces a ceiling
/// rather than a floor -- see <see cref="TopLayerFillsFromTop"/>.
/// </summary>
public static class UnityGenerators {
    /// <summary>
    /// True because PerlinTopLayer / SimplexTopLayer / SmoothedRandomWalkTop fill rows
    /// 0..height, and row 0 is the top of a Celeste room. The Lua comments describe this
    /// as filling "everything below" the profile, which holds under Unity's y-up axis but
    /// not after the grid is written to a Celeste room. Callers wanting ground rather than
    /// ceiling should flip the result (PcgGrid.FlipVertical).
    /// </summary>
    public const bool TopLayerFillsFromTop = true;

    private const double TopLayerScale = 0.05;

    /// <summary>Perlin height profile; fills rows 0..height for each column.</summary>
    public static PcgGrid PerlinTopLayer(int w, int h, Random rng, char solid = '1',
        double? seed = null, double reduction = 0.5) {
        var s = seed ?? rng.NextDouble() * 1000;
        var grid = new PcgGrid(w, h);
        for (var x = 0; x < w; x++) {
            var n = Noise.Perlin2D(x * TopLayerScale, s);
            var height = ClampHeight((int) Math.Floor((n - reduction) * h) + h / 2, h);
            for (var y = 0; y <= height; y++)
                grid[x, y] = solid;
        }
        return grid;
    }

    /// <summary>Simplex drop-in replacement for <see cref="PerlinTopLayer"/>.</summary>
    public static PcgGrid SimplexTopLayer(int w, int h, Random rng, char solid = '1',
        double? seed = null, double reduction = 0.5) {
        var s = seed ?? rng.NextDouble() * 1000;
        var grid = new PcgGrid(w, h);
        for (var x = 0; x < w; x++) {
            var n = Noise.Simplex2D(x * TopLayerScale, s);
            var height = ClampHeight((int) Math.Floor((n - reduction) * h) + h / 2, h);
            for (var y = 0; y <= height; y++)
                grid[x, y] = solid;
        }
        return grid;
    }

    /// <summary>2D Perlin thresholded at 0.5, with optional solid border.</summary>
    public static PcgGrid PerlinCave(int w, int h, Random rng, char solid = '1',
        double modifier = 0.1, bool edgesAreWalls = true) {
        var seed = rng.NextDouble() * 1000;
        var grid = new PcgGrid(w, h);
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++) {
            if (edgesAreWalls && IsEdge(x, y, w, h)) {
                grid[x, y] = solid;
                continue;
            }
            grid[x, y] = Noise.Perlin2D(x * modifier, y * modifier, seed) >= 0.5 ? solid : '0';
        }
        return grid;
    }

    /// <summary>2D simplex thresholded at 0.5; drop-in replacement for <see cref="PerlinCave"/>.</summary>
    public static PcgGrid SimplexCave(int w, int h, Random rng, char solid = '1',
        double modifier = 0.1, bool edgesAreWalls = true) {
        var seedX = rng.NextDouble() * 1000;
        var seedY = rng.NextDouble() * 1000;
        var grid = new PcgGrid(w, h);
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++) {
            if (edgesAreWalls && IsEdge(x, y, w, h)) {
                grid[x, y] = solid;
                continue;
            }
            grid[x, y] = Noise.Simplex2D(x * modifier + seedX, y * modifier + seedY) >= 0.5 ? solid : '0';
        }
        return grid;
    }

    /// <summary>Layered simplex octaves before thresholding, for less uniform texture.</summary>
    public static PcgGrid SimplexFbmCave(int w, int h, Random rng, char solid = '1',
        double modifier = 0.1, int octaves = 4, double persistence = 0.5, bool edgesAreWalls = true) {
        octaves = Math.Max(1, octaves);
        var seedX = rng.NextDouble() * 1000;
        var seedY = rng.NextDouble() * 1000;
        var grid = new PcgGrid(w, h);
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++) {
            if (edgesAreWalls && IsEdge(x, y, w, h)) {
                grid[x, y] = solid;
                continue;
            }
            var n = Noise.SimplexFbm2D(x * modifier + seedX, y * modifier + seedY, octaves, persistence, 2.0);
            grid[x, y] = n >= 0.5 ? solid : '0';
        }
        return grid;
    }

    /// <summary>Drunkard's walk carved through a solid grid until enough floor is open.</summary>
    public static PcgGrid RandomWalkCave(int w, int h, Random rng, char solid = '1',
        int requiredFloorPercent = 45) {
        var grid = new PcgGrid(w, h, solid);
        var x = rng.Next(1, w - 1);   // Lua nextRange(1, w - 2) is inclusive
        var y = rng.Next(1, h - 1);
        grid[x, y] = '0';

        var floorCount = 1;
        var reqFloor = w * h * requiredFloorPercent / 100;
        // The walk clamps to [1, w-2] x [1, h-2], so it can never open more than that.
        // Without this cap a high requiredFloorPercent would spin forever.
        var openable = Math.Max(0, w - 2) * Math.Max(0, h - 2);
        reqFloor = Math.Min(reqFloor, openable);

        ReadOnlySpan<(int dx, int dy)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
        while (floorCount < reqFloor) {
            var d = dirs[rng.Next(4)];
            x = Math.Clamp(x + d.dx, 1, w - 2);
            y = Math.Clamp(y + d.dy, 1, h - 2);
            if (grid[x, y] == '0')
                continue;
            grid[x, y] = '0';
            floorCount++;
        }
        return grid;
    }

    /// <summary>Vertical tunnel carved top to bottom with drifting width and position.</summary>
    public static PcgGrid DirectionalTunnel(int w, int h, Random rng, char solid = '1',
        int minPathWidth = 1, int maxPathWidth = 3, int maxPathChange = 1,
        int roughness = 1, int curvyness = 1) {
        var grid = new PcgGrid(w, h, solid);

        var tunnelWidth = minPathWidth + rng.Next(maxPathWidth - minPathWidth + 1);
        var x = w / 2;
        var y = 0;
        Carve(grid, x, y, tunnelWidth, w, h);

        while (y < h - 1) {
            if (rng.Next(100) < roughness)
                tunnelWidth = Math.Clamp(tunnelWidth + rng.Next(-1, 2), minPathWidth, maxPathWidth);
            if (rng.Next(100) < curvyness) {
                var lo = tunnelWidth + 1;
                var hi = w - tunnelWidth - 2;
                // On narrow rooms the clamp bounds can invert; Math.Clamp throws on that.
                x = lo > hi ? w / 2 : Math.Clamp(x + rng.Next(-maxPathChange, maxPathChange + 1), lo, hi);
            }
            y++;
            Carve(grid, x, y, tunnelWidth, w, h);
        }
        return grid;

        static void Carve(PcgGrid g, int cx, int cy, int width, int w, int h) {
            for (var i = -width; i <= width; i++)
                if (cx + i >= 0 && cx + i < w && cy < h)
                    g[cx + i, cy] = '0';
        }
    }

    /// <summary>Random seed smoothed by birth/survival rules; border stays solid.</summary>
    public static PcgGrid CellularAutomata(int w, int h, Random rng, char solid = '1',
        int birthLimit = 4, int deathLimit = 3, int passes = 4, double initialDensity = 0.45) {
        var grid = new PcgGrid(w, h);
        for (var x = 1; x < w - 1; x++)
        for (var y = 1; y < h - 1; y++)
            grid[x, y] = rng.NextDouble() < initialDensity ? solid : '0';
        grid.FillBorder(solid);

        for (var pass = 0; pass < passes; pass++) {
            var next = new PcgGrid(w, h);
            for (var x = 1; x < w - 1; x++)
            for (var y = 1; y < h - 1; y++) {
                var neighbours = 0;
                for (var dx = -1; dx <= 1; dx++)
                for (var dy = -1; dy <= 1; dy++) {
                    if (dx == 0 && dy == 0)
                        continue;
                    if (grid.SafeGet(x + dx, y + dy) == solid)
                        neighbours++;
                }
                next[x, y] = grid[x, y] == solid
                    ? neighbours < deathLimit ? '0' : solid
                    : neighbours > birthLimit ? solid : '0';
            }
            next.FillBorder(solid);
            grid = next;
        }
        return grid;
    }

    /// <summary>Perlin height sampled every <paramref name="interval"/> columns, linearly interpolated between samples.</summary>
    public static PcgGrid SmoothedRandomWalkTop(int w, int h, Random rng, char solid = '1', int interval = 5) {
        interval = Math.Max(2, interval);
        var seed = rng.NextDouble() * 1000;

        var xs = new List<int>();
        var ys = new List<int>();
        for (var x = 0; x < w; x += interval) {
            var n = Noise.Perlin2D(x * TopLayerScale, seed);
            ys.Add((int) Math.Floor((n - 0.5) * h));
            xs.Add(x);
        }
        if (ys.Count == 0) {
            ys.Add(h / 2);
            xs.Add(0);
        }

        var grid = new PcgGrid(w, h);
        for (var i = 1; i < ys.Count; i++) {
            var lastX = xs[i - 1];
            var lastY = ys[i - 1] + h / 2;
            var curX = xs[i];
            var curY = ys[i] + h / 2;
            for (var x = lastX; x <= curX && x < w; x++) {
                var t = curX - lastX == 0 ? 0 : (double) (x - lastX) / (curX - lastX);
                var height = ClampHeight((int) Math.Floor(lastY + t * (curY - lastY)), h);
                for (var y = 0; y <= height; y++)
                    grid[x, y] = solid;
            }
        }
        return grid;
    }

    private static bool IsEdge(int x, int y, int w, int h)
        => x == 0 || y == 0 || x == w - 1 || y == h - 1;

    private static int ClampHeight(int height, int h) => Math.Clamp(height, 0, h - 1);
}
