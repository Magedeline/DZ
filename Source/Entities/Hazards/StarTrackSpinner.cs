using Microsoft.Xna.Framework;
using DZ.Nez;
using DZ.Entities.Player;

namespace DZ.Entities.Hazards;

/// <summary>
/// A moon-blade / star spinner that slides along a fixed track (Start ↔ End),
/// cycling through three colour variants and emitting a particle trail while
/// moving.  On each track start, the colour advances and a spin animation plays.
/// Ported from Celeste's StarTrackSpinner.cs.
///
/// Inherits track movement from <see cref="TrackSpinner"/>.
/// Audio + sprite animations replaced with TODO stubs.
/// </summary>
public class StarTrackSpinner : TrackSpinner
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const int   ColorCount    = 3;
    private const float TrailInterval = 0.03f;

    // ── State ─────────────────────────────────────────────────────────────────
    /// <summary>True once the spinner has made its first pass (gates audio).</summary>
    private bool hasStarted;

    /// <summary>Current colour index in [0, 2].</summary>
    private int colorID;

    /// <summary>True while mid-travel; trail particles are emitted.</summary>
    private bool trail;

    private float trailTimer;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="start">World-space start node.</param>
    /// <param name="end">World-space end node.</param>
    /// <param name="speed">Speed preset.</param>
    /// <param name="startCenter">Begin at the track midpoint.</param>
    public StarTrackSpinner(Vector2 start, Vector2 end, Speeds speed, bool startCenter = false)
        : base(start, end, speed, startCenter)
    {
        colorID = DZ.Nez.Random.Range(0, ColorCount);
        Name    = "StarTrackSpinner";
        // TODO: load sprite: moonBlade — play animation "idle{colorID}"
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        base.Update(); // movement + player collision

        if (!trail) return;

        float dt = Time.DeltaTime;
        trailTimer += dt;
        if (trailTimer >= TrailInterval)
        {
            trailTimer = 0f;
            // TODO: emit particles: StarTrail[colorID] (1 particle at Position, spread 3 px)
        }
    }

    // ── Track callbacks ───────────────────────────────────────────────────────
    public override void OnTrackStart()
    {
        colorID = (colorID + 1) % ColorCount;
        // TODO: play sprite animation: moonBlade "spin{colorID}"

        if (hasStarted)
        {
            // TODO: play sound: event:/game/05_mirror_temple/bladespinner_spin
        }
        hasStarted = true;
        trail      = true;
        trailTimer = 0f;
    }

    public override void OnTrackEnd()
    {
        trail      = false;
        trailTimer = 0f;
    }

    // ── Player collision override ─────────────────────────────────────────────
    public override void OnPlayer(MadelinePlayer player)
    {
        base.OnPlayer(player);
    }
}
