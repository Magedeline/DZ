namespace Celeste.Effects
{
    /// <summary>
    /// Popstar animated background effect - displays a looping animated background
    /// featuring Popstar (Kirby's home planet) with customizable speed and effects.
    /// </summary>
    [CustomBackdrop("DZ/PopstarBg")]
    [HotReloadable]
    public class PopstarBg : Backdrop
    {
        /// <summary>
        /// Visual style modes for the Popstar background
        /// </summary>
        public enum StyleMode
        {
            /// <summary>Normal animated display</summary>
            Normal,
            /// <summary>Dreamy ethereal effect with glow</summary>
            Dreamy,
            /// <summary>Sunset/twilight color grading</summary>
            Sunset,
            /// <summary>Night mode with stars overlay</summary>
            Night,
            /// <summary>Rainbow color cycling overlay</summary>
            Rainbow
        }

        // Animation system
        private List<MTexture> frames;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private float frameDelay = 0.08f; // ~12.5 FPS by default
        private bool loops = true;
        private bool pingPong = false;
        private int pingPongDirection = 1;

        // Visual properties
        private float scale = 1f;
        private float alpha = 1f;
        private float rotation = 0f;
        private float rotationSpeed = 0f;
        private StyleMode style = StyleMode.Normal;
        private Color tintColor = Color.White;
        
        // Position and scrolling
        private Vector2 position = Vector2.Zero;
        private Vector2 scrollSpeed = Vector2.Zero;
        private Vector2 parallaxOffset = Vector2.Zero;
        
        // Effects
        private float pulseAmount = 0f;
        private float pulseSpeed = 1f;
        private float pulseTimer = 0f;
        private float glowIntensity = 0f;
        private float rainbowSpeed = 1f;
        private float rainbowTimer = 0f;
        
        // Stars overlay for night mode
        private struct Star
        {
            public Vector2 Position;
            public float Alpha;
            public float TwinkleSpeed;
            public float TwinklePhase;
            public float Size;
        }
        private Star[] stars;
        private const int STAR_COUNT = 100;

        public PopstarBg(BinaryPacker.Element data) : this()
        {
            // Parse optional parameters from map data
            if (data.HasAttr("frameDelay"))
                frameDelay = Math.Max(0.01f, data.AttrFloat("frameDelay", 0.08f));
            
            if (data.HasAttr("scale"))
                scale = data.AttrFloat("scale", 1f);
            
            if (data.HasAttr("alpha"))
                alpha = MathHelper.Clamp(data.AttrFloat("alpha", 1f), 0f, 1f);
            
            if (data.HasAttr("loops"))
                loops = data.AttrBool("loops", true);
            
            if (data.HasAttr("pingPong"))
                pingPong = data.AttrBool("pingPong", false);
            
            if (data.HasAttr("rotationSpeed"))
                rotationSpeed = data.AttrFloat("rotationSpeed", 0f);
            
            if (data.HasAttr("style"))
            {
                string styleStr = data.Attr("style", "Normal");
                if (Enum.TryParse<StyleMode>(styleStr, true, out StyleMode parsedStyle))
                {
                    style = parsedStyle;
                }
            }
            
            if (data.HasAttr("tintColor"))
            {
                string colorStr = data.Attr("tintColor", "FFFFFF");
                tintColor = Calc.HexToColor(colorStr);
            }
            
            if (data.HasAttr("scrollSpeedX") || data.HasAttr("scrollSpeedY"))
            {
                scrollSpeed = new Vector2(
                    data.AttrFloat("scrollSpeedX", 0f),
                    data.AttrFloat("scrollSpeedY", 0f)
                );
            }
            
            if (data.HasAttr("pulseAmount"))
                pulseAmount = MathHelper.Clamp(data.AttrFloat("pulseAmount", 0f), 0f, 1f);
            
            if (data.HasAttr("pulseSpeed"))
                pulseSpeed = data.AttrFloat("pulseSpeed", 1f);
            
            if (data.HasAttr("glowIntensity"))
                glowIntensity = MathHelper.Clamp(data.AttrFloat("glowIntensity", 0f), 0f, 2f);
            
            if (data.HasAttr("rainbowSpeed"))
                rainbowSpeed = data.AttrFloat("rainbowSpeed", 1f);
            
            // Initialize stars for night mode
            if (style == StyleMode.Night)
            {
                InitializeStars();
            }
        }

        public PopstarBg()
        {
            // Load animation frames
            frames = GFX.Game.GetAtlasSubtextures("bgs/19/say_goodbye/popstar/popstar");
            
            if (frames == null || frames.Count == 0)
            {
                Logger.Log(LogLevel.Warn, "PopstarBg", "No frames found for popstar animation");
                frames = new List<MTexture>();
            }
        }

        private void InitializeStars()
        {
            stars = new Star[STAR_COUNT];
            Random rand = new Random();
            
            for (int i = 0; i < STAR_COUNT; i++)
            {
                stars[i] = new Star
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * 320f,
                        (float)rand.NextDouble() * 180f
                    ),
                    Alpha = (float)rand.NextDouble() * 0.8f + 0.2f,
                    TwinkleSpeed = (float)rand.NextDouble() * 3f + 1f,
                    TwinklePhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Size = (float)rand.NextDouble() * 2f + 1f
                };
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            if (!Visible || frames == null || frames.Count == 0)
                return;

            // Update animation frame
            frameTimer += Engine.DeltaTime;
            if (frameTimer >= frameDelay)
            {
                frameTimer -= frameDelay;
                
                if (pingPong)
                {
                    currentFrame += pingPongDirection;
                    if (currentFrame >= frames.Count - 1)
                    {
                        currentFrame = frames.Count - 1;
                        pingPongDirection = -1;
                    }
                    else if (currentFrame <= 0)
                    {
                        currentFrame = 0;
                        pingPongDirection = 1;
                    }
                }
                else
                {
                    currentFrame++;
                    if (currentFrame >= frames.Count)
                    {
                        currentFrame = loops ? 0 : frames.Count - 1;
                    }
                }
            }

            // Update rotation
            rotation += rotationSpeed * Engine.DeltaTime;

            // Update scroll position
            position += scrollSpeed * Engine.DeltaTime;

            // Update pulse effect
            pulseTimer += Engine.DeltaTime * pulseSpeed;

            // Update rainbow timer
            rainbowTimer += Engine.DeltaTime * rainbowSpeed;

            // Update camera parallax
            if (scene is Level level)
            {
                parallaxOffset = level.Camera.Position * 0.1f;
            }
        }

        public override void Render(Scene scene)
        {
            if (frames == null || frames.Count == 0)
                return;

            MTexture frame = frames[currentFrame];
            Vector2 screenCenter = new Vector2(160f, 90f);
            Vector2 frameOrigin = new Vector2(frame.Width / 2f, frame.Height / 2f);

            // Calculate scale with pulse effect
            float currentScale = scale;
            if (pulseAmount > 0f)
            {
                currentScale *= 1f + (float)Math.Sin(pulseTimer) * pulseAmount;
            }

            // Calculate render position with scroll and parallax
            Vector2 renderPos = screenCenter + position - parallaxOffset;

            // Get tint color based on style
            Color finalColor = GetStyleColor();

            // Render glow effect first (if enabled)
            if (glowIntensity > 0f && style != StyleMode.Night)
            {
                Color glowColor = finalColor * glowIntensity * 0.3f;
                float glowScale = currentScale * 1.1f;
                frame.DrawCentered(renderPos, glowColor, glowScale, rotation);
            }

            // Main frame render
            frame.DrawCentered(renderPos, finalColor, currentScale, rotation);

            // Render stars overlay for night mode
            if (style == StyleMode.Night && stars != null)
            {
                RenderStars();
            }
        }

        private Color GetStyleColor()
        {
            Color baseColor = tintColor * alpha * FadeAlphaMultiplier;

            switch (style)
            {
                case StyleMode.Dreamy:
                    // Soft ethereal blue-pink tint
                    float dreamPhase = (float)Math.Sin(pulseTimer * 0.5f) * 0.5f + 0.5f;
                    Color dreamColor1 = new Color(200, 180, 255);
                    Color dreamColor2 = new Color(255, 200, 220);
                    return Color.Lerp(dreamColor1, dreamColor2, dreamPhase) * (baseColor.A / 255f);

                case StyleMode.Sunset:
                    // Warm orange-purple gradient
                    float sunsetPhase = (float)Math.Sin(pulseTimer * 0.3f) * 0.5f + 0.5f;
                    Color sunsetColor1 = new Color(255, 150, 100);
                    Color sunsetColor2 = new Color(200, 100, 180);
                    return Color.Lerp(sunsetColor1, sunsetColor2, sunsetPhase) * (baseColor.A / 255f);

                case StyleMode.Night:
                    // Dark blue tint
                    return new Color(50, 50, 100) * (baseColor.A / 255f);

                case StyleMode.Rainbow:
                    // Cycling rainbow colors
                    return HSVToRGB((rainbowTimer % 1f), 0.6f, 1f) * (baseColor.A / 255f);

                case StyleMode.Normal:
                default:
                    return baseColor;
            }
        }

        private void RenderStars()
        {
            if (stars == null) return;

            for (int i = 0; i < stars.Length; i++)
            {
                float twinkle = (float)Math.Sin(pulseTimer * stars[i].TwinkleSpeed + stars[i].TwinklePhase);
                float starAlpha = stars[i].Alpha * (0.5f + twinkle * 0.5f) * FadeAlphaMultiplier;
                
                Vector2 starPos = stars[i].Position + position - parallaxOffset;
                
                // Wrap star positions
                starPos.X = ((starPos.X % 320f) + 320f) % 320f;
                starPos.Y = ((starPos.Y % 180f) + 180f) % 180f;
                
                Draw.Rect(starPos, stars[i].Size, stars[i].Size, Color.White * starAlpha);
            }
        }

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
                _ => Color.White,
            };
        }

        /// <summary>
        /// Sets the animation speed (frame delay in seconds)
        /// </summary>
        public void SetFrameDelay(float delay)
        {
            frameDelay = Math.Max(0.01f, delay);
        }

        /// <summary>
        /// Sets the visual style mode
        /// </summary>
        public void SetStyle(StyleMode newStyle)
        {
            if (style != newStyle)
            {
                style = newStyle;
                if (style == StyleMode.Night && stars == null)
                {
                    InitializeStars();
                }
            }
        }

        /// <summary>
        /// Gets the current style mode
        /// </summary>
        public StyleMode GetStyle() => style;

        /// <summary>
        /// Sets the scale of the background
        /// </summary>
        public void SetScale(float newScale)
        {
            scale = Math.Max(0.1f, newScale);
        }

        /// <summary>
        /// Sets the alpha transparency
        /// </summary>
        public void SetAlpha(float newAlpha)
        {
            alpha = MathHelper.Clamp(newAlpha, 0f, 1f);
        }

        /// <summary>
        /// Sets the tint color
        /// </summary>
        public void SetTintColor(Color color)
        {
            tintColor = color;
        }

        /// <summary>
        /// Enables or disables the pulse effect
        /// </summary>
        public void SetPulse(float amount, float speed)
        {
            pulseAmount = MathHelper.Clamp(amount, 0f, 1f);
            pulseSpeed = speed;
        }

        /// <summary>
        /// Sets the glow intensity
        /// </summary>
        public void SetGlowIntensity(float intensity)
        {
            glowIntensity = MathHelper.Clamp(intensity, 0f, 2f);
        }

        /// <summary>
        /// Jumps to a specific frame
        /// </summary>
        public void SetFrame(int frame)
        {
            if (frames != null && frames.Count > 0)
            {
                currentFrame = (int)MathHelper.Clamp(frame, 0, frames.Count - 1);
            }
        }

        /// <summary>
        /// Gets the current frame index
        /// </summary>
        public int GetCurrentFrame() => currentFrame;

        /// <summary>
        /// Gets the total number of frames
        /// </summary>
        public int GetFrameCount() => frames?.Count ?? 0;
    }
}
