namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// DarkMirror - Mirror that shows corrupted reflection
    /// Can be shattered to reveal secrets or create passages
    /// Sprite path: objects/DZ/DZ/dark_mirror/
    /// </summary>
    [CustomEntity("DZ/DarkMirror")]
    [Tracked]
    public class DarkMirror : Actor
    {
        #region Enums
        public enum MirrorState
        {
            Intact,
            Shattered
        }
        #endregion

        #region Properties
        public MirrorState State { get; private set; }
        public int Health { get; private set; }
        public bool RevealsSecret { get; private set; }
        public bool IsIntact => State == MirrorState.Intact;
        
        private Sprite sprite;
        private VertexLight mirrorLight;
        private Level level;
        private List<MirrorShard> shards;
        private Player reflectingPlayer;
        private Color mirrorColor;
        #endregion

        #region Constructor
        public DarkMirror(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Int("health", 2), data.Bool("revealsSecret", false));
        }

        public DarkMirror(Vector2 position, int health = 2, bool revealsSecret = false)
            : base(position)
        {
            Initialize(health, revealsSecret);
        }

        private void Initialize(int health, bool revealsSecret)
        {
            Health = health;
            RevealsSecret = revealsSecret;
            
            State = MirrorState.Intact;
            shards = new List<MirrorShard>();
            mirrorColor = Color.DarkBlue;
            
            Collider = new Hitbox(48f, 64f, -24f, -64f);
            
            Add(sprite = DZModule.SpriteBank.Create("dark_mirror"));
            sprite.Play("intact");
            
            Add(mirrorLight = new VertexLight(mirrorColor, 0.4f, 12, 32));
        }
        #endregion

        #region Public Methods
        public void Shatter()
        {
            if (State != MirrorState.Intact) return;
            
            State = MirrorState.Shattered;
            sprite.Play("shattered");
            
            Audio.Play("event:/game/gen_crumble_fall", Position);
            level?.Shake(0.3f);
            
            Add(new Coroutine(ShatterRoutine()));
        }

        public void Damage(int amount)
        {
            Health -= amount;
            
            if (Health <= 0)
            {
                Shatter();
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator ShatterRoutine()
        {
            // Create shards
            for (int i = 0; i < 12; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                float speed = Calc.Random.NextFloat() * 100f + 100f;
                
                var shard = new MirrorShard(
                    Position,
                    Calc.AngleToVector(angle, speed)
                );
                shards.Add(shard);
                Scene.Add(shard);
            }
            
            // Reveal secret if applicable
            if (RevealsSecret)
            {
                yield return RevealSecretRoutine();
            }
            
            yield return 0.5f;
            RemoveSelf();
        }

        private IEnumerator RevealSecretRoutine()
        {
            level?.Flash(Color.White * 0.4f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            level?.Session.SetFlag("mirror_secret_revealed", true);
            
            yield return 0.5f;
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
            
            if (State == MirrorState.Intact)
            {
                // Check for player looking at mirror
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    reflectingPlayer = player;
                    mirrorLight.Alpha = 0.6f;
                }
                else
                {
                    reflectingPlayer = null;
                    mirrorLight.Alpha = 0.4f;
                }
            }
            
            shards.RemoveAll(s => s == null || s.Scene == null);
        }

        public override void Render()
        {
            if (State == MirrorState.Intact)
            {
                // Draw reflection
                if (reflectingPlayer != null)
                {
                    Vector2 reflectPos = Position + new Vector2(0f, -32f);
                    Draw.Circle(reflectPos, 20f, mirrorColor * 0.3f, 8);
                }
            }
            base.Render();
        }
        #endregion
    }

    public class MirrorShard : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public MirrorShard(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
            lifetime = maxLifetime;
            
            Collider = new Hitbox(10f, 10f, -5f, -5f);
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
            Draw.Rect(Position - new Vector2(5, 5), 10, 10, Color.DarkBlue * (alpha * 0.7f));
        }
    }
}

