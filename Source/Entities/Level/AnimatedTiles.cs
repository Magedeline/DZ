using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using Camera = DZ.Nez.Camera;
using System;
using System.Collections.Generic;

namespace DZ.Entities.Level;

/// <summary>
/// Animated tile grid component.  Ported from Celeste's AnimatedTiles.cs.
///
/// Stores a 2-D grid of <see cref="TileEntry"/> objects, each referencing a named
/// animation from an <see cref="AnimatedTilesBank"/>.  Each tile independently
/// advances its frame counter so animations can start at random offsets.
///
/// Usage:
/// <code>
///   var tiles = entity.AddComponent(new AnimatedTiles(cols, rows, bank));
///   tiles.Set(x, y, "lava", scaleX: 1, scaleY: 1);
/// </code>
/// </summary>
public class AnimatedTiles : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public
    // -------------------------------------------------------------------------

    /// <summary>Optional camera used for frustum culling (null = render all).</summary>
    public Camera ClipCamera { get; set; }

    /// <summary>Local offset applied to tile-grid rendering origin.</summary>
    public Vector2 Position { get; set; } = Vector2.Zero;

    /// <summary>Tint colour applied to all tiles.</summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>Opacity multiplier applied on top of <see cref="Color"/>.</summary>
    public float Alpha { get; set; } = 1f;

    /// <summary>Animation bank that resolves animation names to frame data.</summary>
    public AnimatedTilesBank Bank { get; }

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private readonly int _columns;
    private readonly int _rows;

    /// <summary>
    /// Flat array of tile-lists, indexed [x + y * columns].
    /// null entries = empty cell.
    /// </summary>
    private readonly List<TileEntry>[] _tiles;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="columns">Width of the tile grid.</param>
    /// <param name="rows">Height of the tile grid.</param>
    /// <param name="bank">Animation bank that owns the frame data.</param>
    public AnimatedTiles(int columns, int rows, AnimatedTilesBank bank)
    {
        _columns = columns;
        _rows    = rows;
        Bank     = bank;
        _tiles   = new List<TileEntry>[columns * rows];
    }

    // -------------------------------------------------------------------------
    // Grid helpers
    // -------------------------------------------------------------------------

    private int Index(int x, int y) => x + y * _columns;

    private List<TileEntry> Get(int x, int y)
    {
        if (x < 0 || x >= _columns || y < 0 || y >= _rows) return null;
        return _tiles[Index(x, y)];
    }

    private List<TileEntry> GetOrCreate(int x, int y)
    {
        int idx = Index(x, y);
        _tiles[idx] ??= new List<TileEntry>();
        return _tiles[idx]!;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Assigns an animated tile to grid cell (<paramref name="x"/>, <paramref name="y"/>).
    /// Multiple animations can occupy the same cell.
    /// </summary>
    public void Set(int x, int y, string animationName, float scaleX = 1f, float scaleY = 1f)
    {
        if (string.IsNullOrEmpty(animationName)) return;
        var anim = Bank.GetAnimation(animationName);
        if (anim == null) return;

        GetOrCreate(x, y).Add(new TileEntry
        {
            AnimationName = animationName,
            Frame         = DZ.Nez.Random.NextInt(Math.Max(1, anim.FrameCount)),
            Scale         = new Vector2(scaleX, scaleY)
        });
    }

    // -------------------------------------------------------------------------
    // Culling helper
    // -------------------------------------------------------------------------

    /// <summary>Returns the visible tile rectangle (in tile coords), extended by <paramref name="extend"/>.</summary>
    public Rectangle GetClippedRenderTiles(int extend = 1)
    {
        if (ClipCamera == null)
            return new Rectangle(-extend, -extend, _columns + extend * 2, _rows + extend * 2);

        Vector2 origin = Entity.Position + Position;
        int x1 = (int)Math.Max(0, Math.Floor((ClipCamera.Bounds.Left   - origin.X) / 8.0) - extend);
        int y1 = (int)Math.Max(0, Math.Floor((ClipCamera.Bounds.Top    - origin.Y) / 8.0) - extend);
        int x2 = (int)Math.Min(_columns, Math.Ceiling((ClipCamera.Bounds.Right  - origin.X) / 8.0) + extend);
        int y2 = (int)Math.Min(_rows,    Math.Ceiling((ClipCamera.Bounds.Bottom - origin.Y) / 8.0) + extend);
        return new Rectangle(x1, y1, x2 - x1, y2 - y1);
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;
        var clip = GetClippedRenderTiles(1);

        for (int y = clip.Top; y < clip.Bottom; y++)
        for (int x = clip.Left; x < clip.Right; x++)
        {
            var list = Get(x, y);
            if (list == null) continue;
            foreach (var tile in list)
            {
                var anim = Bank.GetAnimation(tile.AnimationName);
                if (anim == null) continue;
                tile.Frame += dt / anim.Delay;
                if (tile.Frame >= anim.FrameCount)
                    tile.Frame -= anim.FrameCount;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    /// <summary>
    /// Renders animated tiles at <c>Entity.Position + Position</c>.
    /// Call from a custom renderer or override in a subclass.
    /// </summary>
    public void RenderAt(Vector2 origin)
    {
        var clip  = GetClippedRenderTiles(1);
        Color col = Color * Alpha;

        for (int y = clip.Top; y < clip.Bottom; y++)
        for (int x = clip.Left; x < clip.Right; x++)
        {
            var list = Get(x, y);
            if (list == null) continue;
            foreach (var tile in list)
            {
                var anim = Bank.GetAnimation(tile.AnimationName);
                if (anim == null) continue;
                int frame = (int)tile.Frame % Math.Max(1, anim.FrameCount);
                // TODO: draw anim.Frames[frame] at:
                //   origin + anim.Offset + new Vector2(x + 0.5f, y + 0.5f) * 8f
                //   with anim.Origin, col, tile.Scale
            }
        }
    }

    // -------------------------------------------------------------------------
    // Nested types
    // -------------------------------------------------------------------------

    /// <summary>Per-cell tile animation instance.</summary>
    private class TileEntry
    {
        public string AnimationName = string.Empty;
        public float Frame;
        public Vector2 Scale;
    }
}

// ---------------------------------------------------------------------------
// AnimatedTilesBank – minimal stub (full bank loaded from atlas/XML by game)
// ---------------------------------------------------------------------------

/// <summary>
/// Holds named animation definitions referenced by <see cref="AnimatedTiles"/>.
/// In the full game this is populated from level-map XML data.
/// </summary>
public class AnimatedTilesBank
{
    // -------------------------------------------------------------------------
    // Nested types
    // -------------------------------------------------------------------------

    public class AnimationDef
    {
        public string Name     = string.Empty;
        public float  Delay    = 0.08f;          // seconds per frame
        public int    FrameCount = 1;
        public Vector2 Offset  = Vector2.Zero;
        public Vector2 Origin  = Vector2.Zero;
        // TODO: MTexture[] Frames – populate from game atlas
    }

    // -------------------------------------------------------------------------
    // Registry
    // -------------------------------------------------------------------------

    private readonly Dictionary<string, AnimationDef> _animations = new();

    public void Register(AnimationDef def) => _animations[def.Name] = def;

    public AnimationDef GetAnimation(string name)
        => _animations.TryGetValue(name, out var def) ? def : null;
}
