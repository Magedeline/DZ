namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// VirusCloud - Corrupted data cloud that damages and slows player
    /// Moves erratically and spreads corruption
    /// Sprite path: characters/DZ/virus_cloud/
    /// </summary>
    [CustomEntity("DZ/VirusCloud")]
    [Tracked]
    public class VirusCloud : Actor
    {
        #region Enums
        public enum VirusState
        {
            Dormant,
            Spreading,
            Hunting,
            Infecting,
            Dissolving,
            Destroyed
        }
        #endregion

        #region Properties
        public VirusState State { get; private set; }
        public int Health { get; private set; }
        public float SpreadRadius { get; private set; }
        public float MoveSpeed { get; private set; }
        public float DamageRate { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 velocity;
        private float stateTimer;
        private Player targetPlayer;
        private Level level;
        private List<VirusParticle> virusParticles;
        private List<VirusCloud> spreadClouds;
        private float damageTimer;
        private bool hasSpread;
        private VertexLight virusLight;
        #endregion

        #region Constructor
        public VirusCloud(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 3),
                data.Float("spreadRadius", 100f),
                data.Float("moveSpeed", 40f),
                data.Float("damageRate", 0.5f)
            );
        }

        public VirusCloud(Vector2 position, int health = 3, float spreadRadius = 100f,
            float moveSpeed = 40f, float damageRate = 0.5f)
            : base(position)
        {
            Initialize(health, spreadRadius, moveSpeed, damageRate);
        }

        private void Initialize(int health, float spreadRadius, float moveSpeed, float damageRate)
        {
            Health = health;
            SpreadRadius = spreadRadius;
            MoveSpeed = moveSpeed;
            DamageRate = damageRate;
            
            State = VirusState.Dormant;
            velocity = Vector2.Zero;
            stateTimer = 0f;
            damageTimer = 0f;
            hasSpread = false;
            virusParticles = new List<VirusParticle>();
            spreadClouds = new List<VirusCloud>();
            
            Collider = new Hitbox(40f, 40f, -20f, -20f);
            
            Add(sprite = DZModule.SpriteBank.Create("virus_cloud"));
            sprite.Play("dormant");
            
            Add(virusLight = new VertexLight(Color.Red, 0.3f, 12, 32));
            
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void DormantBegin()
        {
            sprite.Play("dormant");
            State = VirusState.Dormant;
            virusLight.Alpha = 0.3f;
        }

        private void SpreadingBegin()
        {
            sprite.Play("spreading");
            State = VirusState.Spreading;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void HuntingBegin()
        {
            sprite.Play("hunting");
            State = VirusState.Hunting;
        }

        private void InfectingBegin()
        {
            sprite.Play("infecting");
            State = VirusState.Infecting;
        }

        private void DissolvingBegin()
        {
            sprite.Play("dissolving");
            State = VirusState.Dissolving;
        }

        private void DestroyedBegin()
        {
            sprite.Play("destroyed");
            State = VirusState.Destroyed;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator DormantRoutine()
        {
            while (true)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < SpreadRadius)
                {
                    stateMachine.State = 1;
                    yield break;
                }
                yield return null;
            }
        }

        private IEnumerator SpreadingRoutine()
        {
            // Expand and create particles
            for (int i = 0; i < 20; i++)
            {
                CreateVirusParticle();
                yield return 0.1f;
            }
            
            // Spread to nearby location
            if (!hasSpread && Calc.Random.NextFloat() < 0.3f)
            {
                Vector2 spreadPos = Position + new Vector2(
                    Calc.Random.NextFloat() * 80f - 40f, Calc.Random.NextFloat() * 80f - 40f
                );
                
                var newCloud = new VirusCloud(spreadPos, Health / 2, SpreadRadius * 0.7f, MoveSpeed);
                spreadClouds.Add(newCloud);
                Scene.Add(newCloud);
                hasSpread = true;
            }
            
            stateMachine.State = 2;
        }

        private IEnumerator HuntingRoutine()
        {
            float huntTime = 3f;
            stateTimer = huntTime;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null)
                {
                    // Move toward player erratically
                    Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
                    velocity = direction * MoveSpeed;
                    velocity.X += Calc.Random.NextFloat() * 40f - 20f;
                    velocity.Y += Calc.Random.NextFloat() * 40f - 20f;
                    
                    Position += velocity * Engine.DeltaTime;
                    
                    // Create particles
                    if (Scene.OnInterval(0.1f))
                    {
                        CreateVirusParticle();
                    }
                    
                    // Check player collision
                    if (targetPlayer != null && Collide.Check(this, targetPlayer))
                    {
                        State = VirusState.Infecting;
                        stateTimer = 2f;
                        yield break;
                    }
                    
                    yield return null;
                }
                
                yield return null;
            }
        }

        private IEnumerator InfectingRoutine()
        {
            // Damage player over time
            float infectTime = 2f;
            stateTimer = infectTime;
            
            while (stateTimer > 0f && targetPlayer != null)
            {
                stateTimer -= Engine.DeltaTime;
                
                // Continuous damage
                damageTimer += Engine.DeltaTime;
                if (damageTimer >= DamageRate)
                {
                    damageTimer = 0f;
                    // Apply slow effect
                    targetPlayer.Speed *= 0.8f;
                }
                
                // Create infection particles
                for (int i = 0; i < 3; i++)
                {
                    CreateVirusParticle();
                }
                
                // Check if player escaped
                if (!Collide.Check(this, targetPlayer))
                {
                    stateMachine.State = 2;
                    yield break;
                }
                
                yield return null;
            }
            
            stateMachine.State = 2;
        }

        private IEnumerator DissolvingRoutine()
        {
            // Fade out
            for (int i = 0; i < 15; i++)
            {
                CreateVirusParticle();
                virusLight.Alpha -= 0.05f;
                yield return 0.05f;
            }
            
            RemoveSelf();
        }

        private IEnumerator DestroyedRoutine()
        {
            // Explode
            for (int i = 0; i < 25; i++)
            {
                CreateVirusParticle();
            }
            
            level?.Shake(0.2f);
            level?.Flash(Color.Red * 0.3f);
            
            yield return 0.3f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private void CreateVirusParticle()
        {
            var particle = new VirusParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 30f - 15f, Calc.Random.NextFloat() * 30f - 15f),
                new Vector2(Calc.Random.NextFloat() * 60f - 30f, -Calc.Random.NextFloat() * 60f)
            );
            virusParticles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == VirusState.Destroyed) return;
            
            Health -= damage;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            if (Health <= 0)
            {
                stateMachine.State = 5;
            }
            else
            {
                // Brief stagger
                stateMachine.State = 4;
                Add(new Coroutine(StaggerRoutine()));
            }
        }

        private IEnumerator StaggerRoutine()
        {
            yield return 0.5f;
            stateMachine.State = 2;
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
            virusParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw cloud area
            Draw.Circle(Position, 20f, Color.Red * 0.2f, 12);
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// VirusParticle - Particle for virus effects
    /// </summary>
    public class VirusParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public VirusParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.5f - 0.5f) + 0.5f;
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
}
