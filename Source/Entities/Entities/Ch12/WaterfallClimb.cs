namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// WaterfallClimb - Vertical climb section with water currents
    /// Player must climb against flowing water with hazards
    /// Sprite path: objects/DZ/DZ/waterfall_climb/
    /// </summary>
    [CustomEntity("DZ/WaterfallClimb")]
    [Tracked]
    public class WaterfallClimb : Actor
    {
        #region Enums
        public enum WaterState
        {
            Flowing,
            Rushing,
            Calm
        }
        #endregion

        #region Properties
        public WaterState State { get; private set; }
        public float FlowStrength { get; private set; }
        public float RushInterval { get; private set; }
        public float RushDuration { get; private set; }
        public bool IsActive { get; private set; }
        
        private Sprite sprite;
        private Rectangle climbArea;
        private float rushTimer;
        private float flowTimer;
        private Level level;
        private List<WaterDroplet> droplets;
        private List<WaterSplash> splashes;
        private bool isRushing;
        private float currentStrength;
        #endregion

        #region Constructor
        public WaterfallClimb(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("flowStrength", 80f),
                data.Float("rushInterval", 5f),
                data.Float("rushDuration", 2f),
                data.Width,
                data.Height
            );
        }

        public WaterfallClimb(Vector2 position, int width, int height, float flowStrength = 80f,
            float rushInterval = 5f, float rushDuration = 2f)
            : base(position)
        {
            Initialize(flowStrength, rushInterval, rushDuration, width, height);
        }

        private void Initialize(float flowStrength, float rushInterval, float rushDuration, int width, int height)
        {
            FlowStrength = flowStrength;
            RushInterval = rushInterval;
            RushDuration = rushDuration;
            
            climbArea = new Rectangle((int)Position.X, (int)Position.Y, width, height);
            State = WaterState.Flowing;
            rushTimer = 0f;
            flowTimer = 0f;
            isRushing = false;
            currentStrength = flowStrength;
            IsActive = true;
            droplets = new List<WaterDroplet>();
            splashes = new List<WaterSplash>();
            
            Collider = new Hitbox(width, height);
            
            Add(sprite = GFX.SpriteBank.Create("waterfall"));
            sprite.Play("flowing");
        }
        #endregion

        #region Public Methods
        public void StartRush()
        {
            if (isRushing) return;
            
            isRushing = true;
            State = WaterState.Rushing;
            currentStrength = FlowStrength * 2f;
            sprite.Play("rushing");
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.3f);
            
            Add(new Coroutine(RushRoutine()));
        }

        public void SetCalm()
        {
            State = WaterState.Calm;
            currentStrength = FlowStrength * 0.3f;
            sprite.Play("calm");
        }
        #endregion

        #region Private Methods
        private IEnumerator RushRoutine()
        {
            float duration = RushDuration;
            
            while (duration > 0f)
            {
                duration -= Engine.DeltaTime;
                
                // Create extra droplets during rush
                if (Scene.OnInterval(0.03f))
                {
                    CreateDroplet();
                }
                
                yield return null;
            }
            
            isRushing = false;
            State = WaterState.Flowing;
            currentStrength = FlowStrength;
            sprite.Play("flowing");
        }

        private void CreateDroplet()
        {
            var droplet = new WaterDroplet(
                new Vector2(
                    Position.X + Calc.Random.NextFloat() * climbArea.Width - climbArea.Width / 2,
                    Position.Y
                ),
                new Vector2(
                    Calc.Random.NextFloat() * currentStrength * 0.5f - 20f,
                    -Calc.Random.NextFloat() * 40f
                )
            );
            droplets.Add(droplet);
            Scene.Add(droplet);
        }

        private void CreateSplash(Vector2 position)
        {
            var splash = new WaterSplash(position);
            splashes.Add(splash);
            Scene.Add(splash);
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
            
            if (!IsActive) return;
            
            flowTimer += Engine.DeltaTime;
            
            // Auto rush cycle
            if (!isRushing && State == WaterState.Flowing)
            {
                rushTimer += Engine.DeltaTime;
                if (rushTimer >= RushInterval)
                {
                    rushTimer = 0f;
                    StartRush();
                }
            }
            
            // Create droplets
            if (Scene.OnInterval(0.08f))
            {
                CreateDroplet();
            }
            
            // Apply water current to player
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && climbArea.Contains(new Point((int)player.Position.X, (int)player.Position.Y)))
            {
                // Push player down
                player.Speed.Y += currentStrength * Engine.DeltaTime;
                
                // Slow horizontal movement
                player.Speed.X *= 0.95f;
                
                // Check if player is climbing
                if (player.StateMachine.State == Player.StClimb)
                {
                    // Extra stamina drain
                    if (Scene.OnInterval(0.3f))
                    {
                        // Would need access to player stamina
                    }
                }
            }
            
            droplets.RemoveAll(d => d == null || d.Scene == null);
            splashes.RemoveAll(s => s == null || s.Scene == null);
        }

        public override void Render()
        {
            // Draw water area
            Draw.Rect(climbArea, Color.Cyan * 0.3f);
            
            // Draw flow lines
            int lineCount = (int)(climbArea.Height / 20f);
            for (int i = 0; i < lineCount; i++)
            {
                float y = Position.Y + i * 20 + (flowTimer * currentStrength * 0.5f) % 20;
                float alpha = 0.2f + (float)Math.Sin(flowTimer * 2f + i) * 0.1f;
                Draw.Line(Position.X, y, Position.X + climbArea.Width, y, Color.White * alpha, 1f);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// WaterDroplet - Falling water particle
    /// </summary>
    public class WaterDroplet : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public WaterDroplet(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = 2f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 50f * Engine.DeltaTime;
            
            lifetime -= Engine.DeltaTime;
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                // Minor push
                player.Speed += velocity * 0.1f;
            }
            
            // Check ground collision
            if (OnGround() || lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 3f, Color.Cyan * (alpha * 0.5f), 4);
        }
    }

    /// <summary>
    /// WaterSplash - Splash effect when water hits surface
    /// </summary>
    public class WaterSplash : Actor
    {
        private float lifetime;
        private float maxLifetime;
        private List<Vector2> splashPoints;

        public WaterSplash(Vector2 position)
            : base(position)
        {
            maxLifetime = 0.3f;
            lifetime = maxLifetime;
            splashPoints = new List<Vector2>();
            
            for (int i = 0; i < 6; i++)
            {
                float angle = -MathHelper.PiOver2 + (i - 2.5f) * 0.3f;
                splashPoints.Add(Calc.AngleToVector(angle, 10f));
            }
        }

        public override void Update()
        {
            base.Update();
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            float scale = 1f + (1f - alpha) * 2f;
            
            foreach (var point in splashPoints)
            {
                Draw.Circle(Position + point * scale, 2f, Color.White * (alpha * 0.6f), 3);
            }
        }
    }

    /// <summary>
    /// WaterCurrentZone - Area with directional water current
    /// </summary>
    [CustomEntity("DZ/WaterCurrentZone")]
    public class WaterCurrentZone : Trigger
    {
        private Vector2 currentDirection;
        private float currentStrength;

        public WaterCurrentZone(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            float angle = data.Float("currentAngle", 0f) * (float)(Math.PI / 180f);
            currentDirection = Calc.AngleToVector(angle, 1f);
            currentStrength = data.Float("currentStrength", 100f);
        }

        public override void OnEnter(Player player)
        {
            player.Speed += currentDirection * currentStrength * Engine.DeltaTime;
        }
    }
}

