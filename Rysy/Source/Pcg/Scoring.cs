namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Per-room metrics from pcg_toolkit.lua (paper sections 4.1-4.3). Interestingness and
/// difficulty are weighted sums over the component terms, so callers can retune without
/// rescoring.
/// </summary>
public sealed record RoomMetrics {
    public required int ExitCount { get; init; }
    public required int SampledPairs { get; init; }
    public required int ConnectedPairs { get; init; }
    public required IReadOnlyList<int> PathLengths { get; init; }
    public required double PathMean { get; init; }
    public required double PathVariance { get; init; }

    /// <summary>Accent-tile density over the whole interior.</summary>
    public required double GlobalNleDensity { get; init; }

    /// <summary>Accent-tile density within the area of interest.</summary>
    public required double LocalNleDensity { get; init; }

    /// <summary>Shannon entropy of the tile mix, normalised to [0, 1].</summary>
    public required double ShannonDiversity { get; init; }

    /// <summary>Floor gaps per unit of path length.</summary>
    public required double HoleFrequency { get; init; }

    /// <summary>Exposed dominant-tile surfaces within the area of interest.</summary>
    public required double LocalLeDensity { get; init; }

    /// <summary>Inverse local accent density, capped.</summary>
    public required double Scarcity { get; init; }

    public double Interestingness(double w1 = 1, double w2 = 1, double w3 = 1)
        => w1 * GlobalNleDensity + w2 * LocalNleDensity + w3 * ShannonDiversity;

    public double Difficulty(double z1 = 1, double z2 = 1, double z3 = 1)
        => z1 * HoleFrequency + z2 * LocalLeDensity + z3 * Scarcity;

    /// <summary>
    /// Rejects rooms whose path lengths vary wildly, which usually means the exits are
    /// connected only through a degenerate detour.
    /// </summary>
    public bool PassesVarianceCheck() {
        if (PathLengths.Count < 2)
            return true;
        var sorted = PathLengths.Order().ToArray();
        var n = sorted.Length;
        var median = n % 2 == 1 ? sorted[n / 2] : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        return PathVariance <= 2 * median;
    }
}

public static class Scoring {
    /// <summary>
    /// Cap on 1/localDensity. The Lua notes the paper's own instability here: without a cap
    /// a sparse room drives difficulty to a sentinel value and swamps every other term.
    /// </summary>
    public const double ScarcityCap = 20;

    /// <summary>How far the area of interest extends around each path cell (paper AOI_n, n=2).</summary>
    public const int AoiRadius = 2;

    /// <summary>
    /// Scores a room. Paths run exit-to-exit through real border openings so vertical and
    /// multi-exit rooms score correctly; rooms with fewer than two exits fall back to
    /// horizontal sweeps. The area of interest is the union of sampled paths dilated by
    /// <see cref="AoiRadius"/> -- the space the player actually occupies -- rather than a
    /// fixed centre box.
    /// </summary>
    public static RoomMetrics Score(PcgGrid grid, char dominant, int numPaths, Random rng) {
        var w = grid.Width;
        var h = grid.Height;

        var exits = Connectivity.FindExits(grid);
        var pathLens = new List<int>();
        var pathCells = new HashSet<int>();
        var sampledPairs = 0;
        var connectedPairs = 0;

        if (exits.Count >= 2) {
            var exitPairs = new List<(int A, int B)>();
            for (var i = 0; i < exits.Count; i++)
            for (var j = i + 1; j < exits.Count; j++)
                exitPairs.Add((i, j));

            // Fisher-Yates, matching the Lua's downward sweep.
            for (var i = exitPairs.Count - 1; i >= 1; i--) {
                var j = rng.Next(i + 1);
                (exitPairs[i], exitPairs[j]) = (exitPairs[j], exitPairs[i]);
            }

            sampledPairs = Math.Min(exitPairs.Count, Math.Max(1, numPaths));
            for (var k = 0; k < sampledPairs; k++) {
                var (a, b) = exitPairs[k];
                if (Connectivity.BfsExitToExitWithPath(grid, exits[a].Cells, exits[b].Cells) is not { } hit)
                    continue;
                connectedPairs++;
                pathLens.Add(hit.Dist);
                foreach (var c in hit.Path)
                    pathCells.Add(c.Y * w + c.X);
            }
        } else {
            for (var p = 0; p < numPaths; p++) {
                var len = Connectivity.BfsPathLength(grid, rng);
                if (len >= 0)
                    pathLens.Add(len);
            }
        }

        // Area of interest: dilated paths, or a centre-third box when no path was found.
        var aoi = new HashSet<int>();
        if (pathCells.Count > 0) {
            foreach (var k in pathCells) {
                var cx = k % w;
                var cy = k / w;
                for (var dx = -AoiRadius; dx <= AoiRadius; dx++)
                for (var dy = -AoiRadius; dy <= AoiRadius; dy++) {
                    var nx = cx + dx;
                    var ny = cy + dy;
                    if (nx >= 1 && ny >= 1 && nx < w - 1 && ny < h - 1)
                        aoi.Add(ny * w + nx);
                }
            }
        } else {
            for (var x = w / 3; x < w * 2 / 3; x++)
            for (var y = h / 3; y < h * 2 / 3; y++)
                aoi.Add(y * w + x);
        }
        var aoiArea = Math.Max(1, aoi.Count);

        var area = Math.Max(1, (w - 2) * (h - 2));
        int nleTotal = 0, nleInAoi = 0, leInAoi = 0, holeCols = 0;
        var tileFreq = new Dictionary<char, int>();

        for (var x = 1; x < w - 1; x++) {
            if (grid[x, h - 2] == '0')
                holeCols++;
            for (var y = 1; y < h - 1; y++) {
                var t = grid[x, y];
                if (t == '0')
                    continue;

                tileFreq[t] = tileFreq.GetValueOrDefault(t) + 1;
                var inAoi = aoi.Contains(y * w + x);

                if (t != dominant) {
                    nleTotal++;
                    if (inAoi)
                        nleInAoi++;
                } else if (grid[x, y - 1] == '0' && inAoi) {
                    // Dominant tile with air above: an exposed walkable surface.
                    leInAoi++;
                }
            }
        }

        double pathMean = 0, pathVar = 0;
        if (pathLens.Count > 0) {
            pathMean = pathLens.Average();
            if (pathLens.Count > 1)
                pathVar = pathLens.Sum(l => (l - pathMean) * (l - pathMean)) / pathLens.Count;
        }

        // Normalising by log(tileTypes) keeps diversity in [0,1] so it cannot reward noise.
        var totalTiles = tileFreq.Values.Sum();
        var entropy = 0.0;
        if (totalTiles > 0 && tileFreq.Count > 1) {
            foreach (var cnt in tileFreq.Values) {
                var pi = cnt / (double) totalTiles;
                if (pi > 0)
                    entropy -= pi * Math.Log(pi);
            }
            entropy /= Math.Log(tileFreq.Count);
        }

        var dLocal = nleInAoi / (double) aoiArea;

        return new RoomMetrics {
            ExitCount = exits.Count,
            SampledPairs = sampledPairs,
            ConnectedPairs = connectedPairs,
            PathLengths = pathLens,
            PathMean = pathMean,
            PathVariance = pathVar,
            GlobalNleDensity = nleTotal / (double) area,
            LocalNleDensity = dLocal,
            ShannonDiversity = entropy,
            // Holes per unit path; without a path, normalise by interior width so the
            // term stays a bounded ratio rather than exploding.
            HoleFrequency = holeCols / (pathMean > 0 ? pathMean : Math.Max(w - 2, 1)),
            LocalLeDensity = leInAoi / (double) aoiArea,
            Scarcity = dLocal > 0 ? Math.Min(1 / dLocal, ScarcityCap) : ScarcityCap,
        };
    }
}
