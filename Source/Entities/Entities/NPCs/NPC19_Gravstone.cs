using Celeste.Cutscenes;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

public class NPC19_Gravestone : NPC
{
    private const string Flag = "cs19_gravestone";

    private Vector2 boostTarget;

    private TalkComponent talk;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC19_Gravestone(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        boostTarget = data.FirstNodeNullable(offset) ?? Vector2.Zero;
        Add(talk = new TalkComponent(new Rectangle(-24, -8, 32, 8), new Vector2(-0.5f, -20f), Interact));
        talk.PlayerMustBeFacing = false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (Level.Session.GetFlag("cs19_gravestone"))
        {
            Level.Add(new CharaBoost(new Vector2[1] { boostTarget }, lockCamera: false));
            talk.RemoveSelf();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Interact(Player player)
    {
        Level.Session.SetFlag("cs19_gravestone");
        base.Scene.Add(new CS19_Gravestone(player, this, boostTarget));
        talk.Enabled = false;
    }
}
