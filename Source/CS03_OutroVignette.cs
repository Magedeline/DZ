#nullable enable

using FMOD.Studio;

namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Chapter 3 Outro Vignette - Postcard display after chapter completion
    /// </summary>
    [HotReloadable]
    public class Cs03OutroVignette : Scene
    {
        private readonly Session session;
        private readonly string? areaMusic;
        private readonly HudRenderer hud;
        private float fade = 1f;
        private TextMenu? menu;
        private float pauseFade = 0f;
        private bool exiting;
        private Coroutine? textCoroutine;
        private float textAlpha = 0f;
        private Image? postcard;
        private float postcardAlpha = 0f;
        private float postcardScale = 0.9f;
        private bool postcardVisible = false;
        private EventInstance? outroMusic;

        public bool CanPause => menu == null;

        public Cs03OutroVignette(Session session, TextMenu? menu = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();

            // Load outro postcard image
            Entity postcardHolder = new Entity();
            if (GFX.Game.Has("postcards/ch3_outro"))
            {
                postcard = new Image(GFX.Game["postcards/ch3_outro"]);
                postcard.CenterOrigin();
                postcard.Position = Celeste.Celeste.TargetCenter;
                postcardHolder.Add(postcard);
            }
            else if (GFX.Game.Has("postcards/ch3_intro"))
            {
                // Fallback to intro postcard
                postcard = new Image(GFX.Game["postcards/ch3_intro"]);
                postcard.CenterOrigin();
                postcard.Position = Celeste.Celeste.TargetCenter;
                postcardHolder.Add(postcard);
                IngesteLogger.Warn("Chapter 3 outro postcard not found, using intro postcard");
            }
            else
            {
                IngesteLogger.Warn("Chapter 3 postcard images not found");
            }
            Add(postcardHolder);

            textCoroutine = new Coroutine(outroSequence());
        }

        public Cs03OutroVignette(Session session1) : this(session1, null)
        {
            Add(new HiresSnow());
            Add(new FadeWipe(this, true));
        }

        private IEnumerator outroSequence()
        {
            yield return 0.5f;

            // Fade in from black
            while (fade > 0f)
            {
                fade -= Engine.DeltaTime * 0.5f;
                yield return null;
            }
            fade = 0f;

            yield return 1f;

            // Play outro music
            outroMusic = Audio.Play("event:/desolozantas/music/lvl3/outro");
            yield return 1f;

            // Show postcard with zoom in effect
            postcardVisible = true;
            float timer = 0f;
            float duration = 2f;

            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float progress = timer / duration;
                
                postcardAlpha = Ease.SineOut(progress);
                postcardScale = MathHelper.Lerp(0.9f, 1f, Ease.SineOut(progress));
                
                yield return null;
            }

            postcardAlpha = 1f;
            postcardScale = 1f;

            yield return 1.5f;

            // Display outro dialog
            var outroTextbox = new Textbox("CH3_POSTCARD_OUTRO");
            yield return say(outroTextbox);

            yield return 2f;

            // Gentle fade out
            timer = 0f;
            duration = 2f;

            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float progress = timer / duration;
                
                postcardAlpha = 1f - Ease.SineIn(progress);
                textAlpha = progress;
                
                yield return null;
            }

            postcardVisible = false;
            postcardAlpha = 0f;

            yield return 0.5f;

            // Complete the chapter
            completeChapter();
        }

        private IEnumerator say(Textbox textbox)
        {
            Engine.Scene.Add(textbox);
            while (textbox.Opened)
            {
                yield return true;
            }
        }

        public override void Update()
        {
            if (menu == null)
            {
                base.Update();
                if (!exiting)
                {
                    textCoroutine?.Update();
                    if (Input.Pause.Pressed || Input.ESC.Pressed)
                    {
                        OpenMenu();
                    }
                }
            }
            else if (!exiting)
            {
                menu.Update();
            }
            pauseFade = Calc.Approach(pauseFade, menu != null ? 1 : 0, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, menu != null ? 0.6f : 0f, Engine.DeltaTime * 3f);
        }

        public void OpenMenu()
        {
            pauseSfx();
            Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(closeMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(completeChapter));
            menu.OnCancel = menu.OnESC = menu.OnPause = closeMenu;
        }

        private void closeMenu()
        {
            resumeSfx();
            Audio.Play("event:/ui/game/unpause");
            if (menu != null)
            {
                menu.RemoveSelf();
            }
            menu = null;
        }

        private void completeChapter()
        {
            stopSfx();
            textCoroutine = null;
            session.Audio.Music.Event = areaMusic;
            if (menu != null)
            {
                menu.RemoveSelf();
                menu = null;
            }

            // Mark chapter as complete
            session.SetFlag("ch3_outro_complete");

            var fadeWipe = new FadeWipe(this, false, delegate
            {
                // Complete the area
                var level = new Level();
                level.Session = session;
                level.CompleteArea(spotlightWipe: false, skipScreenWipe: false, skipCompleteScreen: false);
            });
            fadeWipe.OnUpdate = delegate (float f)
            {
                textAlpha = Math.Min(textAlpha, 1f - f);
            };
            exiting = true;
        }

        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            // Draw background fade
            if (fade > 0f)
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
            }

            // Draw postcard with effects
            if (postcardVisible && postcard != null && postcardAlpha > 0f)
            {
                // Draw warm glow behind postcard (sunset/completion theme)
                Vector2 glowPosition = postcard.Position;
                float glowSize = 60f * postcardScale;
                
                for (int i = 0; i < 12; i++)
                {
                    float angle = (float)i / 12f * MathHelper.TwoPi;
                    Vector2 offset = new Vector2(
                        (float)Math.Cos(angle) * glowSize,
                        (float)Math.Sin(angle) * glowSize
                    );
                    
                    // Warm orange/gold glow
                    Color glowColor = Color.Lerp(Color.Orange, Color.Gold, (float)i / 12f);
                    Draw.Rect(
                        glowPosition.X + offset.X - 12f,
                        glowPosition.Y + offset.Y - 12f,
                        24f,
                        24f,
                        glowColor * (postcardAlpha * 0.12f)
                    );
                }
            }

            // Warm vignette effect for completion
            if (postcardVisible || textAlpha > 0f)
            {
                float vignetteStrength = Math.Max(postcardAlpha * 0.15f, textAlpha * 0.3f);
                Color vignetteColor = Color.Lerp(Color.DarkOrange, Color.Black, 0.7f);
                Draw.Rect(0f, 0f, 1920f, 1080f, vignetteColor * vignetteStrength);
            }

            // Final fade to black
            if (textAlpha > 0.5f)
            {
                float blackFade = (textAlpha - 0.5f) * 2f;
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * blackFade);
            }

            Draw.SpriteBatch.End();
        }

        private void pauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
            outroMusic?.setPaused(true);
        }

        private void resumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            outroMusic?.setPaused(false);
        }

        private void stopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            outroMusic?.stop(STOP_MODE.IMMEDIATE);
        }
    }
}




