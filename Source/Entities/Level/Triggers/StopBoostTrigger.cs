#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/StopBoostTrigger")]
public class StopBoostTrigger : Trigger {
    public StopBoostTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if (player.StateMachine.State != Player.StSummitLaunch)
            return;
        player.StopSummitLaunch();
    }
}
