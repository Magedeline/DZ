using System;
using System.Reflection;
using global::Celeste.Mod.MaggyHelper;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

/// <summary>
/// Keeps the hidden vanilla <c>global::Celeste.Player</c> synchronised with
/// <see cref="MaggyHelper.Entities.Player"/> so that Celeste's built-in
/// systems (camera, transitions, etc.) keep working.
///
/// The MaggyHelper Player already handles camera, triggers, and
/// PlayerCollider detection via <c>SelfPlayer</c> / <c>Unsafe.As</c>,
/// so this shim only needs to:
///   1. Sync vanilla player position each frame.
///   2. Keep vanilla player in a safe inert state (StDummy, invisible,
///      non-collidable) so it doesn't interfere.
/// </summary>
public static class PlayerCompatShim
{
    // ─────────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────────

    public static void Load()
    {
        On.Celeste.Level.Update += Hook_Level_Update;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[PlayerCompatShim] Loaded (position sync only)");
    }

    public static void Unload()
    {
        On.Celeste.Level.Update -= Hook_Level_Update;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[PlayerCompatShim] Unloaded");
    }

    // ─────────────────────────────────────────────────
    //  POSITION SYNC
    // ─────────────────────────────────────────────────

    private static void Hook_Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        // Sync vanilla player position BEFORE the level update so that
        // Celeste's camera, parallax, and renderer code see the correct
        // player position during orig.
        SyncVanillaPlayer(self);

        orig(self);
    }

    /// <summary>
    /// Copies our player's position onto the hidden vanilla player each frame.
    /// This keeps Celeste's built-in camera, snow displacement, and other
    /// player-position-dependent systems working without custom camera math.
    /// </summary>
    private static void SyncVanillaPlayer(Level level)
    {
        if (level == null)
            return;

        var maggyPlayer = level.Tracker.GetEntity<global::Celeste.Entities.Player>();
        if (maggyPlayer == null)
            return;

        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        if (vanillaPlayer == null || vanillaPlayer.Visible)
            return; // Vanilla player is visible (in control) — don't interfere

        // Keep vanilla player at our player's position so
        // Celeste's camera and transition systems work naturally.
        vanillaPlayer.Position = maggyPlayer.Position;
    }

    // ─────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────

    /// <summary>
    /// Returns true when our MaggyHelper Player is in the scene and the
    /// vanilla player is hidden.
    /// </summary>
    public static bool IsMaggyPlayerActive(Level level)
    {
        if (level == null)
            return false;

        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        var maggyPlayer = level.Tracker.GetEntity<global::Celeste.Entities.Player>();

        // Vanilla player is hidden (Visible=false, StDummy) when our player is in control
        return maggyPlayer != null
            && vanillaPlayer != null
            && !vanillaPlayer.Visible;
    }

    /// <summary>
    /// Gets our <see cref="MaggyHelper.Entities.Player"/> if it's active,
    /// or null if vanilla controls are in use.
    /// </summary>
    public static global::Celeste.Entities.Player GetActivePlayer(Level level)
    {
        if (level == null)
            return null;

        return IsMaggyPlayerActive(level)
            ? level.Tracker.GetEntity<global::Celeste.Entities.Player>()
            : null;
    }
}