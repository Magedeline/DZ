#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/WindTrigger")]
public class WindTrigger : Trigger {
    private WindController.Patterns pattern;

    public WindTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        pattern = data.Enum("pattern", WindController.Patterns.None);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        WindController? controller = Scene.Tracker.GetEntity<WindController>();
        if (controller == null)
            Scene.Add(new WindController(pattern));
        else
            controller.SetPattern(pattern);
    }
}
