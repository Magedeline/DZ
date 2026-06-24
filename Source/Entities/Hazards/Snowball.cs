using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// A snowball that spawns off-screen to the right, drifts left while bobbing
/// vertically on a sine wave, and kills the player on contact.
/// It can be bounced off from above, which destroys it safely.
/// After leaving the left side of the screen it respawns after a short delay.
/// Ported from Celeste's Snowball.cs.
///
/// Porting notes:
/// <list type="bullet">
///   <item>SineWave replaced with a manual accumulator (<see cref="sineTime"/>).</item>
///   <item>Camera bounds approximated via <c>Nez.Core.Scene.Camera</c>.</item>
///   <item>Audio / sprite replaced with TODO stubs.</item>
///   <item>Screen freeze (<c>Celeste.Freeze</c>) noted as TODO.</item>
/// </list>
/// </summary>
public class Snowball : Nez.Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const float MoveSpeed       = 200f;   // px/s leftward
    private const float BobAmplitude    = 4f;     // px vertical sine amplitude
    private const float BobFrequency    = 0.5f;   // Hz
    private const float ResetDelay      = 0.8f;   // seconds before respawn
    private const float SpawnRightPad   = 10f;    // pixels past right camera edge
    private const float DespawnLeftPad  = 60f;    // pixels past left camera edge
    private const float RightMargin     = 64f;    // player must be this far from right edge to spawn

    // ── Colliders (approximate as world-space offsets from Position) ──────────
    // Main hit:  12 × 9, offset (-5, -2)  — centred roughly
    private const float HitW = 12f, HitH = 9f, HitOX = -5f, HitOY = -2f;
    // Bounce:    16 × 6, offset (-6, -8)  — above the ball
    private const float BncW = 16f, BncH = 6f, BncOX = -6f, BncOY = -8f;

    // ── State ─────────────────────────────────────────────────────────────────
    private bool  collidable = false;
    private float atY;           // baseline Y for bobbing
    private float sineTime;      // accumulator for sine wave
    private float resetTimer;    // countdown to respawn after leaving screen
    private bool  broken;        // true after being bounced (visual state only)

    // ── Constructor ───────────────────────────────────────────────────────────
    public Snowball()
    {
        Name = "Snowball";
        // TODO: load sprite: snowball — play "spin" animation
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        ResetPosition();
    }

    public override void Update()
    {
        float dt = Time.DeltaTime;

        if (!collidable)
        {
            // Waiting to respawn.
            resetTimer -= dt;
            if (resetTimer <= 0f)
                ResetPosition();
            return;
        }

        // Move left and bob vertically.
        sineTime += dt * BobFrequency * MathF.PI * 2f;
        Position = new Vector2(
            Position.X - MoveSpeed * dt,
            atY + BobAmplitude * MathF.Sin(sineTime));

        // Check if the ball has left the left side of the screen.
        float camLeft = GetCameraLeft();
        if (Position.X < camLeft - DespawnLeftPad)
        {
            collidable = false;
            resetTimer = ResetDelay;
            return;
        }

        if (!broken)
            CheckPlayerCollision();
    }

    // ── Position reset ────────────────────────────────────────────────────────
    private void ResetPosition()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        float camRight = GetCameraRight();

        if (player != null && player.Position.X < camRight - RightMargin)
        {
            // TODO: play sound: event:/game/04_cliffside/snowball_spawn
            broken = false;
            collidable = true;
            resetTimer = 0f;
            sineTime = 0f;

            Position = new Vector2(camRight + SpawnRightPad, player.Position.Y);
            atY = Position.Y;
            // TODO: play sprite animation: snowball "spin"
        }
        else
        {
            // Player is too close to the right — try again shortly.
            resetTimer = 0.05f;
        }
    }

    // ── Collision ─────────────────────────────────────────────────────────────
    private void CheckPlayerCollision()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        var pPos = player.Position;

        // Bounce zone (above the ball) — checked first.
        if (PointInRect(pPos, Position + new Vector2(BncOX, BncOY), BncW, BncH))
        {
            OnPlayerBounce(player, pPos);
            return;
        }

        // Main hit zone.
        if (PointInRect(pPos, Position + new Vector2(HitOX, HitOY), HitW, HitH))
            OnPlayer(player);
    }

    private void OnPlayer(MadelinePlayer player)
    {
        player.Die(new Vector2(-1f, 0f));
        // TODO: play sound: event:/game/04_cliffside/snowball_impact
        Destroy();
    }

    private void OnPlayerBounce(MadelinePlayer player, Vector2 pPos)
    {
        // Only register the bounce if the player is NOT also inside the main hitbox.
        if (PointInRect(pPos, Position + new Vector2(HitOX, HitOY), HitW, HitH))
            return;

        // TODO: freeze screen for 0.1s (Celeste.Freeze)
        // player.Bounce((int)(Position.Y + BncOY - 2f)); // TODO: not implemented in this port
        // TODO: play sound: event:/game/general/thing_booped
        Destroy();
    }

    private void Destroy()
    {
        broken     = false;
        collidable = false;
        resetTimer = ResetDelay;
        // TODO: play sprite animation: snowball "break"
    }

    // ── Camera helpers ────────────────────────────────────────────────────────
    private float GetCameraRight()
    {
        var cam = Scene?.Camera;
        return cam != null ? cam.Position.X + cam.Bounds.Width : 320f;
    }

    private float GetCameraLeft()
    {
        var cam = Scene?.Camera;
        return cam != null ? cam.Position.X : 0f;
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private static bool PointInRect(Vector2 pt, Vector2 origin, float w, float h)
        => pt.X >= origin.X && pt.X <= origin.X + w &&
           pt.Y >= origin.Y && pt.Y <= origin.Y + h;
}
