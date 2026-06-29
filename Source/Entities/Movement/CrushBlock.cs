using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using System.Collections.Generic;
using DZ.Core;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

// ── Axes enum ──────────────────────────────────────────────────────────────────

/// <summary>Axes along which a <see cref="CrushBlock"/> ("Kevin") can move.</summary>
public enum CrushBlockAxes
{
    /// <summary>Only moves left or right.</summary>
    Horizontal,
    /// <summary>Only moves up or down.</summary>
    Vertical,
    /// <summary>Moves along whichever axis the dash hit.</summary>
    Both,
}

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Internal state-machine states for <see cref="CrushBlock"/>.</summary>
public enum CrushBlockState
{
    /// <summary>Resting; waiting for a player dash.</summary>
    Idle,
    /// <summary>Brief wind-up before the crush lunge.</summary>
    Charging,
    /// <summary>Actively lunging at full crush speed.</summary>
    Crushing,
    /// <summary>Returning along the recorded waypoint path.</summary>
    Returning,
}

/// <summary>
/// Port of Celeste's CrushBlock.cs ("Kevin") to Nez/MonoGame.
///
/// A solid block that launches itself in the direction of a player dash at
/// <see cref="CrushSpeed"/> pixels per second.  When it strikes a wall it
/// records waypoints and then slowly returns to its spawn position at
/// <see cref="ReturnSpeed"/> pixels per second, retracing its path in reverse.
///
/// The <see cref="Axes"/> property restricts which axes the block can move on.
/// When <see cref="ChillOut"/> is <c>true</c> the block does not return after
/// hitting a second wall.
/// </summary>
public class CrushBlock : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Speed during the active crush lunge (pixels/second).</summary>
    public const float CrushSpeed  = 240f;

    /// <summary>Speed while returning along the waypoint path (pixels/second).</summary>
    public const float ReturnSpeed = 60f;

    /// <summary>Duration of the charging wind-up before the lunge (seconds).</summary>
    public const float ChargeTime  = 0.08f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>Axes on which this block is allowed to move.</summary>
    public CrushBlockAxes Axes { get; }

    /// <summary>
    /// When <c>true</c> the block does not return after striking a second wall
    /// ("chills out" in place).
    /// </summary>
    public bool ChillOut { get; }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Current state-machine state.</summary>
    public CrushBlockState State { get; private set; } = CrushBlockState.Idle;

    /// <summary>Unit vector along which the block is currently lunging.</summary>
    private Vector2 _crushDirection;

    /// <summary>General-purpose countdown timer.</summary>
    private float _stateTimer;

    /// <summary>Number of walls struck during the current lunge (for ChillOut logic).</summary>
    private int _wallHits;

    /// <summary>Waypoints recorded during the crush lunge, used to retrace the path.</summary>
    private readonly Stack<Vector2> _waypoints = new();

    /// <summary>The world-space position to which the block is currently returning.</summary>
    private Vector2 _returnTarget;

    /// <summary>Spawn position; the final waypoint to return to.</summary>
    private readonly Vector2 _startPosition;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="CrushBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="axes">Axes on which the block may move.</param>
    /// <param name="chillOut">If <c>true</c>, the block stays put after a second wall hit.</param>
    public CrushBlock(
        Vector2 position,
        float width,
        float height,
        CrushBlockAxes axes     = CrushBlockAxes.Both,
        bool           chillOut = false)
        : base(position, width, height)
    {
        Axes           = axes;
        ChillOut       = chillOut;
        _startPosition = position;
        Name           = "CrushBlock";

        // Register dash-collision callback.
        OnDashCollide = HandleDashCollide;

        // TODO: load sprite
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        State = CrushBlockState.Idle;
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the state machine every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case CrushBlockState.Idle:      UpdateIdle();      break;
            case CrushBlockState.Charging:  UpdateCharging();  break;
            case CrushBlockState.Crushing:  UpdateCrushing();  break;
            case CrushBlockState.Returning: UpdateReturning(); break;
        }
    }

    // ── State: Idle ───────────────────────────────────────────────────────────

    private void UpdateIdle()
    {
        // Nothing to do — the block waits for OnDashCollide.
    }

    // ── Dash-collision handler ────────────────────────────────────────────────

    /// <summary>
    /// Called by the player system when a dash hits this block.
    /// Records the crush direction (filtered by <see cref="Axes"/>) and starts
    /// the charging wind-up.
    /// </summary>
    private void HandleDashCollide(Vector2 dashDirection)
    {
        if (State != CrushBlockState.Idle) return;

        Vector2 dir = FilterDirection(dashDirection);
        if (dir == Vector2.Zero) return;

        _crushDirection = dir;
        _wallHits       = 0;
        _waypoints.Clear();
        _waypoints.Push(_startPosition);

        EnterCharging();
    }

    /// <summary>
    /// Filters <paramref name="rawDir"/> to the allowed axes, returning the dominant
    /// cardinal direction or <see cref="Vector2.Zero"/> if neither axis is allowed.
    /// </summary>
    private Vector2 FilterDirection(Vector2 rawDir)
    {
        float ax = Math.Abs(rawDir.X);
        float ay = Math.Abs(rawDir.Y);

        switch (Axes)
        {
            case CrushBlockAxes.Horizontal:
                if (ax < 0.1f) return Vector2.Zero;
                return new Vector2(Math.Sign(rawDir.X), 0);

            case CrushBlockAxes.Vertical:
                if (ay < 0.1f) return Vector2.Zero;
                return new Vector2(0, Math.Sign(rawDir.Y));

            default: // Both — take dominant axis.
                if (ax >= ay)
                    return ax < 0.1f ? Vector2.Zero : new Vector2(Math.Sign(rawDir.X), 0);
                else
                    return new Vector2(0, Math.Sign(rawDir.Y));
        }
    }

    // ── State: Charging ───────────────────────────────────────────────────────

    private void EnterCharging()
    {
        State       = CrushBlockState.Charging;
        _stateTimer = ChargeTime;
        StartShaking(ChargeTime);
        // TODO: play sound (charge wind-up)
    }

    private void UpdateCharging()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer <= 0f)
            EnterCrushing();
    }

    // ── State: Crushing ───────────────────────────────────────────────────────

    private void EnterCrushing()
    {
        State = CrushBlockState.Crushing;
        StopShaking();
        // TODO: play sound (crush lunge)
        // TODO: emit particles (speed lines)
    }

    private void UpdateCrushing()
    {
        float dt    = Time.DeltaTime;
        float moveX = _crushDirection.X * CrushSpeed * dt;
        float moveY = _crushDirection.Y * CrushSpeed * dt;

        bool hitWall = false;

        if (moveX != 0f)
            hitWall |= MoveHCollideSolids(moveX, OnHitWall);
        if (moveY != 0f)
            hitWall |= MoveVCollideSolids(moveY, OnHitWall);

        SetLiftSpeed(_crushDirection * CrushSpeed);

        if (hitWall)
        {
            _wallHits++;
            // Record current position as a waypoint before deciding next action.
            _waypoints.Push(Position);

            if (ChillOut && _wallHits >= 2)
            {
                // Stay put.
                State = CrushBlockState.Idle;
                // TODO: play sound (settle)
            }
            else
            {
                EnterReturning();
            }
        }
    }

    private void OnHitWall(CelesteSolid _)
    {
        // TODO: play sound (wall thud)
        // TODO: emit particles (impact)
        StartShaking(0.2f);
    }

    // ── State: Returning ─────────────────────────────────────────────────────

    private void EnterReturning()
    {
        State = CrushBlockState.Returning;
        AdvanceWaypoint(); // Pop the next waypoint immediately.
        // TODO: play sound (returning hum)
    }

    private void UpdateReturning()
    {
        float dt      = Time.DeltaTime;
        float speed   = ReturnSpeed * dt;
        Vector2 delta = _returnTarget - Position;
        float   dist  = delta.Length();

        if (dist <= speed)
        {
            // Snap to waypoint.
            MoveTo(_returnTarget);

            if (_waypoints.Count > 0)
            {
                AdvanceWaypoint();
            }
            else
            {
                // Reached start — return to idle.
                State = CrushBlockState.Idle;
                // TODO: play sound (settle)
            }
        }
        else
        {
            // Step toward waypoint.
            Vector2 step = Vector2.Normalize(delta) * speed;
            MoveH(step.X);
            MoveV(step.Y);
            SetLiftSpeed(Vector2.Normalize(delta) * ReturnSpeed);
        }
    }

    private void AdvanceWaypoint()
    {
        _returnTarget = _waypoints.Count > 0 ? _waypoints.Pop() : _startPosition;
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
