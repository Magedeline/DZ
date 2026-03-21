using System;
using System.Collections;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper
{
    /// <summary>
    /// Cutscene for Chapter 20: Asriel Reveal Identity
    /// Handles the dramatic reveal where Els transforms back into Asriel, revealing his true identity
    /// </summary>
    [Tracked(true)]
    public class CS20_AsrielRevealIdentity : CutsceneEntity
    {
        #region Constants
        
        private const string DIALOGUE_KEY = "CH20_ASRIEL_REVEAL_IDENTITY";
        private const string NAME_REVEAL_KEY = "CH20_ASRIEL_NAME_REVEAL";
        private const string FLAG_REVEAL_IDENTITY = "asriel_reveal_identity";
        
        // SFX Events
        private const string SFX_RIFT_FLICKER = "event:/desolozantas/final_content/game/19_the_end/glitch_long";
        private const string SFX_TRANSFORMATION = "event:/desolozantas/final_content/char/asriel/Asriel_Create";
        private const string SFX_DRAMATIC_REVEAL = "event:/new_content/game/general/dramatic_reveal";
        
        // Camera settings
        private const float ZOOM_SPEED = 0.5f;
        private const float ZOOM_TARGET = 1.5f;
        private const float APPROACH_SPEED = 30f;
        
        #endregion
        
        #region Fields
        
        private Player player;
        private AsrielGodBoss asrielBoss;
        private Level level;
        private Vector2 playerStartPosition;
        private Vector2 cameraStartPosition;
        private float glitchIntensity;
        private bool hasTransformed = false;
        
        #endregion
        
        #region Constructor
        
        public CS20_AsrielRevealIdentity(Player player, AsrielGodBoss asrielBoss)
        {
            this.player = player;
            this.asrielBoss = asrielBoss;
        }
        
        /// <summary>
        /// Static factory method to create and start the reveal identity cutscene
        /// Called from AsrielGodBoss.Added() method
        /// </summary>
        public static IEnumerator RevealIdentityCutscene(AsrielGodBoss boss, string roomId)
        {
            Level level = boss.SceneAs<Level>();
            Player player = level.Tracker.GetEntity<Player>();
            
            if (player == null)
                yield break;
            
            // Set the flag for this specific room so we don't replay it
            string introFlagForRoom = $"asriel_god_boss_intro_{roomId}";
            level.Session.SetFlag(introFlagForRoom);
            level.Session.SetFlag("asriel_god_boss_intro");
            
            // Create and start the cutscene
            CS20_AsrielRevealIdentity cutscene = new CS20_AsrielRevealIdentity(player, boss);
            level.Add(cutscene);
            
            yield return null;
        }
        
        /// <summary>
        /// Determines if the intro cutscene should be shown for a given room ID.
        /// Override or modify this list to add more rooms that trigger the intro.
        /// </summary>
        public static bool ShouldShowIntroForRoom(string roomId)
        {
            // List of room IDs where the Asriel God Boss intro should play
            // Add any room ID that contains this boss where you want an intro
            string[] introRoomIds = new string[]
            {
                "azzyboss-00",      // Original intro room
            };
            
            foreach (string id in introRoomIds)
            {
                if (roomId.Equals(id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region Cutscene Sequence
        
        public override void OnBegin(Level level)
        {
            this.level = level;
            playerStartPosition = player.Position;
            cameraStartPosition = level.Camera.Position;
            if (asrielBoss.NormalSprite != null && asrielBoss.NormalSprite.Has("idle"))
                asrielBoss.NormalSprite.Play("idle");
            if (asrielBoss.NormalSprite != null)
                asrielBoss.NormalSprite.Scale.X = 1f;
            Add(new Coroutine(CutsceneSequence()));
        }
        
        private IEnumerator CutsceneSequence()
        {
            // Disable player control
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;
            
            // Initial dialogue - Kirby notices voice change
            yield return Textbox.Say(DIALOGUE_KEY, 
                Trigger0_OminousZoomAndApproach,
                Trigger1_RiftFlicker,
                Trigger2_TransformToGodForm);
            
            // Name reveal with dramatic effect
            yield return ShowNameReveal();
            
            // Set completion flag
            level.Session.SetFlag(FLAG_REVEAL_IDENTITY);
            
            EndCutscene(level);
        }
        
        #endregion
        
        #region Trigger Methods
        
        /// <summary>
        /// Trigger 0: Ominous zoom in and Kirby approaches Asriel
        /// </summary>
        private IEnumerator Trigger0_OminousZoomAndApproach()
        {
            // Create ominous atmosphere
            float originalZoom = level.Zoom;
            Vector2 targetCameraPos = asrielBoss.Position - new Vector2(160f, 90f);
            Vector2 startCameraPos = level.Camera.Position;
            
            // Calculate approach target position
            Vector2 approachTarget = asrielBoss.Position + new Vector2(-60f, 0f);
            Vector2 playerStartPos = player.Position;
            
            // Simultaneous zoom and approach
            float duration = 2.0f;
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                float progress = Ease.CubeInOut(t / duration);
                
                // Camera zoom
                level.Zoom = Calc.LerpClamp(originalZoom, ZOOM_TARGET, progress);
                
                // Camera pan to Asriel
                level.Camera.Position = Vector2.Lerp(startCameraPos, targetCameraPos, progress);
                
                // Player walks toward Asriel
                player.Position = Vector2.Lerp(playerStartPos, approachTarget, progress);
                
                // Face Asriel
                if (player.X < asrielBoss.X)
                    player.Facing = Facings.Right;
                else
                    player.Facing = Facings.Left;
                
                yield return null;
            }
            
            // Play dramatic sting
            Audio.Play(SFX_DRAMATIC_REVEAL, player.Position);
            
            yield return 0.5f;
        }
        
        /// <summary>
        /// Trigger 1: Rift flicker for a moment with glitch effect
        /// </summary>
        private IEnumerator Trigger1_RiftFlicker()
        {
            // Create glitch effect
            CreateGlitchEffect();
            
            // Play glitch sound
            Audio.Play(SFX_RIFT_FLICKER, asrielBoss.Position);
            
            // Flicker Asriel's sprite
            float flickerDuration = 1.5f;
            int flickerCount = 8;
            float flickerInterval = flickerDuration / flickerCount;
            
            for (int i = 0; i < flickerCount; i++)
            {
                if (asrielBoss.NormalSprite != null)
                    asrielBoss.NormalSprite.Visible = !asrielBoss.NormalSprite.Visible;
                
                // Add screen shake
                level.Shake(0.2f);
                
                yield return flickerInterval;
            }
            
            // Ensure sprite is visible
            if (asrielBoss.NormalSprite != null)
                asrielBoss.NormalSprite.Visible = true;
            
            RemoveGlitchEffect();
            
            yield return 0.3f;
        }
        
        /// <summary>
        /// Trigger 2: Rift flicker again with glitch effects and Asriel transforms from kid to adult god form
        /// </summary>
        private IEnumerator Trigger2_TransformToGodForm()
        {
            if (hasTransformed)
                yield break;
                
            hasTransformed = true;
            
            // Create intense glitch effect
            CreateGlitchEffect(intensity: 2.0f);
            
            // Play transformation sound
            Audio.Play(SFX_TRANSFORMATION, asrielBoss.Position);
            Audio.Play(SFX_RIFT_FLICKER, asrielBoss.Position);
            
            // Intense screen shake
            level.Shake(0.5f);
            
            // Create flash effect
            level.Flash(Color.Purple, drawPlayerOver: false);
            
            yield return 0.2f;
            
            // More intense flickering during transformation
            float transformDuration = 2.5f;
            int flickerCount = 16;
            float flickerInterval = transformDuration / flickerCount;
            
            for (int i = 0; i < flickerCount; i++)
            {
                if (asrielBoss.NormalSprite != null)
                    asrielBoss.NormalSprite.Visible = !asrielBoss.NormalSprite.Visible;
                
                // Escalating shake
                level.Shake(0.3f + (i * 0.05f));
                
                // Color flashes
                Color flashColor = i % 2 == 0 ? Color.Purple : Color.Red;
                level.Flash(flashColor * 0.5f, drawPlayerOver: false);
                
                yield return flickerInterval;
            }
            
            // Final transformation flash
            level.Flash(Color.White, drawPlayerOver: false);
            level.Shake(1.0f);
            
            // Show adult god form
            if (asrielBoss.NormalSprite != null)
            {
                asrielBoss.NormalSprite.Visible = true;
                // Trigger transformation animation if it exists
                if (asrielBoss.NormalSprite.Has("transform"))
                    asrielBoss.NormalSprite.Play("transform");
                else if (asrielBoss.NormalSprite.Has("boss"))
                    asrielBoss.NormalSprite.Play("boss");
            }
            
            RemoveGlitchEffect();
            
            // Particle burst effect
            CreateTransformationParticles();
            
            yield return 1.0f;
        }
        
        #endregion
        
        #region Visual Effects
        
        /// <summary>
        /// Creates a glitch/distortion effect around Asriel
        /// </summary>
        private void CreateGlitchEffect(float intensity = 1.0f)
        {
            glitchIntensity = 0.5f * intensity;
            Glitch.Value = glitchIntensity;
        }
        
        /// <summary>
        /// Removes the glitch effect
        /// </summary>
        private void RemoveGlitchEffect()
        {
            Glitch.Value = 0f;
            glitchIntensity = 0f;
        }
        
        /// <summary>
        /// Creates particle effects for the transformation
        /// </summary>
        private void CreateTransformationParticles()
        {
            if (asrielBoss == null)
                return;
                
            Vector2 center = asrielBoss.Center;
            
            // Create burst of particles
            for (int i = 0; i < 50; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                
                level.ParticlesFG.Emit(
                    AsrielGodBoss.PBurst,
                    center,
                    angle
                );
            }
            
            // Create expanding ring effect
            for (int i = 0; i < 32; i++)
            {
                float angle = (i / 32f) * MathHelper.TwoPi;
                
                level.ParticlesFG.Emit(
                    AsrielGodBoss.PShoot,
                    center,
                    angle
                );
            }
        }
        
        /// <summary>
        /// Shows the dramatic name reveal text
        /// </summary>
        private IEnumerator ShowNameReveal()
        {
            // Create distorted name reveal effect
            yield return Textbox.Say(NAME_REVEAL_KEY);
            
            // Screen flash for emphasis
            level.Flash(Color.Red, drawPlayerOver: false);
            
            yield return 1.0f;
        }
        
        #endregion
        
        #region Cutscene Control
        
        public override void OnEnd(Level level)
        {
            // Restore player control
            if (player != null && player.StateMachine.State == Player.StDummy)
            {
                player.StateMachine.State = Player.StNormal;
                player.DummyAutoAnimate = true;
            }
            
            // Clean up effects
            RemoveGlitchEffect();
            
            // Restore camera zoom
            level.Zoom = 1f;
            
            // Start boss music after reveal
            level.Session.Audio.Music.Event = "event:/desolozantas/final_content/music/lvl20/kirby_vs_asriel_fight_1";
            level.Session.Audio.Apply();
            
            // Trigger boss fight to next phase if needed
            if (asrielBoss != null)
            {
                // Boss should now be in full god form and ready to fight
                // The boss can now start its attack patterns
            }
        }
        
        #endregion
    }
}
