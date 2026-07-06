namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// EchoFlowerEntity - Flower that repeats player's last dash as a delayed projectile
    /// Creates a "echo" of the player's dash direction after a delay
    /// Sprite path: objects/DZ/DZ/echo_flower/
    /// </summary>
    [CustomEntity("DZ/EchoFlowerEntity")]
    [Tracked]
    public class EchoFlowerEntity : Actor
    {
        #region Enums
        public enum FlowerState
        {
            Idle,
            Recording,
            Charging,
            Firing,
            Cooldown
        }
        #endregion

        #region Properties
        public FlowerState State { get; private set; }
        public float EchoDelay { get; private set; }
        public float EchoSpeed { get; private set; }
        public float CooldownTime { get; private set; }
        public int MaxEchoes { get; private set; }
        
        private Sprite sprite;
        private Vector2 recordedDashDirection;
        private float chargeTimer;
        private float cooldownTimer;
        private int currentEchoCount;
        private Player lastPlayer;
        private Level level;
        private List<EchoProjectile> activeEchoes;
        private VertexLight flowerLight;
        private float pulseTimer;
        #endregion

        #region Constructor
        public EchoFlowerEntity(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("echoDelay", 0.5f),
                data.Float("echoSpeed", 200f),
                data.Float("cooldownTime", 1f),
                data.Int("maxEchoes", 3)
            );
        }

        public EchoFlowerEntity(Vector2 position, float echoDelay = 0.5f, float echoSpeed = 200f,
            float cooldownTime = 1f, int maxEchoes = 3)
            : base(position)
        {
            Initialize(echoDelay, echoSpeed, cooldownTime, maxEchoes);
        }

        private void Initialize(float echoDelay, float echoSpeed, float cooldownTime, int maxEchoes)
        {
            EchoDelay = echoDelay;
            EchoSpeed = echoSpeed;
            CooldownTime = cooldownTime;
            MaxEchoes = maxEchoes;
            
            State = FlowerState.Idle;
            recordedDashDirection = Vector2.Zero;
            chargeTimer = 0f;
            cooldownTimer = 0f;
            currentEchoCount = 0;
            pulseTimer = 0f;
            activeEchoes = new List<EchoProjectile>();
            
            // Setup collider
            Collider = new Hitbox(16f, 24f, -8f, -24f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("echo_flower"));
            sprite.Play("idle");
            
            // Add glow
            Add(flowerLight = new VertexLight(new Color(0.6f, 0.8f, 1f), 0.5f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void RecordDash(Vector2 direction)
        {
            if (State != FlowerState.Idle && State != FlowerState.Cooldown) return;
            if (direction == Vector2.Zero) return;
            
            recordedDashDirection = direction.SafeNormalize();
            State = FlowerState.Recording;
            sprite.Play("recording");
            
            Audio.Play("event:/game/general/diamond_get", Position);
            
            Add(new Coroutine(ChargeAndFire()));
        }
        #endregion

        #region Private Methods
        private IEnumerator ChargeAndFire()
        {
            // Charging phase
            State = FlowerState.Charging;
            sprite.Play("charging");
            chargeTimer = EchoDelay;
            
            // Visual charging effect
            while (chargeTimer > 0f)
            {
                chargeTimer -= Engine.DeltaTime;
                
                // Pulse effect
                float pulse = 1f - (chargeTimer / EchoDelay);
                flowerLight.Alpha = 0.5f + pulse * 0.5f;
                
                yield return null;
            }
            
            // Fire echo
            State = FlowerState.Firing;
            FireEcho();
            
            yield return 0.1f;
            
            // Cooldown
            State = FlowerState.Cooldown;
            sprite.Play("cooldown");
            cooldownTimer = CooldownTime;
            
            while (cooldownTimer > 0f)
            {
                cooldownTimer -= Engine.DeltaTime;
                yield return null;
            }
            
            // Return to idle
            State = FlowerState.Idle;
            sprite.Play("idle");
            flowerLight.Alpha = 0.5f;
        }

        private void FireEcho()
        {
            if (currentEchoCount >= MaxEchoes) return;
            
            var echo = new EchoProjectile(Position - Vector2.UnitY * 16f, recordedDashDirection * EchoSpeed);
            Scene.Add(echo);
            activeEchoes.Add(echo);
            currentEchoCount++;
            
            Audio.Play("event:/game/char_badeline/beam_launch", Position);
            
            // Flash effect
            level?.Flash(Color.Cyan * 0.2f);
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
            
            // Pulse animation when idle
            pulseTimer += Engine.DeltaTime * 2f;
            
            // Check for player dash nearby
            if (State == FlowerState.Idle || State == FlowerState.Cooldown)
            {
                lastPlayer = Scene.Tracker.GetEntity<Player>();
                if (lastPlayer != null)
                {
                    // Check if player is near the flower
                    if (Vector2.Distance(Position, lastPlayer.Position) < 50f)
                    {
                        // Record the player's movement direction
                        Vector2 dashDir = lastPlayer.Speed;
                        if (dashDir != Vector2.Zero)
                        {
                            RecordDash(dashDir);
                        }
                    }
                }
            }
            
            // Clean up destroyed echoes
            activeEchoes.RemoveAll(e => e == null || e.Scene == null);
            
            // Reset echo count if all echoes are gone
            if (activeEchoes.Count == 0 && State == FlowerState.Idle)
            {
                currentEchoCount = 0;
            }
        }

        public override void Render()
        {
            base.Render();
            
            // Draw charging indicator
            if (State == FlowerState.Charging)
            {
                float progress = 1f - (chargeTimer / EchoDelay);
                Draw.Circle(Position - Vector2.UnitY * 32f, 8f + progress * 8f, Color.Cyan * 0.5f, 8);
            }
        }
        #endregion
    }

    /// <summary>
    /// EchoProjectile - Projectile fired by EchoFlowerEntity
    /// Represents a "echo" of the player's dash
    /// Sprite path: projectiles/echo_projectile/
    /// </summary>
    public class EchoProjectile : Actor
    {
        #region Properties
        private Vector2 velocity;
        private float lifetime;
        private Sprite sprite;
        private float rotation;
        private TrailManager trail;
        #endregion

        #region Constructor
        public EchoProjectile(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 4f;
            rotation = Calc.Angle(velocity);
            
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Add(sprite = GFX.SpriteBank.Create("echo_projectile"));
            sprite.Rotation = rotation;
            
            // Add trail
            Add(trail = new TrailManager());
        }
        #endregion

        #region Entity Overrides
        public override void Update()
        {
            base.Update();
            
            // Move
            Position += velocity * Engine.DeltaTime;
            
            // Slow down slightly
            velocity *= 0.995f;
            
            lifetime -= Engine.DeltaTime;
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
                return;
            }
            
            // Check tile collision
            if (CollideCheck<Solid>())
            {
                CreateImpactEffect();
                RemoveSelf();
                return;
            }
            
            // Expire
            if (lifetime <= 0f)
            {
                CreateImpactEffect();
                RemoveSelf();
            }
        }

        private void CreateImpactEffect()
        {
            // Particle burst
            var level = Scene as Level;
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 6, Position, Vector2.One * 4f, Color.Cyan);
        }

        public override void Render()
        {
            // Draw glow
            Draw.Circle(Position, 10f, Color.Cyan * 0.3f, 8);
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// TrailManager - Simple trail effect for projectiles
    /// </summary>
    public class TrailManager : Component
    {
        private List<Vector2> trailPositions;
        private int maxTrailLength;

        public TrailManager(int maxTrailLength = 8) : base(true, true)
        {
            this.maxTrailLength = maxTrailLength;
            trailPositions = new List<Vector2>();
        }

        public override void Update()
        {
            base.Update();
            
            trailPositions.Insert(0, Entity.Position);
            
            while (trailPositions.Count > maxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }

        public override void Render()
        {
            base.Render();
            
            for (int i = 0; i < trailPositions.Count; i++)
            {
                float alpha = 1f - ((float)i / trailPositions.Count);
                Draw.Circle(trailPositions[i], 4f, Color.Cyan * (alpha * 0.3f), 4);
            }
        }
    }
}
