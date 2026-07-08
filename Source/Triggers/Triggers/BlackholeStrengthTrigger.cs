using Monocle;
using Celeste.Entities;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that controls the strength of blackhole effects in the level
    /// </summary>
    [CustomEntity(ids: "DZ/BlackholeStrengthTrigger")]
    public class BlackholeStrengthTrigger : Trigger
    {
        public enum BlackholeStrength
        {
            None,
            Weak,
            Medium,
            Strong,
            Wild,
            Maximum
        }

        public BlackholeStrength Strength { get; private set; }

        public BlackholeStrengthTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            string strengthStr = data.Attr("strength", "Medium");
            Strength = Enum.TryParse<BlackholeStrength>(strengthStr, true, out var parsed) 
                ? parsed : BlackholeStrength.Medium;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            var level = SceneAs<Level>();
            if (level != null)
            {
                // Map our strength enum to RainbowBlackholeBg.Strengths
                RainbowBlackholeBg.Strengths backdropStrength = Strength switch
                {
                    BlackholeStrength.None => RainbowBlackholeBg.Strengths.Mild,
                    BlackholeStrength.Weak => RainbowBlackholeBg.Strengths.Mild,
                    BlackholeStrength.Medium => RainbowBlackholeBg.Strengths.Medium,
                    BlackholeStrength.Strong => RainbowBlackholeBg.Strengths.High,
                    BlackholeStrength.Wild => RainbowBlackholeBg.Strengths.Wild,
                    BlackholeStrength.Maximum => RainbowBlackholeBg.Strengths.Insane,
                    _ => RainbowBlackholeBg.Strengths.Medium
                };

                // Update any RainbowBlackholeBg in the level
                foreach (var backdrop in level.Background.Backdrops)
                {
                    if (backdrop is RainbowBlackholeBg blackholeBg)
                    {
                        blackholeBg.SetStrength(backdropStrength);
                    }
                }

                // Also check foreground backdrops
                foreach (var backdrop in level.Foreground.Backdrops)
                {
                    if (backdrop is RainbowBlackholeBg blackholeBg)
                    {
                        blackholeBg.SetStrength(backdropStrength);
                    }
                }

                // Set session data for other systems to read
                level.Session.SetFlag($"blackhole_strength_{Strength.ToString().ToLower()}", true);
            }
        }
    }
}
