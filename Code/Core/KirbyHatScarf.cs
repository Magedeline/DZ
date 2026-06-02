using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Renders a Kirby-style hat + scarf on top of the player sprite.
    /// Replaces the hair visual when KirbyModeActive is true.
    /// Color mirrors the hair dash-tier color system so all existing
    /// dash, Kirby-pink, and combat colors still apply.
    /// </summary>
    public class KirbyHatScarf : Component
    {
        // ── Layout constants (pixels, relative to K_Player Center) ─────────────
        // All offsets assume the sprite is facing RIGHT; X is flipped for left.

        // Hat brim — wide flat rectangle sitting just above the head
        private const float BrimW      = 10f;
        private const float BrimH      =  2f;
        private const float BrimOffY   = -11f;   // up from Center

        // Hat crown — narrower rectangle on top of brim
        private const float CrownW     =  7f;
        private const float CrownH     =  5f;
        private const float CrownOffY  = -13f;   // up from Center (brim top)

        // Hat band — thin stripe across the crown base
        private const float BandH      =  1f;

        // Scarf — two rectangles: a collar wrap + a trailing tail
        private const float CollarW    =  8f;
        private const float CollarH    =  3f;
        private const float CollarOffY = -4f;    // just below chin

        private const float TailW      =  3f;
        private const float TailH      =  5f;
        private const float TailOffX   =  3f;    // hangs to the side facing away
        private const float TailOffY   = -3f;

        // ── State ─────────────────────────────────────────────────────────────

        /// <summary>Current hat + scarf tint. Set each frame from UpdateHair.</summary>
        public Color Color = Calc.HexToColor("ff99cc");

        /// <summary>Secondary darker color for the hat band / scarf shadow.</summary>
        public Color AccentColor = Color.White * 0.55f;

        /// <summary>Whether to draw the hat+scarf at all.</summary>
        public new bool Visible = false;

        private readonly K_Player player;

        // ── Constructor ───────────────────────────────────────────────────────

        public KirbyHatScarf(K_Player player) : base(active: true, visible: false)
        {
            this.player = player;
        }

        // ── Render ────────────────────────────────────────────────────────────

        public override void Render()
        {
            if (!Visible) return;

            // Pixel-snap position like the rest of the sprite
            Vector2 center = (player.Sprite.RenderPosition).Floor();
            int dir = (int)player.Facing;   // 1 = right, -1 = left

            Color main   = Color;
            Color accent = AccentColor;
            Color shadow = main * 0.6f;

            // ── Hat brim ──────────────────────────────────────────────────────
            Draw.Rect(
                center.X - BrimW * 0.5f,
                center.Y + BrimOffY - BrimH,
                BrimW, BrimH,
                main);

            // ── Hat crown ─────────────────────────────────────────────────────
            Draw.Rect(
                center.X - CrownW * 0.5f,
                center.Y + CrownOffY - CrownH,
                CrownW, CrownH,
                main);

            // ── Hat band (accent stripe at the base of the crown) ────────────
            Draw.Rect(
                center.X - CrownW * 0.5f,
                center.Y + CrownOffY - BandH,
                CrownW, BandH,
                accent);

            // ── Hat shadow (right side of crown, 1-pixel darkening) ──────────
            Draw.Rect(
                center.X + CrownW * 0.5f - 1f,
                center.Y + CrownOffY - CrownH,
                1f, CrownH,
                shadow);

            // ── Scarf collar ─────────────────────────────────────────────────
            Draw.Rect(
                center.X - CollarW * 0.5f,
                center.Y + CollarOffY,
                CollarW, CollarH,
                main);

            // ── Scarf tail (hangs away from facing direction) ─────────────────
            // tail hangs on the side opposite to facing (behind the K_Player)
            float tailX = center.X - dir * TailOffX - TailW * 0.5f;
            Draw.Rect(
                tailX,
                center.Y + TailOffY,
                TailW, TailH,
                main);

            // Tail tip — slightly lighter to show it's dangling
            Draw.Rect(
                tailX,
                center.Y + TailOffY + TailH - 1f,
                TailW, 1f,
                accent);
        }
    }
}
