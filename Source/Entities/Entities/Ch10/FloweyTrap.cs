namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// FloweyTrap - Malicious flower that pops up and fires pellets in patterns
    /// Has multiple attack patterns based on proximity and can be defeated
    /// Sprite path: characters/DZ/flowey_trap/
    /// Pellet sprite path: characters/DZ/flowey_pellet/
    /// </summary>
    [CustomEntity("DZ/FloweyTrap")]
    [Tracked]
    public class FloweyTrap : Actor
    {
        #region Enums
        public enum FloweyState
        {
            Hidden,
            Emerging,
            Idle,
            Attacking,
            Retracting,
            Defeated
        }

        public enum AttackPattern
        {
            Circular,
            Aimed,
            Spread,
            Spiral
        }
        #endregion

        #region Properties
        public FloweyState State { get; private set; }
        public AttackPattern CurrentPattern { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float DetectionRange { get; private set; }
        public float RetractRange { get; private set; }
        public int PelletCount { get; private set; }
        public float PelletSpeed { get; private set; }
        public float AttackInterval { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private float attackCooldown;
        private float stateTimer;
        private int attackCount;
        private Player targetPlayer;
        private Level level;
        private List<FloweyPellet> activePellets;
        private Vector2 emergePosition;
        #endregion

        #region Constructor
        public FloweyTrap(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 2),
                data.Float("detectionRange", 120f),
                data.Float("retractRange", 180f),
                data.Int("pelletCount", 5),
                data.Float("pelletSpeed", 150f),
                data.Float("attackInterval", 1.5f),
                data.Enum("attackPattern", AttackPattern.Circular)
            );
        }

        public FloweyTrap(Vector2 position, int health = 2, float detectionRange = 120f,
            float retractRange = 180f, int pelletCount = 5, float pelletSpeed = 150f,
            float attackInterval = 1.5f, AttackPattern pattern = AttackPattern.Circular)
            : base(position)
        {
            Initialize(health, detectionRange, retractRange, pelletCount, pelletSpeed, attackInterval, pattern);
        }

        private void Initialize(int health, float detectionRange, float retractRange,
            int pelletCount, float pelletSpeed, float attackInterval, AttackPattern pattern)
        {
            Health = health;
            MaxHealth = health;
            DetectionRange = detectionRange;
            RetractRange = retractRange;
            PelletCount = pelletCount;
            PelletSpeed = pelletSpeed;
            AttackInterval = attackInterval;
            CurrentPattern = pattern;
            
            stateTimer = 0f;
            attackCooldown = 0f;
            attackCount = 0;
            activePellets = new List<FloweyPellet>();
            emergePosition = Position;
            
            // Setup collider
            Collider = new Hitbox(16f, 24f, -8f, -24f);
            
            // Setup sprite (starts hidden)
            Add(sprite = GFX.SpriteBank.Create("flowey_trap"));
            sprite.Visible = false;
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
            
            State = FloweyState.Hidden;
        }
        #endregion

        #region State Begin Methods
        private void HiddenBegin()
        {
            sprite.Visible = false;
            State = FloweyState.Hidden;
        }

        private void EmergingBegin()
        {
            sprite.Visible = true;
            sprite.Play("emerge");
            State = FloweyState.Emerging;
            Audio.Play("event:/game/char_maddy/jump", Position);
        }

        private void IdleBegin()
        {
            sprite.Play("idle_menacing");
            State = FloweyState.Idle;
            attackCooldown = AttackInterval;
        }

        private void AttackingBegin()
        {
            sprite.Play("attack");
            State = FloweyState.Attacking;
            attackCount = 0;
        }

        private void RetractingBegin()
        {
            sprite.Play("retract");
            State = FloweyState.Retracting;
            Audio.Play("event:/game/char_maddy/land", Position);
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = FloweyState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            // Clear all pellets
            ClearPellets();
        }
        #endregion

        #region State Routines
        private IEnumerator HiddenRoutine()
        {
            while (true)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                {
                    stateMachine.State = 1; // Emerging
                    yield break;
                }
                yield return null;
            }
        }

        private IEnumerator EmergingRoutine()
        {
            // Emerge animation
            yield return 0.5f;
            
            // Small screen shake
            level?.Shake(0.1f);
            
            stateMachine.State = 2; // Idle
        }

        private IEnumerator IdleRoutine()
        {
            while (attackCooldown > 0f)
            {
                attackCooldown -= Engine.DeltaTime;
                
                // Face the player
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null)
                {
                    sprite.Scale.X = targetPlayer.Position.X > Position.X ? 1 : -1;
                }
                
                yield return null;
            }
            
            stateMachine.State = 3; // Attacking
        }

        private IEnumerator AttackingRoutine()
        {
            targetPlayer = Scene.Tracker.GetEntity<Player>();
            
            // Fire pellets based on attack pattern
            switch (CurrentPattern)
            {
                case AttackPattern.Circular:
                    yield return FireCircularPattern();
                    break;
                case AttackPattern.Aimed:
                    yield return FireAimedPattern();
                    break;
                case AttackPattern.Spread:
                    yield return FireSpreadPattern();
                    break;
                case AttackPattern.Spiral:
                    yield return FireSpiralPattern();
                    break;
            }
            
            yield return 0.3f;
            
            // Check if player still in range
            targetPlayer = Scene.Tracker.GetEntity<Player>();
            if (targetPlayer == null || Vector2.Distance(Position, targetPlayer.Position) > RetractRange)
            {
                stateMachine.State = 4; // Retracting
            }
            else
            {
                stateMachine.State = 2; // Idle
            }
        }

        private IEnumerator RetractingRoutine()
        {
            yield return 0.5f;
            
            sprite.Visible = false;
            stateMachine.State = 0; // Hidden
        }

        private IEnumerator DefeatedRoutine()
        {
            // Death particles
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 8, Position - Vector2.UnitY * 12f, Vector2.One * 6f);
            
            yield return 0.8f;
            RemoveSelf();
        }
        #endregion

        #region Attack Patterns
        private IEnumerator FireCircularPattern()
        {
            for (int i = 0; i < PelletCount; i++)
            {
                float angle = (MathHelper.TwoPi / PelletCount) * i;
                FirePellet(angle);
                yield return 0.08f;
            }
        }

        private IEnumerator FireAimedPattern()
        {
            if (targetPlayer == null) yield break;
            
            Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
            float baseAngle = Calc.Angle(direction);
            
            // Fire 3 pellets: center and two offsets
            FirePellet(baseAngle);
            yield return 0.1f;
            FirePellet(baseAngle - 0.3f);
            yield return 0.1f;
            FirePellet(baseAngle + 0.3f);
        }

        private IEnumerator FireSpreadPattern()
        {
            float startAngle = -0.6f;
            float angleStep = 1.2f / (PelletCount - 1);
            
            for (int i = 0; i < PelletCount; i++)
            {
                float angle = startAngle + angleStep * i;
                FirePellet(angle);
            }
            yield return 0.1f;
        }

        private IEnumerator FireSpiralPattern()
        {
            float currentAngle = 0f;
            
            for (int i = 0; i < PelletCount * 2; i++)
            {
                currentAngle += 0.5f;
                FirePellet(currentAngle, PelletSpeed * 0.8f);
                yield return 0.1f;
            }
        }
        #endregion

        #region Pellet Management
        private void FirePellet(float angle, float? speed = null)
        {
            float pelletSpeed = speed ?? PelletSpeed;
            Vector2 spawnPos = Position - Vector2.UnitY * 16f;
            var pellet = new FloweyPellet(spawnPos, angle, pelletSpeed);
            Scene.Add(pellet);
            activePellets.Add(pellet);
            
            Audio.Play("event:/game/char_badeline/beam_launch", Position);
        }

        private void ClearPellets()
        {
            foreach (var pellet in activePellets)
            {
                if (pellet != null && pellet.Scene != null)
                {
                    pellet.RemoveSelf();
                }
            }
            activePellets.Clear();
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            Health -= damage;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 4, Position - Vector2.UnitY * 12f, Vector2.One * 4f);
            
            if (Health <= 0)
            {
                stateMachine.State = 5; // Defeated
            }
            else
            {
                // Brief stun - pause current state
                sprite.Play("hurt");
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
            
            // Clean up destroyed pellets
            activePellets.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Removed(Scene scene)
        {
            ClearPellets();
            base.Removed(scene);
        }
        #endregion
    }

    /// <summary>
    /// FloweyPellet - Projectile fired by FloweyTrap
    /// Small pellet that travels in a straight line
    /// Sprite path: characters/DZ/flowey_pellet/
    /// </summary>
    public class FloweyPellet : Actor
    {
        #region Properties
        private Vector2 velocity;
        private float lifetime;
        private Sprite sprite;
        private float rotation;
        #endregion

        #region Constructor
        public FloweyPellet(Vector2 position, float angle, float speed)
            : base(position)
        {
            velocity = Calc.AngleToVector(angle, speed);
            lifetime = 4f;
            rotation = angle;
            
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(sprite = GFX.SpriteBank.Create("flowey_pellet"));
        }
        #endregion

        #region Entity Overrides
        public override void Update()
        {
            base.Update();
            
            // Move
            Position += velocity * Engine.DeltaTime;
            
            // Rotate sprite
            rotation += Engine.DeltaTime * 5f;
            sprite.Rotation = rotation;
            
            // Lifetime
            lifetime -= Engine.DeltaTime;
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
                return;
            }
            
            // Check if off-screen or expired
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();
            
            // Draw pellet with slight glow
            Draw.Circle(Position, 4f, Color.Yellow * 0.3f, 4);
        }
        #endregion
    }
}
