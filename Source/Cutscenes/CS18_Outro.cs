#nullable enable

using DZ;
using FMOD.Studio;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 18 Outro Vignette - Phone call ending that restarts game to chapter 19
    /// </summary>
    [HotReloadable]
    public class CS18_Outro : DesoloZantasVignette
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
        private Player? player;

        public override bool CanPause => menu == null;

        public CS18_Outro(Session session, TextMenu? menu = null)
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

        public CS18_Outro(Session session1) : this(session1, null)
        {
            Add(new DZHiresSnow());
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
                madelineWalkRight,     // trigger 0 - madeline walk to right 4 step
                badelineAppear,        // trigger 1 - badeline appear attach to madeline
                cellPhoneRumble,       // trigger 2 - cellPhoneRumble
                doorShut,              // trigger 3 - doorshut
                madelineRunLeft,       // trigger 4 - madeline run to left 3 step
                glitchEffectStart,     // trigger 5 - glitcheffect start
                fadeToWhite,           // trigger 6 - fade to white
                restartToChapter19     // trigger 7 - restart game to chapter 19
            );

            // After dialog, start the game restart sequence
            yield return gameRestartSequence();
        }

        private void SetupPlayer()
        {
            // Player setup is handled differently in vignette mode
            // We'll create a dummy player for visual effects if needed
        }

        // Trigger 0: Madeline walk to right 4 steps
        private IEnumerator madelineWalkRight()
        {
            // Simulate walking animation with screen effects
            for (int i = 0; i < 4; i++)
            {
                glitchIntensity = 0.05f;
                yield return 0.5f;
            }
            glitchIntensity = 0f;
        }

        // Trigger 1: Badeline appear attach to Madeline
        private IEnumerator badelineAppear()
        {
            // Play appearance sound
            try
            {
                Audio.Play("event:/char/badeline/madeline_appear", Vector2.Zero);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CS18_Outro", $"Failed to play badeline appear sound: {ex.Message}");
                Audio.Play("event:/game/general/thing_booped", Vector2.Zero);
            }

            // Screen shake for effect
            glitchIntensity = 0.3f;
            yield return 1f;
            glitchIntensity = 0f;
        }

        // Trigger 2: Cell phone rumbling effect
        private IEnumerator cellPhoneRumble()
        {
            phoneRinging = true;

            // Play phone rumble sound with EventInstance
            phoneRumbleSfx = Audio.Play("event:/pusheen/game/04_legend/sequence_phone_ring_loop", Vector2.Zero);

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

        // Trigger 3: Door shutting and locking sound
        private IEnumerator doorShut()
        {
            doorLocked = true;

            // Play door closing sound
            Audio.Play("event:/game/03_resort/door_metal_close", Vector2.Zero);

            yield return 0.5f;

            // Play locking sound
            Audio.Play("event:/pusheen/new_content/game/19_spaces/locked_door_appear_1", Vector2.Zero);

            // Screen shake for emphasis
            glitchIntensity = 0.3f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);

            yield return 1f;
            glitchIntensity = 0f;
        }

        // Trigger 4: Madeline run to left 3 steps
        private IEnumerator madelineRunLeft()
        {
            // Simulate running animation with screen effects
            for (int i = 0; i < 3; i++)
            {
                glitchIntensity = 0.08f;
                yield return 0.3f; // Faster than walking
            }
            glitchIntensity = 0f;
        }

        // Trigger 5: Start glitch effects
        private IEnumerator glitchEffectStart()
        {
            gameClosing = true;

            // Begin glitch effects sequence
            yield return beginGlitchSequence();
        }

        // Trigger 6: Fade to white
        private IEnumerator fadeToWhite()
        {
            // Fade to white effect
            for (float alpha = 0f; alpha < 1f; alpha += Engine.DeltaTime * 0.5f)
            {
                textAlpha = alpha;
                yield return null;
            }
            textAlpha = 1f;
            yield return 2f;
        }

        // Trigger 7: Restart game to chapter 19
        private IEnumerator restartToChapter19()
        {
            IngesteLogger.Info("Restarting game to chapter 19 via CH18_OUTRO vignette");
            yield return 0.5f;
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

        private IEnumerator gameRestartSequence()
        {
            // Save progression and set chapter 19 target
            var saveData = DZModule.SaveData;
            if (saveData != null)
            {
                saveData.PendingRestartToChapter19 = true;
                saveData.UnlockedChapter19 = true;
                IngesteLogger.Info("Chapter 19 unlocked and queued for restart via CH18_OUTRO vignette");
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

            // Show "Chapter 19 Loading" message
            IngesteLogger.Info("Loading Chapter 19 (19_Space) in-session via CH18_OUTRO vignette");

            yield return 2f;

            // Transition directly into Chapter 19 instead of closing the game.
            // The save data flags set above ensure Ch19 is unlocked on subsequent
            // launches; here we load it immediately so the player continues play.
            string ch19Sid = DZ.AreaModeExtender.Build0SID("19_Space");
            AreaData ch19Area = AreaData.Get(ch19Sid);
            if (ch19Area != null)
            {
                IngesteLogger.Info($"Transitioning to Chapter 19 (SID={ch19Sid}, ID={ch19Area.ID})");
                var ch19Session = new Session(ch19Area.ToKey());
                LevelEnter.Go(ch19Session, fromSaveData: false);
            }
            else
            {
                IngesteLogger.Error($"Could not find AreaData for Chapter 19 SID '{ch19Sid}'; falling back to chapter select.");
                Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete);
            }

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

        public override void OpenMenu()
        {
            if (!CanPause || Paused) return;
            Paused = true;
            pauseSfx();
            Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(closeMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("WARNING: Skipping will not load Chapter 19")).Pressed(skipToEnd));
            menu.OnCancel = menu.OnESC = menu.OnPause = closeMenu;
        }

        public override void CloseMenu()
        {
            Paused = false;
            resumeSfx();
            Audio.Play("event:/ui/game/unpause");
            if (menu != null)
            {
                menu.RemoveSelf();
            }
            menu = null;
        }

        private void closeMenu() => CloseMenu();

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
                Draw.Rect(0f, 0f, 1920f, 1080f, Color.White * (textAlpha * 0.8f));
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
