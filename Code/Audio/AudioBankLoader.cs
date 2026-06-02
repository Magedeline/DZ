using System;
using System.Collections.Generic;
using System.IO;
using FMOD;
using FMOD.Studio;
using Celeste.Mod;
using Monocle;

namespace Celeste.Mod.MaggyHelper.Audio
{
    /// <summary>
    /// Manages FMOD 1.10.20 bank loading for DesoloZantas audio.
    ///
    /// FMOD Studio 1.10.20 setup (must match Celeste's FMOD version):
    ///   1. Open your FMOD Studio project
    ///   2. Edit → Preferences → Build → enable "Build strings bank"
    ///   3. File → Build All  (or Ctrl+B)
    ///   4. Copy the output files to this mod's Audio/ folder:
    ///        desolozantas.strings.bank   ← REQUIRED, contains event:/ path table
    ///        desolozantas.bank
    ///        music.bank
    ///        sfx.bank
    ///        ui.bank
    ///        dlc_music.bank
    ///        dlc_sfx.bank
    ///
    /// Why manual loading instead of Everest's auto-loader:
    ///   Everest calls Audio.Banks.Load(name, loadStrings: true) for every .bank
    ///   file it finds. That tries to open [name].strings.bank — which throws
    ///   FMOD ERR_FILE_NOTFOUND when the strings bank is absent. We bypass that
    ///   by calling Audio.System.loadBankFile directly and controlling which files
    ///   to load and in what order.
    /// </summary>
    public static class AudioBankLoader
    {
        // Event path prefix used by this mod — must match what was authored in FMOD Studio
        public const string EventPrefix = "event:/pusheen/";

        // Strings bank — built by FMOD Studio 1.10.20 alongside the content banks.
        // Contains the event:/ path → GUID table; must be loaded before any other bank.
        private const string StringsBankFile = "desolozantas.strings.bank";

        // Content banks in load order (sfx before music so ambience is ready first)
        private static readonly string[] ContentBankFiles =
        {
            "desolozantas.bank",
            "desolozantas_sfx.bank",
            "desolozantas_ui.bank",
            "desolozantas_music.bank",
            "desolozantas_dlc_sfx.bank",
            "desolozantas_dlc_music.bank",
        };

        private static readonly List<Bank> _banks = new();
        private static bool _loaded;

        // ── Public API ────────────────────────────────────────────────────────

        public static bool IsLoaded => _loaded && _banks.Count > 0;

        /// <summary>
        /// Load all DesoloZantas FMOD banks.  Call from EverestModule.LoadContent.
        /// </summary>
        public static void Load()
        {
            if (_loaded) return;

            // Audio.System is null when LoadContent fires before FMOD finishes init.
            // Don't set _loaded so the hooks can retry once FMOD is ready.
            if (global::Celeste.Audio.System == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    "[AudioBankLoader] Audio.System is null — will retry on first music event");
                return;
            }

            _loaded = true;

            string audioDir = AudioDirectory();
            if (audioDir == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "[AudioBankLoader] Audio directory not found — no banks loaded");
                return;
            }

            // Strings bank must come first so subsequent loadBankFile calls can
            // resolve event:/ paths by name instead of only by GUID.
            LoadSingleBank(Path.Combine(audioDir, StringsBankFile), required: true);

            foreach (string file in ContentBankFiles)
                LoadSingleBank(Path.Combine(audioDir, file), required: false);
        }

        /// <summary>
        /// Unload all banks.  Call from EverestModule.Unload.
        /// </summary>
        public static void Unload()
        {
            foreach (Bank bank in _banks)
            {
                try { bank.unload(); }
                catch { }
            }
            _banks.Clear();
            _loaded = false;
        }

        // ── FMOD 1.10.20 direct loading ───────────────────────────────────────

        private static void LoadSingleBank(string path, bool required)
        {
            string name = Path.GetFileName(path);

            if (!File.Exists(path))
            {
                LogLevel level = required ? LogLevel.Warn : LogLevel.Verbose;
                Logger.Log(level, "MaggyHelper", $"[AudioBankLoader] Not found: {name}" +
                    (required ? " — event:/pusheen/* paths cannot be resolved without the strings bank" : ""));
                return;
            }

            if (global::Celeste.Audio.System == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[AudioBankLoader] Audio.System is null, cannot load {name}");
                _loaded = false;
                return;
            }

            // FMOD.Studio.System.loadBankFile is the FMOD 1.10.20 API call.
            // LOAD_BANK_FLAGS.NORMAL loads the bank on demand (not all at once).
            RESULT result = global::Celeste.Audio.System.loadBankFile(path, LOAD_BANK_FLAGS.NORMAL, out Bank bank);

            if (result != RESULT.OK)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[AudioBankLoader] loadBankFile failed for {name}: {result}");
                return;
            }

            _banks.Add(bank);
            Logger.Log(LogLevel.Info, "MaggyHelper", $"[AudioBankLoader] Loaded {name}");
        }

        // ── Utility ───────────────────────────────────────────────────────────

        private static string AudioDirectory()
        {
            string modDir = MaggyHelperModule.Instance?.Metadata?.PathDirectory;
            if (string.IsNullOrEmpty(modDir)) return null;
            string dir = Path.Combine(modDir, "Audio");
            return Directory.Exists(dir) ? dir : null;
        }

        /// <summary>
        /// Returns true if the given event path exists in the loaded banks.
        /// Useful for graceful fallback when a bank failed to load.
        /// </summary>
        public static bool EventExists(string eventPath)
        {
            if (!IsLoaded || string.IsNullOrEmpty(eventPath)) return false;
            RESULT r = global::Celeste.Audio.System.getEvent(eventPath, out EventDescription _);
            return r == RESULT.OK;
        }

        /// <summary>
        /// Play an event by path, falling back to the given vanilla fallback path
        /// if the custom event is not available.
        /// </summary>
        public static void PlayWithFallback(string eventPath, string fallback = null)
        {
            string path = (EventExists(eventPath)) ? eventPath : fallback;
            if (!string.IsNullOrEmpty(path))
                global::Celeste.Audio.Play(path);
        }
    }
}
