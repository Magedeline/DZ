#nullable enable


namespace Celeste.Cutscenes
{
    /// <summary>
    /// Part 1 Credits cutscene implementation
    /// Displays credits for the first part of the Ingeste mod
    /// </summary>
    public class CsPart1Credit : Oui
    {
        #region Constants
        private static readonly Vector2 on_screen_position = new Vector2(960f, 0f);
        private static readonly Vector2 off_screen_position = new Vector2(3840f, 0f);
        private const float transition_speed = 4f;
        private const float vignette_alpha_speed = 1f;
        private const float credits_timeout = 3f;
        private const string music_event = "event:/desolozantas/music/lvl9/epilouge/outro";
        #endregion

        #region Fields
        private Credits? credits;
        private float vignetteAlpha;
        private bool isInitialized;
        #endregion

        #region Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            initialize();
        }

        public override IEnumerator Enter(Oui from)
        {
            yield return enterSequence();
        }

        public override IEnumerator Leave(Oui next)
        {
            yield return leaveSequence();
        }

        public override void Update()
        {
            try
            {
                handleInput();
                updateCredits();
                updateVignette();
                base.Update();
            }
            catch (System.Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper", $"Error in CsPart1Credit.Update: {ex}");
            }
        }

        public override void Render()
        {
            try
            {
                // Only render when this Oui is selected and visible to prevent overlap
                if (!Selected || !Visible) return;
                
                renderVignette();
                renderCredits();
            }
            catch (System.Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper", $"Error in CsPart1Credit.Render: {ex}");
            }
        }
        #endregion

        #region Private Methods
        private void initialize()
        {
            if (isInitialized) return;

            Position = off_screen_position;
            Visible = false;
            vignetteAlpha = 0f;
            isInitialized = true;
        }

        private IEnumerator enterSequence()
        {
            // Setup audio
            Audio.SetMusic(music_event);

            if (Overworld != null)
            {
                Overworld.ShowConfirmUI = false;
            }

            // Setup credits
            Credits.BorderColor = Color.Black;
            credits = new Credits
            {
                Enabled = false
            };

            // Make visible and animate in
            Visible = true;
            vignetteAlpha = 0f;

            // Smooth transition animation
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime * transition_speed)
            {
                Position = Vector2.Lerp(off_screen_position, on_screen_position, Ease.CubeOut(progress));
                vignetteAlpha = Ease.CubeIn(progress) * vignette_alpha_speed;
                yield return null;
            }

            Position = on_screen_position;
            vignetteAlpha = vignette_alpha_speed;
        }

        private void handleInput()
        {
            if (!Focused) return;

            bool shouldExit = Input.MenuCancel.Pressed ||
                             (credits?.BottomTimer > credits_timeout);

            if (shouldExit && Overworld != null)
            {
                Overworld.Goto<CsPart1Credit>();
            }
        }

        private void updateCredits()
        {
            if (credits == null) return;

            credits.Update();
            credits.Enabled = Focused && Selected;
        }

        private void updateVignette()
        {
            float targetAlpha = Selected ? 1f : 0f;
            float speed = Selected ? vignette_alpha_speed : vignette_alpha_speed * 4f;

            vignetteAlpha = Calc.Approach(vignetteAlpha, targetAlpha,
                Engine.DeltaTime * speed);
        }

        private void renderVignette()
        {
            if (vignetteAlpha <= 0f) return;

            // Draw background overlay
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * vignetteAlpha * 0.4f);

            // Draw vignette effect if available
            var alpha = Ease.CubeInOut(vignetteAlpha);
            var vignetteTexture = TextureUtil.TryGetOverworldTexture("vignette");

            if (vignetteTexture != null)
            {
                // Use Draw.SpriteBatch or similar method to draw the Texture2D
                // Example using Monocle's Draw.SpriteBatch:
                Draw.SpriteBatch.Draw(
                    vignetteTexture.Texture.Texture,
                    Vector2.Zero,
                    null,
                    Color.White * alpha,
                    0f,
                    Vector2.Zero,
                    1f,
                    Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                    0f
                );
            }
        }

        private void renderCredits()
        {
            credits?.Render(Position);
        }

        private IEnumerator leaveSequence()
        {
            Audio.Play("event:/ui/main/whoosh_large_out");

            if (Overworld != null)
            {
                Overworld.SetNormalMusic();
                Overworld.ShowConfirmUI = true;
            }

            // Smooth transition animation
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime * transition_speed)
            {
                Position = Vector2.Lerp(on_screen_position, off_screen_position, Ease.CubeIn(progress));
                yield return null;
            }

            Position = off_screen_position;
            Visible = false;
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            credits = null;
            base.Removed(scene);
        }
        #endregion
    }
}
