// File 3: CS11_MaggyEnd.cs
using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Maggy End
    /// After the mountain eruption, Maggy and Kirby discuss the disaster and plan to collect mini heart gems
    /// Includes the collecting mini heart check dialog
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_MaggyEnd")]
    public class CS11_MaggyEnd : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_maggy_end_complete";
        private const string FLAG_MINI_HEARTS_QUEST_STARTED = "ch11_mini_hearts_quest_started";
        private const string FLAG_MAGGY_ALLY = "ch11_maggy_ally";
        
        private const string DIALOG_KEY_END = "CH11_MAGGY_END";
        private const string DIALOG_KEY_NOT_ENOUGH = "CH11_COLLECTING_MINIHEART_NOT_ENOUGH";
        
        // Required mini hearts to proceed
        private const int REQUIRED_MINI_HEARTS = 5;
        #endregion

        #region Fields
        private Player player;
        private NPC maggy;
        private NPC kirby;
        private NPC madeline;
        private NPC starlo;
        #endregion

        public CS11_MaggyEnd(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_MaggyEnd(Player player)
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
            // FindFirst API not available in this Monocle version
            // NPCs would need to be found differently
            maggy = null;
            kirby = null;
            madeline = null;
            starlo = null;
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Maggy recounts the disaster
            yield return Textbox.Say(DIALOG_KEY_END, MaggyRecountsDisaster);
            
            // Kirby agrees it was bad
            yield return Textbox.Say(DIALOG_KEY_END, KirbyAgreesItWasBad);
            
            yield return 0.3f;
            
            // Kirby panics about what to do
            yield return Textbox.Say(DIALOG_KEY_END, KirbyPanics);
            
            yield return 0.5f;
            
            // Maggy says they need to stop it
            yield return Textbox.Say(DIALOG_KEY_END, MaggySaysStopIt);
            
            // Kirby says they need to power up
            yield return Textbox.Say(DIALOG_KEY_END, KirbySaysPowerUp);
            
            yield return 0.3f;
            
            // Kirby asks to collect mini heart gems
            yield return Textbox.Say(DIALOG_KEY_END, KirbyCollectMiniHearts);
            
            // Maggy agrees
            yield return Textbox.Say(DIALOG_KEY_END, MaggyAgrees);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_MINI_HEARTS_QUEST_STARTED, true);
            level.Session.SetFlag(FLAG_MAGGY_ALLY, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        
        private IEnumerator MaggyRecountsDisaster()
        {
            // [MAGGY left normal]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("idle");
                maggy.Sprite.Scale.X = -1;
            }
            yield return null;
        }

        private IEnumerator KirbyAgreesItWasBad()
        {
            // [Kirby left sadder]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("sad");
                kirby.Sprite.Scale.X = -1;
            }
            yield return null;
        }

        private IEnumerator KirbyPanics()
        {
            // [Kirby left panic]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("panic");
            }
            yield return null;
        }

        private IEnumerator MaggySaysStopIt()
        {
            // [MAGGY left normal]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("idle");
            }
            yield return null;
        }

        private IEnumerator KirbySaysPowerUp()
        {
            // [KIRBY left determined]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("determined");
            }
            yield return null;
        }

        private IEnumerator KirbyCollectMiniHearts()
        {
            // [KIRBY left together]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("together");
            }
            yield return null;
        }

        private IEnumerator MaggyAgrees()
        {
            // [MAGGY left normal]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("idle");
            }
            yield return null;
        }
        
        #endregion

        /// <summary>
        /// Check if player has collected enough mini hearts
        /// This should be called by a trigger or entity in the level
        /// </summary>
        public static bool HasEnoughMiniHearts(Session session)
        {
            int miniHeartCount = session.GetCounter("ch11_mini_hearts_collected");
            return miniHeartCount >= REQUIRED_MINI_HEARTS;
        }

        /// <summary>
        /// Get current mini heart count
        /// </summary>
        public static int GetMiniHeartCount(Session session)
        {
            return session.GetCounter("ch11_mini_hearts_collected");
        }

        /// <summary>
        /// Increment mini heart counter
        /// </summary>
        public static void CollectMiniHeart(Session session)
        {
            int current = session.GetCounter("ch11_mini_hearts_collected");
            session.SetCounter("ch11_mini_hearts_collected", current + 1);
        }

        /// <summary>
        /// Show dialog when player tries to open door without enough mini hearts
        /// </summary>
        public static IEnumerator ShowNotEnoughMiniHeartsDialog()
        {
            yield return Textbox.Say("CH11_COLLECTING_MINIHEART_NOT_ENOUGH");
        }

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

    /// <summary>
    /// Helper entity that checks mini heart count and blocks door if not enough collected
    /// Place this entity near the door that requires mini hearts
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_MiniHeartDoor")]
    public class CS11_MiniHeartDoor : Entity
    {
        private const int REQUIRED_MINI_HEARTS = 5;
        private bool hasShownDialog = false;

        public CS11_MiniHeartDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Update()
        {
            base.Update();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                Level level = Scene as Level;
                int miniHeartCount = CS11_MaggyEnd.GetMiniHeartCount(level.Session);

                if (miniHeartCount < REQUIRED_MINI_HEARTS && !hasShownDialog)
                {
                    hasShownDialog = true;
                    // Use Add with Coroutine instead of AddCoroutine
                    Add(new Coroutine(ShowDialog()));
                }
            }
            else
            {
                hasShownDialog = false;
            }
        }

        private IEnumerator ShowDialog()
        {
            yield return CS11_MaggyEnd.ShowNotEnoughMiniHeartsDialog();
        }
    }
}