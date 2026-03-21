namespace MaggyHelper.Entities
{
    // Note: Abstract class - cannot have [CustomEntity] attribute as Everest cannot instantiate abstract classes
    [Tracked(true)]
    [HotReloadable]
    public abstract class CutsceneNode : Entity
    {
        private readonly string name;

        protected CutsceneNode(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            name = data.Attr("player_skip");
        }

        public static CutsceneNode Find(string name)
        {
            foreach (var entity1 in Engine.Scene.Tracker.GetEntities<CutsceneNode>())
            {
                var entity = (CutsceneNode)entity1;
                if (entity.name != null && entity.name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return entity;
            }
            return null;
        }
    }
}



