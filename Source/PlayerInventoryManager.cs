using Celeste.Extensions;
using MonoMod.Utils;

namespace Celeste
{
    /// <summary>
    /// Manages player inventory states based on refill types from the Celeste mod.
    /// Uses dash count from AdvancedRefill to define inventory types, integrating with
    /// KirbyPlayerExtension system for Kirby-specific functionality.
    /// </summary>
    public static class PlayerInventoryManager
    {
        // Session flag constants for inventory persistence
        private const string FLAG_KIRBY_MODE = "kirby_mode";
        private const string FLAG_CURRENT_DASH_INVENTORY = "current_dash_inventory";

        /// <summary>
        /// Refill-based inventory types corresponding to dash counts from AdvancedRefill
        /// </summary>
        public enum RefillInventoryType
        {
            /// <summary>Standard refill (1 dash) - Default mode</summary>
            Standard = 1,
            /// <summary>Two-dash refill (pink) - Heart mode</summary>
            TwoDash = 2,
            /// <summary>Solar refill (3 dashes) - Titan Tower mode</summary>
            Solar = 3,
            /// <summary>Lunar refill (4 dashes) - TheEnd mode</summary>
            Lunar = 4,
            /// <summary>Black hole refill (5 dashes) - Corruption mode</summary>
            BlackHole = 5,
            /// <summary>Save star refill (10+ dashes) - Kirby mode</summary>
            SaveStar = 10
        }

        /// <summary>
        /// Currently active refill-based inventory type
        /// </summary>
        public static RefillInventoryType CurrentInventory { get; private set; } = RefillInventoryType.Standard;

        /// <summary>
        /// Enable Standard inventory (1 dash) - equivalent to Default
        /// </summary>
        public static void EnableStandardInventory(Level level)
        {
            if (level?.Session == null) return;

            CurrentInventory = RefillInventoryType.Standard;
            StoreInventoryInSession(level, RefillInventoryType.Standard);

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.DisableKirbyMode();
                SetPlayerDashesWithLessDasheline(player, 1);
            }

            IngesteLogger.Info("Standard inventory (1 dash) enabled");
        }

        /// <summary>
        /// Enable Two-Dash inventory (pink refill) - Heart power mode
        /// </summary>
        public static void EnableTwoDashInventory(Level level)
        {
            if (level?.Session == null) return;

            CurrentInventory = RefillInventoryType.TwoDash;
            StoreInventoryInSession(level, RefillInventoryType.TwoDash);

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                SetPlayerDashesWithLessDasheline(player, 2);
            }

            IngesteLogger.Info("Two-Dash inventory (Heart power) enabled");
        }

        /// <summary>
        /// Enable Solar inventory (3 dashes) - Titan Tower Climbing mode
        /// </summary>
        public static void EnableSolarInventory(Level level)
        {
            if (level?.Session == null) return;

            CurrentInventory = RefillInventoryType.Solar;
            StoreInventoryInSession(level, RefillInventoryType.Solar);

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                SetPlayerDashesWithLessDasheline(player, 3);
            }

            IngesteLogger.Info("Solar inventory (Titan Tower - 3 dashes) enabled");
        }

        /// <summary>
        /// Enable Lunar inventory (4 dashes) - TheEnd mode
        /// </summary>
        public static void EnableLunarInventory(Level level)
        {
            if (level?.Session == null) return;

            CurrentInventory = RefillInventoryType.Lunar;
            StoreInventoryInSession(level, RefillInventoryType.Lunar);

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                SetPlayerDashesWithLessDasheline(player, 4);
            }

            IngesteLogger.Info("Lunar inventory (TheEnd - 4 dashes) enabled");
        }

        /// <summary>
        /// Enable Black Hole inventory (5 dashes) - Corruption mode
        /// </summary>
        public static void EnableBlackHoleInventory(Level level)
        {
            if (level?.Session == null) return;

            CurrentInventory = RefillInventoryType.BlackHole;
            StoreInventoryInSession(level, RefillInventoryType.BlackHole);

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                SetPlayerDashesWithLessDasheline(player, 5);
            }

            IngesteLogger.Info("Black Hole inventory (Corruption - 5 dashes) enabled");
        }

        /// <summary>
        /// Enable Save Star inventory (10 dashes) - Kirby player mode with special abilities
        /// </summary>
        public static void EnableSaveStarInventory(Level level)
        {
            if (level?.Session == null) return;

            CurrentInventory = RefillInventoryType.SaveStar;
            StoreInventoryInSession(level, RefillInventoryType.SaveStar);
            level.Session.SetFlag(FLAG_KIRBY_MODE, true);

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.EnableKirbyMode();
                SetPlayerDashesWithLessDasheline(player, 10);
            }

            IngesteLogger.Info("Save Star inventory (Kirby mode - 10 dashes) enabled");
        }

        /// <summary>
        /// Enable Save Star inventory with a specific Kirby power state
        /// </summary>
        public static void EnableSaveStarInventory(Level level, KirbyMode.KirbyPowerState powerState)
        {
            EnableSaveStarInventory(level);

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            player?.SetKirbyPowerState(powerState);

            IngesteLogger.Info($"Save Star inventory enabled with Kirby power: {powerState}");
        }

        /// <summary>
        /// Set inventory based on a specific dash count (from refill pickup)
        /// </summary>
        public static void SetInventoryFromDashCount(Level level, int dashCount)
        {
            var inventoryType = GetInventoryTypeFromDashCount(dashCount);
            SetInventory(level, inventoryType);
        }

        /// <summary>
        /// Set the player's inventory to a specific refill type
        /// </summary>
        public static void SetInventory(Level level, RefillInventoryType inventoryType)
        {
            switch (inventoryType)
            {
                case RefillInventoryType.Standard:
                    EnableStandardInventory(level);
                    break;
                case RefillInventoryType.TwoDash:
                    EnableTwoDashInventory(level);
                    break;
                case RefillInventoryType.Solar:
                    EnableSolarInventory(level);
                    break;
                case RefillInventoryType.Lunar:
                    EnableLunarInventory(level);
                    break;
                case RefillInventoryType.BlackHole:
                    EnableBlackHoleInventory(level);
                    break;
                case RefillInventoryType.SaveStar:
                    EnableSaveStarInventory(level);
                    break;
                default:
                    EnableStandardInventory(level);
                    break;
            }
        }

        /// <summary>
        /// Reset to default/standard inventory state
        /// </summary>
        public static void ResetToDefault(Level level)
        {
            if (level?.Session == null) return;

            level.Session.SetFlag(FLAG_KIRBY_MODE, false);
            EnableStandardInventory(level);

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            player?.DisableKirbyMode();

            IngesteLogger.Info("Reset to default (standard) inventory");
        }

        /// <summary>
        /// Get the maximum number of dashes for a given refill inventory type
        /// </summary>
        public static int GetMaxDashesForInventory(RefillInventoryType inventoryType)
        {
            return (int)inventoryType;
        }

        /// <summary>
        /// Get the refill inventory type from a dash count
        /// </summary>
        public static RefillInventoryType GetInventoryTypeFromDashCount(int dashCount)
        {
            return dashCount switch
            {
                1 => RefillInventoryType.Standard,
                2 => RefillInventoryType.TwoDash,
                3 => RefillInventoryType.Solar,
                4 => RefillInventoryType.Lunar,
                5 => RefillInventoryType.BlackHole,
                >= 10 => RefillInventoryType.SaveStar,
                _ => RefillInventoryType.Standard
            };
        }

        /// <summary>
        /// Check if Kirby mode is currently active
        /// </summary>
        public static bool IsKirbyModeActive(Level level)
        {
            return level?.Session?.GetFlag(FLAG_KIRBY_MODE) ?? false;
        }

        /// <summary>
        /// Get current inventory type from session
        /// </summary>
        public static RefillInventoryType GetCurrentInventoryFromSession(Level level)
        {
            if (level?.Session == null) return RefillInventoryType.Standard;

            // Check Kirby mode flag first
            if (level.Session.GetFlag(FLAG_KIRBY_MODE))
            {
                return RefillInventoryType.SaveStar;
            }

            // Retrieve stored dash inventory from session counter
            int storedDashCount = level.Session.GetCounter(FLAG_CURRENT_DASH_INVENTORY);
            if (storedDashCount > 0)
            {
                return GetInventoryTypeFromDashCount(storedDashCount);
            }

            return RefillInventoryType.Standard;
        }

        /// <summary>
        /// Get the sprite name for the current inventory type (matches AdvancedRefill sprites)
        /// </summary>
        public static string GetSpriteNameForInventory(RefillInventoryType inventoryType)
        {
            return inventoryType switch
            {
                RefillInventoryType.TwoDash => "refillTwo",
                RefillInventoryType.Solar => "solarrefill",
                RefillInventoryType.Lunar => "lunarrefill",
                RefillInventoryType.BlackHole => "blackholerefill",
                RefillInventoryType.SaveStar => "savestarrefill",
                _ => "refill"
            };
        }

        /// <summary>
        /// Get the color associated with an inventory type (matches AdvancedRefill colors)
        /// </summary>
        public static Color GetColorForInventory(RefillInventoryType inventoryType)
        {
            return inventoryType switch
            {
                RefillInventoryType.TwoDash => Color.Pink,
                RefillInventoryType.Solar => Color.Orange,
                RefillInventoryType.Lunar => Color.LightBlue,
                RefillInventoryType.BlackHole => Color.Purple,
                RefillInventoryType.SaveStar => Color.Gold,
                _ => Color.White
            };
        }

        /// <summary>
        /// Get the sound event for the current inventory type (matches AdvancedRefill sounds)
        /// </summary>
        public static string GetSoundForInventory(RefillInventoryType inventoryType)
        {
            return inventoryType switch
            {
                RefillInventoryType.TwoDash => "event:/game/general/refill_two_get",
                RefillInventoryType.Solar => "event:/desolozantas/game/general/reddiamond_touch",
                RefillInventoryType.Lunar => "event:/desolozantas/game/general/cyandiamond_touch",
                RefillInventoryType.BlackHole => "event:/desolozantas/final_content/game/19_the_end/gigadiamond_touch",
                RefillInventoryType.SaveStar => "event:/desolozantas/final_content/game/20_last_push/savediamond_touch",
                _ => "event:/game/general/refill_get"
            };
        }

        /// <summary>
        /// Sets the player's dashes using LessDasheline-compatible DynData method.
        /// Properly handles extended dash counts (3-10) for compatibility with LessDasheline/MoreDasheline mods.
        /// </summary>
        private static void SetPlayerDashesWithLessDasheline(global::Celeste.Player player, int dashes)
        {
            int previousDashes = player.Dashes;

            using (var dynData = new DynData<global::Celeste.Player>(player))
            {
                player.Dashes = dashes;

                // For extended dashes (3+), set LessDasheline-compatible fields
                if (dashes >= 3)
                {
                    dynData.Set("LessDasheline/rechargeAt", previousDashes);
                    dynData.Set("LessDasheline/rechargeInto", dashes);
                    dynData.Set("LessDasheline/rechargeTimer", 0.12f);
                    dynData.Set("LessDasheline/startDashCount", dashes);
                }
            }
        }

        /// <summary>
        /// Store inventory type in session for persistence across room transitions
        /// </summary>
        private static void StoreInventoryInSession(Level level, RefillInventoryType inventoryType)
        {
            level.Session.SetCounter(FLAG_CURRENT_DASH_INVENTORY, (int)inventoryType);
        }

        #region Legacy Compatibility Methods

        // These methods maintain backwards compatibility with the old PlayerInventoryTrigger.InventoryType enum

        /// <summary>
        /// Enable Heart power mode - maps to TwoDash inventory (legacy compatibility)
        /// </summary>
        public static void EnableHeartPower(Level level) => EnableTwoDashInventory(level);

        /// <summary>
        /// Enable Kirby player mode - maps to SaveStar inventory (legacy compatibility)
        /// </summary>
        public static void EnableKirbyPlayer(Level level) => EnableSaveStarInventory(level);

        /// <summary>
        /// Enable Kirby player mode with power state (legacy compatibility)
        /// </summary>
        public static void EnableKirbyPlayer(Level level, KirbyMode.KirbyPowerState powerState) 
            => EnableSaveStarInventory(level, powerState);

        /// <summary>
        /// Enable Say Goodbye mode - maps to TwoDash inventory (legacy compatibility)
        /// </summary>
        public static void EnableSayGoodbye(Level level) => EnableTwoDashInventory(level);

        /// <summary>
        /// Enable Titan Tower Climbing mode - maps to Solar inventory (legacy compatibility)
        /// </summary>
        public static void EnableTitanTowerClimbing(Level level) => EnableSolarInventory(level);

        /// <summary>
        /// Enable Corruption mode - maps to BlackHole inventory (legacy compatibility)
        /// </summary>
        public static void EnableCorruption(Level level) => EnableBlackHoleInventory(level);

        /// <summary>
        /// Enable TheEnd mode - maps to Lunar inventory (legacy compatibility)
        /// </summary>
        public static void EnableTheEnd(Level level) => EnableLunarInventory(level);

        /// <summary>
        /// Get max dashes for legacy InventoryType (legacy compatibility)
        /// </summary>
        public static int GetMaxDashesForInventory(Triggers.PlayerInventoryTrigger.InventoryType inventoryType)
        {
            return inventoryType switch
            {
                Triggers.PlayerInventoryTrigger.InventoryType.KirbyPlayer => 10,
                Triggers.PlayerInventoryTrigger.InventoryType.SayGoodbye => 2,
                Triggers.PlayerInventoryTrigger.InventoryType.TitanTowerClimbing => 3,
                Triggers.PlayerInventoryTrigger.InventoryType.Corruption => 5,
                Triggers.PlayerInventoryTrigger.InventoryType.TheEnd => 4,
                _ => 1
            };
        }

        /// <summary>
        /// Get current inventory from session as legacy type (legacy compatibility)
        /// </summary>
        public static Triggers.PlayerInventoryTrigger.InventoryType GetCurrentInventoryAsLegacyType(Level level)
        {
            var refillType = GetCurrentInventoryFromSession(level);
            return refillType switch
            {
                RefillInventoryType.TwoDash => Triggers.PlayerInventoryTrigger.InventoryType.CH6End,
                RefillInventoryType.Solar => Triggers.PlayerInventoryTrigger.InventoryType.TitanTowerClimbing,
                RefillInventoryType.Lunar => Triggers.PlayerInventoryTrigger.InventoryType.TheEnd,
                RefillInventoryType.BlackHole => Triggers.PlayerInventoryTrigger.InventoryType.Corruption,
                RefillInventoryType.SaveStar => Triggers.PlayerInventoryTrigger.InventoryType.KirbyPlayer,
                _ => Triggers.PlayerInventoryTrigger.InventoryType.Default
            };
        }

        #endregion
    }
}
