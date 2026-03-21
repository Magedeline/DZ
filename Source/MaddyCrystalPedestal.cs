namespace MaggyHelper.Entities
{
    [CustomEntity("MaggyHelper/MaddyCrystalPedestal")]
    [Tracked]
    public class MaddyCrystalPedestal : Solid
    {
        public Image sprite;
        public bool DroppedMaddy;

        public MaddyCrystalPedestal(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, safe: false)
        {
            Add(sprite = new Image(GFX.Game["characters/theoCrystal/pedestal"]));
            EnableAssistModeChecks = false;
            sprite.JustifyOrigin(0.5f, 1f);
            Depth = 8998;
            Collider.Position = new Vector2(-16f, -64f);
            Collidable = false;
            OnDashCollide = (Player player, Vector2 direction) =>
            {
                MaddyCrystal entity = Scene.Tracker.GetEntity<MaddyCrystal>();
                if (entity == null)
                    return DashCollisionResults.NormalCollision;
                entity.OnPedestal = false;
                entity.Speed = new Vector2(0f, -300f);
                DroppedMaddy = true;
                Collidable = false;
                (Scene as Level).Flash(Color.White);
                Celeste.Celeste.Freeze(0.1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_break_free", entity.Position);
                return DashCollisionResults.Rebound;
            };
            Tag = Tags.TransitionUpdate;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.GetFlag("foundMaddyInCrystal"))
            {
                DroppedMaddy = true;
                return;
            }
            MaddyCrystal maddyCrystal = Scene.Entities.FindFirst<MaddyCrystal>();
            if (maddyCrystal != null)
            {
                maddyCrystal.Depth = Depth + 1;
            }
        }

        public override void Update()
        {
            MaddyCrystal entity = Scene.Tracker.GetEntity<MaddyCrystal>();
            if (entity != null && !DroppedMaddy)
            {
                entity.Position = Position + new Vector2(0f, -32f);
                entity.OnPedestal = true;
            }
            base.Update();
        }
    }
}
