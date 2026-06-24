using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Collectibles;

/// <summary>
/// Port of Celeste's TheoCrystal.cs.
///
/// Holdable crystal that contains Theo. Can be carried by the player and shatters
/// when hitting hazards. Has physics and collision similar to the player.
/// </summary>
public class TheoCrystal : CelesteActor, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const float Gravity = 800f;
    private const float MaxFall = 200f;
    private const float Friction = 800f;
    private const float AirFriction = 350f;
    private const float BounceFactor = -0.4f;
    private const float NoGravityTime = 0.15f;

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Current velocity of the crystal.</summary>
    public Vector2 Speed;

    /// <summary>Whether the crystal is on its pedestal (stationary).</summary>
    public bool OnPedestal { get; set; }

    /// <summary>Whether the crystal is being held by the player.</summary>
    public bool IsHeld { get; private set; }

    /// <summary>Whether the crystal has been destroyed.</summary>
    public bool IsDestroyed => _dead;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private MadelinePlayer? _holder;
    private Vector2 _spawnPosition;
    private Vector2 _previousPosition;
    private Vector2 _prevLiftSpeed;
    private float _noGravityTimer;
    private float _hardVerticalHitSoundCooldown;
    private bool _dead;
    private bool _shattering;
    private bool _tutorialShown;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public TheoCrystal(Vector2 position) : base(position, 8f, 10f)
    {
        _spawnPosition = position;
        _previousPosition = position;
        // Depth = 100; // TODO: not supported in Nez
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Position collider to be at feet
        if (Collider != null)
        {
            Collider.SetLocalOffset(new Vector2(0f, -10f));
        }

        // TODO: load sprite "theo_crystal"
        // TODO: set up vertex light
        // TODO: add mirror reflection
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        if (_shattering || _dead) return;

        _hardVerticalHitSoundCooldown -= Time.DeltaTime;

        if (OnPedestal)
        {
            // Depth = 8999; // TODO: not supported in Nez
            Speed = Vector2.Zero;
            return;
        }

        // Depth = 100; // TODO: not supported in Nez

        if (IsHeld)
        {
            // Follow holder
            if (_holder != null)
            {
                Position = _holder.Position + new Vector2(0f, -20f);
                Speed = _holder.Speed;
            }
            _prevLiftSpeed = Vector2.Zero;
            return;
        }

        // Apply physics
        ApplyPhysics();

        // Check bounds
        CheckBounds();

        // Move with collision
        _previousPosition = Position;
        MoveH(Speed.X * Time.DeltaTime, OnCollideH);
        MoveV(Speed.Y * Time.DeltaTime, OnCollideV);
    }

    // -------------------------------------------------------------------------
    // Physics
    // -------------------------------------------------------------------------

    private void ApplyPhysics()
    {
        float dt = Time.DeltaTime;

        if (OnGround())
        {
            // Ground friction and slope handling
            // TODO: OnGround() doesn't accept offset param in this Nez port
            bool onRightSlope = OnGround(); // Position + Vector2.UnitX * 3f
            bool onLeftSlope = OnGround(); // Position - Vector2.UnitX * 3f

            if (onRightSlope && !onLeftSlope)
                Speed.X = Calc.Approach(Speed.X, -20f, Friction * dt);
            else if (!onRightSlope && onLeftSlope)
                Speed.X = Calc.Approach(Speed.X, 20f, Friction * dt);
            else
                Speed.X = Calc.Approach(Speed.X, 0f, Friction * dt);

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
                    _noGravityTimer = NoGravityTime;
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
            float gravMult = MathF.Abs(Speed.Y) <= 30f ? 0.5f : 1f;
            Speed.X = Calc.Approach(Speed.X, 0f, AirFriction * dt);

            if (_noGravityTimer > 0f)
            {
                _noGravityTimer -= dt;
            }
            else
            {
                Speed.Y = Calc.Approach(Speed.Y, MaxFall, Gravity * gravMult * dt);
            }
        }
    }

    private void CheckBounds()
    {
        // Check level bounds
        var scene = Scene as Scene;
        if (scene == null) return;

        // TODO: check against level bounds and respawn or die
    }

    private void OnCollideH(CelesteSolid solid)
    {
        Speed.X *= BounceFactor;
        // TODO: play impact sound
    }

    private void OnCollideV(CelesteSolid solid)
    {
        if (Speed.Y > 0f && Speed.Y > 100f && _hardVerticalHitSoundCooldown <= 0f)
        {
            _hardVerticalHitSoundCooldown = 0.5f;
            // TODO: play heavy impact sound
        }
        Speed.Y *= BounceFactor;
        // TODO: play impact sound
    }

    // -------------------------------------------------------------------------
    // Interaction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the player picks up the crystal.
    /// </summary>
    public void Grab(MadelinePlayer player)
    {
        if (IsHeld || OnPedestal) return;

        IsHeld = true;
        _holder = player;
        _prevLiftSpeed = Vector2.Zero;

        // TODO: play pickup sound
    }

    /// <summary>
    /// Called when the player releases the crystal.
    /// </summary>
    public void Release(Vector2 throwSpeed)
    {
        if (!IsHeld) return;

        IsHeld = false;
        Speed = throwSpeed;
        _holder = null;
        _noGravityTimer = NoGravityTime;

        // TODO: play release sound
    }

    /// <summary>
    /// Shatters the crystal (Theo is freed).
    /// </summary>
    public IEnumerator Shatter()
    {
        _shattering = true;

        // TODO: add bloom and light effects
        // TODO: play shatter animation

        // Slow zoom and effects
        for (float p = 0f; p < 1f; p += Time.DeltaTime)
        {
            // TODO: animate effects
            yield return null;
        }

        yield return 0.5f;

        // TODO: shake level
        // TODO: play shatter sound

        yield return 1f;

        // TODO: shake level again
        // TODO: spawn Theo NPC

        _dead = true;
        this.Destroy();
    }

    /// <summary>
    /// Called when hit by a seeker.
    /// </summary>
    public void HitSeeker(Vector2 direction)
    {
        // TODO: implement seeker hit response
        Speed += direction * 100f;
    }

    /// <summary>
    /// Called when hit by a spinner.
    /// </summary>
    public void HitSpinner()
    {
        // TODO: implement spinner hit response
    }

    /// <summary>
    /// Called when hit by a spring.
    /// </summary>
    public void HitSpring(Spring spring)
    {
        // TODO: implement spring bounce
    }

    /// <summary>
    /// Dies (falls out of level).
    /// </summary>
    public void Die()
    {
        _dead = true;
        // TODO: play die sound
        // TODO: respawn after delay
    }
}

/// <summary>
/// Spring entity that can bounce TheoCrystal.
/// </summary>
public class Spring : Component
{
    // Placeholder for spring reference
}

/// <summary>
/// Helper class for mathematical calculations.
/// </summary>
public static class Calc
{
    public static float Approach(float val, float target, float maxMove)
    {
        return val > target ? Math.Max(val - maxMove, target) : Math.Min(val + maxMove, target);
    }
}
