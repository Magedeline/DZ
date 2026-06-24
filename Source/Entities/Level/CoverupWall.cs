using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's CoverupWall.cs.
///
/// A wall of tiles that covers up something. Uses the tile autotiler
/// to match the existing tile style.
/// </summary>
public class CoverupWall : Component
{
    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private char _fillTile;
    private float _width;
    private float _height;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public CoverupWall(Vector2 position, char tile, float width, float height)
    {
        _fillTile = tile;
        _width = width;
        _height = height;
        // Depth = -13000; // TODO: Depth not available in Nez.Entity
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Set up collider
        var collider = Entity.AddComponent(new BoxCollider(_width, _height));
        collider.IsTrigger = true;

        // TODO: set up effect cutout

        // Generate tile grid
        GenerateTiles();
    }

    // -------------------------------------------------------------------------
    // Tile generation
    // -------------------------------------------------------------------------

    private void GenerateTiles()
    {
        int tilesX = (int)(_width / 8f);
        int tilesY = (int)(_height / 8f);

        // TODO: get level data
        // Level level = SceneAs<Level>();
        // Rectangle tileBounds = level.Session.MapData.TileBounds;
        // VirtualMap<char> solidsData = level.SolidsData;

        // int x = (int)Entity.X / 8 - tileBounds.Left;
        // int y = (int)Entity.Y / 8 - tileBounds.Top;

        // TODO: generate tile grid using GFX.FGAutotiler
        // var result = GFX.FGAutotiler.GenerateOverlay(_fillTile, x, y, tilesX, tilesY, solidsData);
        // Add(result.TileGrid);
        // Add(new TileInterceptor(result.TileGrid, false));
    }
}
