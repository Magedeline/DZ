using Celeste.Utils;

namespace Celeste.Entities
{
    /// <summary>
    /// Factory entity that creates sets of obstacles for the 3D Tower
    /// Place in map editor to automatically generate obstacle patterns around a Tower3D
    /// </summary>
    [CustomEntity(ids: "DZ/TowerObstacleFactory")]
    public class TowerObstacleFactoryEntity : Entity
    {
        private static Pcg32Random IndexedRandom(int index)
        {
            return new Pcg32Random(unchecked((uint)index));
        }

        public enum BackgroundStyle
        {
            Default,
            Mystical,
            Dark,
            Golden,
            Ethereal,
            Crystal
        }

        public enum ObstacleSetType
        {
            Beginner,
            Intermediate,
            Advanced,
            Expert,
            Random
        }

        public enum ObstaclePattern
        {
            Rings,
            Spiral,
            Zigzag,
            Gauntlet,
            Scattered
        }

        public ObstacleSetType SetType { get; private set; }
        public ObstaclePattern Pattern { get; private set; }
        public BackgroundStyle BgStyle { get; private set; }
        public bool CreateBackground { get; private set; }
        public bool CreateObstacles { get; private set; }
        public bool AutoPositionAroundTower { get; private set; }
        public float TowerRadius { get; private set; }
        public int ObstacleCount { get; private set; }
        public float VerticalSpacing { get; private set; }
        public float PatternRotation { get; private set; }
        public float ActivationDelay { get; private set; }

        private TitanTower3D associatedTower;
        private List<TowerObstacle> createdObstacles = new List<TowerObstacle>();
        private bool initialized = false;

        public TowerObstacleFactoryEntity(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            // Parse obstacle set type
            string setTypeStr = data.Attr("obstacleSetType", "Intermediate");
            SetType = Enum.TryParse<ObstacleSetType>(setTypeStr, true, out var parsedSetType) 
                ? parsedSetType : ObstacleSetType.Intermediate;

            // Parse pattern
            string patternStr = data.Attr("obstaclePattern", "Spiral");
            Pattern = Enum.TryParse<ObstaclePattern>(patternStr, true, out var parsedPattern) 
                ? parsedPattern : ObstaclePattern.Spiral;

            // Parse background style
            string bgStyleStr = data.Attr("backgroundStyle", "Default");
            BgStyle = Enum.TryParse<BackgroundStyle>(bgStyleStr, true, out var parsedBgStyle) 
                ? parsedBgStyle : BackgroundStyle.Default;

            // Read other properties
            CreateBackground = data.Bool("createBackground", true);
            CreateObstacles = data.Bool("createObstacles", true);
            AutoPositionAroundTower = data.Bool("autoPositionAroundTower", true);
            TowerRadius = data.Float("towerRadius", 120f);
            ObstacleCount = data.Int("obstacleCount", 15);
            VerticalSpacing = data.Float("verticalSpacing", 150f);
            PatternRotation = data.Float("patternRotation", 0f);
            ActivationDelay = data.Float("activationDelay", 0f);

            Depth = -200;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            // Defer initialization to allow tower to be added first
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            
            if (!initialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            initialized = true;

            // Find associated tower
            if (AutoPositionAroundTower)
            {
                associatedTower = Scene.Tracker.GetEntity<TitanTower3D>();
            }

            // Create background if requested
            if (CreateBackground)
            {
                CreateTowerBackground();
            }

            // Create obstacles if requested
            if (CreateObstacles)
            {
                CreateObstacleSet();
            }
        }

        private void CreateTowerBackground()
        {
            Color tintColor = BgStyle switch
            {
                BackgroundStyle.Mystical => new Color(147, 112, 219),
                BackgroundStyle.Dark => new Color(77, 77, 102),
                BackgroundStyle.Golden => new Color(255, 215, 0),
                BackgroundStyle.Ethereal => new Color(173, 216, 230),
                BackgroundStyle.Crystal => new Color(102, 153, 204),
                _ => Color.White
            };

            // Background creation handled by TowerBackgroundStyleground entity
        }

        private void CreateObstacleSet()
        {
            Vector2 centerPos = AutoPositionAroundTower && associatedTower != null 
                ? associatedTower.Position 
                : Position;

            for (int i = 0; i < ObstacleCount; i++)
            {
                Vector2 obstaclePos = CalculateObstaclePosition(i, centerPos);
                TowerObstacle.ObstacleType type = GetObstacleTypeForSet(i);
                TowerObstacle.MovementPattern pattern = GetMovementPatternForSet(i);

                var obstacle = new TowerObstacle(obstaclePos, type, pattern);
                Scene.Add(obstacle);
                createdObstacles.Add(obstacle);
            }
        }

        private Vector2 CalculateObstaclePosition(int index, Vector2 center)
        {
            float height = index * VerticalSpacing;
            float angle = PatternRotation + GetAngleForPattern(index);

            return Pattern switch
            {
                ObstaclePattern.Rings => center + new Vector2(
                    (float)Math.Cos(angle) * TowerRadius,
                    -height
                ),
                ObstaclePattern.Spiral => center + new Vector2(
                    (float)Math.Cos(angle + index * 0.3f) * TowerRadius,
                    -height
                ),
                ObstaclePattern.Zigzag => center + new Vector2(
                    (index % 2 == 0 ? TowerRadius : -TowerRadius) * 0.8f,
                    -height
                ),
                ObstaclePattern.Gauntlet => center + new Vector2(
                    (float)Math.Sin(index * 0.5f) * TowerRadius,
                    -height
                ),
                ObstaclePattern.Scattered => center + new Vector2(
                    (float)(IndexedRandom(index).NextDouble() * 2 - 1) * TowerRadius,
                    -height
                ),
                _ => center + new Vector2(0, -height)
            };
        }

        private float GetAngleForPattern(int index)
        {
            return Pattern switch
            {
                ObstaclePattern.Rings => index * MathHelper.TwoPi / Math.Max(1, ObstacleCount / 5),
                ObstaclePattern.Spiral => index * 0.5f,
                _ => 0f
            };
        }

        private TowerObstacle.ObstacleType GetObstacleTypeForSet(int index)
        {
            return SetType switch
            {
                ObstacleSetType.Beginner => TowerObstacle.ObstacleType.Spikes,
                ObstacleSetType.Intermediate => index % 2 == 0 
                    ? TowerObstacle.ObstacleType.Spikes 
                    : TowerObstacle.ObstacleType.Spinner,
                ObstacleSetType.Advanced => index % 3 == 0 
                    ? TowerObstacle.ObstacleType.LaserBeam 
                    : (index % 3 == 1 ? TowerObstacle.ObstacleType.Spinner : TowerObstacle.ObstacleType.FallingBlock),
                ObstacleSetType.Expert => (TowerObstacle.ObstacleType)(index % 5),
                ObstacleSetType.Random => (TowerObstacle.ObstacleType)(IndexedRandom(index).Next(0, 5)),
                _ => TowerObstacle.ObstacleType.Spikes
            };
        }

        private TowerObstacle.MovementPattern GetMovementPatternForSet(int index)
        {
            return SetType switch
            {
                ObstacleSetType.Beginner => TowerObstacle.MovementPattern.Static,
                ObstacleSetType.Intermediate => index % 2 == 0 
                    ? TowerObstacle.MovementPattern.Static 
                    : TowerObstacle.MovementPattern.Horizontal,
                ObstacleSetType.Advanced => index % 3 == 0 
                    ? TowerObstacle.MovementPattern.Circular 
                    : TowerObstacle.MovementPattern.Zigzag,
                ObstacleSetType.Expert => (TowerObstacle.MovementPattern)(index % 4),
                ObstacleSetType.Random => (TowerObstacle.MovementPattern)(IndexedRandom(index).Next(0, 4)),
                _ => TowerObstacle.MovementPattern.Static
            };
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);

            // Clean up created obstacles
            foreach (var obstacle in createdObstacles)
            {
                obstacle?.RemoveSelf();
            }
            createdObstacles.Clear();
        }
    }

    /// <summary>
    /// Static helper class for creating tower backgrounds and obstacle sets programmatically
    /// </summary>
    public static class TowerObstacleFactory
    {
        public static TowerBackgroundStyleground CreateTowerBackground(Vector2 position, TowerObstacleFactoryEntity.BackgroundStyle style)
        {
            var background = new TowerBackgroundStyleground(position);
            background.SetTintColor(style switch
            {
                TowerObstacleFactoryEntity.BackgroundStyle.Mystical => new Color(0.6f, 0.4f, 0.8f),
                TowerObstacleFactoryEntity.BackgroundStyle.Dark => new Color(0.3f, 0.3f, 0.4f),
                TowerObstacleFactoryEntity.BackgroundStyle.Crystal => new Color(0.4f, 0.6f, 0.8f),
                _ => Color.White
            });
            return background;
        }

        public static List<TowerObstacle> CreateObstacleSet(TitanTower3D tower, TowerObstacleFactoryEntity.ObstacleSetType setType)
        {
            var obstacles = new List<TowerObstacle>();
            int count = setType switch
            {
                TowerObstacleFactoryEntity.ObstacleSetType.Beginner => 5,
                TowerObstacleFactoryEntity.ObstacleSetType.Intermediate => 8,
                TowerObstacleFactoryEntity.ObstacleSetType.Advanced => 12,
                TowerObstacleFactoryEntity.ObstacleSetType.Expert => 15,
                _ => 5
            };

            for (int i = 0; i < count; i++)
            {
                float height = setType == TowerObstacleFactoryEntity.ObstacleSetType.Beginner ? 100 + i * 150 : 100 + i * 120;
                var type = i % 2 == 0 ? TowerObstacle.ObstacleType.Spikes : TowerObstacle.ObstacleType.MovingPlatform;
                var pattern = i % 3 == 0 ? TowerObstacle.MovementPattern.Circular : TowerObstacle.MovementPattern.Static;

                var obstacle = new TowerObstacle(tower.Position + new Vector2(0, -height), type, pattern);
                obstacles.Add(obstacle);
            }

            return obstacles;
        }
    }
}
