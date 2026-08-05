namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Style/era detection and decal texture resolution, ported from pcg_toolkit.lua's
/// style-family and decal-catalog sections. This is what lets decoration/decal placement
/// pick tiles and decals that match the map being generated from, instead of hardcoding
/// the "city" style.
/// </summary>
public static class Style {
    private static readonly Dictionary<char, string> TileToStyle = new() {
        ['1'] = "city", ['2'] = "city",
        ['3'] = "snow", ['4'] = "snow",
        ['5'] = "resort", ['6'] = "resort",
        ['8'] = "cave", ['9'] = "cave",
        ['d'] = "temple", ['a'] = "temple",
        ['g'] = "reflection", ['b'] = "reflection",
        ['i'] = "summit", ['c'] = "summit",
        ['k'] = "core", ['e'] = "core",
    };

    private static readonly Dictionary<char, string> TileToEra = new() {
        ['1'] = "new", ['2'] = "old",
        ['3'] = "new", ['4'] = "old",
        ['5'] = "new", ['6'] = "old",
        ['8'] = "new", ['9'] = "old",
        ['d'] = "new", ['a'] = "old",
        ['g'] = "new", ['b'] = "old",
        ['i'] = "new", ['c'] = "old",
        ['k'] = "new", ['e'] = "old",
    };

    public readonly record struct Detected(string StyleName, char Primary, char Secondary, string Era);

    /// <summary>
    /// Detects the dominant tileset style/era across a set of training rooms by tile
    /// frequency. Falls back to "default"/'1'/'3'/"new" when the grids contain no solid
    /// tiles at all.
    /// </summary>
    public static Detected Detect(IEnumerable<PcgGrid> grids) {
        var counts = new Dictionary<char, int>();
        foreach (var g in grids)
        for (var x = 0; x < g.Width; x++)
        for (var y = 0; y < g.Height; y++) {
            var t = g[x, y];
            if (t != '0')
                counts[t] = counts.GetValueOrDefault(t) + 1;
        }

        if (counts.Count == 0)
            return new Detected("default", '1', '3', "new");

        var sorted = counts.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).ToArray();
        var primary = sorted[0].Key;
        var secondary = sorted.Length > 1 ? sorted[1].Key : primary;

        var styleName = TileToStyle.GetValueOrDefault(primary, "custom");
        var era = TileToEra.GetValueOrDefault(primary, "new");
        return new Detected(styleName, primary, secondary, era);
    }

    private static readonly Dictionary<string, char> StyleDefaultTile = new() {
        ["city"] = '1', ["snow"] = '3', ["resort"] = '5', ["temple"] = 'd',
        ["reflection"] = 'g', ["summit"] = 'i', ["core"] = 'k', ["cave"] = '8',
    };

    /// <summary>Resolves a style/era pair to the matching Celeste tileset id.</summary>
    public static char ResolveTile(string style, string era) {
        if (MdmcConfig.StyleFamilies.TryGetValue(style, out var fam))
            return era == "old" ? fam.Old : fam.New;
        return StyleDefaultTile.GetValueOrDefault(style, '1');
    }

    /// <summary>Inverse of <see cref="ResolveTile"/>: which style/era a tile id belongs to.</summary>
    public static (string Style, string Era) StyleAndEraForTile(char id) {
        foreach (var (style, fam) in MdmcConfig.StyleFamilies) {
            if (fam.New == id)
                return (style, "new");
            if (fam.Old == id)
                return (style, "old");
        }
        return ("custom", "new");
    }

    public sealed record DecalSet(IReadOnlyList<string> Bg, IReadOnlyList<string> Fg);

    // Vanilla Celeste decal paths (relative to Graphics/Atlases/Gameplay) that Lönn and
    // in-game rendering can resolve without any custom assets.
    private static readonly Dictionary<string, DecalSet> DecalCatalog = new() {
        ["city"] = new(["particles/circle", "particles/cloud"],
            ["decals/generic/grass_a", "decals/generic/grass_b", "decals/generic/hanginggrass_a"]),
        ["snow"] = new(["particles/snow"],
            ["decals/generic/snow_a", "decals/generic/snow_b", "decals/generic/snow_c"]),
        ["resort"] = new(["particles/circle", "particles/cloud"],
            ["decals/generic/grass_a", "decals/generic/algae_a", "decals/generic/algae_b"]),
        ["cave"] = new(["particles/blob", "particles/circle"],
            ["decals/generic/algae_a", "decals/generic/algae_b", "decals/generic/algae_c"]),
        ["temple"] = new(["particles/circle", "particles/blob"],
            ["decals/generic/algae_a", "decals/generic/algae_b"]),
        ["reflection"] = new(["particles/circle", "particles/cloud"],
            ["decals/generic/grass_a", "decals/generic/algae_a"]),
        ["summit"] = new(["particles/snow"],
            ["decals/generic/snow_a", "decals/generic/snow_b", "decals/generic/grass_a"]),
        ["core"] = new(["particles/circle", "particles/fire"],
            ["decals/generic/algae_a", "decals/generic/algae_b"]),
    };

    // "old" era keeps the same bg particles; only the generic fg set differs slightly.
    private static readonly Dictionary<string, IReadOnlyList<string>> OldEraFgOverrides = new() {
        ["city"] = ["decals/generic/grass_a", "decals/generic/grass_b"],
        ["snow"] = ["decals/generic/snow_a", "decals/generic/snow_b"],
        ["resort"] = ["decals/generic/grass_a", "decals/generic/algae_a"],
    };

    public static DecalSet DecalSetForStyle(string style, string era) {
        var set = DecalCatalog.GetValueOrDefault(style, DecalCatalog["city"]);
        if (era == "old" && OldEraFgOverrides.TryGetValue(style, out var oldFg))
            set = set with { Fg = oldFg };
        return set;
    }

    private static readonly HashSet<string> KnownDecalTextures = new(StringComparer.Ordinal) {
        "particles/circle", "particles/cloud", "particles/blob", "particles/snow", "particles/fire",
        "particles/stars/00", "particles/starfield/00",
        "decals/generic/grass_a", "decals/generic/grass_b", "decals/generic/grass_c", "decals/generic/grass_d",
        "decals/generic/hanginggrass_a",
        "decals/generic/snow_a", "decals/generic/snow_b", "decals/generic/snow_c", "decals/generic/snow_d",
        "decals/generic/snow_e", "decals/generic/snow_f", "decals/generic/snow_g", "decals/generic/snow_h",
        "decals/generic/snow_i", "decals/generic/snow_j", "decals/generic/snow_k", "decals/generic/snow_l",
        "decals/generic/snow_m", "decals/generic/snow_n", "decals/generic/snow_o",
        "decals/generic/algae_a", "decals/generic/algae_b", "decals/generic/algae_c",
        "decals/generic/algae_d", "decals/generic/algae_e",
    };

    private static readonly Dictionary<string, string> DecalDirectoryVariants = new(StringComparer.Ordinal) {
        ["decals/generic/grass"] = "decals/generic/grass_a",
        ["decals/generic/snow"] = "decals/generic/snow_a",
        ["decals/generic/algae"] = "decals/generic/algae_a",
        ["particles/stars"] = "particles/stars/00",
        ["particles/starfield"] = "particles/starfield/00",
    };

    /// <summary>
    /// Resolves a decal name to a specific, renderable texture path. Unrecognised names
    /// fall back to a vanilla particle that is guaranteed to render, so placement can never
    /// stamp down a broken decal reference.
    /// </summary>
    public static string ResolveDecalTexture(string? name) {
        if (string.IsNullOrEmpty(name))
            return "particles/circle";
        if (KnownDecalTextures.Contains(name))
            return name;
        return DecalDirectoryVariants.GetValueOrDefault(name, "particles/circle");
    }
}
