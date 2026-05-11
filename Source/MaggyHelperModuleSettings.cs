using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.MaggyHelper
{
    public class MaggyHelperModuleSettings : EverestModuleSettings
    {
        #region Hot Reload Settings (Development)

        [SettingSubHeader("MAGGYHELPER_HOTRELOAD_HEADER")]
        public bool HotReloadEnabled { get; set; } = true;

        public bool HotReloadAuto { get; set; } = true;

        public bool HotReloadShowUI { get; set; } = true;

        public bool HotReloadSound { get; set; } = true;

        public bool HotReloadAllMethods { get; set; }

        public bool HotReloadVerbose { get; set; }

        [SettingIgnore]
        public string HotReloadSourcePath { get; set; }

        #endregion

        #region Key Bindings

        [DefaultButtonBinding(Buttons.LeftShoulder, Keys.F5)]
        public ButtonBinding HotReloadToggle { get; set; }

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.F6)]
        public ButtonBinding HotReloadManual { get; set; }

        [DefaultButtonBinding(Buttons.LeftStick, Keys.F7)]
        public ButtonBinding HotReloadAll { get; set; }

        [DefaultButtonBinding(Buttons.RightStick, Keys.F8)]
        public ButtonBinding HotReloadUI { get; set; }

        #endregion

        public bool BossesExampleResetKeysForSession { get; set; }
        public int BossDifficultyMultiplier { get; set; } = 1;
        public bool EnableBossMusic { get; set; } = true;
        public bool EnableKirbyPlayer { get; set; } = true;
        public bool KirbyPlayerEnabled { get; set; } = true;
        public int KirbyMaxFloatJumps { get; set; } = 5;
        public bool DebugMode { get; set; }
        public bool SkipModIntro { get; set; }
        public bool HasSeenIntroWarning { get; set; }
    }
}
