using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Collectibles;

/// <summary>
/// Port of Celeste's FlyFeather.cs.
///
/// A collectible feather that grants the player the "star-fly" (free-flight) state.
///
/// Behaviour:
/// <list type="bullet">
///   <item>Bobs up and down on a sine wave while idle.</item>
///   <item>
///     When <see cref="Shielded"/> is <c>true</c> the player <em>must</em> be
///     dashing to collect it; a non-dashing touch instead bounces the player away
///     (<c>player.Velocity = -player.Velocity * 0.5f</c>).
///   </item>
///   <item>
///     When <see cref="SingleUse"/> is <c>true</c> the feather is destroyed on
///     collection rather than respawning.
///   </item>
///   <item>Respawns after <see cref="RespawnTime"/> seconds otherwise.</item>
/// </list>
///
/// Collision: 20 × 20 box collider (trigger), centred on <see cref="Entity.Position"/>.
/// </summary>
public class FlyFeather : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Seconds until the feather reappears after being collected.</summary>
    public const float RespawnTime = 3f;

    /// <summary>Amplitude of the sine-wave bob, in pixels.</summary>
    private const float BobAmplitude = 3f;

    /// <summary>Speed (radians / second) of the sine-wave bob.</summary>
    private const float BobSpeed = 2f;

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// When <c>true</c> the player must be mid-dash to pick up the feather;
    /// a non-dashing collision bounces the player.
    /// </summary>
    public bool Shielded { get; private set; }

    /// <summary>
    /// When <c>true</c> the feather is destroyed permanently on first pickup
    /// and never respawns.
    /// </summary>
    public bool SingleUse { get; private set; }

    /// <summary>
    /// Countdown (seconds) until the feather reappears.
    /// Only active while the feather is invisible / respawning.
    /// </summary>
    public float RespawnTimer { get; private set; }

    /// <summary>Whether the feather is currently visible and collectible.</summary>
    public bool IsActive { get; private set; } = true;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    /// <summary>Elapsed time used to drive the sine-wave bob.</summary>
    private float _bobTimer;

    /// <summary>Original world position (spawn origin).</summary>
    private Vector2 _spawnPosition;

    /// <summary>Box collider attached to this entity (20 × 20).</summary>
    private BoxCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="FlyFeather"/> at the given world position.
    /// </summary>
    /// <param name="position">World position (entity centre).</param>
    /// <param name="shielded">
    ///   If <c>true</c>, only a dashing player can collect the feather.
    /// </param>
    /// <param name="singleUse">
    ///   If <c>true</c>, the feather is permanently destroyed after collection.
    /// </param>
    public FlyFeather(Vector2 position, bool shielded = false, bool singleUse = false)
    {
        Shielded   = shielded;
        SingleUse  = singleUse;
        _spawnPosition = position;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        Entity.Position = _spawnPosition;

        // 20 × 20 trigger collider, centred on Position.
        _collider = Entity.AddComponent(new BoxCollider(20f, 20f));
        _collider.IsTrigger = true;

        // TODO: load sprite — e.g. Entity.AddComponent(new SpriteRenderer(featherTexture));
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    /// <summary>
    /// Each frame: bob the sprite, check for player overlap (respecting
    /// <see cref="Shielded"/>), and tick the respawn countdown when inactive.
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
        float yOffset = (float)Math.Sin(_bobTimer) * BobAmplitude;
        Entity.Position = new Vector2(_spawnPosition.X, _spawnPosition.Y + yOffset);
    }

    // Reusable results buffer — kept static so we don't allocate each frame.
    private static readonly Collider[] _overlapResults = new Collider[8];

    /// <summary>
    /// Checks for player overlap and decides whether to collect or bounce.
    /// </summary>
    private void CheckPlayerOverlap()
    {
        if (_collider == null) return;

        var rect = new RectangleF(
            Entity.Position.X - 10f,
            Entity.Position.Y - 10f,
            20f,
            20f);

        int count = Nez.Physics.OverlapRectangleAll(ref rect, _overlapResults);

        for (int i = 0; i < count; i++)
        {
            var hit = _overlapResults[i];
            if (hit == _collider) continue;

            var player = hit.Entity?.GetComponent<PlayerController>();
            if (player == null) continue;

            HandlePlayerContact(player);
            break;
        }
    }

    /// <summary>
    /// Handles the moment a player enters the feather's trigger area.
    /// If <see cref="Shielded"/>, only a dashing player triggers collection;
    /// otherwise the player is bounced back.
    /// </summary>
    private void HandlePlayerContact(PlayerController player)
    {
        if (Shielded)
        {
            // Check whether the player is currently dashing.
            // The _isDashing field is private in PlayerController; expose a
            // public property or method if needed.
            // TODO: replace with actual dash-state check, e.g. player.IsDashing
            bool playerIsDashing = false; // placeholder

            if (!playerIsDashing)
            {
                // Bounce the player away from the feather.
                player.Velocity = -player.Velocity * 0.5f;
                return;
            }
        }

        // Player qualifies — collect the feather.
        OnCollected(player);
    }

    /// <summary>Handles collection logic.</summary>
    private void OnCollected(PlayerController player)
    {
        // TODO: grant player star-fly / feather state
        // e.g. player.ActivateStarFly();

        // TODO: play sound
        // e.g. Audio.Play("event:/game/general/feather_get");

        // TODO: emit particles
        // e.g. ParticleSystem.Emit(featherParticles, Entity.Position);

        if (SingleUse)
        {
            Entity.Destroy();
            return;
        }

        SetActive(false);
        RespawnTimer = RespawnTime;
    }

    /// <summary>Ticks the respawn countdown and re-activates when it expires.</summary>
    private void UpdateRespawn(float dt)
    {
        RespawnTimer -= dt;
        if (RespawnTimer <= 0f)
        {
            Respawn();
        }
    }

    /// <summary>Re-activates the feather at its original spawn position.</summary>
    private void Respawn()
    {
        Entity.Position = _spawnPosition;
        _bobTimer       = 0f;
        SetActive(true);

        // TODO: play respawn sound
        // TODO: emit respawn particles
    }

    /// <summary>Enables or disables the feather's visibility and collider.</summary>
    private void SetActive(bool active)
    {
        IsActive = active;

        if (_collider != null)
            _collider.SetEnabled(active);

        // TODO: show/hide SpriteRenderer
        // e.g. GetComponent<SpriteRenderer>()?.SetEnabled(active);
    }
}
