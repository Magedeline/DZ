#nullable enable

using FMOD.Studio;

namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Chapter 18 Intro Vignette - "1 Month Later" opening scene
    /// </summary>
    public class Cs18IntroVignette : Scene
    {
        public static class LoadingVignetteText
        {
            public const string Dialog = "CH18_INTRO";
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
        private float titleAlpha = 0f;
        private string titleText = "1 Month Later";

        public bool CanPause => menu == null;

        public Cs18IntroVignette(Session session, TextMenu? menu = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            textCoroutine = new Coroutine(introSequence());
        }

        public Cs18IntroVignette(Session session1) : this(session1, null)
        {
            Add(new HiresSnow());
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

            // Show "1 Month Later" title
            yield return showTitle();

            yield return 1f;

            // Play the intro dialog
            textAlpha = 1f;
            yield return Textbox.Say("CH18_INTRO");

            yield return 0.5f;

            // Fade out text
            while (textAlpha > 0f)
            {
                textAlpha -= Engine.DeltaTime;
                yield return null;
            }
            textAlpha = 0f;

            yield return 0.5f;

            // Transition to game
            startGame();
        }

        private IEnumerator showTitle()
        {
            // Fade in the title
            while (titleAlpha < 1f)
            {
                titleAlpha += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            titleAlpha = 1f;

            // Hold the title
            yield return 2f;

            // Fade out the title
            while (titleAlpha > 0f)
            {
                titleAlpha -= Engine.DeltaTime * 0.5f;
                yield return null;
            }
            titleAlpha = 0f;
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
                titleAlpha = Math.Min(titleAlpha, 1f - f);
            };

            exiting = true;
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

        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            if (fade > 0f)
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
            }

            // Render "1 Month Later" title
            if (titleAlpha > 0f)
            {
                // Use ActiveFont for rendering
                var scale = 1.5f;
                var titleSize = ActiveFont.Measure(titleText) * scale;
                var titlePosition = new Vector2((1920f - titleSize.X) / 2f, (1080f - titleSize.Y) / 2f);

                // Draw title with outline
                ActiveFont.DrawOutline(
                    titleText,
                    titlePosition,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * scale,
                    Color.White * titleAlpha,
                    2f,
                    Color.Black * titleAlpha
                );
            }

            if (textAlpha > 0f)
            {
                // Optional: Add a subtle background overlay
                Draw.Rect(0f, 0f, 1920f, 1080f, Color.Black * (textAlpha * 0.3f));
            }

            Draw.SpriteBatch.End();
        }

        private void pauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
        }

        private void resumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
        }

        private void stopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
        }
    }
}




