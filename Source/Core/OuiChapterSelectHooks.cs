using System;
using System.Collections;
using Monocle;

namespace DZ;

/// <summary>
/// Hooks for OuiChapterSelect initialization.
/// Safely handles chapter select entry with exception handling to prevent crashes.
/// </summary>
public static class OuiChapterSelectHooks
{
    private static bool _hooked;

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        try
        {
            On.Celeste.OuiChapterSelect.Enter += OnChapterSelectEnter;
            On.Celeste.OuiChapterSelect.Update += OnChapterSelectUpdate;
            Logger.Log(LogLevel.Info, "DZ", "[OuiChapterSelectHooks] Loaded");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ",
                $"[OuiChapterSelectHooks] Failed to load: {ex.Message}");
        }
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        try
        {
            On.Celeste.OuiChapterSelect.Enter -= OnChapterSelectEnter;
            On.Celeste.OuiChapterSelect.Update -= OnChapterSelectUpdate;
            Logger.Log(LogLevel.Info, "DZ", "[OuiChapterSelectHooks] Unloaded");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ",
                $"[OuiChapterSelectHooks] Failed to unload: {ex.Message}");
        }
    }

    private static IEnumerator OnChapterSelectEnter(
        On.Celeste.OuiChapterSelect.orig_Enter orig,
        OuiChapterSelect self,
        Oui from)
    {
        IEnumerator routine;
        try
        {
            routine = orig(self, from);
        }
        catch (NullReferenceException ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"[OuiChapterSelectHooks] Caught NullReferenceException in OuiChapterSelect.Enter: {ex.Message}");
            Logger.Log(LogLevel.Verbose, "DZ",
                $"[OuiChapterSelectHooks] Stack trace: {ex.StackTrace}");
            yield break;
        }

        while (routine.MoveNext())
        {
            yield return routine.Current;
        }
    }

    private static void OnChapterSelectUpdate(
        On.Celeste.OuiChapterSelect.orig_Update orig,
        OuiChapterSelect self)
    {
        try
        {
            orig(self);
        }
        catch (NullReferenceException ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"[OuiChapterSelectHooks] Caught NullReferenceException in OuiChapterSelect.Update: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"[OuiChapterSelectHooks] Caught exception in OuiChapterSelect.Update: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
