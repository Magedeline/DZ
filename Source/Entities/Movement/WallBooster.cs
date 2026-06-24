using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Movement;

/// <summary>
/// Port of Celeste's WallBooster.cs to Nez/MonoGame.
///
/// A non-solid strip (4 px wide, <see cref="Height"/> px tall) mounted on a
/// wall.  When the player is sliding or climbing adjacent to this strip, their
/// vertical speed is boosted upward, effectively giving them a speed-booster
/// wall section.
///
/// Unlike solid blocks this class extends <see cref="Nez.Entity"/> directly
/// (not <see cref="KirbyCelesteStandalone.Entities.Core.CelesteSolid"/>)
/// because it carries no collision geometry of its own — it is purely a
/// proximity detector.
///
/// <list type="bullet">
///   <item>
///     <see cref="Left"/> = <c>true</c>  → strip is on the left wall
///     (player touches it from the right).
///   </item>
///   <item>
///     <see cref="Left"/> = <c>false</c> → strip is on the right wall
///     (player touches it from the left).
///   </item>
/// </list>
/// </summary>
public class WallBooster : Nez.Entity, IUpdatable
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Width of the booster strip in pixels.</summary>
    public const float StripWidth       = 4f;

    /// <summary>
    /// Upward speed boost applied to the player's Y velocity per second while
    /// they are in contact with the strip (px/s).
    /// </summary>
    public const float BoostSpeed       = 240f;

    /// <summary>
    /// Maximum upward speed the booster will push the player toward (px/s).
    /// The player's Y velocity is clamped to this after boosting.
    /// </summary>
    public const float MaxBoostSpeed    = 240f;

    /// <summary>
    /// Proximity tolerance — how close horizontally the player must be (in pixels)
    /// to receive the boost.
    /// </summary>
    private const float ProximityThresh = 6f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>
    /// <c>true</c> if the strip is attached to the left wall (player approaches
    /// from the right); <c>false</c> for the right wall.
    /// </summary>
    public bool Left { get; }

    /// <summary>Height of the booster strip in pixels.</summary>
    public float Height { get; }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary><c>true</c> while the player is actively receiving the boost.</summary>
    public bool IsBoosting { get; private set; }

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="WallBooster"/>.
    /// </summary>
    /// <param name="position">Top-left world position of the strip in pixels.</param>
    /// <param name="height">Height of the strip in pixels.</param>
    /// <param name="left">
    /// <c>true</c> if attached to the left wall; <c>false</c> for the right wall.
    /// </param>
    public WallBooster(Vector2 position, int height, bool left)
    {
        Position = position;
        Height   = height;
        Left     = left;
        Name     = left ? "WallBooster_Left" : "WallBooster_Right";
        // TODO: load sprite (booster strip texture)
    }

    // ── Bounds ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Axis-aligned bounds of the strip (4 px wide × <see cref="Height"/> px tall).
    /// </summary>
    public RectangleF Bounds =>
        new RectangleF(Position.X, Position.Y, StripWidth, Height);

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        IsBoosting = false;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    /// <summary>Checks player proximity and applies the boost every frame.</summary>
    public override void Update()
    {
        var player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null)
        {
            IsBoosting = false;
            return;
        }

        if (IsPlayerAdjacent(player))
        {
            ApplyBoost(player);
        }
        else
        {
            IsBoosting = false;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> when the player is sliding against the wall this
    /// booster is mounted on and overlaps it vertically.
    /// </summary>
    private bool IsPlayerAdjacent(PlayerController player)
    {
        var pb = GetPlayerBounds(player);

        // Vertical overlap with the strip.
        bool overlapV = pb.Bottom > Bounds.Top && pb.Top < Bounds.Bottom;
        if (!overlapV) return false;

        // Horizontal proximity: player must be pressed against this wall side.
        if (Left)
        {
            // Strip is on the left wall → player's right edge must be near the strip's right edge.
            float dist = Math.Abs(pb.Right - Bounds.Right);
            return dist <= ProximityThresh;
        }
        else
        {
            // Strip is on the right wall → player's left edge must be near the strip's left edge.
            float dist = Math.Abs(pb.Left - Bounds.Left);
            return dist <= ProximityThresh;
        }
    }

    /// <summary>
    /// Pushes the player's Y velocity upward by <see cref="BoostSpeed"/> per
    /// second, capped at <see cref="MaxBoostSpeed"/>.
    /// </summary>
    private void ApplyBoost(PlayerController player)
    {
        IsBoosting = true;

        float dt = Time.DeltaTime;

        // Boost Y velocity upward (negative = up in screen-space).
        float newVY = player.Velocity.Y - BoostSpeed * dt;
        newVY = Math.Max(newVY, -MaxBoostSpeed);

        player.Velocity = new Vector2(player.Velocity.X, newVY);

        // TODO: emit particles (speed sparks)
        // TODO: play sound (booster hiss — looping while IsBoosting)
    }

    /// <summary>
    /// Returns the player's collider bounds, falling back to a zero-size rect
    /// at the player's position if no <see cref="BoxCollider"/> is found.
    /// </summary>
    private static RectangleF GetPlayerBounds(PlayerController player)
    {
        var col = player.Entity.GetComponent<BoxCollider>();
        return col?.Bounds ?? new RectangleF(player.Entity.Position.X, player.Entity.Position.Y, 0, 0);
    }
}
