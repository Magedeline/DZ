using Microsoft.Xna.Framework;
using Nez;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A temple blade that orbits a fixed pivot point, emitting a particle trail
/// while moving and periodically playing a spin animation.
/// Ported from Celeste's BladeRotateSpinner.cs.
///
/// Inherits rotation logic from <see cref="RotateSpinner"/>.
/// Audio + sprite replaced with TODO stubs; particle emission is noted inline.
/// </summary>
public class BladeRotateSpinner : RotateSpinner
{
    // ── Timing ────────────────────────────────────────────────────────────────
    /// <summary>How often the trail particle is emitted (seconds).</summary>
    private const float TrailInterval = 0.04f;

    /// <summary>How often the spin animation re-triggers (seconds).</summary>
    private const float SpinAnimInterval = 1f;

    // ── Timers ────────────────────────────────────────────────────────────────
    private float trailTimer;
    private float spinAnimTimer;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a BladeRotateSpinner.
    /// </summary>
    /// <param name="position">Starting world position of the blade.</param>
    /// <param name="pivotCenter">Pivot point the blade orbits.</param>
    /// <param name="clockwise">True = clockwise orbit.</param>
    public BladeRotateSpinner(Vector2 position, Vector2 pivotCenter, bool clockwise)
        : base(position, pivotCenter, clockwise)
    {
        Name = "BladeRotateSpinner";
        // TODO: load sprite: templeBlade — play "idle" animation
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        base.Update(); // handles orbit math + player collision

        float dt = Time.DeltaTime;

        // Emit trail particles at regular intervals.
        trailTimer += dt;
        if (trailTimer >= TrailInterval)
        {
            trailTimer = 0f;
            // TODO: emit particles: BladeTrail (2 particles at Position, spread 3 px)
        }

        // Trigger spin animation periodically.
        spinAnimTimer += dt;
        if (spinAnimTimer >= SpinAnimInterval)
        {
            spinAnimTimer = 0f;
            // TODO: play sprite animation: templeBlade "spin"
        }
    }

    // ── Player collision override ─────────────────────────────────────────────
    public override void OnPlayer(MadelinePlayer player)
    {
        // Delegate to base which calls player.Die and stops movement.
        base.OnPlayer(player);
    }
}
