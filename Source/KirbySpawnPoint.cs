using System;
using Microsoft.Xna.Framework;
using Monocle;
using MaggyHelper.Extensions;

namespace MaggyHelper.Entities;

/// <summary>
/// Spawn point for Kirby player mode. Ensures a Player exists and optionally enables Kirby mode.
/// </summary>
[CustomEntity("MaggyHelper/KirbySpawnPoint")]
[Tracked(false)]
public class KirbySpawnPoint : Entity
{
    private readonly bool spawnAsKirby;
    private readonly KirbyMode.KirbyPowerState startingPower;
    private bool applied;

    public KirbySpawnPoint(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        spawnAsKirby = data.Bool("spawnAsKirby", true);

        var powerString = data.Attr("startingAbility", "None");
        if (!Enum.TryParse(powerString, true, out startingPower))
        {
            startingPower = KirbyMode.KirbyPowerState.None;
        }

        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Depth = Depths.Player;
        Visible = false;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        ApplySpawn(scene);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        ApplySpawn(scene);
    }

    private void ApplySpawn(Scene scene)
    {
        if (applied || scene is not Level level)
        {
            return;
        }

        var player = level.Tracker.GetEntity<global::Celeste.Player>();
        if (player == null)
        {
            return;
        }

        KirbyPlayerSpawner.ConfigureVanillaPlayer(
            level,
            player,
            Position,
            spawnAsKirby,
            startingPower,
            spawnCompanion: false);

        applied = true;
    }
}
