using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// Base class for spinners that slide back and forth along a line segment
/// between two nodes (Start ↔ End), pausing briefly at each end.
/// Ported from Celeste's TrackSpinner.cs.
///
/// Movement uses a sine-eased lerp.  Three speed presets control the
/// move/pause timing (<see cref="Speeds"/>).  Subclasses receive
/// <see cref="OnTrackStart"/> / <see cref="OnTrackEnd"/> callbacks
/// for audio/particle effects.
/// </summary>
public class TrackSpinner : Nez.Entity
{
    // ── Speed tables (mirrors Celeste source exactly) ─────────────────────────
    public static readonly float[] PauseTimes = { 0.3f, 0.2f, 0.6f };
    public static readonly float[] MoveTimes  = { 0.9f, 0.4f, 0.3f };

    // ── State ─────────────────────────────────────────────────────────────────
    /// <summary>When true, moving toward End; when false, moving toward Start.</summary>
    public bool Up = true;

    /// <summary>Remaining pause time at the current endpoint (seconds).</summary>
    public float PauseTimer;

    /// <summary>Current speed preset.</summary>
    public Speeds Speed;

    /// <summary>Whether this spinner is actively moving.</summary>
    public bool Moving = true;

    /// <summary>
    /// The angle of the Start→End vector (radians), kept for visual orientation
    /// by subclasses.
    /// </summary>
    public float Angle;

    // ── Endpoints ─────────────────────────────────────────────────────────────
    public Vector2 Start { get; private set; }
    public Vector2 End   { get; private set; }

    /// <summary>Current interpolation progress along Start→End in [0, 1].</summary>
    public float Percent { get; private set; }

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a TrackSpinner moving between <paramref name="start"/> and
    /// <paramref name="end"/>.
    /// </summary>
    /// <param name="start">World-space start position.</param>
    /// <param name="end">World-space end position.</param>
    /// <param name="speed">Speed preset.</param>
    /// <param name="startCenter">When true, begin at the midpoint (50 %).</param>
    public TrackSpinner(Vector2 start, Vector2 end, Speeds speed, bool startCenter = false)
    {
        Start  = start;
        End    = end;
        Speed  = speed;

        // angle of track direction
        Vector2 diff = start - end;
        Angle = (float)Math.Atan2(diff.Y, diff.X);

        Percent = startCenter ? 0.5f : 0f;
        if (Percent >= 1f) Up = false;

        UpdatePosition();
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        OnTrackStart();
    }

    public override void Update()
    {
        float dt = Time.DeltaTime;

        if (!Moving) return;

        if (PauseTimer > 0f)
        {
            PauseTimer -= dt;
            if (PauseTimer <= 0f)
                OnTrackStart();
            return;
        }

        // Approach the target end (0 or 1) at the selected speed.
        float target = Up ? 1f : 0f;
        float moveTime = MoveTimes[(int)Speed];
        Percent = Approach(Percent, target, dt / moveTime);
        UpdatePosition();

        bool atEnd = Up ? Percent >= 1f : Percent <= 0f;
        if (atEnd)
        {
            Up = !Up;
            PauseTimer = PauseTimes[(int)Speed];
            OnTrackEnd();
        }

        CheckPlayerCollision();
    }

    // ── Position ──────────────────────────────────────────────────────────────
    /// <summary>Snaps entity position to the sine-eased lerp of Start→End.</summary>
    public void UpdatePosition()
        => Position = Vector2.Lerp(Start, End, SineInOut(Percent));

    // ── Player collision ──────────────────────────────────────────────────────
    /// <summary>
    /// Checks overlap with the player using a 6-pixel circle and a 16×4 hitbox
    /// (mirrors the original ColliderList).
    /// </summary>
    protected virtual void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        var pPos = player?.Position ?? Vector2.Zero;

        bool circleHit = Vector2.Distance(Position, pPos) <= 6f;
        bool boxHit = HitboxOverlap(Position + new Vector2(-8f, -3f), 16f, 4f, pPos);

        if (circleHit || boxHit)
            OnPlayer(player);
    }

    /// <summary>Called when the spinner touches the player. Override in subclasses.</summary>
    public virtual void OnPlayer(MadelinePlayer player)
    {
        var pPos = player?.Position ?? Position;
        Vector2 dir = pPos - Position;
        Vector2 knockback = dir.LengthSquared() > 0f ? Vector2.Normalize(dir) : Vector2.Zero;
        player.Die(knockback);
        Moving = false;
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────
    /// <summary>Called each time the spinner begins moving from an endpoint.</summary>
    public virtual void OnTrackStart() { }

    /// <summary>Called each time the spinner arrives at an endpoint.</summary>
    public virtual void OnTrackEnd() { }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static float Approach(float val, float target, float delta)
    {
        if (val < target)
            return Math.Min(val + delta, target);
        return Math.Max(val - delta, target);
    }

    /// <summary>Sine-eased in-out, matches Celeste's Ease.SineInOut.</summary>
    private static float SineInOut(float t)
        => -(MathF.Cos(MathF.PI * t) - 1f) * 0.5f;

    /// <summary>Simple AABB overlap test (player position as centre).</summary>
    private static bool HitboxOverlap(Vector2 boxOrigin, float w, float h, Vector2 point)
    {
        return point.X >= boxOrigin.X && point.X <= boxOrigin.X + w &&
               point.Y >= boxOrigin.Y && point.Y <= boxOrigin.Y + h;
    }

    // ── Nested enum ───────────────────────────────────────────────────────────
    public enum Speeds
    {
        Slow,
        Normal,
        Fast,
    }
}
