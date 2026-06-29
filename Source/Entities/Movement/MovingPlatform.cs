using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Entities.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's MovingPlatform.cs.
///
/// A jump-through platform that oscillates between a start and end point using
/// a sine-eased yoyo tween. While a player rides it the platform sinks
/// slightly downward (addY), then floats back up when unoccupied.
/// </summary>
public class MovingPlatform : CelesteJumpThru
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float SinkSpeed   = 50f;   // px/s rate the platform sinks
    private const float RiseSpeed   = 20f;   // px/s rate the platform recovers
    private const float SinkTarget  = 3f;    // max extra sag in pixels
    private const float SinkHold    = 0.2f;  // seconds the sink is held after rider leaves

    // ── Tween state ──────────────────────────────────────────────────────────

    private readonly Vector2 _start;
    private readonly Vector2 _end;

    /// <summary>Elapsed tween time (0..Duration then back).</summary>
    private float _tweenTimer;

    /// <summary>Current tween direction: +1 = start→end, -1 = end→start.</summary>
    private int _tweenDir = 1;

    private const float TweenDuration = 2f;  // seconds for one leg of the yoyo

    // ── Sink state ────────────────────────────────────────────────────────────

    private float _addY;
    private float _sinkTimer;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="MovingPlatform"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="node">End-point world position.</param>
    public MovingPlatform(Vector2 position, int width, Vector2 node)
        : base(position, width)
    {
        _start = position;
        _end   = node;
        Name   = "MovingPlatform";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        // Place at the lerped position immediately (t=0 → _start).
        // Note: base.OnAddedToScene() is called by the framework's virtual dispatch.
        Position = _start;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        float dt = Time.DeltaTime;

        // ── Advance tween ────────────────────────────────────────────────────
        _tweenTimer += dt * _tweenDir;

        if (_tweenTimer >= TweenDuration)
        {
            _tweenTimer = TweenDuration;
            _tweenDir   = -1;
        }
        else if (_tweenTimer <= 0f)
        {
            _tweenTimer = 0f;
            _tweenDir   = 1;
        }

        float t = SineInOut(_tweenTimer / TweenDuration);

        // ── Sink logic ───────────────────────────────────────────────────────
        var player = GetPlayerRider();
        if (player != null)
        {
            _sinkTimer = SinkHold;
            _addY      = Approach(_addY, SinkTarget, SinkSpeed * dt);
        }
        else if (_sinkTimer > 0f)
        {
            _sinkTimer -= dt;
            _addY       = Approach(_addY, SinkTarget, SinkSpeed * dt);
        }
        else
        {
            _addY = Approach(_addY, 0f, RiseSpeed * dt);
        }

        // ── Move platform ────────────────────────────────────────────────────
        Vector2 target = Vector2.Lerp(_start, _end, t) + new Vector2(0f, _addY);
        MoveTo(target);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static float Approach(float val, float target, float maxMove)
    {
        return val < target
            ? Math.Min(val + maxMove, target)
            : Math.Max(val - maxMove, target);
    }

    /// <summary>Smooth sine-ease-in-out curve mapped to [0,1].</summary>
    private static float SineInOut(float t) =>
        (float)(-(Math.Cos(Math.PI * t) - 1.0) / 2.0);

    // ── Rider helpers ─────────────────────────────────────────────────────────

    /// <summary>Returns the <see cref="MadelinePlayer"/> currently standing on this platform, or null.</summary>
    private MadelinePlayer GetPlayerRider()
    {
        if (Scene == null) return null;
        for (int _mpi = 0; _mpi < Scene.Entities.Count; _mpi++)
        {
            if (Scene.Entities[_mpi] is MadelinePlayer p && IsPlayerRiding(p))
                return p;
        }
        return null;
    }

    private bool IsPlayerRiding(MadelinePlayer player)
    {
        // Player is riding if their bottom aligns with our top and they overlap horizontally.
        return Math.Abs(player.Position.Y + player.Height - Position.Y) <= 2f
            && player.Position.X + player.Width > Position.X
            && player.Position.X < Position.X + Width;
    }
}
