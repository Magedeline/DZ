using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Ambient moon creature that floats around a spawn origin and optionally
/// follows the player when touched.  Ported from Celeste's MoonCreature.cs.
///
/// Each creature has a 10-node trail that lags behind with spring-like
/// physics, giving a worm/ribbon visual effect.
/// </summary>
public class MoonCreature : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const float Acceleration       = 90f;
    private const float FollowAcceleration = 120f;
    private const float MaxSpeed           = 40f;
    private const float MaxFollowSpeed     = 70f;
    private const float MaxFollowDistance  = 200f;
    private const int   TrailLength        = 10;

    // -------------------------------------------------------------------------
    // Nested types
    // -------------------------------------------------------------------------

    private struct TrailNode
    {
        public Vector2 Position;
        public Color   Color;
    }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly Vector2  _startPosition;
    private Vector2           _target;
    private float             _targetTimer;
    private Vector2           _speed;
    private Vector2           _bump;

    private MadelinePlayer?   _following;
    private Vector2           _followOffset;
    private float             _followTime;

    private readonly TrailNode[] _trail;
    private readonly Color       _orbColor;
    private readonly Color       _centerColor;

    private MadelinePlayer? _player;

    // Spawn count for additional creatures
    private readonly int _spawnExtra;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">Spawn / roam origin.</param>
    /// <param name="spawnExtra">Additional creatures to spawn alongside this one.</param>
    public MoonCreature(Vector2 position, int spawnExtra = 0)
    {
        _startPosition = position;
        _spawnExtra    = spawnExtra;

        // Colour
        _orbColor    = new Color(0xB0, 0xE6, 0xFF);
        _centerColor = Nez.Random.Choose(
            new Color(0xC3, 0x4F, 0xC7),
            new Color(0x4F, 0x95, 0xC7),
            new Color(0x53, 0xC7, 0x4F));

        Color c1 = Color.Lerp(_centerColor, new Color(0xBD, 0xE4, 0xEE), 0.5f);
        Color c2 = Color.Lerp(_centerColor, new Color(0x2F, 0x29, 0x41), 0.5f);

        _trail = new TrailNode[TrailLength];
        for (int i = 0; i < TrailLength; i++)
            _trail[i] = new TrailNode
            {
                Position = position,
                Color    = Color.Lerp(c1, c2, i / (float)(TrailLength - 1))
            };

        // Start with a random target
        PickRandomTarget();
        _target = position + Nez.Random.NextUnitVector() * Nez.Random.NextFloat() * 32f;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _startPosition;

        // TODO: add BoxCollider 20x20, offset (-10,-10), trigger
        // TODO: add "moonCreatureTiny" sprite renderer

        // Spawn additional creatures if requested
        for (int i = 0; i < _spawnExtra; i++)
        {
            var extra = new MoonCreature(
                _startPosition + new Vector2(
                    Nez.Random.Range(-4f, 4f),
                    Nez.Random.Range(-4f, 4f)));
            Entity.Scene?.AddEntity(new Nez.Entity()).AddComponent(extra);
        }
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntityOfType<MadelinePlayer>();

        if (_following == null)
        {
            _targetTimer -= dt;
            if (_targetTimer <= 0f)
            {
                _targetTimer = Nez.Random.Range(0.8f, 4f);
                PickRandomTarget();
            }
        }
        else
        {
            _followTime  -= dt;
            _targetTimer -= dt;
            if (_targetTimer <= 0f)
            {
                _targetTimer = Nez.Random.Range(0.8f, 2f);
                PickFollowOffset();
            }
            _target = _following.Position + _followOffset;

            float distToStart = Vector2.Distance(Entity.Position, _startPosition);
            if (distToStart > MaxFollowDistance || _followTime <= 0f)
            {
                _following   = null;
                _targetTimer = 0f;
            }
        }

        // Steering
        float accel = _following != null ? FollowAcceleration : Acceleration;
        float maxSpd = _following != null ? MaxFollowSpeed   : MaxSpeed;

        Vector2 dir = _target - Entity.Position;
        if (dir.Length() > 0f)
            _speed += Vector2.Normalize(dir) * accel * dt;
        float spd = _speed.Length();
        if (spd > maxSpd && spd > 0f)
            _speed = _speed / spd * maxSpd;

        // Bump decay
        float bumpLen = _bump.Length();
        if (bumpLen > 0f)
            _bump = _bump / bumpLen * Math.Max(0f, bumpLen - dt * 80f);

        Entity.Position += (_speed + _bump) * dt;
        // TODO: clamp to level bounds

        // Trail physics – each node chases the one in front
        Vector2 prev = Entity.Position;
        for (int i = 0; i < _trail.Length; i++)
        {
            Vector2 toNode = _trail[i].Position - prev;
            float   dLen   = toNode.Length();
            Vector2 norm   = dLen > 0f ? toNode / dLen : new Vector2(0f, 1f);
            norm.Y += 0.05f; // slight gravity bias
            Vector2 trailTarget = prev + norm * 2f;
            float   travelDist  = Math.Min(dLen, 128f * dt);
            if (dLen > 0f)
                _trail[i].Position += (trailTarget - _trail[i].Position) / dLen * travelDist;
            else
                _trail[i].Position = trailTarget;
            prev = _trail[i].Position;
        }
    }

    // -------------------------------------------------------------------------
    // Rendering (call from scene renderer)
    // -------------------------------------------------------------------------

    public void Render()
    {
        // Render trail from back to front
        for (int i = _trail.Length - 1; i >= 0; i--)
        {
            float size = MathHelper.Lerp(3f, 1f, i / (float)(_trail.Length - 1));
            // TODO: Draw.Rect(_trail[i].Position, size, size, _trail[i].Color)
        }
        // TODO: render sprite at Entity.Position (snap to floor)
    }

    // -------------------------------------------------------------------------
    // Player contact
    // -------------------------------------------------------------------------

    public void OnPlayerContact(MadelinePlayer player)
    {
        Vector2 push = (Entity.Position - player.Position);
        float   pLen = push.Length();
        if (pLen > 0f) push = push / pLen * (player.Speed.Length() * 0.3f);
        if (push.LengthSquared() > _bump.LengthSquared())
            _bump = push;

        float distToStart = Vector2.Distance(player.Position, _startPosition);
        if (distToStart < MaxFollowDistance && _following == null)
        {
            _following   = player;
            _followTime  = Nez.Random.Range(6f, 12f);
            PickFollowOffset();
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void PickRandomTarget()
    {
        _target = _startPosition
                + Nez.Random.NextUnitVector() * Nez.Random.NextFloat() * 32f;
    }

    private void PickFollowOffset()
    {
        _followOffset = new Vector2(
            Nez.Random.Choose(-1, 1) * Nez.Random.Range(8f, 16f),
            Nez.Random.Range(-20f, 0f));
    }
}
