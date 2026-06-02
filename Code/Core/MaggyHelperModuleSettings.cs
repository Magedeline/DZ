using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Persistent settings for MaggyHelper mod.
    /// Includes: Hot reload config, key bindings, boss/Kirby settings,
    /// overworld 3D preferences, area data display options.
    /// </summary>
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

        [DefaultButtonBinding(Buttons.BigButton, Keys.F9)]
        public ButtonBinding DebugMapEditor { get; set; }

        [DefaultButtonBinding(Buttons.LeftTrigger, Keys.F10)]
        public ButtonBinding InGameMapEditor { get; set; }

        [DefaultButtonBinding(0, Keys.P)]
        public ButtonBinding PCGQuickMenu { get; set; }

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

        /// <summary>
        /// Developer bypass flag: skips all introductory sequences and enables room-warp debug menus.
        /// Set this to true during testing cycles to bypass the mod selection screen, intro warning, and vessel creation.
        /// </summary>
        [SettingIgnore]
        public bool DeveloperBypass { get; set; }

        #region Overworld 3D Settings

        [SettingSubHeader("MAGGYHELPER_OVERWORLD_HEADER")]
        public bool EnableCustomMountainModels { get; set; } = true;

        public bool LockMountainCameraRotation { get; set; } = true;

        public bool SmoothCameraTransitions { get; set; } = true;

        public bool EnableMountainFogEffects { get; set; } = true;

        public bool ShowChapterPreviewInOverworld { get; set; } = true;

        [SettingRange(0, 2)]
        public int DefaultMountainStateOverride { get; set; } = 0;

        #endregion

        #region Area Data Display Settings

        [SettingSubHeader("MAGGYHELPER_AREADATA_HEADER")]
        public bool ShowSideUnlockNotifications { get; set; } = true;

        public bool ShowChapterMasteryOnPanel { get; set; } = true;

        public bool EnableCosmicBackgroundEffect { get; set; } = true;

        public bool ShowDSideDXSideInMenu { get; set; } = true;

        [SettingRange(0, 5)]
        public int ChapterDisplayMode { get; set; } = 0;

        #endregion

        #region Chapter Progression Settings

        [SettingSubHeader("MAGGYHELPER_PROGRESSION_HEADER")]
        public bool EnableLateChapterUnlockFlow { get; set; } = true;

        public bool AutoUnlockBSides { get; set; } = false;

        public bool AutoUnlockCSides { get; set; } = false;

        public bool EnableCassetteCollectibles { get; set; } = true;

        [SettingIgnore]
        public string LastPlayedChapterSID { get; set; }

        [SettingIgnore]
        public int LastPlayedSideIndex { get; set; }

        #endregion

        #region PCG Settings

        [SettingSubHeader("MAGGYHELPER_PCG_HEADER")]
        [SettingRange(1, 20)]
        [SettingName("MAGGYHELPER_PCG_FADE")]
        public int PCGWarpFadeDurationTenths { get; set; } = 5;

        public float PCGWarpFadeDuration => PCGWarpFadeDurationTenths / 10f;

        #endregion

        #region Mod Integration Settings

        [SettingSubHeader("MAGGYHELPER_INTEGRATIONS_HEADER")]
        [SettingName("MAGGYHELPER_DEATHLINK_DAMAGE")]
        public bool DeathlinkDamageEnabled
        {
            get => global::Celeste.DeathlinkIntegration.IsDamageModeEnabled();
            set => global::Celeste.DeathlinkIntegration.SetDamageModeEnabled(value);
        }

        #endregion
    }
}
