#pragma warning disable CS8632 // this file uses '?' on a few types without opting the whole file into nullable-reference-type analysis

using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Enemies;

/// <summary>
/// Bird NPC with path following, tutorial guidance, and flee-on-approach behaviour.
/// Ported from Celeste's BirdNPC.cs.
///
/// Modes:
///   <see cref="BirdMode.Idle"/>       – stands still, pecks occasionally.
///   <see cref="BirdMode.MoveToNodes"/> – follows an array of waypoints in order.
///   <see cref="BirdMode.FlyAway"/>    – waits until player approaches, then startles and flies off.
///   <see cref="BirdMode.Sleeping"/>   – plays sleep animation, ignores player.
/// </summary>
public class BirdNPC : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public types
    // -------------------------------------------------------------------------

    public enum BirdMode
    {
        Idle,
        MoveToNodes,
        FlyAway,
        Sleeping,
        ClimbingTutorial,
        DashingTutorial,
    }

    public enum Facing { Left = -1, Right = 1 }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    /// <summary>Current facing direction.</summary>
    public Facing CurrentFacing { get; set; } = Facing.Left;

    /// <summary>Whether to fly upward (true) or downward when fleeing.</summary>
    public bool FlyAwayUp { get; set; } = true;

    /// <summary>Waypoint nodes for <see cref="BirdMode.MoveToNodes"/>.</summary>
    public Vector2[] Nodes { get; set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private BirdMode _mode;
    private Vector2 _startPosition;

    // Node-following state
    private int _nodeIndex;
    private float _nodeMoveSpeed = 80f;

    // Fly-away state
    private bool _flyingAway;
    private Vector2 _flySpeed;

    // Idle animation timer
    private float _peckTimer;

    // Reference to player
    private MadelinePlayer? _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">Spawn position in world space.</param>
    /// <param name="mode">Starting behaviour mode.</param>
    public BirdNPC(Vector2 position, BirdMode mode = BirdMode.Idle)
    {
        _spawnPosition = position;
        _mode = mode;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;
        _startPosition = _spawnPosition;

        // TODO: add sprite renderer with "bird" atlas animations
        // TODO: add VertexLight component (offset 0,-8; radius 8-32)

        SetMode(_mode);
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        // TODO: update sprite scale X from CurrentFacing

        if (_flyingAway)
        {
            UpdateFlyAway(dt);
            return;
        }

        switch (_mode)
        {
            case BirdMode.Idle:
            case BirdMode.ClimbingTutorial:
            case BirdMode.DashingTutorial:
                UpdateIdle(dt);
                break;
            case BirdMode.MoveToNodes:
                UpdateMoveToNodes(dt);
                break;
            case BirdMode.FlyAway:
                UpdateFlyAwayWatch(dt);
                break;
            case BirdMode.Sleeping:
                // No movement; just play sleep anim
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Mode switching
    // -------------------------------------------------------------------------

    public void SetMode(BirdMode mode)
    {
        _mode = mode;
        _flyingAway = false;
        _nodeIndex = 0;

        switch (mode)
        {
            case BirdMode.Sleeping:
                // TODO: play "sleep" animation; face right
                CurrentFacing = Facing.Right;
                break;
            case BirdMode.MoveToNodes:
                // TODO: play "fly" animation
                break;
            default:
                // TODO: play "idle" / "peck" animation cycle
                break;
        }
    }

    // -------------------------------------------------------------------------
    // State update helpers
    // -------------------------------------------------------------------------

    private void UpdateIdle(float dt)
    {
        _peckTimer -= dt;
        if (_peckTimer <= 0f)
        {
            _peckTimer = 1f + DZ.Nez.Random.NextFloat() * 2f;
            // TODO: play "peck" animation
            // TODO: play sound: event:/game/general/bird_peck
        }

        // React to close player
        if (_player != null)
        {
            float dx = Math.Abs(_player.Position.X - Entity.Position.X);
            if (dx < 48f)
                StartFlyAway();
        }
    }

    private void UpdateMoveToNodes(float dt)
    {
        if (Nodes == null || Nodes.Length == 0) return;
        if (_nodeIndex >= Nodes.Length) return;

        Vector2 target = Nodes[_nodeIndex];
        Vector2 dir = target - Entity.Position;
        float dist = dir.Length();

        if (dist < 4f)
        {
            Entity.Position = target;
            _nodeIndex++;
            if (_nodeIndex >= Nodes.Length)
            {
                // All nodes visited – idle
                _mode = BirdMode.Idle;
            }
        }
        else
        {
            CurrentFacing = dir.X >= 0 ? Facing.Right : Facing.Left;
            Entity.Position += Vector2.Normalize(dir) * _nodeMoveSpeed * dt;
        }
    }

    private void UpdateFlyAwayWatch(float dt)
    {
        if (_player == null) return;
        float dx = Math.Abs(_player.Position.X - Entity.Position.X);
        float dy = _player.Position.Y - Entity.Position.Y;
        if (dx < 80f && dy > -16f && dy < 32f)
            StartFlyAway();
    }

    private void UpdateFlyAway(float dt)
    {
        // TODO: apply actual level bounds check; here we use a simple timer/position
        _flySpeed += new Vector2((int)CurrentFacing * 140f, FlyAwayUp ? -120f : 120f) * dt;
        Entity.Position += _flySpeed * dt;

        // Remove when far offscreen (simple Y-based check)
        if (FlyAwayUp && Entity.Position.Y < -200f)
            Entity.Destroy();
        else if (!FlyAwayUp && Entity.Position.Y > 2000f)
            Entity.Destroy();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Startles the bird (brief pause) then flies away.</summary>
    public void StartFlyAway(float delay = 0f)
    {
        if (_flyingAway) return;
        _flyingAway = true;
        CurrentFacing = (Facing)(-(int)CurrentFacing); // flip direction
        _flySpeed = new Vector2((int)CurrentFacing * 20f, FlyAwayUp ? -40f : 40f);

        // TODO: play sound: event:/game/general/bird_startle
        // TODO: play "fly" animation
        // TODO: session flag: bird_fly_away_<level>
    }

    /// <summary>Plays the "caw" croak animation and sound.</summary>
    public void Caw()
    {
        // TODO: play "croak" animation
        // TODO: play sound: event:/game/general/bird_squawk
    }
}
