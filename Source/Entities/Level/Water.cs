using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using static DZ.Nez.Time;
using System;
using System.Collections.Generic;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's Water.cs.
///
/// A body of water that the player can swim in.  It maintains a top and/or
/// bottom <see cref="Surface"/> that ripple when entities enter or leave.
///
/// Simplified from the original:
/// - WaterInteraction component tracking preserved (enter/exit ripple triggers).
/// - Displacement rendering, GameplayRenderer.Begin/End, and audio omitted (TODO).
/// - Surface wave mesh rendering is simplified to a line.
/// </summary>
public class Water : DZ.Nez.Entity
{
    // ── Colours ───────────────────────────────────────────────────────────────

    public static readonly Color FillColor    = Color.LightSkyBlue * 0.3f;
    public static readonly Color SurfaceColor = Color.LightSkyBlue * 0.8f;
    public static readonly Color RayTopColor  = Color.LightSkyBlue * 0.6f;

    // ── Surfaces ──────────────────────────────────────────────────────────────

    public Surface?       TopSurface;
    public Surface?       BottomSurface;
    public List<Surface>  Surfaces = new();

    // ── Dimensions ────────────────────────────────────────────────────────────

    public float BodyWidth  { get; }
    public float BodyHeight { get; }

    // ── Internal ──────────────────────────────────────────────────────────────

    private readonly HashSet<WaterInteraction> _contains = new();
    private readonly Microsoft.Xna.Framework.Rectangle _fill;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Water"/> body.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="topSurface">Whether to add a rippling top surface.</param>
    /// <param name="bottomSurface">Whether to add a rippling bottom surface.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public Water(Vector2 position, bool topSurface, bool bottomSurface, float width, float height)
    {
        Position    = position;
        BodyWidth   = width;
        BodyHeight  = height;
        Name        = "Water";

        int surfaceH = 8;
        int fillX    = 0;
        int fillY    = 0;
        int fillW    = (int)width;
        int fillH    = (int)height;

        if (topSurface)
        {
            TopSurface = new Surface(
                position + new Vector2(width / 2f, surfaceH),
                new Vector2(0f, -1f), width);
            Surfaces.Add(TopSurface);
            fillY += surfaceH;
            fillH -= surfaceH;
        }

        if (bottomSurface)
        {
            BottomSurface = new Surface(
                position + new Vector2(width / 2f, height - surfaceH),
                new Vector2(0f, 1f), width);
            Surfaces.Add(BottomSurface);
            fillH -= surfaceH;
        }

        _fill = new Microsoft.Xna.Framework.Rectangle(fillX, fillY, fillW, fillH);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        foreach (var surface in Surfaces)
            surface.Update();

        if (Scene == null) return;

        // Track entities entering/leaving the water volume.
        for (int _ei = 0; _ei < Scene.Entities.Count; _ei++)
        {
            var e = Scene.Entities[_ei];
            var wi = e.GetComponent<WaterInteraction>();
            if (wi == null) continue;

            bool wasIn = _contains.Contains(wi);
            bool isIn  = Overlaps(e);

            if (wasIn == isIn) continue;

            // Trigger ripple on the appropriate surface.
            if (e.Position.Y <= Position.Y + BodyHeight / 2f && TopSurface != null)
                TopSurface.DoRipple(e.Position, 1f);
            else if (e.Position.Y > Position.Y + BodyHeight / 2f && BottomSurface != null)
                BottomSurface.DoRipple(e.Position, 1f);

            if (isIn)
            {
                _contains.Add(wi);
                // TODO: play "water_in" or "water_dash_in" audio
                wi.DrippingTimer = 0f;
            }
            else
            {
                _contains.Remove(wi);
                // TODO: play "water_out" or "water_dash_out" audio
                wi.DrippingTimer = 2f;
            }
        }
    }

    // ── Render ────────────────────────────────────────────────────────────────

    // TODO: Render via RenderableComponent

    // ── Overlap helper ────────────────────────────────────────────────────────

    private bool Overlaps(DZ.Nez.Entity e)
    {
        var wb = new Microsoft.Xna.Framework.Rectangle(
            (int)Position.X, (int)Position.Y, (int)BodyWidth, (int)BodyHeight);
        var eb = new Microsoft.Xna.Framework.Rectangle(
            (int)e.Position.X, (int)e.Position.Y, 8, 8); // rough
        return wb.Intersects(eb);
    }

    // ── Surface inner class ───────────────────────────────────────────────────

    /// <summary>
    /// Simplified port of Celeste's Water.Surface — tracks a set of ripples.
    /// </summary>
    public class Surface
    {
        public Vector2      Position;
        public Vector2      Outwards;
        public float        BodyWidth;
        public List<Ripple> Ripples = new();

        private float _timer;

        public Surface(Vector2 position, Vector2 outwards, float bodyWidth)
        {
            Position  = position;
            Outwards  = outwards;
            BodyWidth = bodyWidth;
        }

        public void Update()
        {
            _timer += Time.DeltaTime;
            for (int i = Ripples.Count - 1; i >= 0; i--)
            {
                var r = Ripples[i];
                r.Percent += Time.DeltaTime / r.Duration;
                if (r.Percent >= 1f)
                    Ripples.RemoveAt(i);
            }
        }

        /// <summary>Spawns a new ripple centred near <paramref name="worldPos"/>.</summary>
        public void DoRipple(Vector2 worldPos, float strength)
        {
            Ripples.Add(new Ripple
            {
                Position = worldPos.X - Position.X + BodyWidth / 2f,
                Speed    = 100f * strength,
                Height   = 8f  * strength,
                Percent  = 0f,
                Duration = 0.8f,
            });
        }

        /// <summary>Gets the wave height at a world-space X coordinate.</summary>
        public float GetSurfaceHeight(Vector2 worldPos) => 0f; // simplified
    }

    // ── Ripple data ───────────────────────────────────────────────────────────

    public class Ripple
    {
        public float Position;
        public float Speed;
        public float Height;
        public float Percent;
        public float Duration;
    }
}
