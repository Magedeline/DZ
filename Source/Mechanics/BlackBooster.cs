namespace Celeste.Entities
{
    [CustomEntity("DesoloZatnas/BlackBooster")]
    [Tracked]
    public class BlackBooster : GreyBooster
    {
        public BlackBooster(EntityData data, Vector2 offset)
            : base(data.Position + offset, true)
        {
        }
    }

    [CustomEntity("DesoloZatnas/GasterBooster")]
    [Tracked]
    public class GasterBooster : BlackBooster
    {
        public GasterBooster(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }
    }
}
