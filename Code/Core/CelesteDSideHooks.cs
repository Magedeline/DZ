#pragma warning disable CS0436

using global::Celeste.Mod.Meta;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste;

/// <summary>
/// D-Side hook system for Crystal Heart collection, module initialization, and overworld D-Side management.
/// Extends the AreaModeExtender with additional layer for Celeste mod-specific D-Side interactions.
/// </summary>
public static class CelesteDSideHooks
{
    private static bool _loaded;
    private static Hook _crystalHeartOnCollectHook;
    private static Hook _overworldOnEnterHook;

    public static void Load()
    {
        if (_loaded)
            return;

        _loaded = true;

        // Crystal Heart collection hook for D-Side tracking
        On.Celeste.HeartGem.Collect += OnCrystalHeartCollect;

        // Overworld interaction hooks for D-Side panel management
        On.Celeste.Overworld.Begin += OnOverworldBegin;
        On.Celeste.OuiChapterPanel.Enter += OnChapterPanelEnter;
        On.Celeste.OuiChapterPanel.Leave += OnChapterPanelLeave;

        // Module hooks for D-Side initialization
        On.Celeste.Level.LoadLevel += OnLevelLoadLevel;
        On.Celeste.Level.End += OnLevelEnd;

        // Save data hooks for D-Side stats persistence
        On.Celeste.SaveData.BeforeInitialize += OnSaveDataBeforeInitialize;

        InstallCrystalHeartHook();

        Logger.Log(LogLevel.Info, "MaggyHelper", "CelesteDSideHooks loaded");
    }

    public static void Unload()
    {
        if (!_loaded)
            return;

        _loaded = false;

        On.Celeste.HeartGem.Collect -= OnCrystalHeartCollect;
        On.Celeste.Overworld.Begin -= OnOverworldBegin;
        On.Celeste.OuiChapterPanel.Enter -= OnChapterPanelEnter;
        On.Celeste.OuiChapterPanel.Leave -= OnChapterPanelLeave;
        On.Celeste.Level.LoadLevel -= OnLevelLoadLevel;
        On.Celeste.Level.End -= OnLevelEnd;
        On.Celeste.SaveData.BeforeInitialize -= OnSaveDataBeforeInitialize;

        _crystalHeartOnCollectHook?.Dispose();
        _crystalHeartOnCollectHook = null;

        Logger.Log(LogLevel.Info, "MaggyHelper", "CelesteDSideHooks unloaded");
    }

    private static void InstallCrystalHeartHook()
    {
        if (_crystalHeartOnCollectHook != null)
            return;

        MethodInfo target = typeof(HeartGem).GetMethod(
            "Collect",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
            null,
            new[] { typeof(Player) },
            null);

        if (target == null)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", "Failed to find HeartGem.Collect method for hook installation.");
            return;
        }

        _crystalHeartOnCollectHook = new Hook(target, typeof(CelesteDSideHooks).GetMethod(
            nameof(Hook_HeartGem_Collect),
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
    }

    private delegate void orig_HeartGem_Collect(HeartGem self, Player player);

    private static void Hook_HeartGem_Collect(orig_HeartGem_Collect orig, HeartGem self, Player player)
    {
        orig(self, player);

        Level level = self.Scene as Level;
        if (level == null)
            return;

        AreaData area = AreaData.Get(level.Session.Area);
        if (!AreaModeExtender.IsOurMap(area))
            return;

        int mode = (int)level.Session.Area.Mode;
        if (mode < AreaModeExtender.MODE_DSIDE)
            return;

        // Track D-Side crystal heart collection
        string heartId = $"{area.SID}_{AreaModeExtender.GetModeName(mode)}_crystal";
        MaggyHelperModule.SaveData?.CollectHeartGem(heartId);

        // Play D-Side specific heart collection sound
        if (mode >= 0 && mode < AreaModeExtender.HeartGemGetSounds.Length)
            Audio.Play(AreaModeExtender.HeartGemGetSounds[mode]);

        Logger.Log(LogLevel.Debug, "MaggyHelper", $"D-Side crystal heart collected: {heartId}");
    }

    private static void OnCrystalHeartCollect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player)
    {
        // Pre-collection hook for D-Side state tracking
        Level level = self.Scene as Level;
        if (level != null)
        {
            AreaData area = AreaData.Get(level.Session.Area);
            if (AreaModeExtender.IsOurMap(area))
            {
                int mode = (int)level.Session.Area.Mode;
                if (mode >= AreaModeExtender.MODE_DSIDE)
                {
                    Logger.Log(LogLevel.Debug, "MaggyHelper", $"Collecting heart gem in {AreaModeExtender.GetSideLabel(mode)}-Side: {area.SID}");
                }
            }
        }

        orig(self, player);
    }

    private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
    {
        orig(self);

        // Initialize D-Side specific overworld state
        try
        {
            InitializeDSideOverworldState(self);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to initialize D-Side overworld state: {ex.Message}");
        }
    }

    private static void InitializeDSideOverworldState(Overworld overworld)
    {
        SaveData save = SaveData.Instance;
        if (save == null)
            return;

        // Ensure D-Side chapter panels are properly initialized
        foreach (AreaData area in AreaData.Areas)
        {
            if (!AreaModeExtender.IsOurMap(area))
                continue;

            if (area.Mode == null || area.Mode.Length <= AreaModeExtender.MODE_DSIDE)
                continue;

            // Verify D-Side save stats exist
            object stats = AreaModeExtender.TryGetSaveAreaStats(area.ID);
            if (stats == null)
                continue;

            DynamicData dyn = DynamicData.For(stats);
            var modes = dyn.Get("Modes") as System.Array ?? dyn.Get("modes") as System.Array;

            if (modes == null || modes.Length <= AreaModeExtender.MODE_DSIDE)
            {
                Logger.Log(LogLevel.Debug, "MaggyHelper", $"D-Side stats missing for {area.SID}, creating placeholder");
            }
        }
    }

    private static void OnChapterPanelEnter(On.Celeste.OuiChapterPanel.orig_Enter orig, OuiChapterPanel self, Oui from)
    {
        orig(self, from);

        AreaData area = AreaData.Get(self.Area);
        if (!AreaModeExtender.IsOurMap(area))
            return;

        // Initialize D-Side panel visibility based on unlock state
        try
        {
            InitializeDSidePanelVisibility(self, area);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to initialize D-Side panel visibility: {ex.Message}");
        }
    }

    private static void InitializeDSidePanelVisibility(OuiChapterPanel panel, AreaData area)
    {
        if (area?.Mode == null || area.Mode.Length <= AreaModeExtender.MODE_DSIDE)
            return;

        SaveData save = SaveData.Instance;
        if (save == null)
            return;

        bool dSideUnlocked = AreaModeExtender.IsSideUnlocked(
            new AreaKey(area.ID, (global::Celeste.AreaMode)AreaModeExtender.MODE_DSIDE),
            AreaModeExtender.MODE_DSIDE);

        Logger.Log(LogLevel.Debug, "MaggyHelper",
            $"D-Side panel for {area.SID}: {(dSideUnlocked ? "unlocked" : "locked")}");
    }

    private static void OnChapterPanelLeave(On.Celeste.OuiChapterPanel.orig_Leave orig, OuiChapterPanel self, Oui next)
    {
        AreaData area = AreaData.Get(self.Area);
        if (AreaModeExtender.IsOurMap(area))
        {
            // Save D-Side panel state before leaving
            try
            {
                SaveDSidePanelState(self, area);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to save D-Side panel state: {ex.Message}");
            }
        }

        orig(self, next);
    }

    private static void SaveDSidePanelState(OuiChapterPanel panel, AreaData area)
    {
        DynamicData panelDyn = DynamicData.For(panel);
        int currentMode = panelDyn.Get("mode") ?? panelDyn.Get("Mode") ?? 0;

        if (currentMode >= AreaModeExtender.MODE_DSIDE)
        {
            string stateKey = $"{area.SID}_last_mode";
            Logger.Log(LogLevel.Debug, "MaggyHelper", $"Saved D-Side panel mode for {area.SID}: {AreaModeExtender.GetSideLabel(currentMode)}");
        }
    }

    private static void OnLevelLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes introType, bool isFromLoader)
    {
        orig(self, introType, isFromLoader);

        AreaData area = AreaData.Get(self.Session.Area);
        if (!AreaModeExtender.IsOurMap(area))
            return;

        int mode = (int)self.Session.Area.Mode;
        if (mode < AreaModeExtender.MODE_DSIDE)
            return;

        // Initialize D-Side specific level state
        try
        {
            InitializeDSideLevelState(self, area, mode);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to initialize D-Side level state: {ex.Message}");
        }
    }

    private static void InitializeDSideLevelState(Level level, AreaData area, int mode)
    {
        // Set up D-Side specific difficulty/mechanics
        string sideLabel = AreaModeExtender.GetSideLabel(mode);
        Logger.Log(LogLevel.Info, "MaggyHelper", $"Loaded {sideLabel}-Side level: {area.SID}");

        // D-Side specific initializations can be added here
        if (mode == AreaModeExtender.MODE_DSIDE)
        {
            // D-Side specific setup
            Logger.Log(LogLevel.Debug, "MaggyHelper", "D-Side level mode initialized");
        }
        else if (mode == AreaModeExtender.MODE_DXSIDE)
        {
            // DX-Side specific setup
            Logger.Log(LogLevel.Debug, "MaggyHelper", "DX-Side level mode initialized");
        }
    }

    private static void OnLevelEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        AreaData area = AreaData.Get(self.Session.Area);
        if (AreaModeExtender.IsOurMap(area))
        {
            int mode = (int)self.Session.Area.Mode;
            if (mode >= AreaModeExtender.MODE_DSIDE)
            {
                // Clean up D-Side specific state
                Logger.Log(LogLevel.Debug, "MaggyHelper", $"D-Side level ended: {AreaModeExtender.GetSideLabel(mode)}-Side");
            }
        }

        orig(self);
    }

    private static void OnSaveDataBeforeInitialize(On.Celeste.SaveData.orig_BeforeInitialize orig, SaveData self)
    {
        orig(self);

        // Ensure D-Side save data structures exist
        try
        {
            EnsureDSideSaveStructures(self);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to ensure D-Side save structures: {ex.Message}");
        }
    }

    private static void EnsureDSideSaveStructures(SaveData save)
    {
        if (save?.Areas_Safe == null)
            return;

        foreach (AreaData area in AreaData.Areas)
        {
            if (!AreaModeExtender.IsOurMap(area))
                continue;

            if (area.ID < 0 || area.ID >= save.Areas_Safe.Count)
                continue;

            object stats = save.Areas_Safe[area.ID];
            if (stats == null)
                continue;

            // Ensure D-Side mode stats exist
            DynamicData dyn = DynamicData.For(stats);
            var modes = dyn.Get("Modes") as System.Array ?? dyn.Get("modes") as System.Array;

            if (modes != null && modes.Length > AreaModeExtender.MODE_DSIDE)
            {
                if (modes.GetValue(AreaModeExtender.MODE_DSIDE) == null)
                {
                    Logger.Log(LogLevel.Debug, "MaggyHelper", $"Creating D-Side stats for {area.SID}");
                }
            }
        }
    }

    /// <summary>
    /// Check if a D-Side heart gem has been collected for a specific chapter.
    /// </summary>
    public static bool HasDSideHeartGem(AreaData area, int modeIndex = AreaModeExtender.MODE_DSIDE)
    {
        if (area == null || !AreaModeExtender.IsOurMap(area))
            return false;

        string heartId = $"{area.SID}_{AreaModeExtender.GetModeName(modeIndex)}_crystal";
        return MaggyHelperModule.SaveData?.HasCollectedHeartGem(heartId) == true;
    }

    /// <summary>
    /// Get D-Side completion status for a chapter.
    /// </summary>
    public static bool IsDSideCompleted(AreaData area, int modeIndex = AreaModeExtender.MODE_DSIDE)
    {
        if (area == null || !AreaModeExtender.IsOurMap(area))
            return false;

        return AreaModeExtender.IsSideUnlocked(new AreaKey(area.ID), modeIndex);
    }

    /// <summary>
    /// Manually unlock a D-Side for testing/debug purposes.
    /// </summary>
    public static void UnlockDSide(AreaData area, int modeIndex = AreaModeExtender.MODE_DSIDE)
    {
        if (area == null || !AreaModeExtender.IsOurMap(area))
            return;

        string heartId = $"{area.SID}_{AreaModeExtender.GetModeName(modeIndex)}_crystal";
        MaggyHelperModule.SaveData?.CollectHeartGem(heartId);

        Logger.Log(LogLevel.Info, "MaggyHelper", $"Unlocked {AreaModeExtender.GetSideLabel(modeIndex)}-Side for {area.SID}");
    }
}
