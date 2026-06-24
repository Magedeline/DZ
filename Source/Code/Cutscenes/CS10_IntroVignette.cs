#nullable enable

using FMOD.Studio;

namespace Celeste.Cutscenes
{
    class Cs10IntroVignetteAlt : Scene
    {
        public static class LoadingVignetteText
        {
            public const string Dialog = "MAGGYHELPER_CH10_RUINS_INTRO";
        }

        private readonly Session session;
        private readonly string? areaMusic;
        private readonly HudRenderer hud;
        private readonly Textbox textbox;
        private float fade = 0f;
        private TextMenu? menu;
        private float pauseFade = 0f;
        private bool exiting;
        private Coroutine? textCoroutine;
        private float textAlpha = 0f;
        private EventInstance? introMusic;
        private EventInstance? ringtone;
        private bool ringtoneActive = false;

        public bool CanPause => menu == null;

        public Cs10IntroVignetteAlt(Session session, TextMenu? menu, bool playRingtone = false, HiresSnow? _ = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            this.ringtoneActive = playRingtone;
            areaMusic = Audio.CurrentMusic;
            Audio.CurrentMusic = null;
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            textbox = new Textbox("MAGGYHELPER_CH10_RUINS_INTRO");
            textCoroutine = new Coroutine(TextSequence());
        }

        public Cs10IntroVignetteAlt(Session session1) : this(session1, null, false, null)
        {
            Add(new MaggyHiresSnow());
            Add(new FadeWipe(this, true));
        }

        private IEnumerator TextSequence()
        {
            yield return 1f;
            // Use FMOD EventInstance for intro music
            introMusic = Audio.Play("event:/music/pusheen/lvl10/intro");
            yield return 4f;
            // Stop ringtone using FMOD EventInstance
            if (ringtoneActive && ringtone != null)
            {
                ringtone.stop(STOP_MODE.ALLOWFADEOUT);
                ringtoneActive = false;
            }
            Audio.SetMusicParam(nameof(fade), 1f);
            yield return 1f;
            yield return Say(textbox);
            yield return 0.5f;
            Audio.SetMusicParam("pitch", 1f);
            yield return 1f;
            StartGame();
        }

        private static IEnumerator Say(Textbox textbox)
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
            fade = Calc.Approach(fade, 0f, Engine.DeltaTime);
        }

        public void OpenMenu()
        {
            PauseSfx();
            Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(CloseMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(StartGame));
            menu.OnCancel = menu.OnESC = menu.OnPause = CloseMenu;
        }

        private void CloseMenu()
        {
            ResumeSfx();
            Audio.Play("event:/ui/game/unpause");
            menu?.RemoveSelf();
            menu = null;
        }

        private void StartGame()
        {
            StopSfx();
            textCoroutine = null;
            Audio.CurrentMusic = areaMusic;
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
                Draw.SpriteBatch.End();
            }
        }

        private void PauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
            introMusic?.setPaused(true);
            ringtone?.setPaused(true);
        }

        private void ResumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            introMusic?.setPaused(false);
            ringtone?.setPaused(false);
        }

        private void StopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            introMusic?.stop(STOP_MODE.IMMEDIATE);
            ringtone?.stop(STOP_MODE.IMMEDIATE);
        }
    }
}

