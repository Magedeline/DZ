using System;
using System.Reflection;
using Celeste.Entities;
using Celeste.Extensions;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste
{
    /// <summary>
    /// Centralized map-entry hooks that make the Kirby player work in-game
    /// without spawning a second player entity.
    ///
    /// Covers three hook layers:
    ///   1. MonoMod manual hook — Player.ctor (auto-attach controllers on spawn)
    ///   2. MonoMod manual hook — Player.Added (fallback attachment)
    ///   3. Everest event — Everest.Events.Level.OnLoadLevel (metadata-based activation)
    /// </summary>
    public static class KirbyPlayerMapHooks
    {
        // ── Hook storage ───────────────────────────────────────────────────
        private static Hook _playerCtorHook;
        private static Hook _playerAddedHook;

        // ── Public API ─────────────────────────────────────────────────────

        public static void Load()
        {
            // 1. MonoMod manual hook — Player constructor
            //    (Level.LoadLevel is already handled by MonoModHooks; we focus
            //     on ensuring controllers attach the instant Player is created.)
            //    When the vanilla Player spawns, we immediately attach Kirby
            //    controllers if the session/map says Kirby mode should be active.
            try
            {
                var playerCtor = typeof(global::Celeste.Player).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(Vector2), typeof(PlayerSpriteMode) },
                    null);

                if (playerCtor != null)
                {
                    _playerCtorHook = new Hook(
                        playerCtor,
                        typeof(KirbyPlayerMapHooks).GetMethod(
                            nameof(Hook_Player_Ctor),
                            BindingFlags.Static | BindingFlags.NonPublic));

                    Logger.Log(LogLevel.Info, "MaggyHelper",
                        "[KirbyPlayerMapHooks] Player constructor hook registered");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[KirbyPlayerMapHooks] Failed to hook Player.ctor: {ex.Message}");
            }

            // 3. MonoMod manual hook — Player.Added as fallback
            try
            {
                MethodInfo playerAdded = typeof(global::Celeste.Player).GetMethod(
                    "Added",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (playerAdded != null)
                {
                    _playerAddedHook = new Hook(
                        playerAdded,
                        typeof(KirbyPlayerMapHooks).GetMethod(
                            nameof(Hook_Player_Added),
                            BindingFlags.Static | BindingFlags.NonPublic));

                    Logger.Log(LogLevel.Info, "MaggyHelper",
                        "[KirbyPlayerMapHooks] Player.Added hook registered");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[KirbyPlayerMapHooks] Failed to hook Player.Added: {ex.Message}");
            }

            // 4. Everest event — OnLoadLevel for metadata-based activation
            Everest.Events.Level.OnLoadLevel += OnEverestLoadLevel;

            Logger.Log(LogLevel.Info, "MaggyHelper",
                "[KirbyPlayerMapHooks] All map-entry hooks loaded");
        }

        public static void Unload()
        {
            _playerCtorHook?.Dispose();
            _playerCtorHook = null;

            _playerAddedHook?.Dispose();
            _playerAddedHook = null;

            Everest.Events.Level.OnLoadLevel -= OnEverestLoadLevel;

            Logger.Log(LogLevel.Info, "MaggyHelper",
                "[KirbyPlayerMapHooks] All map-entry hooks unloaded");
        }

        // ── MonoMod hook: Player constructor ───────────────────────────────

        private delegate void orig_PlayerCtor(global::Celeste.Player self, Vector2 position, PlayerSpriteMode spriteMode);

        private static void Hook_Player_Ctor(orig_PlayerCtor orig, global::Celeste.Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            // Call original constructor first
            orig(self, position, spriteMode);

            try
            {
                // If Kirby mode should be active, attach controllers immediately.
                // This prevents the "missing player" crash because we ensure the
                // vanilla Player is fully set up for Kirby before any frame runs.
                var session = MaggyHelperModule.Session;
                if (session?.IsKirbyModeActive == true && self.Scene is Level level)
                {
                    // Attach gameplay controller if missing
                    if (self.Get<KirbyPlayerController>() == null)
                        self.Add(new KirbyPlayerController());

                    // Attach sprite state controller if missing
                    if (self.Get<KirbyPlayerSpriteController>() == null)
                        self.Add(new KirbyPlayerSpriteController());

                    // Ensure health system is ready
                    var healthManager = PlayerHealthManager.GetOrCreate(level, 6);
                    if (!healthManager.IsKirbyMode)
                        healthManager.EnableKirbyMode(6);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[KirbyPlayerMapHooks] Player ctor hook error: {ex.Message}");
            }
        }

        // ── MonoMod hook: Player.Added (fallback) ────────────────────────

        private delegate void orig_PlayerAdded(global::Celeste.Player self, Scene scene);

        private static void Hook_Player_Added(orig_PlayerAdded orig, global::Celeste.Player self, Scene scene)
        {
            // Call original first
            orig(self, scene);

            try
            {
                var session = MaggyHelperModule.Session;
                if (session?.IsKirbyModeActive == true && scene is Level level)
                {
                    if (self.Get<KirbyPlayerController>() == null)
                        self.Add(new KirbyPlayerController());

                    if (self.Get<KirbyPlayerSpriteController>() == null)
                        self.Add(new KirbyPlayerSpriteController());
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[KirbyPlayerMapHooks] Player.Added hook error: {ex.Message}");
            }
        }

        // ── Everest event: Level.OnLoadLevel ─────────────────────────────

        private static void OnEverestLoadLevel(Level level, global::Celeste.Player.IntroTypes playerIntro, bool isFromLoader)
        {
            try
            {
                // Metadata-based activation: if the map's custom metadata says
                // "kirbyMode = true", enable Kirby mode even without a spawner entity.
                var session = MaggyHelperModule.Session;
                if (session == null || level?.Session == null)
                    return;

                // Check map-level custom metadata via Everest's Meta attribute.
                // Maps can set <meta><kirbyMode>true</kirbyMode></meta> in their
                // bin to auto-enable Kirby without placing a spawner entity.
                var mapData = level.Session.MapData;
                bool enableFromMeta = false;
                try
                {
                    var meta = mapData?.Meta;
                    if (meta != null)
                    {
                        var prop = meta.GetType().GetProperty("KirbyMode");
                        if (prop != null && prop.GetValue(meta) is bool b)
                            enableFromMeta = b;
                    }
                }
                catch { /* ignore reflection errors */ }

                if (enableFromMeta)
                {
                    var player = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (player != null && !player.IsKirbyMode())
                    {
                        player.EnableKirbyMode();
                        Logger.Log(LogLevel.Info, "MaggyHelper",
                            "[KirbyPlayerMapHooks] Kirby mode enabled via map metadata");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[KirbyPlayerMapHooks] Everest OnLoadLevel error: {ex.Message}");
            }
        }
    }
}
