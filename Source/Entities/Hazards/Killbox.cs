using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// An invisible, instant-kill zone typically placed below a level to catch falling players.
/// Ported from Celeste's Killbox entity.
///
/// The killbox only activates when the player is descending into it from above (gate logic),
/// preventing kills on upward-moving passes. Any player contact triggers an instant death
/// via <see cref="PlayerController.TakeDamage"/> with 999 damage.
/// </summary>
public class Killbox : Entity
{
    // ── Dimensions ────────────────────────────────────────────────────────────
    private readonly float _width;
    private const float Height = 32f;

    // ── Activation gate ───────────────────────────────────────────────────────
    /// <summary>
    /// Set to <c>true</c> after the player has been seen above the killbox.
    /// Prevents killing a player who teleports or spawns inside the box.
    /// </summary>
    private bool _playerWasAbove;

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// World-space axis-aligned bounding rectangle of the killbox.
    /// </summary>
    public RectangleF Bounds =>
        new RectangleF(Position.X, Position.Y, _width, Height);

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Killbox"/>.
    /// </summary>
    /// <param name="position">Top-left world position of the killbox.</param>
    /// <param name="width">Horizontal extent of the killbox in pixels.</param>
    public Killbox(Vector2 position, float width)
    {
        Position = position;
        _width = width;
        Name = "Killbox";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        _playerWasAbove = false;
    }

    public override void Update()
    {
        if (!CheckPlayerCollision(out PlayerController player))
        {
            // Track whether player is clearly above the box so the gate can arm.
            PlayerController candidate = Scene?.FindComponentOfType<PlayerController>();
            if (candidate != null)
            {
                float playerBottom = candidate.Entity.Position.Y
                    + (candidate.Entity.GetComponent<BoxCollider>()?.Bounds.Height ?? 0f);

                if (playerBottom < Position.Y)
                    _playerWasAbove = true;
            }
            return;
        }

        // ── Gate check ──────────────────────────────────────────────────────
        // Only kill when the player has been confirmed above first.
        if (!_playerWasAbove)
            return;

        // ── Instant kill ────────────────────────────────────────────────────
        // TODO: play sound: event:/game/general/assist_screenwrap
        player.TakeDamage(999, Vector2.Zero);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> and populates <paramref name="player"/> when the
    /// player's collider overlaps this killbox.
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
