namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// RuinsSentinel - Stone guardian that awakens when player approaches
    /// Patrols ruins corridors and activates when player enters detection range
    /// Sprite path: characters/ruins_sentinel/
    /// </summary>
    [CustomEntity("MaggyHelper/RuinsSentinel")]
    [Tracked]
    public class RuinsSentinel : Actor
    {
        #region Enums
        public enum SentinelState
        {
            Dormant,
            Awakening,
            Patrol,
            Chase,
            Attack,
            Stunned,
            Defeated
        }
        #endregion

        #region Properties
        public SentinelState State { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float DetectionRange { get; private set; }
        public float AttackRange { get; private set; }
        public float MoveSpeed { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 startPosition;
        private Vector2 patrolTarget;
        private float patrolDistance;
        private Facings facing;
        private float alertTimer;
        private float invincibilityTimer;
        private float attackCooldown;
        private Player targetPlayer;
        private Level level;
        #endregion

        #region Constructor
        public RuinsSentinel(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 3),
                data.Float("detectionRange", 150f),
                data.Float("attackRange", 60f),
                data.Float("moveSpeed", 50f),
                data.Float("patrolDistance", 100f)
            );
        }

        public RuinsSentinel(Vector2 position, int health = 3, float detectionRange = 150f, 
            float attackRange = 60f, float moveSpeed = 50f, float patrolDistance = 100f)
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
            
            // Setup collider
            Collider = new Hitbox(24f, 32f, -12f, -32f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("ruins_sentinel"));
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
            
            // Add light
            Add(new VertexLight(Color.Orange, 1f, 8, 32));
        }
        #endregion

        #region State Begin Methods
        private void DormantBegin()
        {
            sprite.Play("dormant");
            State = SentinelState.Dormant;
        }

        private void AwakeningBegin()
        {
            sprite.Play("awaken");
            State = SentinelState.Awakening;
            Audio.Play("event:/game/gen_crumble_fall", Position);
            level?.Shake(0.3f);
        }

        private void PatrolBegin()
        {
            sprite.Play("walk");
            State = SentinelState.Patrol;
            patrolTarget = startPosition + new Vector2(patrolDistance, 0);
            facing = Facings.Right;
            sprite.Scale.X = 1;
        }

        private void ChaseBegin()
        {
            sprite.Play("chase");
            State = SentinelState.Chase;
            alertTimer = 3f;
        }

        private void AttackBegin()
        {
            sprite.Play("attack_windup");
            State = SentinelState.Attack;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void StunnedBegin()
        {
            sprite.Play("stunned");
            State = SentinelState.Stunned;
        }

        private void DefeatedBegin()
        {
            sprite.Play("death");
            State = SentinelState.Defeated;
            Audio.Play("event:/game/gen_crumble_fall", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator DormantRoutine()
        {
            while (true)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                {
                    stateMachine.State = 1; // Awakening
                    yield break;
                }
                yield return null;
            }
        }

        private IEnumerator AwakeningRoutine()
        {
            yield return 1.5f;
            stateMachine.State = 2; // Patrol
        }

        private IEnumerator PatrolRoutine()
        {
            while (true)
            {
                // Move toward patrol target
                Vector2 direction = (patrolTarget - Position).SafeNormalize();
                MoveH(direction.X * MoveSpeed * Engine.DeltaTime);
                
                // Flip direction at patrol bounds
                if (Position.X >= patrolTarget.X && facing == Facings.Right)
                {
                    facing = Facings.Left;
                    patrolTarget = startPosition - new Vector2(patrolDistance, 0);
                    sprite.Scale.X = -1;
                }
                else if (Position.X <= patrolTarget.X && facing == Facings.Left)
                {
                    facing = Facings.Right;
                    patrolTarget = startPosition + new Vector2(patrolDistance, 0);
                    sprite.Scale.X = 1;
                }
                
                // Check for player to chase
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange * 0.8f)
                {
                    stateMachine.State = 3; // Chase
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator ChaseRoutine()
        {
            while (alertTimer > 0f)
            {
                if (targetPlayer == null)
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                
                if (targetPlayer != null)
                {
                    // Move toward player
                    Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
                    MoveH(direction.X * MoveSpeed * 1.5f * Engine.DeltaTime);
                    
                    // Update facing
                    facing = targetPlayer.Position.X > Position.X ? Facings.Right : Facings.Left;
                    sprite.Scale.X = facing == Facings.Right ? 1 : -1;
                    
                    // Check attack range
                    if (Vector2.Distance(Position, targetPlayer.Position) < AttackRange)
                    {
                        stateMachine.State = 4; // Attack
                        yield break;
                    }
                }
                
                alertTimer -= Engine.DeltaTime;
                yield return null;
            }
            
            // Lost player, return to patrol
            stateMachine.State = 2; // Patrol
        }

        private IEnumerator AttackRoutine()
        {
            // Wind-up
            yield return 0.3f;
            
            sprite.Play("attack_strike");
            
            // Create attack hitbox
            var attackOffset = new Vector2(facing == Facings.Right ? 20f : -20f, -16f);
            var attackHitbox = new Hitbox(40f, 24f, attackOffset.X - 20f, attackOffset.Y - 12f);
            
            // Check if player is hit
            if (targetPlayer != null)
            {
                if (attackHitbox.Bounds.Intersects(targetPlayer.Collider.Bounds))
                {
                    targetPlayer.Die(Vector2.Zero);
                }
            }
            
            // Recovery
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
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 10, Position, Vector2.One * 8f);
            
            yield return 1f;
            RemoveSelf();
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (invincibilityTimer > 0f) return;
            
            Health -= damage;
            invincibilityTimer = 0.5f;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 5, Position, Vector2.One * 4f);
            
            if (Health <= 0)
            {
                stateMachine.State = 6; // Defeated
            }
            else
            {
                stateMachine.State = 5; // Stunned
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
        }
        #endregion
    }
}
