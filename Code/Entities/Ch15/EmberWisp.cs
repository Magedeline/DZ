namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// EmberWisp - Small flame spirit that ignites platforms temporarily
    /// Floats around and creates fire hazards on contact with surfaces
    /// Sprite path: characters/ember_wisp/
    /// </summary>
    [CustomEntity("MaggyHelper/EmberWisp")]
    [Tracked]
    public class EmberWisp : Actor
    {
        #region Enums
        public enum WispState
        {
            Idle,
            Floating,
            Igniting,
            Burning,
            Extinguishing,
            Defeated
        }
        #endregion

        #region Properties
        public WispState State { get; private set; }
        public int Health { get; private set; }
        public float FloatSpeed { get; private set; }
        public float IgniteRadius { get; private set; }
        public float BurnDuration { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 floatDirection;
        private float floatTimer;
        private float burnTimer;
        private List<FireHazard> createdFires;
        private Level level;
        private VertexLight fireGlow;
        private List<EmberParticle> emberParticles;
        #endregion

        #region Constructor
        public EmberWisp(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 1),
                data.Float("floatSpeed", 40f),
                data.Float("igniteRadius", 16f),
                data.Float("burnDuration", 3f)
            );
        }

        public EmberWisp(Vector2 position, int health = 1, float floatSpeed = 40f,
            float igniteRadius = 16f, float burnDuration = 3f)
            : base(position)
        {
            Initialize(health, floatSpeed, igniteRadius, burnDuration);
        }

        private void Initialize(int health, float floatSpeed, float igniteRadius, float burnDuration)
        {
            Health = health;
            FloatSpeed = floatSpeed;
            IgniteRadius = igniteRadius;
            BurnDuration = burnDuration;
            
            State = WispState.Idle;
            floatDirection = new Vector2(Calc.Random.NextFloat() * 2f - 1f, Calc.Random.NextFloat() * 2f - 1f).SafeNormalize();
            floatTimer = 0f;
            burnTimer = 0f;
            createdFires = new List<FireHazard>();
            emberParticles = new List<EmberParticle>();
            
            // Setup collider - small floating hitbox
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("ember_wisp"));
            sprite.Play("idle");
            
            // Add fire glow
            Add(fireGlow = new VertexLight(Color.Orange, 0.8f, 8, 24));
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void IdleBegin()
        {
            sprite.Play("idle");
            State = WispState.Idle;
        }

        private void FloatingBegin()
        {
            sprite.Play("float");
            State = WispState.Floating;
        }

        private void IgnitingBegin()
        {
            sprite.Play("ignite");
            State = WispState.Igniting;
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        private void BurningBegin()
        {
            sprite.Play("burn");
            State = WispState.Burning;
            burnTimer = BurnDuration;
        }

        private void ExtinguishingBegin()
        {
            sprite.Play("extinguish");
            State = WispState.Extinguishing;
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = WispState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator IdleRoutine()
        {
            yield return 0.5f;
            stateMachine.State = 1; // Floating
        }

        private IEnumerator FloatingRoutine()
        {
            while (true)
            {
                // Float in current direction
                Position += floatDirection * FloatSpeed * Engine.DeltaTime;
                
                // Wobble effect
                floatTimer += Engine.DeltaTime * 3f;
                sprite.Position = new Vector2((float)Math.Sin(floatTimer) * 2f, (float)Math.Cos(floatTimer * 1.5f) * 2f);
                
                // Change direction occasionally
                if (Scene.OnInterval(2f))
                {
                    floatDirection = new Vector2(Calc.Random.NextFloat() * 2f - 1f, Calc.Random.NextFloat() * 2f - 1f).SafeNormalize();
                }
                
                // Bounce off walls
                if (CollideCheck<Solid>(Position + floatDirection * 8f))
                {
                    floatDirection *= -1;
                    
                    // Check if should ignite surface
                    stateMachine.State = 2; // Igniting
                    yield break;
                }
                
                // Create ember particles
                if (Scene.OnInterval(0.1f))
                {
                    CreateEmberParticle();
                }
                
                // Check player collision
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    player.Die(Vector2.Zero);
                }
                
                yield return null;
            }
        }

        private IEnumerator IgnitingRoutine()
        {
            // Create fire hazard at contact point
            var fire = new FireHazard(Position + floatDirection * 8f, BurnDuration);
            Scene.Add(fire);
            createdFires.Add(fire);
            
            // Ignition effect
            for (int i = 0; i < 8; i++)
            {
                CreateEmberParticle();
                yield return 0.05f;
            }
            
            stateMachine.State = 3; // Burning
        }

        private IEnumerator BurningRoutine()
        {
            while (burnTimer > 0f)
            {
                burnTimer -= Engine.DeltaTime;
                
                // Continue creating particles
                if (Scene.OnInterval(0.15f))
                {
                    CreateEmberParticle();
                }
                
                yield return null;
            }
            
            stateMachine.State = 4; // Extinguishing
        }

        private IEnumerator ExtinguishingRoutine()
        {
            yield return 0.5f;
            stateMachine.State = 1; // Floating
        }

        private IEnumerator DefeatedRoutine()
        {
            // Scatter embers
            for (int i = 0; i < 12; i++)
            {
                CreateEmberParticle();
            }
            
            yield return 0.5f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private void CreateEmberParticle()
        {
            var particle = new EmberParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 8f - 4f, Calc.Random.NextFloat() * 8f - 4f),
                new Vector2(Calc.Random.NextFloat() * 30f - 15f, -Calc.Random.NextFloat() * 30f)
            );
            emberParticles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == WispState.Defeated) return;
            
            Health -= damage;
            
            if (Health <= 0)
            {
                stateMachine.State = 5; // Defeated
            }
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
            emberParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw glow
            Draw.Circle(Position, 16f, Color.Orange * 0.3f, 8);
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// FireHazard - Temporary fire created by EmberWisp
    /// Damages player on contact
    /// </summary>
    public class FireHazard : Actor
    {
        private float duration;
        private float timer;
        private Sprite sprite;
        private bool isActive;

        public FireHazard(Vector2 position, float duration)
            : base(position)
        {
            this.duration = duration;
            timer = 0f;
            isActive = true;
            
            Collider = new Hitbox(24f, 16f, -12f, -8f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(sprite = GFX.SpriteBank.Create("fire_hazard"));
            sprite.Play("burn");
            Add(new VertexLight(Color.Orange, 0.5f, 8, 16));
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            if (timer >= duration)
            {
                isActive = false;
                sprite.Play("fade");
                
                if (sprite.CurrentAnimationID == "fade" && !sprite.Animating)
                {
                    RemoveSelf();
                }
            }
            
            if (isActive)
            {
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    player.Die(Vector2.Zero);
                }
            }
        }
    }

    /// <summary>
    /// EmberParticle - Small ember particle effect
    /// </summary>
    public class EmberParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public EmberParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
            
            Color[] colors = { Color.Orange, Color.OrangeRed, Color.Yellow, Color.Red };
            color = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 80f * Engine.DeltaTime;
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
            Draw.Circle(Position, 4f, color * (alpha * 0.7f), 4);
        }
    }
}

