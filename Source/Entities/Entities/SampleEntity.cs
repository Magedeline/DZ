namespace Celeste.Entities;

[CustomEntity(ids: "DZ/SampleEntity")]
[Tracked]
public class SampleEntity : Entity
{
    public SampleEntity(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        // Sample entity for testing - reads basic properties from data
        bool isActive = data.Bool("active", true);
        float speed = data.Float("speed", 100f);
        Add(GFX.SpriteBank.Create("sampleEntity"));
        Collider = new Hitbox(16, 16, -8, -8);
    }
}



