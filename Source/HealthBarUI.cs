using System;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Phase 5: Health Bar UI - Top-screen overlay displaying player health.
    /// Features:
    /// - Positioned at top of screen (10-20px height)
    /// - Color gradient: Green (75%+) → Yellow (25-75%) → Red (<25%)
    /// - Updates on PlayerHealthManager.OnHealthChanged event
    /// - Only renders when player has health > 0 (Kirby mode)
    /// - Smooth bar animation
    /// </summary>
    [Tracked(false)]
    public class HealthBarUI : Entity
    {
        // Dimensions and positioning
        private const float BarHeight = 16f;
        private const float BarWidth = 300f;
        private const float BarX = 10f;
        private const float BarY = 10f;
        private const float BorderThickness = 2f;

        // Colors
        private static readonly Color ColorBorder = Color.DarkGray;
        private static readonly Color ColorHealthFull = new Color(34, 177, 76);      // Green
        private static readonly Color ColorHealthMid = new Color(237, 201, 72);      // Yellow
        private static readonly Color ColorHealthLow = new Color(234, 67, 53);       // Red
        private static readonly Color ColorBackground = new Color(0, 0, 0, 180);     // Dark semi-transparent

        // Health tracking
        private int currentHealth = 0;
        private int maxHealth = 1;
        private float displayHealth = 0f;  // Smoothly animates to currentHealth
        private const float HealthAnimationSpeed = 200f;  // Pixels per second for bar animation

        // Reference to health manager
        private PlayerHealthManager healthManager;
        private Level level;

        public HealthBarUI(Level level) : base(Vector2.Zero)
        {
            this.level = level;
            Tag = Tags.HUD;
            Depth = -1000;  // Render in front of most entities, behind UI

            // Find and subscribe to PlayerHealthManager
            healthManager = level?.Tracker?.GetEntity<PlayerHealthManager>();
            if (healthManager != null)
            {
                currentHealth = healthManager.CurrentHP;
                maxHealth = healthManager.MaxHP;
                displayHealth = currentHealth;
                healthManager.OnHealthChanged += OnHealthChanged;
            }
        }

        /// <summary>
        /// Called when player health changes. Updates current health value.
        /// Event signature: OnHealthChanged(int currentHP, int maxHP)
        /// </summary>
        private void OnHealthChanged(int newHealth, int newMax)
        {
            currentHealth = newHealth;
            maxHealth = newMax;
            // displayHealth will smoothly animate to currentHealth in Update()
        }

        public override void Update()
        {
            base.Update();

            // Update health manager reference if needed
            if (healthManager == null && level != null)
            {
                healthManager = level.Tracker?.GetEntity<PlayerHealthManager>();
                if (healthManager != null)
                {
                    currentHealth = healthManager.CurrentHP;
                    maxHealth = healthManager.MaxHP;
                    healthManager.OnHealthChanged += OnHealthChanged;
                }
            }

            // Smoothly animate display health towards current health
            if (Math.Abs(displayHealth - currentHealth) > 0.01f)
            {
                float direction = Math.Sign(currentHealth - displayHealth);
                float step = HealthAnimationSpeed * Engine.DeltaTime;
                displayHealth += direction * Math.Min(step, Math.Abs(currentHealth - displayHealth));
            }
        }

        public override void Render()
        {
            base.Render();

            // Only render if health manager exists and player has health
            if (healthManager == null || maxHealth <= 0 || currentHealth <= 0)
                return;

            // Calculate bar fill percentage (0-1) - use HealthPercent from manager
            float healthPercent = Calc.Clamp(displayHealth / maxHealth, 0f, 1f);
            float filledWidth = BarWidth * healthPercent;

            // Draw background
            Draw.Rect(BarX - BorderThickness, BarY - BorderThickness,
                      BarWidth + BorderThickness * 2, BarHeight + BorderThickness * 2,
                      ColorBackground);

            // Draw border
            Draw.HollowRect(BarX - BorderThickness, BarY - BorderThickness,
                            BarWidth + BorderThickness * 2, BarHeight + BorderThickness * 2,
                            ColorBorder);

            // Determine health bar color based on percentage
            Color barColor = GetHealthColor(healthPercent);

            // Draw health bar
            if (filledWidth > 0)
                Draw.Rect(BarX, BarY, filledWidth, BarHeight, barColor);

            // Draw empty portion
            if (filledWidth < BarWidth)
                Draw.Rect(BarX + filledWidth, BarY, BarWidth - filledWidth, BarHeight,
                          new Color(50, 50, 50, 255));

            // Draw health text (e.g., "HP: 5/10")
            string healthText = $"HP: {currentHealth}/{maxHealth}";
            ActiveFont.Draw(healthText, new Vector2(BarX + BarWidth + 20f, BarY + 2f),
                           Vector2.Zero, Vector2.One, Color.White);
        }

        /// <summary>
        /// Get color based on health percentage.
        /// Green (75%+) → Yellow (25-75%) → Red (<25%)
        /// </summary>
        private Color GetHealthColor(float healthPercent)
        {
            if (healthPercent >= 0.75f)
                return ColorHealthFull;  // Green
            else if (healthPercent >= 0.25f)
                return ColorHealthMid;   // Yellow
            else
                return ColorHealthLow;   // Red
        }
    }
}
