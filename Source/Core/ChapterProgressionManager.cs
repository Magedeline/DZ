namespace DZ;

using System.Collections;
using System.Runtime.CompilerServices;
using MonoMod.Utils;

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
    private static readonly string Ch9Sid  = AreaModeExtender.BuildASideSID("09_Summit");
    private static readonly string Ch10Sid = AreaModeExtender.BuildASideSID("10_Ruins");
    private static readonly string Ch15Sid = AreaModeExtender.BuildASideSID("15_Castle");
    private static readonly string Ch16Sid = AreaModeExtender.BuildASideSID("16_Corruption");
    private static readonly string Ch18Sid = AreaModeExtender.BuildASideSID("18_Heart");
    private static readonly string Ch19Sid = AreaModeExtender.BuildASideSID("19_Space");
    private static readonly string Ch20Sid = AreaModeExtender.BuildASideSID("20_TheEnd");
    private static readonly string Ch21Sid = AreaModeExtender.BuildASideSID("21_LastLevel");

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
        On.Celeste.OuiChapterSelect.PerformCh8Unlock += OnPerformCh8Unlock;
        On.Celeste.OuiChapterSelect.PerformCh9Unlock += OnPerformCh9Unlock;
        On.Celeste.OuiChapterSelect.Enter += OnChapterSelectEnter;

        Logger.Log(LogLevel.Info, "DZ", "ChapterProgressionManager loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;

        On.Celeste.Overworld.Begin -= OnOverworldBegin;
        On.Celeste.LevelExit.ctor -= OnLevelExitCtor;
        On.Celeste.OuiChapterSelect.Update -= OnChapterSelectUpdate;
        On.Celeste.OuiChapterSelect.PerformCh8Unlock -= OnPerformCh8Unlock;
        On.Celeste.OuiChapterSelect.PerformCh9Unlock -= OnPerformCh9Unlock;
        On.Celeste.OuiChapterSelect.Enter -= OnChapterSelectEnter;

        Logger.Log(LogLevel.Info, "DZ", "ChapterProgressionManager unloaded");
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

    private static IEnumerator OnChapterSelectEnter(On.Celeste.OuiChapterSelect.orig_Enter orig, OuiChapterSelect self, Oui from)
    {
        yield return new SwapImmediately(orig(self, from));

        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (save == null || save.PendingCSideUnlockIDs.Count == 0)
            yield break;

        var pending = new List<string>(save.PendingCSideUnlockIDs);
        save.PendingCSideUnlockIDs.Clear();

        DynamicData dd = new DynamicData(self);
        var icons = dd.Get<List<OuiChapterSelectIcon>>("icons");
        if (icons == null)
            yield break;

        foreach (string cSideId in pending)
        {
            OuiChapterSelectIcon icon = FindCSideIcon(icons, cSideId);
            if (icon == null)
                continue;

            Audio.Play("event:/Mods/pusheen/ui/postgame/unlock_cside");

            bool ready = false;
            icon.HighlightUnlock(delegate { ready = true; });
            while (!ready)
                yield return null;

            yield return 0.2f;

            Logger.Log(LogLevel.Info, "DZ", $"C-Side unlock animation played for: {cSideId}");
        }
    }

    private static OuiChapterSelectIcon FindCSideIcon(List<OuiChapterSelectIcon> icons, string cSideId)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            var icon = icons[i];
            if (icon == null)
                continue;
                
            try
            {
                DynamicData iconData = new DynamicData(icon);
                if (iconData == null)
                    continue;
                
                // Use Get with null check instead of TryGet
                object areaObj = iconData.Get<object>("area");
                object modeObj = iconData.Get<object>("mode");
                
                if (areaObj == null || modeObj == null)
                    continue;
                
                int area = Convert.ToInt32(areaObj);
                int mode = Convert.ToInt32(modeObj);

                if (mode != 2)
                    continue;

                if (area < 0 || area >= AreaData.Areas.Count)
                    continue;

                var areaData = AreaData.Areas[area];
                if (areaData?.SID == null)
                    continue;

                string sid = areaData.SID;
                if (sid.Equals(cSideId, StringComparison.OrdinalIgnoreCase) ||
                    sid.Contains(cSideId, StringComparison.OrdinalIgnoreCase))
                    return icon;
            }
            catch
            {
                // Skip any icon that causes an exception
                continue;
            }
        }
        return null;
    }

    private static IEnumerator OnPerformCh8Unlock(On.Celeste.OuiChapterSelect.orig_PerformCh8Unlock orig, OuiChapterSelect self)
    {
        IEnumerator inner = orig(self);
        while (inner.MoveNext())
            yield return inner.Current;

        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (save == null || global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch18Sid))
            yield break;

        Audio.Play("event:/Mods/pusheen/ui/postgame/unlock_newchapter");
        Audio.Play("event:/Mods/pusheen/ui/postgame/unlock_newchapter");
        Audio.Play("event:/ui/world_map/icon/roll_right");

        DynamicData dd = new DynamicData(self);
        dd.Set("area", 18);
        EaseCamera(self);
        self.Overworld.Maddy.Hide();

        bool ready = false;
        var icons = dd.Get<List<OuiChapterSelectIcon>>("icons");
        if (icons != null && icons.Count > 9)
        {
            icons[9].HighlightUnlock(delegate { ready = true; });
            while (!ready)
                yield return null;
        }

        save.BossRushUnlocked = true;
        UnlockChapter(Ch18Sid);
        Logger.Log(LogLevel.Info, "DZ", "PerformCh8Unlock: new content (Ch18) unlocked via chapter select animation.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerator OnPerformCh9Unlock(On.Celeste.OuiChapterSelect.orig_PerformCh9Unlock orig, OuiChapterSelect self, bool easeCamera = true)
    {
        IEnumerator inner = orig(self, easeCamera);
        while (inner.MoveNext())
            yield return inner.Current;

        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (save == null || global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch19Sid))
            yield break;

        Audio.Play("event:/Mods/pusheen/ui/postgame/unlock_newchapter");
        Audio.Play("event:/Mods/pusheen/ui/postgame/unlock_finalchapter_icon");
        Audio.Play("event:/ui/world_map/icon/roll_right");

        DynamicData dd = new DynamicData(self);
        dd.Set("area", 19);

        yield return 0.25f;

        bool ready = false;
        var icons = dd.Get<List<OuiChapterSelectIcon>>("icons");
        if (icons != null && icons.Count > 10)
        {
            icons[10].HighlightUnlock(delegate { ready = true; });
            while (!ready)
                yield return null;
        }

        if (easeCamera)
            EaseCamera(self);

        self.Overworld.Maddy.Hide();

        save.FinalDlcContentUnlocked = true;
        save.UnlockedChapter19 = true;
        UnlockChapter(Ch19Sid);
        UnlockChapter(Ch20Sid);
        UnlockChapter(Ch21Sid);
        save.VoidMoonUnlocked = true;
        save.TrueFinaleUnlocked = true;
        save.UnlockedChapter21 = true;
        SaveData.Instance.RevealedChapter9 = true;
        Logger.Log(LogLevel.Info, "DZ", "PerformCh9Unlock: final content (Ch19-21) unlocked via chapter select animation.");

        // Chain into the Ch10 (Ruins) flipped unlock if it hasn't happened yet
        if (!global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch10Sid))
        {
            IEnumerator ch10 = PerformCh10Unlock(self);
            while (ch10.MoveNext())
                yield return ch10.Current;
        }
    }

    private static IEnumerator PerformCh10Unlock(OuiChapterSelect self)
    {
        Audio.Play("event:/Mods/pusheen/ui/postgame/unlock_newchapter");
        Audio.Play("event:/Mods/pusheen/ui/postgame/unlock_newchapter");
        Audio.Play("event:/ui/world_map/icon/roll_left");

        DynamicData dd = new DynamicData(self);
        dd.Set("area", 10);

        // Flipped order: camera eases first, then the delay, then icon animates, Maddy hides last
        EaseCamera(self);

        yield return 0.25f;

        bool ready = false;
        var icons = dd.Get<List<OuiChapterSelectIcon>>("icons");
        if (icons != null && icons.Count > 10)
        {
            icons[10].HighlightUnlock(delegate { ready = true; });
            while (!ready)
                yield return null;
        }

        // Flipped ending: Maggy appears on the DZ mountain instead of Maddy hiding
        self.Overworld.Maddy.Running();
        var connector = self.Overworld.Entities.FindFirst<global::Celeste.OverworldConnector>();
        connector?.EnableMaggyMarker();

        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (save != null)
        {
            save.UnlockedChapter10 = true;
            save.PendingUnlockChapter10OnRestart = false;
        }
        UnlockChapter(Ch10Sid);
        Logger.Log(LogLevel.Info, "DZ", "PerformCh10Unlock: Chapter 10 (Ruins) unlocked via flipped chapter select animation.");
    }

    private static void EaseCamera(OuiChapterSelect self)
    {
        DynamicData dd = new DynamicData(self);
        if (dd.TryGet<System.Action>("EaseCamera", out var easeCam) && easeCam != null)
        {
            easeCam();
            return;
        }
        // Fallback: invoke via reflection
        self.GetType()
            .GetMethod("EaseCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(self, null);
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

        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (save == null)
            return;

        if (session.Area.SID.Equals(Ch9Sid, StringComparison.OrdinalIgnoreCase))
        {
            if (!global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch10Sid))
            {
                save.PendingUnlockChapter10OnRestart = true;
                Logger.Log(LogLevel.Info, "DZ",
                    "Chapter 9 completed: queued Chapter 10 (Ruins) unlock for next launch.");
            }
            return;
        }

        if (session.Area.SID.Equals(Ch15Sid, StringComparison.OrdinalIgnoreCase))
        {
            save.CompleteChapter(Ch15Sid);
            save.PendingUnlockChapter16OnRestart = true;

            Logger.Log(LogLevel.Info, "DZ",
                "Chapter 15 completed: queued Chapter 16 unlock for next launch and closing game now.");

            Engine.Instance.Exit();
            return;
        }

        if (session.Area.SID.Equals(Ch20Sid, StringComparison.OrdinalIgnoreCase))
        {
            save.CompleteChapter(Ch20Sid);
            save.TrueFinaleUnlocked = true;
            save.PendingUnlockChapter21OnRestart = true;

            Logger.Log(LogLevel.Info, "DZ",
                "Chapter 20 completed: queued Chapter 21 unlock for next launch and closing game now.");

            Engine.Instance.Exit();
            return;
        }
    }

    private static void ProcessPendingUnlocks()
    {
        global::Celeste.Helpers.MaggySaveDataMigration.Run();

        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (save == null)
            return;

        ApplyProgressionUnlocks(save);

        if (save.PendingUnlockChapter10OnRestart)
        {
            UnlockChapter(Ch10Sid);
            save.UnlockedChapter10 = true;
            save.PendingUnlockChapter10OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "Processed pending unlock: Chapter 10 (Ruins)");
        }

        if (save.PendingUnlockChapter16OnRestart)
        {
            UnlockChapter(Ch16Sid);
            save.PendingUnlockChapter16OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "Processed pending unlock: Chapter 16");
        }

        if (save.PendingUnlockChapter19OnRestart)
        {
            UnlockChapter(Ch19Sid);
            save.UnlockedChapter19 = true;
            save.PendingUnlockChapter19OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "Processed pending unlock: Chapter 19");
        }

        if (save.PendingUnlockChapter20OnRestart)
        {
            UnlockChapter(Ch20Sid);
            save.VoidMoonUnlocked = true;
            save.PendingUnlockChapter20OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "Processed pending unlock: Chapter 20");
        }

        if (save.PendingUnlockChapter21OnRestart)
        {
            UnlockChapter(Ch21Sid);
            save.UnlockedChapter21 = true;
            save.TrueFinaleUnlocked = true;
            save.PendingUnlockChapter21OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "Processed pending unlock: Chapter 21");
        }
    }

    private static void ApplyProgressionUnlocks(global::Celeste.Mod.DZ.DZModuleSaveData save)
    {
        if (save.UnlockedChapter10 && !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch10Sid))
        {
            UnlockChapter(Ch10Sid);
            save.PendingUnlockChapter10OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "Chapter 9 progression unlocked Chapter 10 (Ruins).");
        }

        if (save.BossRushUnlocked && !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch19Sid))
        {
            UnlockChapter(Ch19Sid);
            save.UnlockedChapter19 = true;
            save.PendingUnlockChapter19OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "Boss rush progression unlocked Chapter 19.");
        }

        if (save.FinalDlcContentUnlocked && !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch20Sid))
        {
            UnlockChapter(Ch20Sid);
            save.VoidMoonUnlocked = true;
            save.PendingUnlockChapter20OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "Final DLC progression unlocked Chapter 20.");
        }

        if (save.TrueFinaleUnlocked && !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch21Sid))
        {
            UnlockChapter(Ch21Sid);
            save.UnlockedChapter21 = true;
            save.PendingUnlockChapter21OnRestart = false;
            Logger.Log(LogLevel.Info, "DZ", "True finale progression unlocked Chapter 21.");
        }
    }

    private static void UnlockChapter(string sid)
    {
        global::Celeste.DZSaveFacade.UnlockChapter(sid);
    }

    public static bool IsChapterLockedForUI(string sid)
    {
        return IsLockedChapterSID(sid);
    }

    private static void EnforceChapterSelectLock()
    {
        if (_forcingSelection || !global::Celeste.DZSaveFacade.IsLoaded())
            return;

        int selectedArea = global::Celeste.DZSaveFacade.SelectedAreaId;
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
            global::Celeste.DZSaveFacade.TrySelectArea(fallbackArea);
        }
        finally
        {
            _forcingSelection = false;
        }
    }

    private static bool IsLockedChapterSID(string sid)
    {
        if (!global::Celeste.DZSaveFacade.HasModSave())
            return false;

        var save = global::Celeste.Mod.DZ.DZModule.SaveData;

        if (sid.Equals(Ch10Sid, StringComparison.OrdinalIgnoreCase))
            return !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch10Sid)
                && save?.UnlockedChapter10 != true;

        if (sid.Equals(Ch16Sid, StringComparison.OrdinalIgnoreCase))
            return !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch16Sid);

        if (sid.Equals(Ch19Sid, StringComparison.OrdinalIgnoreCase))
            return !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch19Sid)
                && save?.BossRushUnlocked != true;

        if (sid.Equals(Ch20Sid, StringComparison.OrdinalIgnoreCase))
            return !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch20Sid)
                && save?.FinalDlcContentUnlocked != true;

        if (sid.Equals(Ch21Sid, StringComparison.OrdinalIgnoreCase))
            return !global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch21Sid)
                && save?.TrueFinaleUnlocked != true;

        return false;
    }

    private static int FindNearestUnlockedArea(int fromArea)
    {
        var origin = AreaData.Get(fromArea);
        if (origin?.SID == null)
            return global::Celeste.DZSaveFacade.SelectedAreaId;

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

        return global::Celeste.DZSaveFacade.SelectedAreaId;
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
        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (save == null)
        {
            Engine.Commands?.Log("[DZ] SaveData is null.");
            return;
        }

        action = (action ?? "status").Trim().ToLowerInvariant();

        switch (action)
        {
            case "queue16":
                save.PendingUnlockChapter16OnRestart = true;
                Engine.Commands?.Log("[DZ] Queued Chapter 16 unlock on restart.");
                break;

            case "queue19":
                save.PendingUnlockChapter19OnRestart = true;
                Engine.Commands?.Log("[DZ] Queued Chapter 19 unlock on restart.");
                break;

            case "queue20":
                save.PendingUnlockChapter20OnRestart = true;
                Engine.Commands?.Log("[DZ] Queued Chapter 20 unlock on restart.");
                break;

            case "queue21":
                save.PendingUnlockChapter21OnRestart = true;
                Engine.Commands?.Log("[DZ] Queued Chapter 21 unlock on restart.");
                break;

            case "unlock16":
                UnlockChapter(Ch16Sid);
                save.PendingUnlockChapter16OnRestart = false;
                Engine.Commands?.Log("[DZ] Unlocked Chapter 16 immediately.");
                break;

            case "unlock19":
                UnlockChapter(Ch19Sid);
                save.UnlockedChapter19 = true;
                save.BossRushUnlocked = true;
                save.PendingUnlockChapter19OnRestart = false;
                Engine.Commands?.Log("[DZ] Unlocked Chapter 19 immediately.");
                break;

            case "unlock20":
                UnlockChapter(Ch20Sid);
                save.VoidMoonUnlocked = true;
                save.FinalDlcContentUnlocked = true;
                save.PendingUnlockChapter20OnRestart = false;
                Engine.Commands?.Log("[DZ] Unlocked Chapter 20 immediately.");
                break;

            case "unlock21":
                UnlockChapter(Ch21Sid);
                save.UnlockedChapter21 = true;
                save.TrueFinaleUnlocked = true;
                save.PendingUnlockChapter21OnRestart = false;
                Engine.Commands?.Log("[DZ] Unlocked Chapter 21 immediately.");
                break;

            case "apply":
                ProcessPendingUnlocks();
                EnforceChapterSelectLock();
                Engine.Commands?.Log("[DZ] Applied pending unlocks now.");
                break;

            case "status":
            default:
                break;
        }

        bool unlocked16 = global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch16Sid);
        bool unlocked19 = global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch19Sid);
        bool unlocked20 = global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch20Sid);
        bool unlocked21 = global::Celeste.DZSaveFacade.IsChapterUnlocked(Ch21Sid);

        Engine.Commands?.Log(
            $"[DZ] status: unlocked16={unlocked16}, unlocked19={unlocked19}, unlocked20={unlocked20}, unlocked21={unlocked21}, " +
            $"pending16={save.PendingUnlockChapter16OnRestart}, pending19={save.PendingUnlockChapter19OnRestart}, pending20={save.PendingUnlockChapter20OnRestart}, pending21={save.PendingUnlockChapter21OnRestart}");
    }

    [Command("maggy_unlock_dside", "Unlock D-Side (or DX-Side) for all Maggy chapters. Usage: maggy_unlock_dside [dside|dxside|status]")]
    private static void CmdUnlockDSide(string mode = "dside")
    {
        var vanillaSave = SaveData.Instance;
        var maggySave = global::Celeste.Mod.DZ.DZModule.SaveData;

        if (vanillaSave == null || maggySave == null)
        {
            Engine.Commands?.Log("[DZ] SaveData is null â€” load a save file first.");
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
            Engine.Commands?.Log($"[DZ] Checked {count} KIRBY_CELESTE chapter(s).");
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
            $"[DZ] D-Side unlocked for {unlocked} chapter(s)" +
            (unlockDX ? " (and DX-Side)" : "") +
            ". Reopen the chapter select to see changes.");
    }

    [Command("maggy_unlock_all", "Unlock all late chapters (18-21) at once.")]
    private static void CmdUnlockAll()
    {
        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (save == null)
        {
            Engine.Commands?.Log("[DZ] SaveData is null â€” load a save file first.");
            return;
        }

        // Unlock all chapters immediately
        UnlockChapter(Ch18Sid);
        save.BossRushUnlocked = true;

        UnlockChapter(Ch19Sid);
        save.UnlockedChapter19 = true;
        save.FinalDlcContentUnlocked = true;

        UnlockChapter(Ch20Sid);
        save.VoidMoonUnlocked = true;

        UnlockChapter(Ch21Sid);
        save.UnlockedChapter21 = true;
        save.TrueFinaleUnlocked = true;

        // Clear any pending flags
        save.PendingUnlockChapter19OnRestart = false;
        save.PendingUnlockChapter20OnRestart = false;
        save.PendingUnlockChapter21OnRestart = false;

        Engine.Commands?.Log("[DZ] All chapters (18, 19, 20, 21) unlocked!");
        Engine.Commands?.Log("  - Chapter 18 (Heart): Boss Rush unlocked");
        Engine.Commands?.Log("  - Chapter 19 (Space): Unlocked");
        Engine.Commands?.Log("  - Chapter 20 (The End): Void Moon unlocked");
        Engine.Commands?.Log("  - Chapter 21 (Last Level): True Finale unlocked");
        Engine.Commands?.Log("Reopen chapter select to see changes.");
    }

    [Command("maggy_reset_chapters", "Reset all chapter unlocks (18-21) for testing. Usage: maggy_reset_chapters [confirm]")]
    private static void CmdResetChapters(string confirm = "")
    {
        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        var vanillaSave = SaveData.Instance;
        if (save == null || vanillaSave == null)
        {
            Engine.Commands?.Log("[DZ] SaveData is null â€” load a save file first.");
            return;
        }

        if (confirm.ToLowerInvariant() != "confirm")
        {
            Engine.Commands?.Log("[DZ] WARNING: This will reset chapter 18-21 unlocks!");
            Engine.Commands?.Log("Run 'maggy_reset_chapters confirm' to proceed.");
            return;
        }

        // Reset save flags
        save.BossRushUnlocked = false;
        save.UnlockedChapter19 = false;
        save.FinalDlcContentUnlocked = false;
        save.VoidMoonUnlocked = false;
        save.UnlockedChapter21 = false;
        save.TrueFinaleUnlocked = false;

        // Reset pending flags
        save.PendingUnlockChapter16OnRestart = false;
        save.PendingUnlockChapter19OnRestart = false;
        save.PendingUnlockChapter20OnRestart = false;
        save.PendingUnlockChapter21OnRestart = false;

        // Lock chapters by removing from UnlockedModes
        void LockChapter(string sid)
        {
            string unlockKey = "chapter_unlocked:" + sid;
            save.UnlockedModes.Remove(unlockKey);
        }

        LockChapter(Ch10Sid);
        LockChapter(Ch16Sid);
        LockChapter(Ch18Sid);
        LockChapter(Ch19Sid);
        LockChapter(Ch20Sid);
        LockChapter(Ch21Sid);

        Engine.Commands?.Log("[DZ] Chapter unlocks (18-21) have been reset!");
        Engine.Commands?.Log("Reopen chapter select to see changes.");
    }

    [Command("maggy_mountain_warp", "Warp to the Desolo Zantas mountain (A-Side).")]
    private static void CmdMountainWarp()
    {
        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        var vanillaSave = SaveData.Instance;
        if (save == null || vanillaSave == null)
        {
            Engine.Commands?.Log("[DZ] SaveData is null â€” load a save file first.");
            return;
        }

        // Find first Maggy chapter to warp to
        AreaData targetArea = null;
        foreach (var area in AreaData.Areas)
        {
            if (area?.SID != null && AreaModeExtender.IsOurMap(area))
            {
                targetArea = area;
                break;
            }
        }

        if (targetArea == null)
        {
            Engine.Commands?.Log("[DZ] No KIRBY_CELESTE chapters found!");
            return;
        }

        // Set the last area to our chapter
        vanillaSave.LastArea = targetArea.ToKey();
        vanillaSave.LastArea_Safe = targetArea.ToKey();

        // If in overworld, force camera to update
        if (Engine.Scene is Overworld overworld)
        {
            overworld.Mountain?.EaseCamera(targetArea.ID, targetArea.MountainSelect, null, false);
            Engine.Commands?.Log($"[DZ] Warped to DZ mountain at chapter: {targetArea.SID}");
        }
        else
        {
            Engine.Commands?.Log($"[DZ] Set target to DZ mountain: {targetArea.SID}");
            Engine.Commands?.Log("Return to overworld to see the DZ mountain.");
        }
    }

    [Command("maggy_unlock_dz", "Unlock the Desolo Zantas campaign/mountain access.")]
    private static void CmdUnlockDZ()
    {
        var save = global::Celeste.Mod.DZ.DZModule.SaveData;
        var vanillaSave = SaveData.Instance;
        if (save == null || vanillaSave == null)
        {
            Engine.Commands?.Log("[DZ] SaveData is null â€” load a save file first.");
            return;
        }

        // Unlock Chapter 10 (Ruins) which acts as entry point to DZ
        save.UnlockedChapter10 = true;
        save.PendingUnlockChapter10OnRestart = false;
        UnlockChapter(Ch10Sid);

        // Also unlock early Maggy chapters by finding them
        int unlocked = 0;
        foreach (var area in AreaData.Areas)
        {
            if (area?.SID != null && AreaModeExtender.IsOurMap(area))
            {
                if (!global::Celeste.DZSaveFacade.IsChapterUnlocked(area.SID))
                {
                    UnlockChapter(area.SID);
                    unlocked++;
                }
            }
        }

        Engine.Commands?.Log($"[DZ] Desolo Zantas campaign unlocked!");
        Engine.Commands?.Log($"  - Unlocked {unlocked} KIRBY_CELESTE chapter(s)");
        Engine.Commands?.Log($"  - Chapter 10 (Ruins) accessible");
        Engine.Commands?.Log("Reopen chapter select or restart the game to see the DZ mountain.");
    }
}

