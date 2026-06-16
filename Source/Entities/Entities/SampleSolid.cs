namespace Celeste.Entities;

[CustomEntity(ids: "DZ/SampleSolid")]
public class SampleSolid : Solid
{
    public SampleSolid(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, true)
    {
        // Sample solid for testing - reads basic properties from data
        bool climbable = data.Bool("climbable", false);
        // Note: SurfaceIndex is typically set by the base class or through specific methods
        // int surfaceIndexValue = data.Int("surfaceIndex", 0);
    }
}



