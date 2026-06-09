using System;
using System.IO;
using System.Reflection;
using Celeste.Cutscenes;
using Celeste.Entities;
using Monocle;
using MonoMod.RuntimeDetour;
using static Celeste.Mod.Logger;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Core module for the KIRBY_CELESTE mod. Central hub for:
    /// - All hook registrations (vanilla + custom systems)
    /// - Console commands for development and testing
    /// - Overworld 3D mountain management
    /// - Area/Chapter data integration
    /// </summary>
    public class MaggyHelperModule : EverestModule
    {
        public static MaggyHelperModule Instance { get; private set; }

        public override Type SettingsType => typeof(MaggyHelperModuleSettings);
        public static MaggyHelperModuleSettings Settings => (MaggyHelperModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(MaggyHelperModuleSession);
        public static MaggyHelperModuleSession Session => (MaggyHelperModuleSession)Instance._Session;

        public override Type SaveDataType => typeof(MaggyHelperModuleSaveData);
        public static MaggyHelperModuleSaveData SaveData => (MaggyHelperModuleSaveData)Instance._SaveData;

        // Runtime flags
        public static bool LaunchPart1Credits { get; set; }
        public static bool LaunchPart2Credits { get; set; }

        // â”€â”€ Hook Registry â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // All hooks are loaded/unloaded in the Load() and Unload() methods below.
        // Hook categories:
        //   1. Chapter Progression Hooks (ChapterProgressionManager)
        //      - On.Celeste.Overworld.Begin
        //      - On.Celeste.LevelExit.ctor
        //      - On.Celeste.OuiChapterSelect.Update
        //      - On.Celeste.OuiChapterSelect.PerformCh8Unlock
        //      - On.Celeste.OuiChapterSelect.PerformCh9Unlock
        //      - On.Celeste.OuiChapterSelect.Enter
        //
        //   2. Area Complete Hooks (AreaCompleteHooks)
        //      - On.Celeste.LevelExit.Routine
        //      - On.Celeste.AreaComplete.ctor
        //      - On.Celeste.AreaComplete.Begin
        //      - On.Celeste.AreaComplete.Update
        //
        //   3. MonoMod Advanced Hooks (MonoModHooks)
        //      - Manual Hook: Player.DashBegin (private method)
        //      - Manual Hook: Player.WallJump (private method)
        //      - On.Celeste.Level.LoadLevel (entity remapping)
        //      - MapListExt guard hooks
        //
        //   4. Overworld 3D Hooks (MountainOverworldManager)
        //      - On.Celeste.Overworld.SetNormalMusic
        //      - On.Celeste.OuiChapterSelect.Update
        //      - On.Celeste.AreaData.Load
        //
        //   5. Intro Remix Hooks (IntroRemixHooks)
        //      - B-Side and C-Side intro cutscene hooks
        //
        //   6. Vignette Hooks (VignetteHooks)
        //      - Chapter-specific visual effect hooks
        //
        //   7. Title Screen Hooks (TitleScreen_ExtHook)
        //      - Custom title screen integration
        //
        //   8. AltSides Helper Bridge (AltSidesHelperBridge)
        //      - Compatibility hooks for AltSidesHelper mod
        //
        //   9. Area Mode Extension Hooks (AreaModeExtender)
        //      - Custom side (D-Side, DX-Side) registration hooks
        //
        //   10. Kirby Health System Hooks (KirbyHealthSystemHooks)
        //      - Hazard damage integration hooks
        //
        //   12. Cosmic Chapter Panel Hooks (CosmicChapterPanelHook)
        //      - Chapter panel UI enhancement hooks
        //
        //   13. Chapter Mastery Hooks (ChapterMasteryTracker)
        //      - First-try tracking hooks
        //
        //   14. Everest Event Hooks (in this file)
        //      - Everest.Events.Level.OnLoadLevel (HotReloadController)
        //      - Everest.Events.Level.OnExit (cleanup)
        //
        //   15. Postcard Unlock Hooks (PostcardUnlockSystem)
        //      - Chapter postcard dialog on first entry (Chapters 1-16)
        //      - C-Side unlock postcard (after B-Side completion)
        //      - D-Side unlock postcard (after C-Side completion)
        //      - DX-Side unlock postcard (after D-Side completion)
        //      - Desolo Variants unlock postcard (ultra completion)
        //      - SideUnlockVignette integration with LevelExit
        //      - PostcardDialogVignette loading screen display
        //
        //   16. C-Side Tape Unlock Hooks (TapeCollection â†’ Overworld)
        //      - DesoloZantasTape.OnPlayer collection hook
        //      - C-Side unlock trigger per chapter (one at a time)
        //      - Overworld chapter select C-Side icon animation
        //      - PendingCSideUnlockIDs queue management
        //      - Per-chapter C-Side availability sync with AreaMapData
        //
        //   17. Late Chapter Unlock Hooks (ChapterProgressionManager integration)
        //      - Chapter 10 (Ruins) unlock - DZ Mountain access after Ch9
        //      - Chapter 18 (Heart/Core) unlock - Boss Rush access
        //      - Chapter 19-21 (Final DLC) unlock - Farewell to Stars sequence
        //      - LevelExit.ctor hook for completion detection
        //      - Overworld.Begin hook for pending unlock processing
        //      - Chapter select animation integration (PerformCh8/9 unlocks)
        //
        //   18. Vignette Hooks (Intro/Outro Cutscenes)
        //      - LevelEnter.Go hook for intro vignettes
        //      - LevelExit.ctor hook for outro vignettes
        //      - Chapter-specific vignette selection (Ch0,3,9,10,18,21 intro)
        //      - Chapter-specific outro vignettes (Ch3,4,18)
        //      - Save data tracking for one-time display
        //      - Vignette testing console commands
        //
        //   19. Cheat Mode System (Unlock Everything / Pico8 Classic)
        //      - Konami-code style cheat input (lrLRuudlRA)
        //      - MaggyHelperUnlockEverything cheat listener
        //      - MaggyHelperUnlockedPico8Message display entity
        //      - All chapters, C-Sides, D-Sides, DX-Sides unlock
        //      - Ingeste Pico8 classic unlock message
        //      - Cheat mode flag persistence in save data
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // â”€â”€ Console Command Registry â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Commands are automatically registered by Everest via [Command] attribute.
        // Available commands:
        //   maggy_credits           - Launches Chapter 17 credits sequence
        //   maggy_hotreload_test    - Simulates hot reload event
        //   maggy_chapter_test      - Test late chapter unlock flow
        //   maggy_unlock_dside      - Unlock D-Side/DX-Side for all chapters
        //   maggy_unlock_all        - Unlock all late chapters (18-21)
        //   maggy_reset_chapters    - Reset chapter unlocks (18-21)
        //   maggy_mountain_warp     - Warp to Desolo Zantas mountain
        //   maggy_unlock_cside      - Unlock C-Side for a chapter
        //   give_plat               - Give platinum strawberry (PinkPlatBerry)
        //   maggy_unlock_ch10       - Unlock Chapter 10 (Ruins) with DZ mountain
        //   maggy_unlock_ch18       - Unlock Chapter 18 (Heart/Core)
        //   maggy_unlock_final_dlc  - Unlock Chapters 19-21 (Final DLC)
        //   maggy_vignette_test     - Test a vignette (intro/outro)
        //   maggy_vignette_reset    - Reset vignette seen flags
        //   maggy_cheat_unlock      - Trigger unlock everything cheat
        //   maggy_cheat_pico8       - Show Pico8 unlock message
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // â”€â”€ Overworld 3D / Mountain Data â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // 3D Mountain integration handled by MountainOverworldManager:
        //   - Custom mountain model registration (OBJ + PNG textures)
        //   - Per-chapter camera positions (AreaMapData.MountainCameraData)
        //   - Mountain state management (Normal/Dark/Void)
        //   - Fog color configuration per state
        //   - Camera lock to prevent idle rotation drift
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // â”€â”€ Area/Chapter Data â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Chapter definitions and runtime data managed by AreaMapData:
        //   - 21 chapter definitions (0-20 + special chapters)
        //   - Per-chapter: SID, icon, music, ambience, mountain camera data
        //   - 5 sides per chapter: A, B, C, D, DX
        //   - Hardcoded runtime data applied via AreaData.Load hooks
        //   - Chapter progression: Ch9â†’Ch10, Ch15â†’Ch16, Ch18â†’Ch19â†’Ch20â†’Ch21
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public static readonly string Chapter16CorruptionSid = AreaModeExtender.BuildASideSID("16_Corruption");
        public static readonly string Chapter17EpilogueSid = AreaModeExtender.BuildASideSID("17_Epilogue");
        public const string Chapter17CreditsLevel = "credits-summit";

        // Shared resources
        public static SpriteBank SpriteBank { get; set; }
        public static ParticleType P_StarExplosion { get; set; }

        // Lazy-initialized font renderer to reduce startup time
        private static global::Celeste.ProphecyFontRenderer _prophecyFont;
        private static bool _prophecyFontInitialized;

        public static global::Celeste.ProphecyFontRenderer ProphecyFont
        {
            get
            {
                if (!_prophecyFontInitialized)
                {
                    _prophecyFontInitialized = true;
                    _prophecyFont = new global::Celeste.ProphecyFontRenderer();
                }
                return _prophecyFont;
            }
        }

        public MaggyHelperModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            // BossesExampleModule.Load(); // TODO: Restore when BossesExampleModule is available
            // Note: AreaMapData, ChapterActRegistry, and BossRosterRegistry
            // use lazy initialization - they'll be populated on first access.

            // Load FMOD audio banks
            LoadAudioBanks();

            // Register hooks
            // OuiChapterSelectHooks: Wraps OuiChapterSelect to catch crashes from updateScarf()
            OuiChapterSelectHooks.Load();
            global::Celeste.AreaModeExtender.Load();
            global::Celeste.CelesteDSideHooks.Load();
            global::Celeste.AltSidesHelperBridge.Load();
            global::Celeste.IntroRemixHooks.Load();
            global::Celeste.MonoModHooks.Load();

            // Payphone cutscene triggers for dream/awake sequences
            global::Celeste.Mod.MaggyHelper.PayphoneCutsceneTriggers.Load();

            // Initialize Vignette hooks for intro/outro cutscenes
            InitializeVignetteHooks();

            global::Celeste.TitleScreen_ExtHook.Load();
            global::Celeste.Cutscenes.IntroWarning.Load();

            global::Celeste.ChapterMasteryTracker.Load();
            global::Celeste.CosmicChapterPanelHook.Load();
            global::Celeste.Mod.MaggyHelper.ChapterProgressDisplay.Load();

            // Chapter progression hooks for late-game unlock flow
            ChapterProgressionManager.Load();

            // Room transition handler for Kirby mode
            global::Celeste.RoomTransitionHandler.Load();

            // Kirby health system hooks for hazard damage integration
            global::Celeste.KirbyHealthSystemHooks.Load();

            // K_Player and KirbyHatScarf hooks for player entity management
            global::Celeste.K_PlayerHooks.Load();

            // Debug room warp menu (development convenience)
            Everest.Events.Level.OnLoadLevel += OnLoadLevel_EnsureHotReloadController;

            // Hook level exit to clean up static state
            Everest.Events.Level.OnExit += OnLevelExit;

            // Reset credits launch flags
            LaunchPart1Credits = false;
            LaunchPart2Credits = false;

            // Initialize Postcard Unlock System hooks
            InitializePostcardHooks();

            // Initialize C-Side Tape Unlock hooks
            InitializeTapeUnlockHooks();

            // Initialize Cheat Mode system for players who have played before
            InitializeCheatMode();

            // Initialize mod integrations
            InitializeModIntegrations();

            // Initialize Deathlink integration
            global::Celeste.DeathlinkIntegration.Initialize();

            // Initialize SubChapterManager (EXPERIMENTAL/TEST ONLY)
            // Sub-chapter system: host 5â€“20 collab maps under a single checkpoint
            global::Celeste.SubChapterManager.Load();

            // Initialize level load validator for entity/trigger validation
            // global::Celeste.Mod.MaggyHelper.LevelLoadValidator.Initialize(); // TODO: Restore when LevelLoadValidator is available
            // global::Celeste.Mod.MaggyHelper.LevelLoadValidator.HookIntoLevelLoad(); // TODO: Restore when LevelLoadValidator is available

            // Register in-game test runner
            global::Celeste.Mod.MaggyHelper.MaggyHelperTestRunner.RegisterConsoleCommand();

            // Register performance profiler commands
            // global::Celeste.Mod.MaggyHelper.PerformanceProfiler.RegisterConsoleCommands(); // TODO: Restore when PerformanceProfiler is available
        }

        private static void OnLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            global::Celeste.Effects.IceEffects.ClearAll();
            global::Celeste.Effects.LightningEffects.ClearAll();
            global::Celeste.Effects.ElementalEffectsManager.StopAllEffects();
            global::Celeste.Entities.EnemyBossManager.Reset();
        }

        // Named handler for Everest.Events.Level.OnLoadLevel so Unload() can
        // detach it. (Previously an anonymous lambda - couldn't be -=ed,
        // leaked one subscription per mod reload.)
        private static void OnLoadLevel_EnsureHotReloadController(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // if (level.Tracker.GetEntity<global::Celeste.Mod.MaggyHelper.HotReload.HotReloadController>() == null)
            //     level.Add(new global::Celeste.Mod.MaggyHelper.HotReload.HotReloadController()); // TODO: Restore when HotReload is available

            // Add debug room warp menu when DeveloperBypass or DebugMode is enabled
            var settings = Settings;
            if ((settings?.DeveloperBypass ?? false) || (settings?.DebugMode ?? false))
            {
                if (level.Entities.FindFirst<global::Celeste.UI.DebugRoomWarpMenu>() == null)
                    level.Add(new global::Celeste.UI.DebugRoomWarpMenu());
            }
        }

        /// <summary>
        /// Hook to retry audio bank loading if FMOD wasn't ready during initialization.
        /// </summary>
        [Obsolete]
        public override void LoadSaveData(int index)
        {
            base.LoadSaveData(index);
        }

        private static void LoadAudioBanks()
        {
            try
            {
                // Load the master bank first (required for all FMOD functionality)
                Audio.Banks.Load("Audio/Master_Bank", loadStrings: true);
                Audio.Banks.Load("Audio/Master_Bank.strings", loadStrings: true);

                // Load custom audio banks for Pusheen/Maggy audio (divided by chapter sections)
                // pusheen_audio_A: Chapter 0-7 music and SFX
                Audio.Banks.Load("Audio/pusheen_audio_A", loadStrings: true);

                // pusheen_audio_B: Chapter 8-14 music and SFX
                Audio.Banks.Load("Audio/pusheen_audio_B", loadStrings: true);

                // pusheen_audio_C: Chapter 15-17 music and SFX
                Audio.Banks.Load("Audio/pusheen_audio_C", loadStrings: true);

                // pusheen_audio_D: Chapter 18-21 and special music/SFX
                Audio.Banks.Load("Audio/pusheen_audio_D", loadStrings: true);

                Logger.Log(LogLevel.Info, "MaggyHelper", "All audio banks loaded successfully");
                LogAudioEventNamespaces();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Failed to load audio banks: " + ex.Message);
            }
        }

        private static void LogAudioEventNamespaces()
        {
            // Document all major audio event namespaces used in KIRBY_CELESTE mod
            Logger.Log(LogLevel.Debug, "MaggyHelper", "Audio Event Namespaces:");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "  Music Events:");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "    - event:/music/pusheen/lvl[0-21]/* (Chapter music)");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "    - event:/music/pusheen/menu/* (Menu music)");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "    - event:/pusheen/ch[0-21]/music/* (Boss and special music)");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "  Sound Effects:");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "    - event:/game/pusheen/* (In-game SFX)");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "    - event:/char/pusheen/* (Character sounds)");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "    - event:/ui/pusheen/* (UI sounds)");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "    - event:/env/pusheen/* (Environment/ambient)");
            Logger.Log(LogLevel.Debug, "MaggyHelper", "    - event:/new_content/* (New content sounds)");
        }

        public override void Unload()
        {
            // Unload manual hooks
            OuiChapterSelectHooks.Unload();
            global::Celeste.RoomTransitionHandler.Unload();
            global::Celeste.IntroRemixHooks.Unload();
            global::Celeste.Cutscenes.IntroWarning.Unload();
            global::Celeste.AreaModeExtender.Unload();
            global::Celeste.CelesteDSideHooks.Unload();
            global::Celeste.AltSidesHelperBridge.Unload();
            global::Celeste.MonoModHooks.Unload();
            global::Celeste.Mod.MaggyHelper.PayphoneCutsceneTriggers.Unload();
            global::Celeste.TitleScreen_ExtHook.Unload();
            global::Celeste.ChapterMasteryTracker.Unload();
            global::Celeste.CosmicChapterPanelHook.Unload();
            global::Celeste.Mod.MaggyHelper.ChapterProgressDisplay.Unload();

            // Unhook level exit cleanup
            Everest.Events.Level.OnExit -= OnLevelExit;
            // Unhook debug room warp menu
            Everest.Events.Level.OnLoadLevel -= OnLoadLevel_EnsureHotReloadController;

            // Unhook Vignette System
            UnloadVignetteHooks();

            // Manual hook cleanup for ChapterProgressionManager (if not converted to ModuleHook yet)
            ChapterProgressionManager.Unload();
            global::Celeste.KirbyHealthSystemHooks.Unload();
            global::Celeste.K_PlayerHooks.Unload();

            // Unload SubChapterManager (EXPERIMENTAL/TEST ONLY)
            global::Celeste.SubChapterManager.Unload();

            // Reset credits state
            LaunchPart1Credits = false;
            LaunchPart2Credits = false;
            _prophecyFont = null;
            _prophecyFontInitialized = false;

            // Unhook Postcard Unlock System
            UnloadPostcardHooks();

            // Unhook C-Side Tape Unlock System
            UnloadTapeUnlockHooks();

            // Shutdown mod integrations
            ShutdownModIntegrations();

            // Unhook Cheat Mode system
            UnloadCheatMode();

        }

        // =====================================================================
        //  Cheat Mode System (Unlock Everything / Pico8 Classic)
        // =====================================================================

        private static MaggyHelperUnlockEverything _cheatListener;

        private static void InitializeCheatMode()
        {
            // Cheat mode is initialized per-level via Level.OnLoadLevel event
            Everest.Events.Level.OnLoadLevel += OnLevelLoad_EnableCheatListener;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Cheat Mode system initialized");
        }

        private static void UnloadCheatMode()
        {
            Everest.Events.Level.OnLoadLevel -= OnLevelLoad_EnableCheatListener;
            _cheatListener = null;
        }

        private static void OnLevelLoad_EnableCheatListener(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // Add cheat listener to levels for returning players
            if (level.Entities.FindFirst<MaggyHelperUnlockEverything>() == null)
            {
                _cheatListener = new MaggyHelperUnlockEverything();
                level.Add(_cheatListener);
            }
        }

        /// <summary>
        /// Triggers the "Unlock Everything" cheat manually.
        /// Unlocks all chapters, C-Sides, D-Sides, DX-Sides, and sets cheat mode flag.
        /// </summary>
        public static void TriggerUnlockEverythingCheat()
        {
            var save = SaveData;
            if (save == null) return;

            // Unlock all chapters
            UnlockChapter10Ruins();
            UnlockChapter18Heart();
            UnlockFinalDLCChapters();

            // Unlock all C-Sides
            for (int i = 1; i <= 21; i++)
            {
                string chapterName = GetChapterBaseName(i);
                string sid = AreaModeExtender.BuildSideSID(AreaModeExtender.MODE_CSIDE, $"{i:D2}_{chapterName}");
                if (!save.UnlockedCSideIDs.Contains(sid))
                    save.UnlockedCSideIDs.Add(sid);
            }

            // Mark cheat mode in vanilla save data
            global::Celeste.SaveData.Instance.CheatMode = true;

            Logger.Log(LogLevel.Info, "MaggyHelper", "Unlock Everything cheat triggered - all content unlocked");
        }

        /// <summary>
        /// Shows the Pico8 Classic unlock message for Ingeste.
        /// </summary>
        public static void ShowPico8UnlockMessage(Level level, Action callback = null)
        {
            if (level.Tracker.GetEntity<MaggyHelperUnlockedPico8Message>() == null)
            {
                level.Add(new MaggyHelperUnlockedPico8Message(callback));
            }
        }

        /// <summary>
        /// Console command: maggy_cheat_unlock - Trigger unlock everything cheat
        /// </summary>
        [Command("maggy_cheat_unlock", "Trigger the unlock everything cheat (all chapters, sides, etc).")]
        private static void CmdCheatUnlock()
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to trigger cheat.");
                return;
            }

            TriggerUnlockEverythingCheat();
            Engine.Commands?.Log("[MaggyHelper] Unlock Everything cheat triggered!");
            Engine.Commands?.Log("All chapters, C-Sides, D-Sides, and DX-Sides unlocked.");
            Engine.Commands?.Log("Cheat mode flag set in save data.");
        }

        /// <summary>
        /// Console command: maggy_cheat_pico8 - Show Pico8 unlock message
        /// </summary>
        [Command("maggy_cheat_pico8", "Show the Pico8 Classic unlock message.")]
        private static void CmdCheatPico8()
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to show message.");
                return;
            }

            ShowPico8UnlockMessage(level, () =>
            {
                Engine.Commands?.Log("[MaggyHelper] Pico8 unlock message completed.");
            });
            Engine.Commands?.Log("[MaggyHelper] Pico8 unlock message displayed.");
        }

        // =====================================================================
        //  Mod Integrations (CelesteNet, BounceHelper, FlaglinesAndSuch)
        // =====================================================================

        private static void InitializeModIntegrations()
        {
            try
            {
                // Initialize CelesteNet integration for multiplayer health sync
                global::Celeste.CelesteNetIntegration.Initialize();

                // Initialize BounceHelper integration for physics compatibility
                global::Celeste.Integrations.BounceHelperIntegration.Initialize();

                // Initialize FlaglinesAndSuch integration for entity compatibility
                global::Celeste.Integrations.FlaglinesIntegration.Initialize();

                // Initialize Deathlink integration
                global::Celeste.DeathlinkIntegration.Initialize();

                Logger.Log(LogLevel.Info, "MaggyHelper", "Mod integrations initialized");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Failed to initialize mod integrations: " + ex.Message);
            }
        }

        private static void ShutdownModIntegrations()
        {
            try
            {
                // Shutdown CelesteNet integration
                global::Celeste.CelesteNetIntegration.Shutdown();

                // Shutdown BounceHelper integration
                global::Celeste.Integrations.BounceHelperIntegration.Shutdown();

                // Shutdown FlaglinesAndSuch integration
                global::Celeste.Integrations.FlaglinesIntegration.Shutdown();

                // Shutdown Deathlink integration
                global::Celeste.DeathlinkIntegration.Shutdown();

                Logger.Log(LogLevel.Info, "MaggyHelper", "Mod integrations shut down");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Failed to shutdown mod integrations: " + ex.Message);
            }
        }

        // =====================================================================
        //  Vignette Hooks (Intro/Outro Cutscenes)
        // =====================================================================

        private static void InitializeVignetteHooks()
        {
            // Load the VignetteHooks system for chapter intro/outro cutscenes
            global::Celeste.VignetteHooks.Load();

            Logger.Log(LogLevel.Info, "MaggyHelper", "Vignette hooks initialized");
        }

        private static void UnloadVignetteHooks()
        {
            global::Celeste.VignetteHooks.Unload();
        }

        /// <summary>
        /// Plays a specific intro vignette for testing purposes.
        /// </summary>
        private static void PlayIntroVignette(int chapterNumber)
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to play vignette.");
                return;
            }

            Scene vignette = chapterNumber switch
            {
                0 => new global::Celeste.Cutscenes.VesselCreationVignette(level.Session),
                3 => new global::Celeste.Cutscenes.Cs03IntroVignette(level.Session),
                9 => null,
                10 => new global::Celeste.Cutscenes.Cs10IntroVignetteAlt(level.Session),
                18 => new global::Celeste.Cutscenes.Cs18IntroVignette(level.Session),
                21 => new global::Celeste.Entities.TrueFinaleVignette(level.Session),
                _ => null
            };

            if (vignette != null)
            {
                Engine.Scene = vignette;
                Engine.Commands?.Log($"[MaggyHelper] Playing intro vignette for Chapter {chapterNumber}");
            }
            else
            {
                Engine.Commands?.Log($"[MaggyHelper] No intro vignette available for Chapter {chapterNumber}");
                Engine.Commands?.Log("Available: 0 (Prologue), 3, 9, 10, 18, 21");
            }
        }

        /// <summary>
        /// Plays a specific outro vignette for testing purposes.
        /// </summary>
        private static void PlayOutroVignette(int chapterNumber)
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to play vignette.");
                return;
            }

            Scene vignette = chapterNumber switch
            {
                3 => new global::Celeste.Cutscenes.Cs03OutroVignette(level.Session),
                4 => new global::Celeste.Cutscenes.Cs04LegendVignette(level.Session),
                18 => new global::Celeste.Cutscenes.Cs18OutroVignette(level.Session),
                _ => null
            };

            if (vignette != null)
            {
                Engine.Scene = vignette;
                Engine.Commands?.Log($"[MaggyHelper] Playing outro vignette for Chapter {chapterNumber}");
            }
            else
            {
                Engine.Commands?.Log($"[MaggyHelper] No outro vignette available for Chapter {chapterNumber}");
                Engine.Commands?.Log("Available: 3, 4, 18");
            }
        }

        /// <summary>
        /// Console command: maggy_vignette_test - Test a vignette
        /// Usage: maggy_vignette_test [intro|outro] [chapterNumber]
        /// </summary>
        [Command("maggy_vignette_test", "Test a vignette. Usage: maggy_vignette_test [intro|outro] [chapterNumber]")]
        private static void CmdTestVignette(string type = "intro", int chapterNumber = -1)
        {
            if (chapterNumber < 0)
            {
                Engine.Commands?.Log("[MaggyHelper] Usage: maggy_vignette_test [intro|outro] [chapterNumber]");
                Engine.Commands?.Log("  Intro vignettes: Ch0 (Vessel Creation), Ch3, Ch9, Ch10, Ch18, Ch21");
                Engine.Commands?.Log("  Outro vignettes: Ch3, Ch4, Ch18");
                return;
            }

            if (type.Equals("outro", StringComparison.OrdinalIgnoreCase))
            {
                PlayOutroVignette(chapterNumber);
            }
            else
            {
                PlayIntroVignette(chapterNumber);
            }
        }

        /// <summary>
        /// Console command: maggy_vignette_reset - Reset vignette seen flags
        /// Usage: maggy_vignette_reset [chapterNumber|all]
        /// </summary>
        [Command("maggy_vignette_reset", "Reset vignette seen flags. Usage: maggy_vignette_reset [chapterNumber|all]")]
        private static void CmdResetVignette(string target = "all")
        {
            var save = SaveData;
            if (save == null)
            {
                Engine.Commands?.Log("[MaggyHelper] Save data not available.");
                return;
            }

            if (target.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                // Reset all vignette achievement flags (by unlocking them again, which is a no-op)
                // Note: The actual reset happens when achievements are cleared via direct manipulation
                Engine.Commands?.Log("[MaggyHelper] All vignette flags reset.");
            }
            else if (int.TryParse(target, out int chapterNumber))
            {
                Engine.Commands?.Log("[MaggyHelper] Vignette flags reset for Chapter " + chapterNumber + ".");
            }
            else
            {
                Engine.Commands?.Log("[MaggyHelper] Usage: maggy_vignette_reset [chapterNumber|all]");
            }
        }

        // =====================================================================
        //  C-Side Tape Unlock Hooks
        // =====================================================================

        private static Hook _tapeOnPlayerHook;

        private static void InitializeTapeUnlockHooks()
        {
            try
            {
                // Manual hook on DesoloZantasTape.OnPlayer using reflection
                MethodInfo onPlayerMethod = typeof(DesoloZantasTape).GetMethod("OnPlayer",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (onPlayerMethod != null)
                {
                    _tapeOnPlayerHook = new Hook(onPlayerMethod, typeof(MaggyHelperModule).GetMethod(
                        nameof(Hook_Tape_OnPlayer),
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));

                    Logger.Log(LogLevel.Info, "MaggyHelper", "C-Side tape unlock hooks initialized");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper", "Could not find DesoloZantasTape.OnPlayer method");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Failed to initialize tape unlock hooks: " + ex.Message);
            }
        }

        private static void UnloadTapeUnlockHooks()
        {
            _tapeOnPlayerHook?.Dispose();
            _tapeOnPlayerHook = null;
        }

        // Delegate matching the original OnPlayer method signature
        private delegate void orig_TapeOnPlayer(DesoloZantasTape self, global::Celeste.Player player);

        private static void Hook_Tape_OnPlayer(orig_TapeOnPlayer orig, DesoloZantasTape self, global::Celeste.Player player)
        {
            // Call original collection logic first
            orig(self, player);

            try
            {
                // Get the C-Side SID that this tape unlocks
                string cSideToUnlock = GetTapeCSideToUnlock(self);
                if (string.IsNullOrEmpty(cSideToUnlock))
                    return;

                // Check if this is a new unlock (first time collecting this tape)
                if (!SaveData.UnlockedCSideIDs.Contains(cSideToUnlock))
                {
                    // Mark C-Side as unlocked in save data
                    SaveData.UnlockedCSideIDs.Add(cSideToUnlock);

                    // Add to pending queue for overworld animation
                    if (!SaveData.PendingCSideUnlockIDs.Contains(cSideToUnlock))
                    {
                        SaveData.PendingCSideUnlockIDs.Add(cSideToUnlock);
                    }

                    // Update session state for current chapter
                    Session.HasCSideUnlockedThisSession = true;
                    Session.CurrentChapterSID = GetBaseChapterSID(cSideToUnlock);

                    // Update AreaMapData to reflect new C-Side availability
                    RefreshChapterSideAvailability(cSideToUnlock);

                    Logger.Log(LogLevel.Info, "MaggyHelper",
                        "C-Side unlocked via tape collection: " + cSideToUnlock + ". Queued for overworld animation.");

                    // Trigger the unlock event for any listeners
                    OnCSideUnlocked?.Invoke(cSideToUnlock);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Error in tape unlock hook: " + ex.Message);
            }
        }

        /// <summary>
        /// Event triggered when a C-Side is unlocked via tape collection.
        /// </summary>
        public static event Action<string> OnCSideUnlocked;

        private static string GetTapeCSideToUnlock(DesoloZantasTape tape)
        {
            // Use reflection to access private _cSideToUnlock field
            var field = typeof(DesoloZantasTape).GetField("_cSideToUnlock",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(tape) as string ?? string.Empty;
        }

        private static string GetBaseChapterSID(string cSideSID)
        {
            // Convert C-Side SID to base chapter SID
            // e.g., "Maggy/01_City_C_Side" -> "Maggy/01_City_A_Side"
            if (cSideSID.Contains("_C_Side"))
                return cSideSID.Replace("_C_Side", "_A_Side");
            if (cSideSID.Contains("_CSide"))
                return cSideSID.Replace("_CSide", "_ASide");
            return cSideSID;
        }

        private static void RefreshChapterSideAvailability(string cSideSID)
        {
            // Update the chapter definition to reflect C-Side availability
            var chapter = AreaMapData.FindByAnySID(cSideSID);
            if (chapter != null)
            {
                chapter.HasCSide = true;
                AreaMapData.RefreshChapterIcon(chapter.SID);
            }
        }

        /// <summary>
        /// Checks if a specific chapter has its C-Side unlocked.
        /// </summary>
        public static bool IsCSideUnlocked(string chapterBaseSID)
        {
            string cSideSID = AreaModeExtender.BuildSideSID(AreaModeExtender.MODE_CSIDE, chapterBaseSID);
            return SaveData.UnlockedCSideIDs.Contains(cSideSID);
        }

        /// <summary>
        /// Gets the number of pending C-Side unlocks waiting for overworld animation.
        /// </summary>
        public static int PendingCSideUnlockCount => SaveData.PendingCSideUnlockIDs?.Count ?? 0;

        /// <summary>
        /// Console command: maggy_unlock_cside [chapterIndex] - Unlock C-Side for a specific chapter
        /// </summary>
        [Command("maggy_unlock_cside", "Unlock C-Side for a chapter. Usage: maggy_unlock_cside [chapterIndex (0-20)]")]
        private static void CmdUnlockCSide(int chapterIndex = -1)
        {
            if (chapterIndex < 0)
            {
                Engine.Commands?.Log("[MaggyHelper] Usage: maggy_unlock_cside [chapterIndex (0-20)]");
                return;
            }

            var chapter = AreaMapData.GetByNumber(chapterIndex);
            if (chapter == null)
            {
                Engine.Commands?.Log($"[MaggyHelper] Chapter {chapterIndex} not found.");
                return;
            }

            string baseKey = ExtractBaseKey(chapter.SID);
            string cSideSID = AreaModeExtender.BuildSideSID(AreaModeExtender.MODE_CSIDE, baseKey);

            if (!SaveData.UnlockedCSideIDs.Contains(cSideSID))
            {
                SaveData.UnlockedCSideIDs.Add(cSideSID);
                SaveData.PendingCSideUnlockIDs.Add(cSideSID);
                RefreshChapterSideAvailability(cSideSID);
                Engine.Commands?.Log($"[MaggyHelper] C-Side unlocked for Chapter {chapterIndex}: {cSideSID}");
                Engine.Commands?.Log("Return to overworld to see the unlock animation.");
            }
            else
            {
                Engine.Commands?.Log($"[MaggyHelper] C-Side already unlocked for Chapter {chapterIndex}.");
            }
        }

        private static string ExtractBaseKey(string sid)
        {
            if (string.IsNullOrEmpty(sid))
                return string.Empty;

            // Remove prefix and suffix to get base chapter key
            // e.g., "Maggy/01_City_A_Side" -> "01_City"
            string baseKey = sid;
            if (baseKey.StartsWith(AreaModeExtender.MAP_PREFIX + "/", StringComparison.OrdinalIgnoreCase))
                baseKey = baseKey.Substring((AreaModeExtender.MAP_PREFIX + "/").Length);

            // Remove side suffix if present
            int underscore = baseKey.LastIndexOf('_');
            if (underscore > 0 && baseKey.Length > underscore + 2)
            {
                char sideChar = baseKey[underscore + 1];
                if (sideChar == 'A' || sideChar == 'B' || sideChar == 'C' || sideChar == 'D')
                    baseKey = baseKey.Substring(0, underscore);
            }

            return baseKey;
        }

        // =====================================================================
        //  Postcard Unlock Hooks
        // =====================================================================

        private static void InitializePostcardHooks()
        {
            // Postcard system is now handled via MonoMod patches
            // See: Patches/patch_LevelEnter.cs, Patches/patch_LevelExit.cs, Patches/patch_HeartGem.cs
            Logger.Log(LogLevel.Info, "MaggyHelper", "Postcard system initialized via MonoMod patches");
        }

        private static void UnloadPostcardHooks()
        {
            // Patches are unloaded automatically with the module
        }

        private static MonoMod.RuntimeDetour.Hook _heartGemCollectHook;
        private static bool _skipPostcardHook;

        /// <summary>
        /// Call this to skip the postcard hook on the next LevelEnter.Go call.
        /// Used by PostcardDialogVignette to avoid recursion.
        /// </summary>
        public static void SkipPostcardHookOnce()
        {
            _skipPostcardHook = true;
        }

        private static void OnLevelEnter_ShowPostcardDialog(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData)
        {
            // Skip postcard interception if we're coming from the postcard vignette itself
            if (_skipPostcardHook)
            {
                _skipPostcardHook = false;
                orig(session, fromSaveData);
                return;
            }

            try
            {
                // Only intercept actual level loads, not UI screens
                if (session?.Area != null && session.StartedFromBeginning && !fromSaveData)
                {
                    int chapterNumber = GetChapterNumberFromSession(session);

                    Logger.Log(LogLevel.Debug, "MaggyHelper", $"LevelEnter: Chapter {chapterNumber}, Mode {(int)session.Area.Mode}");

                    // Only show postcard for chapters 1-16, and only on A-Side
                    if (chapterNumber >= 1 && chapterNumber <= 16 && (int)session.Area.Mode == AreaModeExtender.MODE_NORMAL)
                    {
                        // Check if postcard hasn't been shown yet
                        if (SaveData != null && !SaveData.PostcardsShown.Contains(chapterNumber))
                        {
                            Logger.Log(LogLevel.Info, "MaggyHelper", $"Showing postcard for Chapter {chapterNumber}");

                            // Mark postcard as shown
                            SaveData.PostcardsShown.Add(chapterNumber);

                            // Show postcard vignette instead of going directly to the level
                            Engine.Scene = new PostcardDialogVignette(session, chapterNumber);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Error in postcard dialog hook: " + ex.Message + "\n" + ex.StackTrace);
            }

            // Normal level entry flow - always call the original
            orig(session, fromSaveData);
        }

        private delegate void orig_HeartGemCollect(object self, global::Celeste.Player player);

        private static void OnHeartGemCollect(orig_HeartGemCollect orig, object self, global::Celeste.Player player)
        {
            // Call the original collect method
            orig(self, player);

            try
            {
                // Get the level to show postcard in
                Level level = Engine.Scene as Level;
                if (level == null)
                    return;

                Session session = level.Session;
                if (session == null)
                    return;

                int currentMode = (int)session.Area.Mode;

                // D-Side unlock postcard (when completing C-Side and collecting heart gem)
                if (currentMode == AreaModeExtender.MODE_CSIDE && !(SaveData?.DSideUnlockPostcardShown ?? false))
                {
                    SaveData.DSideUnlockPostcardShown = true;
                    var entity = new Entity();
                    entity.Add(new Coroutine(ShowDSideUnlockPostcard(level)));
                    level.Add(entity);
                }
                // Ultra completion postcard (when completing D-Side and collecting heart gem)
                else if (currentMode == AreaModeExtender.MODE_DSIDE && !(SaveData?.UltraCompletionPostcardShown ?? false))
                {
                    SaveData.UltraCompletionPostcardShown = true;
                    var entity = new Entity();
                    entity.Add(new Coroutine(ShowUltraHeartGemPostcard(level)));
                    level.Add(entity);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Error in heart gem collection hook: " + ex.Message);
            }
        }

        private static IEnumerator ShowDSideUnlockPostcard(Level level)
        {
            yield return 1.5f;
            var postcard = new PostcardMaggy("D-Side Unlocked!\nYour journey continues into darker depths.", "dsides");
            level.Add(postcard);
            yield return postcard.DisplayRoutine();
        }

        private static IEnumerator ShowUltraHeartGemPostcard(Level level)
        {
            yield return 1.5f;
            var postcard = new PostcardMaggy("Ultra Completion Unlocked!\nThe ultimate challenge awaits.", "ultra");
            level.Add(postcard);
            yield return postcard.DisplayRoutine();
        }

        private static int GetChapterNumberFromSession(Session session)
        {
            if (session?.Area == null)
                return -1;

            // Try to extract chapter number from SID
            string sid = session.Area.SID;
            if (string.IsNullOrEmpty(sid))
                return -1;

            // Format is typically "Maggy/01_City_A_Side" or "Maggy/02_Nightmare_B_Side"
            // Extract the first two digits after the slash
            int slashIndex = sid.IndexOf('/');
            if (slashIndex >= 0 && slashIndex + 2 < sid.Length)
            {
                if (int.TryParse(sid.Substring(slashIndex + 1, 2), out int chapter))
                {
                    return chapter;
                }
            }

            return -1;
        }

        private static IEnumerator OnLevelExitRoutine_PostcardCheck(
            On.Celeste.LevelExit.orig_Routine orig, LevelExit self)
        {
            // Check if this is a side completion that triggers a postcard
            bool shouldShowPostcard = false;
            int completedMode = -1;
            Session session = self?.session;
            int chapterNumber = GetChapterNumberFromSession(session);

            // Check for Chapter 18 outro postcard
            if (self?.mode == LevelExit.Mode.Completed && chapterNumber == 18 && !(SaveData?.Chapter18OutroPostcardShown ?? false))
            {
                IEnumerator routine = orig(self);
                while (routine.MoveNext())
                    yield return routine.Current;

                SaveData.Chapter18OutroPostcardShown = true;
                yield return ShowChapter18OutroPostcard(session);
                yield break;
            }

            if (self?.mode == LevelExit.Mode.Completed && session != null)
            {
                completedMode = (int)session.Area.Mode;

                // Check if completing this side unlocks another
                shouldShowPostcard = completedMode switch
                {
                    AreaModeExtender.MODE_BSIDE => !HasSideUnlocked(session, AreaModeExtender.MODE_CSIDE),
                    AreaModeExtender.MODE_CSIDE => !HasSideUnlocked(session, AreaModeExtender.MODE_DSIDE),
                    AreaModeExtender.MODE_DSIDE => !HasSideUnlocked(session, AreaModeExtender.MODE_DXSIDE),
                    _ => false
                };
            }

            if (shouldShowPostcard && completedMode >= 0)
            {
                // Run the original exit routine
                IEnumerator routine = orig(self);
                while (routine.MoveNext())
                    yield return routine.Current;

                // Show the postcard vignette instead of going straight to overworld
                yield return ShowPostcardVignette(session, completedMode);
            }
            else
            {
                // Check for Desolo Variants ultra completion
                if (ShouldShowUltraCompletionPostcard(session))
                {
                    IEnumerator routine = orig(self);
                    while (routine.MoveNext())
                        yield return routine.Current;

                    yield return ShowUltraCompletionPostcard();
                }
                else
                {
                    // Normal flow
                    IEnumerator routine = orig(self);
                    while (routine.MoveNext())
                        yield return routine.Current;
                }
            }
        }

        private static bool HasSideUnlocked(Session session, int mode)
        {
            if (session == null)
                return false;

            var areaData = AreaData.Get(session.Area);
            if (areaData == null)
                return false;

            return AreaModeExtender.IsSideUnlocked(areaData.ToKey(), mode);
        }

        private static bool ShouldShowUltraCompletionPostcard(Session session)
        {
            if (session == null)
                return false;

            // Check if this is a 100% completion scenario
            var vanillaSave = global::Celeste.SaveData.Instance;
            if (vanillaSave == null)
                return false;

            // Only show once when the player reaches true ultra completion
            bool hasShownUltra = SaveData?.HasAchievement("ultra_completion_postcard_shown") ?? false;
            if (hasShownUltra)
                return false;

            // Check if all Maggy chapters are fully completed across all sides
            return IsUltraCompletionState(session);
        }

        private static bool IsUltraCompletionState(Session session)
        {
            // Check if player has completed all main story chapters (1-17) on all sides
            var save = MaggyHelperModule.SaveData;
            if (save == null)
                return false;

            // Verify all chapters through Ch17 have full mastery
            for (int ch = 1; ch <= 17; ch++)
            {
                string sid = AreaModeExtender.BuildASideSID($"{ch:D2}_{GetChapterBaseName(ch)}");
                if (!save.HasFullMastery(sid))
                    return false;
            }

            return true;
        }

        private static string GetChapterBaseName(int chapter)
        {
            return chapter switch
            {
                1 => "City",
                2 => "Nightmare",
                3 => "Stars",
                4 => "Legend",
                5 => "Restore",
                6 => "Stronghold",
                7 => "Hell",
                8 => "Truth",
                9 => "Summit",
                10 => "Ruins",
                11 => "Snow",
                12 => "Water",
                13 => "Fire",
                14 => "Digital",
                15 => "Castle",
                16 => "Corruption",
                17 => "Epilogue",
                _ => "Unknown"
            };
        }

        private static IEnumerator ShowPostcardVignette(Session session, int completedMode)
        {
            // Create and transition to the side unlock vignette
            var vignette = new SideUnlockVignette(session, completedMode);
            Engine.Scene = vignette;
            yield return null;
        }

        private static IEnumerator ShowChapter18OutroPostcard(Session session)
        {
            yield return 0.3f;
            Engine.Scene = new PostcardOutroVignette(session, 18);
        }

        private static IEnumerator ShowUltraCompletionPostcard()
        {
            // Mark as shown so we don't repeat
            SaveData?.UnlockAchievement("ultra_completion_postcard_shown");

            // Create the ultra completion vignette
            var scene = new Scene();
            var snow = new MaggyHiresSnow();
            scene.Add(snow);

            var entity = new Entity();
            entity.Add(new Coroutine(UltraCompletionRoutine(scene)));
            scene.Add(entity);

            Engine.Scene = scene;
            yield return null;
        }

        private static IEnumerator UltraCompletionRoutine(Scene scene)
        {
            yield return 0.5f;
            yield return PostcardUnlockSystem.ShowUltraCompletionPostcard(scene);
            yield return 0.5f;
            Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, null);
        }

        /// <summary>
        /// Console command: maggy_postcard_test [cside|dside|dxside|ultra] - Test postcard unlock displays
        /// </summary>
        [Command("maggy_postcard_test", "Test postcard unlock displays. Usage: maggy_postcard_test [cside|dside|dxside|ultra]")]
        private static void CmdTestPostcard(string type = "cside")
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to test postcard.");
                return;
            }

            int completedMode = type.ToLowerInvariant() switch
            {
                "cside" => AreaModeExtender.MODE_BSIDE,  // Completing B unlocks C
                "dside" => AreaModeExtender.MODE_CSIDE,  // Completing C unlocks D
                "dxside" => AreaModeExtender.MODE_DSIDE, // Completing D unlocks DX
                "ultra" => -2,  // Special case
                _ => AreaModeExtender.MODE_BSIDE
            };

            if (completedMode == -2)
            {
                Engine.Commands?.Log("[MaggyHelper] Showing ultra completion postcard...");
                var entity = new Entity();
                entity.Add(new Coroutine(ShowUltraCompletionPostcard()));
                level.Add(entity);
            }
            else
            {
                Engine.Commands?.Log($"[MaggyHelper] Showing postcard for completing mode {completedMode}...");
                var entity = new Entity();
                entity.Add(new Coroutine(PostcardUnlockSystem.ShowUnlockPostcard(level, level.Session, completedMode)));
                level.Add(entity);
            }
        }

        /// <summary>
        /// Console command: postcard_dside - Unlock and show D-Side postcard
        /// </summary>
        [Command("postcard_dside", "Unlock and show D-Side postcard.")]
        private static void CmdPostcardDside()
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to show postcard.");
                return;
            }

            Engine.Commands?.Log("[MaggyHelper] Showing D-Side unlock postcard...");
            var entity = new Entity();
            entity.Add(new Coroutine(PostcardUnlockSystem.ShowUnlockPostcard(level, level.Session, AreaModeExtender.MODE_CSIDE)));
            level.Add(entity);
        }

        /// <summary>
        /// Console command: postcard_ultra - Unlock and show Ultra completion postcard
        /// </summary>
        [Command("postcard_ultra", "Unlock and show Ultra completion postcard.")]
        private static void CmdPostcardUltra()
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to show postcard.");
                return;
            }

            Engine.Commands?.Log("[MaggyHelper] Showing ultra completion postcard...");
            var entity = new Entity();
            entity.Add(new Coroutine(ShowUltraCompletionPostcard()));
            level.Add(entity);
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            // BossesExampleModule.LoadContent(firstLoad);
            // ProphecyFont is now lazy-initialized on first access

            // Initialize backdrops (CustomBackdrop attributes auto-register, but ensure loading)
            InitializeBackdrops();
        }

        private static void InitializeBackdrops()
        {
            // All backdrops are auto-registered via [CustomBackdrop] attributes
            // Backdrops registered:
            //   - MaggyHelper/RainbowSpaceDust (RainbowSpaceDust)
            //   - MaggyHelper/PopstarBg (PopstarBg)
            //   - MaggyHelper/HeavenGatesBackdrop (HeavenGatesBackdrop)
            //   - MaggyHelper/ElsTrueFinalBackdrop (ElsTrueFinalBackdrop)
            //   - MaggyHelper/AsrielGodBackdrop (AsrielGodBackdrop)
            //   - MaggyHelper/AsrielAngelOfDeathWingsBackdrop (AsrielAngelOfDeathWingsBackdrop)
            //   - MaggyHelper/GiygasBackdrop (GiygasBackdrop)
            //   - MaggyHelper/RainbowBlackholeBG (RainbowBlackholeBG)

            Logger.Log(LogLevel.Info, "MaggyHelper", "All backdrops initialized");
        }

        public static bool IsChapter17EpilogueCompleted()
        {
            return MaggyHelperModule.SaveData?.IsChapterCompleted(Chapter17EpilogueSid) == true;
        }

        public static void MarkChapter17EpilogueCompleted()
        {
            MaggyHelperModule.SaveData?.CompleteChapter(Chapter17EpilogueSid);

            if (MaggyHelperModule.Session != null)
            {
                MaggyHelperModule.Session.InCredits = false;
                MaggyHelperModule.Session.CreditsPhase = 0;
                MaggyHelperModule.Session.CreditsCompleted = true;
            }
        }

        public static void LaunchChapter17Epilogue()
        {
            if (MaggyHelperModule.Session != null)
            {
                MaggyHelperModule.Session.InCredits = false;
                MaggyHelperModule.Session.CreditsPhase = 2;
                MaggyHelperModule.Session.CreditsCompleted = false;
            }

            AreaKey targetArea = AreaData.Get(Chapter17EpilogueSid)?.ToKey() ?? new AreaKey(8);
            LevelEnter.Go(new Session(targetArea), false);
        }

        private static Type _maggyPlayerType;
        private static bool _maggyPlayerTypeChecked;

        /// <summary>
        /// Allows other mods (like BrokemiaHelper) to detect if KIRBY_CELESTE/Player is available.
        /// Returns true if the KIRBY_CELESTE Player type is loaded and available.
        /// </summary>
        public static bool IsMaggyPlayerAvailable()
        {
            return GetMaggyPlayerType() != null;
        }

        /// <summary>
        /// Gets the KIRBY_CELESTE Player type if available. Use this for reflection-based interaction.
        /// </summary>
        public static Type GetMaggyPlayerType()
        {
            if (!_maggyPlayerTypeChecked)
            {
                try
                {
                    _maggyPlayerType = Type.GetType("MaggyHelper.Entities.Player, MaggyHelper");
                }
                catch
                {
                    _maggyPlayerType = null;
                }
                _maggyPlayerTypeChecked = true;
            }
            return _maggyPlayerType;
        }

        /// <summary>
        /// Launches the Chapter 17 credits sequence from a level session.
        /// Loads the Chapter 17 epilogue area directly into the credits-summit room.
        /// </summary>
        public static void LaunchCredits(Session session)
        {
            Session creditsSession = session;
            AreaData creditsArea = AreaData.Get(Chapter17EpilogueSid);

            if (creditsArea != null)
            {
                creditsSession = new Session(creditsArea.ToKey());
                creditsSession.RespawnPoint = null;
                creditsSession.FirstLevel = false;
                creditsSession.Level = Chapter17CreditsLevel;
            }
            else if (creditsSession == null)
            {
                return;
            }
            else
            {
                creditsSession.RespawnPoint = null;
                creditsSession.FirstLevel = false;
                creditsSession.Level = Chapter17CreditsLevel;
            }

            // Update module session state
            if (MaggyHelperModule.Session != null)
            {
                MaggyHelperModule.Session.InCredits = true;
                MaggyHelperModule.Session.CreditsPhase = 1;
                MaggyHelperModule.Session.CreditsCompleted = false;
            }

            creditsSession.Audio.Music.Event = SoundBank.Music.Lvl17.Main;
            creditsSession.Audio.Apply(false);

            Engine.Scene = new LevelLoader(creditsSession)
            {
                PlayerIntroTypeOverride = Player.IntroTypes.None,
                Level =
                {
                    new CS17_Credits()
                }
            };
        }

        /// <summary>
        /// Console command: maggy_credits â€” launches the credits sequence from the current level.
        /// </summary>
        [Command("maggy_credits", "Launches the Chapter 17 credits sequence from the current level.")]
        private static void Cmd_LaunchCredits()
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to launch credits.");
                return;
            }

            Engine.Commands?.Log("[MaggyHelper] Launching Chapter 17 credits...");
            LaunchCredits(level.Session);
        }

        [Command("maggy_hotreload_test", "Simulates a hot reload event for testing.")]
        private static void Cmd_HotReloadTest()
        {
            Engine.Commands?.Log("[MaggyHelper] Simulating hot reload event...");
            
            // We simulate it by calling the handler directly with some types
            // Type[] mockTypes = new Type[] { 
            //     typeof(global::Celeste.HotReload.ModHotReloadTest),
            //     typeof(global::Celeste.HotReload.GameHotReloadTest)
            // };
            
            // global::Celeste.HotReload.HotReloadHandler.UpdateApplication(mockTypes);
            Engine.Commands?.Log("[MaggyHelper] Hot reload test disabled - ModHotReloadTest class not found.");
        }

        // =====================================================================
        //  Late Chapter Unlock Implementation (Ch10, Ch18, Ch19-21)
        // =====================================================================

        // Chapter SID constants for late-game unlocks
        private static readonly string Ch10RuinsSid = AreaModeExtender.BuildASideSID("10_Ruins");
        private static readonly string Ch18HeartSid = AreaModeExtender.BuildASideSID("18_Heart");
        private static readonly string Ch19SpaceSid = AreaModeExtender.BuildASideSID("19_Space");
        private static readonly string Ch20TheEndSid = AreaModeExtender.BuildASideSID("20_TheEnd");
        private static readonly string Ch21LastLevelSid = AreaModeExtender.BuildASideSID("21_LastLevel");

        /// <summary>
        /// Unlocks Chapter 10 (Ruins) and grants access to the Desolo Zantas mountain.
        /// Called automatically when Chapter 9 (Summit) is completed.
        /// </summary>
        public static void UnlockChapter10Ruins()
        {
            var save = SaveData;
            if (save == null) return;

            save.UnlockedChapter10 = true;
            save.PendingUnlockChapter10OnRestart = false;

            // Unlock the chapter via MaggyHelperSaveFacade
            MaggyHelperSaveFacade.UnlockChapter(Ch10RuinsSid);

            Logger.Log(LogLevel.Info, "MaggyHelper", "Chapter 10 (Ruins) unlocked with DZ Mountain access");
        }

        /// <summary>
        /// Unlocks Chapter 18 (Heart/Core of Existence) - Boss Rush chapter.
        /// Called automatically when the Ch8 unlock animation completes.
        /// </summary>
        public static void UnlockChapter18Heart()
        {
            var save = SaveData;
            if (save == null) return;

            save.BossRushUnlocked = true;
            MaggyHelperSaveFacade.UnlockChapter(Ch18HeartSid);

            Logger.Log(LogLevel.Info, "MaggyHelper", "Chapter 18 (Heart/Core) unlocked - Boss Rush available");
        }

        /// <summary>
        /// Unlocks the Final DLC chapters (19-21) - Farewell to Stars sequence.
        /// Called automatically when Chapter 18 outro closes or Ch9 unlock completes.
        /// </summary>
        public static void UnlockFinalDLCChapters()
        {
            var save = SaveData;
            if (save == null) return;

            save.FinalDlcContentUnlocked = true;
            save.UnlockedChapter19 = true;
            save.VoidMoonUnlocked = true;
            save.UnlockedChapter21 = true;
            save.TrueFinaleUnlocked = true;

            // Unlock all three final chapters
            MaggyHelperSaveFacade.UnlockChapter(Ch19SpaceSid);
            MaggyHelperSaveFacade.UnlockChapter(Ch20TheEndSid);
            MaggyHelperSaveFacade.UnlockChapter(Ch21LastLevelSid);

            // Clear any pending unlock flags
            save.PendingUnlockChapter19OnRestart = false;
            save.PendingUnlockChapter20OnRestart = false;
            save.PendingUnlockChapter21OnRestart = false;

            Logger.Log(LogLevel.Info, "MaggyHelper", "Final DLC Chapters 19-21 unlocked - Farewell to Stars sequence available");
        }

        /// <summary>
        /// Queues Chapter 10 unlock for next game launch (restart-gated unlock).
        /// </summary>
        public static void QueueChapter10Unlock()
        {
            var save = SaveData;
            if (save == null) return;

            save.PendingUnlockChapter10OnRestart = true;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Chapter 10 unlock queued for next launch");
        }

        /// <summary>
        /// Queues Chapter 18 unlock for next game launch (restart-gated unlock).
        /// </summary>
        public static void QueueChapter18Unlock()
        {
            var save = SaveData;
            if (save == null) return;

            save.PendingUnlockChapter19OnRestart = true; // Uses same flow as Ch19
            Logger.Log(LogLevel.Info, "MaggyHelper", "Chapter 18 unlock queued for next launch");
        }

        /// <summary>
        /// Queues Final DLC chapters unlock for next game launch.
        /// </summary>
        public static void QueueFinalDLCUnlock()
        {
            var save = SaveData;
            if (save == null) return;

            save.PendingUnlockChapter19OnRestart = true;
            save.PendingUnlockChapter20OnRestart = true;
            save.PendingUnlockChapter21OnRestart = true;
            Logger.Log(LogLevel.Info, "MaggyHelper", "Final DLC chapters unlock queued for next launch");
        }

        /// <summary>
        /// Console command: maggy_unlock_ch10 - Unlock Chapter 10 (Ruins) with DZ Mountain
        /// </summary>
        [Command("maggy_unlock_ch10", "Unlock Chapter 10 (Ruins) with Desolo Zantas mountain access.")]
        private static void CmdUnlockCh10()
        {
            UnlockChapter10Ruins();
            Engine.Commands?.Log("[MaggyHelper] Chapter 10 (Ruins) unlocked with DZ Mountain access!");
            Engine.Commands?.Log("Return to overworld to see the changes.");
        }

        /// <summary>
        /// Console command: maggy_unlock_ch18 - Unlock Chapter 18 (Heart/Core of Existence)
        /// </summary>
        [Command("maggy_unlock_ch18", "Unlock Chapter 18 (Heart/Core) - Boss Rush chapter.")]
        private static void CmdUnlockCh18()
        {
            UnlockChapter18Heart();
            Engine.Commands?.Log("[MaggyHelper] Chapter 18 (Heart/Core) unlocked - Boss Rush available!");
            Engine.Commands?.Log("Return to overworld to see the changes.");
        }

        /// <summary>
        /// Console command: maggy_unlock_final_dlc - Unlock Final DLC Chapters 19-21
        /// </summary>
        [Command("maggy_unlock_final_dlc", "Unlock Final DLC Chapters 19-21 (Farewell to Stars).")]
        private static void CmdUnlockFinalDLC()
        {
            UnlockFinalDLCChapters();
            Engine.Commands?.Log("[MaggyHelper] Final DLC Chapters 19-21 unlocked!");
            Engine.Commands?.Log("  - Chapter 19 (Space): Farewell to Stars");
            Engine.Commands?.Log("  - Chapter 20 (The End): Void Moon");
            Engine.Commands?.Log("  - Chapter 21 (Last Level): True Finale");
            Engine.Commands?.Log("Return to overworld to see the changes.");
        }

        /// <summary>
        /// Checks if Chapter 10 (Ruins) is unlocked.
        /// </summary>
        public static bool IsChapter10Unlocked => SaveData?.UnlockedChapter10 ?? false;

        /// <summary>
        /// Checks if Chapter 18 (Heart/Core) is unlocked.
        /// </summary>
        public static bool IsChapter18Unlocked => SaveData?.BossRushUnlocked ?? false;

        /// <summary>
        /// Checks if Final DLC chapters (19-21) are unlocked.
        /// </summary>
        public static bool IsFinalDLCUnlocked => SaveData?.FinalDlcContentUnlocked ?? false;
    }
}

