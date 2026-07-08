using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's RotatingPlatform.cs.
///
/// A jump-through platform that orbits a fixed <see cref="Center"/> point at a
/// constant angular speed.  The platform is always positioned at radius
/// <see cref="_length"/> from the centre, rotating clockwise or
/// counter-clockwise.
/// </summary>
public class RotatingPlatform : CelesteJumpThru
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    /// <summary>Angular speed in radians/second (≈ 60 °/s).</summary>
    private const float RotateSpeed = 1.04719758f; // π/3

    // ── State ─────────────────────────────────────────────────────────────────

    private readonly Vector2 _center;
    private readonly bool    _clockwise;
    private readonly float   _length;
    private float            _currentAngle;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="RotatingPlatform"/>.
    /// </summary>
    /// <param name="position">Initial world position of the platform.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="center">World-space pivot point to rotate around.</param>
    /// <param name="clockwise">
    ///   <c>true</c> to rotate clockwise (angle decreases),
    ///   <c>false</c> for counter-clockwise.
    /// </param>
    public RotatingPlatform(Vector2 position, int width, Vector2 center, bool clockwise)
        : base(position, width)
    {
        _center       = center;
        _clockwise    = clockwise;
        _length       = (position - center).Length();
        _currentAngle = Angle(position - center);
        Name          = "RotatingPlatform";
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        float dt = Time.DeltaTime;

        if (_clockwise)
            _currentAngle -= RotateSpeed * dt;
        else
            _currentAngle += RotateSpeed * dt;

        _currentAngle = WrapAngle(_currentAngle);

        // Derive new position from polar coordinates.
        Vector2 target = _center + AngleToVector(_currentAngle, _length);
        MoveTo(target);
    }

    // ── Math helpers ──────────────────────────────────────────────────────────

    private static float Angle(Vector2 v) => (float)Math.Atan2(v.Y, v.X);

    private static Vector2 AngleToVector(float angle, float length) =>
        new Vector2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length);

    private static float WrapAngle(float angle)
    {
        while (angle > Math.PI)  angle -= (float)(2 * Math.PI);
        while (angle < -Math.PI) angle += (float)(2 * Math.PI);
        return angle;
    }
}
