using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Boss aura backdrop effect that renders a colored aura overlay.
    /// </summary>
    [CustomBackdrop("DZ/BossAuraEffect")]
    [HotReloadable]
    public class BossAuraEffect : Backdrop
    {
        private Color auraColor;
        private float intensity;
        private float time;
        private float visibleFade;

        public BossAuraEffect()
        {
            auraColor = Color.Red;
            intensity = 1f;
            time = 0f;
            visibleFade = 0f;
        }

        public BossAuraEffect(BinaryPacker.Element data) : this()
        {
            string colorName = data.Attr("auraColor", "red");
            auraColor = colorName.ToLower() switch
            {
                "red" => Color.Red,
                "blue" => Color.Blue,
                "purple" => Color.Purple,
                "green" => Color.Green,
                "yellow" => Color.Yellow,
                "orange" => Color.Orange,
                "white" => Color.White,
                "black" => Color.Black,
                _ => Calc.HexToColor(colorName)
            };
            intensity = data.AttrFloat("intensity", 1f);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update(Scene scene)
        {
            base.Update(scene);

            Level level = scene as Level;
            visibleFade = Calc.Approach(visibleFade, IsVisible(level) ? 1f : 0f, Engine.DeltaTime * 2f);
            time += Engine.DeltaTime;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render(Scene scene)
        {
            float alpha = visibleFade * intensity * 0.25f;
            if (alpha <= 0f)
                return;

            float pulse = 0.8f + 0.2f * (float)System.Math.Sin(time * 2f);
            Color renderColor = auraColor * (alpha * pulse);

            Draw.Rect(0f, 0f, 320f, 180f, renderColor);
        }
    }
}
