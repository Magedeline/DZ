#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/MiniTextboxTrigger")]
public class MiniTextboxTrigger : Trigger {
    private string[] dialogOptions;
    private Modes mode;
    private bool triggered;
    private bool onlyOnce;
    private int deathCount;
    private EntityID entityID;

    public enum Modes { OnPlayerEnter, OnLevelStart, OnTheoEnter }

    public MiniTextboxTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        mode = data.Enum("mode", Modes.OnPlayerEnter);
        dialogOptions = data.Attr("dialogId", "").Split(',');
        onlyOnce = data.Bool("onlyOnce", false);
        deathCount = data.Int("deathCount", -1);
        entityID = new EntityID(data.Name, data.ID);
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        if (mode == Modes.OnLevelStart)
            Trigger();
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        if (mode != Modes.OnPlayerEnter) return;
        Trigger();
    }

    private void Trigger() {
        if (triggered) return;
        Level level = SceneAs<Level>();
        if (deathCount >= 0 && level.Session.DeathsInCurrentLevel != deathCount)
            return;
        triggered = true;
        if (dialogOptions.Length > 0)
            Scene.Add(new MiniTextbox(Calc.Random.Choose(dialogOptions)));
        if (onlyOnce)
            level.Session.DoNotLoad.Add(entityID);
    }
}
