using Monocle;
using Celeste.Entities;
using Celeste.Mod;
using static Celeste.Entities.PlayerSelectionManager;

namespace Celeste
{
    /// <summary>
    /// Hooks for K_Player and KirbyHatScarf integration.
    /// Manages lifecycle events, scene registration, and vanilla Player replacement.
    /// </summary>
    public static class K_PlayerHooks
    {
        private static bool _loaded = false;

        /// <summary>
        /// Loads hooks for K_Player and KirbyHatScarf.
        /// Called from MaggyHelperModule.Load()
        /// </summary>
        public static void Load()
        {
            if (_loaded)
                return;

            // Hook into level load to handle player replacement
            Everest.Events.Level.OnLoadLevel += OnLevelLoad_HandleK_Player;

            // Hook into entity added to catch K_Player spawn
            On.Monocle.EntityList.Add_Entity += OnEntityAdded;

            Logger.Log(LogLevel.Info, "MaggyHelper", "[K_PlayerHooks] Loaded");
            _loaded = true;
        }

        /// <summary>
        /// Unloads hooks for K_Player and KirbyHatScarf.
        /// Called from MaggyHelperModule.Unload()
        /// </summary>
        public static void Unload()
        {
            if (!_loaded)
                return;

            Everest.Events.Level.OnLoadLevel -= OnLevelLoad_HandleK_Player;
            On.Monocle.EntityList.Add_Entity -= OnEntityAdded;

            Logger.Log(LogLevel.Info, "MaggyHelper", "[K_PlayerHooks] Unloaded");
            _loaded = false;
        }

        /// <summary>
        /// Called when a level loads - handles player replacement logic.
        /// </summary>
        private static void OnLevelLoad_HandleK_Player(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // Check if we should spawn K_Player instead of vanilla Player
            // This is controlled by PlayerSelectionManager or map data
            var selectedPlayer = PlayerSelectionManager.GetSelectedPlayer();

            if (selectedPlayer != PlayerType.Madeline)
            {
                Logger.Log(LogLevel.Info, "MaggyHelper",
                    $"[K_PlayerHooks] Level loading with player type: {PlayerSelectionManager.GetPlayerName(selectedPlayer)}");
            }
        }

        /// <summary>
        /// Hook into entity addition to handle K_Player and KirbyHatScarf setup.
        /// </summary>
        private static void OnEntityAdded(On.Monocle.EntityList.orig_Add_Entity orig, EntityList self, Entity entity)
        {
            orig(self, entity);

            // When K_Player is added to the scene, ensure it's properly initialized
            if (entity is K_Player kPlayer)
            {
                Logger.Log(LogLevel.Debug, "MaggyHelper",
                    $"[K_PlayerHooks] K_Player added to scene at {kPlayer.Position}");

                // Ensure KirbyHatScarf is visible if in Kirby mode
                if (kPlayer.KirbyModeActive && kPlayer.HatScarf != null)
                {
                    kPlayer.HatScarf.Visible = true;
                    kPlayer.HatScarf.Color = kPlayer.Hair.Color;
                }
            }

            // When vanilla Player is added, check if we need to replace it
            if (entity is global::Celeste.Player vanillaPlayer)
            {
                var selectedPlayer = PlayerSelectionManager.GetSelectedPlayer();
                if (selectedPlayer != PlayerType.Madeline)
                {
                    Logger.Log(LogLevel.Info, "MaggyHelper",
                        "[K_PlayerHooks] Vanilla Player spawned but K_Player is selected - replacement may be needed");
                }
            }
        }
    }
}
