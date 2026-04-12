using System;
using Celeste.Mod.MaggyHelper;
using MaggyHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities;

/// <summary>
/// Room-local controller that configures the vanilla Celeste.Player for Kirby
/// gameplay at load time. Place this in a room via Lonn to enable Kirby mode,
/// assign a starting copy ability, or snap the spawn to a specific marker.
///
/// Unlike the older implementation, this does not create a second player entity.
/// It keeps the real Celeste.Player authoritative and layers Kirby mechanics
/// on top, which matches the common Everest modding pattern.
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
        var player = level.Tracker.GetEntity<CelestePlayer>();
        if (player == null)
        {
            IngesteLogger.Info("[KirbyPlayerSpawner] Celeste.Player not available during Awake");
            return;
        }

        Vector2? forcedSpawnPos = Position == Vector2.Zero
            ? null
            : Position;

        ConfigureVanillaPlayer(
            level,
            player,
            forcedSpawnPos,
            enableKirbyMode,
            startingPower,
            spawnCompanion);

        IngesteLogger.Info(
            $"[KirbyPlayerSpawner] Configured Celeste.Player at {player.Position} " +
            $"(kirby={enableKirbyMode})");
    }

    internal static void ConfigureVanillaPlayer(
        Level level,
        CelestePlayer player,
        Vector2? forcedSpawnPos,
        bool enableKirbyMode,
        KirbyMode.KirbyPowerState startingPower,
        bool spawnCompanion)
    {
        if (level == null || player == null)
            return;

        if (forcedSpawnPos.HasValue)
        {
            player.Position = forcedSpawnPos.Value;
            level.Session.RespawnPoint = forcedSpawnPos.Value;
        }

        EnsureRoomState(level, enableKirbyMode);

        if (enableKirbyMode)
        {
            player.EnableKirbyMode();

            if (startingPower != KirbyMode.KirbyPowerState.None)
                player.SetKirbyPowerState(startingPower);

            IngesteLogger.Info(
                $"[KirbyPlayerSpawner] Kirby mode enabled" +
                (startingPower != KirbyMode.KirbyPowerState.None
                    ? $" with power: {startingPower}"
                    : string.Empty));
        }
        else
        {
            player.DisableKirbyMode();
            IngesteLogger.Info("[KirbyPlayerSpawner] Madeline mode restored for this room");
        }

        if (spawnCompanion && level.Tracker.GetEntity<KirbyDummy>() == null)
        {
            level.Add(new KirbyDummy(player.Position + new Vector2(-16f, 0f)));
            IngesteLogger.Info("[KirbyPlayerSpawner] Spawned KirbyDummy companion");
        }
    }

    internal static void EnsureRoomState(Level level)
    {
        bool enableKirbyMode = MaggyHelperModule.Session?.IsKirbyModeActive == true;
        EnsureRoomState(level, enableKirbyMode);
    }

    internal static void EnsureRoomState(Level level, bool enableKirbyMode)
    {
        if (level == null)
            return;

        PlayerSelectionManager.GetOrCreate(level);
        PlayerSelectionManager.SetLevelOverride(
            enableKirbyMode
                ? PlayerSelectionManager.PlayerType.Kirby
                : PlayerSelectionManager.PlayerType.Madeline);

        var healthManager = PlayerHealthManager.GetOrCreate(level, enableKirbyMode ? 6 : 1);
        if (enableKirbyMode)
        {
            healthManager.EnableKirbyMode(6);

            if (level.Tracker.GetEntity<HealthBarUI>() == null)
                level.Add(new HealthBarUI(level));
        }
        else
        {
            healthManager.DisableKirbyMode();
        }
    }

    public static void RestoreVanillaPlayer(Level level)
    {
        if (level == null)
            return;

        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        if (vanillaPlayer == null)
            return;

        vanillaPlayer.DisableKirbyMode();
        EnsureRoomState(level, false);

        IngesteLogger.Info("[KirbyPlayerSpawner] Celeste.Player restored to Madeline mode");
    }
}