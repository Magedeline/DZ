using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Entities.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's SinkingPlatform.cs.
///
/// A jump-through platform that sinks under a player's weight then floats back
/// up to its <see cref="_startY"/> when the player leaves.
///
/// Speed model (mirrors Celeste):
/// <list type="bullet">
///   <item>Player on board → accelerate down toward 30 px/s (60 if ducking).</item>
///   <item>Post-rider hold → accelerate down toward 45 px/s for 0.1 s.</item>
///   <item>No rider → decelerate, then rise at <c>-speed</c> toward startY.</item>
/// </list>
/// </summary>
public class SinkingPlatform : CelesteJumpThru
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float SinkSpeedNormal  = 30f;
    private const float SinkSpeedDucking = 60f;
    private const float SinkAccel        = 400f;
    private const float RiseSpeed        = 45f;
    private const float RiseAccel        = 600f;
    private const float FloatAccel       = 400f;  // acceleration toward -50 (rising)
    private const float FloatTarget      = -50f;
    private const float RiseHold         = 0.1f;

    // ── State ─────────────────────────────────────────────────────────────────

    private readonly float _startY;
    private float          _speed;
    private float          _riseTimer;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="SinkingPlatform"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    public SinkingPlatform(Vector2 position, int width)
        : base(position, width)
    {
        _startY = position.Y;
        Name    = "SinkingPlatform";
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        float dt     = Time.DeltaTime;
        var   player = GetPlayerRider();

        if (player != null)
        {
            // Only start the downward sound on the first frame contact.
            if (_riseTimer <= 0f && Position.Y <= _startY)
            {
                // TODO: play "platform_vert_start" sound
            }

            _riseTimer = RiseHold;
            bool ducking = false; // TODO: read player.Ducking when available
            _speed = Approach(_speed, ducking ? SinkSpeedDucking : SinkSpeedNormal, SinkAccel * dt);
        }
        else if (_riseTimer > 0f)
        {
            _riseTimer -= dt;
            _speed      = Approach(_speed, RiseSpeed, RiseAccel * dt);
        }
        else
        {
            _speed = Approach(_speed, FloatTarget, FloatAccel * dt);
        }

        if (_speed > 0f)
        {
            // Moving downward — just sink.
            MoveV(_speed * dt);
        }
        else if (_speed < 0f && Position.Y > _startY)
        {
            // Moving upward — clamp to startY.
            MoveTowardsY(_startY, -_speed * dt);

            if (Position.Y <= _startY)
            {
                // TODO: play "platform_vert_end" sound
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static float Approach(float val, float target, float maxMove)
    {
        return val < target
            ? Math.Min(val + maxMove, target)
            : Math.Max(val - maxMove, target);
    }

    /// <summary>Returns the <see cref="MadelinePlayer"/> riding this platform, or null.</summary>
    private MadelinePlayer GetPlayerRider()
    {
        if (Scene == null) return null;
        for (int _spi = 0; _spi < Scene.Entities.Count; _spi++)
        {
            if (Scene.Entities[_spi] is MadelinePlayer p && IsPlayerRiding(p))
                return p;
        }
        return null;
    }

    private bool IsPlayerRiding(MadelinePlayer player) =>
        Math.Abs(player.Position.Y + player.Height - Position.Y) <= 2f
        && player.Position.X + player.Width > Position.X
        && player.Position.X < Position.X + Width;

    /// <summary>Move toward a Y target without overshooting.</summary>
    private void MoveTowardsY(float targetY, float maxMove)
    {
        float diff = targetY - Position.Y;
        if (Math.Abs(diff) <= maxMove)
            MoveTo(new Vector2(Position.X, targetY));
        else
            MoveV(Math.Sign(diff) * maxMove);
    }
}
