#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/NoRefillTrigger")]
public class NoRefillTrigger : Trigger {
    private bool state;

    public NoRefillTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        state = data.Bool("state", true);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        Level level = SceneAs<Level>();
        level.Session.Inventory.NoRefills = state;
    }
}
