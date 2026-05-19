using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

/// <summary>
/// Integrates AreaMapData's mountain camera positions with Celeste's 3D overworld.
/// Hooks into MountainRenderer and OuiChapterSelect to apply per-chapter camera 
/// positions and mountain state changes (e.g., darkened mountain for late-game chapters).
///
/// Works with the existing OverworldConnector entity (which handles primary/secondary
/// renderers, Maggy3D marker, and VoidMoon) by providing chapter-specific camera data.
///
/// The DZ mountain is designed for a specific front-facing viewing angle, so the
/// camera is locked (no idle rotation) once the chapter-switch ease completes.
/// </summary>
public static class MountainOverworldManager
{
    private static bool _hooked = false;

    // Mountain texture states
    public const int STATE_NORMAL = 0;
    public const int STATE_DARK = 1;
    public const int STATE_VOID = 2;

    // ── Custom Mountain Models ────────────────────────────────────────────

    /// <summary>Base path for Desolo Zantas mountain OBJ models.</summary>
    private const string MOUNTAIN_MODEL_DIR = "Mountain/Maggy/Desolo_Zantas";

    /// <summary>Whether custom mountain models have been registered.</summary>
    private static bool _modelsRegistered;

    // ── Camera Lock State ────────────────────────────────────────────────
    // Prevents the mountain's idle camera rotation from drifting away from
    // the designed viewing angle for DZ chapters.

    /// <summary>Last DZ area ID the player was viewing (or -1 if non-DZ).</summary>
    private static int _lastDZArea = -1;

    /// <summary>Countdown (seconds) during which the chapter-switch ease is allowed to play.</summary>
    private static float _easeCountdown = 0f;

    /// <summary>How long to allow the camera ease transition before locking.</summary>
    private const float EASE_WINDOW = 1.6f;

    // ── Hook Management ──────────────────────────────────────────────────

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        // Hook Mountain.MountainCamera property sets for each chapter
        On.Celeste.Overworld.SetNormalMusic += OnOverworldSetNormalMusic;
        On.Celeste.OuiChapterSelect.Update += OnChapterSelectUpdate;
        On.Celeste.AreaData.Load += OnAreaDataLoad;

        Logger.Log(LogLevel.Info, "MaggyHelper", "MountainOverworldManager loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.Overworld.SetNormalMusic -= OnOverworldSetNormalMusic;
        On.Celeste.OuiChapterSelect.Update -= OnChapterSelectUpdate;
        On.Celeste.AreaData.Load -= OnAreaDataLoad;

        _modelsRegistered = false;
        _lastDZArea = -1;
        _easeCountdown = 0f;

        Logger.Log(LogLevel.Info, "MaggyHelper", "MountainOverworldManager unloaded");
    }

    // ── AreaData.Load Hook ───────────────────────────────────────────────

    /// <summary>
    /// After vanilla AreaData loads, inject our mountain data into each chapter's
    /// ModeProperties. This ensures the 3D mountain uses our camera positions.
    /// </summary>
    private static void OnAreaDataLoad(On.Celeste.AreaData.orig_Load orig)
    {
        orig();

        // Register custom mountain models (must happen after AreaData loads so SIDs are known)
        RegisterMountainModels();

        // Apply mountain data to our chapters
        ApplyMountainCameraData();

    }

    /// <summary>
    /// Sets mountain camera positions for each of our chapters by updating
    /// the AreaData.MountainCursor, MountainIdle, MountainSelect, MountainZoom vectors.
    /// </summary>
    private static void ApplyMountainCameraData()
    {
        // Ensure AreaMapData is initialized before accessing its data
        AreaMapData.Initialize();

        if (AreaMapData.Chapters == null || AreaMapData.Chapters.Count == 0)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                "MountainOverworld: AreaMapData has no chapters registered");
            return;
        }

        foreach (var chapter in AreaMapData.Chapters)
        {
            if (chapter.MountainData == null)
                continue;

            // Find the matching AreaData entry
            AreaData areaData = FindAreaData(chapter.SID);
            if (areaData == null)
                continue;

            try
            {
                // Set mountain camera positions
                areaData.MountainCursor = chapter.MountainData.Cursor;
                areaData.MountainIdle = new MountainCamera(
                    chapter.MountainData.IdlePos,
                    chapter.MountainData.IdleTarget
                );
                areaData.MountainSelect = new MountainCamera(
                    chapter.MountainData.SelectPos,
                    chapter.MountainData.SelectTarget
                );
                areaData.MountainZoom = new MountainCamera(
                    chapter.MountainData.ZoomPos,
                    chapter.MountainData.ZoomTarget
                );

                // Set mountain state (normal, dark, etc.)
                areaData.MountainState = chapter.MountainState;

                Logger.Log(LogLevel.Debug, "MaggyHelper",
                    $"MountainOverworld: Applied camera data for Ch.{chapter.Number} ({chapter.Name})");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"MountainOverworld: Failed to apply camera for Ch.{chapter.Number}: {ex.Message}");
            }
        }

        Logger.Log(LogLevel.Info, "MaggyHelper",
            $"MountainOverworld: Applied mountain data for {AreaMapData.Chapters.Count} chapters");
    }

    // ── Custom Mountain Model Registration ────────────────────────────────

    /// <summary>
    /// Registers the Desolo Zantas OBJ mountain models for every MaggyHelper chapter
    /// via MTNExt.MountainMappings. The meta.yaml MountainModelDirectory approach cannot
    /// be used because it requires textures named mountain_0/mountain_1/buildings_0 etc.
    /// in the MTN.Mountain atlas, which this mod's assets don't follow.
    /// </summary>
    private static void RegisterMountainModels()
    {
        if (_modelsRegistered) return;
        if (AreaData.Areas == null || AreaData.Areas.Count == 0) return;

        try
        {
            ObjModel terrain = TryLoadObjModel(MOUNTAIN_MODEL_DIR, "building")
                            ?? TryLoadObjModel(MOUNTAIN_MODEL_DIR, "mountain");
            ObjModel wall    = TryLoadObjModel(MOUNTAIN_MODEL_DIR, "mountain_wall");
            ObjModel bird    = TryLoadObjModel(MOUNTAIN_MODEL_DIR, "bird");
            ObjModel moon    = TryLoadObjModel(MOUNTAIN_MODEL_DIR, "moon");
            ObjModel buildings = TryLoadObjModel(MOUNTAIN_MODEL_DIR, "buildings");

            if (terrain == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    "MountainOverworld: building.obj / mountain.obj not found — skipping model registration");
                return;
            }

            var resources = new MountainResources
            {
                MountainTerrain    = terrain,
                MountainBuildings  = buildings,
                MountainCoreWall   = wall,
                MountainBird       = bird,
                MountainMoon       = moon
            };

            int registered = 0;
            // Iterate AreaData.Areas directly — BeforeRender uses area.GetSID() as the key,
            // which comes from meta.yaml SID field, not from AreaMapData.Chapters.
            foreach (var areaData in AreaData.Areas)
            {
                if (!AreaModeExtender.IsOurMap(areaData)) continue;
                if (string.IsNullOrEmpty(areaData.SID)) continue;

                // MountainModel.BeforeRender looks up Path.Combine("Maps", area.GetSID())
                string key = $"Maps/{areaData.SID}";

                if (!MTNExt.MountainMappings.ContainsKey(key))
                {
                    MTNExt.MountainMappings[key] = resources;
                    registered++;
                }
            }

            _modelsRegistered = true;
            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"MountainOverworld: Registered custom mountain models for {registered} chapters");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"MountainOverworld: Failed to register mountain models: {ex}");
        }
    }

    /// <summary>Attempts to load an ObjModel; returns null if not found.</summary>
    private static ObjModel TryLoadObjModel(string dir, string name)
    {
        string path = $"{dir}/{name}";
        if (Everest.Content.TryGet(path, out _))
        {
            var model = ObjModel.Create(path);
            Logger.Log(LogLevel.Debug, "MaggyHelper", $"MountainOverworld: Loaded {name}.obj");
            return model;
        }
        Logger.Log(LogLevel.Debug, "MaggyHelper", $"MountainOverworld: {name}.obj not found at {path}");
        return null;
    }

    // ── Overworld Music Hook ─────────────────────────────────────────────

    /// <summary>
    /// Replaces the overworld's "normal" music with our custom level select music
    /// when the player is viewing MaggyHelper chapters.
    /// </summary>
    private static void OnOverworldSetNormalMusic(On.Celeste.Overworld.orig_SetNormalMusic orig,
        Overworld self)
    {
        // Check if we're looking at our chapters
        if (IsViewingOurChapters())
        {
            Audio.SetMusic(OverworldMusicManager.MUSIC_LEVEL_SELECT);
            return;
        }

        orig(self);
    }

    // ── Chapter Select Updates ───────────────────────────────────────────

    /// <summary>
    /// During chapter select updates, handle camera transitions and prevent
    /// the idle rotation from drifting the DZ mountain's designed viewing angle.
    /// </summary>
    private static void OnChapterSelectUpdate(On.Celeste.OuiChapterSelect.orig_Update orig,
        OuiChapterSelect self)
    {
        orig(self);

        try
        {
            var overworld = self.Scene as Overworld;
            if (overworld == null) return;

            int selectedArea = SaveData.Instance?.LastArea_Safe.ID ?? -1;
            if (selectedArea < 0 || selectedArea >= AreaData.Areas.Count)
                return;

            var areaData = AreaData.Get(selectedArea);
            if (areaData == null || !AreaModeExtender.IsOurMap(areaData))
            {
                _lastDZArea = -1;
                return;
            }

            var mountain = overworld.Mountain;
            if (mountain == null) return;

            // Detect chapter change → start a one-shot ease transition
            if (selectedArea != _lastDZArea)
            {
                _lastDZArea = selectedArea;
                _easeCountdown = EASE_WINDOW;
                mountain.EaseCamera(selectedArea, areaData.MountainSelect, null, false);
            }

            // Count down the ease window
            if (_easeCountdown > 0f)
            {
                _easeCountdown -= Engine.DeltaTime;
            }
            else
            {
                // Ease complete → snap camera each frame to prevent idle rotation.
                // This keeps the DZ mountain at its designed front-facing angle.
                mountain.SnapCamera(selectedArea, areaData.MountainSelect);
            }
        }
        catch
        {
            // Mountain update is non-critical
        }
    }

    // ── Utility ──────────────────────────────────────────────────────────

    /// <summary>
    /// Checks if the player is currently viewing a MaggyHelper chapter.
    /// </summary>
    private static bool IsViewingOurChapters()
    {
        try
        {
            if (SaveData.Instance == null) return false;

            int area = SaveData.Instance.LastArea_Safe.ID;
            if (area >= 0 && area < AreaData.Areas.Count)
            {
                return AreaModeExtender.IsOurMap(AreaData.Get(area));
            }
        }
        catch { }
        return false;
    }

    /// <summary>
    /// Finds the AreaData for a given SID, searching through all registered areas.
    /// </summary>
    private static AreaData FindAreaData(string sid)
    {
        if (string.IsNullOrEmpty(sid)) return null;

        // First try direct SID match
        for (int i = 0; i < AreaData.Areas.Count; i++)
        {
            var area = AreaData.Areas[i];
            if (string.Equals(area.SID, sid, StringComparison.OrdinalIgnoreCase))
                return area;
        }

        // Try partial match (chapter key)
        string baseKey = sid.Split('/').LastOrDefault();
        if (baseKey == null) return null;

        // Remove side suffix (_A, _B, _C)
        if (baseKey.Length > 2 && baseKey[^2] == '_')
            baseKey = baseKey[..^2];

        for (int i = 0; i < AreaData.Areas.Count; i++)
        {
            var area = AreaData.Areas[i];
            if (area.SID != null && area.SID.Contains(baseKey, StringComparison.OrdinalIgnoreCase))
                return area;
        }

        return null;
    }

    // ── Public API ───────────────────────────────────────────────────────

    /// <summary>
    /// Gets the mountain camera data for a specific chapter number.
    /// </summary>
    public static AreaMapData.MountainCameraData GetCameraForChapter(int chapterNumber)
    {
        return AreaMapData.GetByNumber(chapterNumber)?.MountainData;
    }

    /// <summary>
    /// Gets the mountain state for a specific chapter.
    /// </summary>
    public static int GetMountainStateForChapter(int chapterNumber)
    {
        return AreaMapData.GetByNumber(chapterNumber)?.MountainState ?? STATE_NORMAL;
    }
}

