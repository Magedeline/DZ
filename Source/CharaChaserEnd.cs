using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities;
    
[Tracked(true)]
[CustomEntity(new string[] { "MaggyHelper/CharaChaserEnd" })]
[HotReloadable]
public class CharaChaserEnd : Entity
{
    public CharaChaserEnd(Vector2 position, int width, int height)
        : base(position)
    {
        base.Collider = new Hitbox(width, height);
    }

    public CharaChaserEnd(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height)
    {
    }
}
