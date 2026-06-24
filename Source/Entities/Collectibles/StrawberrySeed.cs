using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Collectibles;

/// <summary>
/// Port of Celeste's StrawberrySeed.cs.
///
/// Individual seed that follows the player. When all seeds in a strawberry
/// are collected, the strawberry appears. Seeds can be lost if the player
/// takes damage.
/// </summary>
public class StrawberrySeed : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const float LoseDelay = 0.25f;
    private const float LoseGraceTime = 0.15f;

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Whether the seed has been collected (is following player or finished).</summary>
    public bool Collected => _hasLeader || _finished;

    /// <summary>The strawberry this seed belongs to.</summary>
    public Strawberry Strawberry { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _startPosition;
    private int _index;
    private bool _ghost;
    private bool _hasLeader;
    private bool _finished;
    private bool _losing;
    private float _canLoseTimer;
    private float _loseTimer;
    private MadelinePlayer? _player;

    private float _sineTimer;
    private float _shakerTime;
    private Vector2 _shakerOffset;

    private BoxCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public StrawberrySeed(Strawberry strawberry, Vector2 position, int index, bool ghost)
    {
        Strawberry = strawberry;
        _startPosition = position;
        _index = index;
        _ghost = ghost;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _startPosition;

        // 12x12 hitbox, centered
        _collider = Entity.AddComponent(new BoxCollider(-6f, -6f, 12f, 12f));
        _collider.IsTrigger = true;

        // Randomize sine wave phase
        _sineTimer = Nez.Random.NextFloat() * MathF.PI * 2f;

        // TODO: load sprite based on ghost/gold/normal state
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        if (!_finished)
        {
            // Update lose timers
            if (_canLoseTimer > 0f)
                _canLoseTimer -= dt;
            // else if (_hasLeader && _player != null && _player.LoseShards) // TODO: LoseShards not implemented
            //     _losing = true;

            if (_losing)
            {
                if (_loseTimer <= 0f || (_player != null && _player.Speed.Y < 0f))
                {
                    LoseLeader();
                    _losing = false;
                }
                // else if (_player != null && _player.LoseShards) // TODO: LoseShards not implemented
                // {
                //     _loseTimer -= dt;
                // }
                else
                {
                    _loseTimer = LoseGraceTime;
                    _losing = false;
                }
            }

            // Update animation
            _sineTimer += dt;
            float sineX = MathF.Sin(_sineTimer) * 2f;
            float sineY = MathF.Sin(_sineTimer * 0.5f) * 1f;

            _shakerTime -= dt;
            if (_shakerTime > 0f)
            {
                _shakerOffset = new Vector2(
                    (Nez.Random.NextFloat() - 0.5f) * 2f,
                    (Nez.Random.NextFloat() - 0.5f) * 2f);
            }
            else
            {
                _shakerOffset = Vector2.Zero;
            }

            // TODO: sprite.Position = new Vector2(sineX, sineY) + _shakerOffset;
        }
        else
        {
            // Finished - fade out light
            // TODO: light.Alpha = Approach(light.Alpha, 0f, dt * 4f);
        }

        if (!_hasLeader && !_finished)
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
            Entity.Position.X - 6f,
            Entity.Position.Y - 6f,
            12f,
            12f);

        int count = Nez.Physics.OverlapRectangleAll(ref rect, _overlapResults);

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
        if (_hasLeader || _finished) return;

        // TODO: play sound: event:/game/general/seed_touch

        _player = player;
        _hasLeader = true;
        _canLoseTimer = LoseDelay;
        _loseTimer = LoseGraceTime;

        _collider?.SetEnabled(false);
        // Entity.Depth = -1000000; // TODO: not supported in Nez

        // Check if all seeds collected
        // Strawberry.CheckAllSeedsCollected(); // TODO: implement seed tracking
    }

    private void LoseLeader()
    {
        if (_finished) return;

        _hasLeader = false;
        _player = null;

        // TODO: Add(new Coroutine(ReturnRoutine()));
    }

    private IEnumerator ReturnRoutine()
    {
        // TODO: play poof sound

        _collider?.SetEnabled(false);
        // TODO: sprite.Scale = Vector2.One * 2f;

        yield return 0.05f;

        // TODO: rumble
        // TODO: emit burst particles

        // Entity.Visible = false; // TODO: not supported in Nez

        yield return 0.3f + _index * 0.1f;

        // TODO: play reappear sound
        Entity.Position = _startPosition;
        _shakerTime = 0.4f;
        // TODO: sprite.Scale = Vector2.One;
        // Entity.Visible = true; // TODO: not supported in Nez
        _collider?.SetEnabled(true);

        // TODO: emit displacement burst
    }

    /// <summary>
    /// Called when all seeds in the strawberry are collected.
    /// </summary>
    public void OnAllCollected()
    {
        _finished = true;
        _hasLeader = false;
        // Entity.Depth = -2000002; // TODO: not supported in Nez

        // TODO: start spin animation
    }

    /// <summary>
    /// Starts the spin animation towards the strawberry center.
    /// </summary>
    public void StartSpinAnimation(Vector2 averagePos, Vector2 centerPos, float angleOffset, float time)
    {
        float spinLerp = 0f;
        Vector2 start = Entity.Position;

        // TODO: sprite.Play("noFlash");

        // Animate towards center
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.DeltaTime;
            float t = Ease.CubeInOut(elapsed / time);
            float angle = 1.57079637f + angleOffset - MathHelper.Lerp(0f, 32.2013245f, t);
            Vector2 midPoint = Vector2.Lerp(averagePos, centerPos, spinLerp);
            Entity.Position = Vector2.Lerp(start, midPoint + new Vector2(MathF.Cos(angle) * 25f, MathF.Sin(angle) * 25f), spinLerp);
        }
    }

    /// <summary>
    /// Starts the combine animation to merge into the strawberry.
    /// </summary>
    public void StartCombineAnimation(Vector2 centerPos, float time)
    {
        Vector2 start = Entity.Position;
        float startAngle = MathF.Atan2(start.Y - centerPos.Y, start.X - centerPos.X);

        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.DeltaTime;
            float t = Ease.BigBackIn(elapsed / time);
            float angle = MathHelper.Lerp(startAngle, startAngle + MathF.PI * 2f, t);
            float dist = MathF.Sin(t * MathF.PI) * 25f;
            Entity.Position = centerPos + new Vector2(MathF.Cos(angle) * dist, MathF.Sin(angle) * dist);
        }
    }

    private void ShakeFor(float duration, bool strong)
    {
        _shakerTime = duration;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static class Ease
    {
        public static float CubeInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
        public static float BigBackIn(float t) => t * t * (7f * t - 6f);
    }
}

// Strawberry class is defined in Strawberry.cs
