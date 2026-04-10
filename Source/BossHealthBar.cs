using System;
using MaggyHelper.Entities;
using MaggyHelper.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Universal health bar that displays above any boss entity.
    /// Works with BossActor and any entity that has Health/MaxHealth properties.
    /// </summary>
    [Tracked]
    [HotReloadable]
    public class BossHealthBar : Entity
    {
        #region Constants
        
        // Bar dimensions
        private const float BAR_WIDTH = 200f;
        private const float BAR_HEIGHT = 12f;
        private const float BAR_PADDING = 2f;
        private const float BAR_Y_OFFSET = -40f;
        
        // Animation
        private const float DAMAGE_FLASH_TIME = 0.15f;
        private const float SMOOTH_SPEED = 5f;
        private const float SHAKE_INTENSITY = 3f;
        private const float SHAKE_DURATION = 0.2f;
        
        // Colors
        private static readonly Color BAR_BACKGROUND = new Color(40, 40, 40, 220);
        private static readonly Color BAR_BORDER = new Color(80, 80, 80, 255);
        private static readonly Color HEALTH_HIGH = new Color(100, 255, 100);
        private static readonly Color HEALTH_MID = new Color(255, 255, 100);
        private static readonly Color HEALTH_LOW = new Color(255, 100, 100);
        private static readonly Color HEALTH_CRITICAL = new Color(255, 50, 50);
        private static readonly Color DAMAGE_FLASH = Color.White;
        
        #endregion
        
        #region Fields
        
        // Target boss
        private Entity targetBoss;
        private Func<int> getHealth;
        private Func<int> getMaxHealth;
        
        // Health tracking
        private float displayedHealth;
        private float previousHealth;
        private int currentHealth;
        private int maxHealth;
        
        // Visual effects
        private float damageFlashTimer;
        private float shakeTimer;
        private Vector2 shakeOffset;
        private float pulseTimer;
        
        // Display options
        private string bossName;
        private bool showName;
        private bool isWorldSpace;
        private bool followBoss;
        private Vector2 fixedPosition;
        
        // State
        private bool isVisible = true;
        private float fadeAlpha = 1f;
        
        #endregion
        
        #region Properties
        
        public Entity TargetBoss => targetBoss;
        public bool IsVisible { get => isVisible; set => isVisible = value; }
        public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Create a health bar for a BossActor
        /// </summary>
        public BossHealthBar(BossActor boss, string name = null, bool worldSpace = false) 
            : base(boss.Position)
        {
            Setup(boss, name, worldSpace);
            
            getHealth = () => boss.Health;
            getMaxHealth = () => boss.MaxHealth;
        }
        
        /// <summary>
        /// Create a health bar for any entity with health getters
        /// </summary>
        public BossHealthBar(Entity boss, Func<int> healthGetter, Func<int> maxHealthGetter, string name = null, bool worldSpace = false) 
            : base(boss.Position)
        {
            Setup(boss, name, worldSpace);
            
            getHealth = healthGetter;
            getMaxHealth = maxHealthGetter;
        }
        
        /// <summary>
        /// Create a health bar at a fixed screen position
        /// </summary>
        public BossHealthBar(Entity boss, Func<int> healthGetter, Func<int> maxHealthGetter, Vector2 screenPosition, string name = null) 
            : base(Vector2.Zero)
        {
            targetBoss = boss;
            bossName = name;
            showName = !string.IsNullOrEmpty(name);
            isWorldSpace = false;
            followBoss = false;
            fixedPosition = screenPosition;
            
            getHealth = healthGetter;
            getMaxHealth = maxHealthGetter;
            
            Initialize();
        }
        
        private void Setup(Entity boss, string name, bool worldSpace)
        {
            targetBoss = boss;
            bossName = name;
            showName = !string.IsNullOrEmpty(name);
            isWorldSpace = worldSpace;
            followBoss = true;
        }
        
        private void Initialize()
        {
            Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate;
            Depth = -1000001;
            
            maxHealth = getMaxHealth?.Invoke() ?? 100;
            currentHealth = getHealth?.Invoke() ?? maxHealth;
            displayedHealth = currentHealth;
            previousHealth = currentHealth;
        }
        
        #endregion
        
        #region Lifecycle
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Initialize();
        }
        
        public override void Update()
        {
            base.Update();
            
            // Check if boss still exists
            if (targetBoss == null || targetBoss.Scene == null)
            {
                RemoveSelf();
                return;
            }
            
            // Update health values
            int newHealth = getHealth?.Invoke() ?? 0;
            maxHealth = getMaxHealth?.Invoke() ?? 100;
            
            // Detect damage
            if (newHealth < currentHealth)
            {
                OnDamageTaken(currentHealth - newHealth);
            }
            
            previousHealth = currentHealth;
            currentHealth = newHealth;
            
            // Smooth health bar animation
            displayedHealth = Calc.Approach(displayedHealth, currentHealth, SMOOTH_SPEED * maxHealth * Engine.DeltaTime);
            
            // Update effects
            UpdateEffects();
            
            // Update position if following boss
            if (followBoss && isWorldSpace)
            {
                Position = targetBoss.Position + new Vector2(0, BAR_Y_OFFSET);
            }
        }
        
        private void UpdateEffects()
        {
            // Damage flash
            if (damageFlashTimer > 0)
            {
                damageFlashTimer -= Engine.DeltaTime;
            }
            
            // Shake effect
            if (shakeTimer > 0)
            {
                shakeTimer -= Engine.DeltaTime;
                shakeOffset = new Vector2(
                    Calc.Random.Range(-SHAKE_INTENSITY, SHAKE_INTENSITY),
                    Calc.Random.Range(-SHAKE_INTENSITY, SHAKE_INTENSITY)
                );
            }
            else
            {
                shakeOffset = Vector2.Zero;
            }
            
            // Critical health pulse
            if (HealthPercent <= 0.2f && currentHealth > 0)
            {
                pulseTimer += Engine.DeltaTime * 6f;
            }
        }
        
        private void OnDamageTaken(int amount)
        {
            damageFlashTimer = DAMAGE_FLASH_TIME;
            shakeTimer = SHAKE_DURATION;
            
            // Screen shake for big hits
            if (amount >= maxHealth * 0.1f)
            {
                (Scene as Level)?.Shake(0.1f);
            }
        }
        
        #endregion
        
        #region Rendering
        
        public override void Render()
        {
            if (!isVisible || fadeAlpha <= 0)
                return;
            
            Vector2 renderPos = GetRenderPosition();
            
            // Apply shake
            renderPos += shakeOffset;
            
            // Draw name if enabled
            if (showName && !string.IsNullOrEmpty(bossName))
            {
                DrawBossName(renderPos);
                renderPos.Y += 16f;
            }
            
            // Draw health bar
            DrawHealthBar(renderPos);
            
            // Draw health numbers
            DrawHealthNumbers(renderPos);
        }
        
        private Vector2 GetRenderPosition()
        {
            if (!followBoss)
            {
                return fixedPosition;
            }
            
            if (isWorldSpace)
            {
                return Position - (Scene as Level)?.Camera?.Position ?? Vector2.Zero;
            }
            
            // HUD position - top center of screen
            return new Vector2(
                (Engine.Width / 2f - BAR_WIDTH / 2f) / 6f,
                20f
            );
        }
        
        private void DrawBossName(Vector2 position)
        {
            float nameWidth = ActiveFont.Measure(bossName).X * 0.5f;
            Vector2 namePos = position + new Vector2(BAR_WIDTH / 2f - nameWidth / 2f, -20f);
            
            // Draw with outline
            ActiveFont.DrawOutline(bossName, namePos, Vector2.Zero, Vector2.One * 0.5f, Color.White * fadeAlpha, 2f, Color.Black * fadeAlpha * 0.8f);
        }
        
        private void DrawHealthBar(Vector2 position)
        {
            float alpha = fadeAlpha;
            
            // Background
            Draw.Rect(
                position.X - BAR_PADDING, 
                position.Y - BAR_PADDING, 
                BAR_WIDTH + BAR_PADDING * 2, 
                BAR_HEIGHT + BAR_PADDING * 2, 
                BAR_BACKGROUND * alpha
            );
            
            // Border
            Draw.HollowRect(
                position.X - BAR_PADDING - 1, 
                position.Y - BAR_PADDING - 1, 
                BAR_WIDTH + BAR_PADDING * 2 + 2, 
                BAR_HEIGHT + BAR_PADDING * 2 + 2, 
                BAR_BORDER * alpha
            );
            
            // Health fill
            float healthWidth = maxHealth > 0 ? (displayedHealth / maxHealth) * BAR_WIDTH : 0f;
            if (healthWidth > 0)
            {
                Color healthColor = GetHealthColor();
                
                // Flash white when taking damage
                if (damageFlashTimer > 0)
                {
                    float flashLerp = damageFlashTimer / DAMAGE_FLASH_TIME;
                    healthColor = Color.Lerp(healthColor, DAMAGE_FLASH, flashLerp);
                }
                
                // Critical pulse
                if (HealthPercent <= 0.2f && currentHealth > 0)
                {
                    float pulse = (float)Math.Sin(pulseTimer) * 0.3f + 0.7f;
                    healthColor = Color.Lerp(healthColor, HEALTH_CRITICAL, 1f - pulse);
                }
                
                Draw.Rect(position.X, position.Y, healthWidth, BAR_HEIGHT, healthColor * alpha);
                
                // Highlight on top
                Draw.Rect(position.X, position.Y, healthWidth, 2f, Color.White * 0.3f * alpha);
            }
            
            // Damage indicator (shows recently lost health in different color)
            if (displayedHealth < previousHealth)
            {
                float damageWidth = maxHealth > 0 ? ((previousHealth - displayedHealth) / maxHealth) * BAR_WIDTH : 0f;
                Draw.Rect(
                    position.X + healthWidth, 
                    position.Y, 
                    damageWidth, 
                    BAR_HEIGHT, 
                    Color.Red * 0.5f * alpha
                );
            }
        }
        
        private void DrawHealthNumbers(Vector2 position)
        {
            string healthText = $"{currentHealth}/{maxHealth}";
            float textWidth = ActiveFont.Measure(healthText).X * 0.35f;
            Vector2 textPos = position + new Vector2(BAR_WIDTH / 2f - textWidth / 2f, BAR_HEIGHT + 4f);
            
            // Draw with outline
            ActiveFont.DrawOutline(healthText, textPos, Vector2.Zero, Vector2.One * 0.35f, Color.White * fadeAlpha, 1f, Color.Black * fadeAlpha * 0.8f);
        }
        
        private Color GetHealthColor()
        {
            float percent = HealthPercent;
            
            if (percent > 0.6f)
                return HEALTH_HIGH;
            else if (percent > 0.3f)
                return Color.Lerp(HEALTH_MID, HEALTH_HIGH, (percent - 0.3f) / 0.3f);
            else if (percent > 0.2f)
                return Color.Lerp(HEALTH_LOW, HEALTH_MID, (percent - 0.2f) / 0.1f);
            else
                return HEALTH_CRITICAL;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Fade out and remove the health bar
        /// </summary>
        public void FadeOut(float duration = 0.5f)
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, duration);
            tween.OnUpdate = t => fadeAlpha = 1f - t.Eased;
            tween.OnComplete = t => RemoveSelf();
            Add(tween);
            tween.Start();
        }
        
        /// <summary>
        /// Show the health bar
        /// </summary>
        public void Show()
        {
            isVisible = true;
            fadeAlpha = 1f;
        }
        
        /// <summary>
        /// Hide the health bar
        /// </summary>
        public void Hide()
        {
            isVisible = false;
        }
        
        /// <summary>
        /// Update the boss name
        /// </summary>
        public void SetBossName(string name)
        {
            bossName = name;
            showName = !string.IsNullOrEmpty(name);
        }
        
        /// <summary>
        /// Force an immediate update of health values
        /// </summary>
        public void RefreshHealth()
        {
            currentHealth = getHealth?.Invoke() ?? 0;
            maxHealth = getMaxHealth?.Invoke() ?? 100;
            displayedHealth = currentHealth;
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Create and attach a health bar to a BossActor
        /// </summary>
        public static BossHealthBar AttachToBoss(BossActor boss, string name = null, bool worldSpace = false)
        {
            var healthBar = new BossHealthBar(boss, name, worldSpace);
            boss.Scene?.Add(healthBar);
            return healthBar;
        }
        
        /// <summary>
        /// Create a health bar for any entity with custom health getters
        /// </summary>
        public static BossHealthBar AttachToEntity(Entity entity, Func<int> healthGetter, Func<int> maxHealthGetter, string name = null)
        {
            var healthBar = new BossHealthBar(entity, healthGetter, maxHealthGetter, name, true);
            entity.Scene?.Add(healthBar);
            return healthBar;
        }
        
        /// <summary>
        /// Create a HUD-style boss health bar at the top of the screen
        /// </summary>
        public static BossHealthBar CreateHUDBar(Level level, Entity boss, Func<int> healthGetter, Func<int> maxHealthGetter, string name = null)
        {
            var healthBar = new BossHealthBar(boss, healthGetter, maxHealthGetter, name, false);
            level.Add(healthBar);
            return healthBar;
        }
        
        #endregion
    }
}
