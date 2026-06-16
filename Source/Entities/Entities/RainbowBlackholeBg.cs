using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Rainbow black hole backdrop effect.
    /// A visual effect that creates a rainbow-colored black hole in the background.
    /// </summary>
    [CustomEntity("DZ/RainbowBlackholeBg")]
    [HotReloadable]
    public class RainbowBlackholeBg : Backdrop
    {
        public enum Strengths
        {
            Weak,
            Mild,
            Medium,
            High,
            Strong,
            Wild,
            Maximum,
            Insane,
            RainbowChaos,
            Cosmic
        }

        public float Alpha { get; set; } = 1f;
        public float Strength { get; set; } = 1f;
        public Strengths RainbowMode { get; set; } = Strengths.Medium;
        public Strengths NextStrengthValue { get; set; } = Strengths.Medium;
        public float Scale { get; set; } = 1f;
        public Vector2 Direction { get; set; } = Vector2.Zero;
        
        public void NextStrength(Level level, Strengths strength)
        {
            NextStrengthValue = strength;
        }

        public void SetGeneratorBreakProgress(float progress)
        {
            // Stub implementation
        }
        
        private Vector2 position;
        private float rotation;

        public RainbowBlackholeBg()
        {
            position = Vector2.Zero;
            rotation = 0f;
        }

        public RainbowBlackholeBg(EntityData data, Vector2 offset)
        {
            position = data.Position + offset;
            Alpha = data.Float("alpha", 1f);
            Strength = data.Float("strength", 1f);
        }

        public void SetStrength(Strengths strength)
        {
            RainbowMode = strength;
            Strength = strength switch
            {
                Strengths.Weak => 0.25f,
                Strengths.Mild => 0.35f,
                Strengths.Medium => 0.5f,
                Strengths.High => 0.65f,
                Strengths.Strong => 0.75f,
                Strengths.Wild => 0.85f,
                Strengths.Maximum => 1f,
                Strengths.Insane => 1.5f,
                Strengths.RainbowChaos => 2f,
                Strengths.Cosmic => 2.5f,
                _ => 0.5f
            };
        }

        public override void Update(Scene scene)
        {
            rotation += 0.01f * Engine.DeltaTime;
        }

        public override void Render(Scene scene)
        {
            if (!Visible)
                return;

            // Simple rendering - in a real implementation this would use actual graphics
            // For now, this is a stub to resolve the build error
        }
    }
}
