using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using System;
using System.Collections.Generic;
using DZ.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Collectibles;

/// <summary>
/// Port of Celeste's Refill.cs.
///
/// A collectible crystal that restores the player's dash charge(s).
/// The gem bobs up and down on a sine wave while waiting to be collected.
/// After collection it becomes invisible and either disappears permanently
/// (when <see cref="OneUse"/> is <c>true</c>) or respawns after
/// <see cref="RespawnTime"/> seconds.
///
/// Collision: 16 × 16 box collider, centred on <see cref="Entity.Position"/>.
/// </summary>
public class Refill : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Seconds until the refill reappears after being collected.</summary>
    public const float RespawnTime = 2.5f;

    /// <summary>Amplitude of the sine-wave bob, in pixels.</summary>
    private const float BobAmplitude = 3f;

    /// <summary>Speed (radians / second) of the sine-wave bob.</summary>
    private const float BobSpeed = 2f;

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// When <c>true</c> the refill restores 2 dash charges; otherwise 1.
    /// Corresponds to the "two-dash crystal" variant in Celeste.
    /// </summary>
    public bool TwoDashes { get; private set; }

    /// <summary>
    /// When <c>true</c> the refill is consumed on first pickup and never
    /// respawns (single-use crystal).
    /// </summary>
    public bool OneUse { get; private set; }

    /// <summary>
    /// Countdown timer (seconds) until the refill reappears.
    /// Only active while the refill is invisible / respawning.
    /// </summary>
    public float RespawnTimer { get; private set; }

    /// <summary>Whether the refill is currently visible and collectible.</summary>
    public bool IsActive { get; private set; } = true;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    /// <summary>Elapsed time used to drive the sine-wave bob.</summary>
    private float _bobTimer;

    /// <summary>Base world position (spawn origin), used to restore position on respawn.</summary>
    private Vector2 _spawnPosition;

    /// <summary>Box collider attached to this entity (16 × 16).</summary>
    private BoxCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="Refill"/> at the given world position.
    /// </summary>
    /// <param name="position">World position (entity centre).</param>
    /// <param name="twoDashes">
    ///   If <c>true</c>, grants the player 2 dash charges instead of 1.
    /// </param>
    /// <param name="oneUse">
    ///   If <c>true</c>, the refill disappears permanently after collection.
    /// </param>
    public Refill(Vector2 position, bool twoDashes = false, bool oneUse = false)
    {
        TwoDashes = twoDashes;
        OneUse    = oneUse;
        _spawnPosition = position;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Place entity at spawn position.
        Entity.Position = _spawnPosition;

        // 16 × 16 hitbox, centred on Position.
        _collider = Entity.AddComponent(new BoxCollider(16f, 16f));
        _collider.IsTrigger = true;

        // TODO: load sprite — e.g. Entity.AddComponent(new SpriteRenderer(refillTexture));
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    /// <summary>
    /// Each frame: bob the sprite, check for player overlap, and handle respawn
    /// countdown when inactive.
    /// </summary>
    public void Update()
    {
        float dt = Time.DeltaTime;

        if (IsActive)
        {
            UpdateBob(dt);
            CheckPlayerOverlap();
        }
        else
        {
            UpdateRespawn(dt);
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Advances the sine-wave bob animation.</summary>
    private void UpdateBob(float dt)
    {
        _bobTimer += dt * BobSpeed;

        // Offset the entity vertically around its spawn position.
        float yOffset = (float)Math.Sin(_bobTimer) * BobAmplitude;
        Entity.Position = new Vector2(_spawnPosition.X, _spawnPosition.Y + yOffset);
    }

    // Reusable results buffer — kept static so we don't allocate each frame.
    private static readonly Collider[] _overlapResults = new Collider[8];

    /// <summary>
    /// Checks whether the player is overlapping the trigger collider;
    /// if so, triggers collection.
    /// </summary>
    private void CheckPlayerOverlap()
    {
        if (_collider == null) return;

        var rect = new RectangleF(
            Entity.Position.X - 8f,
            Entity.Position.Y - 8f,
            16f,
            16f);

        int count = DZ.Nez.Physics.OverlapRectangleAll(ref rect, _overlapResults);

        for (int i = 0; i < count; i++)
        {
            var hit = _overlapResults[i];
            if (hit == _collider) continue;

            var player = hit.Entity?.GetComponent<PlayerController>();
            if (player == null) continue;

            OnCollected(player);
            break;
        }
    }

    /// <summary>Handles the logic that runs when a player touches the refill.</summary>
    private void OnCollected(PlayerController player)
    {
        // TODO: refill player dash
        // e.g. player.RefillDash(TwoDashes ? 2 : 1);

        // TODO: play sound
        // e.g. Audio.Play("event:/game/general/diamond_touch");

        // TODO: emit particles
        // e.g. ParticleSystem.Emit(refillParticles, Entity.Position);

        if (OneUse)
        {
            // Permanently remove this collectible.
            Entity.Destroy();
            return;
        }

        // Hide and start respawn countdown.
        SetActive(false);
        RespawnTimer = RespawnTime;
    }

    /// <summary>
    /// Ticks the respawn countdown and re-activates the refill when it expires.
    /// </summary>
    private void UpdateRespawn(float dt)
    {
        RespawnTimer -= dt;
        if (RespawnTimer <= 0f)
        {
            Respawn();
        }
    }

    /// <summary>Re-activates the refill at its original spawn position.</summary>
    private void Respawn()
    {
        Entity.Position = _spawnPosition;
        _bobTimer       = 0f;
        SetActive(true);

        // TODO: play respawn sound
        // TODO: emit respawn particles
    }

    /// <summary>Shows or hides the refill and toggles its collider.</summary>
    private void SetActive(bool active)
    {
        IsActive = active;

        if (_collider != null)
            _collider.SetEnabled(active);

        // TODO: show/hide SpriteRenderer
        // e.g. GetComponent<SpriteRenderer>()?.SetEnabled(active);
    }
}
