namespace MaggyHelper.Effects
{
    /// <summary>
    /// Els True Final Boss backdrop effect.
    /// Features a dark void with black center, rainbow edges that expand outward,
    /// and corrupted perspective grid effect.
    /// Uses the bg00.png perspective grid texture with inverted/dark theming.
    /// </summary>
    [CustomBackdrop("MaggyHelper/ElsTrueFinalBackdrop")]
    [HotReloadable]
    public class ElsTrueFinalBackdrop : Backdrop
    {
        #region Structs
        private struct VoidParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float Alpha;
            public float RainbowPhase;
            public float Lifetime;
            public bool IsRainbow;
        }

        private struct GridLayer
        {
            public float Scale;
            public float Rotation;
            public float Alpha;
            public float RainbowPhase;
            public float ExpandSpeed;
            public float MaxScale;
            public bool IsInverted;
            public float DistortionPhase;
        }

        private struct CorruptionTendril
        {
            public float Angle;
            public float Length;
            public float Width;
            public float Phase;
            public float Speed;
            public float RainbowOffset;
        }

        private struct VoidRing
        {
            public float Radius;
            public float ExpandSpeed;
            public float Alpha;
            public float RainbowPhase;
            public float Thickness;
        }
        #endregion

        #region Constants
        private const int VOID_PARTICLE_COUNT = 150;
        private const int GRID_LAYER_COUNT = 8;
        private const int TENDRIL_COUNT = 16;
        private const int VOID_RING_COUNT = 5;
        private const int RAINBOW_COLOR_COUNT = 16;
        private const int RAINBOW_LUT_SIZE = 256;
        #endregion

        #region Fields
        private readonly MTexture gridTexture;
        private readonly VoidParticle[] voidParticles;
        private readonly GridLayer[] gridLayers;
        private readonly CorruptionTendril[] tendrils;
        private readonly VoidRing[] voidRings;
        private readonly Color[] rainbowColors;
        // Pre-computed rainbow lookup table for fast indexed access
        private readonly Color[] rainbowLUT;

        private VirtualRenderTarget renderTarget;
        private float globalTime;
        private float rainbowTime;
        private float voidPulse;
        private float corruptionIntensity;
        private Vector2 center;
        private Vector2 cameraOffset;

        // Configuration
        public float Intensity = 1f;
        public new float Speed = 1f;
        public float VoidRadius = 60f;
        public float RainbowEdgeIntensity = 1f;
        public float GridExpansionSpeed = 0.4f;
        public float RainbowSpeed = 1.5f;
        public float CorruptionSpeed = 0.8f;
        public Color VoidColor = Color.Black;
        public Color BackgroundColor = new(5, 0, 10); // Deep void purple-black
        #endregion

        #region Constructor
        public ElsTrueFinalBackdrop()
        {
            center = new Vector2(160f, 90f);

            // Load the perspective grid texture with null check
            try
            {
                gridTexture = GFX.Game["bgs/maggy/12/bg00"];
                if (gridTexture == null)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper/ElsTrueFinalBackdrop", "Grid texture 'bgs/maggy/12/bg00' not found");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper/ElsTrueFinalBackdrop", $"Failed to load grid texture: {ex.Message}");
                gridTexture = null;
            }

            // Initialize rainbow colors
            rainbowColors = new Color[RAINBOW_COLOR_COUNT];
            rainbowLUT = new Color[RAINBOW_LUT_SIZE];
            InitializeRainbowColors();

            // Initialize void particles
            voidParticles = new VoidParticle[VOID_PARTICLE_COUNT];
            InitializeVoidParticles();

            // Initialize grid layers
            gridLayers = new GridLayer[GRID_LAYER_COUNT];
            InitializeGridLayers();

            // Initialize corruption tendrils
            tendrils = new CorruptionTendril[TENDRIL_COUNT];
            InitializeTendrils();

            // Initialize void rings
            voidRings = new VoidRing[VOID_RING_COUNT];
            InitializeVoidRings();
        }

        public ElsTrueFinalBackdrop(BinaryPacker.Element data) : this()
        {
            if (data.HasAttr("intensity"))
                Intensity = data.AttrFloat("intensity", 1f);
            
            if (data.HasAttr("speed"))
                Speed = data.AttrFloat("speed", 1f);
            
            if (data.HasAttr("voidRadius"))
                VoidRadius = data.AttrFloat("voidRadius", 60f);
            
            if (data.HasAttr("rainbowEdgeIntensity"))
                RainbowEdgeIntensity = data.AttrFloat("rainbowEdgeIntensity", 1f);
            
            if (data.HasAttr("gridExpansionSpeed"))
                GridExpansionSpeed = data.AttrFloat("gridExpansionSpeed", 0.4f);
            
            if (data.HasAttr("rainbowSpeed"))
                RainbowSpeed = data.AttrFloat("rainbowSpeed", 1.5f);
            
            if (data.HasAttr("corruptionSpeed"))
                CorruptionSpeed = data.AttrFloat("corruptionSpeed", 0.8f);
        }
        #endregion

        #region Initialization
        private void InitializeRainbowColors()
        {
            for (int i = 0; i < RAINBOW_COLOR_COUNT; i++)
            {
                float hue = (float)i / RAINBOW_COLOR_COUNT;
                rainbowColors[i] = HSVToRGB(hue, 1f, 1f);
            }
            // Build LUT for fast rainbow color lookups
            for (int i = 0; i < RAINBOW_LUT_SIZE; i++)
            {
                float hue = (float)i / RAINBOW_LUT_SIZE;
                rainbowLUT[i] = HSVToRGB(hue, 1f, 1f);
            }
        }

        private void InitializeVoidParticles()
        {
            for (int i = 0; i < VOID_PARTICLE_COUNT; i++)
            {
                ResetVoidParticle(ref voidParticles[i], true);
            }
        }

        private void ResetVoidParticle(ref VoidParticle particle, bool randomLifetime = false)
        {
            // Spawn from edges, moving towards center (or from center moving outward for rainbow)
            float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
            bool isRainbow = Calc.Random.NextFloat() > 0.6f;
            
            if (isRainbow)
            {
                // Rainbow particles spawn from center, move outward
                float distance = Calc.Random.Range(10f, VoidRadius);
                particle.Position = center + Calc.AngleToVector(angle, distance);
                particle.Velocity = Calc.AngleToVector(angle, Calc.Random.Range(20f, 80f));
            }
            else
            {
                // Void particles spawn from edges, move toward center
                float distance = Calc.Random.Range(150f, 250f);
                particle.Position = center + Calc.AngleToVector(angle, distance);
                particle.Velocity = -Calc.AngleToVector(angle, Calc.Random.Range(30f, 100f));
            }
            
            particle.Size = Calc.Random.Range(1f, 4f);
            particle.Alpha = Calc.Random.Range(0.5f, 1f);
            particle.RainbowPhase = Calc.Random.NextFloat() * MathHelper.TwoPi;
            particle.Lifetime = randomLifetime ? Calc.Random.Range(0f, 3f) : 0f;
            particle.IsRainbow = isRainbow;
        }

        private void InitializeGridLayers()
        {
            for (int i = 0; i < GRID_LAYER_COUNT; i++)
            {
                float layerFactor = (float)i / GRID_LAYER_COUNT;
                gridLayers[i] = new GridLayer
                {
                    Scale = 0.1f + layerFactor * 0.3f,
                    Rotation = Calc.Random.Range(-0.15f, 0.15f),
                    Alpha = 0.6f - layerFactor * 0.2f,
                    RainbowPhase = layerFactor * MathHelper.TwoPi,
                    ExpandSpeed = GridExpansionSpeed * (0.8f + layerFactor * 0.4f),
                    MaxScale = 4f + layerFactor * 2f,
                    IsInverted = i % 2 == 0,
                    DistortionPhase = Calc.Random.NextFloat() * MathHelper.TwoPi
                };
            }
        }

        private void InitializeTendrils()
        {
            for (int i = 0; i < TENDRIL_COUNT; i++)
            {
                float angle = (float)i / TENDRIL_COUNT * MathHelper.TwoPi;
                tendrils[i] = new CorruptionTendril
                {
                    Angle = angle,
                    Length = Calc.Random.Range(100f, 200f),
                    Width = Calc.Random.Range(3f, 10f),
                    Phase = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    Speed = Calc.Random.Range(1f, 3f),
                    RainbowOffset = Calc.Random.NextFloat() * MathHelper.TwoPi
                };
            }
        }

        private void InitializeVoidRings()
        {
            for (int i = 0; i < VOID_RING_COUNT; i++)
            {
                float layerFactor = (float)i / VOID_RING_COUNT;
                voidRings[i] = new VoidRing
                {
                    Radius = VoidRadius * (1f + layerFactor * 0.5f),
                    ExpandSpeed = Calc.Random.Range(20f, 50f),
                    Alpha = 0.8f,
                    RainbowPhase = layerFactor * MathHelper.TwoPi,
                    Thickness = Calc.Random.Range(2f, 6f)
                };
            }
        }
        #endregion

        #region Update
        public override void Update(Scene scene)
        {
            base.Update(scene);

            if (!Visible)
                return;

            globalTime += Engine.DeltaTime * Speed;
            rainbowTime += Engine.DeltaTime * RainbowSpeed;
            
            // Pulsing void effect
            voidPulse = (float)Math.Sin(globalTime * 0.8f) * 0.15f + 1f;
            corruptionIntensity = (float)Math.Sin(globalTime * CorruptionSpeed) * 0.3f + 0.7f;

            // Update grid layers - continuous expansion
            for (int i = 0; i < GRID_LAYER_COUNT; i++)
            {
                gridLayers[i].Scale += Engine.DeltaTime * gridLayers[i].ExpandSpeed * Speed;
                gridLayers[i].RainbowPhase += Engine.DeltaTime * RainbowSpeed * 0.5f;
                gridLayers[i].DistortionPhase += Engine.DeltaTime * 2f;
                
                // Alternate rotation directions
                float rotationDir = gridLayers[i].IsInverted ? -1f : 1f;
                gridLayers[i].Rotation += Engine.DeltaTime * 0.08f * rotationDir;
                
                // Reset when fully expanded
                if (gridLayers[i].Scale > gridLayers[i].MaxScale)
                {
                    gridLayers[i].Scale = 0.1f;
                    gridLayers[i].Alpha = 0.6f;
                }
                else
                {
                    // Fade out as it expands
                    float expandProgress = gridLayers[i].Scale / gridLayers[i].MaxScale;
                    gridLayers[i].Alpha = MathHelper.Lerp(0.6f, 0f, expandProgress);
                }
            }

            // Update void particles
            for (int i = 0; i < VOID_PARTICLE_COUNT; i++)
            {
                voidParticles[i].Position += voidParticles[i].Velocity * Engine.DeltaTime;
                voidParticles[i].Lifetime += Engine.DeltaTime;
                voidParticles[i].RainbowPhase += Engine.DeltaTime * 3f;
                
                // Reset particles that go too far or too close
                float distanceFromCenter = Vector2.Distance(voidParticles[i].Position, center);
                
                if (voidParticles[i].IsRainbow)
                {
                    if (distanceFromCenter > 250f || voidParticles[i].Lifetime > 3f)
                        ResetVoidParticle(ref voidParticles[i]);
                }
                else
                {
                    if (distanceFromCenter < VoidRadius * 0.5f || voidParticles[i].Lifetime > 4f)
                        ResetVoidParticle(ref voidParticles[i]);
                }
            }

            // Update tendrils
            for (int i = 0; i < TENDRIL_COUNT; i++)
            {
                tendrils[i].Phase += Engine.DeltaTime * tendrils[i].Speed;
                tendrils[i].Angle += Engine.DeltaTime * 0.05f;
            }

            // Update void rings
            for (int i = 0; i < VOID_RING_COUNT; i++)
            {
                voidRings[i].Radius += Engine.DeltaTime * voidRings[i].ExpandSpeed;
                voidRings[i].RainbowPhase += Engine.DeltaTime * RainbowSpeed;
                
                // Fade and reset
                float maxRadius = 250f;
                if (voidRings[i].Radius > maxRadius)
                {
                    voidRings[i].Radius = VoidRadius;
                    voidRings[i].Alpha = 0.8f;
                }
                else
                {
                    float progress = (voidRings[i].Radius - VoidRadius) / (maxRadius - VoidRadius);
                    voidRings[i].Alpha = MathHelper.Lerp(0.8f, 0f, progress);
                }
            }

            // Camera tracking
            if (scene is Level level)
            {
                Vector2 targetOffset = level.Camera.Position * 0.05f;
                cameraOffset += (targetOffset - cameraOffset) * (1f - (float)Math.Pow(0.01, Engine.DeltaTime));
            }
        }
        #endregion

        #region Rendering
        public override void BeforeRender(Scene scene)
        {
            if (renderTarget == null || renderTarget.IsDisposed)
            {
                renderTarget = VirtualContent.CreateRenderTarget("ElsTrueFinal Backdrop", 320, 180);
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Engine.Graphics.GraphicsDevice.Clear(BackgroundColor);

            // Draw expanding grid layers (rainbow edges)
            DrawGridLayers();

            // Draw corruption tendrils
            DrawCorruptionTendrils();

            // Draw void rings expanding outward + void particles (merged batch)
            DrawVoidRingsAndParticles();

            // Draw central void (black hole effect)
            DrawCentralVoid();

            // Add rainbow edge glow
            DrawRainbowEdgeGlow();
        }

        private void DrawGridLayers()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            if (gridTexture != null)
            {
                for (int i = 0; i < GRID_LAYER_COUNT; i++)
                {
                    ref readonly GridLayer layer = ref gridLayers[i];
                    
                    if (layer.Alpha <= 0.01f)
                        continue;

                    // Rainbow color with dark tint
                    float hue = (layer.RainbowPhase + rainbowTime * 0.3f) % MathHelper.TwoPi / MathHelper.TwoPi;
                    Color baseColor = HSVToRGB(hue, 1f, 0.8f);
                    
                    // Mix with black for corrupted look
                    Color gridColor;
                    if (layer.IsInverted)
                    {
                        // Dark/inverted layers - more black with rainbow edges
                        gridColor = Color.Lerp(VoidColor, baseColor, 0.3f) * layer.Alpha * Intensity;
                    }
                    else
                    {
                        // Rainbow layers
                        gridColor = baseColor * layer.Alpha * Intensity * RainbowEdgeIntensity;
                    }

                    Vector2 origin = new(gridTexture.Width / 2f, gridTexture.Height / 2f);
                    Vector2 pos = center - cameraOffset * (1f + i * 0.05f);
                    
                    // Add distortion to scale
                    float distortion = (float)Math.Sin(layer.DistortionPhase) * 0.1f;
                    float scale = layer.Scale * voidPulse * (1f + distortion);

                    gridTexture.Draw(
                        pos,
                        origin,
                        gridColor,
                        scale,
                        layer.Rotation
                    );
                }
            }

            Draw.SpriteBatch.End();
        }

        private void DrawCorruptionTendrils()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            for (int i = 0; i < TENDRIL_COUNT; i++)
            {
                ref readonly CorruptionTendril tendril = ref tendrils[i];
                
                // Rainbow color at the tip, fading to black at center - use LUT
                float hue = (rainbowTime * 0.4f + tendril.RainbowOffset) % MathHelper.TwoPi / MathHelper.TwoPi;
                Color tipColor = rainbowLUT[(int)(hue * (RAINBOW_LUT_SIZE - 1)) & (RAINBOW_LUT_SIZE - 1)];
                
                float wave = (float)Math.Sin(tendril.Phase) * 20f;
                float length = tendril.Length * corruptionIntensity + wave;
                
                // Draw tendril as series of segments (reduced from 15 to 8)
                int segments = 8;
                for (int j = 0; j < segments; j++)
                {
                    float t1 = (float)j / segments;
                    float t2 = (float)(j + 1) / segments;
                    
                    float dist1 = VoidRadius + t1 * length;
                    float dist2 = VoidRadius + t2 * length;
                    
                    // Wavy angle
                    float waveOffset1 = (float)Math.Sin(tendril.Phase + t1 * 4f) * 0.2f;
                    float waveOffset2 = (float)Math.Sin(tendril.Phase + t2 * 4f) * 0.2f;
                    
                    Vector2 p1 = center + Calc.AngleToVector(tendril.Angle + waveOffset1, dist1);
                    Vector2 p2 = center + Calc.AngleToVector(tendril.Angle + waveOffset2, dist2);
                    
                    // Color fades from black to rainbow
                    Color segmentColor = Color.Lerp(VoidColor, tipColor, t1 * t1) * (1f - t1 * 0.5f) * Intensity * 0.6f;
                    
                    Draw.Line(p1, p2, segmentColor);
                }
            }

            Draw.SpriteBatch.End();
        }

        /// <summary>
        /// Merged DrawVoidRings + DrawVoidParticles into a single SpriteBatch to save Begin/End calls.
        /// Ring segments halved from 48 to 24.
        /// </summary>
        private void DrawVoidRingsAndParticles()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            // --- Void Rings ---
            for (int i = 0; i < VOID_RING_COUNT; i++)
            {
                ref readonly VoidRing ring = ref voidRings[i];
                
                if (ring.Alpha <= 0.01f)
                    continue;

                // Rainbow ring with dark edges - use LUT
                float hue = (ring.RainbowPhase + rainbowTime * 0.5f) % MathHelper.TwoPi / MathHelper.TwoPi;
                float ringAlphaBase = ring.Alpha * Intensity * RainbowEdgeIntensity;

                // Draw ring as connected segments (halved from 48 to 24)
                int segments = 24;
                for (int j = 0; j < segments; j++)
                {
                    float angle1 = (float)j / segments * MathHelper.TwoPi;
                    float angle2 = (float)(j + 1) / segments * MathHelper.TwoPi;
                    
                    Vector2 p1 = center + Calc.AngleToVector(angle1, ring.Radius * voidPulse);
                    Vector2 p2 = center + Calc.AngleToVector(angle2, ring.Radius * voidPulse);
                    
                    // Vary color around the ring - use LUT
                    float segmentHue = (hue + (float)j / segments) % 1f;
                    Color segmentColor = rainbowLUT[(int)(segmentHue * (RAINBOW_LUT_SIZE - 1)) & (RAINBOW_LUT_SIZE - 1)] * ringAlphaBase * 0.4f;
                    
                    Draw.Line(p1, p2, segmentColor, ring.Thickness);
                }
            }

            // --- Void Particles ---
            for (int i = 0; i < VOID_PARTICLE_COUNT; i++)
            {
                ref readonly VoidParticle particle = ref voidParticles[i];
                
                Vector2 pos = particle.Position - cameraOffset;
                Color particleColor;
                
                if (particle.IsRainbow)
                {
                    // Rainbow particles - use LUT
                    float hue = (particle.RainbowPhase + rainbowTime) % MathHelper.TwoPi / MathHelper.TwoPi;
                    particleColor = rainbowLUT[(int)(hue * (RAINBOW_LUT_SIZE - 1)) & (RAINBOW_LUT_SIZE - 1)] * particle.Alpha * Intensity * RainbowEdgeIntensity;
                }
                else
                {
                    // Dark void particles with slight color tint - use LUT
                    float hue = (particle.RainbowPhase * 0.2f) % 1f;
                    Color tint = rainbowLUT[(int)(hue * (RAINBOW_LUT_SIZE - 1)) & (RAINBOW_LUT_SIZE - 1)];
                    // Approximate HSVToRGB(hue, 0.5, 0.3) by scaling LUT color
                    tint = new Color(tint.ToVector3() * 0.3f);
                    particleColor = Color.Lerp(VoidColor, tint, 0.2f) * particle.Alpha * Intensity;
                }

                Draw.Rect(pos.X - particle.Size * 0.5f, pos.Y - particle.Size * 0.5f, particle.Size, particle.Size, particleColor);
            }

            Draw.SpriteBatch.End();
        }

        private void DrawCentralVoid()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Draw black void center with gradient
            float currentRadius = VoidRadius * voidPulse;
            Color voidIntensity = VoidColor * Intensity;
            
            // Solid black center (halved from 32 to 16 segments)
            int centerSegments = 16;
            for (int j = 0; j < centerSegments; j++)
            {
                float angle1 = (float)j / centerSegments * MathHelper.TwoPi;
                float angle2 = (float)(j + 1) / centerSegments * MathHelper.TwoPi;
                
                Vector2 p1 = center + Calc.AngleToVector(angle1, currentRadius * 0.5f);
                Vector2 p2 = center + Calc.AngleToVector(angle2, currentRadius * 0.5f);
                
                // Draw filled triangle to center
                Draw.Line(center, p1, voidIntensity);
                Draw.Line(center, p2, voidIntensity);
                Draw.Line(p1, p2, voidIntensity);
            }

            // Gradient edge of void (reduced from 10 to 5 rings)
            for (int ring = 0; ring < 5; ring++)
            {
                float t = (float)ring / 5f;
                float radius = currentRadius * (0.5f + t * 0.5f);
                float alpha = 1f - t;
                Color ringColor = VoidColor * alpha * Intensity;
                
                for (int j = 0; j < centerSegments; j++)
                {
                    float angle1 = (float)j / centerSegments * MathHelper.TwoPi;
                    float angle2 = (float)(j + 1) / centerSegments * MathHelper.TwoPi;
                    
                    Vector2 p1 = center + Calc.AngleToVector(angle1, radius);
                    Vector2 p2 = center + Calc.AngleToVector(angle2, radius);
                    
                    Draw.Line(p1, p2, ringColor, 3f);
                }
            }

            Draw.SpriteBatch.End();
        }

        private void DrawRainbowEdgeGlow()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            // Rainbow glow around the void edge
            float edgeRadius = VoidRadius * voidPulse;
            int glowLayers = 8;
            
            for (int layer = 0; layer < glowLayers; layer++)
            {
                float layerRadius = edgeRadius + layer * 5f;
                float layerAlpha = (1f - (float)layer / glowLayers) * 0.4f * Intensity * RainbowEdgeIntensity;
                
                // Halved from 64 to 32 segments
                int segments = 32;
                for (int j = 0; j < segments; j++)
                {
                    float angle = (float)j / segments * MathHelper.TwoPi;
                    float nextAngle = (float)(j + 1) / segments * MathHelper.TwoPi;
                    
                    // Rainbow color based on angle and time - use LUT
                    float hue = (angle / MathHelper.TwoPi + rainbowTime * 0.2f) % 1f;
                    Color glowColor = rainbowLUT[(int)(hue * (RAINBOW_LUT_SIZE - 1)) & (RAINBOW_LUT_SIZE - 1)] * layerAlpha;
                    
                    Vector2 p1 = center + Calc.AngleToVector(angle, layerRadius);
                    Vector2 p2 = center + Calc.AngleToVector(nextAngle, layerRadius);
                    
                    Draw.Line(p1, p2, glowColor, 2f);
                }
            }

            Draw.SpriteBatch.End();
        }

        public override void Render(Scene scene)
        {
            if (renderTarget != null && !renderTarget.IsDisposed && Visible)
            {
                Vector2 renderPos = new(160, 90);
                Vector2 origin = new Vector2(renderTarget.Width, renderTarget.Height) / 2f;

                Draw.SpriteBatch.Draw(
                    (RenderTarget2D)renderTarget,
                    renderPos,
                    renderTarget.Bounds,
                    Color.White * FadeAlphaMultiplier * Intensity,
                    0f,
                    origin,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        #endregion

        #region Cleanup
        public override void Ended(Scene scene)
        {
            base.Ended(scene);

            if (renderTarget != null)
            {
                renderTarget.Dispose();
                renderTarget = null;
            }
        }
        #endregion

        #region Helpers
        private static Color HSVToRGB(float h, float s, float v)
        {
            int i = (int)Math.Floor(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            return (i % 6) switch
            {
                0 => new Color(v, t, p),
                1 => new Color(q, v, p),
                2 => new Color(p, v, t),
                3 => new Color(p, q, v),
                4 => new Color(t, p, v),
                5 => new Color(v, p, q),
                _ => Color.White
            };
        }
        #endregion
    }
}
