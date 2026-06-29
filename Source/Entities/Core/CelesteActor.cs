using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using System;
using DZ.Core;

namespace DZ.Entities.Core;

/// <summary>
/// Port of Celeste's Actor.cs.
///
/// Abstract base class for all "actor" entities — things that move through the world
/// and interact with <see cref="CelesteSolid"/> and <see cref="CelesteJumpThru"/>
/// platforms. The canonical subclass in this project is <see cref="PlayerController"/>.
///
/// Design notes (Nez port):
/// <list type="bullet">
///   <item>Inherits from <see cref="DZ.Nez.Entity"/> directly — it IS the scene object.</item>
///   <item>Sub-pixel movement is accumulated in <see cref="movementCounter"/> so the
///         actor always moves in whole pixels while keeping fractional precision.</item>
///   <item>Collision detection iterates scene entities rather than using Monocle's
///         Tracker; see <see cref="CollideSolid(Vector2)"/> and related helpers.</item>
///   <item>A <see cref="DZ.Nez.BoxCollider"/> component is added automatically in
///         <see cref="OnAddedToScene"/>; subclasses should set <see cref="Width"/>
///         and <see cref="Height"/> before that point (e.g., in their constructor).</item>
///   <item>All <c>Audio.Play</c>, <c>GFX</c>, <c>SaveData</c>, and Tracker references
///         from the original source have been removed.</item>
/// </list>
/// </summary>
public abstract class CelesteActor : DZ.Nez.Entity
{
    // -------------------------------------------------------------------------
    // Geometry
    // -------------------------------------------------------------------------

    /// <summary>Width of this actor's hitbox in pixels.</summary>
    public float Width;

    /// <summary>Height of this actor's hitbox in pixels.</summary>
    public float Height;

    /// <summary>
    /// Axis-aligned bounding rectangle used for all manual collision queries.
    /// Positioned so that <c>Position</c> is the top-left corner of the hitbox.
    /// Updated automatically by <see cref="UpdateBounds"/>.
    /// </summary>
    public RectangleF Bounds { get; protected set; }

    // -------------------------------------------------------------------------
    // Celeste geometry helpers (mirrors Monocle.Entity)
    // -------------------------------------------------------------------------

    /// <summary>Render / update depth. Maps to Nez <see cref="DZ.Nez.Entity.UpdateOrder"/>.</summary>
    public int Depth
    {
        get => UpdateOrder;
        set => UpdateOrder = value;
    }

    /// <summary>Centre point of the hitbox.</summary>
    public Vector2 Center => Position + new Vector2(Width * 0.5f, Height * 0.5f);

    /// <summary>X co-ordinate of the centre of the hitbox.</summary>
    public float CenterX => Position.X + Width * 0.5f;

    /// <summary>Y co-ordinate of the centre of the hitbox.</summary>
    public float CenterY => Position.Y + Height * 0.5f;

    /// <summary>Left edge of the hitbox (same as <c>Position.X</c>).</summary>
    public float Left => Position.X;

    /// <summary>Right edge of the hitbox.</summary>
    public float Right => Position.X + Width;

    /// <summary>Top edge of the hitbox (same as <c>Position.Y</c>).</summary>
    public float Top => Position.Y;

    /// <summary>Bottom edge of the hitbox.</summary>
    public float Bottom => Position.Y + Height;

    // -------------------------------------------------------------------------
    // Sub-pixel movement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sub-pixel remainder accumulator (mirrors Celeste's Actor.movementCounter).
    /// MoveH/MoveV add to this; only whole-pixel steps are committed to Position.
    /// </summary>
    protected Vector2 movementCounter;

    // -------------------------------------------------------------------------
    // LiftSpeed (velocity from riding a moving platform)
    // -------------------------------------------------------------------------

    /// <summary>
    /// The velocity (px/s) imparted to this actor by a moving platform it is
    /// currently riding. Mirrors Celeste's <c>LiftSpeed</c>.
    /// Decays to zero after <see cref="LiftSpeedGraceTime"/> seconds.
    /// </summary>
    public Vector2 LiftSpeed { get; private set; }

    private float _liftSpeedTimer;

    /// <summary>
    /// Seconds that <see cref="LiftSpeed"/> is preserved after the riding platform
    /// stops pushing the actor (mirrors Celeste's liftSpeedTimer grace window).
    /// </summary>
    public const float LiftSpeedGraceTime = 0.16f;

    // -------------------------------------------------------------------------
    // Behaviour flags
    // -------------------------------------------------------------------------

    /// <summary>
    /// When true the actor passes through <see cref="CelesteJumpThru"/> platforms
    /// (e.g. while falling through one deliberately).
    /// </summary>
    public bool IgnoreJumpThrus;

    /// <summary>
    /// When true, solids are permitted to push this actor when they move into it.
    /// Set to false to prevent a solid from squishing the actor (used by some
    /// cutscene dummies in the original).
    /// </summary>
    public bool AllowPushing = true;

    /// <summary>
    /// When true, MoveH/MoveV use a simplified "naive" path that does not check
    /// for solid collisions pixel-by-pixel (mirrors Celeste's Actor.TreatNaive).
    /// </summary>
    public bool TreatNaive;

    /// <summary>
    /// Whether this actor participates in physics / collision tests.
    /// Mirrors Celeste's Entity.Collidable.
    /// </summary>
    public bool Collidable = true;

    // -------------------------------------------------------------------------
    // Squish callback
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when a solid tries to crush this actor and there is no room to
    /// dodge. The default implementation tries to wiggle the actor free; if that
    /// fails it calls <see cref="DZ.Nez.Entity.Destroy"/>.
    /// Assign a replacement to override death behaviour.
    /// </summary>
    public Action SquishCallback;

    // -------------------------------------------------------------------------
    // Nez BoxCollider (added automatically)
    // -------------------------------------------------------------------------

    /// <summary>The Nez BoxCollider component attached to this entity.</summary>
    public BoxCollider Collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new actor at <paramref name="position"/> with the given hitbox size.
    /// </summary>
    /// <param name="position">Top-left world position of the hitbox.</param>
    /// <param name="width">Hitbox width in pixels.</param>
    /// <param name="height">Hitbox height in pixels.</param>
    protected CelesteActor(Vector2 position, float width, float height)
    {
        Position = position;
        Width    = width;
        Height   = height;
        UpdateBounds();
        SquishCallback = DefaultSquish;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by Nez when the entity is added to a scene.
    /// Creates the <see cref="Collider"/> component sized to Width × Height.
    /// </summary>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // BoxCollider(x, y, w, h) — offset so the collider top-left == entity position.
        Collider = AddComponent(new BoxCollider(0, 0, Width, Height));
        Collider.PhysicsLayer = PhysicsLayers.Actor;

        UpdateBounds();
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    /// <summary>
    /// Base update — decays <see cref="LiftSpeed"/> grace timer.
    /// Subclasses that override <see cref="Update"/> should call <c>base.Update()</c>
    /// or handle the timer themselves.
    /// </summary>
    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Decay LiftSpeed grace timer.
        if (_liftSpeedTimer > 0f)
        {
            _liftSpeedTimer -= dt;
            if (_liftSpeedTimer <= 0f)
            {
                _liftSpeedTimer = 0f;
                LiftSpeed = Vector2.Zero;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Horizontal movement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Accumulates <paramref name="moveH"/> into the sub-pixel counter and
    /// flushes whole pixels to <see cref="MoveHExact"/>.
    /// </summary>
    /// <param name="moveH">Fractional horizontal movement in pixels.</param>
    /// <param name="onCollide">Optional callback invoked if a <see cref="CelesteSolid"/> is hit.</param>
    public void MoveH(float moveH, Action<CelesteSolid> onCollide = null)
    {
        if (TreatNaive)
        {
            Position += new Vector2(moveH, 0f);
            UpdateBounds();
            return;
        }

        movementCounter.X += moveH;
        int pixels = (int)MathF.Round(movementCounter.X);
        if (pixels != 0)
        {
            movementCounter.X -= pixels;
            MoveHExact(pixels, onCollide);
        }
    }

    /// <summary>
    /// Accumulates <paramref name="moveV"/> into the sub-pixel counter and
    /// flushes whole pixels to <see cref="MoveVExact"/>.
    /// </summary>
    /// <param name="moveV">Fractional vertical movement in pixels.</param>
    /// <param name="onCollide">Optional callback invoked if a <see cref="CelesteSolid"/> is hit.</param>
    public void MoveV(float moveV, Action<CelesteSolid> onCollide = null)
    {
        if (TreatNaive)
        {
            Position += new Vector2(0f, moveV);
            UpdateBounds();
            return;
        }

        movementCounter.Y += moveV;
        int pixels = (int)MathF.Round(movementCounter.Y);
        if (pixels != 0)
        {
            movementCounter.Y -= pixels;
            MoveVExact(pixels, onCollide);
        }
    }

    // -------------------------------------------------------------------------
    // Exact (pixel-by-pixel) movement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves exactly <paramref name="move"/> pixels horizontally, checking for
    /// <see cref="CelesteSolid"/> collisions one pixel at a time.
    ///
    /// If a solid is encountered the actor stops, the <see cref="CelesteSolid"/>'s
    /// squish logic runs, and <paramref name="onCollide"/> is invoked.
    /// </summary>
    public void MoveHExact(int move, Action<CelesteSolid> onCollide = null)
    {
        int sign = Math.Sign(move);
        while (move != 0)
        {
            var solid = CollideSolid(new Vector2(sign, 0));
            if (solid != null)
            {
                onCollide?.Invoke(solid);
                break;
            }

            // Check jump-thrus — actors can walk off them horizontally without issue.
            Position += new Vector2(sign, 0f);
            UpdateBounds();
            move -= sign;
        }
    }

    /// <summary>
    /// Moves exactly <paramref name="move"/> pixels vertically, checking for
    /// <see cref="CelesteSolid"/> collisions one pixel at a time.
    ///
    /// When moving downward also checks for <see cref="CelesteJumpThru"/> platforms
    /// (unless <see cref="IgnoreJumpThrus"/> is set).
    /// </summary>
    public void MoveVExact(int move, Action<CelesteSolid> onCollide = null)
    {
        int sign = Math.Sign(move);
        while (move != 0)
        {
            // Check solid collision.
            var solid = CollideSolid(new Vector2(0, sign));
            if (solid != null)
            {
                onCollide?.Invoke(solid);
                break;
            }

            // When descending check one-way platforms.
            if (sign > 0 && !IgnoreJumpThrus)
            {
                var jt = CollideJumpThru(new Vector2(0, sign));
                if (jt != null)
                {
                    // Land on top of the jump-thru.
                    break;
                }
            }

            Position += new Vector2(0f, sign);
            UpdateBounds();
            move -= sign;
        }
    }

    // -------------------------------------------------------------------------
    // Ground / riding queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns true if the actor is standing on a <see cref="CelesteSolid"/> or
    /// (unless <see cref="IgnoreJumpThrus"/> is set) a <see cref="CelesteJumpThru"/>.
    /// </summary>
    public bool OnGround()
    {
        // Check solids directly below.
        if (CollideSolid(new Vector2(0, 1)) != null)
            return true;

        // Check jump-thrus below.
        if (!IgnoreJumpThrus && CollideJumpThru(new Vector2(0, 1)) != null)
            return true;

        return false;
    }

    /// <summary>
    /// Returns true if the actor is standing on the given <see cref="CelesteSolid"/>
    /// (i.e. the solid is directly below and the actor overlaps it horizontally).
    /// </summary>
    public bool IsRiding(CelesteSolid solid)
    {
        if (!solid.Collidable) return false;
        // The actor's bottom edge must be at the solid's top edge.
        var below = GetBoundsAt(Position + new Vector2(0, 1));
        return below.Intersects(solid.Bounds);
    }

    /// <summary>
    /// Returns true if the actor is standing on the given <see cref="CelesteJumpThru"/>.
    /// </summary>
    public bool IsRiding(CelesteJumpThru jumpThru)
    {
        if (IgnoreJumpThrus) return false;
        if (!jumpThru.Collidable) return false;
        var below = GetBoundsAt(Position + new Vector2(0, 1));
        return below.Intersects(jumpThru.Bounds);
    }

    // -------------------------------------------------------------------------
    // LiftSpeed API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets <see cref="LiftSpeed"/> and resets the grace timer.
    /// Called by <see cref="CelesteSolid"/> and <see cref="CelesteJumpThru"/>
    /// when they carry this actor.
    /// </summary>
    public void SetLiftSpeed(Vector2 speed)
    {
        LiftSpeed = speed;
        _liftSpeedTimer = LiftSpeedGraceTime;
    }

    // -------------------------------------------------------------------------
    // Squish / crush handling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by a <see cref="CelesteSolid"/> when it tries to push this actor
    /// but there is no free space to move to.  Invokes <see cref="SquishCallback"/>.
    /// </summary>
    public virtual void Squish(CollisionData data)
    {
        SquishCallback?.Invoke();
    }

    /// <summary>
    /// Default squish implementation: tries to wiggle the actor up to 3 pixels in
    /// each cardinal direction to escape the crush.  If no escape is found the actor
    /// is destroyed.
    /// </summary>
    protected virtual void DefaultSquish()
    {
        // Try to find a free spot within ±3 pixels.
        for (int dx = -3; dx <= 3; dx++)
        {
            for (int dy = -3; dy <= 3; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var offset = new Vector2(dx, dy);
                if (CollideSolid(offset) == null)
                {
                    Position += offset;
                    UpdateBounds();
                    return;
                }
            }
        }

        // No escape — destroy.
        Destroy();
    }

    // -------------------------------------------------------------------------
    // Collision helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the first <see cref="CelesteSolid"/> whose bounds intersect
    /// <c>Bounds + offset</c>, or <c>null</c> if none.
    /// </summary>
    protected CelesteSolid CollideSolid(Vector2 offset)
    {
        if (Scene == null) return null;
        var testBounds = GetBoundsAt(Position + offset);
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            var entity = Scene.Entities[i];
            if (entity == this) continue;
            if (entity is CelesteSolid solid && solid.Collidable && solid.Bounds.Intersects(testBounds))
                return solid;
        }
        return null;
    }

    /// <summary>
    /// Returns the first <see cref="CelesteJumpThru"/> whose bounds intersect
    /// <c>Bounds + offset</c>, respecting one-way directionality (only land from above).
    /// Returns <c>null</c> if none.
    /// </summary>
    protected CelesteJumpThru CollideJumpThru(Vector2 offset)
    {
        if (Scene == null || IgnoreJumpThrus) return null;
        var testBounds = GetBoundsAt(Position + offset);
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            var entity = Scene.Entities[i];
            if (entity == this) continue;
            if (entity is CelesteJumpThru jt && jt.Collidable)
            {
                // One-way: actor must be coming from above the jump-thru's top edge.
                // The actor's bottom (before offset) must be at or above jt.Bounds.Top.
                float actorBottom = Bounds.Bottom;
                if (actorBottom <= jt.Bounds.Top + 1f && testBounds.Intersects(jt.Bounds))
                    return jt;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks whether the actor (placed at <paramref name="atPosition"/>) overlaps
    /// any <see cref="CelesteSolid"/>.  Useful for find-free-space searches.
    /// </summary>
    protected bool OverlapsSolid(Vector2 atPosition)
        => CollideSolid(atPosition - Position) != null;

    // -------------------------------------------------------------------------
    // Bounds helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Recomputes <see cref="Bounds"/> from current <see cref="DZ.Nez.Entity.Position"/>,
    /// <see cref="Width"/>, and <see cref="Height"/>.
    /// </summary>
    protected void UpdateBounds()
    {
        Bounds = new RectangleF(Position.X, Position.Y, Width, Height);
    }

    /// <summary>
    /// Returns the <see cref="RectangleF"/> that would apply if the entity were at
    /// <paramref name="atPosition"/> (top-left).
    /// </summary>
    protected RectangleF GetBoundsAt(Vector2 atPosition)
        => new RectangleF(atPosition.X, atPosition.Y, Width, Height);
}

// -------------------------------------------------------------------------
// Collision data struct (mirrors Celeste's CollisionData)
// -------------------------------------------------------------------------

/// <summary>
/// Minimal collision data passed to <see cref="CelesteActor.Squish"/> so the
/// callback can inspect what caused the squish.
/// </summary>
public struct CollisionData
{
    /// <summary>The direction the actor was being pushed when it was squished.</summary>
    public Vector2 Direction;

    /// <summary>The solid that caused the squish (may be null).</summary>
    public CelesteSolid Pusher;

    public CollisionData(Vector2 direction, CelesteSolid pusher)
    {
        Direction = direction;
        Pusher    = pusher;
    }
}

// -------------------------------------------------------------------------
// Physics layer constants (project-wide convenience)
// -------------------------------------------------------------------------

/// <summary>
/// Bit-flag constants for Nez physics layers used throughout this project.
/// Mirrors the project's physics layer assignments defined elsewhere.
/// </summary>
public static class PhysicsLayers
{
    /// <summary>Layer for player and NPC actors.</summary>
    public const int Actor   = 1 << 0;

    /// <summary>Layer for solid surfaces (CelesteSolid).</summary>
    public const int Solid   = 1 << 1;

    /// <summary>Layer for one-way platforms (CelesteJumpThru).</summary>
    public const int JumpThru = 1 << 2;

    /// <summary>Layer for hazards (spikes, lava, etc.).</summary>
    public const int Hazard  = 1 << 3;
}
