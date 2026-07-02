using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.DZ
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

    public class DZModuleSettings : EverestModuleSettings
    {
        #region Key Bindings

        [DefaultButtonBinding(Buttons.LeftTrigger, Keys.F10)]
        public ButtonBinding InGameMapEditor { get; set; }

        #endregion

        public bool BossesExampleResetKeysForSession { get; set; }
        public int BossDifficultyMultiplier { get; set; } = 1;
        public bool EnableBossMusic { get; set; } = true;
        public bool KirbyPlayerEnabled { get; set; } = true;
        public int KirbyMaxFloatJumps { get; set; } = 5;

        /// <summary>
        /// Gentle Breeze mode: a full assist bundle for the Kirby player.
        /// Enables dash-assist freeze (slow-mo aim arrow), infinite stamina,
        /// infinite dashes, and invincibility. Plays the gentlebreeze audio
        /// cue when the dash-assist freeze activates.
        /// </summary>
        [SettingName("DZ_GENTLEBREEZE_MODE")]
        public bool GentleBreezeMode { get; set; }

        /// <summary>
        /// Last Endemy mode: CelesteNet online multiplayer for the DZ mod.
        /// When enabled, the player can play online with other players.
        /// Requires the player to have agreed to the rules via OuiLastEndemyMode.
        /// </summary>
        [SettingName("DZ_LASTENDEMY_MODE")]
        public bool LastEndemyMode { get; set; }

        /// <summary>
        /// Tracks whether the player has agreed to the Last Endemy online rules
        /// and code of conduct presented in OuiLastEndemyMode. Persisted so the
        /// agreement only needs to be accepted once.
        /// </summary>
        [SettingIgnore]
        public bool LastEndemyAgreed { get; set; }

        public bool DebugMode { get; set; }
        public bool SkipModIntro { get; set; }
        public bool HasSeenIntroWarning { get; set; }
        public AudioThemeMode AudioThemeMode { get; set; } = AudioThemeMode.Pusheen;

        /// <summary>
        /// Developer bypass flag: skips all introductory sequences and enables room-warp debug menus.
        /// Set this to true during testing cycles to bypass the mod selection screen, intro warning, and vessel creation.
        /// </summary>
        [SettingIgnore]
        public bool DeveloperBypass { get; set; }

        #region Overworld 3D Settings

        [SettingSubHeader("DZ_OVERWORLD_HEADER")]
        public bool EnableCustomMountainModels { get; set; } = true;

        public bool LockMountainCameraRotation { get; set; } = true;

        public bool SmoothCameraTransitions { get; set; } = true;

        public bool EnableMountainFogEffects { get; set; } = true;

        public bool ShowChapterPreviewInOverworld { get; set; } = true;

        [SettingRange(0, 2)]
        public int DefaultMountainStateOverride { get; set; } = 0;

        #endregion

        #region Area Data Display Settings

        [SettingSubHeader("DZ_AREADATA_HEADER")]
        public bool ShowSideUnlockNotifications { get; set; } = true;

        public bool ShowChapterMasteryOnPanel { get; set; } = true;

        public bool EnableCosmicBackgroundEffect { get; set; } = true;

        public bool Show2DXSideInMenu { get; set; } = true;

        [SettingRange(0, 5)]
        public int ChapterDisplayMode { get; set; } = 0;

        #endregion

        #region Chapter Progression Settings

        [SettingSubHeader("DZ_PROGRESSION_HEADER")]
        public bool EnableLateChapterUnlockFlow { get; set; } = true;

        public bool AutoUnlock1s { get; set; } = false;

        public bool AutoUnlock2s { get; set; } = false;

        public bool EnableCassetteCollectibles { get; set; } = true;

        [SettingIgnore]
        public string LastPlayedChapterSID { get; set; }

        [SettingIgnore]
        public int LastPlaye2Index { get; set; }

        #endregion

        #region Hot Reload Settings (Development)

        [SettingSubHeader("DZ_HOTRELOAD_HEADER")]
        [SettingName("DZ_HOTRELOAD_ENABLED")]
        public bool HotReloadEnabled { get; set; } = true;

        [SettingName("DZ_HOTRELOAD_SHOW_UI")]
        public bool HotReloadShowUI { get; set; } = true;

        [SettingName("DZ_HOTRELOAD_SOUND")]
        public bool HotReloadSound { get; set; } = true;

        [SettingName("DZ_HOTRELOAD_VERBOSE")]
        public bool HotReloadVerbose { get; set; }

        [SettingName("DZ_BIND_HOTRELOAD_TOGGLE")]
        [DefaultButtonBinding(0, Keys.F9)]
        public ButtonBinding HotReloadToggle { get; set; }

        [SettingName("DZ_BIND_HOTRELOAD_RELOAD")]
        [DefaultButtonBinding(0, Keys.F11)]
        public ButtonBinding HotReloadManual { get; set; }

        [SettingName("DZ_BIND_HOTRELOAD_UI")]
        [DefaultButtonBinding(0, Keys.F12)]
        public ButtonBinding HotReloadUIBinding { get; set; }

        #endregion

        #region Mod Integration Settings

        [SettingSubHeader("DZ_INTEGRATIONS_HEADER")]
        [SettingName("DZ_DEATHLINK_DAMAGE")]
        public bool DeathlinkDamageEnabled
        {
            get => global::Celeste.DeathlinkIntegration.IsDamageModeEnabled();
            set => global::Celeste.DeathlinkIntegration.SetDamageModeEnabled(value);
        }

        #endregion

        /// <summary>Reset settings to defaults for hot reload.</summary>
        public void Reset()
        {
            // Reset key bindings
            InGameMapEditor = new ButtonBinding(Buttons.LeftTrigger, Keys.F10);

            // Reset boss/Kirby settings
            BossesExampleResetKeysForSession = false;
            BossDifficultyMultiplier = 1;
            EnableBossMusic = true;
            KirbyPlayerEnabled = true;
            KirbyMaxFloatJumps = 5;
            GentleBreezeMode = false;
            LastEndemyMode = false;
            LastEndemyAgreed = false;

            // Reset debug settings
            DebugMode = false;
            SkipModIntro = false;
            HasSeenIntroWarning = false;
            AudioThemeMode = AudioThemeMode.Pusheen;
            DeveloperBypass = false;

            // Reset overworld settings
            EnableCustomMountainModels = true;
            LockMountainCameraRotation = true;
            SmoothCameraTransitions = true;
            EnableMountainFogEffects = true;
            ShowChapterPreviewInOverworld = true;
            DefaultMountainStateOverride = 0;

            // Reset area data display settings
            ShowSideUnlockNotifications = true;
            ShowChapterMasteryOnPanel = true;
            EnableCosmicBackgroundEffect = true;
            Show2DXSideInMenu = true;
            ChapterDisplayMode = 0;
        }
    }
}
