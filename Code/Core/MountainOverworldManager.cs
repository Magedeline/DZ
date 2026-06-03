using System;
using Celeste.Mod;

namespace Celeste;

/// <summary>
/// Integrates camera positions and music with Celeste's 3D overworld.
/// </summary>
public static class MountainOverworldManager
{
    private static bool _hooked = false;

    // Mountain states matching vanilla
    public const int STATE_NORMAL = 0;
    public const int STATE_DARK = 1;
    public const int STATE_VOID = 2;
    public const int STATE_SUMMIT = 3;

    // ── Hook Management ──────────────────────────────────────────────────

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.AreaData.Load += OnAreaDataLoad;
        On.Celeste.Overworld.SetNormalMusic += OnOverworldSetNormalMusic;

        Logger.Log(LogLevel.Info, "MaggyHelper", "MountainOverworldManager loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.AreaData.Load -= OnAreaDataLoad;
        On.Celeste.Overworld.SetNormalMusic -= OnOverworldSetNormalMusic;

        Logger.Log(LogLevel.Info, "MaggyHelper", "MountainOverworldManager unloaded");
    }

    // ── AreaData.Load Hook ───────────────────────────────────────────────

    /// <summary>
    /// After vanilla AreaData loads, apply camera data.
    /// </summary>
    private static void OnAreaDataLoad(On.Celeste.AreaData.orig_Load orig)
    {
        orig();

        ApplyMountainCameraData();
    }

    /// <summary>
    /// Applies hardcoded mountain camera data to all MaggyHelper chapters.
    /// </summary>
    private static void ApplyMountainCameraData()
    {
        try
        {
            AreaMapData.ApplyHardcodedRuntimeData();
            Logger.Log(LogLevel.Info, "MaggyHelper", "Mountain camera data applied");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"ApplyMountainCameraData failed: {ex.Message}");
        }
    }

    // ── Overworld Music Hook ─────────────────────────────────────────────

    private static void OnOverworldSetNormalMusic(On.Celeste.Overworld.orig_SetNormalMusic orig, Overworld self)
    {
        orig(self);

        // Audio.SetMusic(OverworldMusicManager.MUSIC_LEVEL_SELECT); // removed: audio system deleted
    }

    // ── Utility ──────────────────────────────────────────────────────────

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
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper/MountainOverworldManager", $"IsViewingOurChapters failed: {ex.Message}");
        }
        return false;
    }

    // ── Public API ───────────────────────────────────────────────────────

    public static AreaMapData.MountainCameraData GetCameraForChapter(int chapterNumber)
    {
        return AreaMapData.GetByNumber(chapterNumber)?.MountainData;
    }

    public static int GetMountainStateForChapter(int chapterNumber)
    {
        return AreaMapData.GetByNumber(chapterNumber)?.MountainState ?? STATE_NORMAL;
    }
}

