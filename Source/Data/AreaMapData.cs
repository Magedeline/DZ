using System.Collections.Generic;
using Celeste.Cutscenes;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

/// <summary>
/// Defines each chapter's area data in a vanilla Celeste-like manner for our mod.
/// This replaces AltSideHelper's meta YAML system with direct code registration,
/// making each chapter work like a vanilla Celeste chapter with all 5 sides 
/// (A, B, C, D, DX) natively integrated.
/// 
/// Each chapter gets:
/// - Proper mountain camera positions per side
/// - Correct music assignments per side
/// - Heart gem tracking per side
/// - Completion flags per side
/// - Side unlock chain: A → B → C (postcard) → D (postcard) → DX (postcard)
/// </summary>
public static class AreaMapData
{
    private const string GuiAreaIconRoot = "areas/Maggy";
    private const string LockedGuiAreaIcon = GuiAreaIconRoot + "/lock";

    private static readonly IReadOnlyDictionary<int, string> GuiIconNameByChapter = new Dictionary<int, string>
    {
        [0] = "prolouge",
        [1] = "city",
        [2] = "nightmare",
        [3] = "star",
        [4] = "legend",
        [5] = "resort",
        [6] = "stronghold",
        [7] = "hell",
        [8] = "truth",
        [9] = "summit",
        [10] = "ruins",
        [11] = "snow",
        [12] = "water",
        [13] = "fire",
        [14] = "digital",
        [15] = "castle",
        [16] = "corruption",
        [17] = "postepilogue",
        [18] = "heart",
        [19] = "farewell",
        [20] = "theend"
    };

    /// <summary>
    /// Chapter definition containing all metadata needed for vanilla-like integration.
    /// </summary>
    public class ChapterDef
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string SID { get; set; }
        public string Icon { get; set; }
        public bool IsInterlude { get; set; }
        public bool HasBSide { get; set; }
        public bool HasCSide { get; set; }
        public bool HasDSide { get; set; }
        public bool HasDXSide { get; set; }

        /// <summary>Music event per side (A, B, C, D, DX)</summary>
        public string[] MusicEvents { get; set; }

        /// <summary>Ambience event per side</summary>
        public string[] AmbienceEvents { get; set; }

        /// <summary>Cassette song event</summary>
        public string CassetteSong { get; set; }

        /// <summary>Mountain camera data</summary>
        public MountainCameraData MountainData { get; set; }

        /// <summary>3D overworld state index (0=normal, 1=dark, 2=void)</summary>
        public int MountainState { get; set; }

        /// <summary>
        /// Optional override for the mountain model/texture directory used by
        /// MountainOverworldManager when registering this chapter's MountainResources.
        /// Null means use the shared default (Mountain/Maggy/Desolo_Zantas).
        /// </summary>
        public string MountainModelDir { get; set; }
    }

    /// <summary>Camera positions for the 3D mountain overworld per chapter.</summary>
    public class MountainCameraData
    {
        public Vector3 IdlePos { get; set; }
        public Vector3 IdleTarget { get; set; }
        public Vector3 SelectPos { get; set; }
        public Vector3 SelectTarget { get; set; }
        public Vector3 ZoomPos { get; set; }
        public Vector3 ZoomTarget { get; set; }
        public Vector3 Cursor { get; set; }
    }

    // ── Chapter Registry ─────────────────────────────────────────────────

    /// <summary>All chapter definitions for the mod</summary>
    public static readonly List<ChapterDef> Chapters = new();

    /// <summary>Lookup by chapter number</summary>
    private static readonly Dictionary<int, ChapterDef> _byNumber = new();

    /// <summary>Lookup by SID</summary>
    private static readonly Dictionary<string, ChapterDef> _bySID = new();

    /// <summary>Lookup by side-agnostic chapter key (for A/B/C/D/DX variants).</summary>
    private static readonly Dictionary<string, ChapterDef> _byBaseKey = new(StringComparer.OrdinalIgnoreCase);

    private static bool _initialized;

    // ── Initialization ───────────────────────────────────────────────────

    /// <summary>
    /// Ensures the registry is initialized before use. Lazy initialization
    /// reduces startup time by deferring chapter registration until first access.
    /// </summary>
    private static void EnsureInitialized()
    {
        if (!_initialized)
        {
            _initialized = true;
            Initialize();
        }
    }

    /// <summary>
    /// Registers all chapter definitions. Called during module initialization.
    /// </summary>
    public static void Initialize()
    {
        Chapters.Clear();
        _byNumber.Clear();
        _bySID.Clear();
        _byBaseKey.Clear();

        // ── Prologue (Chapter 0) ──
        // Image 1: position=(-1.374, 1.224, 7.971) target=(-0.440, 0.499, 6.358)
        Register(new ChapterDef
        {
            Number = 0,
            Name = "prolouge",
            SID = AreaModeExtender.BuildASideSID("00_Prologue"),
            Icon = "areas/prologue",
            IsInterlude = true,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/pusheen/music/lvl0/intro" },
            AmbienceEvents = new[] { "event:/pusheen/env/00_prologue" },
            MountainState = 0,
            MountainData = new MountainCameraData
            {
                IdlePos    = new Vector3(-1.374f, 1.224f, 9.371f),
                IdleTarget = new Vector3(-0.440f, 0.499f, 7.758f),
                SelectPos    = new Vector3(-1.374f, 1.224f, 7.971f),
                SelectTarget = new Vector3(-0.440f, 0.499f, 6.358f),
                ZoomPos    = new Vector3(-1.007f, 0.862f, 6.965f),
                ZoomTarget = new Vector3(-0.073f, 0.137f, 5.352f),
                Cursor = new Vector3(-0.440f, 0.499f, 6.358f)
            }
        });

        // ── Chapter 1: Forbidden Metropolis ──
        // Image 9: position=(-1.234, 0.677, 7.598) target=(-0.221, 0.734, 5.875)
        RegisterStandardChapter(1, "forbiddenmetro", "01_City",
            "areas/city", 0,
            idle:   (new Vector3(-1.234f,  0.677f,  9.198f), new Vector3(-0.221f,  0.734f,  7.475f)),
            select: (new Vector3(-1.234f,  0.677f,  7.598f), new Vector3(-0.221f,  0.734f,  5.875f)),
            zoom:   (new Vector3(-0.867f,  0.315f,  6.592f), new Vector3( 0.146f,  0.372f,  4.869f)),
            cursor:  new Vector3(-0.221f,  0.734f,  5.875f));

        // ── Chapter 2: Veil of Shadows ──
        // Image 2: position=(-0.952, 4.218, 9.744) target=(-0.111, 3.393, 8.128)
        RegisterStandardChapter(2, "shadowofveil", "02_Nightmare",
            "areas/nightmare", 0,
            idle:   (new Vector3(-0.952f,  4.218f, 11.344f), new Vector3(-0.111f,  3.393f,  9.728f)),
            select: (new Vector3(-0.952f,  4.218f,  9.744f), new Vector3(-0.111f,  3.393f,  8.128f)),
            zoom:   (new Vector3(-0.585f,  3.856f,  8.738f), new Vector3( 0.256f,  3.031f,  7.122f)),
            cursor:  new Vector3(-0.111f,  3.393f,  8.128f));

        // ── Chapter 3: Arrival ──
        // Image 3: position=(-3.431, 5.512, 4.284) target=(-2.287, 4.359, 3.118)
        RegisterStandardChapter(3, "arrivial", "03_Stars",
            "areas/stars", 0,
            idle:   (new Vector3(-3.431f,  5.512f,  5.884f), new Vector3(-2.287f,  4.359f,  4.718f)),
            select: (new Vector3(-3.431f,  5.512f,  4.284f), new Vector3(-2.287f,  4.359f,  3.118f)),
            zoom:   (new Vector3(-3.064f,  5.150f,  3.278f), new Vector3(-1.920f,  3.997f,  2.112f)),
            cursor:  new Vector3(-2.287f,  4.359f,  3.118f));

        // ── Chapter 4: Chronicles of Destiny ──
        // Image 8: position=(-14.620, 3.606, 19.135) target=(-13.134, 4.115, 17.897)
        RegisterStandardChapter(4, "thelegend", "04_Legend",
            "areas/legend", 0,
            idle:   (new Vector3(-14.620f,  3.606f, 20.735f), new Vector3(-13.134f,  4.115f, 19.497f)),
            select: (new Vector3(-14.620f,  3.606f, 19.135f), new Vector3(-13.134f,  4.115f, 17.897f)),
            zoom:   (new Vector3(-14.253f,  3.244f, 18.129f), new Vector3(-12.767f,  3.753f, 16.891f)),
            cursor:  new Vector3(-13.134f,  4.115f, 17.897f));

        // ── Chapter 5: Fractured Memories ──
        // Image 10: position=(-4.473, 7.158, 5.463) target=(-3.630, 6.660, 3.719)
        RegisterStandardChapter(5, "fractureresort", "05_Restore",
            "areas/restore", 0,
            idle:   (new Vector3(-4.473f,  7.158f,  7.063f), new Vector3(-3.630f,  6.660f,  5.319f)),
            select: (new Vector3(-4.473f,  7.158f,  5.463f), new Vector3(-3.630f,  6.660f,  3.719f)),
            zoom:   (new Vector3(-4.106f,  6.796f,  4.457f), new Vector3(-3.263f,  6.298f,  2.713f)),
            cursor:  new Vector3(-3.630f,  6.660f,  3.719f));

        // ── Chapter 6: Fortress of Solitude ──
        // Image 4: position=(5.961, 8.823, 5.058) target=(5.061, 7.757, 3.625)
        RegisterStandardChapter(6, "stronghold", "06_Stronghold",
            "areas/stronghold", 0,
            idle:   (new Vector3( 5.961f,  8.823f,  6.658f), new Vector3( 5.061f,  7.757f,  5.225f)),
            select: (new Vector3( 5.961f,  8.823f,  5.058f), new Vector3( 5.061f,  7.757f,  3.625f)),
            zoom:   (new Vector3( 6.328f,  8.461f,  4.052f), new Vector3( 5.428f,  7.395f,  2.619f)),
            cursor:  new Vector3( 5.061f,  7.757f,  3.625f));

        // ── Chapter 7: Infernal Reflections ──
        // Image 5: position=(9.626, 8.824, -4.140) target=(7.924, 8.240, -3.267)
        RegisterStandardChapter(7, "infornoreflection", "07_Hell",
            "areas/hell", MountainOverworldManager.STATE_DARK,
            idle:   (new Vector3( 9.626f,  8.824f, -2.540f), new Vector3( 7.924f,  8.240f, -1.667f)),
            select: (new Vector3( 9.626f,  8.824f, -4.140f), new Vector3( 7.924f,  8.240f, -3.267f)),
            zoom:   (new Vector3( 9.993f,  8.462f, -5.146f), new Vector3( 8.291f,  7.878f, -4.273f)),
            cursor:  new Vector3( 7.924f,  8.240f, -3.267f));

        // ── Chapter 8: Revelation's Edge ──
        // Image 6: position=(-0.963, 10.542, -5.314) target=(-0.178, 9.588, -3.741)
        RegisterStandardChapter(8, "revelationedge", "08_Truth",
            "areas/truth", 0,
            idle:   (new Vector3(-0.963f, 10.542f, -3.714f), new Vector3(-0.178f,  9.588f, -2.141f)),
            select: (new Vector3(-0.963f, 10.542f, -5.314f), new Vector3(-0.178f,  9.588f, -3.741f)),
            zoom:   (new Vector3(-0.596f, 10.180f, -6.320f), new Vector3( 0.189f,  9.226f, -4.747f)),
            cursor:  new Vector3(-0.178f,  9.588f, -3.741f));

        // ── Chapter 9: Apex of Reality (Summit) ──
        // Image 7: position=(1.113, 12.154, 6.334) target=(-0.086, 11.118, 5.115)
        RegisterStandardChapter(9, "beyondsummit", "09_Summit",
            "areas/summit", 0,
            idle:   (new Vector3( 1.113f, 12.154f,  7.934f), new Vector3(-0.086f, 11.118f,  6.715f)),
            select: (new Vector3( 1.113f, 12.154f,  6.334f), new Vector3(-0.086f, 11.118f,  5.115f)),
            zoom:   (new Vector3( 1.480f, 11.792f,  5.328f), new Vector3( 0.281f, 10.756f,  4.109f)),
            cursor:  new Vector3(-0.086f, 11.118f,  5.115f));

        // ── Chapter 10: Echoes of the Past ──
        // Blend between img7 and img11 — mid climb, slight upward shift
        RegisterStandardChapter(10, "echosofpast", "10_Ruins",
            "areas/ruins", 0,
            idle:   (new Vector3( 0.514f, 14.102f,  8.460f), new Vector3(-0.462f, 13.157f,  6.891f)),
            select: (new Vector3( 0.514f, 14.102f,  7.860f), new Vector3(-0.462f, 13.157f,  6.291f)),
            zoom:   (new Vector3( 0.881f, 13.740f,  6.854f), new Vector3(-0.095f, 12.795f,  5.285f)),
            cursor:  new Vector3(-0.462f, 13.157f,  6.291f));

        // ── Chapter 11: Frozen Sanctuary ──
        RegisterStandardChapter(11, "frozensanctuary", "11_Snow",
            "areas/snow", 0,
            idle:   (new Vector3(-0.185f, 16.051f,  9.185f), new Vector3(-0.838f, 15.230f,  7.315f)),
            select: (new Vector3(-0.185f, 16.051f,  8.585f), new Vector3(-0.838f, 15.230f,  6.715f)),
            zoom:   (new Vector3( 0.182f, 15.689f,  7.579f), new Vector3(-0.471f, 14.868f,  5.709f)),
            cursor:  new Vector3(-0.838f, 15.230f,  6.715f));

        // ── Chapter 12: Cascading Depths ──
        RegisterStandardChapter(12, "cascadingdepths", "12_Water",
            "areas/water", 0,
            idle:   (new Vector3(-0.884f, 18.000f,  9.910f), new Vector3(-1.214f, 17.303f,  7.539f)),
            select: (new Vector3(-0.884f, 18.000f,  9.310f), new Vector3(-1.214f, 17.303f,  6.939f)),
            zoom:   (new Vector3(-0.517f, 17.638f,  8.304f), new Vector3(-0.847f, 16.941f,  5.933f)),
            cursor:  new Vector3(-1.214f, 17.303f,  6.939f));

        // ── Chapter 13: Blazing Territories ──
        RegisterStandardChapter(13, "balzingteritory", "13_Fire",
            "areas/fire", MountainOverworldManager.STATE_DARK,
            idle:   (new Vector3(-1.583f, 19.949f, 10.635f), new Vector3(-1.590f, 19.376f,  8.163f)),
            select: (new Vector3(-1.583f, 19.949f, 10.035f), new Vector3(-1.590f, 19.376f,  7.563f)),
            zoom:   (new Vector3(-1.216f, 19.587f,  9.029f), new Vector3(-1.223f, 19.014f,  6.557f)),
            cursor:  new Vector3(-1.590f, 19.376f,  7.563f));

        // ── Chapter 14: Cyber Nexus ──
        RegisterStandardChapter(14, "cybernexus", "14_Digital",
            "areas/digital", 0,
            idle:   (new Vector3(-1.660f, 22.498f, 10.160f), new Vector3(-1.966f, 21.449f,  7.987f)),
            select: (new Vector3(-1.660f, 22.498f,  9.560f), new Vector3(-1.966f, 21.449f,  7.387f)),
            zoom:   (new Vector3(-1.293f, 22.136f,  8.554f), new Vector3(-1.599f, 21.087f,  6.381f)),
            cursor:  new Vector3(-1.966f, 21.449f,  7.387f));

        // ── Chapter 15: Ethereal Citadel ──
        RegisterStandardChapter(15, "etheraealcitadel", "15_Castle",
            "areas/castle", 0,
            idle:   (new Vector3(-1.737f, 25.047f,  9.685f), new Vector3(-1.722f, 24.522f,  7.411f)),
            select: (new Vector3(-1.737f, 25.047f,  9.085f), new Vector3(-1.722f, 24.522f,  6.811f)),
            zoom:   (new Vector3(-1.370f, 24.685f,  8.079f), new Vector3(-1.355f, 24.160f,  5.805f)),
            cursor:  new Vector3(-1.722f, 24.522f,  6.811f));

        // ── Chapter 16: Organ Garden of Despair (A-Side only) ──
        // Image 11: position=(-1.916, 33.050, 9.585) target=(-1.479, 32.938, 7.636)
        Register(new ChapterDef
        {
            Number = 16,
            Name = "organgarden",
            SID = AreaModeExtender.BuildASideSID("16_Corruption"),
            Icon = "areas/corruption",
            IsInterlude = false,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/pusheen/music/lvl16/cinematic/intro01" },
            AmbienceEvents = new[] { "event:/pusheen/env/16_myworld" },
            MountainState = MountainOverworldManager.STATE_DARK,
            MountainData = new MountainCameraData
            {
                IdlePos    = new Vector3(-1.916f, 33.050f, 11.185f),
                IdleTarget = new Vector3(-1.479f, 32.938f,  9.236f),
                SelectPos    = new Vector3(-1.916f, 33.050f,  9.585f),
                SelectTarget = new Vector3(-1.479f, 32.938f,  7.636f),
                ZoomPos    = new Vector3(-1.549f, 32.688f,  8.579f),
                ZoomTarget = new Vector3(-1.112f, 32.576f,  6.630f),
                Cursor = new Vector3(-1.479f, 32.938f,  7.636f)
            }
        });

        // ── Chapter 17: Epilogue (A-Side only) ──
        // Above ch16 — further up the same high tower area
        Register(new ChapterDef
        {
            Number = 17,
            Name = "epilouge",
            SID = AreaModeExtender.BuildASideSID("17_Epilogue"),
            Icon = "areas/epilogue",
            IsInterlude = true,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/pusheen/music/lvl17/main" },
            AmbienceEvents = new[] { "event:/pusheen/env/00_main" },
            MountainState = 0,
            MountainData = new MountainCameraData
            {
                IdlePos    = new Vector3(-1.916f, 35.450f, 11.185f),
                IdleTarget = new Vector3(-1.479f, 35.338f,  9.236f),
                SelectPos    = new Vector3(-1.916f, 35.450f,  9.585f),
                SelectTarget = new Vector3(-1.479f, 35.338f,  7.636f),
                ZoomPos    = new Vector3(-1.549f, 35.088f,  8.579f),
                ZoomTarget = new Vector3(-1.112f, 34.976f,  6.630f),
                Cursor = new Vector3(-1.479f, 35.338f,  7.636f)
            }
        });

        // ── Chapter 18: Core of Existence ──
        RegisterStandardChapter(18, "coreexistence", "18_Heart",
            "areas/heart", MountainOverworldManager.STATE_DARK,
            idle:   (new Vector3(-1.916f, 37.850f, 11.185f), new Vector3(-1.479f, 37.738f,  9.236f)),
            select: (new Vector3(-1.916f, 37.850f,  9.585f), new Vector3(-1.479f, 37.738f,  7.636f)),
            zoom:   (new Vector3(-1.549f, 37.488f,  8.579f), new Vector3(-1.112f, 37.376f,  6.630f)),
            cursor:  new Vector3(-1.479f, 37.738f,  7.636f));

        // ── Chapter 19: Farewell to Stars (A-Side only) ──
        Register(new ChapterDef
        {
            Number = 19,
            Name = "farewellstar",
            SID = AreaModeExtender.BuildASideSID("19_Space"),
            Icon = "areas/space",
            IsInterlude = false,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/pusheen/music/lvl18/main" },
            AmbienceEvents = new[] { "event:/pusheen/env/18_main" },
            MountainState = MountainOverworldManager.STATE_VOID,
            MountainData = new MountainCameraData
            {
                IdlePos    = new Vector3(-1.916f, 40.250f, 11.185f),
                IdleTarget = new Vector3(-1.479f, 40.138f,  9.236f),
                SelectPos    = new Vector3(-1.916f, 40.250f,  9.585f),
                SelectTarget = new Vector3(-1.479f, 40.138f,  7.636f),
                ZoomPos    = new Vector3(-1.549f, 39.888f,  8.579f),
                ZoomTarget = new Vector3(-1.112f, 39.776f,  6.630f),
                Cursor = new Vector3(-1.479f, 40.138f,  7.636f)
            }
        });

        // ── Chapter 20: The Last Push (A-Side only) ──
        Register(new ChapterDef
        {
            Number = 20,
            Name = "lastpush",
            SID = AreaModeExtender.BuildASideSID("20_TheEnd"),
            Icon = "areas/theend",
            IsInterlude = false,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/" },
            AmbienceEvents = new[] { "event:/" },
            MountainState = MountainOverworldManager.STATE_DARK,
            MountainData = new MountainCameraData
            {
                IdlePos    = new Vector3(-1.916f, 42.650f, 11.185f),
                IdleTarget = new Vector3(-1.479f, 42.538f,  9.236f),
                SelectPos    = new Vector3(-1.916f, 42.650f,  9.585f),
                SelectTarget = new Vector3(-1.479f, 42.538f,  7.636f),
                ZoomPos    = new Vector3(-1.549f, 42.288f,  8.579f),
                ZoomTarget = new Vector3(-1.112f, 42.176f,  6.630f),
                Cursor = new Vector3(-1.479f, 42.538f,  7.636f)
            }
        });

        Logger.Log(LogLevel.Info, "MaggyHelper",
            $"Registered {Chapters.Count} chapters in AreaMapData");
    }

    /// <summary>
    /// Applies hardcoded chapter metadata to runtime AreaData entries.
    /// This is intentionally done during AreaData.Load hooks so chapter panel data
    /// is deterministic and not left to map-name parsing heuristics.
    /// </summary>
    public static void ApplyHardcodedRuntimeData()
    {
        EnsureInitialized();
        RefreshAvailableSides();

        foreach (var chapter in Chapters.OrderBy(ch => ch.Number))
        {
            try
            {
                var area = AreaData.Get(chapter.SID);
                if (area == null)
                    continue;

                ApplyHardcodedRuntimeData(area, chapter);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"Skipped hardcoded chapter data for '{chapter?.SID ?? "<null>"}' due to {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    public static void ApplyHardcodedRuntimeData(AreaData area)
    {
        if (area == null)
            return;

        if (!AreaModeExtender.IsOurMap(area))
            return;

        EnsureInitialized();
        var chapter = FindByAnySID(area.SID);
        if (chapter == null)
            return;

        try
        {
            ApplyHardcodedRuntimeData(area, chapter);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"Failed applying hardcoded chapter data to '{area.SID}': {ex.GetType().Name}: {ex.Message}");
        }
    }

    public static void RefreshChapterIcon(string sid)
    {
        if (string.IsNullOrWhiteSpace(sid))
            return;

        EnsureInitialized();

        try
        {
            var area = AreaData.Get(sid);
            if (area != null)
                ApplyHardcodedRuntimeData(area);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"RefreshChapterIcon skipped for '{sid}' due to {ex.GetType().Name}: {ex.Message}");
        }
    }

    public static string ResolveChapterIconPath(ChapterDef chapter)
    {
        if (chapter == null)
            return null;

        if (ChapterProgressionManager.IsChapterLockedForUI(chapter.SID) && HasGuiTexture(LockedGuiAreaIcon))
            return LockedGuiAreaIcon;

        if (GuiIconNameByChapter.TryGetValue(chapter.Number, out string iconName))
        {
            string guiIcon = $"{GuiAreaIconRoot}/{iconName}";
            if (HasGuiTexture(guiIcon))
                return guiIcon;
        }

        return chapter.Icon;
    }

    // ── Registration Helpers ─────────────────────────────────────────────

    /// <summary>
    /// Registers a standard chapter with all 5 sides (A/B/C/D/DX).
    /// </summary>
    private static void RegisterStandardChapter(
        int number, string name, string chapterKey, string icon, int mountainState,
        (Vector3 pos, Vector3 target) idle,
        (Vector3 pos, Vector3 target) select,
        (Vector3 pos, Vector3 target) zoom,
        Vector3 cursor)
    {
        string numStr = number.ToString("D2");
        string mainMusic = $"event:/pusheen/music/lvl{numStr}/main";
        string ambience = GetStandardAmbienceEvent(number);

        Register(new ChapterDef
        {
            Number = number,
            Name = name,
            SID = AreaModeExtender.BuildASideSID(chapterKey),
            Icon = icon,
            IsInterlude = false,
            HasBSide = true,
            HasCSide = true,
            HasDSide = false,
            HasDXSide = false,
            MusicEvents = new[]
            {
                mainMusic,
                mainMusic,
                mainMusic,
                mainMusic,
                mainMusic,
            },
            AmbienceEvents = new[]
            {
                ambience,
                ambience,
                ambience,
                ambience,
                ambience,
            },
            CassetteSong = mainMusic,
            MountainState = mountainState,
            MountainData = new MountainCameraData
            {
                IdlePos = idle.pos,
                IdleTarget = idle.target,
                SelectPos = select.pos,
                SelectTarget = select.target,
                ZoomPos = zoom.pos,
                ZoomTarget = zoom.target,
                Cursor = cursor
            }
        });
    }

    private static string GetStandardAmbienceEvent(int chapterNumber)
    {
        return chapterNumber switch
        {
            2 => "event:/pusheen/env/02_awake",
            4 => "event:/pusheen/env/04_awake",
            5 => "event:/pusheen/env/05_exterior",
            7 => "event:/pusheen/env/07_interior_main",
            8 => "event:/pusheen/env/08_main",
            9 => "event:/pusheen/env/09_summit",
            10 => "event:/pusheen/env/10_ruins",
            11 => "event:/pusheen/env/11_snow_daytime",
            12 => "event:/pusheen/env/12_waterfall",
            13 => "event:/pusheen/env/13_factory",
            14 => "event:/pusheen/env/14_digital",
            15 => "event:/pusheen/env/15_castle",
            18 => "event:/pusheen/env/18_main",
            _ => $"event:/pusheen/env/{chapterNumber:D2}_main"
        };
    }

    public static void RefreshAvailableSides()
    {
        EnsureInitialized();
        foreach (ChapterDef chapter in Chapters)
        {
            string baseKey = ExtractBaseKey(chapter.SID);
            if (string.IsNullOrEmpty(baseKey))
                continue;

            chapter.HasBSide = HasLoadedSide(baseKey, AreaModeExtender.MODE_BSIDE);
            chapter.HasCSide = HasLoadedSide(baseKey, AreaModeExtender.MODE_CSIDE);
            chapter.HasDSide = HasLoadedSide(baseKey, AreaModeExtender.MODE_DSIDE);
            chapter.HasDXSide = HasLoadedSide(baseKey, AreaModeExtender.MODE_DXSIDE);
        }
    }

    private static bool HasLoadedSide(string baseKey, int modeIndex)
    {
        try
        {
            return AreaData.Get(AreaModeExtender.BuildSideSID(modeIndex, baseKey)) != null;
        }
        catch
        {
            return false;
        }
    }

    private static void Register(ChapterDef chapter)
    {
        Chapters.Add(chapter);
        _byNumber[chapter.Number] = chapter;
        _bySID[chapter.SID] = chapter;

        string baseKey = ExtractBaseKey(chapter.SID);
        if (!string.IsNullOrEmpty(baseKey))
            _byBaseKey[baseKey] = chapter;
    }

    private static void EnsureModeArray(AreaData area)
    {
        int target = AreaModeExtender.TOTAL_MODES;
        var old = area.Mode ?? Array.Empty<ModeProperties>();
        if (old.Length >= target)
            return;

        var resized = new ModeProperties[target];
        for (int i = 0; i < old.Length; i++)
            resized[i] = old[i];

        area.Mode = resized;
    }

    private static void ApplyModes(AreaData area, ChapterDef chapter)
    {
        string baseKey = ExtractBaseKey(chapter.SID);
        if (string.IsNullOrEmpty(baseKey))
            return;

        area.Mode[0] = BuildOrUpdateMode(area.Mode[0], chapter.SID,
            GetMusic(chapter, 0), GetAmbience(chapter, 0));

        area.Mode[1] = chapter.HasBSide
            ? BuildOrUpdateMode(area.Mode[1], AreaModeExtender.BuildSideSID(AreaModeExtender.MODE_BSIDE, baseKey), GetMusic(chapter, 1), GetAmbience(chapter, 1))
            : null;

        area.Mode[2] = chapter.HasCSide
            ? BuildOrUpdateMode(area.Mode[2], AreaModeExtender.BuildSideSID(AreaModeExtender.MODE_CSIDE, baseKey), GetMusic(chapter, 2), GetAmbience(chapter, 2))
            : null;

        area.Mode[3] = chapter.HasDSide
            ? BuildOrUpdateMode(area.Mode[3], AreaModeExtender.BuildSideSID(AreaModeExtender.MODE_DSIDE, baseKey), GetMusic(chapter, 3), GetAmbience(chapter, 3))
            : null;

        area.Mode[4] = chapter.HasDXSide
            ? BuildOrUpdateMode(area.Mode[4], AreaModeExtender.BuildSideSID(AreaModeExtender.MODE_DXSIDE, baseKey), GetMusic(chapter, 4), GetAmbience(chapter, 4))
            : null;

        if (!string.IsNullOrEmpty(chapter.CassetteSong))
            area.CassetteSong = chapter.CassetteSong;

        // Everest registers each .bin sidecar as its own standalone AreaData, so
        // Mode[1] (B-Side) and Mode[2] (C-Side) come back from BuildOrUpdateMode
        // with MapData == null when the slot was previously empty.
        // Session.orig_ctor reads Mode[modeIndex].MapData and crashes on null,
        // so load MapData now for any non-null mode that still lacks it.
        for (int mi = 1; mi < area.Mode.Length; mi++)
        {
            if (area.Mode[mi] == null || area.Mode[mi].MapData != null)
                continue;
            try
            {
                var key = new AreaKey(area.ID, (global::Celeste.AreaMode)mi);
                area.Mode[mi].MapData = new MapData(key);
                Logger.Log(LogLevel.Verbose, "MaggyHelper",
                    $"ApplyModes: loaded MapData for {area.SID} mode {mi} ({area.Mode[mi].Path})");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper",
                    $"ApplyModes: could not load MapData for {area.SID} mode {mi} (path: {area.Mode[mi].Path}): {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                // Null out the slot so HasMode() returns false and no session will be constructed.
                area.Mode[mi] = null;
            }
        }
    }

    private static void EnsureASideMode(AreaData area, ChapterDef chapter)
    {
        var old = area.Mode ?? Array.Empty<ModeProperties>();
        if (old.Length == 0)
            area.Mode = new ModeProperties[1];

        area.Mode[0] = BuildOrUpdateMode(area.Mode[0], chapter.SID,
            GetMusic(chapter, 0), GetAmbience(chapter, 0));

        if (!string.IsNullOrEmpty(chapter.CassetteSong))
            area.CassetteSong = chapter.CassetteSong;
    }

    private static ModeProperties BuildOrUpdateMode(ModeProperties existing, string path, string music, string ambience)
    {
        var mode = existing ?? new ModeProperties
        {
            Inventory = PlayerInventory.Default,
            Checkpoints = null
        };

        mode.Path = path;

        if (!string.IsNullOrEmpty(music) || !string.IsNullOrEmpty(ambience))
        {
            mode.AudioState = new AudioState(
                string.IsNullOrEmpty(music) ? "event:/music/lvl1/main" : music,
                string.IsNullOrEmpty(ambience) ? "event:/env/amb/00_prologue" : ambience
            );
        }

        return mode;
    }

    private static string GetMusic(ChapterDef chapter, int index)
    {
        if (chapter.MusicEvents == null || chapter.MusicEvents.Length == 0)
            return null;

        if (index < chapter.MusicEvents.Length)
            return chapter.MusicEvents[index];

        return chapter.MusicEvents[chapter.MusicEvents.Length - 1];
    }

    private static string GetAmbience(ChapterDef chapter, int index)
    {
        if (chapter.AmbienceEvents == null || chapter.AmbienceEvents.Length == 0)
            return null;

        if (index < chapter.AmbienceEvents.Length)
            return chapter.AmbienceEvents[index];

        return chapter.AmbienceEvents[chapter.AmbienceEvents.Length - 1];
    }

    // ── Lookup Methods ───────────────────────────────────────────────────

    /// <summary>Gets a chapter by its number</summary>
    public static ChapterDef GetByNumber(int number)
    {
        EnsureInitialized();
        return _byNumber.TryGetValue(number, out var ch) ? ch : null;
    }

    /// <summary>Gets a chapter by its SID</summary>
    public static ChapterDef GetBySID(string sid)
    {
        EnsureInitialized();
        return _bySID.TryGetValue(sid, out var ch) ? ch : null;
    }

    /// <summary>Gets a chapter by matching any of its side SIDs</summary>
    public static ChapterDef FindByAnySID(string sid)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(sid))
            return null;

        if (!sid.StartsWith(AreaModeExtender.MAP_PREFIX + "/", StringComparison.OrdinalIgnoreCase))
            return null;

        // Direct match
        if (_bySID.TryGetValue(sid, out var ch))
            return ch;

        string baseKey = ExtractBaseKey(sid);
        if (!string.IsNullOrEmpty(baseKey) && _byBaseKey.TryGetValue(baseKey, out ch))
            return ch;

        return null;
    }

    /// <summary>Gets the total number of chapters with alt-sides</summary>
    public static int GetAltSideChapterCount()
    {
        EnsureInitialized();
        return Chapters.Count(c => c.HasBSide);
    }

    private static string ExtractBaseKey(string sid)
    {
        if (string.IsNullOrEmpty(sid)) return null;
        var parts = sid.Split('/');
        if (parts.Length < 3) return null;
        return parts[^1];
    }

    private static void ApplyHardcodedRuntimeData(AreaData area, ChapterDef chapter)
    {
        area.Name = chapter.Name;
        area.Icon = ResolveChapterIconPath(chapter);
        area.Interlude_Safe = chapter.IsInterlude;

        // Build and apply a vanilla MapMeta so the Everest meta pipeline is
        // exercised for overworld cameras, fog, audio state, and mode properties.
        // This makes DZ chapters behave identically to vanilla/Everest-modded chapters.
        MapMeta meta = BuildMapMeta(chapter, area);
        meta.ApplyTo(area);

        // Hard-override mountain cameras and state after ApplyTo so our code-defined
        // positions always win over anything already stored in area.Meta at load time.
        if (chapter.MountainData != null)
        {
            area.MountainCursor = chapter.MountainData.Cursor;
            area.MountainIdle   = new MountainCamera(chapter.MountainData.IdlePos,   chapter.MountainData.IdleTarget);
            area.MountainSelect = new MountainCamera(chapter.MountainData.SelectPos, chapter.MountainData.SelectTarget);
            area.MountainZoom   = new MountainCamera(chapter.MountainData.ZoomPos,   chapter.MountainData.ZoomTarget);
        }
        area.MountainState = chapter.MountainState;

        bool hasAltSides = chapter.HasBSide || chapter.HasCSide || chapter.HasDSide || chapter.HasDXSide;
        if (hasAltSides)
        {
            Logger.Log(LogLevel.Verbose, "MaggyHelper",
                $"ApplyHardcodedRuntimeData: '{area.SID}' has alt-sides (B={chapter.HasBSide}, C={chapter.HasCSide}, D={chapter.HasDSide}, DX={chapter.HasDXSide})");
            EnsureModeArray(area);
            ApplyModes(area, chapter);
        }
        else
        {
            Logger.Log(LogLevel.Verbose, "MaggyHelper",
                $"ApplyHardcodedRuntimeData: '{area.SID}' is A-Side only");
            EnsureASideMode(area, chapter);
        }
    }

    /// <summary>
    /// Constructs a vanilla <see cref="MapMeta"/> from a <see cref="ChapterDef"/> so that
    /// Everest's standard <c>MapMeta.ApplyTo</c> pipeline is exercised for this chapter.
    /// This covers mountain cameras, fog/star colors, audio state, and mode properties —
    /// matching exactly how vanilla Celeste chapters and Everest YAML mods work.
    /// </summary>
    private static MapMeta BuildMapMeta(ChapterDef chapter, AreaData area)
    {
        // ── Mountain (overworld 3-D model / cameras / fog) ────────────────
        MapMetaMountain mountain = null;
        if (chapter.MountainData != null)
        {
            mountain = new MapMetaMountain
            {
                State = chapter.MountainState,
                ShowSnow = true,

                Idle = new MapMetaMountainCamera
                {
                    Position = new[] { chapter.MountainData.IdlePos.X,   chapter.MountainData.IdlePos.Y,   chapter.MountainData.IdlePos.Z },
                    Target   = new[] { chapter.MountainData.IdleTarget.X, chapter.MountainData.IdleTarget.Y, chapter.MountainData.IdleTarget.Z }
                },
                Select = new MapMetaMountainCamera
                {
                    Position = new[] { chapter.MountainData.SelectPos.X,   chapter.MountainData.SelectPos.Y,   chapter.MountainData.SelectPos.Z },
                    Target   = new[] { chapter.MountainData.SelectTarget.X, chapter.MountainData.SelectTarget.Y, chapter.MountainData.SelectTarget.Z }
                },
                Zoom = new MapMetaMountainCamera
                {
                    Position = new[] { chapter.MountainData.ZoomPos.X,   chapter.MountainData.ZoomPos.Y,   chapter.MountainData.ZoomPos.Z },
                    Target   = new[] { chapter.MountainData.ZoomTarget.X, chapter.MountainData.ZoomTarget.Y, chapter.MountainData.ZoomTarget.Z }
                },

                Cursor = new[] { chapter.MountainData.Cursor.X, chapter.MountainData.Cursor.Y, chapter.MountainData.Cursor.Z },

                // DZ fog palette — deep space/indigo theme
                FogColors = new[]
                {
                    "0d0a1f", // normal
                    "050310", // dark
                    "040d1a", // void
                    "0d0a1f"  // summit
                },
                StarFogColor    = "040d1a",
                StarStreamColors = new[] { "000000", "9228e2", "30ffff" },
                StarBeltColors1  = new[] { "ffb8e6", "c8b8ff" },
                StarBeltColors2  = new[] { "80ffff", "b8ffff" },

                BackgroundMusic   = chapter.MusicEvents?.Length > 0 ? chapter.MusicEvents[0] : null,
                BackgroundAmbience = chapter.AmbienceEvents?.Length > 0 ? chapter.AmbienceEvents[0] : null,
            };
        }

        return new MapMeta
        {
            Interlude         = chapter.IsInterlude,
            Dreaming          = false,
            CassetteSong      = chapter.CassetteSong,
            Mountain          = mountain,
            OverrideASideMeta = false,
        };
    }

    private static bool HasGuiTexture(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && GFX.Gui != null && GFX.Gui.Has(path);
    }
}

/// <summary>
/// Hooks into the LevelEnter flow to show VHS intro remix cutscenes
/// when entering B-Side or C-Side levels for the first time.
/// </summary>
public static class IntroRemixHooks
{
    private static bool _hooked = false;

    /// <summary>
    /// Hookable delegate for D-Side chapter entry.
    /// Subscribe to this to customize D-Side intro behavior.
    /// Return true to skip default entry (handled by subscriber).
    /// </summary>
    public delegate bool DSideEnterHandler(Session session);

    /// <summary>
    /// Event invoked when entering a D-Side chapter.
    /// Hook this from anywhere in AreaMapData or other classes to customize entry.
    /// </summary>
    public static event DSideEnterHandler OnDSideEnter;

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.LevelEnter.Go += OnLevelEnterGo;

        Logger.Log(LogLevel.Info, "MaggyHelper", "IntroRemixHooks loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.LevelEnter.Go -= OnLevelEnterGo;

        Logger.Log(LogLevel.Info, "MaggyHelper", "IntroRemixHooks unloaded");
    }

    /// <summary>
    /// Intercepts level entry to show VHS remix intros for B-Side and C-Side.
    /// </summary>
    private static void OnLevelEnterGo(On.Celeste.LevelEnter.orig_Go orig,
        Session session, bool fromSaveData)
    {
        if (fromSaveData || !session.StartedFromBeginning)
        {
            orig(session, fromSaveData);
            return;
        }

        var area = AreaData.Get(session.Area);
        if (!AreaModeExtender.IsOurMap(area))
        {
            orig(session, fromSaveData);
            return;
        }

        int mode = (int)session.Area.Mode;

        // Check if this side's meta.yaml has ShowBSideRemixIntro set
        // or if we should show it based on mode
        switch (mode)
        {
            case AreaModeExtender.MODE_BSIDE:
                // Show VHS B-Side intro remix
                if (ShouldShowRemixIntro(session, mode))
                {
                    Engine.Scene = new CS_Gen_IntroRemix_BSide(session);
                    return;
                }
                break;

            case AreaModeExtender.MODE_CSIDE:
                // Show VHS C-Side intro remix (more damaged/corrupted)
                if (ShouldShowRemixIntro(session, mode))
                {
                    Engine.Scene = new CS_Gen_IntroRemix_CSide(session);
                    return;
                }
                break;
            case AreaModeExtender.MODE_DSIDE:
                // D-Side: Invoke hookable event, fall through to default if not handled
                if (OnDSideEnter != null)
                {
                    bool handled = false;
                    foreach (DSideEnterHandler handler in OnDSideEnter.GetInvocationList())
                    {
                        if (handler(session))
                        {
                            handled = true;
                            break;
                        }
                    }
                    if (handled)
                    {
                        return; // Custom handler took over
                    }
                }
                // No intro, quick entry to chapter map
                break;
        }

        orig(session, fromSaveData);
    }

    /// <summary>
    /// Determines if the VHS remix intro should be shown.
    /// Shows on first entry or if the player hasn't seen it before.
    /// </summary>
    private static bool ShouldShowRemixIntro(Session session, int mode)
    {
        // Check if user has already seen this intro
        string flagKey = $"seen_remix_intro_{session.Area.SID}_{mode}";
        bool alreadySeen = MaggyHelperModule.SaveData?.HasAchievement(flagKey) == true;

        if (!alreadySeen)
        {
            // Mark as seen for next time
            MaggyHelperModule.SaveData?.UnlockAchievement(flagKey);
            return true;
        }

        return false;
    }
}
