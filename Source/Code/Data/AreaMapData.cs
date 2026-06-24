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
        [20] = "theend",
        [21] = "lastlevel"
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

        /// <summary>Optional intro vignette factory for chapter intro</summary>
        public Func<Session, HiresSnow, Scene> VignetteHooks { get; set; }

        /// <summary>Optional true finale vignette factory for chapter outro</summary>
        public Func<Session, Scene> TrueFinaleVignette { get; set; }

        /// <summary>Optional postcard vignette factory for chapter completion</summary>
        public Func<Session, Scene> PostcardMaggy { get; set; }
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

        ChapterRegistry.RegisterAllChapters(Chapters);
    }

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
            return "areas/Maggy/lock";

        if (ChapterProgressionManager.IsChapterLockedForUI(chapter.SID) && HasGuiTexture(LockedGuiAreaIcon))
            return LockedGuiAreaIcon;

        if (GuiIconNameByChapter.TryGetValue(chapter.Number, out string iconName))
        {
            string guiIcon = $"{GuiAreaIconRoot}/{iconName}";
            if (HasGuiTexture(guiIcon))
                return guiIcon;
        }

        return !string.IsNullOrWhiteSpace(chapter.Icon) ? chapter.Icon : "areas/Maggy/lock";
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
            string finalMusic = music;
            string finalAmbience = string.IsNullOrEmpty(ambience) ? "event:/env/amb/00_prologue" : ambience;

            Logger.Log(LogLevel.Debug, "MaggyHelper",
                $"BuildOrUpdateMode: Setting audio for {path} - Music: {finalMusic}, Ambience: {finalAmbience}");

            mode.AudioState = new AudioState(finalMusic, finalAmbience);
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
        Logger.Log(LogLevel.Debug, "MaggyHelper",
            $"ApplyHardcodedRuntimeData: Chapter {chapter.Number} ({chapter.Name}), SID: {area.SID}");

        area.Name = chapter.Name;
        area.Icon = ResolveChapterIconPath(chapter);
        area.Interlude_Safe = chapter.IsInterlude;

        // Build and apply a vanilla MapMeta so the Everest meta pipeline is
        // exercised for overworld cameras, fog, audio state, and mode properties.
        // This makes DZ chapters behave identically to vanilla/Everest-modded chapters.
        MapMeta meta = BuildMapMeta(chapter, area);

        Logger.Log(LogLevel.Debug, "MaggyHelper",
            $"  Music Events: {(chapter.MusicEvents?.Length > 0 ? string.Join(", ", chapter.MusicEvents) : "none")}");
        Logger.Log(LogLevel.Debug, "MaggyHelper",
            $"  Ambience Events: {(chapter.AmbienceEvents?.Length > 0 ? string.Join(", ", chapter.AmbienceEvents) : "none")}");

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

