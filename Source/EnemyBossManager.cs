

// Add this namespace if 'Enemy' is defined here

namespace MaggyHelper.Entities
{
    public static class EnemyBossManager
    {
        private static Dictionary<string, List<EntityData>> roomEnemies = new();
        private static Dictionary<string, BossData> roomBosses = new();

        public class BossData
        {
            public string BossType { get; set; }
            public Vector2 Position { get; set; }
            public bool Defeated { get; set; }
        }

        public static void RegisterRoomEnemies(string room, List<EntityData> enemies)
        {
            roomEnemies[room] = enemies;
        }

        public static void RegisterRoomBoss(string room, BossData boss)
        {
            roomBosses[room] = boss;
        }

        public static void OnRoomTransition(Level level, string fromRoom, string toRoom)
        {
            // Spawn new room enemies  
            if (roomEnemies.TryGetValue(toRoom, out var enemies))
            {
                foreach (var enemyData in enemies)
                {
                    level.Add(createBossFromData(enemyData)); // Replace 'enemyData.Create' with a helper method  
                }
            }

            // Check for boss  
            if (roomBosses.TryGetValue(toRoom, out var boss) && !boss.Defeated)
            {
                level.Add(createBossFromData(boss)); // Replace 'BossData.Create' with a helper method  
            }
        }

        private static Entity createBossFromData(EntityData enemyData) {
            throw new NotImplementedException();
        }

        private static Entity createBossFromData(BossData bossData)
        {
            // Create a Boss entity from BossData
            // Convert BossData to EntityData format
            var entityData = new EntityData
            {
                Position = bossData.Position,
                Values = new Dictionary<string, object>
                {
                    ["bossType"] = bossData.BossType
                }
            };
            
            return new Boss(entityData, Vector2.Zero);
        }

        /// <summary>
        /// Clear all registered room data. Call on level transitions to prevent stale data.
        /// </summary>
        public static void Reset()
        {
            roomEnemies.Clear();
            roomBosses.Clear();
        }
    }
}



