using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using BoxCollider = DZ.Nez.BoxCollider;
using System;
using DZ.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's Checkpoint entity.
///
/// When the player touches the checkpoint's AABB zone it becomes <see cref="Triggered"/>,
/// fires the static <see cref="OnCheckpointReached"/> event (so external systems such as a
/// save/respawn manager can store the respawn position), and then plays a looping sine-wave
/// glow animation.
///
/// <list type="bullet">
///   <item><see cref="SpawnOffset"/> – pixel offset from this entity's <c>Position</c> that
///     defines the actual respawn location (mirrors Celeste's optional spawnTarget parameter).</item>
///   <item><see cref="SpawnPosition"/> – convenience property: <c>Position + SpawnOffset</c>.</item>
///   <item><see cref="OnCheckpointReached"/> – raised once on first activation; subscribe to
///     save the respawn point in your session/save system.</item>
/// </list>
///
/// Visual: a 16×16 trigger zone. Glow animation uses a sine wave on <see cref="_glowSine"/>
/// once triggered. Replace the <c>// TODO: load sprite</c> block with real art.
/// </summary>
public class CelesteCheckpoint : Entity, IUpdatable
{
    // ── Constants ────────────────────────────────────────────────────────────

    /// <summary>Half-size of the trigger hitbox in pixels.</summary>
    private const float HitboxHalfW = 8f;
    private const float HitboxHalfH = 8f;

    /// <summary>Speed of the sine-glow animation (radians per second).</summary>
    private const float GlowSineSpeed = 2.2f;

    // ── Events ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Raised once the first time this checkpoint is activated by the player.
    /// Subscribe to record the respawn position in your game's save/session layer.
    /// </summary>
    public static event Action<CelesteCheckpoint> OnCheckpointReached;

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary><c>true</c> after the player has touched and activated this checkpoint.</summary>
    public bool Triggered { get; private set; }

    /// <summary>
    /// Pixel offset from this entity's <c>Position</c> that defines where the player
    /// will actually be placed on respawn.
    /// Mirrors Celeste's optional <c>spawnTarget</c> constructor parameter.
    /// </summary>
    public Vector2 SpawnOffset { get; set; }

    /// <summary>World-space respawn location: <c>Position + SpawnOffset</c>.</summary>
    public Vector2 SpawnPosition => Position + SpawnOffset;

    // ── Animation ────────────────────────────────────────────────────────────

    /// <summary>Accumulates time for the sine-wave glow once triggered.</summary>
    private float _glowSine;

    // ── Trigger zone ─────────────────────────────────────────────────────────

    /// <summary>AABB trigger zone centered on this entity's <c>Position</c>.</summary>
    public RectangleF Bounds =>
        new RectangleF(
            Position.X - HitboxHalfW,
            Position.Y - HitboxHalfH,
            HitboxHalfW * 2f,
            HitboxHalfH * 2f);

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="CelesteCheckpoint"/>.
    /// </summary>
    /// <param name="position">World-space centre position of the checkpoint.</param>
    /// <param name="spawnTarget">
    ///   Optional world-space position where the player will respawn.
    ///   When <c>null</c>, defaults to <paramref name="position"/> (zero offset).
    /// </param>
    public CelesteCheckpoint(Vector2 position, Vector2? spawnTarget = null)
    {
        Position   = position;
        SpawnOffset = spawnTarget.HasValue
            ? spawnTarget.Value - position
            : Vector2.Zero;

        Name = "CelesteCheckpoint";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        // TODO: load sprite – add SpriteRenderer with checkpoint sprite sheet
    }

    // ── IUpdatable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Update()
    {
        float dt = Time.DeltaTime;

        if (Triggered)
        {
            // Advance the sine-glow animation.
            _glowSine += GlowSineSpeed * dt;

            // Glow intensity oscillates 0–1. Drive a visual effect here, e.g.:
            //   float glow = (float)(Math.Sin(_glowSine) * 0.5 + 0.5);
            //   renderer.Color = Color.Lerp(Color.White, Color.Cyan, glow);
            return;
        }

        // ── Check player overlap ─────────────────────────────────────────────
        PlayerController player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return;

        RectangleF playerBounds = player.Entity.GetComponent<BoxCollider>()?.Bounds
                                  ?? RectangleF.Empty;

        if (!Bounds.Intersects(playerBounds)) return;

        // ── First activation ─────────────────────────────────────────────────
        Triggered = true;
        _glowSine = 0f;

        // TODO: play sound: event:/game/general/checkpoint_activate
        // TODO: emit particles – sparkle/glow burst at Position

        OnCheckpointReached?.Invoke(this);
    }
}
