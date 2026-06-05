using System;
using System.Collections;
using Monocle;

namespace Celeste;

/// <summary>
/// Hooks for OuiChapterSelect to safely handle initialization and prevent crashes.
/// </summary>
public static class OuiChapterSelectHooks
{
    public static void Load()
    {
        On.Celeste.OuiChapterSelect.Enter += OnChapterSelectEnter;
        Logger.Log(LogLevel.Info, "MaggyHelper", "[OuiChapterSelectHooks] Loaded");
    }

    public static void Unload()
    {
        On.Celeste.OuiChapterSelect.Enter -= OnChapterSelectEnter;
        Logger.Log(LogLevel.Info, "MaggyHelper", "[OuiChapterSelectHooks] Unloaded");
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
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"[OuiChapterSelectHooks] Caught NullReferenceException in OuiChapterSelect.Enter: {ex.Message}");
            Logger.Log(LogLevel.Verbose, "MaggyHelper",
                $"[OuiChapterSelectHooks] Stack trace: {ex.StackTrace}");
            // Continue without crashing - the scarf update error is non-fatal
            yield break;
        }

        while (routine.MoveNext())
        {
            yield return routine.Current;
        }
    }
}
