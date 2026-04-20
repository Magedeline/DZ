namespace Celeste.Effects
{
    /// <summary>
    /// Visual glitch effect component for character portraits
    /// Provides various glitch intensities and patterns for Chara and other characters
    /// </summary>
    [HotReloadable]
    public class GlitchyPortraitEffect : Component
    {
        // Glitch intensity levels
        public enum GlitchLevel
        {
            None = 0,
            Subtle = 1,      // 0.3-0.4 intensity
            Moderate = 2,    // 0.5-0.6 intensity
            High = 3,        // 0.7-0.8 intensity
            Extreme = 4,     // 0.9-1.0 intensity
            Chaos = 5        // Maximum distortion
        }

        private GlitchLevel currentLevel;
        private float intensity;
        private bool isActive;
        private float timer;
        private Random random;
        private Sprite targetSprite;
        
        // Effect parameters
        private Vector2 originalPosition;
        private Vector2 originalScale;
        private Color originalColor;
        
        // Glitch patterns
        private float offsetTimer;
        private float colorShiftTimer;
        private float scaleTimer;
        
        // Configuration
        private bool enablePositionGlitch = true;
        private bool enableColorGlitch = true;
        private bool enableScaleGlitch = true;
        private bool enableRotationGlitch = false;

        public GlitchyPortraitEffect(GlitchLevel level = GlitchLevel.None) 
            : base(active: true, visible: false)
        {
            currentLevel = level;
            random = new Random();
            UpdateIntensity();
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            targetSprite = entity.Get<Sprite>();
            
            if (targetSprite != null)
            {
                originalPosition = targetSprite.Position;
                originalScale = targetSprite.Scale;
                originalColor = targetSprite.Color;
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (!isActive || targetSprite == null)
                return;

            timer += Engine.DeltaTime;
            
            ApplyGlitchEffects();
        }

        /// <summary>
        /// Activates the glitch effect with specified intensity level
        /// </summary>
        public void Activate(GlitchLevel level)
        {
            currentLevel = level;
            UpdateIntensity();
            isActive = true;
            timer = 0f;
        }

        /// <summary>
        /// Deactivates the glitch effect and resets visual properties
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
            ResetSprite();
        }

        /// <summary>
        /// Sets glitch intensity directly (0.0 to 1.0)
        /// </summary>
        public void SetIntensity(float value)
        {
            intensity = Calc.Clamp(value, 0f, 1f);
            isActive = intensity > 0f;
        }

        /// <summary>
        /// Gets current glitch level
        /// </summary>
        public GlitchLevel GetLevel() => currentLevel;

        /// <summary>
        /// Enables/disables specific glitch effects
        /// </summary>
        public void ConfigureEffects(bool position = true, bool color = true, 
                                      bool scale = true, bool rotation = false)
        {
            enablePositionGlitch = position;
            enableColorGlitch = color;
            enableScaleGlitch = scale;
            enableRotationGlitch = rotation;
        }

        /// <summary>
        /// Updates intensity based on current glitch level
        /// </summary>
        private void UpdateIntensity()
        {
            switch (currentLevel)
            {
                case GlitchLevel.None:
                    intensity = 0f;
                    break;
                case GlitchLevel.Subtle:
                    intensity = 0.35f;
                    break;
                case GlitchLevel.Moderate:
                    intensity = 0.55f;
                    break;
                case GlitchLevel.High:
                    intensity = 0.75f;
                    break;
                case GlitchLevel.Extreme:
                    intensity = 0.95f;
                    break;
                case GlitchLevel.Chaos:
                    intensity = 1.0f;
                    break;
            }
            
            isActive = intensity > 0f;
        }

        /// <summary>
        /// Applies visual glitch effects to the sprite
        /// </summary>
        private void ApplyGlitchEffects()
        {
            // Position offset glitch
            if (enablePositionGlitch)
            {
                ApplyPositionGlitch();
            }

            // Color shift glitch
            if (enableColorGlitch)
            {
                ApplyColorGlitch();
            }

            // Scale distortion glitch
            if (enableScaleGlitch)
            {
                ApplyScaleGlitch();
            }

            // Rotation glitch (optional, can be disorienting)
            if (enableRotationGlitch)
            {
                ApplyRotationGlitch();
            }
        }

        /// <summary>
        /// Applies position offset based on glitch intensity
        /// </summary>
        private void ApplyPositionGlitch()
        {
            offsetTimer += Engine.DeltaTime;
            
            // Frequency increases with intensity
            float frequency = 0.08f / (1f + intensity);
            
            if (offsetTimer >= frequency)
            {
                offsetTimer = 0f;
                
                // Random chance to glitch based on intensity
                if (random.NextDouble() < 0.3f + (intensity * 0.5f))
                {
                    float offsetX = (float)(random.NextDouble() - 0.5) * intensity * 6f;
                    float offsetY = (float)(random.NextDouble() - 0.5) * intensity * 5f;
                    targetSprite.Position = originalPosition + new Vector2(offsetX, offsetY);
                }
                else
                {
                    targetSprite.Position = originalPosition;
                }
            }
        }

        /// <summary>
        /// Applies color shift/corruption effects
        /// </summary>
        private void ApplyColorGlitch()
        {
            colorShiftTimer += Engine.DeltaTime;
            
            float frequency = 0.12f / (1f + intensity * 0.5f);
            
            if (colorShiftTimer >= frequency)
            {
                colorShiftTimer = 0f;
                
                if (random.NextDouble() < 0.25f + (intensity * 0.4f))
                {
                    // RGB channel separation effect
                    float r = originalColor.R / 255f + (float)(random.NextDouble() - 0.5) * intensity * 0.3f;
                    float g = originalColor.G / 255f + (float)(random.NextDouble() - 0.5) * intensity * 0.3f;
                    float b = originalColor.B / 255f + (float)(random.NextDouble() - 0.5) * intensity * 0.3f;
                    
                    targetSprite.Color = new Color(
                        Calc.Clamp(r, 0f, 1f),
                        Calc.Clamp(g, 0f, 1f),
                        Calc.Clamp(b, 0f, 1f),
                        1f
                    );
                }
                else
                {
                    targetSprite.Color = originalColor;
                }
            }
        }

        /// <summary>
        /// Applies scale distortion effects
        /// </summary>
        private void ApplyScaleGlitch()
        {
            scaleTimer += Engine.DeltaTime;
            
            float frequency = 0.15f / (1f + intensity * 0.3f);
            
            if (scaleTimer >= frequency && intensity > 0.5f)
            {
                scaleTimer = 0f;
                
                if (random.NextDouble() < (intensity - 0.5f) * 0.6f)
                {
                    float scaleX = 1f + (float)(random.NextDouble() - 0.5) * intensity * 0.2f;
                    float scaleY = 1f + (float)(random.NextDouble() - 0.5) * intensity * 0.2f;
                    
                    targetSprite.Scale = new Vector2(
                        Math.Sign(originalScale.X) * scaleX,
                        scaleY
                    );
                }
                else
                {
                    targetSprite.Scale = originalScale;
                }
            }
        }

        /// <summary>
        /// Applies rotation glitch (use sparingly)
        /// </summary>
        private void ApplyRotationGlitch()
        {
            if (intensity > 0.7f && random.NextDouble() < 0.05f)
            {
                targetSprite.Rotation = (float)(random.NextDouble() - 0.5) * intensity * 0.1f;
            }
            else if (timer % 0.2f < 0.1f)
            {
                targetSprite.Rotation = 0f;
            }
        }

        /// <summary>
        /// Resets sprite to original visual state
        /// </summary>
        private void ResetSprite()
        {
            if (targetSprite == null)
                return;

            targetSprite.Position = originalPosition;
            targetSprite.Scale = originalScale;
            targetSprite.Color = originalColor;
            targetSprite.Rotation = 0f;
        }

        public override void Removed(Entity entity)
        {
            ResetSprite();
            base.Removed(entity);
        }
    }
}




