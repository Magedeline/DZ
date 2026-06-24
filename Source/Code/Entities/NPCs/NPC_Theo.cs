namespace Celeste.NPCs
{
    [CustomEntity("DesoloZantas/NPC_Theo")]
    public class NPC_Theo : Entity
    {
        private const string DONE_TALKING = "theo_done_talking";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;
        private string dialogKey;
        private string flagName;
        private bool enabledByDefault;

        private int currentConversation
        {
            get => (Scene as Level)?.Session.GetCounter("theo_conversation") ?? 0;
            set => (Scene as Level)?.Session.SetCounter("theo_conversation", value);
        }

        public NPC_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            dialogKey = data.Attr("dialogKey", "MAGGYHELPER_CH0_THEO_A");
            flagName = data.Attr("flagName", "theo_met");
            enabledByDefault = data.Bool("enabledByDefault", true);
            
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("theo"));
            sprite.Play("idle");
            sprite.Color = Color.White;
        }

        private void setupCollision()
        {
            Add(talker = new TalkComponent(
                new Rectangle(-20, -8, 40, 16),
                new Vector2(0f, -24f),
                onTalk
            ));
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level)
            {
                if (level.Session.GetFlag(DONE_TALKING) && !enabledByDefault)
                {
                    talker.Enabled = false;
                    return;
                }
            }

            talker.Enabled = true;
        }

        private void onTalk(global::Celeste.Player player)
        {
            if (isInteracting) return;

            if (Scene is Level level)
            {
                isInteracting = true;
                level.StartCutscene(onTalkEnd);
                Add(talkRoutine = new Coroutine(talkCoroutine(player)));
            }
        }

        private IEnumerator talkCoroutine(global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;

            sprite.Play("talk");
            
            switch (currentConversation)
            {
                case 0:
                    yield return Textbox.Say("MAGGYHELPER_CH0_THEO_A");
                    break;
                case 1:
                    yield return Textbox.Say("MAGGYHELPER_CH0_THEO_0_B");
                    break;
                case 2:
                    yield return Textbox.Say("MAGGYHELPER_CH4_MAGOLOR_AND_THEO");
                    break;
                default:
                    yield return Textbox.Say(dialogKey);
                    break;
            }

            currentConversation++;
            sprite.Play("idle");
            endCutscene();
        }

        private void endCutscene()
        {
            if (Scene is Level level)
            {
                level.EndCutscene();
                onTalkEnd(level);
            }
        }

        private void onTalkEnd(Level level)
        {
            if (talkRoutine != null && talkRoutine.Active)
            {
                talkRoutine.Cancel();
                talkRoutine = null;
            }

            isInteracting = false;
            
            // Set flag that Theo has been talked to
            level.Session.SetFlag(flagName);
            
            if (level.Session.GetFlag(DONE_TALKING))
            {
                talker.Enabled = false;
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            
            if (talkRoutine != null && talkRoutine.Active)
            {
                talkRoutine.Cancel();
                talkRoutine = null;
            }
        }

        // Theo can walk away in cutscenes
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



