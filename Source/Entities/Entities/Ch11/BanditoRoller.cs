namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// BanditoRoller - Enemy that rolls toward player and bounces off walls
    /// Rolls continuously, changing direction on collision
    /// Sprite path: characters/DZ/bandito_roller/
    /// </summary>
    [CustomEntity("DZ/BanditoRoller")]
    [Tracked]
    public class BanditoRoller : Actor
    {
        #region Enums
        public enum RollerState
        {
            Idle,
            Rolling,
            Bouncing,
            Dashing,
            Stunned,
            Defeated
        }
        #endregion

        #region Properties
        public RollerState State { get; private set; }
        public int Health { get; private set; }
        public float RollSpeed { get; private set; }
        public float BounceSpeed { get; private set; }
        public float DetectionRange { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 velocity;
        private Vector2 rollDirection;
        private float rotation;
        private float stateTimer;
        private int bounceCount;
        private Player targetPlayer;
        private Level level;
        private List<BanditoDustParticle> dustParticles;
        private bool isChasing;
        #endregion

        #region Constructor
        public BanditoRoller(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 2),
                data.Float("rollSpeed", 150f),
                data.Float("bounceSpeed", 200f),
                data.Float("detectionRange", 200f)
            );
        }

        public BanditoRoller(Vector2 position, int health = 2, float rollSpeed = 150f,
            float bounceSpeed = 200f, float detectionRange = 200f)
            : base(position)
        {
            Initialize(health, rollSpeed, bounceSpeed, detectionRange);
        }

        private void Initialize(int health, float rollSpeed, float bounceSpeed, float detectionRange)
        {
            Health = health;
            RollSpeed = rollSpeed;
            BounceSpeed = bounceSpeed;
            DetectionRange = detectionRange;
            
            State = RollerState.Idle;
            velocity = Vector2.Zero;
            rollDirection = Vector2.UnitX;
            rotation = 0f;
            stateTimer = 0f;
            bounceCount = 0;
            isChasing = false;
            dustParticles = new List<BanditoDustParticle>();
            
            // Circular collider for rolling
            Collider = new Hitbox(24f, 24f, -12f, -12f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("bandito_roller"));
            sprite.Play("idle");
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void IdleBegin()
        {
            sprite.Play("idle");
            State = RollerState.Idle;
            velocity = Vector2.Zero;
        }

        private void RollingBegin()
        {
            sprite.Play("rolling");
            State = RollerState.Rolling;
        }

        private void BouncingBegin()
        {
            sprite.Play("bounce");
            State = RollerState.Bouncing;
            Audio.Play("event:/game/char_maddy/jump", Position);
        }

        private void DashingBegin()
        {
            sprite.Play("dash");
            State = RollerState.Dashing;
            Audio.Play("event:/game/char_maddy/dash", Position);
        }

        private void StunnedBegin()
        {
            sprite.Play("stunned");
            State = RollerState.Stunned;
            velocity = Vector2.Zero;
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = RollerState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator IdleRoutine()
        {
            // Wait briefly then start rolling
            yield return 0.5f;
            
            // Find player to chase
            targetPlayer = Scene.Tracker.GetEntity<Player>();
            if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
            {
                isChasing = true;
                rollDirection = (targetPlayer.Position - Position).SafeNormalize();
            }
            else
            {
                // Random direction
                rollDirection = new Vector2(Calc.Random.NextFloat() * 2f - 1f, 0f).SafeNormalize();
                isChasing = false;
            }
            
            stateMachine.State = 1; // Rolling
        }

        private IEnumerator RollingRoutine()
        {
            while (true)
            {
                // Update rotation based on movement
                rotation += (velocity.X > 0 ? -1 : 1) * Math.Abs(velocity.X) * 0.01f * Engine.DeltaTime;
                sprite.Rotation = rotation;
                
                // Move
                velocity = rollDirection * RollSpeed;
                
                // Check for wall collision - bounce
                if (MoveH(velocity.X * Engine.DeltaTime))
                {
                    // Hit wall - bounce
                    rollDirection.X *= -1;
                    bounceCount++;
                    
                    if (bounceCount >= 3)
                    {
                        stateMachine.State = 3; // Dashing
                        yield break;
                    }
                    
                    stateMachine.State = 2; // Bouncing
                    yield break;
                }
                
                // Check for player collision
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    player.Die(Vector2.Zero);
                }
                
                // Create dust particles
                if (Scene.OnInterval(0.1f))
                {
                    CreateDustParticle();
                }
                
                // Recalculate direction if chasing
                if (isChasing && Scene.OnInterval(1f))
                {
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                    if (targetPlayer != null)
                    {
                        rollDirection = (targetPlayer.Position - Position).SafeNormalize();
                        rollDirection.Y = 0f; // Keep horizontal
                        rollDirection.Normalize();
                    }
                }
                
                yield return null;
            }
        }

        private IEnumerator BouncingRoutine()
        {
            // Quick bounce animation
            velocity = rollDirection * BounceSpeed;
            
            float bounceTime = 0.3f;
            stateTimer = bounceTime;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                
                // Continue moving
                MoveH(velocity.X * Engine.DeltaTime);
                
                // Update rotation
                rotation += (velocity.X > 0 ? -1 : 1) * Math.Abs(velocity.X) * 0.01f * Engine.DeltaTime;
                sprite.Rotation = rotation;
                
                // Create dust
                CreateDustParticle();
                
                // Check player collision
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    player.Die(Vector2.Zero);
                }
                
                yield return null;
            }
            
            stateMachine.State = 1; // Rolling
        }

        private IEnumerator DashingRoutine()
        {
            // Fast dash toward player
            targetPlayer = Scene.Tracker.GetEntity<Player>();
            if (targetPlayer != null)
            {
                rollDirection = (targetPlayer.Position - Position).SafeNormalize();
            }
            
            velocity = rollDirection * BounceSpeed * 1.5f;
            bounceCount = 0;
            
            float dashTime = 0.5f;
            stateTimer = dashTime;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                
                // Fast movement
                MoveH(velocity.X * Engine.DeltaTime);
                
                // Fast rotation
                rotation += (velocity.X > 0 ? -1 : 1) * Math.Abs(velocity.X) * 0.02f * Engine.DeltaTime;
                sprite.Rotation = rotation;
                
                // Create dust trail
                for (int i = 0; i < 2; i++)
                {
                    CreateDustParticle();
                }
                
                // Check player collision
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    player.Die(Vector2.Zero);
                }
                
                yield return null;
            }
            
            stateMachine.State = 4; // Stunned (brief pause after dash)
        }

        private IEnumerator StunnedRoutine()
        {
            float stunDuration = 1f;
            stateTimer = stunDuration;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                yield return null;
            }
            
            stateMachine.State = 0; // Idle
        }

        private IEnumerator DefeatedRoutine()
        {
            // Death animation - unroll and fall
            for (int i = 0; i < 10; i++)
            {
                CreateDustParticle();
                yield return 0.05f;
            }
            
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 10, Position, Vector2.One * 8f);
            
            yield return 0.5f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private void CreateDustParticle()
        {
            var particle = new BanditoDustParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 16f - 8f, Calc.Random.NextFloat() * 16f - 8f),
                new Vector2(Calc.Random.NextFloat() * 30f - 15f, -Calc.Random.NextFloat() * 40f)
            );
            dustParticles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == RollerState.Defeated) return;
            
            Health -= damage;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            if (Health <= 0)
            {
                stateMachine.State = 5; // Defeated
            }
            else
            {
                stateMachine.State = 4; // Stunned
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
            dustParticles.RemoveAll(p => p == null || p.Scene == null);
        }
        #endregion
    }

    /// <summary>
    /// BanditoDustParticle - Simple dust particle effect for BanditoRoller
    /// </summary>
    public class BanditoDustParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public BanditoDustParticle(Vector2 position, Vector2 velocity)
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
            velocity.Y += 100f * Engine.DeltaTime;
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
            Draw.Rect(Position - new Vector2(4 * scale, 2 * scale), 8 * scale, 4 * scale, Color.Brown * (alpha * 0.5f));
        }
    }
}
