using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Hypergod backdrop effect that renders an intense power-level-based visual.
    /// </summary>
    [CustomBackdrop("DZ/HypergodEffect")]
    [HotReloadable]
    public class HypergodEffect : Backdrop
    {
        private int powerLevel;
        private bool active;
        private float time;
        private float visibleFade;

        public HypergodEffect()
        {
            powerLevel = 100;
            active = true;
            time = 0f;
            visibleFade = 0f;
        }

        public HypergodEffect(BinaryPacker.Element data) : this()
        {
            powerLevel = data.AttrInt("powerLevel", 100);
            active = data.AttrBool("active", true);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update(Scene scene)
        {
            base.Update(scene);

            Level level = scene as Level;
            visibleFade = Calc.Approach(visibleFade, (IsVisible(level) && active) ? 1f : 0f, Engine.DeltaTime * 2f);
            time += Engine.DeltaTime;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render(Scene scene)
        {
            float alpha = visibleFade * (powerLevel / 100f) * 0.2f;
            if (alpha <= 0f)
                return;

            float hue = (time * 0.5f) % 1f;
            Color godColor = Calc.HsvToColor(hue, 0.5f, 1f) * alpha;

            Draw.Rect(0f, 0f, 320f, 180f, godColor);
        }
    }
}
