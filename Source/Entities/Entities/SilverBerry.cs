#nullable enable

using CelesteEntities = global::Celeste.Mod.Entities;

namespace Celeste.Entities
{
    [CelesteEntities.CustomEntity("DZ/SilverBerry")]
    [Monocle.Tracked]
    [CelesteEntities.RegisterStrawberry(tracked: true, blocksCollection: true)]
    [TrackedAs(typeof(CelesteStrawberry))]
    internal class SilverBerry : PinkPlatinumBerry
    {
        protected override string SpriteName => "strawberry";

        public SilverBerry(EntityData data, Vector2 offset, EntityID gid)
            : base(data, offset, gid)
        {
            // Silver visual override on the vanilla strawberry sprite
            this.sprite.Color = Color.Silver;
        }

        public override void Added(Scene scene)
        {
            Level? level = scene as Level;
            if (level == null || !HasAllABCHearts(level))
            {
                RemoveSelf();
                return;
            }

            base.Added(scene);
            this.sprite.Color = Color.Silver;
        }

        public override void OnCollect()
        {
            if (this.collected)
                return;

            Level? level = this.Scene as Level;
            Player? player = this.Scene?.Tracker?.GetEntity<Player>();

            base.OnCollect();
            SpawnRewardBerries(level, player);
            UnlockDSide(level);
        }

        protected override void RecordCollection(Level level, string berryId)
        {
            // Do not count the silver berry itself as a pink platinum berry;
            // the rewards are tracked separately.
        }

        private static bool HasAllABCHearts(Level level)
        {
            AreaData? area = AreaData.Get(level.Session.Area);
            if (area == null || area.Mode == null || area.Mode.Length < 3)
                return false;

            AreaStats? stats = SaveData.Instance.GetAreaStatsFor(level.Session.Area);
            if (stats == null)
                return false;

            return stats.Modes[0].HeartGem
                && stats.Modes[1].HeartGem
                && stats.Modes[2].HeartGem;
        }

        private void SpawnRewardBerries(Level? level, Player? player)
        {
            if (level == null || player == null)
                return;

            SpawnAndCollect<PinkPlatinumBerry>(level, player, "DZ/PinkPlatinumStrawberry", new Vector2(-8f, -16f));
            SpawnAndCollect<PenumbraPhantasmBerry>(level, player, "DZ/PenumbraPhantasmBerry", new Vector2(8f, -16f));
        }

        private void SpawnAndCollect<T>(Level level, Player player, string entityName, Vector2 offset)
            where T : PinkPlatinumBerry
        {
            EntityData data = new EntityData
            {
                Position = player.Position + offset,
                ID = Calc.Random.Next(),
                Name = entityName
            };

            EntityID id = new EntityID(level.Session.Level, data.ID);
            PinkPlatinumBerry berry = entityName switch
            {
                "DZ/PinkPlatinumStrawberry" => new PinkPlatinumBerry(data, Vector2.Zero, id),
                "DZ/PenumbraPhantasmBerry" => new PenumbraPhantasmBerry(data, Vector2.Zero, id),
                _ => new PinkPlatinumBerry(data, Vector2.Zero, id)
            };

            level.Add(berry);
            level.Entities.UpdateLists();
            berry.OnCollect();
        }

        private static void UnlockDSide(Level? level)
        {
            if (level == null)
                return;

            AreaData? area = AreaData.Get(level.Session.Area);
            if (area == null)
                return;

            global::DZ.Celeste2Hooks.UnlockDSide(area);
        }
    }
}
