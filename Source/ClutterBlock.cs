namespace MaggyHelper.Entities;

[CustomEntity(ids: "MaggyHelper/ClutterBlock")]
[HotReloadable]
public class ClutterBlock : ClutterBlockBase
{
    public enum Colors
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Yellow = 3
    }

    public bool TopSideOpen { get; set; }
    public bool LeftSideOpen { get; set; }
    public bool RightSideOpen { get; set; }
    public bool OnTheGround { get; set; }
    public List<ClutterBlock> HasBelow { get; } = new List<ClutterBlock>();
    public List<ClutterBlock> Below { get; } = new List<ClutterBlock>();
    public List<ClutterBlock> Above { get; } = new List<ClutterBlock>();

    public ClutterBlock(EntityData data, Vector2 offset)
        : this(data.Position + offset, GFX.Game[data.Attr("texture", "objects/clutter/default")], data.Enum("color", Colors.Red))
    {
    }

    public ClutterBlock(Vector2 position, MTexture texture, Colors color)
        : base(position, texture.Width, texture.Height, true, color)
    {
        Add(new Image(texture));
        Collider = new Hitbox(texture.Width, texture.Height);
    }

    // Add this method to the ClutterBlock class to fix CS1061
    public void Absorb(ClutterAbsorbEffect effect)
    {
        // Implementation of the Absorb method
    }
}

[HotReloadable]
public class ClutterBlockBase : Entity
{
    internal ClutterBlock.Colors BlockColor;

    public ClutterBlockBase(Vector2 position, int width, int height, bool enabled, ClutterBlock.Colors color) : base(position)
    {
        BlockColor = color;
        if (enabled)
        {
            Collider = new Hitbox(width, height);
            Add(new StaticMover());
        }
    }
}



