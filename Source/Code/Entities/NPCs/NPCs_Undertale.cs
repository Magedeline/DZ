using Microsoft.Xna.Framework;

namespace Celeste.NPCs
{
    [CustomEntity("MaggyHelper/NPC_Sans")]
    public class NpcSans : NpcBase
    {
        public NpcSans(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "SANS_TALK")
        {
            AddSprite("sans", new Color(100, 150, 255));
        }
    }

    [CustomEntity("MaggyHelper/NPC_Undyne")]
    public class NpcUndyne : NpcBase
    {
        public NpcUndyne(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "UNDYNE_TALK")
        {
            AddSprite("undyne", new Color(255, 50, 100));
        }
    }

    [CustomEntity("MaggyHelper/NPC_Alphys")]
    public class NpcAlphys : NpcBase
    {
        public NpcAlphys(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "ALPHYS_TALK")
        {
            AddSprite("alphys", new Color(255, 200, 100));
        }
    }

    [CustomEntity("MaggyHelper/NPC_Asgore")]
    public class NpcAsgore : NpcBase
    {
        public NpcAsgore(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "ASGORE_TALK")
        {
            AddSprite("asgore", new Color(200, 150, 50));
        }
    }

    [CustomEntity("MaggyHelper/NPC_Papyrus")]
    public class NpcPapyrus : NpcBase
    {
        public NpcPapyrus(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "PAPYRUS_TALK")
        {
            AddSprite("papyrus", new Color(255, 100, 50));
        }
    }

    [CustomEntity("MaggyHelper/NPC_Susie")]
    public class NpcSusie : NpcBase
    {
        public NpcSusie(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "SUSIE_TALK")
        {
            AddSprite("susie", new Color(150, 50, 200));
        }
    }

    [CustomEntity("MaggyHelper/NPC_TobySleepingDog")]
    public class NpcTobySleepingDog : NpcBase
    {
        public NpcTobySleepingDog(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "TOBY_DOG_TALK")
        {
            AddSprite("toby_dog", new Color(100, 100, 100));
            CanInteract = false;
        }
    }
}
