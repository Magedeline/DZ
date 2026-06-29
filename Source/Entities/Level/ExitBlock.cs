using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Collider = DZ.Nez.Collider;
using System;
using DZ.Entities.Player;
using DZ.Entities.Core;



namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's ExitBlock.cs.
///
/// A block that appears when the player leaves the area. Fades in when
/// the player is not touching it. Used to seal off areas the player
/// has already passed through.
/// </summary>
public class ExitBlock : CelesteSolid
{
    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private char _tileType;
    private float _alpha;
    private bool _transitionFade;
    private float _transitionStartAlpha;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public ExitBlock(Vector2 position, float width, float height, char tileType)
        : base(position, width, height, safe: true)
    {
        _tileType = tileType;
        // Depth = -13000; // TODO: Depth not available in DZ.Nez.Entity
        Collidable = false; // Start non-collidable, fade in when player leaves
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // TODO: set up transition listener
        // OnOutBegin = OnTransitionOutBegin,
        // OnInBegin = OnTransitionInBegin

        // TODO: set up effect cutout
        // TODO: set surface sound index based on tile type

        // Generate tile grid
        GenerateTiles();

        // Check if player is inside on spawn (transitioned into level)
        if (CheckPlayerInside())
        {
            _alpha = 0f;
            // TODO: cutout.Alpha = 0f;
            Collidable = false;
        }
    }

    // -------------------------------------------------------------------------
    // Tile generation
    // -------------------------------------------------------------------------

    private void GenerateTiles()
    {
        int tilesX = (int)(Width / 8f);
        int tilesY = (int)(Height / 8f);

        // TODO: get level data
        // Level level = SceneAs<Level>();
        // Rectangle tileBounds = level.Session.MapData.TileBounds;
        // VirtualMap<char> solidsData = level.SolidsData;

        int x = (int)(Position.X / 8f); // - tileBounds.Left;
        int y = (int)(Position.Y / 8f); // - tileBounds.Top;

        // TODO: generate tile grid using GFX.FGAutotiler
        // tiles = GFX.FGAutotiler.GenerateOverlay(_tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
        // TODO: Add(tiles);
        // TODO: Add(new TileInterceptor(tiles, false));
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        float dt = Time.DeltaTime;

        if (Collidable)
        {
            // Fade in when collidable
            _alpha = Calc.Approach(_alpha, 1f, dt);
            // TODO: tiles.Alpha = _alpha;
        }
        else
        {
            // Fade out when not collidable (player inside)
            if (!CheckPlayerInside())
            {
                // Player left - make collidable
                Collidable = true;
                // TODO: play sound: event:/game/general/passage_closed_behind
            }
            else
            {
                _alpha = Calc.Approach(_alpha, 0f, dt);
                // TODO: tiles.Alpha = _alpha;
                // TODO: cutout.Alpha = _alpha;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Collision detection
    // -------------------------------------------------------------------------

    private bool CheckPlayerInside()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return false;

        // Check if player collider intersects this block
        var playerCollider = player.GetComponent<Collider>();
        if (playerCollider == null) return false;

        var playerRect = new RectangleF(
            player.Position.X + playerCollider.LocalOffset.X,
            player.Position.Y + playerCollider.LocalOffset.Y,
            playerCollider.Bounds.Width,
            playerCollider.Bounds.Height);

        var blockCollider = GetComponent<Collider>();
        if (blockCollider == null) return false;

        var blockRect = new RectangleF(
            Position.X + blockCollider.LocalOffset.X,
            Position.Y + blockCollider.LocalOffset.Y,
            blockCollider.Bounds.Width,
            blockCollider.Bounds.Height);

        return playerRect.Intersects(blockRect);
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
        // Check if this block is in the level bounds
        // TODO: if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
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
        // TODO: cutout.Alpha = _alpha = MathHelper.Lerp(_transitionStartAlpha, 0f, percent);
        // TODO: cutout.Update();
    }

    private void OnTransitionInBegin()
    {
        var level = Scene as Scene;
        if (level == null) return;

        // TODO: check if was in previous bounds
        // if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
        {
            if (CheckPlayerInside())
            {
                _transitionFade = true;
                _alpha = 0f;
            }
        }
        // else
        // {
        //     _transitionFade = false;
        // }
    }

    private void OnTransitionIn(float percent)
    {
        if (!_transitionFade) return;
        // TODO: cutout.Alpha = _alpha = percent;
        // TODO: cutout.Update();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static class Calc
    {
        public static float Approach(float val, float target, float maxMove)
        {
            return val > target ? Math.Max(val - maxMove, target) : Math.Min(val + maxMove, target);
        }
    }
}
