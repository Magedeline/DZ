namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC08_Theo_Ending")]
    public class Npc08TheoEnding : Entity
    {
        private const string DoneTalkingFlag = "ch8_ending";

        private Sprite sprite;
        private TalkComponent talker;
        private bool isInteracting = false;

        public Npc08TheoEnding(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetupSprite();
            SetupCollision();
            Depth = 100;
        }

        private void SetupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("theo"));
            sprite.Play("idle");
        }

        private void SetupCollision()
        {
            Add(talker = new TalkComponent(
                new Rectangle(-20, -8, 40, 16),
                new Vector2(0f, -24f),
                OnTalk
            ));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level)
            {
                talker.Enabled = !level.Session.GetFlag(DoneTalkingFlag);
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (isInteracting) return;

            if (Scene is Level level)
            {
                isInteracting = true;
                level.StartCutscene(OnTalkEnd);
            }
        }

        private void OnTalkEnd(Level level)
        {
            isInteracting = false;
            level.Session.SetFlag(DoneTalkingFlag, true);
            talker.Enabled = false;

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
