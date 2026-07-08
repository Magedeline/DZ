using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Entities.Core;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's TempleCrackedBlock.cs.
///
/// A solid block found in Chapter 5 (Mirror Temple) that plays a destruction
/// animation when <see cref="Break"/> is called, then removes itself.
///
/// The animation cycles through a sequence of frames at 15 fps.
/// Tile-frame atlas is TODO (renders as a grey block until broken).
/// </summary>
public class TempleCrackedBlock : CelesteSolid
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float BreakAnimFps = 15f;
    private const int   FrameCount   = 8;   // placeholder; real count from atlas

    // ── Configuration ─────────────────────────────────────────────────────────

    private readonly bool   _persistent;
    private readonly string _entityId;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool  _broken;
    private float _frame;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="TempleCrackedBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="persistent">
    ///   When <c>true</c>, breaking is remembered across room reloads.
    /// </param>
    /// <param name="entityId">Unique entity ID string (for persistence).</param>
    public TempleCrackedBlock(
        Vector2 position,
        float   width,
        float   height,
        bool    persistent,
        string  entityId = "")
        : base(position, width, height, safe: true)
    {
        _persistent = persistent;
        _entityId   = entityId;
        Collidable  = false;
        Name        = "TempleCrackedBlock";
        // TODO: load atlas sub-textures "objects/DZ/DZ/temple/breakBlock"
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // If a player is already inside, treat as already broken.
        if (Scene != null)
        {
            for (int _tci = 0; _tci < Scene.Entities.Count; _tci++)
            {
                var e = Scene.Entities[_tci];
                if (e is DZ.Entities.Player.MadelinePlayer p)
                {
                    var pb = new RectangleF(
                        p.Position.X, p.Position.Y,
                        p.Width,      p.Height);
                    if (Bounds.Intersects(pb))
                    {
                        // TODO: persist if _persistent
                        Destroy();
                        return;
                    }
                }
            }
        }

        Collidable = true;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        if (!_broken) return;

        _frame += Time.DeltaTime * BreakAnimFps;
        if (_frame >= FrameCount)
            Destroy();
    }

    // ── Render ────────────────────────────────────────────────────────────────

    // TODO: Render via RenderableComponent

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts the break animation and spawns debris.
    /// </summary>
    /// <param name="from">World position the break was triggered from (for debris direction).</param>
    public void Break(Vector2 from)
    {
        // TODO: persist if _persistent via _entityId
        // TODO: play "crackedwall_vanish" audio
        _broken    = true;
        Collidable = false;

        int tilesX = (int)(Width  / 8);
        int tilesY = (int)(Height / 8);
        for (int tx = 0; tx < tilesX; tx++)
        {
            for (int ty = 0; ty < tilesY; ty++)
            {
                // TODO: spawn Debris at (Position + (tx*8+4, ty*8+4)) blasted from `from`
            }
        }
    }
}
