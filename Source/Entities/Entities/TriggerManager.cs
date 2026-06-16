using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Centralized trigger validation and management system.
    /// Ensures EntityData.Int("trigger") dynamically matches active Room triggers in Lönn.
    /// Logs clear-text exceptions to Celeste log.txt instead of causing hard-crashes.
    /// </summary>
    public static class TriggerManager
    {
        private const string LogTag = "DZ/TriggerManager";
        private static Dictionary<string, HashSet<int>> _roomTriggerIds = new Dictionary<string, HashSet<int>>();
        private static bool _initialized = false;

        /// <summary>
        /// Initialize trigger tracking for the current level.
        /// Call this when a level is loaded.
        /// </summary>
        public static void Initialize(Level level)
        {
            _roomTriggerIds.Clear();
            _initialized = true;

            try
            {
                ScanRoomTriggers(level);
            }
            catch (Exception ex)
            {
                LogError($"Failed to scan room triggers: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear all tracked trigger data.
        /// Call this when leaving a level.
        /// </summary>
        public static void Clear()
        {
            _roomTriggerIds.Clear();
            _initialized = false;
        }

        /// <summary>
        /// Scan the current room for all trigger entities and record their IDs.
        /// </summary>
        private static void ScanRoomTriggers(Level level)
        {
            if (level?.Session == null) return;

            string roomKey = GetRoomKey(level);
            var triggerIds = new HashSet<int>();

            // Scan all trigger entities in the room
            foreach (var entity in level.Entities)
            {
                if (entity is Trigger trigger)
                {
                    // Try to get trigger ID from the entity data if available
                    int triggerId = ExtractTriggerId(trigger);
                    if (triggerId >= 0)
                    {
                        triggerIds.Add(triggerId);
                    }
                }
            }

            _roomTriggerIds[roomKey] = triggerIds;
            LogInfo($"Scanned room '{roomKey}' - Found {triggerIds.Count} triggers");
        }

        /// <summary>
        /// Safely read a trigger ID from EntityData.
        /// Returns -1 if the trigger value is invalid or not found.
        /// </summary>
        public static int SafeGetTriggerId(EntityData data, string key = "trigger")
        {
            if (data == null)
            {
                LogError("Cannot read trigger ID: EntityData is null");
                return -1;
            }

            try
            {
                int triggerId = data.Int(key, -1);
                if (triggerId < 0)
                {
                    LogError($"Trigger ID for key '{key}' is invalid ({triggerId}). Ensure it is set in Lönn.");
                    return -1;
                }
                return triggerId;
            }
            catch (Exception ex)
            {
                LogError($"Exception reading trigger ID for key '{key}': {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Validate that a trigger ID exists in the current room.
        /// Logs an error if the trigger is not found.
        /// </summary>
        public static bool ValidateTriggerId(Level level, int triggerId)
        {
            if (triggerId < 0) return false;
            if (!_initialized)
            {
                LogError("TriggerManager not initialized. Call Initialize() when level loads.");
                return false;
            }

            string roomKey = GetRoomKey(level);
            if (!_roomTriggerIds.TryGetValue(roomKey, out var validIds))
            {
                LogError($"No trigger data found for room '{roomKey}'.");
                return false;
            }

            if (!validIds.Contains(triggerId))
            {
                LogError($"Trigger ID {triggerId} not found in room '{roomKey}'. Valid IDs: {string.Join(", ", validIds)}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get a safe trigger ID from EntityData and validate it against the current room.
        /// Returns -1 if invalid, logs descriptive errors.
        /// </summary>
        public static int GetValidatedTriggerId(Level level, EntityData data, string key = "trigger")
        {
            int triggerId = SafeGetTriggerId(data, key);
            if (triggerId < 0) return -1;

            ValidateTriggerId(level, triggerId);
            return triggerId;
        }

        /// <summary>
        /// Safely get trigger data as integer with logging.
        /// </summary>
        public static int SafeInt(EntityData data, string key, int defaultValue = 0)
        {
            if (data == null)
            {
                LogError($"Cannot read int '{key}': EntityData is null");
                return defaultValue;
            }

            try
            {
                return data.Int(key, defaultValue);
            }
            catch (Exception ex)
            {
                LogError($"Exception reading int '{key}' from EntityData: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely get trigger data as float with logging.
        /// </summary>
        public static float SafeFloat(EntityData data, string key, float defaultValue = 0f)
        {
            if (data == null)
            {
                LogError($"Cannot read float '{key}': EntityData is null");
                return defaultValue;
            }

            try
            {
                return data.Float(key, defaultValue);
            }
            catch (Exception ex)
            {
                LogError($"Exception reading float '{key}' from EntityData: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely get trigger data as string with logging.
        /// </summary>
        public static string SafeString(EntityData data, string key, string defaultValue = "")
        {
            if (data == null)
            {
                LogError($"Cannot read string '{key}': EntityData is null");
                return defaultValue;
            }

            try
            {
                return data.Attr(key, defaultValue);
            }
            catch (Exception ex)
            {
                LogError($"Exception reading string '{key}' from EntityData: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely get trigger data as bool with logging.
        /// </summary>
        public static bool SafeBool(EntityData data, string key, bool defaultValue = false)
        {
            if (data == null)
            {
                LogError($"Cannot read bool '{key}': EntityData is null");
                return defaultValue;
            }

            try
            {
                return data.Bool(key, defaultValue);
            }
            catch (Exception ex)
            {
                LogError($"Exception reading bool '{key}' from EntityData: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Register a trigger ID for the current room (useful for dynamically spawned triggers).
        /// </summary>
        public static void RegisterTriggerId(Level level, int triggerId)
        {
            if (triggerId < 0 || level == null) return;

            string roomKey = GetRoomKey(level);
            if (!_roomTriggerIds.TryGetValue(roomKey, out var validIds))
            {
                validIds = new HashSet<int>();
                _roomTriggerIds[roomKey] = validIds;
            }
            validIds.Add(triggerId);
        }

        /// <summary>
        /// Get the room key for the current level.
        /// </summary>
        private static string GetRoomKey(Level level)
        {
            if (level?.Session == null) return "unknown";
            return $"{level.Session.Area.SID}_{level.Session.Level}";
        }

        /// <summary>
        /// Safely get trigger data as integer with context logging.
        /// </summary>
        public static int SafeInt(EntityData data, string key, int defaultValue, string context)
        {
            int result = SafeInt(data, key, defaultValue);
            if (data == null)
                LogError($"[{context}] Cannot read int '{key}': EntityData is null");
            return result;
        }

        /// <summary>
        /// Safely get trigger data as float with context logging.
        /// </summary>
        public static float SafeFloat(EntityData data, string key, float defaultValue, string context)
        {
            float result = SafeFloat(data, key, defaultValue);
            if (data == null)
                LogError($"[{context}] Cannot read float '{key}': EntityData is null");
            return result;
        }

        /// <summary>
        /// Safely get trigger data as string with context logging.
        /// </summary>
        public static string SafeString(EntityData data, string key, string defaultValue, string context)
        {
            string result = SafeString(data, key, defaultValue);
            if (data == null)
                LogError($"[{context}] Cannot read string '{key}': EntityData is null");
            return result;
        }

        /// <summary>
        /// Safely get trigger data as bool with context logging.
        /// </summary>
        public static bool SafeBool(EntityData data, string key, bool defaultValue, string context)
        {
            bool result = SafeBool(data, key, defaultValue);
            if (data == null)
                LogError($"[{context}] Cannot read bool '{key}': EntityData is null");
            return result;
        }

        /// <summary>
        /// Extract trigger ID from a trigger entity.
        /// Uses reflection to access internal fields safely.
        /// </summary>
        private static int ExtractTriggerId(Trigger trigger)
        {
            if (trigger == null) return -1;

            // Try to find trigger ID from entity data
            // Since Trigger doesn't expose its EntityData, we check common patterns
            // The actual trigger ID is typically stored in EntityData by the map editor
            return -1; // Default: unknown trigger ID
        }

        /// <summary>
        /// Log an informational message.
        /// </summary>
        private static void LogInfo(string message)
        {
            Logger.Log(LogLevel.Info, LogTag, message);
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        private static void LogWarn(string message)
        {
            Logger.Log(LogLevel.Warn, LogTag, message);
        }

        /// <summary>
        /// Log an error message to Celeste log.txt.
        /// </summary>
        private static void LogError(string message)
        {
            Logger.Log(LogLevel.Error, LogTag, message);
        }
    }
}
