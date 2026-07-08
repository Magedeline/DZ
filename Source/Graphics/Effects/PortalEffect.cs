using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Portal backdrop effect that renders a swirling portal visual.
    /// </summary>
    [CustomBackdrop("DZ/PortalEffect")]
    [HotReloadable]
    public class PortalEffect : Backdrop
    {
        private string destination;
        private bool active;
        private float time;
        private float visibleFade;

        public PortalEffect()
        {
            destination = "";
            active = true;
            time = 0f;
            visibleFade = 0f;
        }

        public PortalEffect(BinaryPacker.Element data) : this()
        {
            destination = data.Attr("destination", "");
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
            float alpha = visibleFade * 0.2f;
            if (alpha <= 0f)
                return;

            float pulse = 0.6f + 0.4f * (float)System.Math.Sin(time * 2f);
            Color portalColor = Color.Lerp(Color.DarkMagenta, Color.Cyan, pulse) * alpha;

            Draw.Rect(0f, 0f, 320f, 180f, portalColor);
        }
    }
}
