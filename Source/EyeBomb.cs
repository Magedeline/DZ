namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/EyeBomb")]
    [Tracked]
    public class EyeBomb : Entity
    {
        public static ParticleType P_Explode;

        private Sprite sprite;
        private VertexLight light;
        private BloomPoint bloom;
        private Level level;
        private float detectionRadius;
        private float explosionRadius;
        private float fuseTime;
        private float fuseTimer;
        private bool triggered;
        private bool exploded;
        private Wiggler wiggler;
        private Player targetPlayer;

        public EyeBomb(Vector2 position, float detectionRadius, float explosionRadius, float fuseTime)
            : base(position)
        {
            this.detectionRadius = detectionRadius;
            this.explosionRadius = explosionRadius;
            this.fuseTime = fuseTime;
            
            base.Collider = new Circle(12f);
            
            Add(sprite = AtlasPathHelper.CreateSprite("objects/eyebomb/"));
            sprite.AddLoop("idle", "eye", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            
            Add(light = new VertexLight(Color.Red, 1f, 16, 48));
            Add(bloom = new BloomPoint(0.5f, 16f));
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.1f);
            }));
            
            base.Depth = -50;
        }

        public EyeBomb(EntityData data, Vector2 offset)
            : this(data.Position + offset, 
                   data.Float("detectionRadius", 64f), 
                   data.Float("explosionRadius", 48f), 
                   data.Float("fuseTime", 1.5f))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            
            if (exploded) return;
            
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;
            
            float distanceToPlayer = Vector2.Distance(Position, player.Center);
            
            // Detection - the eye sees the player
            if (!triggered && distanceToPlayer <= detectionRadius)
            {
                triggered = true;
                fuseTimer = fuseTime;
                targetPlayer = player;
                Audio.Play("event:/game/09_core/rising_flame_charge", Position);
                wiggler.Start();
            }
            
            // Track player while triggered
            if (triggered && !exploded)
            {
                fuseTimer -= Engine.DeltaTime;
                
                // Visual feedback - pulsing faster as fuse runs out
                float pulseRate = Calc.ClampedMap(fuseTimer, fuseTime, 0f, 0.3f, 0.05f);
                if (Scene.OnInterval(pulseRate))
                {
                    wiggler.Start();
                    light.Color = Color.Lerp(Color.Red, Color.Orange, Calc.Random.NextFloat());
                }
                
                if (fuseTimer <= 0f)
                {
                    Explode();
                }
            }
        }

        private void Explode()
        {
            exploded = true;
            Collidable = false;
            sprite.Visible = false;
            
            Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
            level.Shake(0.3f);
            
            // Emit explosion particles
            for (int i = 0; i < 20; i++)
            {
                float angle = Calc.Random.NextAngle();
                level.ParticlesFG.Emit(P_Explode, Position, angle);
            }
            
            // Damage player if in explosion radius
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(Position, player.Center);
                if (distanceToPlayer <= explosionRadius)
                {
                    // Launch player away from explosion
                    Vector2 launchDir = (player.Center - Position).SafeNormalize();
                    player.ExplodeLaunch(launchDir * -1f, false);
                }
            }
            
            // Destroy nearby breakables
            foreach (CrystalStaticSpinner spinner in Scene.Tracker.GetEntities<CrystalStaticSpinner>())
            {
                if (Vector2.Distance(Position, spinner.Position) <= explosionRadius)
                {
                    spinner.Destroy();
                }
            }
            
            RemoveSelf();
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
            
            // Debug: draw detection radius
            // Draw.Circle(Position, detectionRadius, Color.Yellow * 0.3f, 16);
            // Draw.Circle(Position, explosionRadius, Color.Red * 0.3f, 16);
        }

        public static void LoadParticles()
        {
            P_Explode = new ParticleType
            {
                Color = Color.Red,
                Color2 = Color.Orange,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                SpeedMin = 60f,
                SpeedMax = 120f,
                SpeedMultiplier = 0.5f,
                DirectionRange = (float)Math.PI * 2f
            };
        }
    }
}
