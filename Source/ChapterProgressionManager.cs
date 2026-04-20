namespace Celeste;

/// <summary>
/// Hardcoded chapter progression rules for late-game chapters.
/// Implements restart-gated unlock flow:
/// - Ch15 completion => close game, unlock Ch16 on next launch.
/// - Ch18 outro close => unlock Ch19 on next launch.
/// - Ch19 completion => close game, unlock Ch20 on next launch.
/// - Ch20 completion => close game, unlock Ch21 on next launch.
/// </summary>
public static class ChapterProgressionManager
{
    private static readonly string Ch15Sid = AreaModeExtender.BuildASideSID("15_Castle");
    private static readonly string Ch16Sid = AreaModeExtender.BuildASideSID("16_Corruption");
    private static readonly string Ch19Sid = AreaModeExtender.BuildASideSID("19_Space");
    private static readonly string Ch20Sid = AreaModeExtender.BuildASideSID("20_TheEnd");
    private static readonly string Ch21Sid = AreaModeExtender.BuildASideSID("lastlevel");

    private static bool _hooked;
    private static bool _processedLaunchPendingUnlocks;
    private static bool _forcingSelection;

    public static void Load()
    {
        if (_hooked)
            return;

        _hooked = true;
        _processedLaunchPendingUnlocks = false;

        On.Celeste.Overworld.Begin += OnOverworldBegin;
        On.Celeste.LevelExit.ctor += OnLevelExitCtor;
        On.Celeste.OuiChapterSelect.Update += OnChapterSelectUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper", "ChapterProgressionManager loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;

        On.Celeste.Overworld.Begin -= OnOverworldBegin;
        On.Celeste.LevelExit.ctor -= OnLevelExitCtor;
        On.Celeste.OuiChapterSelect.Update -= OnChapterSelectUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper", "ChapterProgressionManager unloaded");
    }

    private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
    {
        orig(self);

        if (_processedLaunchPendingUnlocks)
        {
            AreaMapData.ApplyHardcodedRuntimeData();
            return;
        }

        _processedLaunchPendingUnlocks = true;
        ProcessPendingUnlocks();
        EnforceChapterSelectLock();
        AreaMapData.ApplyHardcodedRuntimeData();
    }

    private static void OnChapterSelectUpdate(On.Celeste.OuiChapterSelect.orig_Update orig, OuiChapterSelect self)
    {
        orig(self);
        EnforceChapterSelectLock();
        AreaMapData.ApplyHardcodedRuntimeData();
    }

    private static void OnLevelExitCtor(On.Celeste.LevelExit.orig_ctor orig,
        LevelExit self, LevelExit.Mode mode, Session session, HiresSnow snow)
    {
        orig(self, mode, session, snow);

        if (mode != LevelExit.Mode.Completed || session?.Area.SID == null)
            return;

        if ((int)session.Area.Mode != 0)
            return;

        var save = MaggyHelperModule.SaveData;
        if (save == null)
            return;

        if (session.Area.SID.Equals(Ch15Sid, StringComparison.OrdinalIgnoreCase))
        {
            save.CompleteChapter(Ch15Sid);
            save.PendingUnlockChapter16OnRestart = true;

            Logger.Log(LogLevel.Info, "MaggyHelper",
                "Chapter 15 completed: queued Chapter 16 unlock for next launch and closing game now.");

            Engine.Instance.Exit();
            return;
        }

        if (session.Area.SID.Equals(Ch20Sid, StringComparison.OrdinalIgnoreCase))
        {
            save.CompleteChapter(Ch20Sid);
            save.TrueFinaleUnlocked = true;
            save.PendingUnlockChapter21OnRestart = true;

            Logger.Log(LogLevel.Info, "MaggyHelper",
                "Chapter 20 completed: queued Chapter 21 unlock for next launch and closing game now.");

            Engine.Instance.Exit();
            return;
        }
    }

    private static void ProcessPendingUnlocks()
    {
        MaggySaveDataMigration.Run();

        var save = MaggyHelperModule.SaveData;
        if (save == null)
            return;

        ApplyProgressionUnlocks(save);

        if (save.PendingUnlockChapter16OnRestart)
        {
            UnlockChapter(Ch16Sid);
            save.PendingUnlockChapter16OnRestart = false;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Processed pending unlock: Chapter 16");
        }

        if (save.PendingUnlockChapter19OnRestart)
        {
            UnlockChapter(Ch19Sid);
            save.UnlockedChapter19 = true;
            save.PendingUnlockChapter19OnRestart = false;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Processed pending unlock: Chapter 19");
        }

        if (save.PendingUnlockChapter20OnRestart)
        {
            UnlockChapter(Ch20Sid);
            save.VoidMoonUnlocked = true;
            save.PendingUnlockChapter20OnRestart = false;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Processed pending unlock: Chapter 20");
        }

        if (save.PendingUnlockChapter21OnRestart)
        {
            UnlockChapter(Ch21Sid);
            save.UnlockedChapter21 = true;
            save.TrueFinaleUnlocked = true;
            save.PendingUnlockChapter21OnRestart = false;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Processed pending unlock: Chapter 21");
        }
    }

    private static void ApplyProgressionUnlocks(MaggyHelperModuleSaveData save)
    {
        if (save.BossRushUnlocked && !MaggySaveFacade.IsChapterUnlocked(Ch19Sid))
        {
            UnlockChapter(Ch19Sid);
            save.UnlockedChapter19 = true;
            save.PendingUnlockChapter19OnRestart = false;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Boss rush progression unlocked Chapter 19.");
        }

        if (save.FinalDlcContentUnlocked && !MaggySaveFacade.IsChapterUnlocked(Ch20Sid))
        {
            UnlockChapter(Ch20Sid);
            save.VoidMoonUnlocked = true;
            save.PendingUnlockChapter20OnRestart = false;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Final DLC progression unlocked Chapter 20.");
        }

        if (save.TrueFinaleUnlocked && !MaggySaveFacade.IsChapterUnlocked(Ch21Sid))
        {
            UnlockChapter(Ch21Sid);
            save.UnlockedChapter21 = true;
            save.PendingUnlockChapter21OnRestart = false;
            Logger.Log(LogLevel.Info, "MaggyHelper", "True finale progression unlocked Chapter 21.");
        }
    }

    private static void UnlockChapter(string sid)
    {
        MaggySaveFacade.UnlockChapter(sid);
    }

    public static bool IsChapterLockedForUI(string sid)
    {
        return IsLockedChapterSID(sid);
    }

    private static void EnforceChapterSelectLock()
    {
        if (_forcingSelection || !MaggySaveFacade.IsLoaded)
            return;

        int selectedArea = MaggySaveFacade.SelectedAreaId;
        if (selectedArea < 0 || selectedArea >= AreaData.Areas.Count)
            return;

        var selectedData = AreaData.Get(selectedArea);
        if (selectedData?.SID == null || !IsLockedChapterSID(selectedData.SID))
            return;

        // Never force chapter selection changes outside our own maps.
        if (!AreaModeExtender.IsOurMap(selectedData))
            return;

        int fallbackArea = FindNearestUnlockedArea(selectedArea);
        if (fallbackArea < 0 || fallbackArea == selectedArea)
            return;

        _forcingSelection = true;
        try
        {
            MaggySaveFacade.TrySelectArea(fallbackArea);
        }
        finally
        {
            _forcingSelection = false;
        }
    }

    private static bool IsLockedChapterSID(string sid)
    {
        if (!MaggySaveFacade.HasModSave)
            return false;

        var save = MaggyHelperModule.SaveData;

        if (sid.Equals(Ch16Sid, StringComparison.OrdinalIgnoreCase))
            return !MaggySaveFacade.IsChapterUnlocked(Ch16Sid);

        if (sid.Equals(Ch19Sid, StringComparison.OrdinalIgnoreCase))
            return !MaggySaveFacade.IsChapterUnlocked(Ch19Sid)
                && save?.BossRushUnlocked != true;

        if (sid.Equals(Ch20Sid, StringComparison.OrdinalIgnoreCase))
            return !MaggySaveFacade.IsChapterUnlocked(Ch20Sid)
                && save?.FinalDlcContentUnlocked != true;

        if (sid.Equals(Ch21Sid, StringComparison.OrdinalIgnoreCase))
            return !MaggySaveFacade.IsChapterUnlocked(Ch21Sid)
                && save?.TrueFinaleUnlocked != true;

        return false;
    }

    private static int FindNearestUnlockedArea(int fromArea)
    {
        var origin = AreaData.Get(fromArea);
        if (origin?.SID == null)
            return MaggySaveFacade.SelectedAreaId;

        bool originIsOurMap = AreaModeExtender.IsOurMap(origin);
        string originPrefix = GetSidPrefix(origin.SID);

        for (int i = fromArea - 1; i >= 0; i--)
        {
            var ad = AreaData.Get(i);
            if (ad?.SID == null)
                continue;

            // Keep fallback inside the same campaign lane to avoid cross-campaign softlocks.
            if (AreaModeExtender.IsOurMap(ad) != originIsOurMap)
                continue;

            if (!string.Equals(GetSidPrefix(ad.SID), originPrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            if (IsLockedChapterSID(ad.SID))
                continue;

            return i;
        }

        return MaggySaveFacade.SelectedAreaId;
    }

    private static string GetSidPrefix(string sid)
    {
        if (string.IsNullOrEmpty(sid))
            return string.Empty;

        int slash = sid.LastIndexOf('/');
        return slash > 0 ? sid[..slash] : sid;
    }

    [Command("maggy_chapter_test", "Test late chapter unlock flow. Usage: maggy_chapter_test [status|queue16|queue19|queue20|queue21|unlock16|unlock19|unlock20|unlock21|apply]")]
    private static void CmdChapterTest(string action = "status")
    {
        var save = MaggyHelperModule.SaveData;
        if (save == null)
        {
            Engine.Commands?.Log("[MaggyHelper] SaveData is null.");
            return;
        }

        action = (action ?? "status").Trim().ToLowerInvariant();

        switch (action)
        {
            case "queue16":
                save.PendingUnlockChapter16OnRestart = true;
                Engine.Commands?.Log("[MaggyHelper] Queued Chapter 16 unlock on restart.");
                break;

            case "queue19":
                save.PendingUnlockChapter19OnRestart = true;
                Engine.Commands?.Log("[MaggyHelper] Queued Chapter 19 unlock on restart.");
                break;

            case "queue20":
                save.PendingUnlockChapter20OnRestart = true;
                Engine.Commands?.Log("[MaggyHelper] Queued Chapter 20 unlock on restart.");
                break;

            case "queue21":
                save.PendingUnlockChapter21OnRestart = true;
                Engine.Commands?.Log("[MaggyHelper] Queued Chapter 21 unlock on restart.");
                break;

            case "unlock16":
                UnlockChapter(Ch16Sid);
                save.PendingUnlockChapter16OnRestart = false;
                Engine.Commands?.Log("[MaggyHelper] Unlocked Chapter 16 immediately.");
                break;

            case "unlock19":
                UnlockChapter(Ch19Sid);
                save.UnlockedChapter19 = true;
                save.BossRushUnlocked = true;
                save.PendingUnlockChapter19OnRestart = false;
                Engine.Commands?.Log("[MaggyHelper] Unlocked Chapter 19 immediately.");
                break;

            case "unlock20":
                UnlockChapter(Ch20Sid);
                save.VoidMoonUnlocked = true;
                save.FinalDlcContentUnlocked = true;
                save.PendingUnlockChapter20OnRestart = false;
                Engine.Commands?.Log("[MaggyHelper] Unlocked Chapter 20 immediately.");
                break;

            case "unlock21":
                UnlockChapter(Ch21Sid);
                save.UnlockedChapter21 = true;
                save.TrueFinaleUnlocked = true;
                save.PendingUnlockChapter21OnRestart = false;
                Engine.Commands?.Log("[MaggyHelper] Unlocked Chapter 21 immediately.");
                break;

            case "apply":
                ProcessPendingUnlocks();
                EnforceChapterSelectLock();
                Engine.Commands?.Log("[MaggyHelper] Applied pending unlocks now.");
                break;

            case "status":
            default:
                break;
        }

        bool unlocked16 = MaggySaveFacade.IsChapterUnlocked(Ch16Sid);
        bool unlocked19 = MaggySaveFacade.IsChapterUnlocked(Ch19Sid);
        bool unlocked20 = MaggySaveFacade.IsChapterUnlocked(Ch20Sid);
        bool unlocked21 = MaggySaveFacade.IsChapterUnlocked(Ch21Sid);

        Engine.Commands?.Log(
            $"[MaggyHelper] status: unlocked16={unlocked16}, unlocked19={unlocked19}, unlocked20={unlocked20}, unlocked21={unlocked21}, " +
            $"pending16={save.PendingUnlockChapter16OnRestart}, pending19={save.PendingUnlockChapter19OnRestart}, pending20={save.PendingUnlockChapter20OnRestart}, pending21={save.PendingUnlockChapter21OnRestart}");
    }

    [Command("maggy_unlock_dside", "Unlock D-Side (or DX-Side) for all Maggy chapters. Usage: maggy_unlock_dside [dside|dxside|status]")]
    private static void CmdUnlockDSide(string mode = "dside")
    {
        var vanillaSave = SaveData.Instance;
        var maggySave = MaggyHelperModule.SaveData;

        if (vanillaSave == null || maggySave == null)
        {
            Engine.Commands?.Log("[MaggyHelper] SaveData is null — load a save file first.");
            return;
        }

        mode = (mode ?? "dside").Trim().ToLowerInvariant();

        if (mode == "status")
        {
            int count = 0;
            foreach (var ad in AreaData.Areas)
            {
                if (ad?.SID == null || !AreaModeExtender.IsOurMap(ad)) continue;
                bool dUnlocked = AreaModeExtender.IsSideUnlocked(ad.ToKey(), AreaModeExtender.MODE_DSIDE);
                bool dxUnlocked = AreaModeExtender.IsSideUnlocked(ad.ToKey(), AreaModeExtender.MODE_DXSIDE);
                Engine.Commands?.Log($"  {ad.SID}: D-Side={dUnlocked}, DX-Side={dxUnlocked}");
                count++;
            }
            Engine.Commands?.Log($"[MaggyHelper] Checked {count} Maggy chapter(s).");
            return;
        }

        bool unlockDX = mode == "dxside" || mode == "all";
        int unlocked = 0;

        foreach (var ad in AreaData.Areas)
        {
            if (ad?.SID == null || !AreaModeExtender.IsOurMap(ad)) continue;

            if (AreaModeExtender.TryGetSaveAreaStats(ad.ID) == null) continue;

            // Mark A, B, C-Side hearts collected so IsSideUnlocked returns true for D-Side
            for (int m = 0; m < Math.Min(3, AreaModeExtender.GetSaveAreaModeCount(ad.ID)); m++)
            {
                AreaModeExtender.SetSaveAreaModeHeartGem(ad.ID, m, true);
            }

            if (unlockDX)
            {
                // Mark D-Side heart in custom save data so DX-Side also unlocks
                string dHeartId = $"{ad.SID}_{AreaModeExtender.GetModeName(AreaModeExtender.MODE_DSIDE)}";
                maggySave.CollectHeartGem(dHeartId);
            }

            unlocked++;
        }

        Engine.Commands?.Log(
            $"[MaggyHelper] D-Side unlocked for {unlocked} chapter(s)" +
            (unlockDX ? " (and DX-Side)" : "") +
            ". Reopen the chapter select to see changes.");
    }
}
