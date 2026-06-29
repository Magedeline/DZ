using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's StarJumpBlock.cs.
///
/// A solid block that, when <see cref="Sinks"/> is <c>true</c>, slowly sinks
/// under the player's weight using the same lerp logic as
/// <see cref="GoldenBlock"/> and <see cref="GlassBlock"/> (yLerp → 12 px down).
///
/// Visual edge/corner decoration is omitted (TODO: add sprite sub-images).
/// </summary>
public class StarJumpBlock : CelesteSolid
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float SinkHold      = 0.1f;  // seconds rider must leave before rising
    private const float SinkLerpRate  = 1f;    // yLerp approach rate per second
    private const float SinkMaxOffset = 12f;   // max pixels sunk

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>When <c>true</c>, the block sinks slowly under the player.</summary>
    public bool Sinks { get; }

    // ── State ─────────────────────────────────────────────────────────────────

    private readonly float _startY;
    private float          _yLerp;
    private float          _sinkTimer;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="StarJumpBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="sinks">Whether the block sinks under the player.</param>
    public StarJumpBlock(Vector2 position, float width, float height, bool sinks)
        : base(position, width, height, safe: false)
    {
        Sinks   = sinks;
        _startY = position.Y;
        Name    = "StarJumpBlock";
        // TODO: load starjump tile edge sprites
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        if (!Sinks) return;

        float dt = Time.DeltaTime;

        if (HasPlayerRider())
            _sinkTimer = SinkHold;
        else if (_sinkTimer > 0f)
            _sinkTimer -= dt;

        _yLerp = _sinkTimer > 0f
            ? Approach(_yLerp, 1f, SinkLerpRate * dt)
            : Approach(_yLerp, 0f, SinkLerpRate * dt);

        float targetY = Lerp(_startY, _startY + SinkMaxOffset, SineInOut(_yLerp));
        MoveV(targetY - Position.Y);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool HasPlayerRider()
    {
        if (Scene == null) return false;
        for (int _sji = 0; _sji < Scene.Entities.Count; _sji++)
        {
            if (Scene.Entities[_sji] is DZ.Entities.Player.MadelinePlayer p
                && IsPlayerRiding(p))
                return true;
        }
        return false;
    }

    private bool IsPlayerRiding(DZ.Entities.Player.MadelinePlayer player) =>
        Math.Abs(player.Position.Y + player.Height - Position.Y) <= 2f
        && player.Position.X + player.Width > Position.X
        && player.Position.X < Position.X + Width;

    private static float Approach(float val, float target, float maxMove) =>
        val < target ? Math.Min(val + maxMove, target)
                     : Math.Max(val - maxMove, target);

    private static float Lerp(float a, float b, float t) =>
        a + (b - a) * Math.Clamp(t, 0f, 1f);

    private static float SineInOut(float t) =>
        (float)(-(Math.Cos(Math.PI * t) - 1.0) / 2.0);
}
