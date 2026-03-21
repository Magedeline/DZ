namespace MaggyHelper
{
    /// <summary>
    /// Auto-animator for Chara character that syncs sprite animations with portrait expressions,
    /// including support for glitchy portrait effects
    /// </summary>
    [HotReloadable]
    public class CharaAutoAnimator : Component
    {
        private const bool enabled = true;
        private string lastAnimation = "fallSlow";
        private bool wasSyncingSprite = true;
        private Wiggler pop;

        // Glitch effect properties
        private bool isGlitching = false;
        private float glitchTimer = 0f;
        private float glitchIntensity = 0f;
        private Random glitchRandom;

        public CharaAutoAnimator() : base(true, false)
        {
            glitchRandom = new Random();
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Add(pop = Wiggler.Create(0.5f, 4f, f =>
            {
                Sprite sprite = Entity.Get<Sprite>();
                if (sprite == null)
                    return;
                sprite.Scale = new Microsoft.Xna.Framework.Vector2(Math.Sign(sprite.Scale.X), 1f) * (float)(1.0 + 0.25 * f);
            }));
        }

        public override void Removed(Entity entity)
        {
            entity.Remove(pop);
            base.Removed(entity);
        }

        public void SetReturnToAnimation(string anim) => lastAnimation = anim;

        public override void Update()
        {
            Sprite sprite = Entity.Get<Sprite>();
            if (Scene == null || sprite == null)
                return;

            bool flag = false;
            Textbox entity = Scene.Tracker.GetEntity<Textbox>();
            
            // Update glitch effects
            if (isGlitching)
            {
                UpdateGlitchEffect(sprite);
            }

            if (enabled && entity != null)
            {
                if (entity.PortraitName.IsIgnoreCase("chara"))
                {
                    string portraitAnim = entity.PortraitAnimation;
                    
                    // Check for glitchy portrait expressions
                    if (IsGlitchyExpression(portraitAnim))
                    {
                        HandleGlitchyExpression(sprite, portraitAnim);
                        wasSyncingSprite = flag = true;
                    }
                    // Standard expressions
                    else if (portraitAnim.IsIgnoreCase("laugh"))
                    {
                        if (!wasSyncingSprite)
                            lastAnimation = sprite.CurrentAnimationID;
                        PlaySafe(sprite, "laugh", "idle", "fallSlow");
                        StopGlitchEffect();
                        wasSyncingSprite = flag = true;
                    }
                    else if (portraitAnim.IsIgnoreCase("yell", "freakA", "freakB", "freakC"))
                    {
                        if (!wasSyncingSprite)
                        {
                            pop.Start();
                            lastAnimation = sprite.CurrentAnimationID;
                        }
                        PlaySafe(sprite, "angry", "idle", "fallSlow");
                        StopGlitchEffect();
                        wasSyncingSprite = flag = true;
                    }
                    else
                    {
                        // Stop glitching for normal expressions
                        if (!IsGlitchyExpression(portraitAnim))
                        {
                            StopGlitchEffect();
                        }
                    }
                }
            }

            if (!wasSyncingSprite || flag)
                return;
            wasSyncingSprite = true;
            if (string.IsNullOrEmpty(lastAnimation) || lastAnimation == "spin")
                lastAnimation = "fallSlow";
            if (sprite.CurrentAnimationID == "angry")
                pop.Start();
            PlaySafe(sprite, lastAnimation, "idle", "fallSlow");
        }

        /// <summary>
        /// Checks if the portrait expression is a glitchy variant
        /// </summary>
        private bool IsGlitchyExpression(string expression)
        {
            return !string.IsNullOrEmpty(expression) && 
                   expression.IndexOf("glitchy", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Handles glitchy portrait expressions and activates glitch effects
        /// </summary>
        private void HandleGlitchyExpression(Sprite sprite, string expression)
        {
            if (!wasSyncingSprite)
                lastAnimation = sprite.CurrentAnimationID;

            // Determine glitch intensity based on expression type
            if (expression.IsIgnoreCase("glitchyfreak"))
            {
                glitchIntensity = 1.0f; // Maximum chaos
                PlaySafe(sprite, "angry", "idle", "fallSlow");
            }
            else if (expression.IsIgnoreCase("glitchycreepy"))
            {
                glitchIntensity = 0.8f; // High intensity
                PlaySafe(sprite, "idle", "fallSlow");
            }
            else if (expression.IsIgnoreCase("glitchyangry", "glitchyrevenge"))
            {
                glitchIntensity = 0.7f; // High-medium intensity
                PlaySafe(sprite, "angry", "idle", "fallSlow");
            }
            else if (expression.IsIgnoreCase("glitchypanic"))
            {
                glitchIntensity = 0.9f; // Very high intensity
                PlaySafe(sprite, "idle", "fallSlow");
            }
            else if (expression.IsIgnoreCase("glitchysmirk"))
            {
                glitchIntensity = 0.6f; // Medium intensity
                PlaySafe(sprite, "laugh", "idle", "fallSlow");
            }
            else // glitchy (default)
            {
                glitchIntensity = 0.4f; // Subtle distortion
                PlaySafe(sprite, "idle", "fallSlow");
            }

            StartGlitchEffect();
        }

        /// <summary>
        /// Starts the glitch effect
        /// </summary>
        private void StartGlitchEffect()
        {
            isGlitching = true;
            glitchTimer = 0f;
        }

        /// <summary>
        /// Stops the glitch effect
        /// </summary>
        private void StopGlitchEffect()
        {
            isGlitching = false;
            glitchTimer = 0f;
            glitchIntensity = 0f;
        }

        /// <summary>
        /// Updates glitch visual effects on the sprite
        /// </summary>
        private void UpdateGlitchEffect(Sprite sprite)
        {
            glitchTimer += Engine.DeltaTime;

            // Randomize sprite position with glitch intensity
            if (glitchTimer % 0.1f < 0.05f)
            {
                float offsetX = (float)(glitchRandom.NextDouble() - 0.5) * glitchIntensity * 4f;
                float offsetY = (float)(glitchRandom.NextDouble() - 0.5) * glitchIntensity * 3f;
                sprite.Position = new Microsoft.Xna.Framework.Vector2(offsetX, offsetY);
            }
            else
            {
                // Reset to normal position
                sprite.Position = Microsoft.Xna.Framework.Vector2.Zero;
            }

            // Random scale fluctuations for more intense glitches
            if (glitchIntensity > 0.6f && glitchTimer % 0.15f < 0.05f)
            {
                float scaleVariation = 1f + (float)(glitchRandom.NextDouble() - 0.5) * glitchIntensity * 0.15f;
                sprite.Scale = new Microsoft.Xna.Framework.Vector2(
                    Math.Sign(sprite.Scale.X) * scaleVariation,
                    scaleVariation
                );
            }
        }

        private static void PlaySafe(Sprite sprite, string preferred, params string[] fallbacks)
        {
            if (sprite == null)
                return;

            if (!string.IsNullOrEmpty(preferred) && sprite.Has(preferred))
            {
                sprite.Play(preferred);
                return;
            }

            if (fallbacks != null)
            {
                foreach (string fallback in fallbacks)
                {
                    if (!string.IsNullOrEmpty(fallback) && sprite.Has(fallback))
                    {
                        sprite.Play(fallback);
                        return;
                    }
                }
            }
        }
    }
}




