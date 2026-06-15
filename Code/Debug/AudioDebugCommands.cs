using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaggyHelper.Debug
{
    /// <summary>
    /// Console commands for audio debugging and testing.
    /// Commands:
    ///   maggy_audio_test <event_path>      - Test if an audio event exists and play it
    ///   maggy_audio_list <filter>          - List available audio events (with optional filter)
    ///   maggy_audio_missing                - List all missing audio events encountered
    ///   maggy_audio_stats                  - Print audio debugging statistics
    ///   maggy_audio_find <keyword>         - Search for audio events by keyword
    /// </summary>
    public static class AudioDebugCommands
    {
        [Command("maggy_audio_test", "Test playing an audio event")]
        public static void AudioTest(string eventPath)
        {
            if (string.IsNullOrWhiteSpace(eventPath))
            {
                Engine.Commands.Log("Usage: maggy_audio_test <event_path>");
                Engine.Commands.Log("Example: maggy_audio_test event:/new_content/music/pusheen/lvl19/last_push");
                return;
            }

            bool isValid = AudioEventValidator.ValidateEvent(eventPath);

            if (isValid)
            {
                try
                {
                    var instance = Audio.Play(eventPath);
                    Engine.Commands.Log($"✓ Audio event played: {eventPath}");
                    Engine.Commands.Log($"  Instance: {instance}");
                }
                catch (Exception ex)
                {
                    Engine.Commands.Log($"✗ Error playing event: {ex.Message}");
                }
            }
            else
            {
                Engine.Commands.Log($"✗ Audio event NOT found: {eventPath}");

                // Suggest alternative
                var suggestion = AudioEventValidator.SuggestAlternative(eventPath);
                if (!string.IsNullOrEmpty(suggestion))
                {
                    Engine.Commands.Log($"  Suggestion: {suggestion}");
                }
            }
        }

        [Command("maggy_audio_missing", "List all missing audio events encountered")]
        public static void AudioMissing()
        {
            var missing = AudioEventValidator.GetMissingEvents();

            if (missing.Count == 0)
            {
                Engine.Commands.Log("No missing audio events detected!");
                return;
            }

            Engine.Commands.Log($"=== MISSING AUDIO EVENTS ({missing.Count}) ===");

            foreach (var kvp in missing.OrderBy(x => x.Key))
            {
                Engine.Commands.Log($"  ✗ {kvp.Key}");
            }
        }

        [Command("maggy_audio_stats", "Print audio debugging statistics")]
        public static void AudioStats()
        {
            AudioDebugHooks.PrintAudioDebugStats();
        }

        [Command("maggy_audio_find", "Search for audio events by keyword")]
        public static void AudioFind(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                Engine.Commands.Log("Usage: maggy_audio_find <keyword>");
                Engine.Commands.Log("Example: maggy_audio_find last_push");
                return;
            }

            var valid = AudioEventValidator.GetValidEvents();
            var matches = valid.Keys
                .Where(e => e.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                Engine.Commands.Log($"No audio events found containing: {keyword}");
                return;
            }

            Engine.Commands.Log($"=== AUDIO EVENTS MATCHING '{keyword}' ({matches.Count}) ===");

            foreach (var match in matches.OrderBy(m => m))
            {
                Engine.Commands.Log($"  ✓ {match}");
            }
        }

        [Command("maggy_audio_dump_missing", "Detailed dump of missing audio events with suggestions")]
        public static void AudioDumpMissing()
        {
            var missing = AudioEventValidator.GetMissingEvents();

            if (missing.Count == 0)
            {
                Engine.Commands.Log("No missing audio events!");
                return;
            }

            Engine.Commands.Log($"=== MISSING AUDIO EVENTS WITH SUGGESTIONS ({missing.Count}) ===");
            Engine.Commands.Log("");

            foreach (var kvp in missing.OrderBy(x => x.Key))
            {
                Engine.Commands.Log($"Missing: {kvp.Key}");
                Engine.Commands.Log($"  Reason: {kvp.Value}");

                var suggestion = AudioEventValidator.SuggestAlternative(kvp.Key);
                if (!string.IsNullOrEmpty(suggestion))
                {
                    Engine.Commands.Log($"  Suggestion: {suggestion}");
                }
                Engine.Commands.Log("");
            }
        }

        [Command("maggy_audio_validate", "Validate a specific audio event")]
        public static void AudioValidate(string eventPath)
        {
            if (string.IsNullOrWhiteSpace(eventPath))
            {
                Engine.Commands.Log("Usage: maggy_audio_validate <event_path>");
                return;
            }

            bool isValid = AudioEventValidator.ValidateEvent(eventPath);

            if (isValid)
            {
                Engine.Commands.Log($"✓ VALID: {eventPath}");
            }
            else
            {
                Engine.Commands.Log($"✗ INVALID: {eventPath}");

                var suggestion = AudioEventValidator.SuggestAlternative(eventPath);
                if (!string.IsNullOrEmpty(suggestion))
                {
                    Engine.Commands.Log($"  Try: {suggestion}");
                }
            }
        }

        [Command("maggy_audio_reset", "Reset the audio event cache")]
        public static void AudioReset()
        {
            AudioEventValidator.Reset();
            Engine.Commands.Log("Audio event cache cleared");
        }

        [Command("maggy_check_gen_last_push", "Check the status of the missing gen_last_push event")]
        public static void CheckGenLastPush()
        {
            Engine.Commands.Log("=== GEN_LAST_PUSH AUDIO EVENT STATUS ===");
            Engine.Commands.Log("");

            string missingEvent = "event:/new_content/env/pusheen/gen_last_push";
            string correctEvent = "event:/new_content/music/pusheen/lvl19/last_push";

            Engine.Commands.Log($"Missing event:  {missingEvent}");
            Engine.Commands.Log($"Status: ✗ NOT FOUND");
            Engine.Commands.Log("");

            Engine.Commands.Log($"Correct event:  {correctEvent}");

            bool isValid = AudioEventValidator.ValidateEvent(correctEvent);
            Engine.Commands.Log($"Status: {(isValid ? "✓ FOUND" : "✗ NOT FOUND")}");
            Engine.Commands.Log("");

            Engine.Commands.Log("To fix:");
            Engine.Commands.Log("1. Find where 'event:/new_content/env/pusheen/gen_last_push' is used");
            Engine.Commands.Log("2. Replace with: 'event:/new_content/music/pusheen/lvl19/last_push'");
            Engine.Commands.Log("3. Or use a different ambient event from the new_content/env/pusheen category");
            Engine.Commands.Log("");

            Engine.Commands.Log("Use 'maggy_audio_missing' to see all missing events");
            Engine.Commands.Log("Use 'maggy_audio_find <keyword>' to search for alternatives");
        }

        [Command("maggy_mod_audio_scan", "Scan all loaded mods for audio assets")]
        public static void ModAudioScan()
        {
            MaggyHelperModule.ScanAllModAudioAssets(force: true);
            Engine.Commands.Log($"=== MOD AUDIO SCAN COMPLETE ===");
            Engine.Commands.Log($"Assets: {MaggyHelperModule.DiscoveredModAudioAssets.Count}");
            Engine.Commands.Log($"Banks: {MaggyHelperModule.DiscoveredModAudioBanks.Count}");
        }

        [Command("maggy_mod_audio_list", "List discovered mod audio assets. Usage: maggy_mod_audio_list [banks|all]")]
        public static void ModAudioList(string mode = "")
        {
            MaggyHelperModule.ScanAllModAudioAssets();

            if (string.Equals(mode, "banks", StringComparison.OrdinalIgnoreCase))
            {
                Engine.Commands.Log($"=== MOD AUDIO BANKS ({MaggyHelperModule.DiscoveredModAudioBanks.Count}) ===");
                foreach (string bank in MaggyHelperModule.DiscoveredModAudioBanks.OrderBy(b => b))
                {
                    Engine.Commands.Log($"  [Bank] {bank}");
                }
            }
            else if (string.Equals(mode, "all", StringComparison.OrdinalIgnoreCase))
            {
                Engine.Commands.Log($"=== ALL MOD AUDIO ASSETS ({MaggyHelperModule.DiscoveredModAudioAssets.Count}) ===");
                foreach (string asset in MaggyHelperModule.DiscoveredModAudioAssets.OrderBy(a => a))
                {
                    Engine.Commands.Log($"  {asset}");
                }
            }
            else
            {
                Engine.Commands.Log($"=== MOD AUDIO SUMMARY ===");
                Engine.Commands.Log($"Total assets: {MaggyHelperModule.DiscoveredModAudioAssets.Count}");
                Engine.Commands.Log($"Total banks: {MaggyHelperModule.DiscoveredModAudioBanks.Count}");
                Engine.Commands.Log("");
                Engine.Commands.Log("Banks:");
                foreach (string bank in MaggyHelperModule.DiscoveredModAudioBanks.Take(20).OrderBy(b => b))
                {
                    Engine.Commands.Log($"  [Bank] {bank}");
                }
                if (MaggyHelperModule.DiscoveredModAudioBanks.Count > 20)
                {
                    Engine.Commands.Log($"  ... and {MaggyHelperModule.DiscoveredModAudioBanks.Count - 20} more");
                }
                Engine.Commands.Log("");
                Engine.Commands.Log("Usage: maggy_mod_audio_list banks | maggy_mod_audio_list all");
            }
        }

        [Command("maggy_mod_audio_ingest", "Re-ingest mod audio banks via Celeste's Audio system")]
        public static void ModAudioIngest()
        {
            MaggyHelperModule.TryIngestNewBanks("ConsoleCommand");
            Engine.Commands.Log("Bank ingestion complete.");
        }
    }
}
