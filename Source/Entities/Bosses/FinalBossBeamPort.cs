using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Bosses;

// ═══════════════════════════════════════════════════════════════════════════════
// Beam state enum
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>Discrete phases of a <see cref="FinalBossBeamPort"/> attack.</summary>
public enum BeamState
{
    /// <summary>
    /// 1.4-second charge phase: angle slowly tracks the player for 0.9 s then locks.
    /// </summary>
    Charging,
    /// <summary>
    /// 0.12-second fire phase: deals damage to any player overlapping the beam line.
    /// </summary>
    Active,
    /// <summary>The beam has expired and will be removed from the scene.</summary>
    Done,
}

// ═══════════════════════════════════════════════════════════════════════════════
// FinalBossBeamPort
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Port of Celeste's <c>FinalBossBeam.cs</c>.
///
/// <para>
/// The beam has two phases:
/// <list type="bullet">
///   <item>
///     <b>Charging</b> (1.4 s): the angle slowly rotates toward the player's current
///     position for the first 0.9 s, then locks in place for the remaining 0.5 s so the
///     player can dodge.
///   </item>
///   <item>
///     <b>Active</b> (0.12 s): the beam fires and checks each frame whether any player
///     centre is within <see cref="BeamHalfWidth"/> px of the infinite ray cast from
///     <c>boss.BeamOrigin</c> along the locked angle. A hit calls
///     <c>player.TakeDamage(1, knockback)</c>.
///   </item>
/// </list>
/// The beam geometry is a 2000 px ray; visually it should be rendered as a thin line or
/// sprite along that ray.
/// </para>
///
/// Initialise via <see cref="Init"/> after adding to the scene.
/// </summary>
public class FinalBossBeamPort : Entity, IUpdatable
{
    // ── Timing ────────────────────────────────────────────────────────────────

    /// <summary>Total duration of the charging phase (seconds).</summary>
    public const float ChargeDuration = 1.4f;

    /// <summary>
    /// How long the angle continues to follow the player during charging (seconds).
    /// After this the angle locks.
    /// </summary>
    public const float TrackDuration = 0.9f;

    /// <summary>Duration of the active (firing) phase (seconds).</summary>
    public const float ActiveDuration = 0.12f;

    // ── Geometry ─────────────────────────────────────────────────────────────

    /// <summary>Length of the beam ray in pixels.</summary>
    public const float BeamLength = 2000f;

    /// <summary>
    /// Half-width of the beam collision strip in pixels.
    /// A player whose centre is within this distance of the ray counts as hit.
    /// </summary>
    public const float BeamHalfWidth = 4f;

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>Current phase of this beam.</summary>
    public BeamState CurrentState { get; private set; } = BeamState.Charging;

    /// <summary>Time elapsed within the current phase (seconds).</summary>
    private float _phaseTimer;

    /// <summary>Current aim angle in radians (measured from positive-X, standard math coords).</summary>
    private float _angle;

    /// <summary>Whether the angle has been locked (tracking phase has ended).</summary>
    private bool _angleLocked;

    // ── References ────────────────────────────────────────────────────────────

    private FinalBossPort? _boss;

    /// <summary>
    /// The boss that owns this beam.  Set via <see cref="Init"/>.
    /// </summary>
    public FinalBossPort? Boss => _boss;

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an uninitialised beam.  Call <see cref="Init"/> immediately after
    /// adding to the scene.
    /// </summary>
    public FinalBossBeamPort()
    {
        Name = "FinalBossBeamPort";
    }

    // ── Init ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Binds this beam to its owning <paramref name="boss"/> and aims it toward
    /// <paramref name="targetPosition"/> as the initial tracking angle.
    /// </summary>
    /// <param name="boss">The <see cref="FinalBossPort"/> that fired this beam.</param>
    /// <param name="targetPosition">World-space position to aim at initially (usually the player's position at fire time).</param>
    public void Init(FinalBossPort boss, Vector2 targetPosition)
    {
        _boss         = boss;
        _angle        = (float)Math.Atan2(
            targetPosition.Y - boss.BeamOrigin.Y,
            targetPosition.X - boss.BeamOrigin.X);
        _angleLocked  = false;
        _phaseTimer   = 0f;
        CurrentState  = BeamState.Charging;

        // TODO: load sprite – add SpriteRenderer or custom render component for beam visuals
        // TODO: play sound: event:/game/07_summit/boss_beam_charge
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
        if (_boss == null || CurrentState == BeamState.Done) return;

        float dt = Time.DeltaTime;
        _phaseTimer += dt;

        switch (CurrentState)
        {
            case BeamState.Charging:
                UpdateCharging(dt);
                break;
            case BeamState.Active:
                UpdateActive();
                break;
        }

        // Position the beam entity at the boss's beam origin each frame.
        if (_boss != null)
            Position = _boss.BeamOrigin;
    }

    // ── Phase updates ─────────────────────────────────────────────────────────

    private void UpdateCharging(float dt)
    {
        // Track the player for the first TrackDuration seconds.
        if (!_angleLocked && _phaseTimer < TrackDuration)
        {
            var player = Scene?.FindComponentOfType<PlayerController>();
            if (player != null && _boss != null)
            {
                float targetAngle = (float)Math.Atan2(
                    player.Entity.Position.Y - _boss.BeamOrigin.Y,
                    player.Entity.Position.X - _boss.BeamOrigin.X);

                // Smoothly interpolate toward the player's angle.
                // Approx: rotate at max 180°/s.
                float maxRotate = MathF.PI * dt;
                float delta     = WrapAngle(targetAngle - _angle);
                float clampedDelta = MathHelper.Clamp(delta, -maxRotate, maxRotate);
                _angle += clampedDelta;
            }
        }
        else if (!_angleLocked && _phaseTimer >= TrackDuration)
        {
            // Lock the angle.
            _angleLocked = true;
        }

        // Transition to Active phase after ChargeDuration.
        if (_phaseTimer >= ChargeDuration)
        {
            _phaseTimer  = 0f;
            CurrentState = BeamState.Active;

            // TODO: play sound: event:/game/07_summit/boss_beam_fire
            // TODO: emit particles – bright flash/bloom at beam origin
        }
    }

    private void UpdateActive()
    {
        // Deal damage each frame while active.
        CheckPlayerHit();

        if (_phaseTimer >= ActiveDuration)
        {
            CurrentState = BeamState.Done;
            // TODO: play sound: event:/game/07_summit/boss_beam_end
            Destroy();
        }
    }

    // ── Collision ────────────────────────────────────────────────────────────

    /// <summary>
    /// Tests whether the player's centre lies within <see cref="BeamHalfWidth"/> px
    /// of the beam ray and, if so, deals 1 damage.
    /// </summary>
    private void CheckPlayerHit()
    {
        var player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null || _boss == null) return;

        Vector2 origin    = _boss.BeamOrigin;
        Vector2 direction = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle));
        Vector2 playerPos = player.Entity.Position;

        if (IsPointOnBeam(playerPos, origin, direction))
        {
            // Knockback away from beam origin.
            Vector2 toPlayer = playerPos - origin;
            float   len      = toPlayer.Length();
            Vector2 knockback = len > 0f ? toPlayer / len : Vector2.UnitY;

            player.TakeDamage(1, knockback);
        }
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="point"/> is within
    /// <see cref="BeamHalfWidth"/> pixels of the ray starting at <paramref name="origin"/>
    /// going in <paramref name="direction"/> for <see cref="BeamLength"/> pixels.
    /// </summary>
    private static bool IsPointOnBeam(Vector2 point, Vector2 origin, Vector2 direction)
    {
        Vector2 toPoint   = point - origin;
        float   proj      = Vector2.Dot(toPoint, direction);

        // Only test within the beam's length (and on the correct side).
        if (proj < 0f || proj > BeamLength) return false;

        // Perpendicular distance from the ray.
        Vector2 closest   = origin + direction * proj;
        float   perpDist  = Vector2.Distance(point, closest);

        return perpDist <= BeamHalfWidth;
    }

    // ── Angle helpers ─────────────────────────────────────────────────────────

    /// <summary>Wraps <paramref name="angle"/> to the range [−π, +π].</summary>
    private static float WrapAngle(float angle)
    {
        while (angle >  MathF.PI) angle -= MathF.Tau;
        while (angle < -MathF.PI) angle += MathF.Tau;
        return angle;
    }

    // ── Debug helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the two world-space endpoints of the current beam ray.
    /// Useful for debug rendering.
    /// </summary>
    public (Vector2 start, Vector2 end) GetBeamEndpoints()
    {
        Vector2 origin    = _boss?.BeamOrigin ?? Position;
        Vector2 direction = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle));
        return (origin, origin + direction * BeamLength);
    }
}
