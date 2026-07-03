namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// TitanFootprint - Massive crushing hazard with shadow warning
    /// Shows a shadow indicator before the foot comes down
    /// Sprite path: objects/DZ/DZ/titan_footprint/
    /// </summary>
    [CustomEntity("DZ/TitanFootprint")]
    [Tracked]
    public class TitanFootprint : Actor
    {
        #region Enums
        public enum FootprintState
        {
            Idle,
            Warning,
            Falling,
            Crushing,
            Rising,
            Cooldown
        }
        #endregion

        #region Properties
        public FootprintState State { get; private set; }
        public float WarningDuration { get; private set; }
        public float CrushDuration { get; private set; }
        public float CrushWidth { get; private set; }
        public float CrushHeight { get; private set; }
        public float CooldownTime { get; private set; }
        public float TriggerDistance { get; private set; }
        
        private Sprite sprite;
        private Image shadowImage;
        private float stateTimer;
        private float shadowAlpha;
        private float crushY;
        private float targetY;
        private Player targetPlayer;
        private Level level;
        private bool triggered;
        private float shakeIntensity;
        #endregion

        #region Constructor
        public TitanFootprint(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("warningDuration", 1.5f),
                data.Float("crushDuration", 0.3f),
                data.Float("crushWidth", 120f),
                data.Float("crushHeight", 200f),
                data.Float("cooldownTime", 3f),
                data.Float("triggerDistance", 80f)
            );
        }

        public TitanFootprint(Vector2 position, float warningDuration = 1.5f, float crushDuration = 0.3f,
            float crushWidth = 120f, float crushHeight = 200f, float cooldownTime = 3f, float triggerDistance = 80f)
            : base(position)
        {
            Initialize(warningDuration, crushDuration, crushWidth, crushHeight, cooldownTime, triggerDistance);
        }

        private void Initialize(float warningDuration, float crushDuration, float crushWidth,
            float crushHeight, float cooldownTime, float triggerDistance)
        {
            WarningDuration = warningDuration;
            CrushDuration = crushDuration;
            CrushWidth = crushWidth;
            CrushHeight = crushHeight;
            CooldownTime = cooldownTime;
            TriggerDistance = triggerDistance;
            
            State = FootprintState.Idle;
            stateTimer = 0f;
            shadowAlpha = 0f;
            crushY = -CrushHeight - 100f;
            targetY = 0f;
            triggered = false;
            shakeIntensity = 0f;
            
            // Large warning area collider
            Collider = new Hitbox(CrushWidth, 16f, -CrushWidth / 2f, -8f);
            
            // Setup sprite (the actual foot)
            Add(sprite = DZModule.SpriteBank.Create("titan_footprint"));
            sprite.Position = new Vector2(0f, crushY);
            sprite.Play("idle");
        }
        #endregion

        #region Public Methods
        public void Trigger()
        {
            if (State != FootprintState.Idle) return;
            
            triggered = true;
            State = FootprintState.Warning;
            stateTimer = WarningDuration;
            shadowAlpha = 0f;
            
            Audio.Play("event:/game/gen_crumble_fall", Position);
        }
        #endregion

        #region Private Methods
        private void Crush()
        {
            State = FootprintState.Crushing;
            crushY = -CrushHeight - 100f;
            targetY = 0f;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.8f);
            
            Add(new Coroutine(CrushRoutine()));
        }

        private IEnumerator CrushRoutine()
        {
            float crushSpeed = (crushY - targetY) / CrushDuration;
            
            while (crushY > targetY)
            {
                crushY -= crushSpeed * Engine.DeltaTime;
                sprite.Position = new Vector2(0f, crushY);
                
                // Screen shake intensifies as foot gets closer
                shakeIntensity = 1f - (crushY / -CrushHeight);
                level?.Shake(shakeIntensity * 0.5f);
                
                // Check for player crush
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    Rectangle crushZone = new Rectangle(
                        (int)(Position.X - CrushWidth / 2),
                        (int)(Position.Y + crushY),
                        (int)CrushWidth,
                        (int)(-crushY)
                    );
                    
                    if (player.Collider.Bounds.Intersects(crushZone))
                    {
                        player.Die(Vector2.Zero);
                    }
                }
                
                yield return null;
            }
            
            // Impact
            crushY = targetY;
            sprite.Position = new Vector2(0f, crushY);
            sprite.Play("impact");
            
            level?.Shake(1f);
            level?.Flash(Color.White * 0.3f);
            
            // Create dust particles
            for (int i = 0; i < 20; i++)
            {
                level?.ParticlesFG.Emit(ParticleTypes.Dust, 1,
                    Position + new Vector2(Calc.Random.NextFloat() * CrushWidth - CrushWidth / 2, Calc.Random.NextFloat() * CrushWidth - CrushWidth / 2),
                    Vector2.One * 8f);
            }
            
            State = FootprintState.Crushing;
            stateTimer = 0.5f;
        }

        private void Rise()
        {
            State = FootprintState.Rising;
            targetY = -CrushHeight - 100f;
            
            Add(new Coroutine(RiseRoutine()));
        }

        private IEnumerator RiseRoutine()
        {
            float riseSpeed = 300f;
            
            while (crushY > targetY)
            {
                crushY -= riseSpeed * Engine.DeltaTime;
                sprite.Position = new Vector2(0f, crushY);
                yield return null;
            }
            
            State = FootprintState.Cooldown;
            stateTimer = CooldownTime;
            triggered = false;
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
                case FootprintState.Idle:
                    // Check for player in trigger zone
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                    if (targetPlayer != null)
                    {
                        float distance = Vector2.Distance(Position, targetPlayer.Position);
                        if (distance < TriggerDistance)
                        {
                            Trigger();
                        }
                    }
                    break;
                    
                case FootprintState.Warning:
                    stateTimer -= Engine.DeltaTime;
                    shadowAlpha = 1f - (stateTimer / WarningDuration);
                    
                    // Increasing screen shake
                    shakeIntensity = shadowAlpha;
                    level?.Shake(shakeIntensity * 0.3f);
                    
                    if (stateTimer <= 0f)
                    {
                        Crush();
                    }
                    break;
                    
                case FootprintState.Crushing:
                    stateTimer -= Engine.DeltaTime;
                    if (stateTimer <= 0f)
                    {
                        Rise();
                    }
                    break;
                    
                case FootprintState.Cooldown:
                    stateTimer -= Engine.DeltaTime;
                    if (stateTimer <= 0f)
                    {
                        State = FootprintState.Idle;
                        sprite.Play("idle");
                    }
                    break;
            }
        }

        public override void Render()
        {
            // Draw shadow warning
            if (State == FootprintState.Warning || State == FootprintState.Falling)
            {
                // Dark shadow on ground
                Draw.Rect(
                    Position.X - CrushWidth / 2,
                    Position.Y - 8,
                    CrushWidth,
                    16,
                    Color.Black * (shadowAlpha * 0.6f)
                );
                
                // Expanding rings
                for (int i = 0; i < 3; i++)
                {
                    float ringSize = (shadowAlpha + i * 0.1f) * CrushWidth * 0.6f;
                    Draw.Circle(Position, ringSize, Color.Red * (shadowAlpha * 0.3f), 20);
                }
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// TitanFootprintSequence - Manages multiple footprints in sequence
    /// </summary>
    [CustomEntity("DZ/TitanFootprintSequence")]
    public class TitanFootprintSequence : Entity
    {
        private List<TitanFootprint> footprints;
        private int currentIndex;
        private float interval;
        private float timer;
        private bool active;

        public TitanFootprintSequence(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            interval = data.Float("interval", 1f);
            footprints = new List<TitanFootprint>();
            currentIndex = 0;
            timer = 0f;
            active = false;
        }

        public void AddFootprint(TitanFootprint footprint)
        {
            footprints.Add(footprint);
        }

        public void Start()
        {
            active = true;
            timer = 0f;
            currentIndex = 0;
        }

        public void Stop()
        {
            active = false;
        }

        public override void Update()
        {
            base.Update();
            
            if (!active || currentIndex >= footprints.Count) return;
            
            timer += Engine.DeltaTime;
            
            if (timer >= interval)
            {
                timer = 0f;
                footprints[currentIndex].Trigger();
                currentIndex++;
            }
        }
    }
}

