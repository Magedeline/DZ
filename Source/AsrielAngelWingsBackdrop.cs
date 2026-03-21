namespace MaggyHelper.Entities
{
    /// <summary>
    /// Divine wings backdrop for Asriel Angel of Death boss fight.
    /// Features ethereal, moving wings that expand and pulse with divine light.
    /// Positioned behind the boss's actual wings for a layered effect.
    /// </summary>
    [HotReloadable]
    public class AsrielAngelWingsBackdrop : Backdrop
    {
        public float Alpha = 1f;
        public float WingSpan = 0f;
        public const float MAX_WING_SPAN = 300f;
        
        private const int WING_COUNT = 6;
        private const int FEATHER_COUNT = 120;
        private const int LIGHT_RAY_COUNT = 12;
        
        private WingLayer[] wings = new WingLayer[WING_COUNT];
        private Feather[] feathers = new Feather[FEATHER_COUNT];
        private LightRay[] lightRays = new LightRay[LIGHT_RAY_COUNT];
        
        private float time;
        private float pulseTime;
        private float divineGlowTime;
        private Vector2 center;
        
        private MTexture featherTexture;
        private VertexPositionColor[] wingVerts;
        private VertexPositionColor[] lightVerts;
        
        private struct WingLayer
        {
            public float Angle;
            public float BaseAngle;
            public float Radius;
            public float Size;
            public float FlutterSpeed;
            public float FlutterPhase;
            public Color TintColor;
        }
        
        private struct Feather
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public float Alpha;
            public float Size;
            public Color Color;
            public float LifeTime;
        }
        
        private struct LightRay
        {
            public float Angle;
            public float Length;
            public float Width;
            public float Intensity;
            public float PulsePhase;
            public Color Color;
        }
        
        public AsrielAngelWingsBackdrop()
        {
            UseSpritebatch = false;
            center = new Vector2(160f, 90f);
            
            // Initialize wing layers with ethereal colors
            Color[] divineColors = new Color[]
            {
                new Color(255, 255, 255, 200), // Pure white
                new Color(255, 245, 200, 180), // Golden white
                new Color(200, 220, 255, 160), // Celestial blue
                new Color(255, 200, 255, 140), // Divine pink
                new Color(220, 255, 220, 120), // Angelic green
                new Color(255, 220, 180, 100)  // Soft gold
            };
            
            for (int i = 0; i < WING_COUNT; i++)
            {
                wings[i].BaseAngle = (i / (float)WING_COUNT) * MathHelper.TwoPi;
                wings[i].Angle = wings[i].BaseAngle;
                wings[i].Radius = 0f;
                wings[i].Size = 80f + i * 15f;
                wings[i].FlutterSpeed = 1.5f - i * 0.2f;
                wings[i].FlutterPhase = Calc.Random.NextFloat() * MathHelper.TwoPi;
                wings[i].TintColor = divineColors[i];
            }
            
            // Initialize feathers
            featherTexture = GFX.Game.Has("particles/feather") 
                ? GFX.Game["particles/feather"] 
                : GFX.Game["particles/dust"];
            
            for (int i = 0; i < FEATHER_COUNT; i++)
            {
                ResetFeather(ref feathers[i]);
            }
            
            // Initialize light rays
            for (int i = 0; i < LIGHT_RAY_COUNT; i++)
            {
                lightRays[i].Angle = (i / (float)LIGHT_RAY_COUNT) * MathHelper.TwoPi;
                lightRays[i].Length = Calc.Random.Range(100f, 200f);
                lightRays[i].Width = Calc.Random.Range(8f, 20f);
                lightRays[i].Intensity = Calc.Random.Range(0.3f, 0.8f);
                lightRays[i].PulsePhase = Calc.Random.NextFloat() * MathHelper.TwoPi;
                lightRays[i].Color = new Color(255, 240, 200);
            }
            
            // Initialize vertex arrays
            wingVerts = new VertexPositionColor[WING_COUNT * 6 * 3]; // Each wing has 3 triangular segments
            lightVerts = new VertexPositionColor[LIGHT_RAY_COUNT * 6];
        }
        
        private void ResetFeather(ref Feather feather)
        {
            float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
            float distance = Calc.Random.Range(50f, 150f);
            
            feather.Position = center + Calc.AngleToVector(angle, distance);
            
            Vector2 perpendicular = Calc.AngleToVector(angle + MathHelper.PiOver2, 1f);
            feather.Velocity = perpendicular * Calc.Random.Range(10f, 30f) + 
                              Vector2.UnitY * Calc.Random.Range(-20f, 20f);
            
            feather.Rotation = Calc.Random.NextFloat() * MathHelper.TwoPi;
            feather.RotationSpeed = Calc.Random.Range(-2f, 2f);
            feather.Alpha = 0f;
            feather.Size = Calc.Random.Range(0.5f, 1.5f);
            feather.Color = Calc.Random.Choose(
                new Color(255, 255, 255),
                new Color(255, 245, 200),
                new Color(240, 250, 255)
            );
            feather.LifeTime = 0f;
        }
        
        public override void Update(Scene scene)
        {
            base.Update(scene);
            
            if (!Visible || Alpha <= 0f)
                return;
            
            time += Engine.DeltaTime;
            pulseTime += Engine.DeltaTime * 2f;
            divineGlowTime += Engine.DeltaTime * 0.8f;
            
            // Update wing positions with graceful movement
            for (int i = 0; i < WING_COUNT; i++)
            {
                wings[i].FlutterPhase += Engine.DeltaTime * wings[i].FlutterSpeed;
                
                float flutter = (float)Math.Sin(wings[i].FlutterPhase) * 0.15f;
                wings[i].Angle = wings[i].BaseAngle + flutter;
                
                // Smoothly expand wings to current wingspan
                wings[i].Radius = MathHelper.Lerp(wings[i].Radius, WingSpan * (0.5f + i * 0.08f), 
                    Engine.DeltaTime * 3f);
            }
            
            // Update feathers
            for (int i = 0; i < FEATHER_COUNT; i++)
            {
                feathers[i].LifeTime += Engine.DeltaTime;
                feathers[i].Position += feathers[i].Velocity * Engine.DeltaTime;
                feathers[i].Rotation += feathers[i].RotationSpeed * Engine.DeltaTime;
                
                // Fade in and out
                if (feathers[i].LifeTime < 1f)
                {
                    feathers[i].Alpha = Ease.SineIn(feathers[i].LifeTime);
                }
                else if (feathers[i].LifeTime > 4f)
                {
                    feathers[i].Alpha = Ease.SineOut(1f - Calc.ClampedMap(feathers[i].LifeTime, 4f, 6f));
                }
                else
                {
                    feathers[i].Alpha = 1f;
                }
                
                // Gentle floating motion
                feathers[i].Velocity += new Vector2(
                    (float)Math.Sin(time * 2f + i * 0.1f) * 5f,
                    (float)Math.Cos(time * 1.5f + i * 0.15f) * 3f
                ) * Engine.DeltaTime;
                
                feathers[i].Velocity *= 0.98f;
                
                // Reset if too far or too old
                if (feathers[i].LifeTime > 6f || 
                    Vector2.Distance(feathers[i].Position, center) > 250f)
                {
                    ResetFeather(ref feathers[i]);
                }
            }
            
            // Update light rays
            for (int i = 0; i < LIGHT_RAY_COUNT; i++)
            {
                lightRays[i].Angle += Engine.DeltaTime * 0.3f;
                lightRays[i].PulsePhase += Engine.DeltaTime * 1.5f;
                
                float pulse = (float)Math.Sin(lightRays[i].PulsePhase) * 0.3f + 0.7f;
                lightRays[i].Intensity = Calc.Random.Range(0.3f, 0.8f) * pulse;
            }
        }
        
        public override void Render(Scene scene)
        {
            if (WingSpan <= 0f || Alpha <= 0f)
                return;
            
            Level level = scene as Level;
            Vector2 cameraPos = level.Camera.Position;
            
            // Adjust center based on camera for parallax effect
            Vector2 renderCenter = center - cameraPos * 0.1f;
            
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            
            // Render divine glow
            float glowPulse = (float)Math.Sin(divineGlowTime) * 0.3f + 0.7f;
            Color glowColor = new Color(255, 245, 220, (int)(120 * Alpha * glowPulse));
            
            for (int i = 0; i < 3; i++)
            {
                float glowSize = (WingSpan * 0.8f + i * 30f) * glowPulse;
                Draw.Circle(renderCenter, glowSize, glowColor * (0.3f - i * 0.08f), 32);
            }
            
            Draw.SpriteBatch.End();
            
            // Render light rays using vertices
            int lightVertIndex = 0;
            for (int i = 0; i < LIGHT_RAY_COUNT; i++)
            {
                Vector2 direction = Calc.AngleToVector(lightRays[i].Angle, 1f);
                Vector2 perpendicular = direction.Perpendicular();
                
                Vector2 start = direction * 20f;
                Vector2 end = direction * (lightRays[i].Length + WingSpan * 0.3f);
                
                float startWidth = lightRays[i].Width * 0.3f;
                float endWidth = lightRays[i].Width;
                
                Color startColor = lightRays[i].Color * lightRays[i].Intensity * Alpha;
                Color endColor = lightRays[i].Color * 0f;
                
                Vector2 v1 = start - perpendicular * startWidth;
                Vector2 v2 = start + perpendicular * startWidth;
                Vector2 v3 = end + perpendicular * endWidth;
                Vector2 v4 = end - perpendicular * endWidth;
                
                lightVerts[lightVertIndex].Position = new Vector3(v1, 0f);
                lightVerts[lightVertIndex].Color = startColor;
                lightVerts[lightVertIndex + 1].Position = new Vector3(v2, 0f);
                lightVerts[lightVertIndex + 1].Color = startColor;
                lightVerts[lightVertIndex + 2].Position = new Vector3(v3, 0f);
                lightVerts[lightVertIndex + 2].Color = endColor;
                lightVerts[lightVertIndex + 3].Position = new Vector3(v1, 0f);
                lightVerts[lightVertIndex + 3].Color = startColor;
                lightVerts[lightVertIndex + 4].Position = new Vector3(v3, 0f);
                lightVerts[lightVertIndex + 4].Color = endColor;
                lightVerts[lightVertIndex + 5].Position = new Vector3(v4, 0f);
                lightVerts[lightVertIndex + 5].Color = endColor;
                
                lightVertIndex += 6;
            }
            
            Matrix transform = Matrix.CreateTranslation(renderCenter.X, renderCenter.Y, 0f);
            GFX.DrawVertices(transform, lightVerts, lightVerts.Length);
            
            // Render wings using vertices
            int wingVertIndex = 0;
            for (int i = 0; i < WING_COUNT; i++)
            {
                Vector2 direction = Calc.AngleToVector(wings[i].Angle, 1f);
                Vector2 perpendicular = direction.Perpendicular();
                
                float radius = wings[i].Radius;
                float size = wings[i].Size;
                
                // Wing consists of 3 triangular segments for a feathered look
                for (int segment = 0; segment < 3; segment++)
                {
                    float segmentStart = segment / 3f;
                    float segmentEnd = (segment + 1) / 3f;
                    
                    Vector2 innerPos = direction * (radius * segmentStart);
                    Vector2 outerPos = direction * (radius * segmentEnd);
                    
                    float innerWidth = size * (1f - segmentStart * 0.5f);
                    float outerWidth = size * (1f - segmentEnd * 0.5f);
                    
                    Color innerColor = wings[i].TintColor * Alpha * (0.8f + (float)Math.Sin(pulseTime + i) * 0.2f);
                    Color outerColor = wings[i].TintColor * Alpha * 0.3f;
                    
                    Vector2 v1 = innerPos - perpendicular * innerWidth;
                    Vector2 v2 = innerPos + perpendicular * innerWidth;
                    Vector2 v3 = outerPos + perpendicular * outerWidth;
                    Vector2 v4 = outerPos - perpendicular * outerWidth;
                    
                    wingVerts[wingVertIndex].Position = new Vector3(v1, 0f);
                    wingVerts[wingVertIndex].Color = innerColor;
                    wingVerts[wingVertIndex + 1].Position = new Vector3(v2, 0f);
                    wingVerts[wingVertIndex + 1].Color = innerColor;
                    wingVerts[wingVertIndex + 2].Position = new Vector3(v3, 0f);
                    wingVerts[wingVertIndex + 2].Color = outerColor;
                    wingVerts[wingVertIndex + 3].Position = new Vector3(v1, 0f);
                    wingVerts[wingVertIndex + 3].Color = innerColor;
                    wingVerts[wingVertIndex + 4].Position = new Vector3(v3, 0f);
                    wingVerts[wingVertIndex + 4].Color = outerColor;
                    wingVerts[wingVertIndex + 5].Position = new Vector3(v4, 0f);
                    wingVerts[wingVertIndex + 5].Color = outerColor;
                    
                    wingVertIndex += 6;
                }
            }
            
            GFX.DrawVertices(transform, wingVerts, wingVerts.Length);
            
            // Render feathers
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            
            for (int i = 0; i < FEATHER_COUNT; i++)
            {
                if (feathers[i].Alpha <= 0f)
                    continue;
                
                Color featherColor = feathers[i].Color * feathers[i].Alpha * Alpha;
                Vector2 featherPos = feathers[i].Position - cameraPos * 0.15f;
                
                if (featherTexture != null)
                {
                    featherTexture.DrawCentered(featherPos, featherColor, 
                        feathers[i].Size, feathers[i].Rotation);
                }
                else
                {
                    float size = 4f * feathers[i].Size;
                    Draw.Rect(featherPos - new Vector2(size / 2f), size, size, featherColor);
                }
            }
            
            Draw.SpriteBatch.End();
        }
        
        /// <summary>
        /// Sets the wing span for the backdrop to match the boss's transformation
        /// </summary>
        public void SetWingSpan(float span)
        {
            WingSpan = MathHelper.Clamp(span, 0f, MAX_WING_SPAN);
        }
        
        /// <summary>
        /// Animates the wings expanding over time
        /// </summary>
        public void ExpandWings(float targetSpan, float speed = 1f)
        {
            WingSpan = MathHelper.Lerp(WingSpan, 
                MathHelper.Clamp(targetSpan, 0f, MAX_WING_SPAN), 
                Engine.DeltaTime * speed);
        }
    }
}




