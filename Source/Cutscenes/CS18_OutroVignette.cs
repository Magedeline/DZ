#nullable enable

using DZ;
using FMOD.Studio;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 18 Outro Vignette - Phone call ending that closes the game
    /// </summary>
    public class Cs18OutroVignette : Scene
    {
        public static class LoadingVignetteText
        {
            public const string Dialog = "DZ_CH18_OUTRO";
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
        private float glitchIntensity = 0f;
        private bool phoneRinging = false;
#pragma warning disable CS0414
        private bool doorLocked = false;
#pragma warning restore CS0414
        private bool gameClosing = false;
        private EventInstance? phoneRumbleSfx;

        public bool CanPause => menu == null;

        public Cs18OutroVignette(Session session, TextMenu? menu = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            textCoroutine = new Coroutine(outroSequence());
        }

        public Cs18OutroVignette(Session session1) : this(session1, null)
        {
            Add(new MaggyHiresSnow());
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

            // Play outro dialog with triggers
            yield return Textbox.Say("DZ_CH18_OUTRO",
                cellPhoneRumble,     // trigger 0 - phone rumbling
                doorShut,            // trigger 1 - door shut
                glitchEffectStart    // trigger 2 - glitch start
            );

            // After dialog, start the game closing sequence
            yield return gameClosingSequence();
        }

        // Trigger 0: Cell phone rumbling effect
        private IEnumerator cellPhoneRumble()
        {
            phoneRinging = true;

            // Play phone rumble sound with EventInstance
            phoneRumbleSfx = Audio.Play("event:/char/pusheen/madeline/cell_phone_ringing", Vector2.Zero);

            // Add screen shake for phone vibration
            for (int i = 0; i < 20; i++)
            {
                glitchIntensity = 0.1f;
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                yield return 0.1f;
            }

            phoneRinging = false;
            glitchIntensity = 0f;
        }

        // Trigger 1: Door shutting and locking sound
        private IEnumerator doorShut()
        {
            doorLocked = true;

            // Play door closing sound
            Audio.Play("event:/game/03_resort/door_metal_close", Vector2.Zero);

            yield return 0.5f;

            // Play locking sound
            Audio.Play("event:/game/03_resort/key_unlock", Vector2.Zero);

            // Screen shake for emphasis
            glitchIntensity = 0.3f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);

            yield return 1f;
            glitchIntensity = 0f;
        }

        // Trigger 2: Start glitch effects
        private IEnumerator glitchEffectStart()
        {
            gameClosing = true;

            // Begin glitch effects sequence
            yield return beginGlitchSequence();
        }

        private IEnumerator beginGlitchSequence()
        {
            // Start with subtle glitches
            for (int i = 0; i < 5; i++)
            {
                glitchIntensity = 0.2f;
                yield return 0.3f;
                glitchIntensity = 0.1f;
                yield return 0.1f;
            }

            // Increase intensity
            for (int i = 0; i < 3; i++)
            {
                glitchIntensity = 0.5f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);

                Audio.Play("event:/classic/sfx38", Vector2.Zero);

                yield return 0.2f;
            }

            yield return 1f;
            glitchIntensity = 0f;
        }

        private IEnumerator gameClosingSequence()
        {
            // Save progression before closing (unlock on next launch)
            var saveData = DZModule.SaveData;
            if (saveData != null)
            {
                saveData.PendingUnlockChapter19OnRestart = true;
                IngesteLogger.Info("Chapter 19 queued for unlock via CH18_OUTRO vignette (next launch)");
            }

            // Heavy glitch effects
            for (int i = 0; i < 10; i++)
            {
                glitchIntensity = 1.0f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

                // Rapid fire glitch sounds
                Audio.Play("event:/classic/sfx38", Vector2.Zero);

                yield return 0.1f;

                glitchIntensity = 0.5f;
                yield return 0.05f;
            }

            // Final massive glitch
            glitchIntensity = 2.0f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);

            Audio.Play("event:/classic/sfx38", Vector2.Zero);

            yield return 2f;

            // Fade to black
            fade = 0f;
            while (fade < 1f)
            {
                fade += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            fade = 1f;

            yield return 1f;

            // Show "New Content Unlocked" message
            IngesteLogger.Info("Displaying Chapter 19 unlock notification");

            yield return 2f;

            // Close the game (keeping save intact)
            IngesteLogger.Info("Closing game via CH18_OUTRO vignette");
            Engine.Instance.Exit();

            yield return 3f;
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

            // Add glitch effects during the cutscene
            if (gameClosing && this.OnRawInterval(0.1f))
            {
                glitchIntensity = Calc.Random.Range(0.1f, 0.3f);
            }
        }

        public void OpenMenu()
        {
            pauseSfx();
            Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(closeMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("WARNING: Skipping will not unlock Chapter 19")).Pressed(skipToEnd));
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

        private void skipToEnd()
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
                Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete);
            });

            exiting = true;
        }

        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            if (fade > 0f)
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
            }

            // Render glitch effects
            if (glitchIntensity > 0f)
            {
                // Random color shifts
                Color glitchColor = Calc.Random.Choose(Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Magenta);
                float alpha = glitchIntensity * 0.3f;

                // Random rectangles for glitch effect
                for (int i = 0; i < (int)(glitchIntensity * 10); i++)
                {
                    float x = Calc.Random.Range(0f, 1920f);
                    float y = Calc.Random.Range(0f, 1080f);
                    float w = Calc.Random.Range(10f, 200f);
                    float h = Calc.Random.Range(5f, 50f);

                    Draw.Rect(x, y, w, h, glitchColor * alpha);
                }

                // Screen distortion overlay
                Draw.Rect(0f, 0f, 1920f, 1080f, Color.White * (glitchIntensity * 0.1f));
            }

            if (textAlpha > 0f)
            {
                Draw.Rect(0f, 0f, 1920f, 1080f, Color.DarkRed * (textAlpha * 0.2f));
            }

            Draw.SpriteBatch.End();
        }

        private void pauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
            phoneRumbleSfx?.setPaused(true);
        }

        private void resumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            phoneRumbleSfx?.setPaused(false);
        }

        private void stopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            phoneRumbleSfx?.stop(STOP_MODE.IMMEDIATE);
        }
    }
}

