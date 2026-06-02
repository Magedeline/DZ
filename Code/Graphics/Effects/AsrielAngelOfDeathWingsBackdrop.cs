using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Effects
{
    /// <summary>
    /// Asriel Angel of Death wings backdrop.
    /// Renders the Asriel God background visible through a central gap while two demon wings
    /// slide outward from the center, revealing the bg00 grid texture in between.
    /// </summary>
    [CustomBackdrop("MaggyHelper/AsrielAngelOfDeathWingsBackdrop")]
    [HotReloadable]
    public class AsrielAngelOfDeathWingsBackdrop : Backdrop
    {
        #region Constants
        private const float SCREEN_W = 320f;
        private const float SCREEN_H = 180f;
        private const float MAX_WING_OFFSET = 260f;
        #endregion

        #region Fields
        private readonly MTexture wingTexture;
        private readonly MTexture bgTexture;

        private VirtualRenderTarget renderTarget;

        private float globalTime;
        private float wingOffset;         // How far each wing has slid outward from center (px)
        private float bgScrollPhase;      // Phase for bg00 colour cycling / expansion

        // Configuration
        public float Intensity = 1f;
        public new float Speed = 1f;
        public float WingScale = 1f;
        public float ExpansionSpeed = 18f; // px/s outward travel speed
        public bool Loop = true;           // Reset wings and repeat
        public Color WingTint = Color.White;
        public float BgAlpha = 1f;         // Alpha of the bg00 seen through the gap
        public float WingAlpha = 1f;       // Alpha of the wing sprites

        // Internal rainbow colour for the bg
        private float rainbowPhase;
        #endregion

        #region Constructor
        public AsrielAngelOfDeathWingsBackdrop()
        {
            try
            {
                wingTexture = GFX.Game["decals/19_goodbye/hyperdreemurr_massive_wings"];
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/AsrielAngelOfDeathWingsBackdrop",
                    $"Wing texture not found: {ex.Message}");
            }

            try
            {
                bgTexture = GFX.Game["bgs/20/asriel/bg00"];
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/AsrielAngelOfDeathWingsBackdrop",
                    $"Bg texture not found: {ex.Message}");
            }
        }

        public AsrielAngelOfDeathWingsBackdrop(BinaryPacker.Element data) : this()
        {
            if (data.HasAttr("intensity"))
                Intensity = data.AttrFloat("intensity", 1f);
            if (data.HasAttr("speed"))
                Speed = data.AttrFloat("speed", 1f);
            if (data.HasAttr("wingScale"))
                WingScale = data.AttrFloat("wingScale", 1f);
            if (data.HasAttr("expansionSpeed"))
                ExpansionSpeed = data.AttrFloat("expansionSpeed", 18f);
            if (data.HasAttr("loop"))
                Loop = data.AttrBool("loop", true);
            if (data.HasAttr("bgAlpha"))
                BgAlpha = data.AttrFloat("bgAlpha", 1f);
            if (data.HasAttr("wingAlpha"))
                WingAlpha = data.AttrFloat("wingAlpha", 1f);
        }
        #endregion

        #region Update
        public override void Update(Scene scene)
        {
            base.Update(scene);

            if (!Visible)
                return;

            float dt = Engine.DeltaTime * Speed;
            globalTime += dt;
            rainbowPhase += dt * 0.5f;

            // bg00 scroll phase (matches AsrielGodBackdrop feel)
            bgScrollPhase += dt * 0.3f;

            // Expand wings outward
            wingOffset += dt * ExpansionSpeed;

            if (Loop && wingOffset > MAX_WING_OFFSET)
            {
                wingOffset = 0f;
            }
            else if (!Loop)
            {
                wingOffset = Math.Min(wingOffset, MAX_WING_OFFSET);
            }
        }
        #endregion

        #region Rendering
        public override void BeforeRender(Scene scene)
        {
            if (renderTarget == null || renderTarget.IsDisposed)
            {
                renderTarget = VirtualContent.CreateRenderTarget("AsrielAngelOfDeathWings", 320, 180);
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Black);

            DrawBackground();
            DrawWings();
        }

        private void DrawBackground()
        {
            if (bgTexture == null)
                return;

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Cycle through colours matching the God backdrop aesthetic
            float hue = (rainbowPhase * 0.15f) % 1f;
            Color bgTint = HSVToRGB(hue, 0.7f, 1f) * BgAlpha * Intensity;

            // Expansion pulse
            float pulse = (float)Math.Sin(globalTime * 0.5f) * 0.15f + 1f;
            float bgScale = (SCREEN_W / bgTexture.Width) * pulse * 1.1f;

            Vector2 bgCenter = new(SCREEN_W * 0.5f, SCREEN_H * 0.5f);
            Vector2 bgOrigin = new(bgTexture.Width * 0.5f, bgTexture.Height * 0.5f);

            bgTexture.Draw(bgCenter, bgOrigin, bgTint, bgScale, 0f);

            Draw.SpriteBatch.End();
        }

        private void DrawWings()
        {
            if (wingTexture == null)
                return;

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            float scale = WingScale;
            Vector2 origin = new(wingTexture.Width * 0.5f, wingTexture.Height * 0.5f);

            // Fade wings in slightly as they move out so the gap starts clear
            float fadeAlpha = MathHelper.Clamp(wingOffset / 30f, 0f, 1f) * WingAlpha * Intensity;

            Color tint = WingTint * fadeAlpha;

            // Center screen
            float cx = SCREEN_W * 0.5f;
            float cy = SCREEN_H * 0.5f;

            // Right wing — original orientation, slides right
            Vector2 rightPos = new(cx + wingOffset, cy);
            wingTexture.Draw(rightPos, origin, tint, scale, 0f);

            // Left wing — mirrored horizontally, slides left
            Vector2 leftPos = new(cx - wingOffset, cy);
            wingTexture.Draw(leftPos, origin, tint, new Vector2(-scale, scale), 0f);

            Draw.SpriteBatch.End();
        }

        public override void Render(Scene scene)
        {
            if (renderTarget == null || renderTarget.IsDisposed || !Visible)
                return;

            Vector2 renderPos = new(160f, 90f);
            Vector2 origin = new Vector2(renderTarget.Width, renderTarget.Height) * 0.5f;

            Draw.SpriteBatch.Draw(
                (RenderTarget2D)renderTarget,
                renderPos,
                renderTarget.Bounds,
                Color.White * FadeAlphaMultiplier * Intensity,
                0f,
                origin,
                1f,
                SpriteEffects.None,
                0f
            );
        }
        #endregion

        #region Cleanup
        public override void Ended(Scene scene)
        {
            base.Ended(scene);

            if (renderTarget != null)
            {
                renderTarget.Dispose();
                renderTarget = null;
            }
        }
        #endregion

        #region Helpers
        private static Color HSVToRGB(float h, float s, float v)
        {
            int i = (int)Math.Floor(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            return (i % 6) switch
            {
                0 => new Color(v, t, p),
                1 => new Color(q, v, p),
                2 => new Color(p, v, t),
                3 => new Color(p, q, v),
                4 => new Color(t, p, v),
                5 => new Color(v, p, q),
                _ => Color.White
            };
        }
        #endregion
    }
}
