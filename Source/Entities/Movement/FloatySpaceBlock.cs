using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Movement;

/// <summary>
/// Port of Celeste's FloatySpaceBlock.cs.
///
/// Solid blocks that float in space, oscillating vertically with a sine wave.
/// Adjacent blocks of the same tile-type form a group and move together.
/// The master block drives movement for the entire group each frame.
///
/// Celeste grouping detail faithfully reproduced; tile-grid GFX omitted
/// (TODO: hook up autotiler).
/// </summary>
public class FloatySpaceBlock : CelesteSolid
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float FloatAmplitude  = 4f;    // pixels of sine bob
    private const float FloatSpeed      = 1f;    // yLerp approach rate
    private const float SinkHold        = 0.3f;  // seconds to stay sunk after rider leaves
    private const float DashEaseDecay   = 1.5f;  // rate dashEase decays per second

    // ── Group identity ────────────────────────────────────────────────────────

    /// <summary>All <see cref="FloatySpaceBlock"/>s in this group (master-only).</summary>
    public List<FloatySpaceBlock> Group    { get; private set; } = null!;

    /// <summary>Original positions of every platform in the group (master-only).</summary>
    public Dictionary<FloatySpaceBlock, Vector2> Moves { get; private set; } = null!;

    public bool HasGroup       { get; private set; }
    public bool MasterOfGroup  { get; private set; }

    private FloatySpaceBlock _master = null!;
    private bool             _awake;

    // ── Per-block state ───────────────────────────────────────────────────────

    private readonly char  _tileType;

    // ── Master-only state ─────────────────────────────────────────────────────

    private float   _yLerp;
    private float   _sinkTimer;
    private float   _sineWave;
    private float   _dashEase;
    private Vector2 _dashDirection;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="FloatySpaceBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="tileType">Tile character used for autotiling (visual, not logic).</param>
    /// <param name="disableSpawnOffset">
    ///   When <c>true</c>, starts all blocks in sync (sine phase = 0).
    /// </param>
    public FloatySpaceBlock(
        Vector2 position,
        float   width,
        float   height,
        char    tileType           = '3',
        bool    disableSpawnOffset = false)
        : base(position, width, height, safe: false)
    {
        _tileType  = tileType;
        _sineWave  = disableSpawnOffset ? 0f : Nez.Random.NextFloat() * (float)(2 * Math.PI);
        Name       = "FloatySpaceBlock";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        _awake = true;

        if (!HasGroup)
        {
            MasterOfGroup = true;
            Moves         = new Dictionary<FloatySpaceBlock, Vector2>();
            Group         = new List<FloatySpaceBlock>();
            AddToGroupAndFindChildren(this);
        }

        if (MasterOfGroup)
            MoveToTarget();
        else
            _master.MoveToTarget();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        if (!MasterOfGroup) return;

        float dt = Time.DeltaTime;

        // Check if any group member has a player rider.
        bool hasRider = false;
        foreach (var block in Group)
        {
            if (block.HasPlayerRider())
            {
                hasRider = true;
                break;
            }
        }

        if (hasRider)
            _sinkTimer = SinkHold;
        else if (_sinkTimer > 0f)
            _sinkTimer -= dt;

        // Approach yLerp based on sink state.
        _yLerp = _sinkTimer > 0f
            ? Approach(_yLerp, 1f, FloatSpeed * dt)
            : Approach(_yLerp, 0f, FloatSpeed * dt);

        _sineWave += dt;
        _dashEase  = Approach(_dashEase, 0f, DashEaseDecay * dt);

        MoveToTarget();
    }

    // ── Group movement ────────────────────────────────────────────────────────

    private void MoveToTarget()
    {
        // Sine wave bob.
        float sineOffset = (float)Math.Sin(_sineWave) * FloatAmplitude;

        // Sink lerp (player weight).
        float sinkOffset = Lerp(0f, 12f, SineInOut(_yLerp));

        // Dash-push.
        Vector2 dashOffset = _dashEase > 0f
            ? _dashDirection * Lerp(0f, 8f, _dashEase)
            : Vector2.Zero;

        Vector2 baseOffset = new Vector2(dashOffset.X, sineOffset + sinkOffset + dashOffset.Y);

        foreach (var (block, origin) in Moves)
        {
            bool hasBoarder = block.HasPlayerRider();

            // In original: only move if rider state matches; simplified here.
            float targetY = origin.Y + baseOffset.Y;
            float targetX = origin.X + baseOffset.X;
            block.MoveToY(targetY);
            block.MoveToX(targetX);
        }
    }

    // ── Group building ────────────────────────────────────────────────────────

    private void AddToGroupAndFindChildren(FloatySpaceBlock from)
    {
        from.HasGroup = true;
        from._master  = this;
        Group.Add(from);
        Moves[from] = from.Position;

        // Scan scene for touching FloatySpaceBlocks of the same tile type.
        if (Scene == null) return;
        for (int _fsi = 0; _fsi < Scene.Entities.Count; _fsi++)
        {
            var e = Scene.Entities[_fsi];
            if (e is not FloatySpaceBlock other) continue;
            if (other.HasGroup)          continue;
            if (other._tileType != _tileType) continue;

            // Check adjacency (within 1 px).
            if (Touches(from, other))
                AddToGroupAndFindChildren(other);
        }
    }

    private static bool Touches(FloatySpaceBlock a, FloatySpaceBlock b)
    {
        // Horizontally adjacent.
        bool hAdj = Math.Abs(a.Position.X + a.Width  - b.Position.X) <= 1f
                 || Math.Abs(b.Position.X + b.Width  - a.Position.X) <= 1f;
        bool vOver = a.Position.Y < b.Position.Y + b.Height
                  && b.Position.Y < a.Position.Y + a.Height;

        // Vertically adjacent.
        bool vAdj = Math.Abs(a.Position.Y + a.Height - b.Position.Y) <= 1f
                 || Math.Abs(b.Position.Y + b.Height - a.Position.Y) <= 1f;
        bool hOver = a.Position.X < b.Position.X + b.Width
                  && b.Position.X < a.Position.X + a.Width;

        return (hAdj && vOver) || (vAdj && hOver);
    }

    // ── Math helpers ──────────────────────────────────────────────────────────

    private static float Approach(float val, float target, float maxMove) =>
        val < target ? Math.Min(val + maxMove, target)
                     : Math.Max(val - maxMove, target);

    private static float Lerp(float a, float b, float t) =>
        a + (b - a) * Math.Clamp(t, 0f, 1f);

    private static float SineInOut(float t) =>
        (float)(-(Math.Cos(Math.PI * t) - 1.0) / 2.0);

    // ── Rider helpers ─────────────────────────────────────────────────────────

    private bool HasPlayerRider()
    {
        if (Scene == null) return false;
        for (int _fsj = 0; _fsj < Scene.Entities.Count; _fsj++)
        {
            if (Scene.Entities[_fsj] is KirbyCelesteStandalone.Entities.Player.MadelinePlayer p
                && IsPlayerRiding(p))
                return true;
        }
        return false;
    }

    private bool IsPlayerRiding(KirbyCelesteStandalone.Entities.Player.MadelinePlayer player) =>
        Math.Abs(player.Position.Y + player.Height - Position.Y) <= 2f
        && player.Position.X + player.Width > Position.X
        && player.Position.X < Position.X + Width;

    // ── Positioning helpers ───────────────────────────────────────────────────

    private void MoveToX(float targetX) => MoveH(targetX - Position.X);
    private void MoveToY(float targetY) => MoveV(targetY - Position.Y);
}
