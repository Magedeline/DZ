namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that controls Rainbow Blackhole background effects
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/RainbowBlackholeTrigger")]
    [HotReloadable]
    public class RainbowBlackholeTrigger : Trigger
    {
        public enum ActionType
        {
            Enable,
            Disable,
            ChangeStrength,
            SetAlpha,
            SetScale,
            SetDirection,
            Toggle
        }

        public enum StrengthLevel
        {
            Mild,
            Medium,
            High,
            Wild,
            Insane,
            RainbowChaos,
            Cosmic
        }

        private ActionType action;
        private StrengthLevel strengthLevel;
        private float alphaValue;
        private float scaleValue;
        private float directionValue;
        private bool triggerOnce;
        private bool hasTriggered;
        private float fadeTime;
        private string flag;
        private bool onlyIfFlag;
        private Level level;

        public RainbowBlackholeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            // Parse action type
            string actionName = data.Attr("action", "Enable");
            if (!Enum.TryParse(actionName, true, out action))
            {
                action = ActionType.Enable;
            }

            // Parse strength level
            string strengthName = data.Attr("strength", "Medium");
            if (!Enum.TryParse(strengthName, true, out strengthLevel))
            {
                strengthLevel = StrengthLevel.Medium;
            }

            alphaValue = data.Float("alpha", 1.0f);
            scaleValue = data.Float("scale", 1.0f);
            directionValue = data.Float("direction", 1.0f);
            triggerOnce = data.Bool("triggerOnce", false);
            fadeTime = data.Float("fadeTime", 1.0f);
            flag = data.Attr("flag", "");
            onlyIfFlag = data.Bool("onlyIfFlag", false);
            hasTriggered = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggerOnce && hasTriggered)
                return;

            // Check flag condition if specified
            if (!string.IsNullOrEmpty(flag))
            {
                bool flagValue = (level?.Session?.GetFlag(flag) ?? false);
                if (onlyIfFlag && !flagValue)
                    return;
                if (!onlyIfFlag && flagValue)
                    return;
            }

            TriggerRainbowBlackholeEffect();
            hasTriggered = true;
        }

        private void TriggerRainbowBlackholeEffect()
        {
            if (level == null) return;

            // Find all RainbowBlackholeBg instances in the scene
            foreach (var backdrop in level.Background.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    ApplyActionToBackdrop(rainbowBg);
                }
            }

            foreach (var backdrop in level.Foreground.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    ApplyActionToBackdrop(rainbowBg);
                }
            }

            // Set session flag based on action
            switch (action)
            {
                case ActionType.Enable:
                    level.Session.SetFlag("rainbow_blackhole_enabled", true);
                    break;
                case ActionType.Disable:
                    level.Session.SetFlag("rainbow_blackhole_enabled", false);
                    break;
                case ActionType.ChangeStrength:
                    level.Session.SetFlag($"rainbow_blackhole_strength_{strengthLevel.ToString().ToLower()}", true);
                    break;
            }
        }

        private void ApplyActionToBackdrop(RainbowBlackholeBg backdrop)
        {
            switch (action)
            {
                case ActionType.Enable:
                    backdrop.Visible = true;
                    backdrop.Alpha = alphaValue;
                    break;

                case ActionType.Disable:
                    backdrop.Visible = false;
                    backdrop.Alpha = 0f;
                    break;

                case ActionType.ChangeStrength:
                    // Convert our enum to the backdrop's enum
                    var backdropStrength = (RainbowBlackholeBg.Strengths)Enum.Parse(typeof(RainbowBlackholeBg.Strengths), strengthLevel.ToString());
                    backdrop.SetStrength(backdropStrength);
                    break;

                case ActionType.SetAlpha:
                    backdrop.Alpha = alphaValue;
                    break;

                case ActionType.SetScale:
                    backdrop.Scale = scaleValue;
                    break;

                case ActionType.SetDirection:
                    backdrop.Direction = directionValue;
                    break;

                case ActionType.Toggle:
                    backdrop.Visible = !backdrop.Visible;
                    if (backdrop.Visible)
                    {
                        backdrop.Alpha = alphaValue;
                    }
                    break;
            }
        }
    }
}



