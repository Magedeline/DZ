#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/RespawnTargetTrigger")]
public class RespawnTargetTrigger : Trigger {
    public Vector2 Target;

    public RespawnTargetTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        Target = data.NodesOffset(offset).Length > 0 ? data.NodesOffset(offset)[0] : data.Position + offset;
    }
}
