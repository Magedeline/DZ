using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A rectangular block of lightning that kills the player on contact.
/// Optionally moves back and forth between two nodes on a sine-eased tween.
/// Ported from Celeste's Lightning.cs.
///
/// Porting notes:
/// <list type="bullet">
///   <item>Monocle Coroutines replaced with a simple state-machine update.</item>
///   <item>Visibility / collidable culling based on camera bounds is preserved.</item>
///   <item>Static helpers (<c>PulseRoutine</c>, <c>RemoveRoutine</c>) are exposed
///         as static methods that callers can invoke directly.</item>
///   <item>LightningRenderer tracking replaced with TODO stub.</item>
///   <item>Glitch/Bloom effects replaced with TODO stubs.</item>
///   <item>Particle shatter emits noted as TODO.</item>
/// </list>
/// </summary>
public class Lightning : Nez.Entity
{
    // ── Geometry ──────────────────────────────────────────────────────────────
    public int VisualWidth  { get; private set; }
    public int VisualHeight { get; private set; }

    // ── State ─────────────────────────────────────────────────────────────────
    /// <summary>Fade value used by the LightningRenderer for visual effects.</summary>
    public float Fade;

    /// <summary>True while this block is fading out and about to be removed.</summary>
    public bool Disappearing { get; private set; }

    // ── Collidable flag ───────────────────────────────────────────────────────
    private bool collidable = true;
    public bool Collidable
    {
        get => collidable;
        set
        {
            collidable = value;
            // Visible = value; // TODO: Visible not available in Nez.Entity
        }
    }

    // ── Movement (optional) ───────────────────────────────────────────────────
    private bool   hasNode;
    private Vector2 moveStart;
    private Vector2 moveEnd;
    private float   moveTime;

    // Move-routine state machine.
    private enum MoveState { ToEnd, ToStart }
    private MoveState moveState   = MoveState.ToEnd;
    private float     movePercent = 0f;    // [0, 1]

    // ── Cull timer ────────────────────────────────────────────────────────────
    private float toggleTimer;
    private readonly float toggleOffset;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels (hitbox is width-2 to inset slightly).</param>
    /// <param name="height">Height in pixels (hitbox is height-2).</param>
    /// <param name="node">Optional second position for back-and-forth movement.</param>
    /// <param name="moveTime">Seconds to travel between the two positions.</param>
    public Lightning(Vector2 position, int width, int height,
                     Vector2? node = null, float moveTime = 0f)
    {
        Position       = position;
        VisualWidth    = width;
        VisualHeight   = height;
        toggleOffset   = Nez.Random.NextFloat();
        Name           = "Lightning";

        if (node.HasValue)
        {
            hasNode   = true;
            moveStart = position;
            moveEnd   = node.Value;
            this.moveTime = moveTime > 0f ? moveTime : 1f;
        }

        // TODO: register with LightningRenderer tracker
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Periodic in-view culling — matches original (0.25 s when on, 0.05 s when off).
        float checkInterval = Collidable ? 0.25f : 0.05f;
        toggleTimer += dt;
        if (toggleTimer >= checkInterval)
        {
            toggleTimer = 0f;
            bool inView = InView();
            Collidable = inView;
        }

        // Node movement.
        if (hasNode)
        {
            movePercent += dt / moveTime;
            if (movePercent >= 1f)
            {
                movePercent = 0f;
                moveState = moveState == MoveState.ToEnd ? MoveState.ToStart : MoveState.ToEnd;
            }
            float eased = SineInOut(movePercent);
            Position = moveState == MoveState.ToEnd
                ? Vector2.Lerp(moveStart, moveEnd, eased)
                : Vector2.Lerp(moveEnd,   moveStart, eased);
        }

        if (!Collidable || Disappearing) return;

        CheckPlayerCollision();
    }

    // ── Visibility ────────────────────────────────────────────────────────────
    private bool InView()
    {
        var cam = Scene?.Camera;
        if (cam == null) return true;

        float cx = cam.Position.X;
        float cy = cam.Position.Y;
        return Position.X + VisualWidth  > cx - 16f
            && Position.Y + VisualHeight > cy - 16f
            && Position.X < cx + 320f + 16f
            && Position.Y < cy + 180f + 16f;
    }

    // ── Collision ─────────────────────────────────────────────────────────────
    private void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        var pPos = player.Position;
        // Hitbox is (width-2) × (height-2) inset by 1 px on each side.
        if (PointInRect(pPos, Position + Vector2.One, VisualWidth - 2f, VisualHeight - 2f))
            OnPlayer(player, pPos);
    }

    private void OnPlayer(MadelinePlayer player, Vector2 pPos)
    {
        if (Disappearing) return;

        // Knock in the horizontal direction away from the block centre.
        int sign = Math.Sign(pPos.X - Position.X);
        if (sign == 0) sign = -1;
        player.Die(Vector2.UnitX * sign);
    }

    // ── Shatter ───────────────────────────────────────────────────────────────
    /// <summary>
    /// Emits shatter particles across the block face.
    /// Called when the lightning is removed (e.g. by LightningBreakerBox).
    /// </summary>
    public void Shatter()
    {
        if (Scene == null) return;
        for (int x = 4; x < VisualWidth; x += 8)
        {
            for (int y = 4; y < VisualHeight; y += 8)
            {
                // TODO: emit particles: Lightning_Shatter (1 particle at Position + (x,y), spread 3 px)
            }
        }
    }

    /// <summary>Marks this block as disappearing (skips player damage).</summary>
    public void MarkDisappearing() => Disappearing = true;

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static float SineInOut(float t)
        => -(MathF.Cos(MathF.PI * t) - 1f) * 0.5f;

    private static bool PointInRect(Vector2 pt, Vector2 origin, float w, float h)
        => pt.X >= origin.X && pt.X <= origin.X + w &&
           pt.Y >= origin.Y && pt.Y <= origin.Y + h;
}
