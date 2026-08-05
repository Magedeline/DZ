using System.Numerics;

namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Overlapping-model Wave Function Collapse, in the style of Gumin's original.
///
/// This is an addition, not a port. PCGHelper's <see cref="Wfc"/> learns adjacency between
/// individual tiles, which carries no information once the alphabet is small: with air plus
/// one tileset all four ordered pairs occur in any real room, the constraint relation is
/// fully permissive, and collapse degenerates to independent weighted sampling.
///
/// Here the alphabet is instead every distinct NxN block observed in the training rooms.
/// Two patterns may sit at a given offset only if their overlapping cells agree, so local
/// structure is encoded in the alphabet itself and survives to the output.
/// </summary>
public static class PatternWfc {
    /// <summary>A distinct NxN block seen in the training data, with how often it occurred.</summary>
    internal sealed class Pattern {
        public required char[] Cells { get; init; }   // row-major, length N*N
        public required int Size { get; init; }
        public int Weight;

        public char At(int dx, int dy) => Cells[dy * Size + dx];

        /// <summary>Whether this pattern agrees with <paramref name="other"/> placed at (dx, dy).</summary>
        public bool Agrees(Pattern other, int dx, int dy) {
            var xMin = Math.Max(0, dx);
            var xMax = Math.Min(Size, Size + dx);
            var yMin = Math.Max(0, dy);
            var yMax = Math.Min(Size, Size + dy);
            for (var y = yMin; y < yMax; y++)
            for (var x = xMin; x < xMax; x++) {
                if (At(x, y) != other.At(x - dx, y - dy))
                    return false;
            }
            return true;
        }
    }

    public sealed class Model {
        internal Pattern[] Patterns { get; }
        internal double[] Weights { get; }
        /// <summary>Compatible[d][p] = bitset of patterns allowed at direction d from p.</summary>
        internal ulong[][][] Compatible { get; }
        internal (int Dx, int Dy)[] Dirs { get; }
        internal int Words { get; }

        internal Model(Pattern[] patterns, double[] weights, ulong[][][] compatible,
            (int Dx, int Dy)[] dirs, int words) {
            Patterns = patterns;
            Weights = weights;
            Compatible = compatible;
            Dirs = dirs;
            Words = words;
        }

        public int PatternCount => Patterns.Length;
        public int PatternSize => Patterns.Length == 0 ? 0 : Patterns[0].Size;

        /// <summary>
        /// Fraction of (direction, pattern, pattern) triples that are compatible. Low values
        /// mean the constraints actually bite -- contrast <see cref="Wfc"/>, where a binary
        /// alphabet yields 100%.
        /// </summary>
        public double Permissiveness {
            get {
                if (Patterns.Length == 0)
                    return 1;
                long ok = 0;
                for (var d = 0; d < Dirs.Length; d++)
                for (var p = 0; p < Patterns.Length; p++)
                foreach (var word in Compatible[d][p])
                    ok += BitOperations.PopCount(word);
                return ok / (double) (Dirs.Length * (long) Patterns.Length * Patterns.Length);
            }
        }
    }

    /// <summary>
    /// Extracts every distinct NxN block from the training rooms.
    /// <paramref name="periodic"/> wraps sampling at the edges, which yields more patterns
    /// from small inputs at the cost of blending opposite borders.
    /// </summary>
    public static Model? Train(IReadOnlyList<PcgGrid> grids, int patternSize = 3, bool periodic = false) {
        if (patternSize < 2)
            patternSize = 2;

        var byKey = new Dictionary<string, Pattern>(StringComparer.Ordinal);

        foreach (var g in grids) {
            var maxX = periodic ? g.Width : g.Width - patternSize + 1;
            var maxY = periodic ? g.Height : g.Height - patternSize + 1;
            for (var oy = 0; oy < maxY; oy++)
            for (var ox = 0; ox < maxX; ox++) {
                var cells = new char[patternSize * patternSize];
                for (var y = 0; y < patternSize; y++)
                for (var x = 0; x < patternSize; x++) {
                    var sx = periodic ? (ox + x) % g.Width : ox + x;
                    var sy = periodic ? (oy + y) % g.Height : oy + y;
                    cells[y * patternSize + x] = g[sx, sy];
                }

                var key = new string(cells);
                if (byKey.TryGetValue(key, out var existing)) {
                    existing.Weight++;
                } else {
                    byKey[key] = new Pattern { Cells = cells, Size = patternSize, Weight = 1 };
                }
            }
        }

        if (byKey.Count < 2)
            return null;

        // Deterministic ordering so a seed reproduces a run.
        var patterns = byKey.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value).ToArray();
        var n = patterns.Length;
        var words = (n + 63) / 64;

        // Overlap offsets: every shift that leaves the two patterns sharing at least one cell.
        var dirs = new List<(int Dx, int Dy)>();
        for (var dy = -(patternSize - 1); dy <= patternSize - 1; dy++)
        for (var dx = -(patternSize - 1); dx <= patternSize - 1; dx++) {
            if (dx != 0 || dy != 0)
                dirs.Add((dx, dy));
        }

        var compatible = new ulong[dirs.Count][][];
        for (var d = 0; d < dirs.Count; d++) {
            compatible[d] = new ulong[n][];
            var (dx, dy) = dirs[d];
            for (var p = 0; p < n; p++) {
                var row = new ulong[words];
                for (var q = 0; q < n; q++) {
                    if (patterns[p].Agrees(patterns[q], dx, dy))
                        row[q >> 6] |= 1UL << (q & 63);
                }
                compatible[d][p] = row;
            }
        }

        double total = patterns.Sum(p => (double) p.Weight);
        var weights = patterns.Select(p => p.Weight / total).ToArray();

        return new Model(patterns, weights, compatible, dirs.ToArray(), words);
    }

    /// <summary>Generates a room, or null if propagation hit a contradiction.</summary>
    public static PcgGrid? Generate(Model model, int w, int h, Random rng) {
        var n = model.PatternCount;
        if (n == 0)
            return null;

        var words = model.Words;
        var cells = w * h;

        // wave[cell] = bitset over patterns still possible at that cell.
        var wave = new ulong[cells * words];
        for (var i = 0; i < cells; i++)
        for (var q = 0; q < n; q++)
            wave[i * words + (q >> 6)] |= 1UL << (q & 63);

        var queue = new Queue<int>();
        var inQueue = new bool[cells];

        void Enqueue(int idx) {
            if (inQueue[idx])
                return;
            inQueue[idx] = true;
            queue.Enqueue(idx);
        }

        int Popcount(int cell) {
            var c = 0;
            for (var q = 0; q < words; q++)
                c += BitOperations.PopCount(wave[cell * words + q]);
            return c;
        }

        while (true) {
            var bestIdx = -1;
            var bestEntropy = double.PositiveInfinity;

            for (var i = 0; i < cells; i++) {
                var count = Popcount(i);
                if (count == 0)
                    return null;
                if (count == 1)
                    continue;

                double sumW = 0, sumWLog = 0;
                for (var q = 0; q < n; q++) {
                    if ((wave[i * words + (q >> 6)] & (1UL << (q & 63))) == 0)
                        continue;
                    var wt = model.Weights[q];
                    sumW += wt;
                    sumWLog += wt * Math.Log(wt);
                }

                var entropy = Math.Log(sumW) - sumWLog / sumW + rng.NextDouble() * 1e-6;
                if (entropy < bestEntropy) {
                    bestEntropy = entropy;
                    bestIdx = i;
                }
            }

            if (bestIdx < 0)
                break;

            // Collapse, weighted by observed frequency.
            double totalW = 0;
            for (var q = 0; q < n; q++) {
                if ((wave[bestIdx * words + (q >> 6)] & (1UL << (q & 63))) != 0)
                    totalW += model.Weights[q];
            }

            var roll = rng.NextDouble() * totalW;
            double cum = 0;
            var chosen = -1;
            for (var q = 0; q < n; q++) {
                if ((wave[bestIdx * words + (q >> 6)] & (1UL << (q & 63))) == 0)
                    continue;
                chosen = q;
                cum += model.Weights[q];
                if (roll <= cum)
                    break;
            }
            if (chosen < 0)
                return null;

            for (var q = 0; q < words; q++)
                wave[bestIdx * words + q] = 0;
            wave[bestIdx * words + (chosen >> 6)] = 1UL << (chosen & 63);

            Enqueue(bestIdx);
            if (!Propagate())
                return null;
        }

        // Read out: each cell takes the top-left cell of its surviving pattern.
        var grid = new PcgGrid(w, h);
        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++) {
            var cell = y * w + x;
            var pick = -1;
            for (var q = 0; q < n && pick < 0; q++) {
                if ((wave[cell * words + (q >> 6)] & (1UL << (q & 63))) != 0)
                    pick = q;
            }
            grid[x, y] = pick < 0 ? '0' : model.Patterns[pick].At(0, 0);
        }
        return grid;

        bool Propagate() {
            // Allocated once: this sits inside two loops, and stackalloc per iteration
            // would grow the frame without bound.
            var allow = new ulong[words];

            while (queue.Count > 0) {
                var idx = queue.Dequeue();
                inQueue[idx] = false;

                var cx = idx % w;
                var cy = idx / w;

                for (var d = 0; d < model.Dirs.Length; d++) {
                    var (dx, dy) = model.Dirs[d];
                    var nx = cx + dx;
                    var ny = cy + dy;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        continue;

                    // Union of what every pattern still possible here permits at this offset.
                    Array.Clear(allow);
                    for (var q = 0; q < n; q++) {
                        if ((wave[idx * words + (q >> 6)] & (1UL << (q & 63))) == 0)
                            continue;
                        var row = model.Compatible[d][q];
                        for (var word = 0; word < words; word++)
                            allow[word] |= row[word];
                    }

                    var nIdx = ny * w + nx;
                    var changed = false;
                    var empty = true;
                    for (var word = 0; word < words; word++) {
                        var before = wave[nIdx * words + word];
                        var after = before & allow[word];
                        if (after != before) {
                            wave[nIdx * words + word] = after;
                            changed = true;
                        }
                        if (after != 0)
                            empty = false;
                    }

                    if (!changed)
                        continue;
                    if (empty)
                        return false;
                    Enqueue(nIdx);
                }
            }
            return true;
        }
    }

    /// <summary>Retries <see cref="Generate"/> until it succeeds or the budget runs out.</summary>
    public static PcgGrid? GenerateWithRetries(Model model, int w, int h, Random rng,
        int maxAttempts = 10, Action<string>? log = null) {
        for (var attempt = 1; attempt <= maxAttempts; attempt++) {
            if (Generate(model, w, h, rng) is { } grid) {
                if (attempt > 1)
                    log?.Invoke($"Pattern WFC succeeded on attempt {attempt}");
                return grid;
            }
        }
        log?.Invoke($"Pattern WFC hit a contradiction on all {maxAttempts} attempts");
        return null;
    }
}
