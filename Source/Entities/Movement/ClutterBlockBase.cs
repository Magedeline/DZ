using Microsoft.Xna.Framework;
using Nez;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Movement;

/// <summary>
/// Port of Celeste's ClutterBlockBase.cs.
///
/// A coloured solid block that acts as the "floor" for a clutter puzzle room.
/// It can be enabled or disabled: when enabled it is fully collidable and drawn
/// dark; when disabled it becomes non-collidable and drawn with a lighter tint.
///
/// The <see cref="BlockColor"/> matches one of the <see cref="ClutterBlock.Colors"/>
/// categories so the game can enable/disable all bases of a given colour at once.
/// </summary>
public class ClutterBlockBase : CelesteSolid
{
    // ── Colours ───────────────────────────────────────────────────────────────

    private static readonly Color EnabledColor  = Color.Black * 0.7f;
    private static readonly Color DisabledColor = Color.Black * 0.3f;

    // ── State ─────────────────────────────────────────────────────────────────

    public ClutterBlock.Colors BlockColor { get; }

    private bool  _enabled;
    private Color _color;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="ClutterBlockBase"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="enabled">Initial enabled state.</param>
    /// <param name="blockColor">Colour category.</param>
    public ClutterBlockBase(
        Vector2             position,
        int                 width,
        int                 height,
        bool                enabled,
        ClutterBlock.Colors blockColor)
        : base(position, width, height, safe: true)
    {
        BlockColor = blockColor;
        _enabled   = enabled;
        _color     = enabled ? EnabledColor : DisabledColor;
        Name       = "ClutterBlockBase";

        if (!enabled)
            Collidable = false;
    }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Disables this base: makes it non-collidable and dims its rendering.
    /// </summary>
    public void Deactivate()
    {
        Collidable = false;
        _color     = DisabledColor;
        _enabled   = false;
    }

    /// <summary>
    /// Re-enables this base: restores collision and brightens rendering.
    /// </summary>
    public void Activate()
    {
        Collidable = true;
        _color     = EnabledColor;
        _enabled   = true;
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public void Render()
    {
        // Draw a filled rectangle; add 2 px to height when enabled (visual lip).
        float extraH = _enabled ? 2f : 0f;
        Graphics.Instance.Batcher.DrawRect(Position.X, Position.Y, Width, Height + extraH, _color);
    }
}
