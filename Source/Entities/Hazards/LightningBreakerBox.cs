using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A 32×32 breakable fuse-box that, when dash-hit twice, removes all
/// <see cref="Lightning"/> blocks in the scene.
/// Ported from Celeste's LightningBreakerBox.cs.
///
/// Porting notes:
/// <list type="bullet">
///   <item>Extends <see cref="Nez.Entity"/> rather than Monocle Solid; the box is
///         treated as a static collidable via a <see cref="BoxCollider"/> component.</item>
///   <item>Dash detection is polled in <see cref="Update"/> instead of via a callback.
///         Call <see cref="OnDash"/> from the player's dash-collision handler.</item>
///   <item>Sine-wave bobbing + bounce wiggle are approximated with simple timers.</item>
///   <item>Spikes adjacency check preserved as boolean flags.</item>
///   <item>Music/session flags, RumbleTrigger, and Glitch noted as TODOs.</item>
///   <item>Sprite / audio replaced with TODO stubs.</item>
/// </list>
/// </summary>
public class LightningBreakerBox : Nez.Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────
    public const float BoxSize = 32f;

    // ── State ─────────────────────────────────────────────────────────────────
    private int   health        = 2;
    private bool  collidable    = true;

    // Shake / bounce
    private float shakeTimer;
    private float bounceTime;
    private float bounceValue;     // [0, 1] wiggler value
    private Vector2 bounceDir;

    // Sink (player riding)
    private float sinkValue;       // [0, 1]

    // Sine wave bob
    private float sineTime;
    private const float SineRate   = 0.5f;   // Hz
    private const float SineAmpMax = 4f;
    private const float SineAmpMin = 2f;

    // Sparks after first hit
    private bool  makeSparks;
    private float sparkTimer;
    private const float SparkInterval = 0.03f;

    // Break state
    private bool  broken;

    // Spikes adjacency (set from level/scene context externally)
    public bool SpikesLeft;
    public bool SpikesRight;
    public bool SpikesUp;
    public bool SpikesDown;

    // Starting position (for the sine bob)
    private Vector2 start;

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="position">Top-left world position.</param>
    /// <param name="flipX">Mirror the sprite horizontally.</param>
    public LightningBreakerBox(Vector2 position, bool flipX = false)
    {
        Position = position;
        start    = position;
        Name     = "LightningBreakerBox";
        // TODO: load sprite: breakerBox — play "idle" animation, FlipX = flipX
        // TODO: attach BoxCollider 32×32 as solid surface
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        if (broken) return;

        float dt = Time.DeltaTime;

        // Sparks emitted after first hit.
        if (makeSparks)
        {
            sparkTimer += dt;
            if (sparkTimer >= SparkInterval)
            {
                sparkTimer = 0f;
                // TODO: emit particles: LightningBreakerBox_Sparks (1 at Centre, spread 12 px)
            }
        }

        // Shake countdown.
        if (shakeTimer > 0f)
        {
            shakeTimer -= dt;
            if (shakeTimer <= 0f)
            {
                // Shake finished → open animation.
                // TODO: play sprite animation: breakerBox "open"
                // TODO: set sprite scale to (1.2, 1.2)
            }
        }

        // Sine bob + bounce wiggle (only while the box is still collidable).
        if (collidable)
        {
            // Check if player is "riding" (standing on top of) the box.
            bool playerRiding = IsPlayerRiding();
            float targetSink  = playerRiding ? 1f : 0f;
            sinkValue = Approach(sinkValue, targetSink, 2f * dt);

            // Adjust sine frequency based on sink.
            float sineFreq = MathHelper.Lerp(1f, 0.5f, sinkValue);
            sineTime += dt * sineFreq * MathF.PI * 2f;

            float sineAmp = MathHelper.Lerp(SineAmpMax, SineAmpMin, sinkValue);
            float bobY    = sinkValue * 6f + MathF.Sin(sineTime) * sineAmp;

            // Bounce wiggle decays.
            bounceTime  = Math.Max(0f, bounceTime - dt);
            bounceValue = bounceTime > 0f
                ? MathF.Sin(bounceTime * MathF.PI * 2f) * bounceTime
                : 0f;

            Vector2 target = start + new Vector2(0f, bobY) + bounceDir * bounceValue * 12f;
            Position = target;
        }

        // Approach sprite scale back to (1, 1) — tracked as TODO for sprite component.
        // TODO: approach sprite scale X and Y toward 1 at rate 4/s
    }

    // ── Dash hit ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Call from the player's dash-collision handler when the player dashes
    /// into this box.  Returns true if the hit was accepted (not a spike-
    /// protected side), false for a normal collision result.
    /// </summary>
    /// <param name="player">The dashing player.</param>
    /// <param name="dir">Dash direction (unit vector).</param>
    /// <returns>True = rebound; false = normal collision (blocked by spikes).</returns>
    public bool OnDash(MadelinePlayer player, Vector2 dir)
    {
        // Block if the dash direction faces into adjacent spikes.
        if ((dir == Vector2.UnitX   && SpikesLeft)  ||
            (dir == -Vector2.UnitX  && SpikesRight) ||
            (dir == Vector2.UnitY   && SpikesUp)    ||
            (dir == -Vector2.UnitY  && SpikesDown))
            return false;   // NormalCollision — tell caller to treat as wall

        // TODO: trigger directional screen-shake toward dir
        // TODO: set sprite scale: 1 + |dir.Y|*0.4 - |dir.X|*0.4, 1 + |dir.X|*0.4 - |dir.Y|*0.4

        health--;
        if (health > 0)
        {
            // First hit.
            // TODO: play sound: event:/new_content/game/10_farewell/fusebox_hit_1
            // TODO: freeze screen for 0.1 s
            shakeTimer = 0.2f;
            bounceDir  = dir;
            bounceTime = 1f;
            makeSparks = true;
            SmashParticles(dir);
            // TODO: trigger Lightning.PulseRoutine equivalent
            // TODO: input rumble (medium)
        }
        else
        {
            // Second hit — break the box.
            // TODO: stop fusebox_hit_1 SFX if playing
            // TODO: play sound: event:/new_content/game/10_farewell/fusebox_hit_2
            // TODO: freeze screen for 0.2 s
            player.RefillDash(false);
            Break();
            SmashParticles(dir.Perpendicular());
            SmashParticles(-dir.Perpendicular());
            // TODO: input rumble (strong/long)
        }

        return true;  // Rebound
    }

    // ── Break sequence ────────────────────────────────────────────────────────
    private void Break()
    {
        broken    = true;
        collidable = false;
        shakeTimer = 0f;
        // TODO: play sprite animation: breakerBox "break"
        // TODO: set Tag to Persistent
        // TODO: set flag "disable_lightning" in session

        // Remove all Lightning entities from the scene.
        var lightningList = Scene?.FindEntitiesWithTag(0).OfType<Lightning>();
        if (lightningList != null)
        {
            foreach (var lt in lightningList)
            {
                lt.MarkDisappearing();
                lt.Shatter();
                lt.Destroy();
            }
        }

        // TODO: play sound: event:/new_content/game/10_farewell/fusebox_break
        // TODO: set music track / progress if configured
    }

    // ── Smash particles ───────────────────────────────────────────────────────
    private void SmashParticles(Vector2 dir)
    {
        Vector2 centre  = Position + new Vector2(BoxSize * 0.5f, BoxSize * 0.5f);
        Vector2 emitPos;
        Vector2 range;
        float   angle;
        int     count;

        if (dir == Vector2.UnitX)
        {
            angle   = 0f;
            emitPos = centre + new Vector2(BoxSize * 0.5f - 12f, 0f);
            range   = Vector2.UnitY * (BoxSize - 6f) * 0.5f;
            count   = (int)(BoxSize / 8f) * 4;
        }
        else if (dir == -Vector2.UnitX)
        {
            angle   = MathF.PI;
            emitPos = centre + new Vector2(-(BoxSize * 0.5f - 12f), 0f);
            range   = Vector2.UnitY * (BoxSize - 6f) * 0.5f;
            count   = (int)(BoxSize / 8f) * 4;
        }
        else if (dir == Vector2.UnitY)
        {
            angle   = MathF.PI * 0.5f;
            emitPos = centre + new Vector2(0f, BoxSize * 0.5f - 12f);
            range   = Vector2.UnitX * (BoxSize - 6f) * 0.5f;
            count   = (int)(BoxSize / 8f) * 4;
        }
        else
        {
            angle   = -MathF.PI * 0.5f;
            emitPos = centre + new Vector2(0f, -(BoxSize * 0.5f - 12f));
            range   = Vector2.UnitX * (BoxSize - 6f) * 0.5f;
            count   = (int)(BoxSize / 8f) * 4;
        }

        // TODO: emit particles: LightningBreakerBox_Smash (count+2 at emitPos ± range, facing angle)
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private bool IsPlayerRiding()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return false;
        var pPos = player.Position;
        // Simple check: player is just above the box top.
        return pPos.X >= Position.X && pPos.X <= Position.X + BoxSize &&
               MathF.Abs(pPos.Y - Position.Y) < 4f;
    }

    private static float Approach(float val, float target, float delta)
        => val < target ? Math.Min(val + delta, target)
                        : Math.Max(val - delta, target);
}

// Extension method for perpendicular vector (matches Celeste's Vector2.Perpendicular).
internal static class Vector2Extensions
{
    public static Vector2 Perpendicular(this Vector2 v) => new Vector2(-v.Y, v.X);
}
