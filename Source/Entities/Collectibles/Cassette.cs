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
/// Port of Celeste's Cassette.cs.
///
/// Cassette tape collectible that unlocks B-Side chapters. Shows a remix preview
/// and unlocks the harder variant of the current chapter.
/// </summary>
public class Cassette : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Whether this is a ghost cassette (already collected).</summary>
    public bool IsGhost { get; private set; }

    /// <summary>Whether the cassette has been collected.</summary>
    public bool Collected { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _spawnPosition;
    private Vector2[] _nodes;
    private bool _collecting;
    private bool _isGhost;
    private float _hoverTimer;

    private BoxCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Cassette(Vector2 position, Vector2[]? nodes = null)
    {
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

        // 16x16 hitbox, centered
        _collider = Entity.AddComponent(new BoxCollider(-8f, -8f, 16f, 16f));
        _collider.IsTrigger = true;

        // TODO: load sprite based on ghost state
        // TODO: set up bloom and light
    }

    public override void OnRemovedFromEntity()
    {
        base.OnRemovedFromEntity();
        // TODO: stop remix preview sound
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        if (!_collecting)
        {
            _hoverTimer += dt;
            float hoverOffset = MathF.Sin(_hoverTimer * 2f) * 2f;
            // TODO: sprite.Y = hoverOffset;

            // Emit shine particles periodically
            // TODO: if (Scene.OnInterval(0.1f)) emit particles
        }

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
        if (Collected) return;
        Collected = true;

        // TODO: play sound: event:/game/general/cassette_get
        // TODO: freeze game for 0.1f
        // TODO: refill player stamina

        _collider?.SetEnabled(false);

        // TODO: Add(new Coroutine(CollectRoutine(player)));
    }

    private IEnumerator CollectRoutine(MadelinePlayer player)
    {
        _collecting = true;

        // TODO: set up level effects
        // - Pause lock
        // - Freeze level
        // - Set cassette flag
        // - Stop cassette block manager

        // Camera zoom and effects
        yield return 0.1f;

        // TODO: shake and flash
        // TODO: clear displacement

        // Spin animation
        float spinDuration = 1.5f;
        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.DeltaTime;
            // TODO: increase spin rate
            yield return null;
        }

        // Launch up animation
        Vector2 start = Entity.Position;
        Vector2 end = new Vector2(Entity.Position.X, Entity.Position.Y - 100f);
        float launchDuration = 0.4f;
        elapsed = 0f;

        while (elapsed < launchDuration)
        {
            elapsed += Time.DeltaTime;
            float t = Ease.CubeIn(elapsed / launchDuration);
            Entity.Position = Vector2.Lerp(start, end, t);
            // TODO: squash and stretch sprite
            yield return null;
        }

        // Entity.Visible = false; // TODO: not supported in Nez

        // TODO: play remix preview sound
        // TODO: show "Unlocked B-Side" message

        // Wait for player confirmation
        // while (!Input.MenuConfirm.Pressed) yield return null;

        // Return camera to normal
        yield return 0.25f;

        // TODO: if nodes exist, start cassette fly
        if (_nodes.Length >= 2)
        {
            // TODO: player.StartCassetteFly(_nodes[1], _nodes[0]);
        }

        // Cleanup
        _collecting = false;
        Entity.Destroy();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static class Ease
    {
        public static float CubeIn(float t) => t * t * t;
    }
}
