namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC01_DZ")]
    public class Npc01DZ : Entity
    {
        private const string donetalking = "DZ01DoneTalking";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;

        private int currentConversation
        {
            get => (Scene as Level)?.Session.GetCounter("DZ01") ?? 0;
            set => (Scene as Level)?.Session.SetCounter("DZ01", value);
        }

        public Npc01DZ(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("magolor"));
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

            switch (currentConversation)
            {
                case 0:
                    yield return Textbox.Say("DZ_CH1_DZ_INTRO");
                    break;
                case 1:
                    yield return Textbox.Say("DZ_CH1_DZ_A");
                    break;
                case 2:
                    yield return Textbox.Say("DZ_CH1_DZ_B");
                    break;
                default:
                    yield return Textbox.Say("DZ_CH1_DZ_DEFAULT");
                    break;
            }

            currentConversation++;
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
            
            if (currentConversation >= 3)
            {
                level.Session.SetFlag(donetalking, true);
                talker.Enabled = false;
            }

            talkRoutine?.RemoveSelf();
            talkRoutine = null;

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }
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

