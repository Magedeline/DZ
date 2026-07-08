#nullable enable

namespace Celeste.Effects
{
    /// <summary>
    /// Heaven Gates backdrop with Void Astral Birth hovering above.
    /// Features majestic golden gates leading to heaven with a cosmic void 
    /// astral birth effect floating above them, symbolizing the ascension
    /// to the afterlife. Designed for level "end-saved".
    /// </summary>
    [CustomBackdrop("DZ/HeavenGatesBackdrop")]
    [HotReloadable]
    public class HeavenGatesBackdrop : Backdrop
    {
        #region Structs
        private struct GateColumn
        {
            public Vector2 BasePosition;
            public float Height;
            public float Width;
            public float GlowIntensity;
            public float GlowPhase;
            public Color BaseColor;
            public Color GlowColor;
        }

        private struct AstralParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float Alpha;
            public float RainbowPhase;
            public float RotationSpeed;
            public float Rotation;
            public float Lifetime;
            public bool IsVoid;
        }

        private struct VoidRing
        {
            public float Radius;
            public float Alpha;
            public float RotationSpeed;
            public float Rotation;
            public Color InnerColor;
            public Color OuterColor;
            public float PulsePhase;
        }

        private struct LightRay
        {
            public float Angle;
            public float Length;
            public float Width;
            public float Alpha;
            public float PulsePhase;
            public Color Color;
        }

        private struct Star
        {
            public Vector2 Position;
            public float Size;
            public float Brightness;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public Color BaseColor;
        }
        #endregion

        #region Constants
        private const int GATE_COLUMN_COUNT = 4;
        private const int ASTRAL_PARTICLE_COUNT = 120;
        private const int VOID_RING_COUNT = 5;
        private const int LIGHT_RAY_COUNT = 24;
        private const int STAR_COUNT = 100;
        private const int RAINBOW_LUT_SIZE = 256;
        #endregion

        #region Fields
        private readonly GateColumn[] gateColumns;
        private readonly AstralParticle[] astralParticles;
        private readonly VoidRing[] voidRings;
        private readonly LightRay[] lightRays;
        private readonly Star[] stars;
        private readonly Color[] rainbowLUT;

        private VirtualRenderTarget? renderTarget;
        private float globalTime;
        private float gateGlowTime;
        private float voidPulseTime;
        private Vector2 center;
        private Vector2 voidCenter; // Position of the void astral birth above gates
        private Vector2 cameraOffset;

        // Configuration
        public float Intensity = 1f;
        public new float Speed = 1f;
        public float GateHeight = 100f;
        public float GateWidth = 40f;
        public float VoidRadius = 35f;
        public float GlowIntensity = 1f;
        public float AstralBirthScale = 1f;
        public Color GateGoldColor = new(255, 215, 100);
        public Color GateWhiteColor = new(255, 250, 240);
        public Color VoidCoreColor = new(20, 5, 40);
        public Color VoidEdgeColor = new(100, 50, 180);
        public Color BackgroundColor = new(10, 5, 25); // Deep heaven blue-black
        #endregion

        #region Constructor
        public HeavenGatesBackdrop()
        {
            center = new Vector2(160f, 90f);
            voidCenter = new Vector2(160f, 35f); // Above the gates

            // Initialize rainbow LUT
            rainbowLUT = new Color[RAINBOW_LUT_SIZE];
            InitializeRainbowLUT();

            // Initialize gate columns
            gateColumns = new GateColumn[GATE_COLUMN_COUNT];
            InitializeGateColumns();

            // Initialize astral particles
            astralParticles = new AstralParticle[ASTRAL_PARTICLE_COUNT];
            InitializeAstralParticles();

            // Initialize void rings
            voidRings = new VoidRing[VOID_RING_COUNT];
            InitializeVoidRings();

            // Initialize light rays
            lightRays = new LightRay[LIGHT_RAY_COUNT];
            InitializeLightRays();

            // Initialize stars
            stars = new Star[STAR_COUNT];
            InitializeStars();
        }

        public HeavenGatesBackdrop(BinaryPacker.Element data) : this()
        {
            if (data.HasAttr("intensity"))
                Intensity = data.AttrFloat("intensity", 1f);
            
            if (data.HasAttr("speed"))
                Speed = data.AttrFloat("speed", 1f);
            
            if (data.HasAttr("gateHeight"))
                GateHeight = data.AttrFloat("gateHeight", 100f);
            
            if (data.HasAttr("gateWidth"))
                GateWidth = data.AttrFloat("gateWidth", 40f);
            
            if (data.HasAttr("voidRadius"))
                VoidRadius = data.AttrFloat("voidRadius", 35f);
            
            if (data.HasAttr("glowIntensity"))
                GlowIntensity = data.AttrFloat("glowIntensity", 1f);
            
            if (data.HasAttr("astralBirthScale"))
                AstralBirthScale = data.AttrFloat("astralBirthScale", 1f);
        }
        #endregion

        #region Initialization
        private void InitializeRainbowLUT()
        {
            for (int i = 0; i < RAINBOW_LUT_SIZE; i++)
            {
                // Deep space/void rainbow palette - purples, violets, indigo, cyan
                float hue = (float)i / RAINBOW_LUT_SIZE;
                // Shift hue to favor deep space colors (240-320 range)
                float adjustedHue = 0.65f + hue * 0.25f; // Range: purple to magenta
                if (adjustedHue > 1f) adjustedHue -= 1f;
                rainbowLUT[i] = HSVToRGB(adjustedHue, 0.8f, 0.9f);
            }
        }

        private void InitializeGateColumns()
        {
            float gateSpacing = 50f;
            float baseX = center.X - gateSpacing * 1.5f;

            for (int i = 0; i < GATE_COLUMN_COUNT; i++)
            {
                gateColumns[i] = new GateColumn
                {
                    BasePosition = new Vector2(baseX + i * gateSpacing, 180f),
                    Height = GateHeight + Calc.Random.Range(-10f, 10f),
                    Width = GateWidth * (i % 2 == 0 ? 0.8f : 1.0f), // Alternate sizes
                    GlowIntensity = 0.8f + Calc.Random.NextFloat() * 0.4f,
                    GlowPhase = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    BaseColor = Color.Lerp(GateGoldColor, GateWhiteColor, Calc.Random.NextFloat() * 0.3f),
                    GlowColor = new Color(255, 240, 200, 150)
                };
            }
        }

        private void InitializeAstralParticles()
        {
            for (int i = 0; i < ASTRAL_PARTICLE_COUNT; i++)
            {
                ResetAstralParticle(ref astralParticles[i], true);
            }
        }

        private void ResetAstralParticle(ref AstralParticle particle, bool randomLifetime = false)
        {
            float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
            float distance = Calc.Random.Range(5f, VoidRadius * 1.5f * AstralBirthScale);
            bool isVoid = Calc.Random.NextFloat() > 0.4f;

            if (isVoid)
            {
                // Void particles orbit around the astral birth
                particle.Position = voidCenter + Calc.AngleToVector(angle, distance);
                particle.Velocity = Calc.AngleToVector(angle + MathHelper.PiOver2, Calc.Random.Range(5f, 25f));
            }
            else
            {
                // Light particles rise from gates toward the void
                float xPos = center.X + Calc.Random.Range(-80f, 80f);
                particle.Position = new Vector2(xPos, 180f);
                particle.Velocity = new Vector2(Calc.Random.Range(-5f, 5f), Calc.Random.Range(-30f, -60f));
            }

            particle.Size = Calc.Random.Range(1f, 4f);
            particle.Alpha = Calc.Random.Range(0.3f, 0.9f);
            particle.RainbowPhase = Calc.Random.NextFloat();
            particle.RotationSpeed = Calc.Random.Range(-2f, 2f);
            particle.Rotation = Calc.Random.NextFloat() * MathHelper.TwoPi;
            particle.Lifetime = randomLifetime ? Calc.Random.Range(0f, 5f) : 0f;
            particle.IsVoid = isVoid;
        }

        private void InitializeVoidRings()
        {
            for (int i = 0; i < VOID_RING_COUNT; i++)
            {
                float t = (float)i / VOID_RING_COUNT;
                voidRings[i] = new VoidRing
                {
                    Radius = VoidRadius * (0.3f + t * 0.7f) * AstralBirthScale,
                    Alpha = 0.7f - t * 0.4f,
                    RotationSpeed = (0.5f + t * 0.5f) * (i % 2 == 0 ? 1f : -1f),
                    Rotation = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    InnerColor = Color.Lerp(VoidCoreColor, VoidEdgeColor, t * 0.5f),
                    OuterColor = Color.Lerp(VoidEdgeColor, new Color(180, 100, 255), t),
                    PulsePhase = Calc.Random.NextFloat() * MathHelper.TwoPi
                };
            }
        }

        private void InitializeLightRays()
        {
            for (int i = 0; i < LIGHT_RAY_COUNT; i++)
            {
                float angle = (float)i / LIGHT_RAY_COUNT * MathHelper.TwoPi;
                // Half rays from gates, half from void
                bool fromGates = i < LIGHT_RAY_COUNT / 2;
                
                lightRays[i] = new LightRay
                {
                    Angle = fromGates ? -MathHelper.PiOver2 + Calc.Random.Range(-0.3f, 0.3f) : angle,
                    Length = Calc.Random.Range(60f, 150f),
                    Width = Calc.Random.Range(2f, 8f),
                    Alpha = Calc.Random.Range(0.1f, 0.4f),
                    PulsePhase = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    Color = fromGates ? GateGoldColor : Color.Lerp(VoidEdgeColor, new Color(200, 150, 255), 0.5f)
                };
            }
        }

        private void InitializeStars()
        {
            for (int i = 0; i < STAR_COUNT; i++)
            {
                stars[i] = new Star
                {
                    Position = new Vector2(Calc.Random.Range(0f, 320f), Calc.Random.Range(0f, 90f)),
                    Size = Calc.Random.Range(0.5f, 2f),
                    Brightness = Calc.Random.Range(0.3f, 1f),
                    TwinklePhase = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    TwinkleSpeed = Calc.Random.Range(1f, 4f),
                    BaseColor = Calc.Random.Chance(0.3f) 
                        ? new Color(200, 180, 255) 
                        : new Color(255, 250, 240)
                };
            }
        }
        #endregion

        #region Update
        public override void Update(Scene scene)
        {
            base.Update(scene);

            float dt = Engine.DeltaTime * Speed;
            globalTime += dt;
            gateGlowTime += dt * 0.5f;
            voidPulseTime += dt * 0.8f;

            // Update camera offset
            if (scene is Level level)
            {
                cameraOffset = level.Camera.Position;
            }

            // Update astral particles
            UpdateAstralParticles(dt);

            // Update void rings
            UpdateVoidRings(dt);
        }

        private void UpdateAstralParticles(float dt)
        {
            for (int i = 0; i < ASTRAL_PARTICLE_COUNT; i++)
            {
                ref AstralParticle p = ref astralParticles[i];
                
                p.Position += p.Velocity * dt;
                p.Rotation += p.RotationSpeed * dt;
                p.Lifetime += dt;
                p.RainbowPhase += dt * 0.1f;
                if (p.RainbowPhase > 1f) p.RainbowPhase -= 1f;

                if (p.IsVoid)
                {
                    // Orbit around void center
                    Vector2 toCenter = voidCenter - p.Position;
                    float dist = toCenter.Length();
                    if (dist > 1f)
                    {
                        Vector2 perpendicular = new Vector2(-toCenter.Y, toCenter.X);
                        perpendicular.Normalize();
                        p.Velocity = perpendicular * (15f + (VoidRadius - dist) * 0.5f);
                        
                        // Slight pull toward center
                        p.Velocity += toCenter * 0.02f;
                    }

                    // Reset if too far from center
                    if (dist > VoidRadius * 2.5f * AstralBirthScale || p.Lifetime > 8f)
                    {
                        ResetAstralParticle(ref p);
                    }
                }
                else
                {
                    // Light particles rising from gates
                    // Fade out near the top
                    if (p.Position.Y < 20f || p.Lifetime > 4f)
                    {
                        ResetAstralParticle(ref p);
                    }
                }
            }
        }

        private void UpdateVoidRings(float dt)
        {
            for (int i = 0; i < VOID_RING_COUNT; i++)
            {
                voidRings[i].Rotation += voidRings[i].RotationSpeed * dt;
            }
        }
        #endregion

        #region Render
        public override void Render(Scene scene)
        {
            EnsureRenderTarget();
            if (renderTarget == null) return;

            var gd = Draw.SpriteBatch.GraphicsDevice;
            RenderTargetBinding[] prevTargets = gd.GetRenderTargets();

            // End the existing SpriteBatch started by BackdropRenderer
            Draw.SpriteBatch.End();

            // Render to our target
            gd.SetRenderTarget(renderTarget);
            gd.Clear(BackgroundColor);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);

            // Draw stars (background)
            DrawStars();

            // Draw void astral birth (above gates, in background)
            DrawVoidAstralBirth();

            // Draw light rays (behind gates)
            DrawLightRays();

            // Draw heaven gates
            DrawHeavenGates();

            // Draw astral particles (foreground)
            DrawAstralParticles();

            Draw.SpriteBatch.End();

            // Restore previous render targets
            gd.SetRenderTargets(prevTargets);

            // Draw our render target to screen
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Draw.SpriteBatch.Draw(renderTarget, Vector2.Zero, Color.White * Intensity);
            Draw.SpriteBatch.End();

            // Restart SpriteBatch for BackdropRenderer to continue
            GameplayRenderer.Begin();
        }

        private void DrawStars()
        {
            for (int i = 0; i < STAR_COUNT; i++)
            {
                ref Star s = ref stars[i];
                float twinkle = (float)Math.Sin(globalTime * s.TwinkleSpeed + s.TwinklePhase);
                float brightness = s.Brightness * (0.5f + twinkle * 0.5f);
                Color color = s.BaseColor * brightness;
                
                Draw.Rect(s.Position.X, s.Position.Y, s.Size, s.Size, color);
            }
        }

        private void DrawVoidAstralBirth()
        {
            // Draw void rings from outside in
            for (int i = VOID_RING_COUNT - 1; i >= 0; i--)
            {
                ref VoidRing ring = ref voidRings[i];
                float pulse = (float)Math.Sin(voidPulseTime * 2f + ring.PulsePhase) * 0.2f + 1f;
                float radius = ring.Radius * pulse;
                float alpha = ring.Alpha * Intensity;

                // Draw ring as series of circles with gradient
                int segments = 32;
                for (int j = 0; j < segments; j++)
                {
                    float angle1 = ring.Rotation + (float)j / segments * MathHelper.TwoPi;
                    float angle2 = ring.Rotation + (float)(j + 1) / segments * MathHelper.TwoPi;

                    Vector2 p1 = voidCenter + Calc.AngleToVector(angle1, radius);
                    Vector2 p2 = voidCenter + Calc.AngleToVector(angle2, radius);

                    // Color varies around the ring
                    float colorPhase = (float)j / segments + globalTime * 0.1f;
                    int lutIndex = (int)(colorPhase * RAINBOW_LUT_SIZE) % RAINBOW_LUT_SIZE;
                    Color ringColor = Color.Lerp(ring.OuterColor, rainbowLUT[lutIndex], 0.3f) * alpha;

                    Draw.Line(p1, p2, ringColor, 2f + i * 0.5f);
                }
            }

            // Draw central void core
            float coreAlpha = 0.8f + (float)Math.Sin(voidPulseTime * 3f) * 0.2f;
            DrawGlowCircle(voidCenter, VoidRadius * 0.3f * AstralBirthScale, VoidCoreColor * coreAlpha * Intensity, 8);
            DrawGlowCircle(voidCenter, VoidRadius * 0.15f * AstralBirthScale, Color.White * 0.5f * Intensity, 4);
        }

        private void DrawGlowCircle(Vector2 center, float radius, Color color, int rings)
        {
            for (int i = rings; i >= 0; i--)
            {
                float t = (float)i / rings;
                float r = radius * (1f - t * 0.5f);
                float a = t;
                
                int segments = 24;
                for (int j = 0; j < segments; j++)
                {
                    float angle1 = (float)j / segments * MathHelper.TwoPi;
                    float angle2 = (float)(j + 1) / segments * MathHelper.TwoPi;

                    Vector2 p1 = center + Calc.AngleToVector(angle1, r);
                    Vector2 p2 = center + Calc.AngleToVector(angle2, r);

                    Draw.Line(p1, p2, color * a, 2f);
                }
            }
        }

        private void DrawLightRays()
        {
            for (int i = 0; i < LIGHT_RAY_COUNT; i++)
            {
                ref LightRay ray = ref lightRays[i];
                float pulse = (float)Math.Sin(gateGlowTime * 2f + ray.PulsePhase) * 0.3f + 0.7f;
                float alpha = ray.Alpha * pulse * GlowIntensity * Intensity;

                Vector2 origin;
                if (i < LIGHT_RAY_COUNT / 2)
                {
                    // Rays from gate area
                    float xOffset = (i - LIGHT_RAY_COUNT / 4) * 15f;
                    origin = new Vector2(center.X + xOffset, 160f);
                }
                else
                {
                    // Rays from void
                    origin = voidCenter;
                }

                Vector2 direction = Calc.AngleToVector(ray.Angle, ray.Length);
                Vector2 end = origin + direction;

                // Draw ray with gradient
                DrawGradientLine(origin, end, ray.Color * alpha, ray.Color * 0f, ray.Width);
            }
        }

        private void DrawGradientLine(Vector2 start, Vector2 end, Color startColor, Color endColor, float width)
        {
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + 1) / segments;
                
                Vector2 p1 = Vector2.Lerp(start, end, t1);
                Vector2 p2 = Vector2.Lerp(start, end, t2);
                Color c = Color.Lerp(startColor, endColor, (t1 + t2) * 0.5f);
                
                Draw.Line(p1, p2, c, width * (1f - t1 * 0.5f));
            }
        }

        private void DrawHeavenGates()
        {
            for (int i = 0; i < GATE_COLUMN_COUNT; i++)
            {
                ref GateColumn col = ref gateColumns[i];
                float glowPulse = (float)Math.Sin(gateGlowTime + col.GlowPhase) * 0.3f + 0.7f;
                float glowAlpha = col.GlowIntensity * glowPulse * GlowIntensity * Intensity;

                // Draw glow behind column
                DrawGateGlow(col.BasePosition, col.Width * 1.5f, col.Height, col.GlowColor * glowAlpha);

                // Draw main column
                float columnTop = col.BasePosition.Y - col.Height;
                Color columnColor = col.BaseColor * Intensity;
                
                // Column body
                Draw.Rect(
                    col.BasePosition.X - col.Width / 2,
                    columnTop,
                    col.Width,
                    col.Height,
                    columnColor
                );

                // Column capital (ornate top)
                float capitalHeight = col.Width * 0.5f;
                float capitalWidth = col.Width * 1.3f;
                Draw.Rect(
                    col.BasePosition.X - capitalWidth / 2,
                    columnTop - capitalHeight,
                    capitalWidth,
                    capitalHeight,
                    Color.Lerp(columnColor, GateWhiteColor, 0.3f)
                );

                // Light edge highlights
                Draw.Line(
                    new Vector2(col.BasePosition.X - col.Width / 2, columnTop),
                    new Vector2(col.BasePosition.X - col.Width / 2, col.BasePosition.Y),
                    GateWhiteColor * 0.6f * Intensity,
                    2f
                );
            }

            // Draw connecting arch between center columns
            if (GATE_COLUMN_COUNT >= 2)
            {
                Vector2 leftTop = new Vector2(gateColumns[1].BasePosition.X, gateColumns[1].BasePosition.Y - gateColumns[1].Height);
                Vector2 rightTop = new Vector2(gateColumns[2].BasePosition.X, gateColumns[2].BasePosition.Y - gateColumns[2].Height);
                Vector2 archCenter = (leftTop + rightTop) / 2f - new Vector2(0, 30f);

                // Draw arch as bezier curve
                DrawArch(leftTop, archCenter, rightTop, GateGoldColor * Intensity);
            }
        }

        private void DrawGateGlow(Vector2 basePos, float width, float height, Color color)
        {
            int layers = 6;
            for (int i = 0; i < layers; i++)
            {
                float t = (float)i / layers;
                float expansion = t * 10f;
                float layerAlpha = 1f - t;
                
                Draw.Rect(
                    basePos.X - width / 2 - expansion,
                    basePos.Y - height - expansion,
                    width + expansion * 2,
                    height + expansion,
                    color * layerAlpha * 0.3f
                );
            }
        }

        private void DrawArch(Vector2 start, Vector2 control, Vector2 end, Color color)
        {
            int segments = 16;
            Vector2 prev = start;
            
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                // Quadratic bezier
                Vector2 p = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * end;
                
                Draw.Line(prev, p, color, 4f);
                prev = p;
            }
        }

        private void DrawAstralParticles()
        {
            for (int i = 0; i < ASTRAL_PARTICLE_COUNT; i++)
            {
                ref AstralParticle p = ref astralParticles[i];
                
                int lutIndex = (int)(p.RainbowPhase * RAINBOW_LUT_SIZE) % RAINBOW_LUT_SIZE;
                Color baseColor = p.IsVoid ? rainbowLUT[lutIndex] : GateGoldColor;
                Color color = baseColor * p.Alpha * Intensity;
                
                float fadeIn = Math.Min(p.Lifetime * 2f, 1f);
                color *= fadeIn;
                
                // Draw particle as small diamond
                DrawDiamond(p.Position, p.Size, p.Rotation, color);
            }
        }

        private void DrawDiamond(Vector2 pos, float size, float rotation, Color color)
        {
            Vector2 up = Calc.AngleToVector(rotation - MathHelper.PiOver2, size);
            Vector2 right = Calc.AngleToVector(rotation, size * 0.6f);
            
            Draw.Line(pos - up, pos + right, color);
            Draw.Line(pos + right, pos + up, color);
            Draw.Line(pos + up, pos - right, color);
            Draw.Line(pos - right, pos - up, color);
        }

        private void EnsureRenderTarget()
        {
            if (renderTarget == null || renderTarget.IsDisposed)
            {
                renderTarget = VirtualContent.CreateRenderTarget("HeavenGatesBackdrop", 320, 180);
            }
        }
        #endregion

        #region Helpers
        private static Color HSVToRGB(float h, float s, float v)
        {
            h = h * 360f % 360f;
            if (h < 0) h += 360f;

            float c = v * s;
            float x = c * (1f - Math.Abs(h / 60f % 2f - 1f));
            float m = v - c;

            float r, g, b;
            if (h < 60f) { r = c; g = x; b = 0; }
            else if (h < 120f) { r = x; g = c; b = 0; }
            else if (h < 180f) { r = 0; g = c; b = x; }
            else if (h < 240f) { r = 0; g = x; b = c; }
            else if (h < 300f) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return new Color(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255)
            );
        }
        #endregion

        #region Cleanup
        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            renderTarget?.Dispose();
            renderTarget = null;
        }
        #endregion
    }
}
