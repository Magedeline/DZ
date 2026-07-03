namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// RevolverTarget - Breakable target for gun training
    /// Tracks accuracy and can be used for shooting galleries
    /// Sprite path: objects/DZ/DZ/revolver_target/
    /// </summary>
    [CustomEntity("DZ/RevolverTarget")]
    [Tracked]
    public class RevolverTarget : Actor
    {
        #region Enums
        public enum TargetState
        {
            Ready,
            Showing,
            Hit,
            Broken,
            Resetting
        }

        public enum TargetType
        {
            Static,
            Popup,
            Moving,
            Swinging
        }
        #endregion

        #region Properties
        public TargetState State { get; private set; }
        public TargetType Type { get; private set; }
        public int Points { get; private set; }
        public float ShowTime { get; private set; }
        public float ResetTime { get; private set; }
        public bool IsHit => State == TargetState.Hit || State == TargetState.Broken;
        
        private Sprite sprite;
        private float stateTimer;
        private Vector2 startPosition;
        private Vector2 targetPosition;
        private float moveSpeed;
        private float swingAngle;
        private float swingSpeed;
        private Level level;
        private int hitCount;
        private List<TargetShard> shards;
        private VertexLight targetLight;
        #endregion

        #region Constructor
        public RevolverTarget(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Enum("targetType", TargetType.Static),
                data.Int("points", 100),
                data.Float("showTime", 2f),
                data.Float("resetTime", 3f),
                data.Float("moveSpeed", 50f)
            );
        }

        public RevolverTarget(Vector2 position, TargetType type = TargetType.Static, int points = 100,
            float showTime = 2f, float resetTime = 3f, float moveSpeed = 50f)
            : base(position)
        {
            Initialize(type, points, showTime, resetTime, moveSpeed);
        }

        private void Initialize(TargetType type, int points, float showTime, float resetTime, float moveSpeed)
        {
            Type = type;
            Points = points;
            ShowTime = showTime;
            ResetTime = resetTime;
            this.moveSpeed = moveSpeed;
            
            startPosition = Position;
            targetPosition = Position + new Vector2(100f, 0f);
            State = TargetState.Ready;
            stateTimer = 0f;
            swingAngle = 0f;
            swingSpeed = 2f;
            hitCount = 0;
            shards = new List<TargetShard>();
            
            // Setup collider
            Collider = new Hitbox(32f, 32f, -16f, -16f);
            
            // Setup sprite
            Add(sprite = DZModule.SpriteBank.Create("revolver_target"));
            sprite.Play("ready");
            
            // Add glow
            Add(targetLight = new VertexLight(Color.Red, 0.3f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (State != TargetState.Ready) return;
            
            State = TargetState.Showing;
            stateTimer = ShowTime;
            sprite.Play("show");
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void Hit()
        {
            if (State != TargetState.Showing) return;
            
            State = TargetState.Hit;
            hitCount++;
            
            // Create shards
            for (int i = 0; i < 8; i++)
            {
                var shard = new TargetShard(
                    Position,
                    new Vector2(Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 100f - 50f)
                );
                shards.Add(shard);
                Scene.Add(shard);
            }
            
            // Effects
            level?.Shake(0.2f);
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            // Notify score tracker
            var tracker = Scene.Tracker.GetEntity<TargetScoreTracker>();
            tracker?.AddScore(Points);
            
            Add(new Coroutine(HitRoutine()));
        }

        public void Break()
        {
            State = TargetState.Broken;
            sprite.Play("broken");
            
            // More shards for break
            for (int i = 0; i < 12; i++)
            {
                var shard = new TargetShard(
                    Position,
                    new Vector2(Calc.Random.NextFloat() * 150f - 75f, Calc.Random.NextFloat() * 150f - 75f)
                );
                shards.Add(shard);
                Scene.Add(shard);
            }
            
            level?.Shake(0.3f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        public void Reset()
        {
            State = TargetState.Resetting;
            Position = startPosition;
            sprite.Play("reset");
            
            Add(new Coroutine(ResetRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator HitRoutine()
        {
            sprite.Play("hit");
            yield return 0.3f;
            
            State = TargetState.Resetting;
            yield return ResetRoutine();
        }

        private IEnumerator ResetRoutine()
        {
            stateTimer = ResetTime;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                yield return null;
            }
            
            State = TargetState.Ready;
            sprite.Play("ready");
            Position = startPosition;
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
                case TargetState.Showing:
                    stateTimer -= Engine.DeltaTime;
                    
                    // Movement based on type
                    switch (Type)
                    {
                        case TargetType.Moving:
                            // Move back and forth
                            Vector2 dir = (targetPosition - Position).SafeNormalize();
                            Position += dir * moveSpeed * Engine.DeltaTime;
                            
                            if (Vector2.Distance(Position, targetPosition) < 5f)
                            {
                                var temp = targetPosition;
                                targetPosition = startPosition;
                                startPosition = temp;
                            }
                            break;
                            
                        case TargetType.Swinging:
                            // Swing like a pendulum
                            swingAngle += swingSpeed * Engine.DeltaTime;
                            Position = startPosition + new Vector2(
                                (float)Math.Sin(swingAngle) * 50f,
                                0f
                            );
                            sprite.Rotation = (float)Math.Sin(swingAngle) * 0.2f;
                            break;
                    }
                    
                    if (stateTimer <= 0f)
                    {
                        // Missed - reset
                        State = TargetState.Resetting;
                        Add(new Coroutine(ResetRoutine()));
                    }
                    break;
            }
            
            shards.RemoveAll(s => s == null || s.Scene == null);
        }

        public override void Render()
        {
            // Draw target ring
            if (State == TargetState.Showing)
            {
                Draw.Circle(Position, 20f, Color.Red * 0.2f, 12);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// TargetShard - Shard from broken target
    /// </summary>
    public class TargetShard : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float rotation;
        private float rotationSpeed;

        public TargetShard(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 1.5f;
            rotation = Calc.Random.NextFloat() * MathHelper.TwoPi;
            rotationSpeed = Calc.Random.NextFloat() * 8f - 4f;
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 300f * Engine.DeltaTime;
            rotation += rotationSpeed * Engine.DeltaTime;
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f || OnGround())
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = Math.Min(1f, lifetime);
            Draw.Rect(Position - new Vector2(4, 4), 8, 8, Color.Orange * (alpha * 0.8f));
        }
    }

    /// <summary>
    /// TargetScoreTracker - Tracks shooting gallery score
    /// </summary>
    [CustomEntity("DZ/TargetScoreTracker")]
    public class TargetScoreTracker : Entity
    {
        private int score;
        private int targetsHit;
        private int totalTargets;
        private float timeLimit;
        private float timer;
        private bool isActive;

        public TargetScoreTracker(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            score = 0;
            targetsHit = 0;
            totalTargets = data.Int("totalTargets", 10);
            timeLimit = data.Float("timeLimit", 30f);
            timer = timeLimit;
            isActive = false;
        }

        public void Start()
        {
            isActive = true;
            timer = timeLimit;
            score = 0;
            targetsHit = 0;
        }

        public void Stop()
        {
            isActive = false;
        }

        public void AddScore(int points)
        {
            score += points;
            targetsHit++;
            
            // Check completion
            if (targetsHit >= totalTargets)
            {
                Complete();
            }
        }

        private void Complete()
        {
            isActive = false;
            
            var level = Scene as Level;
            level?.Session.SetFlag("shooting_gallery_complete", true);
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        public override void Update()
        {
            base.Update();
            
            if (isActive)
            {
                timer -= Engine.DeltaTime;
                
                if (timer <= 0f)
                {
                    Stop();
                }
            }
        }

        public override void Render()
        {
            // Draw HUD
            if (isActive)
            {
                // Would need proper font rendering here
            }
        }
    }

    /// <summary>
    /// TargetSpawner - Spawns targets in sequence
    /// </summary>
    [CustomEntity("DZ/TargetSpawner")]
    public class TargetSpawner : Entity
    {
        private List<RevolverTarget> targets;
        private float spawnInterval;
        private float timer;
        private int currentIndex;
        private bool isSpawning;

        public TargetSpawner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            spawnInterval = data.Float("spawnInterval", 1f);
            targets = new List<RevolverTarget>();
            timer = 0f;
            currentIndex = 0;
            isSpawning = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find nearby targets
            foreach (var target in scene.Tracker.GetEntities<RevolverTarget>())
            {
                targets.Add((RevolverTarget)target);
            }
        }

        public void StartSpawning()
        {
            isSpawning = true;
            timer = 0f;
            currentIndex = 0;
        }

        public void StopSpawning()
        {
            isSpawning = false;
        }

        public override void Update()
        {
            base.Update();
            
            if (!isSpawning || currentIndex >= targets.Count) return;
            
            timer += Engine.DeltaTime;
            
            if (timer >= spawnInterval)
            {
                timer = 0f;
                targets[currentIndex].Show();
                currentIndex++;
            }
        }
    }
}

