using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Hazards;

/// <summary>
/// A lava-gate barrier that is only lethal (and only solid) while the level is
/// in "hot" core mode.  In cold mode it becomes transparent and passable.
/// Ported from Celeste's FireBarrier.cs.
///
/// Porting notes:
/// <list type="bullet">
///   <item>CoreMode toggling is now driven by the public <see cref="SetActive"/>
///         method — call it externally when the core-mode changes.</item>
///   <item>LavaRect visual replaced with TODO stub.</item>
///   <item>Audio replaced with TODO stubs.</item>
///   <item>Deactivation particle burst is noted as a TODO.</item>
/// </list>
/// </summary>
public class FireBarrier : DZ.Nez.Entity
{
    // ── Geometry ──────────────────────────────────────────────────────────────
    private readonly float width;
    private readonly float height;

    // ── State ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// True while the barrier is in "hot" mode — collision active, rendered.
    /// </summary>
    public bool Active { get; private set; } = false;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Barrier width in pixels.</param>
    /// <param name="height">Barrier height in pixels.</param>
    /// <param name="startActive">True to start in hot/active mode.</param>
    public FireBarrier(Vector2 position, float width, float height, bool startActive = false)
    {
        Position   = position;
        this.width  = width;
        this.height = height;
        Active      = startActive;
        Name        = "FireBarrier";

        // TODO: set up LavaRect visual (width, height):
        //   SurfaceColor = RisingLava.Hot[0], EdgeColor = Hot[1], CenterColor = Hot[2]
        //   SmallWaveAmplitude = 2, BigWaveAmplitude = 1, CurveAmplitude = 1
        // TODO: play sound: event:/env/local/09_core/lavagate_idle  (if startActive)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        if (!Active) return;

        CheckPlayerCollision();
    }

    // ── Activation ────────────────────────────────────────────────────────────
    /// <summary>
    /// Switches the barrier between hot (active/lethal) and cold (passable).
    /// Call this when the core mode changes.
    /// </summary>
    public void SetActive(bool active)
    {
        bool wasActive = Active;
        Active = active;

        if (wasActive && !active)
        {
            // Deactivating — emit deactivation particle burst.
            Vector2 centre = Position + new Vector2(width * 0.5f, height * 0.5f);
            for (int x = 0; x < (int)width; x += 4)
            {
                for (int y = 0; y < (int)height; y += 4)
                {
                    Vector2 pPos = Position + new Vector2(x + 2f, y + 2f);
                    // Add a tiny random jitter (±2 px).
                    pPos += new Vector2(
                        DZ.Nez.Random.Range(-2f, 2f),
                        DZ.Nez.Random.Range(-2f, 2f));
                    float dir = (float)System.Math.Atan2(pPos.Y - centre.Y, pPos.X - centre.X);
                    // TODO: emit particles: FireBarrier_Deactivate at pPos facing dir
                }
            }
            // TODO: stop sound: lavagate_idle
        }
        else if (!wasActive && active)
        {
            // TODO: play sound: event:/env/local/09_core/lavagate_idle
        }
    }

    // ── Collision ─────────────────────────────────────────────────────────────
    private void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        var pPos = player.Position;
        if (PointInRect(pPos, Position, width, height))
            OnPlayer(player);
    }

    private void OnPlayer(MadelinePlayer player)
    {
        var centre = Position + new Vector2(width * 0.5f, height * 0.5f);
        var pPos   = player?.Position ?? centre;
        Vector2 dir = pPos - centre;
        Vector2 knockback = dir.LengthSquared() > 0f ? Vector2.Normalize(dir) : Vector2.Zero;
        player.Die(knockback);
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private static bool PointInRect(Vector2 pt, Vector2 origin, float w, float h)
        => pt.X >= origin.X && pt.X <= origin.X + w &&
           pt.Y >= origin.Y && pt.Y <= origin.Y + h;
}
