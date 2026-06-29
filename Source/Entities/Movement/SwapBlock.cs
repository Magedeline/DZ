using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using DZ.Core;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Internal state-machine states for <see cref="SwapBlock"/>.</summary>
public enum SwapBlockState
{
    /// <summary>At rest at whichever endpoint is current; waiting for a player dash.</summary>
    Idle,
    /// <summary>Moving toward the opposite endpoint.</summary>
    Moving,
    /// <summary>Reached the opposite endpoint; waiting before allowing a return dash.</summary>
    Pausing,
}

/// <summary>
/// Port of Celeste's SwapBlock.cs to Nez/MonoGame.
///
/// A solid block that sits at one of two endpoints.  A player dash touching
/// the block triggers it to move toward the other endpoint.  If no new dash
/// arrives during travel or within a short pause at the destination, the block
/// returns to its origin after <see cref="ReturnDelay"/> seconds.
///
/// Movement is lerp-based, always travelling at a fixed <see cref="MoveSpeed"/>
/// (pixels/second); the lerp parameter is advanced proportionally so faster
/// blocks have shorter travel times for shorter paths.
/// </summary>
public class SwapBlock : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Travel speed in pixels/second.</summary>
    public const float MoveSpeed    = 360f;

    /// <summary>Seconds to pause at destination before returning (if not re-dashed).</summary>
    public const float ReturnDelay  = 0.8f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>First (initial) endpoint — the block's spawn position.</summary>
    public Vector2 Start { get; }

    /// <summary>Second (node) endpoint.</summary>
    public Vector2 End { get; }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Current state-machine state.</summary>
    public SwapBlockState State { get; private set; } = SwapBlockState.Idle;

    /// <summary><c>true</c> while the block is actively travelling to an endpoint.</summary>
    public bool Swapping => State == SwapBlockState.Moving;

    /// <summary>
    /// Current lerp value in [0, 1] where 0 = <see cref="Start"/> and
    /// 1 = <see cref="End"/>.
    /// </summary>
    private float _lerp;

    /// <summary>Direction of lerp travel: +1 toward End, -1 toward Start.</summary>
    private int _direction = 1;

    /// <summary>General-purpose countdown timer (used by Pausing and return delay).</summary>
    private float _stateTimer;

    /// <summary>Total pixel distance between the two endpoints.</summary>
    private readonly float _totalDist;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="SwapBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position of the first endpoint (also spawn).</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="node">Top-left world position of the second endpoint.</param>
    public SwapBlock(Vector2 position, float width, float height, Vector2 node)
        : base(position, width, height)
    {
        Start      = position;
        End        = node;
        _totalDist = Vector2.Distance(Start, End);
        Name       = "SwapBlock";

        // Register dash-collision callback.
        OnDashCollide = HandleDashCollide;

        // TODO: load sprite (includes path decoration)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        _lerp       = 0f;
        _direction  = 1;
        State       = SwapBlockState.Idle;
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the state machine every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case SwapBlockState.Idle:    UpdateIdle();    break;
            case SwapBlockState.Moving:  UpdateMoving();  break;
            case SwapBlockState.Pausing: UpdatePausing(); break;
        }
    }

    // ── Dash collision ────────────────────────────────────────────────────────

    /// <summary>
    /// Called when the player dashes into this block.
    /// Starts or reverses travel toward the opposite endpoint.
    /// </summary>
    private void HandleDashCollide(Vector2 _dashDir)
    {
        if (State == SwapBlockState.Moving)
        {
            // Reverse mid-journey.
            _direction = -_direction;
        }
        else
        {
            // Start moving toward the other endpoint.
            _direction = (_lerp < 0.5f) ? 1 : -1;
            EnterMoving();
        }
    }

    // ── State: Idle ───────────────────────────────────────────────────────────

    private void UpdateIdle()
    {
        // Block waits passively; HandleDashCollide starts movement.
    }

    // ── State: Moving ─────────────────────────────────────────────────────────

    private void EnterMoving()
    {
        State = SwapBlockState.Moving;
        // TODO: play sound (swap whoosh — looping)
    }

    private void UpdateMoving()
    {
        float dt = Time.DeltaTime;

        // Advance lerp by the speed-per-pixel-distance ratio.
        float lerpStep = (_totalDist > 0f)
            ? MoveSpeed * dt / _totalDist
            : 1f;

        _lerp = Math.Clamp(_lerp + _direction * lerpStep, 0f, 1f);

        // Apply lerp position.
        Vector2 targetPos = Vector2.Lerp(Start, End, _lerp);

        // Compute displacement and impart lift speed.
        Vector2 prevPos = Position;
        MoveToX(targetPos.X);
        MoveToY(targetPos.Y);
        Vector2 actualDelta = Position - prevPos;
        if (dt > 0f)
            SetLiftSpeed(actualDelta / dt);

        // Check if we've reached an endpoint.
        bool atEnd   = _lerp >= 1f;
        bool atStart = _lerp <= 0f;

        if ((atEnd && _direction > 0) || (atStart && _direction < 0))
            EnterPausing();
    }

    // ── State: Pausing ────────────────────────────────────────────────────────

    private void EnterPausing()
    {
        State       = SwapBlockState.Pausing;
        _stateTimer = ReturnDelay;
        SetLiftSpeed(Vector2.Zero);
        StartShaking(0.1f);
        // TODO: play sound (arrival thud)
    }

    private void UpdatePausing()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer <= 0f)
        {
            // Automatically reverse back.
            _direction = -_direction;
            EnterMoving();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Advances <paramref name="current"/> toward <paramref name="target"/> by at most
    /// <paramref name="maxDelta"/>, never overshooting.
    /// </summary>
    private static float Approach(float current, float target, float maxDelta)
    {
        float diff = target - current;
        return current + Math.Sign(diff) * Math.Min(maxDelta, Math.Abs(diff));
    }
}
