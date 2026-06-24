using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Collectibles;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's FlingBird.cs.
///
/// A bird that grabs and flings the player. Waits at specific locations
/// and follows node paths. Can be skipped if the player passes it.
/// </summary>
public class FlingBird : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    public const float SkipDist = 100f;
    public static readonly Vector2 FlingSpeed = new Vector2(380f, -100f);

    // -------------------------------------------------------------------------
    // Enum
    // -------------------------------------------------------------------------

    private enum States
    {
        Wait,
        Fling,
        Move,
        WaitForLightningClear,
        Leaving
    }

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>List of node segments this bird travels through.</summary>
    public List<Vector2[]> NodeSegments { get; private set; } = new();

    /// <summary>List of whether each segment waits for the player.</summary>
    public List<bool> SegmentsWaiting { get; private set; } = new();

    /// <summary>Whether lightning has been removed from the level.</summary>
    public bool LightningRemoved { get; set; }

    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const float MoveSpeed = 100f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _spriteOffset = new Vector2(0f, 8f);
    private States _state;
    private Vector2 _flingSpeed;
    private Vector2 _flingTargetSpeed;
    private float _flingAccel;
    private Color _trailColor = new Color(0x63, 0x9B, 0xFF);
    private int _segmentIndex;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public FlingBird(Vector2[] nodes, bool skippable)
    {
        // Entity.Depth = -1; // TODO: not supported in Nez
        NodeSegments.Add(nodes);
        SegmentsWaiting.Add(skippable);
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // 16 radius circle collider
        var collider = Entity.AddComponent(new CircleCollider(16f));
        collider.IsTrigger = true;

        // TODO: load sprite "bird"
        // TODO: play hover animation
        // TODO: set up sprite scale and position
        // TODO: add transition listener

        // Merge with other birds in the same level
        MergeWithOtherBirds();

        // Check if already passed
        var player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player != null && player.Position.X > Entity.Position.X)
        {
            Entity.Destroy();
            return;
        }

        // Set up initial animation state
        if (SegmentsWaiting[0])
        {
            // TODO: sprite.Play("hoverStressed");
            // TODO: sprite.Scale.X = 1f;
        }
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    private void MergeWithOtherBirds()
    {
        // Find all birds in the same level and merge their segments
        // This is done so birds can be defined as separate entities but act as one
        // TODO: implement segment merging
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        // Smooth sprite position towards offset when not waiting
        if (_state != States.Wait)
        {
            // TODO: sprite.Position = Calc.Approach(sprite.Position, _spriteOffset, 32f * dt);
        }

        switch (_state)
        {
            case States.Wait:
                UpdateWaitState();
                break;

            case States.Fling:
                // Update fling physics
                if (_flingAccel > 0f)
                {
                    _flingSpeed = Calc.Approach(_flingSpeed, _flingTargetSpeed, _flingAccel * dt);
                }
                Entity.Position += _flingSpeed * dt;
                break;

            case States.WaitForLightningClear:
                // TODO: check if lightning is cleared
                // if (Scene.Entities.FindFirst<Lightning>() == null || X > level.Bounds.Right)
                {
                    // TODO: sprite.Scale.X = 1f;
                    _state = States.Leaving;
                    // TODO: Add(new Coroutine(LeaveRoutine()));
                }
                break;
        }
    }

    private void UpdateWaitState()
    {
        var player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        // Check if player is far past (skip)
        if (player.Position.X - Entity.Position.X >= SkipDist)
        {
            Skip();
            return;
        }

        // Check if waiting for lightning removal
        if (SegmentsWaiting[_segmentIndex] && LightningRemoved)
        {
            Skip();
            return;
        }

        // Sprite follows player when close
        float dist = (player.Position - Entity.Position).Length();
        if (dist < 64f)
        {
            float approachDist = Calc.ClampedMap(dist, 16f, 64f, 12f, 0f);
            Vector2 targetOffset = _spriteOffset + (player.Position - Entity.Position).SafeNormalize(Vector2.UnitY) * approachDist;
            // TODO: sprite.Position = Calc.Approach(sprite.Position, targetOffset, 32f * Time.DeltaTime);
        }
    }

    // -------------------------------------------------------------------------
    // Interaction
    // -------------------------------------------------------------------------

    private void OnPlayer(MadelinePlayer player)
    {
        if (_state != States.Wait) return;
        if (!player.DoFlingBird(this)) return;

        // Start fling sequence
        _flingSpeed = player.Speed * 0.4f;
        _flingSpeed.Y = 120f;
        _flingTargetSpeed = Vector2.Zero;
        _flingAccel = 1000f;
        player.Speed = Vector2.Zero;

        _state = States.Fling;
        // TODO: Add(new Coroutine(DoFlingRoutine(player)));

        // TODO: play sound: event:/new_content/game/10_farewell/bird_throw
    }

    private void Skip()
    {
        _state = States.Move;
        // TODO: Add(new Coroutine(MoveRoutine()));
    }

    // -------------------------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------------------------

    private IEnumerator DoFlingRoutine(MadelinePlayer player)
    {
        // var level = Scene as Level; // TODO: Level type not available

        // TODO: zoom camera
        // TODO: Engine.TimeRate = 0.8f;
        // TODO: rumble

        // Wait for fling to complete initial phase
        while (_flingSpeed != Vector2.Zero)
            yield return null;

        // TODO: sprite.Play("throw");
        // TODO: sprite.Scale.X = 1f;

        _flingSpeed = new Vector2(-140f, 140f);
        _flingTargetSpeed = Vector2.Zero;
        _flingAccel = 1400f;

        yield return 0.1f;

        // TODO: Freeze(0.05f);

        _flingTargetSpeed = FlingSpeed;
        _flingAccel = 6000f;

        yield return 0.1f;

        // TODO: rumble strong
        // TODO: Engine.TimeRate = 1f;
        // TODO: level.Shake();
        // TODO: zoom back

        // TODO: player.FinishFlingBird();

        _flingTargetSpeed = Vector2.Zero;
        _flingAccel = 4000f;

        yield return 0.3f;

        // TODO: Add(new Coroutine(MoveRoutine()));
    }

    private IEnumerator MoveRoutine()
    {
        _state = States.Move;
        // TODO: sprite.Play("fly");
        // TODO: sprite.Scale.X = 1f;
        // TODO: play sound: event:/new_content/game/10_farewell/bird_relocate

        // Move through nodes in current segment
        var segment = NodeSegments[_segmentIndex];
        for (int i = 1; i < segment.Length - 1; i += 2)
        {
            yield return MoveOnCurve(Entity.Position, segment[i], segment[i + 1]);
        }

        _segmentIndex++;
        bool atEnding = _segmentIndex >= NodeSegments.Count;

        if (!atEnding)
        {
            // Move to next segment
            var nextSegment = NodeSegments[_segmentIndex];
            yield return MoveOnCurve(Entity.Position, segment[segment.Length - 1], nextSegment[0]);
        }

        // TODO: sprite.Rotation = 0f;
        // TODO: sprite.Scale = Vector2.One;

        if (atEnding)
        {
            if (LightningRemoved)
            {
                _state = States.WaitForLightningClear;
            }
            else
            {
                _state = States.Leaving;
                yield return LeaveRoutine();
            }
        }
        else if (SegmentsWaiting[_segmentIndex])
        {
            _state = States.Wait;
            // TODO: sprite.Play("hoverStressed");
        }
        else
        {
            yield return MoveRoutine();
        }
    }

    private IEnumerator LeaveRoutine()
    {
        // Fly off screen
        Vector2 target = Entity.Position + new Vector2(200f, -200f);
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.DeltaTime;
            float t = elapsed / duration;
            Entity.Position = Vector2.Lerp(Entity.Position, target, t);
            yield return null;
        }

        Entity.Destroy();
    }

    private IEnumerator MoveOnCurve(Vector2 from, Vector2 anchor, Vector2 to)
    {
        float duration = Vector2.Distance(from, to) / MoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.DeltaTime;
            float t = elapsed / duration;
            float u = 1f - t;

            // Quadratic bezier
            Entity.Position = u * u * from + 2f * u * t * anchor + t * t * to;

            // TODO: rotate sprite to movement direction
            // TODO: emit feathers

            yield return null;
        }

        Entity.Position = to;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static class Calc
    {
        public static float Approach(float val, float target, float maxMove)
        {
            return val > target ? Math.Max(val - maxMove, target) : Math.Min(val + maxMove, target);
        }

        public static Vector2 Approach(Vector2 val, Vector2 target, float maxMove)
        {
            return new Vector2(
                Approach(val.X, target.X, maxMove),
                Approach(val.Y, target.Y, maxMove));
        }

        public static float ClampedMap(float value, float minInput, float maxInput, float minOutput, float maxOutput)
        {
            float t = (value - minInput) / (maxInput - minInput);
            t = Math.Clamp(t, 0f, 1f);
            return minOutput + t * (maxOutput - minOutput);
        }
    }
}

/// <summary>
/// Extension for MadelinePlayer to handle bird interactions.
/// </summary>
public static class FlingBirdPlayerExtensions
{
    public static bool DoFlingBird(this MadelinePlayer player, FlingBird bird)
    {
        // TODO: implement fling bird interaction check
        return true;
    }

    public static void FinishFlingBird(this MadelinePlayer player)
    {
        // TODO: implement finish fling bird
    }
}
