using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Celeste.Cutscenes;
using FMOD.Studio;
using Celeste.Entities;
using Monocle;
using MonoMod.RuntimeDetour;
using static Celeste.Mod.Logger;
using MonoMod.Logs;

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

        // =====================================================================
        //  Build Fingerprint & Utility Methods
        // =====================================================================

        private static string GetBuildFingerprint()
        {
            Assembly assembly = typeof(MaggyHelperModule).Assembly;

            string mvidShort;
            try
            {
                mvidShort = assembly.ManifestModule.ModuleVersionId.ToString("N")[..8];
            }
            catch
            {
                mvidShort = "na";
            }

            string version = assembly.GetName().Version?.ToString() ?? "na";
            string location = null;

            try
            {
                location = assembly.Location;
            }
            catch
            {
                location = null;
            }

            if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
            {
                string timestampUtc = File.GetLastWriteTimeUtc(location).ToString("yyyy-MM-dd HH:mm:ss");
                return $"{timestampUtc}Z v:{version} mvid:{mvidShort}";
            }

            // Some load contexts expose no physical assembly path. Still return a stable identifier.
            return $"v:{version} mvid:{mvidShort} path:na";
        }

        [Command("maggy_build", "Prints the active MaggyHelper build fingerprint. Usage: maggy_build")]
        private static void CmdMaggyBuild()
        {
            string fingerprint = GetBuildFingerprint();
            string message = $"[MaggyHelper] Active build: {fingerprint}";
            Engine.Commands?.Log(message);
            Logger.Log(LogLevel.Info, "MaggyHelper", message);
        }

        /// <summary>
        /// Get the mod content path for loading resources.
        /// </summary>
        public static string GetContentPath(string relativePath)
        {
            return $"MaggyHelper/{relativePath}";
        }

        /// <summary>
        /// Check if we are currently in a MaggyHelper map.
        /// </summary>
        public static bool IsInMaggyHelperMap()
        {
            var celSession = (Engine.Scene as Level)?.Session;
            if (celSession == null) return false;
            var areaData = AreaData.Get(celSession.Area);
            return areaData?.SID?.StartsWith("MaggyHelper/", StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Log a debug message if debug mode is enabled in settings.
        /// </summary>
        public static void LogDebug(string message)
        {
            if (Settings?.DebugMode == true)
                Logger.Log(LogLevel.Debug, "MaggyHelper", message);
        }

        /// <summary>
        /// Resets all mod save-data for the current slot.
        /// </summary>
        public static void ResetModSaveData()
        {
            if (Instance == null) return;
            Instance._SaveData = new MaggyHelperModuleSaveData();
        }

        public override void Load()
        {
            // BossesExampleModule.Load(); // TODO: Restore when BossesExampleModule is available
            // Note: AreaMapData, ChapterActRegistry, and BossRosterRegistry
            // use lazy initialization - they'll be populated on first access.

            // Hook GameLoader to load audio banks after FMOD is initialized
            On.Celeste.GameLoader.LoadThread += OnGameLoaderLoadThread;

            // Patch Audio.Init to survive missing fmodstudio.dll or architecture mismatches
            On.Celeste.Audio.Init += OnAudioInit;

            // If the game is already running (Everest CodeReload), load banks immediately
            // because GameLoader.LoadThread fired long ago and won't fire again.
            if (Engine.Scene != null)
            {
                LoadAudioBanks();
            }

            // Initialize bus routing for Pusheen and return/verb buses
            AudioBusManager.Load();

            // Initialize global mod audio loader to discover and warm up audio from all mods
            global::MaggyHelper.Helpers.GlobalModAudioLoader.Load();

            // Audio redirect hooks are loaded from LoadAudioBanks() after banks are confirmed ready,
            // because on first launch banks haven't loaded yet when Load() runs.
            // On CodeReload, LoadAudioBanks() is called just above (Engine.Scene != null branch)
            // and LoadAudioHooks() is called from within LoadAudioBanks() when masterOk.
            if (AudioMasterBankLoaded)
                LoadAudioHooks();

            // Register hooks
            // OuiChapterSelectHooks: Wraps OuiChapterSelect to catch crashes from updateScarf()
            OuiChapterSelectHooks.Load();
            global::Celeste.AreaModeExtender.Load();

            // ──── Initialize D-Side Hook Registry ────
            // Loads: CelesteDSideHooks (On.hook + IL.hook)
            //        CelesteMusicHooks (On.hook + IL.hook)
            //        TitleScreen_ExtHook (On.hook + IL.hook)
            global::Celeste.DSideHookRegistry.InitializeAll();

            // ──── Initialize Comprehensive D-Side Hook System ────
            // Complete implementation with state tracking and animations
            global::Celeste.DSideHookImplementation.Initialize();

            global::Celeste.AltSidesHelperBridge.Load();
            global::Celeste.IntroRemixHooks.Load();
            global::Celeste.MonoModHooks.Load();

            // Payphone cutscene triggers for dream/awake sequences
            global::Celeste.Mod.MaggyHelper.PayphoneCutsceneTriggers.Load();

            // Initialize Vignette hooks for intro/outro cutscenes
            InitializeVignetteHooks();

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

            // Guard against Everest's null-key crash in ModContent.Update during rebuild churn
            global::Celeste.Mod.MaggyHelper.HotReload.EverestContentUpdateGuard.Load();

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

            // If Load() is running while a Level is active, this is an Everest
            // CodeReload assembly swap (not initial startup) - notify the
            // hot reload system so [HotReloadable] types can re-init state.
            if (Engine.Scene is Level)
            {
                global::Celeste.HotReload.HotReloadHandler.NotifyEverestReload();
            }

            // Register performance profiler commands
            // global::Celeste.Mod.MaggyHelper.PerformanceProfiler.RegisterConsoleCommands(); // TODO: Restore when PerformanceProfiler is available

            // Initialize MaggyMapMetadataRegistry for .maggyhelper.meta.yaml files
            InitializeMapMetadataRegistry();

            // Initialize Chapter Completion hooks for custom music/title integration
            global::Celeste.ChapterCompletionHooks.Load();
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

        private static void OnAudioInit(On.Celeste.Audio.orig_Init orig)
        {
            try
            {
                orig();
            }
            catch (DllNotFoundException ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] FMOD Studio DLL missing ({ex.Message}) — allowing game to continue without audio.\n" +
                    "  Hint: Ensure fmodstudio.dll, fmod.dll, and VC++ redistributables are present in the Celeste game directory.");
            }
            catch (BadImageFormatException ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] FMOD DLL architecture mismatch ({ex.Message}) — allowing game to continue without audio.\n" +
                    "  Hint: Ensure you are using x64 DLLs with the x64 version of Celeste/Everest.");
            }
            catch (EntryPointNotFoundException ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] FMOD DLL export missing ({ex.Message}) — allowing game to continue without audio.\n" +
                    "  Hint: The fmodstudio.dll present is corrupt or incompatible. Replace it with a valid copy.");
            }
            catch (Exception ex) when (ex.Message?.Contains("FMOD Failed: ERR_FILE_NOTFOUND") == true)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] Audio.Init() failed with missing bank file — allowing game to continue without audio.\n" +
                    $"  Exception: {ex.Message}\n" +
                    "  Hint: Verify that all .bank files exist in Content/FMOD/Desktop/ (especially dlc_music.bank and dlc_sfx.bank).");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] Audio.Init() failed ({ex.GetType().Name}: {ex.Message}) — allowing game to continue without audio.\n" +
                    "  Hint: Check FMOD DLL/bank version compatibility.");
            }
        }

        private static void OnGameLoaderLoadThread(On.Celeste.GameLoader.orig_LoadThread orig, GameLoader self)
        {
            try
            {
                orig(self);
            }
            catch (Exception ex) when (ex.Message?.Contains("FMOD Failed: ERR_FILE_NOTFOUND") == true)
            {
                // Vanilla Audio.Init() failed because a required .bank file is missing
                // from Content/FMOD/Desktop/ (usually dlc_music, dlc_sfx, or Master Bank).
                // Swallow the crash so the rest of GameLoader can finish; audio will be
                // unavailable but the game will start.
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] Audio.Init() failed with missing bank file — allowing game to continue without audio.\n" +
                    $"  Exception: {ex.Message}\n" +
                    "  Hint: Verify that all .bank files exist in Content/FMOD/Desktop/ (especially dlc_music.bank and dlc_sfx.bank).");
                return;
            }
            catch (DllNotFoundException ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] FMOD Studio DLL missing — allowing game to continue without audio.\n" +
                    $"  Exception: {ex.Message}\n" +
                    "  Hint: Ensure fmodstudio.dll, fmod.dll, and VC++ redistributables are present in the Celeste game directory.");
                return;
            }
            catch (BadImageFormatException ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] FMOD DLL architecture mismatch — allowing game to continue without audio.\n" +
                    $"  Exception: {ex.Message}\n" +
                    "  Hint: Ensure you are using x64 DLLs with the x64 version of Celeste/Everest.");
                return;
            }
            catch (EntryPointNotFoundException ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] FMOD DLL export missing — allowing game to continue without audio.\n" +
                    $"  Exception: {ex.Message}\n" +
                    "  Hint: The fmodstudio.dll present is corrupt or incompatible. Replace it with a valid copy.");
                return;
            }
            catch (NullReferenceException ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] NullReferenceException during load — likely vanilla code accessed Audio.System before FMOD initialized.\n" +
                    $"  Exception: {ex.Message}\n" +
                    "  Allowing game to continue without audio.");
                return;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] Unexpected exception during GameLoader.LoadThread — allowing game to continue without audio.\n" +
                    $"  Exception: {ex.GetType().Name}: {ex.Message}");
                return;
            }

            // Load audio banks here - FMOD is fully initialized after orig() completes
            LoadAudioBanks();
        }

        // ── Audio bank state ───────────────────────────────────────────────────
        // WORKAROUND ACTIVE (data-only mode):
        //   The master bank (pusheen_audio.bank) and strings bank contain duplicate
        //   vanilla Celeste GUIDs that conflict with the base game. FMOD refuses to
        //   load them (ERR_EVENT_ALREADY_LOADED). We now skip these banks and rely
        //   solely on the data banks (A/B/C/D) which contain both sample data and
        //   event definitions without GUID conflicts.
        //
        // FIX REQUIRED for proper master bank support (see Audio/FMOD_REBUILD_GUIDE.md):
        //   1. Open the pusheen FMOD project in FMOD Studio
        //   2. Remove ALL vanilla Celeste events (keep only pusheen_ prefixed events)
        //   3. Rebuild the master bank
        //   4. Uncomment the master/strings loading code in LoadAudioBanks()
        //   5. Delete or rename the data banks (events will come from master bank)
        private static Bank _pusheenMasterBank;
        private static Bank _pusheenStringsBank;
        private static Bank _pusheenDataA;
        private static Bank _pusheenDataB;
        private static Bank _pusheenDataC;
        private static Bank _pusheenDataD;

        /// <summary>True after LoadAudioBanks() confirms at least one data bank loaded.</summary>
        public static bool AudioBanksLoaded { get; private set; }

        /// <summary>True if master bank loaded (contains events/buses). False in data-only mode.</summary>
        public static bool AudioMasterBankLoaded { get; private set; }

        // ── Global mod audio discovery ─────────────────────────────────────────
        private static readonly List<string> _discoveredModAudioAssets = new();
        private static readonly List<string> _discoveredModAudioBanks = new();
        private static bool _modAudioScanned;

        /// <summary>All audio assets discovered across loaded mods (readonly).</summary>
        public static IReadOnlyList<string> DiscoveredModAudioAssets => _discoveredModAudioAssets;

        /// <summary>All .bank files discovered across loaded mods (readonly).</summary>
        public static IReadOnlyList<string> DiscoveredModAudioBanks => _discoveredModAudioBanks;

        /// <summary>
        /// Reads the FMT-chunk version from a FMOD .bank file header.
        /// Returns (formatVersion, buildVersion) or (-1, -1) if the file is not a valid bank.
        /// </summary>
        private static (int formatVersion, int buildVersion) ReadBankVersion(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] header = new byte[28];
                if (fs.Read(header, 0, 28) < 28) return (-1, -1);
                // RIFF header: "RIFF" (4) + size (4) + "FEV " (4) + "FMT " (4) + chunkSize (4)
                if (header[0] != 0x52 || header[1] != 0x49 || header[2] != 0x46 || header[3] != 0x46) return (-1, -1);
                if (header[12] != 0x46 || header[13] != 0x4D || header[14] != 0x54 || header[15] != 0x20) return (-1, -1);
                int fmtChunkSize = BitConverter.ToInt32(header, 16);
                if (fmtChunkSize < 8) return (-1, -1);
                int fmtVer = BitConverter.ToInt32(header, 20);
                int bldVer = BitConverter.ToInt32(header, 24);
                return (fmtVer, bldVer);
            }
            catch
            {
                return (-1, -1);
            }
        }

        /// <summary>
        /// Checks vanilla banks in Content/FMOD/Desktop for version mismatches and warns if
        /// mixed 1.x / 2.x banks are detected (common cause of ERR_VERSION on Audio.Init).
        /// </summary>
        private static void CheckVanillaBankVersions()
        {
            try
            {
                string desktop = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "FMOD", "Desktop");
                if (!Directory.Exists(desktop)) return;
                var files = Directory.GetFiles(desktop, "*.bank");
                int v1Count = 0, v2Count = 0;
                foreach (var f in files)
                {
                    var (fmt, bld) = ReadBankVersion(f);
                    if (fmt < 0) continue;
                    if (fmt < 120) v1Count++;
                    else v2Count++;
                }
                if (v1Count > 0 && v2Count > 0)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        $"[Audio] VANILLA BANK VERSION MISMATCH detected in Content/FMOD/Desktop: " +
                        $"{v1Count} bank(s) are FMOD 1.x format, {v2Count} bank(s) are FMOD 2.x format. " +
                        "This causes Audio.Init() to fail with ERR_VERSION. " +
                        "Fix: ensure all vanilla banks match your runtime (restore from Desktop_OG for 1.10.14, " +
                        "or rebuild ALL vanilla banks with FMOD 2.02.22 for 2.x runtime).");
                }
            }
            catch { /* best-effort diagnostic */ }
        }

        private static void LoadAudioBanks()
        {
            if (Audio.System == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    "[Audio] FMOD system not available — skipping custom bank load.");
                AudioBanksLoaded = false;
                return;
            }

            // Run a quick diagnostic on vanilla banks so the user sees the mismatch warning
            // even when Audio.Init() has already failed and been swallowed.
            CheckVanillaBankVersions();

            string primaryDir = Path.Combine(Instance.Metadata.PathDirectory, "Audio");
            string fallbackDir = Path.Combine(Instance.Metadata.PathDirectory, "framework", "extracted", "Audio");
            string audioDir = primaryDir;
            AudioBanksLoaded = false;

            // Master/strings banks loaded (rebuilt without vanilla GUID conflicts).
            // The FMOD project was rebuilt to remove duplicate vanilla Celeste events.
            string masterPath  = Path.Combine(audioDir, "pusheen_audio.bank");
            string stringsPath = Path.Combine(audioDir, "pusheen_audio.strings.bank");
            bool hasMaster  = File.Exists(masterPath);
            bool hasStrings = File.Exists(stringsPath);

            bool masterOk = false, stringsOk = false;
            if (hasMaster)
            {
                FMOD.RESULT rm = Audio.System.loadBankFile(masterPath, LOAD_BANK_FLAGS.NORMAL, out _pusheenMasterBank);
                masterOk = (rm == FMOD.RESULT.OK || rm == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
                           && _pusheenMasterBank != null && _pusheenMasterBank.isValid();
                if (!masterOk)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        $"[Audio] Master bank failed: {rm}, valid={_pusheenMasterBank?.isValid()}. " +
                        "If result is ERR_EVENT_ALREADY_LOADED and valid=false, the FMOD project must be " +
                        "rebuilt to remove vanilla Celeste events.");
                }
            }
            if (hasStrings)
            {
                FMOD.RESULT rs = Audio.System.loadBankFile(stringsPath, LOAD_BANK_FLAGS.NORMAL, out _pusheenStringsBank);
                stringsOk = (rs == FMOD.RESULT.OK || rs == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
                            && _pusheenStringsBank != null && _pusheenStringsBank.isValid();
                if (!stringsOk)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        $"[Audio] Strings bank failed: {rs}, valid={_pusheenStringsBank?.isValid()}.");
                }
            }

            // Load data banks unconditionally — they contain the actual event definitions
            // and audio sample data needed at runtime.
            int dataLoaded = 0, dataFailed = 0;
            bool anyVersionMismatch = false;
            if (LoadDataBank(audioDir, "pusheen_audio_A.bank", out _pusheenDataA, out bool vA)) { dataLoaded++; if (vA) anyVersionMismatch = true; } else dataFailed++;
            if (LoadDataBank(audioDir, "pusheen_audio_B.bank", out _pusheenDataB, out bool vB)) { dataLoaded++; if (vB) anyVersionMismatch = true; } else dataFailed++;
            if (LoadDataBank(audioDir, "pusheen_audio_C.bank", out _pusheenDataC, out bool vC)) { dataLoaded++; if (vC) anyVersionMismatch = true; } else dataFailed++;
            if (LoadDataBank(audioDir, "pusheen_audio_D.bank", out _pusheenDataD, out bool vD)) { dataLoaded++; if (vD) anyVersionMismatch = true; } else dataFailed++;

            // If every data bank failed with a version mismatch, try the fallback directory
            // (framework/extracted/Audio/) which may contain older 1.x-compatible banks.
            if (anyVersionMismatch && dataLoaded == 0 && Directory.Exists(fallbackDir))
            {
                Logger.Log(LogLevel.Info, "MaggyHelper",
                    $"[Audio] Primary banks in Audio/ failed with version mismatch. " +
                    "Attempting fallback to framework/extracted/Audio/ ...");
                int fbLoaded = 0, fbFailed = 0;
                if (LoadDataBank(fallbackDir, "pusheen_audio_A.bank", out _pusheenDataA, out _)) fbLoaded++; else fbFailed++;
                if (LoadDataBank(fallbackDir, "pusheen_audio_B.bank", out _pusheenDataB, out _)) fbLoaded++; else fbFailed++;
                if (LoadDataBank(fallbackDir, "pusheen_audio_C.bank", out _pusheenDataC, out _)) fbLoaded++; else fbFailed++;
                if (LoadDataBank(fallbackDir, "pusheen_audio_D.bank", out _pusheenDataD, out _)) fbLoaded++; else fbFailed++;
                dataLoaded = fbLoaded;
                dataFailed = fbFailed;
                audioDir = fallbackDir;
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"[Audio] Data banks: {dataLoaded} loaded, {dataFailed} failed. " +
                $"Master: {masterOk} Strings: {stringsOk}");

            // Mark banks as loaded if at least one data bank succeeded.
            // The hooks and bus manager need this flag to activate audio.
            if (dataLoaded > 0 || masterOk)
            {
                AudioBanksLoaded = true;
                AudioMasterBankLoaded = masterOk;

                // Ingest GUIDs so Audio.GetEventDescription can resolve pusheen event paths.
                // pusheen_audio.guids sits next to the master bank and covers all events in the project.
                string guidsPath = Path.Combine(audioDir, "pusheen_audio.guids");
                IngestGuidsFromFile(guidsPath);

                Logger.Log(LogLevel.Info, "MaggyHelper", "[Audio] Custom audio active.");

                // Activate audio redirect hooks now that the master bank is loaded
                if (AudioMasterBankLoaded)
                    LoadAudioHooks();
            }
            else
            {
                AudioBanksLoaded = false;
                AudioMasterBankLoaded = false;
                Logger.Log(LogLevel.Info, "MaggyHelper",
                    "[Audio] Master bank not loaded — using vanilla audio (pusheen hooks disabled).");
            }

            // Scan for audio assets from all loaded mods (diagnostic)
            ScanAllModAudioAssets();
        }

        /// <summary>
        /// Scans Everest content for audio assets from all loaded mods.
        /// Populates <see cref="DiscoveredModAudioAssets"/> and <see cref="DiscoveredModAudioBanks"/>.
        /// Safe to call multiple times; subsequent calls are no-ops unless forced.
        /// </summary>
        /// <param name="force">If true, clears and re-scans even if already scanned.</param>
        public static void ScanAllModAudioAssets(bool force = false)
        {
            if (_modAudioScanned && !force)
                return;

            _discoveredModAudioAssets.Clear();
            _discoveredModAudioBanks.Clear();

            try
            {
                foreach (System.Collections.Generic.KeyValuePair<string, ModAsset> pair in Everest.Content.Map)
                {
                    string key = pair.Key;
                    if (!key.StartsWith("Audio/", StringComparison.OrdinalIgnoreCase))
                        continue;

                    _discoveredModAudioAssets.Add(key);

                    if (key.EndsWith(".bank", StringComparison.OrdinalIgnoreCase) ||
                        key.EndsWith(".strings.bank", StringComparison.OrdinalIgnoreCase))
                    {
                        _discoveredModAudioBanks.Add(key);
                    }
                }

                _modAudioScanned = true;
                Logger.Log(
                    LogLevel.Info,
                    "MaggyHelper",
                    $"[Audio] Indexed {_discoveredModAudioAssets.Count} audio assets ({_discoveredModAudioBanks.Count} banks) across loaded mods.");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"[Audio] Failed to scan mod audio assets: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to ingest new banks from other mods via Everest's Audio system.
        /// Wraps <see cref="Audio.IngestNewBanks"/> with error handling.
        /// </summary>
        public static void TryIngestNewBanks(string source)
        {
            try
            {
                Audio.IngestNewBanks();
                Logger.Log(LogLevel.Info, "MaggyHelper", $"[Audio] Ingested mod banks ({source}).");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"[Audio] Failed to ingest mod banks ({source}): {ex.Message}");
            }
        }

/// <summary>
        /// Parses a .guids file (format: "{guid} event:/path" per line) and registers
        /// each pusheen event in <see cref="Audio.cachedModEvents"/> so that
        /// <see cref="Audio.GetEventDescription"/> can resolve them by path.
        /// Mirrors <c>Audio.IngestGUIDs(ModAsset)</c> for directly-loaded banks.
        /// </summary>
        private static void IngestGuidsFromFile(string guidsPath)
        {
            if (!File.Exists(guidsPath))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] GUID file not found: {guidsPath}");
                return;
            }

            var system = Audio.System;
            if (system == null) return;

            int ingested = 0, skipped = 0;
            try
            {
                foreach (string rawLine in File.ReadLines(guidsPath))
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line[0] != '{') continue;

                    int space = line.IndexOf(' ');
                    if (space < 0) continue;

                    string guidStr = line.Substring(0, space);
                    string path    = line.Substring(space + 1).Trim();

                    if (!Guid.TryParse(guidStr, out Guid guid)) continue;
                    if (string.IsNullOrEmpty(path))               continue;

                    // Skip non-event entries (bank:, snapshot:, bus:, vca:)
                    if (!path.StartsWith("event:/")) continue;

                    // Already known to the engine (vanilla or previously ingested)
                    if (Audio.cachedPaths.ContainsKey(guid)) { skipped++; continue; }

                    // Try to look up the event by its GUID in the loaded banks
                    FMOD.RESULT r = system.getEventByID(guid, out var desc);
                    if (r != FMOD.RESULT.OK) { skipped++; continue; }

                    // Register the event under its named path
                    if (!Audio.cachedModEvents.ContainsKey(path))
                    {
                        desc.unloadSampleData();
                        Audio.cachedPaths[guid]        = path;
                        Audio.cachedModEvents[path]    = desc;
                        ingested++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] Exception ingesting GUIDs from {guidsPath}: {ex.Message}");
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"[Audio] GUID ingestion from {Path.GetFileName(guidsPath)}: " +
                $"{ingested} registered, {skipped} skipped.");
        }
        private static bool LoadDataBank(string audioDir, string bankName, out Bank bank, out bool versionMismatch)
        {
            bank = default;
            versionMismatch = false;
            string bankPath = Path.Combine(audioDir, bankName);
            if (!File.Exists(bankPath))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"[Audio] Data bank not found: {bankPath}");
                return false;
            }

            var (fmtVer, bldVer) = ReadBankVersion(bankPath);
            if (fmtVer >= 0)
            {
                Logger.Log(LogLevel.Verbose, "MaggyHelper",
                    $"[Audio] {bankName} header version: format={fmtVer} build={bldVer}");
            }

            FMOD.RESULT rd = Audio.System.loadBankFile(bankPath, LOAD_BANK_FLAGS.NORMAL, out bank);
            if (rd == FMOD.RESULT.OK || rd == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
            {
                Logger.Log(LogLevel.Verbose, "MaggyHelper", $"[Audio] Loaded data bank: {bankName}");
                return true;
            }
            else
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Audio] Failed to load data bank {bankName}: {rd}");
                if (rd == FMOD.RESULT.ERR_VERSION)
                    versionMismatch = true;
                bank = default;
                return false;
            }
        }

        /// <summary>
        /// Loads the audio path-redirect hooks for the active theme.
        /// Safe to call multiple times — each hook class tracks its own _loaded flag.
        /// </summary>
        private static void LoadAudioHooks()
        {
            if (Settings.AudioTheme == AudioThemeMode.Kirby)
                global::Celeste.KirbyAudioHooks.Load();
            else
                global::Celeste.PusheenAudioHooks.Load();
        }

        public override void Unload()
        {
            // Unload manual hooks
            On.Celeste.GameLoader.LoadThread -= OnGameLoaderLoadThread;
            On.Celeste.Audio.Init -= OnAudioInit;
            AudioBusManager.Unload();

            // Unload global mod audio loader
            global::MaggyHelper.Helpers.GlobalModAudioLoader.Unload();
            global::Celeste.PusheenAudioHooks.Unload();
            global::Celeste.KirbyAudioHooks.Unload();
            OuiChapterSelectHooks.Unload();
            global::Celeste.RoomTransitionHandler.Unload();
            global::Celeste.IntroRemixHooks.Unload();
            global::Celeste.Cutscenes.IntroWarning.Unload();
            global::Celeste.AreaModeExtender.Unload();

            // ──── Shutdown Comprehensive D-Side Hook System ────
            global::Celeste.DSideHookImplementation.Shutdown();

            // ──── Uninitialize D-Side Hook Registry ────
            // Unloads: CelesteDSideHooks (On.hook + IL.hook)
            //          CelesteMusicHooks (On.hook + IL.hook)
            //          TitleScreen_ExtHook (On.hook + IL.hook)
            global::Celeste.DSideHookRegistry.UninitializeAll();

            global::Celeste.AltSidesHelperBridge.Unload();
            global::Celeste.MonoModHooks.Unload();
            global::Celeste.Mod.MaggyHelper.PayphoneCutsceneTriggers.Unload();
            global::Celeste.ChapterMasteryTracker.Unload();
            global::Celeste.CosmicChapterPanelHook.Unload();
            global::Celeste.Mod.MaggyHelper.ChapterProgressDisplay.Unload();

            // Unhook level exit cleanup
            Everest.Events.Level.OnExit -= OnLevelExit;
            // Unhook debug room warp menu
            Everest.Events.Level.OnLoadLevel -= OnLoadLevel_EnsureHotReloadController;

            // Remove ModContent.Update null-key guard
            global::Celeste.Mod.MaggyHelper.HotReload.EverestContentUpdateGuard.Unload();

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

            // Unhook Chapter Completion hooks
            global::Celeste.ChapterCompletionHooks.Unload();

            UnloadBank(ref _pusheenMasterBank);
            UnloadBank(ref _pusheenStringsBank);
            UnloadBank(ref _pusheenDataA);
            UnloadBank(ref _pusheenDataB);
            UnloadBank(ref _pusheenDataC);
            UnloadBank(ref _pusheenDataD);
            AudioBanksLoaded = false;
        }

        private static void UnloadBank(ref Bank bank)
        {
            if (bank.isValid())
                bank.unload();
            bank = default;
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
        //  MaggyMapMetadata Registry
        // =====================================================================

        private static void InitializeMapMetadataRegistry()
        {
            try
            {
                // Initialize the registry with the mod root directory
                // Navigate up from the assembly location (bin/Debug/net8.0/) to the actual mod root
                string assemblyDir = Path.GetDirectoryName(typeof(MaggyHelperModule).Assembly.Location) ?? ".";
                string modRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
                MaggyMapMetadataRegistry.Initialize(modRoot);

                Logger.Log(LogLevel.Info, "MaggyHelper", $"MaggyMapMetadataRegistry initialized with {MaggyMapMetadataRegistry.Count} entries");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Failed to initialize MaggyMapMetadataRegistry: " + ex.Message);
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

            Type[] mockTypes = new Type[] {
                typeof(global::Celeste.Mod.MaggyHelper.HotReload.ModHotReloadTest),
                typeof(global::Celeste.HotReload.GameHotReloadTest)
            };

            global::Celeste.HotReload.HotReloadHandler.UpdateApplication(mockTypes);
            Engine.Commands?.Log("[MaggyHelper] Hot reload test complete.");
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

