using System;
using System.Linq;
using Monocle;

namespace Celeste.Integrations
{
    /// <summary>
    /// BounceHelper integration for Kirby health system.
    /// Ensures compatibility with BounceHelper's modified physics and mechanics.
    /// </summary>
    public static class BounceHelperIntegration
    {
        private static bool bounceHelperLoaded = false;
        private static bool initialized = false;

        /// <summary>
        /// Initialize BounceHelper integration if the mod is loaded.
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
                return;

            // Check if BounceHelper is loaded
            bounceHelperLoaded = Everest.Modules.Any(m => m.Metadata?.Name == "BounceHelper");

            if (bounceHelperLoaded)
            {
                Logger.Log(LogLevel.Info, "BounceHelperIntegration", "BounceHelper detected - enabling compatibility");
                InitializeBounceCompatibility();
            }
            else
            {
                Logger.Log(LogLevel.Info, "BounceHelperIntegration", "BounceHelper not available - using vanilla physics");
            }

            initialized = true;
        }

        /// <summary>
        /// Shutdown BounceHelper integration.
        /// </summary>
        public static void Shutdown()
        {
            if (!bounceHelperLoaded)
                return;

            ShutdownBounceCompatibility();
            initialized = false;
        }

        /// <summary>
        /// Check if BounceHelper is available.
        /// </summary>
        public static bool IsBounceHelperLoaded => bounceHelperLoaded;

        /// <summary>
        /// Check if a bounce pad should trigger health damage in Kirby mode.
        /// </summary>
        public static bool ShouldBouncePadDamagePlayer()
        {
            if (!bounceHelperLoaded)
                return false;

            // BounceHelper's bounce pads are generally safe
            // They don't deal damage, just provide bounce physics
            return false;
        }

        /// <summary>
        /// Adjust knockback for BounceHelper's modified physics.
        /// </summary>
        public static float AdjustKnockbackForce(float originalForce)
        {
            if (!bounceHelperLoaded)
                return originalForce;

            // BounceHelper may modify physics, so adjust knockback accordingly
            // TODO: Fine-tune this value based on BounceHelper's physics changes
            return originalForce * 1.0f;
        }

        /// <summary>
        /// Check if entity is a BounceHelper entity that should interact with health system.
        /// </summary>
        public static bool IsBounceHelperEntity(Entity entity)
        {
            if (!bounceHelperLoaded || entity == null)
                return false;

            // Check if entity is from BounceHelper
            // TODO: Add actual BounceHelper entity type checks
            return false;
        }

        private static void InitializeBounceCompatibility()
        {
            // TODO: Add hooks for BounceHelper-specific entities if needed
            Logger.Log(LogLevel.Info, "BounceHelperIntegration", "Bounce compatibility initialized");
        }

        private static void ShutdownBounceCompatibility()
        {
            // TODO: Remove BounceHelper hooks
            Logger.Log(LogLevel.Info, "BounceHelperIntegration", "Bounce compatibility shut down");
        }
    }
}
