using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using System;

namespace DZ.Entities.Core;

/// <summary>
/// Port of Celeste's Platform.cs.
///
/// Abstract base class for all solid surfaces (Solids, JumpThrus, moving platforms, etc.)
/// that can be ridden by actors and that carry subpixel movement state.
///
/// Key responsibilities:
/// - Subpixel accumulation via <see cref="movementCounter"/> so platforms move in
///   pixel-perfect integer steps while accumulating fractional remainder.
/// - <see cref="MoveH(float)"/> / <see cref="MoveV(float)"/> — accumulate then flush
///   to integer steps via the abstract <see cref="MoveHExact"/> / <see cref="MoveVExact"/>.
/// - <see cref="MoveToX"/> / <see cref="MoveToY"/> — move toward an absolute target position.
/// - <see cref="MoveHCollideSolids"/> / <see cref="MoveVCollideSolids"/> — move while
///   detecting collisions with other <see cref="CelesteSolid"/> instances.
/// - <see cref="LiftSpeed"/> — velocity imparted to riding <see cref="CelesteActor"/>s.
/// - Visual shake state (<see cref="StartShaking(float)"/> / <see cref="StopShaking()"/>).
/// - <see cref="OnDashCollide"/> delegate for dash interactions.
/// - <see cref="Safe"/> flag used by some game systems.
/// - <see cref="Bounds"/>, <see cref="Width"/>, <see cref="Height"/> geometry fields.
///
/// Subclasses must implement <see cref="MoveHExact"/> and <see cref="MoveVExact"/>.
/// </summary>
public abstract class CelestePlatform : DZ.Nez.Entity
{
    // -------------------------------------------------------------------------
    // Geometry
    // -------------------------------------------------------------------------

    /// <summary>Width of this platform in pixels.</summary>
    public float Width;

    /// <summary>Height of this platform in pixels.</summary>
    public float Height;

    /// <summary>
    /// Axis-aligned bounding rectangle for collision queries.
    /// Positioned so that <c>Position</c> is the top-left corner.
    /// Updated whenever <see cref="Width"/>, <see cref="Height"/>, or
    /// <see cref="Position"/> changes via <see cref="UpdateBounds"/>.
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

    /// <summary>The BoxCollider attached to this platform (mirrors Monocle).</summary>
    public BoxCollider? Collider => GetComponent<BoxCollider>();

    /// <summary>Whether this platform is visible (Monocle API parity).</summary>
    public bool Visible { get; set; } = true;

    // -------------------------------------------------------------------------
    // Physics state
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sub-pixel remainder accumulator (mirrors Celeste's <c>movementCounter</c>).
    /// Fractional movement is accumulated here; only integer pixels are actually
    /// applied to <see cref="DZ.Nez.Entity.Position"/>.
    /// </summary>
    protected Vector2 movementCounter;

    /// <summary>
    /// Velocity (pixels per second) imparted to any <see cref="CelesteActor"/>
    /// riding this platform — i.e. the platform's current world-space speed.
    /// </summary>
    public Vector2 LiftSpeed { get; protected set; }

    /// <summary>
    /// Target lift speed before the grace-timer smoothing applied by
    /// <see cref="SetLiftSpeed(Vector2)"/>.
    /// </summary>
    protected Vector2 liftSpeedTarget;

    /// <summary>
    /// Countdown timer for how long the current <see cref="LiftSpeed"/> value is
    /// maintained after the platform stops accelerating.
    /// </summary>
    protected float liftSpeedTimer;

    /// <summary>Seconds the LiftSpeed is held after the platform decelerates.</summary>
    protected const float LiftSpeedGraceTime = 0.16f;

    // -------------------------------------------------------------------------
    // Flags
    // -------------------------------------------------------------------------

    /// <summary>
    /// Whether this platform is "safe" (used by certain game systems;
    /// mirrors Celeste's Platform.Safe).
    /// </summary>
    public bool Safe;

    /// <summary>
    /// Whether StaticMovers are allowed on this platform (kept as a flag;
    /// StaticMover logic is not implemented in this port).
    /// </summary>
    public bool AllowStaticMovers = true;

    /// <summary>
    /// Whether this platform participates in collision detection.
    /// Actors and solids check this flag before reacting.
    /// </summary>
    public bool Collidable = true;

    // -------------------------------------------------------------------------
    // Visual shake
    // -------------------------------------------------------------------------

    /// <summary>Whether the platform is currently in a shake animation.</summary>
    public bool Shaking { get; private set; }

    private float _shakeTimer;

    /// <summary>
    /// Visual shake offset applied by the renderer each frame.
    /// Resets to zero when shaking stops.
    /// </summary>
    public Vector2 ShakeOffset { get; private set; }

    // -------------------------------------------------------------------------
    // Dash-collision callback
    // -------------------------------------------------------------------------

    /// <summary>
    /// Optional delegate called when a player dashes into this platform.
    /// Signature: <c>(Vector2 dashDirection) => DashCollisionResults</c>.
    /// Return value is ignored in this simplified port; keep it for API parity.
    /// </summary>
    public Action<Vector2>? OnDashCollide;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises the platform at <paramref name="position"/> with the given size.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    protected CelestePlatform(Vector2 position, float width, float height)
    {
        Position = position;
        Width    = width;
        Height   = height;
        UpdateBounds();
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <summary>Called when this entity is first added to the scene.</summary>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        UpdateBounds();
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    /// <summary>
    /// Base update — handles shake animation and LiftSpeed grace timer.
    /// Subclasses that override <see cref="Update"/> should call <c>base.Update()</c>.
    /// </summary>
    public override void Update()
    {
        float dt = Time.DeltaTime;

        // --- Shake animation ---
        if (Shaking)
        {
            _shakeTimer -= dt;
            if (_shakeTimer <= 0f)
            {
                StopShaking();
            }
            else
            {
                // Random sub-pixel offset for a visual wiggle effect.
                ShakeOffset = new Vector2(
                    DZ.Nez.Random.Range(-1, 2),
                    DZ.Nez.Random.Range(-1, 2));
            }
        }

        // --- LiftSpeed grace timer ---
        if (liftSpeedTimer > 0f)
        {
            liftSpeedTimer -= dt;
            if (liftSpeedTimer <= 0f)
            {
                liftSpeedTimer = 0f;
                LiftSpeed = Vector2.Zero;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Movement — public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Accumulates <paramref name="moveH"/> into the sub-pixel counter and
    /// flushes whole pixels via <see cref="MoveHExact"/>.
    /// </summary>
    public void MoveH(float moveH)
    {
        movementCounter.X += moveH;
        int pixels = (int)MathF.Round(movementCounter.X);
        if (pixels != 0)
        {
            movementCounter.X -= pixels;
            MoveHExact(pixels);
        }
    }

    /// <summary>
    /// Accumulates <paramref name="moveV"/> into the sub-pixel counter and
    /// flushes whole pixels via <see cref="MoveVExact"/>.
    /// </summary>
    public void MoveV(float moveV)
    {
        movementCounter.Y += moveV;
        int pixels = (int)MathF.Round(movementCounter.Y);
        if (pixels != 0)
        {
            movementCounter.Y -= pixels;
            MoveVExact(pixels);
        }
    }

    // -------------------------------------------------------------------------
    // Movement — target-position helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves horizontally so that <c>Position.X</c> reaches <paramref name="targetX"/>,
    /// adjusting the lift speed accordingly.
    /// </summary>
    public void MoveToX(float targetX, float liftSpeedOverride = 0f)
    {
        SetLiftSpeed(new Vector2(liftSpeedOverride, LiftSpeed.Y));
        MoveH(targetX - ExactPosition.X);
    }

    /// <summary>
    /// Moves vertically so that <c>Position.Y</c> reaches <paramref name="targetY"/>,
    /// adjusting the lift speed accordingly.
    /// </summary>
    public void MoveToY(float targetY, float liftSpeedOverride = 0f)
    {
        SetLiftSpeed(new Vector2(LiftSpeed.X, liftSpeedOverride));
        MoveV(targetY - ExactPosition.Y);
    }

    /// <summary>
    /// Moves to an absolute target position on both axes simultaneously.
    /// </summary>
    public void MoveTo(Vector2 target, Vector2 liftSpeedOverride = default)
    {
        SetLiftSpeed(liftSpeedOverride);
        MoveH(target.X - ExactPosition.X);
        MoveV(target.Y - ExactPosition.Y);
    }

    // -------------------------------------------------------------------------
    // Movement — solid-colliding variants
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves horizontally, invoking <paramref name="onCollide"/> if another
    /// <see cref="CelesteSolid"/> is encountered.  Returns true if a collision occurred.
    /// </summary>
    /// <param name="moveH">Amount to move in pixels (fractional).</param>
    /// <param name="onCollide">Optional callback on collision.</param>
    public bool MoveHCollideSolids(float moveH, Action<CelesteSolid>? onCollide = null)
    {
        movementCounter.X += moveH;
        int pixels = (int)MathF.Round(movementCounter.X);
        if (pixels == 0) return false;
        movementCounter.X -= pixels;

        int sign = Math.Sign(pixels);
        bool hit = false;
        while (pixels != 0)
        {
            var solid = CollideSolidAt(Position + new Vector2(sign, 0));
            if (solid != null)
            {
                hit = true;
                onCollide?.Invoke(solid);
                break;
            }
            Position += new Vector2(sign, 0);
            UpdateBounds();
            pixels -= sign;
        }
        return hit;
    }

    /// <summary>
    /// Moves vertically, invoking <paramref name="onCollide"/> if another
    /// <see cref="CelesteSolid"/> is encountered.  Returns true if a collision occurred.
    /// </summary>
    /// <param name="moveV">Amount to move in pixels (fractional).</param>
    /// <param name="onCollide">Optional callback on collision.</param>
    public bool MoveVCollideSolids(float moveV, Action<CelesteSolid>? onCollide = null)
    {
        movementCounter.Y += moveV;
        int pixels = (int)MathF.Round(movementCounter.Y);
        if (pixels == 0) return false;
        movementCounter.Y -= pixels;

        int sign = Math.Sign(pixels);
        bool hit = false;
        while (pixels != 0)
        {
            var solid = CollideSolidAt(Position + new Vector2(0, sign));
            if (solid != null)
            {
                hit = true;
                onCollide?.Invoke(solid);
                break;
            }
            Position += new Vector2(0, sign);
            UpdateBounds();
            pixels -= sign;
        }
        return hit;
    }

    // -------------------------------------------------------------------------
    // Abstract contract
    // -------------------------------------------------------------------------

    /// <summary>
    /// Move exactly <paramref name="move"/> pixels horizontally, pushing/carrying
    /// any riding actors. Implemented by concrete subclasses.
    /// </summary>
    public abstract void MoveHExact(int move);

    /// <summary>
    /// Move exactly <paramref name="move"/> pixels vertically, pushing/carrying
    /// any riding actors. Implemented by concrete subclasses.
    /// </summary>
    public abstract void MoveVExact(int move);

    // -------------------------------------------------------------------------
    // LiftSpeed
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets the lift speed and resets the grace timer so the speed is held for
    /// <see cref="LiftSpeedGraceTime"/> seconds after being set.
    /// </summary>
    public void SetLiftSpeed(Vector2 speed)
    {
        liftSpeedTarget = speed;
        LiftSpeed = speed;
        liftSpeedTimer = LiftSpeedGraceTime;
    }

    // -------------------------------------------------------------------------
    // Shake
    // -------------------------------------------------------------------------

    /// <summary>
    /// Begins a visual shake effect lasting <paramref name="time"/> seconds.
    /// </summary>
    public void StartShaking(float time = 0.5f)
    {
        Shaking = true;
        _shakeTimer = time;
    }

    /// <summary>Immediately stops the shake effect.</summary>
    public void StopShaking()
    {
        Shaking = false;
        _shakeTimer = 0f;
        ShakeOffset = Vector2.Zero;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// The exact floating-point position (same as <see cref="DZ.Nez.Entity.Position"/>),
    /// exposed for clarity when computing MoveToX/Y offsets.
    /// </summary>
    public Vector2 ExactPosition => Position;

    /// <summary>
    /// Recomputes <see cref="Bounds"/> from the current <see cref="DZ.Nez.Entity.Position"/>,
    /// <see cref="Width"/>, and <see cref="Height"/>.  Call after any position change.
    /// </summary>
    protected void UpdateBounds()
    {
        Bounds = new RectangleF(Position.X, Position.Y, Width, Height);
    }

    /// <summary>
    /// Queries the scene for a <see cref="CelesteSolid"/> that overlaps the given
    /// world-space rectangle, excluding <c>this</c> entity.
    /// Returns the first match or <c>null</c>.
    /// </summary>
    protected CelesteSolid? CollideSolidAt(Vector2 atPosition)
    {
        if (Scene == null) return null;
        var testBounds = new RectangleF(atPosition.X, atPosition.Y, Width, Height);
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
    /// Returns the <see cref="RectangleF"/> bounds that would apply if this entity
    /// were placed at <paramref name="atPosition"/>.
    /// </summary>
    protected RectangleF GetBoundsAt(Vector2 atPosition)
        => new RectangleF(atPosition.X, atPosition.Y, Width, Height);
}
