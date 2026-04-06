namespace MaggyHelper.Entities;

[CustomEntity(ids: "MaggyHelper/KirbyDummy")]
[Tracked]
[HotReloadable]
public sealed class KirbyDummy : PassiveFollowerDummy
{
    private const float DefaultFollowDelay = 0.3f;
    private const float DefaultFollowSpeed = 360f;

    public KirbyDummy(EntityData data, Vector2 offset)
        : base(data, offset, "KirbyDummy", "kirby", DefaultFollowDelay, DefaultFollowSpeed)
    {
    }

    public KirbyDummy(Vector2 position, string animation = "idle", bool autoFollow = true)
        : base(position, "KirbyDummy", "kirby", animation, 1, 1f, 1f, true, true, autoFollow, DefaultFollowDelay, DefaultFollowSpeed)
    {
    }
}