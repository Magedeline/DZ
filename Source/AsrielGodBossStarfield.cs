namespace MaggyHelper.Entities
{
    /// <summary>
    /// Starfield backdrop for Asriel God of Hyperdeath boss fight.
    /// Features rainbow stars and cosmic effects.
    /// </summary>
    [HotReloadable]
    public class AsrielGodBossStarfield : Backdrop
    {
        public float Alpha = 1f;
        
        private const int PARTICLE_COUNT = 250;
        private const int COLOR_STEPS = 40;
        
        private Particle[] particles = new Particle[PARTICLE_COUNT];
        private VertexPositionColor[] verts = new VertexPositionColor[PARTICLE_COUNT * 6 + 6];
        
        // Rainbow color palette for Asriel's divine power
        private static readonly Color[] rainbowColors = new Color[]
        {
            Calc.HexToColor("FF0080"), // Hot Pink
            Calc.HexToColor("FF00FF"), // Magenta
            Calc.HexToColor("8000FF"), // Purple
            Calc.HexToColor("0080FF"), // Light Blue
            Calc.HexToColor("00FFFF"), // Cyan
            Calc.HexToColor("00FF80"), // Spring Green
            Calc.HexToColor("80FF00"), // Chartreuse
            Calc.HexToColor("FFFF00"), // Yellow
            Calc.HexToColor("FF8000"), // Orange
            Calc.HexToColor("FF0000")  // Red
        };
        
        private float rainbowTime;
        private float pulseTime;
        private MTexture bgTexture;
        
        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Direction;
            public float Speed;
            public Color Color;
            public float DirectionApproach;
            public float Size;
            public float RainbowOffset;
            public float PulsePhase;
        }
        
        public AsrielGodBossStarfield()
        {
            UseSpritebatch = false;
            
            // Try to load the custom background texture
            bgTexture = GFX.Game.Has("bgs/maggy/20/asriel/bg00") 
                ? GFX.Game["bgs/maggy/20/asriel/bg00"] 
                : GFX.Game["objects/temple/portal/portal"];
            
            // Initialize rainbow star particles
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                particles[i].Speed = Calc.Random.Range(400f, 1000f);
                particles[i].Direction = new Vector2(-1f, 0.0f);
                particles[i].DirectionApproach = Calc.Random.Range(0.2f, 3f);
                particles[i].Position.X = Calc.Random.Range(0, 384);
                particles[i].Position.Y = Calc.Random.Range(0, 244);
                particles[i].Color = Calc.Random.Choose(rainbowColors);
                particles[i].Size = Calc.Random.Range(1f, 3f);
                particles[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                particles[i].PulsePhase = Calc.Random.NextFloat() * (float)Math.PI * 2f;
            }
        }
        
        private Color GetRainbowColor(float offset, float intensity = 1f)
        {
            float phase = (rainbowTime * 2f + offset) % ((float)Math.PI * 2f);
            int index1 = (int)Math.Floor(phase / ((float)Math.PI * 2f) * rainbowColors.Length);
            int index2 = (index1 + 1) % rainbowColors.Length;
            float lerp = (phase / ((float)Math.PI * 2f) * rainbowColors.Length) % 1f;
            
            Color color = Color.Lerp(rainbowColors[index1], rainbowColors[index2], lerp);
            
            // Add pulsing effect
            float pulse = (float)Math.Sin(pulseTime * 3f + offset) * 0.3f + 0.7f;
            
            return color * intensity * pulse;
        }
        
        public override void Update(Scene scene)
        {
            base.Update(scene);
            
            if (!Visible || Alpha <= 0.0)
                return;
            
            rainbowTime += Engine.DeltaTime;
            pulseTime += Engine.DeltaTime;
            
            Vector2 vector = new Vector2(-1f, 0.0f);
            Level level = scene as Level;
            
            if (level.Bounds.Height > level.Bounds.Width)
                vector = new Vector2(0.0f, -1f);
            
            float target = vector.Angle();
            
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                particles[i].Position += particles[i].Direction * particles[i].Speed * Engine.DeltaTime;
                
                float angleRadians = Calc.AngleApproach(particles[i].Direction.Angle(), target, 
                    particles[i].DirectionApproach * Engine.DeltaTime);
                particles[i].Direction = Calc.AngleToVector(angleRadians, 1f);
                
                // Update rainbow color
                particles[i].Color = GetRainbowColor(particles[i].RainbowOffset + i * 0.1f);
            }
        }
        
        public override void Render(Scene scene)
        {
            Vector2 position = (scene as Level).Camera.Position;
            
            // Background gradient with rainbow tint
            Color bgColor1 = GetRainbowColor(0f, 0.2f) * Alpha;
            Color bgColor2 = GetRainbowColor(1f, 0.2f) * Alpha;
            Color bgColor3 = GetRainbowColor(2f, 0.2f) * Alpha;
            Color bgColor4 = GetRainbowColor(3f, 0.2f) * Alpha;
            
            verts[0].Color = bgColor1;
            verts[0].Position = new Vector3(-10f, -10f, 0.0f);
            verts[1].Color = bgColor2;
            verts[1].Position = new Vector3(330f, -10f, 0.0f);
            verts[2].Color = bgColor3;
            verts[2].Position = new Vector3(330f, 190f, 0.0f);
            verts[3].Color = bgColor1;
            verts[3].Position = new Vector3(-10f, -10f, 0.0f);
            verts[4].Color = bgColor3;
            verts[4].Position = new Vector3(330f, 190f, 0.0f);
            verts[5].Color = bgColor4;
            verts[5].Position = new Vector3(-10f, 190f, 0.0f);
            
            // Render rainbow stars
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                int vertIndex = (i + 1) * 6;
                
                float length = Calc.ClampedMap(particles[i].Speed, 0.0f, 1000f, 1f, 48f) * particles[i].Size;
                float width = Calc.ClampedMap(particles[i].Speed, 0.0f, 1000f, 2f, 0.4f);
                
                Vector2 direction = particles[i].Direction;
                Vector2 perpendicular = direction.Perpendicular();
                Vector2 pos = particles[i].Position;
                
                pos.X = mod(pos.X - position.X * 0.9f, 384f) - 32f;
                pos.Y = mod(pos.Y - position.Y * 0.9f, 244f) - 32f;
                
                // Add pulse to size
                float pulse = (float)Math.Sin(pulseTime * 4f + particles[i].PulsePhase) * 0.3f + 1f;
                length *= pulse;
                width *= pulse;
                
                Vector2 v1 = pos - direction * length * 0.5f - perpendicular * width;
                Vector2 v2 = pos + direction * length * 1f - perpendicular * width;
                Vector2 v3 = pos + direction * length * 0.5f + perpendicular * width;
                Vector2 v4 = pos - direction * length * 1f + perpendicular * width;
                
                Color color = particles[i].Color * Alpha;
                
                verts[vertIndex].Color = color;
                verts[vertIndex].Position = new Vector3(v1, 0.0f);
                verts[vertIndex + 1].Color = color;
                verts[vertIndex + 1].Position = new Vector3(v2, 0.0f);
                verts[vertIndex + 2].Color = color;
                verts[vertIndex + 2].Position = new Vector3(v3, 0.0f);
                verts[vertIndex + 3].Color = color;
                verts[vertIndex + 3].Position = new Vector3(v1, 0.0f);
                verts[vertIndex + 4].Color = color;
                verts[vertIndex + 4].Position = new Vector3(v3, 0.0f);
                verts[vertIndex + 5].Color = color;
                verts[vertIndex + 5].Position = new Vector3(v4, 0.0f);
            }
            
            GFX.DrawVertices(Matrix.Identity, verts, verts.Length);
        }
        
        private float mod(float x, float m) => (x % m + m) % m;
    }
}




