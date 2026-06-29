using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using DZ.Core;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Internal state-machine states for <see cref="FallingBlock"/>.</summary>
public enum FallingBlockState
{
    /// <summary>At rest; waiting for the player to stand on it (or <see cref="FallingBlock.Triggered"/>).</summary>
    Waiting,
    /// <summary>Shaking as a warning before dropping.</summary>
    Shaking,
    /// <summary>Actively falling downward.</summary>
    Falling,
    /// <summary>Has hit a solid surface below and come to rest.</summary>
    Landed,
}

/// <summary>
/// Port of Celeste's FallingBlock.cs to Nez/MonoGame.
///
/// A solid block that shakes for <see cref="ShakeTime"/> seconds when the player
/// stands on it, then falls at increasing speed up to <see cref="MaxFallSpeed"/>
/// pixels per second.  When it hits a solid surface below it enters the
/// <see cref="FallingBlockState.Landed"/> state and sets <see cref="Safe"/> to
/// <c>true</c>, indicating it is now a permanent platform.
///
/// Setting <see cref="Triggered"/> to <c>true</c> from outside will start the
/// shake sequence regardless of player contact.
/// </summary>
public class FallingBlock : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>How long the block shakes before falling (seconds).</summary>
    public const float ShakeTime    = 0.2f;

    /// <summary>Gravity acceleration applied while falling (pixels/second²).</summary>
    public const float FallAccel    = 500f;

    /// <summary>Maximum downward speed while falling (pixels/second).</summary>
    public const float MaxFallSpeed = 160f;

    // ── Configuration / triggers ──────────────────────────────────────────────

    /// <summary>
    /// Set to <c>true</c> externally to start the shake sequence without
    /// requiring player contact.
    /// </summary>
    public bool Triggered { get; set; }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Current state-machine state.</summary>
    public FallingBlockState State { get; private set; } = FallingBlockState.Waiting;

    /// <summary>Current downward velocity (pixels/second).</summary>
    private float _fallSpeed;

    /// <summary>Countdown timer used by the <see cref="FallingBlockState.Shaking"/> phase.</summary>
    private float _shakeTimer;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="FallingBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public FallingBlock(Vector2 position, int width, int height)
        : base(position, width, height)
    {
        Name = "FallingBlock";
        // TODO: load sprite
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        State     = FallingBlockState.Waiting;
        _fallSpeed = 0f;
        Safe       = false;
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the state machine every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case FallingBlockState.Waiting:  UpdateWaiting();  break;
            case FallingBlockState.Shaking:  UpdateShaking();  break;
            case FallingBlockState.Falling:  UpdateFalling();  break;
            case FallingBlockState.Landed:   /* static */      break;
        }
    }

    // ── State: Waiting ────────────────────────────────────────────────────────

    private void UpdateWaiting()
    {
        if (Triggered)
        {
            EnterShaking();
            return;
        }

        var player = GetPlayer();
        if (player != null && IsPlayerRiding(player))
            EnterShaking();
    }

    // ── State: Shaking ────────────────────────────────────────────────────────

    private void EnterShaking()
    {
        State       = FallingBlockState.Shaking;
        _shakeTimer = ShakeTime;
        StartShaking(ShakeTime);
        // TODO: play sound (rumble/shake)
    }

    private void UpdateShaking()
    {
        _shakeTimer -= Time.DeltaTime;
        if (_shakeTimer <= 0f)
            EnterFalling();
    }

    // ── State: Falling ────────────────────────────────────────────────────────

    private void EnterFalling()
    {
        State      = FallingBlockState.Falling;
        _fallSpeed = 0f;
        StopShaking();
        // TODO: play sound (whoosh)
    }

    private void UpdateFalling()
    {
        float dt = Time.DeltaTime;

        // Accelerate downward.
        _fallSpeed = Math.Min(_fallSpeed + FallAccel * dt, MaxFallSpeed);

        // Apply lift speed so riding actors fall with the block.
        SetLiftSpeed(new Vector2(0f, _fallSpeed));

        // Attempt to move down; detect solid collision below.
        bool hitBelow = MoveVCollideSolids(_fallSpeed * dt, OnLand);

        if (hitBelow)
        {
            EnterLanded();
        }
    }

    private void OnLand(CelesteSolid _)
    {
        // TODO: emit particles (dust/impact)
        // TODO: play sound (thud)
    }

    // ── State: Landed ─────────────────────────────────────────────────────────

    private void EnterLanded()
    {
        State      = FallingBlockState.Landed;
        _fallSpeed = 0f;
        Safe       = true;
        SetLiftSpeed(Vector2.Zero);
        StartShaking(0.2f);   // Brief landing shake.
        // TODO: play sound (land)
        // TODO: emit particles (settle)
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
