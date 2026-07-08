namespace Celeste.Entities;

[CustomEntity(ids: "DZ/GooeyDummy")]
[Tracked]
[HotReloadable]
public sealed class GooeyDummy : PassiveFollowerDummy
{
    private const float DefaultFollowDelay = 0.68f;
    private const float DefaultFollowSpeed = 300f;

    public GooeyDummy(EntityData data, Vector2 offset)
        : base(data, offset, "GooeyDummy", "gooey", DefaultFollowDelay, DefaultFollowSpeed)
    {
    }

    public GooeyDummy(Vector2 position, string animation = "idle", bool autoFollow = true)
        : base(position, "GooeyDummy", "gooey", animation, 1, 1f, 1f, true, true, autoFollow, DefaultFollowDelay, DefaultFollowSpeed)
    {
    }
}