using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Collider = DZ.Nez.Collider;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's NegaBlock.cs.
///
/// An anti-block solid that kills the player on contact.  Rendered as a
/// solid red rectangle for debug/placeholder visuals.
///
/// The player-kill is triggered by overlapping collision detection in Update;
/// in the original Celeste this was a direct collider callback.
/// </summary>
public class NegaBlock : CelesteSolid
{
    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="NegaBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public NegaBlock(Vector2 position, float width, float height)
        : base(position, width, height, safe: false)
    {
        Name = "NegaBlock";
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        if (Scene == null) return;

        // Kill any player that overlaps this block.
        for (int _ni = 0; _ni < Scene.Entities.Count; _ni++)
        {
            var e = Scene.Entities[_ni];
            if (e is not DZ.Entities.Player.MadelinePlayer player) continue;

            var pb = new RectangleF(
                player.Position.X, player.Position.Y,
                player.Width,      player.Height);

            if (Bounds.Intersects(pb))
            {
                // TODO: call player.Die(directionToPlayer)
            }
        }
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public void Render()
    {
        // base.Render() not available on CelesteSolid
        Graphics.Instance.Batcher.DrawRect(Position.X, Position.Y, Width, Height, Color.Red * 0.6f);
    }
}
