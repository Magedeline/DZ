using MaggyHelper.Cutscenes;

namespace MaggyHelper.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC09_SavePoint")]
    public class NPC09_SavePoint : Entity
    {
        private const string donetalking = "ch9_fakesavepoint";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;
        
        // Public properties for cutscene access
        public Sprite Sprite => sprite;
        public string CheckpointId => "npc09_save_point";

        public NPC09_SavePoint(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("FakeSavePoint"));
            sprite.Play("spin");
        }

        private void setupCollision()
        {
            Add(talker = new TalkComponent(
                new Rectangle(-20, -8, 40, 16),
                new Vector2(0f, -24f),
                ontalk
            ));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            if (Scene is Level level)
            {
                // Check if trap was already triggered - disable interaction
                if (level.Session.GetFlag(donetalking) || 
                    level.Session.GetFlag(CS09_FakeSavePoint.FlagTrapTriggered))
                {
                    talker.Enabled = false;
                    return;
                }
            }
            
            talker.Enabled = true;
        }

        private void ontalk(global::Celeste.Player player)
        {
            if (isInteracting) return;
            
            if (Scene is Level level)
            {
                isInteracting = true;
                MaggyProgressionManager.RecordCheckpoint(level, player.Center, CheckpointId);
                
                // Get the current stage based on progression flags
                var currentStage = CS09_FakeSavePoint.GetCurrentStage(level);
                
                // Start the fake save point cutscene with the appropriate stage
                // Pass this NPC reference so the cutscene can interact with the save point
                var cutscene = new CS09_FakeSavePoint(player, currentStage, this);
                level.Add(cutscene);
                level.StartCutscene(ontalkend);
            }
        }

        private void endcutscene()
        {
            if (Scene is Level level)
            {
                level.EndCutscene();
                ontalkend(level);
            }
        }

        private void ontalkend(Level level)
        {
            isInteracting = false;
            
            // Only fully disable if trap was triggered or all stages complete
            if (level.Session.GetFlag(CS09_FakeSavePoint.FlagTrapTriggered))
            {
                level.Session.SetFlag(donetalking, true);
                talker.Enabled = false;
            }

            talkRoutine?.RemoveSelf();
            talkRoutine = null;

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.StateMachine.State = 0; // Return to normal state
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (sprite != null && !isInteracting)
            {
                sprite.Play("spin");
            }
        }
        
        /// <summary>
        /// Plays the activation animation when Kirby interacts with the save point in Stage E
        /// </summary>
        public void PlayActivateAnimation()
        {
            if (sprite != null)
            {
                sprite.Play("activate");
            }
        }
        
        /// <summary>
        /// Plays the trap animation when the save point reveals its true nature
        /// </summary>
        public void PlayTrapAnimation()
        {
            if (sprite != null)
            {
                sprite.Play("trap");
            }
        }
    }
}

