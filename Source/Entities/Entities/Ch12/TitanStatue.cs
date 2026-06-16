namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// TitanStatue - Large decorative statue that can animate
    /// May become active during boss fight or events
    /// Sprite path: objects/titan_statue/
    /// </summary>
    [CustomEntity("DZ/TitanStatue")]
    [Tracked]
    public class TitanStatue : Actor
    {
        #region Enums
        public enum StatueState
        {
            Inactive,
            Glowing,
            Awakening,
            Active,
            Attacking,
            Returning,
            Destroyed
        }
        #endregion

        #region Properties
        public StatueState State { get; private set; }
        public int Health { get; private set; }
        public bool IsAnimated { get; private set; }
        public bool CanAwaken { get; private set; }
        
        private Sprite sprite;
        private VertexLight eyeLight;
        private Vector2 startPosition;
        private float stateTimer;
        private Player targetPlayer;
        private Level level;
        private List<StoneParticle> stoneParticles;
        private bool hasAwakened;
        #endregion

        #region Constructor
        public TitanStatue(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 5),
                data.Bool("canAwaken", false),
                data.Bool("isAnimated", false)
            );
        }

        public TitanStatue(Vector2 position, int health = 5, bool canAwaken = false, bool isAnimated = false)
            : base(position)
        {
            Initialize(health, canAwaken, isAnimated);
        }

        private void Initialize(int health, bool canAwaken, bool isAnimated)
        {
            Health = health;
            CanAwaken = canAwaken;
            IsAnimated = isAnimated;
            
            startPosition = Position;
            State = StatueState.Inactive;
            stateTimer = 0f;
            hasAwakened = false;
            stoneParticles = new List<StoneParticle>();
            
            Collider = new Hitbox(48f, 80f, -24f, -80f);
            
            Add(sprite = GFX.SpriteBank.Create("titan_statue"));
            sprite.Play(isAnimated ? "active" : "inactive");
            
            Add(eyeLight = new VertexLight(Color.Red, isAnimated ? 0.6f : 0f, 8, 24));
            eyeLight.Position = new Vector2(0f, -60f);
        }
        #endregion

        #region Public Methods
        public void Awaken()
        {
            if (!CanAwaken || hasAwakened) return;
            
            hasAwakened = true;
            State = StatueState.Awakening;
            sprite.Play("awakening");
            
            Audio.Play("event:/game/gen_crumble_fall", Position);
            level?.Shake(0.4f);
            
            Add(new Coroutine(AwakenRoutine()));
        }

        public void Attack()
        {
            if (State != StatueState.Active) return;
            
            State = StatueState.Attacking;
            sprite.Play("attack");
            
            Add(new Coroutine(AttackRoutine()));
        }

        public void Destroy()
        {
            State = StatueState.Destroyed;
            
            // Create debris
            for (int i = 0; i < 20; i++)
            {
                var particle = new StoneParticle(
                    Position + new Vector2(Calc.Random.NextFloat() * 40f - 20f, Calc.Random.NextFloat() * 40f - 20f),
                    new Vector2(Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 150f - 50f)
                );
                stoneParticles.Add(particle);
                Scene.Add(particle);
            }
            
            level?.Shake(0.5f);
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private IEnumerator AwakenRoutine()
        {
            // Stone cracking effect
            for (int i = 0; i < 15; i++)
            {
                var particle = new StoneParticle(
                    Position + new Vector2(Calc.Random.NextFloat() * 40f - 20f, Calc.Random.NextFloat() * 40f - 20f),
                    new Vector2(Calc.Random.NextFloat() * 60f - 30f, Calc.Random.NextFloat() * 80f - 30f)
                );
                stoneParticles.Add(particle);
                Scene.Add(particle);
                yield return 0.05f;
            }
            
            eyeLight.Alpha = 0.6f;
            State = StatueState.Active;
            sprite.Play("active");
            
            level?.Session.SetFlag("titan_statue_awakened", true);
        }

        private IEnumerator AttackRoutine()
        {
            // Ground slam attack
            yield return 0.5f;
            
            // Create shockwave
            level?.Shake(0.5f);
            level?.Flash(Color.White * 0.3f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            // Damage player if close
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < 100f)
            {
                player.Die(Vector2.Zero);
            }
            
            yield return 0.5f;
            
            State = StatueState.Active;
            sprite.Play("active");
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == StatueState.Destroyed) return;
            
            Health -= damage;
            
            // Create stone particles
            for (int i = 0; i < 5; i++)
            {
                var particle = new StoneParticle(
                    Position + new Vector2(Calc.Random.NextFloat() * 40f - 20f, Calc.Random.NextFloat() * 40f - 20f),
                    new Vector2(Calc.Random.NextFloat() * 80f - 40f, Calc.Random.NextFloat() * 100f - 40f)
                );
                stoneParticles.Add(particle);
                Scene.Add(particle);
            }
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            if (Health <= 0)
            {
                Destroy();
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
            
            if (State == StatueState.Active)
            {
                // Auto attack cycle
                stateTimer += Engine.DeltaTime;
                if (stateTimer >= 3f)
                {
                    stateTimer = 0f;
                    Attack();
                }
            }
            
            stoneParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw statue base
            Draw.Rect(Position.X - 30, Position.Y - 5, 60, 10, Color.DarkGray);
            
            // Draw eye glow when active
            if (State >= StatueState.Active)
            {
                Vector2 eyePos = Position + new Vector2(0f, -60f);
                Draw.Circle(eyePos, 8f, Color.Red * 0.4f, 6);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// StatueController - Controls multiple titan statues
    /// </summary>
    [CustomEntity("DZ/StatueController")]
    public class StatueController : Entity
    {
        private List<TitanStatue> statues;
        private float activationDelay;
        private float timer;
        private int currentIndex;

        public StatueController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            statues = new List<TitanStatue>();
            activationDelay = data.Float("activationDelay", 2f);
            timer = 0f;
            currentIndex = 0;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            foreach (var statue in scene.Tracker.GetEntities<TitanStatue>())
            {
                var typedStatue = (TitanStatue)statue;
                if (typedStatue.CanAwaken)
                    statues.Add(typedStatue);
            }
        }

        public void ActivateAll()
        {
            foreach (var statue in statues)
            {
                statue.Awaken();
            }
        }

        public void ActivateSequential()
        {
            timer = 0f;
            currentIndex = 0;
            Add(new Coroutine(SequentialRoutine()));
        }

        private IEnumerator SequentialRoutine()
        {
            while (currentIndex < statues.Count)
            {
                timer += Engine.DeltaTime;
                
                if (timer >= activationDelay)
                {
                    timer = 0f;
                    statues[currentIndex].Awaken();
                    currentIndex++;
                }
                
                yield return null;
            }
        }
    }
}

