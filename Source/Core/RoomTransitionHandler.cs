using System;
using global::Celeste.Mod.DZ;
using Celeste.Entities;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

/// <summary>
/// Hooks for room transitions when Kirby mode is active.
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
    private static bool _hooked;

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        try
        {
            On.Celeste.Level.LoadLevel += Hook_Level_LoadLevel;
            Everest.Events.Level.OnTransitionTo += OnTransitionTo;

            Logger.Log(LogLevel.Info, "DZ",
                "[RoomTransitionHandler] Hooks loaded");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ",
                $"[RoomTransitionHandler] Failed to load: {ex.Message}");
        }
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        try
        {
            On.Celeste.Level.LoadLevel -= Hook_Level_LoadLevel;
            Everest.Events.Level.OnTransitionTo -= OnTransitionTo;

            Logger.Log(LogLevel.Info, "DZ",
                "[RoomTransitionHandler] Hooks unloaded");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ",
                $"[RoomTransitionHandler] Failed to unload: {ex.Message}");
        }
    }

    private static void Hook_Level_LoadLevel(
        On.Celeste.Level.orig_LoadLevel orig,
        Level self,
        CelestePlayer.IntroTypes playerIntro,
        bool isFromLoader)
    {
        orig(self, playerIntro, isFromLoader);

        try
        {
            bool hasSpawner = self.Tracker.GetEntities<global::Celeste.Entities.KirbyPlayerSpawner>().Count > 0;
            bool sessionKirby = global::Celeste.Mod.DZ.DZModule.Session?.IsKirbyModeActive == true;

            if (!hasSpawner && sessionKirby)
            {
                var player = self.Tracker.GetEntity<CelestePlayer>();
                if (player != null)
                {
                    player.RestorePersistentState();

                    Logger.Log(LogLevel.Info, "DZ",
                        "[RoomTransitionHandler] Restored Kirby state on vanilla player after transition");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"[RoomTransitionHandler] Error restoring Kirby state: {ex.Message}");
        }
    }

    private static void OnTransitionTo(
        Level level,
        LevelData next,
        Vector2 direction)
    {
        var session = global::Celeste.Mod.DZ.DZModule.Session;
        if (session == null)
            return;

        if (session.IsKirbyModeActive)
        {
            var player = level.Tracker.GetEntity<CelestePlayer>();
            if (player != null)
            {
                level.Session.RespawnPoint = player.Position;
                Logger.Log(LogLevel.Verbose, "DZ",
                    $"[RoomTransitionHandler] Persisted Kirby respawn at {player.Position}");
            }
        }
    }
}