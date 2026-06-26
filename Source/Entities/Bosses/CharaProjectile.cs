using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;
using System.Collections;
using KirbyCelesteStandalone.Core;
using KirbyCelesteStandalone.Entities.Player;
using Entity = Nez.Entity;
using Component = Nez.Component;

namespace KirbyCelesteStandalone.Entities.Bosses;

/// <summary>
/// Chara's projectile shot - can be homing, bouncing, or falling.
/// </summary>
public class CharaProjectile
{
    public Entity Entity { get; private set; }
    public bool IsActive => Entity != null && Entity.Enabled;

    // Physics
    private Vector2 _position;
    private Vector2 _velocity;
    private Vector2 _direction;
    private float _speed;
    private float _gravity;

    // Behavior
    private bool _homing;
    private float _homingStrength;
    private bool _bouncing;
    private int _maxBounces;
    private int _currentBounces;

    // Lifetime
    private float _lifetime;
    private const float MaxLifetime = 5f;
    private float _cantKillTimer = 0.15f;

    // Visual
    private SpriteRenderer? _spriteRenderer;

    public CharaProjectile(Vector2 position, Vector2 direction, float speed,
        bool homing = false, float homingStrength = 0f,
        bool bouncing = false, int maxBounces = 0,
        float gravity = 0f)
    {
        _position = position;
        _direction = Vector2Ext.Normalize(direction);
        _speed = speed;
        _gravity = gravity;
        _homing = homing;
        _homingStrength = homingStrength;
        _bouncing = bouncing;
        _maxBounces = maxBounces;
        _currentBounces = 0;

        CreateEntity();
    }

    private void CreateEntity()
    {
        Entity = new Entity("chara_projectile");
        Entity.Position = _position;

        // Add sprite
        _spriteRenderer = Entity.AddComponent(new SpriteRenderer());
        // TODO: Load actual projectile sprite
        // _spriteRenderer.SetSprite(...);

        // Add collider (small hitbox)
        var collider = Entity.AddComponent(new CircleCollider(4f));
        collider.IsTrigger = true;

        // Add this component for update
        Entity.AddComponent(new ProjectileUpdateComponent(this));
    }

    public void Update()
    {
        float dt = Time.DeltaTime;
        _lifetime += dt;
        _cantKillTimer -= dt;

        // Apply homing
        if (_homing)
        {
            ApplyHoming();
        }

        // Apply gravity
        if (_gravity > 0)
        {
            _velocity.Y += _gravity * dt;
        }
        else
        {
            _velocity = _direction * _speed;
        }

        // Move
        _position += _velocity * dt;
        Entity.Position = _position;

        // Check bounds
        if (_lifetime > MaxLifetime || IsOutOfBounds())
        {
            Destroy();
            return;
        }

        // Check collision with player
        CheckPlayerCollision();
    }

    private void ApplyHoming()
    {
        var player = Entity.Scene.FindEntity("player");
        if (player == null) return;

        Vector2 toPlayer = player.Position - _position;
        toPlayer.Normalize();

        // Gradually turn towards player
        _direction = Vector2.Lerp(_direction, toPlayer, _homingStrength * Time.DeltaTime);
        _direction.Normalize();
    }

    private void CheckPlayerCollision()
    {
        if (_cantKillTimer > 0) return;

        var player = Entity.Scene.FindEntity("player");
        if (player == null) return;

        float dist = Vector2.Distance(_position, player.Position);
        if (dist < 8f) // Player radius
        {
            // Hit player
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Vector2 knockback = Vector2Ext.Normalize(_position - player.Position) * -50f;
                playerController.TakeDamage(1, knockback);
            }

            if (!_bouncing || _currentBounces >= _maxBounces)
            {
                Destroy();
            }
        }
    }

    public void OnCollideWithSolid()
    {
        if (_bouncing && _currentBounces < _maxBounces)
        {
            // Reflect direction
            _direction = new Vector2(-_direction.X, _direction.Y);
            _currentBounces++;

            // Visual bounce effect
            // Spawn particles
        }
        else
        {
            Destroy();
        }
    }

    private bool IsOutOfBounds()
    {
        // Check if far outside screen bounds
        const float margin = 200f;
        var cameraBounds = Nez.Core.Scene?.Camera?.Bounds ?? new RectangleF(-1000, -1000, 2000, 2000);

        return _position.X < cameraBounds.Left - margin ||
               _position.X > cameraBounds.Right + margin ||
               _position.Y < cameraBounds.Top - margin ||
               _position.Y > cameraBounds.Bottom + margin;
    }

    public void Destroy()
    {
        if (Entity != null)
        {
            Entity.Destroy();
            Entity = null;
        }
    }

    /// <summary>
    /// Internal component for updating the projectile
    /// </summary>
    private class ProjectileUpdateComponent : Component, IUpdatable
    {
        private readonly CharaProjectile _projectile;

        public ProjectileUpdateComponent(CharaProjectile projectile)
        {
            _projectile = projectile;
        }

        public void Update()
        {
            _projectile.Update();
        }
    }
}
