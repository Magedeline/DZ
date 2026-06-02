namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// TitanSquire - Armored warrior serving the Titan King
    /// Uses flame sword attacks and can call for reinforcements
    /// Sprite path: characters/titan_squire/
    /// </summary>
    [CustomEntity("MaggyHelper/TitanSquire")]
    [Tracked]
    public class TitanSquire : Actor
    {
        #region Enums
        public enum SquireState
        {
            Idle,
            Patrol,
            Alert,
            Chase,
            SwordSlash,
            FlameThrust,
            ShieldBash,
            CallReinforcements,
            Stunned,
            Defeated
        }

        public enum AttackType
        {
            Slash,
            FlameThrust,
            ShieldBash
        }
        #endregion

        #region Properties
        public SquireState State { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float DetectionRange { get; private set; }
        public float AttackRange { get; private set; }
        public float MoveSpeed { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private Sprite swordSprite;
        private Sprite shieldSprite;
        private StateMachine stateMachine;
        private Vector2 startPosition;
        private float patrolDistance;
        private Facings facing;
        private float alertTimer;
        private float invincibilityTimer;
        private float attackCooldown;
        private Player targetPlayer;
        private Level level;
        private VertexLight flameGlow;
        private List<TitanFlameParticle> flameParticles;
        private bool hasCalledReinforcements;
        #endregion

        #region Constructor
        public TitanSquire(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 4),
                data.Float("detectionRange", 180f),
                data.Float("attackRange", 80f),
                data.Float("moveSpeed", 70f),
                data.Float("patrolDistance", 120f)
            );
        }

        public TitanSquire(Vector2 position, int health = 4, float detectionRange = 180f,
            float attackRange = 80f, float moveSpeed = 70f, float patrolDistance = 120f)
            : base(position)
        {
            Initialize(health, detectionRange, attackRange, moveSpeed, patrolDistance);
        }

        private void Initialize(int health, float detectionRange, float attackRange, float moveSpeed, float patrolDistance)
        {
            startPosition = Position;
            this.patrolDistance = patrolDistance;
            Health = health;
            MaxHealth = health;
            DetectionRange = detectionRange;
            AttackRange = attackRange;
            MoveSpeed = moveSpeed;
            facing = Facings.Right;
            alertTimer = 0f;
            invincibilityTimer = 0f;
            attackCooldown = 0f;
            hasCalledReinforcements = false;
            flameParticles = new List<TitanFlameParticle>();
            
            // Setup collider
            Collider = new Hitbox(20f, 40f, -10f, -40f);
            
            // Setup sprites
            Add(sprite = GFX.SpriteBank.Create("titan_squire"));
            Add(swordSprite = GFX.SpriteBank.Create("titan_sword"));
            swordSprite.Position = new Vector2(12f, -20f);
            Add(shieldSprite = GFX.SpriteBank.Create("titan_shield"));
            shieldSprite.Position = new Vector2(-10f, -16f);
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
            
            // Add flame glow
            Add(flameGlow = new VertexLight(Color.OrangeRed, 0.3f, 8, 24));
            
            State = SquireState.Idle;
        }
        #endregion

        #region State Begin Methods
        private void IdleBegin()
        {
            sprite.Play("idle");
            State = SquireState.Idle;
        }

        private void PatrolBegin()
        {
            sprite.Play("walk");
            State = SquireState.Patrol;
        }

        private void AlertBegin()
        {
            sprite.Play("alert");
            State = SquireState.Alert;
            alertTimer = 0.5f;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void ChaseBegin()
        {
            sprite.Play("chase");
            State = SquireState.Chase;
        }

        private void SwordSlashBegin()
        {
            sprite.Play("slash_windup");
            State = SquireState.SwordSlash;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void FlameThrustBegin()
        {
            sprite.Play("flame_thrust");
            State = SquireState.FlameThrust;
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        private void ShieldBashBegin()
        {
            sprite.Play("shield_bash");
            State = SquireState.ShieldBash;
        }

        private void CallReinforcementsBegin()
        {
            sprite.Play("call");
            State = SquireState.CallReinforcements;
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        private void StunnedBegin()
        {
            sprite.Play("stunned");
            State = SquireState.Stunned;
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = SquireState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator IdleRoutine()
        {
            yield return 1f;
            stateMachine.State = 1; // Patrol
        }

        private IEnumerator PatrolRoutine()
        {
            Vector2 patrolTarget = startPosition + new Vector2(patrolDistance, 0);
            
            while (true)
            {
                // Move toward patrol target
                Vector2 direction = (patrolTarget - Position).SafeNormalize();
                MoveH(direction.X * MoveSpeed * Engine.DeltaTime);
                
                // Flip at patrol bounds
                if (Position.X >= patrolTarget.X && facing == Facings.Right)
                {
                    facing = Facings.Left;
                    patrolTarget = startPosition - new Vector2(patrolDistance, 0);
                    sprite.Scale.X = -1;
                    swordSprite.Scale.X = -1;
                }
                else if (Position.X <= patrolTarget.X && facing == Facings.Left)
                {
                    facing = Facings.Right;
                    patrolTarget = startPosition + new Vector2(patrolDistance, 0);
                    sprite.Scale.X = 1;
                    swordSprite.Scale.X = 1;
                }
                
                // Check for player
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                {
                    stateMachine.State = 2; // Alert
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator AlertRoutine()
        {
            // Face player
            if (targetPlayer != null)
            {
                facing = targetPlayer.Position.X > Position.X ? Facings.Right : Facings.Left;
                sprite.Scale.X = facing == Facings.Right ? 1 : -1;
            }
            
            while (alertTimer > 0f)
            {
                alertTimer -= Engine.DeltaTime;
                yield return null;
            }
            
            stateMachine.State = 3; // Chase
        }

        private IEnumerator ChaseRoutine()
        {
            while (true)
            {
                if (targetPlayer == null)
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                
                if (targetPlayer != null)
                {
                    // Move toward player
                    Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
                    MoveH(direction.X * MoveSpeed * 1.3f * Engine.DeltaTime);
                    
                    // Update facing
                    facing = targetPlayer.Position.X > Position.X ? Facings.Right : Facings.Left;
                    sprite.Scale.X = facing == Facings.Right ? 1 : -1;
                    
                    // Check attack range
                    float distance = Vector2.Distance(Position, targetPlayer.Position);
                    if (distance < AttackRange && attackCooldown <= 0f)
                    {
                        // Choose attack
                        AttackType attack = ChooseAttack();
                        stateMachine.State = (int)(attack == AttackType.Slash ? SquireState.SwordSlash :
                            attack == AttackType.FlameThrust ? SquireState.FlameThrust : SquireState.ShieldBash);
                        yield break;
                    }
                    
                    // Call reinforcements if low health
                    if (Health <= MaxHealth / 2 && !hasCalledReinforcements)
                    {
                        stateMachine.State = 7; // CallReinforcements
                        yield break;
                    }
                }
                
                yield return null;
            }
        }

        private IEnumerator SwordSlashRoutine()
        {
            // Wind-up
            yield return 0.3f;
            
            sprite.Play("slash_strike");
            
            // Create attack hitbox
            Vector2 attackOffset = new Vector2(facing == Facings.Right ? 40f : -40f, -20f);
            var attackHitbox = new Hitbox(50f, 30f, attackOffset.X - 25f, attackOffset.Y - 15f);
            
            // Check hit
            if (targetPlayer != null && attackHitbox.Bounds.Intersects(targetPlayer.Collider.Bounds))
            {
                targetPlayer.Die(Vector2.Zero);
            }
            
            // Create flame particles
            CreateFlameSlash(attackOffset);
            
            yield return 0.4f;
            
            attackCooldown = 0.8f;
            stateMachine.State = 3; // Chase
        }

        private IEnumerator FlameThrustRoutine()
        {
            // Charge flame
            for (int i = 0; i < 10; i++)
            {
                CreateFlameParticle(Position + new Vector2(facing == Facings.Right ? 20f : -20f, -20f));
                yield return 0.05f;
            }
            
            // Thrust forward
            sprite.Play("thrust_strike");
            
            // Create flame projectile
            var projectile = new FlameSwordProjectile(
                Position + new Vector2(facing == Facings.Right ? 30f : -30f, -20f),
                new Vector2(facing == Facings.Right ? 200f : -200f, 0f)
            );
            Scene.Add(projectile);
            
            yield return 0.5f;
            
            attackCooldown = 1.2f;
            stateMachine.State = 3; // Chase
        }

        private IEnumerator ShieldBashRoutine()
        {
            // Quick bash forward
            float bashDistance = 60f;
            float bashSpeed = 300f;
            Vector2 bashTarget = Position + new Vector2(facing == Facings.Right ? bashDistance : -bashDistance, 0);
            
            while (Vector2.Distance(Position, bashTarget) > 4f)
            {
                Vector2 dir = (bashTarget - Position).SafeNormalize();
                MoveH(dir.X * bashSpeed * Engine.DeltaTime);
                
                // Check collision with player
                if (targetPlayer != null && Collide.Check(this, targetPlayer))
                {
                    // Knockback player
                    targetPlayer.Die(Vector2.Zero);
                }
                
                yield return null;
            }
            
            yield return 0.3f;
            
            attackCooldown = 1.5f;
            stateMachine.State = 3; // Chase
        }

        private IEnumerator CallReinforcementsRoutine()
        {
            hasCalledReinforcements = true;
            
            // Animation
            yield return 1f;
            
            // Spawn reinforcement squire
            var reinforcement = new TitanSquire(Position + new Vector2(facing == Facings.Right ? -80f : 80f, 0f));
            Scene.Add(reinforcement);
            
            level?.Shake(0.3f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            yield return 0.5f;
            
            stateMachine.State = 3; // Chase
        }

        private IEnumerator StunnedRoutine()
        {
            float stunDuration = 1f;
            while (stunDuration > 0f)
            {
                stunDuration -= Engine.DeltaTime;
                yield return null;
            }
            
            stateMachine.State = 3; // Chase
        }

        private IEnumerator DefeatedRoutine()
        {
            // Death particles
            for (int i = 0; i < 15; i++)
            {
                level?.ParticlesFG.Emit(ParticleTypes.Dust, 1, Position, Vector2.One * 8f);
                yield return 0.05f;
            }
            
            // Drop flame particles
            for (int i = 0; i < 8; i++)
            {
                CreateFlameParticle(Position + new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f));
            }
            
            yield return 1f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private AttackType ChooseAttack()
        {
            float distance = targetPlayer != null ? Vector2.Distance(Position, targetPlayer.Position) : AttackRange;
            
            if (distance < AttackRange * 0.6f)
                return AttackType.ShieldBash;
            else if (Calc.Random.NextFloat() < 0.4f)
                return AttackType.FlameThrust;
            else
                return AttackType.Slash;
        }

        private void CreateFlameSlash(Vector2 offset)
        {
            for (int i = 0; i < 8; i++)
            {
                var particle = new TitanFlameParticle(
                    Position + offset + new Vector2(Calc.Random.NextFloat() * 30f - 15f, Calc.Random.NextFloat() * 30f - 15f),
                    new Vector2(Calc.Random.NextFloat() * 60f - 30f, -Calc.Random.NextFloat() * 40f)
                );
                flameParticles.Add(particle);
                Scene.Add(particle);
            }
        }

        private void CreateFlameParticle(Vector2 position)
        {
            var particle = new FlameParticle(position, new Vector2(Calc.Random.NextFloat() * 40f - 20f, -Calc.Random.NextFloat() * 40f));
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (invincibilityTimer > 0f) return;
            
            Health -= damage;
            invincibilityTimer = 0.5f;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 6, Position, Vector2.One * 6f);
            
            if (Health <= 0)
            {
                stateMachine.State = 9; // Defeated
            }
            else
            {
                stateMachine.State = 8; // Stunned
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
            
            if (invincibilityTimer > 0f)
                invincibilityTimer -= Engine.DeltaTime;
            
            if (attackCooldown > 0f)
                attackCooldown -= Engine.DeltaTime;
            
            // Clean up particles
            flameParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw flame glow on sword
            if (State == SquireState.FlameThrust || State == SquireState.SwordSlash)
            {
                Vector2 swordTip = Position + new Vector2(facing == Facings.Right ? 24f : -24f, -24f);
                Draw.Circle(swordTip, 12f, Color.OrangeRed * 0.4f, 8);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// FlameSwordProjectile - Projectile fired by TitanSquire's FlameThrust attack
    /// </summary>
    public class FlameSwordProjectile : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private Sprite sprite;
        private List<TitanFlameParticle> trailParticles;

        public FlameSwordProjectile(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 2f;
            trailParticles = new List<TitanFlameParticle>();
            
            Collider = new Hitbox(24f, 16f, -12f, -8f);
            Add(sprite = GFX.SpriteBank.Create("flame_sword_projectile"));
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.98f;
            lifetime -= Engine.DeltaTime;
            
            // Create trail
            if (Scene.OnInterval(0.05f))
            {
                var particle = new TitanFlameParticle(Position, Vector2.Zero);
                trailParticles.Add(particle);
                Scene.Add(particle);
            }
            
            // Check player collision
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

        public override void Render()
        {
            Draw.Circle(Position, 16f, Color.OrangeRed * 0.3f, 8);
            base.Render();
        }
    }

    /// <summary>
    /// TitanFlameParticle - Particle effect for TitanSquire flame attacks
    /// </summary>
    public class TitanFlameParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color particleColor;
        private float scale;

        public TitanFlameParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.3f) + 0.3f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.5f - 0.5f) + 0.5f;
            
            // Random flame color
            Color[] colors = { Color.Orange, Color.OrangeRed, Color.Red, Color.Yellow };
            particleColor = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 60f * Engine.DeltaTime; // Rise
            velocity.X *= 0.95f;
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 6f * scale, particleColor * (alpha * 0.6f), 6);
        }
    }
}
