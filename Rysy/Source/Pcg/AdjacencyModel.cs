namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Which tiles were ever observed adjacent to which, per direction. Used to veto MdMC
/// choices that would create a tile pairing the training data never contained, and as the
/// constraint set for WFC. Ported from pcg_toolkit.lua's trainAdjacency.
/// </summary>
public sealed class AdjacencyModel {
    /// <summary>Tile alphabet, most frequent first, capped at 64.</summary>
    public required char[] Alphabet { get; init; }

    /// <summary>Normalised frequency of each alphabet entry, parallel to <see cref="Alphabet"/>.</summary>
    public required double[] Weights { get; init; }

    /// <summary>Constraint directions: the offsets plus their mirrors.</summary>
    public required Offset[] Dirs { get; init; }

    /// <summary>Allowed[d][i] contains every alphabet index seen at direction d from tile i.</summary>
    public required HashSet<int>[][] Allowed { get; init; }

    /// <summary>
    /// <see cref="Allowed"/> as bitmasks. The alphabet is capped at 64 entries, so a whole
    /// tile domain fits in one ulong and WFC's constraint propagation becomes bitwise AND/OR.
    /// </summary>
    public required ulong[][] AllowedMask { get; init; }

    public required Dictionary<char, int> Index { get; init; }

    public int N => Alphabet.Length;

    /// <summary>Bitmask with every alphabet entry set.</summary>
    public ulong FullDomain => N >= 64 ? ulong.MaxValue : (1UL << N) - 1;

    /// <summary>
    /// Builds the model, or returns null when the training data has fewer than two
    /// distinct tiles and constraints would be meaningless.
    /// </summary>
    public static AdjacencyModel? Train(IReadOnlyList<PcgGrid> grids, IReadOnlyList<Offset> offsets) {
        var freq = new Dictionary<char, int>();
        foreach (var g in grids)
        for (var x = 0; x < g.Width; x++)
        for (var y = 0; y < g.Height; y++)
            freq[g[x, y]] = freq.GetValueOrDefault(g[x, y]) + 1;

        freq['0'] = Math.Max(freq.GetValueOrDefault('0'), 1);
        if (freq.Count < 2)
            return null;

        // Frequency descending. Ties broken by tile id -- the Lua leaves them to table.sort
        // over a pairs() ordering, which is not reproducible.
        var alphabet = freq
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(64)
            .Select(kv => kv.Key)
            .ToArray();

        var index = new Dictionary<char, int>();
        for (var i = 0; i < alphabet.Length; i++)
            index[alphabet[i]] = i;

        // Symmetric closure: a context offset constrains in both directions.
        var dirSet = new HashSet<Offset>();
        foreach (var o in offsets) {
            dirSet.Add(o);
            dirSet.Add(new Offset(-o.Dx, -o.Dy));
        }
        dirSet.Remove(new Offset(0, 0));

        var dirs = dirSet.Count == 0
            ? [new Offset(1, 0), new Offset(-1, 0), new Offset(0, 1), new Offset(0, -1)]
            : dirSet.OrderBy(d => d.Dy).ThenBy(d => d.Dx).ToArray();

        var n = alphabet.Length;
        var allowed = new HashSet<int>[dirs.Length][];
        for (var d = 0; d < dirs.Length; d++) {
            allowed[d] = new HashSet<int>[n];
            for (var i = 0; i < n; i++)
                allowed[d][i] = [];
        }

        foreach (var g in grids)
        for (var x = 0; x < g.Width; x++)
        for (var y = 0; y < g.Height; y++) {
            if (!index.TryGetValue(g[x, y], out var ti))
                continue;
            for (var d = 0; d < dirs.Length; d++) {
                // SafeGet returns '0' out of bounds, matching pcg.getTile.
                if (index.TryGetValue(g.SafeGet(x + dirs[d].Dx, y + dirs[d].Dy), out var ni))
                    allowed[d][ti].Add(ni);
            }
        }

        var allowedMask = new ulong[dirs.Length][];
        for (var d = 0; d < dirs.Length; d++) {
            allowedMask[d] = new ulong[n];
            for (var i = 0; i < n; i++) {
                var mask = 0UL;
                foreach (var ni in allowed[d][i])
                    mask |= 1UL << ni;
                allowedMask[d][i] = mask;
            }
        }

        double total = alphabet.Sum(c => freq[c]);
        var weights = alphabet.Select(c => freq[c] / total).ToArray();

        return new AdjacencyModel {
            Alphabet = alphabet, Weights = weights, Dirs = dirs,
            Allowed = allowed, AllowedMask = allowedMask, Index = index,
        };
    }

    /// <summary>
    /// Whether placing <paramref name="candidate"/> at (x, y) would contradict any
    /// already-placed neighbour.
    /// </summary>
    public bool IsLocallyConsistent(PcgGrid grid, int x, int y, char candidate, Func<int, int, bool> isPlaced) {
        if (!Index.TryGetValue(candidate, out var ci))
            return true;

        for (var d = 0; d < Dirs.Length; d++) {
            var nx = x + Dirs[d].Dx;
            var ny = y + Dirs[d].Dy;
            if (nx < 0 || ny < 0 || nx >= grid.Width || ny >= grid.Height)
                continue;
            if (!isPlaced(nx, ny))
                continue;
            if (Index.TryGetValue(grid[nx, ny], out var ni) && !Allowed[d][ci].Contains(ni))
                return false;
        }

        return true;
    }
}
