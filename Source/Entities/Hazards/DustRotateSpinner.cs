using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A dust-bunny spinner that orbits a pivot point, with eyes that track its
/// direction of travel and a particle trail while in motion.
/// Ported from Celeste's DustRotateSpinner.cs.
///
/// Inherits rotation math from <see cref="RotateSpinner"/>.
/// DustGraphic eye directions are approximated as Vector2 values and passed
/// through TODO stubs.  Particle emission is noted inline.
/// </summary>
public class DustRotateSpinner : RotateSpinner
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const float ParticleInterval = 0.02f;

    // ── Eye direction state ────────────────────────────────────────────────────
    /// <summary>
    /// Current eye direction vector — points perpendicular to the orbit
    /// in the travel direction.  Updated each frame while moving.
    /// </summary>
    public Vector2 EyeDirection        { get; private set; }

    /// <summary>Target eye direction the eyes smoothly track toward.</summary>
    public Vector2 EyeTargetDirection  { get; private set; }

    // ── Timers ────────────────────────────────────────────────────────────────
    private float particleTimer;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="position">World position of the spinner.</param>
    /// <param name="pivotCenter">Orbit pivot.</param>
    /// <param name="clockwise">Direction of orbit.</param>
    public DustRotateSpinner(Vector2 position, Vector2 pivotCenter, bool clockwise)
        : base(position, pivotCenter, clockwise)
    {
        Name = "DustRotateSpinner";
        // TODO: load sprite: DustGraphic (hasGraphic=true)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        base.Update(); // orbit math + player collision

        if (!Moving) return;

        float dt = Time.DeltaTime;

        // Compute the eye direction: 90° perpendicular to the orbit angle,
        // in the direction of travel (matches the original's dusty.EyeDirection logic).
        float perpAngle = Angle + (MathF.PI * 0.5f * (Clockwise ? 1f : -1f));
        Vector2 eyeDir = new Vector2(MathF.Cos(perpAngle), MathF.Sin(perpAngle));

        EyeDirection       = eyeDir;
        EyeTargetDirection = eyeDir;
        // TODO: update DustGraphic EyeDirection and EyeTargetDirection to EyeDirection

        // Trail particles.
        particleTimer += dt;
        if (particleTimer >= ParticleInterval)
        {
            particleTimer = 0f;
            // TODO: emit particles: DustMove (1 particle at Position, spread 4 px)
        }
    }

    // ── Player collision override ─────────────────────────────────────────────
    public override void OnPlayer(MadelinePlayer player)
    {
        base.OnPlayer(player);
        // TODO: trigger DustGraphic OnHitPlayer reaction
    }
}
