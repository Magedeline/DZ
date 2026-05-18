using System;
using Celeste;
using Celeste.Entities;
using global::Celeste.Mod.MaggyHelper;
using MonoMod.Utils;

using CopyAbilityType = Celeste.Entities.Bosses.CopyAbilityType;

namespace Celeste.Extensions
{
    /// <summary>
    /// Extension methods for global::Celeste.Player to add Kirby mode and combat functionality.
    /// These replace the methods that were previously defined in a custom Player class.
    /// </summary>
    public static class PlayerExtensions
    {
        public static bool IsKirbyMode(this Player player)
        {
            return MaggyHelperModule.Session?.IsKirbyModeActive == true;
        }

        public static bool IsKirbyPlayerMode(this Player player)
        {
            return player.IsKirbyMode();
        }

        /// <summary>
        /// Enable Kirby mode on the player.
        /// </summary>
        public static void EnableKirbyMode(this Player player, int maxDashes = 3)
        {
            var session = MaggyHelperModule.Session;
            if (session != null)
            {
                session.IsKirbyModeActive = true;
            }

            PersistDashInventory(player, maxDashes);

            if (player?.Scene is Level level)
            {
                var healthManager = PlayerHealthManager.GetOrCreate(level, 6);
                healthManager.EnableKirbyMode(healthManager.MaxHP);
                UniversalHealthUI.GetOrCreate(level).ShowPlayerHealth = true;

                // Also enable the Kirby health controller for hazard damage integration
                var healthController = KirbyHealthController.GetOrCreate(level);
                healthController.Enable(6);
            }

            TryApplyPlayerSprite(player, "kirby_player");
        }

        /// <summary>
        /// Disable Kirby mode on the player.
        /// </summary>
        public static void DisableKirbyMode(this Player player)
        {
            var session = MaggyHelperModule.Session;
            if (session != null)
            {
                session.IsKirbyModeActive = false;
            }

            if (player?.Scene is Level level)
            {
                PlayerHealthManager.GetOrCreate(level, 1).DisableKirbyMode();
            }

            string spriteId = global::Celeste.PlayerSpriteModeExtensions.GetSpriteBankId(player.Sprite.Mode);
            TryApplyPlayerSprite(player, spriteId);
        }

        public static void EnableKirbyPlayerMode(this Player player, int maxDashes = 3)
        {
            player.EnableKirbyMode(maxDashes);
        }

        public static void DisableKirbyPlayerMode(this Player player)
        {
            player.DisableKirbyMode();
        }

        /// <summary>
        /// Set custom max dashes. Pass -1 to reset to default.
        /// </summary>
        public static void SetMaxDashes(this Player player, int count)
        {
            PersistDashInventory(player, count);
        }

        public static void SetKirbyPowerState(this Player player, KirbyMode.KirbyPowerState powerState)
        {
            var session = MaggyHelperModule.Session;
            if (session != null)
            {
                session.CurrentKirbyPower = powerState.ToString();
                session.CurrentCopyAbility = Enum.TryParse(powerState.ToString(), true, out CopyAbilityType ability)
                    ? ability
                    : CopyAbilityType.None;
            }

            if (player.Scene is Level level)
            {
                var kirbyMode = level.Tracker.GetEntity<KirbyMode>();
                if (kirbyMode == null)
                {
                    kirbyMode = new KirbyMode();
                    level.Add(kirbyMode);
                }

                kirbyMode.SetPowerState(powerState);
            }
        }

        public static void RestorePersistentState(this Player player)
        {
            if (player?.Scene is not Level level)
            {
                return;
            }

            var session = MaggyHelperModule.Session;
            if (session == null)
            {
                return;
            }

            PersistDashInventory(player, level.Session?.Inventory.Dashes ?? 0);

            if (!session.IsKirbyModeActive)
            {
                return;
            }

            TryApplyPlayerSprite(player, "kirby_player");

            if (TryGetStoredKirbyPower(session, out KirbyMode.KirbyPowerState powerState) &&
                powerState != KirbyMode.KirbyPowerState.None)
            {
                var kirbyMode = level.Tracker.GetEntity<KirbyMode>();
                if (kirbyMode == null)
                {
                    kirbyMode = new KirbyMode();
                    level.Add(kirbyMode);
                }

                kirbyMode.SetPowerState(powerState);
            }
        }

        public static bool TryDamageKirby(this Player player, int damage, Vector2 source)
        {
            if (!player.IsKirbyMode() || player.Scene is not Level level)
            {
                return false;
            }

            var healthManager = PlayerHealthManager.Instance ?? level.Tracker.GetEntity<PlayerHealthManager>() ?? PlayerHealthManager.GetOrCreate(level, 6);
            if (!healthManager.IsKirbyMode)
            {
                healthManager.EnableKirbyMode(Math.Max(healthManager.MaxHP, 1));
            }

            return healthManager.Damage(Math.Max(damage, 0));
        }

        /// <summary>
        /// Enable combat mode on the player via DynamicData.
        /// </summary>
        public static void EnableCombat(this Player player)
        {
            if (player == null) return;
            new DynData<Player>(player).Set("CombatEnabled", true);
        }

        /// <summary>
        /// Disable combat mode on the player via DynamicData.
        /// </summary>
        public static void DisableCombat(this Player player)
        {
            if (player == null) return;
            new DynData<Player>(player).Set("CombatEnabled", false);
        }

        public static DynData<Player> GetData(this Player player)
        {
            return player != null ? new DynData<Player>(player) : null;
        }

        private static void TryApplyPlayerSprite(Player player, string spriteId)
        {
            if (player?.Sprite == null || string.IsNullOrEmpty(spriteId))
            {
                return;
            }

            if (GFX.SpriteBank == null || !GFX.SpriteBank.Has(spriteId))
            {
                return;
            }

            string currentAnim = player.Sprite.CurrentAnimationID;
            int currentFrame = player.Sprite.CurrentAnimationFrame;

            GFX.SpriteBank.CreateOn(player.Sprite, spriteId);

            if (!string.IsNullOrEmpty(currentAnim) && player.Sprite.Has(currentAnim))
            {
                player.Sprite.Play(currentAnim, restart: true, randomizeFrame: false);
                player.Sprite.SetAnimationFrame(currentFrame);
            }
            else if (player.Sprite.Has("idle"))
            {
                player.Sprite.Play("idle");
            }
        }

        private static void PersistDashInventory(Player player, int dashCount)
        {
            if (dashCount < 1)
            {
                return;
            }

            if (player?.Scene is Level level)
            {
                level.Session.Inventory.Dashes = dashCount;
            }

            if (player != null)
            {
                player.Dashes = dashCount;
            }
        }

        private static bool TryGetStoredKirbyPower(MaggyHelperModuleSession session, out KirbyMode.KirbyPowerState powerState)
        {
            return Enum.TryParse(session?.CurrentKirbyPower, true, out powerState);
        }
    }
}
