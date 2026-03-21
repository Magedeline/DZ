namespace MaggyHelper.Entities;

[CustomEntity(ids: "MaggyHelper/SampleSolid")]
public class SampleSolid : Solid
{
    public SampleSolid(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, true)
    {
        // TODO: read properties from data
    }
}



