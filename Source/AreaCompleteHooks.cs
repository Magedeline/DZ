namespace MaggyHelper;

using Celeste.Mod.Meta;
using MonoMod.Utils;

/// <summary>
/// Redirects completed Maggy chapters from the vanilla AreaComplete scene to the
/// modded AreaComplete scene once LevelExit finishes building the completion screen.
/// </summary>
public static class AreaCompleteHooks
{
    private static bool _hooked;

    public static void Load()
    {
        if (_hooked)
            return;

        _hooked = true;
        On.Celeste.LevelExit.Routine += OnLevelExitRoutine;

        Logger.Log(LogLevel.Info, "MaggyHelper", "AreaCompleteHooks loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;
        On.Celeste.LevelExit.Routine -= OnLevelExitRoutine;

        Logger.Log(LogLevel.Info, "MaggyHelper", "AreaCompleteHooks unloaded");
    }

    private static IEnumerator OnLevelExitRoutine(On.Celeste.LevelExit.orig_Routine orig, LevelExit self)
    {
        IEnumerator routine = orig(self);

        while (routine.MoveNext())
            yield return routine.Current;

        TrySwapToCustomAreaComplete(self);
    }

    private static void TrySwapToCustomAreaComplete(LevelExit self)
    {
        if (self?.mode != LevelExit.Mode.Completed || self.session == null)
            return;

        AreaData area = AreaData.Get(self.session.Area);
        if (!AreaModeExtender.IsOurMap(area))
            return;

        if (Engine.Scene is not global::Celeste.AreaComplete)
            return;

        if (self.completeXml == null || self.completeAtlas == null)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"AreaCompleteHooks: custom screen skipped for '{area?.SID}' because complete screen assets were unavailable.");
            return;
        }

        DynamicData data = new DynamicData(self);
        MapMetaCompleteScreen completeMeta = data.TryGet<MapMetaCompleteScreen>("completeMeta", out MapMetaCompleteScreen meta)
            ? meta
            : null;

        Engine.Scene = new AreaComplete(self.session, self.completeXml, self.completeAtlas, self.snow, completeMeta);
    }
}