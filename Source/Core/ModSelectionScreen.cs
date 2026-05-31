#nullable enable

using Celeste.Cutscenes;
using FMOD.Studio;

namespace Celeste.UI
{
    /// <summary>
    /// Custom title screen overlay that appears when a new player launches the game
    /// with MaggyHelper installed. Offers two choices:
    ///   1. "Desolo Zantas" – play the mod's intro (vessel creation cutscene)
    ///   2. "Celeste"       – skip straight to normal Everest / vanilla Celeste
    ///
    /// The screen only appears once per save file (tracked via MaggyHelperSaveData.HasSeenModIntro).
    /// After the player makes a choice, the flag is set and this screen is never shown again.
    /// </summary>
    [HotReloadable]
    public class ModSelectionScreen : Scene
    {
        #region Constants
        // ── Timing ──
        private const float FADE_IN_DURATION  = 1.5f;
        private const float TITLE_DELAY       = 0.6f;
        private const float MENU_APPEAR_DELAY = 0.8f;
        private const float FADE_OUT_DURATION = 0.8f;

        // ── Visual layout ──
        private const float TITLE_Y           = 160f;
        private const float SUBTITLE_Y        = 230f;
        private const float NOTICE_Y          = 300f;
        private const float NOTICE2_Y         = 330f;
        private const float MENU_Y            = 440f;
        private const float CHOICE_SPACING    = 80f;

        // ── Audio ──
        private const string CHOICE_MOVE_SFX   = "event:/ui/main/rollover_down";
        private const string CHOICE_SELECT_SFX = "event:/ui/main/button_select";
        private const string TITLE_MUSIC_EVENT = "event:/pusheen/music/lvl0/intro";
        #endregion

        #region Static Hook State
        private static bool _hooked;
        private static bool _shownThisSession;
        #endregion

        #region Fields
        private enum SelectionState { FadingIn, Selecting, FadingOut }

        private SelectionState state = SelectionState.FadingIn;
        private float fade = 0f;       // 0 = black, 1 = fully visible
        private float titleAlpha = 0f;
        private float menuAlpha = 0f;
        private float fadeOutAlpha = 0f;

        private int selectedIndex = 0;  // 0 = Desolo Zantas, 1 = Celeste
        private bool chose = false;
        private bool choiceIsDesoloZantas = true;

        private HudRenderer hud;
        private Coroutine? sequenceCoroutine;
        private EventInstance? titleMusic;
        #endregion

        #region Constructor
        public ModSelectionScreen()
        {
            IngesteLogger.Info("[ModSelectionScreen] Initializing mod selection screen");

            Add(hud = new HudRenderer());
            RendererList.UpdateLists();

            sequenceCoroutine = new Coroutine(screenSequence());
            Add(new HiresSnow());
        }
        #endregion

        #region Static Hook Management
        public static void Load()
        {
            if (_hooked) return;
            _hooked = true;
            _shownThisSession = false;

            On.Celeste.Overworld.Begin += OnOverworldBegin;
            Logger.Log(LogLevel.Info, "MaggyHelper", "ModSelectionScreen hooks loaded");
        }

        public static void Unload()
        {
            if (!_hooked) return;
            _hooked = false;

            On.Celeste.Overworld.Begin -= OnOverworldBegin;
            Logger.Log(LogLevel.Info, "MaggyHelper", "ModSelectionScreen hooks unloaded");
        }

        private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
        {
            orig(self);

            if (!ShouldShow())
                return;

            _shownThisSession = true;
            Logger.Log(LogLevel.Info, "MaggyHelper", "ModSelectionScreen: showing mod selection overlay");
            Engine.Scene = new ModSelectionScreen();
        }

        /// <summary>
        /// Returns true if the mod selection screen should be shown.
        /// Skipped if DeveloperBypass is enabled for testing cycles.
        /// </summary>
        public static bool ShouldShow()
        {
            if (_shownThisSession)
                return false;

            var settings = global::Celeste.Mod.MaggyHelper.MaggyHelperModule.Settings;
            var saveData = global::Celeste.Mod.MaggyHelper.MaggyHelperModule.SaveData;

            if (settings == null || saveData == null)
                return false;

            // Developer bypass: skip all introductory sequences
            if (settings.DeveloperBypass || settings.DebugMode)
                return false;

            if (settings.SkipModIntro)
                return false;

            return !saveData.HasSeenModIntro;
        }
        #endregion

        #region Main Coroutine
        private IEnumerator screenSequence()
        {
            // ── Phase 1: Fade in ──
            state = SelectionState.FadingIn;

            // Optional: start ambient music
            try
            {
                titleMusic = Audio.Play(TITLE_MUSIC_EVENT);
            }
            catch
            {
                // Music event may not exist yet – that's fine
                IngesteLogger.Warn("[ModSelectionScreen] Title music event not found, continuing without music");
            }

            // Fade from black
            float timer = 0f;
            while (timer < FADE_IN_DURATION)
            {
                timer += Engine.DeltaTime;
                fade = Calc.Clamp(timer / FADE_IN_DURATION, 0f, 1f);
                yield return null;
            }
            fade = 1f;

            // Show title text
            yield return TITLE_DELAY;
            timer = 0f;
            while (timer < 0.6f)
            {
                timer += Engine.DeltaTime;
                titleAlpha = Calc.Clamp(timer / 0.6f, 0f, 1f);
                yield return null;
            }
            titleAlpha = 1f;

            // Reveal menu choices
            yield return MENU_APPEAR_DELAY;
            timer = 0f;
            while (timer < 0.5f)
            {
                timer += Engine.DeltaTime;
                menuAlpha = Calc.Clamp(timer / 0.5f, 0f, 1f);
                yield return null;
            }
            menuAlpha = 1f;

            // ── Phase 2: Wait for player selection ──
            state = SelectionState.Selecting;

            while (!chose)
            {
                yield return null;
            }

            // ── Phase 3: Fade out & transition ──
            state = SelectionState.FadingOut;

            // Stop music
            if (titleMusic != null)
            {
                Audio.Stop(titleMusic, allowFadeOut: true);
                titleMusic = null;
            }

            timer = 0f;
            while (timer < FADE_OUT_DURATION)
            {
                timer += Engine.DeltaTime;
                fadeOutAlpha = Calc.Clamp(timer / FADE_OUT_DURATION, 0f, 1f);
                yield return null;
            }
            fadeOutAlpha = 1f;

            // Mark flag so this screen never shows again
            markIntroSeen();

            // Transition
            if (choiceIsDesoloZantas)
            {
                IngesteLogger.Info("[ModSelectionScreen] Player chose Desolo Zantas – launching vessel creation intro");
                launchDesoloZantasIntro();
            }
            else
            {
                IngesteLogger.Info("[ModSelectionScreen] Player chose Celeste – proceeding to Everest");
                launchCeleste();
            }
        }
        #endregion

        #region Update
        public override void Update()
        {
            base.Update();

            // Tick the coroutine manually
            sequenceCoroutine?.Update();

            // Handle input only in Selecting state
            if (state == SelectionState.Selecting && !chose)
            {
                // Navigate up / down
                if (Input.MenuUp.Pressed || Input.Aim.Value.Y < -0.4f && Input.Aim.Value.Y > -0.5f)
                {
                    if (selectedIndex > 0)
                    {
                        selectedIndex--;
                        Audio.Play(CHOICE_MOVE_SFX);
                    }
                }
                else if (Input.MenuDown.Pressed || Input.Aim.Value.Y > 0.4f && Input.Aim.Value.Y < 0.5f)
                {
                    if (selectedIndex < 1)
                    {
                        selectedIndex++;
                        Audio.Play(CHOICE_MOVE_SFX);
                    }
                }

                // Confirm
                if (Input.MenuConfirm.Pressed)
                {
                    chose = true;
                    choiceIsDesoloZantas = (selectedIndex == 0);
                    Audio.Play(CHOICE_SELECT_SFX);
                }
            }
        }
        #endregion

        #region Render
        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                null, null, null,
                Engine.ScreenMatrix);

            // Black background
            Draw.Rect(0, 0, Engine.ViewWidth, Engine.ViewHeight, Color.Black);

            float alpha = fade * (1f - fadeOutAlpha);

            // ── Title ──
            if (titleAlpha > 0f)
            {
                string title = Dialog.Clean("MAGGY_MOD_SELECT_TITLE");
                string subtitle = Dialog.Clean("MAGGY_MOD_SELECT_SUBTITLE");

                float titleScale = 2f;
                float subtitleScale = 1f;
                Color titleColor = Color.White * alpha * titleAlpha;
                Color subtitleColor = Color.LightGray * alpha * titleAlpha * 0.8f;

                ActiveFont.Draw(
                    title,
                    new Vector2(Engine.ViewWidth / 2f, TITLE_Y),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * titleScale,
                    titleColor);

                ActiveFont.Draw(
                    subtitle,
                    new Vector2(Engine.ViewWidth / 2f, SUBTITLE_Y),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * subtitleScale,
                    subtitleColor);

                // One-time choice notice
                string notice  = Dialog.Clean("MAGGY_MOD_SELECT_NOTICE");
                string notice2 = Dialog.Clean("MAGGY_MOD_SELECT_NOTICE2");
                Color noticeColor = Color.OrangeRed * alpha * titleAlpha * 0.9f;
                Color notice2Color = Color.Gray * alpha * titleAlpha * 0.6f;

                ActiveFont.Draw(
                    notice,
                    new Vector2(Engine.ViewWidth / 2f, NOTICE_Y),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.55f,
                    noticeColor);

                ActiveFont.Draw(
                    notice2,
                    new Vector2(Engine.ViewWidth / 2f, NOTICE2_Y),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.45f,
                    notice2Color);
            }

            // ── Menu choices ──
            if (menuAlpha > 0f)
            {
                string choice0 = Dialog.Clean("MAGGY_MOD_SELECT_DESOLOZANTAS");
                string choice1 = Dialog.Clean("MAGGY_MOD_SELECT_CELESTE");
                string desc0   = Dialog.Clean("MAGGY_MOD_SELECT_DESOLOZANTAS_DESC");
                string desc1   = Dialog.Clean("MAGGY_MOD_SELECT_CELESTE_DESC");

                drawChoice(0, choice0, desc0, alpha * menuAlpha);
                drawChoice(1, choice1, desc1, alpha * menuAlpha);
            }

            Draw.SpriteBatch.End();
        }

        private void drawChoice(int index, string label, string description, float alpha)
        {
            bool highlighted = (index == selectedIndex) && (state == SelectionState.Selecting);
            float y = MENU_Y + index * CHOICE_SPACING;
            float x = Engine.ViewWidth / 2f;

            Color labelColor = highlighted
                ? Color.Gold * alpha
                : Color.White * alpha * 0.6f;

            Color descColor = highlighted
                ? Color.LightGoldenrodYellow * alpha * 0.7f
                : Color.Gray * alpha * 0.4f;

            float scale = highlighted ? 1.4f : 1.1f;

            // Selector arrow
            if (highlighted)
            {
                float arrowX = x - ActiveFont.Measure(label).X * scale / 2f - 40f;
                ActiveFont.Draw("»", new Vector2(arrowX, y), new Vector2(0.5f, 0.5f),
                    Vector2.One * scale, Color.Gold * alpha);
            }

            // Label
            ActiveFont.Draw(label, new Vector2(x, y), new Vector2(0.5f, 0.5f),
                Vector2.One * scale, labelColor);

            // Description (smaller, below)
            ActiveFont.Draw(description, new Vector2(x, y + 30f), new Vector2(0.5f, 0.5f),
                Vector2.One * 0.6f, descColor);
        }
        #endregion

        #region Transitions
        private void markIntroSeen()
        {
            // Flag so this screen won't appear again for this save
            var saveData = MaggyHelperModule.SaveData;
            if (saveData != null)
            {
                saveData.HasSeenModIntro = true;
            }

            // Also persist the choice so the module knows at future launches
            var settings = MaggyHelperModule.Settings;
            if (settings != null)
            {
                settings.SkipModIntro = !choiceIsDesoloZantas;
            }
        }

        private void launchDesoloZantasIntro()
        {
            // Build a session for the prologue / vessel creation intro map
            // Prologue is the first area in the mod
            try
            {
                // Find the area for the mod's prologue
                var area = AreaData.Get(AreaModeExtender.BuildASideSID("00_Prologue"));
                if (area != null)
                {
                    var session = new Session(area.ToKey());
                    Engine.Scene = new VesselCreationVignette(session);
                }
                else
                {
                    IngesteLogger.Warn("[ModSelectionScreen] Could not find prologue area – falling back to overworld");
                    launchCeleste();
                }
            }
            catch (Exception ex)
            {
                IngesteLogger.Error($"[ModSelectionScreen] Error launching intro: {ex.Message}");
                launchCeleste();
            }
        }

        private void launchCeleste()
        {
            // Go to normal Everest main menu / overworld
            Engine.Scene = new Overworld(new OverworldLoader(
                Overworld.StartMode.Titlescreen));
        }
        #endregion

        #region Cleanup
        public override void End()
        {
            if (titleMusic != null)
            {
                Audio.Stop(titleMusic, allowFadeOut: false);
                titleMusic = null;
            }
            base.End();
        }
        #endregion
    }
}
