using Microsoft.Xna.Framework;
using DZ.Nez;

namespace DZ.Entities.Hazards;

/// <summary>
/// An invisible rectangular zone that acts as an impassable wall for the
/// Badeline-chaser (and other path-finding entities that respect it).
/// The player passes through freely; only AI-driven chasers are blocked.
/// Ported from Celeste's ChaserBarrier.cs.
///
/// Gameplay notes:
/// <list type="bullet">
///   <item>No player collision — purely an AI navigation blocker.</item>
///   <item>Rendered as a semi-transparent red rectangle in debug/editor mode.</item>
///   <item>Chaser AI should query all <see cref="ChaserBarrier"/> entities in the
///         scene and treat them as solid walls during path-finding.</item>
/// </list>
/// </summary>
public class ChaserBarrier : DZ.Nez.Entity
{
    // ── Geometry ──────────────────────────────────────────────────────────────
    public float Width  { get; private set; }
    public float Height { get; private set; }

    /// <summary>World-space AABB of this barrier zone.</summary>
    public RectangleF Bounds => new RectangleF(Position.X, Position.Y, Width, Height);

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Zone width in pixels.</param>
    /// <param name="height">Zone height in pixels.</param>
    public ChaserBarrier(Vector2 position, int width, int height)
    {
        Position = position;
        Width    = width;
        Height   = height;
        Name     = "ChaserBarrier";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    // No per-frame logic needed — barrier is queried spatially by the chaser AI.

    // ── Helper ────────────────────────────────────────────────────────────────
    /// <summary>
    /// Returns true if <paramref name="point"/> lies within this barrier zone.
    /// Used by chaser path-finding to determine whether a given tile is blocked.
    /// </summary>
    public bool ContainsPoint(Vector2 point)
        => point.X >= Position.X && point.X <= Position.X + Width &&
           point.Y >= Position.Y && point.Y <= Position.Y + Height;
}
