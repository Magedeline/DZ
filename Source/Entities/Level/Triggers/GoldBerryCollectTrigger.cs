#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/GoldBerryCollectTrigger")]
public class GoldBerryCollectTrigger : Trigger {
    public GoldBerryCollectTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }
}
