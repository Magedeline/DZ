namespace MaggyHelper.Entities;

[CustomEntity(ids: "MaggyHelper/MagolorDummy")]
[Tracked]
[HotReloadable]
public sealed class MagolorDummy : PassiveFollowerDummy
{
    private const float DefaultFollowDelay = 0.55f;
    private const float DefaultFollowSpeed = 320f;

    public MagolorDummy(EntityData data, Vector2 offset)
        : base(data, offset, "MagolorDummy", "magolor_npc", DefaultFollowDelay, DefaultFollowSpeed)
    {
    }

    public MagolorDummy(Vector2 position, string animation = "idle", bool autoFollow = true)
        : base(position, "MagolorDummy", "magolor_npc", animation, 1, 1f, 1f, true, true, autoFollow, DefaultFollowDelay, DefaultFollowSpeed)
    {
    }
}