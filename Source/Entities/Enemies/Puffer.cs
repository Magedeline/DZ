using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;
using Component = Nez.Component;

namespace KirbyCelesteStandalone.Entities.Enemies;

/// <summary>
/// Simplified port of Celeste's Puffer.cs.
///
/// The Puffer is a proximity-triggered explosive enemy that patrols back and
/// forth until it detects the player nearby, then inflates and detonates.
///
/// State machine:
/// <list type="bullet">
///   <item>
///     <b>Idle</b> — patrols at <see cref="PatrolSpeed"/> px/s, reversing
///     when the patrol timer expires.
///   </item>
///   <item>
///     <b>Alert</b> — player is within <see cref="AlertRange"/>; puffer
///     inflates for <see cref="AlertDuration"/> seconds then explodes.
///     A dashing player immediately skips to <see cref="PufferState.Exploding"/>.
///   </item>
///   <item>
///     <b>Exploding</b> — deals damage to any player within
///     <see cref="ExplosionRadius"/> and applies pushback; explosion lasts
///     <see cref="ExplosionDuration"/> seconds.
///   </item>
///   <item>
///     <b>Respawning</b> — invisible and inert for <see cref="RespawnDuration"/>
///     seconds before returning to <see cref="PufferState.Idle"/> at the
///     original spawn position.
///   </item>
/// </list>
///
/// Collision: circle collider, radius 8.
/// </summary>
public class Puffer : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Circle collider radius (pixels).</summary>
    public const float ColliderRadius = 8f;

    /// <summary>Patrol movement speed (pixels / second).</summary>
    public const float PatrolSpeed = 24f;

    /// <summary>Seconds between patrol direction reversals.</summary>
    public const float PatrolTurnTime = 1.8f;

    /// <summary>Distance at which the Puffer enters Alert mode (pixels).</summary>
    public const float AlertRange = 60f;

    /// <summary>Seconds the Puffer spends inflating before exploding.</summary>
    public const float AlertDuration = 0.5f;

    /// <summary>Radius of the explosion damage / pushback zone (pixels).</summary>
    public const float ExplosionRadius = 40f;

    /// <summary>Duration of the explosion visual (seconds).</summary>
    public const float ExplosionDuration = 0.3f;

    /// <summary>
    /// Magnitude of the knockback velocity applied to the player (pixels / second).
    /// </summary>
    public const float PushbackSpeed = 200f;

    /// <summary>Seconds the Puffer spends respawning before returning.</summary>
    public const float RespawnDuration = 2.5f;

    // -------------------------------------------------------------------------
    // State machine
    // -------------------------------------------------------------------------

    /// <summary>Discrete states of the Puffer AI.</summary>
    public enum PufferState
    {
        /// <summary>Patrolling calmly.</summary>
        Idle,
        /// <summary>Player is close — inflating, about to explode.</summary>
        Alert,
        /// <summary>Explosion is occurring — damages nearby player.</summary>
        Exploding,
        /// <summary>Recovering after explosion before reappearing.</summary>
        Respawning,
    }

    // -------------------------------------------------------------------------
    // Public properties
    // -------------------------------------------------------------------------

    /// <summary>Current AI state.</summary>
    public PufferState CurrentState { get; private set; } = PufferState.Idle;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _stateTimer;
    private float _patrolDir;           // 1 = right, -1 = left
    private bool _explosionApplied;     // ensures damage is dealt once per explosion

    private PlayerController? _player;
    private CircleCollider?  _collider;

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="Puffer"/> at <paramref name="position"/>.
    /// </summary>
    /// <param name="position">World position (entity centre).</param>
    /// <param name="left">
    ///   When <c>true</c> the Puffer initially patrols to the left.
    /// </param>
    public Puffer(Vector2 position, bool left = false)
    {
        _spawnPosition = position;
        _patrolDir     = left ? -1f : 1f;
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
        // e.g. Entity.AddComponent(new SpriteRenderer(pufferTexture));

        ChangeState(PufferState.Idle);
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
            case PufferState.Idle:       UpdateIdle(dt);       break;
            case PufferState.Alert:      UpdateAlert(dt);      break;
            case PufferState.Exploding:  UpdateExploding(dt);  break;
            case PufferState.Respawning: UpdateRespawning(dt); break;
        }
    }

    // -------------------------------------------------------------------------
    // State handlers
    // -------------------------------------------------------------------------

    private void UpdateIdle(float dt)
    {
        // Patrol left/right.
        Entity.Position += new Vector2(_patrolDir * PatrolSpeed * dt, 0f);

        if (_stateTimer <= 0f)
        {
            _patrolDir *= -1f;
            _stateTimer = PatrolTurnTime;
        }

        // Alert if player is close.
        if (IsPlayerWithin(AlertRange))
            ChangeState(PufferState.Alert);
    }

    private void UpdateAlert(float dt)
    {
        // Stand still while inflating.
        // TODO: scale sprite up to show inflation animation.

        // Check if player dashes into us — trigger immediate explosion.
        if (_player != null)
        {
            // TODO: replace with player.IsDashing check.
            bool playerIsDashing = false; // placeholder

            if (playerIsDashing && IsPlayerWithin(ColliderRadius + 4f))
            {
                ChangeState(PufferState.Exploding);
                return;
            }
        }

        // Player moved away — return to patrol.
        if (!IsPlayerWithin(AlertRange * 1.1f))
        {
            ChangeState(PufferState.Idle);
            return;
        }

        if (_stateTimer <= 0f)
            ChangeState(PufferState.Exploding);
    }

    private void UpdateExploding(float dt)
    {
        // Apply damage / pushback once at the start of the explosion.
        if (!_explosionApplied)
        {
            _explosionApplied = true;
            ApplyExplosion();
        }

        if (_stateTimer <= 0f)
            ChangeState(PufferState.Respawning);
    }

    private void UpdateRespawning(float dt)
    {
        // Invisible and inert — just wait.
        if (_stateTimer <= 0f)
        {
            Respawn();
        }
    }

    // -------------------------------------------------------------------------
    // Explosion logic
    // -------------------------------------------------------------------------

    /// <summary>
    /// Deals damage and applies pushback to the player if they are within the
    /// explosion radius.
    /// </summary>
    private void ApplyExplosion()
    {
        // TODO: emit explosion particles
        // e.g. ParticleSystem.Emit(explosionParticles, Entity.Position);

        // TODO: play explosion sound
        // e.g. Audio.Play("event:/game/general/puffer_explode");

        if (_player == null) return;

        float distToPlayer = Vector2.Distance(Entity.Position, _player.Entity.Position);

        if (distToPlayer <= ExplosionRadius)
        {
            // Damage.
            Vector2 knockDir = _player.Entity.Position - Entity.Position;
            if (knockDir == Vector2.Zero) knockDir = Vector2.UnitY * -1f; // default: up
            knockDir.Normalize();

            _player.TakeDamage(1, knockDir);

            // Pushback proportional to proximity (closer = stronger).
            float strength = 1f - (distToPlayer / ExplosionRadius);
            _player.Velocity += knockDir * PushbackSpeed * strength;

            // TODO: emit player-hit particles
        }
    }

    // -------------------------------------------------------------------------
    // Respawn
    // -------------------------------------------------------------------------

    /// <summary>Resets the Puffer at its original spawn position.</summary>
    private void Respawn()
    {
        Entity.Position   = _spawnPosition;
        _explosionApplied = false;
        SetVisible(true);
        ChangeState(PufferState.Idle);

        // TODO: play respawn sound
        // TODO: emit respawn particles
    }

    // -------------------------------------------------------------------------
    // State helpers
    // -------------------------------------------------------------------------

    /// <summary>Transitions to <paramref name="newState"/> and resets the timer.</summary>
    private void ChangeState(PufferState newState)
    {
        CurrentState = newState;

        _stateTimer = newState switch
        {
            PufferState.Idle       => PatrolTurnTime,
            PufferState.Alert      => AlertDuration,
            PufferState.Exploding  => ExplosionDuration,
            PufferState.Respawning => RespawnDuration,
            _                      => 1f,
        };

        // Hide during respawn; show otherwise.
        SetVisible(newState != PufferState.Respawning);

        if (_collider != null)
            _collider.SetEnabled(newState != PufferState.Respawning);

        // TODO: trigger animation / scale changes per state
    }

    /// <summary>Shows or hides the Puffer's rendered sprite.</summary>
    private void SetVisible(bool visible)
    {
        // TODO: e.g. GetComponent<SpriteRenderer>()?.SetEnabled(visible);
        _ = visible; // suppress unused-variable warning until sprite is wired
    }

    // -------------------------------------------------------------------------
    // Proximity helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the player is within <paramref name="range"/>
    /// pixels of the Puffer centre.
    /// </summary>
    private bool IsPlayerWithin(float range)
    {
        if (_player == null) return false;
        return Vector2.Distance(Entity.Position, _player.Entity.Position) <= range;
    }
}
