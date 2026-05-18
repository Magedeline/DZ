#nullable enable

namespace Celeste.UI
{
    /// <summary>
    /// Custom main-menu credits scene for the Desolo Zantas mod.
    /// Uses vanilla-style credit nodes while keeping all credit copy in
    /// MAGGY_CREDIT_* dialog keys.
    /// </summary>
    [HotReloadable]
    public class CreditsMaggy : Scene
    {
        #region ── Constants ──────────────────────────────────────────────

        private const float FADE_IN_DURATION = 1.5f;
        private const float FADE_OUT_DURATION = 1.0f;
        private const float HOLD_AFTER_LAST = 3.0f;

        private const float CREDIT_SPACING = 64f;
        private const float AUTO_SCROLL_SPEED = 100f;
        private const float INPUT_SCROLL_SPEED = 600f;
        private const float SCROLL_RESUME_DELAY = 1f;
        private const float SCROLL_ACCELERATION = 1800f;
        private const float HEIGHT_PADDING = 476f;

        private const string CREDIT_MUSIC_EVENT = "event:/desolozantas/music/menu/credits";
        private const string BACK_SFX = "event:/ui/main/button_back";

        private static readonly Color BorderColor = Color.Black;

        #endregion

        #region ── Nested types ───────────────────────────────────────────

        private enum SequencePhase
        {
            FadeIn,
            Scroll,
            Hold,
            FadeOut,
            Complete
        }

        private abstract class CreditNode
        {
            public abstract void Render(Vector2 position, float alignment, float scale, float alpha);

            public abstract float Height(float scale);
        }

        private sealed class Role : CreditNode
        {
            private const float NameScale = 1.8f;
            private const float RolesScale = 1f;
            private const float Spacing = 8f;
            private const float BottomSpacing = 64f;

            private static readonly Color NameColor = Color.White;
            private static readonly Color RolesColor = Color.White * 0.8f;

            private readonly string name;
            private readonly string roles;

            public Role(string name, params string[] roles)
            {
                this.name = name;
                this.roles = string.Join(", ", roles);
            }

            public override void Render(Vector2 position, float alignment, float scale, float alpha)
            {
                DrawCreditText(position, alignment, 0f, NameScale * scale, name, NameColor, alpha);
                position.Y += (LineHeight * NameScale + Spacing) * scale;
                DrawCreditText(position, alignment, 0f, RolesScale * scale, roles, RolesColor, alpha);
            }

            public override float Height(float scale)
            {
                return (LineHeight * (NameScale + RolesScale) + Spacing + BottomSpacing) * scale;
            }
        }

        private sealed class Thanks : CreditNode
        {
            private const float TitleScale = 1.4f;
            private const float CreditsScale = 1.15f;
            private const float Spacing = 8f;

            private static readonly Color TitleColor = Color.White;
            private static readonly Color CreditsColor = Color.White * 0.8f;

            private readonly int topPadding;
            private readonly string title;
            private readonly string[] credits;

            public Thanks(string title, params string[] credits)
                : this(0, title, credits)
            {
            }

            public Thanks(int topPadding, string title, params string[] credits)
            {
                this.topPadding = topPadding;
                this.title = title;
                this.credits = credits;
            }

            public override void Render(Vector2 position, float alignment, float scale, float alpha)
            {
                position.Y += topPadding * scale;
                DrawCreditText(position, alignment, 0f, TitleScale * scale, title, TitleColor, alpha);
                position.Y += (LineHeight * TitleScale + Spacing) * scale;

                foreach (string credit in credits)
                {
                    DrawCreditText(position, alignment, 0f, CreditsScale * scale, credit, CreditsColor, alpha);
                    position.Y += LineHeight * CreditsScale * scale;
                }
            }

            public override float Height(float scale)
            {
                return (LineHeight * (TitleScale + credits.Length * CreditsScale)
                    + (credits.Length != 0 ? Spacing : 0f)
                    + topPadding) * scale;
            }
        }

        private sealed class Ending : CreditNode
        {
            private readonly string text;
            private readonly bool spacing;

            public Ending(string text, bool spacing)
            {
                this.text = text;
                this.spacing = spacing;
            }

            public override void Render(Vector2 position, float alignment, float scale, float alpha)
            {
                if (spacing)
                {
                    position.Y += 540f * scale;
                }
                else
                {
                    position.Y += LineHeight * 1.5f * scale * 0.5f;
                }

                DrawCreditText(position, alignment, 0.5f, 1.5f * scale, text, Color.White, alpha);
            }

            public override float Height(float scale)
            {
                return spacing ? 540f * scale : LineHeight * 1.5f * scale;
            }
        }

        private sealed class Break : CreditNode
        {
            private readonly float size;

            public Break(float size = 64f)
            {
                this.size = size;
            }

            public override void Render(Vector2 position, float alignment, float scale, float alpha)
            {
            }

            public override float Height(float scale)
            {
                return size * scale;
            }
        }

        #endregion

        #region ── Fields ─────────────────────────────────────────────────

        private readonly List<CreditNode> credits = new();

        private float height;
        private float scrollY;
        private float scrollSpeed = AUTO_SCROLL_SPEED;
        private float scrollDelay;
        private float scrollbarAlpha;

        private float fade;
        private float fadeOut;
        private float phaseTimer;
        private float holdTimer;

        private bool started;
        private bool finished;
        private bool exiting;

        private SequencePhase phase = SequencePhase.FadeIn;

        private HiresSnow snow = null!;
        private HudRenderer hud = null!;
        private FMOD.Studio.EventInstance? music;

        internal bool AllowInput = true;
        internal float BottomTimer;

        #endregion

        #region ── Constructor ────────────────────────────────────────────

        public CreditsMaggy()
        {
            BuildCredits();
            RecalculateHeight();

            snow = new HiresSnow();
            hud = new HudRenderer();

            Add(snow);
            Add(hud);
            RendererList.UpdateLists();
        }

        #endregion

        #region ── Credit Data ────────────────────────────────────────────

        private static float LineHeight => ActiveFont.LineHeight;

        private static string CreditText(string dialogKey)
        {
            return Dialog.Clean(dialogKey);
        }

        private static string[] CreditRange(string prefix, int first, int last, int padWidth = 1)
        {
            string format = padWidth == 2 ? "00" : "0";
            List<string> values = new(last - first + 1);

            for (int index = first; index <= last; index++)
            {
                values.Add(CreditText(prefix + index.ToString(format)));
            }

            return values.ToArray();
        }

        private void BuildCredits()
        {
            credits.Clear();

            credits.Add(new Ending(CreditText("MAGGY_CREDIT_TITLE"), spacing: false));
            credits.Add(new Break(192f));

            credits.Add(new Role(CreditText("MAGGY_CREDIT_DIRECTION_1"), CreditText("MAGGY_CREDIT_DIRECTION")));
            credits.Add(new Role(CreditText("MAGGY_CREDIT_PROGRAMMING_1"), CreditText("MAGGY_CREDIT_PROGRAMMING")));
            credits.Add(new Role(CreditText("MAGGY_CREDIT_ART_1"), CreditText("MAGGY_CREDIT_ART")));
            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_MUSIC"), CreditRange("MAGGY_CREDIT_MUSIC_", 1, 26)));
            credits.Add(new Role(CreditText("MAGGY_CREDIT_LEVEL_DESIGN_1"), CreditText("MAGGY_CREDIT_LEVEL_DESIGN")));
            credits.Add(new Role(CreditText("MAGGY_CREDIT_WRITING_1"), CreditText("MAGGY_CREDIT_WRITING")));
            credits.Add(new Role(CreditText("MAGGY_CREDIT_TESTING_1"), CreditText("MAGGY_CREDIT_TESTING")));

            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_HELPERS"), CreditRange("MAGGY_CREDIT_HELPER_", 1, 34, padWidth: 2)));
            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_CONTRIBUTORS"), CreditRange("MAGGY_CREDIT_CONTRIB_", 1, 12, padWidth: 2)));

            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_CELESTE_CREATED"), CreditRange("MAGGY_CREDIT_CELESTE_", 1, 2)));
            credits.Add(new Role(CreditText("MAGGY_CREDIT_CELESTE_MUSIC_1"), CreditText("MAGGY_CREDIT_CELESTE_MUSIC_HEAD")));
            credits.Add(new Role(CreditText("MAGGY_CREDIT_CELESTE_AUDIO_1"), CreditText("MAGGY_CREDIT_CELESTE_AUDIO")));
            credits.Add(new Role(CreditText("MAGGY_CREDIT_CELESTE_ART_1"), CreditText("MAGGY_CREDIT_CELESTE_ART")));

            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_EVEREST"), CreditRange("MAGGY_CREDIT_EVEREST_", 1, 4)));
            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_EDITOR"), CreditRange("MAGGY_CREDIT_EDITOR_", 1, 2)));
            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_SPECIAL_THANKS"), CreditRange("MAGGY_CREDIT_SPECIAL_THANKS_", 1, 4)));
            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_NO_SPECIAL_THANKS"), CreditRange("MAGGY_CREDIT_NO_SPECIAL_THANKS_", 1, 4)));
            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_COMPANY_RIGHT_BY"), CreditRange("MAGGY_CREDIT_COMPANY_RIGHT_BY_", 1, 5)));
            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_POWERED_BY"), CreditRange("MAGGY_CREDIT_POWERED_BY_", 1, 2)));
            credits.Add(new Thanks(CreditText("MAGGY_CREDIT_FINAL_THANK_BY"), CreditText("MAGGY_CREDIT_FINAL_THANK_BY_1"), CreditText("MAGGY_CREDIT_FINAL_THANK_BY_SUB")));
        }

        private void RecalculateHeight()
        {
            height = 0f;
            foreach (CreditNode credit in credits)
            {
                height += credit.Height(1f) + CREDIT_SPACING;
            }

            height += HEIGHT_PADDING;
        }

        #endregion

        #region ── Update ─────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();

            StartMusicIfNeeded();
            UpdateSequence();
        }

        private void StartMusicIfNeeded()
        {
            if (started)
            {
                return;
            }

            started = true;
            TryStartMusic();
        }

        private void UpdateSequence()
        {
            if (finished)
            {
                return;
            }

            if (AllowInput && !exiting && Input.MenuCancel.Pressed)
            {
                Audio.Play(BACK_SFX);
                BeginExit();
            }

            switch (phase)
            {
                case SequencePhase.FadeIn:
                    phaseTimer += Engine.DeltaTime;
                    fade = Calc.Clamp(phaseTimer / FADE_IN_DURATION, 0f, 1f);
                    if (phaseTimer >= FADE_IN_DURATION)
                    {
                        fade = 1f;
                        phaseTimer = 0f;
                        phase = SequencePhase.Scroll;
                    }
                    break;

                case SequencePhase.Scroll:
                case SequencePhase.Hold:
                    UpdateScroll();

                    if (scrollY >= height)
                    {
                        BottomTimer += Engine.DeltaTime;

                        if (!AllowInput)
                        {
                            return;
                        }

                        phase = SequencePhase.Hold;
                        holdTimer += Engine.DeltaTime;

                        if (!exiting && (holdTimer >= HOLD_AFTER_LAST || Input.MenuConfirm.Pressed))
                        {
                            Audio.Play(BACK_SFX);
                            BeginExit();
                        }
                    }
                    else
                    {
                        BottomTimer = 0f;
                        holdTimer = 0f;
                        if (phase == SequencePhase.Hold)
                        {
                            phase = SequencePhase.Scroll;
                        }
                    }
                    break;

                case SequencePhase.FadeOut:
                    phaseTimer += Engine.DeltaTime;
                    fadeOut = Calc.Clamp(phaseTimer / FADE_OUT_DURATION, 0f, 1f);
                    if (phaseTimer >= FADE_OUT_DURATION)
                    {
                        fadeOut = 1f;
                        finished = true;
                        phase = SequencePhase.Complete;

                        if (ReferenceEquals(Engine.Scene, this))
                        {
                            ReturnToOverworld();
                        }
                    }
                    break;
            }
        }

        private void UpdateScroll()
        {
            scrollY += scrollSpeed * Engine.DeltaTime;

            if (AllowInput)
            {
                if (Input.MenuDown.Check || Input.MenuConfirm.Check)
                {
                    scrollDelay = SCROLL_RESUME_DELAY;
                    scrollSpeed = Calc.Approach(scrollSpeed, INPUT_SCROLL_SPEED, SCROLL_ACCELERATION * Engine.DeltaTime);
                }
                else if (Input.MenuUp.Check)
                {
                    scrollDelay = SCROLL_RESUME_DELAY;
                    scrollSpeed = Calc.Approach(scrollSpeed, -INPUT_SCROLL_SPEED, SCROLL_ACCELERATION * Engine.DeltaTime);
                }
                else if (scrollDelay > 0f)
                {
                    scrollDelay = Math.Max(0f, scrollDelay - Engine.DeltaTime);
                    scrollSpeed = Calc.Approach(scrollSpeed, 0f, SCROLL_ACCELERATION * Engine.DeltaTime);
                }
                else
                {
                    scrollSpeed = Calc.Approach(scrollSpeed, AUTO_SCROLL_SPEED, SCROLL_ACCELERATION * Engine.DeltaTime);
                }
            }
            else
            {
                scrollDelay = 0f;
                scrollSpeed = Calc.Approach(scrollSpeed, AUTO_SCROLL_SPEED, SCROLL_ACCELERATION * Engine.DeltaTime);
            }

            if (scrollY < 0f || scrollY > height)
            {
                scrollSpeed = 0f;
            }

            scrollY = Calc.Clamp(scrollY, 0f, height);
            scrollbarAlpha = Calc.Approach(scrollbarAlpha, (AllowInput && scrollDelay > 0f) ? 1f : 0f, Engine.DeltaTime * 2f);
        }

        private void BeginExit()
        {
            if (exiting || finished)
            {
                return;
            }

            exiting = true;
            phase = SequencePhase.FadeOut;
            phaseTimer = 0f;
            scrollDelay = 0f;
            scrollSpeed = 0f;
            StopMusic();
        }

        #endregion

        #region ── Render ─────────────────────────────────────────────────

        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                null, null, null,
                Engine.ScreenMatrix);

            Draw.Rect(0f, 0f, 1920f, 1080f, Color.Black);
            RenderCredits(new Vector2(960f, 0f), 0.5f, drawScrollbar: true, drawHint: true);

            Draw.SpriteBatch.End();
        }

        /// <summary>
        /// Renders credit text at the given screen position (for use as an overlay in CS17_Credits).
        /// Caller is responsible for SpriteBatch state.
        /// </summary>
        public void Render(Vector2 position)
        {
            Render(position, 0.5f);
        }

        public void Render(Vector2 position, float justifyX)
        {
            RenderCredits(position, justifyX, drawScrollbar: false, drawHint: false);
        }

        private void RenderCredits(Vector2 position, float alignment, bool drawScrollbar, bool drawHint)
        {
            float alpha = fade * (1f - fadeOut);
            Vector2 cursor = position + new Vector2(0f, 1080f - scrollY).Floor();

            foreach (CreditNode credit in credits)
            {
                float creditHeight = credit.Height(1f);
                if (cursor.Y > -creditHeight && cursor.Y < 1080f)
                {
                    credit.Render(cursor, alignment, 1f, alpha);
                }

                cursor.Y += creditHeight + CREDIT_SPACING;
            }

            if (drawHint && AllowInput && scrollY > 100f && !exiting)
            {
                ActiveFont.Draw(
                    CreditText("MAGGY_CREDIT_BACK_HINT"),
                    new Vector2(960f, 1040f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.5f,
                    Color.Gray * alpha * 0.5f);
            }

            if (drawScrollbar && scrollbarAlpha > 0f && height > 0f)
            {
                const int padding = 64;
                int trackHeight = 1080 - padding * 2;
                float handleHeight = trackHeight * (trackHeight / height);
                float handleOffset = scrollY / height * (trackHeight - handleHeight);

                Draw.Rect(1844f, padding, 12f, trackHeight, Color.White * 0.2f * scrollbarAlpha * alpha);
                Draw.Rect(1844f, padding + handleOffset, 12f, handleHeight, Color.White * 0.5f * scrollbarAlpha * alpha);
            }
        }

        private static void DrawCreditText(Vector2 position, float alignment, float justifyY, float scale, string text, Color color, float alpha)
        {
            ActiveFont.DrawOutline(
                text,
                position.Floor(),
                new Vector2(alignment, justifyY),
                Vector2.One * scale,
                color * alpha,
                2f,
                BorderColor * alpha);
        }

        #endregion

        #region ── Audio helpers ───────────────────────────────────────────

        private void TryStartMusic()
        {
            try
            {
                music = Audio.Play(CREDIT_MUSIC_EVENT);
            }
            catch
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    "[MainMenuCredit] Credits music event not found – continuing without music");
            }
        }

        private void StopMusic()
        {
            if (music != null)
            {
                Audio.Stop(music, allowFadeOut: true);
                music = null;
            }
        }

        #endregion

        #region ── Navigation ─────────────────────────────────────────────

        private void ReturnToOverworld()
        {
            Engine.Scene = new OverworldLoader(
                Overworld.StartMode.Titlescreen,
                snow);
        }

        #endregion

        #region ── Cleanup ────────────────────────────────────────────────

        public override void End()
        {
            StopMusic();
            base.End();
        }

        #endregion
    }
}
