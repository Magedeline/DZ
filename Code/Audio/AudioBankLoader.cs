using System;
using System.Collections.Generic;
using System.IO;
using FMOD;
using FMOD.Studio;
using Celeste.Mod;
using Monocle;
using KIRBY_CELESTE = Celeste.Mod.KIRBY_CELESTE;

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
    ///        desolozantas_sfx.bank
    ///        desolozantas_ui.bank
    ///        desolozantas_music.bank
    ///        desolozantas_dlc_sfx.bank
    ///        desolozantas_dlc_music.bank
    ///
    /// Note: Everest's auto-loader handles bank loading automatically when the strings
    /// bank is present. Manual loading is only needed if the strings bank is absent,
    /// which is not the case here. This class now primarily serves to track loaded banks
    /// and provide utility methods for checking event availability.
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
        /// Track and reference all DesoloZantas FMOD banks loaded by Everest's auto-loader.
        /// Call from EverestModule.LoadContent.
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

            // Since Everest's auto-loader successfully loads the banks (strings bank is present),
            // we just need to track the already-loaded banks for our reference.
            TrackLoadedBanks();
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

        // ── Bank Tracking (Everest auto-loader) ────────────────────────────────

        private static void TrackLoadedBanks()
        {
            // Since Everest's auto-loader has already loaded the banks successfully,
            // we retrieve and track them for our reference.
            string[] bankNames =
            {
                "desolozantas.strings",
                "desolozantas",
                "desolozantas_sfx",
                "desolozantas_ui",
                "desolozantas_music",
                "desolozantas_dlc_sfx",
                "desolozantas_dlc_music",
            };

            int loadedCount = 0;
            foreach (string bankName in bankNames)
            {
                RESULT result = global::Celeste.Audio.System.getBank(bankName, out Bank bank);
                if (result == RESULT.OK)
                {
                    _banks.Add(bank);
                    loadedCount++;
                    Logger.Log(LogLevel.Info, "MaggyHelper", $"[AudioBankLoader] Tracked already-loaded bank: {bankName}");
                }
                else
                {
                    Logger.Log(LogLevel.Verbose, "MaggyHelper", $"[AudioBankLoader] Bank not loaded by Everest: {bankName} ({result})");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper", $"[AudioBankLoader] Tracked {loadedCount}/{bankNames.Length} banks loaded by Everest");
        }

        // ── Utility ───────────────────────────────────────────────────────────

        private static string AudioDirectory()
        {
            // Try KIRBY_CELESTEModule first (this is the active module)
            string modDir = KIRBY_CELESTE.KIRBY_CELESTEModule.Instance?.Metadata?.PathDirectory;
            
            // Fall back to KIRBY_CELESTEModule for compatibility
            if (string.IsNullOrEmpty(modDir))
                modDir = KIRBY_CELESTEModule.Instance?.Metadata?.PathDirectory;
            
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
