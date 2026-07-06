namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// LavaBubble - Rising lava bubble that bursts and damages player
    /// Spawns from lava pools and creates hazards
    /// Sprite path: objects/DZ/DZ/lava_bubble/
    /// </summary>
    [CustomEntity("DZ/LavaBubble")]
    [Tracked]
    public class LavaBubble : Actor
    {
        #region Enums
        public enum BubbleState
        {
            Forming,
            Rising,
            Bursting,
            Defeated
        }
        #endregion

        #region Properties
        public BubbleState State { get; private set; }
        public float RiseSpeed { get; private set; }
        public float BurstHeight { get; private set; }
        public float DamageRadius { get; private set; }
        
        private Sprite sprite;
        private float stateTimer;
        private Level level;
        private List<LavaDroplet> droplets;
        private float currentHeight;
        private float wobbleTimer;
        private VertexLight bubbleLight;
        #endregion

        #region Constructor
        public LavaBubble(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("riseSpeed", 80f),
                data.Float("burstHeight", 100f),
                data.Float("damageRadius", 50f)
            );
        }

        public LavaBubble(Vector2 position, float riseSpeed = 80f, float burstHeight = 100f, float damageRadius = 50f)
            : base(position)
        {
            Initialize(riseSpeed, burstHeight, damageRadius);
        }

        private void Initialize(float riseSpeed, float burstHeight, float damageRadius)
        {
            RiseSpeed = riseSpeed;
            BurstHeight = burstHeight;
            DamageRadius = damageRadius;
            
            State = BubbleState.Forming;
            stateTimer = 0f;
            currentHeight = 0f;
            wobbleTimer = 0f;
            droplets = new List<LavaDroplet>();
            
            Collider = new Hitbox(20f, 20f, -10f, -10f);
            
            Add(sprite = GFX.SpriteBank.Create("lava_bubble"));
            sprite.Play("forming");
            
            Add(bubbleLight = new VertexLight(Color.Orange, 0.5f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void Burst()
        {
            State = BubbleState.Bursting;
            sprite.Play("bursting");
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.2f);
            
            Add(new Coroutine(BurstRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator BurstRoutine()
        {
            // Create lava droplets
            for (int i = 0; i < 12; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                float speed = Calc.Random.NextFloat() * 100f + 100f;
                
                var droplet = new LavaDroplet(
                    Position,
                    Calc.AngleToVector(angle, speed)
                );
                droplets.Add(droplet);
                Scene.Add(droplet);
            }
            
            // Damage nearby player
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < DamageRadius)
            {
                player.Die(Vector2.Zero);
            }
            
            yield return 0.3f;
            
            State = BubbleState.Defeated;
            RemoveSelf();
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
            
            switch (State)
            {
                case BubbleState.Forming:
                    stateTimer += Engine.DeltaTime;
                    if (stateTimer >= 0.5f)
                    {
                        State = BubbleState.Rising;
                        sprite.Play("rising");
                    }
                    break;
                    
                case BubbleState.Rising:
                    currentHeight += RiseSpeed * Engine.DeltaTime;
                    Position.Y -= RiseSpeed * Engine.DeltaTime;
                    
                    // Wobble effect
                    wobbleTimer += Engine.DeltaTime * 5f;
                    Position.X += (float)Math.Sin(wobbleTimer) * 0.5f;
                    
                    // Check if reached burst height
                    if (currentHeight >= BurstHeight)
                    {
                        Burst();
                    }
                    
                    // Check player collision
                    var player = Scene.Tracker.GetEntity<Player>();
                    if (player != null && Collide.Check(this, player))
                    {
                        player.Die(Vector2.Zero);
                    }
                    break;
            }
            
            droplets.RemoveAll(d => d == null || d.Scene == null);
        }

        public override void Render()
        {
            // Draw glow
            if (State == BubbleState.Rising)
            {
                Draw.Circle(Position, 15f, Color.Orange * 0.3f, 8);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// LavaDroplet - Droplet from bursting lava bubble
    /// </summary>
    public class LavaDroplet : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public LavaDroplet(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
            lifetime = maxLifetime;
            
            Collider = new Hitbox(8f, 8f, -4f, -4f);
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 200f * Engine.DeltaTime;
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
            Draw.Circle(Position, 4f, Color.Orange * (alpha * 0.7f), 4);
        }
    }

    /// <summary>
    /// LavaBubbleSpawner - Spawns lava bubbles periodically
    /// </summary>
    [CustomEntity("DZ/LavaBubbleSpawner")]
    public class LavaBubbleSpawner : Entity
    {
        private float spawnInterval;
        private float timer;
        private int maxBubbles;
        private List<LavaBubble> bubbles;

        public LavaBubbleSpawner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            spawnInterval = data.Float("spawnInterval", 2f);
            maxBubbles = data.Int("maxBubbles", 3);
            timer = 0f;
            bubbles = new List<LavaBubble>();
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            // Clean up defeated bubbles
            bubbles.RemoveAll(b => b == null || b.Scene == null);
            
            if (timer >= spawnInterval && bubbles.Count < maxBubbles)
            {
                timer = 0f;
                SpawnBubble();
            }
        }

        private void SpawnBubble()
        {
            var bubble = new LavaBubble(Position);
            bubbles.Add(bubble);
            Scene.Add(bubble);
        }
    }
}

