using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's CrumbleWallOnRumble.cs.
///
/// A solid tile-wall that crumbles (breaks apart into debris) when a rumble
/// event is triggered — typically by the game's earthquake/boss sequence.
///
/// Breaking:
/// <list type="bullet">
///   <item>Sets <see cref="CelesteSolid.Collidable"/> to <c>false</c>.</item>
///   <item>Spawns debris for each 8 × 8 tile cell that isn't blocked by another solid.</item>
///   <item>Optionally marks itself as "do not load again" (persistent).</item>
/// </list>
///
/// Tile-grid visual and autotiler integration are TODO.
/// Debris pooling is TODO (placeholder Draw.Rect in render until broken).
/// </summary>
public class CrumbleWallOnRumble : CelesteSolid
{
    // ── Configuration ─────────────────────────────────────────────────────────

    private readonly char   _tileType;
    private readonly bool   _blendIn;
    private readonly bool   _persistent;
    private readonly string _entityId;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool _broken = false;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="CrumbleWallOnRumble"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="tileType">Tile character for visual / sound selection.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="blendIn">Whether to blend into the surrounding tileset visually.</param>
    /// <param name="persistent">Whether breaking persists across room reloads.</param>
    /// <param name="entityId">Unique entity ID string (used for persistence).</param>
    public CrumbleWallOnRumble(
        Vector2 position,
        char    tileType,
        float   width,
        float   height,
        bool    blendIn,
        bool    persistent,
        string  entityId = "")
        : base(position, width, height, safe: true)
    {
        _tileType   = tileType;
        _blendIn    = blendIn;
        _persistent = persistent;
        _entityId   = entityId;
        Name        = "CrumbleWallOnRumble";
        // TODO: generate tile-grid visual via autotiler
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // If a player is already overlapping us, remove immediately.
        if (Scene != null)
        {
            for (int _cri = 0; _cri < Scene.Entities.Count; _cri++)
            {
                var e = Scene.Entities[_cri];
                if (e is KirbyCelesteStandalone.Entities.Player.MadelinePlayer p)
                {
                    var pb = new RectangleF(
                        p.Position.X, p.Position.Y,
                        p.Width,      p.Height);
                    if (Bounds.Intersects(pb))
                    {
                        Destroy();
                        return;
                    }
                }
            }
        }
    }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Triggers the crumble-break: disables collision, spawns debris, optionally
    /// marks entity as persistent.
    /// </summary>
    public void Break()
    {
        if (_broken || !Collidable || Scene == null) return;

        // TODO: play "quake_rockbreak" audio
        Collidable = false;
        _broken    = true;

        // Spawn debris for each unblocked 8-px tile cell.
        int tilesX = (int)(Width  / 8);
        int tilesY = (int)(Height / 8);
        for (int tx = 0; tx < tilesX; tx++)
        {
            for (int ty = 0; ty < tilesY; ty++)
            {
                var cellRect = new RectangleF(
                    Position.X + tx * 8,
                    Position.Y + ty * 8, 8f, 8f);

                // Only spawn debris in open cells.
                bool blocked = false;
                for (int _crj = 0; _crj < Scene.Entities.Count; _crj++)
                {
                    if (Scene.Entities[_crj] is CelesteSolid solid && solid != this && solid.Collidable
                        && solid.Bounds.Intersects(cellRect))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                {
                    // TODO: spawn Debris entity at (Position + (tx*8+4, ty*8+4), _tileType)
                    // blasted from TopCenter
                }
            }
        }

        // TODO: if persistent, record in session DoNotLoad list via _entityId
        Destroy();
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public void Render()
    {
        if (_broken) return;
        // TODO: render tile-grid
        Graphics.Instance.Batcher.DrawRect(Position.X, Position.Y, Width, Height, Color.Gray * 0.8f);
    }
}
