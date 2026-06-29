using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Hazards;

/// <summary>
/// A static dust-bunny spinner — a lethal dust-graphic hazard that kills on
/// contact and periodically culls its collision based on proximity to the player.
/// Ported from Celeste's DustStaticSpinner.cs.
///
/// Gameplay notes:
/// <list type="bullet">
///   <item>Composite collider: 6 px circle + 16×4 thin rect.</item>
///   <item>Collidable is toggled based on player proximity (128 px radius).</item>
///   <item>Emits DustMove particles when within proximity.</item>
///   <item>Dust graphic rendering replaced with TODO stubs.</item>
/// </list>
/// </summary>
public class DustStaticSpinner : DZ.Nez.Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────
    public const float ParticleInterval  = 0.02f;
    private const float CullRadius       = 128f;
    private const float ProximityCheck   = 0.05f;
    private const float CircleRadius     = 6f;

    // ── State ─────────────────────────────────────────────────────────────────
    /// <summary>Whether the player can currently collide with this spinner.</summary>
    public bool Collidable = true;

    /// <summary>Whether this spinner is attached to a moving solid.</summary>
    public bool AttachToSolid { get; private set; }

    private readonly float offset;         // random phase for staggering intervals
    private float proximityTimer;
    private float particleTimer;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a DustStaticSpinner.
    /// </summary>
    /// <param name="position">World position (centre).</param>
    /// <param name="attachToSolid">When true, rides a moving solid.</param>
    public DustStaticSpinner(Vector2 position, bool attachToSolid = false)
    {
        Position    = position;
        AttachToSolid = attachToSolid;
        offset      = DZ.Nez.Random.NextFloat();
        Name        = "DustStaticSpinner";
        // TODO: load sprite: DustGraphic (dust bunny, ignoreSolids=false, flicker=true, autoExpand=true)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Periodic proximity cull check.
        proximityTimer += dt;
        if (proximityTimer >= ProximityCheck)
        {
            proximityTimer = 0f;
            var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
            if (player != null)
            {
                float dx = Math.Abs(player.Position.X - Position.X);
                float dy = Math.Abs(player.Position.Y - Position.Y);
                Collidable = dx < CullRadius && dy < CullRadius;
            }
        }

        // Emit dust-move particles when established and collidable.
        if (Collidable)
        {
            particleTimer += dt;
            if (particleTimer >= ParticleInterval)
            {
                particleTimer = 0f;
                // TODO: emit particles: DustMove (1 particle at Position, spread 4 px)
            }

            CheckPlayerCollision();
        }
    }

    // ── Collision ─────────────────────────────────────────────────────────────
    private void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        var pPos = player.Position;

        bool circleHit = Vector2.Distance(Position, pPos) <= CircleRadius;
        bool boxHit    = PointInRect(pPos, Position + new Vector2(-8f, -3f), 16f, 4f);

        if (circleHit || boxHit)
            OnPlayer(player);
    }

    private void OnPlayer(MadelinePlayer player)
    {
        var pPos = player?.Position ?? Position;
        Vector2 dir = pPos - Position;
        Vector2 knockback = dir.LengthSquared() > 0f ? Vector2.Normalize(dir) : Vector2.Zero;
        player.Die(knockback);
        // TODO: trigger DustGraphic OnHitPlayer reaction
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private static bool PointInRect(Vector2 pt, Vector2 origin, float w, float h)
        => pt.X >= origin.X && pt.X <= origin.X + w &&
           pt.Y >= origin.Y && pt.Y <= origin.Y + h;
}
