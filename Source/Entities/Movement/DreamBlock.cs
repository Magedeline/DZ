using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using DZ.Core;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Internal state-machine states for <see cref="DreamBlock"/>.</summary>
public enum DreamBlockState
{
    /// <summary>Block is solid; player cannot pass through.</summary>
    Solid,
    /// <summary>Player is dashing through the interior of the block.</summary>
    PlayerInside,
    /// <summary>Block has been activated (dashed through at least once).</summary>
    Activated,
    /// <summary>Block is moving between its two endpoints (optional node mode).</summary>
    Moving,
}

/// <summary>
/// Port of Celeste's DreamBlock.cs to Nez/MonoGame.
///
/// When the player possesses the dream-dash ability, they can dash through this
/// block.  On the way through, the block becomes temporarily passable.  Once
/// the player exits the far side, <see cref="Activated"/> is set to <c>true</c>.
///
/// If a <see cref="Node"/> position is provided the block moves back and forth
/// between <see cref="StartPosition"/> and <see cref="Node"/> after activation
/// at <see cref="TravelSpeed"/> pixels per second; <see cref="FastMoving"/>
/// doubles that speed.
///
/// When <see cref="OneUse"/> is <c>true</c> the block turns into a permanent
/// solid after the first activation.
/// </summary>
public class DreamBlock : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Speed while travelling between node endpoints (pixels/second).</summary>
    public const float TravelSpeed         = 32f;

    /// <summary>Multiplier applied to <see cref="TravelSpeed"/> when <see cref="FastMoving"/>.</summary>
    public const float FastMovingMultiplier = 2f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>
    /// Optional second endpoint.  When set, the block oscillates between
    /// <see cref="StartPosition"/> and <see cref="Node"/> after activation.
    /// </summary>
    public Vector2? Node { get; }

    /// <summary>When <c>true</c>, doubles the oscillation speed.</summary>
    public bool FastMoving { get; }

    /// <summary>
    /// When <c>true</c>, the block becomes permanently solid (no longer passable)
    /// after the first dream-dash passes through it.
    /// </summary>
    public bool OneUse { get; }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Current state-machine state.</summary>
    public DreamBlockState State { get; private set; } = DreamBlockState.Solid;

    /// <summary>
    /// <c>true</c> once the player has dashed through this block at least once.
    /// </summary>
    public bool Activated { get; private set; }

    /// <summary>World-space spawn position.</summary>
    public readonly Vector2 StartPosition;

    // ── Node-movement state ───────────────────────────────────────────────────

    /// <summary>
    /// Current lerp value in [0, 1] between <see cref="StartPosition"/> and
    /// <see cref="Node"/> (only relevant when <see cref="Node"/> is non-null).
    /// </summary>
    private float _nodeLerp;

    /// <summary>Oscillation direction: +1 toward Node, -1 toward Start.</summary>
    private int _nodeDir = 1;

    /// <summary>Total distance between start and node (cached).</summary>
    private readonly float _nodeDist;

    // ── Dream-dash passthrough state ─────────────────────────────────────────

    /// <summary>
    /// <c>true</c> while the player is physically inside the block during a
    /// dream-dash; during this time <see cref="CelestePlatform.Collidable"/> is
    /// set to <c>false</c> so the player can pass through.
    /// </summary>
    private bool _playerInside;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="DreamBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position at rest.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="node">
    /// Optional second endpoint.  When non-null the block oscillates between
    /// <paramref name="position"/> and <paramref name="node"/> once activated.
    /// </param>
    /// <param name="fastMoving">If <c>true</c>, doubles the oscillation speed.</param>
    /// <param name="oneUse">If <c>true</c>, the block becomes solid after one pass-through.</param>
    public DreamBlock(
        Vector2  position,
        float    width,
        float    height,
        Vector2? node        = null,
        bool     fastMoving  = false,
        bool     oneUse      = false)
        : base(position, width, height)
    {
        StartPosition = position;
        Node          = node;
        FastMoving    = fastMoving;
        OneUse        = oneUse;
        Name          = "DreamBlock";

        if (node.HasValue)
            _nodeDist = Vector2.Distance(position, node.Value);

        // Wire up dash-collision callback.
        OnDashCollide = HandleDashCollide;

        // TODO: load sprite (dream texture — sparkles, starfield)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        Activated     = false;
        _playerInside = false;
        Collidable    = true;
        State         = DreamBlockState.Solid;
        _nodeLerp     = 0f;
        _nodeDir      = 1;
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the state machine every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case DreamBlockState.Solid:
            case DreamBlockState.Activated:
                UpdatePassthrough();
                if (Activated && Node.HasValue && !OneUse)
                    UpdateNodeMovement();
                break;

            case DreamBlockState.PlayerInside:
                UpdatePlayerInside();
                break;

            case DreamBlockState.Moving:
                UpdateNodeMovement();
                break;
        }
    }

    // ── Passthrough detection ─────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the player has exited the block after a dream-dash,
    /// restoring collidability once they are clear.
    /// </summary>
    private void UpdatePassthrough()
    {
        if (!_playerInside) return;

        var player = GetPlayer();
        if (player == null || !IsPlayerOverlapping(player))
        {
            // Player has exited — restore solidity.
            _playerInside = false;
            Collidable    = true;
            OnPlayerExited();
        }
    }

    private void UpdatePlayerInside()
    {
        // While the player is inside the block is passable.
        // We wait until they exit (handled in UpdatePassthrough above,
        // but also check here to keep both paths synchronised).
        var player = GetPlayer();
        if (player != null && !IsPlayerOverlapping(player))
        {
            _playerInside = false;
            Collidable    = true;
            State         = Activated ? DreamBlockState.Activated : DreamBlockState.Solid;
            OnPlayerExited();
        }
    }

    // ── Node oscillation ─────────────────────────────────────────────────────

    private void UpdateNodeMovement()
    {
        if (!Node.HasValue || _nodeDist <= 0f) return;

        float dt       = Time.DeltaTime;
        float speed    = TravelSpeed * (FastMoving ? FastMovingMultiplier : 1f);
        float lerpStep = speed * dt / _nodeDist;

        _nodeLerp = Math.Clamp(_nodeLerp + _nodeDir * lerpStep, 0f, 1f);

        Vector2 target = Vector2.Lerp(StartPosition, Node.Value, _nodeLerp);

        // Compute displacement for lift speed.
        Vector2 prev = Position;
        MoveToX(target.X);
        MoveToY(target.Y);
        Vector2 delta = Position - prev;
        if (dt > 0f)
            SetLiftSpeed(delta / dt);

        // Reverse at endpoints.
        if (_nodeLerp >= 1f) _nodeDir = -1;
        else if (_nodeLerp <= 0f) _nodeDir = 1;
    }

    // ── Dash-collision callback ───────────────────────────────────────────────

    /// <summary>
    /// Called when the player initiates a dream-dash into this block.
    /// Makes the block passable for this dash pass.
    /// </summary>
    private void HandleDashCollide(Vector2 dashDirection)
    {
        // TODO: check player.HasDreamDash ability before allowing pass-through.
        // For now, treat every dash as a dream-dash for scaffolding purposes.
        if (OneUse && Activated) return;

        _playerInside = true;
        Collidable    = false;
        State         = DreamBlockState.PlayerInside;

        // TODO: play sound (dream dash entry)
        // TODO: emit particles (star burst)
    }

    // ── Player exit ───────────────────────────────────────────────────────────

    private void OnPlayerExited()
    {
        if (!Activated)
        {
            Activated = true;
            // TODO: play sound (dream dash exit / activation)
            // TODO: emit particles (activation burst)
        }

        State = Activated ? DreamBlockState.Activated : DreamBlockState.Solid;

        if (OneUse)
        {
            // Permanently solid — remove dash callback so it can't be re-entered.
            OnDashCollide = null;
            Collidable    = true;
            // TODO: play sound (one-use lock)
        }
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
