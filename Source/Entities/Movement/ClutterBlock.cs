using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Collider = DZ.Nez.Collider;
using System;
using System.Collections.Generic;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's ClutterBlock.cs.
///
/// A decorative puzzle block that floats gently up and down with a sine wave.
/// When a player touches its top or sides it "weights down" (snaps to ground
/// offset = 0) and propagates the weight-down to all blocks stacked below.
///
/// These are not solids; they use a simple non-blocking <see cref="DZ.Nez.BoxCollider"/>
/// for player-proximity detection only.
/// </summary>
public class ClutterBlock : DZ.Nez.Entity
{
    // ── Block colour enum ─────────────────────────────────────────────────────

    public enum Colors { Red, Green, Yellow, Lightning }

    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float DuckDuration = 3f;   // not used here; from original
    private const float FloatApproachRate = 4f;

    // ── Public state (for stacking logic) ────────────────────────────────────

    public Colors BlockColor;
    public HashSet<ClutterBlock> HasBelow = new();
    public List<ClutterBlock>    Below    = new();
    public List<ClutterBlock>    Above    = new();
    public bool OnTheGround;
    public bool TopSideOpen;
    public bool LeftSideOpen;
    public bool RightSideOpen;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private float _floatTarget;
    private float _floatDelay;
    private float _floatTimer;

    // ── Visual ────────────────────────────────────────────────────────────────

    /// <summary>Y offset applied to the image for the floating animation.</summary>
    public float ImageOffsetY { get; private set; }

    // ── Width / height (set from collider) ───────────────────────────────────

    public float BlockWidth  { get; private set; }
    public float BlockHeight { get; private set; }

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="ClutterBlock"/>.
    /// </summary>
    /// <param name="position">World position (top-left).</param>
    /// <param name="color">Block colour category.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public ClutterBlock(Vector2 position, Colors color, float width, float height)
        : base()
    {
        Position    = position;
        BlockColor  = color;
        BlockWidth  = width;
        BlockHeight = height;
        Name        = "ClutterBlock";
        // TODO: add image component / sprite
    }

    // ── Weight-down ───────────────────────────────────────────────────────────

    /// <summary>
    /// Recursively pushes all blocks below this one down to ground level
    /// and locks this block at float offset 0.
    /// </summary>
    public void WeightDown()
    {
        foreach (var b in Below)
            b.WeightDown();
        _floatTarget = 0f;
        _floatDelay  = 0.1f;
    }

    // ── Absorb (remove from scene) ────────────────────────────────────────────

    /// <summary>
    /// Requests this block be removed from the scene (called by clutter absorb effect).
    /// </summary>
    public void Absorb()
    {
        // TODO: emit fly-clutter visual effect
        Destroy();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        if (OnTheGround) return;

        if (_floatDelay <= 0f)
        {
            // Check for player proximity to weight down.
            if (Scene != null)
            {
                for (int _ci = 0; _ci < Scene.Entities.Count; _ci++)
                {
                    if (Scene.Entities[_ci] is not DZ.Entities.Player.MadelinePlayer player) continue;

                    float pRight  = player.Position.X + player.Width;
                    float pLeft   = player.Position.X;
                    float pBottom = player.Position.Y + player.Height;
                    float pTop    = player.Position.Y;

                    float bRight  = Position.X + BlockWidth;
                    float bLeft   = Position.X;
                    float bBottom = Position.Y + BlockHeight;
                    float bTop    = Position.Y;

                    // Top-side touch.
                    bool topTouch = TopSideOpen
                        && pRight > bLeft && pLeft < bRight
                        && pBottom >= bTop - 1f && pBottom <= bTop + 4f;

                    // Left-side touch (climbing).
                    bool leftTouch = LeftSideOpen
                        && pBottom > bTop && pTop < bBottom
                        && pRight >= bLeft - 1f && pRight <= bLeft + 4f;

                    // Right-side touch (climbing).
                    bool rightTouch = RightSideOpen
                        && pBottom > bTop && pTop < bBottom
                        && pLeft <= bRight + 1f && pLeft >= bRight - 4f;

                    if (topTouch || leftTouch || rightTouch)
                    {
                        WeightDown();
                        break;
                    }
                }
            }
        }

        _floatTimer += Time.DeltaTime;
        _floatDelay -= Time.DeltaTime;

        if (_floatDelay <= 0f)
        {
            float wave = WaveTarget;
            _floatTarget = Approach(_floatTarget, wave, FloatApproachRate * Time.DeltaTime);
        }

        ImageOffsetY = _floatTarget;
    }

    // ── Wave ──────────────────────────────────────────────────────────────────

    private float WaveTarget =>
        (float)(-((Math.Sin((int)Position.X / 16 * 0.25 + _floatTimer * 2.0) + 1.0) / 2.0) - 1.0);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static float Approach(float val, float target, float maxMove) =>
        val < target ? Math.Min(val + maxMove, target) : Math.Max(val - maxMove, target);
}
