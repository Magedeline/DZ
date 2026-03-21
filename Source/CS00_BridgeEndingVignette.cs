#nullable enable

using FMOD.Studio;

namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Chapter 0 Bridge/Ending Vignette - Plays after bridge collapse
    /// </summary>
    [HotReloadable]
    public class Cs00BridgeEndingVignette : Scene
    {
        public static class LoadingVignetteText
        {
            public const string Dialog = "CH0_END_YOUR_CHOICE_MATTERS";
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
        private EventInstance? titlePingMusic;

        public bool CanPause => menu == null;

        public Cs00BridgeEndingVignette(Session session, TextMenu? menu = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            textCoroutine = new Coroutine(endingSequence());
        }

        public Cs00BridgeEndingVignette(Session session1) : this(session1, null)
        {
            Add(new HiresSnow());
            Add(new FadeWipe(this, true));
        }

        private IEnumerator endingSequence()
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

            // Play title ping music
            titlePingMusic = Audio.Play("event:/desolozantas/music/lvl0/title_ping", Vector2.Zero);
            yield return 2f;

            // Display ending dialog
            var endingTextbox = new Textbox("CH0_END_YOUR_CHOICE_MATTERS");
            yield return say(endingTextbox);

            yield return 1.5f;

            // Fade out
            textAlpha = 1f;
            while (textAlpha > 0f)
            {
                textAlpha -= Engine.DeltaTime * 2f;
                yield return null;
            }
            textAlpha = 0f;

            yield return 0.5f;

            // Complete the chapter
            completeChapter();
        }

        private IEnumerator say(Textbox textbox)
        {
            Engine.Scene.Add(textbox);
            while (textbox.Opened)
            {
                yield return null;
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
            session.SetFlag("ch0_ending_complete");

            var fadeWipe = new FadeWipe(this, false, delegate
            {
                // Transition to CS00_EndingMod for final ending sequence
                var level = new Level();
                level.Session = session;
                level.Add(new CS00_EndingMod(level.Tracker.GetEntity<global::Celeste.Player>()));
                Engine.Scene = level;
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
                    // Subtle glow effect for ending
                    Draw.Rect(0f, 0f, 1920f, 1080f, Color.Gold * (textAlpha * 0.15f));
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
            titlePingMusic?.setPaused(true);
        }

        private void resumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            titlePingMusic?.setPaused(false);
        }

        private void stopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            titlePingMusic?.stop(STOP_MODE.IMMEDIATE);
        }
    }
}




