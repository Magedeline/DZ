using Celeste.Utils;

namespace Celeste
{
    /// <summary>
    /// A spectacular rainbow blackhole background effect that shifts through the entire color spectrum
    /// with psychedelic, hypnotic patterns and crazy visual effects.
    /// Supports wind-based direction changes for dynamic spinning effects.
    /// Now supports animated background modes: Soul, Zero, and Void.
    /// </summary>
    [CustomBackdrop("MaggyHelper/RainbowBlackholeBg")]
    [HotReloadable]
    public class RainbowBlackholeBg : Backdrop
    {
        public enum Strengths
        {
            Mild,
            Medium,
            High,
            Wild,
            Insane,
            RainbowChaos,
            Cosmic
        }

        /// <summary>
        /// Animation mode for the background effect
        /// </summary>
        public enum AnimationMode
        {
            /// <summary>Default procedural rainbow blackhole effect</summary>
            None,
            /// <summary>Soul animation (48 frames)</summary>
            Soul,
            /// <summary>Zero animation (24 frames)</summary>
            Zero,
            /// <summary>Void animation (24 frames)</summary>
            Void
        }

        private struct StreamParticle
        {
            public int ColorIndex;
            public float ColorPhase;
            public MTexture Texture;
            public float Percent;
            public float Speed;
            public Vector2 Normal;
            public float RainbowOffset;
            public float PulsePhase;
        }

        private struct Particle
        {
            public int ColorIndex;
            public float ColorPhase;
            public Vector2 Normal;
            public float Percent;
            public float RainbowOffset;
            public float Intensity;
            public float SizeMultiplier;
        }

        private struct SpiralDebris
        {
            public int ColorIndex;
            public float ColorPhase;
            public float Percent;
            public float Offset;
            public float RainbowOffset;
            public float Twist;
        }

        private const string STRENGTH_FLAG = "rainbow_blackhole_strength";
        private const int BG_STEPS = 40;
        private const int STREAM_MIN_COUNT = 80;
        private const int STREAM_MAX_COUNT = 150;
        private const int PARTICLE_MIN_COUNT = 300;
        private const int PARTICLE_MAX_COUNT = 600;
        private const int SPIRAL_MIN_COUNT = 10;
        private const int SPIRAL_MAX_COUNT = 25;
        private const int SPIRAL_SEGMENTS = 20;
        private const int COLOR_STEPS = 40;

        // Rainbow color system
        private readonly float rainbowSpeed = 3f;
        private float rainbowTime;
        private Color[] rainbowColors;
        private const int RAINBOW_COLOR_COUNT = 16;
        
        // Strength-based colors
        private Color strengthColor = Color.Blue; // Default to Mild (blue)

        // Crazy effects
        private float pulseTime;
        private float warpTime;
        private float chaosTime;
        private readonly Vector2[] chaosOffsets;
        private readonly float[] chaosPhases;

        // Wind direction system
        private Vector2 currentWind;
        private Vector2 targetWind;
        private float windDirection = 1f;           // Current smoothed direction (-1 to 1)
        private float targetWindDirection = 1f;      // Target direction based on wind
        private float windInfluence = 0f;            // How much the wind affects movement (0 to 1)
        private float windTransitionSpeed = 2f;      // How fast to transition between directions
        private Vector2 windOffset;                  // Position offset based on wind
        private float windSpeedMultiplier = 1f;      // Speed multiplier from wind strength

        // Animation system
        private AnimationMode animationMode = AnimationMode.None;
        private List<MTexture> animationFrames;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private float frameDelay = 0.08f; // ~12.5 FPS by default
        private float animationScale = 1f;
        private bool animationLoops = true;

        // Optional glitch / distortion overlay controls.
        private float glitchAmount = 0f;
        private float distortionAmount = 0f;
        private float distortionFrequency = 2f;
        private float chromaticAberration = 0f;

        // Baseline values from map data used when generator progression overrides visuals.
        private float baseAlpha = 1f;
        private float baseGlitchAmount = 0f;
        private float baseDistortionAmount = 0f;
        private float baseChromaticAberration = 0f;

        public float Alpha = 1f;
        public float Scale = 1f;
        public float Direction = 1f;
        public float StrengthMultiplier = 1f;
        public Vector2 CenterOffset;
        public Vector2 OffsetOffset;

        /// <summary>
        /// If true, wind will affect the spin direction of the blackhole
        /// </summary>
        public bool WindAffectsDirection = true;
        
        /// <summary>
        /// If true, wind will affect the position offset of the blackhole
        /// </summary>
        public bool WindAffectsPosition = true;
        
        /// <summary>
        /// If true, wind will affect the speed of particles/streams
        /// </summary>
        public bool WindAffectsSpeed = true;

        private Strengths strength;
        private readonly Color bgColorInner = Color.Black;
        private readonly MTexture bgTexture;

        private readonly StreamParticle[] streams;
        private readonly VertexPositionColorTexture[] streamVerts;
        private readonly Particle[] particles;
        private readonly SpiralDebris[] spirals;
        private readonly VertexPositionColorTexture[] spiralVerts;

        private VirtualRenderTarget buffer;
        private Vector2 center;
        private Vector2 offset;
        private Vector2 shake;
        private float spinTime;
        private bool checkedFlag;

        private readonly Color[] colorsLerp;
        private readonly Color[,] colorsLerpBlack;
        private readonly Color[,] colorsLerpTransparent;

        // Cached count values - updated when StrengthMultiplier changes
        private int _cachedStreamCount;
        private int _cachedParticleCount;
        private int _cachedSpiralCount;
        private float _lastCachedStrength = -1f;

        public int StreamCount { get { UpdateCachedCounts(); return _cachedStreamCount; } }
        public int ParticleCount { get { UpdateCachedCounts(); return _cachedParticleCount; } }
        public int SpiralCount { get { UpdateCachedCounts(); return _cachedSpiralCount; } }

        private void UpdateCachedCounts()
        {
            if (_lastCachedStrength == StrengthMultiplier)
                return;
            _lastCachedStrength = StrengthMultiplier;
            float t = MathHelper.Clamp((StrengthMultiplier - 1f) / 6f, 0f, 1f);
            _cachedStreamCount = Math.Min(STREAM_MAX_COUNT, (int)MathHelper.Lerp(STREAM_MIN_COUNT, STREAM_MAX_COUNT, t));
            _cachedParticleCount = Math.Min(PARTICLE_MAX_COUNT, (int)MathHelper.Lerp(PARTICLE_MIN_COUNT, PARTICLE_MAX_COUNT, t));
            _cachedSpiralCount = Math.Min(SPIRAL_MAX_COUNT, (int)MathHelper.Lerp(SPIRAL_MIN_COUNT, SPIRAL_MAX_COUNT, t));
        }

        public RainbowBlackholeBg(BinaryPacker.Element data) : this()
        {
            // Parse optional parameters from map data
            if (data.HasAttr("alpha"))
                Alpha = data.AttrFloat("alpha", 1f);
            
            if (data.HasAttr("scale"))
                Scale = data.AttrFloat("scale", 1f);
            
            if (data.HasAttr("direction"))
                Direction = data.AttrFloat("direction", 1f);
            
            if (data.HasAttr("strength"))
            {
                string strengthStr = data.Attr("strength", "Mild");
                if (Enum.TryParse<Strengths>(strengthStr, true, out Strengths parsedStrength))
                {
                    strength = parsedStrength;
                    StrengthMultiplier = 1f + (float)strength;
                }
            }
            
            if (data.HasAttr("centerOffsetX") || data.HasAttr("centerOffsetY"))
            {
                CenterOffset = new Vector2(
                    data.AttrFloat("centerOffsetX", 0f),
                    data.AttrFloat("centerOffsetY", 0f)
                );
            }
            
            if (data.HasAttr("offsetX") || data.HasAttr("offsetY"))
            {
                OffsetOffset = new Vector2(
                    data.AttrFloat("offsetX", 0f),
                    data.AttrFloat("offsetY", 0f)
                );
            }
            
            // Wind influence settings
            if (data.HasAttr("windAffectsDirection"))
                WindAffectsDirection = data.AttrBool("windAffectsDirection", true);
            
            if (data.HasAttr("windAffectsPosition"))
                WindAffectsPosition = data.AttrBool("windAffectsPosition", true);
            
            if (data.HasAttr("windAffectsSpeed"))
                WindAffectsSpeed = data.AttrBool("windAffectsSpeed", true);
            
            if (data.HasAttr("windTransitionSpeed"))
                windTransitionSpeed = data.AttrFloat("windTransitionSpeed", 2f);
            
            // Animation mode settings
            if (data.HasAttr("animationMode"))
            {
                string modeStr = data.Attr("animationMode", "None");
                if (Enum.TryParse<AnimationMode>(modeStr, true, out AnimationMode parsedMode))
                {
                    animationMode = parsedMode;
                    LoadAnimationFrames();
                }
            }
            
            if (data.HasAttr("frameDelay"))
                frameDelay = data.AttrFloat("frameDelay", 0.08f);
            
            if (data.HasAttr("animationScale"))
                animationScale = data.AttrFloat("animationScale", 1f);
            
            if (data.HasAttr("animationLoops"))
                animationLoops = data.AttrBool("animationLoops", true);

            // Glitch / distortion controls.
            if (data.HasAttr("glitchAmount"))
                glitchAmount = MathHelper.Clamp(data.AttrFloat("glitchAmount", 0f), 0f, 1f);

            if (data.HasAttr("distortionAmount"))
                distortionAmount = MathHelper.Clamp(data.AttrFloat("distortionAmount", 0f), 0f, 1f);

            if (data.HasAttr("distortionFrequency"))
                distortionFrequency = Math.Max(0.1f, data.AttrFloat("distortionFrequency", 2f));

            if (data.HasAttr("chromaticAberration"))
                chromaticAberration = MathHelper.Clamp(data.AttrFloat("chromaticAberration", 0f), 0f, 1f);

            // Cache baseline values so runtime progression can blend from configured map values.
            baseAlpha = Alpha;
            baseGlitchAmount = glitchAmount;
            baseDistortionAmount = distortionAmount;
            baseChromaticAberration = chromaticAberration;
            
            UpdateStrengthColor();
        }

        /// <summary>
        /// Loads animation frames based on the current animation mode
        /// </summary>
        private void LoadAnimationFrames()
        {
            animationFrames = new List<MTexture>();
            string basePath = "bgs/maggy/19/say_goodbye/animination_bg/";
            
            switch (animationMode)
            {
                case AnimationMode.Soul:
                    animationFrames = GFX.Game.GetAtlasSubtextures(basePath + "soul/soul");
                    break;
                case AnimationMode.Zero:
                    animationFrames = GFX.Game.GetAtlasSubtextures(basePath + "zero/zero");
                    break;
                case AnimationMode.Void:
                    animationFrames = GFX.Game.GetAtlasSubtextures(basePath + "void/void");
                    break;
                case AnimationMode.None:
                default:
                    animationFrames = null;
                    break;
            }
            
            currentFrame = 0;
            frameTimer = 0f;
        }

        /// <summary>
        /// Sets the animation mode at runtime
        /// </summary>
        public void SetAnimationMode(AnimationMode mode)
        {
            if (animationMode != mode)
            {
                animationMode = mode;
                LoadAnimationFrames();
            }
        }

        /// <summary>
        /// Gets the current animation mode
        /// </summary>
        public AnimationMode GetAnimationMode() => animationMode;

        public RainbowBlackholeBg()
        {
            // Initialize arrays
            streams = new StreamParticle[STREAM_MAX_COUNT];
            streamVerts = new VertexPositionColorTexture[STREAM_MAX_COUNT * 6];
            particles = new Particle[PARTICLE_MAX_COUNT];
            spirals = new SpiralDebris[SPIRAL_MAX_COUNT];
            spiralVerts = new VertexPositionColorTexture[SPIRAL_MAX_COUNT * SPIRAL_SEGMENTS * 6];

            // Initialize chaos effects
            chaosOffsets = new Vector2[BG_STEPS];
            chaosPhases = new float[BG_STEPS];
            for (int i = 0; i < BG_STEPS; i++)
            {
                chaosOffsets[i] = Calc.AngleToVector(Calc.Random.NextFloat() * (float)Math.PI * 2f, Calc.Random.Range(10f, 30f));
                chaosPhases[i] = Calc.Random.NextFloat();
            }

            // Initialize rainbow color spectrum
            InitializeRainbowColors();
            
            bgTexture = GFX.Game["objects/temple/portal/portal"];
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("bgs/10/blackhole/particle");

            // Initialize streams with rainbow properties
            int vertIndex = 0;
            for (int i = 0; i < STREAM_MAX_COUNT; i++)
            {
                MTexture texture = Calc.Random.Choose(atlasSubtextures);
                streams[i].Texture = texture;
                streams[i].Percent = Calc.Random.NextFloat();
                streams[i].Speed = Calc.Random.Range(0.4f, 1.2f);
                streams[i].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (float)Math.PI * 2f, 1f);
                streams[i].ColorIndex = Calc.Random.Next(RAINBOW_COLOR_COUNT);
                streams[i].ColorPhase = Calc.Random.NextFloat();
                streams[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                streams[i].PulsePhase = Calc.Random.NextFloat() * (float)Math.PI * 2f;

                // Set up texture coordinates
                streamVerts[vertIndex].TextureCoordinate = new Vector2(texture.LeftUV, texture.TopUV);
                streamVerts[vertIndex + 1].TextureCoordinate = new Vector2(texture.RightUV, texture.TopUV);
                streamVerts[vertIndex + 2].TextureCoordinate = new Vector2(texture.RightUV, texture.BottomUV);
                streamVerts[vertIndex + 3].TextureCoordinate = new Vector2(texture.LeftUV, texture.TopUV);
                streamVerts[vertIndex + 4].TextureCoordinate = new Vector2(texture.RightUV, texture.BottomUV);
                streamVerts[vertIndex + 5].TextureCoordinate = new Vector2(texture.LeftUV, texture.BottomUV);
                vertIndex += 6;
            }

            // Initialize spiral debris with rainbow properties
            vertIndex = 0;
            for (int i = 0; i < SPIRAL_MAX_COUNT; i++)
            {
                MTexture texture = Calc.Random.Choose(atlasSubtextures);
                spirals[i].Percent = Calc.Random.NextFloat();
                spirals[i].Offset = Calc.Random.NextFloat((float)Math.PI * 2f);
                spirals[i].ColorIndex = Calc.Random.Next(RAINBOW_COLOR_COUNT);
                spirals[i].ColorPhase = Calc.Random.NextFloat();
                spirals[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                spirals[i].Twist = Calc.Random.Range(0.5f, 3f);

                for (int j = 0; j < SPIRAL_SEGMENTS; j++)
                {
                    float leftUV = MathHelper.Lerp(texture.LeftUV, texture.RightUV, (float)j / SPIRAL_SEGMENTS);
                    float rightUV = MathHelper.Lerp(texture.LeftUV, texture.RightUV, (float)(j + 1) / SPIRAL_SEGMENTS);
                    
                    spiralVerts[vertIndex].TextureCoordinate = new Vector2(leftUV, texture.TopUV);
                    spiralVerts[vertIndex + 1].TextureCoordinate = new Vector2(rightUV, texture.TopUV);
                    spiralVerts[vertIndex + 2].TextureCoordinate = new Vector2(rightUV, texture.BottomUV);
                    spiralVerts[vertIndex + 3].TextureCoordinate = new Vector2(leftUV, texture.TopUV);
                    spiralVerts[vertIndex + 4].TextureCoordinate = new Vector2(rightUV, texture.BottomUV);
                    spiralVerts[vertIndex + 5].TextureCoordinate = new Vector2(leftUV, texture.BottomUV);
                    vertIndex += 6;
                }
            }

            // Initialize particles with rainbow properties
            for (int i = 0; i < PARTICLE_MAX_COUNT; i++)
            {
                particles[i].Percent = Calc.Random.NextFloat();
                particles[i].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (float)Math.PI * 2f, 1f);
                particles[i].ColorIndex = Calc.Random.Next(RAINBOW_COLOR_COUNT);
                particles[i].ColorPhase = Calc.Random.NextFloat();
                particles[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                particles[i].Intensity = Calc.Random.Range(0.8f, 2.5f);
                particles[i].SizeMultiplier = Calc.Random.Range(1f, 3f);
            }

            center = new Vector2(160f, 90f); // True screen center
            offset = Vector2.Zero;
            colorsLerp = new Color[RAINBOW_COLOR_COUNT];
            colorsLerpBlack = new Color[RAINBOW_COLOR_COUNT, COLOR_STEPS];
            colorsLerpTransparent = new Color[RAINBOW_COLOR_COUNT, COLOR_STEPS];

            // Defaults for cases where no map data constructor is used.
            baseAlpha = Alpha;
            baseGlitchAmount = glitchAmount;
            baseDistortionAmount = distortionAmount;
            baseChromaticAberration = chromaticAberration;
        }

        private void InitializeRainbowColors()
        {
            rainbowColors = ColorUtils.GenerateRainbowPalette(RAINBOW_COLOR_COUNT);
        }

        private static Color HSVToRGB(float h, float s, float v)
        {
            return ColorUtils.FromHsv(h, s, v);
        }

        private Color GetStrengthColor()
        {
            return strength switch
            {
                Strengths.Mild => new Color(0, 100, 255),      // Blue
                Strengths.Medium => new Color(150, 50, 200),    // Purple
                Strengths.High => new Color(255, 105, 180),     // Pink
                Strengths.Wild => new Color(255, 0, 255),       // Magenta
                Strengths.Insane => new Color(255, 0, 0),       // Red
                Strengths.RainbowChaos => new Color(255, 0, 255), // Magenta (fallback)
                Strengths.Cosmic => Color.White,                // White for rainbow base
                _ => new Color(0, 100, 255)                     // Default blue
            };
        }

        private Color GetRainbowColor(float offset, float intensity = 1f)
        {
            // For Cosmic strength, use full rainbow effect
            if (strength == Strengths.Cosmic)
            {
                float phase = (rainbowTime * rainbowSpeed + offset) % ((float)Math.PI * 2f);
                float hue = (phase / ((float)Math.PI * 2f)) % 1f;
                
                // Add pulsing effect
                float pulseMult = (float)Math.Sin(pulseTime * 4f + offset) * 0.3f + 0.7f;
                
                return HSVToRGB(hue, 1f, intensity * pulseMult);
            }
            
            // For other strengths, use strength-based color with variation
            Color baseColor = GetStrengthColor();
            float pulse = (float)Math.Sin(pulseTime * 4f + offset) * 0.3f + 0.7f;
            return baseColor * intensity * pulse;
        }

        private Color GetChaosRainbowColor(float offset, float chaos, float intensity = 1f)
        {
            // For Cosmic strength, use full rainbow chaos effect
            if (strength == Strengths.Cosmic)
            {
                float basePhase = (rainbowTime * rainbowSpeed + offset) % ((float)Math.PI * 2f);
                float chaosPhase = (chaosTime * 2f + chaos) % ((float)Math.PI * 2f);
                
                float hue1 = (basePhase / ((float)Math.PI * 2f)) % 1f;
                float hue2 = ((basePhase + chaosPhase) / ((float)Math.PI * 2f)) % 1f;
                
                Color rainbowColor1 = HSVToRGB(hue1, 1f, intensity);
                Color rainbowColor2 = HSVToRGB(hue2, 1f, intensity * 0.7f);
                
                float rainbowMix = (float)Math.Sin(warpTime * 3f + offset) * 0.5f + 0.5f;
                return Color.Lerp(rainbowColor1, rainbowColor2, rainbowMix);
            }
            
            // For other strengths, use strength-based color with chaos variation
            Color baseColor = GetStrengthColor();
            
            // Create slight variations in brightness and saturation
            float variation1 = (float)Math.Sin(rainbowTime * rainbowSpeed + offset) * 0.2f + 1f;
            float variation2 = (float)Math.Sin(chaosTime * 2f + chaos) * 0.15f + 0.85f;
            
            Color color1 = baseColor * intensity * variation1;
            Color color2 = baseColor * intensity * 0.7f * variation2;
            
            float mixFactor = (float)Math.Sin(warpTime * 3f + offset) * 0.5f + 0.5f;
            return Color.Lerp(color1, color2, mixFactor);
        }

        public void SnapStrength(Level level, Strengths strength)
        {
            this.strength = strength;
            StrengthMultiplier = 1f + (float)strength;
            level.Session.SetCounter(STRENGTH_FLAG, (int)strength);
            UpdateStrengthColor();
        }

        public void NextStrength(Level level, Strengths strength)
        {
            this.strength = strength;
            level.Session.SetCounter(STRENGTH_FLAG, (int)strength);
            UpdateStrengthColor();
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            
            Level level = scene as Level;
            
            if (!checkedFlag)
            {
                int counter = level.Session.GetCounter(STRENGTH_FLAG);
                if (counter >= 0)
                {
                    SnapStrength(level, (Strengths)counter);
                }
                checkedFlag = true;
            }

            if (!Visible)
                return;

            // Update animation frame if in animation mode
            if (animationMode != AnimationMode.None && animationFrames != null && animationFrames.Count > 0)
            {
                frameTimer += Engine.DeltaTime;
                if (frameTimer >= frameDelay)
                {
                    frameTimer -= frameDelay;
                    currentFrame++;
                    if (currentFrame >= animationFrames.Count)
                    {
                        currentFrame = animationLoops ? 0 : animationFrames.Count - 1;
                    }
                }
            }

            // Update wind system
            UpdateWindSystem(level);

            // Update time variables for crazy effects
            rainbowTime += Engine.DeltaTime;
            pulseTime += Engine.DeltaTime * (1f + StrengthMultiplier);
            warpTime += Engine.DeltaTime * (0.7f + StrengthMultiplier * 0.5f);
            chaosTime += Engine.DeltaTime * (0.5f + StrengthMultiplier * 0.3f);

            StrengthMultiplier = Calc.Approach(StrengthMultiplier, 1f + (float)strength, Engine.DeltaTime * 0.1f);

            // Update chaos offsets
            for (int i = 0; i < BG_STEPS; i++)
            {
                chaosPhases[i] += Engine.DeltaTime * (1f + StrengthMultiplier);
                chaosOffsets[i] = Calc.AngleToVector(chaosPhases[i], 
                    (float)Math.Sin(chaosTime + i) * 20f * StrengthMultiplier);
            }

            // Update color interpolation
            if (scene.OnInterval(0.03f))
            {
                for (int i = 0; i < RAINBOW_COLOR_COUNT; i++)
                {
                    colorsLerp[i] = GetRainbowColor(i * 0.5f, FadeAlphaMultiplier);
                    for (int j = 0; j < COLOR_STEPS; j++)
                    {
                        float fade = (float)j / (COLOR_STEPS - 1);
                        colorsLerpBlack[i, j] = Color.Lerp(colorsLerp[i], Color.Black, fade);
                        colorsLerpTransparent[i, j] = Color.Lerp(colorsLerp[i], Color.Transparent, fade);
                    }
                }
            }

            // Calculate effective direction based on wind
            float effectiveDirection = WindAffectsDirection ? windDirection : Direction;
            float effectiveSpeedMult = WindAffectsSpeed ? windSpeedMultiplier : 1f;
            
            float speedMultiplier = (1f + (StrengthMultiplier - 1f) * 1.5f) * effectiveSpeedMult;
            int streamCount = StreamCount;

            // Update streams with wind-based direction
            int vertIndex = 0;
            for (int i = 0; i < streamCount; i++)
            {
                streams[i].Percent += streams[i].Speed * Engine.DeltaTime * speedMultiplier * effectiveDirection;
                streams[i].ColorPhase += Engine.DeltaTime * rainbowSpeed;
                streams[i].PulsePhase += Engine.DeltaTime * 3f;

                if (streams[i].Percent >= 1f && effectiveDirection > 0f)
                {
                    streams[i].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (float)Math.PI * 2f, 1f);
                    streams[i].Percent -= 1f;
                    streams[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                }
                else if (streams[i].Percent < 0f && effectiveDirection < 0f)
                {
                    streams[i].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (float)Math.PI * 2f, 1f);
                    streams[i].Percent += 1f;
                    streams[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                }

                float percent = streams[i].Percent;
                float alpha1 = Ease.CubeIn(Calc.ClampedMap(percent, 0f, 0.7f));
                float alpha2 = Ease.CubeIn(Calc.ClampedMap(percent, 0.3f, 1f));

                Vector2 normal = streams[i].Normal;
                Vector2 perpendicular = normal.Perpendicular();

                // Add warp effect influenced by wind
                float windWarp = WindAffectsPosition ? windInfluence * 0.5f : 0f;
                float warp = (float)Math.Sin(warpTime + streams[i].RainbowOffset) * 80f * StrengthMultiplier * (1f + windWarp);
                Vector2 warpOffset = perpendicular * warp;

                Vector2 pos1 = normal * 40f + normal * (1f - alpha1) * (500f + StrengthMultiplier * 200f) + warpOffset;
                Vector2 pos2 = normal * 40f + normal * (1f - alpha2) * (700f + StrengthMultiplier * 300f) + warpOffset;

                float width1 = (1f - alpha1) * (12f + StrengthMultiplier * 8f);
                float width2 = (1f - alpha2) * (12f + StrengthMultiplier * 8f);

                Color color1 = GetChaosRainbowColor(streams[i].RainbowOffset + streams[i].ColorPhase, i * 0.1f, 
                    (1f - alpha1) * (0.8f + StrengthMultiplier * 0.4f));
                Color color2 = GetChaosRainbowColor(streams[i].RainbowOffset + streams[i].ColorPhase, i * 0.1f, 
                    (1f - alpha2) * (0.8f + StrengthMultiplier * 0.4f));

                // Pulse effect
                float pulse = (float)Math.Sin(streams[i].PulsePhase) * 0.3f + 1f;
                color1 *= pulse;
                color2 *= pulse;

                Vector2 a = pos1 - perpendicular * width1;
                Vector2 b = pos1 + perpendicular * width1;
                Vector2 c = pos2 + perpendicular * width2;
                Vector2 d = pos2 - perpendicular * width2;

                AssignVertColors(streamVerts, vertIndex, ref color1, ref color1, ref color2, ref color2);
                AssignVertPosition(streamVerts, vertIndex, ref a, ref b, ref c, ref d);
                vertIndex += 6;
            }

            // Update particles with wind-based direction
            float particleSpeed = StrengthMultiplier * 0.4f * effectiveSpeedMult;
            int particleCount = ParticleCount;
            for (int i = 0; i < particleCount; i++)
            {
                particles[i].Percent += Engine.DeltaTime * particleSpeed * effectiveDirection;
                particles[i].ColorPhase += Engine.DeltaTime * rainbowSpeed * 1.5f;

                if (particles[i].Percent >= 1f && effectiveDirection > 0f)
                {
                    particles[i].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (float)Math.PI * 2f, 1f);
                    particles[i].Percent -= 1f;
                    particles[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                }
                else if (particles[i].Percent < 0f && effectiveDirection < 0f)
                {
                    particles[i].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (float)Math.PI * 2f, 1f);
                    particles[i].Percent += 1f;
                    particles[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                }
            }

            // Update spirals with wind-based direction
            float spiralSpeed = (0.3f + (StrengthMultiplier - 1f) * 0.2f) * effectiveSpeedMult;
            int spiralCount = SpiralCount;
            vertIndex = 0;
            for (int i = 0; i < spiralCount; i++)
            {
                spirals[i].Percent += streams[i % streamCount].Speed * Engine.DeltaTime * spiralSpeed * effectiveDirection;
                spirals[i].ColorPhase += Engine.DeltaTime * rainbowSpeed * 0.8f;

                if (spirals[i].Percent >= 1f && effectiveDirection > 0f)
                {
                    spirals[i].Offset = Calc.Random.NextFloat((float)Math.PI * 2f);
                    spirals[i].Percent -= 1f;
                    spirals[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                }
                else if (spirals[i].Percent < 0f && effectiveDirection < 0f)
                {
                    spirals[i].Offset = Calc.Random.NextFloat((float)Math.PI * 2f);
                    spirals[i].Percent += 1f;
                    spirals[i].RainbowOffset = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                }

                float percent = spirals[i].Percent;
                float spiralOffset = spirals[i].Offset;

                for (int j = 0; j < SPIRAL_SEGMENTS; j++)
                {
                    float segmentPercent1 = 1f - MathHelper.Lerp(percent * 0.8f, percent, (float)j / SPIRAL_SEGMENTS);
                    float segmentPercent2 = 1f - MathHelper.Lerp(percent * 0.8f, percent, (float)(j + 1) / SPIRAL_SEGMENTS);

                    Vector2 dir1 = Calc.AngleToVector(segmentPercent1 * (30f + j * 0.3f) * spirals[i].Twist + spiralOffset, 1f);
                    Vector2 dir2 = Calc.AngleToVector(segmentPercent2 * (30f + (j + 1) * 0.3f) * spirals[i].Twist + spiralOffset, 1f);

                    Vector2 pos1 = dir1 * segmentPercent1 * (600f + StrengthMultiplier * 200f);
                    Vector2 pos2 = dir2 * segmentPercent2 * (600f + StrengthMultiplier * 200f);

                    float width1 = segmentPercent1 * (6f + StrengthMultiplier * 8f);
                    float width2 = segmentPercent2 * (6f + StrengthMultiplier * 8f);

                    Color color1 = GetChaosRainbowColor(spirals[i].RainbowOffset + spirals[i].ColorPhase + j * 0.2f, 
                        i * 0.15f + j * 0.05f, segmentPercent1 * (0.9f + StrengthMultiplier * 0.3f));
                    Color color2 = GetChaosRainbowColor(spirals[i].RainbowOffset + spirals[i].ColorPhase + (j + 1) * 0.2f, 
                        i * 0.15f + (j + 1) * 0.05f, segmentPercent2 * (0.9f + StrengthMultiplier * 0.3f));

                    Vector2 a = pos1 + dir1 * width1;
                    Vector2 b = pos2 + dir2 * width2;
                    Vector2 c = pos2 - dir2 * width2;
                    Vector2 d = pos1 - dir1 * width1;

                    AssignVertColors(spiralVerts, vertIndex, ref color1, ref color2, ref color2, ref color1);
                    AssignVertPosition(spiralVerts, vertIndex, ref a, ref b, ref c, ref d);
                    vertIndex += 6;
                }
            }

            // Update center and offset with wind-based positioning
            Vector2 windPosOffset = WindAffectsPosition ? windOffset : Vector2.Zero;
            Vector2 targetCenter = new Vector2(160f, 90f) + windPosOffset * 0.1f + CenterOffset;
            
            // Add subtle chaos to center (reduced for stability)
            Vector2 chaosCenter = Vector2.Zero;
            for (int i = 0; i < Math.Min(3, chaosOffsets.Length); i++)
            {
                chaosCenter += chaosOffsets[i] * (float)Math.Sin(chaosTime + i) * StrengthMultiplier * 0.05f;
            }
            targetCenter += chaosCenter;

            center += (targetCenter - center) * (1f - (float)Math.Pow(0.001, Engine.DeltaTime));

            Vector2 targetOffset = -windPosOffset * 0.3f + OffsetOffset;
            offset += (targetOffset - offset) * (1f - (float)Math.Pow(0.01, Engine.DeltaTime));

            if (scene.OnInterval(0.02f))
            {
                shake = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 
                    6f * (StrengthMultiplier - 1f) * (1f + (float)Math.Sin(pulseTime * 2f) * 0.5f));
            }

            // Update spin time with wind direction influence
            float spinDirection = WindAffectsDirection ? windDirection : Direction;
            spinTime += (3f + StrengthMultiplier * 2f) * Engine.DeltaTime * spinDirection;
        }

        /// <summary>
        /// Updates the wind system based on level wind
        /// </summary>
        private void UpdateWindSystem(Level level)
        {
            targetWind = level.Wind;
            
            // Smoothly interpolate current wind
            currentWind = Vector2.Lerp(currentWind, targetWind, Engine.DeltaTime * 3f);
            
            // Calculate wind influence (0 to 1 based on wind magnitude)
            float windMagnitude = currentWind.Length();
            windInfluence = MathHelper.Clamp(windMagnitude / 800f, 0f, 1f); // 800 is strong wind
            
            // Determine target direction based on horizontal wind component
            // Left wind (negative X) = clockwise (1), Right wind (positive X) = counter-clockwise (-1)
            if (Math.Abs(currentWind.X) > 50f) // Threshold to avoid flickering
            {
                targetWindDirection = currentWind.X < 0f ? 1f : -1f;
            }
            else if (Math.Abs(currentWind.Y) > 50f)
            {
                // Vertical wind: up = clockwise, down = counter-clockwise
                targetWindDirection = currentWind.Y < 0f ? 1f : -1f;
            }
            else
            {
                // No significant wind, use default direction
                targetWindDirection = Direction;
            }
            
            // Smoothly transition the wind direction
            windDirection = Calc.Approach(windDirection, targetWindDirection, Engine.DeltaTime * windTransitionSpeed);
            
            // Update wind offset for position effects
            windOffset = currentWind;
            
            // Update wind speed multiplier (stronger wind = faster spinning)
            windSpeedMultiplier = 1f + windInfluence * 0.5f;
        }

        private static void AssignVertColors(VertexPositionColorTexture[] verts, int v, ref Color a, ref Color b, ref Color c, ref Color d)
        {
            verts[v].Color = a;
            verts[v + 1].Color = b;
            verts[v + 2].Color = c;
            verts[v + 3].Color = a;
            verts[v + 4].Color = c;
            verts[v + 5].Color = d;
        }

        private static void AssignVertPosition(VertexPositionColorTexture[] verts, int v, ref Vector2 a, ref Vector2 b, ref Vector2 c, ref Vector2 d)
        {
            verts[v].Position = new Vector3(a, 0f);
            verts[v + 1].Position = new Vector3(b, 0f);
            verts[v + 2].Position = new Vector3(c, 0f);
            verts[v + 3].Position = new Vector3(a, 0f);
            verts[v + 4].Position = new Vector3(c, 0f);
            verts[v + 5].Position = new Vector3(d, 0f);
        }

        /// <summary>
        /// Rainbow blackhole should stay as a background visual only.
        /// If a map accidentally places it in foreground, skip rendering there.
        /// </summary>
        private bool ShouldRenderInCurrentContainer(Scene scene)
        {
            if (scene is not Level level)
                return true;

            return level.Foreground?.Backdrops == null || !level.Foreground.Backdrops.Contains(this);
        }

        public override void BeforeRender(Scene scene)
        {
            if (!ShouldRenderInCurrentContainer(scene))
                return;

            // Use normal screen size for cover effect
            int bufferWidth = 320;
            int bufferHeight = 180;
            
            if (buffer == null || buffer.IsDisposed)
            {
                buffer = VirtualContent.CreateRenderTarget("Rainbow Black Hole", bufferWidth, bufferHeight);
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(bgColorInner);

            Draw.SpriteBatch.Begin();

            // Fixed center point for consistent rendering
            Vector2 fixedCenter = new Vector2(160f, 90f);
            
            // Get effective direction for rendering (based on wind if enabled)
            float renderDirection = WindAffectsDirection ? windDirection : Direction;
            
            // Render background with rainbow spiral effect - dark center, bright outer rings
            for (int i = 0; i < BG_STEPS; i++)
            {
                float progress = (float)i / BG_STEPS;
                
                // Multiple rainbow layers with better alpha blending
                // Center is darker, outer rings are brighter
                float alphaMult = Ease.CubeOut(progress);
                Color rainbowColor = GetChaosRainbowColor(progress * 8f + spinTime * 0.5f, i * 0.3f, 
                    alphaMult * (0.6f + StrengthMultiplier * 0.4f));
                
                // Reduced scale for smaller blackhole - from 25f to 8f base scale
                float scale = Ease.SineOut(progress) * (8f + StrengthMultiplier * 3f);
                
                // Ensure minimum scale for center to create the black hole effect
                if (progress < 0.15f)
                {
                    scale = MathHelper.Lerp(0.3f, 1.5f, progress / 0.15f);
                    rainbowColor *= Ease.CubeIn(progress / 0.15f) * 0.3f; // Make center very dark
                }
                
                // Consistent rotation based on wind direction - each layer rotates at different speed
                float baseRotation = spinTime * renderDirection * (1f + StrengthMultiplier * 0.3f);
                float layerRotation = baseRotation + progress * (float)Math.PI * 4f; // Spiral effect
                
                // Very subtle position offset to maintain center stability
                Vector2 renderPos = fixedCenter + shake * (1f - progress) * 0.3f;
                
                bgTexture.DrawCentered(renderPos, rainbowColor, scale, layerRotation);
            }

            Draw.SpriteBatch.End();

            // Render spirals at fixed center with wind-based rotation
            if (SpiralCount > 0)
            {
                Engine.Instance.GraphicsDevice.Textures[0] = GFX.Game.Sources[0].Texture_Safe;
                // Apply rotation transform for spiraling effect using wind direction
                float spiralRotation = spinTime * renderDirection * 0.5f;
                Matrix spiralTransform = Matrix.CreateRotationZ(spiralRotation) * Matrix.CreateTranslation(160f, 90f, 0f);
                GFX.DrawVertices(spiralTransform, spiralVerts, SpiralCount * SPIRAL_SEGMENTS * 6, GFX.FxTexture);
            }

            // Render streams at fixed center with wind-based rotation
            if (StreamCount > 0)
            {
                Engine.Instance.GraphicsDevice.Textures[0] = GFX.Game.Sources[0].Texture_Safe;
                float streamRotation = spinTime * renderDirection * 0.3f;
                Matrix streamTransform = Matrix.CreateRotationZ(streamRotation) * Matrix.CreateTranslation(160f, 90f, 0f);
                GFX.DrawVertices(streamTransform, streamVerts, StreamCount * 6, GFX.FxTexture);
            }

            Draw.SpriteBatch.Begin();

            // Render crazy rainbow particles at fixed center with wind-based rotation
            Vector2 particleCenter = new Vector2(160f, 90f);
            float particleRotationAngle = spinTime * renderDirection * 0.2f;
            int particleCount = ParticleCount;
            for (int i = 0; i < particleCount; i++)
            {
                float alpha = Ease.CubeIn(Calc.Clamp(particles[i].Percent, 0f, 1f));
                // Rotate the particle normal around the center based on wind direction
                Vector2 rotatedNormal = Calc.AngleToVector(Calc.Angle(Vector2.Zero, particles[i].Normal) + particleRotationAngle, 1f);
                Vector2 particlePos = particleCenter + rotatedNormal * Calc.ClampedMap(alpha, 1f, 0f, 20f, 600f + StrengthMultiplier * 200f);
                
                Color particleColor = GetChaosRainbowColor(particles[i].RainbowOffset + particles[i].ColorPhase, 
                    i * 0.01f, particles[i].Intensity * (1f - alpha));
                
                float size = (1f + (1f - alpha) * 2f) * particles[i].SizeMultiplier * (1f + StrengthMultiplier * 0.5f);
                
                // Add pulsing effect to particles
                float pulse = (float)Math.Sin(pulseTime * 5f + particles[i].RainbowOffset) * 0.4f + 1f;
                size *= pulse;
                particleColor *= pulse;
                
                Draw.Rect(particlePos - new Vector2(size, size) / 2f, size, size, particleColor);
            }

            Draw.SpriteBatch.End();
        }

        public override void Ended(Scene scene)
        {
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
        }

        public override void Render(Scene scene)
        {
            if (!ShouldRenderInCurrentContainer(scene))
                return;

            Vector2 screenCenter = new Vector2(160f, 90f);
            float baseAlpha = FadeAlphaMultiplier * Alpha;

            // Compute lightweight procedural distortion from blackhole motion timers.
            float distortionPulse = distortionAmount * (0.5f + 0.5f * (float)Math.Sin(warpTime * distortionFrequency));
            Vector2 distortionOffset = new Vector2(
                (float)Math.Sin(warpTime * (distortionFrequency + 0.7f) + chaosTime) * 2.5f,
                (float)Math.Cos(warpTime * (distortionFrequency + 1.3f) - chaosTime * 0.8f) * 2.0f
            ) * distortionPulse;

            Vector2 glitchJitter = Vector2.Zero;
            if (glitchAmount > 0f)
            {
                // Sparse, short jitter bursts to avoid constant screen shake.
                float burst = (float)Math.Sin((rainbowTime + chaosTime * 0.4f) * 24f);
                if (burst > 0.65f)
                {
                    glitchJitter = new Vector2(
                        Calc.Random.Range(-1.5f, 1.5f),
                        Calc.Random.Range(-1.0f, 1.0f)
                    ) * glitchAmount;
                }
            }

            Vector2 finalCenter = screenCenter + distortionOffset + glitchJitter;

            // If in animation mode, render the animated background
            if (animationMode != AnimationMode.None && animationFrames != null && animationFrames.Count > 0)
            {
                MTexture frame = animationFrames[currentFrame];
                Vector2 frameOrigin = new Vector2(frame.Width / 2f, frame.Height / 2f);
                
                // Apply subtle breathing effect and animation scale
                float breathScale = animationScale * Scale * (1f + (float)Math.Sin(warpTime * 0.5f) * 0.01f * StrengthMultiplier);
                
                // Get color tint based on strength
                Color tint = Color.White * baseAlpha;
                if (strength == Strengths.Cosmic)
                {
                    // Add rainbow tint for cosmic strength
                    tint = GetRainbowColor(rainbowTime, 1f) * baseAlpha;
                }

                float chroma = chromaticAberration * (0.8f + glitchAmount * 1.2f);
                if (chroma > 0.001f)
                {
                    Vector2 chromaOffset = new Vector2(1.2f + distortionPulse * 2f, 0.7f) * chroma;
                    frame.DrawCentered(finalCenter - chromaOffset, new Color(255, 90, 90, 180) * baseAlpha, breathScale);
                    frame.DrawCentered(finalCenter + chromaOffset, new Color(90, 255, 255, 180) * baseAlpha, breathScale);
                }
                
                frame.DrawCentered(finalCenter, tint, breathScale);
                return;
            }
            
            // Default procedural rendering
            if (buffer != null && !buffer.IsDisposed)
            {
                Vector2 bufferCenter = new Vector2(buffer.Width / 2f, buffer.Height / 2f);
                
                // Subtle breathing scale effect
                float warpScale = Scale * 1.0f + (float)Math.Sin(warpTime * 0.5f) * 0.02f * StrengthMultiplier;

                float chroma = chromaticAberration * (0.8f + glitchAmount * 1.2f);
                if (chroma > 0.001f)
                {
                    Vector2 chromaOffset = new Vector2(1.4f + distortionPulse * 2.2f, 0.8f) * chroma;
                    Draw.SpriteBatch.Draw((RenderTarget2D)buffer, finalCenter - chromaOffset, buffer.Bounds,
                        new Color(255, 95, 95, 170) * baseAlpha, 0f, bufferCenter, warpScale, SpriteEffects.None, 0f);
                    Draw.SpriteBatch.Draw((RenderTarget2D)buffer, finalCenter + chromaOffset, buffer.Bounds,
                        new Color(95, 255, 255, 170) * baseAlpha, 0f, bufferCenter, warpScale, SpriteEffects.None, 0f);
                }
                
                // No rotation on final render to keep it stable - rotation is handled in layers
                Draw.SpriteBatch.Draw((RenderTarget2D)buffer, finalCenter, buffer.Bounds,
                    Color.White * baseAlpha, 0f, bufferCenter, warpScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Updates the cached strength color based on current strength
        /// </summary>
        private void UpdateStrengthColor()
        {
            strengthColor = GetStrengthColor();
        }

        /// <summary>
        /// Sets the strength level of the rainbow blackhole effect
        /// </summary>
        /// <param name="newStrength">The new strength level</param>
        public void SetStrength(Strengths newStrength)
        {
            strength = newStrength;
            
            // Update the strength multiplier based on the enum value
            switch (newStrength)
            {
                case Strengths.Mild:
                    StrengthMultiplier = 1f;
                    break;
                case Strengths.Medium:
                    StrengthMultiplier = 2f;
                    break;
                case Strengths.High:
                    StrengthMultiplier = 3f;
                    break;
                case Strengths.Wild:
                    StrengthMultiplier = 4f;
                    break;
                case Strengths.Insane:
                    StrengthMultiplier = 5f;
                    break;
                case Strengths.RainbowChaos:
                    StrengthMultiplier = 6f;
                    break;
                case Strengths.Cosmic:
                    StrengthMultiplier = 7f;
                    break;
            }
            
            UpdateStrengthColor();
        }

        /// <summary>
        /// Applies progression from broken power generators.
        /// 0 = full blackhole pressure, 1 = weakened blackhole with clearer Zero/Darkmatter visuals.
        /// </summary>
        /// <param name="progress">Progress value in range [0, 1].</param>
        public void SetGeneratorBreakProgress(float progress)
        {
            float p = MathHelper.Clamp(progress, 0f, 1f);

            // Step down strength tiers as generators are destroyed.
            Strengths targetStrength = p switch
            {
                < 0.15f => Strengths.Cosmic,
                < 0.35f => Strengths.Insane,
                < 0.55f => Strengths.High,
                < 0.75f => Strengths.Medium,
                _ => Strengths.Mild
            };

            SetStrength(targetStrength);

            // Reduce blackhole opacity and distortion so the background visual layer becomes readable.
            float minVisibleAlpha = Math.Max(0.22f, baseAlpha * 0.3f);
            Alpha = MathHelper.Lerp(baseAlpha, minVisibleAlpha, p);
            glitchAmount = MathHelper.Lerp(baseGlitchAmount, 0.02f, p);
            distortionAmount = MathHelper.Lerp(baseDistortionAmount, 0.04f, p);
            chromaticAberration = MathHelper.Lerp(baseChromaticAberration, 0.03f, p);
        }

        /// <summary>
        /// Gets the current strength level
        /// </summary>
        public Strengths GetStrength()
        {
            return strength;
        }

        /// <summary>
        /// Sets how fast the direction transitions when wind changes
        /// </summary>
        /// <param name="speed">Transition speed (default is 2.0)</param>
        public void SetWindTransitionSpeed(float speed)
        {
            windTransitionSpeed = Math.Max(0.1f, speed);
        }

        /// <summary>
        /// Gets the current wind direction value (-1 to 1)
        /// </summary>
        public float GetWindDirection()
        {
            return windDirection;
        }

        /// <summary>
        /// Gets the current wind influence (0 to 1)
        /// </summary>
        public float GetWindInfluence()
        {
            return windInfluence;
        }

        /// <summary>
        /// Manually sets the spin direction (overrides wind if WindAffectsDirection is false)
        /// </summary>
        /// <param name="direction">1 for clockwise, -1 for counter-clockwise</param>
        public void SetDirection(float direction)
        {
            Direction = MathHelper.Clamp(direction, -1f, 1f);
        }

        /// <summary>
        /// Forces an immediate direction change (bypasses smooth transition)
        /// </summary>
        /// <param name="direction">1 for clockwise, -1 for counter-clockwise</param>
        public void ForceDirection(float direction)
        {
            windDirection = MathHelper.Clamp(direction, -1f, 1f);
            targetWindDirection = windDirection;
            Direction = windDirection;
        }

        /// <summary>
        /// Configures all wind influence settings at once
        /// </summary>
        /// <param name="affectsDirection">Whether wind changes spin direction</param>
        /// <param name="affectsPosition">Whether wind offsets the blackhole position</param>
        /// <param name="affectsSpeed">Whether wind affects particle/stream speed</param>
        public void ConfigureWindInfluence(bool affectsDirection, bool affectsPosition, bool affectsSpeed)
        {
            WindAffectsDirection = affectsDirection;
            WindAffectsPosition = affectsPosition;
            WindAffectsSpeed = affectsSpeed;
        }
    }
}



