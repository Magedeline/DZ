using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Collectibles;

/// <summary>
/// Port of Celeste's Glider.cs.
///
/// Jellyfish/glider holdable that slows the player's fall when held.
/// Can be destroyed by seeker barriers. Has bubble variant that floats.
/// </summary>
public class Glider : CelesteActor, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const float Gravity = 200f;
    private const float MaxFall = 30f;
    private const float HighFrictionTime = 0.5f;
    private const float BubbleGravity = 0f;

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Current velocity of the glider.</summary>
    public Vector2 Speed;

    /// <summary>Whether this is a bubble variant (floats in place).</summary>
    public bool IsBubble { get; private set; }

    /// <summary>Whether the glider is being held by the player.</summary>
    public bool IsHeld { get; private set; }

    /// <summary>Whether the glider has been destroyed.</summary>
    public bool IsDestroyed => _destroyed;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private MadelinePlayer? _holder;
    private Vector2 _spawnPosition;
    private Vector2 _prevLiftSpeed;
    private float _noGravityTimer;
    private float _highFrictionTimer;
    private bool _destroyed;
    private bool _tutorial;
    private float _platformSineTimer;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Glider(Vector2 position, bool bubble = false, bool tutorial = false) : base(position, 8f, 10f)
    {
        _spawnPosition = position;
        IsBubble = bubble;
        _tutorial = tutorial;
        // Depth = -5; // TODO: not supported in Nez
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
            Collider.SetLocalOffset(new Vector2(0f, -10f));
        }

        // TODO: load sprite "glider"
        // TODO: set up platform sine wave for visual bobbing

        if (_tutorial)
        {
            // TODO: add tutorial GUI
        }
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        if (_destroyed) return;

        // Emit glow particles
        // TODO: if (Scene.OnInterval(0.05f)) emit particles

        // Update rotation based on holder movement
        if (IsHeld && _holder != null)
        {
            UpdateRotation();
        }

        // Check for seeker barrier collision
        CheckSeekerBarrier();

        if (IsHeld)
        {
            _prevLiftSpeed = Vector2.Zero;
            return;
        }

        if (IsBubble)
        {
            // Bubble just floats in place
            _platformSineTimer += Time.DeltaTime;
            float sineOffset = MathF.Sin(_platformSineTimer * 2f) * 2f;
            // TODO: sprite.Y = sineOffset;
            return;
        }

        // Apply physics
        ApplyPhysics();

        // Move with collision
        MoveH(Speed.X * Time.DeltaTime, OnCollideH);
        MoveV(Speed.Y * Time.DeltaTime, OnCollideV);

        // Check bounds
        CheckBounds();
    }

    // -------------------------------------------------------------------------
    // Physics
    // -------------------------------------------------------------------------

    private void ApplyPhysics()
    {
        float dt = Time.DeltaTime;

        if (_highFrictionTimer > 0f)
        {
            _highFrictionTimer -= dt;
        }

        if (OnGround())
        {
            // Ground friction and slope handling
            // TODO: OnGround() doesn't accept offset param in this Nez port
            bool onRightSlope = OnGround(); // Position + Vector2.UnitX * 3f
            bool onLeftSlope = OnGround(); // Position - Vector2.UnitX * 3f

            if (onRightSlope && !onLeftSlope)
                Speed.X = Calc.Approach(Speed.X, -20f, 800f * dt);
            else if (!onRightSlope && onLeftSlope)
                Speed.X = Calc.Approach(Speed.X, 20f, 800f * dt);
            else
                Speed.X = Calc.Approach(Speed.X, 0f, 800f * dt);

            // Transfer lift speed
            Vector2 liftSpeed = LiftSpeed;
            if (liftSpeed == Vector2.Zero && _prevLiftSpeed != Vector2.Zero)
            {
                Speed = _prevLiftSpeed;
                _prevLiftSpeed = Vector2.Zero;
                Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                if (Speed.X != 0f && Speed.Y == 0f)
                    Speed.Y = -60f;
                if (Speed.Y < 0f)
                    _noGravityTimer = 0.15f;
            }
            else
            {
                _prevLiftSpeed = liftSpeed;
                if (liftSpeed.Y < 0f && Speed.Y < 0f)
                    Speed.Y = 0f;
            }
        }
        else
        {
            // Air physics
            float gravity = Speed.Y >= -30f ? Gravity * 0.5f : Gravity;
            float friction = _highFrictionTimer > 0f && Speed.Y >= 0f ? 10f : 40f;

            Speed.X = Calc.Approach(Speed.X, 0f, friction * dt);

            if (_noGravityTimer > 0f)
            {
                _noGravityTimer -= dt;
            }
            else
            {
                Speed.Y = Calc.Approach(Speed.Y, MaxFall, gravity * dt);
            }
        }
    }

    private void CheckBounds()
    {
        // Check level bounds and bounce or die
        // TODO: implement level bounds check
    }

    private void OnCollideH(CelesteSolid solid)
    {
        Speed.X *= -0.4f;
        // TODO: play impact sound
    }

    private void OnCollideV(CelesteSolid solid)
    {
        Speed.Y *= -0.4f;
        _highFrictionTimer = HighFrictionTime;
        // TODO: play impact sound
    }

    private void UpdateRotation()
    {
        if (_holder == null) return;

        float targetRotation;
        if (!_holder.OnGround())
        {
            // In air - rotate based on horizontal speed
            float t1 = Math.Clamp((_holder.Speed.X - (-300f)) / (300f - (-300f)), 0f, 1f);
            targetRotation = MathHelper.Lerp(1.04719758f, -1.04719758f, t1);
        }
        else
        {
            // On ground - reduced rotation
            float t2 = Math.Clamp((_holder.Speed.X - (-300f)) / (300f - (-300f)), 0f, 1f);
            targetRotation = MathHelper.Lerp(0.6981317f, -0.6981317f, t2);
        }

        // TODO: smoothly rotate sprite to targetRotation
    }

    private void CheckSeekerBarrier()
    {
        // TODO: check for SeekerBarrier collision
        // If hit, destroy the glider
    }

    // -------------------------------------------------------------------------
    // Interaction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the player picks up the glider.
    /// </summary>
    public void Grab(MadelinePlayer player)
    {
        if (IsHeld || _destroyed) return;

        IsHeld = true;
        _holder = player;
        _prevLiftSpeed = Vector2.Zero;

        // TODO: play pickup sound
    }

    /// <summary>
    /// Called when the player releases the glider.
    /// </summary>
    public void Release(Vector2 throwSpeed)
    {
        if (!IsHeld) return;

        IsHeld = false;
        Speed = throwSpeed;
        _holder = null;
        _noGravityTimer = 0.15f;

        // TODO: play release sound
    }

    /// <summary>
    /// Destroys the glider.
    /// </summary>
    public void Destroy()
    {
        if (_destroyed) return;

        _destroyed = true;
        Collidable = false;

        // If held, drop first
        if (IsHeld && _holder != null)
        {
            Vector2 holderSpeed = _holder.Speed;
            // TODO: _holder.Drop();
            Speed = holderSpeed * 0.333f;
            // TODO: rumble
        }

        // TODO: Add(new Coroutine(DestroyAnimationRoutine()));
    }

    private IEnumerator DestroyAnimationRoutine()
    {
        // TODO: play destroy animation
        // TODO: emit particles

        yield return 0.5f;

        this.Destroy();
    }

    /// <summary>
    /// Called when hit by a spring.
    /// </summary>
    public void HitSpring(Spring spring)
    {
        // TODO: implement spring bounce
    }

    /// <summary>
    /// Respawns the glider at its original position.
    /// </summary>
    public void Respawn()
    {
        _destroyed = false;
        Position = _spawnPosition;
        Speed = Vector2.Zero;
        IsHeld = false;
        _holder = null;

        // TODO: play respawn sound
        // TODO: emit respawn particles
    }
}

/// <summary>
/// Extension for glider-specific calculations.
/// </summary>
public static class GliderCalc
{
    public static float ClampedMap(float value, float minInput, float maxInput, float minOutput, float maxOutput)
    {
        float t = (value - minInput) / (maxInput - minInput);
        t = Math.Clamp(t, 0f, 1f);
        return minOutput + t * (maxOutput - minOutput);
    }
}
