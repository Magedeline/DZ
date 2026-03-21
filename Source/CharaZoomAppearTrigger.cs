namespace MaggyHelper.Triggers
{
    [CustomEntity("MaggyHelper/CharaZoomAppearTrigger")]
    [Tracked]
    public class CharaZoomAppearTrigger : Trigger
    {
        private readonly float targetZoom;
        private readonly float zoomSpeed;
        private readonly bool onlyOnce;
        private readonly bool affectChara;
        private readonly bool affectBadeline;
        private readonly bool showOnEnter;
        private readonly bool hideOnLeave;
        private readonly bool resetZoomOnLeave;

        private bool triggered;

        public CharaZoomAppearTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            targetZoom = data.Float("targetZoom", 2f);
            zoomSpeed = data.Float("zoomSpeed", 2f);
            onlyOnce = data.Bool("onlyOnce", true);
            affectChara = data.Bool("affectChara", true);
            affectBadeline = data.Bool("affectBadeline", true);
            showOnEnter = data.Bool("showOnEnter", true);
            hideOnLeave = data.Bool("hideOnLeave", true);
            resetZoomOnLeave = data.Bool("resetZoomOnLeave", true);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggered && onlyOnce)
            {
                return;
            }

            triggered = true;

            if (showOnEnter)
            {
                setTargetsShown(true);
            }
        }

        public override void OnStay(global::Celeste.Player player)
        {
            base.OnStay(player);

            Level level = SceneAs<Level>();
            if (level != null)
            {
                level.Camera.Zoom = Calc.Approach(level.Camera.Zoom, targetZoom, zoomSpeed * Engine.DeltaTime);
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);

            if (hideOnLeave)
            {
                setTargetsShown(false);
            }

            if (resetZoomOnLeave)
            {
                Level level = SceneAs<Level>();
                if (level != null)
                {
                    level.Camera.Zoom = 1f;
                }
            }
        }

        private void setTargetsShown(bool shown)
        {
            if (affectChara)
            {
                foreach (Entity entity in Scene.Tracker.GetEntities<global::MaggyHelper.NPCs.Npc10Chara>())
                {
                    setEntityShown(entity, shown);
                }

                foreach (Entity entity in Scene.Tracker.GetEntities<global::MaggyHelper.NPCs.NPC_Chara>())
                {
                    setEntityShown(entity, shown);
                }
            }

            if (affectBadeline)
            {
                foreach (Entity entity in Scene.Tracker.GetEntities<global::MaggyHelper.NPCs.Npc10Badeline>())
                {
                    setEntityShown(entity, shown);
                }
            }
        }

        private static void setEntityShown(Entity entity, bool shown)
        {
            entity.Visible = shown;
            entity.Active = shown;
            entity.Collidable = shown;
        }
    }
}
