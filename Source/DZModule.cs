using System;
using MonoMod.ModInterop;
using DZ;
using Celeste.Mod.DZ.HotReload;
using Monocle;
using Celeste.Entities;

namespace Celeste.Mod.DZ;

public class DZModule : EverestModule {
    public static DZModule Instance { get; private set; }

    public override Type SettingsType => typeof(DZModuleSettings);
    public static DZModuleSettings Settings => (DZModuleSettings) Instance._Settings;

    // Tracks the last scene that had the global controller entities added.
    // Avoids repeating the (potentially expensive) FindFirst scan every frame.
    private static Scene _controllersScene;

    public static ParticleType P_StarExplosion => new ParticleType();

    public override Type SessionType => typeof(DZModuleSession);
    public static DZModuleSession Session => (DZModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(DZModuleSaveData);
    public static DZModuleSaveData SaveData => (DZModuleSaveData) Instance._SaveData;

    public DZModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(DZModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(DZModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        typeof(DZExports).ModInterop();

        AudioReplacer.Load();
        EverestContentUpdateGuard.Load();
        LiveWatchPatcher.Load();

        // Set up DLL hot-reload: file watcher on bin/ + enable Everest CodeReload_WIP.
        DllHotReloader.Initialize();

        // Initialize area map data and apply hardcoded runtime data
        AreaMapData.Initialize();
        AreaMapData.ApplyHardcodedRuntimeData();

        // Load D-Side hook registry (D-Side, Music, TitleScreen hooks)
        DSideHookRegistry.InitializeAll();

        VignetteHooks.Load();
        global::DZ.Entities.Payphone.LoadParticles();
        KirbyPlayerController.Load();
        SoulPlayerController.Load();
        BattlePlayerController.Load();
        SideLockDisplaySystem.Load();
        ChapterCompletionHooks.Load();
        OverworldConnectorHooks.Load();
        OuiModeSelectHooks.Load();
        global::DZ.AreaModeExtender.Load();

        // OUI and chapter-select systems
        // Note: OuiChapterSelectCustom is a superset of OuiChapterSelectHooks;
        // only one is registered to avoid double-wrapping vanilla Enter/Update.
        OuiChapterSelectCustom.Load();
        LevelSelectOuiHook.Load();
        ChapterProgressDisplay.Load();
        ChapterProgressionManager.Load();
        CosmicChapterPanelHook.Load();
        ChapterMasteryTracker.Load();

        // Heart gem, player, and room-transition systems
        global::DZ.HeartGemManager.Load();
        global::DZ.AltSidesHelperBridge.Load();
        global::DZ.K_PlayerHooks.Load();
        global::DZ.RoomTransitionHandler.Load();
        global::DZ.PlayerCompatShim.Load();
        PayphoneCutsceneTriggers.Load();

        // Map-list / chapter navigation fixes
        global::DZ.MapListExt.Load();

        // Intro sequence hooks
        global::DZ.IntroRemixHooks.Load();
        global::Celeste.Cutscenes.IntroWarning.Load();

        // Advanced MonoMod hooks (manual Hook + IL hooks)
        global::DZ.MonoModHooks.Load();

        // Kirby health system
        global::DZ.KirbyHealthSystemHooks.Load();

        // Entity-level hooks (IL patches, renderer hooks, etc.)
        global::Celeste.Entities.DesoloZantasTape.Load();
        global::Celeste.Entities.PowerGenerator.Load();
        global::Celeste.Entities.MaddyCrystal.Load();
        global::Celeste.Entities.FlingBirdMod.Load();
        global::Celeste.Entities.StarJumpBlock.Load();
        global::Celeste.Entities.WhiteHole.Load();
        global::Celeste.Entities.PlateauMod.Load();
        // BossesExampleModule.Load() already calls DarkLightningRenderer.Load()
        // and ResetZoneRenderer.Load() internally — do NOT call them separately
        // or Everest will throw a double-hook crime on LevelLoader.LoadingThread.
        global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.Load();

        // Sub-chapter system (experimental)
        global::DZ.SubChapterManager.Load();

        // Initialize metadata registries (loads .bin.DZ.meta.yaml etc.)
        MetadataRegistries.Initialize();

        // Hook into scene changes to add the HotReloadController
        On.Monocle.Engine.Update += OnEngineUpdate;

        Logger.Log(LogLevel.Info, nameof(DZModule), "DZ mod loaded successfully");
    }

    public override void Unload() {
        AudioReplacer.Unload();
        EverestContentUpdateGuard.Unload();
        LiveWatchPatcher.Unload();
        DllHotReloader.Shutdown();

        // Unload D-Side hook registry
        DSideHookRegistry.UninitializeAll();

        VignetteHooks.Unload();
        KirbyPlayerController.Unload();
        SoulPlayerController.Unload();
        BattlePlayerController.Unload();
        SideLockDisplaySystem.Unload();
        ChapterCompletionHooks.Unload();
        OverworldConnectorHooks.Unload();
        OuiModeSelectHooks.Unload();
        global::DZ.AreaModeExtender.Unload();

        // OUI and chapter-select systems
        OuiChapterSelectCustom.Unload();
        LevelSelectOuiHook.Unload();
        ChapterProgressDisplay.Unload();
        ChapterProgressionManager.Unload();
        CosmicChapterPanelHook.Unload();
        ChapterMasteryTracker.Unload();

        // Heart gem, player, and room-transition systems
        global::DZ.HeartGemManager.Unload();
        global::DZ.AltSidesHelperBridge.Unload();
        global::DZ.K_PlayerHooks.Unload();
        global::DZ.RoomTransitionHandler.Unload();
        global::DZ.PlayerCompatShim.Unload();
        PayphoneCutsceneTriggers.Unload();

        // Map-list / chapter navigation fixes
        global::DZ.MapListExt.Unload();

        // Intro sequence hooks
        global::DZ.IntroRemixHooks.Unload();
        global::Celeste.Cutscenes.IntroWarning.Unload();

        // Advanced MonoMod hooks
        global::DZ.MonoModHooks.Unload();

        // Kirby health system
        global::DZ.KirbyHealthSystemHooks.Unload();

        // Entity-level hooks
        global::Celeste.Entities.DesoloZantasTape.Unload();
        global::Celeste.Entities.PowerGenerator.Unload();
        // MaddyCrystal.Load() only sets up particles (no hooks), no Unload needed
        global::Celeste.Entities.FlingBirdMod.Unload();
        global::Celeste.Entities.StarJumpBlock.Unload();
        global::Celeste.Entities.WhiteHole.Unload();
        global::Celeste.Entities.PlateauMod.Unload();
        // BossesExampleModule.Unload() already calls DarkLightningRenderer/ResetZoneRenderer
        global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.Unload();

        // Sub-chapter system
        global::DZ.SubChapterManager.Unload();

        On.Monocle.Engine.Update -= OnEngineUpdate;
        _controllersScene = null;

        Logger.Log(LogLevel.Info, nameof(DZModule), "DZ mod unloaded");
    }

    private static void OnEngineUpdate(On.Monocle.Engine.orig_Update orig, Monocle.Engine self, GameTime gameTime)
    {
        orig(self, gameTime);

        var scene = Monocle.Engine.Scene;
        if (scene != null && scene != _controllersScene)
        {
            _controllersScene = scene;

            // Only scan the entity list once per scene transition for the global controllers.
            if (scene.Entities.FindFirst<HotReloadController>() == null)
            {
                scene.Add(new HotReloadController());
            }

            // Ensure GentleBreezeAssist exists in the current scene so the
            // dash-aim freeze / arrow render for the Kirby player when the
            // Gentle Breeze setting is enabled.
            if (scene.Entities.FindFirst<global::Celeste.Entities.GentleBreezeAssist>() == null)
            {
                scene.Add(new global::Celeste.Entities.GentleBreezeAssist());
            }

            // Ensure cheat listener exists in level scenes
            if (scene is Level && scene.Entities.FindFirst<DZUnlockEverything>() == null)
            {
                scene.Add(new DZUnlockEverything());
            }
        }

        // Ensure LiveWatchOverlay exists if frozen. This is a cheap fallback that only
        // runs while the game is already paused by LiveWatchPatcher, so the extra
        // FindFirst is negligible compared to the constant per-frame scan it replaces.
        if (scene != null && LiveWatchPatcher.IsFrozen && scene.Entities.FindFirst<LiveWatchOverlay>() == null)
        {
            scene.Add(new LiveWatchOverlay(LiveWatchPatcher.LastError, DateTime.Now));
        }
    }

    /// <summary>
    /// Launch part 1 credits by navigating the Overworld to the CsPart1Credit Oui screen.
    /// </summary>
    public static void LaunchPart1Credits()
    {
        if (Monocle.Engine.Scene is Overworld overworld)
        {
            overworld.Goto<global::Celeste.Cutscenes.CsPart1Credit>();
        }
        else
        {
            // Navigate via OverworldLoader if not already in the Overworld
            Monocle.Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu, null);
        }
    }

    /// <summary>
    /// Launch part 2 credits (CS17_Credits) from the current level.
    /// </summary>
    public static void LaunchPart2Credits()
    {
        if (Monocle.Engine.Scene is Level level)
        {
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                level.Add(new global::Celeste.Cutscenes.CS17_Credits());
            }
        }
    }

    /// <summary>
    /// Trigger the unlock everything cheat — unlocks all chapters in save data.
    /// </summary>
    public static void TriggerUnlockEverythingCheat()
    {
        var saveData = SaveData;
        if (saveData == null) return;

        saveData.UnlockedChapter10 = true;
        saveData.UnlockedChapter19 = true;
        saveData.UnlockedChapter21 = true;
        saveData.FinalDlcContentUnlocked = true;
        saveData.TrueFinaleUnlocked = true;
        saveData.BossRushUnlocked = true;
        saveData.VoidMoonUnlocked = true;
        saveData.HasSeenModIntro = true;
    }

    /// <summary>
    /// Gets the SID for chapter 17 epilogue.
    /// </summary>
    public static string Chapter17EpilogueSid => "DZ/17_Epilogue";

    /// <summary>
    /// Marks chapter 17 epilogue as completed in save data.
    /// </summary>
    public static void MarkChapter17EpilogueCompleted()
    {
        var saveData = SaveData;
        if (saveData != null)
            saveData.Chapter17EpilogueCompleted = true;
    }

    /// <summary>
    /// Checks if chapter 17 epilogue is completed.
    /// </summary>
    public static bool IsChapter17EpilogueCompleted()
    {
        return SaveData?.Chapter17EpilogueCompleted == true;
    }

    /// <summary>
    /// Launches the chapter 17 epilogue cutscene in the current level.
    /// </summary>
    public static void LaunchChapter17Epilogue()
    {
        if (Monocle.Engine.Scene is Level level)
        {
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                level.Add(new global::Celeste.Cutscenes.Cs17Epilogue(player));
            }
        }
    }
}