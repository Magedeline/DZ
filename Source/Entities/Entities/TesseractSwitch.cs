namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/TesseractSwitch")]
    public class TesseractSwitch : Solid
    {
        private const string DefaultSpritePath = "objects/DZ/DZ/tesseract_temple/dashButton";
        private const string ActivatedSpritePath = "objects/DZ/DZ/tesseract_temple/dashButtonMirror";

        private readonly bool allGates;
        private readonly bool ceiling;
        private readonly EntityID entityID;
        private readonly bool persistent;
        private readonly string switchFlag;

        private Image sprite;
        private bool activated;

        public TesseractSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset, 16f, 16f, safe: true)
        {
            allGates = data.Bool("allGates");
            ceiling = data.Bool("ceiling");
            persistent = data.Bool("persistent");
            entityID = new EntityID(data.Level.Name, data.ID);

            string configuredFlag = data.Attr("targetFlag", string.Empty);
            switchFlag = string.IsNullOrWhiteSpace(configuredFlag)
                ? $"tesseract_switch_{entityID.Level}_{entityID.ID}"
                : configuredFlag;

            Depth = -1000;
            OnDashCollide = OnDashed;

            Add(sprite = new Image(GFX.Game[DefaultSpritePath + "00"]));
            sprite.CenterOrigin();
            sprite.Position = new Vector2(8f, ceiling ? 0f : 8f);
            sprite.Rotation = ceiling ? -MathHelper.PiOver2 : MathHelper.PiOver2;

            UpdateSprite();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            Level level = SceneAs<Level>();
            if (level != null && level.Session.GetFlag(switchFlag))
            {
                activated = true;
                UpdateSprite();
            }
        }

        private DashCollisionResults OnDashed(global::Celeste.Player player, Vector2 direction)
        {
            if (!activated)
            {
                Activate();
            }

            return DashCollisionResults.Rebound;
        }

        private void Activate()
        {
            Level level = SceneAs<Level>();
            if (level == null)
            {
                return;
            }

            activated = true;
            UpdateSprite();

            if (persistent || !string.IsNullOrWhiteSpace(switchFlag))
            {
                level.Session.SetFlag(switchFlag, true);
            }

            Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
            if (ParticleTypes.SparkyDust != null)
            {
                level.Particles.Emit(ParticleTypes.SparkyDust, 12, Position, Vector2.One * 4f);
            }
            level.Shake(0.2f);

            if (allGates)
            {
                foreach (TesseractMirrorGateway gateway in GetGateways())
                {
                    gateway.TryActivateSide(GetPortalSide(gateway));
                }
            }
            else
            {
                TesseractMirrorGateway nearestPortal = FindNearestPortal();
                nearestPortal?.TryActivateSide(GetPortalSide(nearestPortal));
            }
        }

        private TesseractMirrorGateway FindNearestPortal()
        {
            TesseractMirrorGateway nearestPortal = null;
            float nearestDistanceSq = float.MaxValue;

            foreach (TesseractMirrorGateway gateway in GetGateways())
            {
                float distanceSq = Vector2.DistanceSquared(Position, gateway.Position);
                if (distanceSq < nearestDistanceSq)
                {
                    nearestDistanceSq = distanceSq;
                    nearestPortal = gateway;
                }
            }

            return nearestPortal;
        }

        private IEnumerable<TesseractMirrorGateway> GetGateways()
        {
            if (Scene == null)
            {
                yield break;
            }

            foreach (Entity entity in Scene.Entities)
            {
                if (entity is TesseractMirrorGateway gateway)
                {
                    yield return gateway;
                }
            }
        }

        private int GetPortalSide(TesseractMirrorGateway gateway)
        {
            return Position.X <= gateway.Position.X ? -1 : 1;
        }

        private void UpdateSprite()
        {
            string spritePath = activated ? ActivatedSpritePath : DefaultSpritePath;
            sprite.Texture = GFX.Game[spritePath + "00"];
        }
    }
}





