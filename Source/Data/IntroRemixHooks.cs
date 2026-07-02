using System;
using Celeste.Cutscenes;

namespace DZ;

/// <summary>
/// Hooks for intro remix cutscenes.
/// Hooks into the LevelEnter flow to show VHS intro remix cutscenes
/// when entering B-Side or C-Side levels for the first time.
/// </summary>
public static class IntroRemixHooks
{
    /// <summary>
    /// Hookable delegate for D-Side chapter entry.
    /// Subscribe to this to customize D-Side intro behavior.
    /// Return true to skip default entry (handled by subscriber).
    /// </summary>
    public delegate bool 2EnterHandler(Session session);

    /// <summary>
    /// Event invoked when entering a D-Side chapter.
    /// Hook this from anywhere in AreaMapData or other classes to customize entry.
    /// </summary>
    public static event 2EnterHandler On2Enter;

    private static bool _hooked;

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        try
        {
            On.Celeste.LevelEnter.Go += OnLevelEnterGo;
            Logger.Log(LogLevel.Info, "DZ", "[IntroRemixHooks] Loaded");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ",
                $"[IntroRemixHooks] Failed to load: {ex.Message}");
        }
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        try
        {
            On.Celeste.LevelEnter.Go -= OnLevelEnterGo;
            Logger.Log(LogLevel.Info, "DZ", "[IntroRemixHooks] Unloaded");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ",
                $"[IntroRemixHooks] Failed to unload: {ex.Message}");
        }
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

        switch (mode)
        {
            case AreaModeExtender.MODE_1:
                if (ShouldShowRemixIntro(session, mode))
                {
                    Engine.Scene = new CS_Gen_IntroRemix_1(session);
                    return;
                }
                break;

            case AreaModeExtender.MODE_2:
                if (ShouldShowRemixIntro(session, mode))
                {
                    Engine.Scene = new CS_Gen_IntroRemix_2(session);
                    return;
                }
                break;

            case AreaModeExtender.MODE_2:
                if (On2Enter != null)
                {
                    bool handled = false;
                    foreach (2EnterHandler handler in On2Enter.GetInvocationList())
                    {
                        if (handler(session))
                        {
                            handled = true;
                            break;
                        }
                    }
                    if (handled)
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
        string flagKey = $"seen_remix_intro_{session.Area.SID}_{mode}";
        bool alreadySeen = DZModule.SaveData?.HasAchievement(flagKey) == true;

        if (!alreadySeen)
        {
            DZModule.SaveData?.UnlockAchievement(flagKey);
            return true;
        }

        return false;
    }
}
