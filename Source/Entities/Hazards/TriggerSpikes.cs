using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using KirbyCelesteStandalone.Core;
using Entity = Nez.Entity;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// Spikes that remain retracted until the player approaches, then extend and kill on contact.
/// Ported from Celeste's TriggerSpikes entity.
///
/// State machine:
/// <list type="bullet">
///   <item><see cref="TriggerSpikeState.Idle"/> – retracted, detection zone active.</item>
///   <item><see cref="TriggerSpikeState.Extending"/> – delay before full extension (<see cref="ExtendDelay"/>).</item>
///   <item><see cref="TriggerSpikeState.Extended"/> – fully extended; kills on directional contact.</item>
///   <item><see cref="TriggerSpikeState.Retracting"/> – returning to idle (<see cref="RetractDelay"/>).</item>
/// </list>
/// </summary>
public class TriggerSpikes : Entity
{
    // ── State enum ────────────────────────────────────────────────────────────

    /// <summary>Internal animation/collision state of the <see cref="TriggerSpikes"/>.</summary>
    public enum TriggerSpikeState
    {
        /// <summary>Spikes are hidden. Detection zone is listening for the player.</summary>
        Idle,
        /// <summary>Player was detected; waiting <see cref="ExtendDelay"/> before popping out.</summary>
        Extending,
        /// <summary>Spikes are fully extended and lethal.</summary>
        Extended,
        /// <summary>Player left; spikes are retreating back to <see cref="Idle"/>.</summary>
        Retracting,
    }

    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Seconds from detection until the spikes are fully extended.</summary>
    public float ExtendDelay { get; set; } = 0.04f;

    /// <summary>Seconds from player leaving until the spikes retract.</summary>
    public float RetractDelay { get; set; } = 0.8f;

    /// <summary>Pixel width of the detection zone (perpendicular to direction).</summary>
    private const float DetectionReach = 14f;

    /// <summary>Pixel thickness of the lethal hitbox when extended.</summary>
    private const float ExtendedThickness = 3f;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Current state of the spike strip.</summary>
    public TriggerSpikeState State { get; private set; } = TriggerSpikeState.Idle;

    private float _stateTimer;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>Length of the spike strip in pixels.</summary>
    public int Size { get; }

    /// <summary>Which side the spikes protrude from.</summary>
    public SpikeDirection Direction { get; }

    // ── Bounds ────────────────────────────────────────────────────────────────

    /// <summary>
    /// The lethal hitbox used when spikes are <see cref="TriggerSpikeState.Extended"/>.
    /// Same thin rectangle as <see cref="Spikes"/>.
    /// </summary>
    public RectangleF Bounds =>
        Spikes.BuildBounds(Position, Size, Direction, ExtendedThickness);

    /// <summary>
    /// Wider detection zone used during <see cref="TriggerSpikeState.Idle"/> to sense the player.
    /// Extends inward from the spike face.
    /// </summary>
    private RectangleF DetectionBounds =>
        Spikes.BuildBounds(Position, Size, Direction, DetectionReach);

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="TriggerSpikes"/> strip.
    /// </summary>
    /// <param name="position">Anchor position of the strip's tile block.</param>
    /// <param name="size">Length of the strip in pixels.</param>
    /// <param name="direction">Side the spikes protrude from.</param>
    public TriggerSpikes(Vector2 position, int size, SpikeDirection direction)
    {
        Position = position;
        Size = size;
        Direction = direction;
        Name = $"TriggerSpikes_{direction}";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        State = TriggerSpikeState.Idle;
        _stateTimer = 0f;
    }

    public override void Update()
    {
        float dt = Time.DeltaTime;

        switch (State)
        {
            case TriggerSpikeState.Idle:
                UpdateIdle();
                break;

            case TriggerSpikeState.Extending:
                UpdateExtending(dt);
                break;

            case TriggerSpikeState.Extended:
                UpdateExtended();
                break;

            case TriggerSpikeState.Retracting:
                UpdateRetracting(dt);
                break;
        }
    }

    // ── State update methods ──────────────────────────────────────────────────

    private void UpdateIdle()
    {
        if (PlayerInDetectionZone())
        {
            // TODO: play sound: event:/game/03_resort/trigger_spikes_deploy
            TransitionTo(TriggerSpikeState.Extending, ExtendDelay);
        }
    }

    private void UpdateExtending(float dt)
    {
        _stateTimer -= dt;
        if (_stateTimer <= 0f)
            TransitionTo(TriggerSpikeState.Extended, 0f);
    }

    private void UpdateExtended()
    {
        // Check lethal contact.
        if (CheckPlayerCollision(out PlayerController player))
        {
            // Only damage when approaching from the dangerous side.
            if (IsApproachingFromDangerousSide(player.Velocity))
            {
                Vector2 knockback = DirectionToKnockback(Direction);
                // TODO: play sound: event:/game/general/spikes_touch
                player.TakeDamage(1, knockback);
            }
        }

        // Retract when player moves away.
        if (!PlayerInDetectionZone())
            TransitionTo(TriggerSpikeState.Retracting, RetractDelay);
    }

    private void UpdateRetracting(float dt)
    {
        _stateTimer -= dt;
        if (_stateTimer <= 0f)
            TransitionTo(TriggerSpikeState.Idle, 0f);

        // Re-trigger if the player comes back before fully retracted.
        if (PlayerInDetectionZone())
            TransitionTo(TriggerSpikeState.Extending, ExtendDelay);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void TransitionTo(TriggerSpikeState newState, float timer)
    {
        State = newState;
        _stateTimer = timer;
    }

    /// <summary>
    /// Returns <c>true</c> when the player overlaps the wider detection zone.
    /// </summary>
    private bool PlayerInDetectionZone()
    {
        PlayerController candidate = Scene?.FindComponentOfType<PlayerController>();
        if (candidate == null) return false;

        RectangleF playerBounds = candidate.Entity.GetComponent<BoxCollider>()?.Bounds
                                  ?? RectangleF.Empty;
        return DetectionBounds.Intersects(playerBounds);
    }

    /// <summary>
    /// Returns <c>true</c> when the player velocity vector points into the spike face.
    /// </summary>
    private bool IsApproachingFromDangerousSide(Vector2 velocity)
    {
        return Direction switch
        {
            SpikeDirection.Up    => velocity.Y > 0,
            SpikeDirection.Down  => velocity.Y < 0,
            SpikeDirection.Left  => velocity.X > 0,
            SpikeDirection.Right => velocity.X < 0,
            _                    => false,
        };
    }

    /// <summary>Unit knockback vector pointing away from the spike face.</summary>
    private static Vector2 DirectionToKnockback(SpikeDirection dir)
    {
        return dir switch
        {
            SpikeDirection.Up    => -Vector2.UnitY,
            SpikeDirection.Down  =>  Vector2.UnitY,
            SpikeDirection.Left  => -Vector2.UnitX,
            SpikeDirection.Right =>  Vector2.UnitX,
            _                    =>  Vector2.Zero,
        };
    }

    /// <summary>
    /// Returns <c>true</c> and populates <paramref name="player"/> when the
    /// player's collider overlaps the lethal hitbox.
    /// </summary>
    private bool CheckPlayerCollision(out PlayerController player)
    {
        player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return false;

        RectangleF playerBounds = player.Entity.GetComponent<BoxCollider>()?.Bounds
                                  ?? RectangleF.Empty;
        return Bounds.Intersects(playerBounds);
    }
}
