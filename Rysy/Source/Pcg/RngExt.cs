namespace DZ.RysyPlugin.Pcg;

public static class RngExt {
    /// <summary>
    /// Lua's rng:nextRange(lo, hi) -- inclusive on both ends.
    ///
    /// Mirrors the degenerate case too: the Lua implementation is
    /// "lo + self:next(hi - lo + 1)" and next(n) returns 0 for n &lt;= 0, so an inverted
    /// range yields lo rather than throwing. Random.Next(lo, hi + 1) would throw instead,
    /// and several callers pass ranges that invert on small rooms.
    /// </summary>
    public static int NextRangeInclusive(this Random rng, int lo, int hi)
        => hi < lo ? lo : rng.Next(lo, hi + 1);
}
