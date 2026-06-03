namespace Celeste.Cutscenes
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
            yield return Textbox.Say("MAGGYHELPER_CH18_OUTRO", 
                madelineWalkRight,     // trigger 0 - madeline walk to right 4 step
                badelineAppear,        // trigger 1 - badeline appear attach to madeline
                cellPhoneRumble,       // trigger 2 - cellPhoneRumble
                doorShut,              // trigger 3 - doorshut
                madelineRunLeft,       // trigger 4 - madeline run to left 3 step
                glitchEffectStart,     // trigger 5 - glitcheffect start
                fadeToWhite,           // trigger 6 - fade to white
                closeGame,             // trigger 7 - close game
                unlockChapter19        // trigger 8 - unlock chapter 19 in overworld map
            );
        }

        private void SetupPlayer()
        {
            if (player == null)
                return;

            try
            {
                player.StateMachine.State = Player.StDummy;
                player.StateMachine.Locked = true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CS18_Outro", $"Failed to set player state machine: {ex.Message}");
            }

            player.ForceCameraUpdate = true;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
        }

        // Trigger 0: Madeline walk to right 4 steps
        private IEnumerator madelineWalkRight()
        {
            if (player == null)
                yield break;

            // Walk right 4 steps
            for (int i = 0; i < 4; i++)
            {
                player.Facing = Facings.Right;
                yield return 0.5f;
            }
        }

        // Trigger 1: Badeline appear attach to Madeline
        private IEnumerator badelineAppear()
        {
            if (player == null)
                yield break;

            // Create Badeline entity attached to player
            var badeline = new BadelineOldsite(player.Position, 0);
            Scene.Add(badeline);
            
            // Play appearance sound
            try
            {
                Audio.Play("event:/char/badeline/madeline_appear", player.Position);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CS18_Outro", $"Failed to play badeline appear sound: {ex.Message}");
                Audio.Play("event:/game/general/thing_booped", player.Position);
            }

            level?.Shake(0.3f);
            yield return 1f;
        }

        // Trigger 2: Cell phone rumbling effect
        private IEnumerator cellPhoneRumble()
        {
            phoneRinging = true;
            
            // Play phone rumble sound with "end" parameter at 1f
            try
            {
                Audio.Play("event:/char/pusheen/madeline/cell_phone_ringing", player.Position, "end", 1f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CS18_Outro", $"Failed to play phone ringing sound: {ex.Message}");
                Audio.Play("event:/game/general/thing_booped", player.Position);
            }
            
            // Add screen shake for phone vibration
            level?.Shake(0.3f);
            
            // Rumble controller
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            
            yield return 2f;
            
            phoneRinging = false;
        }

        // Trigger 3: Door shutting and locking sound
        private IEnumerator doorShut()
        {
            doorLocked = true;
            
            // Play door closing sound
            try
            {
                Audio.Play("event:/game/03_resort/door_metal_close", player.Position);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CS18_Outro", $"Failed to play door close sound: {ex.Message}");
                Audio.Play("event:/game/general/fallblock_impact", player.Position);
            }
            
            yield return 0.5f;
            
            // Play locking sound
            try
            {
                Audio.Play("event:/new_content/game/pusheen/19_spaces/locked_door_appear_1", player.Position);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CS18_Outro", $"Failed to play locked door sound: {ex.Message}");
                Audio.Play("event:/game/general/touchswitch_any", player.Position);
            }
            // Create a drawable solid for the locked door
            if (level != null)
            {
                var doorSolid = new Solid(new Vector2((int)player.X - 20, (int)player.Y - 40), 40f, 80f, safe: false);
                doorSolid.Visible = true;
                level.Add(doorSolid);
            }
            
            // Screen shake for emphasis
            level?.Shake(0.4f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            
            yield return 1f;
        }

        // Trigger 4: Madeline run to left 3 steps
        private IEnumerator madelineRunLeft()
        {
            if (player == null)
                yield break;

            // Run left 3 steps
            for (int i = 0; i < 3; i++)
            {
                player.Facing = Facings.Left;
                yield return 0.3f; // Faster than walking
            }
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
            ScreenWipe.WipeColor = Color.White;
            yield return 2f;
        }

        // Trigger 7: Close game
        private IEnumerator closeGame()
        {
            IngesteLogger.Info("Closing game via CH18_OUTRO cutscene");
            Engine.Instance.Exit();
            yield return 3f;
        }

        // Trigger 8: Unlock chapter 19 in overworld map
        private IEnumerator unlockChapter19()
        {
            var saveData = IngesteModule.SaveData;
            if (saveData != null)
            {
                saveData.UnlockedChapter19 = true;
                IngesteLogger.Info("Chapter 19 unlocked via CH18_OUTRO cutscene");
            }
            yield return 0.5f;
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
                
                // Play glitch sound with "crash_pitch" parameter
                try
                {
                    Audio.Play("event:/game/pusheen/16_myworld/destroyed_c", player.Position, "crash_pitch", 1f);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "CS18_Outro", $"Failed to play glitch sound: {ex.Message}");
                    Audio.Play("event:/game/general/thing_booped", player.Position);
                }
                
                yield return 0.2f;
            }

            yield return 1f;
        }

        private IEnumerator gameClosingSequence()
        {
            // This method is no longer used - sequence is now handled by individual triggers
            yield break;
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
                // Random screen flicker effect
                if (Calc.Random.Chance(0.3f))
                {
                    Draw.Rect(new Rectangle(0, 0, 320, 180), new Color(Calc.Random.NextFloat(), Calc.Random.NextFloat(), Calc.Random.NextFloat(), 0.1f));
                }

                // Chromatic aberration effect (RGB split)
                if (Calc.Random.Chance(0.5f))
                {
                    int offset = (int)Calc.Random.Range(-2f, 2f);
                    Draw.Rect(new Rectangle(offset, 0, 320, 180), new Color(1, 0, 0, 0.05f));
                    Draw.Rect(new Rectangle(-offset, 0, 320, 180), new Color(0, 0, 1, 0.05f));
                }

                // Random horizontal glitch lines
                if (Calc.Random.Chance(0.4f))
                {
                    int y = Calc.Random.Next(0, 180);
                    int height = Calc.Random.Range(1, 5);
                    Draw.Rect(new Rectangle(0, y, 320, height), Color.White * 0.2f);
                }

                // Random vertical glitch bars
                if (Calc.Random.Chance(0.3f))
                {
                    int x = Calc.Random.Next(0, 320);
                    int width = Calc.Random.Range(1, 10);
                    Draw.Rect(new Rectangle(x, 0, width, 180), new Color(Calc.Random.NextFloat(), Calc.Random.NextFloat(), Calc.Random.NextFloat(), 0.15f));
                }

                // Color inversion flicker
                if (Calc.Random.Chance(0.1f))
                {
                    Draw.Rect(new Rectangle(0, 0, 320, 180), new Color(1, 1, 1, 0.1f));
                }
            }
        }
    }
}



