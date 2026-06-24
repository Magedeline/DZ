using Microsoft.Xna.Framework;
using Nez;
using System;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Falling cherry-blossom petal background effect.
/// Ported from Celeste's Petals.cs (which extends Backdrop).
///
/// 40 petals drift downward, spinning gently.  They wrap around a 352×212
/// virtual viewport (slightly larger than the 320×180 camera) so they appear
/// seamlessly even with camera movement.
/// </summary>
public class Petals : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Colour palette
    // -------------------------------------------------------------------------

    private static readonly Color[] Colors = { new Color(0xFF, 0x3A, 0xA3) }; // pink

    // -------------------------------------------------------------------------
    // Private types
    // -------------------------------------------------------------------------

    private struct Petal
    {
        public Vector2 Position;
        public float   Speed;
        public float   Spin;
        public float   MaxRotate;
        public int     ColorIndex;
        public float   RotationCounter;
    }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly Petal[] _petals = new Petal[40];
    private float            _fade;

    /// <summary>Whether the petals should be visible (fades in/out).</summary>
    public bool Visible { get; set; } = true;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Petals()
    {
        for (int i = 0; i < _petals.Length; i++)
            Reset(i);
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        for (int i = 0; i < _petals.Length; i++)
        {
            _petals[i].Position.Y    += _petals[i].Speed * dt;
            _petals[i].RotationCounter += _petals[i].Spin * dt;

            // Wrap around 212-pixel height
            if (_petals[i].Position.Y > 212f)
                Reset(i);
        }

        _fade = Approach(_fade, Visible ? 1f : 0f, dt);
    }

    // -------------------------------------------------------------------------
    // Rendering (call from scene renderer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Renders all petals.  <paramref name="cameraPosition"/> is the world-space
    /// camera origin so petals wrap correctly.
    ///
    /// Expects the "particles/petal" texture to be available.
    /// </summary>
    public void Render(Vector2 cameraPosition)
    {
        if (_fade <= 0f) return;

        for (int i = 0; i < _petals.Length; i++)
        {
            float x = Mod(_petals[i].Position.X - cameraPosition.X, 352f) - 16f;
            float y = Mod(_petals[i].Position.Y - cameraPosition.Y, 212f) - 16f;

            float angleRadians = MathF.PI * 0.5f
                               + MathF.Sin(_petals[i].RotationCounter * _petals[i].MaxRotate);

            Vector2 pos = new Vector2(x, y)
                        + new Vector2(MathF.Cos(angleRadians), MathF.Sin(angleRadians)) * 4f;

            // TODO: draw "particles/petal" texture centred at pos,
            //   color = Colors[_petals[i].ColorIndex] * _fade,
            //   scale = 1f, rotation = angleRadians - 0.8f
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void Reset(int i)
    {
        _petals[i] = new Petal
        {
            Position        = new Vector2(Nez.Random.Range(0, 352), Nez.Random.Range(0, 212)),
            Speed           = Nez.Random.Range(6f, 16f),
            Spin            = Nez.Random.Range(8f, 12f) * 0.2f,
            MaxRotate       = Nez.Random.Range(0.3f, 0.6f) * MathF.PI * 0.5f,
            ColorIndex      = Nez.Random.NextInt(Colors.Length),
            RotationCounter = Nez.Random.NextAngle()
        };
    }

    private static float Approach(float v, float target, float maxDelta)
        => v < target ? Math.Min(v + maxDelta, target) : Math.Max(v - maxDelta, target);

    private static float Mod(float x, float m) => (x % m + m) % m;
}
