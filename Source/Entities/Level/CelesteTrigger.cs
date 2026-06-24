using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

// ═══════════════════════════════════════════════════════════════════════════════
// PositionModes enum
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Mirrors Celeste's <c>Trigger.PositionModes</c>.
/// Controls how <see cref="CelesteTrigger.GetPositionLerp"/> maps the player's position
/// within the trigger zone to the [0, 1] range.
/// </summary>
public enum PositionModes
{
    /// <summary>Always returns 0 – the player's position has no effect.</summary>
    NoEffect,
    /// <summary>Lerp is 0 at the left edge, 1 at the right edge.</summary>
    LeftToRight,
    /// <summary>Lerp is 0 at the right edge, 1 at the left edge.</summary>
    RightToLeft,
    /// <summary>Lerp is 0 at the top edge, 1 at the bottom edge.</summary>
    TopToBottom,
    /// <summary>Lerp is 0 at the bottom edge, 1 at the top edge.</summary>
    BottomToTop,
    /// <summary>Lerp is 0 at either horizontal edge, 1 at the horizontal centre.</summary>
    HorizontalCenter,
    /// <summary>Lerp is 0 at either vertical edge, 1 at the vertical centre.</summary>
    VerticalCenter,
}

// ═══════════════════════════════════════════════════════════════════════════════
// CelesteTrigger — abstract base
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Port of Celeste's <c>Trigger.cs</c>.
///
/// An invisible rectangular zone that calls <see cref="OnEnter"/>, <see cref="OnStay"/>,
/// and <see cref="OnLeave"/> as the player moves into, through, and out of the zone.
///
/// <para>
/// Subclass and override any of the three callbacks to implement custom behaviours (music
/// changes, camera offsets, wind forces, etc.). See the Triggers/ subfolder for concrete
/// implementations such as MusicTrigger, CameraOffsetTrigger, and WindTrigger.
/// </para>
///
/// <para>
/// <see cref="GetPositionLerp"/> maps the player's world position within the AABB to
/// a normalised [0, 1] value according to the chosen <see cref="PositionModes"/>. This is
/// the same helper Celeste uses to drive smooth camera/audio transitions inside triggers.
/// </para>
/// </summary>
public abstract class CelesteTrigger : Entity, IUpdatable
{
    // ── Geometry ─────────────────────────────────────────────────────────────

    /// <summary>Width of the trigger zone in pixels.</summary>
    public float Width  { get; protected set; }

    /// <summary>Height of the trigger zone in pixels.</summary>
    public float Height { get; protected set; }

    /// <summary>
    /// Axis-aligned bounding rectangle of this trigger, with <c>Position</c> at the top-left.
    /// </summary>
    public RectangleF Bounds =>
        new RectangleF(Position.X, Position.Y, Width, Height);

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary><c>true</c> while the player is inside the trigger zone.</summary>
    public bool PlayerIsInside { get; private set; }

    /// <summary>
    /// General-purpose flag subclasses can set to record a one-shot activation.
    /// Not toggled automatically by the base class.
    /// </summary>
    public bool Triggered { get; protected set; }

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the trigger zone.
    /// </summary>
    /// <param name="position">Top-left world position of the trigger AABB.</param>
    /// <param name="width">Width of the zone in pixels.</param>
    /// <param name="height">Height of the zone in pixels.</param>
    protected CelesteTrigger(Vector2 position, float width, float height)
    {
        Position = position;
        Width    = width;
        Height   = height;
        Name     = GetType().Name;
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        PlayerIsInside = false;
    }

    // ── IUpdatable ───────────────────────────────────────────────────────────

    /// <summary>
    /// Each frame: finds the player and dispatches <see cref="OnEnter"/>,
    /// <see cref="OnStay"/>, or <see cref="OnLeave"/> as appropriate.
    /// </summary>
    public virtual void Update()
    {
        PlayerController? player = Scene?.FindComponentOfType<PlayerController>();

        if (player == null)
        {
            // If the player disappears while inside, fire OnLeave.
            if (PlayerIsInside)
            {
                PlayerIsInside = false;
                OnLeave(null!);
            }
            return;
        }

        RectangleF playerBounds = player.Entity.GetComponent<BoxCollider>()?.Bounds
                                  ?? RectangleF.Empty;

        bool isInside = Bounds.Intersects(playerBounds);

        if (isInside && !PlayerIsInside)
        {
            PlayerIsInside = true;
            OnEnter(player);
        }
        else if (!isInside && PlayerIsInside)
        {
            PlayerIsInside = false;
            OnLeave(player);
        }
        else if (isInside)
        {
            OnStay(player);
        }
    }

    // ── Virtual callbacks ─────────────────────────────────────────────────────

    /// <summary>
    /// Called the first frame the player enters this trigger zone.
    /// </summary>
    /// <param name="player">The player controller (never <c>null</c> in normal operation).</param>
    public virtual void OnEnter(PlayerController player) { }

    /// <summary>
    /// Called every frame while the player remains inside this trigger zone.
    /// </summary>
    /// <param name="player">The player controller.</param>
    public virtual void OnStay(PlayerController player) { }

    /// <summary>
    /// Called the first frame after the player leaves this trigger zone.
    /// </summary>
    /// <param name="player">The player controller (may be <c>null</c> if the player was removed from the scene).</param>
    public virtual void OnLeave(PlayerController player) { }

    // ── Position lerp helper ──────────────────────────────────────────────────

    /// <summary>
    /// Maps the player's world position within this trigger's AABB to a normalised
    /// [0, 1] float according to <paramref name="mode"/>.
    ///
    /// <para>
    /// Mirrors Celeste's <c>Trigger.GetPositionLerp(Player, PositionModes)</c>.
    /// Useful for smoothly blending camera offsets, audio volumes, or any other
    /// continuously varying value as the player moves across the trigger.
    /// </para>
    /// </summary>
    /// <param name="player">The player whose position is queried.</param>
    /// <param name="mode">Which axis / direction to measure.</param>
    /// <returns>A value clamped to [0, 1].</returns>
    protected float GetPositionLerp(PlayerController player, PositionModes mode)
    {
        if (mode == PositionModes.NoEffect || player == null)
            return 0f;

        Vector2 playerPos = player.Entity.Position;

        switch (mode)
        {
            case PositionModes.LeftToRight:
            {
                float t = (playerPos.X - Position.X) / Width;
                return MathHelper.Clamp(t, 0f, 1f);
            }
            case PositionModes.RightToLeft:
            {
                float t = 1f - (playerPos.X - Position.X) / Width;
                return MathHelper.Clamp(t, 0f, 1f);
            }
            case PositionModes.TopToBottom:
            {
                float t = (playerPos.Y - Position.Y) / Height;
                return MathHelper.Clamp(t, 0f, 1f);
            }
            case PositionModes.BottomToTop:
            {
                float t = 1f - (playerPos.Y - Position.Y) / Height;
                return MathHelper.Clamp(t, 0f, 1f);
            }
            case PositionModes.HorizontalCenter:
            {
                // 0 at either edge, 1 at centre.
                float halfW = Width * 0.5f;
                float t = 1f - Math.Abs(playerPos.X - (Position.X + halfW)) / halfW;
                return MathHelper.Clamp(t, 0f, 1f);
            }
            case PositionModes.VerticalCenter:
            {
                float halfH = Height * 0.5f;
                float t = 1f - Math.Abs(playerPos.Y - (Position.Y + halfH)) / halfH;
                return MathHelper.Clamp(t, 0f, 1f);
            }
            default:
                return 0f;
        }
    }
}
