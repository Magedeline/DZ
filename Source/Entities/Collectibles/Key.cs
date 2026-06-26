using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;
using Component = Nez.Component;
using Collider = Nez.Collider;

namespace KirbyCelesteStandalone.Entities.Collectibles;

/// <summary>
/// Port of Celeste's Key.cs.
///
/// A collectible key that can unlock LockBlock doors. The key follows the player
/// as a follower and can be used to unlock doors when the player approaches them.
/// </summary>
public class Key : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Unique identifier for this key.</summary>
    public string Id { get; private set; }

    /// <summary>Whether the key has been used to unlock a door.</summary>
    public bool IsUsed { get; private set; }

    /// <summary>Whether the key has started being used in the unlock sequence.</summary>
    public bool StartedUsing { get; set; }

    /// <summary>Whether the key is currently turning in the lock.</summary>
    public bool Turning { get; private set; }

    /// <summary>Whether the key is being held by the player.</summary>
    public bool IsHeld => _isFollowing;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private bool _collected;
    private bool _isFollowing;
    private MadelinePlayer? _holder;
    private Vector2 _spawnPosition;
    private Vector2[] _nodes;
    private float _wobble;
    private bool _wobbleActive;

    private BoxCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Key(Vector2 position, string id, Vector2[]? nodes = null)
    {
        Id = id;
        _spawnPosition = position;
        _nodes = nodes ?? Array.Empty<Vector2>();
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // 12x12 hitbox, centered
        _collider = Entity.AddComponent(new BoxCollider(-6f, -6f, 12f, 12f));
        _collider.IsTrigger = true;

        // TODO: load sprite — e.g. Entity.AddComponent(new SpriteRenderer(keyTexture));
        // TODO: play idle animation
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        if (_wobbleActive)
        {
            _wobble += dt * 4f;
            // TODO: animate sprite Y offset: sprite.Y = MathF.Sin(_wobble);
        }

        if (!_collected && _collider != null)
        {
            CheckPlayerOverlap();
        }

        // If following player, update position
        if (_isFollowing && _holder != null && !IsUsed)
        {
            // Key follows behind player at an offset
            float followOffsetX = -12f * _holder.Facing;
            Vector2 targetPos = _holder.Position + new Vector2(followOffsetX, -8f);
            Entity.Position = Vector2.Lerp(Entity.Position, targetPos, 0.2f);
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

            Collect(player);
            break;
        }
    }

    private void Collect(MadelinePlayer player)
    {
        if (_collected) return;

        _collected = true;
        _holder = player;
        _isFollowing = true;
        _wobbleActive = true;

        // TODO: play sound: event:/game/general/key_get
        // TODO: emit particles

        _collider?.SetEnabled(false);

        // TODO: track key in game state
        // GameState.Instance.CollectKey(Id);
    }

    /// <summary>
    /// Called by LockBlock when the key is used to unlock a door.
    /// </summary>
    public void RegisterUsed()
    {
        IsUsed = true;
        _isFollowing = false;

        // TODO: remove from game state tracking
        // GameState.Instance.RemoveKey(Id);
    }

    /// <summary>
    /// Routine for the key turning in the lock.
    /// </summary>
    public IEnumerator UseRoutine(Vector2 target)
    {
        Turning = true;
        _wobbleActive = false;
        // TODO: sprite.Y = 0;

        // TODO: play sound: event:/game/03_resort/key_unlock

        // Curve animation to target
        Vector2 start = Entity.Position;
        Vector2 control = (start + target) / 2f + new Vector2(0f, -48f);
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.DeltaTime;
            float t = Ease.CubeOut(elapsed / duration);
            Entity.Position = SimpleCurve.GetPoint(start, target, control, t);
            yield return null;
        }

        // TODO: play key insert animation, wait for frame 4
        yield return 0.3f;

        // TODO: play rotate animation
        yield return 0.3f;

        yield return 1.0f; // Wait before fading

        // Fade out
        elapsed = 0f;
        duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.DeltaTime;
            float alpha = 1f - Ease.CubeOut(elapsed / duration);
            // TODO: sprite.Color = Color.White * alpha;
            yield return null;
        }

        Turning = false;
        Entity.Destroy();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static class SimpleCurve
    {
        public static Vector2 GetPoint(Vector2 start, Vector2 end, Vector2 control, float t)
        {
            float u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }
    }

    private static class Ease
    {
        public static float CubeOut(float t) => 1f - MathF.Pow(1f - t, 3f);
    }
}
