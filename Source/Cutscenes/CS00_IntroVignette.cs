#nullable enable

using DZ;
using FMOD.Studio;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 0 Intro Vignette - Full screen cutscene after vessel creation
    /// </summary>
    [HotReloadable]
    public class Cs00IntroVignette : Scene
    {
        public static class LoadingVignetteText
        {
            public const string Dialog = "DZ_CH0_WELCOME_BACK";
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
        private EventInstance? introMusic;

        public bool CanPause => menu == null;

        public Cs00IntroVignette(Session session, TextMenu? menu = null)
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

        public Cs00IntroVignette(Session session1) : this(session1, null)
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

            yield return 1f;

            // Play intro music
            introMusic = Audio.Play("event:/Mods/pusheen/music/lvl0/intro", Vector2.Zero);
            yield return 2f;

            // Display intro dialog
            var introTextbox = new Textbox("DZ_CH0_WELCOME_BACK");
            yield return say(introTextbox);

            yield return 1.5f;

            // Fade to white for transition
            textAlpha = 1f;
            while (textAlpha > 0f)
            {
                textAlpha -= Engine.DeltaTime * 2f;
                yield return null;
            }
            textAlpha = 0f;

            yield return 0.5f;

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
                        Input.Pause.ConsumePress();
                        Input.ESC.ConsumePress();
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
            if (fade > 0f || textAlpha > 0f)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                if (fade > 0f)
                {
                    Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
                }

                if (textAlpha > 0f)
                {
                    // Subtle background effect
                    Draw.Rect(0f, 0f, 1920f, 1080f, Color.DarkSlateBlue * (textAlpha * 0.2f));
                }

                Draw.SpriteBatch.End();
            }
        }

        private void pauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
            introMusic?.setPaused(true);
        }

        private void resumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            introMusic?.setPaused(false);
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
        }
    }
}

