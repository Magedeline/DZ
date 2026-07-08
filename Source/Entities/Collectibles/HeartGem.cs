using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using System;
using System.Collections;
using System.Collections.Generic;
using DZ.Entities.Player;
using DZ.Entities.Core;

namespace DZ.Entities.Collectibles;

/// <summary>
/// Port of Celeste's HeartGem.cs.
///
/// Chapter heart collectible that bounces players when touched normally,
/// but can be collected when dash-attacking. Shows a poem cutscene on collection.
/// </summary>
public class HeartGem : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const string FakeHeartFlag = "fake_heart";

    public static readonly Color BlueColor = Color.Aqua;
    public static readonly Color RedColor = Color.Red;
    public static readonly Color GoldColor = Color.Gold;
    public static readonly Color FakeColor = new Color(0xDA, 0xD8, 0xCC);

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Whether this is a ghost heart (already collected).</summary>
    public bool IsGhost { get; private set; }

    /// <summary>Whether this is a fake heart (bounces player, damages on dash).</summary>
    public bool IsFake { get; private set; }

    /// <summary>Whether the heart has been collected.</summary>
    public bool Collected { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _spawnPosition;
    private bool _isGhost;
    private bool _isFake;
    private bool _removeCameraTriggers;
    private bool _autoPulse = true;
    private float _timer;
    private float _bounceSfxDelay;
    private Vector2 _moveWiggleDir;

    private float _scaleWiggleValue;
    private float _scaleWiggleTimer;
    private float _moveWiggleValue;
    private float _moveWiggleTimer;

    private BoxCollider _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public HeartGem(Vector2 position, bool isFake = false, bool removeCameraTriggers = false)
    {
        _spawnPosition = position;
        _isFake = isFake;
        _removeCameraTriggers = removeCameraTriggers;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // 16x16 hitbox, centered
        _collider = Entity.AddComponent(new BoxCollider(-8f, -8f, 16f, 16f));
        _collider.IsTrigger = true;

        // TODO: load sprite based on heart type
        // TODO: set up bloom point and light
        // TODO: set up shine particles
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;

        _bounceSfxDelay -= dt;
        _timer += dt;

        // Bob animation
        float yOffset = MathF.Sin(_timer * 2f) * 2f;
        Vector2 wiggleOffset = _moveWiggleDir * _moveWiggleValue * -8f;
        // TODO: sprite.Position = new Vector2(wiggleOffset.X, yOffset + wiggleOffset.Y);

        // Scale wiggle
        if (_scaleWiggleTimer > 0f)
        {
            _scaleWiggleTimer -= dt;
            _scaleWiggleValue = MathF.Sin(_scaleWiggleTimer * 8f) * MathF.Exp(-_scaleWiggleTimer * 4f);
        }
        else
        {
            _scaleWiggleValue = 0f;
        }

        // Move wiggle
        if (_moveWiggleTimer > 0f)
        {
            _moveWiggleTimer -= dt;
            _moveWiggleValue = MathF.Sin(_moveWiggleTimer * 5f) * MathF.Exp(-_moveWiggleTimer * 2f);
        }
        else
        {
            _moveWiggleValue = 0f;
        }

        // TODO: update bloom and light

        if (!Collected)
        {
            CheckPlayerOverlap();
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static readonly Collider[] _overlapResults = new Collider[8];

    private void CheckPlayerOverlap()
    {
        if (_collider == null) return;

        var rect = new RectangleF(
            Entity.Position.X - 8f,
            Entity.Position.Y - 8f,
            16f,
            16f);

        int count = DZ.Nez.Physics.OverlapRectangleAll(ref rect, _overlapResults);

        for (int i = 0; i < count; i++)
        {
            var hit = _overlapResults[i];
            if (hit == _collider) continue;

            var player = hit.Entity?.GetComponent<MadelinePlayer>();
            if (player == null) continue;

            OnPlayer(player);
            break;
        }
    }

    private void OnPlayer(MadelinePlayer player)
    {
        if (Collected) return;

        if (player.DashAttacking)
        {
            Collect(player);
        }
        else
        {
            BouncePlayer(player);
        }
    }

    private void BouncePlayer(MadelinePlayer player)
    {
        if (_bounceSfxDelay <= 0f)
        {
            // TODO: play sound: _isFake ? event:/new_content/game/10_farewell/fakeheart_bounce : event:/game/general/crystalheart_bounce
            _bounceSfxDelay = 0.1f;
        }

        // Point bounce
        Vector2 bounceDir = (Entity.Position - player.Position).SafeNormalize(Vector2.UnitY);
        // player.PointBounce(Entity.Position); // TODO: not implemented in this port

        _moveWiggleTimer = 0.8f;
        _scaleWiggleTimer = 0.5f;
        _moveWiggleDir = bounceDir;

        // TODO: rumble
    }

    private void Collect(MadelinePlayer player)
    {
        if (Collected) return;
        Collected = true;

        // TODO: stop angry oshiro time control
        // TODO: start cutscene
        // TODO: play collect sound
        // TODO: emit particles
        // TODO: show poem

        // Start collection routine
        // TODO: Add(new Coroutine(CollectRoutine(player)));

        _collider?.SetEnabled(false);

        // TODO: trigger cutscene
        // GameState.Instance.CollectHeartGem();
    }

    private IEnumerator CollectRoutine(MadelinePlayer player)
    {
        // TODO: implement full collection routine
        // - Stop music/ambience
        // - Collect strawberries
        // - Show poem
        // - Complete area

        yield return null;
    }

    /// <summary>
    /// Makes the player bounce off the heart without collecting it.
    /// </summary>
    public void PointBounce(Vector2 from)
    {
        _moveWiggleTimer = 0.8f;
        _scaleWiggleTimer = 0.5f;
        _moveWiggleDir = (Entity.Position - from).SafeNormalize(Vector2.UnitY);
    }
}
