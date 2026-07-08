using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Collider = DZ.Nez.Collider;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Hazards;

/// <summary>
/// Base class for spinning hazards that orbit a fixed centre point.
/// Ported from Celeste's RotateSpinner.cs.
///
/// The spinner completes one full orbit in <see cref="RotationTime"/> seconds.
/// Clockwise rotates in the positive (screen-down) direction; counter-clockwise
/// rotates toward the negative angle.  A 6-pixel-radius circle collider is used
/// for player contact detection.
/// </summary>
public class RotateSpinner : DZ.Nez.Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const float RotationTime = 1.8f;

    // ── Fields ────────────────────────────────────────────────────────────────
    /// <summary>Whether this spinner is currently moving.</summary>
    public bool Moving = true;

    /// <summary>Fixed world-space pivot the spinner orbits around.</summary>
    protected Vector2 center;

    /// <summary>Current rotation progress in [0, 1).</summary>
    private float rotationPercent;

    /// <summary>Distance from <see cref="center"/> to the spinner's position.</summary>
    private float length;

    /// <summary>When true the spinner has been destroyed and falls off screen.</summary>
    private bool fallOutOfScreen;

    // ── Properties ────────────────────────────────────────────────────────────
    /// <summary>Whether this spinner orbits clockwise.</summary>
    public bool Clockwise { get; private set; }

    /// <summary>
    /// Current world-space angle from the centre to the spinner's position,
    /// mapped from the range [4.712389 → −1.570796] as <see cref="rotationPercent"/>
    /// goes from 0 → 1 (mirrors the original's Easer which is linear).
    /// </summary>
    public float Angle => MathHelper.Lerp(4.712389f, -1.57079637f, rotationPercent);

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a RotateSpinner.
    /// </summary>
    /// <param name="position">Starting world position of the spinner.</param>
    /// <param name="pivotCenter">World position of the pivot it orbits.</param>
    /// <param name="clockwise">True = clockwise orbit.</param>
    public RotateSpinner(Vector2 position, Vector2 pivotCenter, bool clockwise)
    {
        Position = position;
        center = pivotCenter;
        Clockwise = clockwise;

        // Compute initial rotationPercent from the angle between center and position.
        float rawAngle = (float)Math.Atan2(position.Y - pivotCenter.Y, position.X - pivotCenter.X);
        float wrapped = WrapAngle(rawAngle);
        // Calc.Percent(val, min, max) = (val - min) / (max - min), clamped to [0, 1].
        rotationPercent = Math.Clamp(
            (wrapped - (-1.57079637f)) / (4.712389f - (-1.57079637f)),
            0f, 1f);
        length = (position - pivotCenter).Length();

        // Snap position to the computed angle so it starts exactly on its orbit.
        Position = center + AngleToVector(Angle, length);
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        float dt = Time.DeltaTime;

        if (Moving)
        {
            if (Clockwise)
            {
                rotationPercent -= dt / RotationTime;
                rotationPercent += 1f; // keep positive before mod
            }
            else
            {
                rotationPercent += dt / RotationTime;
            }
            rotationPercent %= 1f;
            Position = center + AngleToVector(Angle, length);
        }

        if (fallOutOfScreen)
        {
            center.Y += 160f * dt;
            // Remove once well below the screen bottom (arbitrary threshold).
            if (Position.Y > center.Y + 2000f)
                Destroy();
        }

        CheckPlayerCollision();
    }

    // ── Player collision ──────────────────────────────────────────────────────
    /// <summary>
    /// Checks if a player is within 6 px (circle collider radius) of this spinner.
    /// Calls <see cref="OnPlayer"/> on hit.
    /// </summary>
    protected virtual void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        float dist = Vector2.Distance(Position, player.Position);
        if (dist <= 6f)
            OnPlayer(player);
    }

    /// <summary>Called when the spinner touches a player. Override in subclasses.</summary>
    public virtual void OnPlayer(MadelinePlayer player)
    {
        Vector2 dir = (GetPlayerPosition(player) - Position);
        Vector2 knockback = dir.LengthSquared() > 0f
            ? Vector2.Normalize(dir)
            : Vector2.Zero;
        player.Die(knockback);
        Moving = false;
    }

    // ── Trigger fall ──────────────────────────────────────────────────────────
    /// <summary>Makes the spinner fall off the bottom of the screen.</summary>
    public void TriggerFallOutOfScreen() => fallOutOfScreen = true;

    // ── Static helpers ────────────────────────────────────────────────────────
    /// <summary>Converts a polar coordinate to a Cartesian offset.</summary>
    protected static Vector2 AngleToVector(float angle, float length)
        => new Vector2(MathF.Cos(angle) * length, MathF.Sin(angle) * length);

    /// <summary>Wraps an angle to the range [−π, π].</summary>
    private static float WrapAngle(float angle)
    {
        while (angle > MathF.PI)  angle -= MathF.PI * 2f;
        while (angle < -MathF.PI) angle += MathF.PI * 2f;
        return angle;
    }

    private static Vector2 GetPlayerPosition(MadelinePlayer p) => p?.Position ?? Vector2.Zero;
}
