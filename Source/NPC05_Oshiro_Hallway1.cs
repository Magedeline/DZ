namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC05_Oshiro_Hallway1")]
    public class NPC05_Oshiro_Hallway1 : NPC
    {
        private bool talked;
        protected VertexLight Light;

        public NPC05_Oshiro_Hallway1(Vector2 position)
            : base(position)
        {
            Add(Sprite = new OshiroSprite(-1));
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
            MoveAnim = "move";
            IdleAnim = "idle";
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (base.Session.GetFlag("oshiro_resort_talked_2"))
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (!talked && entity != null && entity.X > base.X - 60f)
            {
                base.Scene.Add(new Cutscenes.CS05_OshiroHallway1(entity, this));
                talked = true;
            }
        }
    }
}




