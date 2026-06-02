namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that controls the strength of blackhole effects in the level
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/BlackholeStrengthTrigger")]
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
                // Map our strength enum to RainbowBlackholeBG.Strengths
                RainbowBlackholeBG.Strengths backdropStrength = Strength switch
                {
                    BlackholeStrength.None => RainbowBlackholeBG.Strengths.Mild,
                    BlackholeStrength.Weak => RainbowBlackholeBG.Strengths.Mild,
                    BlackholeStrength.Medium => RainbowBlackholeBG.Strengths.Medium,
                    BlackholeStrength.Strong => RainbowBlackholeBG.Strengths.High,
                    BlackholeStrength.Wild => RainbowBlackholeBG.Strengths.Wild,
                    BlackholeStrength.Maximum => RainbowBlackholeBG.Strengths.Insane,
                    _ => RainbowBlackholeBG.Strengths.Medium
                };

                // Update any RainbowBlackholeBG in the level
                foreach (var backdrop in level.Background.Backdrops)
                {
                    if (backdrop is RainbowBlackholeBG blackholeBg)
                    {
                        blackholeBg.SetStrength(backdropStrength);
                    }
                }

                // Also check foreground backdrops
                foreach (var backdrop in level.Foreground.Backdrops)
                {
                    if (backdrop is RainbowBlackholeBG blackholeBg)
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
