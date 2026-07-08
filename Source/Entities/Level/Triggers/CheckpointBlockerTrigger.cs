#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/CheckpointBlockerTrigger")]
public class CheckpointBlockerTrigger : Trigger {
    public CheckpointBlockerTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }
}
