using Celeste.Cutscenes;
using Celeste.Entities;

namespace Celeste.NPCs;

[Tracked(true)]
[CustomEntity(ids: "DZ/NPC08_Madeline_Plateau")]
public class Npc08MadelinePlateau : global::Celeste.NPC
{
    private float speedY;

    private MadelineNPCDummy dummy;

    public Npc08MadelinePlateau(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        dummy = new MadelineNPCDummy(Vector2.Zero);
        Add(Sprite = dummy.Sprite);
        IdleAnim = "idle";
        MoveAnim = "walk";
        Maxspeed = 48f;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Player entity = base.Scene.Tracker.GetEntity<Player>();
        
        // Only trigger the cutscene if it hasn't been played yet (check flag on room entry)
        if (entity != null && scene is Level level && !level.Session.GetFlag(Cs08Campfire.FLAG))
        {
            base.Scene.Add(new Cs08Campfire(this, entity, this));
        }
        
        Add(base.Light = new VertexLight(new Vector2(0f, -6f), Color.White, 1f, 16, 48));
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
