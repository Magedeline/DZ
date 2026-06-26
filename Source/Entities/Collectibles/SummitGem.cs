using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;
using Component = Nez.Component;
using Collider = Nez.Collider;
using Entity = Nez.Entity;

namespace KirbyCelesteStandalone.Entities.Collectibles;

/// <summary>
/// Port of Celeste's SummitGem.cs.
///
/// Gem collectible found in the Summit (Chapter 7). Each gem has a unique color
/// and can be collected by dash-attacking it. Shows a shatter effect on collection.
/// </summary>
public class SummitGem : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    public static readonly Color[] GemColors = new Color[6]
    {
        new Color(0x9E, 0xE9, 0xFF), // Light blue
        new Color(0x54, 0xBA, 0xFF), // Blue
        new Color(0x90, 0xFF, 0x2D), // Green
        new Color(0xFF, 0xD3, 0x00), // Yellow
        new Color(0xFF, 0x60, 0x9D), // Pink
        new Color(0xC5, 0xE1, 0xBA)  // Light green
    };

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Unique identifier for this gem (0-5).</summary>
    public int GemId { get; private set; }

    /// <summary>Whether the gem has been collected.</summary>
    public bool Collected { get; private set; }

    /// <summary>The color of this gem.</summary>
    public Color GemColor => GemColors[GemId % GemColors.Length];

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _spawnPosition;
    private string _entityId;
    private bool _isGhost;
    private float _bounceSfxDelay;

    private float _scaleWiggleValue;
    private float _scaleWiggleTimer;
    private float _moveWiggleValue;
    private float _moveWiggleTimer;
    private Vector2 _moveWiggleDir;

    private BoxCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public SummitGem(Vector2 position, int gemId, string entityId)
    {
        _spawnPosition = position;
        GemId = Math.Clamp(gemId, 0, 5);
        _entityId = entityId;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // 12x12 hitbox, centered
        _collider = Entity.AddComponent(new BoxCollider(-6f, -6f, 12f, 12f));
        _collider.IsTrigger = true;

        // TODO: load sprite from "collectables/summitgems/{GemId}/gem"
        // TODO: set up wiggler for scale effect
        // TODO: if ghost, set sprite color to Color.White * 0.5f
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        _bounceSfxDelay -= dt;

        // Update wiggles
        if (_scaleWiggleTimer > 0f)
        {
            _scaleWiggleTimer -= dt;
            _scaleWiggleValue = MathF.Sin(_scaleWiggleTimer * 8f) * MathF.Exp(-_scaleWiggleTimer * 4f);
        }
        else
        {
            _scaleWiggleValue = 0f;
        }

        if (_moveWiggleTimer > 0f)
        {
            _moveWiggleTimer -= dt;
            _moveWiggleValue = MathF.Sin(_moveWiggleTimer * 5f) * MathF.Exp(-_moveWiggleTimer * 2f);
        }
        else
        {
            _moveWiggleValue = 0f;
        }

        // TODO: sprite.Position = _moveWiggleDir * _moveWiggleValue * -8f;

        if (!Collected)
        {
            CheckPlayerOverlap();
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static readonly Collider[] _overlapResults = new Collider[8];

    private void CheckPlayerOverlap()
    {
        if (_collider == null) return;

        var rect = new RectangleF(
            Entity.Position.X - 6f,
            Entity.Position.Y - 6f,
            12f,
            12f);

        int count = Nez.Physics.OverlapRectangleAll(ref rect, _overlapResults);

        for (int i = 0; i < count; i++)
        {
            var hit = _overlapResults[i];
            if (hit == _collider) continue;

            var player = hit.Entity?.GetComponent<MadelinePlayer>();
            if (player == null) continue;

            OnPlayer(player);
            break;
        }
    }

    private void OnPlayer(MadelinePlayer player)
    {
        if (Collected) return;

        if (player.DashAttacking)
        {
            // Smash collect
            // TODO: Add(new Coroutine(SmashRoutine(player)));
        }
        else
        {
            // Bounce player
            // player.PointBounce(Entity.Position); // TODO: not implemented in this port
            _moveWiggleTimer = 0.8f;
            _scaleWiggleTimer = 0.5f;
            _moveWiggleDir = (Entity.Position - player.Position).SafeNormalize(Vector2.UnitY);

            // TODO: rumble

            if (_bounceSfxDelay <= 0f)
            {
                // TODO: play sound: event:/game/general/crystalheart_bounce
                _bounceSfxDelay = 0.1f;
            }
        }
    }

    private IEnumerator SmashRoutine(MadelinePlayer player)
    {
        Collected = true;
        // Entity.Visible = false; // TODO: not supported in Nez
        _collider?.SetEnabled(false);

        // Refill player stamina
        player.Stamina = 110f;

        // TODO: play sound: event:/game/07_summit/gem_get
        // TODO: register in game state
        // TODO: shake level
        // TODO: freeze game for 0.1f

        // Emit particles
        float direction = MathF.Atan2(player.Speed.Y, player.Speed.X);
        // TODO: emit shatter particles in direction
        // TODO: emit slash FX

        // Spawn absorb orbs
        for (int i = 0; i < 10; i++)
        {
            // TODO: Scene.Add(new AbsorbOrb(Entity.Position, player));
        }

        // Flash white
        // TODO: level.Flash(Color.White, true);
        // TODO: add BgFlash entity

        // Slow time
        float timeRate = 0.5f;
        while (timeRate < 1f)
        {
            timeRate += Time.DeltaTime * 0.5f;
            // TODO: Engine.TimeRate = timeRate;
            yield return null;
        }

        Entity.Destroy();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Background flash effect entity.
    /// </summary>
    private class BgFlash : Entity, IUpdatable
    {
        private float _alpha = 1f;

        public BgFlash()
        {
            // TODO: set depth and tag
        }

        public void Update()
        {
            _alpha = Calc.Approach(_alpha, 0f, Time.DeltaTime * 0.5f);
            if (_alpha <= 0f)
            {
                Destroy();
            }
        }

        // TODO: Render - draw black rect over screen with _alpha via a RenderableComponent
    }
}

/// <summary>
/// Mathematical helper functions.
/// </summary>
public static class SummitGemCalc
{
    public static float Approach(float val, float target, float maxMove)
    {
        return val > target ? Math.Max(val - maxMove, target) : Math.Min(val + maxMove, target);
    }
}

/// <summary>
/// Extension methods for Vector2.
/// </summary>
public static class Vector2Extensions
{
    public static Vector2 SafeNormalize(this Vector2 vec, Vector2 defaultValue)
    {
        if (vec.LengthSquared() > 0.0001f)
            return Vector2.Normalize(vec);
        return defaultValue;
    }

    public static Vector2 Perpendicular(this Vector2 vec) => new Vector2(-vec.Y, vec.X);
}
