#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/LightFadeTrigger")]
public class LightFadeTrigger : Trigger {
    private float lightAddFrom;
    private float lightAddTo;
    private PositionModes positionMode;

    public LightFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        lightAddFrom = data.Float("lightAddFrom", 0f);
        lightAddTo = data.Float("lightAddTo", 0f);
        positionMode = data.Enum("positionMode", PositionModes.NoEffect);
    }

    public override void OnStay(Player player) {
        base.OnStay(player);
        Level level = SceneAs<Level>();
        float lerp = GetPositionLerp(player, positionMode);
        level.Session.LightingAlphaAdd = lightAddFrom + (lightAddTo - lightAddFrom) * Calc.Clamp(lerp, 0f, 1f);
    }
}
