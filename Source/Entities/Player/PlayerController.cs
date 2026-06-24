using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace KirbyCelesteStandalone.Core;

/// <summary>
/// Player controller component.
/// Replaces: Celeste.Player + K_Player
///
/// Features to port from your mod:
/// - Movement (walk, run, jump)
/// - Dash mechanics (Celeste-style)
/// - Wall jump/climb
/// - Copy ability system (Kirby-specific)
/// - Health system
/// - Death/respawn
/// </summary>
public class PlayerController : Component, IUpdatable
{
    // Physics (using Nez's built-in Mover or Velcro Physics)
    public float MoveSpeed = 90f;     // Walk speed (pixels/sec)
    public float JumpForce = -120f;   // Jump velocity
    public float Gravity = 400f;      // Gravity (pixels/sec^2)
    public float DashSpeed = 240f;    // Dash speed
    public float DashDuration = 0.15f; // Dash duration in seconds

    // State
    public Vector2 Velocity;
    private bool _isGrounded;
    private bool _isDashing;
    private float _dashTimer;
    private Vector2 _dashDirection;

    /// <summary>True while the player is in an active dash.</summary>
    public bool IsDashing => _isDashing;

    // Dash state tracking
    private bool _canDash = true;
    private int _dashCount = 1;
    private int _maxDashes = 1;

    // Copy ability
    public string? CurrentAbility { get; private set; }
    public float AbilityTimer { get; private set; }

    // Components
    private SpriteRenderer? _spriteRenderer;
    private BoxCollider? _collider;

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Add collider
        _collider = Entity.AddComponent(new BoxCollider(8, 12));
        _collider.PhysicsLayer = 1; // Player layer

        // Add sprite renderer (placeholder)
        _spriteRenderer = Entity.AddComponent(new SpriteRenderer());

        // TODO: Load player sprite atlas
        // _spriteRenderer.SetSprite(sprite);
    }

    public void Update()
    {
        float deltaTime = Time.DeltaTime;

        // Handle dash
        if (_isDashing)
        {
            UpdateDash(deltaTime);
            return; // Skip normal movement during dash
        }

        // Check grounded
        _isGrounded = CheckGrounded();

        // Reset dash when grounded
        if (_isGrounded)
        {
            _dashCount = _maxDashes;
        }

        // Handle input
        HandleMovementInput(deltaTime);
        HandleJumpInput();
        HandleDashInput();

        // Apply gravity
        if (!_isGrounded)
        {
            Velocity.Y += Gravity * deltaTime;
        }

        // Apply velocity
        MoveAndSlide(deltaTime);

        // Update ability timer
        if (!string.IsNullOrEmpty(CurrentAbility))
        {
            AbilityTimer -= deltaTime;
            if (AbilityTimer <= 0)
            {
                LoseAbility();
            }
        }
    }

    private void HandleMovementInput(float deltaTime)
    {
        float horizontal = 0f;

        if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A))
            horizontal -= 1f;
        if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D))
            horizontal += 1f;

        // Apply movement
        Velocity.X = horizontal * MoveSpeed;

        // Face direction
        if (horizontal != 0 && _spriteRenderer != null)
        {
            _spriteRenderer.FlipX = horizontal < 0;
        }
    }

    private void HandleJumpInput()
    {
        if (Input.IsKeyPressed(Keys.Z) || Input.IsKeyPressed(Keys.Space))
        {
            if (_isGrounded)
            {
                // Normal jump
                Velocity.Y = JumpForce;
                _isGrounded = false;

                // Play jump sound
                KirbyGame.Audio.PlaySfx("event:/pusheen/game/general/jump");
            }
            // TODO: Wall jump
            // else if (IsTouchingWall())
            // {
            //     WallJump();
            // }
        }

        // Variable jump height (release early = shorter jump)
        if ((Input.IsKeyReleased(Keys.Z) || Input.IsKeyReleased(Keys.Space)) && Velocity.Y < 0)
        {
            Velocity.Y *= 0.5f;
        }
    }

    private void HandleDashInput()
    {
        if (Input.IsKeyPressed(Keys.X) || Input.IsKeyPressed(Keys.LeftShift))
        {
            if (_dashCount > 0 && !_isDashing)
            {
                StartDash();
            }
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashTimer = DashDuration;
        _dashCount--;

        // Get dash direction from input
        _dashDirection = Vector2.Zero;

        if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W))
            _dashDirection.Y -= 1f;
        if (Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S))
            _dashDirection.Y += 1f;
        if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A))
            _dashDirection.X -= 1f;
        if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D))
            _dashDirection.X += 1f;

        // Default to facing direction if no input
        if (_dashDirection == Vector2.Zero)
        {
            _dashDirection.X = _spriteRenderer?.FlipX == true ? -1f : 1f;
        }

        _dashDirection.Normalize();

        // Play dash sound
        KirbyGame.Audio.PlaySfx("event:/pusheen/game/general/dash");

        // TODO: Create dash after-image effect
    }

    private void UpdateDash(float deltaTime)
    {
        _dashTimer -= deltaTime;

        // Move at dash speed
        Velocity = _dashDirection * DashSpeed;
        Entity.Position += Velocity * deltaTime;

        // End dash
        if (_dashTimer <= 0)
        {
            _isDashing = false;
            Velocity *= 0.5f; // Maintain some momentum
        }
    }

    private void MoveAndSlide(float deltaTime)
    {
        // Simple movement - for full Celeste physics, use Velcro Physics
        Entity.Position += Velocity * deltaTime;

        // TODO: Implement collision detection with solids
        // - Check collision with tiles
        // - Resolve collision (slide along walls)
        // - Handle one-way platforms
    }

    private bool CheckGrounded()
    {
        // TODO: Raycast down to check for ground
        // return Physics.Raycast(Entity.Position, Vector2.Down, _collider.Height / 2 + 2);
        return false; // Placeholder
    }

    /// <summary>
    /// Give player a copy ability (Kirby-specific mechanic)
    /// </summary>
    public void GainAbility(string abilityName, float duration = 30f)
    {
        CurrentAbility = abilityName;
        AbilityTimer = duration;

        // Play ability gain sound
        KirbyGame.Audio.PlaySfx("event:/pusheen/game/ability/gain");

        // TODO: Change sprite, enable special attack
        Console.WriteLine($"[Player] Gained ability: {abilityName}");
    }

    private void LoseAbility()
    {
        CurrentAbility = null;
        AbilityTimer = 0;

        // Play ability lose sound
        KirbyGame.Audio.PlaySfx("event:/pusheen/game/ability/lose");

        Console.WriteLine("[Player] Lost ability");
    }

    /// <summary>
    /// Damage the player
    /// </summary>
    public void TakeDamage(int damage = 1, Vector2 knockbackDirection = default)
    {
        // TODO: Health system
        // - Reduce health
        // - Apply knockback
        // - Play hurt animation
        // - Check for death

        // Play hurt sound
        KirbyGame.Audio.PlaySfx("event:/pusheen/game/char/madeline/hurt");

        // TODO: If health <= 0, Die()
    }

    /// <summary>
    /// Kill player and trigger respawn
    /// </summary>
    public void Die()
    {
        // Play death sound
        KirbyGame.Audio.PlaySfx("event:/pusheen/game/general/death");

        // TODO: Death animation
        // TODO: Respawn at last checkpoint

        Console.WriteLine("[Player] Died - should respawn");
    }

    /// <summary>
    /// Respawn player at checkpoint
    /// </summary>
    public void Respawn(Vector2 position)
    {
        Entity.Position = position;
        Velocity = Vector2.Zero;
        _isDashing = false;
        _dashCount = _maxDashes;

        // TODO: Invincibility frames
    }
}
