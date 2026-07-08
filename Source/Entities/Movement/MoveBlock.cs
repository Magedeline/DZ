using Microsoft.Xna.Framework;
using DZ.Nez;
using Input = DZ.Nez.Input;
using System;
using DZ.Core;
using DZ.Entities.Core;


namespace DZ.Entities.Movement;

// ── Direction enum ─────────────────────────────────────────────────────────────

/// <summary>The axis-aligned direction a <see cref="MoveBlock"/> travels.</summary>
public enum MoveBlockDirection
{
    Left,
    Right,
    Up,
    Down,
}

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Internal state-machine states for <see cref="MoveBlock"/>.</summary>
public enum MoveBlockState
{
    /// <summary>Waiting for the player to stand on the block.</summary>
    Idling,
    /// <summary>Moving in the configured direction.</summary>
    Moving,
    /// <summary>Crashed; playing crash animation then waiting to respawn.</summary>
    Breaking,
}

/// <summary>
/// Port of Celeste's MoveBlock.cs to Nez/MonoGame.
///
/// A solid block that starts moving when the player stands on it.
/// It accelerates toward <see cref="MoveSpeed"/> (or <see cref="FastMoveSpeed"/>
/// when <see cref="Fast"/> is set) in its configured <see cref="Direction"/>.
///
/// After striking a wall the block enters <see cref="MoveBlockState.Breaking"/>
/// for <see cref="CrashTime"/> seconds, disappears, then reappears at its
/// original spawn position after <see cref="RespawnTime"/> seconds.
///
/// When <see cref="CanSteer"/> is <c>true</c> the player may redirect the block
/// along its secondary axis while it is moving.
/// </summary>
public class MoveBlock : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Normal top speed (pixels/second).</summary>
    public const float MoveSpeed     = 60f;

    /// <summary>Top speed when <see cref="Fast"/> is enabled (pixels/second).</summary>
    public const float FastMoveSpeed = 75f;

    /// <summary>Acceleration rate (pixels/second²).</summary>
    private const float Accel        = 300f;

    /// <summary>Steer acceleration along the secondary axis (pixels/second²).</summary>
    private const float SteerAccel   = 200f;

    /// <summary>Seconds the block is held against a wall before breaking.</summary>
    public const float CrashTime     = 0.15f;

    /// <summary>Seconds the block is absent before respawning.</summary>
    public const float RespawnTime   = 3f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>Primary travel direction of this block.</summary>
    public MoveBlockDirection Direction { get; }

    /// <summary>
    /// When <c>true</c> the player can redirect the block along its secondary
    /// axis by holding the matching directional input.
    /// </summary>
    public bool CanSteer { get; }

    /// <summary>When <c>true</c> the block uses <see cref="FastMoveSpeed"/>.</summary>
    public bool Fast { get; }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Current state-machine state.</summary>
    public MoveBlockState State { get; private set; } = MoveBlockState.Idling;

    /// <summary>Current travel speed along the primary axis (pixels/second).</summary>
    private float _speed;

    /// <summary>Current travel speed along the secondary (steer) axis (pixels/second).</summary>
    private float _steerSpeed;

    /// <summary>General-purpose countdown timer used by Breaking and respawn phases.</summary>
    private float _stateTimer;

    /// <summary>
    /// <c>true</c> while the block is invisible and counting down to respawn.
    /// Used to distinguish the two sub-phases of <see cref="MoveBlockState.Breaking"/>.
    /// </summary>
    private bool _respawning;

    /// <summary>World-space position at spawn; the block returns here after breaking.</summary>
    private readonly Vector2 _startPosition;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="MoveBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="direction">Direction the block will travel.</param>
    /// <param name="canSteer">If <c>true</c>, the player may redirect the block.</param>
    /// <param name="fast">If <c>true</c>, uses <see cref="FastMoveSpeed"/>.</param>
    public MoveBlock(
        Vector2 position,
        int width,
        int height,
        MoveBlockDirection direction,
        bool canSteer = true,
        bool fast     = false)
        : base(position, width, height)
    {
        Direction      = direction;
        CanSteer       = canSteer;
        Fast           = fast;
        _startPosition = position;
        Name           = "MoveBlock";
        // TODO: load sprite
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        EnterIdling();
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the state machine every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case MoveBlockState.Idling:
                UpdateIdling();
                break;

            case MoveBlockState.Moving:
                UpdateMoving();
                break;

            case MoveBlockState.Breaking:
                if (_respawning)
                    UpdateRespawnWait();
                else
                    UpdateCrashing();
                break;
        }
    }

    // ── State: Idling ─────────────────────────────────────────────────────────

    private void EnterIdling()
    {
        State       = MoveBlockState.Idling;
        _speed      = 0f;
        _steerSpeed = 0f;
        _respawning = false;
        Collidable  = true;
        // TODO: load sprite (idle frame / reset animation)
    }

    private void UpdateIdling()
    {
        var player = GetPlayer();
        if (player != null && IsPlayerRiding(player))
            EnterMoving();
    }

    // ── State: Moving ─────────────────────────────────────────────────────────

    private void EnterMoving()
    {
        State = MoveBlockState.Moving;
        // TODO: play sound (engine start)
    }

    private void UpdateMoving()
    {
        float dt       = Time.DeltaTime;
        float topSpeed = Fast ? FastMoveSpeed : MoveSpeed;

        // Accelerate along primary axis.
        _speed = Approach(_speed, topSpeed, Accel * dt);

        // Optional secondary-axis steering.
        float steerTarget = CanSteer ? GetSteerInput() * topSpeed : 0f;
        _steerSpeed = Approach(_steerSpeed, steerTarget, SteerAccel * dt);

        // Resolve displacement.
        Vector2 primary   = DirectionToVector(Direction) * (_speed      * dt);
        Vector2 secondary = SteerVector(Direction)       * (_steerSpeed * dt);
        float   totalX    = primary.X + secondary.X;
        float   totalY    = primary.Y + secondary.Y;

        bool hitWall = false;
        hitWall |= MoveHCollideSolids(totalX, _ => { });
        hitWall |= MoveVCollideSolids(totalY, _ => { });

        SetLiftSpeed(DirectionToVector(Direction) * _speed + SteerVector(Direction) * _steerSpeed);

        if (hitWall)
            EnterCrashing();
    }

    // ── State: Breaking (phase 1 — crash hold) ────────────────────────────────

    private void EnterCrashing()
    {
        State       = MoveBlockState.Breaking;
        _respawning = false;
        _stateTimer = CrashTime;
        _speed      = 0f;
        _steerSpeed = 0f;
        StartShaking(CrashTime);
        // TODO: play sound (crash/thud)
    }

    private void UpdateCrashing()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer > 0f) return;

        // Transition to respawn wait.
        Collidable  = false;
        _respawning = true;
        _stateTimer = RespawnTime;
        StopShaking();
        // TODO: emit particles (shatter)
        // TODO: play sound (break)
    }

    // ── State: Breaking (phase 2 — respawn wait) ──────────────────────────────

    private void UpdateRespawnWait()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer > 0f) return;

        // Return to start position and re-enter idle.
        Position = _startPosition;
        UpdateBounds();
        EnterIdling();
        // TODO: emit particles (respawn flash)
        // TODO: play sound (respawn)
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Maps a <see cref="MoveBlockDirection"/> to its unit vector.</summary>
    private static Vector2 DirectionToVector(MoveBlockDirection dir) => dir switch
    {
        MoveBlockDirection.Left  => -Vector2.UnitX,
        MoveBlockDirection.Right =>  Vector2.UnitX,
        MoveBlockDirection.Up    => -Vector2.UnitY,
        MoveBlockDirection.Down  =>  Vector2.UnitY,
        _                        =>  Vector2.Zero,
    };

    /// <summary>
    /// Returns the perpendicular (steer) unit vector for a given primary direction.
    /// Horizontal blocks steer vertically; vertical blocks steer horizontally.
    /// </summary>
    private static Vector2 SteerVector(MoveBlockDirection dir) => dir switch
    {
        MoveBlockDirection.Left  or MoveBlockDirection.Right => Vector2.UnitY,
        MoveBlockDirection.Up    or MoveBlockDirection.Down  => Vector2.UnitX,
        _                                                    => Vector2.Zero,
    };

    /// <summary>
    /// Reads directional input and returns a steer value in [−1, 1] along the
    /// secondary axis of this block's primary direction.
    /// </summary>
    private static float GetSteerInput()
    {
        // Use arrow keys or WASD.
        float v = 0f;
        if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up)    ||
            Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            v -= 1f;
        if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down)  ||
            Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            v += 1f;
        if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left)  ||
            Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            v -= 1f;
        if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) ||
            Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            v += 1f;
        return Math.Clamp(v, -1f, 1f);
    }

    /// <summary>
    /// Advances <paramref name="current"/> toward <paramref name="target"/> by at most
    /// <paramref name="maxDelta"/>, never overshooting.
    /// (Equivalent to Celeste's <c>Calc.Approach</c>.)
    /// </summary>
    private static float Approach(float current, float target, float maxDelta)
    {
        float diff = target - current;
        return current + Math.Sign(diff) * Math.Min(maxDelta, Math.Abs(diff));
    }
}
