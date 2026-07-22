using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Enemies;

/// <summary>
/// Bird that flings / carries the player through a set of waypoint nodes in a
/// scripted intro cutscene sequence.  Ported from Celeste's FlingBirdIntro.cs.
///
/// Two sub-modes:
///   crashes = false  – the "Miss the Bird" sequence (Farewell ch10).
///   crashes = true   – the "Catch the Bird" crash sequence.
///
/// After the player enters the trigger radius the <see cref="OnPlayerContact"/>
/// callback fires, locking player movement and driving the bird along its node
/// path via a manual Bezier curve lerp each segment.
/// </summary>
public class FlingBirdIntro : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    /// <summary>World position of the final node (destination end-point).</summary>
    public Vector2 BirdEndPosition { get; private set; }

    /// <summary>Whether this intro uses the crash variant.</summary>
    public bool Crashes { get; }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    private readonly Vector2[] _nodes;
    private readonly Vector2 _startPosition;

    /// <summary>Trigger radius for player contact (pixels).</summary>
    private const float TriggerRadius = 16f;

    /// <summary>Speed in px/s while flying to start position.</summary>
    private const float FlyToStartSpeed = 0.3f; // units: fraction per second

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private bool _startedRoutine;
    // Set when the intro cutscene starts; _emitParticles is referenced by the feather-particle
    // TODO further down but that emission isn't wired up yet, so neither field is read yet.
#pragma warning disable CS0414
    private bool _inCutscene;
    private bool _emitParticles;
#pragma warning restore CS0414

    // Fly-to state (bird entering from off-screen)
    private bool _flyingToStart;
    private Vector2 _flyFrom;
    private float _flyProgress; // 0→1

    // Hover sine bob
    private float _sineTimer;
    private Vector2 _hoverBasePos;

    // Node traversal state (during cutscene)
    private int _nodeIndex;
    private float _nodeProgress;   // 0→1 within current segment
    private float _nodeDuration;   // seconds for current segment

    // Lazy player reference
    private MadelinePlayer _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">Bird's world-space start position.</param>
    /// <param name="nodes">Ordered waypoints; final node = end destination.</param>
    /// <param name="crashes">True for the crash variant.</param>
    public FlingBirdIntro(Vector2 position, Vector2[] nodes, bool crashes)
    {
        _startPosition = position;
        _nodes = nodes;
        Crashes = crashes;
        BirdEndPosition = nodes.Length > 0 ? nodes[nodes.Length - 1] : position;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // TODO: add sprite renderer with "bird" atlas
        // TODO: play crashes ? "hoverStressed" : "hover" animation
        // TODO: add circle collider radius 16 (trigger)

        if (!Crashes)
        {
            // Bird enters from off-screen left, flies to start position
            _flyFrom = new Vector2(_startPosition.X - 150f, _startPosition.Y - 200f);
            Entity.Position = _flyFrom;
            _flyingToStart = true;
            _flyProgress = 0f;
            // TODO: play sound: event:/new_content/game/10_farewell/bird_flappyscene_entry
            // TODO: play "fly" animation
        }
        else
        {
            Entity.Position = _startPosition;
            _hoverBasePos = _startPosition;
        }
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        if (_startedRoutine)
        {
            UpdateCutscene(dt);
            return;
        }

        if (_flyingToStart)
        {
            UpdateFlyToStart(dt);
        }
        else
        {
            UpdateHover(dt);
            CheckPlayerContact();
        }

        // TODO: emit feather particles: FlingBird.P_Feather if _emitParticles
    }

    // -------------------------------------------------------------------------
    // Pre-cutscene movement
    // -------------------------------------------------------------------------

    private void UpdateFlyToStart(float dt)
    {
        _flyProgress += dt * FlyToStartSpeed;
        if (_flyProgress >= 1f)
        {
            _flyProgress = 1f;
            _flyingToStart = false;
            _hoverBasePos = _startPosition;
            Entity.Position = _startPosition;
            // TODO: play "hover" animation
        }
        else
        {
            // SineOut ease
            float eased = (float)Math.Sin(_flyProgress * Math.PI * 0.5);
            Entity.Position = Vector2.Lerp(_flyFrom, _startPosition, eased);
        }
    }

    private void UpdateHover(float dt)
    {
        _sineTimer += dt * 2f;
        Entity.Position = _hoverBasePos + Vector2.UnitY * (float)Math.Sin(_sineTimer) * 8f;
    }

    private void CheckPlayerContact()
    {
        if (_player == null) return;
        float dist = Vector2.Distance(Entity.Position, _player.Position);
        if (dist < TriggerRadius)
            OnPlayerContact();
    }

    // -------------------------------------------------------------------------
    // Cutscene (node traversal)
    // -------------------------------------------------------------------------

    private void OnPlayerContact()
    {
        if (_startedRoutine) return;
        _startedRoutine = true;
        _inCutscene = true;
        _emitParticles = true;
        _nodeIndex = 0;
        _nodeProgress = 0f;

        // TODO: play sound: crashes ?
        //   "event:/new_content/game/10_farewell/bird_crashscene_start"
        //   "event:/new_content/game/10_farewell/bird_flappyscene"
        // TODO: play "hoverStressed" animation on bird sprite
        // TODO: lock player state to dummy/grab

        if (_nodes.Length > 0)
            _nodeDuration = ComputeSegmentDuration(0);
    }

    private void UpdateCutscene(float dt)
    {
        if (_nodeIndex >= _nodes.Length - 1)
        {
            // Arrived at final node – cutscene complete
            // TODO: release player, transition to next room
            return;
        }

        _nodeProgress += dt / Math.Max(0.01f, _nodeDuration);
        if (_nodeProgress >= 1f)
        {
            _nodeProgress = 0f;
            _nodeIndex++;
            if (_nodeIndex >= _nodes.Length - 1) return;
            _nodeDuration = ComputeSegmentDuration(_nodeIndex);
        }

        // Evaluate Bezier position for current segment
        Vector2 from = _nodeIndex == 0 ? _startPosition : _nodes[_nodeIndex - 1];
        Vector2 to   = _nodes[_nodeIndex];
        // Quadratic Bezier with control point lifted slightly upward
        Vector2 ctrl = (from + to) * 0.5f + new Vector2(0f, -24f);
        float p = _nodeProgress;
        Entity.Position = (float)Math.Pow(1 - p, 2) * from
                        + 2f * (1 - p) * p * ctrl
                        + p * p * to;

        // Add sine wobble during carry
        _sineTimer += dt * 10f;
        Entity.Position += Vector2.UnitY * (float)Math.Sin(_sineTimer) * 8f;

        // TODO: update player position = Entity.Position + (2, 10) offset
        // TODO: adjust player offset per bird animation frame (1→3 frame offsets)
    }

    private float ComputeSegmentDuration(int segIndex)
    {
        // Approximate duration based on segment length / 100 px/s
        if (segIndex >= _nodes.Length - 1) return 1f;
        Vector2 from = segIndex == 0 ? _startPosition : _nodes[segIndex - 1];
        Vector2 to   = _nodes[segIndex];
        float len = Vector2.Distance(from, to);
        float dur = len / 100f;
        // Apply same heuristic adjustments as original
        if (to.Y < from.Y) dur *= 1.1f; else dur *= 0.8f;
        if (!Crashes)
        {
            if (segIndex == 0) dur = 0.7f;
            if (segIndex == 1) dur += 0.191f;
            if (segIndex == 2) dur += 0.191f;
        }
        return Math.Max(0.1f, dur);
    }
}
