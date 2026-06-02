namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// ObsidianShard - Sharp volcanic crystal hazard
    /// Can be destroyed to create path or collectible
    /// Sprite path: objects/obsidian_shard/
    /// </summary>
    [CustomEntity("MaggyHelper/ObsidianShard")]
    [Tracked]
    public class ObsidianShard : Actor
    {
        #region Enums
        public enum ShardState
        {
            Intact,
            Cracking,
            Shattered,
            Collected
        }
        #endregion

        #region Properties
        public ShardState State { get; private set; }
        public int Health { get; private set; }
        public bool IsCollectible { get; private set; }
        public bool IsIntact => State == ShardState.Intact;
        
        private Sprite sprite;
        private Level level;
        private List<ShardFragment> fragments;
        private VertexLight shardLight;
        #endregion

        #region Constructor
        public ObsidianShard(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Int("health", 1), data.Bool("isCollectible", false));
        }

        public ObsidianShard(Vector2 position, int health = 1, bool isCollectible = false)
            : base(position)
        {
            Initialize(health, isCollectible);
        }

        private void Initialize(int health, bool isCollectible)
        {
            Health = health;
            IsCollectible = isCollectible;
            
            State = ShardState.Intact;
            fragments = new List<ShardFragment>();
            
            Collider = new Hitbox(24f, 32f, -12f, -32f);
            
            Add(sprite = GFX.SpriteBank.Create("obsidian_shard"));
            sprite.Play("intact");
            
            Add(shardLight = new VertexLight(Color.Purple, 0.4f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void Damage(int amount)
        {
            if (State != ShardState.Intact) return;
            
            Health -= amount;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            if (Health <= 0)
            {
                Shatter();
            }
            else
            {
                sprite.Play("cracking");
                Add(new Coroutine(CrackRoutine()));
            }
        }

        public void Collect()
        {
            if (!IsCollectible || State != ShardState.Intact) return;
            
            State = ShardState.Collected;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Flash(Color.Purple * 0.3f);
            
            Add(new Coroutine(CollectRoutine()));
        }
        #endregion

        #region Private Methods
        private void Shatter()
        {
            State = ShardState.Shattered;
            sprite.Play("shattered");
            
            // Create fragments
            for (int i = 0; i < 8; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                float speed = Calc.Random.NextFloat() * 100f + 100f;
                
                var fragment = new ShardFragment(
                    Position,
                    Calc.AngleToVector(angle, speed)
                );
                fragments.Add(fragment);
                Scene.Add(fragment);
            }
            
            level?.Shake(0.2f);
            
            Add(new Coroutine(ShatterRoutine()));
        }

        private IEnumerator CrackRoutine()
        {
            yield return 0.3f;
            sprite.Play("intact");
        }

        private IEnumerator ShatterRoutine()
        {
            yield return 0.3f;
            RemoveSelf();
        }

        private IEnumerator CollectRoutine()
        {
            for (int i = 0; i < 10; i++)
            {
                var fragment = new ShardFragment(
                    Position,
                    new Vector2(Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 100f - 50f)
                );
                fragments.Add(fragment);
                Scene.Add(fragment);
                yield return 0.02f;
            }
            
            level?.Session.SetFlag("obsidian_shard_collected", true);
            yield return 0.2f;
            RemoveSelf();
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            if (level?.Session.GetFlag("obsidian_shard_collected") == true)
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();
            
            fragments.RemoveAll(f => f == null || f.Scene == null);
            
            // Check player collision for collection
            if (IsCollectible && State == ShardState.Intact)
            {
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    Collect();
                }
            }
        }

        public override void Render()
        {
            // Draw shard glow
            if (State == ShardState.Intact)
            {
                Draw.Circle(Position - Vector2.UnitY * 16f, 20f, Color.Purple * 0.2f, 8);
            }
            base.Render();
        }
        #endregion
    }

    public class ShardFragment : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public ShardFragment(Vector2 position, Vector2 velocity)
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
            
            if (lifetime <= 0f || OnGround())
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Rect(Position - new Vector2(4, 4), 8, 8, Color.Purple * (alpha * 0.7f));
        }
    }
}
