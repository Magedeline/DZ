using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using DZ.Nez.Sprites;
using System;
using System.Collections;
using DZ.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Bosses;

/// <summary>
/// Chara's laser beam attack - sweeps across the arena.
/// </summary>
public class CharaBeam
{
    public Entity Entity { get; private set; }
    public bool IsActive => Entity != null && Entity.Enabled;

    // Beam properties
    private Vector2 _origin;
    private Vector2 _direction;
    private float _duration;
    private float _width;
    private float _timer;

    // Sweeping beam
    private bool _sweeping;
    private float _sweepAngle;
    private float _sweepSpeed;
    private float _currentAngle;

    // Full screen beam
    private bool _fullScreen;

    // Visual
    private float _fadeIn = 0.1f;
    private float _fadeOut = 0.2f;
    private float _maxAlpha = 1f;

    // Damage
    private float _damageInterval = 0.1f;
    private float _damageTimer;

    // Components
    private SpriteRenderer _beamRenderer;
    private BoxCollider _hitCollider;

    public CharaBeam(Vector2 origin, Vector2 direction, float duration,
        float width = 1f, bool fullScreen = false,
        bool sweeping = false, float sweepAngle = 0f)
    {
        _origin = origin;
        _direction = Vector2Ext.Normalize(direction);
        _duration = duration;
        _width = width * 8f; // Base width
        _fullScreen = fullScreen;
        _sweeping = sweeping;
        _sweepAngle = sweepAngle;
        _currentAngle = MathF.Atan2(_direction.Y, _direction.X);

        if (sweeping)
        {
            _sweepSpeed = sweepAngle / duration;
        }

        CreateEntity();
    }

    private void CreateEntity()
    {
        Entity = new Entity("chara_beam");
        Entity.Position = _origin;

        // Setup visual
        _beamRenderer = Entity.AddComponent(new SpriteRenderer());
        // TODO: Load beam sprite or use procedural rendering

        // Setup hit collider
        if (_fullScreen)
        {
            // Screen-wide beam
            _hitCollider = Entity.AddComponent(new BoxCollider(320f, _width));
        }
        else
        {
            // Directional beam
            float beamLength = 200f;
            _hitCollider = Entity.AddComponent(new BoxCollider(beamLength, _width));
            _hitCollider.SetLocalOffset(new Vector2(beamLength / 2, 0));
            Entity.Rotation = MathF.Atan2(_direction.Y, _direction.X);
        }

        _hitCollider.IsTrigger = true;

        // Add update component
        Entity.AddComponent(new BeamUpdateComponent(this));

        // Play beam sound
        DZGame.Audio.PlaySfx("event:/DZ/game/boss/chara_beam_fire");
    }

    public void Update()
    {
        float dt = Time.DeltaTime;
        _timer += dt;
        _damageTimer -= dt;

        // Update sweeping
        if (_sweeping)
        {
            _currentAngle += _sweepSpeed * dt;
            _direction = new Vector2(MathF.Cos(_currentAngle), MathF.Sin(_currentAngle));
            Entity.Rotation = _currentAngle;
        }

        // Update alpha based on fade in/out
        float alpha = CalculateAlpha();
        // Apply to sprite
        if (_beamRenderer != null)
        {
            _beamRenderer.Color = Color.White * alpha;
        }

        // Deal damage
        if (alpha > 0.5f && _damageTimer <= 0)
        {
            CheckPlayerDamage();
            _damageTimer = _damageInterval;
        }

        // End beam
        if (_timer >= _duration + _fadeIn + _fadeOut)
        {
            Destroy();
        }
    }

    private float CalculateAlpha()
    {
        if (_timer < _fadeIn)
        {
            // Fading in
            return (_timer / _fadeIn) * _maxAlpha;
        }
        else if (_timer > _duration + _fadeIn)
        {
            // Fading out
            float fadeTime = _timer - (_duration + _fadeIn);
            return (1f - (fadeTime / _fadeOut)) * _maxAlpha;
        }
        else
        {
            // Full strength
            return _maxAlpha;
        }
    }

    private void CheckPlayerDamage()
    {
        var player = Entity.Scene.FindEntity("player");
        if (player == null) return;

        // Check if player is within beam
        if (IsPointInBeam(player.Position))
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Vector2 knockback = _direction * -30f;
                playerController.TakeDamage(1, knockback);
            }
        }
    }

    private bool IsPointInBeam(Vector2 point)
    {
        if (_fullScreen)
        {
            // Check vertical position for full screen beam
            float distY = MathF.Abs(point.Y - _origin.Y);
            return distY < _width / 2;
        }
        else
        {
            // Check distance to line segment
            Vector2 toPoint = point - _origin;
            float projLength = Vector2.Dot(toPoint, _direction);

            if (projLength < 0 || projLength > 200f)
                return false;

            Vector2 closestPoint = _origin + _direction * projLength;
            float dist = Vector2.Distance(point, closestPoint);

            return dist < _width / 2 + 4f; // + player radius
        }
    }

    public void Destroy()
    {
        if (Entity != null)
        {
            // Stop beam sound
            DZGame.Audio.PlaySfx("event:/DZ/game/boss/chara_beam_stop");

            Entity.Destroy();
            Entity = null;
        }
    }

    /// <summary>
    /// Internal component for updating the beam
    /// </summary>
    private class BeamUpdateComponent : Component, IUpdatable
    {
        private readonly CharaBeam _beam;

        public BeamUpdateComponent(CharaBeam beam)
        {
            _beam = beam;
        }

        public override void Update()
        {
            _beam.Update();
        }
    }
}

/// <summary>
/// The "Bigger Beam" - enhanced version for phase 3
/// </summary>
public class CharaBiggerBeam : CharaBeam
{
    public CharaBiggerBeam(Vector2 origin, Vector2 direction, float duration)
        : base(origin, direction, duration, width: 4f, fullScreen: true)
    {
    }
}
