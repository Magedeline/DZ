using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's Bridge.cs (Chapter 1 — Prologue).
///
/// A bridge composed of individual <see cref="BridgeTile"/> entities.
/// When the player passes a certain X threshold the bridge begins to collapse:
/// tiles fall one by one from left to right (and later from the right end).
///
/// The gap between <see cref="_gapStartX"/> and <see cref="_gapEndX"/> is
/// left empty from the start.
///
/// Audio, music cue, and exact tile-size atlas data are omitted (TODO).
/// </summary>
public class Bridge : Nez.Entity
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float CollapseInterval = 0.2f; // seconds between individual tile drops

    // ── Configuration ─────────────────────────────────────────────────────────

    private readonly int   _width;
    private float          _gapStartX;
    private float          _gapEndX;

    // ── State ─────────────────────────────────────────────────────────────────

    private List<BridgeTile> _tiles          = new();
    private bool             _canCollapse    = false;
    private bool             _canEndCollapseA = true;
    private bool             _canEndCollapseB = true;
    private float            _collapseTimer;
    private bool             _ending;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Bridge"/>.
    /// </summary>
    /// <param name="position">Left-end world position.</param>
    /// <param name="width">Total bridge width in pixels.</param>
    /// <param name="gapStartX">X coordinate (world) where the pre-existing gap starts.</param>
    /// <param name="gapEndX">X coordinate (world) where the pre-existing gap ends.</param>
    public Bridge(Vector2 position, int width, float gapStartX, float gapEndX)
        : base()
    {
        Position   = position;
        _width     = width;
        _gapStartX = gapStartX;
        _gapEndX   = gapEndX;
        Name       = "Bridge";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        SpawnTiles();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        MadelinePlayer? player = null;
        if (Scene != null)
            for (int _bi = 0; _bi < Scene.Entities.Count; _bi++)
                if (Scene.Entities[_bi] is MadelinePlayer p) { player = p; break; }

        if (player == null || player.Dead) return;

        if (!_canCollapse)
        {
            // Start collapsing once player passes the trigger point.
            if (player.Position.X >= Position.X + 112f)
            {
                _canCollapse = true;
                _canEndCollapseA = true;
                _canEndCollapseB = true;

                // TODO: change music to bridge theme
                // TODO: play bridge rumble sfx

                // Drop first 11 tiles immediately.
                int toDrop = System.Math.Min(11, _tiles.Count);
                for (int i = 0; i < toDrop; i++)
                {
                    _tiles[0].Fall(Nez.Random.Range(0.1f, 0.5f));
                    _tiles.RemoveAt(0);
                }
            }
        }
        else if (_tiles.Count > 0)
        {
            // End-game tile drops.
            if (_canEndCollapseA && player.Position.X > Position.X + _width - 216f)
            {
                _canEndCollapseA = false;
                int toDrop = System.Math.Min(5, _tiles.Count - 8 > 0 ? 5 : 0);
                for (int i = 0; i < toDrop && _tiles.Count > 8; i++)
                {
                    _tiles[_tiles.Count - 8].Fall(Nez.Random.Range(0.1f, 0.5f));
                    _tiles.RemoveAt(_tiles.Count - 8);
                }
            }
            else if (_canEndCollapseB && player.Position.X > Position.X + _width - 104f)
            {
                _canEndCollapseB = false;
                for (int i = 0; i < 7 && _tiles.Count > 0; i++)
                {
                    _tiles[_tiles.Count - 1].Fall(Nez.Random.Range(0.1f, 0.3f));
                    _tiles.RemoveAt(_tiles.Count - 1);
                }
            }
            else if (_collapseTimer > 0f)
            {
                _collapseTimer -= Time.DeltaTime;
                if (_tiles.Count >= 5 && player.Position.X >= _tiles[4].Position.X)
                {
                    _tiles[0].Fall();
                    _tiles.RemoveAt(0);
                }
            }
            else
            {
                _tiles[0].Fall();
                _tiles.RemoveAt(0);
                _collapseTimer = CollapseInterval;
            }
        }
        else
        {
            if (!_ending)
            {
                _ending = true;
                // TODO: stop collapse SFX loop
            }
        }
    }

    // ── Tile spawning ─────────────────────────────────────────────────────────

    private void SpawnTiles()
    {
        if (Scene == null) return;

        const int tileW = 8;
        float x = Position.X;

        while (x < Position.X + _width)
        {
            if (x < _gapStartX || x >= _gapEndX)
            {
                var tile = new BridgeTile(new Vector2(x, Position.Y), new Microsoft.Xna.Framework.Rectangle(0, 0, tileW, 52));
                _tiles.Add(tile);
                Scene.AddEntity(tile);
            }
            x += tileW;
        }
    }
}
