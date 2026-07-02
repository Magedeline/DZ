namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/Enemy")]
    [Tracked]
    [HotReloadable]
    public class Enemy : Actor
    {
        public enum EnemyType
        {
            Basic,
            Flyer,
            Walker,
            Turret
        }

        public enum EnemyState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Stunned,
            BeingInhaled,
            Defeated
        }

        public enum PowerType
        {
            None,
            Fire,
            Ice,
            Spark,
            Stone,
            Sword
        }

        public EnemyType Type { get; private set; }
        public EnemyState State { get; private set; }
        public PowerType GimmickType { get; private set; } // Added property to fix CS1061  

        public bool CanBeInhaled { get; set; }
        public bool IsDefeated { get; protected set; }
        public bool IsAlive => !IsDefeated && Health > 0;
        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public float Speed { get; set; }
        public float DetectionRange { get; set; }
        public float AttackRange { get; set; }
        public float InvincibilityTimer { get; protected set; }

        private Sprite sprite;
        private Monocle.StateMachine stateMachine;
        private Vector2 startPosition;
        private float patrolDistance;
        private bool facingRight;
        private float stunTimer;
        protected global::Celeste.Player cachedPlayer;
        private global::Celeste.Player targetPlayer;

        public Enemy(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Enum("powerType", PowerType.None))
        {
        }

        public Enemy(Vector2 position, PowerType powerType) : base(position)
        {
            GimmickType = powerType;
            startPosition = position;
            facingRight = true;
            CanBeInhaled = true;
            IsDefeated = false;
            
            // Set properties based on power type
            switch (powerType)
            {
                case PowerType.Fire:
                    MaxHealth = 3;
                    Speed = 80f;
                    DetectionRange = 120f;
                    AttackRange = 60f;
                    Type = EnemyType.Walker;
                    break;
                    
                case PowerType.Ice:
                    MaxHealth = 2;
                    Speed = 60f;
                    DetectionRange = 100f;
                    AttackRange = 80f;
                    Type = EnemyType.Basic;
                    break;
                    
                case PowerType.Spark:
                    MaxHealth = 2;
                    Speed = 120f;
                    DetectionRange = 140f;
                    AttackRange = 40f;
                    Type = EnemyType.Flyer;
                    break;
                    
                case PowerType.Stone:
                    MaxHealth = 5;
                    Speed = 40f;
                    DetectionRange = 80f;
                    AttackRange = 50f;
                    Type = EnemyType.Walker;
                    CanBeInhaled = false; // Stone enemies are too heavy
                    break;
                    
                case PowerType.Sword:
                    MaxHealth = 4;
                    Speed = 100f;
                    DetectionRange = 110f;
                    AttackRange = 70f;
                    Type = EnemyType.Walker;
                    break;
                    
                default: // None
                    MaxHealth = 1;
                    Speed = 70f;
                    DetectionRange = 90f;
                    AttackRange = 30f;
                    Type = EnemyType.Basic;
                    break;
            }
            
            Health = MaxHealth;
            State = EnemyState.Idle;
            patrolDistance = 64f;
            
            // Set up collision bounds
            Collider = new Hitbox(16f, 16f, -8f, -16f);
            
            // Initialize sprite based on enemy type
            string spritePath = GetSpritePathForType(powerType);
            Add(sprite = new Sprite(GFX.Game, spritePath));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.AddLoop("walk", "", 0.1f);
            sprite.AddLoop("attack", "", 0.1f);
            sprite.Play("idle");
            
            // Initialize state machine
            Add(stateMachine = new Monocle.StateMachine());
            SetupStateMachine();
        }

        public Enemy(Vector2 position, int health)
            : this(position, PowerType.None)
        {
            MaxHealth = Math.Max(health, 1);
            Health = MaxHealth;
        }

        private void SetupStateMachine()
        {
            stateMachine.SetCallbacks(
                (int)EnemyState.Idle, 
                IdleUpdate, 
                null,
                IdleBegin,
                null
            );
            
            stateMachine.SetCallbacks(
                (int)EnemyState.Patrol, 
                PatrolUpdate, 
                null,
                PatrolBegin,
                null
            );
            
            stateMachine.SetCallbacks(
                (int)EnemyState.Chase, 
                ChaseUpdate, 
                null,
                ChaseBegin,
                null
            );
            
            stateMachine.SetCallbacks(
                (int)EnemyState.Attack, 
                AttackUpdate, 
                null,
                AttackBegin,
                null
            );
            
            stateMachine.SetCallbacks(
                (int)EnemyState.Stunned, 
                StunnedUpdate, 
                null,
                StunnedBegin,
                null
            );
            
            stateMachine.SetCallbacks(
                (int)EnemyState.BeingInhaled, 
                BeingInhaledUpdate, 
                null,
                BeingInhaledBegin,
                null
            );
            
            stateMachine.SetCallbacks(
                (int)EnemyState.Defeated, 
                DefeatedUpdate, 
                null,
                DefeatedBegin,
                null
            );
        }

        #region State Machine Callbacks

        // Idle State
        private int IdleUpdate()
        {
            // Look for player
            if (targetPlayer == null)
                targetPlayer = Scene.Tracker.GetEntity<global::Celeste.Player>();

            cachedPlayer = targetPlayer;

            if (targetPlayer != null && CanSeePlayer())
            {
                return (int)EnemyState.Chase;
            }

            // Start patrolling after being idle for a while
            return (int)EnemyState.Patrol;
        }

        private void IdleBegin()
        {
            State = EnemyState.Idle;
            // Play idle animation if sprite exists
            sprite?.Play("idle");
        }

        // Patrol State
        private int PatrolUpdate()
        {
            // Look for player
            if (targetPlayer != null && CanSeePlayer())
            {
                return (int)EnemyState.Chase;
            }

            // Patrol between start position and patrol distance
            Vector2 targetPos = facingRight 
                ? startPosition + Vector2.UnitX * patrolDistance 
                : startPosition - Vector2.UnitX * patrolDistance;

            Vector2 direction = (targetPos - Position).SafeNormalize();
            MoveH(direction.X * Speed * 0.5f * Engine.DeltaTime);

            // Flip direction when reaching patrol limits
            if (Math.Abs(Position.X - targetPos.X) < 8f)
            {
                facingRight = !facingRight;
            }

            return (int)EnemyState.Patrol;
        }

        private void PatrolBegin()
        {
            State = EnemyState.Patrol;
            sprite?.Play("walk");
        }

        // Chase State
        private int ChaseUpdate()
        {
            if (targetPlayer == null || !CanSeePlayer())
            {
                return (int)EnemyState.Idle;
            }

            // Check if close enough to attack
            if (Vector2.Distance(Position, targetPlayer.Position) <= AttackRange)
            {
                return (int)EnemyState.Attack;
            }

            // Move towards player
            Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
            MoveH(direction.X * Speed * Engine.DeltaTime);
            
            // Update facing direction
            facingRight = direction.X > 0;

            return (int)EnemyState.Chase;
        }

        private void ChaseBegin()
        {
            State = EnemyState.Chase;
            sprite?.Play("chase");
        }

        // Attack State
        private int AttackUpdate()
        {
            if (targetPlayer == null || Vector2.Distance(Position, targetPlayer.Position) > AttackRange)
            {
                return (int)EnemyState.Chase;
            }

            // Perform attack based on enemy type
            PerformAttack();

            // Return to chase after attack
            return (int)EnemyState.Chase;
        }

        private void AttackBegin()
        {
            State = EnemyState.Attack;
            sprite?.Play("attack");
        }

        // Stunned State
        private int StunnedUpdate()
        {
            stunTimer -= Engine.DeltaTime;
            if (stunTimer <= 0f)
            {
                return (int)EnemyState.Idle;
            }
            return (int)EnemyState.Stunned;
        }

        private void StunnedBegin()
        {
            State = EnemyState.Stunned;
            stunTimer = 2f; // 2 second stun
            sprite?.Play("stunned");
        }

        // Being Inhaled State
        private int BeingInhaledUpdate()
        {
            // Move towards inhaling source (Kirby)
            if (targetPlayer != null)
            {
                Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
                Position += direction * Speed * 2f * Engine.DeltaTime;
                
                // Check if close enough to be consumed
                if (Vector2.Distance(Position, targetPlayer.Position) < 16f)
                {
                    return (int)EnemyState.Defeated;
                }
            }
            return (int)EnemyState.BeingInhaled;
        }

        private void BeingInhaledBegin()
        {
            State = EnemyState.BeingInhaled;
            sprite?.Play("inhaled");
        }

        // Defeated State
        private int DefeatedUpdate()
        {
            // Enemy is defeated, remove from scene
            RemoveSelf();
            return (int)EnemyState.Defeated;
        }

        private void DefeatedBegin()
        {
            State = EnemyState.Defeated;
            IsDefeated = true;
            sprite?.Play("defeated");
        }

        #endregion

        private bool CanSeePlayer()
        {
            if (targetPlayer == null) return false;
            
            float distance = Vector2.Distance(Position, targetPlayer.Position);
            return distance <= DetectionRange;
        }

        private void PerformAttack()
        {
            // Basic attack implementation based on power type
            switch (GimmickType)
            {
                case PowerType.Fire:
                    // Create fire projectile or area effect
                    break;
                case PowerType.Ice:
                    // Create ice projectile or freeze effect
                    break;
                case PowerType.Spark:
                    // Create electric attack
                    break;
                case PowerType.Stone:
                    // Heavy slam attack
                    break;
                case PowerType.Sword:
                    // Melee slash attack
                    break;
                default:
                    // Basic contact damage
                    break;
            }
        }

        protected virtual void OnHitPlayer(global::Celeste.Player player)
        {
            player?.Die((player.Position - Position).SafeNormalize());
        }

        public virtual void TakeDamage(int damage)
        {
            if (IsDefeated || InvincibilityTimer > 0f) return;

            Health -= damage;
            InvincibilityTimer = 0.1f;
            if (Health <= 0)
            {
                Health = 0;
                Die();
            }
            else
            {
                stateMachine.State = (int)EnemyState.Stunned;
            }
        }

        protected virtual void Die()
        {
            stateMachine.State = (int)EnemyState.Defeated;
        }

        public virtual void Heal(int amount)
        {
            if (amount <= 0 || IsDefeated)
            {
                return;
            }

            Health = Math.Min(MaxHealth, Health + amount);
        }

        public void StartBeingInhaled()
        {
            if (CanBeInhaled && !IsDefeated)
            {
                stateMachine.State = (int)EnemyState.BeingInhaled;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            stateMachine.State = (int)EnemyState.Idle;
        }

        private string GetSpritePathForType(PowerType powerType)
        {
            return powerType switch
            {
                PowerType.Fire => "characters/DZ/enemy/fire/",
                PowerType.Ice => "characters/DZ/enemy/ice/", 
                PowerType.Spark => "characters/DZ/enemy/spark/",
                PowerType.Stone => "characters/DZ/enemy/stone/",
                PowerType.Sword => "characters/DZ/enemy/sword/",
                _ => "characters/DZ/enemy/basic/"
            };
        }

        // Other methods and logic for Enemy class  
    }
}



