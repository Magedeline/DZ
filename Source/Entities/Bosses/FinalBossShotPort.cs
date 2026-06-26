using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;
using Entity = Nez.Entity;

namespace KirbyCelesteStandalone.Entities.Bosses;

/// <summary>
/// Port of Celeste's <c>FinalBossShot.cs</c>.
///
/// <para>
/// A projectile fired by <see cref="FinalBossPort"/> that travels toward a target position
/// at 100 px/s while oscillating perpendicular to its direction of travel (sine-wave wobble).
/// </para>
///
/// <b>Lifecycle:</b>
/// <list type="bullet">
///   <item>
///     <b>Appear delay</b> (0.1 s): the shot exists but does not move or deal damage.
///   </item>
///   <item>
///     <b>Grace period</b> (0.15 s after appear delay): the shot moves but cannot yet
///     kill the player (allows it to clear the boss hitbox).
///   </item>
///   <item>
///     <b>Active</b>: moves and can hit the player.
///   </item>
///   <item>
///     Destroyed when it leaves the camera's extended bounds (+200 px margin) or when it
///     hits the player.
///   </item>
/// </list>
///
/// Call <see cref="Init"/> immediately after adding to the scene.
/// </summary>
public class FinalBossShotPort : Entity, IUpdatable
{
    // ── Physics constants ────────────────────────────────────────────────────

    /// <summary>Travel speed in pixels per second.</summary>
    public const float TravelSpeed = 100f;

    /// <summary>Sine-wave perpendicular amplitude in pixels.</summary>
    public const float SineAmplitude = 3f;

    /// <summary>Sine-wave frequency (cycles per second).</summary>
    public const float SineFrequency = 1.4f;

    // ── Timing constants ──────────────────────────────────────────────────────

    /// <summary>Delay before the shot begins moving (seconds).</summary>
    private const float AppearDelay = 0.1f;

    /// <summary>Duration of the grace period where the shot is moving but cannot kill (seconds).</summary>
    private const float CantKillDuration = 0.15f;

    // ── Hitbox ────────────────────────────────────────────────────────────────

    /// <summary>Half-size of the shot's AABB hit zone in pixels (4×4 box, ±2 in each axis).</summary>
    private const float HitboxHalf = 2f;

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>Seconds remaining before the shot starts moving (<c>appearTimer</c> in original).</summary>
    private float _appearTimer = AppearDelay;

    /// <summary>Seconds remaining in the cant-kill grace period.</summary>
    private float _cantKillTimer;

    /// <summary>Accumulated time for the sine oscillation.</summary>
    private float _sineTime;

    // ── Direction / movement ─────────────────────────────────────────────────

    /// <summary>Unit-length travel direction (toward the target at spawn time).</summary>
    private Vector2 _direction;

    /// <summary>
    /// Perpendicular unit vector (90° counter-clockwise from <see cref="_direction"/>)
    /// used to compute the sine-wave offset.
    /// </summary>
    private Vector2 _perpendicular;

    // ── References ────────────────────────────────────────────────────────────

    private FinalBossPort? _boss;

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an uninitialised shot.  Call <see cref="Init"/> immediately after
    /// adding to the scene.
    /// </summary>
    public FinalBossShotPort()
    {
        Name = "FinalBossShotPort";
    }

    // ── Init ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Binds this shot to its owning <paramref name="boss"/>, sets its start position to
    /// <c>boss.ShotOrigin</c>, and aims it toward <paramref name="targetPosition"/> rotated
    /// by <paramref name="angleOffset"/> radians.
    /// </summary>
    /// <param name="boss">The <see cref="FinalBossPort"/> that fired this shot.</param>
    /// <param name="targetPosition">World-space aim point (usually the player's current position).</param>
    /// <param name="angleOffset">
    ///   Radians to rotate the base aim angle. Positive = counter-clockwise.
    ///   Use this to spread multiple shots (e.g. ±12°).
    /// </param>
    public void Init(FinalBossPort boss, Vector2 targetPosition, float angleOffset = 0f)
    {
        _boss         = boss;
        Position      = boss.ShotOrigin;
        _appearTimer  = AppearDelay;
        _cantKillTimer = AppearDelay + CantKillDuration;
        _sineTime     = 0f;

        // Compute base direction from ShotOrigin to target.
        Vector2 delta = targetPosition - boss.ShotOrigin;
        float   len   = delta.Length();
        Vector2 baseDir = len > 0f ? delta / len : new Vector2(boss.Facing, 0f);

        // Apply the angular offset.
        if (angleOffset != 0f)
        {
            float baseAngle      = (float)Math.Atan2(baseDir.Y, baseDir.X);
            float offsetAngle    = baseAngle + angleOffset;
            baseDir = new Vector2(MathF.Cos(offsetAngle), MathF.Sin(offsetAngle));
        }

        _direction    = baseDir;

        // Perpendicular: rotate direction 90° counter-clockwise.
        _perpendicular = new Vector2(-_direction.Y, _direction.X);

        // TODO: load sprite – add SpriteRenderer with boss shot sprite
        // TODO: play sound: event:/game/07_summit/boss_shot
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
    }

    // ── IUpdatable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Update()
    {
        float dt = Time.DeltaTime;

        // ── Appear delay ─────────────────────────────────────────────────────
        if (_appearTimer > 0f)
        {
            _appearTimer -= dt;
            return; // Not moving yet.
        }

        // ── Cant-kill grace period countdown ─────────────────────────────────
        if (_cantKillTimer > 0f)
            _cantKillTimer -= dt;

        // ── Sine oscillation ─────────────────────────────────────────────────
        _sineTime += dt;
        float prevSineOffset = SineAmplitude * MathF.Sin(MathF.Tau * SineFrequency * (_sineTime - dt));
        float currSineOffset = SineAmplitude * MathF.Sin(MathF.Tau * SineFrequency * _sineTime);

        // ── Position update ──────────────────────────────────────────────────
        // Straight-line component.
        Vector2 linearMove = _direction * TravelSpeed * dt;

        // Perpendicular sine-wave component (delta so the shot wobbles rather than drifts).
        Vector2 sineMove   = _perpendicular * (currSineOffset - prevSineOffset);

        Position += linearMove + sineMove;

        // ── Out-of-bounds check ───────────────────────────────────────────────
        if (IsOutOfBounds())
        {
            Destroy();
            return;
        }

        // ── Player hit check ─────────────────────────────────────────────────
        if (_cantKillTimer <= 0f)
            CheckPlayerHit();
    }

    // ── Collision ────────────────────────────────────────────────────────────

    /// <summary>
    /// AABB of the shot centred on its current <c>Position</c>.
    /// </summary>
    private RectangleF ShotBounds =>
        new RectangleF(
            Position.X - HitboxHalf,
            Position.Y - HitboxHalf,
            HitboxHalf * 2f,
            HitboxHalf * 2f);

    private void CheckPlayerHit()
    {
        var player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return;

        RectangleF playerBounds = player.Entity.GetComponent<BoxCollider>()?.Bounds
                                  ?? RectangleF.Empty;

        if (!ShotBounds.Intersects(playerBounds)) return;

        // Knockback direction: away from the shot's current position.
        Vector2 toPlayer = player.Entity.Position - Position;
        float   len      = toPlayer.Length();
        Vector2 knockback = len > 0f ? toPlayer / len : Vector2.UnitY;

        player.TakeDamage(1, knockback);

        // TODO: emit particles – small burst on impact
        // TODO: play sound: event:/game/07_summit/boss_shot_impact

        Destroy();
    }

    // ── Out-of-bounds ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> when the shot has moved more than 200 px outside the
    /// camera's visible bounds.
    /// </summary>
    private bool IsOutOfBounds()
    {
        if (Scene?.Camera == null) return false;

        const float Margin = 200f;
        RectangleF cam = Scene.Camera.Bounds;

        return Position.X < cam.Left   - Margin
            || Position.X > cam.Right  + Margin
            || Position.Y < cam.Top    - Margin
            || Position.Y > cam.Bottom + Margin;
    }
}
