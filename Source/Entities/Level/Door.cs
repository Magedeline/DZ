using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Collider = DZ.Nez.Collider;
using System;
using System.Collections.Generic;
using System.Linq;
using DZ.Entities.Player;
using DZ.Entities.Core;
using CollisionData = DZ.Entities.Core.CollisionData;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's Door.cs.
///
/// A simple door that opens when the player touches it. Can be wood or metal.
/// Opens in the direction away from the player.
/// </summary>
public class Door : CelesteActor
{
    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>The type of door (wood or metal).</summary>
    public string Type { get; private set; }

    /// <summary>Whether the door is currently disabled (stuck in a wall).</summary>
    public bool Disabled { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private string _openSfx;
    private string _closeSfx;
    private bool _wasCollidingWithSolid;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Door(Vector2 position, string type = "wood") : base(position, 12f, 22f)
    {
        Type = type;
        // Depth = 8998; // TODO: Depth not available in DZ.Nez.Entity

        if (type == "wood")
        {
            _openSfx = "event:/game/03_resort/door_wood_open";
            _closeSfx = "event:/game/03_resort/door_wood_close";
        }
        else
        {
            _openSfx = "event:/game/03_resort/door_metal_open";
            _closeSfx = "event:/game/03_resort/door_metal_close";
        }
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Position collider at feet
        if (Collider != null)
        {
            Collider.SetLocalOffset(new Vector2(0f, -23f));
        }

        // TODO: load sprite based on type
        // TODO: play idle animation
        // TODO: add light occlude
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        // Check if colliding with solid
        bool collidingWithSolid = false;
        foreach (var solid in Scene.FindEntitiesWithTag(0).OfType<CelesteSolid>())
        {
            if (solid.GetType() == this.GetType()) continue;
            if (solid.GetComponent<Collider>()?.Collides(Collider) ?? false)
            {
                collidingWithSolid = true;
                break;
            }
        }

        if (collidingWithSolid && !Disabled)
        {
            Disabled = true;
        }

        _wasCollidingWithSolid = collidingWithSolid;

        // Check for player collision
        if (!Disabled)
        {
            CheckPlayerCollision();
        }
    }

    // -------------------------------------------------------------------------
    // Interaction
    // -------------------------------------------------------------------------

    private void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        if (Collider?.Collides(player.GetComponent<Collider>()) ?? false)
        {
            Open(player.Position.X);
        }
    }

    /// <summary>
    /// Opens the door in the direction away from the given X position.
    /// </summary>
    public void Open(float fromX)
    {
        // TODO: check if already in open animation
        // if (sprite.CurrentAnimationID == "idle")
        // {
        //     TODO: play sound: _openSfx
        //     TODO: sprite.Play("open");
        //     TODO: sprite.Scale.X = MathF.Sign(fromX - Position.X);
        // }
        // else if (sprite.CurrentAnimationID == "close")
        // {
        //     TODO: sprite.Play("close", true); // Play in reverse
        // }
    }

    /// <summary>
    /// Closes the door.
    /// </summary>
    public void Close()
    {
        // TODO: play close animation
        // TODO: play sound: _closeSfx when animation completes
    }

    protected void OnSquish(CollisionData data)
    {
        // Doors don't die when squished
    }

    public new bool IsRiding(CelesteSolid solid)
    {
        // TODO: Physics.OverlapCircle not available - using stub
        // return Physics.OverlapCircle(Position, 2f, (int)PhysicsLayers.Solid).Any(c => c.Entity.GetComponent<CelesteSolid>() == solid);
        return false;
    }
}

/// <summary>
/// Extension methods for collision detection.
/// </summary>
public static class DoorCollisionExtensions
{
    public static bool Collides(this Collider a, Collider? b)
    {
        if (a == null || b == null) return false;
        return a.Bounds.Intersects(b.Bounds);
    }
}
