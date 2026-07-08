using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Time distortion backdrop effect that creates a warping visual around a radius.
    /// </summary>
    [CustomBackdrop("DZ/TimeDistortionEffect")]
    [HotReloadable]
    public class TimeDistortionEffect : Backdrop
    {
        private float multiplier;
        private float radius;
        private float time;
        private float visibleFade;

        public TimeDistortionEffect()
        {
            multiplier = 0.5f;
            radius = 200f;
            time = 0f;
            visibleFade = 0f;
        }

        public TimeDistortionEffect(BinaryPacker.Element data) : this()
        {
            multiplier = data.AttrFloat("multiplier", 0.5f);
            radius = data.AttrFloat("radius", 200f);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update(Scene scene)
        {
            base.Update(scene);

            Level level = scene as Level;
            visibleFade = Calc.Approach(visibleFade, IsVisible(level) ? 1f : 0f, Engine.DeltaTime * 2f);
            time += Engine.DeltaTime * multiplier;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render(Scene scene)
        {
            float alpha = visibleFade * 0.3f;
            if (alpha <= 0f)
                return;

            float pulse = 0.5f + 0.5f * (float)System.Math.Sin(time * 3f);
            Color distortColor = Color.Lerp(Color.DarkBlue, Color.Purple, pulse) * alpha;

            Draw.Rect(0f, 0f, 320f, 180f, distortColor);
        }
    }
}
