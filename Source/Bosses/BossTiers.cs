namespace Celeste.Entities
{
    // Tier 1 - Lowest Boss
    [CustomEntity(ids: "MaggyHelper/BossTier1")]
    [Tracked]
    [HotReloadable]
    public class BossTier1 : Boss
    {
        public BossTier1(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tier = BossTier.Lowest;
            Gimmick = GimmickAbility.None;
            BossType = "BasicEnemy";
        }
    }

    // Tier 2 - Low Boss
    [CustomEntity(ids: "MaggyHelper/BossTier2")]
    [Tracked]
    [HotReloadable]
    public class BossTier2 : Boss
    {
        public BossTier2(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tier = BossTier.Low;
            Gimmick = GimmickAbility.Teleport;
            BossType = "ElementalGuardian";
        }
    }

    // Tier 3 - Mid Boss
    [CustomEntity(ids: "MaggyHelper/BossTier3")]
    [Tracked]
    [HotReloadable]
    public class BossTier3 : Boss
    {
        public BossTier3(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tier = BossTier.Mid;
            Gimmick = GimmickAbility.TimeFreeze;
            BossType = "ShadowWarrior";
        }
    }

    // Tier 4 - High Boss
    [CustomEntity(ids: "MaggyHelper/BossTier4")]
    [Tracked]
    [HotReloadable]
    public class BossTier4 : Boss
    {
        public BossTier4(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tier = BossTier.High;
            Gimmick = GimmickAbility.ElementalFusion;
            BossType = "CrystalLord";
        }
    }

    // Tier 5 - Highest Boss
    [CustomEntity(ids: "MaggyHelper/BossTier5")]
    [Tracked]
    [HotReloadable]
    public class BossTier5 : Boss
    {
        public BossTier5(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tier = BossTier.Highest;
            Gimmick = GimmickAbility.GravityControl;
            BossType = "VoidKnight";
        }
    }

    // Tier 6 - Final Boss (Blackhole Angel / The End)
    [CustomEntity(ids: "MaggyHelper/BossTier6")]
    [Tracked]
    [HotReloadable]
    public class BossTier6 : Boss
    {
        public BossTier6(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tier = BossTier.Final;
            Gimmick = GimmickAbility.DimensionRift;
            BossType = "BlackholeAngel";
        }
    }
}



