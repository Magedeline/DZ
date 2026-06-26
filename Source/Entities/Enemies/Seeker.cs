using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;
using Component = Nez.Component;
using Collider = Nez.Collider;

namespace KirbyCelesteStandalone.Entities.Enemies;

/// <summary>
/// Simplified port of Celeste's Seeker.cs.
///
/// The Seeker is an enemy that:
/// <list type="bullet">
///   <item>
///     Patrols slowly in its current direction until the player moves within
///     <see cref="DetectionRange"/> pixels.
///   </item>
///   <item>
///     Chases at <see cref="ChaseSpeed"/> px/s once alerted, and performs a
///     short dash burst at <see cref="DashSpeed"/> px/s when the player is
///     very close.
///   </item>
///   <item>
///     Deals 1 point of damage on contact with the player.
///   </item>
///   <item>
///     Can be stunned when <see cref="CanStun"/> is <c>true</c> and the player
///     dashes into it; recovers after <see cref="StunDuration"/> seconds.
///   </item>
/// </list>
///
/// Collision: circle collider, radius 6.
/// </summary>
public class Seeker : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Radius of the circle collider (pixels).</summary>
    public const float ColliderRadius = 6f;

    /// <summary>Distance at which the Seeker begins chasing the player (pixels).</summary>
    public const float DetectionRange = 200f;

    /// <summary>Distance at which the Seeker switches from Chase to Attack dash (pixels).</summary>
    public const float DashTriggerRange = 60f;

    /// <summary>Normal chase speed (pixels / second).</summary>
    public const float ChaseSpeed = 90f;

    /// <summary>Dash burst speed (pixels / second).</summary>
    public const float DashSpeed = 320f;

    /// <summary>Duration of the dash burst (seconds).</summary>
    public const float DashDuration = 0.25f;

    /// <summary>Duration of the patrol movement before reversing direction (seconds).</summary>
    public const float PatrolTime = 1.5f;

    /// <summary>Patrol speed (pixels / second).</summary>
    public const float PatrolSpeed = 30f;

    /// <summary>How long the Seeker stays stunned after a player dash (seconds).</summary>
    public const float StunDuration = 2f;

    // -------------------------------------------------------------------------
    // State machine
    // -------------------------------------------------------------------------

    /// <summary>Discrete behaviour states for the Seeker.</summary>
    public enum SeekerState
    {
        /// <summary>Standing still, scanning for the player.</summary>
        Idle,
        /// <summary>Walking back and forth on a short patrol path.</summary>
        Patrol,
        /// <summary>Moving steadily toward the player.</summary>
        Chase,
        /// <summary>Performing a fast dash burst at the player.</summary>
        Attack,
        /// <summary>Temporarily disabled after being hit by a dash.</summary>
        Stunned,
    }

    // -------------------------------------------------------------------------
    // Public properties
    // -------------------------------------------------------------------------

    /// <summary>Current state of the Seeker AI.</summary>
    public SeekerState CurrentState { get; private set; } = SeekerState.Idle;

    /// <summary>
    /// When <c>true</c> the Seeker can be stunned by the player dashing into it.
    /// </summary>
    public bool CanStun { get; set; } = true;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    /// <summary>Countdown timer used for state durations.</summary>
    private float _stateTimer;

    /// <summary>Velocity vector applied this frame (pixels / second).</summary>
    private Vector2 _velocity;

    /// <summary>Current patrol direction: 1 = right, -1 = left.</summary>
    private float _patrolDir = 1f;

    /// <summary>Normalised direction of the current dash burst.</summary>
    private Vector2 _dashDirection;

    /// <summary>Reference to the player (lazily fetched each frame if null).</summary>
    private PlayerController? _player;

    /// <summary>Circle trigger collider.</summary>
    private CircleCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="Seeker"/> at the given world position.
    /// </summary>
    /// <param name="position">World position (entity centre).</param>
    public Seeker(Vector2 position)
    {
        // Position is set on AddedToEntity via Entity.Position.
        _spawnPosition = position;
    }

    private readonly Vector2 _spawnPosition;

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
        // e.g. Entity.AddComponent(new SpriteRenderer(seekerTexture));

        ChangeState(SeekerState.Patrol);
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public void Update()
    {
        float dt = Time.DeltaTime;

        // Lazily acquire player reference.
        _player ??= Entity.Scene?.FindComponentOfType<PlayerController>();

        _stateTimer -= dt;

        switch (CurrentState)
        {
            case SeekerState.Idle:    UpdateIdle(dt);    break;
            case SeekerState.Patrol:  UpdatePatrol(dt);  break;
            case SeekerState.Chase:   UpdateChase(dt);   break;
            case SeekerState.Attack:  UpdateAttack(dt);  break;
            case SeekerState.Stunned: UpdateStunned(dt); break;
        }

        // Apply movement.
        Entity.Position += _velocity * dt;

        // Check player contact (damage / stun).
        CheckPlayerContact();
    }

    // -------------------------------------------------------------------------
    // State handlers
    // -------------------------------------------------------------------------

    private void UpdateIdle(float dt)
    {
        _velocity = Vector2.Zero;

        if (_stateTimer <= 0f)
            ChangeState(SeekerState.Patrol);

        if (IsPlayerInRange(DetectionRange))
            ChangeState(SeekerState.Chase);
    }

    private void UpdatePatrol(float dt)
    {
        _velocity = new Vector2(_patrolDir * PatrolSpeed, 0f);

        // Reverse after patrol period expires.
        if (_stateTimer <= 0f)
        {
            _patrolDir *= -1f;
            ChangeState(SeekerState.Patrol); // reset timer
        }

        if (IsPlayerInRange(DetectionRange))
            ChangeState(SeekerState.Chase);
    }

    private void UpdateChase(float dt)
    {
        if (_player == null) { ChangeState(SeekerState.Patrol); return; }

        // Move directly toward the player.
        Vector2 toPlayer = _player.Entity.Position - Entity.Position;
        float dist = toPlayer.Length();

        if (dist > 0f)
        {
            _velocity = Vector2.Normalize(toPlayer) * ChaseSpeed;
        }

        // Switch to dash attack when close enough.
        if (dist <= DashTriggerRange)
            ChangeState(SeekerState.Attack);

        // Give up chase if player moves out of range.
        if (dist > DetectionRange * 1.5f)
            ChangeState(SeekerState.Patrol);
    }

    private void UpdateAttack(float dt)
    {
        // Move in the locked dash direction at full speed.
        _velocity = _dashDirection * DashSpeed;

        if (_stateTimer <= 0f)
        {
            // Dash over — bleed off speed and resume chase.
            _velocity = Vector2.Zero;
            ChangeState(SeekerState.Chase);
        }
    }

    private void UpdateStunned(float dt)
    {
        // Slide to a stop during stun.
        _velocity = Vector2.Zero;

        if (_stateTimer <= 0f)
            ChangeState(SeekerState.Patrol);
    }

    // -------------------------------------------------------------------------
    // Collision / interaction
    // -------------------------------------------------------------------------

    // Reusable results buffer — kept static so we don't allocate each frame.
    private static readonly Collider[] _overlapResults = new Collider[8];

    /// <summary>
    /// Checks whether the seeker's collider is overlapping the player and
    /// applies damage or stun as appropriate.
    /// </summary>
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

            // TODO: check player.IsDashing to determine stun vs damage.
            bool playerIsDashing = false; // placeholder

            if (CanStun && playerIsDashing && CurrentState != SeekerState.Stunned)
            {
                OnStunned();
            }
            else if (CurrentState != SeekerState.Stunned)
            {
                // Damage the player, knocking them away.
                Vector2 knockDir = player.Entity.Position - Entity.Position;
                if (knockDir == Vector2.Zero) knockDir = Vector2.UnitX;
                knockDir.Normalize();

                player.TakeDamage(1, knockDir);

                // TODO: play hit sound
                // TODO: emit hit particles
            }

            break;
        }
    }

    /// <summary>
    /// Called when a dashing player hits the Seeker (if <see cref="CanStun"/>).
    /// </summary>
    private void OnStunned()
    {
        ChangeState(SeekerState.Stunned);

        // TODO: play stun sound
        // TODO: emit stun particles
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Transitions to <paramref name="newState"/> and resets the state timer.
    /// </summary>
    private void ChangeState(SeekerState newState)
    {
        CurrentState = newState;

        _stateTimer = newState switch
        {
            SeekerState.Idle    => 0.5f,
            SeekerState.Patrol  => PatrolTime,
            SeekerState.Chase   => float.MaxValue,
            SeekerState.Attack  => DashDuration,
            SeekerState.Stunned => StunDuration,
            _                   => 1f,
        };

        // Lock dash direction when entering Attack.
        if (newState == SeekerState.Attack && _player != null)
        {
            _dashDirection = _player.Entity.Position - Entity.Position;
            if (_dashDirection != Vector2.Zero)
                _dashDirection.Normalize();
        }

        // TODO: trigger animation changes per state
    }

    /// <summary>Returns <c>true</c> if the player is within <paramref name="range"/> pixels.</summary>
    private bool IsPlayerInRange(float range)
    {
        if (_player == null) return false;
        return Vector2.Distance(Entity.Position, _player.Entity.Position) <= range;
    }
}
