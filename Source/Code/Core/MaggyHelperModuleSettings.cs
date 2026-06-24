using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Persistent settings for KIRBY_CELESTE mod.
    /// Includes: Hot reload config, key bindings, boss/Kirby settings,
    /// overworld 3D preferences, area data display options.
    /// </summary>
    public enum AudioThemeMode
    {
        Pusheen,
        Kirby
    }

    public class MaggyHelperModuleSettings : EverestModuleSettings
    {
        #region Key Bindings

        [DefaultButtonBinding(Buttons.LeftTrigger, Keys.F10)]
        public ButtonBinding InGameMapEditor { get; set; }

        #endregion

        [SettingSubHeader("MaggyHelper_AUDIO_HEADER")]
        public AudioThemeMode AudioTheme { get; set; } = AudioThemeMode.Pusheen;

        public bool BossesExampleResetKeysForSession { get; set; }
        public int BossDifficultyMultiplier { get; set; } = 1;
        public bool EnableBossMusic { get; set; } = true;
        public bool KirbyPlayerEnabled { get; set; } = true;
        public int KirbyMaxFloatJumps { get; set; } = 5;
        public bool DebugMode { get; set; }
        public bool SkipModIntro { get; set; }
        public bool HasSeenIntroWarning { get; set; }

        /// <summary>
        /// Developer bypass flag: skips all introductory sequences and enables room-warp debug menus.
        /// Set this to true during testing cycles to bypass the mod selection screen, intro warning, and vessel creation.
        /// </summary>
        [SettingIgnore]
        public bool DeveloperBypass { get; set; }

        #region Overworld 3D Settings

        [SettingSubHeader("MaggyHelper_OVERWORLD_HEADER")]
        public bool EnableCustomMountainModels { get; set; } = true;

        public bool LockMountainCameraRotation { get; set; } = true;

        public bool SmoothCameraTransitions { get; set; } = true;

        public bool EnableMountainFogEffects { get; set; } = true;

        public bool ShowChapterPreviewInOverworld { get; set; } = true;

        [SettingRange(0, 2)]
        public int DefaultMountainStateOverride { get; set; } = 0;

        #endregion

        #region Area Data Display Settings

        [SettingSubHeader("MaggyHelper_AREADATA_HEADER")]
        public bool ShowSideUnlockNotifications { get; set; } = true;

        public bool ShowChapterMasteryOnPanel { get; set; } = true;

        public bool EnableCosmicBackgroundEffect { get; set; } = true;

        public bool ShowDSideDXSideInMenu { get; set; } = true;

        [SettingRange(0, 5)]
        public int ChapterDisplayMode { get; set; } = 0;

        #endregion

        #region Chapter Progression Settings

        [SettingSubHeader("MaggyHelper_PROGRESSION_HEADER")]
        public bool EnableLateChapterUnlockFlow { get; set; } = true;

        public bool AutoUnlockBSides { get; set; } = false;

        public bool AutoUnlockCSides { get; set; } = false;

        public bool EnableCassetteCollectibles { get; set; } = true;

        [SettingIgnore]
        public string LastPlayedChapterSID { get; set; }

        [SettingIgnore]
        public int LastPlayedSideIndex { get; set; }

        #endregion

        #region Hot Reload Settings (Development)

        [SettingSubHeader("MAGGYHELPER_HOTRELOAD_HEADER")]
        [SettingName("MAGGYHELPER_HOTRELOAD_ENABLED")]
        public bool HotReloadEnabled { get; set; } = true;

        [SettingName("MAGGYHELPER_HOTRELOAD_SHOW_UI")]
        public bool HotReloadShowUI { get; set; } = true;

        [SettingName("MAGGYHELPER_HOTRELOAD_SOUND")]
        public bool HotReloadSound { get; set; } = true;

        [SettingName("MAGGYHELPER_HOTRELOAD_VERBOSE")]
        public bool HotReloadVerbose { get; set; }

        [SettingName("MAGGYHELPER_BIND_HOTRELOAD_TOGGLE")]
        [DefaultButtonBinding(0, Keys.F5)]
        public ButtonBinding HotReloadToggle { get; set; }

        [SettingName("MAGGYHELPER_BIND_HOTRELOAD_RELOAD")]
        [DefaultButtonBinding(0, Keys.F6)]
        public ButtonBinding HotReloadManual { get; set; }

        [SettingName("MAGGYHELPER_BIND_HOTRELOAD_UI")]
        [DefaultButtonBinding(0, Keys.F8)]
        public ButtonBinding HotReloadUIBinding { get; set; }

        #endregion

        #region Mod Integration Settings

        [SettingSubHeader("MaggyHelper_INTEGRATIONS_HEADER")]
        [SettingName("MaggyHelper_DEATHLINK_DAMAGE")]
        public bool DeathlinkDamageEnabled
        {
            get => global::Celeste.DeathlinkIntegration.IsDamageModeEnabled();
            set => global::Celeste.DeathlinkIntegration.SetDamageModeEnabled(value);
        }

        #endregion
    }
}
