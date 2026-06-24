using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Hazards;

// ── Orientation enum ──────────────────────────────────────────────────────────

/// <summary>Which surface the <see cref="Spring"/> is attached to.</summary>
public enum SpringOrientation
{
    /// <summary>Spring sits on the floor and launches the player straight up.</summary>
    Floor,
    /// <summary>Spring is mounted on a left wall and launches the player rightward.</summary>
    WallLeft,
    /// <summary>Spring is mounted on a right wall and launches the player leftward.</summary>
    WallRight,
}

/// <summary>
/// A launch pad that hurls the player in a fixed direction when touched.
/// Ported from Celeste's Spring entity.
///
/// <list type="bullet">
///   <item><b>Floor</b> – launches up via SuperBounce equivalent (sets Y velocity to −280).</item>
///   <item><b>WallLeft / WallRight</b> – launches sideways via SideBounce equivalent.</item>
/// </list>
///
/// After activating, the spring plays a 0.5-second bounce animation (timer-based) and
/// cannot trigger again until the animation completes.
/// Set <see cref="PlayerCanUse"/> to <c>false</c> to disable player interaction entirely.
/// </summary>
public class Spring : Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────

    /// <summary>Duration of the bounce animation in seconds.</summary>
    private const float BounceAnimationDuration = 0.5f;

    /// <summary>Half-size of the spring's trigger hitbox in pixels.</summary>
    private const float HitboxHalfW = 6f;
    private const float HitboxHalfH = 4f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>Which surface this spring is attached to.</summary>
    public SpringOrientation Orientation { get; }

    /// <summary>When <c>false</c> the spring will never activate for the player.</summary>
    public bool PlayerCanUse { get; set; }

    // ── Animation state ───────────────────────────────────────────────────────

    /// <summary><c>true</c> while the bounce animation is playing.</summary>
    public bool IsBouncing => _bounceTimer > 0f;

    private float _bounceTimer;

    // ── Bounds ────────────────────────────────────────────────────────────────

    /// <summary>
    /// AABB trigger zone centred on the spring's position, adjusted for orientation.
    /// </summary>
    public RectangleF Bounds
    {
        get
        {
            // For floor springs the active zone is the top surface.
            // For wall springs it is the face pointing into the room.
            return Orientation switch
            {
                SpringOrientation.Floor =>
                    new RectangleF(
                        Position.X - HitboxHalfW,
                        Position.Y - HitboxHalfH,
                        HitboxHalfW * 2f,
                        HitboxHalfH * 2f),

                SpringOrientation.WallLeft =>
                    new RectangleF(
                        Position.X,
                        Position.Y - HitboxHalfW,
                        HitboxHalfH * 2f,
                        HitboxHalfW * 2f),

                SpringOrientation.WallRight =>
                    new RectangleF(
                        Position.X - HitboxHalfH * 2f,
                        Position.Y - HitboxHalfW,
                        HitboxHalfH * 2f,
                        HitboxHalfW * 2f),

                _ => new RectangleF(
                        Position.X - HitboxHalfW,
                        Position.Y - HitboxHalfH,
                        HitboxHalfW * 2f,
                        HitboxHalfH * 2f),
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Spring"/>.
    /// </summary>
    /// <param name="position">World-space centre position of the spring.</param>
    /// <param name="orientation">Which surface the spring is mounted on.</param>
    /// <param name="playerCanUse">Whether the player can trigger this spring.</param>
    public Spring(Vector2 position, SpringOrientation orientation, bool playerCanUse = true)
    {
        Position = position;
        Orientation = orientation;
        PlayerCanUse = playerCanUse;
        Name = $"Spring_{orientation}";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        _bounceTimer = 0f;
        // TODO: load sprite – add SpriteRenderer with spring sheet
    }

    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Tick animation timer.
        if (_bounceTimer > 0f)
            _bounceTimer -= dt;

        // Skip collision while animating (cooldown).
        if (_bounceTimer > 0f) return;
        if (!PlayerCanUse) return;

        if (!CheckPlayerCollision(out PlayerController player)) return;

        LaunchPlayer(player);
    }

    // ── Launch logic ──────────────────────────────────────────────────────────

    /// <summary>
    /// Launches the player in the appropriate direction for this spring's orientation
    /// and starts the bounce animation.
    /// </summary>
    private void LaunchPlayer(PlayerController player)
    {
        // TODO: play sound: event:/game/general/spring
        // TODO: emit particles

        switch (Orientation)
        {
            case SpringOrientation.Floor:
                // SuperBounce equivalent: slam Y velocity upward.
                player.Velocity = new Vector2(player.Velocity.X, -280f);
                break;

            case SpringOrientation.WallLeft:
                // SideBounce equivalent: push right, cap upward speed.
                player.Velocity = new Vector2(1 * 250f, Math.Min(player.Velocity.Y, -105f));
                break;

            case SpringOrientation.WallRight:
                // SideBounce equivalent: push left, cap upward speed.
                player.Velocity = new Vector2(-1 * 250f, Math.Min(player.Velocity.Y, -105f));
                break;
        }

        // Start bounce animation cooldown.
        _bounceTimer = BounceAnimationDuration;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> and populates <paramref name="player"/> when the
    /// player's collider overlaps this spring's trigger zone.
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
