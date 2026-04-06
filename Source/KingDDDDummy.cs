namespace MaggyHelper.Entities;

[CustomEntity(ids: "MaggyHelper/KingDDDDummy")]
[Tracked]
[HotReloadable]
public sealed class KingDDDDummy : PassiveFollowerDummy
{
    private const float DefaultFollowDelay = 0.95f;
    private const float DefaultFollowSpeed = 260f;

    public KingDDDDummy(EntityData data, Vector2 offset)
        : base(data, offset, "KingDDDDummy", "king_dedede", DefaultFollowDelay, DefaultFollowSpeed)
    {
    }

    public KingDDDDummy(Vector2 position, string animation = "idle", bool autoFollow = true)
        : base(position, "KingDDDDummy", "king_dedede", animation, 1, 1f, 1f, true, true, autoFollow, DefaultFollowDelay, DefaultFollowSpeed)
    {
    }
}