using System;
using Celeste.Entities;
using Celeste.Extensions;
using Celeste.Mod;
using Celeste.Mod.KIRBY_CELESTE;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste
{
    /// <summary>
    /// Hooks into vanilla Celeste systems to integrate Kirby health mechanics.
    /// Handles spikes, spinners, crushing, and death interception.
    /// </summary>
    public static class KirbyHealthSystemHooks
    {
        private static Hook hookPlayerDie;
        private static Hook hookPlayerOnSquish;
        private static Hook hookCrystalStaticSpinnerOnPlayer;
        private static Hook hookSpikeOnPlayer;

        private static bool hooksLoaded = false;

        /// <summary>
        /// Load all hooks
        /// </summary>
        public static void Load()
        {
            if (hooksLoaded)
                return;

            try
            {
                // Hook Player.Die to intercept death in Kirby mode
                // Must specify parameter types explicitly - Player.Die has multiple overloads
                var dieMethod = typeof(global::Celeste.Player).GetMethod("Die",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] { typeof(Vector2), typeof(bool), typeof(bool) },
                    null);
                var dieHook = typeof(KirbyHealthSystemHooks).GetMethod("PlayerDieHook",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (dieMethod != null && dieHook != null)
                {
                    hookPlayerDie = new Hook(dieMethod, dieHook);
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "KirbyHealthSystemHooks", "Could not find Player.Die(Vector2, bool, bool) - skipping hook");
                }

                // Hook Player.OnSquish to handle crushing damage
                var squishMethod = typeof(global::Celeste.Player).GetMethod("OnSquish",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] { typeof(CollisionData) },
                    null);
                var squishHook = typeof(KirbyHealthSystemHooks).GetMethod("PlayerOnSquishHook",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (squishMethod != null && squishHook != null)
                {
                    hookPlayerOnSquish = new Hook(squishMethod, squishHook);
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "KirbyHealthSystemHooks", "Could not find Player.OnSquish(CollisionData) - skipping hook");
                }

                // Hook CrystalStaticSpinner.OnPlayer for spinner damage
                var spinnerType = typeof(CrystalStaticSpinner);
                if (spinnerType != null)
                {
                    var spinnerMethod = spinnerType.GetMethod("OnPlayer",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (spinnerMethod != null)
                    {
                        var spinnerHook = typeof(KirbyHealthSystemHooks).GetMethod("SpinnerOnPlayerHook",
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                        if (spinnerHook != null)
                        {
                            hookCrystalStaticSpinnerOnPlayer = new Hook(spinnerMethod, spinnerHook);
                        }
                    }
                }

                // Hook Spike.OnCollide for spike damage
                var spikeType = typeof(global::Celeste.Spikes);
                if (spikeType != null)
                {
                    var spikeMethod = spikeType.GetMethod("OnCollide",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (spikeMethod != null)
                    {
                        var spikeHook = typeof(KirbyHealthSystemHooks).GetMethod("SpikeOnCollideHook",
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                        if (spikeHook != null)
                        {
                            hookSpikeOnPlayer = new Hook(spikeMethod, spikeHook);
                        }
                    }
                }

                hooksLoaded = true;
                Logger.Log(LogLevel.Info, "KirbyHealthSystemHooks", "Hooks loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "KirbyHealthSystemHooks", "Failed to load hooks: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Unload all hooks
        /// </summary>
        public static void Unload()
        {
            if (!hooksLoaded)
                return;

            hookPlayerDie?.Dispose();
            hookPlayerOnSquish?.Dispose();
            hookCrystalStaticSpinnerOnPlayer?.Dispose();
            hookSpikeOnPlayer?.Dispose();

            hookPlayerDie = null;
            hookPlayerOnSquish = null;
            hookCrystalStaticSpinnerOnPlayer = null;
            hookSpikeOnPlayer = null;

            hooksLoaded = false;
        }

        /// <summary>
        /// Hook for Player.Die - intercepts death in Kirby mode and converts to health damage
        /// Also handles deathlink by damaging Kirby instead of instant death
        /// </summary>
        private static global::Celeste.PlayerDeadBody PlayerDieHook(
            Func<global::Celeste.Player, Vector2, bool, bool, global::Celeste.PlayerDeadBody> orig,
            global::Celeste.Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            // Check if deathlink is loaded and handle it for Kirby players
            if (DeathlinkIntegration.IsDeathlinkLoaded() && DeathlinkIntegration.HandleDeathlinkDeath(self))
            {
                // Deathlink handled - Kirby took damage, return null to prevent death
                return null;
            }

            // Check if Kirby mode is active
            if (!self.IsKirbyMode())
            {
                // Not in Kirby mode, use normal death
                return orig(self, direction, evenIfInvincible, registerDeathInStats);
            }

            // Get the health controller
            var controller = KirbyHealthController.Instance;
            if (controller == null || !controller.IsEnabled)
            {
                // No health controller, use normal death
                return orig(self, direction, evenIfInvincible, registerDeathInStats);
            }

            // Check if player is already dead
            if (controller.IsDead)
            {
                // Health is 0, allow death to proceed for respawn
                return orig(self, direction, evenIfInvincible, registerDeathInStats);
            }

            // In Kirby mode with health remaining - don't die, just take damage
            // The health controller will handle the visual feedback
            Logger.Log(LogLevel.Info, "KirbyHealthSystemHooks", "Death intercepted - health remaining: " + controller.CurrentHealth);

            // Return null to prevent death body from spawning
            return null;
        }

        /// <summary>
        /// Hook for Player.OnSquish - handles crushing damage
        /// </summary>
        private static void PlayerOnSquishHook(
            Action<global::Celeste.Player, CollisionData> orig,
            global::Celeste.Player self, CollisionData data)
        {
            // Check if Kirby mode is active
            if (!self.IsKirbyMode())
            {
                orig(self, data);
                return;
            }

            // Get the health controller
            var controller = KirbyHealthController.Instance;
            if (controller == null || !controller.IsEnabled)
            {
                orig(self, data);
                return;
            }

            // Try to wiggle out first (vanilla behavior)
            bool ducked = false;
            if (!self.Ducking)
            {
                ducked = true;
                self.Ducking = true;
                data.Pusher.Collidable = true;

                if (!self.CollideCheck<Solid>())
                {
                    data.Pusher.Collidable = false;
                    return;
                }

                var was = self.Position;
                self.Position = data.TargetPosition;
                if (!self.CollideCheck<Solid>())
                {
                    data.Pusher.Collidable = false;
                    return;
                }

                self.Position = was;
                data.Pusher.Collidable = false;
            }

            // Check if we can wiggle out
            if (!TrySquishWiggle(self, data))
            {
                // Can't wiggle out - apply crushing damage (instant death in Kirby mode)
                Logger.Log(LogLevel.Info, "KirbyHealthSystemHooks", "Crushing damage applied");
                controller.DamageFromCrush();

                // Check if we died from the crush
                if (controller.IsDead)
                {
                    // Let the death proceed normally for respawn handling
                    orig(self, data);
                }
            }
            else if (ducked && CanUnDuck(self))
            {
                self.Ducking = false;
            }
        }

        /// <summary>
        /// Hook for CrystalStaticSpinner.OnPlayer - handles spinner damage
        /// </summary>
        private static void SpinnerOnPlayerHook(
            Action<CrystalStaticSpinner, global::Celeste.Player> orig,
            CrystalStaticSpinner self, global::Celeste.Player player)
        {
            // Check if Kirby mode is active
            if (!player.IsKirbyMode())
            {
                orig(self, player);
                return;
            }

            // Get the health controller
            var controller = KirbyHealthController.Instance;
            if (controller == null || !controller.IsEnabled)
            {
                orig(self, player);
                return;
            }

            // Check if player is dashing or moving fast enough to break through
            const float MinFlingSpeed = 220f;
            const float MinFlingSpeedSq = MinFlingSpeed * MinFlingSpeed;

            if (player.StateMachine.State == global::Celeste.Player.StDash ||
                player.Speed.LengthSquared() >= MinFlingSpeedSq)
            {
                // Player is dashing or moving fast - don't hurt
                return;
            }

            // Apply spinner damage through health controller
            if (controller.DamageFromSpinner(self.Position))
            {
                // Damage applied successfully - don't call original Die
                Logger.Log(LogLevel.Info, "KirbyHealthSystemHooks", "Spinner damage applied");

                // If still alive, push player away from spinner
                if (!controller.IsDead)
                {
                    Vector2 pushDir = (player.Position - self.Position).SafeNormalize();
                    player.Speed = pushDir * 100f;
                }
                else
                {
                    // Player died from damage, let original death handler run
                    orig(self, player);
                }
            }
        }

        /// <summary>
        /// Hook for Spike.OnCollide - handles spike damage
        /// </summary>
        private static void SpikeOnCollideHook(
            Action<global::Celeste.Spikes, global::Celeste.Player> orig,
            global::Celeste.Spikes self, global::Celeste.Player player)
        {
            // Check if Kirby mode is active
            if (!player.IsKirbyMode())
            {
                orig(self, player);
                return;
            }

            // Get the health controller
            var controller = KirbyHealthController.Instance;
            if (controller == null || !controller.IsEnabled)
            {
                orig(self, player);
                return;
            }

            // Apply spike damage through health controller
            if (controller.DamageFromSpike(self.Position))
            {
                Logger.Log(LogLevel.Info, "KirbyHealthSystemHooks", "Spike damage applied");

                if (controller.IsDead)
                {
                    // Player died, let original death handler run
                    orig(self, player);
                }
            }
        }

        #region Helper Methods

        private static bool TrySquishWiggle(global::Celeste.Player player, CollisionData data)
        {
            // Replicate vanilla squish wiggle behavior
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if (Math.Abs(i) + Math.Abs(j) > 3)
                        continue;

                    Vector2 offset = new Vector2(i * 2, j * 2);
                    player.Position += offset;
                    data.Pusher.Collidable = true;

                    bool colliding = player.CollideCheck<Solid>();
                    data.Pusher.Collidable = false;

                    if (!colliding)
                        return true;

                    player.Position -= offset;
                }
            }
            return false;
        }

        private static bool CanUnDuck(global::Celeste.Player player)
        {
            // Check if player can unduck at current position
            var normalHitbox = new Hitbox(8, 11, -4, -11);
            Vector2 position = player.Position;

            // Temporarily change collider to check
            var oldCollider = player.Collider;
            player.Collider = normalHitbox;
            bool result = !player.CollideCheck<Solid>();
            player.Collider = oldCollider;

            return result;
        }

        #endregion
    }
}
