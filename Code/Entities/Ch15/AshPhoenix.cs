namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// AshPhoenix - Fire bird enemy that revives from its ashes once
    /// Has two phases: first defeat turns to ash, second defeat is permanent
    /// Sprite path: characters/ash_phoenix/
    /// </summary>
    [CustomEntity("MaggyHelper/AshPhoenix")]
    [Tracked]
    public class AshPhoenix : Actor
    {
        #region Enums
        public enum PhoenixState
        {
            Flying,
            Diving,
            Attacking,
            Dying,
            AshForm,
            Reviving,
            Reborn,
            Defeated
        }

        public enum PhoenixPhase
        {
            First,
            Second
        }
        #endregion

        #region Properties
        public PhoenixState State { get; private set; }
        public PhoenixPhase Phase { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float FlySpeed { get; private set; }
        public float DetectionRange { get; private set; }
        public bool IsAlive => Health > 0;
        public bool CanRevive => Phase == PhoenixPhase.First;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 flyDirection;
        private Vector2 homePosition;
        private float diveAngle;
        private float stateTimer;
        private int attackCount;
        private Player targetPlayer;
        private Level level;
        private VertexLight flameGlow;
        private List<PhoenixAshParticle> ashParticles;
        private List<PhoenixFeatherProjectile> feathers;
        private bool isReviving;
        #endregion

        #region Constructor
        public AshPhoenix(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 3),
                data.Float("flySpeed", 100f),
                data.Float("detectionRange", 250f)
            );
        }

        public AshPhoenix(Vector2 position, int health = 3, float flySpeed = 100f, float detectionRange = 250f)
            : base(position)
        {
            Initialize(health, flySpeed, detectionRange);
        }

        private void Initialize(int health, float flySpeed, float detectionRange)
        {
            Health = health;
            MaxHealth = health;
            FlySpeed = flySpeed;
            DetectionRange = detectionRange;
            Phase = PhoenixPhase.First;
            
            homePosition = Position;
            flyDirection = Vector2.UnitX;
            diveAngle = 0f;
            stateTimer = 0f;
            attackCount = 0;
            isReviving = false;
            ashParticles = new List<PhoenixAshParticle>();
            feathers = new List<PhoenixFeatherProjectile>();
            
            // Flying collider
            Collider = new Hitbox(32f, 28f, -16f, -14f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("ash_phoenix"));
            sprite.Play("fly");
            
            // Add flame glow
            Add(flameGlow = new VertexLight(Color.Orange, 0.7f, 12, 32));
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
            
            State = PhoenixState.Flying;
        }
        #endregion

        #region State Begin Methods
        private void FlyingBegin()
        {
            sprite.Play("fly");
            State = PhoenixState.Flying;
            flameGlow.Color = Phase == PhoenixPhase.First ? Color.Orange : Color.DarkOrange;
        }

        private void DivingBegin()
        {
            sprite.Play("dive");
            State = PhoenixState.Diving;
            Audio.Play("event:/game/char_maddy/jump", Position);
        }

        private void AttackingBegin()
        {
            sprite.Play("attack");
            State = PhoenixState.Attacking;
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        private void DyingBegin()
        {
            sprite.Play("death");
            State = PhoenixState.Dying;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void AshFormBegin()
        {
            sprite.Play("ash");
            State = PhoenixState.AshForm;
            flameGlow.Alpha = 0.1f;
        }

        private void RevivingBegin()
        {
            sprite.Play("revive");
            State = PhoenixState.Reviving;
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            flameGlow.Alpha = 0.3f;
        }

        private void RebornBegin()
        {
            sprite.Play("reborn");
            State = PhoenixState.Reborn;
            flameGlow.Alpha = 0.9f;
            flameGlow.Color = Color.DarkOrange;
        }

        private void DefeatedBegin()
        {
            sprite.Play("final_death");
            State = PhoenixState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator FlyingRoutine()
        {
            while (true)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                
                if (targetPlayer != null)
                {
                    float distance = Vector2.Distance(Position, targetPlayer.Position);
                    
                    // Circle around player or home
                    Vector2 center = distance < DetectionRange ? targetPlayer.Position : homePosition;
                    
                    // Circular flight pattern
                    diveAngle += Engine.DeltaTime * 1.5f;
                    Vector2 targetPos = center + new Vector2(
                        (float)Math.Cos(diveAngle) * 80f,
                        (float)Math.Sin(diveAngle * 0.5f) * 40f - 60f
                    );
                    
                    flyDirection = (targetPos - Position).SafeNormalize();
                    Position += flyDirection * FlySpeed * Engine.DeltaTime;
                    
                    // Face movement direction
                    sprite.Scale.X = flyDirection.X > 0 ? 1 : -1;
                    
                    // Create trail particles
                    if (Scene.OnInterval(0.1f))
                    {
                        CreateAshParticle();
                    }
                    
                    // Attack if player in range
                    if (distance < 180f && attackCount < 3)
                    {
                        if (Scene.OnInterval(2f))
                        {
                            stateMachine.State = 2; // Attacking
                            yield break;
                        }
                    }
                    
                    // Dive attack
                    if (distance < 120f && Scene.OnInterval(3f))
                    {
                        stateMachine.State = 1; // Diving
                        yield break;
                    }
                }
                
                yield return null;
            }
        }

        private IEnumerator DivingRoutine()
        {
            if (targetPlayer == null)
                targetPlayer = Scene.Tracker.GetEntity<Player>();
            
            Vector2 diveTarget = targetPlayer?.Position ?? Position + Vector2.UnitY * 100f;
            Vector2 diveDirection = (diveTarget - Position).SafeNormalize();
            
            // Quick dive
            float diveSpeed = 300f;
            float diveDuration = 0.5f;
            stateTimer = 0f;
            
            while (stateTimer < diveDuration)
            {
                stateTimer += Engine.DeltaTime;
                Position += diveDirection * diveSpeed * Engine.DeltaTime;
                
                // Create flame trail
                for (int i = 0; i < 2; i++)
                {
                    CreateAshParticle();
                }
                
                // Check player collision
                if (targetPlayer != null && Collide.Check(this, targetPlayer))
                {
                    targetPlayer.Die(Vector2.Zero);
                }
                
                yield return null;
            }
            
            // Rise back up
            stateMachine.State = 0; // Flying
        }

        private IEnumerator AttackingRoutine()
        {
            attackCount++;
            
            // Fire feathers in spread pattern
            int featherCount = Phase == PhoenixPhase.First ? 3 : 5;
            
            for (int i = 0; i < featherCount; i++)
            {
                if (targetPlayer == null)
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                
                Vector2 baseDir = targetPlayer != null ? 
                    (targetPlayer.Position - Position).SafeNormalize() : 
                    Vector2.UnitX * sprite.Scale.X;
                
                float spread = (i - featherCount / 2) * 0.3f;
                Vector2 featherDir = Calc.Rotate(baseDir, spread);
                
                var feather = new PhoenixFeatherProjectile(
                    Position,
                    featherDir * 150f
                );
                feathers.Add(feather);
                Scene.Add(feather);
                
                Audio.Play("event:/game/char_badeline/beam_launch", Position);
                yield return 0.1f;
            }
            
            yield return 0.5f;
            
            if (attackCount >= 3)
            {
                attackCount = 0;
            }
            
            stateMachine.State = 0; // Flying
        }

        private IEnumerator DyingRoutine()
        {
            // Scatter into ash
            for (int i = 0; i < 20; i++)
            {
                CreateAshParticle();
                yield return 0.05f;
            }
            
            level?.Shake(0.3f);
            
            if (Phase == PhoenixPhase.First)
            {
                // Turn to ash form
                stateMachine.State = 4; // AshForm
            }
            else
            {
                // Permanent death
                stateMachine.State = 7; // Defeated
            }
        }

        private IEnumerator AshFormRoutine()
        {
            // Remain as ash pile
            sprite.Visible = false;
            
            // Create ash pile visual
            for (int i = 0; i < 10; i++)
            {
                var particle = new PhoenixAshParticle(
                    Position + new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f),
                    Vector2.Zero,
                    true
                );
                ashParticles.Add(particle);
                Scene.Add(particle);
            }
            
            // Wait for revive trigger
            yield return 3f;
            
            stateMachine.State = 5; // Reviving
        }

        private IEnumerator RevivingRoutine()
        {
            sprite.Visible = true;
            
            // Dramatic revival
            for (int i = 0; i < 15; i++)
            {
                CreateAshParticle();
                flameGlow.Alpha = 0.3f + (i / 15f) * 0.6f;
                yield return 0.1f;
            }
            
            level?.Flash(Color.Orange * 0.4f);
            level?.Shake(0.2f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            yield return 0.5f;
            
            stateMachine.State = 6; // Reborn
        }

        private IEnumerator RebornRoutine()
        {
            // Reset for second phase
            Phase = PhoenixPhase.Second;
            Health = MaxHealth;
            attackCount = 0;
            
            yield return 0.5f;
            
            stateMachine.State = 0; // Flying
        }

        private IEnumerator DefeatedRoutine()
        {
            // Final death - scatter all ash
            for (int i = 0; i < 30; i++)
            {
                CreateAshParticle();
                yield return 0.03f;
            }
            
            level?.Shake(0.4f);
            level?.Flash(Color.DarkOrange * 0.3f);
            
            yield return 1f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private void CreateAshParticle()
        {
            var particle = new PhoenixAshParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 16f - 8f, Calc.Random.NextFloat() * 16f - 8f),
                new Vector2(Calc.Random.NextFloat() * 40f - 20f, -Calc.Random.NextFloat() * 30f)
            );
            ashParticles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == PhoenixState.AshForm || State == PhoenixState.Reviving || State == PhoenixState.Defeated)
                return;
            
            Health -= damage;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            if (Health <= 0)
            {
                if (Phase == PhoenixPhase.First)
                {
                    stateMachine.State = 3; // Dying (will revive)
                }
                else
                {
                    stateMachine.State = 7; // Defeated
                }
            }
            else
            {
                // Brief stagger
                stateMachine.State = 0; // Flying
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
            
            ashParticles.RemoveAll(p => p == null || p.Scene == null);
            feathers.RemoveAll(f => f == null || f.Scene == null);
        }

        public override void Render()
        {
            if (State != PhoenixState.AshForm)
            {
                Draw.Circle(Position, 24f, Color.Orange * 0.2f, 8);
            }
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// PhoenixAshParticle - Ash/flame particle for Phoenix
    /// </summary>
    public class PhoenixAshParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private float scale;
        private bool isStatic;

        public PhoenixAshParticle(Vector2 position, Vector2 velocity, bool isStatic = false)
            : base(position)
        {
            this.velocity = velocity;
            this.isStatic = isStatic;
            maxLifetime = isStatic ? 3f : Calc.Random.NextFloat() * (1.2f - 0.5f) + 0.5f;
            lifetime = maxLifetime;
            scale = isStatic ? 1f : Calc.Random.NextFloat() * (1.2f - 0.5f) + 0.5f;
            
            Color[] colors = { Color.Orange, Color.OrangeRed, Color.DarkGray, Color.Gray, Color.Yellow };
            color = isStatic ? Color.DarkGray : colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            if (!isStatic)
            {
                Position += velocity * Engine.DeltaTime;
                velocity.Y -= 40f * Engine.DeltaTime;
                velocity *= 0.96f;
            }
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 6f * scale, color * (alpha * 0.7f), 5);
        }
    }

    /// <summary>
    /// PhoenixFeatherProjectile - Feather projectile fired by Phoenix
    /// </summary>
    public class PhoenixFeatherProjectile : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private Sprite sprite;
        private float rotation;

        public PhoenixFeatherProjectile(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 2.5f;
            rotation = Calc.Angle(velocity);
            
            Collider = new Hitbox(16f, 8f, -8f, -4f);
            Add(sprite = GFX.SpriteBank.Create("phoenix_feather"));
            sprite.Rotation = rotation;
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 30f * Engine.DeltaTime; // Slight arc
            rotation = Calc.Angle(velocity);
            sprite.Rotation = rotation;
            
            lifetime -= Engine.DeltaTime;
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
                return;
            }
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }
    }
}
