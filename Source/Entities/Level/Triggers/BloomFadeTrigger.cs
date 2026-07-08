#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/BloomFadeTrigger")]
public class BloomFadeTrigger : Trigger {
    private float bloomAddFrom;
    private float bloomAddTo;
    private PositionModes positionMode;

    public BloomFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        bloomAddFrom = data.Float("bloomAddFrom", 0f);
        bloomAddTo = data.Float("bloomAddTo", 0f);
        positionMode = data.Enum("positionMode", PositionModes.NoEffect);
    }

    public override void OnStay(Player player) {
        base.OnStay(player);
        Level level = SceneAs<Level>();
        float lerp = GetPositionLerp(player, positionMode);
        level.Session.BloomBaseAdd = bloomAddFrom + (bloomAddTo - bloomAddFrom) * Calc.Clamp(lerp, 0f, 1f);
    }
}
