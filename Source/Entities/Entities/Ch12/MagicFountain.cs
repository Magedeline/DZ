namespace DZ
{
    /// <summary>
    /// MagicFountain - Central fountain with magical water effects
    /// Main setpiece that activates tower climbing sequence
    /// Sprite path: objects/DZ/DZ/magic_fountain/
    /// </summary>
    [CustomEntity("DZ/MagicFountain")]
    [Tracked]
    public class MagicFountain : Actor
    {
        #region Enums
        public enum FountainState
        {
            Dormant,
            Awakening,
            Active,
            Surging,
            Overflowing,
            Complete
        }
        #endregion

        #region Properties
        public FountainState State { get; private set; }
        public float WaterHeight { get; private set; }
        public float MaxWaterHeight { get; private set; }
        public float SurgeInterval { get; private set; }
        public bool IsActivated => State >= FountainState.Active;
        
        private Sprite sprite;
        private VertexLight fountainLight;
        private float stateTimer;
        private float surgeTimer;
        private Level level;
        private List<FountainWaterParticle> waterParticles;
        private List<FountainMist> mistParticles;
        private bool hasActivated;
        private float currentHeight;
        private float targetHeight;
        #endregion

        #region Constructor
        public MagicFountain(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("maxWaterHeight", 200f),
                data.Float("surgeInterval", 8f)
            );
        }

        public MagicFountain(Vector2 position, float maxWaterHeight = 200f, float surgeInterval = 8f)
            : base(position)
        {
            Initialize(maxWaterHeight, surgeInterval);
        }

        private void Initialize(float maxWaterHeight, float surgeInterval)
        {
            MaxWaterHeight = maxWaterHeight;
            SurgeInterval = surgeInterval;
            
            State = FountainState.Dormant;
            WaterHeight = 0f;
            currentHeight = 0f;
            targetHeight = 0f;
            stateTimer = 0f;
            surgeTimer = 0f;
            hasActivated = false;
            waterParticles = new List<FountainWaterParticle>();
            mistParticles = new List<FountainMist>();
            
            Collider = new Hitbox(80f, 40f, -40f, -40f);
            
            Add(sprite = GFX.SpriteBank.Create("magic_fountain"));
            sprite.Play("dormant");
            
            Add(fountainLight = new VertexLight(Color.Cyan, 0.2f, 24, 64));
        }
        #endregion

        #region Public Methods
        public void Activate()
        {
            if (hasActivated) return;
            
            hasActivated = true;
            State = FountainState.Awakening;
            sprite.Play("awakening");
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.3f);
            
            Add(new Coroutine(ActivateRoutine()));
        }

        public void Surge()
        {
            if (State != FountainState.Active) return;
            
            State = FountainState.Surging;
            targetHeight = MaxWaterHeight * 1.5f;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.4f);
            level?.Flash(Color.Cyan * 0.3f);
            
            Add(new Coroutine(SurgeRoutine()));
        }

        public void Overflow()
        {
            State = FountainState.Overflowing;
            sprite.Play("overflowing");
            
            level?.Shake(0.5f);
            level?.Flash(Color.White * 0.4f);
            
            Add(new Coroutine(OverflowRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator ActivateRoutine()
        {
            // Awakening animation
            for (int i = 0; i < 20; i++)
            {
                CreateWaterParticle();
                CreateMist();
                fountainLight.Alpha = i / 20f * 0.8f;
                yield return 0.05f;
            }
            
            State = FountainState.Active;
            sprite.Play("active");
            targetHeight = MaxWaterHeight * 0.5f;
            
            // Set flag
            level?.Session.SetFlag("magic_fountain_activated", true);
        }

        private IEnumerator SurgeRoutine()
        {
            float surgeDuration = 3f;
            stateTimer = surgeDuration;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                
                // Extra particles during surge
                for (int i = 0; i < 3; i++)
                {
                    CreateWaterParticle();
                }
                
                CreateMist();
                
                yield return null;
            }
            
            State = FountainState.Active;
            targetHeight = MaxWaterHeight * 0.5f;
        }

        private IEnumerator OverflowRoutine()
        {
            // Massive water burst
            for (int i = 0; i < 50; i++)
            {
                CreateWaterParticle();
                CreateMist();
                yield return 0.02f;
            }
            
            yield return 2f;
            
            State = FountainState.Complete;
            sprite.Play("complete");
            
            level?.Session.SetFlag("magic_fountain_complete", true);
        }

        private void CreateWaterParticle()
        {
            float angle = -MathHelper.PiOver2 + Calc.Random.NextFloat() * 0.8f - 0.4f;
            float speed = Calc.Random.NextFloat() * 100f + 50f;
            
            var particle = new FountainWaterParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 40f - 20f, -currentHeight / 2),
                Calc.AngleToVector(angle, speed)
            );
            waterParticles.Add(particle);
            Scene.Add(particle);
        }

        private void CreateMist()
        {
            var mist = new FountainMist(
                Position + new Vector2(Calc.Random.NextFloat() * 60f - 30f, -currentHeight / 2),
                new Vector2(Calc.Random.NextFloat() * 20f - 10f, -Calc.Random.NextFloat() * 20f)
            );
            mistParticles.Add(mist);
            Scene.Add(mist);
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
            
            // Smooth height transition
            currentHeight = Calc.Approach(currentHeight, targetHeight, 100f * Engine.DeltaTime);
            WaterHeight = currentHeight;
            
            if (State == FountainState.Active)
            {
                // Auto surge cycle
                surgeTimer += Engine.DeltaTime;
                if (surgeTimer >= SurgeInterval)
                {
                    surgeTimer = 0f;
                    Surge();
                }
                
                // Continuous particles
                if (Scene.OnInterval(0.05f))
                {
                    CreateWaterParticle();
                }
                
                if (Scene.OnInterval(0.2f))
                {
                    CreateMist();
                }
            }
            
            waterParticles.RemoveAll(p => p == null || p.Scene == null);
            mistParticles.RemoveAll(m => m == null || m.Scene == null);
        }

        public override void Render()
        {
            // Draw water column
            if (currentHeight > 0f)
            {
                Draw.Rect(Position.X - 20, Position.Y - currentHeight, 40, currentHeight, Color.Cyan * 0.3f);
                
                // Water surface
                Draw.Circle(Position - Vector2.UnitY * currentHeight, 20f, Color.Cyan * 0.5f, 12);
            }
            
            // Draw fountain base
            Draw.Rect(Position.X - 40, Position.Y - 10, 80, 20, Color.Gray);
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// FountainWaterParticle - Water particle from fountain
    /// </summary>
    public class FountainWaterParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public FountainWaterParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (1.5f - 0.5f) + 0.5f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 150f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 4f, Color.Cyan * (alpha * 0.6f), 4);
        }
    }

    /// <summary>
    /// FountainMist - Mist particle from fountain
    /// </summary>
    public class FountainMist : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public FountainMist(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (2f - 1f) + 1f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.5f - 0.5f) + 0.5f;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.98f;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 12f * scale, Color.White * (alpha * 0.2f), 8);
        }
    }
}

