#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/SpawnFacingTrigger")]
public class SpawnFacingTrigger : Trigger {
    public Facings Facing;

    public SpawnFacingTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        Facing = data.Enum("facing", Facings.Right);
    }
}
