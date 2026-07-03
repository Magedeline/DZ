namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// MagmaPool - Lava pool that kills on contact
    /// Instant death hazard with bubbling effects
    /// Sprite path: objects/DZ/DZ/magma_pool/
    /// </summary>
    [CustomEntity("DZ/MagmaPool")]
    [Tracked]
    public class MagmaPool : Actor
    {
        #region Enums
        public enum PoolState
        {
            Idle,
            Bubbling,
            Erupting
        }
        #endregion

        #region Properties
        public PoolState State { get; private set; }
        public float BubbleInterval { get; private set; }
        public float EruptInterval { get; private set; }
        public bool IsInstantDeath { get; private set; }
        
        private Sprite sprite;
        private Rectangle poolArea;
        private float bubbleTimer;
        private float eruptTimer;
        private Level level;
        private List<MagmaBubble> bubbles;
        private List<MagmaParticle> magmaParticles;
        private VertexLight magmaLight;
        #endregion

        #region Constructor
        public MagmaPool(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("bubbleInterval", 0.5f),
                data.Float("eruptInterval", 3f),
                data.Bool("isInstantDeath", true)
            );
        }

        public MagmaPool(Vector2 position, int width, int height, float bubbleInterval = 0.5f,
            float eruptInterval = 3f, bool isInstantDeath = true)
            : base(position)
        {
            Initialize(bubbleInterval, eruptInterval, isInstantDeath);
            
            poolArea = new Rectangle((int)position.X, (int)position.Y, width, height);
            Collider = new Hitbox(width, height);
        }

        private void Initialize(float bubbleInterval, float eruptInterval, bool isInstantDeath)
        {
            BubbleInterval = bubbleInterval;
            EruptInterval = eruptInterval;
            IsInstantDeath = isInstantDeath;
            
            State = PoolState.Idle;
            bubbleTimer = 0f;
            eruptTimer = 0f;
            bubbles = new List<MagmaBubble>();
            magmaParticles = new List<MagmaParticle>();
            
            Add(sprite = DZModule.SpriteBank.Create("magma_pool"));
            sprite.Play("idle");
            
            Add(magmaLight = new VertexLight(Color.OrangeRed, 0.5f, 16, 40));
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
            
            bubbleTimer += Engine.DeltaTime;
            eruptTimer += Engine.DeltaTime;
            
            // Create bubbles
            if (bubbleTimer >= BubbleInterval)
            {
                bubbleTimer = 0f;
                CreateBubble();
            }
            
            // Erupt periodically
            if (eruptTimer >= EruptInterval)
            {
                eruptTimer = 0f;
                Erupt();
            }
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && poolArea.Contains(new Point((int)player.Position.X, (int)player.Position.Y)))
            {
                if (IsInstantDeath)
                {
                    player.Die(Vector2.Zero);
                }
                else
                {
                    // Push player up and damage
                    player.Speed.Y = -200f;
                    // Would apply damage here
                }
            }
            
            bubbles.RemoveAll(b => b == null || b.Scene == null);
            magmaParticles.RemoveAll(m => m == null || m.Scene == null);
        }

        private void CreateBubble()
        {
            var bubble = new MagmaBubble(
                Position + new Vector2(Calc.Random.NextFloat() * poolArea.Width - poolArea.Width / 2, Calc.Random.NextFloat() * poolArea.Height - poolArea.Height / 2)
            );
            bubbles.Add(bubble);
            Scene.Add(bubble);
        }

        private void Erupt()
        {
            State = PoolState.Erupting;
            sprite.Play("erupting");
            
            // Create multiple particles
            for (int i = 0; i < 10; i++)
            {
                var particle = new MagmaParticle(
                    Position + new Vector2(Calc.Random.NextFloat() * poolArea.Width - poolArea.Width / 2, Calc.Random.NextFloat() * poolArea.Height - poolArea.Height / 2),
                    new Vector2(Calc.Random.NextFloat() * 200f + 100f, -Calc.Random.NextFloat() * 150f - 50f)
                );
                magmaParticles.Add(particle);
                Scene.Add(particle);
            }
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.3f);
            
            Add(new Coroutine(EruptRoutine()));
        }

        private IEnumerator EruptRoutine()
        {
            yield return 0.5f;
            State = PoolState.Idle;
            sprite.Play("idle");
        }

        public override void Render()
        {
            // Draw magma pool
            Draw.Rect(poolArea, Color.OrangeRed * 0.6f);
            
            // Draw magma surface
            Draw.Rect(poolArea.X, poolArea.Y, poolArea.Width, poolArea.Height, Color.Orange * 0.3f);
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// MagmaBubble - Rising bubble in magma pool
    /// </summary>
    public class MagmaBubble : Actor
    {
        private float lifetime;
        private float maxLifetime;
        private float size;

        public MagmaBubble(Vector2 position)
            : base(position)
        {
            maxLifetime = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
            lifetime = maxLifetime;
            size = Calc.Random.NextFloat() * (8f - 4f) + 4f;
        }

        public override void Update()
        {
            base.Update();
            Position.Y -= 30f * Engine.DeltaTime;
            Position.X += Calc.Random.NextFloat() * size - size / 2;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, size, Color.Orange * (alpha * 0.6f), 4);
        }
    }

    /// <summary>
    /// MagmaParticle - Particle from magma eruption
    /// </summary>
    public class MagmaParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public MagmaParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (1.2f - 0.6f) + 0.6f;
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
            Draw.Circle(Position, 4f, Color.OrangeRed * (alpha * 0.7f), 4);
        }
    }
}

