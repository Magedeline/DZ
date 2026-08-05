namespace DZ.RysyPlugin.Pcg;

public enum DecorationKind { SpikesUp, Spring }

/// <summary>A hazard/pickup placement candidate found by <see cref="Decoration.DecorationPass"/>.</summary>
public readonly record struct DecorationSpot(int X, int Y, DecorationKind Kind);

/// <summary>
/// Hazard scattering and background-fill passes, ported from pcg_toolkit.lua.
/// </summary>
public static class Decoration {
    /// <summary>
    /// Forces the room border solid except at existing exit openings, so post-processing
    /// (autotile, cleanup) can't accidentally seal off a door the generator carved.
    /// </summary>
    public static void PreserveBordersAndExits(PcgGrid tiles, PcgGrid original, char borderTile) {
        var w = tiles.Width;
        var h = tiles.Height;

        bool hasLeftExit = false, hasRightExit = false, hasTopExit = false, hasBottomExit = false;
        for (var y = 0; y < h; y++) {
            if (original[0, y] == '0') hasLeftExit = true;
            if (original[w - 1, y] == '0') hasRightExit = true;
        }
        for (var x = 0; x < w; x++) {
            if (original[x, 0] == '0') hasTopExit = true;
            if (original[x, h - 1] == '0') hasBottomExit = true;
        }

        var midLo = w / 2 - 1;
        var midHi = w / 2 + 1;
        for (var x = 0; x < w; x++) {
            if (!hasTopExit || x < midLo || x > midHi)
                tiles[x, 0] = borderTile;
            if (!hasBottomExit || x < midLo || x > midHi)
                tiles[x, h - 1] = borderTile;
        }

        var midLoY = h / 2 - 1;
        var midHiY = h / 2 + 1;
        for (var y = 0; y < h; y++) {
            if (!hasLeftExit || y < midLoY || y > midHiY)
                tiles[0, y] = borderTile;
            if (!hasRightExit || y < midLoY || y > midHiY)
                tiles[w - 1, y] = borderTile;
        }
    }

    /// <summary>
    /// Scatters floor spikes and wall-adjacent springs across candidate surfaces. Returns
    /// spots only -- placing the actual entities is the caller's job, since that requires a
    /// live Room to add them to.
    /// </summary>
    public static List<DecorationSpot> DecorationPass(PcgGrid tiles, Random rng,
        double hazardDensity = 0.05, double springDensity = 0.02) {
        var w = tiles.Width;
        var h = tiles.Height;
        var spots = new List<DecorationSpot>();

        // Floor cells with two clear tiles of headroom: safe to drop a spike onto.
        var exposedSurfaces = new List<Poi>();
        for (var x = 1; x < w - 1; x++)
        for (var y = 1; y < h - 2; y++) {
            if (tiles[x, y] != '0' || tiles[x, y + 1] == '0')
                continue;
            if (y > 2 && tiles[x, y - 1] == '0' && tiles[x, y - 2] == '0')
                exposedSurfaces.Add(new Poi(x, y));
        }

        var numSpikes = (int) (exposedSurfaces.Count * hazardDensity);
        Shuffle(exposedSurfaces, rng);
        var spikeLimit = Math.Min(numSpikes, exposedSurfaces.Count);
        for (var i = 0; i < spikeLimit; i++) {
            var s = exposedSurfaces[i];
            if (s.X > 2 && s.X < w - 3)
                spots.Add(new DecorationSpot(s.X, s.Y, DecorationKind.SpikesUp));
        }

        // Vertical shafts with headroom and a wall to brace against: spring candidates.
        for (var x = 2; x < w - 3; x++)
        for (var y = 2; y < h - 5; y++) {
            if (tiles[x, y] != '0' || tiles[x, y + 1] == '0' || tiles[x, y - 1] != '0' || tiles[x, y - 2] != '0')
                continue;

            var hasHeadroom = true;
            for (var dy = 2; dy <= 5; dy++) {
                if (y - dy >= 0 && tiles[x, y - dy] != '0') {
                    hasHeadroom = false;
                    break;
                }
            }
            var hasWallNearby = (x > 2 && tiles[x - 1, y] != '0') || (x < w - 3 && tiles[x + 1, y] != '0');

            if (hasHeadroom && hasWallNearby && rng.NextDouble() < springDensity)
                spots.Add(new DecorationSpot(x, y, DecorationKind.Spring));
        }

        return spots;
    }

    /// <summary>Fills bg tiles under and one tile below every fg solid.</summary>
    public static PcgGrid GenerateBackground(PcgGrid fg, char material) {
        var bg = new PcgGrid(fg.Width, fg.Height);
        for (var y = 0; y < fg.Height; y++)
        for (var x = 0; x < fg.Width; x++) {
            if (fg[x, y] == '0')
                continue;
            bg[x, y] = material;
            if (y + 1 < fg.Height && fg[x, y + 1] == '0')
                bg[x, y + 1] = material;
        }
        return bg;
    }

    /// <summary>
    /// As <see cref="GenerateBackground"/>, but with thicker fill (cave style gets 3 tiles
    /// instead of 2) and a one-tile horizontal skirt, for a less flat-looking backdrop.
    /// </summary>
    public static PcgGrid GenerateBackgroundEnhanced(PcgGrid fg, char material, string style) {
        var w = fg.Width;
        var h = fg.Height;
        var bg = new PcgGrid(w, h);
        var thickness = style == "cave" ? 3 : 2;

        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++) {
            if (fg[x, y] == '0')
                continue;
            bg[x, y] = material;
            for (var dy = 1; dy <= thickness; dy++) {
                if (y + dy < h && fg[x, y + dy] == '0')
                    bg[x, y + dy] = material;
            }
            if (x > 0 && fg[x - 1, y] == '0') bg[x - 1, y] = material;
            if (x < w - 1 && fg[x + 1, y] == '0') bg[x + 1, y] = material;
        }
        return bg;
    }

    private static void Shuffle<T>(IList<T> list, Random rng) {
        for (var i = list.Count - 1; i >= 1; i--) {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
