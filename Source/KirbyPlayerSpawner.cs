using System;
using Celeste.Mod.MaggyHelper;
using MaggyHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities;

/// <summary>
/// Map entity that replaces the vanilla Celeste.Player with MaggyHelper's own
/// Player entity at load time. Place this in any room via the Lönn map editor
/// to get full Kirby gameplay without relying on a vanilla player spawn.
///
/// Execution order (see <see cref="PlayerCompatShim"/> for hooks):
///   1. Level.LoadLevel spawns a vanilla Celeste.Player (unavoidable).
///   2. This entity's <see cref="Awake"/> fires and:
///      a) Records the vanilla player's position.
///      b) Removes / hides the vanilla player.
///      c) Spawns <see cref="MaggyHelper.Entities.Player"/> at the same position.
///      d) Enables Kirby mode flags in the session.
///   3. <see cref="PlayerCompatShim"/> redirects camera, triggers, and
///      <see cref="PlayerCollider"/> to our entity.
/// </summary>
[CustomEntity("MaggyHelper/KirbyPlayerSpawner")]
[Tracked(false)]
public sealed class KirbyPlayerSpawner : Entity
{
    private readonly bool enableKirbyMode;
    private readonly KirbyMode.KirbyPowerState startingPower;
    private readonly bool spawnCompanion;
    private bool applied;

    public KirbyPlayerSpawner(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        enableKirbyMode = data.Bool("enableKirbyMode", true);
        spawnCompanion = data.Bool("spawnCompanion", false);

        var powerStr = data.Attr("startingAbility", "None");
        if (!Enum.TryParse(powerStr, true, out startingPower))
        {
            startingPower = KirbyMode.KirbyPowerState.None;
        }

        // Invisible marker entity
        Visible = false;
        Depth = Depths.Top;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (applied || scene is not Level level)
            return;

        applied = true;
        ApplySpawn(level);
    }

    private void ApplySpawn(Level level)
    {
        // --- 1. Find the vanilla player that Celeste spawned automatically ---
        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        Vector2 spawnPos = Position;

        if (vanillaPlayer != null)
        {
            // Use entity position if set, otherwise inherit vanilla player position
            if (Position == Vector2.Zero)
                spawnPos = vanillaPlayer.Position;

            // Hide the vanilla player: move offscreen, make non-interactive
            HideVanillaPlayer(vanillaPlayer, level);
        }

        // --- 2. Spawn our MaggyHelper Player ---
        var maggyPlayer = new MaggyHelper.Entities.Player(
            spawnPos,
            PlayerSpriteMode.Madeline);

        level.Add(maggyPlayer);
        level.Session.RespawnPoint = spawnPos;

        IngesteLogger.Info(
            $"[KirbyPlayerSpawner] Spawned MaggyHelper.Player at {spawnPos}");

        // --- 3. Enable Kirby mode if requested ---
        if (enableKirbyMode)
        {
            var session = MaggyHelperModule.Session;
            if (session != null)
            {
                session.IsKirbyModeActive = true;
            }

            maggyPlayer.KirbyModeActive = true;
            maggyPlayer.CombatEnabled = true;

            if (startingPower != KirbyMode.KirbyPowerState.None)
            {
                // Ensure KirbyMode entity exists in scene
                var kirbyMode = level.Tracker.GetEntity<KirbyMode>();
                if (kirbyMode == null)
                {
                    kirbyMode = new KirbyMode();
                    level.Add(kirbyMode);
                }
                kirbyMode.SetPowerState(startingPower);
            }

            IngesteLogger.Info(
                $"[KirbyPlayerSpawner] Kirby mode enabled" +
                (startingPower != KirbyMode.KirbyPowerState.None
                    ? $" with power: {startingPower}"
                    : ""));
        }

        // --- 4. Spawn KirbyDummy companion if requested ---
        if (spawnCompanion)
        {
            var dummy = new KirbyDummy(spawnPos + new Vector2(-16f, 0f));
            level.Add(dummy);
            IngesteLogger.Info("[KirbyPlayerSpawner] Spawned KirbyDummy companion");
        }

        // --- 5. Initialize health + selection managers ---
        PlayerSelectionManager.GetOrCreate(level);
        PlayerSelectionManager.SetLevelOverride(
            enableKirbyMode
                ? PlayerSelectionManager.PlayerType.Kirby
                : PlayerSelectionManager.PlayerType.Madeline);

        PlayerHealthManager.GetOrCreate(level, enableKirbyMode ? 6 : 1);

        // --- 6. Initialize health bar UI for Kirby mode ---
        if (enableKirbyMode && level.Tracker.GetEntity<HealthBarUI>() == null)
        {
            level.Add(new HealthBarUI(level));
        }
    }

    /// <summary>
    /// Hides the vanilla Celeste.Player without fully removing it (some vanilla
    /// systems hard-crash if no Player exists at all). Instead we:
    ///  - Move it far offscreen
    ///  - Make it invisible
    ///  - Set it to the Dummy state so it doesn't process input
    /// </summary>
    private static void HideVanillaPlayer(CelestePlayer player, Level level)
    {
        player.Position = new Vector2(-9999f, -9999f);
        player.Visible = false;
        player.Active = false;
        player.Collidable = false;

        IngesteLogger.Info("[KirbyPlayerSpawner] Vanilla Celeste.Player hidden");
    }

    /// <summary>
    /// Restores a hidden vanilla player. Called when the spawner is removed or
    /// Kirby mode is disabled and we need to fall back to vanilla.
    /// </summary>
    public static void RestoreVanillaPlayer(Level level)
    {
        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        if (vanillaPlayer == null)
            return;

        var maggyPlayer = level.Tracker.GetEntity<MaggyHelper.Entities.Player>();
        Vector2 restorePos = maggyPlayer?.Position ?? (level.Session.RespawnPoint ?? Vector2.Zero);

        vanillaPlayer.Position = restorePos;
        vanillaPlayer.Visible = true;
        vanillaPlayer.Active = true;
        vanillaPlayer.Collidable = true;

        // Remove our player
        maggyPlayer?.RemoveSelf();

        IngesteLogger.Info("[KirbyPlayerSpawner] Vanilla Celeste.Player restored");
    }
}