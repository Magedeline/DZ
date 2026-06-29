using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Rainbow color overlay backdrop effect.
    /// Renders a full-screen rainbow color wash that cycles through hues.
    /// </summary>
    [CustomBackdrop("DZ/RainbowEffect")]
    [HotReloadable]
    public class RainbowEffect : Backdrop
    {
        private float speed;
        private float intensity;
        private float hueTime;
        private float visibleFade;

        public RainbowEffect()
        {
            speed = 1f;
            intensity = 1f;
            hueTime = 0f;
            visibleFade = 0f;
        }

        public RainbowEffect(BinaryPacker.Element data) : this()
        {
            speed = data.AttrFloat("speed", 1f);
            intensity = MathHelper.Clamp(data.AttrFloat("intensity", 1f), 0f, 1f);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update(Scene scene)
        {
            base.Update(scene);

            Level level = scene as Level;
            visibleFade = Calc.Approach(visibleFade, IsVisible(level) ? 1f : 0f, Engine.DeltaTime * 2f);
            hueTime += Engine.DeltaTime * speed;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render(Scene scene)
        {
            float alpha = visibleFade * intensity;
            if (alpha <= 0f)
                return;

            float hue = (hueTime % 6f) / 6f;
            Color rainbowColor = Calc.HsvToColor(hue, 0.6f, 1f) * (alpha * 0.3f);

            Draw.Rect(0f, 0f, 320f, 180f, rainbowColor);
        }
    }
}
