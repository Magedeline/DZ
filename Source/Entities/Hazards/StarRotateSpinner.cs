using Microsoft.Xna.Framework;
using Nez;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A moon-blade / star spinner that orbits a pivot point, cycling through three
/// colour variants and emitting a particle trail while moving.
/// Ported from Celeste's StarRotateSpinner.cs.
///
/// Inherits rotation math from <see cref="RotateSpinner"/>.
/// Sprite colour cycling and particle emission are noted as TODO stubs.
/// </summary>
public class StarRotateSpinner : RotateSpinner
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const int   ColorCount      = 3;
    private const float TrailInterval   = 0.03f;
    private const float ColorInterval   = 0.8f;

    // ── State ─────────────────────────────────────────────────────────────────
    /// <summary>Current colour index in [0, 2] — cycles every <see cref="ColorInterval"/> seconds.</summary>
    private int colorID;

    private float trailTimer;
    private float colorTimer;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="position">World position of the spinner.</param>
    /// <param name="pivotCenter">Orbit pivot.</param>
    /// <param name="clockwise">Orbit direction.</param>
    public StarRotateSpinner(Vector2 position, Vector2 pivotCenter, bool clockwise)
        : base(position, pivotCenter, clockwise)
    {
        colorID = Nez.Random.Range(0, ColorCount);
        Name    = "StarRotateSpinner";
        // TODO: load sprite: moonBlade — play animation "idle{colorID}"
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        base.Update(); // orbit math + player collision

        float dt = Time.DeltaTime;

        if (Moving)
        {
            // Trail particles.
            trailTimer += dt;
            if (trailTimer >= TrailInterval)
            {
                trailTimer = 0f;
                // TODO: emit particles: StarTrail[colorID] (1 particle at Position, spread 3 px)
            }
        }

        // Colour cycling.
        colorTimer += dt;
        if (colorTimer >= ColorInterval)
        {
            colorTimer = 0f;
            colorID = (colorID + 1) % ColorCount;
            // TODO: play sprite animation: moonBlade "spin{colorID}"
        }
    }

    // ── Player collision override ─────────────────────────────────────────────
    public override void OnPlayer(MadelinePlayer player)
    {
        base.OnPlayer(player);
    }
}
