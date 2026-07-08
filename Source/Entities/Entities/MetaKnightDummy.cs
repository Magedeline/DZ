namespace Celeste.Entities;

[CustomEntity(ids: "DZ/MetaKnightDummy")]
[Tracked]
[HotReloadable]
public sealed class MetaKnightDummy : PassiveFollowerDummy
{
    private const float DefaultFollowDelay = 0.72f;
    private const float DefaultFollowSpeed = 340f;

    public MetaKnightDummy(EntityData data, Vector2 offset)
        : base(data, offset, "MetaKnightDummy", "meta_knight_npc", DefaultFollowDelay, DefaultFollowSpeed)
    {
    }

    public MetaKnightDummy(Vector2 position, string animation = "idle", bool autoFollow = true)
        : base(position, "MetaKnightDummy", "meta_knight_npc", animation, 1, 1f, 1f, true, true, autoFollow, DefaultFollowDelay, DefaultFollowSpeed)
    {
    }
}