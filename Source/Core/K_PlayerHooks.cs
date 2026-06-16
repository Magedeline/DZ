using System.Collections.Generic;
using Monocle;
using Celeste.Entities;
using Celeste.Mod;
using static Celeste.Entities.PlayerSelectionManager;

namespace DZ
{
    /// <summary>
    /// Hooks for K_Player and KirbyHatScarf integration.
    /// Manages lifecycle events, scene registration, and vanilla Player replacement.
    /// Also suppresses the hidden shadow Player (kept for API compatibility) so it
    /// doesn't respond to input or draw.
    /// </summary>
    public static class K_PlayerHooks
    {
        private static bool _loaded = false;

        /// <summary>
        /// Tracks which vanilla Player instances are shadow players managed by K_Player.
        /// Used to suppress Update and Render for those players.
        /// </summary>
        public static readonly HashSet<global::Celeste.Player> ShadowPlayers = new();

        /// <summary>
        /// Loads hooks for K_Player and KirbyHatScarf.
        /// Called from DZModule.Load()
        /// </summary>
        public static void Load()
        {
            if (_loaded)
                return;

            // Hook into level load to handle player replacement
            Everest.Events.Level.OnLoadLevel += OnLevelLoad_HandleK_Player;

            // Hook into entity added to catch K_Player spawn
            On.Monocle.EntityList.Add_Entity += OnEntityAdded;

            // Suppress shadow player input processing and rendering
            On.Celeste.Player.Update += OnPlayerUpdate;
            On.Celeste.Player.Render += OnPlayerRender;

            Logger.Log(LogLevel.Info, "DZ", "[K_PlayerHooks] Loaded");
            _loaded = true;
        }

        /// <summary>
        /// Unloads hooks for K_Player and KirbyHatScarf.
        /// Called from DZModule.Unload()
        /// </summary>
        public static void Unload()
        {
            if (!_loaded)
                return;

            Everest.Events.Level.OnLoadLevel -= OnLevelLoad_HandleK_Player;
            On.Monocle.EntityList.Add_Entity -= OnEntityAdded;
            On.Celeste.Player.Update -= OnPlayerUpdate;
            On.Celeste.Player.Render -= OnPlayerRender;

            ShadowPlayers.Clear();

            Logger.Log(LogLevel.Info, "DZ", "[K_PlayerHooks] Unloaded");
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
                Logger.Log(LogLevel.Info, "DZ",
                    $"[K_PlayerHooks] Level loading with player type: {PlayerSelectionManager.GetPlayerName(selectedPlayer)}");
            }
        }

        /// <summary>
        /// Hook Player.Update to suppress shadow players.
        /// Shadow players exist only for API compatibility and should not process input or game logic.
        /// </summary>
        private static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, global::Celeste.Player self)
        {
            if (ShadowPlayers.Contains(self))
                return; // Shadow player: do nothing

            orig(self);
        }

        /// <summary>
        /// Hook Player.Render to suppress shadow players.
        /// Shadow players should not be visible.
        /// </summary>
        private static void OnPlayerRender(On.Celeste.Player.orig_Render orig, global::Celeste.Player self)
        {
            if (ShadowPlayers.Contains(self))
                return; // Shadow player: do not draw

            orig(self);
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
                Logger.Log(LogLevel.Debug, "DZ",
                    $"[K_PlayerHooks] K_Player added to scene at {kPlayer.Position}");

                // Ensure KirbyHatScarf is visible if in Kirby mode
                if (kPlayer.KirbyModeActive && kPlayer.HatScarf != null)
                {
                    kPlayer.HatScarf.Visible = true;
                    kPlayer.HatScarf.Color = kPlayer.Hair.Color;
                }
            }

            // When vanilla Player is added, replace it with K_Player if Kirby is selected.
            // Skip if this vanilla Player is itself a shadow player.
            if (entity is global::Celeste.Player vanillaPlayer)
            {
                if (ShadowPlayers.Contains(vanillaPlayer))
                    return;

                var selectedPlayer = PlayerSelectionManager.GetSelectedPlayer();
                if (selectedPlayer != PlayerType.Madeline)
                {
                    var level = vanillaPlayer.SceneAs<Level>();
                    if (level != null)
                    {
                        // Remember position before removal
                        Vector2 spawnPos = vanillaPlayer.Position;

                        // Remove vanilla Player
                        vanillaPlayer.RemoveSelf();

                        // Spawn K_Player at the same spot
                        var kirbyPlayer = new K_Player(spawnPos, PlayerSpriteMode.Madeline);
                        level.Add(kirbyPlayer);
                        kirbyPlayer.EnableKirbyMode();

                        Logger.Log(LogLevel.Info, "DZ",
                            $"[K_PlayerHooks] Replaced vanilla Player with K_Player at {kirbyPlayer.Position}");
                    }
                }
            }
        }
    }
}
