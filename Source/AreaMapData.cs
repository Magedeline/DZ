using System.Collections.Generic;
using MaggyHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper;

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

        /// <summary>3D overworld state index (0=normal, 1=dark, etc.)</summary>
        public int MountainState { get; set; }
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

    // ── Initialization ───────────────────────────────────────────────────

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
        Register(new ChapterDef
        {
            Number = 0,
            Name = "prolouge",
            SID = AreaModeExtender.BuildASideSID("00_Prologue"),
            Icon = "areas/prologue",
            IsInterlude = true,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/desolozantas/music/lvl0/intro" },
            AmbienceEvents = new[] { "event:/desolozantas/env/00_prologue" },
            MountainState = 0,
            MountainData = new MountainCameraData
            {
                IdlePos = new Vector3(0.658f, 2.872f, 7.876f),
                IdleTarget = new Vector3(0.658f, 2.572f, 6.876f),
                SelectPos = new Vector3(0.658f, 2.572f, 7.876f),
                SelectTarget = new Vector3(0.658f, 2.272f, 6.376f),
                ZoomPos = new Vector3(0.658f, 2.372f, 6.876f),
                ZoomTarget = new Vector3(0.658f, 2.072f, 5.876f),
                Cursor = new Vector3(0.658f, 2.872f, 7.876f)
            }
        });

        // ── Chapter 1: Forbidden Metropolis ──
        RegisterStandardChapter(1, "forbiddenmetro", "01_City",
            "areas/city", 0,
            idle: (new Vector3(2.45f, 6.12f, 14.25f), new Vector3(1.68f, 5.39f, 12.8f)),
            select: (new Vector3(1.85f, 3.75f, 14.58f), new Vector3(2.95f, 3.62f, 13.12f)),
            zoom: (new Vector3(3.28f, 4.15f, 11.65f), new Vector3(3.18f, 3.08f, 9.95f)),
            cursor: new Vector3(3.14f, 3.80f, 9.73f));

        // ── Chapter 2: Veil of Shadows ──
        RegisterStandardChapter(2, "shadowofveil", "02_Nightmare",
            "areas/nightmare", 0,
            idle: (new Vector3(-1.2f, 4.8f, 12.5f), new Vector3(-0.5f, 4.2f, 11.0f)),
            select: (new Vector3(-0.8f, 3.5f, 12.8f), new Vector3(0.2f, 3.3f, 11.3f)),
            zoom: (new Vector3(0.5f, 3.8f, 10.0f), new Vector3(0.4f, 2.9f, 8.3f)),
            cursor: new Vector3(0.4f, 3.5f, 8.0f));

        // ── Chapter 3: Arrival ──
        RegisterStandardChapter(3, "arrivial", "03_Stars",
            "areas/stars", 0,
            idle: (new Vector3(3.5f, 7.2f, 15.0f), new Vector3(2.8f, 6.5f, 13.5f)),
            select: (new Vector3(2.8f, 4.8f, 15.3f), new Vector3(3.8f, 4.7f, 13.8f)),
            zoom: (new Vector3(4.2f, 5.2f, 12.4f), new Vector3(4.1f, 4.1f, 10.7f)),
            cursor: new Vector3(4.0f, 4.8f, 10.5f));

        // ── Chapter 4: Chronicles of Destiny ──
        RegisterStandardChapter(4, "thelegend", "04_Legend",
            "areas/legend", 0,
            idle: (new Vector3(5.2f, 8.5f, 16.0f), new Vector3(4.5f, 7.8f, 14.5f)),
            select: (new Vector3(4.5f, 6.0f, 16.3f), new Vector3(5.5f, 5.9f, 14.8f)),
            zoom: (new Vector3(5.9f, 6.4f, 13.4f), new Vector3(5.8f, 5.3f, 11.7f)),
            cursor: new Vector3(5.7f, 6.0f, 11.5f));

        // ── Chapter 5: Fractured Memories ──
        RegisterStandardChapter(5, "fractureresort", "05_Restore",
            "areas/restore", 0,
            idle: (new Vector3(-2.5f, 5.5f, 13.0f), new Vector3(-1.8f, 4.8f, 11.5f)),
            select: (new Vector3(-2.0f, 4.0f, 13.3f), new Vector3(-1.0f, 3.9f, 11.8f)),
            zoom: (new Vector3(-0.5f, 4.4f, 10.5f), new Vector3(-0.6f, 3.3f, 8.8f)),
            cursor: new Vector3(-0.6f, 4.0f, 8.5f));

        // ── Chapter 6: Fortress of Solitude ──
        RegisterStandardChapter(6, "stronghold", "06_Stronghold",
            "areas/stronghold", 0,
            idle: (new Vector3(6.5f, 9.5f, 17.0f), new Vector3(5.8f, 8.8f, 15.5f)),
            select: (new Vector3(5.8f, 7.0f, 17.3f), new Vector3(6.8f, 6.9f, 15.8f)),
            zoom: (new Vector3(7.2f, 7.4f, 14.4f), new Vector3(7.1f, 6.3f, 12.7f)),
            cursor: new Vector3(7.0f, 7.0f, 12.5f));

        // ── Chapter 7: Infernal Reflections ──
        RegisterStandardChapter(7, "infornoreflection", "07_Hell",
            "areas/hell", 1,
            idle: (new Vector3(-3.5f, 6.5f, 14.0f), new Vector3(-2.8f, 5.8f, 12.5f)),
            select: (new Vector3(-3.0f, 5.0f, 14.3f), new Vector3(-2.0f, 4.9f, 12.8f)),
            zoom: (new Vector3(-1.5f, 5.4f, 11.5f), new Vector3(-1.6f, 4.3f, 9.8f)),
            cursor: new Vector3(-1.6f, 5.0f, 9.5f));

        // ── Chapter 8: Revelation's Edge ──
        RegisterStandardChapter(8, "revelationedge", "08_Truth",
            "areas/truth", 0,
            idle: (new Vector3(7.5f, 10.5f, 18.0f), new Vector3(6.8f, 9.8f, 16.5f)),
            select: (new Vector3(6.8f, 8.0f, 18.3f), new Vector3(7.8f, 7.9f, 16.8f)),
            zoom: (new Vector3(8.2f, 8.4f, 15.4f), new Vector3(8.1f, 7.3f, 13.7f)),
            cursor: new Vector3(8.0f, 8.0f, 13.5f));

        // ── Chapter 9: Apex of Reality (Summit) ──
        RegisterStandardChapter(9, "beyondsummit", "09_Summit",
            "areas/summit", 0,
            idle: (new Vector3(0.0f, 12.0f, 20.0f), new Vector3(0.0f, 11.0f, 18.0f)),
            select: (new Vector3(0.0f, 10.0f, 20.0f), new Vector3(0.0f, 9.5f, 18.5f)),
            zoom: (new Vector3(0.0f, 10.5f, 17.0f), new Vector3(0.0f, 9.5f, 15.0f)),
            cursor: new Vector3(0.0f, 10.0f, 15.0f));

        // ── Chapter 10: Echoes of the Past ──
        // Climb path: base of mountain, ancient ruins biome
        RegisterStandardChapter(10, "echosofpast", "10_Ruins",
            "areas/ruins", 0,
            idle: (new Vector3(-0.5f, 6.2f, 12.0f), new Vector3(-0.5f, 5.5f, 10.5f)),
            select: (new Vector3(-0.5f, 5.8f, 12.3f), new Vector3(-0.5f, 5.5f, 10.8f)),
            zoom: (new Vector3(-0.5f, 5.7f, 10.0f), new Vector3(-0.5f, 5.3f, 8.5f)),
            cursor: new Vector3(-0.5f, 5.5f, 8.5f));

        // ── Chapter 11: Frozen Sanctuary ──
        // Climb path: ascending into snow biome
        RegisterStandardChapter(11, "frozensanctuary", "11_Snow",
            "areas/snow", 0,
            idle: (new Vector3(0.3f, 7.2f, 12.5f), new Vector3(0.3f, 6.5f, 11.0f)),
            select: (new Vector3(0.3f, 6.8f, 12.8f), new Vector3(0.3f, 6.5f, 11.3f)),
            zoom: (new Vector3(0.3f, 6.7f, 10.5f), new Vector3(0.3f, 6.3f, 9.0f)),
            cursor: new Vector3(0.3f, 6.5f, 9.0f));

        // ── Chapter 12: Cascading Depths ──
        // Climb path: waterfall region, mid-mountain
        RegisterStandardChapter(12, "cascadingdepths", "12_Water",
            "areas/water", 0,
            idle: (new Vector3(-0.3f, 8.2f, 13.0f), new Vector3(-0.3f, 7.5f, 11.5f)),
            select: (new Vector3(-0.3f, 7.8f, 13.3f), new Vector3(-0.3f, 7.5f, 11.8f)),
            zoom: (new Vector3(-0.3f, 7.7f, 11.0f), new Vector3(-0.3f, 7.3f, 9.5f)),
            cursor: new Vector3(-0.3f, 7.5f, 9.5f));

        // ── Chapter 13: Blazing Territories ──
        // Climb path: hotland/fire biome, upper-mid mountain
        RegisterStandardChapter(13, "balzingteritory", "13_Fire",
            "areas/fire", 1,
            idle: (new Vector3(0.5f, 9.2f, 13.5f), new Vector3(0.5f, 8.5f, 12.0f)),
            select: (new Vector3(0.5f, 8.8f, 13.8f), new Vector3(0.5f, 8.5f, 12.3f)),
            zoom: (new Vector3(0.5f, 8.7f, 11.5f), new Vector3(0.5f, 8.3f, 10.0f)),
            cursor: new Vector3(0.5f, 8.5f, 10.0f));

        // ── Chapter 14: Cyber Nexus ──
        // Climb path: digital realm, higher up
        RegisterStandardChapter(14, "cybernexus", "14_Digital",
            "areas/digital", 0,
            idle: (new Vector3(-0.2f, 10.2f, 14.0f), new Vector3(-0.2f, 9.5f, 12.5f)),
            select: (new Vector3(-0.2f, 9.8f, 14.3f), new Vector3(-0.2f, 9.5f, 12.8f)),
            zoom: (new Vector3(-0.2f, 9.7f, 12.0f), new Vector3(-0.2f, 9.3f, 10.5f)),
            cursor: new Vector3(-0.2f, 9.5f, 10.5f));

        // ── Chapter 15: Ethereal Citadel ──
        // Climb path: castle biome, near summit
        RegisterStandardChapter(15, "etheraealcitadel", "15_Castle",
            "areas/castle", 0,
            idle: (new Vector3(0.4f, 11.2f, 15.0f), new Vector3(0.4f, 10.5f, 13.5f)),
            select: (new Vector3(0.4f, 10.8f, 15.3f), new Vector3(0.4f, 10.5f, 13.8f)),
            zoom: (new Vector3(0.4f, 10.7f, 13.0f), new Vector3(0.4f, 10.3f, 11.5f)),
            cursor: new Vector3(0.4f, 10.5f, 11.5f));

        // ── Chapter 16: Organ Garden of Despair (A-Side only) ──
        // Climb path: corruption biome, summit approach
        Register(new ChapterDef
        {
            Number = 16,
            Name = "organgarden",
            SID = AreaModeExtender.BuildASideSID("16_Corruption"),
            Icon = "areas/corruption",
            IsInterlude = false,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/desolozantas/music/lvl16/cinematic/intro01" },
            AmbienceEvents = new[] { "event:/desolozantas/env/16_myworld" },
            MountainState = 1,
            MountainData = new MountainCameraData
            {
                IdlePos = new Vector3(0.0f, 12.2f, 15.5f),
                IdleTarget = new Vector3(0.0f, 11.5f, 14.0f),
                SelectPos = new Vector3(0.0f, 11.8f, 15.8f),
                SelectTarget = new Vector3(0.0f, 11.5f, 14.3f),
                ZoomPos = new Vector3(0.0f, 11.7f, 13.5f),
                ZoomTarget = new Vector3(0.0f, 11.3f, 12.0f),
                Cursor = new Vector3(0.0f, 11.5f, 12.0f)
            }
        });

        // ── Chapter 17: Epilogue (A-Side only) ──
        Register(new ChapterDef
        {
            Number = 17,
            Name = "epilouge",
            SID = AreaModeExtender.BuildASideSID("17_Epilogue"),
            Icon = "areas/epilogue",
            IsInterlude = true,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/desolozantas/music/lvl17/main" },
            AmbienceEvents = new[] { "event:/desolozantas/env/00_main" },
            MountainState = 0,
            MountainData = new MountainCameraData
            {
                IdlePos = new Vector3(0.0f, 13.0f, 22.0f),
                IdleTarget = new Vector3(0.0f, 12.0f, 20.0f),
                SelectPos = new Vector3(0.0f, 12.0f, 22.0f),
                SelectTarget = new Vector3(0.0f, 11.5f, 20.5f),
                ZoomPos = new Vector3(0.0f, 12.0f, 19.0f),
                ZoomTarget = new Vector3(0.0f, 11.0f, 17.0f),
                Cursor = new Vector3(0.0f, 12.0f, 17.0f)
            }
        });

        // ── Chapter 18: Core of Existence ──
        RegisterStandardChapter(18, "coreexistence", "18_Heart",
            "areas/heart", 1,
            idle: (new Vector3(0.0f, 14.0f, 24.0f), new Vector3(0.0f, 13.0f, 22.0f)),
            select: (new Vector3(0.0f, 12.0f, 24.0f), new Vector3(0.0f, 11.5f, 22.5f)),
            zoom: (new Vector3(0.0f, 12.5f, 21.0f), new Vector3(0.0f, 11.5f, 19.0f)),
            cursor: new Vector3(0.0f, 12.0f, 19.0f));

        // ── Chapter 19: Farewell to Stars (A-Side only) ──
        Register(new ChapterDef
        {
            Number = 19,
            Name = "farewellstar",
            SID = AreaModeExtender.BuildASideSID("19_Space"),
            Icon = "areas/space",
            IsInterlude = false,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/desolozantas/music/lvl18/main" },
            AmbienceEvents = new[] { "event:/desolozantas/env/18_main" },
            MountainState = 0,
            MountainData = new MountainCameraData
            {
                IdlePos = new Vector3(0.0f, 15.0f, 26.0f),
                IdleTarget = new Vector3(0.0f, 14.0f, 24.0f),
                SelectPos = new Vector3(0.0f, 14.0f, 26.0f),
                SelectTarget = new Vector3(0.0f, 13.5f, 24.5f),
                ZoomPos = new Vector3(0.0f, 14.0f, 23.0f),
                ZoomTarget = new Vector3(0.0f, 13.0f, 21.0f),
                Cursor = new Vector3(0.0f, 14.0f, 21.0f)
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
            MountainState = 1,
            MountainData = new MountainCameraData
            {
                IdlePos = new Vector3(0.0f, 16.0f, 28.0f),
                IdleTarget = new Vector3(0.0f, 15.0f, 26.0f),
                SelectPos = new Vector3(0.0f, 15.0f, 28.0f),
                SelectTarget = new Vector3(0.0f, 14.5f, 26.5f),
                ZoomPos = new Vector3(0.0f, 15.0f, 25.0f),
                ZoomTarget = new Vector3(0.0f, 14.0f, 23.0f),
                Cursor = new Vector3(0.0f, 15.0f, 23.0f)
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
        string mainMusic = $"event:/desolozantas/music/lvl{numStr}/main";
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
            2 => "event:/desolozantas/env/02_awake",
            4 => "event:/desolozantas/env/04_awake",
            5 => "event:/desolozantas/env/05_exterior",
            7 => "event:/desolozantas/env/07_interior_main",
            8 => "event:/desolozantas/env/08_main",
            9 => "event:/desolozantas/env/09_summit",
            10 => "event:/desolozantas/env/10_ruins",
            11 => "event:/desolozantas/env/11_snow_daytime",
            12 => "event:/desolozantas/env/12_waterfall",
            13 => "event:/desolozantas/env/13_factory",
            14 => "event:/desolozantas/env/14_digital",
            15 => "event:/desolozantas/env/15_castle",
            18 => "event:/desolozantas/env/18_main",
            _ => $"event:/desolozantas/env/{chapterNumber:D2}_main"
        };
    }

    public static void RefreshAvailableSides()
    {
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
    public static ChapterDef GetByNumber(int number) =>
        _byNumber.TryGetValue(number, out var ch) ? ch : null;

    /// <summary>Gets a chapter by its SID</summary>
    public static ChapterDef GetBySID(string sid) =>
        _bySID.TryGetValue(sid, out var ch) ? ch : null;

    /// <summary>Gets a chapter by matching any of its side SIDs</summary>
    public static ChapterDef FindByAnySID(string sid)
    {
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
    public static int GetAltSideChapterCount() =>
        Chapters.Count(c => c.HasBSide);

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
        area.MountainState = chapter.MountainState;

        if (chapter.MountainData != null)
        {
            area.MountainCursor = chapter.MountainData.Cursor;
            area.MountainIdle = new MountainCamera(chapter.MountainData.IdlePos, chapter.MountainData.IdleTarget);
            area.MountainSelect = new MountainCamera(chapter.MountainData.SelectPos, chapter.MountainData.SelectTarget);
            area.MountainZoom = new MountainCamera(chapter.MountainData.ZoomPos, chapter.MountainData.ZoomTarget);
        }

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
