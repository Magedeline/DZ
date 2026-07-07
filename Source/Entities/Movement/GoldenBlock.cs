using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's GoldenBlock.cs.
///
/// A solid block that only appears when the player is carrying the golden
/// strawberry.  It slides in from below the screen using a
/// <see cref="_renderLerp"/> and slowly sinks under the player's weight
/// (identical yLerp mechanic to <see cref="StarJumpBlock"/>).
///
/// The block starts invisible/non-collidable and activates when the player
/// comes within 80 px of its left edge.
///
/// Nine-slice rendering and berry-icon drawing are TODO (sprite atlas).
/// </summary>
public class GoldenBlock : CelesteSolid
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float SinkHold        = 0.1f;
    private const float SinkLerpRate    = 1f;
    private const float SinkMaxOffset   = 12f;
    private const float RenderLerpRate  = 3f;    // rate renderLerp fades to 0
    private const float ActivateXOffset = 80f;   // player must be within this many px

    // ── State ─────────────────────────────────────────────────────────────────

    private readonly float _startY;
    private float          _yLerp;
    private float          _sinkTimer;
    private float          _renderLerp = 1f;  // 1 = fully offset (hidden), 0 = at rest
    private bool           _visible    = false;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="GoldenBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public GoldenBlock(Vector2 position, float width, float height)
        : base(position, width, height, safe: false)
    {
        _startY    = position.Y;
        Collidable = false;
        Enabled    = false;
        Name       = "GoldenBlock";
        // TODO: load nine-slice "objects/DZ/DZ/goldblock" texture
        // TODO: load berry icon "collectables/goldberry/idle00"
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        _renderLerp = 1f;

        // TODO: check if player is carrying a golden strawberry;
        // if not, RemoveSelf().
        // For now the block always exists but stays hidden until player is near.
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        float dt = Time.DeltaTime;

        // Activate when player is close.
        if (!_visible && Scene != null)
        {
            for (int _gi = 0; _gi < Scene.Entities.Count; _gi++)
            {
                var e = Scene.Entities[_gi];
                if (e is DZ.Entities.Player.MadelinePlayer player
                    && player.Position.X > Position.X - ActivateXOffset)
                {
                    _visible   = true;
                    Collidable = true;
                    Enabled    = true;
                    _renderLerp = 1f;
                    break;
                }
            }
        }

        // Fade in render offset.
        if (_visible)
            _renderLerp = Approach(_renderLerp, 0f, RenderLerpRate * dt);

        // Sink logic.
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

    // ── Render ────────────────────────────────────────────────────────────────

    public override void Render()
    {
        if (!_visible) return;

        // TODO: render nine-slice block shifted down by CubeIn(_renderLerp) * boundsBottom offset
        // TODO: render berry icon at center

        // Placeholder: tinted rectangle
        Graphics.Instance.Batcher.DrawRect(Position.X, Position.Y, Width, Height, Color.Gold * 0.7f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private new bool HasPlayerRider()
    {
        if (Scene == null) return false;
        for (int _gi2 = 0; _gi2 < Scene.Entities.Count; _gi2++)
            if (Scene.Entities[_gi2] is DZ.Entities.Player.MadelinePlayer p && IsRiding(p))
                return true;
        return false;
    }

    private bool IsRiding(DZ.Entities.Player.MadelinePlayer player) =>
        Math.Abs(player.Position.Y + player.Height - Position.Y) <= 2f
        && player.Position.X + player.Width > Position.X
        && player.Position.X < Position.X + Width;

    private static float Approach(float val, float target, float maxMove) =>
        val < target ? Math.Min(val + maxMove, target) : Math.Max(val - maxMove, target);

    private static float Lerp(float a, float b, float t) =>
        a + (b - a) * Math.Clamp(t, 0f, 1f);

    private static float SineInOut(float t) =>
        (float)(-(Math.Cos(Math.PI * t) - 1.0) / 2.0);
}
