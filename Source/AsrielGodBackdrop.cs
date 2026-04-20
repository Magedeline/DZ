namespace Celeste.Effects
{
    /// <summary>
    /// Asriel God of Hyperdeath boss backdrop effect.
    /// Features a rainbow perspective grid that expands outward from the center
    /// with twinkling stars and cosmic rainbow effects.
    /// Uses the bg00.png perspective grid texture.
    /// </summary>
    [CustomBackdrop("MaggyHelper/AsrielGodBackdrop")]
    [HotReloadable]
    public class AsrielGodBackdrop : Backdrop
    {
        #region Structs
        private struct Star
        {
            public Vector2 Position;
            public float Size;
            public float Brightness;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public Color BaseColor;
            public float RainbowOffset;
        }

        private struct GridLayer
        {
            public float Scale;
            public float Rotation;
            public float Alpha;
            public float RainbowPhase;
            public float ExpandSpeed;
            public float MaxScale;
        }

        private struct RainbowRay
        {
            public float Angle;
            public float Width;
            public float Length;
            public float RainbowOffset;
            public float PulsePhase;
            public float Alpha;
        }
        #endregion

        #region Constants
        private const int STAR_COUNT = 200;
        private const int GRID_LAYER_COUNT = 6;
        private const int RAINBOW_RAY_COUNT = 12;
        private const int RAINBOW_COLOR_COUNT = 16;
        private const int RAINBOW_LUT_SIZE = 256;
        #endregion

        #region Fields
        private readonly MTexture gridTexture;
        private readonly Star[] stars;
        private readonly GridLayer[] gridLayers;
        private readonly RainbowRay[] rainbowRays;
        private readonly Color[] rainbowColors;
        // Pre-computed rainbow lookup table for fast indexed access
        private readonly Color[] rainbowLUT;

        private VirtualRenderTarget renderTarget;
        private float globalTime;
        private float rainbowTime;
        private float expansionPulse;
        private Vector2 center;
        private Vector2 cameraOffset;

        // Configuration
        public float Intensity = 1f;
        public new float Speed = 1f;
        public float StarIntensity = 1f;
        public float GridExpansionSpeed = 0.3f;
        public float RainbowSpeed = 2f;
        public Color BackgroundColor = new(10, 5, 30); // Deep space blue-black
        #endregion

        #region Constructor
        public AsrielGodBackdrop()
        {
            center = new Vector2(160f, 90f);

            // Load the perspective grid texture with null check
            try
            {
                gridTexture = GFX.Game["bgs/maggy/20/asriel/bg00"];
                if (gridTexture == null)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper/AsrielGodBackdrop", "Grid texture 'bgs/maggy/20/asriel/bg00' not found");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper/AsrielGodBackdrop", $"Failed to load grid texture: {ex.Message}");
                gridTexture = null;
            }

            // Initialize rainbow colors
            rainbowColors = new Color[RAINBOW_COLOR_COUNT];
            rainbowLUT = new Color[RAINBOW_LUT_SIZE];
            InitializeRainbowColors();

            // Initialize stars
            stars = new Star[STAR_COUNT];
            InitializeStars();

            // Initialize grid layers for expanding effect
            gridLayers = new GridLayer[GRID_LAYER_COUNT];
            InitializeGridLayers();

            // Initialize rainbow rays
            rainbowRays = new RainbowRay[RAINBOW_RAY_COUNT];
            InitializeRainbowRays();
        }

        public AsrielGodBackdrop(BinaryPacker.Element data) : this()
        {
            if (data.HasAttr("intensity"))
                Intensity = data.AttrFloat("intensity", 1f);
            
            if (data.HasAttr("speed"))
                Speed = data.AttrFloat("speed", 1f);
            
            if (data.HasAttr("starIntensity"))
                StarIntensity = data.AttrFloat("starIntensity", 1f);
            
            if (data.HasAttr("gridExpansionSpeed"))
                GridExpansionSpeed = data.AttrFloat("gridExpansionSpeed", 0.3f);
            
            if (data.HasAttr("rainbowSpeed"))
                RainbowSpeed = data.AttrFloat("rainbowSpeed", 2f);
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
            for (int i = 0; i < RAINBOW_LUT_SIZE; i++)
            {
                float hue = (float)i / RAINBOW_LUT_SIZE;
                rainbowLUT[i] = HSVToRGB(hue, 1f, 1f);
            }
        }

        private void InitializeStars()
        {
            for (int i = 0; i < STAR_COUNT; i++)
            {
                stars[i] = new Star
                {
                    Position = new Vector2(
                        Calc.Random.Range(-40f, 360f),
                        Calc.Random.Range(-40f, 220f)
                    ),
                    Size = Calc.Random.Range(1f, 4f),
                    Brightness = Calc.Random.Range(0.3f, 1f),
                    TwinklePhase = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    TwinkleSpeed = Calc.Random.Range(2f, 6f),
                    BaseColor = Color.White,
                    RainbowOffset = Calc.Random.NextFloat() * MathHelper.TwoPi
                };
            }
        }

        private void InitializeGridLayers()
        {
            for (int i = 0; i < GRID_LAYER_COUNT; i++)
            {
                float layerFactor = (float)i / GRID_LAYER_COUNT;
                gridLayers[i] = new GridLayer
                {
                    Scale = 0.1f + layerFactor * 0.5f,
                    Rotation = Calc.Random.Range(-0.1f, 0.1f),
                    Alpha = 0.8f - layerFactor * 0.3f,
                    RainbowPhase = layerFactor * MathHelper.TwoPi,
                    ExpandSpeed = GridExpansionSpeed * (1f + layerFactor * 0.5f),
                    MaxScale = 3f + layerFactor * 2f
                };
            }
        }

        private void InitializeRainbowRays()
        {
            for (int i = 0; i < RAINBOW_RAY_COUNT; i++)
            {
                float angle = (float)i / RAINBOW_RAY_COUNT * MathHelper.TwoPi;
                rainbowRays[i] = new RainbowRay
                {
                    Angle = angle,
                    Width = Calc.Random.Range(20f, 60f),
                    Length = Calc.Random.Range(200f, 400f),
                    RainbowOffset = angle,
                    PulsePhase = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    Alpha = Calc.Random.Range(0.3f, 0.6f)
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
            
            // Pulsing expansion effect
            expansionPulse = (float)Math.Sin(globalTime * 0.5f) * 0.2f + 1f;

            // Update grid layers - continuous expansion
            for (int i = 0; i < GRID_LAYER_COUNT; i++)
            {
                gridLayers[i].Scale += Engine.DeltaTime * gridLayers[i].ExpandSpeed * Speed;
                gridLayers[i].RainbowPhase += Engine.DeltaTime * RainbowSpeed;
                gridLayers[i].Rotation += Engine.DeltaTime * 0.05f * (i % 2 == 0 ? 1 : -1);
                
                // Reset when fully expanded
                if (gridLayers[i].Scale > gridLayers[i].MaxScale)
                {
                    gridLayers[i].Scale = 0.1f;
                    gridLayers[i].Alpha = 0.8f;
                }
                else
                {
                    // Fade out as it expands
                    float expandProgress = gridLayers[i].Scale / gridLayers[i].MaxScale;
                    gridLayers[i].Alpha = MathHelper.Lerp(0.8f, 0f, expandProgress);
                }
            }

            // Update stars
            for (int i = 0; i < STAR_COUNT; i++)
            {
                stars[i].TwinklePhase += Engine.DeltaTime * stars[i].TwinkleSpeed;
                
                // Rainbow color cycling for some stars - use LUT instead of HSVToRGB
                if (i % 3 == 0)
                {
                    float hue = (rainbowTime * 0.2f + stars[i].RainbowOffset) % MathHelper.TwoPi / MathHelper.TwoPi;
                    Color fullColor = rainbowLUT[(int)(hue * (RAINBOW_LUT_SIZE - 1)) & (RAINBOW_LUT_SIZE - 1)];
                    // Approximate HSVToRGB(hue, 0.5, 1.0) by lerping with white
                    stars[i].BaseColor = Color.Lerp(Color.White, fullColor, 0.5f);
                }
            }

            // Update rainbow rays
            for (int i = 0; i < RAINBOW_RAY_COUNT; i++)
            {
                rainbowRays[i].PulsePhase += Engine.DeltaTime * 2f;
                rainbowRays[i].Angle += Engine.DeltaTime * 0.1f;
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
                renderTarget = VirtualContent.CreateRenderTarget("AsrielGod Backdrop", 320, 180);
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Engine.Graphics.GraphicsDevice.Clear(BackgroundColor);

            // Draw rainbow rays behind everything
            DrawRainbowRays();

            // Draw expanding grid layers
            DrawGridLayers();

            // Draw stars on top
            DrawStars();

            // Add cosmic glow overlay
            DrawCosmicGlow();
        }

        private void DrawRainbowRays()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            for (int i = 0; i < RAINBOW_RAY_COUNT; i++)
            {
                ref readonly RainbowRay ray = ref rainbowRays[i];
                
                float pulse = (float)Math.Sin(ray.PulsePhase) * 0.3f + 0.7f;
                float hue = (rainbowTime * 0.3f + ray.RainbowOffset) % MathHelper.TwoPi / MathHelper.TwoPi;
                Color rayColor = HSVToRGB(hue, 1f, 1f) * ray.Alpha * pulse * Intensity;

                Vector2 direction = Calc.AngleToVector(ray.Angle, 1f);
                Vector2 perpendicular = new(-direction.Y, direction.X);
                
                Vector2 start = center;
                Vector2 end = center + direction * ray.Length * expansionPulse;
                
                // Draw gradient ray
                for (int j = 0; j < 10; j++)
                {
                    float t = (float)j / 10f;
                    Vector2 pos = Vector2.Lerp(start, end, t);
                    float width = MathHelper.Lerp(ray.Width * 0.1f, ray.Width, t);
                    Color segmentColor = rayColor * (1f - t * 0.5f);
                    
                    Draw.Line(
                        pos - perpendicular * width * 0.5f,
                        pos + perpendicular * width * 0.5f,
                        segmentColor
                    );
                }
            }

            Draw.SpriteBatch.End();
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

                    // Calculate rainbow color for this layer
                    float hue = (layer.RainbowPhase + rainbowTime * 0.5f) % MathHelper.TwoPi / MathHelper.TwoPi;
                    Color gridColor = HSVToRGB(hue, 1f, 1f) * layer.Alpha * Intensity;

                    Vector2 origin = new(gridTexture.Width / 2f, gridTexture.Height / 2f);
                    Vector2 pos = center - cameraOffset * (1f + i * 0.1f);
                    float scale = layer.Scale * expansionPulse;

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

        private void DrawStars()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            for (int i = 0; i < STAR_COUNT; i++)
            {
                ref readonly Star star = ref stars[i];
                
                float twinkle = (float)Math.Sin(star.TwinklePhase) * 0.5f + 0.5f;
                float brightness = star.Brightness * twinkle * StarIntensity * Intensity;
                
                if (brightness <= 0.01f)
                    continue;

                Vector2 pos = star.Position - cameraOffset;
                Color starColor = star.BaseColor * brightness;
                float size = star.Size * (1f + twinkle * 0.5f);

                // Draw star with glow
                Draw.Rect(pos - new Vector2(size / 2), size, size, starColor);
                
                // Add cross flare for brighter stars
                if (brightness > 0.5f && star.Size > 2f)
                {
                    float flareSize = size * 2f;
                    Color flareColor = starColor * 0.5f;
                    Draw.Line(pos - new Vector2(flareSize, 0), pos + new Vector2(flareSize, 0), flareColor);
                    Draw.Line(pos - new Vector2(0, flareSize), pos + new Vector2(0, flareSize), flareColor);
                }
            }

            Draw.SpriteBatch.End();
        }

        private void DrawCosmicGlow()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            // Central rainbow glow
            for (int i = 0; i < 5; i++)
            {
                float radius = 30f + i * 20f;
                float hue = (rainbowTime * 0.2f + i * 0.2f) % 1f;
                Color glowColor = rainbowLUT[(int)(hue * (RAINBOW_LUT_SIZE - 1)) & (RAINBOW_LUT_SIZE - 1)];
                // Approximate saturation 0.8 by lerping with white
                glowColor = Color.Lerp(Color.White, glowColor, 0.8f) * (0.3f - i * 0.05f) * Intensity;
                
                // Draw circular glow approximation (halved from 32 to 16 segments)
                int segments = 16;
                for (int j = 0; j < segments; j++)
                {
                    float angle1 = (float)j / segments * MathHelper.TwoPi;
                    float angle2 = (float)(j + 1) / segments * MathHelper.TwoPi;
                    
                    Vector2 p1 = center + Calc.AngleToVector(angle1, radius);
                    Vector2 p2 = center + Calc.AngleToVector(angle2, radius);
                    
                    Draw.Line(p1, p2, glowColor * 2f);
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
