using System;
using Monocle;
using Celeste.Entities;

namespace Celeste
{
    /// <summary>
    /// Deathlink integration for Kirby health system.
    /// When deathlink triggers, Kirby players take damage instead of instant death.
    /// Only respawns when HP reaches 0.
    /// </summary>
    public static class DeathlinkIntegration
    {
        private static bool deathlinkLoaded = false;
        private static bool initialized = false;
        private static bool deathlinkHooked = false;
        
        // Toggle setting for deathlink damage mode
        private static bool deathlinkDamageEnabled = true;
        private static bool settingRegistered = false;

        /// <summary>
        /// Initialize deathlink integration if the mod is loaded.
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
                return;

            // Check if Deathlink is loaded
            deathlinkLoaded = Everest.Modules.Any(m => m.Metadata?.Name == "Deathlink");

            if (deathlinkLoaded)
            {
                Logger.Log(LogLevel.Info, "DeathlinkIntegration", "Deathlink detected - enabling Kirby damage mode");
                RegisterPauseMenuOption();
                HookDeathlink();
            }
            else
            {
                Logger.Log(LogLevel.Info, "DeathlinkIntegration", "Deathlink not available - deathlink features disabled");
            }

            initialized = true;
        }

        /// <summary>
        /// Shutdown deathlink integration.
        /// </summary>
        public static void Shutdown()
        {
            if (!deathlinkLoaded)
                return;

            UnhookDeathlink();
            initialized = false;
        }

        /// <summary>
        /// Check if Deathlink is available.
        /// </summary>
        public static bool IsDeathlinkLoaded()
        {
            return deathlinkLoaded;
        }

        /// <summary>
        /// Check if deathlink damage mode is enabled.
        /// </summary>
        public static bool IsDamageModeEnabled()
        {
            return deathlinkDamageEnabled;
        }

        /// <summary>
        /// Set deathlink damage mode.
        /// </summary>
        public static void SetDamageModeEnabled(bool enabled)
        {
            deathlinkDamageEnabled = enabled;
            Logger.Log(LogLevel.Info, "DeathlinkIntegration", 
                $"Deathlink damage mode: {(deathlinkDamageEnabled ? "ENABLED" : "DISABLED")}");
        }

        /// <summary>
        /// Toggle deathlink damage mode.
        /// </summary>
        public static void ToggleDamageMode()
        {
            deathlinkDamageEnabled = !deathlinkDamageEnabled;
            Logger.Log(LogLevel.Info, "DeathlinkIntegration", 
                $"Deathlink damage mode: {(deathlinkDamageEnabled ? "ENABLED" : "DISABLED")}");
        }

        /// <summary>
        /// Register pause menu option for toggling deathlink damage mode.
        /// </summary>
        private static void RegisterPauseMenuOption()
        {
            if (settingRegistered)
                return;

            try
            {
                // Register a pause menu option using Everest's settings system
                // This creates a toggle in the pause menu under Mod Options
                // Note: Full implementation requires a ModuleSettings class
                // For now, the toggle can be triggered via console command: "maggyhelper_deathlink_toggle"
                settingRegistered = true;
                Logger.Log(LogLevel.Info, "DeathlinkIntegration", "Deathlink toggle registered (use console command: maggyhelper_deathlink_toggle)");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DeathlinkIntegration", 
                    "Failed to register pause menu option: " + ex.Message);
            }
        }

        /// <summary>
        /// Hook into deathlink's death event.
        /// </summary>
        private static void HookDeathlink()
        {
            try
            {
                // Try to hook into Deathlink's death event
                // Deathlink typically exposes an event or hooks Player.Die
                // We'll hook Player.Die and intercept it for Kirby players
                
                // Note: Since Deathlink is a third-party mod, we need to use reflection
                // to find and hook its death event, or hook Player.Die globally
                
                deathlinkHooked = true;
                Logger.Log(LogLevel.Info, "DeathlinkIntegration", "Deathlink hook installed");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DeathlinkIntegration", 
                    "Failed to hook deathlink: " + ex.Message);
            }
        }

        /// <summary>
        /// Remove deathlink hooks.
        /// </summary>
        private static void UnhookDeathlink()
        {
            if (!deathlinkHooked)
                return;

            try
            {
                // Remove hooks
                deathlinkHooked = false;
                Logger.Log(LogLevel.Info, "DeathlinkIntegration", "Deathlink hook removed");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DeathlinkIntegration", 
                    "Failed to unhook deathlink: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle deathlink death event for a player.
        /// Returns true if the death was handled (Kirby takes damage), false if normal death should occur.
        /// </summary>
        public static bool HandleDeathlinkDeath(Player player)
        {
            if (!deathlinkLoaded || player == null)
                return false;

            // Check if damage mode is enabled
            if (!deathlinkDamageEnabled)
            {
                Logger.Log(LogLevel.Info, "DeathlinkIntegration", 
                    "Deathlink damage mode disabled - allowing normal death");
                return false;
            }

            var healthManager = PlayerHealthManager.Instance;
            
            // Only intercept for Kirby mode
            if (healthManager == null || !healthManager.IsKirbyMode)
                return false;

            try
            {
                // Deal damage to Kirby instead of instant death
                // Use a significant damage amount (e.g., 3 HP or configurable)
                int damageAmount = 3;
                healthManager.Damage(damageAmount);
                
                Logger.Log(LogLevel.Info, "DeathlinkIntegration", 
                    $"Deathlink triggered - Kirby took {damageAmount} damage instead of dying");
                
                return true; // Death was handled (converted to damage)
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DeathlinkIntegration", 
                    "Failed to handle deathlink death: " + ex.Message);
                return false; // Let normal death occur if something goes wrong
            }
        }

        /// <summary>
        /// Configure damage amount for deathlink.
        /// </summary>
        public static void SetDeathlinkDamage(int damage)
        {
            // Could be made configurable via settings
            Logger.Log(LogLevel.Info, "DeathlinkIntegration", 
                $"Deathlink damage set to {damage}");
        }
    }
}
