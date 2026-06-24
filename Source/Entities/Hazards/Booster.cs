using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A launch bubble that propels the player in a chosen direction when entered.
/// Ported from Celeste's Booster entity; coroutine logic converted to an explicit state machine.
///
/// Variants:
/// <list type="bullet">
///   <item><b>Green</b> (<c>red = false</c>) – classic booster.</item>
///   <item><b>Red</b>   (<c>red = true</c>)  – faster, always-dash booster.</item>
/// </list>
///
/// State machine:
/// <list type="bullet">
///   <item><see cref="BoosterState.Available"/>   – waiting for player to enter.</item>
///   <item><see cref="BoosterState.Boosting"/>    – player is inside; boost applied.</item>
///   <item><see cref="BoosterState.Respawning"/>  – cooling down for <see cref="RespawnTime"/> seconds.</item>
/// </list>
/// </summary>
public class Booster : Entity
{
    // ── State enum ────────────────────────────────────────────────────────────

    /// <summary>Operational state of the <see cref="Booster"/>.</summary>
    public enum BoosterState
    {
        /// <summary>Booster is lit and ready to accept the player.</summary>
        Available,
        /// <summary>Player is inside; boost velocity has been applied.</summary>
        Boosting,
        /// <summary>Booster used; regenerating for <see cref="RespawnTime"/> seconds.</summary>
        Respawning,
    }

    // ── Constants ─────────────────────────────────────────────────────────────

    /// <summary>Circular collision / trigger radius in pixels.</summary>
    public const float Radius = 10f;

    /// <summary>Speed boost applied to the player when boosted (pixels per second).</summary>
    private const float BoostSpeed = 240f;

    /// <summary>Seconds the red booster spends in the Boosting state before releasing.</summary>
    private const float RedBoostDuration = 0.25f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary><c>true</c> for the faster red (dash-always) variant.</summary>
    public bool IsRed { get; }

    /// <summary>Seconds to wait in <see cref="BoosterState.Respawning"/> before becoming available.</summary>
    public float RespawnTime { get; set; } = 1f;

    // ── Public state ──────────────────────────────────────────────────────────

    /// <summary>Current operational state.</summary>
    public BoosterState State { get; private set; } = BoosterState.Available;

    /// <summary>The player currently being boosted, or <c>null</c>.</summary>
    public PlayerController? BoostingPlayer { get; private set; }

    // ── Internal state ────────────────────────────────────────────────────────

    private float _stateTimer;

    /// <summary>Direction the boost was applied in; used for red-booster release.</summary>
    private Vector2 _boostDirection;

    // ── Bounds ────────────────────────────────────────────────────────────────

    /// <summary>
    /// AABB derived from the booster's circular radius for broad-phase overlap checks.
    /// </summary>
    public RectangleF Bounds =>
        new RectangleF(
            Position.X - Radius,
            Position.Y - Radius,
            Radius * 2f,
            Radius * 2f);

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Booster"/>.
    /// </summary>
    /// <param name="position">World-space centre of the booster bubble.</param>
    /// <param name="red"><c>true</c> to create the red (faster) variant.</param>
    public Booster(Vector2 position, bool red = false)
    {
        Position = position;
        IsRed = red;
        Name = red ? "RedBooster" : "GreenBooster";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        State = BoosterState.Available;
        _stateTimer = 0f;
        BoostingPlayer = null;
        // TODO: load sprite – booster sprite sheet (green or red variant)
    }

    public override void Update()
    {
        float dt = Time.DeltaTime;

        switch (State)
        {
            case BoosterState.Available:
                UpdateAvailable();
                break;

            case BoosterState.Boosting:
                UpdateBoosting(dt);
                break;

            case BoosterState.Respawning:
                UpdateRespawning(dt);
                break;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called externally (or by this entity) when the booster launches the player.
    /// Sets the player's velocity in <paramref name="direction"/> at <see cref="BoostSpeed"/>.
    /// </summary>
    /// <param name="player">The player to boost.</param>
    /// <param name="direction">Normalised direction vector for the boost.</param>
    public void PlayerBoosted(PlayerController player, Vector2 direction)
    {
        if (direction == Vector2.Zero)
            direction = -Vector2.UnitY; // default: upward

        _boostDirection = direction;
        player.Velocity = direction * BoostSpeed;

        BoostingPlayer = player;
        TransitionTo(BoosterState.Boosting, IsRed ? RedBoostDuration : 0f);

        // TODO: emit particles – boost trail
        // TODO: play sound: event:/game/04_cliffside/greenbooster_enter (or red variant)
    }

    /// <summary>
    /// Called when the player exits the booster voluntarily or through movement.
    /// Transitions to <see cref="BoosterState.Respawning"/>.
    /// </summary>
    public void PlayerReleased()
    {
        if (State != BoosterState.Boosting) return;

        BoostingPlayer = null;
        TransitionTo(BoosterState.Respawning, RespawnTime);

        // TODO: play sound: event:/game/04_cliffside/greenbooster_end (or red variant)
    }

    /// <summary>
    /// Called when the boosted player dies mid-boost.
    /// Immediately begins respawn without releasing normally.
    /// </summary>
    public void PlayerDied()
    {
        BoostingPlayer = null;
        TransitionTo(BoosterState.Respawning, RespawnTime);
    }

    // ── State update methods ──────────────────────────────────────────────────

    private void UpdateAvailable()
    {
        if (!CheckPlayerCollision(out PlayerController player)) return;

        // Determine boost direction from player's current velocity or default upward.
        Vector2 dir = player.Velocity;
        if (dir != Vector2.Zero)
            dir.Normalize();
        else
            dir = -Vector2.UnitY;

        PlayerBoosted(player, dir);
    }

    private void UpdateBoosting(float dt)
    {
        // Keep boosting player centred on the booster (green booster sucks the player in).
        if (BoostingPlayer != null)
        {
            if (!IsRed)
            {
                // Green booster: hold the player at the booster centre.
                BoostingPlayer.Entity.Position = Vector2.Lerp(
                    BoostingPlayer.Entity.Position,
                    Position,
                    10f * dt);
            }
        }

        // Red booster releases automatically after its duration.
        if (IsRed)
        {
            _stateTimer -= dt;
            if (_stateTimer <= 0f)
            {
                // Re-apply velocity in the stored direction on release.
                if (BoostingPlayer != null)
                    BoostingPlayer.Velocity = _boostDirection * BoostSpeed;

                PlayerReleased();
            }
        }
    }

    private void UpdateRespawning(float dt)
    {
        _stateTimer -= dt;
        if (_stateTimer <= 0f)
        {
            TransitionTo(BoosterState.Available, 0f);
            // TODO: emit particles – respawn pop
            // TODO: play sound: event:/game/04_cliffside/greenbooster_reappear
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void TransitionTo(BoosterState newState, float timer)
    {
        State = newState;
        _stateTimer = timer;
    }

    /// <summary>
    /// Returns <c>true</c> and populates <paramref name="player"/> when the
    /// player's collider overlaps the booster's trigger zone.
    /// A secondary circular distance check provides a tighter hitbox.
    /// </summary>
    private bool CheckPlayerCollision(out PlayerController player)
    {
        player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return false;

        RectangleF playerBounds = player.Entity.GetComponent<BoxCollider>()?.Bounds
                                  ?? RectangleF.Empty;

        if (!Bounds.Intersects(playerBounds)) return false;

        // Tighter circular check: closest point on AABB to circle centre.
        float closestX = Math.Clamp(Position.X, playerBounds.X, playerBounds.X + playerBounds.Width);
        float closestY = Math.Clamp(Position.Y, playerBounds.Y, playerBounds.Y + playerBounds.Height);
        float dx = Position.X - closestX;
        float dy = Position.Y - closestY;
        return (dx * dx + dy * dy) <= (Radius * Radius);
    }
}
