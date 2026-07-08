#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/CameraAdvanceTargetTrigger")]
public class CameraAdvanceTargetTrigger : Trigger {
    private Vector2 target;
    private Vector2 lerpStrength;
    private PositionModes positionModeX;
    private PositionModes positionModeY;
    private bool xOnly;
    private bool yOnly;

    public CameraAdvanceTargetTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        target = new Vector2(data.Float("targetX", 0f), data.Float("targetY", 0f));
        lerpStrength = new Vector2(data.Float("lerpStrengthX", 1f), data.Float("lerpStrengthY", 1f));
        positionModeX = data.Enum("positionModeX", PositionModes.NoEffect);
        positionModeY = data.Enum("positionModeY", PositionModes.NoEffect);
        xOnly = data.Bool("xOnly", false);
        yOnly = data.Bool("yOnly", false);
    }

    public override void OnStay(Player player) {
        base.OnStay(player);
        player.CameraAnchor = target;
        player.CameraAnchorLerp.X = Calc.Clamp(lerpStrength.X * GetPositionLerp(player, positionModeX), 0f, 1f);
        player.CameraAnchorLerp.Y = Calc.Clamp(lerpStrength.Y * GetPositionLerp(player, positionModeY), 0f, 1f);
        player.CameraAnchorIgnoreX = yOnly;
        player.CameraAnchorIgnoreY = xOnly;
    }

    public override void OnLeave(Player player) {
        base.OnLeave(player);
        bool insideOther = false;
        foreach (var t in Scene.Tracker.GetEntities<CameraTargetTrigger>()) {
            if (((CameraTargetTrigger)t).PlayerIsInside) { insideOther = true; break; }
        }
        if (!insideOther) {
            foreach (var t in Scene.Tracker.GetEntities<CameraAdvanceTargetTrigger>()) {
                if (t != this && ((CameraAdvanceTargetTrigger)t).PlayerIsInside) { insideOther = true; break; }
            }
        }
        if (!insideOther)
            player.CameraAnchorLerp = Vector2.Zero;
    }
}
