using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using Camera = DZ.Nez.Camera;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Enemies;

/// <summary>
/// Player-controlled Seeker entity used in the Mirror Temple void sequence.
/// Ported from Celeste's PlayerSeeker.cs.
///
/// The player takes over a Seeker statue, controlling it with the movement
/// stick. Pressing Dash triggers a high-speed lunge in the aimed direction.
/// The intro plays an un-hatching animation before control is granted.
/// </summary>
public class PlayerSeeker : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const float Acceleration    = 600f;   // px/s²
    private const float MaxSpeed        = 120f;   // px/s during movement
    private const float DashSpeed       = 450f;   // px/s during dash lunge
    private const float DashDuration    = 0.3f;   // seconds
    private const float ScaleApproach   = 2f;     // scale normalisation rate

    // -------------------------------------------------------------------------
    // State machine
    // -------------------------------------------------------------------------

    private enum State { Intro, Idle, Moving, Dashing }

    private State _state = State.Intro;

    // -------------------------------------------------------------------------
    // Private fields
    // -------------------------------------------------------------------------

    private bool _enabled;
    private Vector2 _speed;
    private float _dashTimer;
    private Vector2 _dashDirection;

    // Trail timers
    private float _trailTimerA;
    private float _trailTimerB;

    // Intro sequence timer
    private float _introTimer;

    // Scale squash/stretch (visual only)
    private Vector2 _scale = Vector2.One;

    // Lazy player reference (for kill-on-touch)
    private MadelinePlayer _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public PlayerSeeker(Vector2 position)
    {
        _spawnPosition = position;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // TODO: add "seeker" sprite, play "statue" animation
        // TODO: add Hitbox collider 10x10 offset -5,-5
        // TODO: add VertexLight Color.White alpha 1 startFade 32 endFade 64
        // TODO: add PlayerCollider → OnPlayer

        // TODO: session setup – ColorGrade = "templevoid", CanRetry = false

        // Start intro sequence
        _state = State.Intro;
        _introTimer = 0f;
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        // Normalise scale back to 1 each frame
        _scale.X = Approach(_scale.X, 1f, ScaleApproach * dt);
        _scale.Y = Approach(_scale.Y, 1f, ScaleApproach * dt);
        // TODO: apply _scale to sprite

        switch (_state)
        {
            case State.Intro:
                UpdateIntro(dt);
                break;
            case State.Idle:
            case State.Moving:
                UpdateMovement(dt);
                break;
            case State.Dashing:
                UpdateDash(dt);
                break;
        }

        CheckPlayerContact();
    }

    // -------------------------------------------------------------------------
    // Intro sequence (simplified: just a timed delay)
    // -------------------------------------------------------------------------

    private void UpdateIntro(float dt)
    {
        _introTimer += dt;

        // Phase 1 (0-3s): glitch + put player to sleep
        if (_introTimer < 3f) return;

        // Phase 2 (3-5s): camera pan to seeker
        // TODO: tween camera to CameraTarget position

        // Phase 3 (5-5.5s): first shake
        if (_introTimer >= 5f && _introTimer < 5.5f)
        {
            // TODO: shake entity
            EmitBreakOutParticles();
        }

        // Phase 4 (6.5s): second shake
        if (_introTimer >= 6.5f && _introTimer < 7f)
        {
            EmitBreakOutParticles();
        }

        // Phase 5 (7.5s): break free
        if (_introTimer >= 7.5f && !_enabled)
        {
            _enabled = true;
            _state = State.Idle;
            // TODO: play "hatch" animation
            // TODO: play sound: event:/game/05_mirror_temple/seeker_statue_break
        }
    }

    // -------------------------------------------------------------------------
    // Player-controlled movement
    // -------------------------------------------------------------------------

    private void UpdateMovement(float dt)
    {
        // TODO: read Input.Aim.Value from actual input system
        // Using placeholder zero-vector; replace with real input
        Vector2 aim = Vector2.Zero;
        // e.g. aim = Input.Aim.Value.SafeNormalize();

        _speed += aim * Acceleration * dt;

        float speed = _speed.Length();
        if (speed > MaxSpeed)
            _speed = Vector2.Normalize(_speed) * Approach(speed, MaxSpeed, 700f * dt);

        if (aim.Y == 0f)
            _speed.Y = Approach(_speed.Y, 0f, 400f * dt);
        if (aim.X == 0f)
            _speed.X = Approach(_speed.X, 0f, 400f * dt);

        if (aim.Length() > 0f && _state == State.Idle)
        {
            // TODO: displacement burst at Entity.Position
            _state = State.Moving;
            // TODO: play "spotted" animation
            // TODO: play sound: event:/game/05_mirror_temple/seeker_playercontrolstart
        }

        // TODO: check if facing needs to flip → play "flipMouth" or "flipEyes"

        // TODO: check Dash button pressed → call Dash(aim.EightWayNormal())

        // Apply movement
        Entity.Position += _speed * dt;

        // TODO: clamp Entity.Position to scene bounds
    }

    private void Dash(Vector2 direction)
    {
        if (direction == Vector2.Zero) return;
        _dashDirection = direction;
        _dashTimer = DashDuration;
        _speed = direction * DashSpeed;
        _state = State.Dashing;
        _trailTimerA = DashDuration * 0.25f;
        _trailTimerB = DashDuration * 0.5f;
        _scale = new Vector2(1f + Math.Abs(direction.Y) * 0.4f, 1f + Math.Abs(direction.X) * 0.4f);
        // TODO: play "attack" animation
        // TODO: play sound: event:/game/05_mirror_temple/seeker_dash
    }

    private void UpdateDash(float dt)
    {
        _speed = Approach(_speed, Vector2.Zero, 800f * dt);
        _dashTimer -= dt;

        if (_trailTimerA > 0f) { _trailTimerA -= dt; if (_trailTimerA <= 0f) CreateTrail(); }
        if (_trailTimerB > 0f) { _trailTimerB -= dt; if (_trailTimerB <= 0f) CreateTrail(); }

        // TODO: emit: Seeker.P_Attack particles along _speed direction on interval

        if (_dashTimer <= 0f)
        {
            _state = State.Idle;
            // TODO: play "spotted" animation
        }

        Entity.Position += _speed * dt;
        // TODO: clamp to scene bounds
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void EmitBreakOutParticles()
    {
        // TODO: emit Seeker.P_BreakOut particles in a ring around entity
    }

    private void CreateTrail()
    {
        // TODO: TrailManager.Add snapshot of current sprite
    }

    private void CheckPlayerContact()
    {
        if (_player == null || !_enabled) return;
        float dist = Vector2.Distance(Entity.Position, _player.Position);
        if (dist < 10f)
        {
            // TODO: kill player, slow time, run End() to reload level
        }
    }

    /// <summary>Reloads the level after the player-seeker kills the player.</summary>
    private void End()
    {
        // TODO: reset Glitch, Distort, TimeRate to defaults
        // TODO: session.ColorGrade = null
        // TODO: reload level to room "c-00", CanRetry = true
    }

    private static Vector2 Approach(Vector2 v, Vector2 target, float maxDelta)
    {
        Vector2 diff = target - v;
        float dist = diff.Length();
        return dist <= maxDelta ? target : v + diff / dist * maxDelta;
    }

    private static float Approach(float v, float target, float maxDelta)
    {
        return v < target
            ? Math.Min(v + maxDelta, target)
            : Math.Max(v - maxDelta, target);
    }

    /// <summary>Computed world-space camera target for intro pan.</summary>
    private Vector2 CameraTarget => Entity.Position - new Vector2(160f, 90f);
}
