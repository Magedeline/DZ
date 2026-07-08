using System;
using global::Celeste.Mod.DZ;
using Celeste.Extensions;
using Monocle;

namespace DZ;

/// <summary>
/// Compatibility shim for the vanilla player when Kirby mode is active.
///
/// With the NEW architecture the vanilla <c>global::Celeste.Player</c> stays
/// authoritative; Kirby mechanics are layered via components. This shim is
/// therefore a no-op placeholder kept for backward compatibility with any
/// code that still references <c>PlayerCompatShim</c>.
/// </summary>
public static class PlayerCompatShim
{
    public static void Load()
    {
        Logger.Log(LogLevel.Info, "DZ",
            "[PlayerCompatShim] Loaded (no-op — vanilla player remains authoritative)");
    }

    public static void Unload()
    {
        Logger.Log(LogLevel.Info, "DZ",
            "[PlayerCompatShim] Unloaded");
    }

    /// <summary>
    /// Always returns true when Kirby mode is active on the vanilla player.
    /// </summary>
    public static bool IsDZPlayerActive(Level level)
    {
        if (level == null)
            return false;

        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        return vanillaPlayer != null && vanillaPlayer.IsKirbyMode();
    }

    /// <summary>
    /// Returns the vanilla player when Kirby mode is active, otherwise null.
    /// </summary>
    public static CelestePlayer GetActivePlayer(Level level)
    {
        if (level == null)
            return null;

        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        return (vanillaPlayer != null && vanillaPlayer.IsKirbyMode())
            ? vanillaPlayer
            : null;
    }
}