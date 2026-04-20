using Celeste.Cutscenes;

namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC05_Oshiro_Rooftop")]
    public class NPC05_Oshiro_Rooftop : global::Celeste.NPC
    {
        private const string donetalking = "oshiro_rooftopDoneTalking";

        protected new TalkComponent Talker;

        public NPC05_Oshiro_Rooftop(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = GFX.SpriteBank.Create("maggy_oshiro"));
            Sprite.Play("idle");

            Add(Talker = new TalkComponent(
                new Rectangle(-20, -8, 40, 16),
                new Vector2(0f, -24f),
                ontalk
            ));
            Depth = 100;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level)
            {
                if (level.Session.GetFlag(CS05_OshiroRooftop.Flag))
                {
                    RemoveSelf();
                    return;
                }
            }

            Talker.Enabled = true;
        }

        private void ontalk(global::Celeste.Player player)
        {
            Scene.Add(new CS05_OshiroRooftop(this));
            Talker.Enabled = false;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
    }
}




