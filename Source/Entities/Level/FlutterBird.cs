using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Small decorative flutter bird that hops around its spawn point and flies away
/// when the player gets too close (startling nearby birds too).
/// Ported from Celeste's FlutterBird.cs.
/// </summary>
public class FlutterBird : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Colour palette
    // -------------------------------------------------------------------------

    private static readonly Color[] Colors =
    {
        new Color(0x89, 0xFB, 0xFF), // aqua
        new Color(0xF0, 0xFC, 0x6C), // lime yellow
        new Color(0xF4, 0x93, 0xFF), // lavender
        new Color(0x93, 0xBA, 0xFF), // sky blue
    };

    // -------------------------------------------------------------------------
    // State machine
    // -------------------------------------------------------------------------

    private enum BirdState { Idle, Hopping, FlyAway }
    private BirdState _state = BirdState.Idle;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly Vector2 _startPosition;
    private bool             _flyingAway;

    // Idle/hop timing
    private float   _idleTimer;
    private float   _idleDelay;

    // Bezier hop
    private Vector2 _hopFrom, _hopTo, _hopCtrl;
    private float   _hopProgress;
    private const float HopSpeed = 4f;  // 0→1 per second

    // Fly-away
    private Vector2 _flySpeed;
    private float   _flyAwayDelay;
    private bool    _flyAwayPending;

    // Scale squash/stretch
    private Vector2 _scale = Vector2.One;

    private MadelinePlayer? _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public FlutterBird(Vector2 position)
    {
        _startPosition = position;
        _idleDelay     = 0.25f + Nez.Random.NextFloat();
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _startPosition;

        // TODO: add "flutterbird" sprite renderer
        // TODO: sprite.Color = Nez.Random.Choose(Colors)
        // TODO: play sound loop: event:/game/general/birdbaby_tweet_loop
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        // Smooth scale back to 1
        _scale.X = Approach(_scale.X, Math.Sign(_scale.X), 4f * dt);
        _scale.Y = Approach(_scale.Y, 1f, 4f * dt);
        // TODO: apply _scale to sprite

        // Handle pending fly-away with delay
        if (_flyAwayPending)
        {
            _flyAwayDelay -= dt;
            if (_flyAwayDelay <= 0f)
            {
                _flyAwayPending = false;
                BeginFlyAway();
            }
        }

        switch (_state)
        {
            case BirdState.Idle:
                UpdateIdle(dt);
                break;
            case BirdState.Hopping:
                UpdateHopping(dt);
                break;
            case BirdState.FlyAway:
                UpdateFlyAway(dt);
                break;
        }
    }

    // -------------------------------------------------------------------------
    // State handlers
    // -------------------------------------------------------------------------

    private void UpdateIdle(float dt)
    {
        // Check player proximity
        if (!_flyingAway && _player != null)
        {
            float dx = Math.Abs(_player.Position.X - Entity.Position.X);
            float dy = _player.Position.Y - Entity.Position.Y;
            if (dx < 48f && dy > -40f && dy < 8f)
            {
                FlyAway(Math.Sign(Entity.Position.X - _player.Position.X),
                        Nez.Random.NextFloat() * 0.2f);
                return;
            }
        }

        _idleTimer += dt;
        if (_idleTimer >= _idleDelay)
        {
            _idleTimer = 0f;
            _idleDelay = 0.25f + Nez.Random.NextFloat();

            // Begin hop
            _hopFrom = Entity.Position;
            _hopTo   = _startPosition + new Vector2(Nez.Random.NextFloat() * 8f - 4f, 0f);
            _hopCtrl = (_hopFrom + _hopTo) * 0.5f - Vector2.UnitY * 14f;
            _hopProgress = 0f;

            // Face direction of hop
            float faceDir = Math.Sign(_hopTo.X - _hopFrom.X);
            if (faceDir == 0f) faceDir = 1f;
            _scale.X = faceDir;

            // TODO: play sound: event:/game/general/birdbaby_hop
            _state = BirdState.Hopping;
        }
    }

    private void UpdateHopping(float dt)
    {
        _hopProgress += dt * HopSpeed;
        if (_hopProgress >= 1f)
        {
            _hopProgress = 1f;
            Entity.Position = _hopTo;
            // Landing squash
            _scale.X = Math.Sign(_scale.X) * 1.4f;
            _scale.Y = 0.6f;
            _state = BirdState.Idle;
        }
        else
        {
            float p = _hopProgress;
            Entity.Position = (1 - p) * (1 - p) * _hopFrom
                            + 2 * (1 - p) * p      * _hopCtrl
                            + p * p                * _hopTo;
        }
    }

    private void UpdateFlyAway(float dt)
    {
        _flySpeed += new Vector2((int)Math.Sign(_flySpeed.X) * 64f, -128f) * dt;
        Entity.Position += _flySpeed * dt;

        // Startle nearby birds
        // TODO: find nearby FlutterBird components within 48px and call FlyAway on them

        // Remove when above level bounds (simple Y check)
        if (Entity.Position.Y < -300f)
            Entity.Destroy();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void FlyAway(int direction, float delay)
    {
        if (_flyingAway) return;
        _flyingAway     = true;
        _flyAwayPending = true;
        _flyAwayDelay   = delay;
        // TODO: stop tweet loop sound
        // TODO: play sound: event:/game/general/birdbaby_flyaway
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void BeginFlyAway()
    {
        // Face away from player
        float dir = _player != null
            ? Math.Sign(Entity.Position.X - _player.Position.X)
            : 1f;
        if (dir == 0f) dir = 1f;

        _flySpeed = new Vector2(dir * 4f, -8f);

        // Stretch scale on launch
        _scale = new Vector2(-dir * 1.25f, 1.25f);

        // TODO: play "fly" animation
        // TODO: emit dust particle

        _state = BirdState.FlyAway;
    }

    private static float Approach(float v, float target, float maxDelta)
        => v < target ? Math.Min(v + maxDelta, target) : Math.Max(v - maxDelta, target);
}
