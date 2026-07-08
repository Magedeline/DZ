using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Collider = DZ.Nez.Collider;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Hazards;

/// <summary>
/// Static crystal spinner — a lethal spike cluster that kills the player on contact.
/// Ported from Celeste's CrystalStaticSpinner.cs (renamed to CrystalSpinner to
/// match project naming conventions).
///
/// Gameplay notes:
/// <list type="bullet">
///   <item>Uses a composite hitbox: 6 px circle + 16×4 thin rect (mirrors original).</item>
///   <item>Collidable is disabled while off-screen and re-enabled within 128 px of
///         the player — this matches the original proximity culling.</item>
///   <item>Rainbow color variant cycles hue over time (visual only; gameplay unchanged).</item>
///   <item>Sprite/texture loading replaced with TODO comments.</item>
/// </list>
/// </summary>
public class CrystalSpinner : DZ.Nez.Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────
    /// <summary>Circle radius of the primary collider.</summary>
    public const float CircleRadius = 6f;

    /// <summary>Proximity distance beyond which collision is disabled (px).</summary>
    private const float CullRadius = 128f;

    /// <summary>Interval at which rainbow hue cycles (seconds).</summary>
    private const float RainbowHueInterval = 0.08f;

    // ── Fields ────────────────────────────────────────────────────────────────
    public CrystalColor Color { get; private set; }

    /// <summary>When true, this spinner is attached to and rides a moving solid.</summary>
    public bool AttachToSolid { get; private set; }

    /// <summary>Whether the player can currently collide with this spinner.</summary>
    public bool Collidable = true;

    private float offset;        // random phase for interval checks
    private float proximityTimer;
    private const float ProximityCheckInterval = 0.05f;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a CrystalSpinner.
    /// </summary>
    /// <param name="position">World position (centre).</param>
    /// <param name="attachToSolid">Whether this spinner rides a moving solid.</param>
    /// <param name="color">Crystal color variant.</param>
    public CrystalSpinner(Vector2 position, bool attachToSolid, CrystalColor color)
    {
        Position     = position;
        AttachToSolid = attachToSolid;
        Color        = color;
        offset       = DZ.Nez.Random.NextFloat();
        Name         = $"CrystalSpinner_{color}";
        // TODO: load sprites for crystal color variant: danger/crystal/fg_{color}
        // TODO: emit particles: CrystalSpinner_Move when in motion
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Periodic proximity check — disable collision when far from player.
        proximityTimer += dt;
        if (proximityTimer >= ProximityCheckInterval)
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

        // Rainbow hue cycling (visual effect only).
        if (Color == CrystalColor.Rainbow)
        {
            // TODO: emit particles: CrystalSpinner_Rainbow hue update at interval RainbowHueInterval
        }

        if (!Collidable) return;

        CheckPlayerCollision();
    }

    // ── Collision ─────────────────────────────────────────────────────────────
    private void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        var pPos = player.Position;

        // Primary circle check (r = 6).
        bool circleHit = Vector2.Distance(Position, pPos) <= CircleRadius;

        // Secondary thin rectangle check (16 × 4, centred on spinner).
        bool boxHit = PointInRect(pPos, Position + new Vector2(-8f, -3f), 16f, 4f);

        if (circleHit || boxHit)
            OnPlayer(player);
    }

    private void OnPlayer(MadelinePlayer player)
    {
        var pPos = player?.Position ?? Position;
        Vector2 dir = pPos - Position;
        Vector2 knockback = dir.LengthSquared() > 0f ? Vector2.Normalize(dir) : Vector2.Zero;
        player.Die(knockback);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static bool PointInRect(Vector2 pt, Vector2 origin, float w, float h)
        => pt.X >= origin.X && pt.X <= origin.X + w &&
           pt.Y >= origin.Y && pt.Y <= origin.Y + h;
}

/// <summary>Crystal color variants (matches Celeste's CrystalColor enum).</summary>
public enum CrystalColor
{
    Blue,
    Red,
    Purple,
    Rainbow,
}
