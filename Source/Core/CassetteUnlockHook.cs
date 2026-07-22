using System;
using Monocle;

namespace DZ;

/// <summary>
/// Hooks cassette/tape collection so that collecting a cassette in a DZ map
/// unlocks the C-Side for that chapter when the level ends or is completed.
/// </summary>
public static class CassetteUnlockHook
{
    private static bool _loaded;

    public static void Load()
    {
        if (_loaded)
            return;
        _loaded = true;

        On.Celeste.LevelExit.ctor += OnLevelExitCtor;
        Logger.Log(LogLevel.Info, "DZ", "CassetteUnlockHook loaded");
    }

    public static void Unload()
    {
        if (!_loaded)
            return;
        _loaded = false;

        On.Celeste.LevelExit.ctor -= OnLevelExitCtor;
    }

    private static void OnLevelExitCtor(On.Celeste.LevelExit.orig_ctor orig, global::Celeste.LevelExit self,
        global::Celeste.LevelExit.Mode mode, global::Celeste.Session session, HiresSnow snow)
    {
        orig(self, mode, session, snow);

        if (mode != global::Celeste.LevelExit.Mode.Completed || session == null)
            return;

        if (!session.Cassette)
            return;

        AreaData area = AreaData.Get(session.Area);
        if (area == null || !AreaModeExtender.IsOurMap(area))
            return;

        int cSideMode = AreaModeExtender.MODE_2;
        if (area.Mode == null || cSideMode >= area.Mode.Length || area.Mode[cSideMode] == null)
            return;

        string unlockKey = $"{area.SID}_{AreaModeExtender.GetModeName(cSideMode)}_unlocked";
        if (DZModule.SaveData?.HasAchievement(unlockKey) == true)
            return;

        DZModule.SaveData?.UnlockAchievement(unlockKey);
        AreaModeExtender.SetPendingSideReturn(area.ID, cSideMode);

        Logger.Log(LogLevel.Info, "DZ/CassetteUnlockHook",
            $"C-Side unlocked via cassette for {area.SID}");

        Audio.Play("event:/DZ/ui/postgame/unlock_cside");

        Engine.Scene = new SideUnlockVignette(session, (int)session.Area.Mode);
    }
}
