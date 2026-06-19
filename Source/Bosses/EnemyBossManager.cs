
namespace Celeste.Entities
{
    public class BossData
    {
        public string BossType { get; set; }
        public Vector2 Position { get; set; }
        public bool Defeated { get; set; }
    }

    public static class EnemyBossManager
    {
        private static Dictionary<string, List<EntityData>> roomEnemies = new();
        private static Dictionary<string, BossData> roomBosses = new();

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
                "DZ/MetaKnightTerminatorBoss" => new MetaKnightTerminatorBoss(enemyData, offset),
                "DZ/DigitalKingDDDBoss" => new DigitalKingDDDBoss(enemyData, offset),
                "DZ/MartletBirdPossessBoss" => new MartletBirdPossessBoss(enemyData, offset),
                "DZ/BlackDarkMatterBoss" => new BlackDarkMatterBoss(enemyData, offset),
                "DZ/DarkMatterKnifeBoss" => new DarkMatterKnifeBoss(enemyData, offset),

                // Tier Bosses from BossTiers.cs
                "DZ/BossTier1" => new BossTier1(enemyData, offset),
                "DZ/BossTier2" => new BossTier2(enemyData, offset),
                "DZ/BossTier3" => new BossTier3(enemyData, offset),
                "DZ/BossTier4" => new BossTier4(enemyData, offset),
                "DZ/BossTier5" => new BossTier5(enemyData, offset),
                "DZ/BossTier6" => new BossTier6(enemyData, offset),

                // Base Boss class
                "DZ/Boss" => new Boss(enemyData, offset),

                _ => null
            };
        }

        private static Entity createBossFromData(BossData bossData)
        {
            // Build a minimal EntityData from BossData without relying on a
            // parameterless constructor or a settable Values dictionary.
            var entityData = new EntityData();
            entityData.Position = bossData.Position;
            entityData.Name = "DZ/Boss";
            // Populate the Values dictionary via reflection so the Boss ctor can
            // read data.Attr("bossType") as expected.
            var valuesField = typeof(EntityData).GetField("Values",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (valuesField != null)
            {
                var dict = valuesField.GetValue(entityData) as Dictionary<string, object>;
                if (dict == null)
                {
                    dict = new Dictionary<string, object>();
                    valuesField.SetValue(entityData, dict);
                }
                dict["bossType"] = bossData.BossType ?? "Generic";
            }

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
