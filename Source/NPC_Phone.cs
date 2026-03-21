namespace MaggyHelper.NPCs
{
    [CustomEntity("DesoloZantas/NPC_Phone")]
    public class NPC_Phone : Entity
    {
        private const string DONE_TALKING = "phone_done_talking";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;
        private string phoneType;
        private string dialogKey;

        public NPC_Phone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            phoneType = data.Attr("phoneType", "mom");
            dialogKey = data.Attr("dialogKey", "KIRBY_PAYPHONE_AWAKE_END");
            
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("maggy_payphone"));
            sprite.Play("idle");
            sprite.Color = Color.White;
        }

        private void setupCollision()
        {
            Add(talker = new TalkComponent(
                new Rectangle(-16, -8, 32, 16),
                new Vector2(0f, -24f),
                onTalk
            ));
            
            Collider = new Hitbox(16f, 32f, -8f, -16f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
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

            sprite.Play("ringing");
            yield return 0.5f;
            
            sprite.Play("active");
            
            switch (phoneType)
            {
                case "mom":
                    yield return Textbox.Say("KIRBY_PAYPHONE_AWAKE_END");
                    break;
                case "ex":
                    yield return Textbox.Say("KIRBY_PAYPHONE_DREAM_END");
                    break;
                default:
                    yield return Textbox.Say(dialogKey);
                    break;
            }

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
            
            // Set flag that phone has been used
            level.Session.SetFlag($"phone_{phoneType}_used");
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
    }
}



