namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Tile post-processing passes, ported from pcg_toolkit.lua.
/// Each mutates the grid in place unless it returns a new one.
/// </summary>
public static class TileOps {
    /// <summary>Forces a solid one-tile border on all four edges.</summary>
    public static void ApplyBorder(PcgGrid grid, char borderTile) => grid.FillBorder(borderTile);

    /// <summary>
    /// Simple hand-built room used when a generator fails: a floor slab across the bottom
    /// plus a scattering of horizontal platforms.
    /// </summary>
    public static PcgGrid GenerateFallback(int w, int h, char material, Random rng, double solidRatio = 0.35) {
        var grid = new PcgGrid(w, h);
        for (var y = Math.Max(0, h - 3); y < h; y++)
        for (var x = 0; x < w; x++)
            grid[x, y] = material;

        var platformCount = Math.Max(2, (int) (w * h * solidRatio * 0.02));
        for (var p = 0; p < platformCount; p++) {
            var px = rng.NextRangeInclusive(3, w - 6);
            var py = rng.NextRangeInclusive(4, h - 5);
            var len = rng.NextRangeInclusive(3, 7);
            for (var dx = 0; dx < len; dx++) {
                if (px + dx < w - 1 && py >= 0 && py < h)
                    grid[px + dx, py] = material;
            }
        }
        return grid;
    }

    /// <summary>
    /// Removes free-floating single tiles, then plugs 1x1 holes fully enclosed by solids.
    /// Run this before autotiling to avoid single-tile speckle.
    /// </summary>
    public static void CleanupTiles(PcgGrid grid, char dominantTile) {
        var w = grid.Width;
        var h = grid.Height;

        for (var y = 1; y < h - 1; y++)
        for (var x = 1; x < w - 1; x++) {
            if (grid[x, y] == '0')
                continue;
            if (CountSolidNeighbours(grid, x, y) == 0)
                grid[x, y] = '0';
        }

        for (var y = 1; y < h - 1; y++)
        for (var x = 1; x < w - 1; x++) {
            if (grid[x, y] != '0')
                continue;
            if (CountSolidNeighbours(grid, x, y) == 4)
                grid[x, y] = dominantTile;
        }
    }

    /// <summary>
    /// Harmonises tilesets: a solid tile whose orthogonal neighbours are all a different
    /// tileset is rewritten to the most common neighbouring tileset, preferring
    /// <paramref name="dominantTile"/> on ties.
    ///
    /// DIVERGENCE FROM LUA: the original resolves a non-dominant tie with
    /// "for t, c in pairs(typeCounts) ... break", and Lua's pairs order is unspecified, so
    /// that pick is non-deterministic. Ties are broken by lowest tile id here so the same
    /// seed always produces the same room.
    /// </summary>
    public static void AutoTilePass(PcgGrid grid, char dominantTile) {
        var w = grid.Width;
        var h = grid.Height;

        for (var y = 1; y < h - 1; y++)
        for (var x = 1; x < w - 1; x++) {
            var current = grid[x, y];
            if (current == '0')
                continue;

            Span<char> neighbours = [grid[x - 1, y], grid[x + 1, y], grid[x, y - 1], grid[x, y + 1]];

            var types = new Dictionary<char, int>();
            foreach (var t in neighbours) {
                if (t == '0')
                    continue;
                types[t] = types.GetValueOrDefault(t) + 1;
            }

            // Only rewrite tiles that clash with a mixed neighbourhood.
            if (types.Count <= 1 || types.ContainsKey(current))
                continue;

            var maxCount = types.Values.Max();
            grid[x, y] = types.GetValueOrDefault(dominantTile) == maxCount
                ? dominantTile
                : types.Where(kv => kv.Value == maxCount).Min(kv => kv.Key);
        }
    }

    private static int CountSolidNeighbours(PcgGrid grid, int x, int y) {
        var n = 0;
        if (grid[x - 1, y] != '0') n++;
        if (grid[x + 1, y] != '0') n++;
        if (grid[x, y - 1] != '0') n++;
        if (grid[x, y + 1] != '0') n++;
        return n;
    }

    /// <summary>Most common non-air tile across the given grids, defaulting to '1'.</summary>
    public static char DominantSolidTile(IEnumerable<PcgGrid> grids) {
        var freq = new Dictionary<char, int>();
        foreach (var g in grids)
        for (var x = 0; x < g.Width; x++)
        for (var y = 0; y < g.Height; y++) {
            var t = g[x, y];
            if (t == '0')
                continue;
            freq[t] = freq.GetValueOrDefault(t) + 1;
        }

        if (freq.Count == 0)
            return '1';

        var best = freq.Values.Max();
        return freq.Where(kv => kv.Value == best).Min(kv => kv.Key);
    }
}
