using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Kirby transform backdrop effect that renders a transformation flash.
    /// </summary>
    [CustomBackdrop("DZ/KirbyTransformEffect")]
    [HotReloadable]
    public class KirbyTransformEffect : Backdrop
    {
        private string transformType;
        private float duration;
        private float time;
        private float visibleFade;

        public KirbyTransformEffect()
        {
            transformType = "hypergod";
            duration = 2f;
            time = 0f;
            visibleFade = 0f;
        }

        public KirbyTransformEffect(BinaryPacker.Element data) : this()
        {
            transformType = data.Attr("transformType", "hypergod");
            duration = data.AttrFloat("duration", 2f);
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
            float alpha = visibleFade * 0.15f;
            if (alpha <= 0f)
                return;

            float flash = (float)System.Math.Max(0f, 1f - (time % duration) / duration);
            Color transformColor = Color.White * (alpha * flash);

            Draw.Rect(0f, 0f, 320f, 180f, transformColor);
        }
    }
}
