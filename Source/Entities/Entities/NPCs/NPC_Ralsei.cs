namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity("DZ/NPC_Ralsei")]
    public class NPC_Ralsei : Entity
    {
        private const string DONE_TALKING = "ralsei_done_talking";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;
        private string dialogKey;
        private string flagName;
        private bool enabledByDefault;
        private bool isFreed = false;

        private int currentConversation
        {
            get => (Scene as Level)?.Session.GetCounter("ralsei_conversation") ?? 0;
            set => (Scene as Level)?.Session.SetCounter("ralsei_conversation", value);
        }

        public NPC_Ralsei(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            dialogKey = data.Attr("dialogKey", "RALSEI_FREE");
            flagName = data.Attr("flagName", "ralsei_freed");
            enabledByDefault = data.Bool("enabledByDefault", false);
            
            setupSprite();
            setupCollision();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = DZModule.SpriteBank.Create("ralsei"));
            sprite.Play("idle");
            sprite.Color = isFreed ? Color.White : Color.Gray * 0.5f;
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
                isFreed = level.Session.GetFlag("ralsei_freed");
                
                if (level.Session.GetFlag(DONE_TALKING) && !enabledByDefault)
                {
                    talker.Enabled = false;
                    return;
                }
            }

            talker.Enabled = isFreed || enabledByDefault;
            sprite.Color = isFreed ? Color.White : Color.Gray * 0.5f;
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
            
            if (!isFreed)
            {
                yield return Textbox.Say("RALSEI_FREE");
                
                // Free Ralsei animation
                isFreed = true;
                sprite.Color = Color.White;
                
                if (Scene is Level level)
                {
                    level.Session.SetFlag("ralsei_freed");
                    // Add dream feather or other items
                    level.Session.SetFlag("dream_feather_unlocked");
                }
            }
            else
            {
                switch (currentConversation)
                {
                    case 0:
                        yield return Textbox.Say("DZ_CH4_LEGEND_A");
                        yield return Textbox.Say("DZ_CH4_LEGEND_B");
                        yield return Textbox.Say("DZ_CH4_LEGEND_C");
                        yield return Textbox.Say("DZ_CH4_LEGEND_D");
                        yield return Textbox.Say("DZ_CH4_LEGEND_E");
                        yield return Textbox.Say("DZ_CH4_LEGEND_F");
                        yield return Textbox.Say("DZ_CH4_LEGENDOUTRO");
                        break;
                    case 1:
                        yield return Textbox.Say("DZ_CH4_CHARA_2ND_INTRO");
                        break;
                    default:
                        yield return Textbox.Say(dialogKey);
                        break;
                }
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
            
            // Set flag that Ralsei has been talked to
            level.Session.SetFlag(flagName);
            
            if (level.Session.GetFlag(DONE_TALKING))
            {
                talker.Enabled = false;
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Ralsei gentle hovering animation
            if (isFreed)
            {
                Position.Y += (float)Math.Sin(Scene.TimeActive * 1.5) * 0.15f;
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
    }
}

