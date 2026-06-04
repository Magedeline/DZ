namespace Celeste.Entities
{
    [CustomEntity("MaggyHelper/MaddyCrystalPedestal")]
    [Tracked]
    public class MaddyCrystalPedestal : Solid
    {
        private const string PedestalTexturePath = "characters/theoCrystal/pedestal";
        public Image sprite;
        public bool DroppedMaddy;

        public MaddyCrystalPedestal(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, safe: false)
        {
            Add(sprite = new Image(GFX.Game[PedestalTexturePath]));
            EnableAssistModeChecks = false;
            sprite.JustifyOrigin(0.5f, 1f);
            Depth = 8998;
            Collider.Position = new Vector2(-16f, -64f);
            Collidable = false;
            OnDashCollide = (global::Celeste.Player player, Vector2 direction) =>
            {
                if (Scene is not Level level)
                    return DashCollisionResults.NormalCollision;

                MaddyCrystal entity = level.Tracker.GetEntity<MaddyCrystal>();
                if (entity == null)
                    return DashCollisionResults.NormalCollision;
                entity.OnPedestal = false;
                entity.Speed = new Vector2(0f, -300f);
                DroppedMaddy = true;
                Collidable = false;
                level.Flash(Color.White);
                CelesteGame.Freeze(0.1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                Audio.Play("guid://{1e303df8-ca93-4c8a-83a4-671e311469b0}", entity.Position);
                return DashCollisionResults.Rebound;
            };
            Tag = Tags.TransitionUpdate;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene is not Level level)
                return;

            if (level.Session.GetFlag("foundMaddyInCrystal"))
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
            MaddyCrystal entity = Scene?.Tracker?.GetEntity<MaddyCrystal>();
            if (entity != null && !DroppedMaddy)
            {
                entity.Position = Position + new Vector2(0f, -32f);
                entity.OnPedestal = true;
            }
            base.Update();
        }
    }
}

