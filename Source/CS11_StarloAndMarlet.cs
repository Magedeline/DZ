using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Starlo and Marlet encounter
    /// Handles the conversation between Starlo, Marlet, and the main characters
    /// about the missing Christmas gifts and the mysterious thief
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_StarloAndMarlet")]
    public class CS11_StarloAndMarlet : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_starlo_marlet_complete";
        private const string FLAG_MARLET_SUSPICIOUS = "ch11_marlet_suspicious";
        private const string FLAG_BAR_UNLOCKED = "ch11_bar_unlocked";
        
        private const string DIALOG_PREFIX = "CH11_STARLO_AND_MARLET";
        #endregion

        #region Fields
        private Player player;
        private NPC marlet;
        private NPC starlo;
        private NPC madeline;
        private NPC kirby;
        private NPC theo;
        private NPC badeline;
        #endregion

        public CS11_StarloAndMarlet(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_StarloAndMarlet(Player player)
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
            // Try to find NPCs in the scene
            // These would need to be placed in the level via Loenn
            marlet = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Marlet"));
            starlo = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Starlo"));
            madeline = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Madeline"));
            kirby = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Kirby"));
            theo = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Theo"));
            badeline = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Badeline"));
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Marlet complaining about gifts
            yield return Textbox.Say(DIALOG_PREFIX, MarletComplains);
            
            // Starlo accuses Marlet
            yield return Textbox.Say(DIALOG_PREFIX, StarloAccuses);
            
            // Marlet defends herself
            yield return Textbox.Say(DIALOG_PREFIX, MarletDefends);
            
            // Starlo explains suspicion
            yield return Textbox.Say(DIALOG_PREFIX, StarloExplains);
            
            // Madeline intervenes
            yield return Textbox.Say(DIALOG_PREFIX, MadelineIntervenes);
            
            // Starlo recognizes Madeline
            yield return Textbox.Say(DIALOG_PREFIX, StarloRecognizes);
            
            // Kirby questions
            yield return Textbox.Say(DIALOG_PREFIX, KirbyQuestions);
            
            // Starlo confirms
            yield return Textbox.Say(DIALOG_PREFIX, StarloConfirms);
            
            // Marlet feels sick
            yield return Textbox.Say(DIALOG_PREFIX, MarletFeelsSick);
            
            // Make Marlet walk away
            if (marlet != null)
            {
                yield return MoveNPCAway(marlet, level);
            }
            
            // Starlo frustrated
            yield return Textbox.Say(DIALOG_PREFIX, StarloFrustrated);
            
            // Theo confused
            yield return Textbox.Say(DIALOG_PREFIX, TheoConfused);
            
            // Starlo explains and invites to bar
            yield return Textbox.Say(DIALOG_PREFIX, StarloInvitesToBar);
            
            // Badeline reacts
            yield return Textbox.Say(DIALOG_PREFIX, BadelineReacts);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_MARLET_SUSPICIOUS, true);
            level.Session.SetFlag(FLAG_BAR_UNLOCKED, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        private IEnumerator MarletComplains()
        {
            // [MARLET left normal]
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Play("idle");
                marlet.Sprite.Scale.X = -1; // Face left
            }
            yield break;
        }

        private IEnumerator StarloAccuses()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("idle");
                starlo.Sprite.Scale.X = -1; // Face left
            }
            yield break;
        }

        private IEnumerator MarletDefends()
        {
            // [MARLET left angry]
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Play("angry");
            }
            yield break;
        }

        private IEnumerator StarloExplains()
        {
            // Continue starlo normal pose
            yield break;
        }

        private IEnumerator MadelineIntervenes()
        {
            // [MADELINE left sad]
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("sad");
                madeline.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator StarloRecognizes()
        {
            // Starlo realizes who Madeline is
            yield break;
        }

        private IEnumerator KirbyQuestions()
        {
            // [KIRBY left upset]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("upset");
                kirby.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator StarloConfirms()
        {
            // Starlo confirms
            yield break;
        }

        private IEnumerator MarletFeelsSick()
        {
            // [MARLET left NOTFEELINGWELL]
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Play("dizzy");
            }
            yield break;
        }

        private IEnumerator StarloFrustrated()
        {
            // [STARLO left piss]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("angry");
            }
            yield break;
        }

        private IEnumerator TheoConfused()
        {
            // [THEO left wtf]
            if (theo != null && theo.Sprite != null)
            {
                theo.Sprite.Play("wtf");
                theo.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator StarloInvitesToBar()
        {
            // Starlo returns to normal and invites
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("idle");
            }
            yield break;
        }

        private IEnumerator BadelineReacts()
        {
            // [BADELINE left angry]
            if (badeline != null && badeline.Sprite != null)
            {
                badeline.Sprite.Play("angry");
                badeline.Sprite.Scale.X = -1;
            }
            yield break;
        }
        #endregion

        #region Helper Methods
        private IEnumerator MoveNPCAway(NPC npc, Level level)
        {
            // Make NPC walk away
            float moveDistance = 200f;
            float moveSpeed = 48f;
            Vector2 targetPos = npc.Position + new Vector2(moveDistance, 0);
            
            if (npc.Sprite != null)
            {
                npc.Sprite.Play("walk");
                npc.Sprite.Scale.X = 1; // Face right
            }

            float moved = 0f;
            while (moved < moveDistance)
            {
                float delta = moveSpeed * Engine.DeltaTime;
                npc.Position += new Vector2(delta, 0);
                moved += delta;
                yield return null;
            }

            // Fade out NPC
            float fadeTime = 0.5f;
            for (float t = 0; t < fadeTime; t += Engine.DeltaTime)
            {
                if (npc.Sprite != null)
                {
                    npc.Sprite.Color = Color.White * (1f - t / fadeTime);
                }
                yield return null;
            }

            // Remove NPC from scene
            npc.RemoveSelf();
        }

        public override void OnEnd(Level level)
        {
            // Unlock player
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal;
            }
        }
        #endregion
    }
}