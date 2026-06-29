using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using System;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Decorative floating debris piece that bobs in place and gets knocked around
/// by the player (or nearby explosions).  Ported from Celeste's FloatingDebris.cs.
///
/// Each piece picks a random sub-cell from the "scenery/debris" sprite sheet
/// and slowly rotates.  When pushed, it drifts away from the contact point
/// then gradually floats back to its spawn position.
/// </summary>
public class FloatingDebris : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly Vector2 _startPosition;
    private Vector2          _pushOut;
    private float            _accelMult = 1f;

    // Sine bob
    private float _sinePhase;
    private const float SineFrequency = 0.4f; // Hz
    private const float SineAmplitude = 2f;   // px

    // Rotation
    private readonly float _rotateSpeed; // radians/s

    // TODO: image sub-texture index (chosen randomly from "scenery/debris")
    private readonly int _textureIndex;

    private MadelinePlayer? _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public FloatingDebris(Vector2 position)
    {
        _startPosition = position;
        _sinePhase     = DZ.Nez.Random.NextFloat() * MathF.PI * 2f;
        _textureIndex  = DZ.Nez.Random.NextInt(8); // assume 8 columns in debris sheet

        // Random rotation speed (some pieces don't rotate)
        int rChoice = DZ.Nez.Random.NextInt(10);
        int deg     = rChoice switch { 0 => -2, 1 => -1, 8 => 1, 9 => 2, _ => 0 };
        _rotateSpeed = deg * 40f * MathF.PI / 180f;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _startPosition;

        // TODO: add BoxCollider 12x12, offset (-6,-6), trigger
        // TODO: add Image renderer using "scenery/debris" sub-texture at column _textureIndex
        //   with centered origin
        // TODO: set render depth -5
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        // Check player contact
        if (_player != null && IsPlayerOverlapping())
            OnPlayerContact();

        // Movement
        if (_pushOut != Vector2.Zero)
        {
            Entity.Position += _pushOut * dt;
            float pushLen = _pushOut.Length();
            float reduce  = 64f * _accelMult * dt;
            if (pushLen <= reduce)
                _pushOut = Vector2.Zero;
            else
                _pushOut = _pushOut / pushLen * (pushLen - reduce);
        }
        else
        {
            _accelMult = 1f;
            // Float back to start
            float dist = Vector2.Distance(Entity.Position, _startPosition);
            if (dist > 0.5f)
            {
                Vector2 dir = (_startPosition - Entity.Position) / dist;
                Entity.Position += dir * Math.Min(6f * dt, dist);
            }
        }

        // Sine bob (vertical offset applied to image Y)
        _sinePhase += dt * SineFrequency * MathF.PI * 2f;
        float sineY = MathF.Sin(_sinePhase) * SineAmplitude;
        // TODO: apply sineY to image.Y local offset

        // Rotation
        // TODO: apply _rotateSpeed * dt to image.Rotation
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Pushes the debris away from an explosion centre.</summary>
    public void OnExplode(Vector2 from)
    {
        Vector2 dir = Entity.Position - from;
        float   len = dir.Length();
        if (len > 0f) _pushOut = dir / len * 160f;
        else          _pushOut = Vector2.UnitX * 160f;
        _accelMult = 4f;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void OnPlayerContact()
    {
        if (_player == null) return;
        float playerSpeed = _player.Speed.Length();
        Vector2 dir       = Entity.Position - _player.Position;
        float   len       = dir.Length();
        Vector2 push      = len > 0f ? dir / len * (playerSpeed * 0.2f) : Vector2.Zero;
        if (push.LengthSquared() > _pushOut.LengthSquared())
            _pushOut = push;
        _accelMult = 1f;
    }

    private bool IsPlayerOverlapping()
    {
        if (_player == null) return false;
        Vector2 d = _player.Position - Entity.Position;
        return Math.Abs(d.X) < 12f && Math.Abs(d.Y) < 12f;
    }
}
