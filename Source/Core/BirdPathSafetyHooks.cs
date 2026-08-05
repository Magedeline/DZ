using System;
using Monocle;

namespace Celeste.Mod.DZ;

/// <summary>
/// Protects against malformed BirdPath entities crashing room updates.
/// </summary>
public static class BirdPathSafetyHooks {
    private static bool loaded;

    public static void Load() {
        if (loaded)
            return;

        On.Celeste.BirdPath.Update += OnBirdPathUpdate;
        loaded = true;
    }

    public static void Unload() {
        if (!loaded)
            return;

        On.Celeste.BirdPath.Update -= OnBirdPathUpdate;
        loaded = false;
    }

    private static void OnBirdPathUpdate(On.Celeste.BirdPath.orig_Update orig, global::Celeste.BirdPath self) {
        try {
            orig(self);
        } catch (IndexOutOfRangeException ex) {
            Logger.Log(LogLevel.Warn, "DZ", $"[BirdPathSafetyHooks] Removed invalid BirdPath at {self.Position} after IndexOutOfRangeException: {ex.Message}");
            self.RemoveSelf();
        }
    }
}
