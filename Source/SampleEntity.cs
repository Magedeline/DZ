namespace MaggyHelper.Entities;

[CustomEntity(ids: "MaggyHelper/SampleEntity")]
[Tracked]
public class SampleEntity : Entity
{
    public SampleEntity(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        // TODO: read properties from data
        Add(GFX.SpriteBank.Create("sampleEntity"));
        Collider = new Hitbox(16, 16, -8, -8);
    }
}



