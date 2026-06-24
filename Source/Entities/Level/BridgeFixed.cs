using Microsoft.Xna.Framework;
using Nez;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's BridgeFixed.cs.
///
/// A static solid bridge segment — 8 px tall, visually decorated with a
/// repeating texture.  Unlike <see cref="Bridge"/> it never collapses.
///
/// Sprite/texture loading is TODO; renders a placeholder brown rectangle.
/// </summary>
public class BridgeFixed : CelesteSolid
{
    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="BridgeFixed"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    public BridgeFixed(Vector2 position, float width)
        : base(position, width, 8f, safe: true)
    {
        Name = "BridgeFixed";
        // TODO: load "scenery/bridge_fixed" texture and tile images
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public void Render()
    {
        // Placeholder: brown rectangle at the bridge surface.
        Graphics.Instance.Batcher.DrawRect(Position.X, Position.Y, Width, Height, Color.SaddleBrown);
    }
}
