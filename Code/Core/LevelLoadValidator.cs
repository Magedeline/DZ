using System;
using System.Collections.Generic;
using System.Linq;
using Monocle;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Validates level entities and triggers on load to catch configuration errors early.
    /// Logs warnings instead of crashing for common setup mistakes.
    /// </summary>
    public static class LevelLoadValidator
    {
        private static HashSet<string> _knownDialogKeys = new HashSet<string>();
        private static HashSet<string> _knownAudioEvents = new HashSet<string>();
        private static bool _initialized = false;

        /// <summary>
        /// Initialize the validator with known dialog and audio keys.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // Pre-load known dialog keys from the game's dialog database
            try
            {
                var dialogType = typeof(Dialog);
                var loadMethod = dialogType.GetMethod("Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (loadMethod != null)
                {
                    // Dialog is already loaded by Everest, just reference existing keys
                    _knownDialogKeys = new HashSet<string>(GetLoadedDialogKeys());
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator", $"Could not pre-load dialog keys: {ex.Message}");
            }

            Logger.Log(LogLevel.Info, "MaggyHelper/LevelValidator", $"Level validator initialized with {_knownDialogKeys.Count} known dialog keys");
        }

        /// <summary>
        /// Validates all entities in a level after load.
        /// Call from Everest.Events.Level.OnLoadLevel.
        /// </summary>
        public static void ValidateLevel(Level level)
        {
            if (level?.Session?.MapData == null) return;

            double elapsed = PerformanceProfiler.Profile("LevelLoadValidator.ValidateLevel", () =>
            {
                int warnings = 0;
                string levelName = level.Session.Level ?? "unknown";

                // Validate entities
                foreach (var entity in level.Entities)
                {
                    warnings += ValidateEntity(entity, levelName);
                }

                // Validate triggers
                foreach (Trigger trigger in level.Tracker.GetEntities<Trigger>())
                {
                    warnings += ValidateTrigger(trigger, levelName);
                }

                if (warnings > 0)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                        $"Level '{levelName}' validation: {warnings} warnings found");
                }
            });
        }

        #region Entity Validation

        private static int ValidateEntity(Entity entity, string levelName)
        {
            int warnings = 0;
            var typeName = entity.GetType().Name;

            // Check for NPCs with missing dialog
            if (entity is NPC npc)
            {
                var talkComponent = entity.Get<TalkComponent>();
                if (talkComponent != null)
                {
                    // NPC has talk component - check if dialog is likely configured
                }
            }

            // Check for DialogTrigger
            if (typeName == "DialogTrigger" || typeName.Contains("Dialog"))
            {
                warnings += ValidateDialogTrigger(entity, levelName);
            }

            // Check for custom entities that might use EntityData
            if (entity is Platform || entity is Solid)
            {
                // Platforms generally don't need validation
            }

            return warnings;
        }

        private static int ValidateDialogTrigger(Entity entity, string levelName)
        {
            int warnings = 0;

            try
            {
                var dataField = entity.GetType().GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (dataField?.GetValue(entity) is EntityData data)
                {
                    string dialogKey = data.Attr("dialog", "");
                    if (!string.IsNullOrEmpty(dialogKey) && !Dialog.Has(dialogKey))
                    {
                        Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                            $"[{levelName}] DialogTrigger references missing dialog key: '{dialogKey}'");
                        warnings++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Debug, "MaggyHelper/LevelValidator",
                    $"Could not validate DialogTrigger in {levelName}: {ex.Message}");
            }

            return warnings;
        }

        #endregion

        #region Trigger Validation

        private static int ValidateTrigger(Trigger trigger, string levelName)
        {
            int warnings = 0;
            var typeName = trigger.GetType().Name;

            // Check trigger bounds
            if (trigger.Width <= 0 || trigger.Height <= 0)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                    $"[{levelName}] {typeName} has invalid bounds: {trigger.Width}x{trigger.Height}");
                warnings++;
            }

            // Check for extremely large triggers (possible mistake)
            if (trigger.Width > 1000 || trigger.Height > 1000)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                    $"[{levelName}] {typeName} is unusually large: {trigger.Width}x{trigger.Height}");
                warnings++;
            }

            return warnings;
        }

        #endregion

        #region Map Data Validation

        /// <summary>
        /// Validates map-level data before entities are instantiated.
        /// Call this before level load for early error detection.
        /// </summary>
        public static void ValidateMapData(MapData mapData, string mapName)
        {
            if (mapData == null) return;

            int warnings = 0;

            foreach (var levelData in mapData.Levels)
            {
                foreach (var entityData in levelData.Entities)
                {
                    warnings += ValidateEntityData(entityData, levelData.Name);
                }

                foreach (var triggerData in levelData.Triggers)
                {
                    warnings += ValidateEntityData(triggerData, levelData.Name);
                }
            }

            if (warnings > 0)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                    $"Map '{mapName}' data validation: {warnings} warnings");
            }
        }

        private static int ValidateEntityData(EntityData data, string roomName)
        {
            int warnings = 0;
            string entityName = data.Name ?? "unknown";

            // Check for common entity misconfigurations
            switch (entityName)
            {
                case "DialogTrigger":
                    string dialogKey = data.Attr("dialog", "");
                    if (string.IsNullOrEmpty(dialogKey))
                    {
                        Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                            $"[{roomName}] DialogTrigger has empty dialog key");
                        warnings++;
                    }
                    break;

                case "TeleportTrigger":
                case "RoomTeleportTrigger":
                    string targetRoom = data.Attr("targetRoom", "");
                    if (string.IsNullOrEmpty(targetRoom))
                    {
                        Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                            $"[{roomName}] {entityName} has empty targetRoom");
                        warnings++;
                    }
                    break;

                case "NPC":
                    string npcDialog = data.Attr("dialogId", "");
                    if (string.IsNullOrEmpty(npcDialog))
                    {
                        Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                            $"[{roomName}] NPC has empty dialogId");
                        warnings++;
                    }
                    break;
            }

            // Check for invalid trigger IDs
            if (data.Has("trigger"))
            {
                int triggerId = data.Int("trigger", -1);
                if (triggerId < 0)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper/LevelValidator",
                        $"[{roomName}] {entityName} has invalid trigger ID: {triggerId}");
                    warnings++;
                }
            }

            return warnings;
        }

        #endregion

        #region Helper Methods

        private static IEnumerable<string> GetLoadedDialogKeys()
        {
            // Access the internal dialog dictionary through reflection
            try
            {
                var dialogType = typeof(Dialog);
                var languageField = dialogType.GetField("Language", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (languageField?.GetValue(null) is Dictionary<string, string> language)
                {
                    return language.Keys;
                }
            }
            catch
            {
                // Fallback: return empty
            }
            return Enumerable.Empty<string>();
        }

        #endregion

        #region Integration Hook

        /// <summary>
        /// Hooks into Everest level load to validate automatically.
        /// </summary>
        public static void HookIntoLevelLoad()
        {
            Everest.Events.Level.OnLoadLevel += (level, playerIntro, isFromLoader) =>
            {
                if (isFromLoader)
                {
                    ValidateLevel(level);
                }
            };
        }

        #endregion
    }
}
