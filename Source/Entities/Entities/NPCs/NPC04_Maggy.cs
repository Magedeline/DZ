namespace Celeste.NPCs
{
    [Tracked]
    [CustomEntity(ids: "DZ/NPC04_Magolor")]
    public class NPC04_Magolor : Entity
    {
        private const string donetalking = "magolor02DoneTalking";

        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;

        private int currentConversation
        {
            get => (Scene as Level)?.Session.GetCounter("magolor02") ?? 0;
            set => (Scene as Level)?.Session.SetCounter("magolor02", value);
        }

        public NPC04_Magolor(EntityData data, Vector2 offset) : base(data.Position + offset)
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
                    yield return Textbox.Say("DZ_CH1_MAGOLOR_INTRO");
                    break;
                case 1:
                    yield return Textbox.Say("DZ_CH1_MAGOLOR_A");
                    break;
                case 2:
                    yield return Textbox.Say("DZ_CH1_MAGOLOR_B");
                    break;
                case 3:
                    yield return Textbox.Say("DZ_CH4_MAGOLOR_AND_THEO", onMagolorFlyoff, onTheoWalkoff);
                    break;
                default:
                    yield return Textbox.Say("DZ_CH1_MAGOLOR_DEFAULT");
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

        private IEnumerator onMagolorFlyoff()
        {
            sprite.Play("run");
            sprite.Scale.X = 1f;
            float startX = Position.X;
            float startY = Position.Y;
            float targetX = (Scene as Level).Bounds.Right + 64;
            float duration = 1.5f;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float t = Ease.CubeIn(Math.Min(timer / duration, 1f));
                Position.X = MathHelper.Lerp(startX, targetX, t);
                Position.Y = startY - (t * 40f);
                yield return null;
            }
            RemoveSelf();
        }

        public IEnumerator FlyOffRight()
        {
            yield return onMagolorFlyoff();
        }

        private IEnumerator onTheoWalkoff()
        {
            var theo = Scene.Tracker.GetEntity<NPC04_Theo>();
            if (theo != null)
            {
                yield return theo.WalkOffLeft();
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



