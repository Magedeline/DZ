namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// SpiralStaircase - Rotating staircase with moving platforms
    /// Platforms rotate around central pillar
    /// Sprite path: objects/spiral_staircase/
    /// </summary>
    [CustomEntity("MaggyHelper/SpiralStaircase")]
    [Tracked]
    public class SpiralStaircase : Actor
    {
        #region Enums
        public enum StaircaseState
        {
            Static,
            Rotating,
            Accelerating,
            Reversing,
            Stopped
        }
        #endregion

        #region Properties
        public StaircaseState State { get; private set; }
        public float RotationSpeed { get; private set; }
        public float MaxSpeed { get; private set; }
        public int PlatformCount { get; private set; }
        public float Radius { get; private set; }
        public bool IsRotating => State == StaircaseState.Rotating || State == StaircaseState.Accelerating;
        
        private Sprite centerSprite;
        private List<SpiralPlatform> platforms;
        private float currentAngle;
        private float currentSpeed;
        private bool clockwise;
        private Level level;
        private VertexLight centerLight;
        #endregion

        #region Constructor
        public SpiralStaircase(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("rotationSpeed", 0.5f),
                data.Float("maxSpeed", 2f),
                data.Int("platformCount", 8),
                data.Float("radius", 100f),
                data.Bool("clockwise", true)
            );
        }

        public SpiralStaircase(Vector2 position, float rotationSpeed = 0.5f, float maxSpeed = 2f,
            int platformCount = 8, float radius = 100f, bool clockwise = true)
            : base(position)
        {
            Initialize(rotationSpeed, maxSpeed, platformCount, radius, clockwise);
        }

        private void Initialize(float rotationSpeed, float maxSpeed, int platformCount, float radius, bool clockwise)
        {
            RotationSpeed = rotationSpeed;
            MaxSpeed = maxSpeed;
            PlatformCount = platformCount;
            Radius = radius;
            this.clockwise = clockwise;
            
            State = StaircaseState.Static;
            currentAngle = 0f;
            currentSpeed = 0f;
            platforms = new List<SpiralPlatform>();
            
            Collider = new Hitbox(24f, 24f, -12f, -12f);
            
            Add(centerSprite = GFX.SpriteBank.Create("spiral_center"));
            centerSprite.Play("idle");
            
            Add(centerLight = new VertexLight(Color.Gold, 0.4f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void StartRotation()
        {
            State = StaircaseState.Rotating;
            currentSpeed = RotationSpeed;
            centerSprite.Play("rotating");
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void StopRotation()
        {
            State = StaircaseState.Stopped;
            currentSpeed = 0f;
            centerSprite.Play("idle");
        }

        public void Accelerate()
        {
            State = StaircaseState.Accelerating;
            Add(new Coroutine(AccelerateRoutine()));
        }

        public void Reverse()
        {
            State = StaircaseState.Reversing;
            clockwise = !clockwise;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region Private Methods
        private IEnumerator AccelerateRoutine()
        {
            while (currentSpeed < MaxSpeed)
            {
                currentSpeed += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            
            currentSpeed = MaxSpeed;
            State = StaircaseState.Rotating;
        }

        private void CreatePlatforms()
        {
            for (int i = 0; i < PlatformCount; i++)
            {
                float angle = (MathHelper.TwoPi / PlatformCount) * i;
                float height = (i / (float)PlatformCount) * 200f;
                
                var platform = new SpiralPlatform(
                    Position + new Vector2(0f, -height),
                    Radius,
                    angle,
                    i
                );
                platforms.Add(platform);
                Scene.Add(platform);
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            CreatePlatforms();
        }

        public override void Update()
        {
            base.Update();
            
            if (IsRotating)
            {
                float direction = clockwise ? 1f : -1f;
                currentAngle += currentSpeed * direction * Engine.DeltaTime;
                
                // Update all platforms
                for (int i = 0; i < platforms.Count; i++)
                {
                    float platformAngle = currentAngle + (MathHelper.TwoPi / PlatformCount) * i;
                    platforms[i].UpdatePosition(Position, platformAngle);
                }
            }
        }

        public override void Render()
        {
            // Draw center pillar
            Draw.Rect(Position.X - 12, Position.Y - 200, 24, 200, Color.Brown);
            
            // Draw rotation indicator
            if (IsRotating)
            {
                float indicatorAngle = currentAngle;
                Vector2 indicatorPos = Position + Calc.AngleToVector(indicatorAngle, 20f);
                Draw.Circle(indicatorPos, 4f, Color.Gold * 0.6f, 4);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// SpiralPlatform - Individual platform on spiral staircase
    /// </summary>
    public class SpiralPlatform : Solid
    {
        private float radius;
        private float baseAngle;
        private int index;
        private Sprite sprite;

        public SpiralPlatform(Vector2 centerPosition, float radius, float angle, int index)
            : base(centerPosition, 48f, 8f, false)
        {
            this.radius = radius;
            this.baseAngle = angle;
            this.index = index;
            
            Add(sprite = GFX.SpriteBank.Create("spiral_platform"));
        }

        public void UpdatePosition(Vector2 center, float angle)
        {
            Vector2 offset = Calc.AngleToVector(angle, radius);
            Position = center + offset - new Vector2(24f, 4f);
        }

        public override void Render()
        {
            Draw.Rect(Collider.Bounds, Color.Gray);
            base.Render();
        }
    }

    /// <summary>
    /// StaircaseSwitch - Switch to control spiral staircase
    /// </summary>
    [CustomEntity("MaggyHelper/StaircaseSwitch")]
    public class StaircaseSwitch : Actor
    {
        private Sprite sprite;
        private SpiralStaircase staircase;
        private string staircaseId;
        private bool isPressed;

        public StaircaseSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            staircaseId = data.Attr("staircaseId", "");
            isPressed = false;
            
            Collider = new Hitbox(24f, 16f, -12f, -16f);
            Add(sprite = GFX.SpriteBank.Create("staircase_switch"));
            sprite.Play("unpressed");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find staircase
            foreach (var stair in scene.Tracker.GetEntities<SpiralStaircase>())
            {
                staircase = (SpiralStaircase)stair;
                break;
            }
        }

        public override void Update()
        {
            base.Update();
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                if (!isPressed)
                {
                    Press();
                }
            }
            else if (isPressed)
            {
                Release();
            }
        }

        private void Press()
        {
            isPressed = true;
            sprite.Play("pressed");
            Audio.Play("event:/game/general/diamond_get", Position);
            
            if (staircase.IsRotating)
                staircase.StopRotation();
            else
                staircase.StartRotation();
        }

        private void Release()
        {
            isPressed = false;
            sprite.Play("unpressed");
        }
    }
}
