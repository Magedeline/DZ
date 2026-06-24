using Microsoft.Xna.Framework;
using Nez;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A temple blade that slides back and forth along a fixed track,
/// emitting a particle trail while moving and playing a spin animation
/// when it starts each pass.
/// Ported from Celeste's BladeTrackSpinner.cs.
///
/// Inherits track-movement logic from <see cref="TrackSpinner"/>.
/// Audio + sprite animations replaced with TODO stubs.
/// </summary>
public class BladeTrackSpinner : TrackSpinner
{
    // ── Timing ────────────────────────────────────────────────────────────────
    private const float TrailInterval = 0.04f;

    // ── State ─────────────────────────────────────────────────────────────────
    /// <summary>True once the spinner has started its first pass (used to gate audio).</summary>
    private bool hasStarted;

    /// <summary>True while the spinner is mid-travel (emits trail particles).</summary>
    private bool trail;

    private float trailTimer;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a BladeTrackSpinner.
    /// </summary>
    /// <param name="start">World-space start node.</param>
    /// <param name="end">World-space end node.</param>
    /// <param name="speed">Speed preset.</param>
    /// <param name="startCenter">Begin at mid-point of the track.</param>
    public BladeTrackSpinner(Vector2 start, Vector2 end, Speeds speed, bool startCenter = false)
        : base(start, end, speed, startCenter)
    {
        Name = "BladeTrackSpinner";
        // TODO: load sprite: templeBlade — play "idle" animation
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        base.Update(); // handles movement + player collision

        float dt = Time.DeltaTime;

        // Emit trail particles while moving.
        if (trail)
        {
            trailTimer += dt;
            if (trailTimer >= TrailInterval)
            {
                trailTimer = 0f;
                // TODO: emit particles: BladeTrail (2 particles at Position, spread 3 px)
            }
        }
    }

    // ── Track callbacks ───────────────────────────────────────────────────────
    public override void OnTrackStart()
    {
        // TODO: play sprite animation: templeBlade "spin"
        if (hasStarted)
        {
            // TODO: play sound: event:/game/05_mirror_temple/bladespinner_spin
        }
        hasStarted = true;
        trail = true;
    }

    public override void OnTrackEnd()
    {
        trail = false;
        trailTimer = 0f;
    }

    // ── Player collision override ─────────────────────────────────────────────
    public override void OnPlayer(MadelinePlayer player)
    {
        base.OnPlayer(player);
    }
}
