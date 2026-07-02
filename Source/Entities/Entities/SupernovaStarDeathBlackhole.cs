namespace Celeste.Entities.Projectiles
{
    [CustomEntity("DZ/SupernovaStarDeathBlackhole")]
    public class SupernovaStarDeathBlackhole : Entity
    {
        private Sprite sprite;
        private float pullRadius;
        private float pullStrength;
        private float damageRadius;
        private float lifetime;
        private float growthRate;
        private ParticleType blackholeParticle;
        private float particleTimer;

        /// <summary>
        /// Constructor for Everest entity loader (map data).
        /// </summary>
        public SupernovaStarDeathBlackhole(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Float("pullRadius", 120f), data.Float("lifetime", 5f))
        {
        }

        public SupernovaStarDeathBlackhole(Vector2 position, float pullRadius = 120f, float lifetime = 5f)
            : base(position)
        {
            this.pullRadius = pullRadius;
            this.pullStrength = 200f;
            this.damageRadius = 32f;
            this.lifetime = lifetime;
            this.growthRate = 1.5f;

            Depth = -1000000;
            Collider = new Circle(damageRadius);
            
            // Setup sprite
            Add(sprite = new Sprite(GFX.Game, "objects/DZ/DZ/DZ/blackhole/"));
            sprite.AddLoop("idle", "blackhole", 0.08f);
            sprite.CenterOrigin();
            sprite.Play("idle");
            sprite.Scale = Vector2.Zero;

            // Setup particles
            blackholeParticle = new ParticleType
            {
                Color = Color.Purple,
                Color2 = Color.DarkViolet,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Linear,
                Size = 1f,
                SizeRange = 2f,
                LifeMin = 0.5f,
                LifeMax = 1.5f,
                SpeedMin = 10f,
                SpeedMax = 40f,
                Acceleration = Vector2.Zero
            };

            Add(new Coroutine(LifetimeRoutine()));
        }

        private IEnumerator LifetimeRoutine()
        {
            // Growth phase
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                sprite.Scale = Vector2.One * Ease.CubeOut(elapsed / 0.5f) * growthRate;
                elapsed += Engine.DeltaTime;
                yield return null;
            }

            sprite.Scale = Vector2.One * growthRate;

            // Active phase
            yield return lifetime - 1f;

            // Collapse phase
            elapsed = 0f;
            while (elapsed < 0.5f)
            {
                sprite.Scale = Vector2.One * growthRate * (1f - Ease.CubeIn(elapsed / 0.5f));
                elapsed += Engine.DeltaTime;
                yield return null;
            }

            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();

            // Pull player towards center
            CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();
            if (player != null)
            {
                float distance = Vector2.Distance(Position, player.Center);
                if (distance < pullRadius)
                {
                    Vector2 pullDirection = (Position - player.Center).SafeNormalize();
                    float pullForce = pullStrength * (1f - distance / pullRadius);
                    player.Speed += pullDirection * pullForce * Engine.DeltaTime;
                }

                // Damage player if too close
                if (distance < damageRadius)
                {
                    player.Die((player.Center - Position).SafeNormalize());
                }
            }

            // Particle effects
            particleTimer += Engine.DeltaTime;
            if (particleTimer > 0.05f)
            {
                particleTimer = 0f;
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                Vector2 particlePos = Position + Calc.AngleToVector(angle, pullRadius * 0.8f);
                Vector2 particleSpeed = (Position - particlePos).SafeNormalize() * 50f;
                SceneAs<Level>().ParticlesFG.Emit(blackholeParticle, particlePos, angle);
            }

            // Distortion effect (optional - requires custom shader)
            DrawDistortion();
        }

        private void DrawDistortion()
        {
            // Add visual distortion around blackhole
            // This would require custom rendering code
        }

        public override void Render()
        {
            sprite.DrawOutline(Color.Black, 2);
            base.Render();
        }
    }
}
