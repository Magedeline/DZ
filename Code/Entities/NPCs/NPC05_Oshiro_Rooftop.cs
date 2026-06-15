using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Cutscenes;
using ModOshiroSprite = Celeste.MaggyOshiroSprite;

namespace Celeste.NPCs;

[CustomEntity("MaggyHelper/NPC05_Oshiro_Rooftop")]

public class NPC05_Oshiro_Rooftop : NPC
{
    public NPC05_Oshiro_Rooftop(Vector2 position)
        : base(position)
    {
        Add(Sprite = new ModOshiroSprite(1));
        (Sprite as ModOshiroSprite).AllowTurnInvisible = false;
        MoveAnim = "move";
        IdleAnim = "idle";
        Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (base.Session.GetFlag("oshiro_resort_roof"))
        {
            RemoveSelf();
            return;
        }
        Visible = false;
        base.Scene.Add(new CS05_OshiroRooftop(this));
    }
}
