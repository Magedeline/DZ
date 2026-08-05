using System.Numerics;

namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Wave Function Collapse over the learned <see cref="AdjacencyModel"/>, ported from
/// pcg_toolkit.lua's wfcGenerate.
///
/// Every cell starts able to hold any tile. The lowest-entropy cell is collapsed to a
/// single tile (weighted by training frequency), the choice is propagated outward to shrink
/// neighbouring domains, and the process repeats. Because cells are resolved in entropy
/// order rather than a fixed scan, WFC has none of MdMC's causality constraint -- the
/// configurations <see cref="MdmcConfig.NonCausalCount"/> flags as unusable work here.
///
/// Domains are ulong bitmasks: the alphabet is capped at 64, so propagation is bitwise.
/// </summary>
public static class Wfc {
    /// <summary>
    /// Generates a room, or returns null if propagation hit a contradiction.
    /// Callers should retry with a different seed -- see <see cref="GenerateWithRetries"/>.
    /// </summary>
    public static PcgGrid? Generate(AdjacencyModel model, int w, int h, Random rng,
        bool constrainBorderSolid = true) {
        var n = model.N;
        var full = model.FullDomain;
        var cells = w * h;

        var domains = new ulong[cells];
        Array.Fill(domains, full);

        var queue = new Queue<int>();
        var inQueue = new bool[cells];

        void Enqueue(int idx) {
            if (inQueue[idx])
                return;
            inQueue[idx] = true;
            queue.Enqueue(idx);
        }

        // Force the border to be non-air so rooms come out enclosed.
        if (constrainBorderSolid && model.Index.TryGetValue('0', out var airIdx)) {
            var solidMask = full & ~(1UL << airIdx);
            if (solidMask != 0) {
                for (var x = 0; x < w; x++) {
                    domains[x] = solidMask;
                    Enqueue(x);
                    domains[(h - 1) * w + x] = solidMask;
                    Enqueue((h - 1) * w + x);
                }
                for (var y = 0; y < h; y++) {
                    domains[y * w] = solidMask;
                    Enqueue(y * w);
                    domains[y * w + w - 1] = solidMask;
                    Enqueue(y * w + w - 1);
                }
            }
        }

        if (!Propagate())
            return null;

        while (true) {
            // The Lua clears the queue at the top of each collapse round.
            queue.Clear();
            Array.Clear(inQueue);

            var bestIdx = -1;
            var bestEntropy = double.PositiveInfinity;

            for (var i = 0; i < cells; i++) {
                var dom = domains[i];
                if (BitOperations.PopCount(dom) <= 1)
                    continue;

                double sumW = 0, sumWLog = 0;
                for (var ti = 0; ti < n; ti++) {
                    if ((dom & (1UL << ti)) == 0)
                        continue;
                    var wt = model.Weights[ti];
                    sumW += wt;
                    sumWLog += wt * Math.Log(wt);
                }

                // Tiny random term breaks ties between equal-entropy cells.
                var entropy = Math.Log(sumW) - sumWLog / sumW + rng.NextDouble() * 1e-6;
                if (entropy < bestEntropy) {
                    bestEntropy = entropy;
                    bestIdx = i;
                }
            }

            if (bestIdx < 0)
                break;

            // Collapse, weighted by how often each tile appeared in training.
            var best = domains[bestIdx];
            double totalW = 0;
            for (var ti = 0; ti < n; ti++) {
                if ((best & (1UL << ti)) != 0)
                    totalW += model.Weights[ti];
            }

            var roll = rng.NextDouble() * totalW;
            double cum = 0;
            var chosen = -1;
            for (var ti = 0; ti < n; ti++) {
                if ((best & (1UL << ti)) == 0)
                    continue;
                chosen = ti;
                cum += model.Weights[ti];
                if (roll <= cum)
                    break;
            }
            if (chosen < 0)
                return null;

            domains[bestIdx] = 1UL << chosen;
            Enqueue(bestIdx);

            if (!Propagate())
                return null;
        }

        var grid = new PcgGrid(w, h);
        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++) {
            var dom = domains[y * w + x];
            grid[x, y] = dom == 0 ? '0' : model.Alphabet[BitOperations.TrailingZeroCount(dom)];
        }
        return grid;

        bool Propagate() {
            while (queue.Count > 0) {
                var idx = queue.Dequeue();
                inQueue[idx] = false;

                var cx = idx % w;
                var cy = idx / w;
                var dom = domains[idx];

                for (var d = 0; d < model.Dirs.Length; d++) {
                    var nx = cx + model.Dirs[d].Dx;
                    var ny = cy + model.Dirs[d].Dy;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        continue;

                    // Union of what every tile still possible here permits in direction d.
                    var allow = 0UL;
                    var rest = dom;
                    while (rest != 0) {
                        var ti = BitOperations.TrailingZeroCount(rest);
                        rest &= rest - 1;
                        allow |= model.AllowedMask[d][ti];
                    }

                    var nIdx = ny * w + nx;
                    var before = domains[nIdx];
                    var after = before & allow;
                    if (after == before)
                        continue;
                    if (after == 0)
                        return false;

                    domains[nIdx] = after;
                    Enqueue(nIdx);
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Runs <see cref="Generate"/> until it succeeds or the attempt budget runs out.
    /// Contradictions are normal in WFC; retrying with a fresh roll is the usual remedy.
    /// </summary>
    public static PcgGrid? GenerateWithRetries(AdjacencyModel model, int w, int h, Random rng,
        bool constrainBorderSolid = true, int maxAttempts = 10, Action<string>? log = null) {
        for (var attempt = 1; attempt <= maxAttempts; attempt++) {
            if (Generate(model, w, h, rng, constrainBorderSolid) is { } grid) {
                if (attempt > 1)
                    log?.Invoke($"WFC succeeded on attempt {attempt}");
                return grid;
            }
        }
        log?.Invoke($"WFC hit a contradiction on all {maxAttempts} attempts");
        return null;
    }
}
