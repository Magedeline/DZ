namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC17_Ralsei")]
    public class Npc17Ralsei : Entity
    {
        private const string donetalking = "ralseiDoneTalking";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;

        public Npc17Ralsei(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("ralsei"));
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
                Add(talkRoutine = new Coroutine(talkcoroutine(player)));
            }
        }

        private IEnumerator talkcoroutine(global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;

            yield return Textbox.Say("DZ_CH17_RALSEI_INSIDE");
            
            endcutscene();
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
    }
}




