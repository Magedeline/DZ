using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    /// <summary>
    /// Gondola Mod Darkness - A darkness effect for the gondola cutscene.
    /// Controls the darkness overlay during the gondola sequence.
    /// </summary>
    [CustomEntity("DZ/GondolaModDarkness")]
    [Tracked]
    [HotReloadable]
    public class GondolaModDarkness : Entity
    {
        public float Alpha { get; set; } = 0f;
        public float TargetAlpha { get; set; } = 0f;
        public float FadeSpeed { get; set; } = 0.5f;

        public GondolaModDarkness(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Alpha = data.Float("alpha", 0f);
            TargetAlpha = data.Float("targetAlpha", 0f);
            FadeSpeed = data.Float("fadeSpeed", 0.5f);
        }

        public GondolaModDarkness(Vector2 position)
            : base(position)
        {
            Alpha = 0f;
            TargetAlpha = 0f;
            FadeSpeed = 0.5f;
        }

        public override void Update()
        {
            base.Update();

            if (Alpha < TargetAlpha)
            {
                Alpha = MathHelper.Min(Alpha + FadeSpeed * Engine.DeltaTime, TargetAlpha);
            }
            else if (Alpha > TargetAlpha)
            {
                Alpha = MathHelper.Max(Alpha - FadeSpeed * Engine.DeltaTime, TargetAlpha);
            }
        }

        public override void Render()
        {
            if (Alpha <= 0f)
                return;

            // Simple darkness overlay
            if (Scene is Level level)
            {
                Draw.Rect(level.Camera.Position.X, level.Camera.Position.Y, 1920, 1080, Color.Black * Alpha);
            }
        }
    }
}
