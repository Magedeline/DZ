using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Hazards;

/// <summary>
/// A fireball / iceball that travels continuously along a multi-node path,
/// looping back to the start once it reaches the end.
/// Ported from Celeste's FireBall.cs.
///
/// Gameplay notes:
/// <list type="bullet">
///   <item>In fire mode the ball is always lethal on contact.</item>
///   <item>In ice mode the ball can be bounced off from above; contact from other
///         angles kills the player.  After being bounced the ball becomes non-lethal
///         until the path loops.</item>
///   <item>Speed approaches a target multiplier (1.0 fire / 0.5 ice) at rate 2/s.</item>
///   <item>Multiple fireballs on the same path are created via <see cref="SpawnSiblings"/>.</item>
///   <item>Sprite / SFX replaced with TODO stubs.</item>
/// </list>
/// </summary>
public class FireBall : DZ.Nez.Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const float FireSpeed         = 60f;
    private const float IceSpeedMult      = 0.5f;
    private const float SpeedApproachRate = 2f;

    // ── Path data ─────────────────────────────────────────────────────────────
    private readonly Vector2[] nodes;
    private readonly float[]   lengths;     // cumulative arc-length at each node
    private readonly int       amount;      // total balls on this path
    private readonly int       index;       // this ball's index (0 = "leader")
    private readonly float     startOffset; // fractional offset in [0, 1)
    private readonly float     speedMult;   // speed multiplier from level data

    // ── Motion state ──────────────────────────────────────────────────────────
    private float percent;     // position along the path in [0, 1)
    private float speed;       // base speed in path-percent per second
    private float currentSpeedMult; // smoothed toward target (0.5 or 1.0)

    // ── Mode ──────────────────────────────────────────────────────────────────
    /// <summary>
    /// True = ice (cold core) mode — slower, bounceable.
    /// False = fire (hot) mode — always lethal.
    /// </summary>
    public bool IceMode { get; private set; }

    /// <summary>True after the ball has been bounced; resets when the path loops.</summary>
    private bool broken;

    // ── Particle timers ───────────────────────────────────────────────────────
    private float trailTimer;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a single FireBall instance on the given path.
    /// </summary>
    /// <param name="nodes">Ordered world-space waypoints of the path.</param>
    /// <param name="amount">Total number of balls distributed on the path.</param>
    /// <param name="index">Index of this ball in [0, amount).</param>
    /// <param name="offset">Fractional phase shift applied to all balls.</param>
    /// <param name="speedMult">Speed multiplier from entity data.</param>
    /// <param name="iceMode">True to start in ice mode (e.g. notCoreMode maps).</param>
    public FireBall(Vector2[] nodes, int amount, int index, float offset,
                    float speedMult, bool iceMode)
    {
        this.nodes     = nodes;
        this.amount    = amount;
        this.index     = index;
        this.startOffset = offset;
        this.speedMult = speedMult;
        IceMode        = iceMode;

        // Build cumulative arc-length table.
        lengths = new float[nodes.Length];
        for (int i = 1; i < nodes.Length; i++)
            lengths[i] = lengths[i - 1] + Vector2.Distance(nodes[i - 1], nodes[i]);

        float totalLength = lengths[lengths.Length - 1];
        speed = totalLength > 0f ? FireSpeed / totalLength * speedMult : 0f;

        // Distribute initial percent around the path.
        percent = index != 0 ? index / (float)amount : 0f;
        percent += (1f / amount) * offset;
        percent %= 1f;

        currentSpeedMult = iceMode ? IceSpeedMult : 1f;
        Position = GetPercentPosition(percent);

        Name = $"FireBall_{index}";
        // TODO: load sprite: fireball — play animation (iceMode ? "ice" : "hot") with random frame
    }

    // ── Scene added ───────────────────────────────────────────────────────────
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // The "leader" ball (index 0) spawns the remaining siblings.
        if (index == 0)
            SpawnSiblings();

        // TODO: play sound: event:/env/local/09_core/fireballs_idle (fire mode, leader only)
    }

    /// <summary>Adds one FireBall to the scene for each sibling index.</summary>
    public void SpawnSiblings()
    {
        for (int i = 1; i < amount; i++)
        {
            var sibling = new FireBall(nodes, amount, i, startOffset, speedMult, IceMode);
            Scene?.AddEntity(sibling);
        }
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Smooth speed toward mode target.
        float targetMult = IceMode ? IceSpeedMult : 1f;
        currentSpeedMult = Approach(currentSpeedMult, targetMult, SpeedApproachRate * dt);

        percent += speed * currentSpeedMult * dt;
        if (percent >= 1f)
        {
            percent %= 1f;
            // Reset broken state if path is a loop.
            if (broken && nodes[nodes.Length - 1] != nodes[0])
            {
                broken = false;
                // TODO: play sprite animation: fireball (iceMode ? "ice" : "hot") with random frame
            }
        }

        Position = GetPercentPosition(percent);

        // Trail particles.
        if (!broken)
        {
            float trailInterval = IceMode ? 0.08f : 0.05f;
            trailTimer += dt;
            if (trailTimer >= trailInterval)
            {
                trailTimer = 0f;
                // TODO: emit particles: (IceMode ? FireBall_IceTrail : FireBall_FireTrail)
                //       at Position (centre), 1 particle, spread 4 px
            }
        }

        if (!broken)
            CheckPlayerCollision();
    }

    // ── Collision ─────────────────────────────────────────────────────────────
    private void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        var pPos   = player.Position;
        float dist = Vector2.Distance(Position, pPos);

        // Primary 6 px circle hit.
        if (dist <= 6f)
        {
            OnPlayer(player, pPos);
            return;
        }

        // Bounce zone: 16 × 6 box above the ball, for ice-mode bounce.
        if (IceMode && !broken && PointInRect(pPos, Position + new Vector2(-8f, -3f), 16f, 6f))
            OnBounce(player, pPos);
    }

    private void OnPlayer(MadelinePlayer player, Vector2 pPos)
    {
        if (!IceMode)
        {
            KillPlayer(player, pPos);
        }
        else
        {
            // In ice mode, only kill if player isn't above the ball (not bouncing).
            if (!broken && pPos.Y > Position.Y + 4f)
                KillPlayer(player, pPos);
        }
    }

    private void OnBounce(MadelinePlayer player, Vector2 pPos)
    {
        // Only bounced when player is falling onto it from above.
        if (broken || pPos.Y > Position.Y + 4f) return;
        // TODO: play sound: event:/game/09_core/iceball_break
        // TODO: play sprite animation: fireball "shatter"
        broken = true;
        // TODO: emit particles: FireBall_IceBreak (18 particles at Position, spread 6 px)
        // player.Bounce((int)(Position.Y - 2f)); // TODO: Bounce not implemented in MadelinePlayer
        // TODO: play sound: event:/game/general/thing_booped
    }

    private void KillPlayer(MadelinePlayer player, Vector2 pPos)
    {
        Vector2 dir = pPos - Position;
        Vector2 knockback = dir.LengthSquared() > 0f ? Vector2.Normalize(dir) : Vector2.Zero;
        player.Die(knockback);
        // TODO: trigger hitWiggler with direction knockback
    }

    // ── Public mode switch ────────────────────────────────────────────────────
    /// <summary>Switches between fire and ice mode (called by CoreMode change events).</summary>
    public void SetIceMode(bool ice)
    {
        IceMode = ice;
        if (!broken)
        {
            // TODO: play sprite animation: fireball (IceMode ? "ice" : "hot") with random frame
        }
        // TODO: handle track SFX for leader (index == 0):
        //   if (IceMode) stop "fireballs_idle" else play "event:/env/local/09_core/fireballs_idle"
    }

    // ── Path maths ────────────────────────────────────────────────────────────
    private Vector2 GetPercentPosition(float pct)
    {
        if (pct <= 0f) return nodes[0];
        if (pct >= 1f) return nodes[nodes.Length - 1];

        float totalLen = lengths[lengths.Length - 1];
        float target   = totalLen * pct;
        int   seg      = 0;
        while (seg < lengths.Length - 1 && lengths[seg + 1] <= target)
            seg++;

        float segStart  = lengths[seg]     / totalLen;
        float segEnd    = lengths[seg + 1] / totalLen;
        float segPct    = (pct - segStart) / (segEnd - segStart);
        return Vector2.Lerp(nodes[seg], nodes[seg + 1], Math.Clamp(segPct, 0f, 1f));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static float Approach(float val, float target, float delta)
        => val < target ? Math.Min(val + delta, target)
                        : Math.Max(val - delta, target);

    private static bool PointInRect(Vector2 pt, Vector2 origin, float w, float h)
        => pt.X >= origin.X && pt.X <= origin.X + w &&
           pt.Y >= origin.Y && pt.Y <= origin.Y + h;
}
