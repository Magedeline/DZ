namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/UwUmper")]
    [Tracked]
    public class UwUmper : Entity
    {
        public static ParticleType P_Ambience;
        public static ParticleType P_Launch;

        private Sprite sprite;
        private VertexLight light;
        private Wiggler hitWiggler;
        private Vector2 hitDir;
        private float respawnTimer;
        private bool fireMode;

        public UwUmper(Vector2 position, bool fireMode)
            : base(position)
        {
            this.fireMode = fireMode;
            base.Collider = new Circle(12f);
            Add(new PlayerCollider(player => OnPlayer(player)));
            
            Add(sprite = new Sprite(GFX.Game, "objects/MaggyHelper/uwumper/"));
            sprite.AddLoop("idle", "Idle", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            
            Add(light = new VertexLight(fireMode ? Color.HotPink : Color.Pink, 1f, 16, 32));
            Add(hitWiggler = Wiggler.Create(1.2f, 2f, delegate (float v)
            {
                sprite.Rotation = v * 20f * ((float)Math.PI / 180f);
            }));
            
            Add(new BloomPoint(0.5f, 16f));
            base.Depth = -8500;
        }

        public UwUmper(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("fireMode", false))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    light.Visible = true;
                    sprite.Visible = true;
                    Collidable = true;
                    Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                }
            }
            else if (base.Scene.OnInterval(0.05f))
            {
                float angle = Calc.Random.NextAngle();
                ParticleType particleType = P_Ambience;
                float num = 1f;
                SceneAs<Level>().Particles.Emit(particleType, 1, base.Center + Calc.AngleToVector(angle, 10f), Vector2.One * 2f, particleType.Color, angle + (float)Math.PI / 2f * num);
            }
            if (hitWiggler.Active)
            {
                sprite.Position = hitDir * hitWiggler.Value * 8f;
            }
        }

        private void OnPlayer(Celeste.Player player)
        {
            if (respawnTimer <= 0f)
            {
                Audio.Play(fireMode ? "event:/game/09_core/pinballbumper_hit" : "event:/game/06_reflection/pinballbumper_hit", Position);
                respawnTimer = 0.6f;
                Vector2 direction = (player.Center - base.Center).SafeNormalize();
                hitDir = direction;
                hitWiggler.Start();
                
                player.ExplodeLaunch(base.Center, false);
                
                sprite.Visible = false;
                light.Visible = false;
                Collidable = false;
                
                SceneAs<Level>().DirectionalShake(direction, 0.15f);
                SceneAs<Level>().Particles.Emit(P_Launch, 12, base.Center + direction * 12f, Vector2.One * 3f, direction.Angle());
                
                // Heart particle effect
                for (int i = 0; i < 5; i++)
                {
                    SceneAs<Level>().Particles.Emit(P_Launch, base.Center, Calc.Random.NextAngle());
                }
            }
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }

        public static void LoadParticles()
        {
            P_Ambience = new ParticleType
            {
                Color = Color.HotPink,
                Color2 = Color.Pink,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                SpeedMin = 10f,
                SpeedMax = 20f
            };
            P_Launch = new ParticleType
            {
                Color = Color.HotPink,
                Color2 = Color.LightPink,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                SpeedMin = 80f,
                SpeedMax = 120f,
                SpeedMultiplier = 0.3f
            };
        }
    }
}
