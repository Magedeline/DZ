#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/AmbienceParamTrigger")]
public class AmbienceParamTrigger : Trigger {
    private string parameter;
    private float from;
    private float to;
    private PositionModes positionMode;

    public AmbienceParamTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        parameter = data.Attr("parameter", "");
        from = data.Float("from", 0f);
        to = data.Float("to", 1f);
        positionMode = data.Enum("positionMode", PositionModes.NoEffect);
    }

    public override void OnStay(Player player) {
        base.OnStay(player);
        Level level = SceneAs<Level>();
        float lerp = GetPositionLerp(player, positionMode);
        float value = from + (to - from) * Calc.Clamp(lerp, 0f, 1f);
        level.Session.Audio.Apply();
    }
}
