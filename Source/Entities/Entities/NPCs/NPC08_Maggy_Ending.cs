namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC08_Maggy_Ending")]
    public class Npc08MaggyEnding : Entity
    {
        private const string donetalking = "ch8_ending";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;
        
        // Public properties for cutscene access
        public Sprite Sprite => sprite;

        public Npc08MaggyEnding(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("magolor"));
            sprite.Play("run");
            sprite.Play("idle");
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
                if (level.Session.GetFlag(donetalking))
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
            level.Session.SetFlag(donetalking, true);
            talker.Enabled = false;

            talkRoutine?.RemoveSelf();
            talkRoutine = null;

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            player?.StateMachine.SetStateName(global::Celeste.Player.StNormal, "idle");
        }

        public override void Update()
        {
            base.Update();
            
            if (sprite != null && !isInteracting)
            {
                sprite.Play("idle");
            }
        }

        public override void Removed(Scene scene)
        {
            talkRoutine?.RemoveSelf();
            base.Removed(scene);
        }
        
        // Method for cutscene movement
        public IEnumerator MoveTo(Vector2 target, float speed = 60f)
        {
            Vector2 start = Position;
            Vector2 direction = (target - start).SafeNormalize();
            float distance = Vector2.Distance(start, target);
            float timeToMove = distance / speed;
            
            if (sprite != null)
            {
                sprite.Play("run");
            }
            
            for (float t = 0f; t < timeToMove; t += Engine.DeltaTime)
            {
                Position = Vector2.Lerp(start, target, t / timeToMove);
                yield return null;
            }
            
            Position = target;
            
            if (sprite != null)
            {
                sprite.Play("idle");
            }
        }
    }
}

