using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Soul mode backdrop effect that renders a soul-like overlay.
    /// </summary>
    [CustomBackdrop("DZ/SoulModeEffect")]
    [HotReloadable]
    public class SoulModeEffect : Backdrop
    {
        private bool active;
        private float intensity;
        private float time;
        private float visibleFade;

        public SoulModeEffect()
        {
            active = true;
            intensity = 1f;
            time = 0f;
            visibleFade = 0f;
        }

        public SoulModeEffect(BinaryPacker.Element data) : this()
        {
            active = data.AttrBool("active", true);
            intensity = data.AttrFloat("intensity", 1f);
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
            float alpha = visibleFade * intensity * 0.2f;
            if (alpha <= 0f)
                return;

            float pulse = 0.7f + 0.3f * (float)System.Math.Sin(time * 1.5f);
            Color soulColor = Color.Lerp(Color.Black, Color.DarkRed, 0.3f) * (alpha * pulse);

            Draw.Rect(0f, 0f, 320f, 180f, soulColor);
        }
    }
}
