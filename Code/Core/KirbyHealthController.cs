using System;
using Celeste.Entities;
using Celeste.Extensions;
using Celeste.Integrations;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    /// <summary>
    /// Central controller for Kirby health system.
    /// Handles damage from spikes, spinners, crushing, out of bounds,
    /// and manages respawn with progress preservation.
    /// </summary>
    [Tracked]
    public class KirbyHealthController : Entity
    {
        public static KirbyHealthController Instance { get; private set; }

        // Configuration
        public int MaxHealth { get; private set; } = 6;
        public int CurrentHealth { get; private set; } = 6;
        public bool IsEnabled { get; private set; }

        // Damage settings
        public int SpikeDamage { get; set; } = 1;
        public int SpinnerDamage { get; set; } = 1;
        public int CrushDamage { get; set; } = 6; // Instant death
        public int OutOfBoundsDamage { get; set; } = 6; // Instant death
        public int BossDamage { get; set; } = 1;

        // Invincibility frames after taking damage
        public float InvincibilityDuration { get; set; } = 1.5f;
        private float invincibilityTimer;

        // Progress preservation
        private string respawnRoom;
        private Vector2 respawnPosition;
        private bool hasSavedRespawn;

        // References
        private Player player;
        private Level level;
        private PlayerHealthManager healthManager;
        private UniversalHealthUI healthUI;

        // Events
        public event Action<int, int> OnHealthChanged;
        public event Action OnKirbyDeath;
        public event Action OnKirbyRespawn;

        public bool IsDead => CurrentHealth <= 0;
        public bool IsInvincible => invincibilityTimer > 0f;
        public bool IsLowHealth => CurrentHealth > 0 && CurrentHealth <= Math.Max(1, MaxHealth / 3);
        public float HealthPercent => MaxHealth <= 0 ? 0f : (float)CurrentHealth / MaxHealth;

        public KirbyHealthController() : base(Vector2.Zero)
        {
            Tag = Tags.Persistent | Tags.TransitionUpdate | Tags.Global;
            Depth = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Instance = this;
            level = scene as Level;

            // Get or create health manager
            healthManager = PlayerHealthManager.GetOrCreate(level, MaxHealth);
            healthManager.EnableKirbyMode(MaxHealth);

            // Subscribe to health events
            healthManager.OnHealthChanged += OnHealthManagerChanged;
            healthManager.OnDamageTaken += OnHealthManagerDamage;

            // Get or create UI
            healthUI = UniversalHealthUI.GetOrCreate(level);
            healthUI.ShowPlayerHealth = true;

            // Find player
            player = level.Tracker.GetEntity<Player>();
            if (player == null)
            {
                player = level.Tracker.GetEntity<global::Celeste.Player>();
            }

            // Restore health from health manager
            CurrentHealth = healthManager.CurrentHP;
            MaxHealth = healthManager.MaxHP;

            Logger.Log(LogLevel.Info, "KirbyHealthController", "Initialized with health: " + CurrentHealth + "/" + MaxHealth);
        }

        public override void Removed(Scene scene)
        {
            if (Instance == this)
                Instance = null;

            if (healthManager != null)
            {
                healthManager.OnHealthChanged -= OnHealthManagerChanged;
                healthManager.OnDamageTaken -= OnHealthManagerDamage;
            }

            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            if (!IsEnabled)
                return;

            // Update invincibility timer
            if (invincibilityTimer > 0f)
            {
                invincibilityTimer -= Engine.DeltaTime;

                // Flash player sprite while invincible
                if (player != null && player.Sprite != null)
                {
                    player.Sprite.Visible = (int)(invincibilityTimer * 10) % 2 == 0;
                }
            }
            else if (player != null && player.Sprite != null && !player.Sprite.Visible)
            {
                player.Sprite.Visible = true;
            }

            // Update player reference if needed
            if (player == null || player.Scene == null)
            {
                player = level?.Tracker.GetEntity<Player>() ?? level?.Tracker.GetEntity<global::Celeste.Player>();
            }

            // Check for out of bounds
            if (player != null && !player.Dead && level != null)
            {
                CheckOutOfBounds();
                CheckCrushing();
            }
        }

        #region Damage Handling

        /// <summary>
        /// Attempts to damage the player. Returns true if damage was applied.
        /// </summary>
        public bool Damage(int amount, Vector2 source, DamageType type)
        {
            if (!IsEnabled || IsDead || IsInvincible)
                return false;

            // Apply damage through health manager
            if (healthManager != null)
            {
                healthManager.Damage(amount);
                return true;
            }

            // Fallback direct damage
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            // Check for death
            if (CurrentHealth <= 0)
            {
                HandleDeath();
            }
            else
            {
                // Apply knockback and invincibility
                ApplyKnockback(source);
                invincibilityTimer = InvincibilityDuration;
            }

            return true;
        }

        /// <summary>
        /// Deal damage from a spike hazard
        /// </summary>
        public bool DamageFromSpike(Vector2 source)
        {
            return Damage(SpikeDamage, source, DamageType.Spike);
        }

        /// <summary>
        /// Deal damage from a spinner hazard
        /// </summary>
        public bool DamageFromSpinner(Vector2 source)
        {
            return Damage(SpinnerDamage, source, DamageType.Spinner);
        }

        /// <summary>
        /// Deal damage from being crushed
        /// </summary>
        public bool DamageFromCrush()
        {
            // Crushing is instant death
            if (healthManager != null)
            {
                healthManager.Damage(CrushDamage);
            }
            return true;
        }

        /// <summary>
        /// Deal damage from falling out of bounds
        /// </summary>
        public bool DamageFromOutOfBounds()
        {
            // Out of bounds is instant death - bypass invincibility
            CurrentHealth = 0;
            OnHealthChanged?.Invoke(0, MaxHealth);
            
            if (healthManager != null)
            {
                healthManager.Damage(OutOfBoundsDamage);
            }
            
            // Force death immediately
            HandleDeath();
            return true;
        }

        /// <summary>
        /// Deal damage during boss fight
        /// </summary>
        public bool DamageFromBoss(Vector2 source, int damage = -1)
        {
            int dmg = damage > 0 ? damage : BossDamage;
            return Damage(dmg, source, DamageType.Boss);
        }

        /// <summary>
        /// Deal damage from regular enemy
        /// </summary>
        public bool DamageFromEnemy(Vector2 source, int damage = -1)
        {
            int dmg = damage > 0 ? damage : 1; // Default 1 damage from enemies
            return Damage(dmg, source, DamageType.Enemy);
        }

        /// <summary>
        /// Heal the player
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead)
                return;

            if (healthManager != null)
            {
                healthManager.Heal(amount);
            }
            else
            {
                CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
        }

        /// <summary>
        /// Full heal
        /// </summary>
        public void FullHeal()
        {
            if (healthManager != null)
            {
                healthManager.FullHeal();
            }
            else
            {
                CurrentHealth = MaxHealth;
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
        }

        #endregion

        #region Death and Respawn

        private void HandleDeath()
        {
            OnKirbyDeath?.Invoke();

            if (player != null && !player.Dead)
            {
                // Use the custom respawn system instead of normal death
                StartRespawnSequence();
            }
        }

        private void StartRespawnSequence()
        {
            if (level == null)
                return;

            // Store current progress before respawning
            SaveProgressBeforeRespawn();

            // Start respawn coroutine
            Add(new Coroutine(RespawnCoroutine()));
        }

        private System.Collections.IEnumerator RespawnCoroutine()
        {
            // Brief delay before respawn
            yield return 0.5f;

            // Respawn player
            PerformRespawn();

            yield return null;
        }

        private void PerformRespawn()
        {
            if (level == null || player == null)
                return;

            // Full heal for respawn
            FullHeal();

            // Reset invincibility
            invincibilityTimer = InvincibilityDuration * 2; // Longer invincibility after respawn

            // Find respawn point
            Vector2 spawnPoint = FindRespawnPoint();

            // Move player to respawn point
            player.Position = spawnPoint;
            player.Speed = Vector2.Zero;
            player.Dead = false;

            // Restore progress
            RestoreProgressAfterRespawn();

            // Visual effects
            level.Flash(Color.White * 0.3f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);

            // Create respawn particles
            for (int i = 0; i < 20; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                level.Particles.Emit(Player.P_Split, 20, spawnPoint, Vector2.One * 8f, angle);
            }

            OnKirbyRespawn?.Invoke();

            Logger.Log(LogLevel.Info, "KirbyHealthController", "Player respawned at: " + spawnPoint);
        }

        private Vector2 FindRespawnPoint()
        {
            // Priority: saved respawn point > current room spawn point > level default spawn
            if (hasSavedRespawn && !string.IsNullOrEmpty(respawnRoom))
            {
                var room = level.Session.MapData?.Get(respawnRoom);
                if (room != null)
                {
                    return respawnPosition;
                }
            }

            // Use session respawn point, or default to current position
            return level.Session.RespawnPoint ?? player.Position;
        }

        #endregion

        #region Progress Preservation

        private void SaveProgressBeforeRespawn()
        {
            // Save current room and position as respawn point
            respawnRoom = level.Session.Level;
            respawnPosition = player.Position;
            hasSavedRespawn = true;

            // Store session flags and inventory (they're automatically preserved)
            Logger.Log(LogLevel.Info, "KirbyHealthController", "Progress saved for respawn in room: " + respawnRoom);
        }

        private void RestoreProgressAfterRespawn()
        {
            // Progress is automatically preserved through session
            // Any additional state can be restored here
            Logger.Log(LogLevel.Info, "KirbyHealthController", "Progress restored after respawn");
        }

        #endregion

        #region Hazard Detection

        private void CheckOutOfBounds()
        {
            if (player == null || level == null)
                return;

            // Check if player fell below level bounds
            float deathY = level.Bounds.Bottom + 64f;
            if (player.Position.Y > deathY)
            {
                DamageFromOutOfBounds();
            }
        }

        private void CheckCrushing()
        {
            if (player == null || level == null)
                return;

            // Crushing is handled by OnSquish in the Player class
            // We hook into that via the Die method override
        }

        #endregion

        #region Event Handlers

        private void OnHealthManagerChanged(int current, int max)
        {
            CurrentHealth = current;
            MaxHealth = max;
            OnHealthChanged?.Invoke(current, max);

            if (current <= 0)
            {
                HandleDeath();
            }
        }

        private void OnHealthManagerDamage(int amount)
        {
            ApplyKnockback(Vector2.Zero); // Generic knockback
            invincibilityTimer = InvincibilityDuration;
        }

        #endregion

        #region Helpers

        private void ApplyKnockback(Vector2 source)
        {
            if (player == null)
                return;

            // Apply knockback force away from damage source
            Vector2 knockbackDir = source == Vector2.Zero
                ? new Vector2(-(int)player.Facing, -0.5f)
                : (player.Position - source).SafeNormalize();

            player.Speed = knockbackDir * 120f;

            // Visual feedback
            level?.Shake(0.2f);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enable the Kirby health controller
        /// </summary>
        public void Enable(int maxHP = 6)
        {
            IsEnabled = true;
            MaxHealth = Math.Max(1, maxHP);

            if (healthManager != null)
            {
                healthManager.EnableKirbyMode(MaxHealth);
                CurrentHealth = healthManager.CurrentHP;
            }
            else
            {
                CurrentHealth = MaxHealth;
            }

            // Ensure UI is visible
            if (healthUI != null)
                healthUI.ShowPlayerHealth = true;

            // Hook into CelesteNet integration for multiplayer health sync
            CelesteNetIntegration.HookIntoHealthSystem(this);

            Logger.Log(LogLevel.Info, "KirbyHealthController", "Initialized with health: " + CurrentHealth + "/" + MaxHealth);
        }

        /// <summary>
        /// Disable the Kirby health controller
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;

            if (healthManager != null)
                healthManager.DisableKirbyMode();

            if (healthUI != null)
                healthUI.ShowPlayerHealth = false;
        }

        /// <summary>
        /// Set a custom respawn point
        /// </summary>
        public void SetRespawnPoint(string roomName, Vector2 position)
        {
            respawnRoom = roomName;
            respawnPosition = position;
            hasSavedRespawn = true;
        }

        /// <summary>
        /// Get the singleton instance, creating if necessary
        /// </summary>
        public static KirbyHealthController GetOrCreate(Level level)
        {
            if (Instance != null && Instance.Scene == level)
                return Instance;

            var existing = level.Tracker.GetEntity<KirbyHealthController>();
            if (existing != null)
                return existing;

            var controller = new KirbyHealthController();
            level.Add(controller);
            return controller;
        }

        #endregion

        #region Damage Types

        public enum DamageType
        {
            Generic,
            Spike,
            Spinner,
            Crush,
            OutOfBounds,
            Boss,
            Enemy
        }

        #endregion
    }
}
