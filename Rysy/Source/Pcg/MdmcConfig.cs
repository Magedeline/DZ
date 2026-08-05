namespace DZ.RysyPlugin.Pcg;

/// <summary>A relative neighbour offset used as Markov context.</summary>
public readonly record struct Offset(int Dx, int Dy);

/// <summary>
/// The MdMC configuration matrix from pcg_toolkit.lua (paper section 3.3.2).
///
/// A 9-character string read as a row-major 3x3 grid around the cell being generated,
/// index 4 being the cell itself. A '1' marks that neighbour as context; '0' and '2' do
/// not contribute an offset (the Lua accepts '2' as valid input but only ever branches
/// on '1', so it behaves as "off" for context purposes).
/// </summary>
public static class MdmcConfig {
    public const string Default = "000011012";

    public static readonly IReadOnlyDictionary<string, string> Presets = new Dictionary<string, string> {
        ["000011012"] = "E+S - L-shape (paper default)",
        ["000011112"] = "E+SW+S - 3-neighbour causal",
        ["001001112"] = "NE+E+SW+S - 4-neighbour (paper)",
        ["011011012"] = "N+NE+W+E+S - 5-neighbour",
        ["010111010"] = "N+W+E+S - cross (recommended for WFC)",
        ["101000101"] = "NW+NE+SW+SE - diagonals only",
        ["111101111"] = "All 8 neighbours - full context",
    };

    /// <summary>Celeste tileset families: style -> (post-Farewell id, classic id).</summary>
    public static readonly IReadOnlyDictionary<string, (char New, char Old)> StyleFamilies =
        new Dictionary<string, (char, char)> {
            ["city"] = ('1', '2'),
            ["snow"] = ('3', '4'),
            ["resort"] = ('5', '6'),
            ["cave"] = ('8', '9'),
            ["temple"] = ('d', 'a'),
            ["reflection"] = ('g', 'b'),
            ["summit"] = ('i', 'c'),
            ["core"] = ('k', 'e'),
        };

    /// <summary>
    /// Parses a configuration matrix into context offsets, falling back to
    /// <see cref="Default"/> when the string is malformed or selects no neighbours.
    /// </summary>
    public static List<Offset> Parse(string? cfg, Action<string>? log = null) {
        var valid = cfg is { Length: 9 } && cfg.All(c => c is '0' or '1' or '2');

        if (!valid) {
            if (!string.IsNullOrEmpty(cfg))
                log?.Invoke($"Invalid configuration matrix '{cfg}' - falling back to {Default}");
            cfg = Default;
        }

        var offsets = new List<Offset>();
        for (var k = 0; k < 9; k++) {
            if (k == 4 || cfg![k] != '1')
                continue;
            offsets.Add(new Offset(k % 3 - 1, k / 3 - 1));
        }

        return offsets.Count == 0 ? Parse(Default, log) : offsets;
    }

    /// <summary>
    /// Whether an offset points at a cell MdMC has already generated.
    ///
    /// <see cref="Mdmc.Generate"/> scans bottom-to-top, right-to-left, so only cells below,
    /// or to the right on the same row, are filled in by the time a cell is reached.
    /// </summary>
    public static bool IsCausal(Offset o) => o.Dy > 0 || (o.Dy == 0 && o.Dx > 0);

    /// <summary>
    /// How many of a configuration's offsets read cells that have not been generated yet.
    ///
    /// Non-causal offsets always read '0', which biases every context toward air and makes
    /// MdMC collapse toward an empty room -- measurably so: configurations with two or more
    /// non-causal offsets produce under 3% solid regardless of the training data. Such
    /// configurations are meant for WFC, which resolves cells in entropy order rather than
    /// a fixed scan and so has no causality requirement.
    /// </summary>
    public static int NonCausalCount(IEnumerable<Offset> offsets) => offsets.Count(o => !IsCausal(o));

    /// <summary>Preset description annotated with its suitability for MdMC.</summary>
    public static string DescribeForMdmc(string cfg) {
        var desc = Presets.GetValueOrDefault(cfg, cfg);
        var bad = NonCausalCount(Parse(cfg));
        return bad switch {
            0 => $"{desc} [MdMC ok]",
            1 => $"{desc} [WFC only - 1 non-causal]",
            _ => $"{desc} [WFC only - {bad} non-causal]",
        };
    }
}
