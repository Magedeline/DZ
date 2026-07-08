#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/RumbleTrigger")]
public class RumbleTrigger : Trigger {
    private bool manualTrigger;
    private bool started;
    private bool persistent;
    private string id;
    private float rumble;
    private float left;
    private float right;
    private List<Entity> decals = new();
    private List<CrumbleWallOnRumble> crumbles = new();

    public RumbleTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        manualTrigger = data.Bool("manualTrigger", false);
        persistent = data.Bool("persistent", false);
        id = data.Attr("id", "");
        var nodes = data.NodesOffset(offset);
        if (nodes != null && nodes.Length >= 2) {
            left = MathHelper.Min(nodes[0].X, nodes[1].X);
            right = MathHelper.Max(nodes[0].X, nodes[1].X);
        }
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        Level level = SceneAs<Level>();
        bool flag = persistent && level.Session.GetFlag(id);
        foreach (var crumble in Scene.Tracker.GetEntities<CrumbleWallOnRumble>()) {
            var c = (CrumbleWallOnRumble)crumble;
            if (c.X >= left && c.X <= right) {
                if (flag)
                    c.RemoveSelf();
                else
                    crumbles.Add(c);
            }
        }
        if (flag) {
            RemoveSelf();
            return;
        }
        crumbles = crumbles.OrderBy(_ => Calc.Random.Next()).ToList();
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if (manualTrigger) return;
        Invoke(0f);
    }

    private void Invoke(float delay) {
        if (started) return;
        started = true;
        Level level = SceneAs<Level>();
        if (persistent)
            level.Session.SetFlag(id);
        Add(new Coroutine(RumbleRoutine(delay)));
    }

    private IEnumerator RumbleRoutine(float delay) {
        yield return delay;
        rumble = 1f;
        Audio.Play("event:/new_content/game/10_farewell/quake_onset");
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        foreach (var crumble in crumbles) {
            crumble.Break();
            yield return 0.05f;
        }
    }

    public override void Update() {
        base.Update();
        rumble = Calc.Approach(rumble, 0f, Engine.DeltaTime * 0.7f);
    }

    public static void ManuallyTrigger(float x, float delay) {
        foreach (var trigger in Engine.Scene.Tracker.GetEntities<RumbleTrigger>()) {
            var t = (RumbleTrigger)trigger;
            if (t.manualTrigger && x >= t.left && x <= t.right)
                t.Invoke(delay);
        }
    }
}
