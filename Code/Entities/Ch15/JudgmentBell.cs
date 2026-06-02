namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// JudgmentBell - Massive bell that creates shockwaves when rung
    /// Can be triggered by player or events, creates expanding wave hazards
    /// Sprite path: objects/judgment_bell/
    /// </summary>
    [CustomEntity("MaggyHelper/JudgmentBell")]
    [Tracked]
    public class JudgmentBell : Actor
    {
        #region Enums
        public enum BellState
        {
            Idle,
            Swinging,
            Ringing,
            Shockwave,
            Cooldown
        }
        #endregion

        #region Properties
        public BellState State { get; private set; }
        public float ShockwaveSpeed { get; private set; }
        public float ShockwaveRadius { get; private set; }
        public float ShockwaveDamage { get; private set; }
        public float CooldownTime { get; private set; }
        public int MaxRings { get; private set; }
        public bool CanPlayerRing { get; private set; }
        
        private Sprite sprite;
        private float swingAngle;
        private float swingSpeed;
        private int ringCount;
        private float stateTimer;
        private float currentShockwaveRadius;
        private List<ShockwaveRing> shockwaves;
        private Level level;
        private VertexLight bellLight;
        private bool hasRung;
        #endregion

        #region Constructor
        public JudgmentBell(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("shockwaveSpeed", 200f),
                data.Float("shockwaveRadius", 300f),
                data.Float("cooldownTime", 2f),
                data.Int("maxRings", 3),
                data.Bool("canPlayerRing", true)
            );
        }

        public JudgmentBell(Vector2 position, float shockwaveSpeed = 200f, float shockwaveRadius = 300f,
            float cooldownTime = 2f, int maxRings = 3, bool canPlayerRing = true)
            : base(position)
        {
            Initialize(shockwaveSpeed, shockwaveRadius, cooldownTime, maxRings, canPlayerRing);
        }

        private void Initialize(float shockwaveSpeed, float shockwaveRadius, float cooldownTime, int maxRings, bool canPlayerRing)
        {
            ShockwaveSpeed = shockwaveSpeed;
            ShockwaveRadius = shockwaveRadius;
            CooldownTime = cooldownTime;
            MaxRings = maxRings;
            CanPlayerRing = canPlayerRing;
            
            State = BellState.Idle;
            swingAngle = 0f;
            swingSpeed = 0f;
            ringCount = 0;
            stateTimer = 0f;
            currentShockwaveRadius = 0f;
            hasRung = false;
            shockwaves = new List<ShockwaveRing>();
            
            // Large bell collider
            Collider = new Hitbox(40f, 80f, -20f, -80f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("judgment_bell"));
            sprite.Play("idle");
            
            // Add golden glow
            Add(bellLight = new VertexLight(Color.Gold, 0.4f, 16, 48));
        }
        #endregion

        #region Public Methods
        public void Ring()
        {
            if (State != BellState.Idle) return;
            
            State = BellState.Swinging;
            swingSpeed = 3f;
            ringCount = 0;
            hasRung = false;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.2f);
        }

        public void ForceRing()
        {
            // Can ring even during cooldown
            State = BellState.Swinging;
            swingSpeed = 4f;
            ringCount = 0;
            hasRung = false;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.3f);
        }
        #endregion

        #region Private Methods
        private void CreateShockwave()
        {
            var shockwave = new ShockwaveRing(Position, ShockwaveSpeed, ShockwaveRadius);
            shockwaves.Add(shockwave);
            Scene.Add(shockwave);
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.Shake(0.4f);
            level?.Flash(Color.Gold * 0.2f);
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
                case BellState.Idle:
                    // Check for player interaction
                    if (CanPlayerRing)
                    {
                        var player = Scene.Tracker.GetEntity<Player>();
                        if (player != null && Collide.Check(this, player))
                        {
                            Ring();
                        }
                    }
                    break;
                    
                case BellState.Swinging:
                    // Swing the bell
                    swingAngle += swingSpeed * Engine.DeltaTime;
                    sprite.Rotation = (float)Math.Sin(swingAngle) * 0.3f;
                    
                    // Slow down swing
                    swingSpeed *= 0.98f;
                    
                    // Ring when swing peaks
                    if (!hasRung && Math.Abs(Math.Sin(swingAngle)) > 0.9f)
                    {
                        hasRung = true;
                        State = BellState.Ringing;
                        stateTimer = 0.3f;
                        CreateShockwave();
                        ringCount++;
                    }
                    
                    if (swingSpeed < 0.5f)
                    {
                        State = BellState.Cooldown;
                        stateTimer = CooldownTime;
                        sprite.Rotation = 0f;
                    }
                    break;
                    
                case BellState.Ringing:
                    stateTimer -= Engine.DeltaTime;
                    
                    if (stateTimer <= 0f && ringCount < MaxRings)
                    {
                        // Continue ringing
                        State = BellState.Swinging;
                        swingSpeed = 2.5f;
                        hasRung = false;
                    }
                    else if (stateTimer <= 0f)
                    {
                        State = BellState.Cooldown;
                        stateTimer = CooldownTime;
                    }
                    break;
                    
                case BellState.Cooldown:
                    stateTimer -= Engine.DeltaTime;
                    sprite.Rotation = 0f;
                    
                    if (stateTimer <= 0f)
                    {
                        State = BellState.Idle;
                        sprite.Play("idle");
                    }
                    break;
            }
            
            // Clean up shockwaves
            shockwaves.RemoveAll(s => s == null || s.Scene == null);
        }

        public override void Render()
        {
            // Draw bell rope
            Draw.Line(Position + new Vector2(0f, -80f), Position + new Vector2(0f, -120f), Color.Brown, 2f);
            
            base.Render();
            
            // Draw shockwave indicator
            if (State == BellState.Ringing || State == BellState.Shockwave)
            {
                Draw.Circle(Position, currentShockwaveRadius, Color.Gold * 0.2f, 32);
            }
        }
        #endregion
    }

    /// <summary>
    /// ShockwaveRing - Expanding shockwave hazard
    /// </summary>
    public class ShockwaveRing : Actor
    {
        private float speed;
        private float maxRadius;
        private float currentRadius;
        private float damageWidth;
        private bool isActive;

        public ShockwaveRing(Vector2 position, float speed, float maxRadius)
            : base(position)
        {
            this.speed = speed;
            this.maxRadius = maxRadius;
            currentRadius = 0f;
            damageWidth = 16f;
            isActive = true;
        }

        public override void Update()
        {
            base.Update();
            
            if (!isActive) return;
            
            // Expand
            currentRadius += speed * Engine.DeltaTime;
            
            // Check player collision at ring edge
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                float distance = Vector2.Distance(Position, player.Position);
                
                // Check if player is within the ring's damage zone
                if (Math.Abs(distance - currentRadius) < damageWidth)
                {
                    player.Die(Vector2.Zero);
                }
            }
            
            // Remove when fully expanded
            if (currentRadius >= maxRadius)
            {
                isActive = false;
                RemoveSelf();
            }
        }

        public override void Render()
        {
            if (!isActive) return;
            
            float alpha = 1f - (currentRadius / maxRadius);
            
            // Draw expanding ring
            Draw.Circle(Position, currentRadius, Color.Gold * (alpha * 0.6f), (int)damageWidth);
            Draw.Circle(Position, currentRadius - damageWidth / 2, Color.Orange * (alpha * 0.4f), (int)(damageWidth / 2));
        }
    }

    /// <summary>
    /// BellTrigger - Trigger that rings JudgmentBell when activated
    /// </summary>
    [CustomEntity("MaggyHelper/BellTrigger")]
    public class BellTrigger : Trigger
    {
        private JudgmentBell targetBell;
        private string bellId;
        private bool triggered;
        private bool oneShot;

        public BellTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            bellId = data.Attr("bellId", "");
            oneShot = data.Bool("oneShot", true);
            triggered = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find target bell by ID
            if (!string.IsNullOrEmpty(bellId))
            {
                foreach (var bell in scene.Tracker.GetEntities<JudgmentBell>())
                {
                    // Would need ID property on bell for proper matching
                    targetBell = (JudgmentBell)bell;
                    break;
                }
            }
        }

        public override void OnEnter(Player player)
        {
            if (triggered && oneShot) return;
            if (targetBell == null) return;
            
            triggered = true;
            targetBell.Ring();
        }
    }
}
