using Celeste.Utils;

namespace Celeste
{
    /// <summary>
    /// Tower obstacle entity that can be placed around the 3D tower
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/TowerObstacle")]
    [Tracked]
    [HotReloadable]
    public class TowerObstacle : Entity
    {
        public enum ObstacleType
        {
            Spikes,
            Spinner,
            MovingPlatform,
            FallingBlock,
            LaserBeam,
            WindTunnel,
            Portal,
            None
        }

        public enum MovementPattern
        {
            Static,
            Circular,
            Horizontal,
            Vertical,
            Zigzag,
            None
        }

        public ObstacleType Type { get; private set; }
        public MovementPattern Pattern { get; private set; }
        public float MoveSpeed { get; private set; }
        public float RotationSpeed { get; private set; }
        public float ActivationDelay { get; private set; }
        public float DamageRadius { get; private set; }
        public new float Height { get; private set; }
        public float DetectionRange { get; private set; }

        private Vector2 startPosition;
        private float rotation;
        private Cooldown activationCooldown;
        private bool isActive;
        private Sprite sprite;

        public TowerObstacle(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            startPosition = Position;

            // Parse obstacle type
            string typeStr = data.Attr("obstacleType", "Spikes");
            Type = Enum.TryParse<ObstacleType>(typeStr, true, out var parsedType) ? parsedType : ObstacleType.Spikes;

            // Parse movement pattern
            string patternStr = data.Attr("movementPattern", "Static");
            Pattern = Enum.TryParse<MovementPattern>(patternStr, true, out var parsedPattern) ? parsedPattern : MovementPattern.Static;

            // Read other properties
            MoveSpeed = data.Float("moveSpeed", 50f);
            RotationSpeed = data.Float("rotationSpeed", 1f);
            ActivationDelay = data.Float("activationDelay", 0f);
            DamageRadius = data.Float("damageRadius", 16f);
            Height = data.Float("height", 0f);
            DetectionRange = data.Float("detectionRange", 100f);

            // Set up collider
            Collider = new Circle(DamageRadius);
            
            // Set depth
            Depth = -50;

            // Add sprite based on obstacle type
            SetupSprite();
        }

        // Legacy constructor for internal use
        public TowerObstacle(Vector2 position, ObstacleType type, MovementPattern pattern)
            : base(position)
        {
            startPosition = position;
            Type = type;
            Pattern = pattern;
            MoveSpeed = 50f;
            RotationSpeed = 1f;
            DamageRadius = 16f;
            DetectionRange = 100f;
            Collider = new Circle(DamageRadius);
            Depth = -50;
            SetupSprite();
        }

        public TowerObstacle(Vector2 position) : this(position, ObstacleType.None, MovementPattern.None)
        {
        }

        private void SetupSprite()
        {
            string spritePath = Type switch
            {
                ObstacleType.Spikes => "objects/tower/obstacles/spikes",
                ObstacleType.Spinner => "objects/tower/obstacles/spinner",
                ObstacleType.MovingPlatform => "objects/tower/obstacles/platform",
                ObstacleType.FallingBlock => "objects/tower/obstacles/falling",
                ObstacleType.LaserBeam => "objects/tower/obstacles/laser",
                ObstacleType.WindTunnel => "objects/tower/obstacles/wind",
                ObstacleType.Portal => "objects/tower/obstacles/portal",
                _ => "objects/tower/obstacles/default"
            };

            if (GFX.Game.Has(spritePath))
            {
                sprite = GFX.SpriteBank.Create("towerObstacle");
                Add(sprite);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            activationCooldown = new Cooldown(ActivationDelay, startReady: ActivationDelay <= 0);
            isActive = ActivationDelay <= 0;
        }

        public override void Update()
        {
            base.Update();

            // Handle activation delay
            if (!isActive)
            {
                if (activationCooldown.Update(Engine.DeltaTime))
                {
                    isActive = true;
                }
                return;
            }

            // Update rotation
            rotation += RotationSpeed * Engine.DeltaTime;

            // Update position based on movement pattern
            UpdateMovement();

            // Check for player collision
            CheckPlayerCollision();
        }

        private void UpdateMovement()
        {
            switch (Pattern)
            {
                case MovementPattern.Circular:
                    Position = startPosition + new Vector2(
                        (float)Math.Cos(rotation) * MoveSpeed,
                        (float)Math.Sin(rotation) * MoveSpeed
                    );
                    break;

                case MovementPattern.Horizontal:
                    Position = startPosition + new Vector2(
                        (float)Math.Sin(rotation) * MoveSpeed,
                        0
                    );
                    break;

                case MovementPattern.Vertical:
                    Position = startPosition + new Vector2(
                        0,
                        (float)Math.Sin(rotation) * MoveSpeed
                    );
                    break;

                case MovementPattern.Zigzag:
                    Position = startPosition + new Vector2(
                        (float)Math.Sin(rotation) * MoveSpeed,
                        (float)Math.Sin(rotation * 2) * MoveSpeed * 0.5f
                    );
                    break;
            }
        }

        private void CheckPlayerCollision()
        {
            if (Type == ObstacleType.MovingPlatform || Type == ObstacleType.Portal)
                return; // These don't damage player

            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && Collider != null && Collider.Collide(player))
            {
                OnPlayerCollide(player);
            }
        }

        private void OnPlayerCollide(global::Celeste.Player player)
        {
            switch (Type)
            {
                case ObstacleType.WindTunnel:
                    // Apply wind force instead of damage
                    player.Speed.Y -= MoveSpeed * Engine.DeltaTime;
                    break;

                default:
                    // Deal damage
                    player.Die(Vector2.Normalize(player.Position - Position));
                    break;
            }
        }

        public override void Render()
        {
            base.Render();

            // Fallback render if no sprite
            if (sprite == null)
            {
                ShapeRenderer.DrawCircleOutline(Position, DamageRadius, GetObstacleColor(), 2f);
            }
        }

        private Color GetObstacleColor()
        {
            return Type switch
            {
                ObstacleType.Spikes => Color.Red,
                ObstacleType.Spinner => Color.Orange,
                ObstacleType.MovingPlatform => Color.Green,
                ObstacleType.FallingBlock => Color.Brown,
                ObstacleType.LaserBeam => Color.Cyan,
                ObstacleType.WindTunnel => Color.LightBlue,
                ObstacleType.Portal => Color.Purple,
                _ => Color.Gray
            };
        }
    }
}
