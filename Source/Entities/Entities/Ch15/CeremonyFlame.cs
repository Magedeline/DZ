namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// CeremonyFlame - Eternal fire that spreads across platforms
    /// Creates dynamic fire hazards that grow and can be extinguished
    /// Sprite path: objects/DZ/DZ/ceremony_flame/
    /// </summary>
    [CustomEntity("DZ/CeremonyFlame")]
    [Tracked]
    public class CeremonyFlame : Actor
    {
        #region Enums
        public enum FlameState
        {
            Dormant,
            Igniting,
            Burning,
            Spreading,
            Extinguishing,
            Extinguished
        }
        #endregion

        #region Properties
        public FlameState State { get; private set; }
        public float SpreadSpeed { get; private set; }
        public float MaxSpreadDistance { get; private set; }
        public float BurnIntensity { get; private set; }
        public bool CanSpread { get; private set; }
        public bool IsBurning => State == FlameState.Burning || State == FlameState.Spreading;
        
        private Sprite sprite;
        private float spreadDistance;
        private float intensity;
        private float flickerTimer;
        private List<CeremonyFlame> spreadFlames;
        private Level level;
        private VertexLight flameLight;
        private List<FlameParticle> particles;
        private float spreadCooldown;
        private bool isSource;
        #endregion

        #region Constructor
        public CeremonyFlame(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Bool("isSource", true),
                data.Float("spreadSpeed", 20f),
                data.Float("maxSpreadDistance", 200f),
                data.Bool("canSpread", true)
            );
        }

        public CeremonyFlame(Vector2 position, bool isSource = true, float spreadSpeed = 20f,
            float maxSpreadDistance = 200f, bool canSpread = true)
            : base(position)
        {
            Initialize(isSource, spreadSpeed, maxSpreadDistance, canSpread);
        }

        private void Initialize(bool isSource, float spreadSpeed, float maxSpreadDistance, bool canSpread)
        {
            this.isSource = isSource;
            SpreadSpeed = spreadSpeed;
            MaxSpreadDistance = maxSpreadDistance;
            CanSpread = canSpread;
            
            State = isSource ? FlameState.Burning : FlameState.Dormant;
            spreadDistance = 0f;
            intensity = isSource ? 1f : 0f;
            flickerTimer = 0f;
            spreadCooldown = 0f;
            spreadFlames = new List<CeremonyFlame>();
            particles = new List<FlameParticle>();
            
            // Setup collider
            Collider = new Hitbox(24f, 48f, -12f, -48f);
            
            // Setup sprite
            Add(sprite = DZModule.SpriteBank.Create("ceremony_flame"));
            sprite.Play(isSource ? "burn" : "dormant");
            
            // Add flame light
            Add(flameLight = new VertexLight(Color.Orange, isSource ? 0.7f : 0f, 16, 48));
        }
        #endregion

        #region Public Methods
        public void Ignite()
        {
            if (State != FlameState.Dormant) return;
            
            State = FlameState.Igniting;
            sprite.Play("ignite");
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            Add(new Coroutine(IgniteRoutine()));
        }

        public void Extinguish()
        {
            if (State == FlameState.Extinguished || State == FlameState.Extinguishing) return;
            
            State = FlameState.Extinguishing;
            sprite.Play("extinguish");
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            // Extinguish spread flames
            foreach (var flame in spreadFlames)
            {
                flame?.Extinguish();
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator IgniteRoutine()
        {
            // Ignition animation
            for (int i = 0; i < 8; i++)
            {
                CreateFlameParticle();
                yield return 0.05f;
            }
            
            intensity = 1f;
            flameLight.Alpha = 0.7f;
            
            State = FlameState.Burning;
            sprite.Play("burn");
        }

        private void Spread()
        {
            if (!CanSpread || spreadDistance >= MaxSpreadDistance) return;
            if (spreadCooldown > 0f) return;
            
            // Check for adjacent surfaces to spread to
            Vector2[] spreadDirections = { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
            
            foreach (var dir in spreadDirections)
            {
                Vector2 checkPos = Position + dir * 24f;
                
                // Check if there's a surface to spread to
                if (Scene.CollideCheck<Solid>(checkPos))
                {
                    // Create new flame on surface
                    var newFlame = new CeremonyFlame(
                        checkPos - dir * 12f,
                        false,
                        SpreadSpeed,
                        MaxSpreadDistance - spreadDistance - 24f,
                        CanSpread
                    );
                    newFlame.Ignite();
                    Scene.Add(newFlame);
                    spreadFlames.Add(newFlame);
                    
                    spreadDistance += 24f;
                    spreadCooldown = 1f;
                    
                    Audio.Play("event:/game/general/diamond_get", Position);
                }
            }
        }

        private void CreateFlameParticle()
        {
            var particle = new FlameParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 16f - 8f, Calc.Random.NextFloat() * 16f - 8f),
                new Vector2(Calc.Random.NextFloat() * 30f - 15f, -Calc.Random.NextFloat() * 60f)
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
            
            if (State == FlameState.Burning || State == FlameState.Spreading)
            {
                // Flicker effect
                flickerTimer += Engine.DeltaTime * 10f;
                intensity = 0.8f + (float)Math.Sin(flickerTimer) * 0.2f;
                flameLight.Alpha = intensity * 0.7f;
                
                // Create particles
                if (Scene.OnInterval(0.08f))
                {
                    CreateFlameParticle();
                }
                
                // Check player collision
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    player.Die(Vector2.Zero);
                }
                
                // Spread
                spreadCooldown -= Engine.DeltaTime;
                if (CanSpread && Scene.OnInterval(2f))
                {
                    Spread();
                }
            }
            
            if (State == FlameState.Extinguishing)
            {
                intensity -= Engine.DeltaTime * 0.5f;
                flameLight.Alpha = intensity * 0.7f;
                
                if (intensity <= 0f)
                {
                    State = FlameState.Extinguished;
                    sprite.Play("extinguished");
                }
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            if (IsBurning)
            {
                // Draw flame glow
                Draw.Circle(Position - Vector2.UnitY * 24f, 32f * intensity, Color.Orange * 0.3f, 12);
            }
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// FlameParticle - Particle for ceremony flames
    /// </summary>
    public class FlameParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private float scale;

        public FlameParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.9f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.2f - 0.6f) + 0.6f;
            
            Color[] colors = { Color.Orange, Color.OrangeRed, Color.Yellow, Color.Red };
            color = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 100f * Engine.DeltaTime;
            velocity *= 0.95f;
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 8f * scale, color * (alpha * 0.6f), 5);
        }
    }

    /// <summary>
    /// FlameExtinguisher - Interactive object that can put out flames
    /// </summary>
    [CustomEntity("DZ/FlameExtinguisher")]
    public class FlameExtinguisher : Actor
    {
        private Sprite sprite;
        private float radius;
        private bool activated;

        public FlameExtinguisher(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            radius = data.Float("radius", 100f);
            activated = false;
            
            Collider = new Hitbox(16f, 24f, -8f, -24f);
            Add(sprite = GFX.SpriteBank.Create("flame_extinguisher"));
        }

        public override void Update()
        {
            base.Update();
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player) && !activated)
            {
                Activate();
            }
        }

        private void Activate()
        {
            activated = true;
            sprite.Play("activate");
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            // Extinguish nearby flames
            foreach (var flame in Scene.Tracker.GetEntities<CeremonyFlame>())
            {
                if (Vector2.Distance(Position, flame.Position) < radius)
                {
                    ((CeremonyFlame)flame).Extinguish();
                }
            }
        }
    }
}

