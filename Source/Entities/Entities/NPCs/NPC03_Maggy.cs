namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC03_Maggy")]
    public class Npc03Maggy : Entity
    {
        private const string donetalking = "magolorDoneTalking";
        private const string hadntmet = "WassupMagolor";

        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;

        private int currentConversation
        {
            get => (Scene as Level)?.Session.GetCounter("magolor") ?? 0;
            set => (Scene as Level)?.Session.SetCounter("magolor", value);
        }

        public Npc03Maggy(EntityData data, Vector2 offset) : base(data.Position + offset)
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
                // Check conversation state
                if (!SaveData.Instance.HasFlag(hadntmet) ||
                    !SaveData.Instance.HasFlag("BadelineJoinKirby"))
                {
                    currentConversation = -1;
                }

                isInteracting = true;
                level.StartCutscene(ontalkend);
                Add(talkRoutine = new Coroutine(talkcoroutine(player)));
            }
        }

        private IEnumerator talkcoroutine(global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;

            if (currentConversation == 1)
            {
                yield return playerapproach(player, 48f);
                yield return Textbox.Say("DZ_CH3_MAGGY_A");
            }
            else if (currentConversation == 2)
            {
                yield return playerapproach(player, 48f);
                yield return Textbox.Say("DZ_CH3_MAGGY_B");
            }
            else if (currentConversation == 3)
            {
                yield return playerapproach(player, 48f);
                yield return Textbox.Say("DZ_CH3_MAGGY_C");
            }
            else if (currentConversation == 4)
            {
                yield return playerapproach(player, 48f);
                yield return Textbox.Say("DZ_CH3_MAGGY_D");
            }
            else
            {
                yield return playerapproach(player, 48f);
                yield return Textbox.Say("DZ_CH3_MAGGY_DEFAULT");
            }

            currentConversation++;
            endcutscene();
        }

        private IEnumerator playerapproach(global::Celeste.Player player, float spacing)
        {
            // Simple approach - move player towards NPC
            Vector2 targetPosition = Position + Vector2.UnitX * spacing;

            while (Vector2.Distance(player.Position, targetPosition) > 4f)
            {
                Vector2 direction = (targetPosition - player.Position).SafeNormalize();
                player.Position += direction * 64f * Engine.DeltaTime;
                yield return null;
            }

            player.Facing = Facings.Left;
            yield return 0.2f;
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

            if (currentConversation >= 4)
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

            // Simple animation logic
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

