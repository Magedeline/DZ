namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// CorruptionCrystal - Dark crystal that corrupts nearby entities
    /// Spreads corruption and transforms areas
    /// Sprite path: objects/corruption_crystal/
    /// </summary>
    [CustomEntity("MaggyHelper/CorruptionCrystal")]
    [Tracked]
    public class CorruptionCrystal : Actor
    {
        #region Enums
        public enum CrystalState
        {
            Dormant,
            Awakened,
            Spreading,
            Purifying,
            Destroyed
        }
        #endregion

        #region Properties
        public CrystalState State { get; private set; }
        public int Health { get; private set; }
        public float CorruptionRadius { get; private set; }
        public float SpreadSpeed { get; private set; }
        
        private Sprite sprite;
        private float corruptionTimer;
        private Level level;
        private List<CorruptionParticle> particles;
        private VertexLight crystalLight;
        private Color corruptionColor;
        #endregion

        #region Constructor
        public CorruptionCrystal(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Int("health", 3), data.Float("corruptionRadius", 100f), data.Float("spreadSpeed", 20f));
        }

        public CorruptionCrystal(Vector2 position, int health = 3, float corruptionRadius = 100f, float spreadSpeed = 20f)
            : base(position)
        {
            Initialize(health, corruptionRadius, spreadSpeed);
        }

        private void Initialize(int health, float corruptionRadius, float spreadSpeed)
        {
            Health = health;
            CorruptionRadius = corruptionRadius;
            SpreadSpeed = spreadSpeed;
            
            State = CrystalState.Dormant;
            corruptionTimer = 0f;
            particles = new List<CorruptionParticle>();
            corruptionColor = Color.DarkRed;
            
            Collider = new Hitbox(32f, 48f, -16f, -48f);
            
            Add(sprite = GFX.SpriteBank.Create("corruption_crystal"));
            sprite.Play("dormant");
            
            Add(crystalLight = new VertexLight(corruptionColor, 0.4f, 12, 32));
        }
        #endregion

        #region Public Methods
        public void Awaken()
        {
            if (State != CrystalState.Dormant) return;
            
            State = CrystalState.Awakened;
            sprite.Play("awakened");
            crystalLight.Alpha = 0.8f;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.Shake(0.3f);
            
            Add(new Coroutine(AwakenRoutine()));
        }

        public void Purify()
        {
            if (State == CrystalState.Destroyed) return;
            
            State = CrystalState.Purifying;
            sprite.Play("purifying");
            
            Add(new Coroutine(PurifyRoutine()));
        }

        public void Damage(int amount)
        {
            Health -= amount;
            
            if (Health <= 0)
            {
                Purify();
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator AwakenRoutine()
        {
            for (int i = 0; i < 15; i++)
            {
                CreateCorruptionParticle();
                yield return 0.05f;
            }
            
            State = CrystalState.Spreading;
        }

        private IEnumerator PurifyRoutine()
        {
            for (int i = 0; i < 20; i++)
            {
                CreatePurificationParticle();
                yield return 0.03f;
            }
            
            level?.Flash(Color.White * 0.4f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            State = CrystalState.Destroyed;
            yield return 0.3f;
            RemoveSelf();
        }

        private void CreateCorruptionParticle()
        {
            var particle = new CorruptionParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 24f - 12f, Calc.Random.NextFloat() * 24f - 12f),
                new Vector2(Calc.Random.NextFloat() * 60f - 30f, -Calc.Random.NextFloat() * 40f),
                corruptionColor
            );
            particles.Add(particle);
            Scene.Add(particle);
        }

        private void CreatePurificationParticle()
        {
            var particle = new CorruptionParticle(
                Position,
                new Vector2(Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 100f - 50f),
                Color.White
            );
            particles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            
            if (State == CrystalState.Spreading)
            {
                corruptionTimer += Engine.DeltaTime;
                
                if (Scene.OnInterval(0.1f))
                {
                    CreateCorruptionParticle();
                }
                
                // Spread corruption
                if (Scene.OnInterval(2f))
                {
                    level?.Session.SetFlag("corruption_spread", true);
                }
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw corruption radius
            if (State == CrystalState.Spreading)
            {
                Draw.Circle(Position, CorruptionRadius, corruptionColor * 0.15f, 16);
            }
            
            base.Render();
        }
        #endregion
    }

    public class CorruptionParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public CorruptionParticle(Vector2 position, Vector2 velocity, Color color)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            maxLifetime = Calc.Random.NextFloat() * (0.6f - 0.3f) + 0.3f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.95f;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 4f, color * (alpha * 0.5f), 4);
        }
    }
}
