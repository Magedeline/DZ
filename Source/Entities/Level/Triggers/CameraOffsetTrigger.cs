#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/CameraOffsetTrigger")]
public class CameraOffsetTrigger : Trigger {
    private Vector2 cameraOffset;

    public CameraOffsetTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        cameraOffset = new Vector2(data.Float("cameraOffsetX", 0f), data.Float("cameraOffsetY", 0f));
        cameraOffset.X *= 48f;
        cameraOffset.Y *= 32f;
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        Level level = SceneAs<Level>();
        level.CameraOffset = cameraOffset;
    }
}
