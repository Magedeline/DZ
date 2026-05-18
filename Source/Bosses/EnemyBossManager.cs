

// Add this namespace if 'Enemy' is defined here

namespace Celeste.Entities
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
                    var entity = createBossFromData(enemyData);
                    if (entity != null) level.Add(entity);
                }
            }

            // Check for boss  
            if (roomBosses.TryGetValue(toRoom, out var boss) && !boss.Defeated)
            {
                level.Add(createBossFromData(boss)); // Replace 'BossData.Create' with a helper method  
            }
        }

        private static Entity createBossFromData(EntityData enemyData)
        {
            Vector2 offset = Vector2.Zero;

            return enemyData.Name switch
            {
                // Mini Bosses from MiniBosses.cs
                "MaggyHelper/MetaKnightTerminatorBoss" => new MetaKnightTerminatorBoss(enemyData, offset),
                "MaggyHelper/DigitalKingDDDBoss" => new DigitalKingDDDBoss(enemyData, offset),
                "MaggyHelper/MartletBirdPossessBoss" => new MartletBirdPossessBoss(enemyData, offset),
                "MaggyHelper/BlackDarkMatterBoss" => new BlackDarkMatterBoss(enemyData, offset),
                "MaggyHelper/DarkMatterKnifeBoss" => new DarkMatterKnifeBoss(enemyData, offset),

                // Tier Bosses from BossTiers.cs
                "MaggyHelper/BossTier1" => new BossTier1(enemyData, offset),
                "MaggyHelper/BossTier2" => new BossTier2(enemyData, offset),
                "MaggyHelper/BossTier3" => new BossTier3(enemyData, offset),
                "MaggyHelper/BossTier4" => new BossTier4(enemyData, offset),
                "MaggyHelper/BossTier5" => new BossTier5(enemyData, offset),
                "MaggyHelper/BossTier6" => new BossTier6(enemyData, offset),

                // Base Boss class
                "MaggyHelper/Boss" => new Boss(enemyData, offset),

                _ => null
            };
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
