#nullable enable

using DZ;
using FMOD.Studio;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 3 Intro Vignette - Postcard display before chapter starts
    /// Similar to vanilla Celeste's postcard intros
    /// </summary>
    [HotReloadable]
    public class Cs03IntroVignette : Scene
    {
        public static class LoadingVignetteText
        {
            public const string Dialog = "DZ_CH3_KIRBY_INTRO";
        }

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
        private float postcardScale = 1.2f;
        private bool postcardVisible = false;
        private EventInstance? introMusic;
        private EventInstance? introVignetteSfx;

        public bool CanPause => menu == null;

        public Cs03IntroVignette(Session session, TextMenu? menu = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();

            // Load postcard image
            Entity postcardHolder = new Entity();
            if (GFX.Game.Has("postcards/ch3_intro"))
            {
                postcard = new Image(GFX.Game["postcards/ch3_intro"]);
                postcard.CenterOrigin();
                postcard.Position = CelesteGame.TargetCenter;
                postcardHolder.Add(postcard);
            }
            else
            {
                IngesteLogger.Warn("Chapter 3 postcard image not found, using fallback");
            }
            Add(postcardHolder);

            textCoroutine = new Coroutine(introSequence());
        }

        public Cs03IntroVignette(Session session1) : this(session1, null)
        {
            Add(new MaggyHiresSnow());
            Add(new FadeWipe(this, true));
        }

        private IEnumerator introSequence()
        {
            yield return 0.5f;

            // Fade in from black
            while (fade > 0f)
            {
                fade -= Engine.DeltaTime * 0.5f;
                yield return null;
            }
            fade = 0f;

            yield return 0.5f;

            // Play intro vignette sound effect
            introVignetteSfx = Audio.Play("event:/pusheen/game/03_star/intro_vignette", Vector2.Zero);
            // Show postcard with zoom in effect
            postcardVisible = true;
            float timer = 0f;
            float duration = 2f;

            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float progress = timer / duration;
                
                postcardAlpha = Ease.CubeOut(progress);
                postcardScale = MathHelper.Lerp(1.2f, 1f, Ease.CubeOut(progress));
                
                yield return null;
            }

            postcardAlpha = 1f;
            postcardScale = 1f;

            yield return 2f;

            // Display intro dialog on the postcard
            var postcardTextbox = new Textbox("DZ_CH3_KIRBY_INTRO");
            yield return say(postcardTextbox);

            yield return 1f;

            // Zoom out and fade postcard
            timer = 0f;
            duration = 1.5f;

            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float progress = timer / duration;
                
                postcardAlpha = 1f - Ease.CubeIn(progress);
                postcardScale = MathHelper.Lerp(1f, 0.9f, Ease.CubeIn(progress));
                
                yield return null;
            }

            postcardVisible = false;
            postcardAlpha = 0f;

            yield return 0.5f;

            // Transition to game
            startGame();
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
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(startGame));
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

        private void startGame()
        {
            stopSfx();
            textCoroutine = null;
            session.Audio.Music.Event = areaMusic;
            if (menu != null)
            {
                menu.RemoveSelf();
                menu = null;
            }
            var fadeWipe = new FadeWipe(this, false, delegate
            {
                Engine.Scene = new LevelLoader(session);
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
                // Draw subtle glow behind postcard
                Vector2 glowPosition = postcard.Position;
                float glowSize = 50f * postcardScale;
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = (float)i / 8f * MathHelper.TwoPi;
                    Vector2 offset = new Vector2(
                        (float)Math.Cos(angle) * glowSize,
                        (float)Math.Sin(angle) * glowSize
                    );
                    
                    Draw.Rect(
                        glowPosition.X + offset.X - 10f,
                        glowPosition.Y + offset.Y - 10f,
                        20f,
                        20f,
                        Color.White * (postcardAlpha * 0.15f)
                    );
                }

                // Apply postcard effects
                Color postcardColor = Color.White * postcardAlpha;
                Vector2 scale = new Vector2(postcardScale, postcardScale);
                
                // The postcard itself is rendered by its component
                // We just add atmospheric effects here
            }

            // Subtle vignette effect
            if (textAlpha > 0f || postcardVisible)
            {
                float vignetteStrength = Math.Max(textAlpha * 0.3f, postcardAlpha * 0.2f);
                Draw.Rect(0f, 0f, 1920f, 1080f, Color.DarkBlue * vignetteStrength);
            }

            Draw.SpriteBatch.End();
        }

        private void pauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
            introMusic?.setPaused(true);
            introVignetteSfx?.setPaused(true);
        }

        private void resumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            introMusic?.setPaused(false);
            introVignetteSfx?.setPaused(false);
        }

        private void stopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            introMusic?.stop(STOP_MODE.IMMEDIATE);
            introVignetteSfx?.stop(STOP_MODE.IMMEDIATE);
        }
    }
}

