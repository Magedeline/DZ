using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Celeste;
    [CustomEntity("MaggyHelper/RainbowBlackholeStrengthTrigger")]
    [HotReloadable]
public class RainbowBlackholeStrengthTrigger : Trigger
{
    private RainbowBlackholeBg.Strengths strength;

    private bool rainbowMode;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public RainbowBlackholeStrengthTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        strength = data.Enum("strength", RainbowBlackholeBg.Strengths.Mild);
        rainbowMode = data.Bool("rainbowMode", false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        RainbowBlackholeBg bg = (base.Scene as Level).Background.Get<RainbowBlackholeBg>();
        if (bg != null)
        {
            bg.RainbowMode = rainbowMode;
            bg.NextStrength(base.Scene as Level, strength);
        }
    }
}
