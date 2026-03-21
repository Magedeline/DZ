namespace MaggyHelper.NPCs
{
    [CustomEntity("DesoloZantas/NPC_Chara")]
    public class NPC_Chara : Entity
    {
        private const string DONE_TALKING = "chara_done_talking";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;
        private string dialogKey;
        private string flagName;
        private bool enabledByDefault;

        private int currentConversation
        {
            get => (Scene as Level)?.Session.GetCounter("chara_conversation") ?? 0;
            set => (Scene as Level)?.Session.SetCounter("chara_conversation", value);
        }

        public NPC_Chara(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            dialogKey = data.Attr("dialogKey", "CH2_CHARA_INTRO");
            flagName = data.Attr("flagName", "chara_met");
            enabledByDefault = data.Bool("enabledByDefault", true);
            
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("maggy_chara"));
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

            // Chara-specific animation and effects
            sprite.Play("talk");
            
            switch (currentConversation)
            {
                case 0:
                    yield return Textbox.Say("CH2_CHARA_INTRO");
                    break;
                case 1:
                    yield return Textbox.Say("CH2_CHARA_BREAKSOUT");
                    break;
                case 2:
                    yield return Textbox.Say("CH4_CHARA_2ND_INTRO");
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
            
            // Set flag that Chara has been talked to
            level.Session.SetFlag(flagName);
            
            if (level.Session.GetFlag(DONE_TALKING))
            {
                talker.Enabled = false;
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Chara-specific floating animation
            Position.Y += (float)Math.Sin(Scene.TimeActive * 2.0) * 0.2f;
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



