using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;

namespace DZ.Entities.Level;

/// <summary>
/// Bloom light-point data component.  Ported from Celeste's BloomPoint.cs.
///
/// This is a pure data component – the actual bloom rendering is handled by the
/// bloom/lighting renderer system.  Components that want to emit bloom simply
/// attach a <see cref="BloomPoint"/> to their entity with the desired parameters.
/// </summary>
public class BloomPoint : DZ.Nez.Component
{
    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    /// <summary>Local offset from the owning entity's position.</summary>
    public Vector2 Position { get; set; } = Vector2.Zero;

    /// <summary>Bloom intensity in [0, 1].</summary>
    public float Alpha { get; set; } = 1f;

    /// <summary>Radius of the bloom falloff sphere (pixels).</summary>
    public float Radius { get; set; } = 8f;

    // -------------------------------------------------------------------------
    // Convenience accessors
    // -------------------------------------------------------------------------

    public float X
    {
        get => Position.X;
        set => Position = new Vector2(value, Position.Y);
    }

    public float Y
    {
        get => Position.Y;
        set => Position = new Vector2(Position.X, value);
    }

    /// <summary>World-space centre of the bloom point.</summary>
    public Vector2 WorldPosition => Entity != null ? Entity.Position + Position : Position;

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>Creates a bloom point at the entity's origin.</summary>
    /// <param name="alpha">Intensity (0-1).</param>
    /// <param name="radius">Falloff radius in pixels.</param>
    public BloomPoint(float alpha, float radius)
    {
        Alpha  = alpha;
        Radius = radius;
    }

    /// <summary>Creates a bloom point at a local offset from the entity origin.</summary>
    /// <param name="position">Local offset.</param>
    /// <param name="alpha">Intensity (0-1).</param>
    /// <param name="radius">Falloff radius in pixels.</param>
    public BloomPoint(Vector2 position, float alpha, float radius)
    {
        Position = position;
        Alpha    = alpha;
        Radius   = radius;
    }
}
