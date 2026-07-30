using Celeste.Cutscenes;
using Celeste.Entities;

namespace Celeste.NPCs;

[Tracked(true)]
[CustomEntity(ids: "DZ/NPC08_Madeline_Plateau")]
public class Npc08MadelinePlateau : global::Celeste.NPC
{
    public const string SET_FLAG = "ch8_plat";

    private float speedY;

    public Npc08MadelinePlateau(EntityData data, Vector2 offset)
            : base(data.Position + offset)
    {
        Add(Sprite = GFX.SpriteBank.Create("madeline"));
        IdleAnim = "idle";
        MoveAnim = "walk";
        Maxspeed = 48f;
        MoveY = false;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        var player = Scene.Tracker.GetEntity<Player>();
        if (player != null)
        {
            Scene.Add(new Cs08Campfire(this, player));
        }

        Add(base.Light = new VertexLight(new Vector2(0f, -6f), Color.White, 1f, 16, 48));

        if (scene is Level level)
        {
            level.Session.SetFlag(SET_FLAG, true);
        }
    }

    public override void Removed(Scene scene)
    {
        if (scene is Level level)
        {
            level.Session.SetFlag(SET_FLAG, false);
        }
        base.Removed(scene);
    }

    public override void Update()
    {
        base.Update();
        if (!CollideCheck<Solid>(Position + new Vector2(0f, 1f)))
        {
            speedY += 400f * Engine.DeltaTime;
            Position.Y += speedY * Engine.DeltaTime;
        }
        else
        {
            speedY = 0f;
        }
    }
}
