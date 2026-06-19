using Microsoft.Xna.Framework;

namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity("DZ/NPC_MadelineMother")]
    public class NpcMadelineMother : NpcBase
    {
        public NpcMadelineMother(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "MADELINE_MOTHER_TALK")
        {
            AddSprite("madeline_mother", new Color(200, 200, 200));
        }
    }

    [Tracked(true)]
    [CustomEntity("DZ/NPC_TheoSisterAlexa")]
    public class NpcTheoSisterAlexa : NpcBase
    {
        public NpcTheoSisterAlexa(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "THEO_SISTER_ALEXA_TALK")
        {
            AddSprite("theo_sister_alexa", new Color(100, 180, 255));
        }
    }

    [Tracked(true)]
    [CustomEntity("DZ/NPC_TheoSisterMadeline")]
    public class NpcTheoSisterMadeline : NpcBase
    {
        public NpcTheoSisterMadeline(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "THEO_SISTER_MADELINE_TALK")
        {
            AddSprite("theo_sister_madeline", new Color(255, 150, 180));
        }
    }
}
