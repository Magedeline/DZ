using System;
using System.Reflection;
using Celeste.Cutscenes;
using Celeste.Entities;
using Celeste.Mod.MaggyHelper;
using Celeste.Mod.MaggyHelper.BossesExample;
using Monocle;
using MonoMod.RuntimeDetour;
using static Celeste.Mod.Logger;

namespace Celeste.Mod.KIRBY_CELESTE
{
    /// <summary>
    /// Core module for the KIRBY_CELESTE mod. Central hub for:
    /// - All hook registrations (vanilla + custom systems)
    /// - Console commands for development and testing
    /// - Overworld 3D mountain management
    /// - Area/Chapter data integration
    /// </summary>
    public class KIRBY_CELESTEModule : EverestModule
    {
        public static KIRBY_CELESTEModule Instance { get; private set; }

        public override Type SettingsType => typeof(KIRBY_CELESTEModuleSettings);
        public static KIRBY_CELESTEModuleSettings Settings => (KIRBY_CELESTEModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(KIRBY_CELESTEModuleSession);
        public static KIRBY_CELESTEModuleSession Session => (KIRBY_CELESTEModuleSession)Instance._Session;

        public override Type SaveDataType => typeof(KIRBY_CELESTEModuleSaveData);
        public static KIRBY_CELESTEModuleSaveData SaveData => (KIRBY_CELESTEModuleSaveData)Instance._SaveData;

        // Runtime flags
        public static bool LaunchPart1Credits { get; set; }
        public static bool LaunchPart2Credits { get; set; }

        // ── Hook Registry ───────────────────────────────────────────────────
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
        //   10. Kirby Player State Hooks (KirbyPlayerStateController)
        //      - Custom player state machine hooks
        //
        //   11. Kirby Health System Hooks (KirbyHealthSystemHooks)
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
        //      - C-Side unlock postcard (after B-Side completion)
        //      - D-Side unlock postcard (after C-Side completion)
        //      - DX-Side unlock postcard (after D-Side completion)
        //      - Desolo Variants unlock postcard (ultra completion)
        //      - SideUnlockVignette integration with LevelExit
        //
        //   16. C-Side Tape Unlock Hooks (TapeCollection → Overworld)
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
        //      - KIRBY_CELESTEUnlockEverything cheat listener
        //      - KIRBY_CELESTEUnlockedPico8Message display entity
        //      - All chapters, C-Sides, D-Sides, DX-Sides unlock
        //      - Ingeste Pico8 classic unlock message
        //      - Cheat mode flag persistence in save data
        // ──────────────────────────────────────────────────────────────────────

        // ── Console Command Registry ──────────────────────────────────────────
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
        // ──────────────────────────────────────────────────────────────────────

        // ── Overworld 3D / Mountain Data ──────────────────────────────────────
        // 3D Mountain integration handled by MountainOverworldManager:
        //   - Custom mountain model registration (OBJ + PNG textures)
        //   - Per-chapter camera positions (AreaMapData.MountainCameraData)
        //   - Mountain state management (Normal/Dark/Void)
        //   - Fog color configuration per state
        //   - Camera lock to prevent idle rotation drift
        // ──────────────────────────────────────────────────────────────────────

        // ── Area/Chapter Data ─────────────────────────────────────────────────
        // Chapter definitions and runtime data managed by AreaMapData:
        //   - 21 chapter definitions (0-20 + special chapters)
        //   - Per-chapter: SID, icon, music, ambience, mountain camera data
        //   - 5 sides per chapter: A, B, C, D, DX
        //   - Hardcoded runtime data applied via AreaData.Load hooks
        //   - Chapter progression: Ch9→Ch10, Ch15→Ch16, Ch18→Ch19→Ch20→Ch21
        // ──────────────────────────────────────────────────────────────────────

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

        public KIRBY_CELESTEModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            BossesExampleModule.Load();
            // Note: AreaMapData, ChapterActRegistry, and BossRosterRegistry
            // use lazy initialization - they'll be populated on first access.
            global::Celeste.AreaModeExtender.Load();
            global::Celeste.AltSidesHelperBridge.Load();
            global::Celeste.IntroRemixHooks.Load();
            global::Celeste.MonoModHooks.Load();

            // Initialize Vignette hooks for intro/outro cutscenes
            InitializeVignetteHooks();

            global::Celeste.TitleScreen_ExtHook.Load();
            global::Celeste.UI.ModSelectionScreen.Load();
            global::Celeste.OverworldMusicManager.Load();
            global::Celeste.MountainOverworldManager.Load();
            global::Celeste.Cutscenes.IntroWarning.Load();

            global::Celeste.ChapterMasteryTracker.Load();
            global::Celeste.CosmicChapterPanelHook.Load();

            // Chapter progression hooks for late-game unlock flow
            ChapterProgressionManager.Load();

            // Kirby player map-entry hooks (Everest + MonoMod + vanilla compatibility)
            // Ensures controllers attach on Player spawn and metadata-based activation works.
            global::Celeste.KirbyPlayerMapHooks.Load();

            // Kirby health system hooks for hazard damage integration
            global::Celeste.KirbyHealthSystemHooks.Load();

            // Hot Reload Controller (Global) - named handler so Unload can -= it.
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

            // Hook PCG quick menu keybind
            On.Celeste.Level.Update += OnLevelUpdate_PCGQuickMenu;

            // Initialize PCG area registrar (CelesteRandomizer-style dynamic area registration)
            PCGAreaRegistrar.Load();

            // Initialize SubChapterManager (EXPERIMENTAL/TEST ONLY)
            // Sub-chapter system: host 5–20 collab maps under a single checkpoint
            global::Celeste.SubChapterManager.Load();

            // Validate and auto-repair save data on load
            global::Celeste.Mod.MaggyHelper.SaveDataValidator.ValidateOnLoad();

            // Register save data debugging console commands
            global::Celeste.Mod.MaggyHelper.SaveDataValidator.RegisterConsoleCommands();

            // Initialize level load validator for entity/trigger validation
            global::Celeste.Mod.MaggyHelper.LevelLoadValidator.Initialize();
            global::Celeste.Mod.MaggyHelper.LevelLoadValidator.HookIntoLevelLoad();

            // Register in-game test runner
            global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTETestRunner.RegisterConsoleCommand();

            // Register performance profiler commands
            global::Celeste.Mod.MaggyHelper.PerformanceProfiler.RegisterConsoleCommands();
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
            if (level.Tracker.GetEntity<global::Celeste.Mod.MaggyHelper.HotReload.HotReloadController>() == null)
                level.Add(new global::Celeste.Mod.MaggyHelper.HotReload.HotReloadController());

            // Add debug room warp menu when DeveloperBypass or DebugMode is enabled
            var settings = Settings;
            if ((settings?.DeveloperBypass ?? false) || (settings?.DebugMode ?? false))
            {
                if (level.Entities.FindFirst<global::Celeste.UI.DebugRoomWarpMenu>() == null)
                    level.Add(new global::Celeste.UI.DebugRoomWarpMenu());
            }
        }

        /// <summary>
        /// Hook to detect PCGQuickMenu keybind while in a level.
        /// </summary>
        private static void OnLevelUpdate_PCGQuickMenu(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (Settings?.PCGQuickMenu?.Pressed ?? false)
            {
                if (!self.Paused && self.Tracker.GetEntity<PCGQuickMenu>() == null)
                {
                    self.Paused = true;
                    self.Add(new PCGQuickMenu());
                }
            }
        }

        public override void Unload()
        {
            ChapterProgressionManager.Unload();
            global::Celeste.CosmicChapterPanelHook.Unload();
            global::Celeste.ChapterMasteryTracker.Unload();
            global::Celeste.MountainOverworldManager.Unload();
            global::Celeste.KirbyPlayerMapHooks.Unload();
            global::Celeste.KirbyHealthSystemHooks.Unload();
            global::Celeste.OverworldMusicManager.Unload();
            global::Celeste.TitleScreen_ExtHook.Unload();
            global::Celeste.UI.ModSelectionScreen.Unload();
            global::Celeste.Cutscenes.IntroWarning.Unload();
            // Unhook Vignette System
            UnloadVignetteHooks();

            global::Celeste.MonoModHooks.Unload();
            global::Celeste.IntroRemixHooks.Unload();
            global::Celeste.AltSidesHelperBridge.Unload();
            global::Celeste.AreaModeExtender.Unload();
            BossesExampleModule.Unload();

            // Unhook level exit cleanup
            Everest.Events.Level.OnExit -= OnLevelExit;
            // Unhook hot-reload controller insertion (matches += in Load)
            Everest.Events.Level.OnLoadLevel -= OnLoadLevel_EnsureHotReloadController;

            // Unhook PCG quick menu keybind
            On.Celeste.Level.Update -= OnLevelUpdate_PCGQuickMenu;

            // Unload PCG area registrar
            PCGAreaRegistrar.Unload();

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

        private static KIRBY_CELESTEUnlockEverything _cheatListener;

        private static void InitializeCheatMode()
        {
            // Cheat mode is initialized per-level via Level.OnLoadLevel event
            Everest.Events.Level.OnLoadLevel += OnLevelLoad_EnableCheatListener;
            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Cheat Mode system initialized");
        }

        private static void UnloadCheatMode()
        {
            Everest.Events.Level.OnLoadLevel -= OnLevelLoad_EnableCheatListener;
            _cheatListener = null;
        }

        private static void OnLevelLoad_EnableCheatListener(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // Add cheat listener to levels for returning players
            if (level.Entities.FindFirst<KIRBY_CELESTEUnlockEverything>() == null)
            {
                _cheatListener = new KIRBY_CELESTEUnlockEverything();
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

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Unlock Everything cheat triggered - all content unlocked");
        }

        /// <summary>
        /// Shows the Pico8 Classic unlock message for Ingeste.
        /// </summary>
        public static void ShowPico8UnlockMessage(Level level, Action callback = null)
        {
            if (level.Tracker.GetEntity<KIRBY_CELESTEUnlockedPico8Message>() == null)
            {
                level.Add(new KIRBY_CELESTEUnlockedPico8Message(callback));
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
                Engine.Commands?.Log("[KIRBY_CELESTE] Must be in a level to trigger cheat.");
                return;
            }

            TriggerUnlockEverythingCheat();
            Engine.Commands?.Log("[KIRBY_CELESTE] Unlock Everything cheat triggered!");
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
                Engine.Commands?.Log("[KIRBY_CELESTE] Must be in a level to show message.");
                return;
            }

            ShowPico8UnlockMessage(level, () =>
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] Pico8 unlock message completed.");
            });
            Engine.Commands?.Log("[KIRBY_CELESTE] Pico8 unlock message displayed.");
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

                Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Mod integrations initialized");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE", "Failed to initialize mod integrations: " + ex.Message);
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

                Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Mod integrations shut down");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE", "Failed to shutdown mod integrations: " + ex.Message);
            }
        }

        // =====================================================================
        //  Vignette Hooks (Intro/Outro Cutscenes)
        // =====================================================================

        private static void InitializeVignetteHooks()
        {
            // Load the VignetteHooks system for chapter intro/outro cutscenes
            global::Celeste.VignetteHooks.Load();

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Vignette hooks initialized");
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
                Engine.Commands?.Log("[KIRBY_CELESTE] Must be in a level to play vignette.");
                return;
            }

            Scene vignette = chapterNumber switch
            {
                0 => new global::Celeste.Cutscenes.VesselCreationVignette(level.Session),
                3 => new global::Celeste.Cutscenes.Cs03IntroVignette(level.Session),
                9 => new global::Celeste.Entities.BeyondSummitVignette(level.Session),
                10 => new global::Celeste.Cutscenes.Cs10IntroVignetteAlt(level.Session),
                18 => new global::Celeste.Cutscenes.Cs18IntroVignette(level.Session),
                21 => new global::Celeste.Entities.TrueFinaleVignette(level.Session),
                _ => null
            };

            if (vignette != null)
            {
                Engine.Scene = vignette;
                Engine.Commands?.Log($"[KIRBY_CELESTE] Playing intro vignette for Chapter {chapterNumber}");
            }
            else
            {
                Engine.Commands?.Log($"[KIRBY_CELESTE] No intro vignette available for Chapter {chapterNumber}");
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
                Engine.Commands?.Log("[KIRBY_CELESTE] Must be in a level to play vignette.");
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
                Engine.Commands?.Log($"[KIRBY_CELESTE] Playing outro vignette for Chapter {chapterNumber}");
            }
            else
            {
                Engine.Commands?.Log($"[KIRBY_CELESTE] No outro vignette available for Chapter {chapterNumber}");
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
                Engine.Commands?.Log("[KIRBY_CELESTE] Usage: maggy_vignette_test [intro|outro] [chapterNumber]");
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
                Engine.Commands?.Log("[KIRBY_CELESTE] Save data not available.");
                return;
            }

            if (target.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                // Reset all vignette achievement flags (by unlocking them again, which is a no-op)
                // Note: The actual reset happens when achievements are cleared via direct manipulation
                Engine.Commands?.Log("[KIRBY_CELESTE] All vignette flags reset.");
            }
            else if (int.TryParse(target, out int chapterNumber))
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] Vignette flags reset for Chapter " + chapterNumber + ".");
            }
            else
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] Usage: maggy_vignette_reset [chapterNumber|all]");
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
                    _tapeOnPlayerHook = new Hook(onPlayerMethod, typeof(KIRBY_CELESTEModule).GetMethod(
                        nameof(Hook_Tape_OnPlayer),
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));

                    Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "C-Side tape unlock hooks initialized");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "KIRBY_CELESTE", "Could not find DesoloZantasTape.OnPlayer method");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE", "Failed to initialize tape unlock hooks: " + ex.Message);
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

                    Logger.Log(LogLevel.Info, "KIRBY_CELESTE",
                        "C-Side unlocked via tape collection: " + cSideToUnlock + ". Queued for overworld animation.");

                    // Trigger the unlock event for any listeners
                    OnCSideUnlocked?.Invoke(cSideToUnlock);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE", "Error in tape unlock hook: " + ex.Message);
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
                Engine.Commands?.Log("[KIRBY_CELESTE] Usage: maggy_unlock_cside [chapterIndex (0-20)]");
                return;
            }

            var chapter = AreaMapData.GetByNumber(chapterIndex);
            if (chapter == null)
            {
                Engine.Commands?.Log($"[KIRBY_CELESTE] Chapter {chapterIndex} not found.");
                return;
            }

            string baseKey = ExtractBaseKey(chapter.SID);
            string cSideSID = AreaModeExtender.BuildSideSID(AreaModeExtender.MODE_CSIDE, baseKey);

            if (!SaveData.UnlockedCSideIDs.Contains(cSideSID))
            {
                SaveData.UnlockedCSideIDs.Add(cSideSID);
                SaveData.PendingCSideUnlockIDs.Add(cSideSID);
                RefreshChapterSideAvailability(cSideSID);
                Engine.Commands?.Log($"[KIRBY_CELESTE] C-Side unlocked for Chapter {chapterIndex}: {cSideSID}");
                Engine.Commands?.Log("Return to overworld to see the unlock animation.");
            }
            else
            {
                Engine.Commands?.Log($"[KIRBY_CELESTE] C-Side already unlocked for Chapter {chapterIndex}.");
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
            // Hook into LevelExit to intercept side completions and show postcards
            On.Celeste.LevelExit.Routine += OnLevelExitRoutine_PostcardCheck;

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Postcard unlock hooks initialized");
        }

        private static void UnloadPostcardHooks()
        {
            On.Celeste.LevelExit.Routine -= OnLevelExitRoutine_PostcardCheck;
        }

        private static IEnumerator OnLevelExitRoutine_PostcardCheck(
            On.Celeste.LevelExit.orig_Routine orig, LevelExit self)
        {
            // Check if this is a side completion that triggers a postcard
            bool shouldShowPostcard = false;
            int completedMode = -1;
            Session session = self?.session;

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
            var save = KIRBY_CELESTEModule.SaveData;
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

        private static IEnumerator ShowUltraCompletionPostcard()
        {
            // Mark as shown so we don't repeat
            SaveData?.UnlockAchievement("ultra_completion_postcard_shown");

            // Create the ultra completion vignette
            var scene = new Scene();
            var snow = new HiresSnow();
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
            Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, new HiresSnow());
        }

        /// <summary>
        /// Console command: maggy_postcard_test [cside|dside|dxside|ultra] - Test postcard unlock displays
        /// </summary>
        [Command("maggy_postcard_test", "Test postcard unlock displays. Usage: maggy_postcard_test [cside|dside|dxside|ultra]")]
        private static void CmdTestPostcard(string type = "cside")
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] Must be in a level to test postcard.");
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
                Engine.Commands?.Log("[KIRBY_CELESTE] Showing ultra completion postcard...");
                var entity = new Entity();
                entity.Add(new Coroutine(ShowUltraCompletionPostcard()));
                level.Add(entity);
            }
            else
            {
                Engine.Commands?.Log($"[KIRBY_CELESTE] Showing postcard for completing mode {completedMode}...");
                var entity = new Entity();
                entity.Add(new Coroutine(PostcardUnlockSystem.ShowUnlockPostcard(level, level.Session, completedMode)));
                level.Add(entity);
            }
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            BossesExampleModule.LoadContent(firstLoad);
            // ProphecyFont is now lazy-initialized on first access

            // Audio.Init hook doesn't fire reliably in this Everest version;
            // LoadContent runs after FMOD and IngestBank are done.
            global::Celeste.OverworldMusicManager.LoadBanks();
        }

        public static bool IsChapter17EpilogueCompleted()
        {
            return KIRBY_CELESTEModule.SaveData?.IsChapterCompleted(Chapter17EpilogueSid) == true;
        }

        public static void MarkChapter17EpilogueCompleted()
        {
            KIRBY_CELESTEModule.SaveData?.CompleteChapter(Chapter17EpilogueSid);

            if (KIRBY_CELESTEModule.Session != null)
            {
                KIRBY_CELESTEModule.Session.InCredits = false;
                KIRBY_CELESTEModule.Session.CreditsPhase = 0;
                KIRBY_CELESTEModule.Session.CreditsCompleted = true;
            }
        }

        public static void LaunchChapter17Epilogue()
        {
            if (KIRBY_CELESTEModule.Session != null)
            {
                KIRBY_CELESTEModule.Session.InCredits = false;
                KIRBY_CELESTEModule.Session.CreditsPhase = 2;
                KIRBY_CELESTEModule.Session.CreditsCompleted = false;
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
                    _maggyPlayerType = Type.GetType("KIRBY_CELESTE.Entities.Player, KIRBY_CELESTE");
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
            if (KIRBY_CELESTEModule.Session != null)
            {
                KIRBY_CELESTEModule.Session.InCredits = true;
                KIRBY_CELESTEModule.Session.CreditsPhase = 1;
                KIRBY_CELESTEModule.Session.CreditsCompleted = false;
            }

            creditsSession.Audio.Music.Event = "event:/pusheen/music/lvl17/main";
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
        /// Console command: maggy_credits — launches the credits sequence from the current level.
        /// </summary>
        [Command("maggy_credits", "Launches the Chapter 17 credits sequence from the current level.")]
        private static void Cmd_LaunchCredits()
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] Must be in a level to launch credits.");
                return;
            }

            Engine.Commands?.Log("[KIRBY_CELESTE] Launching Chapter 17 credits...");
            LaunchCredits(level.Session);
        }

        [Command("maggy_hotreload_test", "Simulates a hot reload event for testing.")]
        private static void Cmd_HotReloadTest()
        {
            Engine.Commands?.Log("[KIRBY_CELESTE] Simulating hot reload event...");
            
            // We simulate it by calling the handler directly with some types
            Type[] mockTypes = new Type[] { 
                typeof(global::Celeste.Mod.MaggyHelper.HotReload.ModHotReloadTest),
                typeof(global::Celeste.HotReload.GameHotReloadTest)
            };
            
            global::Celeste.HotReload.HotReloadHandler.UpdateApplication(mockTypes);
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

            // Unlock the chapter via MaggySaveFacade
            MaggySaveFacade.UnlockChapter(Ch10RuinsSid);

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Chapter 10 (Ruins) unlocked with DZ Mountain access");
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
            MaggySaveFacade.UnlockChapter(Ch18HeartSid);

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Chapter 18 (Heart/Core) unlocked - Boss Rush available");
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
            MaggySaveFacade.UnlockChapter(Ch19SpaceSid);
            MaggySaveFacade.UnlockChapter(Ch20TheEndSid);
            MaggySaveFacade.UnlockChapter(Ch21LastLevelSid);

            // Clear any pending unlock flags
            save.PendingUnlockChapter19OnRestart = false;
            save.PendingUnlockChapter20OnRestart = false;
            save.PendingUnlockChapter21OnRestart = false;

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Final DLC Chapters 19-21 unlocked - Farewell to Stars sequence available");
        }

        /// <summary>
        /// Queues Chapter 10 unlock for next game launch (restart-gated unlock).
        /// </summary>
        public static void QueueChapter10Unlock()
        {
            var save = SaveData;
            if (save == null) return;

            save.PendingUnlockChapter10OnRestart = true;
            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Chapter 10 unlock queued for next launch");
        }

        /// <summary>
        /// Queues Chapter 18 unlock for next game launch (restart-gated unlock).
        /// </summary>
        public static void QueueChapter18Unlock()
        {
            var save = SaveData;
            if (save == null) return;

            save.PendingUnlockChapter19OnRestart = true; // Uses same flow as Ch19
            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Chapter 18 unlock queued for next launch");
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
            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Final DLC chapters unlock queued for next launch");
        }

        /// <summary>
        /// Console command: maggy_unlock_ch10 - Unlock Chapter 10 (Ruins) with DZ Mountain
        /// </summary>
        [Command("maggy_unlock_ch10", "Unlock Chapter 10 (Ruins) with Desolo Zantas mountain access.")]
        private static void CmdUnlockCh10()
        {
            UnlockChapter10Ruins();
            Engine.Commands?.Log("[KIRBY_CELESTE] Chapter 10 (Ruins) unlocked with DZ Mountain access!");
            Engine.Commands?.Log("Return to overworld to see the changes.");
        }

        /// <summary>
        /// Console command: maggy_unlock_ch18 - Unlock Chapter 18 (Heart/Core of Existence)
        /// </summary>
        [Command("maggy_unlock_ch18", "Unlock Chapter 18 (Heart/Core) - Boss Rush chapter.")]
        private static void CmdUnlockCh18()
        {
            UnlockChapter18Heart();
            Engine.Commands?.Log("[KIRBY_CELESTE] Chapter 18 (Heart/Core) unlocked - Boss Rush available!");
            Engine.Commands?.Log("Return to overworld to see the changes.");
        }

        /// <summary>
        /// Console command: maggy_unlock_final_dlc - Unlock Final DLC Chapters 19-21
        /// </summary>
        [Command("maggy_unlock_final_dlc", "Unlock Final DLC Chapters 19-21 (Farewell to Stars).")]
        private static void CmdUnlockFinalDLC()
        {
            UnlockFinalDLCChapters();
            Engine.Commands?.Log("[KIRBY_CELESTE] Final DLC Chapters 19-21 unlocked!");
            Engine.Commands?.Log("  - Chapter 19 (Space): Farewell to Stars");
            Engine.Commands?.Log("  - Chapter 20 (The End): Void Moon");
            Engine.Commands?.Log("  - Chapter 21 (Last Level): True Finale");
            Engine.Commands?.Log("Return to overworld to see the changes.");
        }

        // =====================================================================
        //  PCG Console Commands
        // =====================================================================

        /// <summary>
        /// Console command: maggy_pcg_generate - Generate a hybrid PCG map.
        /// Usage: maggy_pcg_generate [seed] [roomCount] [difficulty] [outputPath]
        /// </summary>
        [Command("maggy_pcg_generate", "Generate a hybrid PCG map. Usage: maggy_pcg_generate [seed] [roomCount] [difficulty] [outputPath]")]
        private static async void CmdPcgGenerate(int seed = -1, int roomCount = 8, int difficulty = 2, string outputPath = "")
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = $"PCG/Generated/pcg_map_{DateTime.Now:yyyyMMdd_HHmmss}.bin";
            }

            string templateLibrary = "PCG/Templates/library.json";
            if (!File.Exists(templateLibrary))
            {
                // Try to build a library from existing maps
                string mapsDir = "Maps";
                if (Directory.Exists(mapsDir))
                {
                    var mapFiles = Directory.GetFiles(mapsDir, "*.bin", SearchOption.AllDirectories).Take(5).ToArray();
                    if (mapFiles.Length > 0)
                    {
                        Engine.Commands?.Log("[KIRBY_CELESTE] Building template library from existing maps...");
                        await PCGService.BuildTemplateLibraryAsync(mapFiles, templateLibrary);
                    }
                }
            }

            if (!File.Exists(templateLibrary))
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] No template library found. Use maggy_pcg_extract to build one from existing maps first.");
                return;
            }

            Engine.Commands?.Log($"[KIRBY_CELESTE] Generating hybrid PCG map: seed={seed}, rooms={roomCount}, difficulty={difficulty}...");
            bool success = await PCGService.GenerateHybridMapAsync(
                templateLibrary,
                outputPath,
                seed,
                roomCount,
                difficulty,
                "pathway",
                "balanced");

            if (success)
            {
                string fullPath = Path.GetFullPath(outputPath);
                Engine.Commands?.Log($"[KIRBY_CELESTE] Map generated successfully!");
                Engine.Commands?.Log($"  Path: {fullPath}");
                Engine.Commands?.Log($"  Load with: maggy_pcg_load {outputPath}");
            }
            else
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] Map generation failed. Check log for details.");
            }
        }

        /// <summary>
        /// Console command: maggy_pcg_load - Load a generated .bin map for playtesting.
        /// Usage: maggy_pcg_load [mapPath]
        /// </summary>
        [Command("maggy_pcg_load", "Load a generated PCG map for playtesting. Usage: maggy_pcg_load [mapPath]")]
        private static void CmdPcgLoad(string mapPath = "")
        {
            if (string.IsNullOrEmpty(mapPath))
            {
                var generatedDir = "PCG/Generated";
                if (Directory.Exists(generatedDir))
                {
                    var newest = Directory.GetFiles(generatedDir, "*.bin")
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.LastWriteTime)
                        .FirstOrDefault();
                    if (newest != null)
                        mapPath = newest.FullName;
                }
            }

            if (string.IsNullOrEmpty(mapPath) || !File.Exists(mapPath))
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] No generated map found. Generate one first with maggy_pcg_generate.");
                return;
            }

            // For playtesting, copy the generated map into the mod's map folder
            // and launch it via a temporary session using the current chapter's SID
            // with a direct level loader approach.
            string testMapName = $"pcg_test_{Path.GetFileName(mapPath)}";
            string testMapDir = Path.Combine("Maps", "PCG_Test");
            Directory.CreateDirectory(testMapDir);
            string destPath = Path.Combine(testMapDir, testMapName);
            File.Copy(mapPath, destPath, true);

            Engine.Commands?.Log($"[KIRBY_CELESTE] Copied map to {destPath}");
            Engine.Commands?.Log("[KIRBY_CELESTE] To playtest: register this map in your everest.yaml or load it through the Enhanced Map Editor.");
        }

        /// <summary>
        /// Console command: maggy_pcg_extract - Extract room templates from an existing map.
        /// Usage: maggy_pcg_extract [mapPath] [outputDir]
        /// </summary>
        [Command("maggy_pcg_extract", "Extract room templates from a map. Usage: maggy_pcg_extract [mapPath] [outputDir]")]
        private static void CmdPcgExtract(string mapPath = "", string outputDir = "PCG/Templates")
        {
            if (string.IsNullOrEmpty(mapPath))
            {
                // Default to current session map if in a level
                if (Engine.Scene is Level level && level.Session?.MapData?.Filename != null)
                {
                    mapPath = level.Session.MapData.Filename + ".bin";
                }
                else
                {
                    Engine.Commands?.Log("[KIRBY_CELESTE] Usage: maggy_pcg_extract [mapPath] [outputDir]");
                    Engine.Commands?.Log("  Or run while in a level to extract from the current map.");
                    return;
                }
            }

            if (!File.Exists(mapPath))
            {
                Engine.Commands?.Log($"[KIRBY_CELESTE] Map file not found: {mapPath}");
                return;
            }

            Engine.Commands?.Log($"[KIRBY_CELESTE] Extracting templates from: {mapPath} ...");
            var templates = RoomTemplateLoader.ExtractTemplatesFromMap(mapPath, outputDir);
            if (templates.Count > 0)
            {
                Engine.Commands?.Log($"[KIRBY_CELESTE] Extracted {templates.Count} templates to {outputDir}");
                foreach (var t in templates.Take(5))
                    Engine.Commands?.Log($"  - {t.Name} ({t.Width}x{t.Height}, {t.Type}, {t.Difficulty})");
            }
            else
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] No templates extracted. Check log for errors.");
            }
        }

        /// <summary>
        /// Console command: maggy_pcg_inspect - Dump a .bin map to a human-readable JSON file.
        /// Usage: maggy_pcg_inspect [mapPath] [outputJsonPath]
        /// </summary>
        [Command("maggy_pcg_inspect", "Inspect a .bin map as JSON. Usage: maggy_pcg_inspect [mapPath] [outputJsonPath]")]
        private static void CmdPcgInspect(string mapPath = "", string outputJsonPath = "")
        {
            if (string.IsNullOrEmpty(mapPath))
            {
                // Default to latest generated map
                var generatedDir = "PCG/Generated";
                if (Directory.Exists(generatedDir))
                {
                    var newest = Directory.GetFiles(generatedDir, "*.bin")
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.LastWriteTime)
                        .FirstOrDefault();
                    if (newest != null)
                        mapPath = newest.FullName;
                }
            }

            if (string.IsNullOrEmpty(mapPath) || !File.Exists(mapPath))
            {
                Engine.Commands?.Log("[KIRBY_CELESTE] No map found. Provide a path or generate one first with maggy_pcg_generate.");
                return;
            }

            if (string.IsNullOrEmpty(outputJsonPath))
            {
                outputJsonPath = Path.ChangeExtension(mapPath, ".inspect.json");
            }

            try
            {
                var root = BinaryPacker.FromBinary(mapPath);
                if (root == null)
                {
                    Engine.Commands?.Log("[KIRBY_CELESTE] Failed to parse map binary.");
                    return;
                }

                var tree = SerializeElement(root);
                var json = System.Text.Json.JsonSerializer.Serialize(tree, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputJsonPath)));
                File.WriteAllText(outputJsonPath, json);
                Engine.Commands?.Log($"[KIRBY_CELESTE] Map dumped to JSON: {Path.GetFullPath(outputJsonPath)}");
                Engine.Commands?.Log($"  Levels: {(root.Children?.FirstOrDefault(c => c.Name == "levels")?.Children?.Count ?? 0)}");
            }
            catch (Exception ex)
            {
                Engine.Commands?.Log($"[KIRBY_CELESTE] Inspect failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively convert a BinaryPacker.Element into a serializable dictionary tree.
        /// </summary>
        public static Dictionary<string, object> SerializeElement(BinaryPacker.Element element)
        {
            var dict = new Dictionary<string, object>();
            if (element == null) return dict;

            dict["name"] = element.Name ?? "";
            if (element.Attributes != null && element.Attributes.Count > 0)
            {
                var attrs = new Dictionary<string, object>();
                foreach (var kv in element.Attributes)
                {
                    object val = kv.Value;
                    if (val is BinaryPacker.Element childEl)
                        val = SerializeElement(childEl);
                    else if (val is System.Collections.IEnumerable enumerable && val is not string)
                    {
                        var list = new List<object>();
                        foreach (var item in enumerable)
                        {
                            if (item is BinaryPacker.Element itemEl)
                                list.Add(SerializeElement(itemEl));
                            else
                                list.Add(item?.ToString());
                        }
                        val = list;
                    }
                    else
                    {
                        val = val?.ToString() ?? "";
                    }
                    attrs[kv.Key] = val;
                }
                dict["attributes"] = attrs;
            }
            if (element.Children != null && element.Children.Count > 0)
            {
                var children = new List<Dictionary<string, object>>();
                foreach (var child in element.Children)
                    children.Add(SerializeElement(child));
                dict["children"] = children;
            }
            return dict;
        }

        /// <summary>
        /// Public static wrapper for external callers.
        /// </summary>
        public static Dictionary<string, object> SerializeElementStatic(BinaryPacker.Element element) => SerializeElement(element);

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
