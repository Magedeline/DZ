namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// ShadowFigure - Mysterious shadow entity that follows and haunts
    /// Appears and disappears, creates atmosphere
    /// Sprite path: characters/shadow_figure/
    /// </summary>
    [CustomEntity("DZ/ShadowFigure")]
    [Tracked]
    public class ShadowFigure : Actor
    {
        #region Enums
        public enum FigureState
        {
            Hidden,
            Appearing,
            Watching,
            Following,
            Disappearing,
            Vanished
        }
        #endregion

        #region Properties
        public FigureState State { get; private set; }
        public float DetectionRange { get; private set; }
        public float FollowDistance { get; private set; }
        public bool IsHostile { get; private set; }
        
        private Sprite sprite;
        private Vector2 targetPosition;
        private Player targetPlayer;
        private Level level;
        private List<ShadowParticle> particles;
        private VertexLight figureLight;
        private Color shadowColor;
        #endregion

        #region Constructor
        public ShadowFigure(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Float("detectionRange", 150f), data.Float("followDistance", 80f), data.Bool("isHostile", false));
        }

        public ShadowFigure(Vector2 position, float detectionRange = 150f, float followDistance = 80f, bool isHostile = false)
            : base(position)
        {
            Initialize(detectionRange, followDistance, isHostile);
        }

        private void Initialize(float detectionRange, float followDistance, bool isHostile)
        {
            DetectionRange = detectionRange;
            FollowDistance = followDistance;
            IsHostile = isHostile;
            
            State = FigureState.Hidden;
            targetPosition = Position;
            particles = new List<ShadowParticle>();
            shadowColor = Color.Black;
            
            Collider = new Hitbox(24f, 48f, -12f, -48f);
            
            Add(sprite = GFX.SpriteBank.Create("shadow_figure"));
            sprite.Play("hidden");
            sprite.Visible = false;
            
            Add(figureLight = new VertexLight(shadowColor, 0.3f, 12, 32));
            figureLight.Alpha = 0f;
        }
        #endregion

        #region Public Methods
        public void Reveal()
        {
            if (State != FigureState.Hidden) return;
            
            State = FigureState.Appearing;
            sprite.Visible = true;
            sprite.Play("appearing");
            figureLight.Alpha = 0.5f;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            Add(new Coroutine(RevealRoutine()));
        }

        public void Hide()
        {
            if (State == FigureState.Hidden) return;
            
            State = FigureState.Disappearing;
            sprite.Play("disappearing");
            
            Add(new Coroutine(HideRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator RevealRoutine()
        {
            for (int i = 0; i < 10; i++)
            {
                CreateShadowParticle();
                yield return 0.05f;
            }
            
            State = FigureState.Watching;
            sprite.Play("watching");
        }

        private IEnumerator HideRoutine()
        {
            for (int i = 0; i < 10; i++)
            {
                CreateShadowParticle();
                yield return 0.03f;
            }
            
            State = FigureState.Hidden;
            sprite.Visible = false;
            figureLight.Alpha = 0f;
        }

        private void CreateShadowParticle()
        {
            var particle = new ShadowParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f),
                new Vector2(Calc.Random.NextFloat() * 40f - 20f, -Calc.Random.NextFloat() * 30f)
            );
            particles.Add(particle);
            Scene.Add(particle);
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
                case FigureState.Hidden:
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                    if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                    {
                        Reveal();
                    }
                    break;
                    
                case FigureState.Watching:
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                    if (targetPlayer != null)
                    {
                        float distance = Vector2.Distance(Position, targetPlayer.Position);
                        
                        if (distance > DetectionRange * 1.5f)
                        {
                            Hide();
                        }
                        else if (distance < FollowDistance)
                        {
                            State = FigureState.Following;
                        }
                        
                        // Face player
                        sprite.Scale.X = targetPlayer.Position.X > Position.X ? 1 : -1;
                    }
                    break;
                    
                case FigureState.Following:
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                    if (targetPlayer != null)
                    {
                        Vector2 followPos = targetPlayer.Position - (targetPlayer.Position - Position).SafeNormalize() * FollowDistance;
                        Position = Vector2.Lerp(Position, followPos, 2f * Engine.DeltaTime);
                        
                        sprite.Scale.X = targetPlayer.Position.X > Position.X ? 1 : -1;
                        
                        if (IsHostile && Collide.Check(this, targetPlayer))
                        {
                            targetPlayer.Die(Vector2.Zero);
                        }
                        
                        if (Scene.OnInterval(0.1f))
                        {
                            CreateShadowParticle();
                        }
                    }
                    break;
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            if (State != FigureState.Hidden)
            {
                Draw.Rect(Position.X - 12, Position.Y - 48, 24, 48, Color.Black * 0.5f);
            }
            base.Render();
        }
        #endregion
    }

    public class ShadowParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public ShadowParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.95f;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 3f, Color.Black * (alpha * 0.4f), 3);
        }
    }
}
