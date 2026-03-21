namespace MaggyHelper.Cutscenes
{
    [Tracked]
    public class CS18_Outro : CutsceneEntity
    {
        private global::Celeste.Player player;
        private bool phoneRinging = false;
        private bool doorLocked = false;
        private bool gameClosing = false;
        private Level level;

        public CS18_Outro(global::Celeste.Player player) : base(false, true)
        {
            this.player = player;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
            
            if (player == null)
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
        }

        public override void OnBegin(Level level)
        {
            if (level != null)
            {
                level.TimerStopped = true;
                level.TimerHidden = true;
                level.SaveQuitDisabled = true;
                level.PauseLock = true;
                level.AllowHudHide = false;
            }

            Add(new Coroutine(cutsceneSequence(level)));
        }

        /// <summary>
        /// Get the main cutscene coroutine - used by the trigger
        /// </summary>
        /// <returns>The cutscene coroutine</returns>
        public IEnumerator GetCoroutine()
        {
            return cutsceneSequence(level);
        }

        private IEnumerator cutsceneSequence(Level level)
        {
            // Prepare player for cutscene
            SetupPlayer();
            
            // Initial fade and pause
            yield return 1f;

            // Main dialog with triggers
            yield return Textbox.Say("CH18_OUTRO", 
                cellPhoneRumble,     // trigger 0 - phone rumbling
                doorShut,            // trigger 1 - door shut
                glitchEffectStart    // trigger 0 again - glitch start
            );

            // After dialog, start the game closing sequence
            yield return gameClosingSequence();
        }

        private void SetupPlayer()
        {
            if (player == null)
                return;

            try
            {
                player.StateMachine.State = 11; // dummy state
                player.StateMachine.Locked = true;
            }
            catch { }

            player.ForceCameraUpdate = true;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
        }

        // Trigger 0: Cell phone rumbling effect
        private IEnumerator cellPhoneRumble()
        {
            phoneRinging = true;
            
            // Play phone rumble sound if available
            try
            {
                Audio.Play("event:/game/02_old_site/sequence_phone_ring_loop", player.Position);
            }
            catch
            {
                // Fallback to a generic sound
                Audio.Play("event:/game/general/thing_booped", player.Position);
            }
            
            // Add screen shake for phone vibration
            level?.Shake(0.3f);
            
            // Rumble controller
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            
            yield return 2f;
            
            phoneRinging = false;
        }

        // Trigger 1: Door shutting and locking sound
        private IEnumerator doorShut()
        {
            doorLocked = true;
            
            // Play door closing sound
            try
            {
                Audio.Play("event:/game/03_resort/door_metal_close", player.Position);
            }
            catch
            {
                // Fallback sound
                Audio.Play("event:/game/general/fallblock_impact", player.Position);
            }
            
            yield return 0.5f;
            
            // Play locking sound
            try
            {
                Audio.Play("event:/game/03_resort/key_unlock", player.Position);
            }
            catch
            {
                // Fallback sound
                Audio.Play("event:/game/general/touchswitch_any", player.Position);
            }
            
            // Screen shake for emphasis
            level?.Shake(0.4f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            
            yield return 1f;
        }

        // Trigger 0 (second call): Start glitch effects
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
                // Random screen distortion
                level?.Shake(0.2f);
                yield return 0.3f;
                level?.Shake(0.1f);
                yield return 0.1f;
            }

            // Increase intensity
            for (int i = 0; i < 3; i++)
            {
                level?.Shake(0.5f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                
                // Play glitch sound if available
                try
                {
                    Audio.Play("event:/classic/sfx38", player.Position);
                }
                catch
                {
                    // Fallback glitch sound
                    Audio.Play("event:/game/general/thing_booped", player.Position);
                }
                
                yield return 0.2f;
            }

            yield return 1f;
        }

        private IEnumerator gameClosingSequence()
        {
            // Save progress before closing
            var saveData = IngesteModule.SaveData;
            if (saveData != null)
            {
                saveData.UnlockedChapter19 = true;
                IngesteLogger.Info("Chapter 19 unlocked via CH18_OUTRO cutscene");
            }

            // Heavy glitch effects
            for (int i = 0; i < 10; i++)
            {
                level?.Shake(1.0f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                
                // Rapid fire glitch sounds
                try
                {
                    Audio.Play("event:/classic/sfx38", player.Position);
                }
                catch
                {
                    Audio.Play("event:/game/general/thing_booped", player.Position);
                }
                
                yield return 0.1f;
                
                level?.Shake(0.5f);
                yield return 0.05f;
            }

            // Final massive glitch
            level?.Shake(2.0f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            
            // Play multiple overlapping glitch sounds
            try
            {
                Audio.Play("event:/classic/sfx38", player.Position);
                Audio.Play("event:/classic/sfx37", player.Position);
            }
            catch
            {
                Audio.Play("event:/game/general/thing_booped", player.Position);
            }
            
            yield return 2f;

            // Fade to black with a message
            ScreenWipe.WipeColor = Color.Black;
            
            yield return 1f;

            // Show "New Content Unlocked" message (similar to Farewell)
            IngesteLogger.Info("Displaying Chapter 19 unlock notification");
            
            // This would be where you'd show a "Chapter 19 Unlocked!" message
            // For now, we'll just fade out and close
            
            yield return 2f;

            // Close the game (keeping save intact)
            IngesteLogger.Info("Closing game via CH18_OUTRO cutscene");
            Engine.Instance.Exit();

            yield return 3f;
        }

        public override void OnEnd(Level level)
        {
            // Cleanup if needed
            if (level != null)
            {
                level.TimerStopped = false;
                level.TimerHidden = false;
                level.SaveQuitDisabled = false;
                level.PauseLock = false;
                level.AllowHudHide = true;
            }

            // Queue Chapter 19 unlock for next launch (restart-gated progression)
            var saveData = MaggyHelperModule.SaveData;
            if (saveData != null)
            {
                saveData.PendingUnlockChapter19OnRestart = true;
                IngesteLogger.Info("Chapter 19 queued for unlock on next launch");
            }
        }

        public override void Update()
        {
            base.Update();

            // Add glitch effects during the cutscene
            if (gameClosing)
            {
                // Random screen effects
                if (Scene.OnRawInterval(0.1f))
                {
                    level?.Shake(Calc.Random.Range(0.1f, 0.3f));
                }
            }
        }

        public override void Render()
        {
            base.Render();

            // Add visual glitch effects if the game is closing
            if (gameClosing)
            {
                // You could add custom rendering effects here
                // For now, we rely on the shake effects
            }
        }
    }
}



