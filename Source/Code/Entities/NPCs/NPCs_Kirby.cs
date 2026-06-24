using Microsoft.Xna.Framework;

namespace Celeste.NPCs
{
    [CustomEntity("MaggyHelper/NPC_Magolor")]
    public class NpcMagolor : NpcBase
    {
        public NpcMagolor(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "MAGOLOR_TALK")
        {
            AddSprite("magolor", new Color(150, 100, 255));
        }
    }

    [CustomEntity("MaggyHelper/NPC_Hyness")]
    public class NpcHyness : NpcBase
    {
        public NpcHyness(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "HYNESS_TALK")
        {
            AddSprite("hyness", new Color(50, 50, 100));
        }
    }

    [CustomEntity("MaggyHelper/NPC_MageFrancisca")]
    public class NpcMageFrancisca : NpcBase
    {
        public NpcMageFrancisca(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "FRANCISCA_TALK")
        {
            AddSprite("francisca", new Color(100, 200, 255));
        }
    }

    [CustomEntity("MaggyHelper/NPC_MageFlamberge")]
    public class NpcMageFlamberge : NpcBase
    {
        public NpcMageFlamberge(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "FLAMBERGE_TALK")
        {
            AddSprite("flamberge", new Color(255, 100, 100));
        }
    }

    [CustomEntity("MaggyHelper/NPC_MageZanPartizanne")]
    public class NpcMageZanPartizanne : NpcBase
    {
        public NpcMageZanPartizanne(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "ZAN_PARTIZANNE_TALK")
        {
            AddSprite("zan_partizanne", new Color(255, 255, 100));
        }
    }

    [CustomEntity("MaggyHelper/NPC_SusieHaltmann")]
    public class NpcSusieHaltmann : NpcBase
    {
        public NpcSusieHaltmann(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "SUSIE_HALTMANN_TALK")
        {
            AddSprite("susie_haltmann", new Color(200, 100, 200));
        }
    }

    [CustomEntity("MaggyHelper/NPC_Adeleine")]
    public class NpcAdeleine : NpcBase
    {
        public NpcAdeleine(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "ADELEINE_TALK")
        {
            AddSprite("adeleine", new Color(255, 150, 200));
        }
    }

    [CustomEntity("MaggyHelper/NPC_Ribbon")]
    public class NpcRibbon : NpcBase
    {
        public NpcRibbon(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "RIBBON_TALK")
        {
            AddSprite("ribbon", new Color(255, 200, 255));
        }
    }

    [CustomEntity("MaggyHelper/NPC_BandanaDee")]
    public class NpcBandanaDee : NpcBase
    {
        public NpcBandanaDee(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "BANDANA_DEE_TALK")
        {
            AddSprite("bandana_dee", new Color(50, 200, 100));
        }
    }

    [CustomEntity("MaggyHelper/NPC_KingDedede")]
    public class NpcKingDedede : NpcBase
    {
        public NpcKingDedede(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "KING_DEDEDE_TALK")
        {
            AddSprite("king_dedede", new Color(255, 50, 200));
        }
    }
}
