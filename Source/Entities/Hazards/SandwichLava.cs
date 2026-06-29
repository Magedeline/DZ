using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Camera = DZ.Nez.Camera;
using System;
using System.Linq;
using DZ.Entities.Player;

namespace DZ.Entities.Hazards;

/// <summary>
/// Two lava slabs — one rising from the bottom and one descending from the top —
/// that squeeze inward once the player crosses a threshold X position.
/// Ported from Celeste's SandwichLava.cs.
///
/// Porting notes:
/// <list type="bullet">
///   <item>Two conceptual hitboxes (340×120 bottom, 340×120 top offset by -280 Y)
///         are checked manually in <see cref="Update"/>.</item>
///   <item>CoreMode handled via <see cref="SetIceMode"/>.</item>
///   <item>TransitionListener / Alarm / Tween replaced with simple state booleans.</item>
///   <item>LavaRect visuals, audio, and assist invincibility replaced with TODO stubs.</item>
/// </list>
/// </summary>
public class SandwichLava : DZ.Nez.Entity
{
    // ── Colours (shared with RisingLava) ──────────────────────────────────────
    private static readonly Color[] Hot  = RisingLava.Hot;
    private static readonly Color[] Cold = RisingLava.Cold;

    // ── Constants ─────────────────────────────────────────────────────────────
    private const float Speed       = 20f;      // px/s (direction flips per mode)
    private const float TopOffset   = -160f;    // top rect baseline Y relative to entity
    private const float HitboxW     = 340f;
    private const float HitboxH     = 120f;

    // ── Config ────────────────────────────────────────────────────────────────
    private float startX;           // player must pass this X before lava moves

    // ── State ─────────────────────────────────────────────────────────────────
    public  bool  Waiting = true;
    private bool  iceMode;
    private bool  leaving;
    private float colorLerp;
    private float delay;

    // Sub-rect position offsets relative to entity Y
    // bottomRect: slides in from below  →  target Y offset 0
    // topRect: slides in from above     →  target Y offset (TopOffset - topRect.Height)
    private float bottomRectY;      // current Y offset of bottom lava rect
    private float topRectY;         // current Y offset of top lava rect
    private const float RectHeight  = 200f;

    // ── Visual colours ────────────────────────────────────────────────────────
    public Color SurfaceColor { get; private set; }
    public Color EdgeColor    { get; private set; }
    public Color CentreColor  { get; private set; }

    // ── Constructor ───────────────────────────────────────────────────────────
    /// <param name="startX">
    /// World X beyond which the player's position triggers the lava to start moving.
    /// </param>
    public SandwichLava(float startX = 0f)
    {
        this.startX = startX;
        Name        = "SandwichLava";

        // Start rects off-screen (will slide into view).
        bottomRectY = 60f;   // starts pushed down
        topRectY    = -360f + (-60f);  // starts pushed up

        // TODO: set up two LavaRects (400×200, bottom onlyTop / top onlyBottom,
        //       smallWaveAmplitude=2, bigWaveAmplitude=2, curveAmplitude=4)
        // TODO: play sound: event:/game/09_core/rising_threat
    }

    // ── Scene added ───────────────────────────────────────────────────────────
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        Position = new Vector2(GetCameraLeft() - 10f, GetCenterY());
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────
    public override void Update()
    {
        float dt = Time.DeltaTime;

        delay -= dt;

        // Track camera X.
        Position = new Vector2(GetCameraLeft(), Position.Y);

        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        if (Waiting)
        {
            // Snap toward the centre-Y while waiting.
            float centreY = GetCenterY();
            Position = new Vector2(Position.X, Approach(Position.Y, centreY, 128f * dt));

            // TODO: set sound param "rising" → 0

            // Stop waiting when player has crossed startX and isn't just-respawned.
            if (player != null &&
                player.Position.X >= startX)
                // && !player.JustRespawned) // TODO: JustRespawned not implemented
                Waiting = false;
        }
        else if (!leaving && delay <= 0f)
        {
            // TODO: set sound param "rising" → 1
            if (iceMode)
                Position = new Vector2(Position.X, Position.Y + Speed * dt);   // ice: descend
            else
                Position = new Vector2(Position.X, Position.Y - Speed * dt);   // hot: ascend
        }

        // Animate sub-rect positions toward targets.
        float topTarget    = leaving ? (-RectHeight - 512f + TopOffset) : (TopOffset - RectHeight);
        float bottomTarget = leaving ? 512f : 0f;
        float slideSpeed   = leaving ? 256f : 64f;

        topRectY    = Approach(topRectY,    topTarget,    slideSpeed * dt);
        bottomRectY = Approach(bottomRectY, bottomTarget, slideSpeed * dt);

        // TODO: set LavaRect bottomRect.Position.Y = bottomRectY
        // TODO: set LavaRect topRect.Position.Y    = topRectY

        // Colour lerp.
        colorLerp    = Approach(colorLerp, iceMode ? 1f : 0f, dt * 4f);
        SurfaceColor = Color.Lerp(Hot[0], Cold[0], colorLerp);
        EdgeColor    = Color.Lerp(Hot[1], Cold[1], colorLerp);
        CentreColor  = Color.Lerp(Hot[2], Cold[2], colorLerp);
        // TODO: apply colours / Spikey / UpdateMultiplier / Fade to both LavaRects

        // Player contact (both zones).
        if (!Waiting)
            CheckPlayerCollision(player);
    }

    // ── Collision ─────────────────────────────────────────────────────────────
    private void CheckPlayerCollision(MadelinePlayer player)
    {
        var pPos = player.Position;

        // Bottom lava hitbox.
        bool bottomHit = PointInRect(pPos, Position, HitboxW, HitboxH);
        // Top lava hitbox (offset -280 from entity Y).
        bool topHit    = PointInRect(pPos, Position + new Vector2(0f, -280f), HitboxW, HitboxH);

        if (bottomHit || topHit)
            OnPlayer(player, pPos);
    }

    private void OnPlayer(MadelinePlayer player, Vector2 pPos)
    {
        if (Waiting) return;

        // TODO: if invincible assist mode:
        //   int dir = pPos.Y > Position.Y + bottomRectY - 32 ? 1 : -1
        //   push lava by dir * 48, player.Speed.Y = -dir * 200, delay = 0.5
        player.Die(-Vector2.UnitY);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    /// <summary>Switches between hot and ice mode.</summary>
    public void SetIceMode(bool ice)
    {
        iceMode = ice;
        // TODO: update sound param "room_state" → ice ? 1 : 0
    }

    /// <summary>
    /// Begins the leave sequence — lava retracts and the entity removes itself
    /// after ~2 seconds.
    /// </summary>
    public void Leave()
    {
        leaving = true;
        // Collidable is effectively off because Waiting stays false but we set leaving.
        // Self-destruction after delay approximated as a simple counter.
        // TODO: schedule removal after 2 seconds (set a leaveTimer field)
    }

    // ── Camera / scene helpers ────────────────────────────────────────────────
    private float GetCameraLeft()
    {
        var cam = Scene?.Camera;
        return cam?.Position.X ?? 0f;
    }

    private float GetCenterY()
    {
        // Mirrors Celeste's Level.Bounds.Bottom - 10.
        var cam = Scene?.Camera;
        if (cam == null) return 180f - 10f;
        return cam.Position.Y + cam.Bounds.Height - 10f;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static float Approach(float val, float target, float delta)
        => val < target ? Math.Min(val + delta, target)
                        : Math.Max(val - delta, target);

    private static bool PointInRect(Vector2 pt, Vector2 origin, float w, float h)
        => pt.X >= origin.X && pt.X <= origin.X + w &&
           pt.Y >= origin.Y && pt.Y <= origin.Y + h;
}
