using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Cowboy Bar Introduction
    /// Handles Starlo's explanation of the gun training stages and mechanics
    /// Introduces the revolver gun mechanic to Kirby
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_CowboyBarIntro")]
    public class CS11_CowboyBarIntro : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_cowboy_bar_intro_complete";
        private const string FLAG_GUN_TUTORIAL_ENABLED = "ch11_gun_tutorial_enabled";
        private const string FLAG_STAGES_UNLOCKED = "ch11_stages_unlocked";
        
        private const string DIALOG_PREFIX = "DZ_CH11_COWBOY_BAR_INTRO";
        
        // Tutorial stage positioning
        private static readonly Vector2 TUTORIAL_AREA_OFFSET = new Vector2(100f, 0f);
        #endregion

        #region Fields
        private Player player;
        private NPC starlo;
        private NPC kirby;
        
        // Gun tutorial props
        private Entity gunDisplay;
        private Entity targetDummies;
        #endregion

        public CS11_CowboyBarIntro(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_CowboyBarIntro(Player player)
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

            // Find NPCs and props
            FindNPCs(level);
            FindProps(level);
            
            // Lock player movement
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;

            Add(new Coroutine(CutsceneSequence(level)));
        }

        private bool ShouldSkipCutscene(Level level)
        {
            return level.Session.GetFlag(FLAG_CUTSCENE_COMPLETE);
        }

        private void FindNPCs(Level level)
        {
            starlo = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Starlo"));
            kirby = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Kirby"));
        }

        private void FindProps(Level level)
        {
            // Find gun display and target dummies for tutorial
            gunDisplay = level.Entities.OfType<Entity>().FirstOrDefault(e => e.GetType().Name.Contains("GunDisplay"));
            targetDummies = level.Entities.OfType<Entity>().FirstOrDefault(e => e.GetType().Name.Contains("TargetDummies"));
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Starlo explains the stages
            yield return Textbox.Say(DIALOG_PREFIX, StarloExplains);
            
            // Show gun display
            if (gunDisplay != null)
            {
                yield return ShowGunDisplay();
            }
            
            // Kirby is unsure
            yield return Textbox.Say(DIALOG_PREFIX, KirbyUnsure);
            
            // Starlo reassures and teaches
            yield return Textbox.Say(DIALOG_PREFIX, StarloTeaches);
            
            // Show tutorial area with targets
            if (targetDummies != null)
            {
                yield return ShowTutorialArea(level);
            }
            
            // Kirby accepts
            yield return Textbox.Say(DIALOG_PREFIX, KirbyAccepts);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_GUN_TUTORIAL_ENABLED, true);
            level.Session.SetFlag(FLAG_STAGES_UNLOCKED, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        private IEnumerator StarloExplains()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("idle");
                starlo.Sprite.Scale.X = -1; // Face left
            }
            yield break;
        }

        private IEnumerator KirbyUnsure()
        {
            // [KIRBY right distracted]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("distracted");
                kirby.Sprite.Scale.X = 1; // Face right
            }
            yield break;
        }

        private IEnumerator StarloTeaches()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("explaining");
            }
            yield break;
        }

        private IEnumerator KirbyAccepts()
        {
            // [KIRBY right determined]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("determined");
            }
            yield break;
        }
        #endregion

        #region Helper Methods
        private IEnumerator ShowGunDisplay()
        {
            // Animate gun display appearing
            if (gunDisplay != null)
            {
                // Start invisible
                var sprite = gunDisplay.Get<Sprite>();
                if (sprite != null)
                {
                    sprite.Color = Color.Transparent;
                }

                // Fade in over 1 second
                float fadeTime = 1f;
                for (float t = 0; t < fadeTime; t += Engine.DeltaTime)
                {
                    sprite = gunDisplay.Get<Sprite>();
                    if (sprite != null)
                    {
                        sprite.Color = Color.White * (t / fadeTime);
                    }
                    yield return null;
                }

                // Ensure fully visible
                sprite = gunDisplay.Get<Sprite>();
                if (sprite != null)
                {
                    sprite.Color = Color.White;
                }

                yield return 0.5f; // Hold for half a second
            }
        }

        private IEnumerator ShowTutorialArea(Level level)
        {
            // Pan camera to show tutorial area
            Vector2 originalCameraPos = level.Camera.Position;
            Vector2 tutorialCameraPos = originalCameraPos + TUTORIAL_AREA_OFFSET;

            // Pan to tutorial area
            float panTime = 2f;
            for (float t = 0; t < panTime; t += Engine.DeltaTime)
            {
                level.Camera.Position = Vector2.Lerp(originalCameraPos, tutorialCameraPos, Ease.CubeInOut(t / panTime));
                yield return null;
            }

            level.Camera.Position = tutorialCameraPos;
            
            // Show targets
            if (targetDummies != null)
            {
                // Targets appear with sound effect
                Audio.Play("event:/game/general/thing_appear", targetDummies.Position);
                
                var targetSprite = targetDummies.Get<Sprite>();
                if (targetSprite != null)
                {
                    targetSprite.Play("appear");
                }
            }

            yield return 2f; // Show for 2 seconds

            // Pan back to original position
            for (float t = 0; t < panTime; t += Engine.DeltaTime)
            {
                level.Camera.Position = Vector2.Lerp(tutorialCameraPos, originalCameraPos, Ease.CubeInOut(t / panTime));
                yield return null;
            }

            level.Camera.Position = originalCameraPos;
        }
        #endregion

        public override void OnEnd(Level level)
        {
            // Unlock player
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal;
            }

            // Ensure camera is back to normal
            level.Camera.Position = player.CameraTarget;
        }
    }
}
