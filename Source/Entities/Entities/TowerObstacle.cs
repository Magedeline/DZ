using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Obstacle entity for the 3D Tower minigame.
    /// Represents hazards that the player must avoid or interact with.
    /// </summary>
    public class TowerObstacle : Entity
    {
        public enum ObstacleType
        {
            Spikes,
            Block,
            Spinner,
            Laser,
            Projectile,
            LaserBeam,
            FallingBlock,
            MovingPlatform
        }

        public enum MovementPattern
        {
            Static,
            Rotating,
            Oscillating,
            Chasing,
            Spiraling,
            Horizontal,
            Circular,
            Zigzag
        }

        public ObstacleType Type { get; private set; }
        public MovementPattern Pattern { get; private set; }
        public Vector2 BasePosition { get; private set; }
        public float RotationAngle { get; set; }
        public float OscillationOffset { get; set; }

        private float rotationSpeed;
        private float oscillationSpeed;
        private float oscillationAmplitude;

        public TowerObstacle(Vector2 position, ObstacleType type, MovementPattern pattern)
            : base(position)
        {
            BasePosition = position;
            Type = type;
            Pattern = pattern;
            RotationAngle = 0f;
            OscillationOffset = 0f;

            // Set default speeds based on pattern
            switch (pattern)
            {
                case MovementPattern.Rotating:
                    rotationSpeed = 0.5f;
                    break;
                case MovementPattern.Oscillating:
                    oscillationSpeed = 2f;
                    oscillationAmplitude = 20f;
                    break;
                case MovementPattern.Spiraling:
                    rotationSpeed = 0.3f;
                    oscillationSpeed = 1f;
                    oscillationAmplitude = 10f;
                    break;
                default:
                    rotationSpeed = 0f;
                    oscillationSpeed = 0f;
                    break;
            }

            // Create collider based on type
            switch (type)
            {
                case ObstacleType.Spikes:
                    Collider = new Hitbox(16, 16, -8, -8);
                    break;
                case ObstacleType.Block:
                    Collider = new Hitbox(24, 24, -12, -12);
                    break;
                case ObstacleType.Spinner:
                    Collider = new Hitbox(20, 20, -10, -10);
                    break;
                default:
                    Collider = new Hitbox(16, 16, -8, -8);
                    break;
            }
        }

        public override void Update()
        {
            base.Update();

            switch (Pattern)
            {
                case MovementPattern.Rotating:
                    RotationAngle += rotationSpeed * Engine.DeltaTime;
                    Position = RotatePoint(BasePosition, RotationAngle);
                    break;

                case MovementPattern.Oscillating:
                    OscillationOffset += oscillationSpeed * Engine.DeltaTime;
                    float oscillation = (float)Math.Sin(OscillationOffset) * oscillationAmplitude;
                    Position = BasePosition + new Vector2(oscillation, 0);
                    break;

                case MovementPattern.Spiraling:
                    RotationAngle += rotationSpeed * Engine.DeltaTime;
                    OscillationOffset += oscillationSpeed * Engine.DeltaTime;
                    float spiralOffset = (float)Math.Sin(OscillationOffset) * oscillationAmplitude;
                    Vector2 rotated = RotatePoint(BasePosition, RotationAngle);
                    Position = rotated + new Vector2(spiralOffset, spiralOffset);
                    break;
            }
        }

        private Vector2 RotatePoint(Vector2 point, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            return new Vector2(
                point.X * cos - point.Y * sin,
                point.X * sin + point.Y * cos
            );
        }
    }
}
