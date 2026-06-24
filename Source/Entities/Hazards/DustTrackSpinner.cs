using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A dust-bunny spinner that slides along a fixed track (Start ↔ End),
/// emitting particles while moving and adjusting its eye direction at
/// each endpoint.
/// Ported from Celeste's DustTrackSpinner.cs.
///
/// Inherits track movement from <see cref="TrackSpinner"/>.
/// DustGraphic eye direction logic is preserved as commented math;
/// actual DustGraphic rendering is a TODO stub.
/// </summary>
public class DustTrackSpinner : TrackSpinner
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const float ParticleInterval = 0.02f;

    // ── Eye direction state ────────────────────────────────────────────────────
    /// <summary>
    /// Outward direction from the track (toward a wall, if established).
    /// Zero when no outward wall was found.
    /// </summary>
    private Vector2 outwards = Vector2.Zero;

    /// <summary>Current eye direction (for DustGraphic integration).</summary>
    public Vector2 EyeDirection       { get; private set; }

    /// <summary>Smoothed eye target direction.</summary>
    public Vector2 EyeTargetDirection { get; private set; }

    // ── Timers ────────────────────────────────────────────────────────────────
    private float particleTimer;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="start">World-space start node.</param>
    /// <param name="end">World-space end node.</param>
    /// <param name="speed">Speed preset.</param>
    /// <param name="startCenter">Begin at the track midpoint.</param>
    public DustTrackSpinner(Vector2 start, Vector2 end, Speeds speed, bool startCenter = false)
        : base(start, end, speed, startCenter)
    {
        Name = "DustTrackSpinner";

        // Initial eye direction faces toward the End.
        Vector2 initialEye = SafeNormalize(end - start);
        EyeDirection = EyeTargetDirection = initialEye;
        // TODO: load sprite: DustGraphic (hasGraphic=true) — set initial EyeDirection
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        base.Update(); // movement + player collision

        float dt = Time.DeltaTime;

        // Emit particles while moving (but not while paused at an endpoint).
        if (Moving && PauseTimer <= 0f)
        {
            particleTimer += dt;
            if (particleTimer >= ParticleInterval)
            {
                particleTimer = 0f;
                // TODO: emit particles: DustMove (1 particle at Position, spread 4 px)
            }
        }
    }

    // ── Track callbacks ───────────────────────────────────────────────────────
    public override void OnTrackEnd()
    {
        // Recompute eye target direction at each endpoint (mirrors Celeste source).
        // The target angle lerps 30 % of the way from outwards toward the travel
        // direction (Up = toward End, Down = toward Start → Angle + π).
        float travelAngle = Up ? Angle + MathF.PI : Angle;

        if (outwards != Vector2.Zero)
        {
            float outwardsAngle = (float)Math.Atan2(outwards.Y, outwards.X);
            float eyeAngle = AngleLerp(outwardsAngle, travelAngle, 0.3f);
            EyeTargetDirection = new Vector2(MathF.Cos(eyeAngle), MathF.Sin(eyeAngle));
        }
        else
        {
            EyeTargetDirection = new Vector2(MathF.Cos(travelAngle), MathF.Sin(travelAngle));
            // Flip eye Y when no outward wall found (mirrors EyeFlip = -EyeFlip).
            EyeTargetDirection = new Vector2(EyeTargetDirection.X, -EyeTargetDirection.Y);
        }
        // TODO: update DustGraphic EyeTargetDirection to EyeTargetDirection
    }

    // ── Player collision override ─────────────────────────────────────────────
    public override void OnPlayer(MadelinePlayer player)
    {
        base.OnPlayer(player);
        // TODO: trigger DustGraphic OnHitPlayer reaction
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Vector2 SafeNormalize(Vector2 v)
    {
        float len = v.Length();
        return len > 0f ? v / len : Vector2.Zero;
    }

    /// <summary>
    /// Angle lerp that wraps correctly across the ±π boundary.
    /// Mirrors Celeste's Calc.AngleLerp.
    /// </summary>
    private static float AngleLerp(float from, float to, float t)
    {
        float diff = to - from;
        // Wrap diff to [-π, π]
        while (diff >  MathF.PI) diff -= MathF.PI * 2f;
        while (diff < -MathF.PI) diff += MathF.PI * 2f;
        return from + diff * t;
    }
}
