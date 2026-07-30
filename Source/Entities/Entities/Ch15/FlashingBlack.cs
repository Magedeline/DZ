namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// FlashingBlack - Citadel blackout. Cycles visible, a warning flash, then a full
    /// black screen for <c>blackDuration</c> before fading back. The room stays fully
    /// playable in the dark, so it is a memory test rather than a hazard.
    ///
    /// One instance covers the whole room; it renders over everything via a very
    /// negative depth. <c>keepPlayerVisible</c> leaves a faint marker at the player so
    /// blackouts can be tuned from "hard" to "brutal".
    ///
    /// Placeholder art: full-screen black square (a small marker square in the editor).
    /// </summary>
    [CustomEntity("DZ/FlashingBlack")]
    [Tracked]
    public class FlashingBlack : Entity
    {
        #region Enums
        private enum Phase
        {
            Visible,
            FadingOut,
            Black,
            FadingIn
        }
        #endregion

        #region Fields
        private readonly float visibleDuration;
        private readonly float fadeDuration;
        private readonly float blackDuration;
        private readonly float maxAlpha;
        private readonly bool keepPlayerVisible;
        private readonly bool warnFlash;
        private readonly string flag;
        private readonly bool invertFlag;

        private Phase phase = Phase.Visible;
        private float phaseTimer;
        private float alpha;
        private float warnAlpha;
        private Level level;
        #endregion

        #region Constructor
        public FlashingBlack(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            visibleDuration = Math.Max(0.1f, data.Float("visibleDuration", 2.4f));
            fadeDuration = Math.Max(0.01f, data.Float("fadeDuration", 0.35f));
            blackDuration = Math.Max(0.1f, data.Float("blackDuration", 1.1f));
            maxAlpha = MathHelper.Clamp(data.Float("maxAlpha", 1f), 0f, 1f);
            keepPlayerVisible = data.Bool("keepPlayerVisible", false);
            warnFlash = data.Bool("warnFlash", true);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            if (data.Bool("startBlack", false))
            {
                phase = Phase.Black;
                alpha = maxAlpha;
                phaseTimer = blackDuration;
            }
            else
            {
                phaseTimer = visibleDuration;
            }

            // Above every normal entity and the player.
            Depth = -1000000;
        }
        #endregion

        #region Private
        /// <summary>Whether this entity's enable flag currently permits it to run.</summary>
        private bool FlagAllows()
        {
            if (string.IsNullOrEmpty(flag) || level == null)
            {
                return true;
            }

            return level.Session.GetFlag(flag) != invertFlag;
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            warnAlpha = Calc.Approach(warnAlpha, 0f, Engine.DeltaTime * 4f);

            if (!FlagAllows())
            {
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime / fadeDuration);
                return;
            }

            phaseTimer -= Engine.DeltaTime;

            switch (phase)
            {
                case Phase.Visible:
                    alpha = Calc.Approach(alpha, 0f, maxAlpha / fadeDuration * Engine.DeltaTime);

                    if (warnFlash && phaseTimer <= 0.25f && warnAlpha <= 0f && phaseTimer > 0f)
                    {
                        warnAlpha = 0.5f;
                        Audio.Play("event:/game/general/thing_booped", Position);
                    }

                    if (phaseTimer <= 0f)
                    {
                        phase = Phase.FadingOut;
                        phaseTimer = fadeDuration;
                    }
                    break;

                case Phase.FadingOut:
                    alpha = Calc.Approach(alpha, maxAlpha, maxAlpha / fadeDuration * Engine.DeltaTime);

                    if (phaseTimer <= 0f)
                    {
                        phase = Phase.Black;
                        phaseTimer = blackDuration;
                        alpha = maxAlpha;
                    }
                    break;

                case Phase.Black:
                    if (phaseTimer <= 0f)
                    {
                        phase = Phase.FadingIn;
                        phaseTimer = fadeDuration;
                    }
                    break;

                case Phase.FadingIn:
                    alpha = Calc.Approach(alpha, 0f, maxAlpha / fadeDuration * Engine.DeltaTime);

                    if (phaseTimer <= 0f)
                    {
                        phase = Phase.Visible;
                        phaseTimer = visibleDuration;
                        alpha = 0f;
                    }
                    break;
            }
        }

        public override void Render()
        {
            base.Render();

            if (level == null || (alpha <= 0.002f && warnAlpha <= 0.002f))
            {
                return;
            }

            // Cover the camera view with a margin so no edge shows during shake.
            Vector2 topLeft = level.Camera.Position - new Vector2(16f, 16f);
            const float width = 320f + 32f;
            const float height = 180f + 32f;

            if (warnAlpha > 0.002f)
            {
                Draw.Rect(topLeft.X, topLeft.Y, width, height, Color.White * warnAlpha * 0.35f);
            }

            if (alpha > 0.002f)
            {
                Draw.Rect(topLeft.X, topLeft.Y, width, height, Color.Black * alpha);

                if (keepPlayerVisible)
                {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    if (player != null)
                    {
                        Placeholder.FillCircle(player.Center, 4f, Color.White * (alpha * 0.35f));
                    }
                }
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(X - 8f, Y - 8f, 16f, 16f, Color.Gray);
        }
        #endregion
    }
}
