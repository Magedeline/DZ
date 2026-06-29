using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using DZ.Core;
using DZ.Entities.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Movement;

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Internal state-machine states for <see cref="BounceBlock"/>.</summary>
public enum BounceBlockState
{
    /// <summary>Resting; waiting for a player dash.</summary>
    Idle,
    /// <summary>Wind-up before launching the player.</summary>
    WindingUp,
    /// <summary>Launching the player and playing the release animation.</summary>
    Bouncing,
    /// <summary>Cooldown / recovery after a bounce before resetting to idle.</summary>
    Recovering,
}

/// <summary>
/// Port of Celeste's BounceBlock.cs to Nez/MonoGame.
///
/// A solid block that launches the player in the opposite direction of their dash
/// when they dash into it.  The block winds up briefly (<see cref="WindupTime"/>)
/// before the bounce so the player can react, then fires the player at
/// <see cref="LaunchSpeed"/> pixels per second.
///
/// After each bounce the block enters a short <see cref="RecoveryTime"/>-second
/// cooldown before it can be triggered again.
/// </summary>
public class BounceBlock : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Seconds of wind-up before the bounce fires.</summary>
    public const float WindupTime    = 0.15f;

    /// <summary>Seconds the bounce-release animation plays.</summary>
    public const float BounceTime    = 0.1f;

    /// <summary>Seconds before the block can be triggered again.</summary>
    public const float RecoveryTime  = 0.4f;

    /// <summary>Speed at which the player is launched (pixels/second).</summary>
    public const float LaunchSpeed   = 260f;

    /// <summary>Minimum speed imparted in the reflected direction (pixels/second).</summary>
    public const float MinLaunchSpeed = 200f;

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Current state-machine state.</summary>
    public BounceBlockState State { get; private set; } = BounceBlockState.Idle;

    /// <summary>Countdown timer for the current state.</summary>
    private float _stateTimer;

    /// <summary>The unit-vector direction in which the player will be launched.</summary>
    private Vector2 _launchDirection;

    /// <summary>Cached reference to the player, held across wind-up so we can launch them.</summary>
    private PlayerController? _targetPlayer;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="BounceBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public BounceBlock(Vector2 position, float width, float height)
        : base(position, width, height)
    {
        Name = "BounceBlock";

        // Wire up the dash-collision callback.
        OnDashCollide = HandleDashCollide;

        // TODO: load sprite (bouncy block texture)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        State = BounceBlockState.Idle;
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the state machine every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case BounceBlockState.Idle:       UpdateIdle();       break;
            case BounceBlockState.WindingUp:  UpdateWindingUp();  break;
            case BounceBlockState.Bouncing:   UpdateBouncing();   break;
            case BounceBlockState.Recovering: UpdateRecovering(); break;
        }
    }

    // ── State: Idle ───────────────────────────────────────────────────────────

    private void UpdateIdle()
    {
        // Waiting for HandleDashCollide to fire.
    }

    // ── Dash-collision handler ────────────────────────────────────────────────

    /// <summary>
    /// Called when the player dashes into this block.
    /// Records the reflected launch direction and starts the wind-up.
    /// </summary>
    private void HandleDashCollide(Vector2 dashDirection)
    {
        if (State != BounceBlockState.Idle) return;

        // Reflect the dash direction to get the launch direction.
        _launchDirection = ReflectDirection(dashDirection);
        _targetPlayer    = GetPlayer();

        EnterWindup();
    }

    // ── State: WindingUp ─────────────────────────────────────────────────────

    private void EnterWindup()
    {
        State       = BounceBlockState.WindingUp;
        _stateTimer = WindupTime;
        StartShaking(WindupTime);
        // TODO: play sound (wind-up charge)
        // TODO: emit particles (charge anticipation)
    }

    private void UpdateWindingUp()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer <= 0f)
            EnterBouncing();
    }

    // ── State: Bouncing ───────────────────────────────────────────────────────

    private void EnterBouncing()
    {
        State       = BounceBlockState.Bouncing;
        _stateTimer = BounceTime;
        StopShaking();
        LaunchPlayer();
        // TODO: play sound (boing / spring release)
        // TODO: emit particles (starburst in launch direction)
    }

    private void UpdateBouncing()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer <= 0f)
            EnterRecovering();
    }

    // ── State: Recovering ─────────────────────────────────────────────────────

    private void EnterRecovering()
    {
        State        = BounceBlockState.Recovering;
        _stateTimer  = RecoveryTime;
        _targetPlayer = null;
    }

    private void UpdateRecovering()
    {
        _stateTimer -= Time.DeltaTime;
        if (_stateTimer <= 0f)
            State = BounceBlockState.Idle;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Applies the bounce velocity to the cached <see cref="_targetPlayer"/>.
    /// The launch speed is at least <see cref="MinLaunchSpeed"/> and at most
    /// <see cref="LaunchSpeed"/>.
    /// </summary>
    private void LaunchPlayer()
    {
        if (_targetPlayer == null) return;

        float speed = Math.Clamp(LaunchSpeed, MinLaunchSpeed, LaunchSpeed);
        _targetPlayer.Velocity = _launchDirection * speed;
    }

    /// <summary>
    /// Reflects a dash-direction vector to produce the launch direction.
    /// The launch direction is the cardinal direction opposite to the predominant
    /// component of the incoming dash.
    /// </summary>
    private static Vector2 ReflectDirection(Vector2 dashDir)
    {
        float ax = Math.Abs(dashDir.X);
        float ay = Math.Abs(dashDir.Y);

        // Reflect the dominant axis.
        if (ax >= ay)
        {
            // Horizontal dash → launch horizontally in opposite direction.
            return new Vector2(-Math.Sign(dashDir.X), 0f);
        }
        else
        {
            // Vertical dash → launch vertically in opposite direction.
            return new Vector2(0f, -Math.Sign(dashDir.Y));
        }
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
