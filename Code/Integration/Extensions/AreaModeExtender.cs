#pragma warning disable CS0436

using global::Celeste.Mod.Meta;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste;

/// <summary>
/// Extends Maggy chapters with D/DX sides while keeping vanilla save boundaries stable.
/// </summary>
public static class AreaModeExtender
{
    private static readonly Type RuntimeAreaStatsType = typeof(Session).Assembly.GetType("Celeste.AreaStats", throwOnError: false);
    private static readonly HashSet<string> EarlyMapMetaSkipLog = new(StringComparer.OrdinalIgnoreCase);

    public const int MODE_NORMAL = 0;
    public const int MODE_BSIDE = 1;
    public const int MODE_CSIDE = 2;
    public const int MODE_DSIDE = 3;
    public const int MODE_DXSIDE = 4;
    public const int TOTAL_MODES = 5;

    public const string MAP_PREFIX = "Maggy";
    
    /// <summary>Folder names for each side (A, B, C, D, DX)</summary>
    public static readonly string[] SideFolders =
    {
        "ASide",   // MODE_NORMAL (0)
        "BSide",   // MODE_BSIDE (1)
        "CSide",   // MODE_CSIDE (2)
        "DSide",   // MODE_DSIDE (3)
        "DXSide"   // MODE_DXSIDE (4)
    };

    public static readonly string[] SideSuffixes = { "", " B-Side", " C-Side", " D-Side", " DX-Side" };
    public static readonly string[] HeartGemColors = { "blue", "red", "gold", "rainbow", "void" };

    public static readonly string[] HeartGemGetSounds =
    {
        "event:/game/general/crystalheart_blue_get",
        "event:/game/general/crystalheart_red_get",
        "event:/game/general/crystalheart_gold_get",
        "event:/pusheen/game/general/crystalheart_rainbow_get",
        "event:/pusheen/game/general/crystalheart_void_get"
    };

    private static bool _loaded;
    private static Hook _mapMetaApplyHook;
    private static On.Celeste.Session.hook_ctor_AreaKey_string_AreaStats _sessionCtorHook;
    private static On.Celeste.AreaStats.hook_Clone _areaStatsCloneHook;

    /// <summary>
    /// When set, the next OuiChapterPanel.Reset for this area ID should
    /// snap the panel to the D-Side tab so the overworld returns to the
    /// correct side after a D-Side level completion.
    /// </summary>
    private static int _pendingDSideReturnAreaId = -1;

    /// <summary>
    /// Queues the chapter panel to reopen on the D-Side tab the next time
    /// <paramref name="aSideAreaId"/>'s chapter panel resets.
    /// Called by AltSidesHelperBridge for ASH-routed D-Side completions.
    /// </summary>
    public static void SetPendingDSideReturn(int aSideAreaId)
    {
        _pendingDSideReturnAreaId = aSideAreaId;
    }

    private delegate void orig_MapMetaModeProperties_ApplyTo(MapMetaModeProperties self, AreaData area, AreaMode mode);

    public static string GetSideFolder(int modeIndex)
    {
        if (modeIndex < 0 || modeIndex >= SideFolders.Length)
            return SideFolders[MODE_NORMAL];

        return SideFolders[modeIndex];
    }

    public static string BuildSideSID(int modeIndex, string mapName)
    {
        return BuildSideSID(GetSideFolder(modeIndex), mapName);
    }

    public static string BuildSideSID(string sideFolder, string mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName))
            return MAP_PREFIX;

        // SIDs do not include the Maps/ prefix — Everest strips it when registering .bin files.
        return $"{MAP_PREFIX}/{sideFolder}/{mapName}";
    }

    public static string BuildASideSID(string mapName)
    {
        return BuildSideSID(MODE_NORMAL, mapName);
    }

    public static void Load()
    {
        if (_loaded)
            return;

        _loaded = true;

        On.Celeste.AreaData.Load += OnAreaDataLoad;
        On.Celeste.OuiChapterPanel.Reset += OnChapterPanelReset;
        On.Celeste.OuiChapterPanel.UpdateStats += OnChapterPanelUpdateStats;
        On.Celeste.HeartGem.Collect += OnHeartGemCollect;
        On.Celeste.LevelExit.ctor += OnLevelExitCtor;

        _sessionCtorHook ??= (orig, self, area, checkpoint, oldStats) => OnSessionCtor(orig, self, area, checkpoint, oldStats);
        On.Celeste.Session.ctor_AreaKey_string_AreaStats += _sessionCtorHook;

        _areaStatsCloneHook ??= (orig, self) => OnAreaStatsClone(orig, self);
        On.Celeste.AreaStats.Clone += _areaStatsCloneHook;

        On.Celeste.SaveData.AfterInitialize += OnSaveDataAfterInitialize;
        On.Celeste.UserIO.SaveThread += OnSaveThread;

        InstallMapMetaApplyHook();

        Logger.Log(LogLevel.Info, "MaggyHelper", "AreaModeExtender loaded");
    }

    public static void Unload()
    {
        if (!_loaded)
            return;

        _loaded = false;

        On.Celeste.AreaData.Load -= OnAreaDataLoad;
        On.Celeste.OuiChapterPanel.Reset -= OnChapterPanelReset;
        On.Celeste.OuiChapterPanel.UpdateStats -= OnChapterPanelUpdateStats;
        On.Celeste.HeartGem.Collect -= OnHeartGemCollect;
        On.Celeste.LevelExit.ctor -= OnLevelExitCtor;

        if (_sessionCtorHook != null)
            On.Celeste.Session.ctor_AreaKey_string_AreaStats -= _sessionCtorHook;

        if (_areaStatsCloneHook != null)
            On.Celeste.AreaStats.Clone -= _areaStatsCloneHook;
        _areaStatsCloneHook = null;

        On.Celeste.SaveData.AfterInitialize -= OnSaveDataAfterInitialize;
        On.Celeste.UserIO.SaveThread -= OnSaveThread;

        _mapMetaApplyHook?.Dispose();
        _mapMetaApplyHook = null;
        EarlyMapMetaSkipLog.Clear();
        _pendingDSideReturnAreaId = -1;

        Logger.Log(LogLevel.Info, "MaggyHelper", "AreaModeExtender unloaded");
    }

    private static void InstallMapMetaApplyHook()
    {
        if (_mapMetaApplyHook != null)
            return;

        MethodInfo target = typeof(MapMetaModeProperties).GetMethod(
            "ApplyTo",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(AreaData), typeof(AreaMode) },
            null);

        MethodInfo detour = typeof(AreaModeExtender).GetMethod(
            nameof(Hook_MapMetaModeProperties_ApplyTo),
            BindingFlags.Static | BindingFlags.NonPublic);

        if (target == null || detour == null)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", "Failed to install MapMetaModeProperties.ApplyTo guard hook.");
            return;
        }

        _mapMetaApplyHook = new Hook(target, detour);
    }

    private static void Hook_MapMetaModeProperties_ApplyTo(orig_MapMetaModeProperties_ApplyTo orig,
        MapMetaModeProperties self, AreaData area, AreaMode mode)
    {
        if (area != null && IsOurMap(area))
        {
            int modeIndex = (int) mode;
            int availableModes = area.Mode?.Length ?? 0;

            if (modeIndex >= availableModes)
            {
                string key = $"{area.SID}|{modeIndex}|{availableModes}";
                if (EarlyMapMetaSkipLog.Add(key))
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        $"Skipping early MapMeta apply for '{area.SID}' mode {modeIndex}; Mode[] length {availableModes}.");
                }

                return;
            }
        }

        orig(self, area, mode);
    }

    private static void OnAreaDataLoad(On.Celeste.AreaData.orig_Load orig)
    {
        orig();

        AreaMapData.RefreshAvailableSides();

        int extended = 0;

        foreach (AreaData area in AreaData.Areas)
        {
            if (!IsOurMap(area))
                continue;

            if (TryExtendAreaModes(area))
                extended++;
        }

        try
        {
            AreaMapData.ApplyHardcodedRuntimeData();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"ApplyHardcodedRuntimeData failed: {ex.Message}");
        }

        ConfigureMainSideHierarchy();

        AltSidesHelperBridge.PostAreaDataLoad();

        Logger.Log(LogLevel.Info, "MaggyHelper", $"AreaModeExtender refreshed areas, extended={extended}");
    }

    private static bool TryExtendAreaModes(AreaData area)
    {
        if (area?.Mode == null)
            return false;

        if (!TryParseMainSideSID(area.SID, out string baseKey, out string sideFolder))
            return false;

        // Extend only chapter-parent A side entries (ASide folder).
        if (!sideFolder.Equals("ASide", StringComparison.OrdinalIgnoreCase))
            return false;

        AreaMapData.ChapterDef chapterDef = AreaMapData.FindByAnySID(area.SID);
        if (chapterDef == null)
            return false;

        bool hasD = chapterDef.HasDSide;
        bool hasDX = chapterDef.HasDXSide;
        if (!hasD && !hasDX)
            return false;

        // If AltSidesHelper is present and owns this chapter's D-Side, let ASH
        // manage the panel tab — do not inject Mode[3] here.
        if (hasD && AltSidesHelperBridge.IsAshOwned(area.SID))
        {
            hasD = false;
            if (!hasDX)
                return false;
        }

        int oldLength = area.Mode.Length;
        int required = oldLength;
        if (hasD)
            required = Math.Max(required, MODE_DSIDE + 1);
        if (hasDX)
            required = Math.Max(required, MODE_DXSIDE + 1);

        if (required <= oldLength)
            return false;

        ModeProperties[] newModes = new ModeProperties[required];
        Array.Copy(area.Mode, newModes, oldLength);

        if (hasD)
            newModes[MODE_DSIDE] = BuildExtendedMode(area, baseKey, MODE_DSIDE);

        if (hasDX)
            newModes[MODE_DXSIDE] = BuildExtendedMode(area, baseKey, MODE_DXSIDE);

        area.Mode = newModes;

        for (int mode = oldLength; mode < newModes.Length; mode++)
        {
            if (newModes[mode] == null)
                continue;

            if (!TryAttachMapData(area, newModes[mode], mode))
                newModes[mode] = null;
        }

        // Prevent null trailing slots from leaking into mode iteration code.
        int validLength = newModes.Length;
        while (validLength > 0 && newModes[validLength - 1] == null)
            validLength--;

        if (validLength != newModes.Length)
        {
            ModeProperties[] trimmed = new ModeProperties[validLength];
            Array.Copy(newModes, trimmed, validLength);
            area.Mode = trimmed;
        }

        try
        {
            AreaMapData.ApplyHardcodedRuntimeData(area);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Runtime data apply failed for {area.SID}: {ex.Message}");
        }

        return true;
    }

    private static ModeProperties BuildExtendedMode(AreaData area, string baseKey, int modeIndex)
    {
        ModeProperties baseMode = area.Mode.Length > MODE_CSIDE && area.Mode[MODE_CSIDE] != null
            ? area.Mode[MODE_CSIDE]
            : area.Mode[MODE_NORMAL];

        string sid = BuildSideSID(modeIndex, baseKey);

        return new ModeProperties
        {
            Path = sid,
            Inventory = baseMode?.Inventory ?? PlayerInventory.Default,
            AudioState = new AudioState(
                baseMode?.AudioState?.Music?.Event ?? string.Empty,
                baseMode?.AudioState?.Ambience?.Event ?? string.Empty),
            Checkpoints = null
        };
    }

    private static bool TryAttachMapData(AreaData area, ModeProperties mode, int modeIndex)
    {
        try
        {
            mode.MapData = new MapData(new AreaKey(area.ID, (global::Celeste.AreaMode) modeIndex));
            return mode.MapData != null;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"Failed to load MapData for {area.SID} mode {modeIndex} ({mode.Path}): {ex.Message}");
            return false;
        }
    }

    private static void ConfigureMainSideHierarchy()
    {
        Dictionary<string, (int id, string sid)> parentByBase = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < AreaData.Areas.Count; i++)
        {
            AreaData area = AreaData.Areas[i];
            if (!TryParseMainSideSID(area?.SID, out string baseKey, out string sideFolder))
                continue;

            // Only A-side entries can be parents
            if (sideFolder.Equals("ASide", StringComparison.OrdinalIgnoreCase))
                parentByBase[baseKey] = (i, area.SID);
        }

        for (int i = 0; i < AreaData.Areas.Count; i++)
        {
            AreaData area = AreaData.Areas[i];
            if (!TryParseMainSideSID(area?.SID, out string baseKey, out string sideFolder))
                continue;

            // Skip A-side entries (they are parents, not children)
            if (sideFolder.Equals("ASide", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!parentByBase.TryGetValue(baseKey, out (int id, string sid) parent) || parent.id == i)
                continue;

            DynamicData areaDyn = DynamicData.For(area);
            TrySetMember(area, areaDyn, "ParentSID", parent.sid);
            TrySetMember(area, areaDyn, "ParentSid", parent.sid);
            TrySetMember(area, areaDyn, "ParentID", parent.id);
            TrySetMember(area, areaDyn, "ParentId", parent.id);

            object meta = TryGetMember<object>(areaDyn, "Meta");
            if (meta == null)
                continue;

            DynamicData metaDyn = DynamicData.For(meta);
            TrySetMember(meta, metaDyn, "ParentSID", parent.sid);
            TrySetMember(meta, metaDyn, "ParentSid", parent.sid);
            TrySetMember(meta, metaDyn, "ParentID", parent.id);
            TrySetMember(meta, metaDyn, "ParentId", parent.id);
        }
    }

    private static void OnChapterPanelReset(On.Celeste.OuiChapterPanel.orig_Reset orig, OuiChapterPanel self)
    {
        AreaData area = AreaData.Get(self.Area);
        if (IsOurMap(area))
        {
            EnsureUnlockedModesForChapterPanel(self.Area, area);
            try
            {
                AreaMapData.ApplyHardcodedRuntimeData(area);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"OnChapterPanelReset: ApplyHardcodedRuntimeData failed: {ex.Message}");
            }
        }

        orig(self);

        // If the player just completed a D-Side, snap the chapter panel to the
        // D-Side tab so the overworld doesn't fall back to the A-Side.
        if (_pendingDSideReturnAreaId >= 0 && area != null && area.ID == _pendingDSideReturnAreaId)
        {
            _pendingDSideReturnAreaId = -1;
            try
            {
                DynamicData panelDyn = DynamicData.For(self);
                TrySetMember(self, panelDyn, "mode",      MODE_DSIDE);
                TrySetMember(self, panelDyn, "Mode",      MODE_DSIDE);
                TrySetMember(self, panelDyn, "currentMode", MODE_DSIDE);
                TrySetMember(self, panelDyn, "CurrentMode", MODE_DSIDE);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"OnChapterPanelReset: could not restore D-Side tab: {ex.Message}");
            }
        }
    }

    private static void OnChapterPanelUpdateStats(On.Celeste.OuiChapterPanel.orig_UpdateStats orig,
        OuiChapterPanel self, bool wiggle, bool? overrideStrawberryWiggle, bool? overrideDeathWiggle, bool? overrideHeartWiggle)
    {
        AreaData area = AreaData.Get(self.Area);
        if (IsOurMap(area))
        {
            try
            {
                AreaMapData.ApplyHardcodedRuntimeData(area);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"OnChapterPanelUpdateStats: ApplyHardcodedRuntimeData failed: {ex.Message}");
            }
        }

        orig(self, wiggle, overrideStrawberryWiggle, overrideDeathWiggle, overrideHeartWiggle);
    }

    private static void EnsureUnlockedModesForChapterPanel(AreaKey key, AreaData area)
    {
        SaveData save = SaveData.Instance;
        if (save == null || area?.Mode == null)
            return;

        int required = 3;
        if (area.Mode.Length > MODE_DSIDE && area.Mode[MODE_DSIDE] != null && IsSideUnlocked(key, MODE_DSIDE))
            required = MODE_DSIDE + 1;
        if (area.Mode.Length > MODE_DXSIDE && area.Mode[MODE_DXSIDE] != null && IsSideUnlocked(key, MODE_DXSIDE))
            required = MODE_DXSIDE + 1;

        if (required == save.UnlockedModes)
            return;

        if (required < save.UnlockedModes && save.UnlockedModes > TOTAL_MODES)
            return;

        DynamicData saveDyn = DynamicData.For(save);
        TrySetMember(save, saveDyn, "unlockedModes", required);
        TrySetMember(save, saveDyn, "UnlockedModes", required);
    }

    private static void OnHeartGemCollect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player)
    {
        orig(self, player);

        Level level = self.Scene as Level;
        if (level == null)
            return;

        AreaData area = AreaData.Get(level.Session.Area);
        if (!IsOurMap(area))
            return;

        int mode = (int) level.Session.Area.Mode;
        if (mode < MODE_DSIDE)
            return;

        string heartId = $"{area.SID}_{GetModeName(mode)}";
        KIRBY_CELESTEModule.SaveData?.CollectHeartGem(heartId);

        if (mode >= 0 && mode < HeartGemGetSounds.Length)
            Audio.Play(HeartGemGetSounds[mode]);
    }

    private static void OnLevelExitCtor(On.Celeste.LevelExit.orig_ctor orig, LevelExit self,
        LevelExit.Mode mode, Session session, HiresSnow snow)
    {
        orig(self, mode, session, snow);

        if (mode != LevelExit.Mode.Completed || session == null)
            return;

        AreaData area = AreaData.Get(session.Area);
        if (!IsOurMap(area))
            return;

        int completedMode = (int) session.Area.Mode;
        if (completedMode is not MODE_BSIDE and not MODE_CSIDE and not MODE_DSIDE)
            return;

        // When returning to the overworld after a D-Side, make sure the chapter
        // panel reopens on the D-Side tab rather than defaulting to the A-Side.
        if (completedMode == MODE_DSIDE)
            _pendingDSideReturnAreaId = area.ID;

        int unlockedMode = completedMode + 1;
        if (unlockedMode >= TOTAL_MODES)
            return;

        if (area.Mode == null || unlockedMode >= area.Mode.Length || area.Mode[unlockedMode] == null)
            return;

        string unlockKey = $"{area.SID}_{GetModeName(unlockedMode)}_unlocked";
        if (KIRBY_CELESTEModule.SaveData?.HasAchievement(unlockKey) == true)
            return;

        Engine.Scene = new SideUnlockVignette(session, completedMode);
    }

    private static AreaStats OnAreaStatsClone(On.Celeste.AreaStats.orig_Clone orig, AreaStats self)
    {
        // When MaggyHelper extends Modes beyond 3, vanilla Clone() iterates self.Modes.Length (e.g. 5)
        // but writes into a freshly-created 3-slot destination, causing IndexOutOfRangeException.
        // Guard: temporarily shrink to 3, let vanilla clone handle them, then re-extend the result.
        if (self?.Modes == null || self.Modes.Length <= 3)
            return orig(self);

        AreaModeStats[] fullModes = self.Modes;
        AreaModeStats[] cap = new AreaModeStats[3];
        Array.Copy(fullModes, cap, 3);
        self.Modes = cap;

        AreaStats result;
        try
        {
            result = orig(self);
        }
        finally
        {
            self.Modes = fullModes;
        }

        // Extend result.Modes to carry the cloned extended-mode entries as well.
        AreaModeStats[] extended = new AreaModeStats[fullModes.Length];
        Array.Copy(result.Modes, extended, result.Modes.Length);
        for (int i = result.Modes.Length; i < fullModes.Length; i++)
            extended[i] = fullModes[i]?.Clone() ?? new AreaModeStats();
        result.Modes = extended;

        return result;
    }

    private static void OnSessionCtor(On.Celeste.Session.orig_ctor_AreaKey_string_AreaStats orig, Session self,
        AreaKey area, string checkpoint, object oldStats)
    {
        object stats = EnsureSafeAreaStats(area, oldStats);

        try
        {
            orig.DynamicInvoke(self, area, checkpoint, stats ?? oldStats);
        }
        catch (Exception firstEx)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"Session ctor failed for {area} with provided stats ({firstEx.GetType().Name}); trying regenerated AreaStats.");

            object regenerated = CreateFreshAreaStats(area);
            if (regenerated != null && !ReferenceEquals(regenerated, stats) && !ReferenceEquals(regenerated, oldStats))
            {
                try
                {
                    orig.DynamicInvoke(self, area, checkpoint, regenerated);

                    SaveData save = SaveData.Instance;
                    IList areas = save?.Areas_Safe;
                    if (areas != null && area.ID >= 0 && area.ID < areas.Count)
                        areas[area.ID] = regenerated;

                    return;
                }
                catch (Exception regeneratedEx)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        $"Regenerated AreaStats also failed for {area}: {regeneratedEx.GetType().Name}");
                }
            }

            // Final fallback: retry with original payload so vanilla can own the error path.
            orig.DynamicInvoke(self, area, checkpoint, oldStats);
        }
    }

    private static object EnsureSafeAreaStats(AreaKey area, object oldStats)
    {
        object stats = oldStats;
        bool createdFallback = false;

        if (stats == null)
        {
            SaveData save = SaveData.Instance;
            if (save?.Areas_Safe != null && area.ID >= 0 && area.ID < save.Areas_Safe.Count)
                stats = save.Areas_Safe[area.ID];
        }

        if (stats == null)
        {
            stats = CreateFallbackAreaStats(area);
            createdFallback = stats != null;
        }

        if (stats == null)
            return null;

        int areaModeCount = 0;
        try
        {
            areaModeCount = AreaData.Get(area)?.Mode?.Length ?? 0;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to get area mode count: {ex.Message}");
        }

        int requiredModes = Math.Max((int) area.Mode + 1, Math.Max(areaModeCount, 3));
        EnsureAreaModeStatsArray(stats, requiredModes, allowShrink: areaModeCount > 0);

        if (createdFallback)
            TryStoreSaveAreaStats(area, stats);

        return stats;
    }

    private static object CreateFreshAreaStats(AreaKey area)
    {
        object stats = CreateFallbackAreaStats(area);
        if (stats == null)
            return null;

        int areaModeCount = 0;
        try
        {
            areaModeCount = AreaData.Get(area)?.Mode?.Length ?? 0;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to get area mode count: {ex.Message}");
        }

        int requiredModes = Math.Max((int) area.Mode + 1, Math.Max(areaModeCount, 3));
        EnsureAreaModeStatsArray(stats, requiredModes, allowShrink: areaModeCount > 0);
        TryStoreSaveAreaStats(area, stats);
        return stats;
    }

    private static object CreateFallbackAreaStats(AreaKey area)
    {
        if (RuntimeAreaStatsType == null)
            return null;

        foreach (object[] args in new[]
        {
            new object[] { area.ID },
            Array.Empty<object>()
        })
        {
            try
            {
                object created = Activator.CreateInstance(
                    RuntimeAreaStatsType,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    args,
                    null);

                if (created != null)
                    return created;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to create AreaStats instance: {ex.Message}");
            }
        }

        return null;
    }

    private static void OnSaveDataAfterInitialize(On.Celeste.SaveData.orig_AfterInitialize orig, SaveData self)
    {
        // Vanilla must run first; new slots are partially constructed before orig.
        try
        {
            orig(self);
        }
        catch (Exception ex) when (TryRecoverAfterInitialize(self, ex))
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"SaveData.AfterInitialize recovered from {ex.GetType().Name}; retrying once.");
            orig(self);
        }

        try
        {
            EnsureExtendedSaveAreaStats(self);
            SanitizeVanillaSaveTargets(self, temporary: false);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Post AfterInitialize save repair skipped: {ex.Message}");
        }
    }

    private static bool TryRecoverAfterInitialize(SaveData save, Exception exception)
    {
        if (save == null)
            return false;

        if (exception is not NullReferenceException and not IndexOutOfRangeException)
            return false;

        int repaired = EnsureSaveDataStructure(save);
        repaired += EnsureExtendedSaveAreaStats(save);

        if (repaired <= 0)
            return false;

        Logger.Log(LogLevel.Warn, "MaggyHelper",
            $"SaveData.AfterInitialize repaired {repaired} entries after {exception.GetType().Name}.");
        return true;
    }

    private static int EnsureSaveDataStructure(SaveData save)
    {
        if (save == null)
            return 0;

        DynamicData saveDyn = DynamicData.For(save);
        int repaired = 0;

        repaired += EnsureLevelSetCollection(TryGetMember<IList>(saveDyn, "LevelSets"));
        repaired += EnsureLevelSetCollection(TryGetMember<IList>(saveDyn, "LevelSetRecycleBin"));

        return repaired;
    }

    private static int EnsureLevelSetCollection(IList levelSets)
    {
        if (levelSets == null)
            return 0;

        int repaired = 0;

        for (int i = levelSets.Count - 1; i >= 0; i--)
        {
            object levelSet = levelSets[i];
            if (levelSet == null)
            {
                levelSets.RemoveAt(i);
                repaired++;
                continue;
            }

            DynamicData levelSetDyn = DynamicData.For(levelSet);

            if (TryGetMember<string>(levelSetDyn, "Name") == null
                && TrySetMember(levelSet, levelSetDyn, "Name", string.Empty))
            {
                repaired++;
            }

            repaired += EnsureCollectionMember(levelSet, levelSetDyn, "Areas");
            repaired += EnsureCollectionMember(levelSet, levelSetDyn, "Poem");
        }

        return repaired;
    }

    private static int EnsureCollectionMember(object target, DynamicData dyn, string memberName)
    {
        if (target == null || dyn == null)
            return 0;

        if (TryGetMember<IList>(dyn, memberName) != null)
            return 0;

        if (!TryCreateMemberInstance(target.GetType(), memberName, out object instance))
            return 0;

        return TrySetMember(target, dyn, memberName, instance) ? 1 : 0;
    }

    private static bool TryCreateMemberInstance(Type targetType, string memberName, out object instance)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        instance = null;

        Type memberType = targetType.GetProperty(memberName, flags)?.PropertyType
            ?? targetType.GetField(memberName, flags)?.FieldType;

        if (memberType == null)
            return false;

        try
        {
            instance = Activator.CreateInstance(memberType);
            return instance != null;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to create member instance: {ex.Message}");
            return false;
        }
    }

    private static void OnSaveThread(On.Celeste.UserIO.orig_SaveThread orig)
    {
        SaveDataSanitizationSnapshot snapshot = null;
        try
        {
            snapshot = SanitizeVanillaSaveTargets(SaveData.Instance, temporary: true);
            orig();
        }
        finally
        {
            snapshot?.Restore();
        }
    }

    private static int EnsureExtendedSaveAreaStats(SaveData save)
    {
        if (save?.Areas_Safe == null || AreaData.Areas == null)
            return 0;

        int repaired = 0;

        while (save.Areas_Safe.Count < AreaData.Areas.Count)
        {
            save.Areas_Safe.Add(null);
            repaired++;
        }

        for (int i = 0; i < AreaData.Areas.Count; i++)
        {
            AreaData area = AreaData.Areas[i];
            if (!IsOurMap(area))
                continue;

            object stats = save.Areas_Safe[i];
            if (stats == null)
                continue;

            int before = GetModesArray(stats)?.Length ?? 0;
            int requiredModes = Math.Max(area?.Mode?.Length ?? 0, 3);
            EnsureAreaModeStatsArray(stats, requiredModes);
            if (before < requiredModes)
                repaired++;
        }

        if (repaired > 0)
            Logger.Log(LogLevel.Info, "MaggyHelper", $"Repaired {repaired} save stats entries");

        return repaired;
    }

    private static SaveDataSanitizationSnapshot SanitizeVanillaSaveTargets(SaveData save, bool temporary)
    {
        if (save == null)
            return null;

        SaveDataSanitizationSnapshot snapshot = temporary ? new SaveDataSanitizationSnapshot(save) : null;
        int changes = 0;

        AreaData lastAreaData = null;
        try { lastAreaData = AreaData.Get(save.LastArea); } catch (Exception ex) { Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to get last area data: {ex.Message}"); }

        if (IsOurMap(lastAreaData) && TrySanitizeAreaKey(save.LastArea, out AreaKey sanitizedLastArea))
        {
            if (temporary)
                snapshot.LastArea = save.LastArea;

            save.LastArea = sanitizedLastArea;
            changes++;
        }

        Session currentSession = GetCurrentSession(save);
        AreaData currentSessionAreaData = null;
        if (currentSession != null)
        {
            try { currentSessionAreaData = AreaData.Get(currentSession.Area); } catch (Exception ex) { Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to get current session area data: {ex.Message}"); }
        }

        if (currentSession != null && IsOurMap(currentSessionAreaData)
            && TrySanitizeAreaKey(currentSession.Area, out AreaKey sanitizedSessionArea))
        {
            if (temporary)
            {
                snapshot.CurrentSession = currentSession;
                snapshot.CurrentSessionArea = currentSession.Area;
            }

            SetSessionArea(currentSession, sanitizedSessionArea);
            changes++;
        }

        if (!temporary)
            return null;

        return changes > 0 ? snapshot : null;
    }

    private static Session GetCurrentSession(SaveData save)
    {
        if (save == null)
            return null;

        try
        {
            return save.CurrentSession_Safe ?? save.CurrentSession;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to get current session: {ex.Message}");
            return null;
        }
    }

    private static void SetSessionArea(Session session, AreaKey area)
    {
        if (session == null)
            return;

        DynamicData dyn = DynamicData.For(session);
        TrySetMember(session, dyn, "area", area);
        TrySetMember(session, dyn, "Area", area);
    }

    private static bool TrySanitizeAreaKey(AreaKey key, out AreaKey sanitized)
    {
        sanitized = key;

        int mode = (int) key.Mode;
        if (mode >= MODE_NORMAL && mode <= MODE_CSIDE)
            return false;

        sanitized = new AreaKey(key.ID, (global::Celeste.AreaMode) Math.Clamp(mode, MODE_NORMAL, MODE_CSIDE));
        return true;
    }

    private sealed class SaveDataSanitizationSnapshot
    {
        private readonly SaveData _save;

        public SaveDataSanitizationSnapshot(SaveData save)
        {
            _save = save;
        }

        public AreaKey? LastArea { get; set; }
        public Session CurrentSession { get; set; }
        public AreaKey? CurrentSessionArea { get; set; }

        public void Restore()
        {
            if (_save == null)
                return;

            if (LastArea.HasValue)
                _save.LastArea = LastArea.Value;

            if (CurrentSession != null && CurrentSessionArea.HasValue)
                SetSessionArea(CurrentSession, CurrentSessionArea.Value);
        }
    }

    internal static object TryGetSaveAreaStats(int areaId)
    {
        SaveData save = SaveData.Instance;
        if (save?.Areas_Safe == null || areaId < 0 || areaId >= save.Areas_Safe.Count)
            return null;

        return save.Areas_Safe[areaId];
    }

    internal static object TryGetSaveAreaStats(AreaKey area)
    {
        return TryGetSaveAreaStats(area.ID);
    }

    private static bool TryStoreSaveAreaStats(AreaKey area, object stats)
    {
        SaveData save = SaveData.Instance;
        IList areas = save?.Areas_Safe;
        if (areas == null || area.ID < 0)
            return false;

        while (areas.Count <= area.ID)
            areas.Add(null);

        areas[area.ID] = stats;
        return true;
    }

    internal static int GetSaveAreaModeCount(int areaId)
    {
        Array modes = GetModesArray(TryGetSaveAreaStats(areaId));
        return modes?.Length ?? 0;
    }

    internal static bool GetSaveAreaModeHeartGem(int areaId, int modeIndex)
    {
        return GetSaveAreaModeBool(areaId, modeIndex, "HeartGem");
    }

    internal static bool GetSaveAreaModeCompleted(int areaId, int modeIndex)
    {
        return GetSaveAreaModeBool(areaId, modeIndex, "Completed");
    }

    internal static bool SetSaveAreaModeHeartGem(int areaId, int modeIndex, bool value)
    {
        return SetSaveAreaModeBool(areaId, modeIndex, "HeartGem", value);
    }

    public static bool IsOurMap(AreaData area)
    {
        if (area?.SID == null)
            return false;

        // Check if SID starts with "Maggy/" followed by one of our side folder names
        foreach (var sideFolder in SideFolders)
        {
            if (area.SID.StartsWith($"{MAP_PREFIX}/{sideFolder}/", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static string GetModeName(int modeIndex)
    {
        return modeIndex switch
        {
            MODE_NORMAL => "Normal",
            MODE_BSIDE => "BSide",
            MODE_CSIDE => "CSide",
            MODE_DSIDE => "DSide",
            MODE_DXSIDE => "DXSide",
            _ => $"Mode{modeIndex}"
        };
    }

    public static string GetSideLabel(int modeIndex)
    {
        return modeIndex switch
        {
            MODE_NORMAL => "A",
            MODE_BSIDE => "B",
            MODE_CSIDE => "C",
            MODE_DSIDE => "D",
            MODE_DXSIDE => "DX",
            _ => "?"
        };
    }

    private static bool HasDebugOrCheatExtendedSideAccess(SaveData saveData)
    {
        if (saveData?.CheatMode == true)
            return true;

        if (CelesteGame.PlayMode == CelesteGame.PlayModes.Debug)
            return true;

        return KIRBY_CELESTEModule.Instance != null && KIRBY_CELESTEModule.Settings?.DebugMode == true;
    }

    public static bool IsSideUnlocked(AreaKey area, int modeIndex)
    {
        if (modeIndex == MODE_NORMAL)
            return true;

        SaveData saveData = SaveData.Instance;
        if (saveData == null)
            return false;

        if (saveData.CheatMode || HasDebugOrCheatExtendedSideAccess(saveData))
            return true;

        // For ASH-owned D-Sides, check MaggyHelper's extended save data which the
        // bridge populates on completion, rather than vanilla AreaModeStats slots.
        if (modeIndex == MODE_DSIDE)
        {
            AreaData areaData = AreaData.Get(area);
            if (areaData != null && AltSidesHelperBridge.IsAshOwned(areaData))
            {
                string ashHeartId = $"{areaData.SID}_{GetModeName(MODE_DSIDE)}";
                return KIRBY_CELESTEModule.SaveData?.HasCollectedHeartGem(ashHeartId) == true
                    || GetSaveAreaModeHeartGem(area.ID, MODE_CSIDE)
                    || GetSaveAreaModeCompleted(area.ID, MODE_CSIDE);
            }
        }

        if (TryGetSaveAreaStats(area) == null)
            return false;

        int previousMode = modeIndex - 1;
        if (previousMode < 0)
            return true;

        if (previousMode < GetSaveAreaModeCount(area.ID))
        {
            return GetSaveAreaModeHeartGem(area.ID, previousMode)
                || GetSaveAreaModeCompleted(area.ID, previousMode);
        }

        string sid = AreaData.Get(area)?.SID;
        if (string.IsNullOrWhiteSpace(sid))
            return false;

        string heartId = $"{sid}_{GetModeName(previousMode)}";
        return KIRBY_CELESTEModule.SaveData?.HasCollectedHeartGem(heartId) == true;
    }

    private static bool GetSaveAreaModeBool(int areaId, int modeIndex, string memberName)
    {
        object modeStats = GetSaveAreaModeStats(areaId, modeIndex);
        if (modeStats == null)
            return false;

        DynamicData dyn = DynamicData.For(modeStats);
        return TryGetMember(dyn, memberName, false);
    }

    private static bool SetSaveAreaModeBool(int areaId, int modeIndex, string memberName, bool value)
    {
        object modeStats = GetSaveAreaModeStats(areaId, modeIndex);
        if (modeStats == null)
            return false;

        DynamicData dyn = DynamicData.For(modeStats);
        return TrySetMember(modeStats, dyn, memberName, value);
    }

    private static object GetSaveAreaModeStats(int areaId, int modeIndex)
    {
        if (modeIndex < 0)
            return null;

        Array modes = GetModesArray(TryGetSaveAreaStats(areaId));
        if (modes == null || modeIndex >= modes.Length)
            return null;

        return modes.GetValue(modeIndex);
    }

    private static Array GetModesArray(object stats)
    {
        if (stats == null)
            return null;

        DynamicData dyn = DynamicData.For(stats);
        return TryGetMember<Array>(dyn, "Modes") ?? TryGetMember<Array>(dyn, "modes");
    }

    private static Type GetModesArrayType(object stats)
    {
        if (stats == null)
            return null;

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        return stats.GetType().GetProperty("Modes", flags)?.PropertyType
            ?? stats.GetType().GetProperty("modes", flags)?.PropertyType
            ?? stats.GetType().GetField("Modes", flags)?.FieldType
            ?? stats.GetType().GetField("modes", flags)?.FieldType;
    }

    private static void EnsureAreaModeStatsArray(object stats, int requiredModes, bool allowShrink = false)
    {
        DynamicData dyn = DynamicData.For(stats);
        Array modes = GetModesArray(stats);
        Type arrayType = GetModesArrayType(stats);
        Type modeType = arrayType?.GetElementType();

        if (modeType == null)
            return;

        if (modes == null)
        {
            modes = Array.CreateInstance(modeType, requiredModes);
            TrySetMember(stats, dyn, "Modes", modes);
            TrySetMember(stats, dyn, "modes", modes);
        }
        else if (allowShrink && modes.Length > requiredModes)
        {
            Array resized = Array.CreateInstance(modeType, requiredModes);
            Array.Copy(modes, resized, requiredModes);
            modes = resized;
            TrySetMember(stats, dyn, "Modes", modes);
            TrySetMember(stats, dyn, "modes", modes);
        }
        else if (modes.Length < requiredModes)
        {
            Array resized = Array.CreateInstance(modeType, requiredModes);
            Array.Copy(modes, resized, modes.Length);
            modes = resized;
            TrySetMember(stats, dyn, "Modes", modes);
            TrySetMember(stats, dyn, "modes", modes);
        }

        for (int i = 0; i < requiredModes; i++)
        {
            if (modes.GetValue(i) != null)
                continue;

            try
            {
                modes.SetValue(Activator.CreateInstance(modeType, nonPublic: true), i);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to create mode instance at index {i}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Parses a map SID in the new folder structure (Maggy/ASide/01_City, etc.)
    /// Returns the base map name and which side folder it's in.
    /// baseKey: The map name (e.g., "01_City")
    /// sideFolder: The side folder name (e.g., "ASide", "BSide", "DSide")
    /// </summary>
    internal static bool TryParseMainSideSID(string sid, out string baseKey, out string sideFolder)
    {
        baseKey = null;
        sideFolder = null;

        if (string.IsNullOrWhiteSpace(sid))
            return false;

        // Check if this is one of our chapters (format: Maggy/SideFolder/MapName)
        string prefix = MAP_PREFIX + "/";
        if (!sid.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        // Extract everything after "Maggy/"
        string remainder = sid[prefix.Length..];
        var parts = remainder.Split('/');

        if (parts.Length < 2)
            return false;

        sideFolder = parts[0];
        baseKey = parts[1];

        // Verify it's a valid side folder
        bool isKnownSideFolder = false;
        foreach (string folder in SideFolders)
        {
            if (!string.Equals(folder, sideFolder, StringComparison.OrdinalIgnoreCase))
                continue;

            isKnownSideFolder = true;
            break;
        }

        if (!isKnownSideFolder)
            return false;

        return !string.IsNullOrWhiteSpace(baseKey);
    }

    private static bool TrySetMember(object target, DynamicData dyn, string name, object value)
    {
        if (!TryResolveWritableMember(target?.GetType(), name, value?.GetType(), out string resolvedName))
            return false;

        try
        {
            dyn.Set(resolvedName, value);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "AreaModeExtender", $"Failed to set member {resolvedName}: {ex.Message}");
            return false;
        }
    }

    private static bool TryResolveWritableMember(Type targetType, string name, Type valueType, out string resolvedName)
    {
        resolvedName = name;
        if (targetType == null)
            return false;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        PropertyInfo property = targetType.GetProperty(name, flags);
        if (property != null && property.CanWrite && IsValueCompatible(property.PropertyType, valueType))
        {
            resolvedName = property.Name;
            return true;
        }

        FieldInfo field = targetType.GetField(name, flags);
        if (field != null && IsValueCompatible(field.FieldType, valueType))
        {
            resolvedName = field.Name;
            return true;
        }

        return false;
    }

    private static bool IsValueCompatible(Type targetType, Type valueType)
    {
        if (valueType == null)
            return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;

        Type effectiveTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (effectiveTargetType.IsAssignableFrom(valueType))
            return true;

        if (effectiveTargetType.IsEnum)
            return IsValueCompatible(Enum.GetUnderlyingType(effectiveTargetType), valueType);

        return effectiveTargetType == typeof(int) && valueType == typeof(int)
            || effectiveTargetType == typeof(string) && valueType == typeof(string)
            || effectiveTargetType == typeof(bool) && valueType == typeof(bool)
            || effectiveTargetType == typeof(AreaKey) && valueType == typeof(AreaKey);
    }

    private static T TryGetMember<T>(DynamicData dyn, string name, T fallback = default)
    {
        if (dyn == null)
            return fallback;

        if (dyn.TryGet(name, out object value))
        {
            if (value is T typedValue)
                return typedValue;
        }

        return fallback;
    }
}