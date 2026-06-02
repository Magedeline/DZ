namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// FallenLeafPile - Decorative leaf pile that hides hazards or enemies
    /// Rustles when player approaches, can contain hidden spikes or enemies
    /// Sprite path: objects/fallen_leaf_pile/
    /// </summary>
    [CustomEntity("MaggyHelper/FallenLeafPile")]
    [Tracked]
    public class FallenLeafPile : Actor
    {
        #region Enums
        public enum HiddenContentType
        {
            Nothing,
            Spikes,
            Enemy,
            Collectible,
            SecretPath
        }

        public enum LeafState
        {
            Idle,
            Rustling,
            Revealing,
            Revealed
        }
        #endregion

        #region Properties
        public HiddenContentType HiddenContent { get; private set; }
        public LeafState State { get; private set; }
        public float DetectionRange { get; private set; }
        public bool IsRevealed => State == LeafState.Revealed;
        
        private Sprite sprite;
        private float rustleTimer;
        private float rustleIntensity;
        private Player nearbyPlayer;
        private Entity hiddenEntity;
        private Level level;
        private List<LeafParticle> leafParticles;
        private bool hasSpawnedContent;
        private string enemyType;
        private string collectibleType;
        #endregion

        #region Constructor
        public FallenLeafPile(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Enum("hiddenContent", HiddenContentType.Nothing),
                data.Float("detectionRange", 40f),
                data.Attr("enemyType", "MaggyHelper/RuinsSentinel"),
                data.Attr("collectibleType", "")
            );
        }

        public FallenLeafPile(Vector2 position, HiddenContentType hiddenContent = HiddenContentType.Nothing,
            float detectionRange = 40f, string enemyType = "MaggyHelper/RuinsSentinel", string collectibleType = "")
            : base(position)
        {
            Initialize(hiddenContent, detectionRange, enemyType, collectibleType);
        }

        private void Initialize(HiddenContentType hiddenContent, float detectionRange, string enemyType, string collectibleType)
        {
            HiddenContent = hiddenContent;
            DetectionRange = detectionRange;
            this.enemyType = enemyType;
            this.collectibleType = collectibleType;
            
            State = LeafState.Idle;
            rustleTimer = 0f;
            rustleIntensity = 0f;
            hasSpawnedContent = false;
            leafParticles = new List<LeafParticle>();
            
            // Setup collider - leaf pile on ground
            Collider = new Hitbox(48f, 16f, -24f, -16f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("fallen_leaf_pile"));
            sprite.Play("idle");
        }
        #endregion

        #region Public Methods
        public void Reveal()
        {
            if (State == LeafState.Revealed) return;
            
            State = LeafState.Revealing;
            sprite.Play("reveal");
            
            // Spawn hidden content
            if (!hasSpawnedContent)
            {
                SpawnHiddenContent();
                hasSpawnedContent = true;
            }
            
            // Create leaf scatter effect
            CreateLeafScatter();
            
            Audio.Play("event:/game/general/diamond_get", Position);
            
            Add(new Coroutine(RevealRoutine()));
        }

        public void Rustle(float intensity)
        {
            if (State == LeafState.Revealed) return;
            
            rustleIntensity = intensity;
            rustleTimer = 0.5f;
            
            if (State == LeafState.Idle)
            {
                State = LeafState.Rustling;
                sprite.Play("rustle");
            }
        }
        #endregion

        #region Private Methods
        private void SpawnHiddenContent()
        {
            switch (HiddenContent)
            {
                case HiddenContentType.Spikes:
                    hiddenEntity = new HiddenSpike(Position);
                    Scene.Add(hiddenEntity);
                    break;
                    
                case HiddenContentType.Enemy:
                    // Create enemy from type string
                    hiddenEntity = CreateEnemy();
                    if (hiddenEntity != null)
                    {
                        Scene.Add(hiddenEntity);
                    }
                    break;
                    
                case HiddenContentType.Collectible:
                    hiddenEntity = CreateCollectible();
                    if (hiddenEntity != null)
                    {
                        Scene.Add(hiddenEntity);
                    }
                    break;
                    
                case HiddenContentType.SecretPath:
                    // Remove collision to reveal path
                    Collider = null;
                    break;
            }
        }

        private Entity CreateEnemy()
        {
            // Simple enemy creation - would need proper entity factory
            // For now, return null and handle via map placement
            return null;
        }

        private Entity CreateCollectible()
        {
            // Create collectible based on type
            return new HiddenGem(Position);
        }

        private void CreateLeafScatter()
        {
            for (int i = 0; i < 12; i++)
            {
                var particle = new LeafParticle(
                    Position + new Vector2(Calc.Random.NextFloat() * 56f - 8f, Calc.Random.NextFloat() * 56f - 8f),
                    new Vector2(Calc.Random.NextFloat() * 70f + 80f, Calc.Random.NextFloat() * 70f + 80f)
                );
                leafParticles.Add(particle);
                Scene.Add(particle);
            }
        }

        private IEnumerator RevealRoutine()
        {
            yield return 0.5f;
            State = LeafState.Revealed;
            sprite.Play("revealed");
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
            
            // Check for nearby player
            nearbyPlayer = Scene.Tracker.GetEntity<Player>();
            if (nearbyPlayer != null && State != LeafState.Revealed)
            {
                float distance = Vector2.Distance(Position, nearbyPlayer.Position);
                
                // Rustle when player is near
                if (distance < DetectionRange * 2f)
                {
                    float intensity = 1f - (distance / (DetectionRange * 2f));
                    Rustle(intensity);
                }
                
                // Reveal when player touches
                if (distance < DetectionRange && Collide.Check(this, nearbyPlayer))
                {
                    Reveal();
                }
            }
            
            // Handle rustling animation
            if (State == LeafState.Rustling)
            {
                rustleTimer -= Engine.DeltaTime;
                if (rustleTimer <= 0f && nearbyPlayer == null)
                {
                    State = LeafState.Idle;
                    sprite.Play("idle");
                }
            }
        }

        public override void Render()
        {
            base.Render();
            
            // Draw shadow
            Draw.Rect(Collider.Bounds.Left, Collider.Bounds.Bottom - 2, Collider.Width, 2, Color.Black * 0.2f);
        }
        #endregion
    }

    /// <summary>
    /// LeafParticle - Particle effect for leaf scatter
    /// </summary>
    public class LeafParticle : Actor
    {
        private Vector2 velocity;
        private float rotation;
        private float rotationSpeed;
        private float lifetime;
        private Color leafColor;
        private float scale;

        public LeafParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            rotation = Calc.Random.NextFloat() * (MathHelper.TwoPi - 10f) + 10f;
            rotationSpeed = Calc.Random.NextFloat() * (MathHelper.TwoPi + 10f) - 10f;
            lifetime = Calc.Random.NextFloat() * (2f - 1f) + 1f;
            scale = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
            
            // Random autumn color
            Color[] colors = { Color.Orange, Color.Red, Color.Yellow, Color.Brown, Color.OrangeRed };
            leafColor = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            // Apply gravity
            velocity.Y += 200f * Engine.DeltaTime;
            
            // Move
            Position += velocity * Engine.DeltaTime;
            
            // Rotate
            rotation += rotationSpeed * Engine.DeltaTime;
            
            // Fade out
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f || OnGround())
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            // Draw simple leaf shape
            float alpha = Math.Min(1f, lifetime * 2f);
            Draw.Rect(Position - new Vector2(4, 2) * scale, 8 * scale, 4 * scale, leafColor * alpha);
        }
    }

    /// <summary>
    /// HiddenSpike - Spike that was hidden under leaf pile
    /// </summary>
    public class HiddenSpike : Actor
    {
        private Sprite sprite;
        private float appearTimer;

        public HiddenSpike(Vector2 position)
            : base(position)
        {
            Collider = new Hitbox(16f, 8f, -8f, -8f);
            appearTimer = 0f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(sprite = GFX.SpriteBank.Create("hidden_spike"));
            sprite.Play("appear");
        }

        public override void Update()
        {
            base.Update();
            
            appearTimer += Engine.DeltaTime;
            
            // Check player collision after brief delay
            if (appearTimer > 0.2f)
            {
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    player.Die(Vector2.Zero);
                }
            }
        }
    }

    /// <summary>
    /// HiddenGem - Collectible gem hidden under leaf pile
    /// </summary>
    public class HiddenGem : Actor
    {
        private Sprite sprite;
        private bool collected;
        private float bounceTimer;

        public HiddenGem(Vector2 position)
            : base(position)
        {
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            collected = false;
            bounceTimer = 0f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(sprite = GFX.SpriteBank.Create("hidden_gem"));
            Add(new VertexLight(Color.Gold, 1f, 8, 16));
        }

        public override void Update()
        {
            base.Update();
            
            if (collected) return;
            
            // Bounce animation
            bounceTimer += Engine.DeltaTime * 3f;
            sprite.Y = (float)Math.Sin(bounceTimer) * 3f;
            
            // Check collection
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                Collect();
            }
        }

        private void Collect()
        {
            collected = true;
            Audio.Play("event:/game/general/diamond_get", Position);
            RemoveSelf();
        }
    }
}
