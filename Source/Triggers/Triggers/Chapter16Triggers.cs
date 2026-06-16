namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger for Chapter 16 cutscenes with corruption and madness effects
    /// </summary>
    [CustomEntity(ids: "DZ/Chapter16CutsceneTrigger")]
    public class Chapter16CutsceneTrigger : Trigger
    {
        public string CutsceneId { get; private set; }
        public string DialogKey { get; private set; }
        public bool TriggerOnce { get; private set; }
        public bool PlayerOnly { get; private set; }
        public bool AutoStart { get; private set; }
        public bool EnableEffects { get; private set; }
        public float CorruptionLevel { get; private set; }
        public bool EnableTentacles { get; private set; }
        public int MadnessLevel { get; private set; }

        private bool triggered = false;

        public Chapter16CutsceneTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            CutsceneId = data.Attr("cutsceneId", "ch16_default");
            DialogKey = data.Attr("dialogKey", "DZ_CH16_DEFAULT");
            TriggerOnce = data.Bool("triggerOnce", true);
            PlayerOnly = data.Bool("playerOnly", true);
            AutoStart = data.Bool("autoStart", false);
            EnableEffects = data.Bool("enableEffects", true);
            CorruptionLevel = data.Float("corruptionLevel", 5f);
            EnableTentacles = data.Bool("enableTentacles", true);
            MadnessLevel = data.Int("madnessLevel", 3);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (AutoStart)
            {
                var level = scene as Level;
                var player = scene?.Tracker.GetEntity<global::Celeste.Player>();
                if (level != null && player != null)
                {
                    TriggerCutscene(level, player);
                }
            }
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (TriggerOnce && triggered)
                return;

            var level = SceneAs<Level>();
            if (level != null)
            {
                TriggerCutscene(level, player);
            }
        }

        private void TriggerCutscene(Level level, global::Celeste.Player player)
        {
            triggered = true;

            // Apply visual effects if enabled
            if (EnableEffects)
            {
                ApplyCorruptionEffects(level);
            }

            // Set session flags
            level.Session.SetFlag($"ch16_cutscene_{CutsceneId}", true);
            level.Session.SetFlag($"ch16_madness_level_{MadnessLevel}", true);

            // Check if Lua cutscene system is available
            if (LuaCutsceneManager.IsInitialized())
            {
                // Lua cutscene system handles the actual cutscene
                LuaCutsceneManager.CallLuaFunction($"triggerCutscene(\"{CutsceneId}\")");
            }
            else
            {
                // Fallback: just show dialog
                if (!string.IsNullOrEmpty(DialogKey))
                {
                    level.Add(new MiniTextbox(DialogKey));
                }
            }
        }

        private void ApplyCorruptionEffects(Level level)
        {
            // Apply screen corruption based on corruption level
            if (CorruptionLevel > 0)
            {
                // Glitch effect intensity
                level.Session.SetFlag("corruption_active", true);
            }

            // Enable tentacle entities if requested
            if (EnableTentacles)
            {
                level.Session.SetFlag("tentacles_active", true);
            }
        }
    }

    /// <summary>
    /// Specialized dialog trigger for Chapter 16 with character state management
    /// </summary>
    [CustomEntity(ids: "DZ/Chapter16DialogTrigger")]
    public class Chapter16DialogTrigger : Trigger
    {
        public string DialogKey { get; private set; }
        public string CharacterState { get; private set; }
        public bool EnableCharacterStates { get; private set; }
        public bool EnablePortraitChanges { get; private set; }
        public bool EnableEffects { get; private set; }
        public bool TriggerOnce { get; private set; }
        public bool PlayerOnly { get; private set; }
        public bool AutoStart { get; private set; }
        public int MadnessLevel { get; private set; }
        public bool EnableTentacles { get; private set; }

        private bool triggered = false;

        public Chapter16DialogTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            DialogKey = data.Attr("dialogKey", "DZ_CH16_DEFAULT");
            CharacterState = data.Attr("characterState", "madeline_default");
            EnableCharacterStates = data.Bool("enableCharacterStates", true);
            EnablePortraitChanges = data.Bool("enablePortraitChanges", true);
            EnableEffects = data.Bool("enableEffects", true);
            TriggerOnce = data.Bool("triggerOnce", true);
            PlayerOnly = data.Bool("playerOnly", true);
            AutoStart = data.Bool("autoStart", false);
            MadnessLevel = data.Int("madnessLevel", 3);
            EnableTentacles = data.Bool("enableTentacles", false);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (TriggerOnce && triggered)
                return;

            triggered = true;
            var level = SceneAs<Level>();
            if (level != null)
            {
                // Set character state flags
                if (EnableCharacterStates)
                {
                    level.Session.SetFlag($"ch16_character_{CharacterState}", true);
                }

                // Apply effects
                if (EnableEffects)
                {
                    level.Session.SetFlag("ch16_effects_active", true);
                    level.Session.SetFlag($"ch16_madness_{MadnessLevel}", true);
                }

                // Show dialog
                if (!string.IsNullOrEmpty(DialogKey))
                {
                    level.Add(new MiniTextbox(DialogKey));
                }
            }
        }
    }

    /// <summary>
    /// Special effects trigger for Chapter 16 screen effects and distortions
    /// </summary>
    [CustomEntity(ids: "DZ/Chapter16EffectsTrigger")]
    public class Chapter16EffectsTrigger : Trigger
    {
        public string EffectType { get; private set; }
        public float Intensity { get; private set; }
        public float Duration { get; private set; }
        public string TriggerCommand { get; private set; }
        public bool AutoTrigger { get; private set; }
        public bool TriggerOnce { get; private set; }
        public bool PlayerOnly { get; private set; }
        public bool EnableSound { get; private set; }
        public string SoundEvent { get; private set; }
        public string FlashColor { get; private set; }

        private bool triggered = false;

        public Chapter16EffectsTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            EffectType = data.Attr("effectType", "screen_shake");
            Intensity = data.Float("intensity", 5f);
            Duration = data.Float("duration", 2f);
            TriggerCommand = data.Attr("triggerCommand", "");
            AutoTrigger = data.Bool("autoTrigger", false);
            TriggerOnce = data.Bool("triggerOnce", true);
            PlayerOnly = data.Bool("playerOnly", true);
            EnableSound = data.Bool("enableSound", false);
            SoundEvent = data.Attr("soundEvent", "");
            FlashColor = data.Attr("flashColor", "FFFFFF");
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (TriggerOnce && triggered)
                return;

            triggered = true;
            var level = SceneAs<Level>();
            if (level != null)
            {
                ApplyEffect(level);
            }
        }

        private void ApplyEffect(Level level)
        {
            switch (EffectType.ToLower())
            {
                case "screen_shake":
                    level.Shake(Intensity * 0.1f);
                    break;

                case "flash_screen":
                    if (TryParseHexColor(FlashColor, out Color color))
                    {
                        level.Flash(color, false);
                    }
                    else
                    {
                        level.Flash(Color.White, false);
                    }
                    break;

                case "glitch_effect":
                    level.Session.SetFlag("ch16_glitch_active", true);
                    break;

                case "distortion":
                    // Apply distortion effect
                    level.Session.SetFlag("ch16_distortion_active", true);
                    break;
            }

            // Play sound if enabled
            if (EnableSound && !string.IsNullOrEmpty(SoundEvent))
            {
                Audio.Play(SoundEvent);
            }
        }

        private bool TryParseHexColor(string hex, out Color color)
        {
            color = Color.White;
            if (string.IsNullOrEmpty(hex))
                return false;

            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                try
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    color = new Color(r, g, b);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
