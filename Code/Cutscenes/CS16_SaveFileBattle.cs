namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 16 Save File Battle and Els Final Confrontation
    /// </summary>
    [Tracked]
    public class CS16_SaveFileBattle : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS16_SaveFileBattle(global::Celeste.Player player) : base(false, true)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(CutsceneSequence(level)));
        }

        public override void OnEnd(Level level)
        {
            // Cleanup when cutscene ends
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Save file corruption attempt
            level.Flash(Color.Red, false);
            
            yield return Textbox.Say("MAGGYHELPER_CH16_SAVE_FILE_CORRUPTION");

            // Save file restoration by lost souls
            level.Flash(Color.White, false);
            yield return 1f;
            
            yield return Textbox.Say("MAGGYHELPER_CH16_SAVE_FILE_RESTORATION");

            // Final confrontation with Els
            level.Shake(5.0f);
            
            yield return Textbox.Say("MAGGYHELPER_CH16_FINAL_CONFRONTATION");

            // After final blow - Flowey reloads and restores HP
            yield return FloweyReloadSequence(level);

            // Violent attack cutscenes
            yield return ViolentAttackCutscenes(level);

            // Counting sequence (slow to fast, max 30)
            yield return CountingSequence(level);

            // Flowey circles and taunts
            yield return FloweyCirclingTaunt(level);

            // Call for help - but nobody came
            yield return CallForHelpSequence(level);

            // Circling pellet attack
            yield return CirclingPelletAttack(level);

            // Party fully healed - Flowey tries to load but fails
            yield return LoadFailureSequence(level);

            // Six human souls appear
            yield return SixSoulsAppearSequence(level);

            // ELS and Badeline boost up
            yield return BoostUpSequence(level);

            // Final wish to undo Flowey's last wish button
            yield return FinalWishSequence(level);

            EndCutscene(level);
        }

        #region Flowey Reload Sequence
        
        private IEnumerator FloweyReloadSequence(Level level)
        {
            // After taking damage, Flowey prepares to reload
            yield return 0.5f;
            
            // Glitch effects
            Glitch.Value = 0.8f;
            Audio.Play("guid://{b26ebe07-7db5-40d0-9282-2d1449db8c5a}");
            yield return 0.3f;
            Glitch.Value = 0f;
            
            // Screen flash white
            level.Flash(Color.White, true);
            yield return 0.5f;
            
            // RELOAD text appears
            yield return ShowReloadText(level);
            yield return 1f;
            
            // HP fully restored with dramatic effect
            level.Shake(3.0f);
            Audio.Play("guid://{bbe7d0c8-45f9-4671-b49e-8e38e357bb81}");
            
            // Visual indication of HP restoration
            for (int i = 0; i < 5; i++)
            {
                level.Flash(Color.LimeGreen, false);
                yield return 0.1f;
            }
            
            yield return 0.5f;
        }

        #endregion

        #region Violent Attack Cutscenes
        
        private IEnumerator ViolentAttackCutscenes(Level level)
        {
            // CRUSHING ATTACK
            yield return CrushingAttackCutscene(level);
            yield return 1f;
            
            // LASER BEAM ATTACK
            yield return LaserBeamAttackCutscene(level);
            yield return 1f;
            
            // TENTACLE ATTACK
            yield return TentacleAttackCutscene(level);
            yield return 1f;
        }

        private IEnumerator CrushingAttackCutscene(Level level)
        {
            IngesteLogger.Info("CRUSHING ATTACK SEQUENCE");
            
            // Build up tension
            yield return 0.5f;
            
            // Screen darkens
            level.Lighting.Alpha = 0.7f;
            yield return 0.3f;
            
            // Giant vines/hands appear from above
            Audio.Play("event:/new_content/game/10_farewell/badeline_chase_rumble");
            level.Shake(2.0f);
            
            // Multiple crushing impacts
            for (int i = 0; i < 4; i++)
            {
                level.Flash(Color.DarkRed, false);
                level.Shake(5.0f);
                Audio.Play("event:/game/09_core/frontside_get");
                yield return 0.4f;
            }
            
            // Final massive crush
            level.Shake(8.0f);
            level.Flash(Color.Red, true);
            Audio.Play("event:/new_content/game/10_farewell/badeline_boss_hurt");
            yield return 0.8f;
            
            // Return to normal
            level.Lighting.Alpha = 0f;
            yield return 0.5f;
        }

        private IEnumerator LaserBeamAttackCutscene(Level level)
        {
            IngesteLogger.Info("LASER BEAM ATTACK SEQUENCE");
            
            // Charging sound and visual
            Audio.Play("event:/new_content/game/10_farewell/badeline_chase_pulse");
            
            // Screen flickers with energy
            for (int i = 0; i < 6; i++)
            {
                level.Flash(Color.Yellow, false);
                yield return 0.15f;
                level.Flash(Color.White, false);
                yield return 0.15f;
            }
            
            // FIRE THE LASER
            level.Flash(Color.White, true);
            level.Shake(10.0f);
            Audio.Play("event:/new_content/game/10_farewell/badeline_boss_laser");
            
            // Sweeping laser effect
            for (int i = 0; i < 3; i++)
            {
                level.Flash(Color.Cyan, false);
                level.Shake(7.0f);
                Audio.Play("guid://{bbe7d0c8-45f9-4671-b49e-8e38e357bb81}");
                yield return 0.3f;
            }
            
            // Final explosion
            level.Flash(Color.White, true);
            level.Shake(8.0f);
            yield return 1f;
        }

        private IEnumerator TentacleAttackCutscene(Level level)
        {
            IngesteLogger.Info("TENTACLE ATTACK SEQUENCE");
            
            // Ominous rumbling from below
            Audio.Play("event:/new_content/game/10_farewell/badeline_chase_rumble");
            level.Shake(1.5f);
            yield return 0.8f;
            
            // Tentacles burst from ground
            level.Shake(6.0f);
            level.Flash(Color.Purple, false);
            Audio.Play("guid://{bbe7d0c8-45f9-4671-b49e-8e38e357bb81}");
            yield return 0.3f;
            
            // Rapid tentacle strikes
            for (int i = 0; i < 8; i++)
            {
                level.Flash(Color.DarkViolet, false);
                level.Shake(4.0f);
                Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}");
                yield return 0.2f;
            }
            
            // Constriction effect
            level.Shake(9.0f);
            level.Flash(Color.Purple, true);
            Audio.Play("event:/new_content/game/10_farewell/badeline_boss_hurt");
            
            // Squeeze multiple times
            for (int i = 0; i < 3; i++)
            {
                level.Shake(7.0f);
                yield return 0.4f;
            }
            
            yield return 0.5f;
        }

        #endregion

        #region Counting Sequence
        
        private IEnumerator CountingSequence(Level level)
        {
            IngesteLogger.Info("COUNTING SEQUENCE - Slow to Fast (Max 30)");
            
            // Start slow, accelerate to fast
            float delay = 1.0f; // Start with 1 second delay
            float minDelay = 0.1f; // Fastest speed
            float acceleration = 0.95f; // Speed multiplier
            
            for (int count = 1; count <= 30; count++)
            {
                // Display number
                yield return ShowCountNumber(level, count);
                
                // Play tick sound
                Audio.Play("guid://{dcef4547-0df6-4b2c-9a70-3aa537982264}");
                
                // Small screen shake that intensifies
                level.Shake(0.2f + (count * 0.05f));
                
                // Wait with decreasing delay
                yield return delay;
                
                // Accelerate
                delay = Math.Max(minDelay, delay * acceleration);
            }
            
            // Final dramatic pause
            level.Shake(5.0f);
            level.Flash(Color.Red, false);
            yield return 0.5f;
        }

        private IEnumerator ShowCountNumber(Level level, int number)
        {
            // Visual display of counting number
            // This would show on-screen text/UI
            IngesteLogger.Info($"Count: {number}");
            yield return null;
        }

        #endregion

        #region Flowey Circling and Taunting
        
        private IEnumerator FloweyCirclingTaunt(Level level)
        {
            IngesteLogger.Info("FLOWEY CIRCLING SEQUENCE");
            
            // Flowey begins circling the party
            Audio.Play("event:/new_content/game/10_farewell/badeline_chase_spiral");
            
            // Spinning visual effect
            for (int i = 0; i < 8; i++)
            {
                level.Shake(2.0f);
                yield return 0.3f;
            }
            
            // Display the "Take the L" dialog
            yield return Textbox.Say("MAGGYHELPER_CH16_TAKE_THE_L");
            
            yield return 0.5f;
        }

        #endregion

        #region Call for Help Sequence
        
        private IEnumerator CallForHelpSequence(Level level)
        {
            IngesteLogger.Info("CALL FOR HELP SEQUENCE");
            
            // Madeline tries to call for help
            yield return 0.5f;
            
            // Silence... eerie pause
            Audio.SetMusic(null);
            yield return 1.5f;
            
            // Flowey responds with the iconic line
            yield return Textbox.Say("MAGGYHELPER_CH16_YOU_CALL_HELP");
            
            yield return 0.5f;
        }

        #endregion

        #region Circling Pellet Attack
        
        private IEnumerator CirclingPelletAttack(Level level)
        {
            IngesteLogger.Info("CIRCLING PELLET ATTACK");
            
            // Pellets surround the party
            Audio.Play("guid://{bbe7d0c8-45f9-4671-b49e-8e38e357bb81}");
            level.Shake(3.0f);
            
            // Pellets close in progressively
            for (int i = 0; i < 10; i++)
            {
                level.Flash(Color.White, false);
                Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}");
                level.Shake(2.0f + i * 0.3f);
                yield return 0.25f;
            }
            
            // Critical moment - about to hit
            level.Shake(8.0f);
            yield return 0.5f;
            
            // BUT THEN...
            level.Flash(Color.LimeGreen, true);
            Audio.Play("event:/new_content/game/10_farewell/badeline_boss_hurt");
            
            yield return 1f;
        }

        #endregion

        #region Load Failure Sequence
        
        private IEnumerator LoadFailureSequence(Level level)
        {
            IngesteLogger.Info("LOAD FAILURE SEQUENCE");
            
            // Party is fully healed miraculously
            level.Flash(Color.LimeGreen, true);
            Audio.Play("event:/game/general/shrine_unlock_complete");
            yield return 1f;
            
            // Flowey tries to LOAD
            Glitch.Value = 0.5f;
            Audio.Play("guid://{b26ebe07-7db5-40d0-9282-2d1449db8c5a}");
            yield return 0.5f;
            
            // LOAD command appears
            yield return ShowLoadText(level);
            yield return 1f;
            
            // ERROR - Load failed
            Glitch.Value = 1f;
            level.Flash(Color.Red, false);
            Audio.Play("guid://{b6aad0d5-cc07-46f4-ab3a-3b3692a57678}");
            yield return 0.3f;
            
            Audio.Play("guid://{b6aad0d5-cc07-46f4-ab3a-3b3692a57678}");
            yield return 0.3f;
            
            Audio.Play("guid://{b6aad0d5-cc07-46f4-ab3a-3b3692a57678}");
            yield return 0.3f;
            
            Glitch.Value = 0f;
            
            // LOAD FAILED text
            yield return ShowLoadFailedText(level);
            yield return 2f;
        }

        #endregion

        #region Six Souls Appear Sequence
        
        private IEnumerator SixSoulsAppearSequence(Level level)
        {
            IngesteLogger.Info("SIX HUMAN SOULS APPEAR");
            
            // Dramatic buildup
            try { Audio.SetMusic(""); } catch { }
            level.Flash(Color.White, false);
            yield return 1f;
            
            // Souls appear one by one
            Color[] soulColors = new Color[]
            {
                Color.Cyan,       // Patience
                Color.Orange,     // Bravery
                Color.Blue,       // Integrity
                Color.Purple,     // Perseverance
                Color.Green,      // Kindness
                Color.Yellow      // Justice
            };
            
            for (int i = 0; i < 6; i++)
            {
                level.Flash(soulColors[i], false);
                Audio.Play("event:/game/general/shrine_unlock_complete");
                level.Shake(2.0f);
                yield return 0.8f;
            }
            
            // All six souls surround Flowey/ELS
            level.Flash(Color.White, true);
            level.Shake(5.0f);
            yield return 1.5f;
            
            // Souls begin attacking
            for (int i = 0; i < 12; i++)
            {
                level.Flash(soulColors[i % 6], false);
                Audio.Play("guid://{bbe7d0c8-45f9-4671-b49e-8e38e357bb81}");
                level.Shake(3.0f);
                yield return 0.3f;
            }
            
            yield return 1f;
        }

        #endregion

        #region Badeline Boost Up
        
        private IEnumerator BoostUpSequence(Level level)
        {
            IngesteLogger.Info("BADELINE BOOST UP");
            
            // ELS powers up
            level.Flash(Color.Cyan, false);
            Audio.Play("event:/new_content/game/10_farewell/badeline_chase_pulse");
            level.Shake(4.0f);
            yield return 1f;
            
            // Badeline powers up
            level.Flash(Color.Red, false);
            Audio.Play("guid://{bbe7d0c8-45f9-4671-b49e-8e38e357bb81}");
            level.Shake(4.0f);
            yield return 1f;
            
            // Combined boost
            level.Flash(Color.Purple, true);
            level.Shake(7.0f);
            Audio.Play("event:/new_content/game/10_farewell/badeline_boss_laser");
            
            // Energy surge
            for (int i = 0; i < 5; i++)
            {
                level.Flash(Color.White, false);
                yield return 0.2f;
            }
            
            yield return 1f;
        }

        #endregion

        #region Final Wish Sequence
        
        private IEnumerator FinalWishSequence(Level level)
        {
            IngesteLogger.Info("FINAL WISH TO UNDO FLOWEY'S LAST WISH");
            
            // Madeline makes the wish
            yield return Textbox.Say("MAGGYHELPER_CH16_FINAL_DEFEAT");
            
            // Dramatic pause
            yield return 1.5f;
            
            // Reality begins to undo
            try { Audio.SetMusic(""); } catch { }
            Glitch.Value = 0.3f;
            
            for (int i = 0; i < 10; i++)
            {
                level.Flash(Color.White, false);
                level.Shake(5.0f - i * 0.3f);
                Audio.Play("guid://{42d600c5-32c3-4433-8a1e-be394b30bcde}");
                yield return 0.4f;
            }
            
            // Final white flash
            level.Flash(Color.White, true);
            level.Shake(10.0f);
            Glitch.Value = 1f;
            Audio.Play("event:/new_content/game/10_farewell/badeline_boss_hurt");
            yield return 2f;
            
            // Everything calms down
            Glitch.Value = 0f;
            level.Lighting.Alpha = 0f;
            try { Audio.SetMusic("event:/music/1-intro/main"); } catch { }
            
            yield return 2f;
        }

        #endregion

        #region Helper Methods
        
        private IEnumerator ShowReloadText(Level level)
        {
            // Display "LOADING..." or "RELOAD" text on screen
            IngesteLogger.Info("*** LOADING... ***");
            yield return null;
        }

        private IEnumerator ShowLoadText(Level level)
        {
            // Display "LOAD" command text
            IngesteLogger.Info("*** LOAD ***");
            yield return null;
        }

        private IEnumerator ShowLoadFailedText(Level level)
        {
            // Display "LOAD FAILED" error text
            IngesteLogger.Info("*** LOAD FAILED ***");
            yield return null;
        }

        #endregion
    }
}




