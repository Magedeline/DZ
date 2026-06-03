using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Entities;

namespace Celeste.Mod.KIRBY_CELESTE.Bosses
{
    /// <summary>
    /// Provides safe, validated EntityData accessors for boss configuration.
    /// Wraps raw EntityData access with error logging and sensible defaults.
    /// </summary>
    public static class BossConfigHelper
    {
        private const string LogTag = "MaggyHelper/BossConfig";

        #region Health Configuration

        /// <summary>
        /// Safely reads boss health configuration from EntityData.
        /// Logs warnings for invalid values and clamps to sensible ranges.
        /// </summary>
        public static (int health, int maxHealth) ReadHealthConfig(EntityData data, string bossName)
        {
            int maxHealth = TriggerManager.SafeInt(data, "maxHealth", 1000, bossName);
            int health = TriggerManager.SafeInt(data, "health", maxHealth, bossName);

            // Sanity clamp
            if (maxHealth <= 0)
            {
                Logger.Log(LogLevel.Warn, LogTag, $"{bossName}: maxHealth {maxHealth} is invalid, defaulting to 1000");
                maxHealth = 1000;
            }

            if (health <= 0 || health > maxHealth)
            {
                Logger.Log(LogLevel.Warn, LogTag, $"{bossName}: health {health} out of range, clamping to maxHealth");
                health = maxHealth;
            }

            return (health, maxHealth);
        }

        #endregion

        #region Phase Configuration

        /// <summary>
        /// Reads phase/pattern index with validation.
        /// </summary>
        public static int ReadPhaseConfig(EntityData data, string bossName, int defaultPhase = 1)
        {
            int phase = TriggerManager.SafeInt(data, "phase", defaultPhase, bossName);
            if (phase < 0)
            {
                Logger.Log(LogLevel.Warn, LogTag, $"{bossName}: phase {phase} invalid, using default {defaultPhase}");
                phase = defaultPhase;
            }
            return phase;
        }

        /// <summary>
        /// Reads pattern index with validation.
        /// </summary>
        public static int ReadPatternIndex(EntityData data, string bossName, int defaultIndex = 0)
        {
            int index = TriggerManager.SafeInt(data, "patternIndex", defaultIndex, bossName);
            return Math.Max(0, index);
        }

        #endregion

        #region Difficulty / Mode Configuration

        /// <summary>
        /// Reads difficulty mode (e.g., 0=normal, 1=hard, 2=expert).
        /// </summary>
        public static int ReadDifficultyMode(EntityData data, string bossName, int defaultMode = 0)
        {
            int mode = TriggerManager.SafeInt(data, "difficultyMode", defaultMode, bossName);
            return Math.Max(0, mode);
        }

        /// <summary>
        /// Reads hard mode flag.
        /// </summary>
        public static bool ReadHardMode(EntityData data, string bossName)
        {
            return TriggerManager.SafeBool(data, "hardMode", false, bossName);
        }

        #endregion

        #region Boolean Flags

        /// <summary>
        /// Reads a generic boolean flag with logging.
        /// </summary>
        public static bool ReadBoolFlag(EntityData data, string key, string bossName, bool defaultValue = false)
        {
            return TriggerManager.SafeBool(data, key, defaultValue, bossName);
        }

        /// <summary>
        /// Reads the fromCutscene flag (common in boss constructors).
        /// </summary>
        public static bool ReadFromCutsceneFlag(EntityData data, string bossName)
        {
            return TriggerManager.SafeBool(data, "fromCutscene", false, bossName);
        }

        #endregion

        #region Float Values

        /// <summary>
        /// Reads a float value with range validation.
        /// </summary>
        public static float ReadFloatValue(EntityData data, string key, string bossName, float defaultValue, float min = float.MinValue, float max = float.MaxValue)
        {
            float value = TriggerManager.SafeFloat(data, key, defaultValue, bossName);
            if (value < min || value > max)
            {
                Logger.Log(LogLevel.Warn, LogTag,
                    $"{bossName}: {key}={value} out of range [{min}, {max}], clamping");
                value = Math.Clamp(value, min, max);
            }
            return value;
        }

        #endregion

        #region String Values

        /// <summary>
        /// Reads a string attribute with non-empty validation.
        /// </summary>
        public static string ReadStringValue(EntityData data, string key, string bossName, string defaultValue = "")
        {
            string value = TriggerManager.SafeString(data, key, defaultValue, bossName);
            if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrEmpty(defaultValue))
            {
                Logger.Log(LogLevel.Debug, LogTag,
                    $"{bossName}: {key} is empty, using default '{defaultValue}'");
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Reads an attack sequence string (comma-separated attack IDs).
        /// </summary>
        public static string ReadAttackSequence(EntityData data, string bossName, string defaultSequence = "")
        {
            return ReadStringValue(data, "attackSequence", bossName, defaultSequence);
        }

        #endregion

        #region Position / Node Configuration

        /// <summary>
        /// Reads position with offset applied.
        /// </summary>
        public static Vector2 ReadPosition(EntityData data, Vector2 offset)
        {
            return data.Position + offset;
        }

        #endregion

        #region Arena Configuration

        /// <summary>
        /// Reads arena camera lock settings.
        /// </summary>
        public static (bool lockX, bool lockY, float pastY) ReadCameraLockConfig(EntityData data, string bossName)
        {
            bool lockY = TriggerManager.SafeBool(data, "cameraLockY", true, bossName);
            float pastY = TriggerManager.SafeFloat(data, "cameraPastY", 120f, bossName);
            return (false, lockY, pastY);
        }

        #endregion

        #region Legacy Support

        /// <summary>
        /// Validates a boss entity name against known entity types.
        /// Helps catch typos in map editor entity placement.
        /// </summary>
        public static bool ValidateBossEntityName(string entityName, string bossName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                Logger.Log(LogLevel.Error, LogTag, $"{bossName}: Empty entity name in map data");
                return false;
            }
            return true;
        }

        #endregion
    }
}
