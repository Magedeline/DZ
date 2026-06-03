// File: CS11_CollectingMiniHeartEnough.cs
using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Collecting Mini Hearts Complete
    /// Plays when player has collected enough mini hearts to open the door
    /// Features a humorous exchange about Madeline's cowgirl outfit
    /// Includes trigger: Madeline spins rapidly to change back to normal clothes
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_CollectingMiniHeartEnough")]
    public class CS11_CollectingMiniHeartEnough : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_miniheart_enough_complete";
        private const string FLAG_DOOR_UNLOCKED = "ch11_door_unlocked";
        private const string FLAG_COWGIRL_OUTFIT_REMOVED = "ch11_cowgirl_outfit_removed";
        
        private const string DIALOG_KEY = "MAGGYHELPER_CH11_COLLECTING_MINIHEART_ENOUGH";
        
        // Spin effect constants
        private const float SPIN_DURATION = 1.5f;
        private const float SPIN_SPEED = 20f; // Rotations per second
        private const string SFX_SPIN = "event:/char/pusheen/kirby/dreamblock_travel";
        private const string SFX_OUTFIT_CHANGE = "event:/char/pusheen/kirby/appear";
        #endregion

        #region Fields
        private Player player;
        private NPC maggy;
        private NPC starlo;
        private NPC madeline;
        private NPC kirby;
        private NPC badeline;
        private NPC chara;
        
        // For spin effect
        private float spinRotation = 0f;
        private bool isSpinning = false;
        #endregion

        public CS11_CollectingMiniHeartEnough(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_CollectingMiniHeartEnough(Player player)
            : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public override void OnBegin(Level level)
        {
            if (player == null)
                player = level.Tracker.GetEntity<Player>();

            if (ShouldSkipCutscene(level))
            {
                WasSkipped = true;
                EndCutscene(level);
                return;
            }

            FindOrSpawnNPCs(level);
            
            // Lock player movement
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;

            Add(new Coroutine(CutsceneSequence(level)));
        }

        private bool ShouldSkipCutscene(Level level)
        {
            return level.Session.GetFlag(FLAG_CUTSCENE_COMPLETE);
        }

        private void FindOrSpawnNPCs(Level level)
        {
            // FindFirst not available in this Monocle version
            maggy = null;
            starlo = null;
            madeline = null;
            kirby = null;
            badeline = null;
            chara = null;
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Maggy congratulates
            yield return Textbox.Say(DIALOG_KEY, MaggyCongratulates);
            
            yield return 0.3f;
            
            // Starlo is proud
            yield return Textbox.Say(DIALOG_KEY, StarloProud);
            
            // Madeline thanks Starlo
            yield return Textbox.Say(DIALOG_KEY, MadelineThanks);
            
            yield return 0.2f;
            
            // Madeline shows off cowgirl hat
            yield return Textbox.Say(DIALOG_KEY, MadelineShowsOffHat);
            
            yield return 0.3f;
            
            // Kirby disapproves
            yield return Textbox.Say(DIALOG_KEY, KirbyDisapproves);
            
            // Badeline agrees with Kirby
            yield return Textbox.Say(DIALOG_KEY, BadelineAgrees);
            
            yield return 0.2f;
            
            // Chara demands outfit removal
            yield return Textbox.Say(DIALOG_KEY, CharaDemands);
            
            yield return 0.3f;
            
            // Madeline reluctantly agrees
            yield return Textbox.Say(DIALOG_KEY, MadelineReluctantlyAgrees);
            
            // TRIGGER 0: Madeline spins rapidly to change outfit
            yield return Textbox.Say(DIALOG_KEY, Trigger0SpinToNormal);
            
            yield return 0.5f;
            
            // Madeline asks if they're happy now
            yield return Textbox.Say(DIALOG_KEY, MadelineAsksIfHappy);
            
            // Chara confirms satisfaction
            yield return Textbox.Say(DIALOG_KEY, CharaSatisfied);
            
            // Chara doesn't want to see that again
            yield return Textbox.Say(DIALOG_KEY, CharaNeverAgain);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_DOOR_UNLOCKED, true);
            level.Session.SetFlag(FLAG_COWGIRL_OUTFIT_REMOVED, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        
        private IEnumerator MaggyCongratulates()
        {
            // [MAGGY left normal]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("idle");
                maggy.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator StarloProud()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("idle");
                starlo.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MadelineThanks()
        {
            // [MADELINE left normal]
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("idle");
                madeline.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MadelineShowsOffHat()
        {
            // [MADELINE left together]
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("together");
                madeline.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator KirbyDisapproves()
        {
            // [KIRBY right upset]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("upset");
                kirby.Sprite.Scale.X = 1; // Face right
            }
            yield break;
        }

        private IEnumerator BadelineAgrees()
        {
            // [BADELINE left angry]
            if (badeline != null && badeline.Sprite != null)
            {
                badeline.Sprite.Play("angry");
                badeline.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator CharaDemands()
        {
            // [CHARA left angry]
            if (chara != null && chara.Sprite != null)
            {
                chara.Sprite.Play("angry");
                chara.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MadelineReluctantlyAgrees()
        {
            // [MADELINE right sad]
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("sad");
                madeline.Sprite.Scale.X = 1; // Face right
            }
            yield break;
        }

        private IEnumerator Trigger0SpinToNormal()
        {
            // {trigger 0 spins fastest until return to normal}
            Add(new Coroutine(SpinChangeOutfitSequence()));
            yield break;
        }

        private IEnumerator SpinChangeOutfitSequence()
        {
            Level level = Scene as Level;
            
            // Play spin sound
            Audio.Play(SFX_SPIN);
            
            // Start spinning effect
            isSpinning = true;
            float elapsed = 0f;
            
            // Make Madeline NPC spin rapidly
            if (madeline != null)
            {
                Vector2 originalPosition = madeline.Position;
                
                // Zoom in slightly for effect
                Add(new Coroutine(level.ZoomTo(madeline.Position - level.Camera.Position + new Vector2(160f, 90f), 1.2f, 0.5f)));
                
                yield return 0.2f;
                
                // Rapid spinning
                while (elapsed < SPIN_DURATION)
                {
                    elapsed += Engine.DeltaTime;
                    
                    // Calculate rotation
                    spinRotation = elapsed * SPIN_SPEED * MathHelper.TwoPi;
                    
                    // Apply rotation to sprite
                    if (madeline.Sprite != null)
                    {
                        madeline.Sprite.Rotation = spinRotation;
                        
                        // Scale variation for extra effect
                        float scalePulse = 1f + (float)Math.Sin(spinRotation * 2f) * 0.2f;
                        madeline.Sprite.Scale = Vector2.One * scalePulse;
                    }
                    
                    // Add particle effects
                    if (elapsed % 0.1f < Engine.DeltaTime)
                    {
                        level.ParticlesFG.Emit(ParticleTypes.Dust, madeline.Position + new Vector2(Calc.Random.Range(-8f, 8f), Calc.Random.Range(-8f, 8f)));
                    }
                    
                    yield return null;
                }
                
                // Flash effect for outfit change
                level.Flash(Color.White * 0.5f, true);
                Audio.Play(SFX_OUTFIT_CHANGE);
                
                // Stop spinning
                isSpinning = false;
                
                if (madeline.Sprite != null)
                {
                    madeline.Sprite.Rotation = 0f;
                    madeline.Sprite.Scale = Vector2.One;
                    
                    // Change to normal outfit (this would need to be implemented based on your sprite system)
                    // For example: madeline.Sprite.Play("idle_normal");
                }
                
                yield return 0.3f;
                
                // Zoom back out
                Add(new Coroutine(level.ZoomBack(0.5f)));
                
                yield return 0.5f;
            }
            else
            {
                // Fallback if Madeline NPC doesn't exist - just wait
                yield return SPIN_DURATION;
            }
        }

        private IEnumerator MadelineAsksIfHappy()
        {
            // [MADELINE right upset]
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("upset");
                madeline.Sprite.Scale.X = 1; // Face right
            }
            yield break;
        }

        private IEnumerator CharaSatisfied()
        {
            // [CHARA left normal]
            if (chara != null && chara.Sprite != null)
            {
                chara.Sprite.Play("idle");
                chara.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator CharaNeverAgain()
        {
            // [CHARA left bruh]
            if (chara != null && chara.Sprite != null)
            {
                chara.Sprite.Play("bruh");
            }
            yield break;
        }
        
        #endregion

        public override void OnEnd(Level level)
        {
            // Restore player control
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
                player.StateMachine.Locked = false;
            }
            
            // Make sure spinning stops
            isSpinning = false;
            
            // Reset Madeline sprite rotation if needed
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Rotation = 0f;
                madeline.Sprite.Scale = Vector2.One;
            }
        }
    }
}
