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
        
        // Dialog key matches the entry in Dialog/English.txt (and all other locales)
        private const string DIALOG_KEY_END = "DZ_CH11_DZ_END";
        private const string DIALOG_KEY_NOT_ENOUGH = "DZ_CH11_COLLECTING_MINIHEART_NOT_ENOUGH";
        
        // Required mini hearts to proceed
        private const int REQUIRED_MINI_HEARTS = 5;
        #endregion

        #region Fields
        private Player player;
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
            // No NPC references needed for this cutscene — portrait animations are
            // handled by dialog portrait tags ([Speaker side anim]) in the dialog file.
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // DZ_CH11_DZ_END contains only portrait tags ([Speaker side anim]) — no {trigger N} tags.
            // Portrait tags are cosmetic and do not invoke C# callbacks, so no callbacks are needed here.
            yield return Textbox.Say(DIALOG_KEY_END);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_MINI_HEARTS_QUEST_STARTED, true);
            level.Session.SetFlag(FLAG_MAGGY_ALLY, true);

            EndCutscene(level);
        }

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
            yield return Textbox.Say("DZ_CH11_COLLECTING_MINIHEART_NOT_ENOUGH");
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
        // dialogRunning stays true for the entire duration of the dialog coroutine,
        // preventing re-trigger while the dialog is open even if the player briefly
        // exits and re-enters the collider.
        private bool dialogRunning = false;

        public CS11_MiniHeartDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Update()
        {
            base.Update();

            if (dialogRunning) return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || !CollideCheck(player)) return;

            Level level = Scene as Level;
            if (CS11_MaggyEnd.GetMiniHeartCount(level.Session) < REQUIRED_MINI_HEARTS)
            {
                dialogRunning = true;
                Add(new Coroutine(ShowDialog()));
            }
        }

        private IEnumerator ShowDialog()
        {
            yield return CS11_MaggyEnd.ShowNotEnoughMiniHeartsDialog();
            dialogRunning = false;
        }
    }
}
