#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/ChangeRespawnTrigger")]
public class ChangeRespawnTrigger : Trigger {
    private Vector2 target;

    public ChangeRespawnTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        target = data.NodesOffset(offset).Length > 0 ? data.NodesOffset(offset)[0] : data.Position + offset;
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        Level level = SceneAs<Level>();
        Vector2 point = target + new Vector2(0f, -4f);
        if (Scene.CollideCheck<Solid>(point))
            return;
        level.Session.HitCheckpoint = true;
        level.Session.RespawnPoint = target;
        level.Session.UpdateLevelStartDashes();
    }
}
