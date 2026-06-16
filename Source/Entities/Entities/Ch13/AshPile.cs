namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// AshPile - Pile of volcanic ash that can be climbed
    /// Provides climbable surfaces but slows player
    /// Sprite path: objects/ash_pile/
    /// </summary>
    [CustomEntity("DZ/AshPile")]
    [Tracked]
    public class AshPile : Actor
    {
        #region Enums
        public enum PileState
        {
            Stable,
            Shifting,
            Collapsing
        }
        #endregion

        #region Properties
        public PileState State { get; private set; }
        public float ClimbSlowFactor { get; private set; }
        public float ShiftInterval { get; private set; }
        public bool IsClimbable { get; private set; }
        
        private Sprite sprite;
        private Rectangle climbArea;
        private float shiftTimer;
        private Level level;
        private List<AshParticle> ashParticles;
        private Player climbingPlayer;
        private float collapseTimer;
        #endregion

        #region Constructor
        public AshPile(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("climbSlowFactor", 0.5f),
                data.Float("shiftInterval", 3f),
                data.Bool("isClimbable", true)
            );
        }

        public AshPile(Vector2 position, int width, int height, float climbSlowFactor = 0.5f,
            float shiftInterval = 3f, bool isClimbable = true)
            : base(position)
        {
            Initialize(climbSlowFactor, shiftInterval, isClimbable);
            
            climbArea = new Rectangle((int)position.X, (int)position.Y, width, height);
            Collider = new Hitbox(width, height);
        }

        private void Initialize(float climbSlowFactor, float shiftInterval, bool isClimbable)
        {
            ClimbSlowFactor = climbSlowFactor;
            ShiftInterval = shiftInterval;
            IsClimbable = isClimbable;
            
            State = PileState.Stable;
            shiftTimer = 0f;
            collapseTimer = 0f;
            ashParticles = new List<AshParticle>();
            
            Add(sprite = GFX.SpriteBank.Create("ash_pile"));
            sprite.Play("stable");
        }
        #endregion

        #region Public Methods
        public void TriggerCollapse()
        {
            if (State == PileState.Collapsing) return;
            
            State = PileState.Collapsing;
            sprite.Play("collapsing");
            
            Audio.Play("event:/game/gen_crumble_fall", Position);
            level?.Shake(0.3f);
            
            Add(new Coroutine(CollapseRoutine()));
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
            
            shiftTimer += Engine.DeltaTime;
            
            // Check for player climbing
            climbingPlayer = GetPlayerInArea();
            
            if (climbingPlayer != null && IsClimbable)
            {
                // Slow player climbing
                if (climbingPlayer.StateMachine.State == Player.StClimb)
                {
                    climbingPlayer.Speed *= ClimbSlowFactor;
                }
                
                // Create ash particles
                if (Scene.OnInterval(0.1f))
                {
                    CreateAshParticle();
                }
                
                // Check for collapse
                if (shiftTimer >= ShiftInterval)
                {
                    shiftTimer = 0f;
                    State = PileState.Shifting;
                    sprite.Play("shifting");
                    
                    level?.Shake(0.1f);
                }
            }
            else if (State == PileState.Shifting)
            {
                State = PileState.Stable;
                sprite.Play("stable");
            }
            
            ashParticles.RemoveAll(a => a == null || a.Scene == null);
        }

        private Player GetPlayerInArea()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && climbArea.Contains(new Point((int)player.Position.X, (int)player.Position.Y)))
            {
                return player;
            }
            return null;
        }

        private void CreateAshParticle()
        {
            var particle = new AshParticle(
                climbingPlayer.Position + new Vector2(Calc.Random.NextFloat() * 16f - 8f, Calc.Random.NextFloat() * 16f - 8f),
                new Vector2(Calc.Random.NextFloat() * 40f - 20f, -Calc.Random.NextFloat() * 30f)
            );
            ashParticles.Add(particle);
            Scene.Add(particle);
        }

        private IEnumerator CollapseRoutine()
        {
            // Collapse animation
            for (int i = 0; i < 15; i++)
            {
                CreateCollapseParticle();
                yield return 0.05f;
            }
            
            // Remove climbable
            IsClimbable = false;
            
            yield return 0.5f;
            
            // Recover
            yield return 2f;
            
            IsClimbable = true;
            State = PileState.Stable;
            sprite.Play("stable");
        }

        private void CreateCollapseParticle()
        {
            var particle = new AshParticle(
                Position + new Vector2(Calc.Random.NextFloat() * climbArea.Width - climbArea.Width / 2, Calc.Random.NextFloat() * climbArea.Height - climbArea.Height / 2),
                new Vector2(Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 150f - 50f)
            );
            ashParticles.Add(particle);
            Scene.Add(particle);
        }

        public override void Render()
        {
            // Draw ash pile
            Draw.Rect(climbArea, Color.Gray * 0.6f);
            
            // Draw climbable indicator
            if (IsClimbable)
            {
                Draw.Rect(climbArea.X + 4, climbArea.Y + 4, climbArea.Width - 8, climbArea.Height - 8, Color.Gray * 0.3f);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// AshParticle - Ash particle effect
    /// </summary>
    public class AshParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public AshParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 50f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Rect(Position - new Vector2(3 * scale, 2 * scale), 6 * scale, 4 * scale, Color.Gray * (alpha * 0.5f));
        }
    }
}
