using Celeste.Entities;

namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC08_Madeline_Ending_Bandage")]
    public class Npc08MadelineEndingBandage : Entity
    {
        private const string DoneTalkingFlag = "ch8_ending";

        private MadelineBandageDummy dummy;
        private TalkComponent talker;
        private bool isInteracting = false;

        public Npc08MadelineEndingBandage(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetupDummy();
            SetupCollision();
            Depth = 100;
        }

        private void SetupDummy()
        {
            dummy = new MadelineBandageDummy(Vector2.Zero);
            Add(dummy.Sprite);
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

            if (dummy?.Sprite != null && !isInteracting)
            {
                dummy.Sprite.Play("idle");
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public IEnumerator WalkTo(Vector2 target, float speed = 64f)
        {
            dummy.Sprite.Play("walk");

            Vector2 direction = (target - Position).SafeNormalize();

            while (Vector2.Distance(Position, target) > 4f)
            {
                Position += direction * speed * Engine.DeltaTime;
                yield return null;
            }

            Position = target;
            dummy.Sprite.Play("idle");
        }
    }
}
