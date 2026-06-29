using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Entities.Core;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's WaterFall.cs.
///
/// A thin (8 px wide) waterfall that falls from its Y position downward until
/// it hits a <see cref="Water"/> surface or a <see cref="CelesteSolid"/>.
/// The effective height is computed in <see cref="OnAddedToScene"/> by scanning
/// tiles below.
///
/// Renders as a pair of surface-coloured edge strips with a fill column in
/// between.  When the bottom lands on a Water surface the fill draws shorter
/// to match the surface ripple.
///
/// Audio and displacement rendering are TODO.
/// </summary>
public class WaterFall : DZ.Nez.Entity
{
    // ── State ─────────────────────────────────────────────────────────────────

    private float        _height;
    private Water?       _water;
    private bool         _hitsWater;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="WaterFall"/> at <paramref name="position"/>.
    /// </summary>
    public WaterFall(Vector2 position)
    {
        Position = position;
        Name     = "WaterFall";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        ComputeHeight();
        // TODO: play looping waterfall SFX
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        // Ripple the water surface periodically.
        if (_water?.TopSurface != null && Scene != null)
        {
            // Emit a ripple roughly every 0.3 s (use a simple timer via scene time).
            // Simplified: ripple every frame (damped by Water.Surface internals).
            _water.TopSurface.DoRipple(new Vector2(Position.X + 4f, _water.Position.Y), 0.75f);
        }

        // TODO: emit splash particles at bottom when hitting water/solid
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public override void Render()
    {
        var batcher = Graphics.Instance.Batcher;
        if (_water?.TopSurface == null)
        {
            // No water below: draw full fill + edges.
            batcher.DrawRect(Position.X + 1f, Position.Y, 6f, _height, Water.FillColor);
            batcher.DrawRect(Position.X - 1f, Position.Y, 2f, _height, Water.SurfaceColor);
            batcher.DrawRect(Position.X + 7f, Position.Y, 2f, _height, Water.SurfaceColor);
        }
        else
        {
            Water.Surface top = _water.TopSurface;
            float totalH = _height + top.Position.Y - _water.Position.Y;

            for (int i = 0; i < 6; i++)
            {
                float surfH = top.GetSurfaceHeight(new Vector2(Position.X + 1f + i, _water.Position.Y));
                batcher.DrawRect(Position.X + 1 + i, Position.Y, 1f, totalH - surfH, Water.FillColor);
            }

            float edgeSurfL = top.GetSurfaceHeight(new Vector2(Position.X,      _water.Position.Y));
            float edgeSurfR = top.GetSurfaceHeight(new Vector2(Position.X + 8f, _water.Position.Y));
            batcher.DrawRect(Position.X - 1f, Position.Y, 2f, totalH - edgeSurfL, Water.SurfaceColor);
            batcher.DrawRect(Position.X + 7f, Position.Y, 2f, totalH - edgeSurfR, Water.SurfaceColor);
        }
    }

    // ── Height computation ────────────────────────────────────────────────────

    private void ComputeHeight()
    {
        if (Scene == null) { _height = 8f; return; }

        float sceneBoundsBottom = float.MaxValue;
        for (int _ei = 0; _ei < Scene.Entities.Count; _ei++)
        {
            var e = Scene.Entities[_ei];
            if (e.Position.Y > Position.Y)
                sceneBoundsBottom = Math.Min(sceneBoundsBottom, e.Position.Y + 1000f);
        }
        if (sceneBoundsBottom == float.MaxValue) sceneBoundsBottom = Position.Y + 800f;

        _height = 8f;
        _water  = null;

        while (Position.Y + _height < sceneBoundsBottom)
        {
            var testRect = new RectangleF(
                Position.X, Position.Y + _height, 8f, 8f);

            // Check for water.
            for (int _wi = 0; _wi < Scene.Entities.Count; _wi++)
            {
                if (Scene.Entities[_wi] is Water w)
                {
                    var wb = new RectangleF(
                        w.Position.X, w.Position.Y,
                        w.BodyWidth,  w.BodyHeight);
                    if (wb.Intersects(testRect)) { _water = w; goto done; }
                }
            }

            // Check for solid.
            for (int _si = 0; _si < Scene.Entities.Count; _si++)
            {
                if (Scene.Entities[_si] is CelesteSolid solid && solid.Collidable
                    && solid.Bounds.Intersects(testRect))
                    goto done;
            }

            _height += 8f;
        }

        done:;
        _hitsWater = _water != null;
    }
}
