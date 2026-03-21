#nullable enable

namespace MaggyHelper.UI
{
    /// <summary>
    /// Custom main-menu credits scene for the Desolo Zantas mod.
    /// Displays scrolling credit entries (directors, artists, composers, etc.)
    /// over a snow backdrop with a slow fade-in/out, driven by a coroutine.
    ///
    /// Accessed from the Overworld via a hooked "Credits" menu option or
    /// instantiated directly: <c>Engine.Scene = new MainMenuCredit();</c>
    ///
    /// Press MenuCancel or MenuConfirm (after the final entry) to return
    /// to the Overworld title screen.
    /// </summary>
    [HotReloadable]
    public class MainMenuCredit : Scene
    {
        #region ── Constants ──────────────────────────────────────────────

        // Timing
        private const float FADE_IN_DURATION   = 1.5f;
        private const float FADE_OUT_DURATION   = 1.0f;
        private const float SCROLL_SPEED        = 40f;   // pixels / second
        private const float FAST_SCROLL_MULT    = 4f;    // hold Confirm to speed up
        private const float HOLD_AFTER_LAST     = 3.0f;  // seconds to linger at the end

        // Layout (1920 × 1080 virtual coords)
        private const float START_Y             = 1200f;  // credits start below screen
        private const float LINE_SPACING        = 60f;
        private const float HEADER_SPACING      = 90f;

        // Audio
        private const string CREDIT_MUSIC_EVENT = "event:/desolozantas/music/menu/credits";
        private const string BACK_SFX           = "event:/ui/main/button_back";

        #endregion

        #region ── Nested types ───────────────────────────────────────────

        private enum CreditLineKind { Header, Name, Spacer }

        private readonly record struct CreditLine(
            CreditLineKind Kind,
            string         Text,
            float          YOffset);

        #endregion

        #region ── Fields ─────────────────────────────────────────────────

        private readonly List<CreditLine> lines = new();
        private float totalHeight;

        private float scrollY;          // current Y offset (increases over time)
        private float fade;             // 0 → 1 during fade-in
        private float fadeOut;          // 0 → 1 during fade-out
#pragma warning disable CS0414
        private bool  finished;
#pragma warning restore CS0414
        private bool  exiting;

        private HiresSnow snow = null!;
        private HudRenderer hud = null!;
        private Coroutine? sequenceCoroutine;
        private FMOD.Studio.EventInstance? music;

        #endregion

        #region ── Constructor ────────────────────────────────────────────

        public MainMenuCredit()
        {
            BuildCreditLines();

            snow = new HiresSnow();
            hud  = new HudRenderer();

            Add(snow);
            Add(hud);
            RendererList.UpdateLists();

            sequenceCoroutine = new Coroutine(Sequence());
        }

        #endregion

        #region ── Credit Data ────────────────────────────────────────────

        /// <summary>
        /// Builds the list of credit lines.
        /// Each entry is either a header (role), a name, or a spacer.
        /// Dialog keys are resolved here so all text is localisable.
        /// </summary>
        private void BuildCreditLines()
        {
            float y = 0f;

            void AddHeader(string dialogKey)
            {
                y += HEADER_SPACING;
                lines.Add(new CreditLine(CreditLineKind.Header, Dialog.Clean(dialogKey), y));
            }

            void AddName(string dialogKey)
            {
                y += LINE_SPACING;
                lines.Add(new CreditLine(CreditLineKind.Name, Dialog.Clean(dialogKey), y));
            }

            void AddSpacer()
            {
                y += LINE_SPACING * 0.5f;
                lines.Add(new CreditLine(CreditLineKind.Spacer, string.Empty, y));
            }

            // ── Title card ──
            AddHeader("MAGGY_CREDIT_TITLE");
            AddSpacer();

            // ── Direction ──
            AddHeader("MAGGY_CREDIT_DIRECTION");
            AddName  ("MAGGY_CREDIT_DIRECTION_1");

            // ── Programming ──
            AddHeader("MAGGY_CREDIT_PROGRAMMING");
            AddName  ("MAGGY_CREDIT_PROGRAMMING_1");

            // ── Art & Sprites ──
            AddHeader("MAGGY_CREDIT_ART");
            AddName  ("MAGGY_CREDIT_ART_1");

            // ── Music & Sound ──
            AddHeader("MAGGY_CREDIT_MUSIC");
            AddName  ("MAGGY_CREDIT_MUSIC_1");

            // ── Level Design ──
            AddHeader("MAGGY_CREDIT_LEVEL_DESIGN");
            AddName  ("MAGGY_CREDIT_LEVEL_DESIGN_1");

            // ── Writing ──
            AddHeader("MAGGY_CREDIT_WRITING");
            AddName  ("MAGGY_CREDIT_WRITING_1");

            // ── Testing ──
            AddHeader("MAGGY_CREDIT_TESTING");
            AddName  ("MAGGY_CREDIT_TESTING_1");

            // ── Special Thanks ──
            AddSpacer();
            AddHeader("MAGGY_CREDIT_SPECIAL_THANKS");
            AddName  ("MAGGY_CREDIT_SPECIAL_THANKS_1");
            AddName  ("MAGGY_CREDIT_SPECIAL_THANKS_2");
            AddName  ("MAGGY_CREDIT_SPECIAL_THANKS_3");

            // ── Powered-by line ──
            AddSpacer();
            AddHeader("MAGGY_CREDIT_POWERED_BY");
            AddName  ("MAGGY_CREDIT_POWERED_BY_1");
            AddName  ("MAGGY_CREDIT_POWERED_BY_2");

            totalHeight = y + 600f; // extra padding so text scrolls off-screen
        }

        #endregion

        #region ── Main Coroutine ─────────────────────────────────────────

        private IEnumerator Sequence()
        {
            // ── Phase 1: fade in ──
            TryStartMusic();

            float timer = 0f;
            while (timer < FADE_IN_DURATION)
            {
                timer += Engine.DeltaTime;
                fade = Calc.Clamp(timer / FADE_IN_DURATION, 0f, 1f);
                yield return null;
            }
            fade = 1f;

            // ── Phase 2: scroll ──
            while (scrollY < totalHeight)
            {
                float mult = Input.MenuConfirm.Check ? FAST_SCROLL_MULT : 1f;
                scrollY += SCROLL_SPEED * mult * Engine.DeltaTime;

                // Allow early exit
                if (Input.MenuCancel.Pressed)
                {
                    Audio.Play(BACK_SFX);
                    break;
                }

                yield return null;
            }

            // Hold at bottom briefly
            if (!exiting)
            {
                float hold = 0f;
                while (hold < HOLD_AFTER_LAST)
                {
                    hold += Engine.DeltaTime;
                    if (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed)
                    {
                        Audio.Play(BACK_SFX);
                        break;
                    }
                    yield return null;
                }
            }

            // ── Phase 3: fade out & return ──
            exiting = true;
            StopMusic();

            timer = 0f;
            while (timer < FADE_OUT_DURATION)
            {
                timer += Engine.DeltaTime;
                fadeOut = Calc.Clamp(timer / FADE_OUT_DURATION, 0f, 1f);
                yield return null;
            }
            fadeOut = 1f;

            finished = true;
            ReturnToOverworld();
        }

        #endregion

        #region ── Update ─────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();
            sequenceCoroutine?.Update();
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

            // Background
            Draw.Rect(0, 0, 1920, 1080, Color.Black);

            float alpha = fade * (1f - fadeOut);
            float cx    = 1920f / 2f;

            foreach (var line in lines)
            {
                if (line.Kind == CreditLineKind.Spacer)
                    continue;

                float drawY = START_Y + line.YOffset - scrollY;

                // Cull off-screen
                if (drawY < -80f || drawY > 1160f)
                    continue;

                // Slight fade near edges
                float edgeFade = 1f;
                if (drawY < 80f)
                    edgeFade = Calc.Clamp(drawY / 80f, 0f, 1f);
                else if (drawY > 1000f)
                    edgeFade = Calc.Clamp((1080f - drawY) / 80f, 0f, 1f);

                float lineAlpha = alpha * edgeFade;

                switch (line.Kind)
                {
                    case CreditLineKind.Header:
                        ActiveFont.DrawOutline(
                            line.Text,
                            new Vector2(cx, drawY),
                            new Vector2(0.5f, 0.5f),
                            Vector2.One * 1.2f,
                            Color.Gold * lineAlpha,
                            2f,
                            Color.Black * lineAlpha * 0.6f);
                        break;

                    case CreditLineKind.Name:
                        ActiveFont.DrawOutline(
                            line.Text,
                            new Vector2(cx, drawY),
                            new Vector2(0.5f, 0.5f),
                            Vector2.One * 0.85f,
                            Color.White * lineAlpha,
                            2f,
                            Color.Black * lineAlpha * 0.5f);
                        break;
                }
            }

            // "Press BACK to return" hint (visible once scroll starts)
            if (scrollY > 100f && !exiting)
            {
                string hint = Dialog.Clean("MAGGY_CREDIT_BACK_HINT");
                ActiveFont.Draw(
                    hint,
                    new Vector2(cx, 1040f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.5f,
                    Color.Gray * alpha * 0.5f);
            }

            Draw.SpriteBatch.End();
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
                // Event may not exist yet – carry on silently
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
