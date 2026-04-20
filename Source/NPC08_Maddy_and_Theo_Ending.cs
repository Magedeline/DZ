namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC08_Maddy_and_Theo_Ending")]
    public class Npc08MaddyAndTheoEnding : Entity
    {
        private const string donetalking = "ch8_endingmod";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;

        public Npc08MaddyAndTheoEnding(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("madNtheo"));
            sprite.Play("walk");
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

        public IEnumerator WalkTo(Vector2 target, float speed = 64f)
        {
            sprite.Play("walk");
            
            Vector2 direction = (target - Position).SafeNormalize();
            
            while (Vector2.Distance(Position, target) > 4f)
            {
                Position += direction * speed * Engine.DeltaTime;
                yield return null;
            }
            
            Position = target;
            sprite.Play("idle");
        }
    }
}




