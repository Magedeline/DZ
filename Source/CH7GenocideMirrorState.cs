using System;
using Microsoft.Xna.Framework;

namespace Celeste.Cutscenes
{
    public static class CH7GenocideMirrorState
    {
        public const string EnabledFlag = "ch7_genocide_mirror_enabled";
        public const string StartedFlag = "ch7_genocide_mirror_started";
        public const string VisionIntroPlayedFlag = "ch7_genocide_vision_intro_played";
        public const string VisionFinalePlayedFlag = "ch7_genocide_vision_finale_played";
        public const string WakeupPlayedFlag = "ch7_genocide_wakeup_played";
        public const string FloweySeenFlag = "ch7_genocide_flowey_seen";
        public const string EquipmentTakenFlag = "ch7_genocide_equipment_taken";
        public const string CompletedFlag = "ch7_genocide_mirror_completed";

        public const string VisionIntroRoom = "geno_intro";
        public const string ExecutionRoom = "geno_execution";
        public const string WakeupRoom = "d-00";

        public static bool IsEnabled(Session session)
        {
            return session != null && session.GetFlag(EnabledFlag);
        }

        public static bool HasRoom(Level level, string roomName)
        {
            return TryGetRoom(level, roomName, out _);
        }

        public static Vector2 GetRespawnProbe(Level level, string roomName = null)
        {
            if (TryGetRoom(level, roomName ?? level?.Session?.Level, out LevelData levelData))
            {
                Rectangle bounds = levelData.Bounds;
                return new Vector2(bounds.Left, bounds.Top);
            }

            if (level != null)
            {
                Rectangle bounds = level.Bounds;
                return new Vector2(bounds.Left, bounds.Top);
            }

            return Vector2.Zero;
        }

        private static bool TryGetRoom(Level level, string roomName, out LevelData levelData)
        {
            levelData = null;
            if (level?.Session?.MapData == null || string.IsNullOrWhiteSpace(roomName))
            {
                return false;
            }

            levelData = level.Session.MapData.Get(roomName);
            return levelData != null;
        }
    }
}