namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// A tile distribution for one n-gram context. Entries are kept sorted by tile id so
/// sampling is reproducible.
///
/// DIVERGENCE FROM LUA: pcg.sample and generateMdmc's inner loop both walk the
/// distribution with pairs(), whose order Lua leaves unspecified, so the same seed can
/// produce different rooms across runs. Sorting makes generation actually deterministic.
/// </summary>
public sealed class TileDistribution {
    public required (char Tile, double P)[] Entries { get; init; }

    public int Count => Entries.Length;

    public static TileDistribution FromCounts(IReadOnlyDictionary<char, int> counts) {
        double total = counts.Values.Sum();
        return new TileDistribution {
            Entries = counts.OrderBy(kv => kv.Key).Select(kv => (kv.Key, kv.Value / total)).ToArray(),
        };
    }

    /// <summary>Weighted pick over the whole distribution.</summary>
    public char Sample(Random rng) {
        if (Entries.Length == 0)
            return '0';
        var r = rng.NextDouble();
        var cum = 0.0;
        var last = Entries[^1].Tile;
        foreach (var (tile, p) in Entries) {
            cum += p;
            if (r <= cum)
                return tile;
        }
        return last;
    }
}

/// <summary>
/// Multi-dimensional Markov Chain training and generation, ported from pcg_toolkit.lua
/// (paper section 3.3.2). Trains on existing rooms so output inherits their tile idiom,
/// then generates with tile-level backtracking and optional adjacency vetoes.
/// </summary>
public static class Mdmc {
    /// <summary>Below this many distinct n-grams the model is treated as too sparse to trust.</summary>
    public const int SparseModelThreshold = 10;

    /// <summary>Builds the context string for (x, y): neighbour tiles, '0' outside the grid.</summary>
    public static string BuildNgram(PcgGrid grid, int x, int y, IReadOnlyList<Offset> offsets) {
        return string.Create(offsets.Count, (grid, x, y, offsets), static (buf, s) => {
            for (var k = 0; k < s.offsets.Count; k++)
                buf[k] = s.grid.SafeGet(s.x + s.offsets[k].Dx, s.y + s.offsets[k].Dy);
        });
    }

    /// <summary>Counts n-gram context -> observed tile frequencies across the training rooms.</summary>
    public static Dictionary<string, Dictionary<char, int>> Train(
        IReadOnlyList<PcgGrid> grids, IReadOnlyList<Offset> offsets) {
        var counts = new Dictionary<string, Dictionary<char, int>>(StringComparer.Ordinal);

        foreach (var grid in grids)
        for (var y = 0; y < grid.Height; y++)
        for (var x = 0; x < grid.Width; x++) {
            var ngram = BuildNgram(grid, x, y, offsets);
            if (!counts.TryGetValue(ngram, out var tc))
                counts[ngram] = tc = new Dictionary<char, int>();
            tc[grid[x, y]] = tc.GetValueOrDefault(grid[x, y]) + 1;
        }

        return counts;
    }

    public static Dictionary<string, TileDistribution> Normalise(
        IReadOnlyDictionary<string, Dictionary<char, int>> counts) {
        var probs = new Dictionary<string, TileDistribution>(StringComparer.Ordinal);
        foreach (var (ngram, tc) in counts)
            probs[ngram] = TileDistribution.FromCounts(tc);
        return probs;
    }

    /// <summary>
    /// Seeds the model with all-air, all-solid and a few mixed contexts so sparse training
    /// data still has somewhere to go. Uses its own fixed-seed RNG, as the Lua does.
    /// </summary>
    public static void InjectSyntheticNGrams(Dictionary<string, Dictionary<char, int>> counts,
        char dominantTile, IReadOnlyList<Offset> offsets, Action<string>? log = null) {
        var contextCount = offsets.Count;
        if (contextCount == 0)
            return;

        var airNgram = new string('0', contextCount);
        var solidNgram = new string(dominantTile, contextCount);

        counts.TryAdd(airNgram, new Dictionary<char, int> { ['0'] = 7, [dominantTile] = 3 });
        counts.TryAdd(solidNgram, new Dictionary<char, int> { ['0'] = 2, [dominantTile] = 8 });

        var rng = new Random(42);
        var limit = (int) Math.Min(8, Math.Pow(2, contextCount));
        for (var i = 0; i < limit; i++) {
            var buf = new char[contextCount];
            for (var j = 0; j < contextCount; j++)
                buf[j] = rng.NextDouble() > 0.5 ? '0' : dominantTile;
            counts.TryAdd(new string(buf), new Dictionary<char, int> {
                ['0'] = rng.NextRangeInclusive(3, 6),
                [dominantTile] = rng.NextRangeInclusive(3, 6),
            });
        }

        log?.Invoke($"Injected synthetic n-grams (now have {counts.Count} total)");
    }

    public readonly record struct Result(PcgGrid Grid, int DegenerateCells);

    /// <summary>
    /// Generates a room, scanning bottom-to-top and right-to-left with tile-level
    /// backtracking. <see cref="Result.DegenerateCells"/> counts cells the model had no
    /// answer for and that were filled randomly -- callers should reject rooms where that
    /// number is high, since the output is then mostly noise rather than learned structure.
    /// </summary>
    public static Result Generate(IReadOnlyDictionary<string, TileDistribution> probs,
        IReadOnlyList<Offset> offsets, int w, int h, int maxBacktrackDepth, Random rng,
        char defaultTile = '3', AdjacencyModel? adjacency = null, Action<string>? log = null) {

        // Every tile the model knows, plus air and the default, in deterministic order.
        var seen = new SortedSet<char>();
        foreach (var dist in probs.Values)
        foreach (var (tile, _) in dist.Entries)
            seen.Add(tile);
        seen.Add('0');
        seen.Add(defaultTile);
        var allTiles = seen.ToArray();

        var grid = new PcgGrid(w, h);

        var order = new List<Poi>(w * h);
        for (var y = h - 1; y >= 0; y--)
        for (var x = w - 1; x >= 0; x--)
            order.Add(new Poi(x, y));

        var tried = new Dictionary<int, HashSet<char>>();
        var useFallback = probs.Count < SparseModelThreshold;
        if (useFallback)
            log?.Invoke("Using fallback distribution for unseen n-grams (sparse training data)");

        // Lua: { ["0"] = 0.5, [defaultTile] = 0.5 } -- collapses to one key if they match.
        var fallbackDist = TileDistribution.FromCounts(
            new Dictionary<char, int> { ['0'] = 1, [defaultTile] = 1 });

        var pos = 0;
        var backtrackCount = 0;
        var maxSteps = Math.Max(order.Count * 8, 4096);
        var steps = 0;
        var degenerate = 0;

        while (pos < order.Count) {
            if (++steps > maxSteps) {
                // Out of step budget: finish greedily so we always return a full room.
                for (var p = pos; p < order.Count; p++) {
                    var cell = order[p];
                    var d = probs.GetValueOrDefault(BuildNgram(grid, cell.X, cell.Y, offsets));
                    if (d is { Count: > 0 }) {
                        grid[cell.X, cell.Y] = d.Sample(rng);
                    } else {
                        grid[cell.X, cell.Y] = allTiles[rng.Next(allTiles.Length)];
                        degenerate++;
                    }
                }
                log?.Invoke("Backtracking exceeded step budget - finished room with a greedy pass");
                break;
            }

            var (x, y) = (order[pos].X, order[pos].Y);
            var posKey = y * w + x;
            if (!tried.TryGetValue(posKey, out var triedSet))
                tried[posKey] = triedSet = [];

            var dist = probs.GetValueOrDefault(BuildNgram(grid, x, y, offsets));
            if (dist is null && useFallback)
                dist = fallbackDist;

            var advanced = false;
            if (dist is not null) {
                var totalWeight = dist.Entries
                    .Where(e => !triedSet.Contains(e.Tile))
                    .Sum(e => e.P);

                if (totalWeight > 0) {
                    var r = rng.NextDouble() * totalWeight;
                    var cum = 0.0;
                    char? chosen = null;
                    char? lastUntried = null;
                    foreach (var (tile, p) in dist.Entries) {
                        if (triedSet.Contains(tile))
                            continue;
                        lastUntried = tile;
                        cum += p;
                        if (r <= cum) {
                            chosen = tile;
                            break;
                        }
                    }
                    chosen ??= lastUntried;

                    if (chosen is { } pick) {
                        triedSet.Add(pick);
                        // Scan order means anything below, or right on the same row, is placed.
                        var consistent = adjacency is null || adjacency.IsLocallyConsistent(
                            grid, x, y, pick, (nx, ny) => ny > y || (ny == y && nx > x));
                        if (consistent) {
                            grid[x, y] = pick;
                            pos++;
                            backtrackCount = 0;
                            advanced = true;
                        }
                    }
                }
            }

            if (advanced)
                continue;

            // Every option at this cell is exhausted or vetoed: step back, or give up and
            // place something random if we are at the start or too deep already.
            tried.Remove(posKey);
            grid[x, y] = '0';
            if (pos == 0 || backtrackCount >= maxBacktrackDepth) {
                grid[x, y] = allTiles[rng.Next(allTiles.Length)];
                degenerate++;
                pos++;
                backtrackCount = 0;
            } else {
                backtrackCount++;
                pos--;
            }
        }

        return new Result(grid, degenerate);
    }
}
