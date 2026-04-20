namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC05_Oshiro_Breakdown")]
    public class NPC05_Oshiro_Breakdown : NPC
    {
        private bool talked;
        protected VertexLight Light;

        public NPC05_Oshiro_Breakdown(Vector2 position)
            : base(position)
        {
            Add(Sprite = new OshiroSprite(1));
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
            MoveAnim = "move";
            IdleAnim = "idle";
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (base.Session.GetFlag("oshiro_breakdown"))
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (!talked && entity != null && ((entity.X <= (float)(Level.Bounds.Left + 370) && entity.OnSafeGround && entity.Y < (float)Level.Bounds.Center.Y) || entity.X <= (float)(Level.Bounds.Left + 320)))
            {
                base.Scene.Add(new Cutscenes.CS05_OshiroBreakdown(entity, this));
                talked = true;
            }
        }
    }
}




