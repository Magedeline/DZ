namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// AxisMinion - Small fire enemy that patrols and attacks
    /// Basic enemy type for Axis boss chapter
    /// Sprite path: characters/DZ/axis_minion/
    /// </summary>
    [CustomEntity("DZ/AxisMinion")]
    [Tracked]
    public class AxisMinion : Actor
    {
        #region Enums
        public enum MinionState
        {
            Idle,
            Patrolling,
            Alert,
            Chasing,
            Attacking,
            Stunned,
            Defeated
        }
        #endregion

        #region Properties
        public MinionState State { get; private set; }
        public int Health { get; private set; }
        public float MoveSpeed { get; private set; }
        public float DetectionRange { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 startPosition;
        private float patrolDistance;
        private Facings facing;
        private Player targetPlayer;
        private Level level;
        private List<FireParticle> fireParticles;
        private float attackCooldown;
        #endregion

        #region Constructor
        public AxisMinion(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 2),
                data.Float("moveSpeed", 60f),
                data.Float("detectionRange", 120f),
                data.Float("patrolDistance", 80f)
            );
        }

        public AxisMinion(Vector2 position, int health = 2, float moveSpeed = 60f,
            float detectionRange = 120f, float patrolDistance = 80f)
            : base(position)
        {
            Initialize(health, moveSpeed, detectionRange, patrolDistance);
        }

        private void Initialize(int health, float moveSpeed, float detectionRange, float patrolDistance)
        {
            Health = health;
            MoveSpeed = moveSpeed;
            DetectionRange = detectionRange;
            this.patrolDistance = patrolDistance;
            
            startPosition = Position;
            facing = Facings.Right;
            attackCooldown = 0f;
            fireParticles = new List<FireParticle>();
            
            State = MinionState.Idle;
            
            Collider = new Hitbox(20f, 24f, -10f, -24f);
            
            Add(sprite = GFX.SpriteBank.Create("axis_minion"));
            sprite.Play("idle");
            
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void IdleBegin()
        {
            sprite.Play("idle");
            State = MinionState.Idle;
        }

        private void PatrollingBegin()
        {
            sprite.Play("walk");
            State = MinionState.Patrolling;
        }

        private void AlertBegin()
        {
            sprite.Play("alert");
            State = MinionState.Alert;
        }

        private void ChasingBegin()
        {
            sprite.Play("chase");
            State = MinionState.Chasing;
        }

        private void AttackingBegin()
        {
            sprite.Play("attack");
            State = MinionState.Attacking;
        }

        private void StunnedBegin()
        {
            sprite.Play("stunned");
            State = MinionState.Stunned;
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = MinionState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator IdleRoutine()
        {
            yield return 0.5f;
            stateMachine.State = 1;
        }

        private IEnumerator PatrollingRoutine()
        {
            Vector2 patrolTarget = startPosition + new Vector2(patrolDistance, 0);
            
            while (true)
            {
                Vector2 dir = (patrolTarget - Position).SafeNormalize();
                MoveH(dir.X * MoveSpeed * Engine.DeltaTime);
                
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
                
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                {
                    stateMachine.State = 2;
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator AlertRoutine()
        {
            if (targetPlayer != null)
            {
                facing = targetPlayer.Position.X > Position.X ? Facings.Right : Facings.Left;
                sprite.Scale.X = facing == Facings.Right ? 1 : -1;
            }
            
            yield return 0.3f;
            stateMachine.State = 3;
        }

        private IEnumerator ChasingRoutine()
        {
            while (true)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                
                if (targetPlayer != null)
                {
                    Vector2 dir = (targetPlayer.Position - Position).SafeNormalize();
                    MoveH(dir.X * MoveSpeed * Engine.DeltaTime);
                    
                    // Check if close enough to attack
                    if (Vector2.Distance(Position, targetPlayer.Position) < 30f && attackCooldown <= 0f)
                    {
                        stateMachine.State = 4;
                        yield break;
                    }
                    
                    // Check if player escaped
                    if (Vector2.Distance(Position, targetPlayer.Position) > DetectionRange * 1.5f)
                    {
                        stateMachine.State = 1;
                        yield break;
                    }
                }
                
                if (attackCooldown > 0f)
                    attackCooldown -= Engine.DeltaTime;
                
                yield return null;
            }
        }

        private IEnumerator AttackingRoutine()
        {
            // Attack animation
            yield return 0.3f;
            
            // Fire projectile
            Vector2 shootDir = new Vector2((int)facing, 0);
            var projectile = new AxisProjectile(Position - Vector2.UnitY * 12f, shootDir * 150f);
            Scene.Add(projectile);
            
            Audio.Play("event:/game/char_badeline/beam_launch", Position);
            
            attackCooldown = 1.5f;
            yield return 0.2f;
            
            stateMachine.State = 3;
        }

        private IEnumerator StunnedRoutine()
        {
            yield return 0.8f;
            stateMachine.State = 3;
        }

        private IEnumerator DefeatedRoutine()
        {
            for (int i = 0; i < 12; i++)
            {
                CreateFireParticle();
                yield return 0.05f;
            }
            
            level?.Shake(0.2f);
            yield return 0.3f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private void CreateFireParticle()
        {
            var particle = new FireParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 16f - 8f, Calc.Random.NextFloat() * 16f - 8f),
                new Vector2(Calc.Random.NextFloat() * 60f - 30f, -Calc.Random.NextFloat() * 50f)
            );
            fireParticles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == MinionState.Defeated) return;
            
            Health -= damage;
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            for (int i = 0; i < 5; i++)
            {
                CreateFireParticle();
            }
            
            if (Health <= 0)
            {
                stateMachine.State = 6;
            }
            else
            {
                stateMachine.State = 5;
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
            fireParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            Draw.Rect(Position.X - 10, Position.Y - 24, 20, 24, Color.Orange * 0.3f);
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// AxisProjectile - Fire projectile from AxisMinion
    /// </summary>
    public class AxisProjectile : Actor
    {
        private Vector2 velocity;
        private float lifetime;

        public AxisProjectile(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 2f;
            
            Collider = new Hitbox(10f, 10f, -5f, -5f);
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
                return;
            }
            
            if (lifetime <= 0f || CollideCheck<Solid>())
                RemoveSelf();
        }

        public override void Render()
        {
            Draw.Circle(Position, 5f, Color.Orange * 0.8f, 5);
        }
    }

    /// <summary>
    /// FireParticle - Fire particle effect
    /// </summary>
    public class FireParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public FireParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.6f - 0.3f) + 0.3f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 80f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 4f, Color.Orange * (alpha * 0.6f), 4);
        }
    }
}
