using Celeste.Cutscenes;
using ModOshiroSprite = DZ.MaggyOshiroSprite;

namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC05_Oshiro_Hallway2")]
    public class NPC05_Oshiro_Hallway2 : NPC
    {
        private bool talked;
        protected new VertexLight Light;

        public NPC05_Oshiro_Hallway2(Vector2 position)
            : base(position)
        {
            Add(Sprite = new ModOshiroSprite(-1));
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
            MoveAnim = "move";
            IdleAnim = "idle";
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (base.Session.GetFlag("oshiro_resort_talked_3"))
            {
                RemoveSelf();
            }
            else
            {
                base.Session.LightingAlphaAdd = 0.15f;
            }
        }

        public override void Update()
        {
            base.Update();
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (!talked && entity != null && entity.X > base.X - 60f)
            {
                base.Scene.Add(new CS05_OshiroHallway2(entity, this));
                talked = true;
            }
        }
    }
}
