using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Movement;

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Internal state-machine states for <see cref="ZipMover"/>.</summary>
public enum ZipMoverState
{
    /// <summary>Resting at the start position; waiting for the player.</summary>
    Idle,
    /// <summary>Moving forward to the <see cref="ZipMover.Target"/> position.</summary>
    Moving,
    /// <summary>Paused at the target position.</summary>
    Waiting,
    /// <summary>Returning slowly to the start position.</summary>
    Returning,
}

/// <summary>
/// Port of Celeste's ZipMover.cs to Nez/MonoGame.
///
/// A solid platform that rockets forward to a fixed <see cref="Target"/> position
/// when the player stands or grabs on it, waits briefly, then slowly returns.
///
/// Movement is lerp-based over configurable durations:
/// <list type="bullet">
///   <item><see cref="ForwardDuration"/> — forward travel time (default 0.5–1 s).</item>
///   <item><see cref="WaitDuration"/>   — pause at target (default 0.5 s).</item>
///   <item><see cref="ReturnDuration"/> — return travel time (default 1–2 s).</item>
/// </list>
/// </summary>
public class ZipMover : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Time to travel from start to <see cref="Target"/> (seconds).</summary>
    public float ForwardDuration { get; set; } = 0.75f;

    /// <summary>Time to wait at <see cref="Target"/> before returning (seconds).</summary>
    public float WaitDuration    { get; set; } = 0.5f;

    /// <summary>Time to travel from <see cref="Target"/> back to start (seconds).</summary>
    public float ReturnDuration  { get; set; } = 1.5f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>World-space destination position (top-left corner).</summary>
    public Vector2 Target { get; }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Current state-machine state.</summary>
    public ZipMoverState State { get; private set; } = ZipMoverState.Idle;

    /// <summary>Elapsed time within the current state (seconds).</summary>
    private float _stateTimer;

    /// <summary>World-space start position saved at spawn.</summary>
    private readonly Vector2 _startPosition;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="ZipMover"/>.
    /// </summary>
    /// <param name="position">Top-left world position at rest.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="target">Destination top-left world position.</param>
    public ZipMover(Vector2 position, int width, int height, Vector2 target)
        : base(position, width, height)
    {
        Target         = target;
        _startPosition = position;
        Name           = "ZipMover";
        // TODO: load sprite (includes rope/chain decoration)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        State       = ZipMoverState.Idle;
        _stateTimer = 0f;
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the state machine every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case ZipMoverState.Idle:      UpdateIdle();      break;
            case ZipMoverState.Moving:    UpdateMoving();    break;
            case ZipMoverState.Waiting:   UpdateWaiting();   break;
            case ZipMoverState.Returning: UpdateReturning(); break;
        }
    }

    // ── State: Idle ───────────────────────────────────────────────────────────

    private void UpdateIdle()
    {
        var player = GetPlayer();
        if (player == null) return;

        if (IsPlayerRiding(player) || IsPlayerOverlapping(player))
            EnterMoving();
    }

    // ── State: Moving ─────────────────────────────────────────────────────────

    private void EnterMoving()
    {
        State       = ZipMoverState.Moving;
        _stateTimer = 0f;
        // TODO: play sound (zip sound — looping)
    }

    private void UpdateMoving()
    {
        float dt = Time.DeltaTime;
        _stateTimer += dt;

        float t = Math.Clamp(_stateTimer / ForwardDuration, 0f, 1f);

        // Ease-in: use a smoothstep for a snappy launch.
        float eased = EaseInOut(t);

        Vector2 newPos = Vector2.Lerp(_startPosition, Target, eased);
        ApplyMoveTo(newPos);

        // Impart lift speed proportional to velocity.
        if (_stateTimer > 0f)
        {
            float prevT    = Math.Clamp((_stateTimer - dt) / ForwardDuration, 0f, 1f);
            Vector2 prevPos = Vector2.Lerp(_startPosition, Target, EaseInOut(prevT));
            SetLiftSpeed((newPos - prevPos) / dt);
        }

        if (t >= 1f)
            EnterWaiting();
    }

    // ── State: Waiting ────────────────────────────────────────────────────────

    private void EnterWaiting()
    {
        State       = ZipMoverState.Waiting;
        _stateTimer = WaitDuration;
        SetLiftSpeed(Vector2.Zero);
        StartShaking(0.1f);
        // TODO: play sound (thud at target)
    }

    private void UpdateWaiting()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer <= 0f)
            EnterReturning();
    }

    // ── State: Returning ─────────────────────────────────────────────────────

    private void EnterReturning()
    {
        State       = ZipMoverState.Returning;
        _stateTimer = 0f;
        // TODO: play sound (slow return creak)
    }

    private void UpdateReturning()
    {
        float dt = Time.DeltaTime;
        _stateTimer += dt;

        float t = Math.Clamp(_stateTimer / ReturnDuration, 0f, 1f);

        // Ease-out: gentle deceleration back to start.
        float eased = EaseOut(t);

        Vector2 newPos = Vector2.Lerp(Target, _startPosition, eased);
        ApplyMoveTo(newPos);

        if (_stateTimer > 0f)
        {
            float prevT    = Math.Clamp((_stateTimer - dt) / ReturnDuration, 0f, 1f);
            Vector2 prevPos = Vector2.Lerp(Target, _startPosition, EaseOut(prevT));
            SetLiftSpeed((newPos - prevPos) / dt);
        }

        if (t >= 1f)
        {
            State = ZipMoverState.Idle;
            SetLiftSpeed(Vector2.Zero);
            // TODO: play sound (return thud / settle)
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Moves this platform so its top-left corner is at <paramref name="targetPos"/>
    /// using sub-pixel-accurate <see cref="CelestePlatform.MoveToX"/> /
    /// <see cref="CelestePlatform.MoveToY"/>.
    /// </summary>
    private void ApplyMoveTo(Vector2 targetPos)
    {
        MoveToX(targetPos.X);
        MoveToY(targetPos.Y);
    }

    /// <summary>Smooth-step ease-in-out for [0,1].</summary>
    private static float EaseInOut(float t) => t * t * (3f - 2f * t);

    /// <summary>Quadratic ease-out for [0,1].</summary>
    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}
