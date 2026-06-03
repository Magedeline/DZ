using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Bar Arrival
    /// Handles the arrival at Starlo's bar where Madeline meets Undyne and the Feisty Five
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_BarArrival")]
    public class CS11_BarArrival : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_bar_arrival_complete";
        private const string FLAG_MET_UNDYNE = "ch11_met_undyne";
        private const string FLAG_MET_FEISTY_FIVE = "ch11_met_feisty_five";
        
        private const string DIALOG_PREFIX = "MAGGYHELPER_CH11_BAR_ARRIVIAL"; // Note: Keeping original typo for consistency
        #endregion

        #region Fields
        private Player player;
        private NPC starlo;
        private NPC undyne;
        private NPC madeline;
        private NPC kirby;
        private NPC squirtle;
        private NPC burgdog;
        private NPC spader;
        #endregion

        public CS11_BarArrival(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_BarArrival(Player player)
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

            // Find NPCs in the scene
            FindNPCs(level);
            
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
            // Find NPCs in the scene - these should be placed via Loenn
            starlo = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Starlo"));
            undyne = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Undyne"));
            madeline = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Madeline"));
            kirby = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Kirby"));
            squirtle = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Squirtle"));
            burgdog = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Burgdog"));
            spader = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Spader"));
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Starlo greets
            yield return Textbox.Say(DIALOG_PREFIX, StarloGreets);
            
            // Undyne is shocked
            yield return Textbox.Say(DIALOG_PREFIX, UndyneShocked);
            
            // Madeline responds
            yield return Textbox.Say(DIALOG_PREFIX, MadelineResponds);
            
            // Madeline mentions being saved
            yield return Textbox.Say(DIALOG_PREFIX, MadelineSaved);
            
            // Undyne confused
            yield return Textbox.Say(DIALOG_PREFIX, UndyneConfused);
            
            // Kirby introduces himself
            yield return Textbox.Say(DIALOG_PREFIX, KirbyIntroduces);
            
            // Undyne reacts to Kirby
            yield return Textbox.Say(DIALOG_PREFIX, UndyneReacts);
            
            // Squirtle touches Kirby
            yield return Textbox.Say(DIALOG_PREFIX, SquirtleComments);
            
            // Kirby clarifies
            yield return Textbox.Say(DIALOG_PREFIX, KirbyClarifies);
            
            // Burgdog suggests practice
            yield return Textbox.Say(DIALOG_PREFIX, BurgdogSuggests);
            
            // Kirby protests
            yield return Textbox.Say(DIALOG_PREFIX, KirbyProtests);
            
            // Spader repeats
            yield return Textbox.Say(DIALOG_PREFIX, SpaderRepeats);
            
            // Starlo interrupts
            yield return Textbox.Say(DIALOG_PREFIX, StarloInterrupts);
            
            // Kirby questions
            yield return Textbox.Say(DIALOG_PREFIX, KirbyQuestions);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_MET_UNDYNE, true);
            level.Session.SetFlag(FLAG_MET_FEISTY_FIVE, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        private IEnumerator StarloGreets()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("idle");
                starlo.Sprite.Scale.X = -1; // Face left
            }
            yield break;
        }

        private IEnumerator UndyneShocked()
        {
            // [UNDYNE left normal]
            if (undyne != null && undyne.Sprite != null)
            {
                undyne.Sprite.Play("shocked");
                undyne.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MadelineResponds()
        {
            // [MADELINE left normal]
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("normal");
                madeline.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MadelineSaved()
        {
            // [MADELINE left distracted] then [MADELINE left normal]
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("distracted");
            }
            yield break;
        }

        private IEnumerator UndyneConfused()
        {
            // [UNDYNE left akward]
            if (undyne != null && undyne.Sprite != null)
            {
                undyne.Sprite.Play("awkward");
            }
            yield break;
        }

        private IEnumerator KirbyIntroduces()
        {
            // [KIRBY left normal]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("idle");
                kirby.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator UndyneReacts()
        {
            // [UNDYNE left normal]
            if (undyne != null && undyne.Sprite != null)
            {
                undyne.Sprite.Play("normal");
            }
            yield break;
        }

        private IEnumerator SquirtleComments()
        {
            // [squirtle right normal]
            if (squirtle != null && squirtle.Sprite != null)
            {
                squirtle.Sprite.Play("idle");
                squirtle.Sprite.Scale.X = 1; // Face right
            }
            yield break;
        }

        private IEnumerator KirbyClarifies()
        {
            // [KIRBY left distracted]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("distracted");
            }
            yield break;
        }

        private IEnumerator BurgdogSuggests()
        {
            // [burgdog right normal]
            if (burgdog != null && burgdog.Sprite != null)
            {
                burgdog.Sprite.Play("idle");
                burgdog.Sprite.Scale.X = 1;
            }
            yield break;
        }

        private IEnumerator KirbyProtests()
        {
            // [KIRBY left upset]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("upset");
            }
            yield break;
        }

        private IEnumerator SpaderRepeats()
        {
            // [spader right normal]
            if (spader != null && spader.Sprite != null)
            {
                spader.Sprite.Play("idle");
                spader.Sprite.Scale.X = 1;
            }
            yield break;
        }

        private IEnumerator StarloInterrupts()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("excited");
            }
            yield break;
        }

        private IEnumerator KirbyQuestions()
        {
            // [KIRBY left upset]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("confused");
            }
            yield break;
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
        }
    }
}
