using System;
using Celeste.Mod.MaggyHelper;
using MaggyHelper.Entities;
using MaggyHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace MaggyHelper;

/// <summary>
/// Handles room transitions, death, and respawn for
/// <see cref="MaggyHelper.Entities.Player"/> when the vanilla player is hidden.
///
/// Hooks:
///   • <c>Level.LoadLevel</c>  — after a room transition (or reload), re-spawn
///     our Player and re-hide the vanilla one.
///   • <c>Player.Die</c>      — when vanilla player is triggered to die, relay
///     that to kill our Player instead.
///   • <c>Level.Reload</c>    — ensure a fresh our-Player after death.
///   • <c>Level.TransitionTo</c> — carry state across room transitions.
/// </summary>
public static class RoomTransitionHandler
{
    // ─────────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────────

    public static void Load()
    {
        On.Celeste.Level.LoadLevel += Hook_Level_LoadLevel;
        On.Celeste.Player.Die += Hook_Player_Die;
        Everest.Events.Level.OnTransitionTo += OnTransitionTo;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[RoomTransitionHandler] Hooks loaded");
    }

    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= Hook_Level_LoadLevel;
        On.Celeste.Player.Die -= Hook_Player_Die;
        Everest.Events.Level.OnTransitionTo -= OnTransitionTo;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[RoomTransitionHandler] Hooks unloaded");
    }

    // ─────────────────────────────────────────────────
    //  HOOK: Level.LoadLevel
    // ─────────────────────────────────────────────────

    private static void Hook_Level_LoadLevel(
        On.Celeste.Level.orig_LoadLevel orig,
        Level self,
        CelestePlayer.IntroTypes playerIntro,
        bool isFromLoader)
    {
        // Let the original run first (spawns vanilla player, loads entities)
        orig(self, playerIntro, isFromLoader);

        // Check if this level should use our Player
        // (either a KirbyPlayerSpawner exists in the room, or
        //  the session says Kirby mode is still active from a previous room)
        bool hasSpawner = self.Tracker.GetEntities<MaggyHelper.Entities.KirbyPlayerSpawner>().Count > 0;
        bool sessionKirby = MaggyHelperModule.Session?.IsKirbyModeActive == true;

        if (!hasSpawner && sessionKirby)
        {
            // No spawner in this room but session says Kirby mode is on
            // (player walked into a new room). Re-spawn our player.
            RespawnMaggyPlayer(self, playerIntro);
        }

        // If a spawner exists, it handles everything in its own Awake().
        // If neither condition is true, vanilla player is in charge — do nothing.
    }

    /// <summary>
    /// Re-spawns a <see cref="MaggyHelper.Entities.Player"/> in a new room
    /// during a session where Kirby mode is active but no
    /// <see cref="Entities.KirbyPlayerSpawner"/> is placed.
    /// </summary>
    private static void RespawnMaggyPlayer(Level level, CelestePlayer.IntroTypes playerIntro)
    {
        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        if (vanillaPlayer == null)
            return;

        // Check if our player is already there (e.g. tagged Persistent and carried over)
        var existing = level.Tracker.GetEntity<MaggyHelper.Entities.Player>();
        if (existing != null)
        {
            // Just re-hide vanilla and update position
            HideVanillaPlayer(vanillaPlayer);
            return;
        }

        Vector2 spawnPos = vanillaPlayer.Position;

        // Hide vanilla player
        HideVanillaPlayer(vanillaPlayer);

        // Spawn our player
        var maggyPlayer = new MaggyHelper.Entities.Player(
            spawnPos, PlayerSpriteMode.Madeline);
        maggyPlayer.KirbyModeActive = true;
        maggyPlayer.CombatEnabled = true;
        level.Add(maggyPlayer);

        // Restore HP state
        PlayerHealthManager.GetOrCreate(level, 6);

        // Restore copy ability from session
        var session = MaggyHelperModule.Session;
        if (session != null &&
            Enum.TryParse(session.CurrentKirbyPower, true,
                out KirbyMode.KirbyPowerState power) &&
            power != KirbyMode.KirbyPowerState.None)
        {
            var kirbyMode = level.Tracker.GetEntity<KirbyMode>();
            if (kirbyMode == null)
            {
                kirbyMode = new KirbyMode();
                level.Add(kirbyMode);
            }
            kirbyMode.SetPowerState(power);
        }

        IngesteLogger.Info(
            $"[RoomTransitionHandler] Re-spawned MaggyHelper.Player at {spawnPos} " +
            $"(intro: {playerIntro})");
    }

    // ─────────────────────────────────────────────────
    //  HOOK: Player.Die
    // ─────────────────────────────────────────────────

    private static PlayerDeadBody Hook_Player_Die(
        On.Celeste.Player.orig_Die orig,
        CelestePlayer self,
        Vector2 direction,
        bool evenIfInvincible,
        bool registerDeathInStats)
    {
        // If vanilla player is hidden, relay the death to our player
        if (self.Scene is Level level && PlayerCompatShim.IsMaggyPlayerActive(level))
        {
            var maggyPlayer = level.Tracker.GetEntity<MaggyHelper.Entities.Player>();
            if (maggyPlayer != null)
            {
                // Kill our player
                KillMaggyPlayer(maggyPlayer, level, direction);

                // Don't let vanilla player die (it's hidden) — return null
                // to skip the death body animation on the vanilla player
                return null;
            }
        }

        // Fall through to vanilla death
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    /// <summary>
    /// Handles our Player's death: plays effects, then triggers level reload.
    /// </summary>
    private static void KillMaggyPlayer(
        MaggyHelper.Entities.Player maggyPlayer,
        Level level,
        Vector2 direction)
    {
        if (maggyPlayer.IsDead)
            return;

        IngesteLogger.Info("[RoomTransitionHandler] MaggyHelper.Player died");

        // Death particles
        level.Particles.Emit(
            ParticleTypes.Dust, 12,
            maggyPlayer.Position,
            Vector2.One * 8f);

        // Screen shake
        level.Shake(0.15f);

        // Play death sound (reuse Kirby SFX path)
        Audio.Play("event:/char/madeline/death", maggyPlayer.Position);

        // Remove our player
        maggyPlayer.RemoveSelf();

        // Trigger level reload after a short delay
        level.DoScreenWipe(wipeIn: false, onComplete: () =>
        {
            level.Session.Deaths++;
            level.Session.DeathsInCurrentLevel++;
            level.Reload();
        });
    }

    // ─────────────────────────────────────────────────
    //  EVENT: Level.OnTransitionTo
    // ─────────────────────────────────────────────────

    private static void OnTransitionTo(
        Level level,
        LevelData next,
        Vector2 direction)
    {
        if (!PlayerCompatShim.IsMaggyPlayerActive(level))
            return;

        var maggyPlayer = level.Tracker.GetEntity<MaggyHelper.Entities.Player>();
        if (maggyPlayer == null)
            return;

        // Store the session's respawn point at the maggy player's current
        // position so the next room knows where to put the player.
        level.Session.RespawnPoint = maggyPlayer.Position;

        IngesteLogger.Info(
            $"[RoomTransitionHandler] Transitioning to {next.Name}, " +
            $"saved respawn at {maggyPlayer.Position}");
    }

    // ─────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────

    private static void HideVanillaPlayer(CelestePlayer player)
    {
        player.Position = new Vector2(-9999f, -9999f);
        player.Visible = false;
        player.Active = false;
        player.Collidable = false;
    }
}