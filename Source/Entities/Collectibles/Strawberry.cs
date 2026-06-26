using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;
using Component = Nez.Component;
using Collider = Nez.Collider;

namespace KirbyCelesteStandalone.Entities.Collectibles;

/// <summary>
/// Simplified port of Celeste's Strawberry.cs.
///
/// The Strawberry is a collectible that the player must hold for
/// <see cref="CollectHoldTime"/> seconds without being hurt to claim.
///
/// Variants:
/// <list type="bullet">
///   <item>
///     <see cref="Winged"/> — the berry flies upward when the player is near
///     until the player dashes, after which it becomes collectible normally.
///   </item>
///   <item>
///     <see cref="Golden"/> — a special hard-mode berry; purely cosmetic at
///     this level of port; extra logic can be added by callers.
///   </item>
/// </list>
///
/// If the player is hurt before fully collecting the berry, the berry returns
/// to its <see cref="_spawnPosition"/> via <see cref="ReturnHome"/>.
///
/// Collision: 14 × 14 box collider (trigger), centred on <see cref="Entity.Position"/>.
/// </summary>
public class Strawberry : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// How long the player must continuously hold the berry before it is
    /// considered "collected" (seconds).
    /// </summary>
    public const float CollectHoldTime = 0.6f;

    /// <summary>Amplitude of the sine-wave bob, in pixels.</summary>
    private const float BobAmplitude = 3f;

    /// <summary>Speed (radians / second) of the sine-wave bob.</summary>
    private const float BobSpeed = 2f;

    /// <summary>
    /// Horizontal distance at which a winged berry begins fleeing (pixels).
    /// </summary>
    private const float WingFleeDistance = 60f;

    /// <summary>Speed at which a winged berry flies away (pixels / second).</summary>
    private const float WingFleeSpeed = 80f;

    /// <summary>Speed at which the berry returns home after the player is hurt (pixels / second).</summary>
    private const float ReturnSpeed = 60f;

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// When <c>true</c> the berry initially flies away from the player until
    /// the player dashes, at which point it settles and can be collected.
    /// </summary>
    public bool Winged { get; private set; }

    /// <summary>
    /// Marks this as the golden strawberry variant.
    /// Cosmetically distinct; callers may attach additional logic.
    /// </summary>
    public bool Golden { get; private set; }

    /// <summary>Whether the berry has been fully collected by the player.</summary>
    public bool Collected { get; private set; }

    // -------------------------------------------------------------------------
    // Private state machine
    // -------------------------------------------------------------------------

    private enum StrawberryState
    {
        /// <summary>Bobbing, waiting for the player to touch it.</summary>
        Idle,
        /// <summary>
        /// Winged berry is fleeing; transitions to <see cref="Idle"/> once the
        /// player dashes.
        /// </summary>
        Fleeing,
        /// <summary>Player is holding the berry (collect timer counting down).</summary>
        Collecting,
        /// <summary>
        /// Player was hurt — berry is flying back to spawn position.
        /// </summary>
        ReturningHome,
        /// <summary>Berry has been awarded; entity can be destroyed.</summary>
        Collected,
    }

    private StrawberryState _state = StrawberryState.Idle;

    /// <summary>Accumulates the duration the player has been touching the berry.</summary>
    private float _collectTimer;

    /// <summary>Elapsed time used to drive the sine-wave bob.</summary>
    private float _bobTimer;

    /// <summary>Original world position; used as return target.</summary>
    private Vector2 _spawnPosition;

    /// <summary>Reference to the player while within range.</summary>
    private PlayerController? _trackedPlayer;

    /// <summary>Box collider (14 × 14 trigger).</summary>
    private BoxCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="Strawberry"/> at <paramref name="position"/>.
    /// </summary>
    /// <param name="position">World position (entity centre).</param>
    /// <param name="winged">
    ///   If <c>true</c>, the berry flies away until the player dashes.
    /// </param>
    /// <param name="golden">
    ///   If <c>true</c>, this is the golden-berry hard-mode variant.
    /// </param>
    public Strawberry(Vector2 position, bool winged = false, bool golden = false)
    {
        Winged  = winged;
        Golden  = golden;
        _spawnPosition = position;

        // Winged berries start in flee mode.
        _state = winged ? StrawberryState.Fleeing : StrawberryState.Idle;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        Entity.Position = _spawnPosition;

        // 14 × 14 trigger hitbox, centred on Position.
        _collider = Entity.AddComponent(new BoxCollider(14f, 14f));
        _collider.IsTrigger = true;

        // TODO: load sprite
        // e.g. Entity.AddComponent(new SpriteRenderer(strawberryTexture));
        // Use golden sprite variant when Golden == true.
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public void Update()
    {
        float dt = Time.DeltaTime;

        switch (_state)
        {
            case StrawberryState.Idle:
                UpdateIdle(dt);
                break;

            case StrawberryState.Fleeing:
                UpdateFleeing(dt);
                break;

            case StrawberryState.Collecting:
                UpdateCollecting(dt);
                break;

            case StrawberryState.ReturningHome:
                UpdateReturningHome(dt);
                break;

            case StrawberryState.Collected:
                // Nothing to do — entity will be destroyed shortly.
                break;
        }
    }

    // -------------------------------------------------------------------------
    // State handlers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Normal idle: bob up and down and check for player contact.
    /// </summary>
    private void UpdateIdle(float dt)
    {
        UpdateBob(dt);

        var player = FindNearbyPlayer();
        if (player == null) return;

        _trackedPlayer = player;
        _state = StrawberryState.Collecting;
        _collectTimer = 0f;
    }

    /// <summary>
    /// Winged mode: flee upward from the player until the player dashes.
    /// When the player dashes, land and switch to <see cref="StrawberryState.Idle"/>.
    /// </summary>
    private void UpdateFleeing(float dt)
    {
        UpdateBob(dt);

        var player = FindPlayerInRange(WingFleeDistance);
        if (player != null)
        {
            // Fly upward (away from player) and horizontally away.
            Vector2 awayDir = Entity.Position - player.Entity.Position;
            if (awayDir == Vector2.Zero)
                awayDir = -Vector2.UnitY; // default: straight up
            awayDir.Normalize();

            Entity.Position += awayDir * WingFleeSpeed * dt;

            // TODO: check player.IsDashing instead of placeholder.
            bool playerIsDashing = false; // placeholder

            if (playerIsDashing)
            {
                // Player dashed — stop fleeing, become collectible.
                _state = StrawberryState.Idle;

                // TODO: play "wings fold" sound / particle
            }
        }
    }

    /// <summary>
    /// Collecting: hold timer counts up while player stays in contact.
    /// Resets if player moves out of range. Completes at <see cref="CollectHoldTime"/>.
    /// </summary>
    private void UpdateCollecting(float dt)
    {
        if (_trackedPlayer == null)
        {
            _state = StrawberryState.Idle;
            _collectTimer = 0f;
            return;
        }

        // TODO: if player.IsHurt → call ReturnHome() and break.
        // bool playerHurt = _trackedPlayer.IsHurt; // placeholder
        // if (playerHurt) { ReturnHome(); return; }

        // Check player is still overlapping.
        var player = FindNearbyPlayer();
        if (player == null)
        {
            // Player moved away — reset timer but stay in collecting state
            // (Celeste lets the timer reset when contact is broken).
            _state = StrawberryState.Idle;
            _collectTimer = 0f;
            _trackedPlayer = null;
            return;
        }

        _collectTimer += dt;

        if (_collectTimer >= CollectHoldTime)
        {
            CompleteCollection();
        }
    }

    /// <summary>
    /// Fly back to the spawn position after the player was hurt.
    /// </summary>
    private void UpdateReturningHome(float dt)
    {
        Vector2 toHome = _spawnPosition - Entity.Position;
        float dist = toHome.Length();

        if (dist <= ReturnSpeed * dt)
        {
            Entity.Position = _spawnPosition;
            _bobTimer       = 0f;
            _collectTimer   = 0f;
            _trackedPlayer  = null;
            _state          = Winged ? StrawberryState.Fleeing : StrawberryState.Idle;
        }
        else
        {
            toHome.Normalize();
            Entity.Position += toHome * ReturnSpeed * dt;
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Advances the sine-wave bob animation.</summary>
    private void UpdateBob(float dt)
    {
        _bobTimer += dt * BobSpeed;
        float yOffset = (float)Math.Sin(_bobTimer) * BobAmplitude;
        Entity.Position = new Vector2(_spawnPosition.X, _spawnPosition.Y + yOffset);
    }

    // Reusable results buffer — kept static so we don't allocate each frame.
    private static readonly Collider[] _overlapResults = new Collider[8];

    /// <summary>
    /// Returns the first <see cref="PlayerController"/> whose collider overlaps
    /// the strawberry's 14 × 14 hitbox, or <c>null</c> if none.
    /// </summary>
    private PlayerController? FindNearbyPlayer()
    {
        if (_collider == null) return null;

        var rect = new RectangleF(
            Entity.Position.X - 7f,
            Entity.Position.Y - 7f,
            14f,
            14f);

        int count = Nez.Physics.OverlapRectangleAll(ref rect, _overlapResults);

        for (int i = 0; i < count; i++)
        {
            var hit = _overlapResults[i];
            if (hit == _collider) continue;
            var player = hit.Entity?.GetComponent<PlayerController>();
            if (player != null) return player;
        }

        return null;
    }

    /// <summary>
    /// Returns the first player within <paramref name="range"/> pixels, or
    /// <c>null</c> if none is close enough.
    /// </summary>
    private PlayerController? FindPlayerInRange(float range)
    {
        var player = Entity.Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return null;

        float dist = Vector2.Distance(Entity.Position, player.Entity.Position);
        return dist <= range ? player : null;
    }

    /// <summary>Awards the strawberry to the player and marks it as collected.</summary>
    private void CompleteCollection()
    {
        Collected = true;
        _state    = StrawberryState.Collected;

        // TODO: award strawberry
        // e.g. player.StrawberryCount++;

        // TODO: play collection sound
        // e.g. Audio.Play("event:/game/general/strawberry_get");

        // TODO: emit celebration particles

        // Hide visual / disable collider.
        if (_collider != null)
            _collider.SetEnabled(false);

        // TODO: hide SpriteRenderer
        // e.g. GetComponent<SpriteRenderer>()?.SetEnabled(false);

        // Optionally remove entity after a brief delay.
        Nez.Core.Schedule(1f, _ => Entity.Destroy());
    }

    /// <summary>
    /// Call this when the player is hurt mid-collection to send the berry home.
    /// </summary>
    public void ReturnHome()
    {
        _collectTimer  = 0f;
        _trackedPlayer = null;
        _state         = StrawberryState.ReturningHome;

        // TODO: play "return" sound
    }
}
