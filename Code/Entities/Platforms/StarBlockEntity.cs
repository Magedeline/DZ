namespace Celeste.Entities;

[CustomEntity("MaggyHelper/StarBlock")]
[Tracked(true)]
[HotReloadable]
public sealed class StarBlockEntity : StarBlock
{
    public StarBlockEntity(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height)
    {
    }
}
