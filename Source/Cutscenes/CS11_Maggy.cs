// File 1: CS11_DZ.cs
using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - DZ Encounter
    /// Handles the conversation between Kirby and DZ (Magolor)
    /// where DZ explains why he brought Madeline's mother, Theo's sister, and Oshiro to the vortex
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_DZ")]
    public class CS11_DZ : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_DZ_complete";
        private const string FLAG_DZ_ARRIVED = "ch11_DZ_arrived";
        private const string FLAG_EARTH_CRISIS_REVEALED = "ch11_earth_crisis_revealed";
        
        private const string DIALOG_KEY = "DZ_CH11_DZ";
        #endregion

        #region Fields
        private Player player;
        private NPC kirby;
        private NPC DZ;
        private NPC madeline;
        private NPC theo;
        private NPC badeline;
        #endregion

        public CS11_DZ(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_DZ(Player player)
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
            // Try to find NPCs in the scene
            kirby = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Kirby"));
            DZ = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("DZ") || npc.GetType().Name.Contains("Magolor"));
            madeline = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Madeline"));
            theo = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Theo"));
            badeline = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Badeline"));
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Kirby shocked to see Magolor
            yield return Textbox.Say(DIALOG_KEY, KirbyShocked);
            
            // Wait a moment for dramatic effect
            yield return 0.5f;
            
            // DZ explains casually
            yield return Textbox.Say(DIALOG_KEY, DZExplainsCasual);
            
            // DZ mentions bringing people
            yield return Textbox.Say(DIALOG_KEY, DZBroughtPeople);
            
            // DZ mentions creepy vibe
            yield return Textbox.Say(DIALOG_KEY, DZCreepyVibe);
            
            // Kirby gets angry
            yield return Textbox.Say(DIALOG_KEY, KirbyAngry);
            
            yield return 0.3f;
            
            // DZ reveals something bad is happening
            yield return Textbox.Say(DIALOG_KEY, DZRevealsEarthCrisis);
            
            // Kirby asks what's happening
            yield return Textbox.Say(DIALOG_KEY, KirbyAsksWhatHappening);
            
            yield return 0.3f;
            
            // DZ says he'll explain
            yield return Textbox.Say(DIALOG_KEY, DZWillExplain);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_DZ_ARRIVED, true);
            level.Session.SetFlag(FLAG_EARTH_CRISIS_REVEALED, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        
        private IEnumerator KirbyShocked()
        {
            // [KIRBY left normal] -> shocked expression
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("idle");
                kirby.Sprite.Scale.X = -1; // Face left
            }
            yield break;
        }

        private IEnumerator DZExplainsCasual()
        {
            // [DZ left normal]
            if (DZ != null && DZ.Sprite != null)
            {
                DZ.Sprite.Play("idle");
                DZ.Sprite.Scale.X = -1; // Face left
            }
            yield break;
        }

        private IEnumerator DZBroughtPeople()
        {
            // Continue DZ normal pose
            yield break;
        }

        private IEnumerator DZCreepyVibe()
        {
            // [DZ left annoyed]
            if (DZ != null && DZ.Sprite != null)
            {
                DZ.Sprite.Play("annoyed");
            }
            yield break;
        }

        private IEnumerator KirbyAngry()
        {
            // [KIRBY left angry]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("angry");
            }
            yield break;
        }

        private IEnumerator DZRevealsEarthCrisis()
        {
            // [DZ left normal]
            if (DZ != null && DZ.Sprite != null)
            {
                DZ.Sprite.Play("idle");
            }
            yield break;
        }

        private IEnumerator KirbyAsksWhatHappening()
        {
            // [KIRBY left upset]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("upset");
            }
            yield break;
        }

        private IEnumerator DZWillExplain()
        {
            // [DZ left normal] - ready to transition to next cutscene
            if (DZ != null && DZ.Sprite != null)
            {
                DZ.Sprite.Play("idle");
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
        }
    }
}
