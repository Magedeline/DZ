using Celeste;
using Celeste.Mod.MaggyHelper;
using MonoMod.Utils;

namespace MaggyHelper.Extensions
{
    /// <summary>
    /// Extension methods for Celeste.Player to add Kirby mode and combat functionality.
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
                if (maxDashes >= 1)
                {
                    player.Dashes = maxDashes;
                }
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

            string spriteId = global::MaggyHelper.PlayerSpriteModeExtensions.GetSpriteBankId(player.Sprite.Mode);
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
            if (count >= 1)
                player.Dashes = count;
        }

        public static void SetKirbyPowerState(this Player player, KirbyMode.KirbyPowerState powerState)
        {
            var session = MaggyHelperModule.Session;
            if (session != null)
            {
                session.CurrentKirbyPower = powerState.ToString();
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

        public static bool TryDamageKirby(this Player player, int damage, Vector2 source)
        {
            if (!player.IsKirbyMode() || player.Scene is not Level level)
            {
                return false;
            }

            var kirbyMode = level.Tracker.GetEntity<KirbyMode>();
            if (kirbyMode == null)
            {
                return false;
            }

            kirbyMode.MaxHealth = Math.Max(kirbyMode.MaxHealth, 1);
            kirbyMode.CurrentHealth = Math.Max(0, kirbyMode.CurrentHealth - Math.Max(damage, 0));
            kirbyMode.IsDead = kirbyMode.CurrentHealth <= 0;
            return true;
        }

        public static DynamicData GetData(this Player player)
        {
            return new DynamicData(player);
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
    }
}
