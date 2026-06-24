using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Enemies;

/// <summary>
/// Simplified port of Celeste's AngryOshiro.cs.
///
/// Oshiro chases the player horizontally across the screen, accelerating to
/// keep up when the player dashes and easing back when the player slows down.
///
/// Behaviour overview:
/// <list type="bullet">
///   <item>
///     <b>Chase</b> — accelerates toward the player's X position at up to
///     <see cref="MaxChaseSpeed"/> px/s.  The gap to the player is maintained
///     via a soft "preferred distance" so Oshiro doesn't overshoot and block.
///   </item>
///   <item>
///     <b>Attack</b> — triggered when Oshiro is within <see cref="AttackRange"/>
///     of the player; rushes at <see cref="AttackSpeed"/> px/s briefly.
///   </item>
///   <item>
///     <b>Wait</b> — brief pause state used after an attack or at spawn when
///     <paramref name="fromCutscene"/> is <c>true</c>.
///   </item>
///   <item>Player can dash into Oshiro to trigger a brief stun.</item>
///   <item>Contact with the player deals 1 point of damage.</item>
/// </list>
///
/// Collision: circle collider, radius 12.
/// </summary>
public class AngryOshiro : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Circle collider radius (pixels).</summary>
    public const float ColliderRadius = 12f;

    /// <summary>Maximum horizontal chase speed (pixels / second).</summary>
    public const float MaxChaseSpeed = 120f;

    /// <summary>Horizontal acceleration toward the player (pixels / second²).</summary>
    public const float ChaseAcceleration = 200f;

    /// <summary>Deceleration when the player is not dashing (pixels / second²).</summary>
    public const float ChaseDeceleration = 80f;

    /// <summary>Preferred horizontal gap between Oshiro and the player (pixels).</summary>
    public const float PreferredGap = 24f;

    /// <summary>
    /// Horizontal distance at which Oshiro transitions from Chase to Attack (pixels).
    /// </summary>
    public const float AttackRange = 32f;

    /// <summary>Attack rush speed (pixels / second).</summary>
    public const float AttackSpeed = 260f;

    /// <summary>Duration of the Attack rush (seconds).</summary>
    public const float AttackDuration = 0.3f;

    /// <summary>Duration of the Wait / stun state (seconds).</summary>
    public const float WaitDuration = 0.8f;

    /// <summary>Duration of the stun when hit by a player dash (seconds).</summary>
    public const float StunDuration = 1.2f;

    // -------------------------------------------------------------------------
    // State machine
    // -------------------------------------------------------------------------

    /// <summary>Discrete AI states for Oshiro.</summary>
    public enum OshiroState
    {
        /// <summary>Brief pause — used at spawn or after an attack.</summary>
        Wait,
        /// <summary>Accelerating to match the player's X position.</summary>
        Chase,
        /// <summary>Short horizontal rush toward the player.</summary>
        Attack,
        /// <summary>Temporarily immobilised by a player dash.</summary>
        Stunned,
    }

    // -------------------------------------------------------------------------
    // Public properties
    // -------------------------------------------------------------------------

    /// <summary>Current AI state.</summary>
    public OshiroState CurrentState { get; private set; } = OshiroState.Wait;

    /// <summary>Whether Oshiro entered the scene from a cutscene trigger.</summary>
    public bool FromCutscene { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _stateTimer;
    private float _currentSpeedX;
    private Vector2 _attackDirection;
    private PlayerController? _player;
    private CircleCollider? _collider;
    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates an <see cref="AngryOshiro"/> at the given world position.
    /// </summary>
    /// <param name="position">World position (entity centre).</param>
    /// <param name="fromCutscene">
    ///   When <c>true</c>, Oshiro starts in the <see cref="OshiroState.Wait"/>
    ///   state for a longer initial delay (simulating a cutscene entry).
    /// </param>
    public AngryOshiro(Vector2 position, bool fromCutscene = false)
    {
        _spawnPosition = position;
        FromCutscene   = fromCutscene;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        Entity.Position = _spawnPosition;

        _collider = Entity.AddComponent(new CircleCollider(ColliderRadius));
        _collider.IsTrigger = true;

        // TODO: load sprite
        // e.g. Entity.AddComponent(new SpriteRenderer(oshiroTexture));

        // Longer initial wait if entering from a cutscene.
        ChangeState(OshiroState.Wait, FromCutscene ? 2.0f : WaitDuration);
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public void Update()
    {
        float dt = Time.DeltaTime;

        _player ??= Entity.Scene?.FindComponentOfType<PlayerController>();
        _stateTimer -= dt;

        switch (CurrentState)
        {
            case OshiroState.Wait:    UpdateWait(dt);    break;
            case OshiroState.Chase:   UpdateChase(dt);   break;
            case OshiroState.Attack:  UpdateAttack(dt);  break;
            case OshiroState.Stunned: UpdateStunned(dt); break;
        }

        // Check contact with player.
        CheckPlayerContact();
    }

    // -------------------------------------------------------------------------
    // State handlers
    // -------------------------------------------------------------------------

    private void UpdateWait(float dt)
    {
        // Bleed off any horizontal speed.
        _currentSpeedX = Approach(_currentSpeedX, 0f, ChaseDeceleration * dt);
        Entity.Position += new Vector2(_currentSpeedX * dt, 0f);

        if (_stateTimer <= 0f)
            ChangeState(OshiroState.Chase);
    }

    private void UpdateChase(float dt)
    {
        if (_player == null) return;

        Vector2 playerPos = _player.Entity.Position;
        float targetX = playerPos.X - PreferredGap * Math.Sign(Entity.Position.X - playerPos.X);
        float dx = targetX - Entity.Position.X;

        // TODO: read player.IsDashing to decide acceleration vs deceleration.
        bool playerIsDashing = false; // placeholder

        float accel = playerIsDashing ? ChaseAcceleration : ChaseDeceleration;
        _currentSpeedX = Approach(_currentSpeedX, Math.Sign(dx) * MaxChaseSpeed, accel * dt);

        Entity.Position += new Vector2(_currentSpeedX * dt, 0f);

        // Trigger attack when close enough horizontally.
        float distX = Math.Abs(Entity.Position.X - playerPos.X);
        if (distX <= AttackRange)
            ChangeState(OshiroState.Attack);
    }

    private void UpdateAttack(float dt)
    {
        // Rush in the locked direction.
        Entity.Position += _attackDirection * AttackSpeed * dt;

        if (_stateTimer <= 0f)
            ChangeState(OshiroState.Wait);
    }

    private void UpdateStunned(float dt)
    {
        _currentSpeedX = Approach(_currentSpeedX, 0f, ChaseDeceleration * 2f * dt);
        Entity.Position += new Vector2(_currentSpeedX * dt, 0f);

        if (_stateTimer <= 0f)
            ChangeState(OshiroState.Chase);
    }

    // -------------------------------------------------------------------------
    // Collision / interaction
    // -------------------------------------------------------------------------

    // Reusable results buffer — kept static so we don't allocate each frame.
    private static readonly Collider[] _overlapResults = new Collider[8];

    private void CheckPlayerContact()
    {
        if (_collider == null || _player == null) return;

        int count = Nez.Physics.OverlapCircleAll(Entity.Position, ColliderRadius, _overlapResults);

        for (int i = 0; i < count; i++)
        {
            var hit = _overlapResults[i];
            if (hit == _collider) continue;

            var player = hit.Entity?.GetComponent<PlayerController>();
            if (player == null) continue;

            // TODO: replace with player.IsDashing check.
            bool playerIsDashing = false; // placeholder

            if (playerIsDashing && CurrentState != OshiroState.Stunned)
            {
                // Player dashes into Oshiro → brief stun.
                ChangeState(OshiroState.Stunned, StunDuration);

                // TODO: play stun sound
                // TODO: emit stun particles
            }
            else if (CurrentState != OshiroState.Stunned)
            {
                // Damage the player.
                Vector2 knockDir = player.Entity.Position - Entity.Position;
                if (knockDir == Vector2.Zero) knockDir = Vector2.UnitX;
                knockDir.Normalize();

                player.TakeDamage(1, knockDir);

                // TODO: play contact sound
            }

            break;
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Transitions to <paramref name="newState"/> and sets the state timer.
    /// </summary>
    private void ChangeState(OshiroState newState, float? overrideDuration = null)
    {
        CurrentState = newState;

        _stateTimer = overrideDuration ?? newState switch
        {
            OshiroState.Wait    => WaitDuration,
            OshiroState.Chase   => float.MaxValue,
            OshiroState.Attack  => AttackDuration,
            OshiroState.Stunned => StunDuration,
            _                   => 1f,
        };

        // Lock attack direction when entering Attack state.
        if (newState == OshiroState.Attack && _player != null)
        {
            _attackDirection = _player.Entity.Position - Entity.Position;
            if (_attackDirection != Vector2.Zero)
                _attackDirection.Normalize();
        }

        // TODO: trigger animation changes per state
    }

    /// <summary>
    /// Linearly moves <paramref name="current"/> toward <paramref name="target"/>
    /// by at most <paramref name="maxDelta"/> per call (mirrors Calc.Approach).
    /// </summary>
    private static float Approach(float current, float target, float maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;
        return current + Math.Sign(target - current) * maxDelta;
    }
}
