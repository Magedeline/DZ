using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Movement;

/// <summary>
/// Port of Celeste's DashBlock.cs to Nez/MonoGame.
///
/// A solid block that shatters when the player dashes into it.
/// <list type="bullet">
///   <item>
///     When <see cref="Permanent"/> is <c>true</c> the block is removed from
///     the scene permanently (never respawns).
///   </item>
///   <item>
///     When <see cref="Permanent"/> is <c>false</c> the block respawns after
///     <see cref="RespawnTime"/> seconds at its original position.
///   </item>
/// </list>
///
/// The break logic is exposed via <see cref="Break(Vector2, Vector2)"/> so
/// other systems (cutscenes, triggers) can destroy the block programmatically.
/// </summary>
public class DashBlock : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Seconds before a non-permanent block reappears.</summary>
    public const float RespawnTime = 2.5f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>
    /// When <c>true</c>, breaking this block removes it permanently;
    /// when <c>false</c>, it respawns after <see cref="RespawnTime"/> seconds.
    /// </summary>
    public bool Permanent { get; }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary><c>true</c> while the block is broken and invisible.</summary>
    public bool IsBroken { get; private set; }

    /// <summary>Countdown until respawn (only active when <see cref="IsBroken"/> and <see cref="Permanent"/> is <c>false</c>).</summary>
    private float _respawnTimer;

    /// <summary>World-space spawn position for respawn.</summary>
    private readonly Vector2 _startPosition;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="DashBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="permanent">
    /// If <c>true</c>, the block is gone permanently after breaking.
    /// If <c>false</c>, it respawns after <see cref="RespawnTime"/> seconds.
    /// </param>
    public DashBlock(
        Vector2 position,
        float width,
        float height,
        bool permanent = true)
        : base(position, width, height)
    {
        Permanent      = permanent;
        _startPosition = position;
        Name           = "DashBlock";
        IsBroken       = false;

        // Wire up dash-collision callback.
        OnDashCollide = HandleDashCollide;

        // TODO: load sprite
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        Restore();
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Handles respawn countdown when the block is non-permanent.</summary>
    public override void Update()
    {
        base.Update();

        if (!IsBroken || Permanent) return;

        _respawnTimer -= Time.DeltaTime;
        if (_respawnTimer <= 0f)
            Restore();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Shatters the block, spawning debris particles in the direction of the dash.
    /// </summary>
    /// <param name="from">World-space origin of the dash (e.g. player position).</param>
    /// <param name="direction">Unit vector of the dash direction.</param>
    public void Break(Vector2 from, Vector2 direction)
    {
        if (IsBroken) return;

        IsBroken   = true;
        Collidable = false;

        // TODO: emit particles (shatter debris in <direction>)
        // TODO: play sound (dash block break)

        if (Permanent)
        {
            // Permanent blocks are removed from the scene entirely.
            Destroy();
        }
        else
        {
            // Non-permanent: hide and start respawn countdown.
            _respawnTimer = RespawnTime;
            // TODO: emit particles (break flash)
        }
    }

    // ── Dash-collision callback ───────────────────────────────────────────────

    /// <summary>
    /// Called by the player system when a dash collides with this block.
    /// </summary>
    private void HandleDashCollide(Vector2 dashDirection)
    {
        if (IsBroken) return;

        var player = GetPlayer();
        Vector2 from = player?.Entity.Position ?? Position;
        Break(from, dashDirection);
    }

    // ── Restore ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the block to its initial state (visible and collidable) at the
    /// spawn position.  Called at scene entry and after a respawn timer expires.
    /// </summary>
    private void Restore()
    {
        IsBroken       = false;
        Collidable     = true;
        Position       = _startPosition;
        _respawnTimer  = 0f;
        UpdateBounds();
        // TODO: emit particles (respawn flash)
        // TODO: play sound (respawn)
        // TODO: load sprite (restore animation frame)
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Advances <paramref name="current"/> toward <paramref name="target"/> by at most
    /// <paramref name="maxDelta"/>, never overshooting.
    /// </summary>
    private static float Approach(float current, float target, float maxDelta)
    {
        float diff = target - current;
        return current + Math.Sign(diff) * Math.Min(maxDelta, Math.Abs(diff));
    }
}
