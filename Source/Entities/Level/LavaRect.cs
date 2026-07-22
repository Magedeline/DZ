using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;
using System.Collections.Generic;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's LavaRect.cs.
///
/// A <see cref="DZ.Nez.Component"/> that renders an animated lava/liquid surface
/// with:
/// <list type="bullet">
///   <item>A sine-wave surface edge that ripples along each side.</item>
///   <item>Rising bubbles that pop on the surface.</item>
///   <item>A filled interior blending from <see cref="SurfaceColor"/> at the
///         edge to <see cref="CenterColor"/> deeper in.</item>
/// </list>
///
/// <see cref="OnlyMode"/> controls whether the wave appears on all four sides
/// (None) or only on specific faces.
///
/// Attach to a <see cref="DZ.Nez.Entity"/> (e.g., <see cref="IceBlock"/>) and
/// call <see cref="Render"/> in the owning entity's Render method, or rely on
/// the component's own Render override.
/// </summary>
public class LavaRect : DZ.Nez.Component, IUpdatable
{
    // ── Only-mode ─────────────────────────────────────────────────────────────

    public enum OnlyModes { None, Floor, Ceiling, Left, Right }

    // ── Visual settings ───────────────────────────────────────────────────────

    public Vector2     Position;
    public float       Fade               = 16f;
    public float       Spikey             = 0f;
    public OnlyModes   OnlyMode           = OnlyModes.None;
    public float       SmallWaveAmplitude = 1f;
    public float       BigWaveAmplitude   = 4f;
    public float       CurveAmplitude     = 12f;
    public float       UpdateMultiplier   = 1f;
    public Color       SurfaceColor       = Color.White;
    public Color       EdgeColor          = Color.LightGray;
    public Color       CenterColor        = Color.DarkGray;

    // ── Dimensions ────────────────────────────────────────────────────────────

    public float Width      { get; private set; }
    public float Height     { get; private set; }
    public int   SurfaceStep { get; private set; }

    // ── Internal state ────────────────────────────────────────────────────────

    private float    _timer;
    // Marks when surface geometry needs recomputing, but nothing consumes the flag yet —
    // the surface is currently recomputed unconditionally each frame.
#pragma warning disable CS0414
    private bool     _dirty = true;
#pragma warning restore CS0414

    // Bubbles.
    private Bubble[]        _bubbles        = Array.Empty<Bubble>();
    private SurfaceBubble[] _surfaceBubbles = Array.Empty<SurfaceBubble>();
    private int             _surfaceBubbleIndex;

    // Vertex buffer (CPU-side, flushed to Draw calls).
    // Full GPU vertex buffer is omitted; we use Nez Draw calls instead.

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="LavaRect"/>.
    /// </summary>
    /// <param name="width">Rectangle width in pixels.</param>
    /// <param name="height">Rectangle height in pixels.</param>
    /// <param name="step">Surface sample step in pixels (smaller = smoother wave).</param>
    public LavaRect(float width, float height, int step)
    {
        _timer = DZ.Nez.Random.NextFloat(100f);
        Resize(width, height, step);
    }

    // ── Resize ────────────────────────────────────────────────────────────────

    public void Resize(float width, float height, int step)
    {
        Width       = width;
        Height      = height;
        SurfaceStep = step;
        _dirty      = true;

        int bubbleCount = (int)(width * height * 0.005f);
        _bubbles = new Bubble[bubbleCount];
        for (int i = 0; i < _bubbles.Length; i++)
        {
            _bubbles[i] = new Bubble
            {
                Position = new Vector2(1f + DZ.Nez.Random.NextFloat(width - 2f), DZ.Nez.Random.NextFloat(height)),
                Speed    = DZ.Nez.Random.Range(4, 12),
                Alpha    = DZ.Nez.Random.Range(0.4f, 0.8f),
            };
        }

        int sbCount = (int)Math.Max(4f, bubbleCount * 0.25f);
        _surfaceBubbles = new SurfaceBubble[sbCount];
        for (int i = 0; i < _surfaceBubbles.Length; i++)
            _surfaceBubbles[i] = new SurfaceBubble { X = -1f };
    }

    // ── IUpdatable.Update ─────────────────────────────────────────────────────

    public override void Update()
    {
        float dt = Time.DeltaTime;
        _timer += UpdateMultiplier * dt;

        if (UpdateMultiplier != 0f) _dirty = true;

        // Move bubbles upward; recycle when they reach the surface.
        for (int i = 0; i < _bubbles.Length; i++)
        {
            _bubbles[i].Position.Y -= UpdateMultiplier * _bubbles[i].Speed * dt;

            float surfaceWave = Wave((int)(_bubbles[i].Position.X / SurfaceStep), Width);
            if (_bubbles[i].Position.Y < 2f - surfaceWave)
            {
                _bubbles[i].Position.Y = Height - 1f;

                // Promote to surface bubble occasionally.
                if (DZ.Nez.Random.NextFloat() < 0.75f)
                {
                    _surfaceBubbles[_surfaceBubbleIndex].X     = _bubbles[i].Position.X;
                    _surfaceBubbles[_surfaceBubbleIndex].Frame  = 0f;
                    _surfaceBubbleIndex = (_surfaceBubbleIndex + 1) % _surfaceBubbles.Length;
                }
            }
        }

        // Advance surface bubble animations.
        const float SurfaceBubbleFps = 6f;
        const float SurfaceBubbleFrames = 4f; // placeholder frame count
        for (int i = 0; i < _surfaceBubbles.Length; i++)
        {
            if (_surfaceBubbles[i].X >= 0f)
            {
                _surfaceBubbles[i].Frame += dt * SurfaceBubbleFps;
                if (_surfaceBubbles[i].Frame >= SurfaceBubbleFrames)
                    _surfaceBubbles[i].X = -1f;
            }
        }
    }

    // ── Component Render ──────────────────────────────────────────────────────

    public override void Render()
    {
        if (Entity == null) return;
        Vector2 worldPos = Entity.Position + Position;
        RenderAt(worldPos);
    }

    /// <summary>
    /// Renders the lava rect with its top-left at <paramref name="worldPos"/>.
    /// Can be called directly from the owning entity's Render method.
    /// </summary>
    public void RenderAt(Vector2 worldPos)
    {
        float x = worldPos.X;
        float y = worldPos.Y;
        var batcher = Graphics.Instance.Batcher;

        // Center fill.
        batcher.DrawRect(x, y, Width, Height, CenterColor);

        // Surface edge — draw each top-edge sample column with the wave.
        if (OnlyMode == OnlyModes.None || OnlyMode == OnlyModes.Floor)
        {
            for (int s = 0; s < Width / SurfaceStep; s++)
            {
                float wx   = x + s * SurfaceStep;
                float wave = Wave(s, Width);
                float edgeH = Math.Min(wave + Fade, Height);

                // Top surface strip.
                batcher.DrawRect(wx, y, SurfaceStep, 1f, SurfaceColor);

                // Edge gradient.
                Color col = Color.Lerp(EdgeColor, CenterColor, Math.Min(wave / Fade, 1f));
                batcher.DrawRect(wx, y + 1f, SurfaceStep, Math.Max(0f, edgeH - 1f), col);
            }
        }

        // Render rising bubbles.
        foreach (var b in _bubbles)
        {
            float bx = x + b.Position.X;
            float by = y + b.Position.Y;
            Color bc = SurfaceColor * b.Alpha;
            batcher.DrawRect(bx, by, 2f, 2f, bc);
        }

        // Render surface bubbles (pop animation placeholder).
        foreach (var sb in _surfaceBubbles)
        {
            if (sb.X < 0f) continue;
            float wave  = Wave((int)(sb.X / SurfaceStep), Width);
            float sbx   = x + sb.X - 2f;
            float sby   = y + wave - 4f;
            float alpha = 1f - sb.Frame / 4f;
            batcher.DrawRect(sbx, sby, 4f, 4f, SurfaceColor * alpha);
        }
    }

    // ── Wave function ─────────────────────────────────────────────────────────

    private float Wave(int step, float length)
    {
        int   val  = step * SurfaceStep;
        float norm = OnlyMode != OnlyModes.None
            ? 1f
            : ClampedMap(val, 0f, length * 0.1f)
              * ClampedMap(val, length * 0.9f, length, 1f, 0f);

        float wave = Sin(val * 0.25f + _timer * 4f) * SmallWaveAmplitude
                   + Sin(val * 0.05f + _timer * 0.5f) * BigWaveAmplitude;

        if (step % 2 == 0) wave += Spikey;

        if (OnlyMode != OnlyModes.None)
            wave += (1f - YoYo((float)val / length)) * CurveAmplitude;

        return wave * norm;
    }

    // ── Math helpers ──────────────────────────────────────────────────────────

    private static float Sin(float v) =>
        (float)((1.0 + Math.Sin(v)) / 2.0);

    private static float ClampedMap(float val, float fromMin, float fromMax,
                                     float toMin = 0f, float toMax = 1f)
    {
        float t = Math.Clamp((val - fromMin) / (fromMax - fromMin), 0f, 1f);
        return toMin + (toMax - toMin) * t;
    }

    private static float YoYo(float t) =>
        t <= 0.5f ? t * 2f : (1f - t) * 2f;

    // ── Data structures ───────────────────────────────────────────────────────

    private struct Bubble
    {
        public Vector2 Position;
        public float   Speed;
        public float   Alpha;
    }

    private struct SurfaceBubble
    {
        public float X;
        public float Frame;
    }
}
