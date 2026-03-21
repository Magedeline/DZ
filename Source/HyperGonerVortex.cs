namespace MaggyHelper.Entities.Effects
{
    public class HyperGonerVortex : Entity
    {
        private float pullStrength;
        private float safeDistance;
        private float radius;
        private float pulseTimer;
        private const float PULSE_INTERVAL = 0.5f;
        private DisplacementRenderer.Burst burst;
        private VertexLight light;
        private SineWave sine;

        public HyperGonerVortex(Vector2 position, float pullStrength, float safeDistance) : base(position)
        {
            this.pullStrength = pullStrength;
            this.safeDistance = safeDistance;
            this.radius = 64f;

            // Add visual effects
            Add(light = new VertexLight(Color.Purple * 0.5f, 0f, 128, 256));
            Add(sine = new SineWave(0.5f));

            // Set depth to appear behind most entities
            Depth = 1000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            Level level = scene as Level;
            if (level != null)
            {
                burst = level.Displacement.AddBurst(Position, 0.5f, 0f, radius);
            }
        }

        public override void Update()
        {
            base.Update();

            // Pulse effect
            pulseTimer += Engine.DeltaTime;
            if (pulseTimer >= PULSE_INTERVAL)
            {
                pulseTimer -= PULSE_INTERVAL;
                radius = 64f + sine.Value * 16f;
            }

            // Pull effect on player
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 diff = Position - player.Position;
                float distance = diff.Length();
                
                if (distance > safeDistance)
                {
                    Vector2 pull = diff * (pullStrength / Math.Max(distance, 1f));
                    player.Speed += pull * Engine.DeltaTime;
                }
            }

            // Update visual effects and displacement
            if (burst != null)
            {
                burst.Position = Position;
                burst.Percent = 0.5f + sine.Value * 0.2f;
            }
            light.Alpha = 0.5f + sine.Value * 0.3f;
        }

        public override void Render()
        {
            base.Render();
            
            // Render vortex effect
            float alpha = 0.7f + sine.Value * 0.3f;
            Draw.Circle(Position, radius, Color.Purple * alpha, 32);
            Draw.Circle(Position, radius * 0.8f, Color.Purple * (alpha * 0.8f), 32);
            Draw.Circle(Position, radius * 0.6f, Color.Purple * (alpha * 0.6f), 32);
        }
    }
}



