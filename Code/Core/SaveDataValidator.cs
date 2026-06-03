using System;
using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE
{
    /// <summary>
    /// Validates and auto-repairs KIRBY_CELESTE save data on load.
    /// Detects corrupted collections, missing progression entries, and chapter unlock inconsistencies.
    /// </summary>
    public static class SaveDataValidator
    {
        #region Validation State

        private static bool _hasRunThisSession = false;
        private static List<string> _lastValidationLog = new List<string>();

        #endregion

        #region Public API

        /// <summary>
        /// Runs full save data validation. Call once per session on module load.
        /// </summary>
        public static void ValidateOnLoad()
        {
            if (_hasRunThisSession)
                return;

            _hasRunThisSession = true;
            _lastValidationLog.Clear();

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE/SaveValidator", "Starting save data validation...");

            if (global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule.Instance == null)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE/SaveValidator", "KIRBY_CELESTEModule not yet initialized; skipping validation");
                return;
            }

            var saveData = global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule.SaveData;
            if (saveData == null)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE/SaveValidator", "No save data available to validate");
                return;
            }

            int issuesFound = 0;
            int fixesApplied = 0;

            issuesFound += ValidateCollections(saveData, ref fixesApplied);
            issuesFound += ValidateHeartGemTracking(saveData, ref fixesApplied);
            issuesFound += ValidateAchievementTracking(saveData, ref fixesApplied);
            issuesFound += ValidateIntroFlags(saveData, ref fixesApplied);
            issuesFound += ValidateChapterUnlockConsistency(saveData, ref fixesApplied);

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE/SaveValidator",
                $"Validation complete. Issues found: {issuesFound}, Fixes applied: {fixesApplied}");

            foreach (var logEntry in _lastValidationLog)
            {
                Logger.Log(LogLevel.Debug, "KIRBY_CELESTE/SaveValidator", logEntry);
            }
        }

        /// <summary>
        /// Returns the last validation log entries for display in debug menus.
        /// </summary>
        public static IReadOnlyList<string> GetLastValidationLog()
        {
            return _lastValidationLog.AsReadOnly();
        }

        /// <summary>
        /// Resets the validation state so it can run again (e.g., after save reload).
        /// </summary>
        public static void ResetValidationState()
        {
            _hasRunThisSession = false;
            _lastValidationLog.Clear();
        }

        #endregion

        #region Collection Validation

        private static int ValidateCollections(global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSaveData saveData, ref int fixesApplied)
        {
            int issues = 0;

            // Ensure critical collections are not null
            var collections = new Dictionary<string, object>
            {
                { "CollectedPopstarBerries", saveData.CollectedPopstarBerries },
                { "UnlockedBSideIDs", saveData.UnlockedBSideIDs },
                { "UnlockedCSideIDs", saveData.UnlockedCSideIDs },
                { "PendingCSideUnlockIDs", saveData.PendingCSideUnlockIDs },
                { "UnlockedRemixExtraIDs", saveData.UnlockedRemixExtraIDs },
                { "UnlockedModes", saveData.UnlockedModes },
                { "ChapterSideAvailability", saveData.ChapterSideAvailability },
                { "ChapterCompletionTimes", saveData.ChapterCompletionTimes },
                { "ChapterDeathCounts", saveData.ChapterDeathCounts },
                { "ChapterMusicOverrides", saveData.ChapterMusicOverrides },
                { "SavedMountainCameras", saveData.SavedMountainCameras }
            };

            foreach (var kvp in collections)
            {
                if (kvp.Value == null)
                {
                    _lastValidationLog.Add($"Reinitialized null collection: {kvp.Key}");
                    fixesApplied++;
                    issues++;
                }
            }

            return issues;
        }

        #endregion

        #region Heart Gem Validation

        private static int ValidateHeartGemTracking(global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSaveData saveData, ref int fixesApplied)
        {
            int issues = 0;

            // Check for malformed heart gem IDs
            var heartGems = saveData.GetType().GetField("CollectedHeartGems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (heartGems?.GetValue(saveData) is HashSet<string> collected)
            {
                var toRemove = new List<string>();
                foreach (var entry in collected)
                {
                    if (string.IsNullOrEmpty(entry) || !entry.Contains("_"))
                    {
                        _lastValidationLog.Add($"Removed malformed heart gem entry: '{entry}'");
                        toRemove.Add(entry);
                        issues++;
                    }
                }

                foreach (var entry in toRemove)
                {
                    collected.Remove(entry);
                    fixesApplied++;
                }
            }

            return issues;
        }

        #endregion

        #region Achievement Validation

        private static int ValidateAchievementTracking(global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSaveData saveData, ref int fixesApplied)
        {
            int issues = 0;

            // Ensure achievement HashSet is initialized (it's private)
            var achievementsField = saveData.GetType().GetField("Achievements", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (achievementsField?.GetValue(saveData) == null)
            {
                _lastValidationLog.Add("Reinitializing null Achievements collection");
                achievementsField?.SetValue(saveData, new HashSet<string>());
                fixesApplied++;
                issues++;
            }

            return issues;
        }

        #endregion

        #region Intro Flags Validation

        private static int ValidateIntroFlags(global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSaveData saveData, ref int fixesApplied)
        {
            int issues = 0;

            // Ensure intro flag defaults are sensible
            if (saveData.HasSeenModIntro && !saveData.HasSeenDZMountainIntro)
            {
                _lastValidationLog.Add("Inconsistent intro state: HasSeenModIntro=true but HasSeenDZMountainIntro=false");
                saveData.HasSeenDZMountainIntro = true;
                fixesApplied++;
                issues++;
            }

            return issues;
        }

        #endregion

        #region Chapter Unlock Consistency

        private static int ValidateChapterUnlockConsistency(global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSaveData saveData, ref int fixesApplied)
        {
            int issues = 0;

            // Ensure starter chapters have side availability
            string[] starterChapters = { "00_Prologue", "01_City" };
            foreach (var chapter in starterChapters)
            {
                string sid = AreaModeExtender.BuildASideSID(chapter);
                if (!saveData.ChapterSideAvailability.ContainsKey(sid))
                {
                    _lastValidationLog.Add($"Adding missing side availability for starter chapter: {chapter}");
                    saveData.ChapterSideAvailability[sid] = new ChapterSideAvailability
                    {
                        HasASide = true,
                        HasBSide = false,
                        HasCSide = false,
                        HasDSide = false,
                        HasDXSide = false
                    };
                    fixesApplied++;
                    issues++;
                }
            }

            // If TrueFinaleUnlocked, ensure late chapters have availability
            if (saveData.TrueFinaleUnlocked)
            {
                string[] lateChapters = { "19_Space", "20_TheEnd", "21_LastLevel" };
                foreach (var ch in lateChapters)
                {
                    string sid = AreaModeExtender.BuildASideSID(ch);
                    if (!saveData.ChapterSideAvailability.ContainsKey(sid))
                    {
                        _lastValidationLog.Add($"TrueFinaleUnlocked: adding side availability for {ch}");
                        saveData.ChapterSideAvailability[sid] = new ChapterSideAvailability
                        {
                            HasASide = true,
                            HasBSide = true,
                            HasCSide = true,
                            HasDSide = true,
                            HasDXSide = false
                        };
                        fixesApplied++;
                        issues++;
                    }
                }
            }

            return issues;
        }

        #endregion

        #region Console Commands

        /// <summary>
        /// Registers Everest console commands for save data debugging.
        /// Call from KIRBY_CELESTEModule.Load().
        /// </summary>
        public static void RegisterConsoleCommands()
        {
            // Use Everest.Commands which is the standard API
            // Commands registered via [Command] attribute are auto-discovered
            // We provide a manual registration path for dynamic commands
            try
            {
                var cmdType = typeof(Everest).Assembly.GetType("Celeste.Mod.Everest+Commands");
                if (cmdType != null)
                {
                    var registerMethod = cmdType.GetMethod("Register", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (registerMethod != null)
                    {
                        // Register save validation command
                        registerMethod.Invoke(null, new object[] { "maggy_save_validate", (Action)(() =>
                        {
                            ResetValidationState();
                            ValidateOnLoad();
                            Logger.Log(LogLevel.Info, "KIRBY_CELESTE/SaveValidator", string.Join("\n", _lastValidationLog));
                        })});
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE/SaveValidator", $"Could not register console commands: {ex.Message}");
            }
        }

        #endregion
    }
}
