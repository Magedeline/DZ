using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Enemies;

/// <summary>
/// Badeline (Old Site chase enemy). Ported from Celeste's BadelineOldsite.cs.
///
/// Follows the player through rooms, mirroring their recorded position history
/// with a delay. Speeds up when the player is fast. Kills on contact.
/// Laughs and hovers when the player is dead.
/// </summary>
public class BadelineOldsite : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Hair colour matching original Badeline purple.</summary>
    public static readonly Color HairColor = new Color(0x9B, 0x3F, 0xB5);

    /// <summary>Max approach speed while chasing (px/s).</summary>
    private const float FollowApproachSpeed = 500f;

    /// <summary>Delay in seconds behind the player's recorded path.</summary>
    private const float FollowBehindTime = 1.55f;

    /// <summary>Extra delay per additional index (for multi-Badeline rooms).</summary>
    private const float IndexDelay = 0.4f;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private enum State { Waiting, Appearing, Following, StopChasing }

    private State _state = State.Waiting;

    /// <summary>Which Badeline index this is (0 = first, 1 = second …).</summary>
    public int Index { get; }

    /// <summary>Whether Badeline is in the hovering idle pose.</summary>
    public bool Hovering { get; set; }

    private float _hoveringTimer;
    private float _appearTimer;

    // Lerp-based tween state for the PopIn appearance move
    private bool _tweening;
    private Vector2 _tweenFrom;
    private Vector2 _tweenTo;
    private float _tweenTimer;
    private float _tweenDuration;

    // Lazy player reference
    private MadelinePlayer _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">World spawn position.</param>
    /// <param name="index">Badeline instance index (0-based).</param>
    public BadelineOldsite(Vector2 position, int index = 0)
    {
        _spawnPosition = position;
        Index = index;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // TODO: add PlayerSprite (Badeline mode) + PlayerHair components
        // TODO: add CircleCollider radius 3, trigger

        // Begin waiting for player to spawn / finish respawn
        _state = State.Waiting;
        Hovering = true;

        // TODO: session data check – if flag "11" set, remove self
        // TODO: session data check – if flag "3" NOT set, remove self

        // TODO: play sound: event:/music/lvl2/chase when chase begins
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Lazily acquire player reference
        _player ??= Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        // Hovering bob animation
        if (Hovering)
        {
            _hoveringTimer += dt * 2f;
            // TODO: apply Math.Sin(_hoveringTimer) * 4 to sprite X offset
        }

        switch (_state)
        {
            case State.Waiting:
                UpdateWaiting(dt);
                break;
            case State.Appearing:
                UpdateAppearing(dt);
                break;
            case State.Following:
                UpdateFollowing(dt);
                break;
            case State.StopChasing:
                UpdateStopChasing(dt);
                break;
        }
    }

    // -------------------------------------------------------------------------
    // State handlers
    // -------------------------------------------------------------------------

    private void UpdateWaiting(float dt)
    {
        if (_player == null) return;

        // Wait for player to be alive and settled
        _appearTimer += dt;
        if (_appearTimer < IndexDelay * Index) return;

        // Start appearing: tween from current position to behind-player position
        _tweenFrom = Entity.Position;
        _tweenTo = _player.Position;
        _tweenDuration = Math.Max(0.1f, FollowBehindTime - 0.1f);
        _tweenTimer = 0f;
        _tweening = true;
        Hovering = false;

        // TODO: play sound: event:/char/badeline/level_entry
        _state = State.Appearing;
    }

    private void UpdateAppearing(float dt)
    {
        if (_tweening)
        {
            _tweenTimer += dt;
            float t = Math.Min(1f, _tweenTimer / _tweenDuration);
            // CubeIn ease approximation
            float eased = t * t * t;
            Entity.Position = Vector2.Lerp(_tweenFrom, _tweenTo, eased);

            // TODO: emit trail particles at Entity.Position

            if (t >= 1f)
            {
                _tweening = false;
                _state = State.Following;
                // TODO: make collidable = true, add LightOcclude component
            }
        }
    }

    private void UpdateFollowing(float dt)
    {
        if (_player == null) { _state = State.Waiting; return; }

        // Chase: approach player's position from FollowBehindTime seconds ago.
        // In a full port this would read from player's chaser-state history buffer.
        // Here we directly approach the player's current position as a simplified stand-in.
        Vector2 target = _player.Position;
        Entity.Position = MoveToward(Entity.Position, target, FollowApproachSpeed * dt);

        // TODO: mirror player's current animation ID on the Badeline sprite
        // TODO: emit looping sounds mirrored from player ChaserState

        CheckPlayerContact();
    }

    private void UpdateStopChasing(float dt)
    {
        // TODO: session data check – stop chasing when reaching bounds target
        // TODO: play sound: event:/char/badeline/disappear, then RemoveSelf
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void CheckPlayerContact()
    {
        if (_player == null) return;
        float dist = Vector2.Distance(Entity.Position, _player.Position);
        if (dist < 8f)
        {
            // TODO: kill player – call player.Die() equivalent
        }
    }

    /// <summary>Laughs and hovers when player dies.</summary>
    public void OnPlayerDied()
    {
        Hovering = true;
        // TODO: play "laugh" animation on sprite
        // TODO: stop looping sounds
        // TODO: emit trail particles
    }

    private static Vector2 MoveToward(Vector2 current, Vector2 target, float maxDelta)
    {
        Vector2 diff = target - current;
        float dist = diff.Length();
        return dist <= maxDelta ? target : current + diff / dist * maxDelta;
    }
}
