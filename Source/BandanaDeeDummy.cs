namespace MaggyHelper.Entities;

[CustomEntity(ids: "MaggyHelper/BandanaDeeDummy")]
[Tracked]
[HotReloadable]
public sealed class BandanaDeeDummy : PassiveFollowerDummy
{
    private const float DefaultFollowDelay = 0.42f;
    private const float DefaultFollowSpeed = 340f;

    public BandanaDeeDummy(EntityData data, Vector2 offset)
        : base(data, offset, "BandanaDeeDummy", "bandana_dee", DefaultFollowDelay, DefaultFollowSpeed)
    {
    }

    public BandanaDeeDummy(Vector2 position, string animation = "idle", bool autoFollow = true)
        : base(position, "BandanaDeeDummy", "bandana_dee", animation, 1, 1f, 1f, true, true, autoFollow, DefaultFollowDelay, DefaultFollowSpeed)
    {
    }
}