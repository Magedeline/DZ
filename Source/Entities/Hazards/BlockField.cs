using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;

namespace DZ.Entities.Hazards;

/// <summary>
/// An invisible rectangular field that blocks certain projectiles or enemies
/// (not the player).  Acts as a tagged zone that AI or projectile systems can
/// query to determine whether movement is permitted.
/// Ported from Celeste's BlockField.cs.
///
/// Gameplay notes:
/// <list type="bullet">
///   <item>No player collision — purely a blocking zone for enemy/projectile AI.</item>
///   <item>Query <see cref="ContainsPoint"/> or <see cref="Bounds"/> from systems
///         that need to respect this field.</item>
///   <item>All <see cref="BlockField"/> entities in the scene can be found via
///         <c>Scene.FindComponentsOfType&lt;BlockField&gt;()</c>.</item>
/// </list>
/// </summary>
public class BlockField : DZ.Nez.Entity
{
    // ── Geometry ──────────────────────────────────────────────────────────────
    public float Width  { get; private set; }
    public float Height { get; private set; }

    /// <summary>World-space AABB of the block field.</summary>
    public RectangleF Bounds => new RectangleF(Position.X, Position.Y, Width, Height);

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="position">Top-left world position of the field.</param>
    /// <param name="width">Field width in pixels.</param>
    /// <param name="height">Field height in pixels.</param>
    public BlockField(Vector2 position, int width, int height)
    {
        Position = position;
        Width    = width;
        Height   = height;
        Name     = "BlockField";
    }

    // ── No per-frame logic ────────────────────────────────────────────────────
    // BlockField is a pure spatial query zone; no Update() needed.

    // ── Helper ────────────────────────────────────────────────────────────────
    /// <summary>
    /// Returns true if <paramref name="point"/> lies within this block field.
    /// Use this in projectile / enemy movement systems to check for field overlap.
    /// </summary>
    public bool ContainsPoint(Vector2 point)
        => point.X >= Position.X && point.X <= Position.X + Width &&
           point.Y >= Position.Y && point.Y <= Position.Y + Height;
}
