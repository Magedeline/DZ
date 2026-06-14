using FMOD.Studio;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Manages FMOD bus routing for the KIRBY_CELESTE mod.
    ///
    /// Two bus groups are maintained:
    ///   <see cref="PusheenBuses"/>  — primary output buses that carry all Pusheen audio.
    ///   <see cref="ReturnBuses"/>   — send/return buses used for reverb verbs, ambience,
    ///                                 and special routing (tape preview, picoboy verb, etc.).
    ///
    /// Buses are activated (un-paused, volume restored to 1) after every bank load and
    /// on every overworld entry, so both in-game and the chapter-select screen receive audio.
    /// </summary>
    public static class AudioBusManager
    {
        // ─── Pusheen primary output buses ─────────────────────────────────────

        /// <summary>
        /// All primary Pusheen output bus paths.
        /// These carry gameplay SFX, music, UI, ambience, and berry sounds.
        /// </summary>
        public static readonly IReadOnlyList<string> PusheenBuses = new[]
        {
            "bus:/gameplay_sfx/game/pusheen",
            "bus:/music/tunes/pusheen",
            "bus:/gameplay_sfx/ambience/pusheen",
            "bus:/music/tunes/pusheen_arena",
            "bus:/gameplay_sfx/berries/pusheen",
            "bus:/music/tunes/pusheen_classic",
            "bus:/gameplay_sfx/char/pusheen",
            "bus:/music/tunes/pusheen_lobby",
            "bus:/gameplay_sfx/classic/pusheen",
            "bus:/music/tunes/pusheen_lobby_piano",
            "bus:/music/tunes/pusheen_remix",
            "bus:/music/tunes/pusheen_tape",
            "bus:/music/tunes/mains/pusheen",
            "bus:/ui_sfx/pusheen",
            "bus:/ui_sfx/game/pusheen",
            "bus:/ui_sfx/rename_piano/pusheen",
            "bus:/ui_sfx/worldmap_whoosh/pusheen",
            "bus:/music/stings/pusheen",
        };

        // ─── Return / verb buses ───────────────────────────────────────────────

        /// <summary>
        /// Send/return and verb buses used for reverb tails, ambient flavour,
        /// and special audio routing (tape preview, picoboy verb, etc.).
        /// </summary>
        public static readonly IReadOnlyList<string> ReturnBuses = new[]
        {
            "bus:/gameplay_sfx/creation_verb",
            "bus:/gameplay_sfx/greengreens_room_flavor",
            "bus:/gameplay_sfx/heaven_clouds_dialogue",
            "bus:/gameplay_sfx/intro_verb",
            "bus:/gameplay_sfx/pusheen_verb",
            "bus:/gameplay_sfx/sfx_in_void",
            "bus:/music/tunes/tape_preview",
            "bus:/music/tunes/picoboy_mus_verb",
            "bus:/gameplay_sfx/classic/pusheen/picoboy_verb",
        };

        // ─── Hook management ──────────────────────────────────────────────────

        private static bool _loaded;

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;

            On.Celeste.GameLoader.LoadThread += OnGameLoaderLoadThread;
            On.Celeste.Overworld.Begin       += OnOverworldBegin;
        }

        public static void Unload()
        {
            if (!_loaded) return;
            _loaded = false;

            On.Celeste.GameLoader.LoadThread -= OnGameLoaderLoadThread;
            On.Celeste.Overworld.Begin       -= OnOverworldBegin;
        }

        // ─── Hook handlers ────────────────────────────────────────────────────

        private static void OnGameLoaderLoadThread(On.Celeste.GameLoader.orig_LoadThread orig, GameLoader self)
        {
            try
            {
                orig(self);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/AudioBusManager",
                    $"[Audio] GameLoader.LoadThread failed: {ex.GetType().Name}: {ex.Message}");
                // Don't rethrow — let MaggyHelperModule or vanilla handle it, but ensure
                // ActivateAllBuses is not called when FMOD is in a bad state.
                return;
            }
            ActivateAllBuses();
        }

        private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
        {
            orig(self);
            ActivateAllBuses();
        }

        // ─── Core activation logic ────────────────────────────────────────────

        /// <summary>
        /// Activates every bus in both <see cref="PusheenBuses"/> and <see cref="ReturnBuses"/>:
        /// sets volume to 1 and un-pauses so audio flows correctly after bank loads.
        /// Does nothing if <see cref="MaggyHelperModule.AudioBanksLoaded"/> is false (master bank
        /// could not be loaded due to vanilla GUID conflicts — see MaggyHelperModule for details).
        /// </summary>
        public static void ActivateAllBuses()
        {
            if (!MaggyHelperModule.AudioBanksLoaded)
                return;

            ActivateBusGroup("Pusheen", PusheenBuses);
            ActivateBusGroup("Return",  ReturnBuses);
        }

        private static void ActivateBusGroup(string groupLabel, IReadOnlyList<string> paths)
        {
            var system = Audio.System;
            if (system == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/AudioBusManager",
                    $"[{groupLabel}] FMOD Studio system not ready — skipping bus activation.");
                return;
            }

            int ok = 0, fail = 0;
            foreach (string path in paths)
            {
                try
                {
                    FMOD.RESULT result = system.getBus(path, out Bus bus);
                    if (result != FMOD.RESULT.OK)
                    {
                        Logger.Log(LogLevel.Warn, "MaggyHelper/AudioBusManager",
                            $"[{groupLabel}] getBus failed for \"{path}\": {result}");
                        fail++;
                        continue;
                    }

                    bus.setVolume(1f);
                    bus.setPaused(false);
                    ok++;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper/AudioBusManager",
                        $"[{groupLabel}] Exception activating \"{path}\": {ex.Message}");
                    fail++;
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper/AudioBusManager",
                $"[{groupLabel}] Bus activation complete — {ok} OK, {fail} failed.");
        }
    }
}
