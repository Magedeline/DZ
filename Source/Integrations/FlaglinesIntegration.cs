using System;
using System.Linq;
using Monocle;

namespace Celeste.Integrations
{
    /// <summary>
    /// FlaglinesAndSuch integration for Kirby health system.
    /// Ensures compatibility with FlaglinesAndSuch decorative/functional entities.
    /// </summary>
    public static class FlaglinesIntegration
    {
        private static bool flaglinesLoaded = false;
        private static bool initialized = false;

        /// <summary>
        /// Initialize FlaglinesAndSuch integration if the mod is loaded.
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
                return;

            // Check if FlaglinesAndSuch is loaded
            flaglinesLoaded = Everest.Modules.Any(m => m.Metadata?.Name == "FlaglinesAndSuch");

            if (flaglinesLoaded)
            {
                Logger.Log(LogLevel.Info, "FlaglinesIntegration", "FlaglinesAndSuch detected - enabling compatibility");
            }
            else
            {
                Logger.Log(LogLevel.Info, "FlaglinesIntegration", "FlaglinesAndSuch not available - no special handling needed");
            }

            initialized = true;
        }

        /// <summary>
        /// Shutdown FlaglinesAndSuch integration.
        /// </summary>
        public static void Shutdown()
        {
            // No special cleanup needed
            initialized = false;
        }

        /// <summary>
        /// Check if FlaglinesAndSuch is available.
        /// </summary>
        public static bool IsFlaglinesLoaded => flaglinesLoaded;

        /// <summary>
        /// Check if entity is a FlaglinesAndSuch entity.
        /// </summary>
        public static bool IsFlaglinesEntity(Entity entity)
        {
            if (!flaglinesLoaded || entity == null)
                return false;

            // FlaglinesAndSuch entities are decorative/functional
            // They generally don't interact with health system
            // TODO: Add actual FlaglinesAndSuch entity type checks if needed
            return false;
        }
    }
}
