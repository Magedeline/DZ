using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's FakeWall.cs.
///
/// A breakable fake wall that fades out when the player touches it.
/// Can be either a wall (matches existing tiles) or a block (autotiled box).
/// </summary>
public class FakeWall : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Enum
    // -------------------------------------------------------------------------

    public enum Modes
    {
        Wall,
        Block
    }

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>The mode of the fake wall (Wall or Block).</summary>
    public Modes Mode { get; private set; }

    /// <summary>Whether the wall is currently fading out.</summary>
    public bool Fading => _fading;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private char _fillTile;
    private float _width;
    private float _height;
    private string _entityId;
    private bool _fading;
    private bool _transitionFade;
    private float _transitionStartAlpha;
    private float _alpha = 1f;
    private bool _playRevealWhenTransitionedInto;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public FakeWall(Vector2 position, string entityId, char tile, float width, float height, Modes mode, bool playRevealWhenTransitionedInto = false)
    {
        Mode = mode;
        _entityId = entityId;
        _fillTile = tile;
        _width = width;
        _height = height;
        _playRevealWhenTransitionedInto = playRevealWhenTransitionedInto;
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

        // TODO: set up effect cutout

        // Generate tiles
        GenerateTiles();

        // Check if player is already inside (transitioned in)
        if (CheckPlayerInside() != null)
        {
            _alpha = 0f;
            _fading = true;
            // TODO: cutout.Visible = false;

            if (_playRevealWhenTransitionedInto)
            {
                // TODO: play sound: event:/game/general/secret_revealed
            }

            // TODO: mark as DoNotLoad
        }
        else
        {
            // Add transition listeners
            // TODO: Add(new TransitionListener { ... });
        }
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

        if (Mode == Modes.Wall)
        {
            // Rectangle tileBounds = level.Session.MapData.TileBounds;
            // VirtualMap<char> solidsData = level.SolidsData;
            // int x = (int)Entity.X / 8 - tileBounds.Left;
            // int y = (int)Entity.Y / 8 - tileBounds.Top;
            // TODO: tiles = GFX.FGAutotiler.GenerateOverlay(_fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;
        }
        else if (Mode == Modes.Block)
        {
            // TODO: tiles = GFX.FGAutotiler.GenerateBox(_fillTile, tilesX, tilesY).TileGrid;
        }

        // TODO: Add(tiles);
        // TODO: Add(new TileInterceptor(tiles, false));
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        if (_fading)
        {
            // Fade out
            _alpha = Calc.Approach(_alpha, 0f, 2f * dt);
            // TODO: cutout.Alpha = _alpha;
            // TODO: tiles.Alpha = _alpha;

            if (_alpha <= 0f)
            {
                Entity.Destroy();
            }
        }
        else
        {
            // Check for player collision
            var player = CheckPlayerInside();
            if (player != null)
            {
                // TODO: mark as DoNotLoad
                _fading = true;
                // TODO: play sound: event:/game/general/secret_revealed
            }
        }
    }

    // -------------------------------------------------------------------------
    // Collision detection
    // -------------------------------------------------------------------------

    private MadelinePlayer? CheckPlayerInside()
    {
        var collider = Entity.GetComponent<BoxCollider>();
        if (collider == null) return null;

        var rect = new RectangleF(
            Entity.Position.X + collider.LocalOffset.X,
            Entity.Position.Y + collider.LocalOffset.Y,
            collider.Bounds.Width,
            collider.Bounds.Height);

        // TODO: Physics.OverlapRectangleAll not available - using stub
        // var hits = Physics.OverlapRectangleAll(rect, null);
        // foreach (var hit in hits)
        // {
        //     if (hit.Entity == Entity) continue;
        //
        //     var player = hit.Entity.GetComponent<MadelinePlayer>();
        //     if (player != null) return player;
        // }

        // Check for player using scene entities
        var player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player != null)
        {
            var playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null && rect.Intersects(playerCollider.Bounds))
                return player;
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    // TODO: Render via RenderableComponent

    // -------------------------------------------------------------------------
    // Transition handlers
    // -------------------------------------------------------------------------

    private void OnTransitionOutBegin()
    {
        // Check if this wall is in the level bounds
        // TODO: Scene cast to Level type
        // var level = Entity.Scene as Level;
        // if (level == null) return;

        // TODO: check if in bounds
        // if (Collide.CheckRect(this, level.Bounds))
        {
            _transitionFade = true;
            _transitionStartAlpha = _alpha;
        }
        // else
        // {
        //     _transitionFade = false;
        // }
    }

    private void OnTransitionOut(float percent)
    {
        if (!_transitionFade) return;
        // TODO: tiles.Alpha = _transitionStartAlpha * (1f - percent);
        // TODO: cutout.Alpha = _transitionStartAlpha * (1f - percent);
    }

    private void OnTransitionInBegin()
    {
        // TODO: Scene cast to Level type
        // var level = Entity.Scene as Level;
        // if (level == null) return;

        // TODO: check if was in previous bounds
        // if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
        {
            _transitionFade = true;
            _alpha = 0f;
        }
        // else
        // {
        //     _transitionFade = false;
        // }
    }

    private void OnTransitionIn(float percent)
    {
        if (!_transitionFade) return;
        // TODO: tiles.Alpha = percent;
        // TODO: cutout.Alpha = percent;
    }
}

/// <summary>
/// Extension for MadelinePlayer to check dream dash state.
/// </summary>
public static class MadelinePlayerExtensions
{
    public static bool IsDreamDashing(this MadelinePlayer player)
    {
        // TODO: return player.StateMachine.State == StDreamDash
        return false;
    }
}
