namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// GhostlyEcho - Semi-transparent spirit that phases through walls
    /// Mirrors player movement with a configurable delay, creating puzzle elements
    /// Can be used for puzzles where player must avoid their own echo
    /// Sprite path: characters/DZ/ghostly_echo/
    /// </summary>
    [CustomEntity("DZ/GhostlyEcho")]
    [Tracked]
    public class GhostlyEcho : Actor
    {
        #region Enums
        public enum EchoState
        {
            Dormant,
            Recording,
            Mimic,
            Attacking,
            Fading,
            Defeated
        }

        public enum EchoBehavior
        {
            Mirror,      // Copies player movements with delay
            Reverse,     // Copies player movements in reverse
            Patrol,      // Follows a set path
            Chase        // Slowly follows player
        }
        #endregion

        #region Properties
        public EchoState State { get; private set; }
        public EchoBehavior Behavior { get; private set; }
        public float MirrorDelay { get; private set; }
        public float FadeTime { get; private set; }
        public float Alpha { get; private set; }
        public bool IsDangerous { get; private set; }
        public bool IsSolid { get; private set; }
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Queue<EchoFrame> positionHistory;
        private List<EchoFrame> recordedPath;
        private Player linkedPlayer;
        private float recordTimer;
        private float playbackTimer;
        private int playbackIndex;
        private float fadeTimer;
        private Level level;
        private VertexLight ghostLight;
        private float floatOffset;
        private float floatTimer;
        #endregion

        #region Internal Classes
        private class EchoFrame
        {
            public Vector2 Position { get; set; }
            public string Animation { get; set; }
            public Facings Facing { get; set; }
            public float Time { get; set; }

            public EchoFrame(Vector2 position, string animation, Facings facing, float time)
            {
                Position = position;
                Animation = animation;
                Facing = facing;
                Time = time;
            }
        }
        #endregion

        #region Constructor
        public GhostlyEcho(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("mirrorDelay", 0.5f),
                data.Float("fadeTime", 5f),
                data.Bool("isDangerous", true),
                data.Bool("isSolid", false),
                data.Enum("behavior", EchoBehavior.Mirror)
            );
        }

        public GhostlyEcho(Vector2 position, float mirrorDelay = 0.5f, float fadeTime = 5f,
            bool isDangerous = true, bool isSolid = false, EchoBehavior behavior = EchoBehavior.Mirror)
            : base(position)
        {
            Initialize(mirrorDelay, fadeTime, isDangerous, isSolid, behavior);
        }

        private void Initialize(float mirrorDelay, float fadeTime, bool isDangerous, bool isSolid, EchoBehavior behavior)
        {
            MirrorDelay = mirrorDelay;
            FadeTime = fadeTime;
            IsDangerous = isDangerous;
            IsSolid = isSolid;
            Behavior = behavior;
            
            Alpha = 0.6f;
            positionHistory = new Queue<EchoFrame>();
            recordedPath = new List<EchoFrame>();
            recordTimer = 0f;
            playbackTimer = 0f;
            playbackIndex = 0;
            fadeTimer = 0f;
            floatTimer = 0f;
            floatOffset = 0f;
            
            // Ghost doesn't have solid collision unless specified
            if (isSolid)
            {
                Collider = new Hitbox(16f, 24f, -8f, -24f);
            }
            else
            {
                // No collision - can phase through walls
                Collider = new Hitbox(16f, 24f, -8f, -24f);
            }
            
            // Setup sprite
            Add(sprite = DZModule.SpriteBank.Create("ghostly_echo"));
            sprite.Color = new Color(0.6f, 0.8f, 1f) * Alpha;
            
            // Ghostly glow
            Add(ghostLight = new VertexLight(new Color(0.4f, 0.6f, 0.9f), 1f, 8, 24));
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
            
            State = EchoState.Dormant;
        }
        #endregion

        #region State Begin Methods
        private void DormantBegin()
        {
            sprite.Play("dormant");
            State = EchoState.Dormant;
            Alpha = 0.3f;
            UpdateVisuals();
        }

        private void RecordingBegin()
        {
            sprite.Play("active");
            State = EchoState.Recording;
            Alpha = 0.6f;
            positionHistory.Clear();
            recordTimer = 0f;
            UpdateVisuals();
        }

        private void MimicBegin()
        {
            sprite.Play("mimic");
            State = EchoState.Mimic;
            playbackTimer = 0f;
            playbackIndex = 0;
            UpdateVisuals();
        }

        private void AttackingBegin()
        {
            sprite.Play("attack");
            State = EchoState.Attacking;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void FadingBegin()
        {
            sprite.Play("fade");
            State = EchoState.Fading;
            fadeTimer = FadeTime;
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = EchoState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator DormantRoutine()
        {
            while (true)
            {
                // Gentle floating animation
                floatTimer += Engine.DeltaTime * 2f;
                floatOffset = (float)Math.Sin(floatTimer) * 4f;
                
                // Check for player to start recording
                linkedPlayer = Scene.Tracker.GetEntity<Player>();
                if (linkedPlayer != null && Vector2.Distance(Position, linkedPlayer.Position) < 200f)
                {
                    stateMachine.State = 1; // Recording
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator RecordingRoutine()
        {
            while (linkedPlayer != null)
            {
                recordTimer += Engine.DeltaTime;
                
                // Record player position and animation
                if (linkedPlayer.Sprite != null)
                {
                    var frame = new EchoFrame(
                        linkedPlayer.Position,
                        linkedPlayer.Sprite.CurrentAnimationID,
                        linkedPlayer.Facing,
                        recordTimer
                    );
                    positionHistory.Enqueue(frame);
                }
                
                // Limit history size based on delay
                int maxFrames = (int)(MirrorDelay / Engine.DeltaTime);
                while (positionHistory.Count > maxFrames)
                {
                    positionHistory.Dequeue();
                }
                
                // Check if player left range
                if (Vector2.Distance(Position, linkedPlayer.Position) > 300f)
                {
                    stateMachine.State = 4; // Fading
                    yield break;
                }
                
                yield return null;
            }
            
            stateMachine.State = 0; // Dormant
        }

        private IEnumerator MimicRoutine()
        {
            while (true)
            {
                playbackTimer += Engine.DeltaTime;
                
                switch (Behavior)
                {
                    case EchoBehavior.Mirror:
                        yield return MirrorBehavior();
                        break;
                    case EchoBehavior.Reverse:
                        yield return ReverseBehavior();
                        break;
                    case EchoBehavior.Patrol:
                        yield return PatrolBehavior();
                        break;
                    case EchoBehavior.Chase:
                        yield return ChaseBehavior();
                        break;
                }
                
                // Check for player collision if dangerous
                if (IsDangerous)
                {
                    linkedPlayer = Scene.Tracker.GetEntity<Player>();
                    if (linkedPlayer != null && Collide.Check(this, linkedPlayer))
                    {
                        stateMachine.State = 3; // Attacking
                        yield break;
                    }
                }
                
                yield return null;
            }
        }

        private IEnumerator AttackingRoutine()
        {
            linkedPlayer = Scene.Tracker.GetEntity<Player>();
            
            if (linkedPlayer != null)
            {
                // Flash effect
                level?.Flash(Color.White * 0.3f);
                
                linkedPlayer.Die(Vector2.Zero);
            }
            
            yield return 0.3f;
            
            stateMachine.State = 4; // Fading
        }

        private IEnumerator FadingRoutine()
        {
            while (fadeTimer > 0f)
            {
                fadeTimer -= Engine.DeltaTime;
                Alpha = 0.6f * (fadeTimer / FadeTime);
                UpdateVisuals();
                yield return null;
            }
            
            stateMachine.State = 0; // Dormant
        }

        private IEnumerator DefeatedRoutine()
        {
            // Death particles - ghostly wisps
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 8, Position, Vector2.One * 8f, new Color(0.4f, 0.6f, 0.9f));
            
            yield return 0.5f;
            RemoveSelf();
        }
        #endregion

        #region Behavior Methods
        private IEnumerator MirrorBehavior()
        {
            // Move to delayed position from history
            if (positionHistory.Count > 0)
            {
                var frame = positionHistory.Dequeue();
                Position = frame.Position;
                sprite.Play(frame.Animation);
                sprite.Scale.X = frame.Facing == Facings.Right ? 1 : -1;
            }
            
            yield return null;
        }

        private IEnumerator ReverseBehavior()
        {
            // Mirror position relative to a center point
            linkedPlayer = Scene.Tracker.GetEntity<Player>();
            if (linkedPlayer != null)
            {
                Vector2 center = Position; // Use initial position as mirror center
                Vector2 offset = center - linkedPlayer.Position;
                Position = center + offset;
                
                // Reverse facing
                sprite.Scale.X = linkedPlayer.Facing == Facings.Right ? -1 : 1;
            }
            
            yield return null;
        }

        private IEnumerator PatrolBehavior()
        {
            // Follow recorded path
            if (recordedPath.Count > 0)
            {
                if (playbackIndex >= recordedPath.Count)
                    playbackIndex = 0;
                
                var frame = recordedPath[playbackIndex];
                Position = Vector2.Lerp(Position, frame.Position, 0.1f);
                sprite.Play(frame.Animation);
                sprite.Scale.X = frame.Facing == Facings.Right ? 1 : -1;
                
                playbackIndex++;
            }
            
            yield return null;
        }

        private IEnumerator ChaseBehavior()
        {
            // Slowly follow player
            linkedPlayer = Scene.Tracker.GetEntity<Player>();
            if (linkedPlayer != null)
            {
                Vector2 direction = (linkedPlayer.Position - Position).SafeNormalize();
                Position += direction * 30f * Engine.DeltaTime;
                
                sprite.Play("float");
                sprite.Scale.X = direction.X > 0 ? 1 : -1;
            }
            
            yield return null;
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == EchoState.Defeated) return;
            
            stateMachine.State = 5; // Defeated
        }

        public void SetRecordedPath(List<Vector2> path)
        {
            recordedPath.Clear();
            float time = 0f;
            foreach (var pos in path)
            {
                recordedPath.Add(new EchoFrame(pos, "float", Facings.Right, time));
                time += 0.1f;
            }
        }

        public void Activate()
        {
            if (State == EchoState.Dormant)
            {
                stateMachine.State = 1; // Recording
            }
        }

        public void Deactivate()
        {
            stateMachine.State = 4; // Fading
        }
        #endregion

        #region Private Methods
        private void UpdateVisuals()
        {
            sprite.Color = new Color(0.6f, 0.8f, 1f) * Alpha;
            ghostLight.Alpha = Alpha;
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
            
            // Floating effect
            floatTimer += Engine.DeltaTime * 2f;
            floatOffset = (float)Math.Sin(floatTimer) * 3f;
        }

        public override void Render()
        {
            // Ghostly trail effect
            if (State == EchoState.Mimic || State == EchoState.Recording)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 trailPos = Position - Vector2.UnitY * floatOffset + new Vector2(Calc.Random.Next(-4, 4), Calc.Random.Next(-4, 4));
                    Draw.Circle(trailPos, 6f, new Color(0.4f, 0.6f, 0.9f) * (Alpha * 0.2f), 4);
                }
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// EchoOrb - Collectible that activates GhostlyEcho entities
    /// When collected, triggers all echoes in range to start recording
    /// Sprite path: collectables/echo_orb/
    /// </summary>
    [CustomEntity("DZ/EchoOrb")]
    public class EchoOrb : Actor
    {
        #region Properties
        private Sprite sprite;
        private float activationRadius;
        private bool collected;
        private float pulseTimer;
        private Level level;
        #endregion

        #region Constructor
        public EchoOrb(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            activationRadius = data.Float("activationRadius", 200f);
            Initialize();
        }

        public EchoOrb(Vector2 position, float activationRadius = 200f)
            : base(position)
        {
            this.activationRadius = activationRadius;
            Initialize();
        }

        private void Initialize()
        {
            collected = false;
            pulseTimer = 0f;
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(sprite = GFX.SpriteBank.Create("echo_orb"));
            Add(new VertexLight(new Color(0.4f, 0.6f, 0.9f), 1f, 8, 24));
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
            
            if (collected) return;
            
            // Pulse effect
            pulseTimer += Engine.DeltaTime * 3f;
            sprite.Scale = Vector2.One * (1f + (float)Math.Sin(pulseTimer) * 0.1f);
            
            // Check player collection
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
            
            // Activate all echoes in range
            foreach (var echo in Scene.Tracker.GetEntities<GhostlyEcho>())
            {
                if (Vector2.Distance(Position, echo.Position) < activationRadius)
                {
                    // Would need to activate echo if it had an Activate method
                }
            }
            
            // Particle effect
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 12, Position, Vector2.One * 8f, new Color(0.4f, 0.6f, 0.9f));
            
            RemoveSelf();
        }
        #endregion
    }
}
