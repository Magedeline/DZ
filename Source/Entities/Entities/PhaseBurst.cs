namespace Celeste.Entities
{
    public class PhaseBurst : Entity
    {
        private float radius;
        private float targetRadius;
        private float expandSpeed;
        private bool enhanced;
        private VertexLight light;
        
        public PhaseBurst(Vector2 position, float targetRadius, bool enhanced) : base(position)
        {
            this.targetRadius = targetRadius;
            this.enhanced = enhanced;
            radius = 0f;
            expandSpeed = enhanced ? 200f : 150f;
            
            // Add light effect
            Add(light = new VertexLight(Color.Cyan, 0.8f, 32, 64));
        }
        
        public override void Update()
        {
            base.Update();
            
            // Expand radius
            radius = Calc.Approach(radius, targetRadius, expandSpeed * Engine.DeltaTime);
            
            // Check for player collision
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                float dist = Vector2.Distance(Position, player.Center);
                if (Math.Abs(dist - radius) < 8f)
                {
                    player.Die(player.Position);
                }
            }
            
            // Remove when fully expanded
            if (radius >= targetRadius)
            {
                RemoveSelf();
            }
        }
        
        public override void Render()
        {
            base.Render();
            
            // Draw expanding ring
            Color ringColor = enhanced ? Color.Magenta : Color.Cyan;
            Draw.Circle(Position, radius, ringColor * 0.5f, 32);
            Draw.Circle(Position, radius - 1f, ringColor, 32);
            
            // Draw particles along the ring
            if (Scene is Level level)
            {
                float step = MathHelper.TwoPi / 16;
                for (float angle = 0; angle < MathHelper.TwoPi; angle += step)
                {
                    if (Calc.Random.NextFloat() > 0.7f)
                    {
                        Vector2 particlePos = Position + Calc.AngleToVector(angle, radius);
                        level.ParticlesFG.Emit(enhanced ? P_PhaseBurstEnhanced : P_PhaseBurst, particlePos);
                    }
                }
            }
        }
        
        // Custom particle types
        public static ParticleType P_PhaseBurst = new ParticleType
        {
            Source = GFX.Game["particles/pixel"],
            Color = Color.Cyan,
            Color2 = Color.White * 0f,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.4f,
            LifeMax = 0.6f,
            Size = 1f,
            DirectionRange = MathHelper.TwoPi,
            SpeedMin = 20f,
            SpeedMax = 40f,
            SpeedMultiplier = 0.5f
        };
        
        public static ParticleType P_PhaseBurstEnhanced = new ParticleType
        {
            Source = GFX.Game["particles/pixel"],
            Color = Color.Magenta,
            Color2 = Color.White * 0f,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.6f,
            LifeMax = 0.8f,
            Size = 1f,
            DirectionRange = MathHelper.TwoPi,
            SpeedMin = 30f,
            SpeedMax = 50f,
            SpeedMultiplier = 0.5f
        };
    }
}



