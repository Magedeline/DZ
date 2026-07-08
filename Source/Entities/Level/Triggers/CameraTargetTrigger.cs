#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/CameraTargetTrigger")]
public class CameraTargetTrigger : Trigger {
    private Vector2 target;
    private float lerpStrength;
    private PositionModes positionMode;
    private bool xOnly;
    private bool yOnly;
    private string? deleteFlag;

    public CameraTargetTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        target = new Vector2(data.Float("targetX", 0f), data.Float("targetY", 0f));
        lerpStrength = data.Float("lerpStrength", 1f);
        positionMode = data.Enum("positionMode", PositionModes.NoEffect);
        xOnly = data.Bool("xOnly", false);
        yOnly = data.Bool("yOnly", false);
        deleteFlag = data.Attr("deleteFlag", "");
    }

    public override void OnStay(Player player) {
        base.OnStay(player);
        Level level = SceneAs<Level>();
        if (!string.IsNullOrEmpty(deleteFlag) && level.Session.GetFlag(deleteFlag))
            return;
        player.CameraAnchor = target;
        player.CameraAnchorLerp = Vector2.One * Calc.Clamp(lerpStrength * GetPositionLerp(player, positionMode), 0f, 1f);
        player.CameraAnchorIgnoreX = yOnly;
        player.CameraAnchorIgnoreY = xOnly;
    }

    public override void OnLeave(Player player) {
        base.OnLeave(player);
        bool insideOther = false;
        foreach (var trigger in Scene.Tracker.GetEntities<CameraTargetTrigger>()) {
            if (trigger != this && ((CameraTargetTrigger)trigger).PlayerIsInside) {
                insideOther = true;
                break;
            }
        }
        if (!insideOther)
            player.CameraAnchorLerp = Vector2.Zero;
    }
}
