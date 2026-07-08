#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/MusicFadeTrigger")]
public class MusicFadeTrigger : Trigger {
    private bool leftToRight;
    private float fadeA;
    private float fadeB;
    private string? parameter;

    public MusicFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        leftToRight = data.Bool("leftToRight", false);
        fadeA = data.Float("fadeA", 0f);
        fadeB = data.Float("fadeB", 1f);
        parameter = data.Attr("parameter", "");
    }

    public override void OnStay(Player player) {
        base.OnStay(player);
        Level level = SceneAs<Level>();
        float value;
        if (!leftToRight)
            value = Calc.Clamp((player.Y - Y) / Height, 0f, 1f);
        else
            value = Calc.Clamp((player.X - X) / Width, 0f, 1f);
        value = fadeA + (fadeB - fadeA) * value;
        level.Session.Audio.Apply();
    }
}
