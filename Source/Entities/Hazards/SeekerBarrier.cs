using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A semi-transparent energy barrier that the Seeker (enemy) cannot pass through,
/// but the player can pass through freely.
/// When a Seeker collides with it the barrier flashes, solidifies temporarily,
/// and propagates the solidify effect to adjacent barriers.
/// Ported from Celeste's SeekerBarrier.cs.
///
/// Porting notes:
/// <list type="bullet">
///   <item>Original extended <c>Solid</c>; here it is a plain <see cref="Nez.Entity"/>
///         — actual solid behaviour for Seekers should be handled by the Seeker's AI
///         via <see cref="ContainsPoint"/>.</item>
///   <item>Particle rendering (falling dots) kept as a data list; callers draw via
///         <see cref="GetParticles"/> or the TODO render stub.</item>
///   <item>SeekerBarrierRenderer tracking replaced with TODO stub.</item>
/// </list>
/// </summary>
public class SeekerBarrier : Nez.Entity
{
    // ── Geometry ──────────────────────────────────────────────────────────────
    public float Width  { get; private set; }
    public float Height { get; private set; }

    // ── Flash / solidify state ────────────────────────────────────────────────
    /// <summary>Flash intensity in [0, 1], decays toward 0.</summary>
    public float Flash    { get; private set; }

    /// <summary>
    /// Solidify strength in [0, 1].  When > 0, Seeker AI should treat this as solid.
    /// </summary>
    public float Solidify { get; private set; }

    /// <summary>True while the barrier is actively flashing.</summary>
    public bool Flashing  { get; private set; }

    private float solidifyDelay;

    // ── Falling-dot particles ─────────────────────────────────────────────────
    private readonly List<Vector2> particles = new();
    private readonly float[] particleSpeeds  = { 12f, 20f, 40f };

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Barrier width in pixels.</param>
    /// <param name="height">Barrier height in pixels.</param>
    public SeekerBarrier(Vector2 position, float width, float height)
    {
        Position = position;
        Width    = width;
        Height   = height;
        Name     = "SeekerBarrier";

        // Seed falling-dot particles (one per 16 px²).
        int count = (int)(width * height / 16f);
        for (int i = 0; i < count; i++)
            particles.Add(new Vector2(
                Nez.Random.NextFloat(width  - 1f),
                Nez.Random.NextFloat(height - 1f)));

        // TODO: register with SeekerBarrierRenderer tracker
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Flash decay.
        if (Flashing)
        {
            Flash = Math.Max(0f, Flash - dt * 4f);
            if (Flash <= 0f)
                Flashing = false;
        }
        else if (solidifyDelay > 0f)
        {
            solidifyDelay -= dt;
        }
        else if (Solidify > 0f)
        {
            Solidify = Math.Max(0f, Solidify - dt);
        }

        // Advance falling-dot particles downward (wrap at bottom).
        for (int i = 0; i < particles.Count; i++)
        {
            Vector2 p = particles[i];
            p.Y = (p.Y + particleSpeeds[i % particleSpeeds.Length] * dt) % (Height - 1f);
            particles[i] = p;
        }
        // TODO: draw particles via SeekerBarrierRenderer (white pixels at Position + particle)
        // TODO: if Flashing, draw flash rect (Color.White * Flash * 0.5f) over the barrier area
    }

    // ── Seeker reflection ─────────────────────────────────────────────────────
    /// <summary>
    /// Call this when a Seeker collides with this barrier.
    /// Triggers a flash + solidify and propagates to adjacent barriers.
    /// </summary>
    public void OnReflectSeeker()
    {
        Flash         = 1f;
        Solidify      = 1f;
        solidifyDelay = 1f;
        Flashing      = true;

        // Propagate to adjacent barriers in the scene.
        var all = Scene?.FindEntitiesWithTag(0).OfType<SeekerBarrier>().ToList();
        if (all == null || all.Count == 0) return;

        RectangleF expandedH = new RectangleF(Position.X - 2f, Position.Y,           Width + 4f, Height);
        RectangleF expandedV = new RectangleF(Position.X,       Position.Y - 2f, Width,       Height + 4f);

        foreach (var other in all)
        {
            if (other == this || other.Flashing) continue;

            var otherBounds = new RectangleF(other.Position.X, other.Position.Y, other.Width, other.Height);
            if (expandedH.Intersects(otherBounds) || expandedV.Intersects(otherBounds))
                other.OnReflectSeeker();
        }
    }

    // ── Public queries ────────────────────────────────────────────────────────
    /// <summary>AABB of this barrier.</summary>
    public RectangleF Bounds => new RectangleF(Position.X, Position.Y, Width, Height);

    /// <summary>Returns true if <paramref name="point"/> is inside this barrier.</summary>
    public bool ContainsPoint(Vector2 point)
        => point.X >= Position.X && point.X <= Position.X + Width &&
           point.Y >= Position.Y && point.Y <= Position.Y + Height;

    /// <summary>Read-only access to the particle positions (for rendering).</summary>
    public IReadOnlyList<Vector2> GetParticles() => particles;
}
