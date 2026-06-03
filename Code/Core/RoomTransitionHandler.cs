using System;
using global::Celeste.Mod.KIRBY_CELESTE;
using Celeste.Entities;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

/// <summary>
/// Handles room transitions when Kirby mode is active.
///
/// This uses the NEW architecture: the vanilla <c>global::Celeste.Player</c>
/// remains authoritative and Kirby mechanics are layered via
/// <see cref="KirbyPlayerController"/> / <see cref="KirbyPlayerSpriteController"/>.
///
/// Hooks:
///   • <c>Level.LoadLevel</c>  — after a room transition, restore Kirby state
///     on the vanilla player if session says Kirby mode is active.
///   • <c>Level.TransitionTo</c> — persist Kirby state across room transitions.
/// </summary>
public static class RoomTransitionHandler
{
    public static void Load()
    {
        On.Celeste.Level.LoadLevel += Hook_Level_LoadLevel;
        Everest.Events.Level.OnTransitionTo += OnTransitionTo;

        Logger.Log(LogLevel.Info, "KIRBY_CELESTE",
            "[RoomTransitionHandler] Hooks loaded");
    }

    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= Hook_Level_LoadLevel;
        Everest.Events.Level.OnTransitionTo -= OnTransitionTo;

        Logger.Log(LogLevel.Info, "KIRBY_CELESTE",
            "[RoomTransitionHandler] Hooks unloaded");
    }

    private static void Hook_Level_LoadLevel(
        On.Celeste.Level.orig_LoadLevel orig,
        Level self,
        CelestePlayer.IntroTypes playerIntro,
        bool isFromLoader)
    {
        orig(self, playerIntro, isFromLoader);

        bool hasSpawner = self.Tracker.GetEntities<global::Celeste.Entities.KirbyPlayerSpawner>().Count > 0;
        bool sessionKirby = global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule.Session?.IsKirbyModeActive == true;

        // If there's no spawner but session says Kirby mode is active,
        // restore Kirby state on the vanilla player.
        if (!hasSpawner && sessionKirby)
        {
            var player = self.Tracker.GetEntity<CelestePlayer>();
            if (player != null)
            {
                player.RestorePersistentState();

                Logger.Log(LogLevel.Info, "KIRBY_CELESTE",
                    "[RoomTransitionHandler] Restored Kirby state on vanilla player after transition");
            }
        }
    }

    private static void OnTransitionTo(
        Level level,
        LevelData next,
        Vector2 direction)
    {
        var session = global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule.Session;
        if (session == null)
            return;

        // Persist Kirby state into session so the next room knows to restore it
        if (session.IsKirbyModeActive)
        {
            var player = level.Tracker.GetEntity<CelestePlayer>();
            if (player != null)
            {
                level.Session.RespawnPoint = player.Position;
                Logger.Log(LogLevel.Verbose, "KIRBY_CELESTE",
                    $"[RoomTransitionHandler] Persisted Kirby respawn at {player.Position}");
            }
        }
    }
}