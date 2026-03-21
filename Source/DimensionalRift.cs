namespace MaggyHelper.Entities.Effects
{
    public class DimensionalRift : Entity
    {
        private float radius;
        private float duration;
        private float currentTime;
        private Vector2[] distortionPoints;
        private const int DISTORTION_POINT_COUNT = 8;
        private VertexLight light;
        private SineWave sine;

        public DimensionalRift(Vector2 position, float radius, float duration) : base(position)
        {
            this.radius = radius;
            this.duration = duration;
            this.currentTime = 0f;

            // Initialize distortion points
            distortionPoints = new Vector2[DISTORTION_POINT_COUNT];
            for (int i = 0; i < DISTORTION_POINT_COUNT; i++)
            {
                float angle = (i / (float)DISTORTION_POINT_COUNT) * MathHelper.TwoPi;
                distortionPoints[i] = Calc.AngleToVector(angle, radius);
            }

            // Add visual effects
            Add(light = new VertexLight(Color.Purple, 0.5f, 32, 64));
            Add(sine = new SineWave(0.8f));

            // Set depth to appear in front of most entities
            Depth = -1000;
        }

        public override void Update()
        {
            base.Update();

            // Update lifetime
            currentTime += Engine.DeltaTime;
            if (currentTime >= duration)
            {
                RemoveSelf();
                return;
            }

            // Update distortion points
            for (int i = 0; i < DISTORTION_POINT_COUNT; i++)
            {
                float angle = (i / (float)DISTORTION_POINT_COUNT) * MathHelper.TwoPi + currentTime;
                float distortedRadius = radius + sine.Value * 8f;
                distortionPoints[i] = Calc.AngleToVector(angle, distortedRadius);
            }

            // Check for player collision
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < radius)
            {
                // Apply distortion effect to player
                Vector2 diff = player.Position - Position;
                float distortionStrength = 1f - (diff.Length() / radius);
                Vector2 distortion = diff.Perpendicular() * (sine.Value * 4f * distortionStrength);
                player.Position += distortion * Engine.DeltaTime;
            }

            // Update visual effects
            light.Alpha = 0.5f + sine.Value * 0.3f;
        }

        public override void Render()
        {
            base.Render();

            // Render rift effect
            Color color = Color.Purple * (0.7f + sine.Value * 0.3f);
            for (int i = 0; i < DISTORTION_POINT_COUNT; i++)
            {
                int next = (i + 1) % DISTORTION_POINT_COUNT;
                Vector2 current = Position + distortionPoints[i];
                Vector2 nextPoint = Position + distortionPoints[next];
                Draw.Line(current, nextPoint, color);
            }

            // Render inner distortion
            Draw.Circle(Position, radius * 0.5f, color * 0.5f, 16);
        }
    }
}



