using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Resort lantern that wiggles when bumped by the player.
/// Ported from Celeste's ResortLantern.cs.
///
/// The lantern hangs from a holder sprite.  When the player's velocity is
/// non-zero and they touch the lantern, it spins on a damped oscillation.
/// The bloom/light alpha breathes gently with a slow sine wave.
/// </summary>
public class ResortLantern : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private VertexLight? _light;
    private BloomPoint?  _bloom;

    private float _collideTimer;
    private float _alphaTimer;

    // Wiggle (simplified damped oscillation)
    private int   _wiggleSign = 1;
    private float _wiggleAngle;
    private float _wiggleVelocity;
    private float _wiggleDamping  = 3f;   // s⁻¹ decay
    private float _wiggleFreq     = 7.5f; // Hz
    private bool  _wiggling;

    private MadelinePlayer? _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public ResortLantern(Vector2 position)
    {
        _spawnPosition = position;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        _light = Entity.AddComponent(new VertexLight(Color.White, 0.95f, 32, 64));
        _bloom = Entity.AddComponent(new BloomPoint(0.8f, 8f));

        // TODO: add BoxCollider 8x8, offset (-4,-4), trigger
        // TODO: load "objects/resortLantern/holder" image, center-origin
        // TODO: load "objects/resortLantern/" sprite sheet for lantern anim
        //   add loop "light" → frames 0,0,1,2,1 at 0.3s delay
        // TODO: if solid to the right of entity → flip holder/lantern X scale
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntityOfType<MadelinePlayer>();

        // Collision cooldown
        if (_collideTimer > 0f) _collideTimer -= dt;

        // Breathing alpha
        _alphaTimer += dt;
        float breath = (float)(0.95 + Math.Sin(_alphaTimer) * 0.05);
        if (_light != null) _light.Alpha = breath;
        if (_bloom  != null) _bloom.Alpha  = breath;

        // Check player contact
        if (_player != null && IsPlayerOverlapping())
            OnPlayer();

        // Wiggle physics
        if (_wiggling)
        {
            float angAccel = -_wiggleFreq * _wiggleFreq * _wiggleAngle;
            _wiggleVelocity += angAccel * dt;
            _wiggleVelocity *= MathF.Exp(-_wiggleDamping * dt);
            _wiggleAngle    += _wiggleVelocity * dt;

            // TODO: apply lantern.Rotation = _wiggleAngle * _wiggleSign * MathHelper.Pi/6
            if (Math.Abs(_wiggleAngle) < 0.001f && Math.Abs(_wiggleVelocity) < 0.001f)
                _wiggling = false;
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void OnPlayer()
    {
        if (_player == null) return;
        if (_collideTimer <= 0f)
        {
            if (_player.Speed == Vector2.Zero) return;
            _collideTimer = 0.5f;
            _wiggleSign   = Nez.Random.Choose(1, -1);
            StartWiggle();
            // TODO: play sound: event:/game/03_resort/lantern_bump
        }
        else
        {
            _collideTimer = 0.5f;
        }
    }

    private void StartWiggle()
    {
        _wiggling       = true;
        _wiggleAngle    = 0.3f * _wiggleSign; // initial deflection
        _wiggleVelocity = 0f;
    }

    private bool IsPlayerOverlapping()
    {
        if (_player == null) return false;
        Vector2 d = _player.Position - Entity.Position;
        return Math.Abs(d.X) < 8f && Math.Abs(d.Y) < 8f;
    }
}
