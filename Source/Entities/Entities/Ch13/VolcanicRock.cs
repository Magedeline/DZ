namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// VolcanicRock - Falling rock hazard from ceiling
    /// Falls when player is below and creates impact crater
    /// Sprite path: objects/DZ/DZ/volcanic_rock/
    /// </summary>
    [CustomEntity("DZ/VolcanicRock")]
    [Tracked]
    public class VolcanicRock : Actor
    {
        #region Enums
        public enum RockState
        {
            Waiting,
            Shaking,
            Falling,
            Impact
        }
        #endregion

        #region Properties
        public RockState State { get; private set; }
        public float FallSpeed { get; private set; }
        public float DamageRadius { get; private set; }
        public float TriggerRange { get; private set; }
        
        private Sprite sprite;
        private Vector2 startPosition;
        private float shakeTimer;
        private Level level;
        private List<RockDebris> debris;
        private bool hasFallen;
        private VertexLight rockLight;
        #endregion

        #region Constructor
        public VolcanicRock(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("fallSpeed", 300f),
                data.Float("damageRadius", 60f),
                data.Float("triggerRange", 100f)
            );
        }

        public VolcanicRock(Vector2 position, float fallSpeed = 300f, float damageRadius = 60f,
            float triggerRange = 100f)
            : base(position)
        {
            Initialize(fallSpeed, damageRadius, triggerRange);
        }

        private void Initialize(float fallSpeed, float damageRadius, float triggerRange)
        {
            FallSpeed = fallSpeed;
            DamageRadius = damageRadius;
            TriggerRange = triggerRange;
            
            startPosition = Position;
            State = RockState.Waiting;
            shakeTimer = 0f;
            hasFallen = false;
            debris = new List<RockDebris>();
            
            Collider = new Hitbox(32f, 32f, -16f, -16f);
            
            Add(sprite = DZModule.SpriteBank.Create("volcanic_rock"));
            sprite.Play("waiting");
            
            Add(rockLight = new VertexLight(Color.Brown, 0.2f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void ForceFall()
        {
            if (State == RockState.Waiting || State == RockState.Shaking)
            {
                shakeTimer = 0f;
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
            
            var player = Scene.Tracker.GetEntity<Player>();
            
            switch (State)
            {
                case RockState.Waiting:
                    // Check for player in range
                    if (player != null && Vector2.Distance(Position, player.Position) < TriggerRange)
                    {
                        State = RockState.Shaking;
                        sprite.Play("shaking");
                        shakeTimer = 1f;
                        
                        Audio.Play("event:/game/gen_crumble_fall", Position);
                        level?.Shake(0.2f);
                    }
                    break;
                    
                case RockState.Shaking:
                    shakeTimer -= Engine.DeltaTime;
                    
                    // Shake effect
                    Position = startPosition + new Vector2(
                        Calc.Random.NextFloat() * 4f - 2f, Calc.Random.NextFloat() * 4f - 2f
                    );
                    
                    rockLight.Alpha = 0.2f + (float)Math.Sin(shakeTimer * 10f) * 0.2f;
                    
                    if (shakeTimer <= 0f)
                    {
                        State = RockState.Falling;
                        Position = startPosition;
                        sprite.Play("falling");
                        
                        Audio.Play("event:/game/general/crystalheart_pulse", Position);
                    }
                    break;
                    
                case RockState.Falling:
                    Position.Y += FallSpeed * Engine.DeltaTime;
                    
                    // Check for ground collision
                    if (OnGround())
                    {
                        Impact();
                    }
                    
                    // Check player collision while falling
                    if (player != null && Collide.Check(this, player))
                    {
                        player.Die(Vector2.Zero);
                    }
                    break;
                    
                case RockState.Impact:
                    // Wait for debris to clear
                    debris.RemoveAll(d => d == null || d.Scene == null);
                    
                    if (debris.Count == 0)
                    {
                        RemoveSelf();
                    }
                    break;
            }
        }

        private void Impact()
        {
            State = RockState.Impact;
            hasFallen = true;
            sprite.Play("impact");
            
            // Create debris
            for (int i = 0; i < 15; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                float speed = Calc.Random.NextFloat() * 100f + 100f;
                
                var rockDebris = new RockDebris(
                    Position,
                    Calc.AngleToVector(angle, speed)
                );
                debris.Add(rockDebris);
                Scene.Add(rockDebris);
            }
            
            // Damage nearby player
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < DamageRadius)
            {
                player.Die(Vector2.Zero);
            }
            
            // Screen effects
            level?.Shake(0.4f);
            level?.Flash(Color.Brown * 0.3f);
            Audio.Play("event:/game/char_maddy/land", Position);
        }

        public override void Render()
        {
            // Draw shadow when waiting/shaking
            if (State == RockState.Waiting || State == RockState.Shaking)
            {
                float shadowSize = 32f;
                float shadowAlpha = State == RockState.Shaking ? 0.3f + (float)Math.Sin(shakeTimer * 10f) * 0.2f : 0.3f;
                Draw.Circle(Position, shadowSize, Color.Black * shadowAlpha, 16);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// RockDebris - Debris from falling rock
    /// </summary>
    public class RockDebris : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float size;
        private float rotation;
        private float rotationSpeed;

        public RockDebris(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
            lifetime = maxLifetime;
            size = Calc.Random.NextFloat() * (12f - 6f) + 6f;
            rotation = Calc.Random.NextFloat() * MathHelper.TwoPi;
            rotationSpeed = Calc.Random.NextFloat() * 8f - 4f;
            
            Collider = new Hitbox(size, size, -size / 2, -size / 2);
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 300f * Engine.DeltaTime;
            rotation += rotationSpeed * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
                return;
            }
            
            if (lifetime <= 0f || OnGround())
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Rect(Position - new Vector2(size / 2, size / 2), size, size, Color.Brown * (alpha * 0.8f));
        }
    }

    /// <summary>
    /// RockSpawner - Spawns falling rocks periodically
    /// </summary>
    [CustomEntity("DZ/RockSpawner")]
    public class RockSpawner : Entity
    {
        private float spawnInterval;
        private float timer;
        private int maxRocks;
        private List<VolcanicRock> rocks;
        private Vector2 spawnPosition;

        public RockSpawner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            spawnInterval = data.Float("spawnInterval", 5f);
            maxRocks = data.Int("maxRocks", 3);
            timer = 0f;
            rocks = new List<VolcanicRock>();
            spawnPosition = Position;
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            // Clean up fallen rocks
            rocks.RemoveAll(r => r == null || r.Scene == null);
            
            if (timer >= spawnInterval && rocks.Count < maxRocks)
            {
                timer = 0f;
                SpawnRock();
            }
        }

        private void SpawnRock()
        {
            var rock = new VolcanicRock(spawnPosition);
            rocks.Add(rock);
            Scene.Add(rock);
        }
    }
}

