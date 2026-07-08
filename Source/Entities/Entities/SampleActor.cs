namespace Celeste.Entities;

[CustomEntity(ids: "DZ/SampleActor")]
public class SampleActor : Actor
{
    public SampleActor(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        // Reading properties from data
        var actorType = data.Attr("type", "default");
        var isActive = data.Bool("active", true);
        var health = data.Int("health", 100);

        // Additional initialization logic based on properties
        if (actorType == "enemy")
        {
            // Example: Set specific behavior for enemy type
            // Initialize enemy-specific properties
        }

        if (!isActive)
            // Example: Disable actor if not active
            RemoveSelf();

        // Example: Set health or other properties
        // Initialize health or other attributes
    }
}



