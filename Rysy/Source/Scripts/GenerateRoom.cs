using DZ.RysyPlugin.Pcg;
using Rysy;
using Rysy.Graphics;
using Rysy.Gui.FieldTypes;
using Rysy.Gui.Windows;
using Rysy.Scripting;
using Rysy.Shared;

namespace DZ.RysyPlugin.Scripts;

/// <summary>
/// Fills the current room's tiles with a procedural generator (MdMC, WFC, pattern WFC, or
/// one of the noise/CA/walk family), repairs and scores the result, then optionally places
/// entities/decals/triggers -- the full pcg_toolkit.lua pipeline except skeleton layout,
/// which lays out multiple rooms rather than filling one and lives in its own script.
/// </summary>
public sealed class GenerateRoom : Script {
    private const string LogTag = "DZ.Pcg";

    public override string Name => "Generate Room";

    public override string? Tooltip =>
        "Fills the room's tiles using a procedural generator (Perlin / simplex / cellular automata / random walk)";

    private static readonly string[] GeneratorNames = [
        "mdmc", "patternWfc", "wfc", "perlinCave", "simplexCave", "simplexFbmCave",
        "cellularAutomata", "randomWalkCave", "directionalTunnel",
        "perlinTop", "simplexTop", "smoothedWalkTop",
    ];

    /// <summary>Generators that learn from the map's existing rooms rather than pure noise.</summary>
    private static bool IsTrained(string generator) => generator is "mdmc" or "wfc" or "patternWfc";

    /// <summary>
    /// Which knobs each generator actually reads. Everything else is hidden for that
    /// generator, which keeps the form short enough to fit on screen -- FormWindow renders
    /// one row per visible field with no scrolling, so a flat list of every parameter runs
    /// off the top of the window.
    /// </summary>
    private static readonly Dictionary<string, string[]> GeneratorFields = new() {
        ["mdmc"] = ["mdmcConfig", "trainSource", "maxBacktrackDepth", "useAdjacency",
                    "maxDegenerateFraction"],
        ["wfc"] = ["mdmcConfig", "trainSource", "wfcSolidBorder", "wfcMaxAttempts"],
        ["patternWfc"] = ["trainSource", "patternSize", "wfcMaxAttempts"],
        ["perlinCave"] = ["modifier", "edgesAreWalls", "flipVertical"],
        ["simplexCave"] = ["modifier", "edgesAreWalls", "flipVertical"],
        ["simplexFbmCave"] = ["modifier", "edgesAreWalls", "flipVertical",
                              "fbmOctaves", "fbmPersistence"],
        ["cellularAutomata"] = ["birthLimit", "deathLimit", "passes", "initialDensity", "flipVertical"],
        ["randomWalkCave"] = ["floorPercent", "flipVertical"],
        ["directionalTunnel"] = ["flipVertical"],
        ["perlinTop"] = ["flipVertical"],
        ["simplexTop"] = ["flipVertical"],
        ["smoothedWalkTop"] = ["flipVertical"],
    };

    /// <summary>Fields belonging to some generator but not all -- hidden unless selected.</summary>
    private static readonly string[] GeneratorSpecificFields =
        GeneratorFields.Values.SelectMany(v => v).Distinct().ToArray();

    private static readonly string[] ScoringFields = [
        "scoreBy", "scorePaths", "requirePathVariance", "w1", "w2", "w3", "z1", "z2", "z3",
    ];

    private static readonly string[] EntityPlacementFields = [
        "isEnd", "berryDensity", "berryMin", "berryMax", "berrySpacing",
        "hazardDensity", "spikeMax", "springDensity", "springMax",
    ];
    private static readonly string[] DecalPlacementFields = ["decalDensity", "styleOverride", "eraOverride"];
    private static readonly string[] TriggerPlacementFields = ["triggerMode"];

    private static readonly List<string> FieldOrder = [
        "_hdrMain", "generator", "seed", "layer", "tile",
        "_hdrGen", "mdmcConfig", "trainSource", "useAdjacency", "maxBacktrackDepth",
        "maxDegenerateFraction", "patternSize", "wfcSolidBorder", "wfcMaxAttempts",
        "modifier", "edgesAreWalls", "flipVertical", "fbmOctaves", "fbmPersistence",
        "birthLimit", "deathLimit", "passes", "initialDensity", "floorPercent",
        "_hdrPost", "cleanup", "autoTile", "solidBorder", "ensurePlayable", "maxRepairs",
        "_hdrAccept", "maxAttempts", "minSolidFraction", "maxSolidFraction",
        "_hdrScore", "candidates", "scoreBy", "scorePaths", "requirePathVariance",
        "w1", "w2", "w3", "z1", "z2", "z3",
        "_hdrPlace", "placeEntities", "isEnd", "berryDensity", "berryMin", "berryMax", "berrySpacing",
        "hazardDensity", "spikeMax", "springDensity", "springMax",
        "placeDecals", "decalDensity", "styleOverride", "eraOverride",
        "placeTriggers", "triggerMode",
    ];

    private static IEnumerable<string> HiddenFields(FormContext ctx)
        => HiddenFieldsFor(
            ctx.TryGetValue("generator", out var g) && g is string s ? s : "mdmc",
            ctx.TryGetValue("candidates", out var c) && c is int i ? i : 1,
            ctx.TryGetValue("scoreBy", out var sb) && sb is string sbs ? sbs : "difficulty",
            ctx.TryGetValue("placeEntities", out var pe) && pe is bool peb ? peb : true,
            ctx.TryGetValue("placeDecals", out var pd) && pd is bool pdb ? pdb : false,
            ctx.TryGetValue("placeTriggers", out var pt) && pt is bool ptb ? ptb : false);

    /// <summary>
    /// The hide logic proper, over plain values so it can be exercised without a live form.
    /// </summary>
    internal static IEnumerable<string> HiddenFieldsFor(string generator, int candidates, string scoreBy,
        bool placeEntities = true, bool placeDecals = false, bool placeTriggers = false) {
        var used = GeneratorFields.GetValueOrDefault(generator, []);
        foreach (var f in GeneratorSpecificFields) {
            if (!used.Contains(f))
                yield return f;
        }

        // Candidate ranking is off at 1, so none of the scoring knobs apply.
        if (candidates <= 1) {
            foreach (var f in ScoringFields)
                yield return f;
        } else {
            // Only the selected metric's weights are meaningful.
            foreach (var f in scoreBy == "interestingness" ? new[] { "z1", "z2", "z3" } : ["w1", "w2", "w3"])
                yield return f;
        }

        if (!placeEntities) {
            foreach (var f in EntityPlacementFields)
                yield return f;
        }
        if (!placeDecals) {
            foreach (var f in DecalPlacementFields)
                yield return f;
        }
        if (!placeTriggers) {
            foreach (var f in TriggerPlacementFields)
                yield return f;
        }
    }

    public override FieldList? Parameters => new FieldList {
        ["_hdrMain"] = new PaddingField("Generation"),
        ["generator"] = Fields.Dropdown("mdmc", GeneratorNames),
        ["_hdrGen"] = new PaddingField("Generator settings"),
        ["_hdrPost"] = new PaddingField("Post-processing"),
        ["_hdrAccept"] = new PaddingField("Acceptance"),
        ["_hdrScore"] = new PaddingField("Scoring"),
        ["_hdrPlace"] = new PaddingField("Entity / decal / trigger placement"),

        // --- MdMC only ---
        // 3x3 context mask, row-major, index 4 is the cell itself. Labels flag which
        // presets are usable with MdMC's causal scan -- see MdmcConfig.NonCausalCount.
        ["mdmcConfig"] = Fields.Dropdown(MdmcConfig.Default,
            MdmcConfig.Presets.Keys.Order().ToList(), MdmcConfig.DescribeForMdmc),
        ["trainSource"] = Fields.Dropdown("allRooms", ["allRooms", "currentRoom"]),
        ["maxBacktrackDepth"] = Fields.Int(8),
        ["useAdjacency"] = Fields.Bool(true),
        // WFC only: force the room border to be non-air.
        ["wfcSolidBorder"] = Fields.Bool(true),
        // WFC contradictions are normal; retry before giving up on a run.
        ["wfcMaxAttempts"] = Fields.Int(10),
        // patternWfc only: NxN block size. 3 is the usual choice; larger reproduces the
        // source more literally and costs more time.
        ["patternSize"] = Fields.Int(3),

        // --- Scoring (paper sections 4.1-4.3) ---
        // Generate this many candidates and keep the highest scoring, instead of taking
        // the first that passes. 1 disables candidate selection.
        ["candidates"] = Fields.Int(1),
        ["scorePaths"] = Fields.Int(5),
        // Interestingness is built entirely from accent tiles (any tile other than the
        // dominant one) plus tile diversity, so on a room drawn from a single tileset all
        // three of its terms are identically zero and it cannot rank anything. Difficulty
        // stays meaningful there, so it is the default.
        ["scoreBy"] = Fields.Dropdown("difficulty", ["difficulty", "interestingness"]),
        // Interestingness weights: global accent density, local accent density, diversity.
        ["w1"] = Fields.Float(1f),
        ["w2"] = Fields.Float(1f),
        ["w3"] = Fields.Float(1f),
        // Difficulty weights: hole frequency, exposed-surface density, accent scarcity.
        ["z1"] = Fields.Float(1f),
        ["z2"] = Fields.Float(1f),
        ["z3"] = Fields.Float(1f),
        // The paper's path-variance gate assumes skeleton-built rooms with few exits. On a
        // room with many border openings the sampled pairs differ enough that it rejects
        // almost everything, so it is off unless asked for.
        ["requirePathVariance"] = Fields.Bool(false),
        // Reject rooms where the model had no answer for more than this fraction of cells;
        // above it the output is noise rather than anything learned from the map.
        ["maxDegenerateFraction"] = Fields.Float(0.25f),

        // Solid-density bounds. An all-air room passes the playability gate (empty borders
        // read as connected exits) so it has to be rejected on density instead.
        ["minSolidFraction"] = Fields.Float(0.05f),
        ["maxSolidFraction"] = Fields.Float(0.95f),
        // -1 picks a fresh seed per run.
        ["seed"] = Fields.Int(-1),
        ["layer"] = Fields.Dropdown("fg", ["fg", "bg"]),
        // Celeste tileset ids: '0' is air, '1' is the default dirt tileset.
        ["tile"] = Fields.Char('1'),
        ["edgesAreWalls"] = Fields.Bool(true),
        // The *Top generators fill downward from row 0, which is the top of the room.
        // Leave this on to get ground instead of ceiling.
        ["flipVertical"] = Fields.Bool(true),

        // Post-processing.
        ["cleanup"] = Fields.Bool(true),
        ["autoTile"] = Fields.Bool(true),
        ["solidBorder"] = Fields.Bool(false),
        ["ensurePlayable"] = Fields.Bool(true),
        ["maxRepairs"] = Fields.Int(3),
        // Retry with a fresh seed until the room passes the playability gate.
        ["maxAttempts"] = Fields.Int(8),

        // Per-generator knobs. GeneratorFields decides which of these are shown.
        ["modifier"] = Fields.Float(0.1f),
        ["floorPercent"] = Fields.Int(45),
        ["birthLimit"] = Fields.Int(4),
        ["deathLimit"] = Fields.Int(3),
        ["passes"] = Fields.Int(4),
        ["initialDensity"] = Fields.Float(0.45f),
        ["fbmOctaves"] = Fields.Int(4),
        ["fbmPersistence"] = Fields.Float(0.5f),

        // --- Placement ---
        // Player spawn is always added regardless of this toggle (EnsureSpawn runs as a
        // safety net) -- dying in a spawn-less room errors out in-game. This only controls
        // whether berries/spikes/springs/goldenBerry get placed alongside it.
        ["placeEntities"] = Fields.Bool(true),
        ["isEnd"] = Fields.Bool(false),
        ["berryDensity"] = Fields.Float(0.002f),
        ["berryMin"] = Fields.Int(1),
        ["berryMax"] = Fields.Int(3),
        // 0 = auto (max(4, width/14)).
        ["berrySpacing"] = Fields.Int(0),
        ["hazardDensity"] = Fields.Float(0.05f),
        ["spikeMax"] = Fields.Int(12),
        ["springDensity"] = Fields.Float(0f),
        ["springMax"] = Fields.Int(6),

        ["placeDecals"] = Fields.Bool(false),
        ["decalDensity"] = Fields.Float(0.12f),
        // "auto" detects style/era from the same training rooms MdMC/WFC use (or, lacking
        // those, the generated room itself) via Style.Detect.
        ["styleOverride"] = Fields.Dropdown("auto",
            new[] { "auto" }.Concat(MdmcConfig.StyleFamilies.Keys.Order()).ToList()),
        ["eraOverride"] = Fields.Dropdown("auto", ["auto", "new", "old"]),

        ["placeTriggers"] = Fields.Bool(false),
        ["triggerMode"] = Fields.Dropdown("camera", ["camera", "spawn", "all", "none"]),
    }.Ordered(FieldOrder).SetHiddenFields(HiddenFields);

    public override bool Run(Room room, ScriptArgs args) {
        var seed = args.Get<int>("seed");
        var rng = seed == -1 ? new Random() : new Random(seed);

        var target = args.Get<string>("layer") == "bg" ? room.Bg : room.Fg;
        var w = target.Width;
        var h = target.Height;
        if (w <= 0 || h <= 0)
            return false;

        var tile = args.Get<char>("tile");
        var edges = args.Get<bool>("edgesAreWalls");

        var generator = args.Get<string>("generator");
        var flip = args.Get<bool>("flipVertical");
        var cleanup = args.Get<bool>("cleanup");
        var autoTile = args.Get<bool>("autoTile");
        var solidBorder = args.Get<bool>("solidBorder");
        var repair = args.Get<bool>("ensurePlayable");
        var maxRepairs = args.Get<int>("maxRepairs");
        var maxAttempts = Math.Max(1, args.Get<int>("maxAttempts"));

        var isTrained = IsTrained(generator);
        MdmcTrained? trained = null;
        PatternWfc.Model? patternModel = null;

        if (isTrained) {
            trained = TrainMdmc(room, args);
            if (generator == "patternWfc" && trained is not null)
                patternModel = PatternWfc.Train(trained.SourceGrids, Math.Max(2, args.Get<int>("patternSize")));

            // WFC modes have no fallback of their own: without a model there is nothing to
            // collapse against, so drop to a noise generator rather than emitting nothing.
            var unusable = trained is null
                           || (generator == "wfc" && trained.Adjacency is null)
                           || (generator == "patternWfc" && patternModel is null);
            if (unusable) {
                trained = null;
                patternModel = null;
                isTrained = false;
                generator = "cellularAutomata";
            }
        }

        var maxDegenerate = args.Get<float>("maxDegenerateFraction");
        var candidates = Math.Max(1, args.Get<int>("candidates"));
        var scorePaths = Math.Max(1, args.Get<int>("scorePaths"));
        var (w1, w2, w3) = (args.Get<float>("w1"), args.Get<float>("w2"), args.Get<float>("w3"));
        var (z1, z2, z3) = (args.Get<float>("z1"), args.Get<float>("z2"), args.Get<float>("z3"));
        var scoreBy = args.Get<string>("scoreBy");
        var requireVariance = args.Get<bool>("requirePathVariance");

        PcgGrid? accepted = null;
        PcgGrid? lastGrid = null;
        var acceptedScore = double.NegativeInfinity;
        var kept = 0;

        // Generation is not guaranteed to produce a traversable room, so try a few times.
        // With candidates == 1 the first passing room wins; above that, keep generating and
        // retain the most interesting one. The last attempt is used as-is if none pass,
        // which is still better than silently doing nothing.
        for (var attempt = 0; attempt < maxAttempts && (candidates > 1 || accepted is null); attempt++) {
            if (kept >= candidates)
                break;

            PcgGrid grid;
            var degenerateOk = true;

            if (patternModel is { } pm) {
                var pGrid = PatternWfc.GenerateWithRetries(pm, w, h, rng,
                    Math.Max(1, args.Get<int>("wfcMaxAttempts")));
                if (pGrid is null)
                    continue;
                grid = pGrid;
            } else if (trained is { } model && generator == "wfc") {
                var wfcGrid = Wfc.GenerateWithRetries(model.Adjacency!, w, h, rng,
                    args.Get<bool>("wfcSolidBorder"), Math.Max(1, args.Get<int>("wfcMaxAttempts")));
                // Every attempt contradicted; skip this round and let the outer loop retry.
                if (wfcGrid is null)
                    continue;
                grid = wfcGrid;
            } else if (trained is { } mdmcModel) {
                var result = Mdmc.Generate(mdmcModel.Probs, mdmcModel.Offsets, w, h,
                    args.Get<int>("maxBacktrackDepth"), rng, mdmcModel.DominantTile, mdmcModel.Adjacency);
                grid = result.Grid;
                degenerateOk = result.DegenerateCells <= w * h * maxDegenerate;
            } else {
                grid = Generate(generator, w, h, rng, tile, edges, args);
            }

            // Trained generators learn orientation from the source rooms, so flipping undoes it.
            if (flip && !isTrained)
                grid.FlipVertical();
            if (cleanup)
                TileOps.CleanupTiles(grid, tile);
            if (autoTile)
                TileOps.AutoTilePass(grid, tile);
            if (solidBorder)
                TileOps.ApplyBorder(grid, tile);
            if (repair)
                Playability.EnsurePlayable(grid, tile, maxRepairs);

            lastGrid = grid;

            var solidFraction = grid.CountSolid() / (double) (w * h);
            var densityOk = solidFraction >= args.Get<float>("minSolidFraction")
                            && solidFraction <= args.Get<float>("maxSolidFraction");

            if (!degenerateOk || !densityOk || (repair && !Connectivity.IsPlayable(grid)))
                continue;

            kept++;
            if (candidates == 1) {
                accepted = grid;
                continue;
            }

            var metrics = Scoring.Score(grid, tile, scorePaths, rng);
            if (requireVariance && !metrics.PassesVarianceCheck())
                continue;

            var score = scoreBy == "interestingness"
                ? metrics.Interestingness(w1, w2, w3)
                : metrics.Difficulty(z1, z2, z3);

            // Ties keep the earlier candidate, so a degenerate metric that scores every room
            // identically still yields the first passing room rather than nothing.
            if (accepted is not null && score <= acceptedScore)
                continue;
            acceptedScore = score;
            accepted = grid;
        }

        // Report why nothing usable came out, rather than leaving the room silently
        // unchanged after a click.
        if (accepted is null) {
            Logger.Write(LogTag, LogLevel.Warning, lastGrid is null
                ? $"'{generator}' produced no room in {maxAttempts} attempt(s) -- every attempt " +
                  "hit a contradiction. The room was left untouched."
                : $"'{generator}' never passed the acceptance gate in {maxAttempts} attempt(s); " +
                  "using the last attempt. Try relaxing minSolidFraction/maxSolidFraction, " +
                  "or turning off ensurePlayable.");
        }

        var final = accepted ?? lastGrid;
        if (final is null)
            return false;

        var changed = false;
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            changed |= target.SafeSetTile(final[x, y], x, y);

        // Placement always reads room.Fg -- gameplay collision is always the fg layer,
        // regardless of whether this particular run targeted fg or bg.
        var fgGrid = args.Get<string>("layer") == "fg" ? final : SnapshotTiles(room.Fg);
        if (fgGrid is { Width: > 0, Height: > 0 })
            PlaceContent(room, fgGrid, rng, args);

        return changed;
    }

    private static PcgGrid SnapshotTiles(Tilegrid src) {
        var g = new PcgGrid(src.Width, src.Height);
        for (var x = 0; x < src.Width; x++)
        for (var y = 0; y < src.Height; y++)
            g[x, y] = src.Tiles[x, y];
        return g;
    }

    private void PlaceContent(Room room, PcgGrid fgGrid, Random rng, ScriptArgs args) {
        var placeEntities = args.Get<bool>("placeEntities");
        var placeDecals = args.Get<bool>("placeDecals");
        var placeTriggers = args.Get<bool>("placeTriggers");

        var (autoStyle, _, _, autoEra) = Style.Detect([fgGrid]);
        var style = args.Get<string>("styleOverride") is { } sO && sO != "auto" ? sO : autoStyle;
        var era = args.Get<string>("eraOverride") is { } eO && eO != "auto" ? eO : autoEra;

        var opts = new PlacementOptions {
            IsEnd = args.Get<bool>("isEnd"),
            BerryDensity = args.Get<float>("berryDensity"),
            BerryMin = args.Get<int>("berryMin"),
            BerryMax = args.Get<int>("berryMax"),
            BerrySpacing = args.Get<int>("berrySpacing"),
            HazardDensity = args.Get<float>("hazardDensity"),
            SpikeMax = args.Get<int>("spikeMax"),
            SpringDensity = args.Get<float>("springDensity"),
            SpringMax = args.Get<int>("springMax"),
            Style = style,
            Era = era,
            DecalDensity = args.Get<float>("decalDensity"),
            TriggerMode = args.Get<string>("triggerMode"),
        };

        try {
            var result = RoomPlacement.PlaceAll(room, fgGrid, rng, opts, placeEntities, placeDecals, placeTriggers);
            // Dying in a room with no player spawn errors out in-game -- guaranteed
            // regardless of whether entity placement itself was requested this run. Runs
            // before logging so the reported entity count reflects the room's final state.
            var addedSpawn = RoomPlacement.EnsureSpawn(room, fgGrid);
            Logger.Write(LogTag, LogLevel.Info,
                $"Placed {result.Entities + (addedSpawn ? 1 : 0)} entities, {result.Decals} decals, " +
                $"{result.Triggers} triggers (style={style}, era={era}).");
        } catch (Exception ex) {
            // Tile generation already succeeded and was written to the room; a placement
            // failure shouldn't roll that back or crash the script.
            RoomPlacement.EnsureSpawn(room, fgGrid);
            Logger.Write(LogTag, LogLevel.Error, $"Entity/decal/trigger placement failed: {ex}");
        }
    }

    private sealed record MdmcTrained(
        Dictionary<string, TileDistribution> Probs,
        List<Offset> Offsets,
        char DominantTile,
        AdjacencyModel? Adjacency,
        List<PcgGrid> SourceGrids);

    /// <summary>
    /// Trains the Markov model on the map's existing rooms. Returns null when there is
    /// nothing usable to learn from.
    /// </summary>
    /// <summary>
    /// Every room in the map this room belongs to, or null if that cannot be determined.
    /// Tries the room's own Map first, then the editor's current map, then the rooms the
    /// script was invoked over -- each of these can be absent depending on how the script
    /// is being run, and none of them may be dereferenced blindly.
    /// </summary>
    private static IReadOnlyList<Room>? FindSiblingRooms(Room room, ScriptArgs args) {
        try {
            if (room.Map?.Rooms is { Count: > 0 } fromRoom)
                return fromRoom;
            // EditorState.Current resolves through RysyEngine.Scene, which is not guaranteed
            // to be present, so this whole lookup is best-effort.
            if (EditorState.Current?.Map?.Rooms is { Count: > 0 } fromEditor)
                return fromEditor;
        } catch (Exception) {
            // Fall through to the rooms the script was invoked over.
        }

        return args.Rooms is { Count: > 0 } fromArgs ? fromArgs : null;
    }

    private static MdmcTrained? TrainMdmc(Room room, ScriptArgs args) {
        var useBg = args.Get<string>("layer") == "bg";
        var currentOnly = args.Get<string>("trainSource") == "currentRoom";

        // The room passed to Run is a clone staged for edit; training on it would feed the
        // model whatever is about to be overwritten. Prefer the map's other rooms.
        //
        // ScriptArgs.EditorState is never assigned by ScriptTool.RunScript -- it sets Args,
        // RoomPos and Rooms only -- so reading it throws. Room.Clone copies Map onto the
        // clone, which makes room.Map the reliable route to the sibling rooms.
        var sources = new List<Room>();
        if (!currentOnly && FindSiblingRooms(room, args) is { } rooms) {
            foreach (var r in rooms) {
                if (r is not null && r.Name != room.Name)
                    sources.Add(r);
            }
        }
        if (sources.Count == 0)
            sources.Add(room);

        var grids = new List<PcgGrid>();
        foreach (var r in sources) {
            var src = useBg ? r.Bg : r.Fg;
            if (src is null || src.Width <= 0 || src.Height <= 0)
                continue;
            var g = new PcgGrid(src.Width, src.Height);
            for (var x = 0; x < src.Width; x++)
            for (var y = 0; y < src.Height; y++)
                g[x, y] = src.Tiles[x, y];
            grids.Add(g);
        }

        if (grids.Count == 0)
            return null;

        var offsets = MdmcConfig.Parse(args.Get<string>("mdmcConfig"));
        var dominant = TileOps.DominantSolidTile(grids);

        var counts = Mdmc.Train(grids, offsets);
        Mdmc.InjectSyntheticNGrams(counts, dominant, offsets);
        if (counts.Count == 0)
            return null;

        // WFC collapses against the adjacency model, so it is not optional there -- the
        // checkbox is hidden for WFC and a stale 'false' from an earlier MdMC run must not
        // silently disable it.
        var needsAdjacency = args.Get<string>("generator") == "wfc" || args.Get<bool>("useAdjacency");
        var adjacency = needsAdjacency ? AdjacencyModel.Train(grids, offsets) : null;

        return new MdmcTrained(Mdmc.Normalise(counts), offsets, dominant, adjacency, grids);
    }

    private static PcgGrid Generate(string generator, int w, int h, Random rng, char tile,
        bool edges, ScriptArgs args) => generator switch {
        "perlinCave" => UnityGenerators.PerlinCave(w, h, rng, tile, args.Get<float>("modifier"), edges),
        "simplexCave" => UnityGenerators.SimplexCave(w, h, rng, tile, args.Get<float>("modifier"), edges),
        "simplexFbmCave" => UnityGenerators.SimplexFbmCave(w, h, rng, tile, args.Get<float>("modifier"),
            args.Get<int>("fbmOctaves"), args.Get<float>("fbmPersistence"), edges),
        "cellularAutomata" => UnityGenerators.CellularAutomata(w, h, rng, tile,
            args.Get<int>("birthLimit"), args.Get<int>("deathLimit"),
            args.Get<int>("passes"), args.Get<float>("initialDensity")),
        "randomWalkCave" => UnityGenerators.RandomWalkCave(w, h, rng, tile, args.Get<int>("floorPercent")),
        "directionalTunnel" => UnityGenerators.DirectionalTunnel(w, h, rng, tile),
        "perlinTop" => UnityGenerators.PerlinTopLayer(w, h, rng, tile),
        "simplexTop" => UnityGenerators.SimplexTopLayer(w, h, rng, tile),
        "smoothedWalkTop" => UnityGenerators.SmoothedRandomWalkTop(w, h, rng, tile),
        var other => throw new InvalidOperationException($"Unknown generator '{other}'"),
    };
}
