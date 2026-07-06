namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// CrownBearing - Floating crown that creates gravity wells
    /// Affects player movement with gravitational pull
    /// Sprite path: objects/DZ/DZ/crown_bearing/
    /// </summary>
    [CustomEntity("DZ/CrownBearing")]
    [Tracked]
    public class CrownBearing : Actor
    {
        #region Enums
        public enum CrownState
        {
            Idle,
            Active,
            Pulling,
            Pushing,
            Rotating
        }

        public enum GravityType
        {
            Pull,       // Attracts player toward crown
            Push,       // Repels player away from crown
            Orbit       // Creates orbital gravity
        }
        #endregion

        #region Properties
        public CrownState State { get; private set; }
        public GravityType GravityMode { get; private set; }
        public float GravityRadius { get; private set; }
        public float GravityStrength { get; private set; }
        public bool IsActive { get; private set; }
        
        private Sprite sprite;
        private float rotationTimer;
        private float floatTimer;
        private float pulseTimer;
        private Level level;
        private VertexLight crownLight;
        private List<GravityParticle> gravityParticles;
        private bool playerInRange;
        #endregion

        #region Constructor
        public CrownBearing(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Enum("gravityType", GravityType.Pull),
                data.Float("gravityRadius", 150f),
                data.Float("gravityStrength", 200f),
                data.Bool("isActive", true)
            );
        }

        public CrownBearing(Vector2 position, GravityType gravityType = GravityType.Pull,
            float gravityRadius = 150f, float gravityStrength = 200f, bool isActive = true)
            : base(position)
        {
            Initialize(gravityType, gravityRadius, gravityStrength, isActive);
        }

        private void Initialize(GravityType gravityType, float gravityRadius, float gravityStrength, bool isActive)
        {
            GravityMode = gravityType;
            GravityRadius = gravityRadius;
            GravityStrength = gravityStrength;
            IsActive = isActive;
            
            State = isActive ? CrownState.Active : CrownState.Idle;
            rotationTimer = 0f;
            floatTimer = 0f;
            pulseTimer = 0f;
            playerInRange = false;
            gravityParticles = new List<GravityParticle>();
            
            // No solid collision - floating entity
            Collider = new Hitbox(24f, 24f, -12f, -12f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("crown_bearing"));
            sprite.Play(isActive ? "active" : "idle");
            
            // Add golden glow
            Add(crownLight = new VertexLight(Color.Gold, isActive ? 0.6f : 0.2f, 16, 48));
        }
        #endregion

        #region Public Methods
        public void Activate()
        {
            IsActive = true;
            State = CrownState.Active;
            sprite.Play("activate");
            crownLight.Alpha = 0.6f;
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        public void Deactivate()
        {
            IsActive = false;
            State = CrownState.Idle;
            sprite.Play("deactivate");
            crownLight.Alpha = 0.2f;
        }

        public void SetGravityType(GravityType type)
        {
            GravityMode = type;
            
            switch (type)
            {
                case GravityType.Pull:
                    State = CrownState.Pulling;
                    crownLight.Color = Color.Gold;
                    break;
                case GravityType.Push:
                    State = CrownState.Pushing;
                    crownLight.Color = Color.Red;
                    break;
                case GravityType.Orbit:
                    State = CrownState.Rotating;
                    crownLight.Color = Color.Cyan;
                    break;
            }
        }
        #endregion

        #region Private Methods
        private void ApplyGravity(Player player)
        {
            if (!IsActive) return;
            
            Vector2 direction = Position - player.Position;
            float distance = direction.Length();
            
            if (distance > GravityRadius || distance < 10f) return;
            
            playerInRange = true;
            direction.Normalize();
            
            // Calculate force based on distance (stronger closer)
            float force = GravityStrength * (1f - distance / GravityRadius);
            
            switch (GravityMode)
            {
                case GravityType.Pull:
                    // Pull player toward crown
                    player.Speed += direction * force * Engine.DeltaTime;
                    break;
                    
                case GravityType.Push:
                    // Push player away from crown
                    player.Speed -= direction * force * Engine.DeltaTime;
                    break;
                    
                case GravityType.Orbit:
                    // Create orbital movement
                    Vector2 tangent = new Vector2(-direction.Y, direction.X);
                    player.Speed += tangent * force * 0.5f * Engine.DeltaTime;
                    player.Speed += direction * force * 0.3f * Engine.DeltaTime;
                    break;
            }
            
            // Create gravity particles
            if (Scene.OnInterval(0.05f))
            {
                CreateGravityParticle(player.Position);
            }
        }

        private void CreateGravityParticle(Vector2 fromPosition)
        {
            Vector2 direction = GravityMode == GravityType.Push ? 
                (fromPosition - Position).SafeNormalize() :
                (Position - fromPosition).SafeNormalize();
            
            var particle = new GravityParticle(
                fromPosition,
                direction * 100f,
                GravityMode
            );
            gravityParticles.Add(particle);
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
            
            // Rotation and float animation
            rotationTimer += Engine.DeltaTime * 2f;
            floatTimer += Engine.DeltaTime * 1.5f;
            pulseTimer += Engine.DeltaTime * 3f;
            
            // Float up and down
            sprite.Position = new Vector2(0f, (float)Math.Sin(floatTimer) * 6f);
            
            // Rotate
            sprite.Rotation = (float)Math.Sin(rotationTimer) * 0.1f;
            
            // Pulse effect
            float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
            sprite.Scale = Vector2.One * pulse;
            
            // Apply gravity to player
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                ApplyGravity(player);
            }
            
            playerInRange = false;
            
            // Clean up particles
            gravityParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw gravity field visualization
            if (IsActive)
            {
                Color fieldColor = GravityMode == GravityType.Pull ? Color.Gold :
                    GravityMode == GravityType.Push ? Color.Red : Color.Cyan;
                
                // Outer ring
                Draw.Circle(Position, GravityRadius, fieldColor * 0.15f, 32);
                
                // Inner rings
                for (int i = 1; i <= 3; i++)
                {
                    float ringRadius = GravityRadius * (i / 4f);
                    Draw.Circle(Position, ringRadius, fieldColor * 0.1f, 24);
                }
                
                // Direction arrows
                if (playerInRange)
                {
                    int arrowCount = 8;
                    for (int i = 0; i < arrowCount; i++)
                    {
                        float angle = (MathHelper.TwoPi / arrowCount) * i + rotationTimer;
                        Vector2 arrowPos = Position + Calc.AngleToVector(angle, GravityRadius * 0.6f);
                        
                        // Draw arrow pointing in gravity direction
                        Vector2 arrowDir = GravityMode == GravityType.Push ?
                            Calc.AngleToVector(angle, 1f) :
                            Calc.AngleToVector(angle + MathHelper.Pi, 1f);
                        
                        Draw.Line(arrowPos, arrowPos + arrowDir * 15f, fieldColor * 0.4f, 2f);
                    }
                }
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// GravityParticle - Visual particle for gravity effects
    /// </summary>
    public class GravityParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public GravityParticle(Vector2 position, Vector2 velocity, CrownBearing.GravityType gravityType)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = 0.5f;
            lifetime = maxLifetime;
            
            color = gravityType == CrownBearing.GravityType.Pull ? Color.Gold :
                gravityType == CrownBearing.GravityType.Push ? Color.Red : Color.Cyan;
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.95f;
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 4f, color * (alpha * 0.5f), 4);
        }
    }

    /// <summary>
    /// CrownBearingController - Manages multiple crown bearings
    /// </summary>
    [CustomEntity("DZ/CrownBearingController")]
    public class CrownBearingController : Entity
    {
        private List<CrownBearing> bearings;
        private float cycleTime;
        private float timer;

        public CrownBearingController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            cycleTime = data.Float("cycleTime", 5f);
            bearings = new List<CrownBearing>();
            timer = 0f;
        }

        public void RegisterBearing(CrownBearing bearing)
        {
            bearings.Add(bearing);
        }

        public override void Update()
        {
            base.Update();
            
            if (bearings.Count == 0) return;
            
            timer += Engine.DeltaTime;
            
            // Cycle through gravity types
            if (timer >= cycleTime)
            {
                timer = 0f;
                
                foreach (var bearing in bearings)
                {
                    // Rotate gravity type
                    CrownBearing.GravityType nextType = (CrownBearing.GravityType)(
                        ((int)bearing.GravityMode + 1) % 3
                    );
                    bearing.SetGravityType(nextType);
                }
            }
        }
    }
}

