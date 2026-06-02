using Celeste.Entities;
using BadelineDummy = Celeste.Entities.BadelineDummy;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 10 spawn point featuring Madeline, Badeline, and Chara.
    /// This cutscene plays the initial encounter dialog (CH10_MADDY_AND_BADDY_AND_CHARA)
    /// and then spawns NPCs that can be optionally talked to for additional conversations.
    /// 
    /// Dialog IDs used:
    /// - CH10_MADDY_AND_BADDY_AND_CHARA: Initial encounter (auto-plays)
    /// - CH10_TALK_TO_BADDY_AND_CHARA0: Chara talk 1 - About the mountain
    /// - CH10_TALK_TO_BADDY_AND_CHARA1: Badeline talk 1 - About metaphors and forces
    /// - CH10_TALK_TO_BADDY_AND_CHARA2: Chara talk 2 - Warning about monsters
    /// - CH10_TALK_TO_BADDY_AND_CHARA3: Chara talk 3 - About Asriel (interrupted)
    /// </summary>
    public class CS10_MaddyBaddyCharaIntro : CutsceneEntity
    {
        // Session flags
        private const string FLAG_INTRO_PLAYED = "ch10_maddy_baddy_chara_intro_played";
        private const string FLAG_NPCS_PRESENT = "ch10_badeline_chara_present";
        private const string FLAG_ALL_TALKS_DONE = "ch10_all_optional_talks_done";
        
        // Counter keys
        private const string COUNTER_BADELINE_TALK = "ch10_badeline_talk_count";
        private const string COUNTER_CHARA_TALK = "ch10_chara_talk_count";

        private global::Celeste.Player player;
        private BadelineDummy badelineDummy;
        private CharaDummy charaDummy;
        private TalkComponent badelineTalker;
        private TalkComponent charaTalker;
        private string spawnRoomName;

        public CS10_MaddyBaddyCharaIntro(global::Celeste.Player player, string roomName) 
            : base(fadeInOnSkip: true)
        {
            this.player = player;
            this.spawnRoomName = roomName;
        }

        public override void OnBegin(Level level)
        {
            // Check if cutscene already played - spawn NPCs for optional talk instead
            if (level.Session.GetFlag(FLAG_INTRO_PLAYED))
            {
                // If intro already played but NPCs should still be present
                if (!level.Session.GetFlag(FLAG_ALL_TALKS_DONE))
                {
                    Add(new Coroutine(SpawnNPCsOnly(level)));
                }
                else
                {
                    RemoveSelf();
                }
                return;
            }

            Add(new Coroutine(CutsceneSequence(level)));
        }

        /// <summary>
        /// Spawn NPCs without playing the intro cutscene (for when returning to the room)
        /// </summary>
        private IEnumerator SpawnNPCsOnly(Level level)
        {
            yield return null; // Wait one frame for level to be ready

            SpawnBadelineNPC(level);
            SpawnCharaNPC(level);

            level.Session.SetFlag(FLAG_NPCS_PRESENT);
            
            RemoveSelf();
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Wait for player to be ready
            while (player == null || !player.OnGround())
            {
                yield return null;
            }

            // Lock player movement
            player.StateMachine.State = global::Celeste.Player.StDummy;

            // Spawn NPCs
            SpawnBadelineNPC(level);
            SpawnCharaNPC(level);

            yield return 0.5f;

            // Start the main intro dialogue
            yield return Textbox.Say("CH10_MADDY_AND_BADDY_AND_CHARA");

            // Restore player control
            player.StateMachine.State = global::Celeste.Player.StNormal;

            // Mark cutscene as played and flag that NPCs are present
            level.Session.SetFlag(FLAG_INTRO_PLAYED);
            level.Session.SetFlag(FLAG_NPCS_PRESENT);

            EndCutscene(level);
        }

        private void SpawnBadelineNPC(Level level)
        {
            // Check if already exists
            if (level.Entities.FindFirst<BadelineDummy>() != null)
                return;

            // Spawn Badeline dummy to the right of player
            Vector2 badelinePos = player.Position + new Vector2(32f, 0f);
            badelineDummy = new BadelineDummy(badelinePos);
            
            // Setup talk component for optional conversations
            badelineTalker = new TalkComponent(
                new Rectangle(-24, -8, 48, 8),
                new Vector2(0f, -24f),
                OnTalkToBadeline
            )
            {
                PlayerMustBeFacing = true
            };
            badelineDummy.Add(badelineTalker);
            
            level.Add(badelineDummy);
        }

        private void SpawnCharaNPC(Level level)
        {
            // Check if already exists
            if (level.Entities.FindFirst<CharaDummy>() != null)
                return;

            // Spawn Chara dummy to the right of Badeline
            Vector2 charaPos = player.Position + new Vector2(64f, 0f);
            charaDummy = new CharaDummy(charaPos);
            
            // Setup talk component for optional conversations
            charaTalker = new TalkComponent(
                new Rectangle(-24, -8, 48, 8),
                new Vector2(0f, -24f),
                OnTalkToChara
            )
            {
                PlayerMustBeFacing = true
            };
            charaDummy.Add(charaTalker);
            
            level.Add(charaDummy);
        }

        /// <summary>
        /// Handle optional conversation with Badeline.
        /// Cycles through: CH10_TALK_TO_BADDY_AND_CHARA1 -> CH10_TALK_TO_BADDY_AND_CHARA3
        /// </summary>
        private void OnTalkToBadeline(global::Celeste.Player player)
        {
            Level level = Scene as Level;
            if (level == null) return;

            int talkCount = level.Session.GetCounter(COUNTER_BADELINE_TALK);
            
            // Badeline handles dialogs 1 and 3 (the "angry/questioning" ones)
            string dialogKey = talkCount switch
            {
                0 => "CH10_TALK_TO_BADDY_AND_CHARA1",  // About metaphors and dark/light forces
                1 => "CH10_TALK_TO_BADDY_AND_CHARA3",  // About Asriel (gets interrupted)
                _ => null // No more unique dialogs
            };

            if (dialogKey != null)
            {
                level.Session.IncrementCounter(COUNTER_BADELINE_TALK);
                Scene.Add(new CS10_TalkToNPC(player, dialogKey, () => CheckAllTalksDone(level)));
            }
            else
            {
                // All Badeline talks done - disable talker
                if (badelineTalker != null)
                    badelineTalker.Enabled = false;
            }
        }

        /// <summary>
        /// Handle optional conversation with Chara.
        /// Cycles through: CH10_TALK_TO_BADDY_AND_CHARA0 -> CH10_TALK_TO_BADDY_AND_CHARA2
        /// </summary>
        private void OnTalkToChara(global::Celeste.Player player)
        {
            Level level = Scene as Level;
            if (level == null) return;

            int talkCount = level.Session.GetCounter(COUNTER_CHARA_TALK);
            
            // Chara handles dialogs 0 and 2 (the informational ones)
            string dialogKey = talkCount switch
            {
                0 => "CH10_TALK_TO_BADDY_AND_CHARA0",  // About knowing this place
                1 => "CH10_TALK_TO_BADDY_AND_CHARA2",  // Warning about monsters
                _ => null // No more unique dialogs
            };

            if (dialogKey != null)
            {
                level.Session.IncrementCounter(COUNTER_CHARA_TALK);
                Scene.Add(new CS10_TalkToNPC(player, dialogKey, () => CheckAllTalksDone(level)));
            }
            else
            {
                // All Chara talks done - disable talker
                if (charaTalker != null)
                    charaTalker.Enabled = false;
            }
        }

        /// <summary>
        /// Check if all optional conversations have been exhausted
        /// </summary>
        private void CheckAllTalksDone(Level level)
        {
            int badelineTalks = level.Session.GetCounter(COUNTER_BADELINE_TALK);
            int charaTalks = level.Session.GetCounter(COUNTER_CHARA_TALK);

            // Badeline has 2 dialogs (1, 3), Chara has 2 dialogs (0, 2)
            if (badelineTalks >= 2 && charaTalks >= 2)
            {
                level.Session.SetFlag(FLAG_ALL_TALKS_DONE);
            }
        }

        public override void OnEnd(Level level)
        {
            // Restore player movement if still in dummy state
            if (player != null && player.StateMachine.State == global::Celeste.Player.StDummy)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }
        }

        /// <summary>
        /// Call this when player transitions to next room to remove NPCs
        /// </summary>
        public static void RemoveNPCsOnRoomTransition(Level level)
        {
            if (!level.Session.GetFlag(FLAG_NPCS_PRESENT))
                return;

            // Find and remove the dummy NPCs
            foreach (var entity in level.Entities.ToList())
            {
                if (entity is BadelineDummy || entity is CharaDummy)
                {
                    entity.RemoveSelf();
                }
            }

            level.Session.SetFlag(FLAG_NPCS_PRESENT, false);
        }

        /// <summary>
        /// Reset all Chapter 10 intro flags (useful for debug or replay)
        /// </summary>
        public static void ResetCutsceneFlags(Level level)
        {
            level.Session.SetFlag(FLAG_INTRO_PLAYED, false);
            level.Session.SetFlag(FLAG_NPCS_PRESENT, false);
            level.Session.SetFlag(FLAG_ALL_TALKS_DONE, false);
            level.Session.SetCounter(COUNTER_BADELINE_TALK, 0);
            level.Session.SetCounter(COUNTER_CHARA_TALK, 0);
        }
    }

    /// <summary>
    /// Simple cutscene entity for NPC optional conversations in Chapter 10.
    /// Handles locking player, showing dialog, and calling completion callback.
    /// </summary>
    public class CS10_TalkToNPC : CutsceneEntity
    {
        private global::Celeste.Player player;
        private string dialogKey;
        private Action onComplete;

        public CS10_TalkToNPC(global::Celeste.Player player, string dialogKey, Action onComplete = null) 
            : base(fadeInOnSkip: false)
        {
            this.player = player;
            this.dialogKey = dialogKey;
            this.onComplete = onComplete;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(TalkSequence(level)));
        }

        private IEnumerator TalkSequence(Level level)
        {
            // Lock player during conversation
            player.StateMachine.State = global::Celeste.Player.StDummy;
            
            // Show the dialog
            yield return Textbox.Say(dialogKey);
            
            // Restore player control
            player.StateMachine.State = global::Celeste.Player.StNormal;
            
            // Notify completion
            onComplete?.Invoke();
            
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            // Ensure player is restored even if cutscene is skipped
            if (player != null && player.StateMachine.State == global::Celeste.Player.StDummy)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }
        }
    }
}
