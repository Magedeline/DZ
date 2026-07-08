using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC00_Theo")]
    [HotReloadable]
public class NPC00_Theo : NPC
{
    private bool talking;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC00_Theo(Vector2 position)
        : base(position)
    {
        Add(Sprite = GFX.SpriteBank.Create("theo"));
        Sprite.Play("idle");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if ((scene as Level).Session.GetFlag("theo"))
            RemoveSelf();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        Player entity = Level.Tracker.GetEntity<Player>();
        if (entity != null && !base.Session.GetFlag("theo") && !talking)
        {
            int num = Level.Bounds.Left + 96;
            if (entity.OnGround() && entity.X >= (float)num && entity.X <= base.X + 16f && Math.Abs(entity.Y - base.Y) < 4f && entity.Facing == (Facings)Math.Sign(base.X - entity.X))
            {
                talking = true;
                base.Scene.Add(new CS00_Theo(this, entity));
            }
        }
        base.Update();
    }
}
