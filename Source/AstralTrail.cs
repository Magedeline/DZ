namespace MaggyHelper.Entities
{
    [HotReloadable]
    public class AstralTrail : Entity
    {
        private const float Duration = 0.8f;
        private Vector2 start;
        private Vector2 end;
        private float percent;
        private VertexLight light;
        
        public AstralTrail(Vector2 start, Vector2 end) : base(start)
        {
            this.start = start;
            this.end = end;
            percent = 0f;
            
            // Add light effect
            Add(light = new VertexLight(Color.Purple, 0.5f, 16, 32));
            
            // Add trail effect coroutine
            Add(new Coroutine(TrailRoutine()));
        }
        
        private IEnumerator TrailRoutine()
        {
            float timer = 0f;
            
            while (timer < Duration)
            {
                timer += Engine.DeltaTime;
                percent = timer / Duration;
                Position = Vector2.Lerp(start, end, percent);
                
                // Emit particles along the trail
                if (Scene is Level level)
                {
                    Vector2 particlePos = Position + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f));
                    level.ParticlesFG.Emit(P_AstralTrail, particlePos);
                }
                
                yield return null;
            }
            
            RemoveSelf();
        }
        
        public override void Render()
        {
            base.Render();
            
            // Draw trail line
            Color trailColor = Color.Lerp(Color.Purple, Color.White, 0.5f) * (1f - percent);
            Draw.Line(start, end, trailColor * 0.5f, 4f);
        }
        
        // Custom particle type for trail
        public static ParticleType P_AstralTrail = new ParticleType
        {
            Source = GFX.Game["particles/pixel"],
            Color = Color.Purple,
            Color2 = Color.White * 0f,
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.3f,
            LifeMax = 0.5f,
            Size = 1f,
            DirectionRange = MathHelper.TwoPi,
            SpeedMin = 10f,
            SpeedMax = 20f,
            SpeedMultiplier = 0.5f
        };
    }
}



