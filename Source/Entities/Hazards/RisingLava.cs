using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Hazards;

/// <summary>
/// Lava (or ice) that rises from the bottom of the screen, killing any player
/// it touches.  Waits until the player has moved away from their spawn point
/// before starting to rise.
/// Ported from Celeste's RisingLava.cs.
///
/// Porting notes:
/// <list type="bullet">
///   <item>CoreMode toggling handled via <see cref="SetIceMode"/>.</item>
///   <item>LavaRect visual replaced with colour-lerp metadata; actual rendering is a TODO.</item>
///   <item>Audio, Tween, and SaveData.Invincible replaced with TODO stubs.</item>
///   <item>Camera access via <c>Scene.Camera</c>.</item>
/// </list>
/// </summary>
public class RisingLava : Nez.Entity
{
    // ── Colours (mirrors Celeste's static arrays) ─────────────────────────────
    public static readonly Color[] Hot  =
    {
        new Color(0xff, 0x89, 0x33), // surface
        new Color(0xf2, 0x5e, 0x29), // edge
        new Color(0xd0, 0x1c, 0x01), // centre
    };
    public static readonly Color[] Cold =
    {
        new Color(0x33, 0xff, 0xe7),
        new Color(0x4c, 0xa2, 0xeb),
        new Color(0x01, 0x51, 0xd0),
    };

    // ── Constants ─────────────────────────────────────────────────────────────
    private const float BaseSpeed      = -30f;   // px/s (negative = upward)
    private const float HitboxW        = 340f;
    private const float HitboxH        = 120f;

    // ── State ─────────────────────────────────────────────────────────────────
    private bool  isIntro;
    private bool  iceMode;
    private bool  waiting;
    private float colorLerp;    // [0 = hot, 1 = ice]
    private float delay;        // assist-mode delay after being pushed down

    // ── Current visual colours (lerped; used by renderer) ────────────────────
    public Color SurfaceColor { get; private set; }
    public Color EdgeColor    { get; private set; }
    public Color CentreColor  { get; private set; }

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="isIntro">True = intro sequence (always visible from the start).</param>
    public RisingLava(bool isIntro = false)
    {
        this.isIntro = isIntro;
        Name         = "RisingLava";
        // TODO: set up LavaRect visual (400 × 200, onlyTop mode, smallWaveAmplitude=2)
        // TODO: play sound: event:/game/09_core/rising_threat ("room_state" param)
    }

    // ── Scene added ───────────────────────────────────────────────────────────
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Snap to below the scene bottom.
        float sceneBottom = GetSceneBottom();
        Position = new Vector2(GetCameraLeft() - 10f, sceneBottom + 16f);

        // Wait if the player has just respawned or this is an intro sequence.
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        // if (isIntro || (player != null && player.JustRespawned)) // TODO: JustRespawned not implemented
        if (isIntro)
            waiting = true;
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        float dt = Time.DeltaTime;

        delay -= dt;

        // Track camera X so the lava fills the whole width.
        Position = new Vector2(GetCameraLeft(), Position.Y);

        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        if (waiting)
        {
            // Approach player Y + 32 while waiting (respawn grace).
            // if (!isIntro && player != null && player.JustRespawned) // TODO: JustRespawned not implemented
            //     Position = new Vector2(Position.X, Approach(Position.Y, player.Position.Y + 32f, 32f * dt));

            // Stop waiting once the player is no longer just-respawned.
            // if ((!iceMode || !isIntro) && (player == null || !player.JustRespawned)) // TODO: JustRespawned not implemented
            if ((!iceMode || !isIntro) && (player == null))
                waiting = false;
        }
        else
        {
            float camBottom = GetCameraBottom() - 12f;

            // Cap distance above the screen bottom so lava can't get too far ahead.
            if (Position.Y > camBottom + 96f)
                Position = new Vector2(Position.X, camBottom + 96f);

            // Speed scales with distance from camera bottom.
            float speedMult;
            if (Position.Y <= camBottom)
                speedMult = MathHelper.Lerp(1f, 0.5f,  Math.Clamp((camBottom - Position.Y) / 32f, 0f, 1f));
            else
                speedMult = MathHelper.Lerp(1f, 2f, Math.Clamp((Position.Y - camBottom) / 96f, 0f, 1f));

            if (delay <= 0f)
                Position = new Vector2(Position.X, Position.Y + BaseSpeed * speedMult * dt);
        }

        // Colour lerp (hot ↔ ice).
        colorLerp    = Approach(colorLerp, iceMode ? 1f : 0f, dt * 4f);
        SurfaceColor = Color.Lerp(Hot[0], Cold[0], colorLerp);
        EdgeColor    = Color.Lerp(Hot[1], Cold[1], colorLerp);
        CentreColor  = Color.Lerp(Hot[2], Cold[2], colorLerp);
        // TODO: apply SurfaceColor/EdgeColor/CentreColor to LavaRect
        // TODO: set LavaRect Spikey = colorLerp * 5, UpdateMultiplier = (1 - colorLerp) * 2, Fade = iceMode ? 128 : 32

        // Player contact.
        if (!waiting)
            CheckPlayerCollision(player);
    }

    // ── Collision ─────────────────────────────────────────────────────────────
    private void CheckPlayerCollision(MadelinePlayer? player)
    {
        if (player == null) return;

        var pPos = player.Position;
        // Hitbox is 340 × 120 starting at Position.
        if (PointInRect(pPos, Position, HitboxW, HitboxH))
            OnPlayer(player);
    }

    private void OnPlayer(MadelinePlayer player)
    {
        // TODO: if invincible assist mode, push lava down 48 px and bounce player up
        //       (delay = 0.5 s, player.Speed.Y = -200, player.RefillDash())
        player.Die(-Vector2.UnitY);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    /// <summary>Switches between hot and ice mode (called by CoreMode change events).</summary>
    public void SetIceMode(bool ice)
    {
        iceMode = ice;
        // TODO: update sound param "room_state" → ice ? 1 : 0
    }

    /// <summary>Whether this lava is in its waiting state.</summary>
    public bool Waiting => waiting;

    // ── Camera / scene helpers ────────────────────────────────────────────────
    private float GetCameraLeft()
    {
        var cam = Scene?.Camera;
        return cam?.Position.X ?? 0f;
    }

    private float GetCameraBottom()
    {
        var cam = Scene?.Camera;
        return cam != null ? cam.Position.Y + cam.Bounds.Height : 180f;
    }

    private float GetSceneBottom()
    {
        // Fallback — in a real level this would be Level.Bounds.Bottom.
        return GetCameraBottom() + 100f;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static float Approach(float val, float target, float delta)
        => val < target ? Math.Min(val + delta, target)
                        : Math.Max(val - delta, target);

    private static bool PointInRect(Vector2 pt, Vector2 origin, float w, float h)
        => pt.X >= origin.X && pt.X <= origin.X + w &&
           pt.Y >= origin.Y && pt.Y <= origin.Y + h;
}
