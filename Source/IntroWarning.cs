#nullable enable

namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// IntroWarning — One-time content disclaimer shown before the title screen
    /// on first launch. Displayed as a full-screen overlay with fade-in text,
    /// dismissed by pressing Confirm/Jump. Persists via module Settings so it
    /// only shows once (before any save slot is selected).
    /// </summary>
    [HotReloadable]
    public class IntroWarning : Scene
    {
        private const float FADE_IN_SPEED = 1.5f;
        private const float FADE_OUT_SPEED = 2f;
        private const float LINE_SPACING = 40f;
        private const float INITIAL_DELAY = 0.5f;

        private readonly HudRenderer hud;
        private readonly HiresSnow snow;

        private float bgFade;
        private float textFade;
        private float promptFade;
        private bool canDismiss;
        private bool exiting;
        private Coroutine? sequence;

        // Runtime guard to prevent re-triggering within the same game session
        private static bool _shownThisSession;

        private static readonly string[] WarningLines = new[]
        {
            "CONTENT WARNING",
            "",
            "This is a fan-made mod and is not affiliated with the",
            " original creators of Celeste or any other media referenced within.",
            "and It put alot of protection for the original creators and the mod creator.",
            "so if you have any question about this mod please",
            "ask the mod creator or the original creators of Celeste.",
            "and you agree on not sharing this mod without",
            "permission and not allowed to make false evidences.",
            "remind you that if you did, you will be held accountable for your",
            " actions and you will be responsible for any consequences that may arise from it.",
            "so please be respectful and responsible when",
            "sharing this mod and any content related to it.",
            "",
            "You have been warned.",
            "Desolo Zantas is a fan-made project.",
            "All rights reserved to their respective owners."};

        private const string PromptText = "Press CONFIRM to continue";

        public IntroWarning()
        {
            Add(hud = new HudRenderer());
            snow = new HiresSnow();
            Add(snow);
            RendererList.UpdateLists();
            sequence = new Coroutine(WarningSequence());
        }

        // ── Hook management ───────────────────────────────────────────────

        private static bool _hooked;

        public static void Load()
        {
            if (_hooked) return;
            _hooked = true;
            _shownThisSession = false;

            On.Celeste.Overworld.Begin += OnOverworldBegin;
            Logger.Log(LogLevel.Info, "MaggyHelper", "IntroWarning hooks loaded");
        }

        public static void Unload()
        {
            if (!_hooked) return;
            _hooked = false;

            On.Celeste.Overworld.Begin -= OnOverworldBegin;
            Logger.Log(LogLevel.Info, "MaggyHelper", "IntroWarning hooks unloaded");
        }

        /// <summary>
        /// Intercepts the first Overworld.Begin to show the content warning
        /// before the title screen appears.
        /// </summary>
        private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
        {
            orig(self);

            if (!ShouldShow())
                return;

            // Replace the Overworld scene with the warning screen
            _shownThisSession = true;
            Logger.Log(LogLevel.Info, "MaggyHelper",
                "IntroWarning: showing content warning before title screen");
            Engine.Scene = new IntroWarning();
        }

        /// <summary>
        /// Returns true if the warning should be shown.
        /// </summary>
        public static bool ShouldShow()
        {
            if (_shownThisSession)
                return false;

            var settings = Celeste.Mod.MaggyHelper.MaggyHelperModule.Settings;
            if (settings == null)
                return false;

            if (settings.SkipModIntro)
                return false;

            return !settings.HasSeenIntroWarning;
        }

        /// <summary>
        /// Marks the warning as seen in module settings and saves immediately.
        /// </summary>
        public static void MarkSeen()
        {
            var settings = Celeste.Mod.MaggyHelper.MaggyHelperModule.Settings;
            if (settings != null)
            {
                settings.HasSeenIntroWarning = true;
                Celeste.Mod.MaggyHelper.MaggyHelperModule.Instance.SaveSettings();
            }
        }

        private IEnumerator WarningSequence()
        {
            // Brief pause before fading in
            yield return INITIAL_DELAY;

            // Fade in background
            while (bgFade < 1f)
            {
                bgFade = Math.Min(bgFade + Engine.DeltaTime * FADE_IN_SPEED, 1f);
                yield return null;
            }

            // Fade in text
            while (textFade < 1f)
            {
                textFade = Math.Min(textFade + Engine.DeltaTime * FADE_IN_SPEED, 1f);
                yield return null;
            }

            yield return 0.5f;

            // Show prompt and allow dismiss
            canDismiss = true;
            while (promptFade < 1f)
            {
                promptFade = Math.Min(promptFade + Engine.DeltaTime * FADE_IN_SPEED, 1f);
                yield return null;
            }

            // Wait for player to press confirm
            while (!Input.MenuConfirm.Pressed && !Input.Jump.Pressed)
            {
                yield return null;
            }

            Audio.Play("event:/ui/main/button_select");
            canDismiss = false;

            // Persist the flag
            MarkSeen();

            // Fade out everything
            while (textFade > 0f || bgFade > 0f)
            {
                textFade = Math.Max(textFade - Engine.DeltaTime * FADE_OUT_SPEED, 0f);
                promptFade = Math.Max(promptFade - Engine.DeltaTime * FADE_OUT_SPEED, 0f);
                bgFade = Math.Max(bgFade - Engine.DeltaTime * FADE_OUT_SPEED, 0f);
                yield return null;
            }

            yield return 0.2f;

            // Proceed to the normal Overworld / title screen
            exiting = true;
            Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen, snow);
        }

        public override void Update()
        {
            base.Update();

            if (!exiting)
            {
                sequence?.Update();

                // Allow skip with ESC if SkipModIntro is enabled
                if (canDismiss && (Input.MenuConfirm.Pressed || Input.Jump.Pressed))
                {
                    // Handled in coroutine
                }
            }
        }

        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            // Dark background
            Draw.Rect(0f, 0f, 1920f, 1080f, Color.Black * bgFade);

            if (textFade <= 0f)
            {
                Draw.SpriteBatch.End();
                return;
            }

            // Calculate vertical start to center the text block
            float totalHeight = WarningLines.Length * LINE_SPACING;
            float startY = (1080f - totalHeight) / 2f - 30f;

            for (int i = 0; i < WarningLines.Length; i++)
            {
                string line = WarningLines[i];
                if (string.IsNullOrEmpty(line))
                    continue;

                // First line (title) is larger and colored
                bool isTitle = i == 0;
                float scale = isTitle ? 1.5f : 0.8f;
                Color color = isTitle
                    ? Color.Gold * textFade
                    : Color.White * textFade;

                float y = startY + i * LINE_SPACING;
                ActiveFont.Draw(
                    line,
                    new Vector2(960f, y),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * scale,
                    color);
            }

            // Prompt at bottom
            if (promptFade > 0f)
            {
                // Pulsing alpha for the prompt
                float pulse = 0.6f + (float)Math.Sin(Engine.Scene.TimeActive * 3f) * 0.4f;
                ActiveFont.Draw(
                    PromptText,
                    new Vector2(960f, 900f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.7f,
                    Color.LightGray * promptFade * pulse);
            }

            Draw.SpriteBatch.End();
        }
    }
}
