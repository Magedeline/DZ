using System;
using System.Collections.Generic;
using Celeste.Entities;
using Celeste.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Unified HUD component that displays both player HP and boss HP.
    /// Supports Kirby-style hearts and traditional bar displays.
    /// Works with PlayerHealthManager and any BossActor.
    /// </summary>
    [Tracked]
    [CustomEntity(ids: "DZ/UniversalHealthUI")]
    public class UniversalHealthUI : Entity
    {
        #region Constants
        
        // Player health display (hearts)
        private const int HEART_SIZE = 16;
        private const int HEART_SPACING = 2;
        private const int HEARTS_PER_ROW = 10;
        private const float PLAYER_HEALTH_X = 20f;
        private const float PLAYER_HEALTH_Y = 20f;
        
        // Boss health display (bar)
        private const float BOSS_BAR_WIDTH = 300f;
        private const float BOSS_BAR_HEIGHT = 16f;
        private const float BOSS_BAR_Y = 30f;
        
        // Animation
        private const float HEART_WIGGLE_DURATION = 0.3f;
        private const float DAMAGE_FLASH_TIME = 0.2f;
        
        #endregion
        
        #region Display Mode
        
        public enum PlayerHealthDisplayMode
        {
            Hearts,      // Kirby-style hearts
            Bar,         // Traditional bar
            Numeric,     // Just numbers
            HeartsAndBar // Hearts with mini bar
        }
        
        #endregion
        
        #region Fields
        
        // References
        private Level level;
        private PlayerHealthManager playerHealth;
        private List<TrackedBoss> trackedBosses = new List<TrackedBoss>();
        
        // Textures
        private MTexture heartFull;
        private MTexture heartEmpty;
        private MTexture heartHalf;
        private MTexture kirbyHeartFull;
        private MTexture kirbyHeartEmpty;
        
        // Display settings
        private PlayerHealthDisplayMode displayMode = PlayerHealthDisplayMode.Hearts;
        private bool showBossHealth = true;
        private bool showPlayerHealth = true;
        
        // Animation state
        private float[] heartWiggles;
        private float playerDamageFlash;
        private float lowHealthPulse;
        private bool lastFrameLowHealth;
        
        // Previous health for change detection
        private int lastPlayerHealth = -1;
        private int lastPlayerMaxHealth = -1;
        
        #endregion
        
        #region Properties
        
        public PlayerHealthDisplayMode DisplayMode 
        { 
            get => displayMode; 
            set => displayMode = value; 
        }
        
        public bool ShowBossHealth 
        { 
            get => showBossHealth; 
            set => showBossHealth = value; 
        }
        
        public bool ShowPlayerHealth 
        { 
            get => showPlayerHealth; 
            set => showPlayerHealth = value; 
        }
        
        public static UniversalHealthUI Instance { get; private set; }
        
        #endregion
        
        #region Tracked Boss Helper
        
        private class TrackedBoss
        {
            public Entity Boss;
            public Func<int> GetHealth;
            public Func<int> GetMaxHealth;
            public string Name;
            public bool UseDynamicName;
            public float DisplayedHealth;
            public float DamageFlash;
            public int LastHealth;
            
            public TrackedBoss(Entity boss, Func<int> health, Func<int> maxHealth, string name, bool useDynamicName)
            {
                Boss = boss;
                GetHealth = health;
                GetMaxHealth = maxHealth;
                Name = name;
                UseDynamicName = useDynamicName;
                DisplayedHealth = health();
                LastHealth = (int)DisplayedHealth;
            }
        }
        
        #endregion
        
        #region Constructor
        
        public UniversalHealthUI() : base(Vector2.Zero)
        {
            Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate;
            Depth = -1000002;
            
            Instance = this;
        }
        
        public UniversalHealthUI(EntityData data, Vector2 offset) : this()
        {
            displayMode = (PlayerHealthDisplayMode)data.Int("displayMode", 0);
            showBossHealth = data.Bool("showBossHealth", true);
            showPlayerHealth = data.Bool("showPlayerHealth", true);
        }
        
        #endregion
        
        #region Lifecycle
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            Instance = this;
            LoadTextures();
            
            // Find or create player health manager
            playerHealth = PlayerHealthManager.Instance;
            if (playerHealth == null)
            {
                playerHealth = level.Tracker.GetEntity<PlayerHealthManager>();
            }
            
            // Initialize heart wiggles
            heartWiggles = new float[20];
            
            // Subscribe to health events
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += OnPlayerHealthChanged;
                playerHealth.OnDamageTaken += OnPlayerDamaged;
            }
            
            // Auto-track all bosses in the scene
            AutoTrackBosses();
        }
        
        public override void Removed(Scene scene)
        {
            if (Instance == this)
                Instance = null;
            
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
                playerHealth.OnDamageTaken -= OnPlayerDamaged;
            }
            
            trackedBosses.Clear();
            base.Removed(scene);
        }
        
        private void LoadTextures()
        {
            MTexture TryGetTexture(params string[] candidates)
            {
                foreach (string candidate in candidates)
                {
                    if (GFX.Gui.Has(candidate))
                    {
                        return GFX.Gui[candidate];
                    }

                    if (GFX.Game.Has(candidate))
                    {
                        return GFX.Game[candidate];
                    }
                }

                return null;
            }

            // Kirby-specific hearts from the GUI atlas.
            kirbyHeartFull = TryGetTexture(
                "kirby/heart_full",
                "maggy/kirby/heart_full",
                "Maggy/kirby/heart_full",
                "gui/kirby/heart_full"
            );
            kirbyHeartEmpty = TryGetTexture(
                "kirby/heart_empty",
                "maggy/kirby/heart_empty",
                "Maggy/kirby/heart_empty",
                "gui/kirby/heart_empty"
            );

            // Generic health hearts; if unavailable, reuse Kirby textures.
            heartFull = TryGetTexture("health/heart_full", "gui/health/heart_full");
            heartEmpty = TryGetTexture("health/heart_empty", "gui/health/heart_empty");
            heartHalf = TryGetTexture("health/heart_half", "gui/health/heart_half");

            if (heartFull == null)
            {
                heartFull = kirbyHeartFull;
            }

            if (heartEmpty == null)
            {
                heartEmpty = kirbyHeartEmpty;
            }

            if (heartHalf == null)
            {
                heartHalf = heartFull;
            }

            if (kirbyHeartFull == null)
            {
                kirbyHeartFull = heartFull;
            }

            if (kirbyHeartEmpty == null)
            {
                kirbyHeartEmpty = heartEmpty;
            }

            // Last fallback if no heart texture is available at all.
            MTexture fallback = TryGetTexture("collectables/heartGem/0/00");
            if (heartFull == null)
            {
                heartFull = fallback;
            }

            if (heartEmpty == null)
            {
                heartEmpty = fallback;
            }

            if (heartHalf == null)
            {
                heartHalf = fallback;
            }

            if (kirbyHeartFull == null)
            {
                kirbyHeartFull = heartFull;
            }

            if (kirbyHeartEmpty == null)
            {
                kirbyHeartEmpty = heartEmpty;
            }
        }
        
        private void AutoTrackBosses()
        {
            // Track all BossActor entities
            foreach (var entity in level.Tracker.GetEntities<BossActor>())
            {
                if (entity is BossActor boss)
                {
                    TrackBoss(boss);
                }
            }
        }
        
        private string GetBossName(Entity boss)
        {
            if (boss is BossActor bossActor && !string.IsNullOrWhiteSpace(bossActor.BossDisplayName))
            {
                return bossActor.BossDisplayName;
            }

            // Try to get a meaningful name from the boss type
            string typeName = boss.GetType().Name;
            
            // Remove common suffixes
            typeName = typeName.Replace("Boss", "").Replace("Actor", "");
            
            // Add spaces before capitals
            var result = new System.Text.StringBuilder();
            foreach (char c in typeName)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result.Append(' ');
                result.Append(c);
            }
            
            return result.ToString();
        }
        
        #endregion
        
        #region Update
        
        public override void Update()
        {
            base.Update();
            
            // Update player health reference
            if (playerHealth == null)
            {
                playerHealth = PlayerHealthManager.Instance ?? level?.Tracker.GetEntity<PlayerHealthManager>();
            }
            
            // Update damage flash
            if (playerDamageFlash > 0)
            {
                playerDamageFlash -= Engine.DeltaTime;
            }
            
            // Update heart wiggles
            for (int i = 0; i < heartWiggles.Length; i++)
            {
                if (heartWiggles[i] > 0)
                {
                    heartWiggles[i] -= Engine.DeltaTime / HEART_WIGGLE_DURATION;
                }
            }
            
            // Low health pulse
            if (playerHealth != null && playerHealth.IsLowHealth && !playerHealth.IsDead)
            {
                lowHealthPulse += Engine.DeltaTime * 5f;
                
                if (!lastFrameLowHealth)
                {
                    // Just became low health - trigger all hearts to wiggle
                    for (int i = 0; i < heartWiggles.Length; i++)
                    {
                        heartWiggles[i] = 1f;
                    }
                }
                lastFrameLowHealth = true;
            }
            else
            {
                lowHealthPulse = 0f;
                lastFrameLowHealth = false;
            }
            
            // Update tracked bosses
            UpdateTrackedBosses();
        }
        
        private void UpdateTrackedBosses()
        {
            for (int i = trackedBosses.Count - 1; i >= 0; i--)
            {
                var tracked = trackedBosses[i];
                
                // Remove dead bosses
                if (tracked.Boss == null || tracked.Boss.Scene == null)
                {
                    trackedBosses.RemoveAt(i);
                    continue;
                }

                if (tracked.UseDynamicName)
                {
                    tracked.Name = GetBossName(tracked.Boss);
                }
                
                // Update health tracking
                int currentHealth = tracked.GetHealth();
                int maxHealth = tracked.GetMaxHealth();
                
                // Detect damage
                if (currentHealth < tracked.LastHealth)
                {
                    tracked.DamageFlash = DAMAGE_FLASH_TIME;
                }
                
                tracked.LastHealth = currentHealth;
                
                // Smooth animation
                tracked.DisplayedHealth = Calc.Approach(tracked.DisplayedHealth, currentHealth, maxHealth * 3f * Engine.DeltaTime);
                
                // Update flash
                if (tracked.DamageFlash > 0)
                {
                    tracked.DamageFlash -= Engine.DeltaTime;
                }
            }
        }
        
        #endregion
        
        #region Events
        
        private void OnPlayerHealthChanged(int current, int max)
        {
            // Trigger wiggle on changed hearts
            if (lastPlayerHealth >= 0 && lastPlayerMaxHealth > 0)
            {
                int changedFrom = Math.Min(lastPlayerHealth, current);
                int changedTo = Math.Max(lastPlayerHealth, current);
                
                for (int i = changedFrom; i <= changedTo && i < heartWiggles.Length; i++)
                {
                    heartWiggles[i] = 1f;
                }
            }
            
            lastPlayerHealth = current;
            lastPlayerMaxHealth = max;
        }
        
        private void OnPlayerDamaged(int amount)
        {
            playerDamageFlash = DAMAGE_FLASH_TIME;
        }
        
        #endregion
        
        #region Render
        
        public override void Render()
        {
            // Player health
            if (showPlayerHealth && playerHealth != null)
            {
                RenderPlayerHealth();
            }
            
            // Boss health bars
            if (showBossHealth)
            {
                RenderBossHealthBars();
            }
        }
        
        private void RenderPlayerHealth()
        {
            Vector2 position = new Vector2(PLAYER_HEALTH_X, PLAYER_HEALTH_Y);
            
            switch (displayMode)
            {
                case PlayerHealthDisplayMode.Hearts:
                    RenderPlayerHearts(position);
                    break;
                case PlayerHealthDisplayMode.Bar:
                    RenderPlayerBar(position);
                    break;
                case PlayerHealthDisplayMode.Numeric:
                    RenderPlayerNumeric(position);
                    break;
                case PlayerHealthDisplayMode.HeartsAndBar:
                    RenderPlayerHearts(position);
                    RenderPlayerBar(position + new Vector2(0, HEART_SIZE + 8));
                    break;
            }
        }
        
        private void RenderPlayerHearts(Vector2 position)
        {
            int maxHP = playerHealth.MaxHP;
            int currentHP = playerHealth.CurrentHP;
            bool isKirby = playerHealth.IsKirbyMode;
            
            MTexture full = isKirby ? kirbyHeartFull : heartFull;
            MTexture empty = isKirby ? kirbyHeartEmpty : heartEmpty;
            
            for (int i = 0; i < maxHP; i++)
            {
                int row = i / HEARTS_PER_ROW;
                int col = i % HEARTS_PER_ROW;
                
                Vector2 heartPos = position + new Vector2(
                    col * (HEART_SIZE + HEART_SPACING),
                    row * (HEART_SIZE + HEART_SPACING)
                );
                
                // Apply wiggle
                float wiggle = heartWiggles[Math.Min(i, heartWiggles.Length - 1)];
                float scale = 1f + wiggle * 0.3f;
                
                // Low health pulse for remaining hearts
                if (playerHealth.IsLowHealth && i < currentHP)
                {
                    float pulse = (float)Math.Sin(lowHealthPulse + i * 0.5f) * 0.15f;
                    scale += pulse;
                }
                
                // Determine color
                Color color = Color.White;
                if (playerDamageFlash > 0 && i >= currentHP)
                {
                    color = Color.Lerp(Color.White, Color.Red, playerDamageFlash / DAMAGE_FLASH_TIME);
                }
                
                // Draw heart
                if (full != null && empty != null)
                {
                    MTexture tex = i < currentHP ? full : empty;
                    tex.DrawCentered(heartPos + new Vector2(HEART_SIZE / 2f), color, scale);
                }
                else
                {
                    // Fallback rectangle hearts
                    Color heartColor = i < currentHP ? Color.Red : Color.Gray * 0.5f;
                    if (playerHealth.IsLowHealth && i < currentHP)
                    {
                        heartColor = Color.Lerp(Color.Red, Color.Yellow, (float)Math.Sin(lowHealthPulse) * 0.5f + 0.5f);
                    }
                    Draw.Rect(heartPos, HEART_SIZE * scale, HEART_SIZE * scale, heartColor);
                }
            }
        }
        
        private void RenderPlayerBar(Vector2 position)
        {
            float barWidth = 120f;
            float barHeight = 10f;
            
            float healthPercent = playerHealth.HealthPercent;
            
            // Background
            Draw.Rect(position, barWidth, barHeight, Color.Black * 0.7f);
            
            // Health fill
            Color healthColor = GetHealthColor(healthPercent);
            if (playerDamageFlash > 0)
            {
                healthColor = Color.Lerp(healthColor, Color.White, playerDamageFlash / DAMAGE_FLASH_TIME);
            }
            
            Draw.Rect(position, barWidth * healthPercent, barHeight, healthColor);
            
            // Border
            Draw.HollowRect(position - Vector2.One, barWidth + 2, barHeight + 2, Color.White * 0.5f);
            
            // HP text
            string hpText = $"{playerHealth.CurrentHP}/{playerHealth.MaxHP}";
            ActiveFont.DrawOutline(hpText, position + new Vector2(barWidth + 8, -2), Vector2.Zero, Vector2.One * 0.4f, Color.White, 1f, Color.Black * 0.8f);
        }
        
        private void RenderPlayerNumeric(Vector2 position)
        {
            string hpText = $"HP: {playerHealth.CurrentHP}/{playerHealth.MaxHP}";
            
            Color textColor = Color.White;
            if (playerHealth.IsLowHealth)
            {
                float pulse = (float)Math.Sin(lowHealthPulse) * 0.5f + 0.5f;
                textColor = Color.Lerp(Color.White, Color.Red, pulse);
            }
            
            // Draw with outline
            ActiveFont.DrawOutline(hpText, position, Vector2.Zero, Vector2.One * 0.6f, textColor, 2f, Color.Black * 0.8f);
        }
        
        private void RenderBossHealthBars()
        {
            float yOffset = BOSS_BAR_Y;
            
            foreach (var tracked in trackedBosses)
            {
                if (tracked.Boss == null) continue;
                
                Vector2 position = new Vector2((Engine.Width / 6f - BOSS_BAR_WIDTH) / 2f, yOffset);
                RenderBossBar(position, tracked);
                
                yOffset += BOSS_BAR_HEIGHT + 25f;
            }
        }
        
        private void RenderBossBar(Vector2 position, TrackedBoss tracked)
        {
            int maxHealth = tracked.GetMaxHealth();
            int currentHealth = tracked.GetHealth();
            float displayedPercent = maxHealth > 0 ? tracked.DisplayedHealth / maxHealth : 0f;
            float actualPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            
            // Boss name
            if (!string.IsNullOrEmpty(tracked.Name))
            {
                ActiveFont.DrawOutline(tracked.Name, position + new Vector2(0, -16), Vector2.Zero, Vector2.One * 0.4f, Color.White, 1f, Color.Black * 0.8f);
            }
            
            // Background
            Draw.Rect(position, BOSS_BAR_WIDTH, BOSS_BAR_HEIGHT, new Color(20, 20, 20, 200));
            
            // Damage indicator (red bar showing recent damage)
            if (displayedPercent > actualPercent)
            {
                float damageWidth = (displayedPercent - actualPercent) * BOSS_BAR_WIDTH;
                Draw.Rect(position.X + actualPercent * BOSS_BAR_WIDTH, position.Y, damageWidth, BOSS_BAR_HEIGHT, Color.Red * 0.6f);
            }
            
            // Health fill
            Color healthColor = GetHealthColor(actualPercent);
            if (tracked.DamageFlash > 0)
            {
                healthColor = Color.Lerp(healthColor, Color.White, tracked.DamageFlash / DAMAGE_FLASH_TIME);
            }
            
            float healthWidth = actualPercent * BOSS_BAR_WIDTH;
            Draw.Rect(position, healthWidth, BOSS_BAR_HEIGHT, healthColor);
            
            // Highlight
            Draw.Rect(position, healthWidth, 3f, Color.White * 0.3f);
            
            // Border
            Draw.HollowRect(position - Vector2.One, BOSS_BAR_WIDTH + 2, BOSS_BAR_HEIGHT + 2, Color.White * 0.6f);
            
            // Health numbers
            string healthText = $"{currentHealth}/{maxHealth}";
            Vector2 textSize = ActiveFont.Measure(healthText) * 0.35f;
            Vector2 textPos = position + new Vector2(BOSS_BAR_WIDTH / 2f - textSize.X / 2f, BOSS_BAR_HEIGHT + 2f);
            
            ActiveFont.DrawOutline(healthText, textPos, Vector2.Zero, Vector2.One * 0.35f, Color.White, 1f, Color.Black * 0.8f);
        }
        
        private Color GetHealthColor(float percent)
        {
            if (percent > 0.6f)
                return new Color(100, 255, 100);
            else if (percent > 0.3f)
                return Color.Lerp(new Color(255, 255, 100), new Color(100, 255, 100), (percent - 0.3f) / 0.3f);
            else if (percent > 0.15f)
                return Color.Lerp(new Color(255, 100, 100), new Color(255, 255, 100), (percent - 0.15f) / 0.15f);
            else
                return new Color(255, 50, 50);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Track a boss to display its health
        /// </summary>
        public void TrackBoss(BossActor boss, string name = null)
        {
            TrackBoss(boss, () => boss.Health, () => boss.MaxHealth, name);
        }
        
        /// <summary>
        /// Track any entity with custom health getters
        /// </summary>
        public void TrackBoss(Entity boss, Func<int> healthGetter, Func<int> maxHealthGetter, string name = null)
        {
            // Don't track duplicates
            if (trackedBosses.Exists(t => t.Boss == boss))
                return;
            
            trackedBosses.Add(new TrackedBoss(boss, healthGetter, maxHealthGetter, name ?? GetBossName(boss), name == null));
        }
        
        /// <summary>
        /// Stop tracking a boss
        /// </summary>
        public void UntrackBoss(Entity boss)
        {
            trackedBosses.RemoveAll(t => t.Boss == boss);
        }
        
        /// <summary>
        /// Clear all tracked bosses
        /// </summary>
        public void ClearTrackedBosses()
        {
            trackedBosses.Clear();
        }
        
        /// <summary>
        /// Set the player health display mode
        /// </summary>
        public void SetDisplayMode(PlayerHealthDisplayMode mode)
        {
            displayMode = mode;
        }
        
        #endregion
        
        #region Static Helpers
        
        /// <summary>
        /// Get or create the UniversalHealthUI for the current level
        /// </summary>
        public static UniversalHealthUI GetOrCreate(Level level)
        {
            if (Instance != null && Instance.Scene == level)
                return Instance;
            
            var existing = level.Tracker.GetEntity<UniversalHealthUI>();
            if (existing != null)
                return existing;
            
            var ui = new UniversalHealthUI();
            level.Add(ui);
            return ui;
        }
        
        /// <summary>
        /// Track a boss through the singleton
        /// </summary>
        public static void TrackBossStatic(BossActor boss, string name = null)
        {
            Instance?.TrackBoss(boss, name);
        }
        
        /// <summary>
        /// Track any entity through the singleton
        /// </summary>
        public static void TrackEntityStatic(Entity entity, Func<int> health, Func<int> maxHealth, string name = null)
        {
            Instance?.TrackBoss(entity, health, maxHealth, name);
        }
        
        #endregion
    }
}
