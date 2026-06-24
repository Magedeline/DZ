using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Bosses;

// ═══════════════════════════════════════════════════════════════════════════════
// State enum
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>Discrete states for <see cref="FinalBossPort"/>.</summary>
public enum FinalBossState
{
    /// <summary>Waiting at a node before the fight begins or after being hit.</summary>
    Idle,
    /// <summary>Flying between waypoint nodes at full speed.</summary>
    Moving,
    /// <summary>Executing an attack pattern.</summary>
    Attacking,
    /// <summary>Flashing from a player-dash hit before moving to the next node.</summary>
    HitStun,
    /// <summary>Boss health reached zero; death sequence in progress.</summary>
    Dead,
}

// ═══════════════════════════════════════════════════════════════════════════════
// FinalBossPort
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Port of Celeste's <c>FinalBoss.cs</c> (the Badeline boss fight).
///
/// <para>
/// The boss moves between a series of <see cref="nodes"/> waypoints at 600 px/s, executing
/// an escalating series of attack patterns between movements.  Patterns cycle through indices
/// 0–7 and grow more complex at higher indices.
/// </para>
///
/// <para>
/// <b>Hit detection:</b>
/// <list type="bullet">
///   <item>Player dashes into boss → <see cref="OnDashHit"/> removes one health point, triggers
///     a flash/hit-stun, then the boss moves to the next node.</item>
///   <item>Boss body touches player → <see cref="OnTouchPlayer"/> calls
///     <c>player.TakeDamage(1, direction)</c>.</item>
/// </list>
/// </para>
///
/// <para>
/// Attacks are spawned into the scene via <see cref="FinalBossBeamPort"/> and
/// <see cref="FinalBossShotPort"/>; the boss itself is not a <see cref="CelesteSolid"/>.
/// </para>
///
/// Constructor: <c>FinalBossPort(Vector2 position, Vector2[] nodes, int patternIndex)</c>
/// </summary>
public class FinalBossPort : Entity, IUpdatable
{
    // ── Constants ─────────────────────────────────────────────────────────────

    /// <summary>Travel speed between nodes (pixels per second).</summary>
    public const float MoveSpeed = 600f;

    /// <summary>Circle-collider radius for both the hit-detection and touch-damage zone.</summary>
    public const float ColliderRadius = 14f;

    /// <summary>Delay between the end of one attack and the start of the next (seconds).</summary>
    private const float AttackCooldownDuration = 1.4f;

    /// <summary>Duration of the hit-flash visual (seconds).</summary>
    private const float HitStunDuration = 0.3f;

    /// <summary>Duration of the invulnerability window after a hit (seconds).</summary>
    private const float InvulnerabilityDuration = 0.8f;

    /// <summary>Maximum health points (each player dash removes 1).</summary>
    public const int MaxHealth = 8;

    // ── Nodes / pattern ──────────────────────────────────────────────────────

    private readonly Vector2[] _nodes;
    private int _nodeIndex;
    private readonly int _startPatternIndex;
    private int _patternIndex;

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>Current FSM state.</summary>
    public FinalBossState State { get; private set; } = FinalBossState.Idle;

    /// <summary>Current health (0 = dead).</summary>
    public int Health { get; private set; }

    /// <summary><c>true</c> while flying between nodes.</summary>
    public bool Moving { get; private set; }

    /// <summary>Whether the boss is currently immune to player-dash hits.</summary>
    public bool IsInvulnerable { get; private set; }

    // ── Facing ───────────────────────────────────────────────────────────────

    /// <summary>Horizontal facing direction: -1 = left, +1 = right.</summary>
    private int _facing = -1;

    /// <summary>
    /// Public read accessor for the current facing direction.
    /// Used by <see cref="FinalBossShotPort"/> to determine the default fire direction.
    /// </summary>
    public int Facing => _facing;

    // ── Timers ───────────────────────────────────────────────────────────────

    private float _stateTimer;
    private float _attackCooldown;
    private float _invulnerableTimer;
    private float _hitStunTimer;

    // ── Flash ────────────────────────────────────────────────────────────────

    private float _flashTimer;
    private const float FlashDuration = 0.12f;
    private bool _flash;

    // ── Movement ─────────────────────────────────────────────────────────────

    private Vector2 _moveTarget;
    private float _moveTravelDistance;
    private float _moveTravelled;

    // ── Collider ─────────────────────────────────────────────────────────────

    private CircleCollider? _collider;

    // ── Origin helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// World-space origin from which beam attacks emanate.
    /// </summary>
    public Vector2 BeamOrigin => Position + new Vector2(_facing * 8f, -6f);

    /// <summary>
    /// World-space origin from which shot projectiles are fired.
    /// </summary>
    public Vector2 ShotOrigin => Position + new Vector2(_facing * 6f, 2f);

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="FinalBossPort"/>.
    /// </summary>
    /// <param name="position">Initial world-space position (first node).</param>
    /// <param name="nodes">
    ///   Array of waypoint positions the boss travels between. Must have at least one element.
    /// </param>
    /// <param name="patternIndex">
    ///   Starting attack pattern index (0–7). Higher values unlock more complex patterns
    ///   immediately (useful for rematch scenarios).
    /// </param>
    public FinalBossPort(Vector2 position, Vector2[] nodes, int patternIndex = 0)
    {
        Position = position;

        // Ensure there is always at least the starting position as node 0.
        if (nodes == null || nodes.Length == 0)
            _nodes = new[] { position };
        else
            _nodes = nodes;

        _nodeIndex         = 0;
        _startPatternIndex = Math.Clamp(patternIndex, 0, 7);
        _patternIndex      = _startPatternIndex;
        Health             = MaxHealth;

        Name = "FinalBossPort";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Attach a circle collider for dash-hit and touch-damage queries.
        _collider = AddComponent(new CircleCollider(ColliderRadius));
        _collider.SetLocalOffset(new Vector2(0f, -6f)); // slightly up (matching original hitbox)

        // TODO: load sprite – add SpriteAnimator with Badeline/boss sprite sheet

        // Snap to the first node.
        Position = _nodes[0];

        // Begin idle; first attack will fire after AttackCooldownDuration.
        ChangeState(FinalBossState.Idle);
        _attackCooldown = AttackCooldownDuration;
    }

    // ── IUpdatable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Update()
    {
        if (State == FinalBossState.Dead) return;

        float dt = Time.DeltaTime;

        // ── Timers ───────────────────────────────────────────────────────────
        _stateTimer      -= dt;
        _attackCooldown  -= dt;
        _hitStunTimer    -= dt;

        if (_invulnerableTimer > 0f)
        {
            _invulnerableTimer -= dt;
            if (_invulnerableTimer <= 0f)
                IsInvulnerable = false;
        }

        // Flash visual tick.
        if (_flashTimer > 0f)
        {
            _flashTimer -= dt;
            _flash = (int)(_flashTimer / (FlashDuration * 0.25f)) % 2 == 0;
        }
        else
        {
            _flash = false;
        }

        // ── FSM update ───────────────────────────────────────────────────────
        switch (State)
        {
            case FinalBossState.Idle:
                UpdateIdle(dt);
                break;
            case FinalBossState.Moving:
                UpdateMoving(dt);
                break;
            case FinalBossState.Attacking:
                UpdateAttacking(dt);
                break;
            case FinalBossState.HitStun:
                UpdateHitStun(dt);
                break;
        }

        // ── Face player ──────────────────────────────────────────────────────
        var player = Scene?.FindComponentOfType<PlayerController>();
        if (player != null)
        {
            float dx = player.Entity.Position.X - Position.X;
            if (Math.Abs(dx) > 1f)
                _facing = dx < 0f ? -1 : 1;
        }

        // ── Touch-damage check ───────────────────────────────────────────────
        CheckTouchDamage();

        // ── Dash-hit check ───────────────────────────────────────────────────
        CheckDashHit();
    }

    // ── FSM states ────────────────────────────────────────────────────────────

    private void UpdateIdle(float dt)
    {
        // Wait for attack cooldown, then enter Attacking state.
        if (_attackCooldown <= 0f)
        {
            ChangeState(FinalBossState.Attacking);
        }
    }

    private void UpdateMoving(float dt)
    {
        float step    = MoveSpeed * dt;
        float remaining = _moveTravelDistance - _moveTravelled;

        if (step >= remaining)
        {
            // Arrived at target node.
            Position  = _moveTarget;
            Moving    = false;
            _moveTravelled = _moveTravelDistance;
            ChangeState(FinalBossState.Idle);
            _attackCooldown = AttackCooldownDuration;
            return;
        }

        // Interpolate towards target.
        float t = _moveTravelled / _moveTravelDistance;
        _moveTravelled += step;
        float tNew      = _moveTravelled / _moveTravelDistance;
        Position = Vector2.Lerp(_nodes[Math.Max(_nodeIndex - 1, 0)], _moveTarget, tNew);
    }

    private void UpdateAttacking(float dt)
    {
        // The attack itself is fire-and-forget: we execute it immediately on state entry
        // (see ChangeState), so UpdateAttacking just waits for _stateTimer.
        if (_stateTimer <= 0f)
        {
            // Advance to the next pattern after each successful attack sequence.
            _patternIndex = (_patternIndex + 1) % 8;
            ChangeState(FinalBossState.Idle);
            _attackCooldown = AttackCooldownDuration;
        }
    }

    private void UpdateHitStun(float dt)
    {
        if (_hitStunTimer <= 0f)
        {
            // Move to the next node.
            MoveToNextNode();
        }
    }

    // ── State transitions ─────────────────────────────────────────────────────

    private void ChangeState(FinalBossState next)
    {
        State = next;

        switch (next)
        {
            case FinalBossState.Attacking:
                ExecutePattern(_patternIndex);
                // stateTimer is set inside ExecutePattern.
                break;

            case FinalBossState.HitStun:
                _hitStunTimer  = HitStunDuration;
                _flashTimer    = InvulnerabilityDuration;
                IsInvulnerable = true;
                _invulnerableTimer = InvulnerabilityDuration;
                break;

            case FinalBossState.Dead:
                Moving = false;
                // TODO: play sound: event:/game/07_summit/boss_finalexplosion
                // TODO: emit particles – large explosion/death burst
                Nez.Core.Schedule(2.5f, _ => Destroy());
                break;
        }
    }

    // ── Attack patterns ───────────────────────────────────────────────────────

    /// <summary>
    /// Fires the attack corresponding to <paramref name="index"/> and sets
    /// <c>_stateTimer</c> to the duration the Attacking state should last.
    /// </summary>
    private void ExecutePattern(int index)
    {
        var player = Scene?.FindComponentOfType<PlayerController>();
        Vector2 target = player?.Entity.Position ?? Position + new Vector2(_facing * 50f, 0f);

        switch (index)
        {
            case 0:
                // Pattern 0: single shot straight at player.
                FireShot(target, 0f);
                _stateTimer = 0.8f;
                break;

            case 1:
                // Pattern 1: two shots spread ±12°.
                FireShot(target, +12f * MathF.PI / 180f);
                FireShot(target, -12f * MathF.PI / 180f);
                _stateTimer = 1.0f;
                break;

            case 2:
                // Pattern 2: beam attack.
                FireBeam(target);
                _stateTimer = 2.0f;
                break;

            case 3:
                // Pattern 3: triple shots spread ±18°.
                FireShot(target, 0f);
                FireShot(target, +18f * MathF.PI / 180f);
                FireShot(target, -18f * MathF.PI / 180f);
                _stateTimer = 1.2f;
                break;

            case 4:
                // Pattern 4: beam + flanking shots.
                FireBeam(target);
                FireShot(target, +22f * MathF.PI / 180f);
                FireShot(target, -22f * MathF.PI / 180f);
                _stateTimer = 2.2f;
                break;

            case 5:
                // Pattern 5: four-way spread.
                FireShot(target, 0f);
                FireShot(target, +30f * MathF.PI / 180f);
                FireShot(target, -30f * MathF.PI / 180f);
                FireShot(target, 180f * MathF.PI / 180f);
                _stateTimer = 1.4f;
                break;

            case 6:
                // Pattern 6: double beam.
                FireBeam(target);
                Nez.Core.Schedule(0.6f, _ => FireBeam(target));
                _stateTimer = 2.8f;
                break;

            case 7:
                // Pattern 7: beam + four-way volley.
                FireBeam(target);
                FireShot(target, 0f);
                FireShot(target, +25f * MathF.PI / 180f);
                FireShot(target, -25f * MathF.PI / 180f);
                FireShot(target, +50f * MathF.PI / 180f);
                FireShot(target, -50f * MathF.PI / 180f);
                _stateTimer = 3.0f;
                break;

            default:
                _stateTimer = 0.5f;
                break;
        }
    }

    // ── Attack helpers ────────────────────────────────────────────────────────

    /// <summary>Spawns a <see cref="FinalBossShotPort"/> aimed at <paramref name="target"/> with an angular offset.</summary>
    private void FireShot(Vector2 target, float angleOffset)
    {
        if (Scene == null) return;

        var shot = new FinalBossShotPort();
        Scene.AddEntity(shot);
        shot.Init(this, target, angleOffset);
    }

    /// <summary>Spawns a <see cref="FinalBossBeamPort"/> aimed at <paramref name="target"/>.</summary>
    private void FireBeam(Vector2 target)
    {
        if (Scene == null) return;

        var beam = new FinalBossBeamPort();
        Scene.AddEntity(beam);
        beam.Init(this, target);
    }

    // ── Node movement ─────────────────────────────────────────────────────────

    private void MoveToNextNode()
    {
        if (_nodes.Length <= 1)
        {
            // Only one node — snap back to idle.
            ChangeState(FinalBossState.Idle);
            _attackCooldown = AttackCooldownDuration;
            return;
        }

        _nodeIndex = (_nodeIndex + 1) % _nodes.Length;
        _moveTarget           = _nodes[_nodeIndex];
        _moveTravelDistance   = Vector2.Distance(Position, _moveTarget);
        _moveTravelled        = 0f;
        Moving                = true;

        ChangeState(FinalBossState.Moving);
    }

    // ── Hit detection ─────────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the player is overlapping the boss body and, if so, deals touch damage.
    /// </summary>
    private void CheckTouchDamage()
    {
        if (State == FinalBossState.Dead) return;

        var player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return;

        float dist = Vector2.Distance(Position, player.Entity.Position);
        if (dist > ColliderRadius + 8f) return; // 8px = approx player radius

        Vector2 dir = player.Entity.Position - Position;
        float   len = dir.Length();
        Vector2 knockback = len > 0f ? dir / len : Vector2.UnitX;

        player.TakeDamage(1, knockback);
    }

    /// <summary>
    /// Checks whether the player is currently dashing into the boss.
    /// A dash hit removes 1 health point.
    /// </summary>
    private void CheckDashHit()
    {
        if (IsInvulnerable || State == FinalBossState.Dead) return;

        var player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return;

        // Treat the player as "dashing" when their IsDashing flag is active and overlapping.
        if (!player.IsDashing) return;

        float dist = Vector2.Distance(Position, player.Entity.Position);
        if (dist > ColliderRadius + 6f) return;

        OnDashHit(player);
    }

    /// <summary>
    /// Called when the player successfully dashes into the boss.
    /// Removes one health point, triggers a flash, then moves to the next node.
    /// </summary>
    private void OnDashHit(PlayerController player)
    {
        Health = Math.Max(0, Health - 1);

        // TODO: play sound: event:/game/07_summit/boss_hit
        // TODO: emit particles – hit sparks at Position

        if (Health <= 0)
        {
            ChangeState(FinalBossState.Dead);
            return;
        }

        ChangeState(FinalBossState.HitStun);
    }
}
