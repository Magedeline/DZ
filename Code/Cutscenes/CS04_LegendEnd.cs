#nullable enable

namespace Celeste.Cutscenes {
    /// <summary>
    /// Cutscene that plays after the Legend Vignette, transitioning back to gameplay
    /// </summary>
    [HotReloadable]
    class Cs04LegendEnd : Scene {
        private Session session;
        private string? areaMusic;
        private float fade = 1f;
        private TextMenu? menu;
        private float pauseFade = 0f;
        private HudRenderer hud;
        private bool exiting;
        private Coroutine? textCoroutine;
        private float textAlpha = 0f;

        public bool CanPause => menu == null;

        public Cs04LegendEnd(Session session, TextMenu? menu = null) 
        {
            this.session = session;
            this.menu = menu;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            textCoroutine = new Coroutine(endSequence());
        }

        public Cs04LegendEnd(Session session1) : this(session1, null) {
            Add(new HiresSnow());
            Add(new FadeWipe(this, true));
        }

        private IEnumerator endSequence() {
            yield return 0.5f;

            // Fade in from black
            while (fade > 0f) {
                fade -= Engine.DeltaTime * 0.5f;
                yield return null;
            }
            fade = 0f;

            yield return 1f;

            // Display Ralsei's outro dialog
            var outroTextbox = new Textbox("MAGGYHELPER_CH4_LEGENDOUTRO");
            yield return say(outroTextbox);

            yield return 1.5f;

            // Restore music and start the level
            textAlpha = 1f;
            while (textAlpha > 0f) {
                textAlpha -= Engine.DeltaTime * 2f;
                yield return null;
            }
            textAlpha = 0f;

            yield return 0.5f;

            startGame();
        }

        private IEnumerator say(Textbox textbox) {
            Engine.Scene.Add(textbox);
            while (textbox.Opened) {
                yield return true;
            }
        }

        public override void Update() {
            if (menu == null) {
                base.Update();
                if (!exiting) {
                    textCoroutine?.Update();
                    if (Input.Pause.Pressed || Input.ESC.Pressed) {
                        OpenMenu();
                    }
                }
            } else if (!exiting) {
                menu.Update();
            }
            pauseFade = Calc.Approach(pauseFade, menu != null ? 1 : 0, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, menu != null ? 0.6f : 0f, Engine.DeltaTime * 3f);
        }

        public void OpenMenu() {
            pauseSfx();
            Audio.Play("guid://{c6311b80-2ae7-4155-80f1-d3e2713efaa3}");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(closeMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(startGame));
            menu.OnCancel = menu.OnESC = menu.OnPause = closeMenu;
        }

        private void closeMenu() {
            resumeSfx();
            Audio.Play("guid://{6096b41c-7b56-4839-ad53-deb9e5693246}");
            if (menu != null) {
                menu.RemoveSelf();
            }
            menu = null;
        }

        private void startGame() {
            stopSfx();
            textCoroutine = null;
            session.Audio.Music.Event = areaMusic;
            if (menu != null) {
                menu.RemoveSelf();
                menu = null;
            }
            var fadeWipe = new FadeWipe(this, false, delegate {
                Engine.Scene = new LevelLoader(session);
            });
            fadeWipe.OnUpdate = delegate (float f) {
                textAlpha = Math.Min(textAlpha, 1f - f);
            };
            exiting = true;
        }

        public override void Render() {
            base.Render();
            if (fade > 0f || textAlpha > 0f) {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                if (fade > 0f) {
                    Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
                }

                // Visual effects for the legend end
                if (textAlpha > 0f) {
                    // Subtle background glow effect
                    Draw.Rect(0f, 0f, 1920f, 1080f, Color.DarkSlateBlue * (textAlpha * 0.2f));
                }

                Draw.SpriteBatch.End();
            }
        }

        private void pauseSfx() {
            foreach (var component in Tracker.GetComponents<SoundSource>()) {
                var sound = (SoundSource)component;
                sound.Pause();
            }
        }

        private void resumeSfx() {
            foreach (var component in Tracker.GetComponents<SoundSource>()) {
                var sound = (SoundSource)component;
                sound.Resume();
            }
        }

        private void stopSfx() {
            var components = new List<Component>();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (var component in components) {
                var sound = (SoundSource)component;
                sound.RemoveSelf();
            }
        }
    }
}





