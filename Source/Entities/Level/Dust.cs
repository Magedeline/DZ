using Microsoft.Xna.Framework;
using Nez;
using System;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Static utility class for emitting dust particles at a world position.
/// Ported from Celeste's Dust.cs (static class).
///
/// All methods are no-ops in the absence of a particle system – replace the
/// TODO stubs with calls to your Nez particle emitter.
/// </summary>
public static class Dust
{
    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Emits <paramref name="count"/> dust particles at <paramref name="position"/>
    /// into the background particle layer, spread in a cone around
    /// <paramref name="direction"/>.
    /// </summary>
    /// <param name="position">World-space emission centre.</param>
    /// <param name="direction">Angle in radians (0 = right).</param>
    /// <param name="count">Number of particles to emit.</param>
    public static void Burst(Vector2 position, float direction, int count = 1)
    {
        Vector2 spread = AngleToVector(direction - MathF.PI * 0.5f, 4f);
        spread.X = Math.Abs(spread.X);
        spread.Y = Math.Abs(spread.Y);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = new Vector2(
                Nez.Random.Range(-spread.X, spread.X),
                Nez.Random.Range(-spread.Y, spread.Y));
            // TODO: emit "Dust" particle at (position + offset) with direction,
            //       into the background particle layer.
        }
    }

    /// <summary>
    /// Emits <paramref name="count"/> dust particles into the foreground particle
    /// layer, spread in a cone of <paramref name="range"/> pixels.
    /// </summary>
    public static void BurstFG(Vector2 position, float direction,
                                int count = 1, float range = 4f)
    {
        Vector2 spread = AngleToVector(direction - MathF.PI * 0.5f, range);
        spread.X = Math.Abs(spread.X);
        spread.Y = Math.Abs(spread.Y);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = new Vector2(
                Nez.Random.Range(-spread.X, spread.X),
                Nez.Random.Range(-spread.Y, spread.Y));
            // TODO: emit "Dust" particle at (position + offset) with direction,
            //       into the foreground particle layer.
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Vector2 AngleToVector(float angle, float length)
        => new Vector2(MathF.Cos(angle) * length, MathF.Sin(angle) * length);
}
