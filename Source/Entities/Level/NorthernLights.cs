using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Northern lights (aurora borealis) background effect.
/// Ported from Celeste's NorthernLights.cs.
///
/// Renders glowing curtain-like vertical strands across the sky plus drifting
/// star particles.  In Celeste this is a <c>Backdrop</c>; here it is a
/// standalone <see cref="Nez.Component"/> that manages its own data and exposes
/// a <see cref="Render"/> method for a custom renderer to call.
/// </summary>
public class NorthernLights : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Colour palette
    // -------------------------------------------------------------------------

    private static readonly Color[] Colors =
    {
        new Color(0x2D, 0xE0, 0x79), // green
        new Color(0x62, 0xF4, 0xF6), // cyan
        new Color(0x45, 0xBC, 0x2E), // lime
        new Color(0x38, 0x56, 0xF0), // blue-violet
    };

    // -------------------------------------------------------------------------
    // Public configuration
    // -------------------------------------------------------------------------

    public float OffsetY            { get; set; } = 0f;
    public float NorthernLightsAlpha { get; set; } = 1f;

    // -------------------------------------------------------------------------
    // Nested types
    // -------------------------------------------------------------------------

    private class StrandNode
    {
        public Vector2 Position;
        public float   TextureOffset;
        public float   Height;
        public float   TopAlpha;
        public float   BottomAlpha;
        public float   SineOffset;
        public Color   Color;
    }

    private class Strand
    {
        public List<StrandNode> Nodes    = new();
        public float            Duration;
        public float            Percent;
        public float            Alpha;

        public void Reset(float startPercent)
        {
            Percent  = startPercent;
            Duration = 12f + Nez.Random.NextFloat() * 20f;
            Alpha    = 0f;
            Nodes.Clear();

            Vector2 pos = new Vector2(
                Nez.Random.Range(-40, 60),
                Nez.Random.Range(40, 90));
            float   texOff = Nez.Random.NextFloat();
            Color   col    = Colors[Nez.Random.NextInt(Colors.Length)];

            for (int i = 0; i < 40; i++)
            {
                Nodes.Add(new StrandNode
                {
                    Position      = pos,
                    TextureOffset = texOff,
                    Height        = Nez.Random.Range(10, 80),
                    TopAlpha      = Nez.Random.Range(0.3f, 0.8f),
                    BottomAlpha   = Nez.Random.Range(0.5f, 1f),
                    SineOffset    = Nez.Random.NextFloat() * MathF.PI * 2f,
                    Color         = Color.Lerp(col, Colors[Nez.Random.NextInt(Colors.Length)],
                                               Nez.Random.Range(0f, 0.3f))
                });
                texOff += Nez.Random.Range(0.02f, 0.2f);
                pos    += new Vector2(
                    Nez.Random.Range(4, 20),
                    Nez.Random.Range(-15, 15));
            }
        }
    }

    private struct Particle
    {
        public Vector2 Position;
        public float   Speed;
        public Color   Color;
    }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly List<Strand> _strands   = new();
    private readonly Particle[]   _particles = new Particle[50];
    private float                 _timer;

    // Gradient background colours
    private static readonly Color GradTop    = new Color(0x02, 0x08, 0x25);
    private static readonly Color GradBottom = new Color(0x17, 0x0C, 0x2F);

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public NorthernLights()
    {
        for (int i = 0; i < 3; i++)
        {
            var s = new Strand();
            s.Reset(Nez.Random.NextFloat());
            _strands.Add(s);
        }

        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i] = new Particle
            {
                Position = new Vector2(Nez.Random.Range(0, 320), Nez.Random.Range(0, 180)),
                Speed    = Nez.Random.Range(4f, 14f),
                Color    = Colors[Nez.Random.NextInt(Colors.Length)]
            };
        }
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;
        _timer += dt;

        foreach (var strand in _strands)
        {
            strand.Percent += dt / strand.Duration;
            strand.Alpha    = Approach(strand.Alpha,
                                       strand.Percent < 1f ? 1f : 0f,
                                       dt);
            if (strand.Alpha <= 0f && strand.Percent >= 1f)
                strand.Reset(0f);

            foreach (var node in strand.Nodes)
                node.SineOffset += dt;
        }

        for (int i = 0; i < _particles.Length; i++)
            _particles[i].Position.Y += _particles[i].Speed * dt;
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    /// <summary>
    /// Renders the aurora effect.  Call from a custom background renderer.
    /// Expects the "northernlights" texture to be bound.
    ///
    /// In Celeste this uses <c>VertexPositionColorTexture</c> quads rendered
    /// to an intermediate render target.  Here we stub the geometry upload
    /// and leave concrete draw calls as TODOs.
    /// </summary>
    public void Render(Vector2 cameraPosition)
    {
        // TODO: draw gradient background quad (GradTop → GradBottom, 320x180)

        foreach (var strand in _strands)
        {
            if (strand.Nodes.Count < 2) continue;
            var n1 = strand.Nodes[0];
            for (int i = 1; i < strand.Nodes.Count; i++)
            {
                var   n2   = strand.Nodes[i];
                float a1   = Math.Min(1f, i / 4f)                          * NorthernLightsAlpha;
                float a2   = Math.Min(1f, (strand.Nodes.Count - i) / 4f)  * NorthernLightsAlpha;
                float oy1  = OffsetY + MathF.Sin(n1.SineOffset) * 3f;
                float oy2  = OffsetY + MathF.Sin(n2.SineOffset) * 3f;

                // TODO: submit 6 VertexPositionColorTexture verts for the quad:
                //   (n1.pos.X, n1.pos.Y + oy1, n1.tex, 1)  color n1.Color * n1.BottomAlpha * strand.Alpha * a1
                //   (n1.pos.X, n1.pos.Y - n1.Height + oy1, n1.tex, 0.05)  …TopAlpha…
                //   (n2.pos.X, n2.pos.Y - n2.Height + oy2, n2.tex, 0.05)  …
                //   (repeat for second triangle)

                n1 = n2;
            }
        }

        // Particles (star field) – wrap around 320x180 viewport
        for (int i = 0; i < _particles.Length; i++)
        {
            float x = Mod(_particles[i].Position.X - cameraPosition.X * 0.2f, 320f);
            float y = Mod(_particles[i].Position.Y - cameraPosition.Y * 0.2f, 180f);
            // TODO: Draw.Rect(x, y, 1, 1, _particles[i].Color)
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static float Approach(float v, float target, float maxDelta)
        => v < target ? Math.Min(v + maxDelta, target) : Math.Max(v - maxDelta, target);

    private static float Mod(float x, float m) => (x % m + m) % m;
}
