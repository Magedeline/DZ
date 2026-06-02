using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Celeste;
    [CustomEntity("MaggyHelper/RainbowBlackholeStrengthTrigger")]
    [HotReloadable]
public class RainbowBlackholeStrengthTrigger : Trigger
{
    private RainbowBlackholeBG.Strengths strength;

    private bool rainbowMode;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public RainbowBlackholeStrengthTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        strength = data.Enum("strength", RainbowBlackholeBG.Strengths.Mild);
        rainbowMode = data.Bool("rainbowMode", false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        RainbowBlackholeBG bg = (base.Scene as Level).Background.Get<RainbowBlackholeBG>();
        if (bg != null)
        {
            bg.RainbowMode = rainbowMode;
            bg.NextStrength(base.Scene as Level, strength);
        }
    }
}
