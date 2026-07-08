#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/WindAttackTrigger")]
public class WindAttackTrigger : Trigger {
    public WindAttackTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if (Scene.Tracker.GetEntity<Snowball>() == null)
            Scene.Add(new Snowball());
        RemoveSelf();
    }
}
