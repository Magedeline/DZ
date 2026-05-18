namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// HeatWave - Expanding wave of heat that pushes player
    /// Creates zone-based hazards with visual distortion
    /// Sprite path: effects/heat_wave/
    /// </summary>
    [CustomEntity("MaggyHelper/HeatWave")]
    [Tracked]
    public class HeatWave : Actor
    {
        #region Enums
        public enum WaveState
        {
            Dormant,
            Building,
            Expanding
        }
        #endregion

        #region Properties
        public WaveState State { get; private set; }
        public float MaxRadius { get; private set; }
        public float ExpansionSpeed { get; private set; }
        public float PushForce { get; private set; }
        public float Interval { get; private set; }
        
        private Sprite sprite;
        private float currentRadius;
        private float stateTimer;
        private Level level;
        private List<HeatParticle> particles;
        private VertexLight waveLight;
        private bool isActive;
        #endregion

        #region Constructor
        public HeatWave(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("maxRadius", 150f),
                data.Float("expansionSpeed", 100f),
                data.Float("pushForce", 150f),
                data.Float("interval", 5f),
                data.Bool("isActive", true)
            );
        }

        public HeatWave(Vector2 position, float maxRadius = 150f, float expansionSpeed = 100f,
            float pushForce = 150f, float interval = 5f, bool isActive = true)
            : base(position)
        {
            Initialize(maxRadius, expansionSpeed, pushForce, interval, isActive);
        }

        private void Initialize(float maxRadius, float expansionSpeed, float pushForce, float interval, bool isActive)
        {
            MaxRadius = maxRadius;
            ExpansionSpeed = expansionSpeed;
            PushForce = pushForce;
            Interval = interval;
            
            State = WaveState.Dormant;
            currentRadius = 0f;
            stateTimer = Interval;
            this.isActive = isActive;
            particles = new List<HeatParticle>();
            
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            
            Add(sprite = GFX.SpriteBank.Create("heat_wave"));
            sprite.Play("dormant");
            
            Add(waveLight = new VertexLight(Color.Red, 0.3f, 12, 32));
        }
        #endregion

        #region Public Methods
        public void Trigger()
        {
            if (State != WaveState.Dormant) return;
            
            State = WaveState.Building;
            stateTimer = 1f;
            sprite.Play("building");
            
            Audio.Play("event:/game/gen_crumble_fall", Position);
        }

        public void Activate()
        {
            isActive = true;
        }

        public void Deactivate()
        {
            isActive = false;
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
            
            if (!isActive) return;
            
            switch (State)
            {
                case WaveState.Dormant:
                    stateTimer -= Engine.DeltaTime;
                    
                    if (stateTimer <= 0f)
                    {
                        Trigger();
                    }
                    break;
                    
                case WaveState.Building:
                    stateTimer -= Engine.DeltaTime;
                    
                    // Create building particles
                    if (Scene.OnInterval(0.1f))
                    {
                        CreateHeatParticle();
                    }
                    
                    // Pulse light
                    waveLight.Alpha = 0.3f + (float)Math.Sin(stateTimer * 5f) * 0.2f;
                    
                    if (stateTimer <= 0f)
                    {
                        State = WaveState.Expanding;
                        currentRadius = 0f;
                        sprite.Play("expanding");
                        
                        Audio.Play("event:/game/general/crystalheart_pulse", Position);
                        level?.Shake(0.2f);
                    }
                    break;
                    
                case WaveState.Expanding:
                    // Expand wave
                    currentRadius += ExpansionSpeed * Engine.DeltaTime;
                    
                    // Create expansion particles
                    if (Scene.OnInterval(0.05f))
                    {
                        CreateExpansionParticle();
                    }
                    
                    // Push player
                    var player = Scene.Tracker.GetEntity<Player>();
                    if (player != null)
                    {
                        float distance = Vector2.Distance(Position, player.Position);
                        if (distance < currentRadius && distance > currentRadius - 20f)
                        {
                            Vector2 pushDir = (player.Position - Position).SafeNormalize();
                            player.Speed += pushDir * PushForce * Engine.DeltaTime;
                        }
                    }
                    
                    // Check if reached max radius
                    if (currentRadius >= MaxRadius)
                    {
                        State = WaveState.Dormant;
                        currentRadius = 0f;
                        stateTimer = Interval;
                        sprite.Play("dormant");
                    }
                    break;
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        private void CreateHeatParticle()
        {
            var particle = new HeatParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f),
                new Vector2(Calc.Random.NextFloat() * 40f - 20f, -Calc.Random.NextFloat() * 40f)
            );
            particles.Add(particle);
            Scene.Add(particle);
        }

        private void CreateExpansionParticle()
        {
            float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
            Vector2 pos = Position + Calc.AngleToVector(angle, currentRadius);
            
            var particle = new HeatParticle(
                pos,
                Calc.AngleToVector(angle, 50f)
            );
            particles.Add(particle);
            Scene.Add(particle);
        }

        public override void Render()
        {
            // Draw wave expansion
            if (State == WaveState.Expanding)
            {
                Draw.Circle(Position, currentRadius, Color.Red * 0.2f, 16);
            }
            
            // Draw building indicator
            if (State == WaveState.Building)
            {
                Draw.Circle(Position, 20f, Color.Red * 0.3f, 8);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// HeatParticle - Particle for heat wave effects
    /// </summary>
    public class HeatParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public HeatParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.6f - 0.3f) + 0.3f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
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
            Draw.Circle(Position, 5f * scale, Color.Red * (alpha * 0.5f), 4);
        }
    }

    /// <summary>
    /// HeatZone - Constant heat zone that affects player
    /// </summary>
    [CustomEntity("MaggyHelper/HeatZone")]
    public class HeatZone : Trigger
    {
        private float heatIntensity;
        private float pushForce;

        public HeatZone(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            heatIntensity = data.Float("heatIntensity", 1f);
            pushForce = data.Float("pushForce", 50f);
        }

        public void OnEnter(Player player)
        {
            // Apply heat effect
            player.Speed += new Vector2(Calc.Random.NextFloat() * pushForce - pushForce / 2, Calc.Random.NextFloat() * pushForce - pushForce / 2) * Engine.DeltaTime;
        }
    }
}
