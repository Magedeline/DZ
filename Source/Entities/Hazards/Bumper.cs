using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using KirbyCelesteStandalone.Core;
using Entity = Nez.Entity;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A circular bumper that launches the player away on contact, then enters a brief cooldown.
/// Ported from Celeste's Bumper entity (fire/core modes stripped – always behaves as normal mode).
///
/// Behaviour summary:
/// <list type="bullet">
///   <item>Circular hitbox with radius <see cref="Radius"/>.</item>
///   <item>On player contact: push the player away at 280 px/s (ExplodeLaunch equivalent)
///         and start a <see cref="HitCooldown"/>-second cooldown.</item>
///   <item>Sine-wave position oscillation when no <c>node</c> is provided.</item>
///   <item>Back-and-forth lerp movement between <c>position</c> and <c>node</c>
///         when a <c>node</c> is supplied.</item>
/// </list>
/// </summary>
public class Bumper : Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────

    /// <summary>Collision and launch radius in pixels.</summary>
    public const float Radius = 12f;

    /// <summary>Launch speed applied to the player (pixels per second).</summary>
    private const float LaunchSpeed = 280f;

    // ── Tuning ────────────────────────────────────────────────────────────────

    /// <summary>Seconds of cooldown after hitting the player.</summary>
    public float HitCooldown { get; set; } = 0.6f;

    /// <summary>Amplitude of sine-wave oscillation in pixels (used when no node is set).</summary>
    public float OscillationAmplitude { get; set; } = 3f;

    /// <summary>Frequency of the sine-wave oscillation (cycles per second).</summary>
    public float OscillationFrequency { get; set; } = 1.2f;

    /// <summary>Speed of back-and-forth movement along the node path (pixels per second).</summary>
    public float NodeMoveSpeed { get; set; } = 60f;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary><c>true</c> while the hit cooldown is active (bumper is "asleep").</summary>
    public bool IsOnCooldown => _hitTimer > 0f;

    private float _hitTimer;

    // ── Position / movement ───────────────────────────────────────────────────

    /// <summary>The bumper's resting origin (never changes).</summary>
    private readonly Vector2 _origin;

    /// <summary>Optional target node for back-and-forth movement.</summary>
    private readonly Vector2? _node;

    /// <summary>Accumulated time used for the sine-wave calculation.</summary>
    private float _sineTimer;

    /// <summary>
    /// Current interpolation parameter [0, 1] for node-based movement.
    /// Advances and reverses at <see cref="NodeMoveSpeed"/>.
    /// </summary>
    private float _nodeLerp;

    /// <summary>Direction of lerp travel: +1 toward node, -1 toward origin.</summary>
    private int _nodeLerpDir = 1;

    // ── Bounds ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Axis-aligned bounding rectangle derived from the bumper's current position
    /// and circular radius. Used for broad-phase overlap checks with the player.
    /// </summary>
    public RectangleF Bounds =>
        new RectangleF(
            Position.X - Radius,
            Position.Y - Radius,
            Radius * 2f,
            Radius * 2f);

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Bumper"/>.
    /// </summary>
    /// <param name="position">World-space centre of the bumper.</param>
    /// <param name="node">
    /// Optional second position. When provided the bumper oscillates back and forth
    /// between <paramref name="position"/> and <paramref name="node"/>.
    /// When <c>null</c> the bumper uses a sine-wave bob in place.
    /// </param>
    public Bumper(Vector2 position, Vector2? node = null)
    {
        Position = position;
        _origin = position;
        _node = node;
        Name = "Bumper";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        _hitTimer = 0f;
        _sineTimer = 0f;
        _nodeLerp = 0f;
        _nodeLerpDir = 1;
        // TODO: load sprite – bumper sprite sheet
    }

    public override void Update()
    {
        float dt = Time.DeltaTime;

        // ── Cooldown ticker ──────────────────────────────────────────────────
        if (_hitTimer > 0f)
        {
            _hitTimer -= dt;
            if (_hitTimer <= 0f)
            {
                _hitTimer = 0f;
                // TODO: emit particles – respawn burst
                // TODO: play sound: event:/game/06_reflection/pinballbumper_reset
            }
        }

        // ── Positional oscillation ───────────────────────────────────────────
        UpdatePosition(dt);

        // ── Skip collision while on cooldown ─────────────────────────────────
        if (IsOnCooldown) return;

        // ── Player collision ─────────────────────────────────────────────────
        if (!CheckPlayerCollision(out PlayerController player)) return;

        LaunchPlayer(player);
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    private void UpdatePosition(float dt)
    {
        if (_node.HasValue)
        {
            // Back-and-forth lerp between _origin and _node.
            float pathLength = Vector2.Distance(_origin, _node.Value);
            if (pathLength > 0f)
            {
                float step = (NodeMoveSpeed / pathLength) * dt;
                _nodeLerp = MathHelper.Clamp(_nodeLerp + step * _nodeLerpDir, 0f, 1f);

                if (_nodeLerp >= 1f) _nodeLerpDir = -1;
                else if (_nodeLerp <= 0f) _nodeLerpDir = 1;
            }

            Position = Vector2.Lerp(_origin, _node.Value, _nodeLerp);
        }
        else
        {
            // Sine-wave bob in place.
            _sineTimer += dt;
            float offset = (float)Math.Sin(_sineTimer * MathHelper.TwoPi * OscillationFrequency)
                           * OscillationAmplitude;
            Position = _origin + new Vector2(0f, offset);
        }
    }

    // ── Launch ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Pushes the player away from the bumper's centre at <see cref="LaunchSpeed"/> px/s
    /// (ExplodeLaunch equivalent) and starts the hit cooldown.
    /// </summary>
    private void LaunchPlayer(PlayerController player)
    {
        // Determine direction from bumper centre to player centre.
        Vector2 playerCenter = player.Entity.Position
            + new Vector2(
                (player.Entity.GetComponent<BoxCollider>()?.Bounds.Width ?? 0f) * 0.5f,
                (player.Entity.GetComponent<BoxCollider>()?.Bounds.Height ?? 0f) * 0.5f);

        Vector2 direction = playerCenter - Position;

        // Avoid zero-length direction (player exactly on centre).
        if (direction == Vector2.Zero)
            direction = Vector2.UnitY * -1f; // default: launch upward

        direction.Normalize();

        // Apply launch velocity.
        player.Velocity = direction * LaunchSpeed;

        // Start cooldown.
        _hitTimer = HitCooldown;

        // TODO: emit particles – hit burst
        // TODO: play sound: event:/game/06_reflection/pinballbumper_hit
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> and populates <paramref name="player"/> when the
    /// player's collider overlaps the bumper's circular bounding box.
    /// A secondary distance check against <see cref="Radius"/> provides
    /// a tighter circular test.
    /// </summary>
    private bool CheckPlayerCollision(out PlayerController player)
    {
        player = Scene?.FindComponentOfType<PlayerController>();
        if (player == null) return false;

        RectangleF playerBounds = player.Entity.GetComponent<BoxCollider>()?.Bounds
                                  ?? RectangleF.Empty;

        if (!Bounds.Intersects(playerBounds)) return false;

        // Circular distance check: closest point on player AABB to bumper centre.
        float closestX = Math.Clamp(Position.X, playerBounds.X, playerBounds.X + playerBounds.Width);
        float closestY = Math.Clamp(Position.Y, playerBounds.Y, playerBounds.Y + playerBounds.Height);
        float dx = Position.X - closestX;
        float dy = Position.Y - closestY;
        return (dx * dx + dy * dy) <= (Radius * Radius);
    }
}
