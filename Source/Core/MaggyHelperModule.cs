using System;
using global::Celeste.Mod.MaggyHelper.BossesExample;
using Celeste.Cutscenes;
using Monocle;

namespace Celeste.Mod.MaggyHelper
{
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

        public override void Load()
        {
            BossesExampleModule.Load();
            // Note: AreaMapData, ChapterActRegistry, and BossRosterRegistry
            // use lazy initialization - they'll be populated on first access.
            global::Celeste.AreaModeExtender.Load();
            global::Celeste.AltSidesHelperBridge.Load();
            global::Celeste.AreaCompleteHooks.Load();
            global::Celeste.IntroRemixHooks.Load();
            global::Celeste.MonoModHooks.Load();
            global::Celeste.VignetteHooks.Load();
            global::Celeste.TitleScreen_ExtHook.Load();
            global::Celeste.OverworldMusicManager.Load();
            global::Celeste.MountainOverworldManager.Load();
            global::Celeste.Cutscenes.IntroWarning.Load();

            global::Celeste.ChapterMasteryTracker.Load();
            global::Celeste.CosmicChapterPanelHook.Load();

            // Chapter progression hooks for late-game unlock flow
            ChapterProgressionManager.Load();

            // Kirby mechanics are layered onto the vanilla player via a custom state.
            global::Celeste.KirbyPlayerStateController.Load();

            global::Celeste.MountainOverworldManager.Load();

            // Kirby health system hooks for hazard damage integration
            global::Celeste.KirbyHealthSystemHooks.Load();

            // Hot Reload Controller (Global) - named handler so Unload can -= it.
            Everest.Events.Level.OnLoadLevel += OnLoadLevel_EnsureHotReloadController;

            // Hook level exit to clean up static state
            Everest.Events.Level.OnExit += OnLevelExit;

            // Reset credits launch flags
            LaunchPart1Credits = false;
            LaunchPart2Credits = false;
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
        }

        public override void Unload()
        {
            ChapterProgressionManager.Unload();
            global::Celeste.CosmicChapterPanelHook.Unload();
            global::Celeste.ChapterMasteryTracker.Unload();
            global::Celeste.KirbyPlayerStateController.Unload();
            global::Celeste.MountainOverworldManager.Unload();
            global::Celeste.KirbyHealthSystemHooks.Unload();
            global::Celeste.MountainOverworldManager.Unload();
            global::Celeste.OverworldMusicManager.Unload();
            global::Celeste.TitleScreen_ExtHook.Unload();
            global::Celeste.Cutscenes.IntroWarning.Unload();
            global::Celeste.VignetteHooks.Unload();
            global::Celeste.MonoModHooks.Unload();
            global::Celeste.IntroRemixHooks.Unload();
            global::Celeste.AreaCompleteHooks.Unload();
            global::Celeste.AltSidesHelperBridge.Unload();
            global::Celeste.AreaModeExtender.Unload();
            BossesExampleModule.Unload();

            // Unhook level exit cleanup
            Everest.Events.Level.OnExit -= OnLevelExit;
            // Unhook hot-reload controller insertion (matches += in Load)
            Everest.Events.Level.OnLoadLevel -= OnLoadLevel_EnsureHotReloadController;

            // Reset credits state
            LaunchPart1Credits = false;
            LaunchPart2Credits = false;
            _prophecyFont = null;
            _prophecyFontInitialized = false;
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            BossesExampleModule.LoadContent(firstLoad);
            // ProphecyFont is now lazy-initialized on first access
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
        /// Allows other mods (like BrokemiaHelper) to detect if MaggyHelper/Player is available.
        /// Returns true if the MaggyHelper Player type is loaded and available.
        /// </summary>
        public static bool IsMaggyPlayerAvailable()
        {
            return GetMaggyPlayerType() != null;
        }

        /// <summary>
        /// Gets the MaggyHelper Player type if available. Use this for reflection-based interaction.
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

            creditsSession.Audio.Music.Event = "event:/desolozantas/music/lvl17/main";
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
            Type[] mockTypes = new Type[] { 
                typeof(global::Celeste.Mod.MaggyHelper.HotReload.ModHotReloadTest),
                typeof(global::Celeste.HotReload.GameHotReloadTest)
            };
            
            global::Celeste.HotReload.HotReloadHandler.UpdateApplication(mockTypes);
        }
    }
}
