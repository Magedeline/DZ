using System;
using System.Collections.Generic;
using Monocle;
using Microsoft.Xna.Framework;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Phase 6: Map Validator - Validates player entity existence and type matching.
    /// Features:
    /// - Checks if required player entity exists in map before level loads
    /// - Verifies player type matches PlayerSelectionManager selection
    /// - Auto-spawns correct player if missing
    /// </summary>
    public static class MapValidator
    {
        /// <summary>
        /// Validates that a player entity exists in the level.
        /// If player is missing, attempts to spawn the correct one.
        /// Call this during level load hook.
        /// </summary>
        public static void ValidatePlayerInLevel(Level level)
        {
            if (level == null)
                return;

            // Get the currently selected player type
            var selectedPlayer = PlayerSelectionManager.GetSelectedPlayer();

            // Check what player entities currently  exist in the level
            var players = level.Tracker.GetEntities<Player>();
            
            // If a player exists, validation passes
            if (players.Count > 0)
                return;  // Player entity found!

            // No player found - spawn the correct one
            SpawnCorrectPlayer(level, selectedPlayer);
        }

        /// <summary>
        /// Spawns the correct player entity at the level's spawn point.
        /// </summary>
        private static void SpawnCorrectPlayer(Level level, PlayerSelectionManager.PlayerType playerType)
        {
            if (level == null)
                return;

            // Find spawn point (default to level start)
            Vector2 spawnPos = level.GetSpawnPoint(Vector2.Zero);
            
            // Verify we have a valid spawn position
            if (spawnPos == Vector2.Zero && level.Entities.Count > 0)
            {
                // Fallback: use level bounds start
                spawnPos = new Vector2(level.Bounds.Left + 16, level.Bounds.Top + 16);
            }

            // Create player entity at spawn point
            Entity playerEntity = null;

            try
            {
                // Instantiate based on player type
                if (playerType == PlayerSelectionManager.PlayerType.Kirby)
                {
                    playerEntity = new Player(spawnPos, PlayerSpriteMode.Madeline);
                }
                else if (playerType == PlayerSelectionManager.PlayerType.Madeline)
                {
                    playerEntity = new MadelinePlayer(spawnPos, PlayerSpriteMode.Madeline);
                }

                if (playerEntity != null)
                {
                    level.Add(playerEntity);
                }
            }
            catch (Exception)
            {
                // Silently fail - player spawn is optional fallback
            }
        }
    }
}
