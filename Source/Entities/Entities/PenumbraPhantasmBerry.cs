#nullable enable

using CelesteEntities = global::Celeste.Mod.Entities;

namespace Celeste.Entities
{
    [CelesteEntities.CustomEntity("DZ/PenumbraPhantasmBerry")]
    [Monocle.Tracked]
    [CelesteEntities.RegisterStrawberry(tracked: true, blocksCollection: true)]
    [TrackedAs(typeof(CelesteStrawberry))]
    internal class PenumbraPhantasmBerry : PinkPlatinumBerry
    {
        protected override string SpriteName => "strawberry";

        public PenumbraPhantasmBerry(EntityData data, Vector2 offset, EntityID gid)
            : base(data, offset, gid)
        {
            this.sprite.Color = Calc.HexToColor("4B0082"); // deep indigo
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.sprite.Color = Calc.HexToColor("4B0082");
        }

        protected override void RecordCollection(Level level, string berryId)
        {
            DZProgressionManager.RecordPinkPlatinumBerry(level, berryId);
        }
    }
}
