using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using System;
using System.Collections.Generic;
using DZ.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Core;

/// <summary>
/// Port of Celeste's Solid.cs.
///
/// Extends <see cref="CelestePlatform"/> to represent a fully-blocking solid surface
/// that can push and carry <see cref="CelesteActor"/> instances as it moves.
///
/// Key behaviours:
/// <list type="bullet">
///   <item><see cref="Speed"/> — velocity in px/s; integrated each frame by
///         <see cref="Update"/> via <see cref="CelestePlatform.MoveH(float)"/> /
///         <see cref="CelestePlatform.MoveV(float)"/>.</item>
///   <item><see cref="MoveHExact"/> / <see cref="MoveVExact"/> — move the solid
///         pixel-by-pixel, carrying riding actors and squishing actors that cannot
///         escape.</item>
///   <item><see cref="GetRiders()"/> — returns all <see cref="CelesteActor"/>s that
///         are currently standing on top of or otherwise riding this solid.</item>
///   <item><see cref="HasPlayerRider"/> / <see cref="GetPlayerRider"/> — convenience
///         helpers scoped to <see cref="PlayerController"/> specifically.</item>
///   <item><see cref="AllowStaticMovers"/> — flag kept for API parity; StaticMover
///         logic is not implemented.</item>
/// </list>
///
/// Nez port notes:
/// <list type="bullet">
///   <item>Inherits from <see cref="CelestePlatform"/>, which in turn inherits from
///         <see cref="DZ.Nez.Entity"/>.</item>
///   <item>Collision detection uses direct scene entity iteration (no Monocle Tracker).</item>
///   <item>Audio, GFX, SaveData and Tracker references have all been removed.</item>
/// </list>
/// </summary>
public class CelesteSolid : CelestePlatform
{
    // -------------------------------------------------------------------------
    // Velocity
    // -------------------------------------------------------------------------

    /// <summary>
    /// Current velocity of this solid in pixels per second.
    /// Integrated by <see cref="Update"/> each frame.
    /// </summary>
    public Vector2 Speed;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new solid at <paramref name="position"/> with the given size.
    /// The solid is collidable and safe by default.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="safe">Whether the solid counts as "safe" ground.</param>
    public CelesteSolid(Vector2 position, float width, float height, bool safe = true)
        : base(position, width, height)
    {
        Safe    = safe;
        Collidable = true;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Register a BoxCollider so Nez's broadphase system is aware of this solid.
        var collider = AddComponent(new BoxCollider(0f, 0f, Width, Height));
        collider.PhysicsLayer = PhysicsLayers.Solid;
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    /// <summary>
    /// Integrates <see cref="Speed"/> each frame, moving the solid and pushing/
    /// carrying any riding <see cref="CelesteActor"/>s.
    /// </summary>
    public override void Update()
    {
        base.Update(); // handles shake + LiftSpeed timer

        float dt = Time.DeltaTime;

        // Impart our current velocity to riders as LiftSpeed.
        SetLiftSpeed(Speed);

        // Move horizontally then vertically.
        if (Speed.X != 0f)
            MoveH(Speed.X * dt);

        if (Speed.Y != 0f)
            MoveV(Speed.Y * dt);
    }

    // -------------------------------------------------------------------------
    // CelestePlatform — exact movement implementation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves the solid exactly <paramref name="move"/> pixels horizontally.
    ///
    /// Algorithm (mirrors Celeste):
    /// <list type="number">
    ///   <item>Collect all current riders.</item>
    ///   <item>Temporarily disable collisions so actors don't get stuck on the
    ///         solid itself during movement.</item>
    ///   <item>Apply the full pixel move to the solid.</item>
    ///   <item>For each rider, move it the same amount horizontally.
    ///         If the rider can't move (blocked by another solid), squish it.</item>
    ///   <item>Re-enable collisions.</item>
    ///   <item>Push any non-rider actors that now overlap the solid.</item>
    /// </list>
    /// </summary>
    public override void MoveHExact(int move)
    {
        if (Scene == null) return;

        // Snapshot riders before moving.
        var riders = GetRiders();

        // Disable this solid's collision so actors don't hit it while it teleports.
        Collidable = false;

        // Move the solid.
        Position += new Vector2(move, 0f);
        UpdateBounds();

        // Re-enable.
        Collidable = true;

        int sign = Math.Sign(move);

        // Carry / push actors.
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            if (Scene.Entities[i] is not CelesteActor actor) continue;
            if (!actor.Collidable) continue;

            if (riders.Contains(actor))
            {
                // Carry: rider moves with us.
                actor.SetLiftSpeed(LiftSpeed);
                actor.MoveH(move, squished =>
                {
                    // The rider was blocked — squish it.
                    actor.Squish(new CollisionData(new Vector2(sign, 0f), this));
                });
            }
            else
            {
                // Push: actor overlaps us after move — shove it out.
                if (Bounds.Intersects(actor.Bounds) && actor.AllowPushing)
                {
                    // Determine how far we need to push.
                    float pushDist = sign > 0
                        ? Bounds.Right - actor.Bounds.Left
                        : actor.Bounds.Right - Bounds.Left;

                    actor.SetLiftSpeed(LiftSpeed);
                    actor.MoveH((int)(pushDist + 0.5f) * sign, squished =>
                    {
                        actor.Squish(new CollisionData(new Vector2(sign, 0f), this));
                    });
                }
            }
        }
    }

    /// <summary>
    /// Moves the solid exactly <paramref name="move"/> pixels vertically.
    ///
    /// Same algorithm as <see cref="MoveHExact"/> but on the vertical axis.
    /// When moving downward, riders are pushed if they are between the solid and
    /// the floor; when moving upward they are carried.
    /// </summary>
    public override void MoveVExact(int move)
    {
        if (Scene == null) return;

        var riders = GetRiders();

        Collidable = false;

        Position += new Vector2(0f, move);
        UpdateBounds();

        Collidable = true;

        int sign = Math.Sign(move);

        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            if (Scene.Entities[i] is not CelesteActor actor) continue;
            if (!actor.Collidable) continue;

            if (riders.Contains(actor))
            {
                // Carry the rider.
                actor.SetLiftSpeed(LiftSpeed);
                actor.MoveV(move, squished =>
                {
                    actor.Squish(new CollisionData(new Vector2(0f, sign), this));
                });
            }
            else
            {
                // Push if overlapping.
                if (Bounds.Intersects(actor.Bounds) && actor.AllowPushing)
                {
                    float pushDist = sign > 0
                        ? Bounds.Bottom - actor.Bounds.Top
                        : actor.Bounds.Bottom - Bounds.Top;

                    actor.SetLiftSpeed(LiftSpeed);
                    actor.MoveV((int)(pushDist + 0.5f) * sign, squished =>
                    {
                        actor.Squish(new CollisionData(new Vector2(0f, sign), this));
                    });
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Rider queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a <see cref="HashSet{T}"/> of all <see cref="CelesteActor"/>s that
    /// are currently riding this solid (i.e. standing on top of it or flagged as
    /// riding via <see cref="CelesteActor.IsRiding(CelesteSolid)"/>).
    /// </summary>
    public HashSet<CelesteActor> GetRiders()
    {
        var riders = new HashSet<CelesteActor>();
        if (Scene == null) return riders;

        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            if (Scene.Entities[i] is CelesteActor actor && actor.IsRiding(this))
                riders.Add(actor);
        }

        return riders;
    }

    /// <summary>
    /// Returns true if at least one entity carrying a <see cref="PlayerController"/>
    /// component is riding this solid right now.
    /// The entity must also be a <see cref="CelesteActor"/> for <c>IsRiding</c> to apply.
    /// </summary>
    public bool HasPlayerRider()
    {
        if (Scene == null) return false;
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            var entity = Scene.Entities[i];
            if (entity is CelesteActor actor
                && actor.GetComponent<PlayerController>() != null
                && actor.IsRiding(this))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the <see cref="PlayerController"/> component of the first entity that is
    /// riding this solid, or <c>null</c> if no player rider exists.
    /// </summary>
    public PlayerController? GetPlayerRider()
    {
        if (Scene == null) return null;
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            var entity = Scene.Entities[i];
            if (entity is CelesteActor actor && actor.IsRiding(this))
            {
                var pc = actor.GetComponent<PlayerController>();
                if (pc != null) return pc;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns true if any <see cref="CelesteActor"/> (not necessarily the player)
    /// is riding this solid.
    /// </summary>
    public bool HasRider()
    {
        if (Scene == null) return false;
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            if (Scene.Entities[i] is CelesteActor actor && actor.IsRiding(this))
                return true;
        }
        return false;
    }

    // -------------------------------------------------------------------------
    // Convenience — solid manipulation helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Immediately teleports the solid to <paramref name="targetPosition"/> (no
    /// rider carry; use this for initialisation only).
    /// </summary>
    public void SetPosition(Vector2 targetPosition)
    {
        Position = targetPosition;
        UpdateBounds();
    }

    /// <summary>
    /// Returns true if this solid's bounds intersect the given world-space rectangle.
    /// </summary>
    public bool OverlapsRect(RectangleF rect) => Bounds.Intersects(rect);

    // -------------------------------------------------------------------------
    // Movement-entity helper methods
    // Used by subclasses in the DZ.Entities.Movement namespace.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Finds the <see cref="PlayerController"/> in the current scene, or
    /// <c>null</c> when no scene is loaded or no player entity exists.
    /// </summary>
    protected PlayerController? GetPlayer()
        => Scene?.FindComponentOfType<PlayerController>();

    /// <summary>
    /// Returns the axis-aligned bounds of the player's <see cref="BoxCollider"/>.
    /// Falls back to a zero-size rectangle at the player's position when no
    /// collider component is present.
    /// </summary>
    protected static RectangleF GetPlayerBounds(PlayerController player)
    {
        var col = player.Entity.GetComponent<BoxCollider>();
        return col?.Bounds ?? new RectangleF(player.Entity.Position.X, player.Entity.Position.Y, 0f, 0f);
    }

    /// <summary>
    /// Returns <c>true</c> when the player is standing directly on top of this
    /// solid: their bottom edge is within 2 px of the solid's top edge and they
    /// overlap horizontally.
    /// </summary>
    protected bool IsPlayerRiding(PlayerController player)
    {
        var pb = GetPlayerBounds(player);
        return pb.Bottom >= Bounds.Top - 2f
            && pb.Bottom <= Bounds.Top + 2f
            && pb.Right  >  Bounds.Left
            && pb.Left   <  Bounds.Right;
    }

    /// <summary>
    /// Returns <c>true</c> when the player's bounds overlap this solid's bounds.
    /// </summary>
    protected bool IsPlayerOverlapping(PlayerController player)
        => Bounds.Intersects(GetPlayerBounds(player));
}
