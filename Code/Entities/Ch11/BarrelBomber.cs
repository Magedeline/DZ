namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// BarrelBomber - Enemy that hides in barrels and explodes when player is near
    /// Can be destroyed before exploding by shooting or attacking
    /// Sprite path: characters/barrel_bomber/
    /// </summary>
    [CustomEntity("MaggyHelper/BarrelBomber")]
    [Tracked]
    public class BarrelBomber : Actor
    {
        #region Enums
        public enum BomberState
        {
            Hidden,
            Peeking,
            Emerging,
            Fusing,
            Exploding,
            Destroyed
        }
        #endregion

        #region Properties
        public BomberState State { get; private set; }
        public int Health { get; private set; }
        public float DetectionRange { get; private set; }
        public float ExplosionRadius { get; private set; }
        public float FuseTime { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite barrelSprite;
        private Sprite enemySprite;
        private StateMachine stateMachine;
        private float fuseTimer;
        private Player targetPlayer;
        private Level level;
        private List<ExplosionParticle> explosionParticles;
        private bool hasDetonated;
        private VertexLight fuseLight;
        private float flickerTimer;
        #endregion

        #region Constructor
        public BarrelBomber(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 1),
                data.Float("detectionRange", 80f),
                data.Float("explosionRadius", 100f),
                data.Float("fuseTime", 1.5f)
            );
        }

        public BarrelBomber(Vector2 position, int health = 1, float detectionRange = 80f,
            float explosionRadius = 100f, float fuseTime = 1.5f)
            : base(position)
        {
            Initialize(health, detectionRange, explosionRadius, fuseTime);
        }

        private void Initialize(int health, float detectionRange, float explosionRadius, float fuseTime)
        {
            Health = health;
            DetectionRange = detectionRange;
            ExplosionRadius = explosionRadius;
            FuseTime = fuseTime;
            
            State = BomberState.Hidden;
            fuseTimer = 0f;
            hasDetonated = false;
            flickerTimer = 0f;
            explosionParticles = new List<ExplosionParticle>();
            
            // Setup collider - barrel size
            Collider = new Hitbox(24f, 32f, -12f, -32f);
            
            // Setup sprites
            Add(barrelSprite = GFX.SpriteBank.Create("barrel"));
            barrelSprite.Play("closed");
            
            enemySprite = GFX.SpriteBank.Create("barrel_bomber");
            enemySprite.Position = new Vector2(0f, -20f);
            enemySprite.Visible = false;
            Add(enemySprite);
            
            // Add fuse light
            Add(fuseLight = new VertexLight(Color.Orange, 0f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void Peek()
        {
            if (State != BomberState.Hidden) return;
            
            State = BomberState.Peeking;
            barrelSprite.Play("peek");
            enemySprite.Visible = true;
            enemySprite.Play("peeking");
            
            Add(new Coroutine(PeekRoutine()));
        }

        public void TriggerExplosion()
        {
            if (State == BomberState.Exploding || State == BomberState.Destroyed) return;
            
            State = BomberState.Fusing;
            fuseTimer = FuseTime;
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void Destroy()
        {
            if (State == BomberState.Destroyed) return;
            
            State = BomberState.Destroyed;
            barrelSprite.Play("destroyed");
            enemySprite.Visible = false;
            
            // Small explosion
            CreateSmallExplosion();
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            Add(new Coroutine(DestroyRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator PeekRoutine()
        {
            yield return 0.5f;
            
            // Check if player is close enough to trigger
            targetPlayer = Scene.Tracker.GetEntity<Player>();
            if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
            {
                State = BomberState.Emerging;
                barrelSprite.Play("open");
                enemySprite.Play("emerging");
                
                yield return 0.3f;
                
                TriggerExplosion();
            }
            else
            {
                // Go back to hiding
                State = BomberState.Hidden;
                barrelSprite.Play("closed");
                enemySprite.Visible = false;
            }
        }

        private IEnumerator DestroyRoutine()
        {
            yield return 0.5f;
            RemoveSelf();
        }

        private void Explode()
        {
            State = BomberState.Exploding;
            hasDetonated = true;
            
            // Create explosion particles
            for (int i = 0; i < 30; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                float speed = Calc.Random.NextFloat() * 100f + 100f;
                
                var particle = new ExplosionParticle(
                    Position,
                    Calc.AngleToVector(angle, speed)
                );
                explosionParticles.Add(particle);
                Scene.Add(particle);
            }
            
            // Screen effects
            level?.Shake(0.6f);
            level?.Flash(Color.Orange * 0.5f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            // Damage player if in radius
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < ExplosionRadius)
            {
                player.Die(Vector2.Zero);
            }
            
            // Remove self
            RemoveSelf();
        }

        private void CreateSmallExplosion()
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                float speed = Calc.Random.NextFloat() * 50f + 50f;
                
                var particle = new ExplosionParticle(
                    Position,
                    Calc.AngleToVector(angle, speed)
                );
                explosionParticles.Add(particle);
                Scene.Add(particle);
            }
            
            level?.Shake(0.2f);
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
            
            // Check for player nearby when hidden
            if (State == BomberState.Hidden)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange * 1.5f)
                {
                    Peek();
                }
            }
            
            // Fuse countdown
            if (State == BomberState.Fusing)
            {
                fuseTimer -= Engine.DeltaTime;
                
                // Flickering light effect
                flickerTimer += Engine.DeltaTime * 15f;
                fuseLight.Alpha = 0.5f + (float)Math.Sin(flickerTimer) * 0.3f;
                fuseLight.Color = Color.Orange;
                
                if (fuseTimer <= 0f)
                {
                    Explode();
                }
            }
            
            explosionParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw explosion radius indicator when fusing
            if (State == BomberState.Fusing)
            {
                float flash = (float)Math.Sin(flickerTimer * 0.5f) * 0.5f + 0.5f;
                Draw.Circle(Position, ExplosionRadius, Color.Red * (flash * 0.2f), 24);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// ExplosionParticle - Particle for explosion effects
    /// </summary>
    public class ExplosionParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private float scale;

        public ExplosionParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.3f) + 0.3f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.5f - 0.5f) + 0.5f;
            
            Color[] colors = { Color.Orange, Color.Red, Color.Yellow, Color.OrangeRed };
            color = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.95f;
            velocity.Y += 100f * Engine.DeltaTime;
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 8f * scale, color * (alpha * 0.7f), 6);
        }
    }

    /// <summary>
    /// ExplosiveBarrel - Destructible barrel that can be shot
    /// </summary>
    [CustomEntity("MaggyHelper/ExplosiveBarrel")]
    public class ExplosiveBarrel : Actor
    {
        private Sprite sprite;
        private int health;
        private float explosionRadius;
        private Level level;

        public ExplosiveBarrel(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            health = data.Int("health", 1);
            explosionRadius = data.Float("explosionRadius", 80f);
            
            Collider = new Hitbox(24f, 32f, -12f, -32f);
            Add(sprite = GFX.SpriteBank.Create("explosive_barrel"));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public void Damage(int amount)
        {
            health -= amount;
            
            if (health <= 0)
            {
                Explode();
            }
        }

        private void Explode()
        {
            // Create explosion
            for (int i = 0; i < 20; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                float speed = Calc.Random.NextFloat() * 80f + 80f;
                
                var particle = new ExplosionParticle(
                    Position,
                    Calc.AngleToVector(angle, speed)
                );
                Scene.Add(particle);
            }
            
            level?.Shake(0.4f);
            level?.Flash(Color.Orange * 0.4f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            // Damage nearby entities
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < explosionRadius)
            {
                player.Die(Vector2.Zero);
            }
            
            RemoveSelf();
        }
    }
}

