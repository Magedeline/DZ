namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// PixelEnemy - Low-resolution enemy made of pixels
    /// Moves in grid patterns and fires pixel projectiles
    /// Sprite path: characters/DZ/pixel_enemy/
    /// </summary>
    [CustomEntity("DZ/PixelEnemy")]
    [Tracked]
    public class PixelEnemy : Actor
    {
        #region Enums
        public enum PixelState
        {
            Idle,
            Patrolling,
            Alert,
            Shooting,
            Dashing,
            Glitching,
            Defeated
        }

        public enum EnemyType
        {
            Walker,
            Shooter,
            Dasher,
            Glitcher
        }
        #endregion

        #region Properties
        public PixelState State { get; private set; }
        public EnemyType Type { get; private set; }
        public int Health { get; private set; }
        public float MoveSpeed { get; private set; }
        public float DetectionRange { get; private set; }
        public int GridSize { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 gridPosition;
        private Vector2 targetGridPosition;
        private Facings facing;
        private float shootCooldown;
        private Player targetPlayer;
        private Level level;
        private List<PixelProjectile> projectiles;
        private List<PixelParticle> particles;
        #endregion

        #region Constructor
        public PixelEnemy(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Enum("enemyType", EnemyType.Walker),
                data.Int("health", 2),
                data.Float("moveSpeed", 60f),
                data.Float("detectionRange", 150f),
                data.Int("gridSize", 8)
            );
        }

        public PixelEnemy(Vector2 position, EnemyType type = EnemyType.Walker, int health = 2,
            float moveSpeed = 60f, float detectionRange = 150f, int gridSize = 8)
            : base(position)
        {
            Initialize(type, health, moveSpeed, detectionRange, gridSize);
        }

        private void Initialize(EnemyType type, int health, float moveSpeed, float detectionRange, int gridSize)
        {
            Type = type;
            Health = health;
            MoveSpeed = moveSpeed;
            DetectionRange = detectionRange;
            GridSize = gridSize;
            
            gridPosition = Position / gridSize;
            targetGridPosition = gridPosition;
            facing = Facings.Right;
            shootCooldown = 0f;
            projectiles = new List<PixelProjectile>();
            particles = new List<PixelParticle>();
            
            State = PixelState.Idle;
            
            Collider = new Hitbox(16f, 16f, -8f, -16f);
            
            Add(sprite = GFX.SpriteBank.Create("pixel_enemy"));
            sprite.Play("idle");
            
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void IdleBegin()
        {
            sprite.Play("idle");
            State = PixelState.Idle;
        }

        private void PatrollingBegin()
        {
            sprite.Play("walk");
            State = PixelState.Patrolling;
        }

        private void AlertBegin()
        {
            sprite.Play("alert");
            State = PixelState.Alert;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void ShootingBegin()
        {
            sprite.Play("shoot");
            State = PixelState.Shooting;
        }

        private void DashingBegin()
        {
            sprite.Play("dash");
            State = PixelState.Dashing;
            Audio.Play("event:/game/char_maddy/dash", Position);
        }

        private void GlitchingBegin()
        {
            sprite.Play("glitch");
            State = PixelState.Glitching;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = PixelState.Defeated;
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
            while (true)
            {
                // Grid-based movement
                Vector2 moveDir = new Vector2((int)facing, 0);
                Vector2 nextPos = Position + moveDir * GridSize;
                
                // Check if can move
                if (!CollideCheck<Solid>(nextPos))
                {
                    Position = nextPos;
                }
                else
                {
                    // Reverse direction
                    facing = facing == Facings.Right ? Facings.Left : Facings.Right;
                    sprite.Scale.X = facing == Facings.Right ? 1 : -1;
                }
                
                // Check for player
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                {
                    stateMachine.State = 2;
                    yield break;
                }
                
                yield return GridSize / MoveSpeed;
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
            
            yield return 0.3f;
            
            // Choose action based on type
            switch (Type)
            {
                case EnemyType.Shooter:
                    stateMachine.State = 3;
                    break;
                case EnemyType.Dasher:
                    stateMachine.State = 4;
                    break;
                case EnemyType.Glitcher:
                    stateMachine.State = 5;
                    break;
                default:
                    stateMachine.State = 1;
                    break;
            }
        }

        private IEnumerator ShootingRoutine()
        {
            // Fire projectile
            Vector2 shootDir = new Vector2((int)facing, 0);
            var projectile = new PixelProjectile(Position, shootDir * 150f);
            projectiles.Add(projectile);
            Scene.Add(projectile);
            
            Audio.Play("event:/game/char_badeline/beam_launch", Position);
            
            yield return 0.5f;
            
            shootCooldown = 1f;
            stateMachine.State = 2;
        }

        private IEnumerator DashingRoutine()
        {
            // Quick dash toward player
            if (targetPlayer != null)
            {
                Vector2 dashDir = (targetPlayer.Position - Position).SafeNormalize();
                dashDir.Y = 0;
                dashDir.Normalize();
                
                float dashSpeed = 300f;
                float dashTime = 0.3f;
                float timer = 0f;
                
                while (timer < dashTime)
                {
                    timer += Engine.DeltaTime;
                    Position += dashDir * dashSpeed * Engine.DeltaTime;
                    
                    // Create trail
                    CreatePixelParticle();
                    
                    // Check player collision
                    if (targetPlayer != null && Collide.Check(this, targetPlayer))
                    {
                        targetPlayer.Die(Vector2.Zero);
                    }
                    
                    yield return null;
                }
            }
            
            yield return 0.5f;
            stateMachine.State = 2;
        }

        private IEnumerator GlitchingRoutine()
        {
            // Teleport around randomly
            for (int i = 0; i < 5; i++)
            {
                // Glitch effect
                for (int j = 0; j < 5; j++)
                {
                    CreatePixelParticle();
                }
                
                // Random teleport
                Vector2 newPos = Position + new Vector2(
                    Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 100f - 50f
                );
                
                if (!CollideCheck<Solid>(newPos))
                {
                    Position = newPos;
                }
                
                level?.Shake(0.1f);
                
                // Check player collision
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    player.Die(Vector2.Zero);
                }
                
                yield return 0.2f;
            }
            
            stateMachine.State = 2;
        }

        private IEnumerator DefeatedRoutine()
        {
            // Explode into pixels
            for (int i = 0; i < 20; i++)
            {
                CreatePixelParticle();
            }
            
            level?.Shake(0.2f);
            
            yield return 0.3f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private void CreatePixelParticle()
        {
            var particle = new PixelParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 16f - 8f, Calc.Random.NextFloat() * 16f - 8f),
                new Vector2(Calc.Random.NextFloat() * 100f - 50f, -Calc.Random.NextFloat() * 50f)
            );
            particles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == PixelState.Defeated) return;
            
            Health -= damage;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            for (int i = 0; i < 5; i++)
            {
                CreatePixelParticle();
            }
            
            if (Health <= 0)
            {
                stateMachine.State = 6;
            }
            else
            {
                // Brief stagger
                stateMachine.State = 0;
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
            
            if (shootCooldown > 0f)
                shootCooldown -= Engine.DeltaTime;
            
            projectiles.RemoveAll(p => p == null || p.Scene == null);
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw pixelated appearance
            Draw.Rect(Position.X - 8, Position.Y - 16, 16, 16, Color.Magenta * 0.3f);
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// PixelProjectile - Small pixel projectile
    /// </summary>
    public class PixelProjectile : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private Color color;

        public PixelProjectile(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 2f;
            color = Color.Magenta;
            
            Collider = new Hitbox(8f, 8f, -4f, -4f);
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
            Draw.Rect(Position.X - 4, Position.Y - 4, 8, 8, color);
        }
    }

    /// <summary>
    /// PixelParticle - Pixel particle effect
    /// </summary>
    public class PixelParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private int size;

        public PixelParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.6f - 0.3f) + 0.3f;
            lifetime = maxLifetime;
            color = Calc.Random.Next(2) == 0 ? Color.Magenta : Color.Cyan;
            size = Calc.Random.Next(2, 6);
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 200f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Rect(Position, size, size, color * (alpha * 0.7f));
        }
    }
}
