namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity("DZ/NPC_Kirby")]
    public class NPC_Kirby : Entity
    {
        private const string DONE_TALKING = "kirby_done_talking";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;
        private string dialogKey;
        private string flagName;
        private bool enabledByDefault;
        private bool canFloat = true;

        private int currentConversation
        {
            get => (Scene as Level)?.Session.GetCounter("kirby_conversation") ?? 0;
            set => (Scene as Level)?.Session.SetCounter("kirby_conversation", value);
        }

        public NPC_Kirby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            dialogKey = data.Attr("dialogKey", "EXAMPLE_ADVANCED_CUTSCENE");
            flagName = data.Attr("flagName", "kirby_met");
            enabledByDefault = data.Bool("enabledByDefault", true);
            canFloat = data.Bool("canFloat", true);
            
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("kirby"));
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
                    yield return Textbox.Say("EXAMPLE_ADVANCED_CUTSCENE");
                    break;
                case 1:
                    yield return Textbox.Say("EXAMPLE_CONCURRENT_ACTIONS");
                    break;
                case 2:
                    yield return Textbox.Say("EXAMPLE_CUSTOM_TRIGGERS");
                    break;
                case 3:
                    yield return Textbox.Say("KIRBY_PAYPHONE_DREAM_END");
                    break;
                case 4:
                    yield return Textbox.Say("KIRBY_PAYPHONE_AWAKE_END");
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
            
            // Set flag that Kirby has been talked to
            level.Session.SetFlag(flagName);
            
            if (level.Session.GetFlag(DONE_TALKING))
            {
                talker.Enabled = false;
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Kirby bouncy floating animation
            if (canFloat)
            {
                Position.Y += (float)Math.Sin(Scene.TimeActive * 3.0) * 0.25f;
            }
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

        // Kirby can also be used for special cutscene actions
        public IEnumerator ActivateDreamLever()
        {
            sprite.Play("action");
            yield return 0.5f;
            
            // Trigger dream block activation
            if (Scene is Level level)
            {
                level.Session.SetFlag("dream_lever_activated");
            }
            
            sprite.Play("idle");
        }

        public IEnumerator FloatTo(Vector2 target, float duration = 1f)
        {
            Vector2 start = Position;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float progress = timer / duration;
                Position = Vector2.Lerp(start, target, Ease.CubeInOut(progress));
                yield return null;
            }

            Position = target;
        }
    }
}

