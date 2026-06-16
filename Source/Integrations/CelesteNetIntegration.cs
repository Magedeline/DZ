using System;
using System.Collections.Generic;
using System.Linq;
using Monocle;

namespace DZ
{
    /// <summary>
    /// CelesteNet integration for Kirby health system.
    /// Syncs health state between players in multiplayer sessions.
    /// </summary>
    public static class CelesteNetIntegration
    {
        private static bool celesteNetLoaded = false;
        private static bool initialized = false;
        private static Dictionary<int, MultiplayerHealthBar> playerHealthBars = new Dictionary<int, MultiplayerHealthBar>();
        private static MultiplayerDebugHUD debugHUD;

        /// <summary>
        /// Initialize CelesteNet integration if the mod is loaded.
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
                return;

            // Check if CelesteNet is loaded
            celesteNetLoaded = Everest.Modules.Any(m => m.Metadata?.Name == "CelesteNet.Client");

            if (celesteNetLoaded)
            {
                Logger.Log(LogLevel.Info, "CelesteNetIntegration", "CelesteNet detected - enabling health sync");
                InitializeHealthSync();
                
                // Add debug HUD to current scene if available
                if (Engine.Scene != null)
                {
                    debugHUD = new MultiplayerDebugHUD();
                    Engine.Scene.Add(debugHUD);
                    Logger.Log(LogLevel.Info, "CelesteNetIntegration", "Debug HUD added to scene");
                }
            }
            else
            {
                Logger.Log(LogLevel.Info, "CelesteNetIntegration", "CelesteNet not available - multiplayer features disabled");
            }

            initialized = true;
        }

        /// <summary>
        /// Shutdown CelesteNet integration.
        /// </summary>
        public static void Shutdown()
        {
            if (!celesteNetLoaded)
                return;

            ShutdownHealthSync();
            ClearHealthBars();
            
            // Remove debug HUD
            if (debugHUD != null)
            {
                debugHUD.RemoveSelf();
                debugHUD = null;
            }
            
            initialized = false;
        }

        /// <summary>
        /// Check if CelesteNet is available and in a multiplayer session.
        /// </summary>
        public static bool IsMultiplayerSession()
        {
            if (!celesteNetLoaded)
                return false;

            // Check if we're in a CelesteNet session
            // This would use CelesteNet's API to check session status
            try
            {
                // TODO: Add actual CelesteNet session check
                // For now, return true if CelesteNet is loaded (for testing)
                return celesteNetLoaded;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sync health state to other players.
        /// </summary>
        public static void SyncHealthState(int currentHealth, int maxHealth, bool isDead)
        {
            if (!celesteNetLoaded || !IsMultiplayerSession())
                return;

            try
            {
                // TODO: Send health sync packet via CelesteNet
                Logger.Log(LogLevel.Verbose, "CelesteNetIntegration", 
                    $"Syncing health: {currentHealth}/{maxHealth}, Dead: {isDead}");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "CelesteNetIntegration", 
                    "Failed to sync health: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle incoming health sync from other players.
        /// </summary>
        private static void OnHealthSyncReceived(int playerId, string playerName, int currentHealth, int maxHealth, bool isDead, bool isKirbyMode, float stamina)
        {
            // Update debug HUD
            if (debugHUD != null)
            {
                debugHUD.UpdateRemotePlayer(playerId, playerName, currentHealth, maxHealth, isDead, isKirbyMode, stamina);
            }
            
            Logger.Log(LogLevel.Verbose, "CelesteNetIntegration", 
                $"Received health sync from player {playerId} ({playerName}): {currentHealth}/{maxHealth}");
        }

        private static void InitializeHealthSync()
        {
            // TODO: Register CelesteNet packet handlers for health sync
            Logger.Log(LogLevel.Info, "CelesteNetIntegration", "Health sync initialized");
        }

        private static void ShutdownHealthSync()
        {
            // TODO: Unregister CelesteNet packet handlers
            Logger.Log(LogLevel.Info, "CelesteNetIntegration", "Health sync shut down");
        }

        // Note: HookIntoHealthSystem was removed because KirbyHealthController no longer exists.
        // Health is now managed directly within K_Player. If CelesteNet health sync is needed
        // in the future, it should be implemented by hooking into K_Player events instead.

        /// <summary>
        /// Register a multiplayer player for health bar display.
        /// </summary>
        public static void RegisterPlayer(int playerId, Player player)
        {
            if (!celesteNetLoaded || player == null || player.Scene == null)
                return;

            // Create health bar for this player if it doesn't exist
            if (!playerHealthBars.ContainsKey(playerId))
            {
                var healthBar = MultiplayerHealthBar.GetOrCreateForPlayer(player.Scene, player);
                playerHealthBars[playerId] = healthBar;
                Logger.Log(LogLevel.Info, "CelesteNetIntegration", $"Registered health bar for player {playerId}");
            }
        }

        /// <summary>
        /// Unregister a multiplayer player.
        /// </summary>
        public static void UnregisterPlayer(int playerId, string playerName = null)
        {
            if (playerHealthBars.TryGetValue(playerId, out var healthBar))
            {
                healthBar.RemoveSelf();
                playerHealthBars.Remove(playerId);
                Logger.Log(LogLevel.Info, "CelesteNetIntegration", $"Unregistered health bar for player {playerId}");
            }
            
            // Remove from debug HUD
            if (debugHUD != null)
            {
                debugHUD.RemovePlayer(playerId);
            }
        }

        /// <summary>
        /// Clear all health bars.
        /// </summary>
        private static void ClearHealthBars()
        {
            foreach (var healthBar in playerHealthBars.Values)
            {
                healthBar?.RemoveSelf();
            }
            playerHealthBars.Clear();
        }

        /// <summary>
        /// Update health bar for a specific player.
        /// </summary>
        public static void UpdatePlayerHealthBar(int playerId, Player player)
        {
            if (playerHealthBars.TryGetValue(playerId, out var healthBar) && healthBar != null)
            {
                healthBar.SetTargetPlayer(player);
            }
        }
    }
}
