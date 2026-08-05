namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Perlin and simplex noise, ported from PCGHelper's pcg_unity_generators.lua.
/// Both are mapped to [0, 1] to match Unity's Mathf.PerlinNoise convention.
/// </summary>
public static class Noise {
    // NOTE: copied verbatim from the Lua so output matches. This is NOT the canonical
    // Ken Perlin permutation table -- rows 10-15 diverge from it (e.g. "...,23,183,194,213"
    // where the reference has "...,223,183,170,213"). SimplexPerm below IS canonical.
    // Left as-is deliberately: "correcting" it would silently desync this port from the
    // Lua original for every seed.
    private static readonly int[] PerlinPerm = BuildDoubled([
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,
        140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,148,
        247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,
        57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,
        74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,
        60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,
        65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,
        200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,
        52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,
        207,206,59,227,47,16,58,17,182,189,28,42,23,183,194,213,
        157,6,76,84,45,35,172,112,128,24,50,129,153,35,189,93,
        180,170,188,131,206,85,29,173,16,38,115,210,247,170,87,68,
        232,58,161,7,203,252,157,59,121,181,106,255,141,180,133,109,
        233,178,235,252,116,251,145,150,110,241,115,233,181,21,245,146,
        144,157,226,143,250,131,141,239,163,81,183,142,255,123,187,62,
    ]);

    private static readonly int[] SimplexPerm = BuildDoubled([
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,
        140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,148,
        247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,
        57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,
        74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,
        60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,
        65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,
        200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,
        52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,
        207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,
        119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,9,
        129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,
        218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,
        81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,
        184,84,204,176,115,121,50,45,127,4,150,254,138,236,205,93,
        222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
    ]);

    /// <summary>
    /// Pads a permutation table to 256 entries, then duplicates it to 512 so the
    /// AA+1 / BB+1 overflow indices stay in range.
    ///
    /// PerlinPerm only has 240 authored entries but is indexed as if it had 256, so in
    /// Lua slots 241..256 (and 497..512 after doubling) are nil and
    /// "PerlinPerm[A] + Z" throws for any A that lands there -- reachable in practice,
    /// since A = PerlinPerm[X] + Y spans [1, 511]. See the note in the port summary.
    ///
    /// Wrapping via "i % src.Length" is the most conservative fix available: it leaves
    /// all 240 authored values at their original indices and only defines the slots that
    /// currently crash. It does mean seeds that hit those slots produce different output
    /// here than in Lua -- but in Lua they produce no output at all.
    /// </summary>
    private static int[] BuildDoubled(int[] src) {
        var full = new int[512];
        for (var i = 0; i < 256; i++)
            full[i] = src[i % src.Length];
        for (var i = 0; i < 256; i++)
            full[i + 256] = full[i];
        return full;
    }

    private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static double Lerp(double t, double a, double b) => a + t * (b - a);

    private static double Grad(int hash, double x, double y, double z) {
        var h = Mod(hash, 16);
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : (h is 12 or 14 ? x : z);
        return (Mod(h, 2) == 0 ? u : -u) + (Mod(h, 4) < 2 ? v : -v);
    }

    /// <summary>Lua's % is a floored modulo; C#'s % is truncated. Match Lua.</summary>
    private static int Mod(int a, int m) {
        var r = a % m;
        return r < 0 ? r + m : r;
    }

    private static double FloorMod(double a, int m) {
        var r = Math.Floor(a) % m;
        return r < 0 ? r + m : r;
    }

    /// <summary>Classic 3D Perlin noise sampled in 2D, mapped to [0, 1].</summary>
    public static double Perlin2D(double x, double y, double z = 0) {
        // Lua is 1-based: X = floor(x) % 256 + 1. Our array is 0-based, so drop the +1.
        var xi = (int) FloorMod(x, 256);
        var yi = (int) FloorMod(y, 256);
        var zi = (int) FloorMod(z, 256);

        x -= Math.Floor(x);
        y -= Math.Floor(y);
        z -= Math.Floor(z);

        var u = Fade(x);
        var v = Fade(y);
        var w = Fade(z);

        var a = PerlinPerm[xi] + yi;
        var aa = PerlinPerm[a] + zi;
        var ab = PerlinPerm[a + 1] + zi;
        var b = PerlinPerm[xi + 1] + yi;
        var ba = PerlinPerm[b] + zi;
        var bb = PerlinPerm[b + 1] + zi;

        var raw = Lerp(w,
            Lerp(v,
                Lerp(u, Grad(PerlinPerm[aa], x, y, z), Grad(PerlinPerm[ba], x - 1, y, z)),
                Lerp(u, Grad(PerlinPerm[ab], x, y - 1, z), Grad(PerlinPerm[bb], x - 1, y - 1, z))),
            Lerp(v,
                Lerp(u, Grad(PerlinPerm[aa + 1], x, y, z - 1), Grad(PerlinPerm[ba + 1], x - 1, y, z - 1)),
                Lerp(u, Grad(PerlinPerm[ab + 1], x, y - 1, z - 1), Grad(PerlinPerm[bb + 1], x - 1, y - 1, z - 1))));

        return (raw + 1) * 0.5;
    }

    private static readonly int[][] SimplexGrad3 = [
        [1, 1, 0], [-1, 1, 0], [1, -1, 0], [-1, -1, 0],
        [1, 0, 1], [-1, 0, 1], [1, 0, -1], [-1, 0, -1],
        [0, 1, 1], [0, -1, 1], [0, 1, -1], [0, -1, -1],
    ];

    private static readonly double SimplexF2 = 0.5 * (Math.Sqrt(3.0) - 1.0);
    private static readonly double SimplexG2 = (3.0 - Math.Sqrt(3.0)) / 6.0;

    private static double Dot2D(int[] g, double x, double y) => g[0] * x + g[1] * y;

    /// <summary>Raw 2D simplex noise in [-1, 1].</summary>
    public static double Simplex2DRaw(double xin, double yin) {
        var s = (xin + yin) * SimplexF2;
        var i = (int) Math.Floor(xin + s);
        var j = (int) Math.Floor(yin + s);
        var t = (i + j) * SimplexG2;
        var x0 = xin - (i - t);
        var y0 = yin - (j - t);

        int i1, j1;
        if (x0 > y0) { i1 = 1; j1 = 0; } else { i1 = 0; j1 = 1; }

        var x1 = x0 - i1 + SimplexG2;
        var y1 = y0 - j1 + SimplexG2;
        var x2 = x0 - 1.0 + 2.0 * SimplexG2;
        var y2 = y0 - 1.0 + 2.0 * SimplexG2;

        var ii = Mod(i, 256);
        var jj = Mod(j, 256);

        var gi0 = Mod(SimplexPerm[ii + SimplexPerm[jj]], 12);
        var gi1 = Mod(SimplexPerm[ii + i1 + SimplexPerm[jj + j1]], 12);
        var gi2 = Mod(SimplexPerm[ii + 1 + SimplexPerm[jj + 1]], 12);

        var n0 = Corner(0.5 - x0 * x0 - y0 * y0, SimplexGrad3[gi0], x0, y0);
        var n1 = Corner(0.5 - x1 * x1 - y1 * y1, SimplexGrad3[gi1], x1, y1);
        var n2 = Corner(0.5 - x2 * x2 - y2 * y2, SimplexGrad3[gi2], x2, y2);

        return 70.0 * (n0 + n1 + n2);

        static double Corner(double tt, int[] g, double x, double y) {
            if (tt < 0) return 0.0;
            tt *= tt;
            return tt * tt * Dot2D(g, x, y);
        }
    }

    /// <summary>2D simplex noise mapped to [0, 1].</summary>
    public static double Simplex2D(double x, double y) => (Simplex2DRaw(x, y) + 1) * 0.5;

    /// <summary>
    /// Fractal Brownian motion: layered simplex octaves at doubling frequency and
    /// decaying amplitude, mapped to [0, 1].
    /// </summary>
    public static double SimplexFbm2D(double x, double y, int octaves = 4, double persistence = 0.5,
        double lacunarity = 2.0) {
        octaves = Math.Max(1, octaves);
        double total = 0, amplitude = 1, frequency = 1, maxValue = 0;
        for (var i = 0; i < octaves; i++) {
            total += Simplex2DRaw(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        return (total / maxValue + 1) * 0.5;
    }
}
