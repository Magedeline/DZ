using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Hazards;

// ── Direction enum shared across hazard files ────────────────────────────────

/// <summary>Direction a spike strip faces (the side the spikes protrude from).</summary>
public enum SpikeDirection
{
    /// <summary>Spikes face upward – kill player approaching from above.</summary>
    Up,
    /// <summary>Spikes face downward – kill player approaching from below.</summary>
    Down,
    /// <summary>Spikes face left – kill player approaching from the left.</summary>
    Left,
    /// <summary>Spikes face right – kill player approaching from the right.</summary>
    Right,
}

/// <summary>
/// A static strip of spikes that kills the player on directional contact.
/// Ported from Celeste's Spikes entity.
///
/// Each spike strip occupies a thin (3-pixel) hitbox aligned to its direction.
/// The entity only applies damage when the player's velocity indicates they are
/// moving into the spikes (e.g. moving downward into upward-facing spikes).
/// Spikes can be toggled via the <see cref="Collidable"/> flag; disabled spikes
/// are tinted with <see cref="DisabledColor"/> and never hurt the player.
/// </summary>
public class Spikes : Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const float HitboxThickness = 3f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>Length of the spike strip in pixels (perpendicular to direction).</summary>
    public int Size { get; }

    /// <summary>Direction the spikes are facing.</summary>
    public SpikeDirection Direction { get; }

    /// <summary>
    /// When <c>false</c> the spikes are visually dimmed and cannot harm the player.
    /// </summary>
    public bool Collidable { get; set; } = true;

    /// <summary>Tint used when the spikes are enabled (default: white).</summary>
    public Color EnabledColor { get; set; } = Color.White;

    /// <summary>Tint used when the spikes are disabled.</summary>
    public Color DisabledColor { get; set; } = Color.Gray * 0.5f;

    // ── Visual ────────────────────────────────────────────────────────────────
    // TODO: load sprite – replace with actual SpriteRenderer once assets exist.

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Thin world-space AABB of the spike strip, aligned to <see cref="Direction"/>.
    /// </summary>
    public RectangleF Bounds => BuildBounds(Position, Size, Direction, HitboxThickness);

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Spikes"/> strip.
    /// </summary>
    /// <param name="position">Anchor position (top-left of the strip's tile block).</param>
    /// <param name="size">Length of the strip in pixels.</param>
    /// <param name="direction">Side the spikes protrude from.</param>
    public Spikes(Vector2 position, int size, SpikeDirection direction)
    {
        Position = position;
        Size = size;
        Direction = direction;
        Name = $"Spikes_{direction}";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void Update()
    {
        if (!Collidable) return;

        if (!CheckPlayerCollision(out PlayerController player)) return;

        // Only damage when the player is moving into the spikes.
        if (!IsApproachingFromDangerousSide(player.Velocity)) return;

        Vector2 knockback = DirectionToVector(Direction);
        // TODO: play sound: event:/game/general/spikes_touch
        player.TakeDamage(1, knockback);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> when the player velocity vector points into the spike face.
    /// </summary>
    private bool IsApproachingFromDangerousSide(Vector2 velocity)
    {
        return Direction switch
        {
            SpikeDirection.Up    => velocity.Y > 0,   // player falling downward → up spikes
            SpikeDirection.Down  => velocity.Y < 0,   // player moving up → down spikes
            SpikeDirection.Left  => velocity.X > 0,   // player moving right → left spikes
            SpikeDirection.Right => velocity.X < 0,   // player moving left → right spikes
            _                    => false,
        };
    }

    /// <summary>
    /// Returns a unit vector pointing away from the spike face (used as knockback direction).
    /// </summary>
    private static Vector2 DirectionToVector(SpikeDirection dir)
    {
        return dir switch
        {
            SpikeDirection.Up    => -Vector2.UnitY,
            SpikeDirection.Down  =>  Vector2.UnitY,
            SpikeDirection.Left  => -Vector2.UnitX,
            SpikeDirection.Right =>  Vector2.UnitX,
            _                    =>  Vector2.Zero,
        };
    }

    /// <summary>Builds a thin AABB for a spike strip aligned to <paramref name="dir"/>.</summary>
    internal static RectangleF BuildBounds(
        Vector2 origin, int size, SpikeDirection dir, float thickness)
    {
        return dir switch
        {
            // Upward spikes: thin strip at the top edge.
            SpikeDirection.Up    => new RectangleF(origin.X, origin.Y, size, thickness),
            // Downward spikes: thin strip at the bottom edge.
            SpikeDirection.Down  => new RectangleF(origin.X, origin.Y + size - thickness, size, thickness),
            // Left spikes: thin strip on the left edge.
            SpikeDirection.Left  => new RectangleF(origin.X, origin.Y, thickness, size),
            // Right spikes: thin strip on the right edge.
            SpikeDirection.Right => new RectangleF(origin.X + size - thickness, origin.Y, thickness, size),
            _                    => new RectangleF(origin.X, origin.Y, size, thickness),
        };
    }

    /// <summary>
    /// Returns <c>true</c> and populates <paramref name="player"/> when the
    /// player's collider overlaps this spike strip.
    /// </summary>
    private bool CheckPlayerCollision(out PlayerController player)
    {
        player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return false;

        RectangleF playerBounds = player.Entity.GetComponent<BoxCollider>()?.Bounds
                                  ?? RectangleF.Empty;
        return Bounds.Intersects(playerBounds);
    }
}
