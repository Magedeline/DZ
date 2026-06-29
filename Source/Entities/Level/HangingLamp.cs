using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using static DZ.Nez.Time;
using System;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Hanging lamp that sways when the player passes through it.
/// Ported from Celeste's HangingLamp.cs.
///
/// The lamp rotates around its top attachment point.  Player contact with the
/// hitbox imparts angular speed proportional to the player's horizontal speed
/// and vertical position within the lamp.  The lamp clacks sounds as it
/// passes the resting angle.
/// </summary>
public class HangingLamp : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public
    // -------------------------------------------------------------------------

    /// <summary>Total lamp length in pixels (minimum 16).</summary>
    public int Length { get; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _angularSpeed;
    private float _rotation;
    private float _soundDelay;

    private VertexLight _light;
    private BloomPoint _bloom;

    private readonly Vector2 _attachOffset = new Vector2(4f, 0f); // right-centre offset

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">Top-centre attachment point in world space.</param>
    /// <param name="length">Lamp length (clamped to min 16 px).</param>
    public HangingLamp(Vector2 position, int length)
    {
        _spawnPosition = position + new Vector2(4f, 0f);
        Length = Math.Max(16, length);
    }

    private readonly Vector2 _spawnPosition;
    private MadelinePlayer _player;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // Lighting at lamp tip
        var tipOffset = Vector2.UnitY * (Length - 4f);
        _light = Entity.AddComponent(new VertexLight(tipOffset, Color.White, 1f, 24, 48));
        _bloom = Entity.AddComponent(new BloomPoint(tipOffset, 1f, 48f));

        // TODO: add BoxCollider width 8, height Length, offset (-4,0)
        // TODO: load "objects/hanginglamp" sub-textures and add image renderers
        //   - top cap:    sub-texture row 0
        //   - chain tiles: sub-texture row 1, tiled from 0 to Length-8, step 8
        //   - lamp head:  sub-texture row 2 at origin-y = -(Length-8)
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;
        _player = Entity.Scene.FindEntityOfType<MadelinePlayer>();
        _soundDelay -= dt;

        // Player collision → impart angular speed
        if (_player != null)
        {
            // Simple AABB check
            Vector2 pPos = _player.Position;
            float   halfW = 4f;
            float   lampBottom = Entity.Position.Y + Length;
            if (Math.Abs(pPos.X - Entity.Position.X) < halfW + 4f
             && pPos.Y > Entity.Position.Y
             && pPos.Y < lampBottom)
            {
                float relY = (pPos.Y - Entity.Position.Y) / Length;
                float push = -_player.Speed.X * 0.005f * relY;
                if (Math.Abs(push) >= 0.1f)
                {
                    _angularSpeed = push;
                    if (_soundDelay <= 0f)
                    {
                        _soundDelay = 0.25f;
                        // TODO: play sound: event:/game/02_old_site/lantern_hit
                    }
                }
            }
        }

        // Spring physics
        float restoreForce = Math.Sign(_rotation) == Math.Sign(_angularSpeed) ? 8f : 6f;
        if (Math.Abs(_rotation) < 0.5f) restoreForce *= 0.5f;
        if (Math.Abs(_rotation) < 0.25f) restoreForce *= 0.5f;

        float prevRotation = _rotation;
        _angularSpeed -= Math.Sign(_rotation) * restoreForce * dt;
        _rotation     += _angularSpeed * dt;
        _rotation      = Math.Clamp(_rotation, -0.4f, 0.4f);

        if (Math.Abs(_rotation) < 0.02f && Math.Abs(_angularSpeed) < 0.2f)
        {
            _rotation = _angularSpeed = 0f;
        }
        else if (Math.Sign(_rotation) != Math.Sign(prevRotation)
              && _soundDelay <= 0f
              && Math.Abs(_angularSpeed) > 0.5f)
        {
            _soundDelay = 0.25f;
            // TODO: play sound: event:/game/02_old_site/lantern_hit
        }

        // Update light/bloom to follow the swinging tip
        Vector2 tipDir = AngleToVector(_rotation + MathF.PI * 0.5f) * (Length - 4f);
        if (_light != null) _light.Position = tipDir;
        if (_bloom  != null) _bloom.Position = tipDir;

        // TODO: apply _rotation to all chain-link images
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Vector2 AngleToVector(float angle)
        => new Vector2(MathF.Cos(angle), MathF.Sin(angle));
}
