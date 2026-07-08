using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using DZ.Core;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Internal state-machine states for <see cref="Cloud"/>.</summary>
public enum CloudState
{
    /// <summary>Drifting passively or waiting for the player.</summary>
    Idle,
    /// <summary>Player is riding the cloud; it moves in its drift direction.</summary>
    Moving,
    /// <summary>
    /// <see cref="Cloud.Fragile"/> variant: the cloud is shaking and about to
    /// crumble after the player has stood on it too long.
    /// </summary>
    Crumbling,
    /// <summary>Cloud has crumbled and is respawning off-screen.</summary>
    Gone,
}

/// <summary>
/// Port of Celeste's Cloud.cs to Nez/MonoGame.
///
/// A one-tile-tall platform that drifts horizontally (or vertically).
/// <list type="bullet">
///   <item>
///     When the player lands on it the cloud begins moving in its configured
///     <see cref="DriftDirection"/> at <see cref="MoveSpeed"/> pixels per second.
///   </item>
///   <item>
///     When the player steps off, the cloud returns to <see cref="CloudState.Idle"/>
///     and decelerates to a stop.
///   </item>
///   <item>
///     When <see cref="Fragile"/> is <c>true</c>, the cloud begins crumbling after
///     <see cref="FragileCrumbleTime"/> seconds of player contact.  It disappears
///     then reappears at its start position after <see cref="RespawnTime"/> seconds.
///   </item>
/// </list>
/// </summary>
public class Cloud : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Width of the cloud platform in pixels.</summary>
    public const float CloudWidth        = 32f;

    /// <summary>Height of the cloud platform in pixels.</summary>
    public const float CloudHeight       = 8f;

    /// <summary>Top speed while carrying the player (pixels/second).</summary>
    public const float MoveSpeed         = 80f;

    /// <summary>Deceleration rate when the player leaves (pixels/second²).</summary>
    public const float Decel             = 200f;

    /// <summary>
    /// Seconds of continuous player contact before a <see cref="Fragile"/> cloud
    /// starts crumbling.
    /// </summary>
    public const float FragileCrumbleTime = 0.5f;

    /// <summary>Seconds the crumble animation plays before the cloud vanishes.</summary>
    public const float CrumbleAnimTime   = 0.3f;

    /// <summary>Seconds the cloud is absent before it respawns.</summary>
    public const float RespawnTime       = 2.5f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>
    /// When <c>true</c> the cloud crumbles after the player stands on it for
    /// <see cref="FragileCrumbleTime"/> seconds.
    /// </summary>
    public bool Fragile { get; }

    /// <summary>
    /// Unit vector giving the drift direction.
    /// Defaults to <see cref="Vector2.UnitX"/> (rightward).
    /// </summary>
    public Vector2 DriftDirection { get; set; } = Vector2.UnitX;

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Current state-machine state.</summary>
    public CloudState State { get; private set; } = CloudState.Idle;

    /// <summary>Current travel speed along the drift axis (pixels/second).</summary>
    private float _speed;

    /// <summary>How long the player has been continuously standing on the cloud (seconds).</summary>
    private float _rideTimer;

    /// <summary>General-purpose countdown timer.</summary>
    private float _stateTimer;

    /// <summary>World-space spawn position; cloud returns here after respawning.</summary>
    private readonly Vector2 _startPosition;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Cloud"/>.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="fragile">If <c>true</c>, the cloud crumbles after brief contact.</param>
    public Cloud(Vector2 position, bool fragile = false)
        : base(position, CloudWidth, CloudHeight)
    {
        Fragile        = fragile;
        _startPosition = position;
        Name           = "Cloud";
        // TODO: load sprite (cloud / fragile cloud texture)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        RestoreCloud();
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the state machine every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case CloudState.Idle:      UpdateIdle();      break;
            case CloudState.Moving:    UpdateMoving();    break;
            case CloudState.Crumbling: UpdateCrumbling(); break;
            case CloudState.Gone:      UpdateGone();      break;
        }
    }

    // ── State: Idle ───────────────────────────────────────────────────────────

    private void UpdateIdle()
    {
        var player = GetPlayer();
        bool riding = player != null && IsPlayerRiding(player);

        if (riding)
        {
            EnterMoving();
            return;
        }

        // Decelerate to rest.
        if (_speed != 0f)
        {
            float dt = Time.DeltaTime;
            _speed = Approach(_speed, 0f, Decel * dt);
            ApplyDriftMovement(dt);
        }
    }

    // ── State: Moving ─────────────────────────────────────────────────────────

    private void EnterMoving()
    {
        State      = CloudState.Moving;
        _rideTimer = 0f;
        // TODO: play sound (cloud whoosh — soft)
    }

    private void UpdateMoving()
    {
        float dt     = Time.DeltaTime;
        var   player = GetPlayer();
        bool  riding = player != null && IsPlayerRiding(player);

        if (!riding)
        {
            // Player left — idle out.
            State      = CloudState.Idle;
            _rideTimer = 0f;
            return;
        }

        // Accelerate toward top speed.
        _speed = Approach(_speed, MoveSpeed, Decel * dt);
        ApplyDriftMovement(dt);
        SetLiftSpeed(DriftDirection * _speed);

        // Fragile crumble check.
        if (Fragile)
        {
            _rideTimer += dt;
            if (_rideTimer >= FragileCrumbleTime)
                EnterCrumbling();
        }
    }

    // ── State: Crumbling ─────────────────────────────────────────────────────

    private void EnterCrumbling()
    {
        State       = CloudState.Crumbling;
        _stateTimer = CrumbleAnimTime;
        StartShaking(CrumbleAnimTime);
        // TODO: play sound (crumble rumble)
    }

    private void UpdateCrumbling()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer > 0f) return;

        // Vanish.
        Collidable  = false;
        State       = CloudState.Gone;
        _stateTimer = RespawnTime;
        StopShaking();
        // TODO: emit particles (poof)
        // TODO: play sound (crumble vanish)
    }

    // ── State: Gone ───────────────────────────────────────────────────────────

    private void UpdateGone()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer <= 0f)
            RestoreCloud();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Applies drift movement for one frame at the current speed.</summary>
    private void ApplyDriftMovement(float dt)
    {
        float moveX = DriftDirection.X * _speed * dt;
        float moveY = DriftDirection.Y * _speed * dt;
        MoveH(moveX);
        MoveV(moveY);
    }

    /// <summary>Resets the cloud to its initial visible, collidable state.</summary>
    private void RestoreCloud()
    {
        State      = CloudState.Idle;
        Collidable = true;
        _speed     = 0f;
        _rideTimer = 0f;
        Position   = _startPosition;
        UpdateBounds();
        StopShaking();
        // TODO: emit particles (respawn poof)
        // TODO: play sound (respawn)
        // TODO: load sprite (idle frame)
    }

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
