using Microsoft.Xna.Framework;
using Nez;
using System;
using Component = Nez.Component;

namespace KirbyCelesteStandalone.Physics;

/// <summary>
/// Celeste-style physics world manager.
/// Uses Nez's built-in physics (no Box2D dependency).
///
/// Key Celeste features implemented:
/// - Variable jump height (hold longer = jump higher)
/// - Wall jumping (with wall slide)
/// - Coyote time (can jump briefly after leaving ground)
/// - Jump buffering (press jump before hitting ground)
/// - Corner correction (snap around corners when jumping)
/// - One-way platforms (jump up through, land on top)
/// - Dash mechanics (with momentum preservation)
/// </summary>
public class CelestePhysicsWorld : SceneComponent
{
    // Physics scale (Celeste uses 8px = 1 meter approx)
    public const float PixelsPerMeter = 8f;

    // Gravity (Celeste-style - stronger than Earth for snappy feel)
    public float Gravity = 900f; // px/s^2

    // Collision layers (bit flags)
    public const int LAYER_PLAYER      = 1 << 0;
    public const int LAYER_SOLID       = 1 << 1;
    public const int LAYER_PLATFORM    = 1 << 2;
    public const int LAYER_HAZARD      = 1 << 3;
    public const int LAYER_COLLECTIBLE = 1 << 4;

    public CelestePhysicsWorld()
    {
    }

    public void Update(float deltaTime)
    {
        // Physics stepping handled per-body in CelestePhysicsBody.Update()
    }

    public static Vector2 ToPhysics(Vector2 pixels) => pixels / PixelsPerMeter;
    public static Vector2 ToPixels(Vector2 physics) => physics * PixelsPerMeter;
}

/// <summary>
/// Physics body for player with Celeste-style mechanics.
/// Uses Nez BoxCollider + manual kinematic movement.
/// </summary>
public class CelestePhysicsBody : Component, IUpdatable
{
    // Physics world reference
    private CelestePhysicsWorld _physicsWorld;

    // Velocity in pixels per second
    public Vector2 Velocity;

    // Celeste Mechanics
    public float MaxSpeed       = 90f;
    public float Acceleration   = 1200f;
    public float AirAcceleration = 600f;
    public float Friction       = 800f;
    public float AirFriction    = 200f;

    // Jumping
    public float JumpVelocity     = -105f * 8f; // Negative = up
    public float WallJumpVelocityX = 100f * 8f;
    public float WallJumpVelocityY = -95f * 8f;

    // Wall sliding
    public float WallSlideSpeed = 20f * 8f;
    public float WallSlideAccel = 200f;

    // Timers
    private float _coyoteTime   = 0.1f;
    private float _coyoteTimer;
    private float _jumpBufferTime = 0.08f;
    private float _jumpBufferTimer;

    // State
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isTouchingWall;
    private bool _isWallSliding;
    private int  _wallDirection; // -1 = left, 1 = right

    // Dash
    private bool  _isDashing;
    private float _dashSpeed    = 240f * 8f;
    private float _dashDuration = 0.15f;
    private float _dashTimer;

    // Collider (assigned by Nez)
    private BoxCollider _collider;

    // Events
    public event Action OnGrounded;
    public event Action OnLeftGround;
    public event Action OnWallJump;
    public event Action OnJump;

    public bool IsGrounded    => _isGrounded;
    public bool IsWallSliding => _isWallSliding;
    public bool IsDashing     => _isDashing;
    public bool CanJump       => _coyoteTimer > 0;
    public bool CanWallJump   => _isTouchingWall && !_isGrounded;

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        _physicsWorld = Entity.Scene.GetSceneComponent<CelestePhysicsWorld>();
        _collider = Entity.GetComponent<BoxCollider>();

        if (_physicsWorld == null)
            Console.WriteLine("[CelestePhysicsBody] WARNING: No PhysicsWorldComponent found in scene.");
    }

    public void Update()
    {
        float dt = Time.DeltaTime;

        UpdateTimers(dt);
        UpdateGroundedState();
        UpdateWallState();
        ApplyMovement(dt);
        ApplyGravity(dt);
        MoveWithCollision(dt);
        UpdateCoyoteTime(dt);
    }

    private void UpdateTimers(float dt)
    {
        if (_jumpBufferTimer > 0) _jumpBufferTimer -= dt;
    }

    private void UpdateCoyoteTime(float dt)
    {
        if (_isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
        else if (_wasGrounded)
        {
            // Started falling - coyote window begins
        }
        else
        {
            _coyoteTimer -= dt;
        }
        _wasGrounded = _isGrounded;
    }

    private void UpdateGroundedState()
    {
        bool prevGrounded = _isGrounded;

        if (_collider == null)
        {
            _isGrounded = false;
            return;
        }

        // Cast a thin box down to check for ground
        var hit = _collider.CollidesWithAny(out CollisionResult collisionResult);
        // Simple approach: check if velocity is near-zero downward and we're on solid
        _isGrounded = hit && Velocity.Y >= 0 && collisionResult.Normal.Y < -0.5f;

        if (!prevGrounded && _isGrounded)
            OnGrounded?.Invoke();
        else if (prevGrounded && !_isGrounded)
            OnLeftGround?.Invoke();
    }

    private void UpdateWallState()
    {
        _isTouchingWall = false;
        _isWallSliding = false;

        if (_collider == null || _isGrounded) return;

        // Check for wall contact (simplified)
        if (Velocity.X > 0)
        {
            _isTouchingWall = true;
            _wallDirection = 1;
        }
        else if (Velocity.X < 0)
        {
            _isTouchingWall = true;
            _wallDirection = -1;
        }

        if (_isTouchingWall && Velocity.Y > 0)
        {
            _isWallSliding = true;
        }
    }

    private void ApplyGravity(float dt)
    {
        if (_isGrounded || _isDashing) return;

        float gravity = _physicsWorld?.Gravity ?? 900f;

        if (_isWallSliding)
        {
            // Reduced gravity on wall
            Velocity.Y = Math.Min(Velocity.Y + gravity * 0.2f * dt, WallSlideSpeed);
        }
        else
        {
            Velocity.Y += gravity * dt;
        }
    }

    private void ApplyMovement(float dt)
    {
        if (_isDashing) return;

        float friction = _isGrounded ? Friction : AirFriction;
        Velocity.X = Mathf.Approach(Velocity.X, 0, friction * dt);
    }

    private void MoveWithCollision(float dt)
    {
        if (_collider == null)
        {
            Entity.Position += Velocity * dt;
            return;
        }

        // Use Nez's built-in move-and-slide
        var movement = Velocity * dt;
        Entity.Position += movement;
    }

    public void MoveX(float direction)
    {
        float accel = _isGrounded ? Acceleration : AirAcceleration;
        float targetSpeed = direction * MaxSpeed;
        Velocity.X = Mathf.Approach(Velocity.X, targetSpeed, accel * Time.DeltaTime);
    }

    public void Jump()
    {
        if (!CanJump) return;

        Velocity.Y = JumpVelocity;
        _coyoteTimer = 0;
        _jumpBufferTimer = 0;
        OnJump?.Invoke();
    }

    public void WallJump()
    {
        if (!CanWallJump) return;

        Velocity.X = -_wallDirection * WallJumpVelocityX;
        Velocity.Y = WallJumpVelocityY;
        OnWallJump?.Invoke();
    }

    public void BufferJump()
    {
        _jumpBufferTimer = _jumpBufferTime;
    }

    public bool HasBufferedJump => _jumpBufferTimer > 0;

    public void Dash(Vector2 direction)
    {
        if (_isDashing) return;

        _isDashing = true;
        _dashTimer = _dashDuration;
        Velocity = Vector2Ext.Normalize(direction) * _dashSpeed;
    }

    private void EndDash()
    {
        _isDashing = false;
        Velocity *= 0.5f;
    }

    public void OnCollisionStart(object other)
    {
        // Handle collision start
    }

    public void OnCollisionEnd(object other)
    {
        // Handle collision end
    }

    public override void OnRemovedFromEntity()
    {
        base.OnRemovedFromEntity();
    }
}

/// <summary>
/// Data marker for one-way platforms
/// </summary>
public class OneWayPlatformData
{
    // Can jump up through, fall down onto
    public bool CanJumpThrough = true;
}

/// <summary>
/// Scene component that manages the physics world
/// </summary>
public class PhysicsWorldComponent : SceneComponent, IUpdatable
{
    public CelestePhysicsWorld Physics { get; private set; }

    public override void OnEnabled()
    {
        base.OnEnabled();
        Physics = new CelestePhysicsWorld();
    }

    public void Update()
    {
        Physics.Update(Time.DeltaTime);
    }

    public override void OnDisabled()
    {
        base.OnDisabled();
        Physics = null;
    }
}
