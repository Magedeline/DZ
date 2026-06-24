using Microsoft.Xna.Framework;
using Nez;
using System;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Dynamic vertex light component.  Ported from Celeste's VertexLight.cs.
///
/// Acts as a data+behaviour component consumed by the lighting renderer.
/// When the light is inside a solid surface, <see cref="InSolidAlphaMultiplier"/>
/// smoothly drops to 0 so the light doesn't bleed through walls.
///
/// Optional spotlight mode narrows the light to a cone defined by
/// <see cref="SpotlightDirection"/> and <see cref="SpotlightPush"/>.
/// </summary>
public class VertexLight : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    /// <summary>Local position offset from the entity origin.</summary>
    public Vector2 Position
    {
        get => _position;
        set { if (_position != value) { Dirty = true; _position = value; } }
    }

    public float X
    {
        get => _position.X;
        set => Position = new Vector2(value, _position.Y);
    }

    public float Y
    {
        get => _position.Y;
        set => Position = new Vector2(_position.X, value);
    }

    /// <summary>World-space centre of the light.</summary>
    public Vector2 Center => Entity != null ? Entity.Position + _position : _position;

    /// <summary>Light tint colour.</summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>Intensity multiplier (0-1).</summary>
    public float Alpha { get; set; } = 1f;

    /// <summary>Radius at which the light begins to fade out (px).</summary>
    public float StartRadius
    {
        get => _startRadius;
        set { if (_startRadius != value) { Dirty = true; _startRadius = value; } }
    }

    /// <summary>Radius at which the light is completely gone (px).</summary>
    public float EndRadius
    {
        get => _endRadius;
        set { if (_endRadius != value) { Dirty = true; _endRadius = value; } }
    }

    // Spotlight
    public bool  Spotlight          { get; set; }
    public float SpotlightDirection { get; set; }
    public float SpotlightPush      { get; set; }

    // Renderer bookkeeping
    public int   Index              { get; set; } = -1;
    public bool  Dirty              { get; set; } = true;
    public bool  InSolid            { get; set; }
    public float InSolidAlphaMultiplier { get; set; } = 1f;

    // Last-known positions for change-detection / solid-check optimisations
    public Vector2 LastNonSolidPosition { get; set; }
    public Vector2 LastEntityPosition   { get; set; }
    public Vector2 LastPosition         { get; set; }
    public bool    Started              { get; set; }

    // -------------------------------------------------------------------------
    // Private backing fields
    // -------------------------------------------------------------------------

    private Vector2 _position;
    private float   _startRadius = 16f;
    private float   _endRadius   = 32f;

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    public VertexLight() { }

    /// <param name="color">Light colour.</param>
    /// <param name="alpha">Intensity (0-1).</param>
    /// <param name="startFade">Inner fade radius (px).</param>
    /// <param name="endFade">Outer fade radius (px).</param>
    public VertexLight(Color color, float alpha, int startFade, int endFade)
        : this(Vector2.Zero, color, alpha, startFade, endFade) { }

    /// <param name="position">Local offset from entity position.</param>
    /// <param name="color">Light colour.</param>
    /// <param name="alpha">Intensity (0-1).</param>
    /// <param name="startFade">Inner fade radius (px).</param>
    /// <param name="endFade">Outer fade radius (px).</param>
    public VertexLight(Vector2 position, Color color, float alpha, int startFade, int endFade)
    {
        Position    = position;
        Color       = color;
        Alpha       = alpha;
        StartRadius = startFade;
        EndRadius   = endFade;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        LastNonSolidPosition = Center;
        LastEntityPosition   = Entity.Position;
        LastPosition         = _position;
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;
        InSolidAlphaMultiplier = Approach(InSolidAlphaMultiplier, InSolid ? 0f : 1f, dt * 4f);
    }

    // -------------------------------------------------------------------------
    // Tween factories (return manual lerp helpers – Monocle Tween replaced)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="PulseTween"/> that briefly expands then contracts
    /// the light radii.
    /// </summary>
    public PulseTween CreatePulseTween()
    {
        return new PulseTween(this, StartRadius, EndRadius, duration: 0.5f);
    }

    /// <summary>
    /// Creates a fade-in lerp helper starting from alpha 0 up to the current Alpha.
    /// </summary>
    public FadeInTween CreateFadeInTween(float duration)
    {
        float from = 0f;
        float to   = Alpha;
        Alpha = 0f;
        return new FadeInTween(this, from, to, duration);
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static float Approach(float v, float target, float maxDelta)
        => v < target ? Math.Min(v + maxDelta, target) : Math.Max(v - maxDelta, target);
}

// ---------------------------------------------------------------------------
// Tween helpers (replace Monocle Tween)
// ---------------------------------------------------------------------------

/// <summary>Manual pulse tween for <see cref="VertexLight"/> radii.</summary>
public class PulseTween : Nez.Component, IUpdatable
{
    private readonly VertexLight _light;
    private readonly float _startA, _startB, _endA, _endB, _duration;
    private float _timer;

    public PulseTween(VertexLight light, float startRadius, float endRadius, float duration)
    {
        _light    = light;
        _startA   = startRadius;
        _startB   = startRadius + 6f;
        _endA     = endRadius;
        _endB     = endRadius + 12f;
        _duration = duration;
    }

    public void Update()
    {
        float dt = Time.DeltaTime;
        _timer = Math.Min(_timer + dt, _duration);
        float t = _duration > 0f ? _timer / _duration : 1f;
        // CubeOut ease: 1 - (1-t)³
        float eased = 1f - (float)Math.Pow(1f - t, 3);
        _light.StartRadius = MathHelper.Lerp(_startB, _startA, eased);
        _light.EndRadius   = MathHelper.Lerp(_endB,   _endA,   eased);
        if (_timer >= _duration) Entity?.RemoveComponent(this);
    }
}

/// <summary>Manual fade-in tween for <see cref="VertexLight.Alpha"/>.</summary>
public class FadeInTween : Nez.Component, IUpdatable
{
    private readonly VertexLight _light;
    private readonly float _from, _to, _duration;
    private float _timer;

    public FadeInTween(VertexLight light, float from, float to, float duration)
    {
        _light    = light;
        _from     = from;
        _to       = to;
        _duration = duration;
    }

    public void Update()
    {
        float dt = Time.DeltaTime;
        _timer = Math.Min(_timer + dt, _duration);
        float t = _duration > 0f ? _timer / _duration : 1f;
        // CubeOut ease
        float eased = 1f - (float)Math.Pow(1f - t, 3);
        _light.Alpha = MathHelper.Lerp(_from, _to, eased);
        if (_timer >= _duration) Entity?.RemoveComponent(this);
    }
}
