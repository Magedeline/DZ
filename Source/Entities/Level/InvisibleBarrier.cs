using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using DZ.Entities.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's InvisibleBarrier.cs.
///
/// A solid that is invisible and initially non-collidable.  Each frame it
/// checks whether the player is overlapping it:
/// <list type="bullet">
///   <item>If no player overlap → become collidable (acts as a wall).</item>
///   <item>If player overlaps → become non-collidable (let player through).</item>
/// </list>
/// Once it turns non-collidable it deactivates itself for that frame so it
/// won't push the player.
///
/// Also contains a <see cref="ClimbBlocker"/> so players cannot wall-climb it.
/// </summary>
public class InvisibleBarrier : CelesteSolid
{
    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="InvisibleBarrier"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public InvisibleBarrier(Vector2 position, float width, float height)
        : base(position, width, height, safe: true)
    {
        Collidable = false;
        // Invisible barrier - no visual rendering needed
        Name       = "InvisibleBarrier";

        // Prevent wall-climbing on this barrier.
        AddComponent(new ClimbBlocker(edge: true));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        // Start each frame assuming we ARE collidable.
        Collidable = true;

        // Check for player overlap.
        if (Scene != null)
        {
            for (int _ibi = 0; _ibi < Scene.Entities.Count; _ibi++)
            {
                var e = Scene.Entities[_ibi];
                if (e is MadelinePlayer player)
                {
                    var pb = new RectangleF(
                        player.Position.X, player.Position.Y,
                        player.Width,      player.Height);

                    if (Bounds.Intersects(pb))
                    {
                        // Player overlaps → turn off so they can pass through.
                        Collidable = false;
                        break;
                    }
                }
            }
        }

        // If we ended up non-collidable this frame, skip further processing.
        // (No equivalent of Celeste's Active = false needed here)
    }
}
