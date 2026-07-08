#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/BirdPathTrigger")]
public class BirdPathTrigger : Trigger {
    private BirdPath? bird;
    private bool triggered;

    public BirdPathTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        bird = Scene.Tracker.GetEntity<BirdPath>();
        if (bird == null)
            RemoveSelf();
        else
            bird.WaitForTrigger();
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if (triggered) return;
        bird?.Trigger();
        triggered = true;
    }
}
