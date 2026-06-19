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

    private static SpriteBank _spriteBank;
    public static SpriteBank SpriteBank => _spriteBank ??= new SpriteBank(GFX.Game, "DZ/Sprites");

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
        // typeof(DZExports).ModInterop(); // TODO: delete this line if you do not need to export any functions

        AudioReplacer.Load();
        EverestContentUpdateGuard.Load();
        LiveWatchPatcher.Load();
        VignetteHooks.Load();
        KirbyPlayerController.Load();
        SoulPlayerController.Load();
        BattlePlayerController.Load();

        // Hook into scene changes to add the HotReloadController
        On.Monocle.Engine.Update += OnEngineUpdate;
    }

    public override void Unload() {
        AudioReplacer.Unload();
        EverestContentUpdateGuard.Unload();
        LiveWatchPatcher.Unload();
        VignetteHooks.Unload();
        KirbyPlayerController.Unload();
        SoulPlayerController.Unload();
        BattlePlayerController.Unload();
        On.Monocle.Engine.Update -= OnEngineUpdate;
        _spriteBank = null;
    }

    private static void OnEngineUpdate(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
    {
        orig(self, gameTime);

        // Ensure HotReloadController exists in the current scene
        if (Engine.Scene != null && Engine.Scene.Tracker.GetEntity<HotReloadController>() == null)
        {
            Engine.Scene.Add(new HotReloadController());
        }

        // Ensure LiveWatchOverlay exists if frozen
        if (Engine.Scene != null && LiveWatchPatcher.IsFrozen && Engine.Scene.Entities.FindFirst<LiveWatchOverlay>() == null)
        {
            Engine.Scene.Add(new LiveWatchOverlay(LiveWatchPatcher.LastError, DateTime.Now));
        }
    }

    /// <summary>
    /// Launch part 1 credits by navigating the Overworld to the CsPart1Credit Oui screen.
    /// </summary>
    public static void LaunchPart1Credits()
    {
        if (Engine.Scene is Overworld overworld)
        {
            overworld.Goto<global::Celeste.Cutscenes.CsPart1Credit>();
        }
        else
        {
            // Navigate via OverworldLoader if not already in the Overworld
            Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu, null);
        }
    }

    /// <summary>
    /// Launch part 2 credits (CS17_Credits) from the current level.
    /// </summary>
    public static void LaunchPart2Credits()
    {
        if (Engine.Scene is Level level)
        {
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                level.Add(new global::Celeste.Cutscenes.CS17_Credits(player));
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
        if (Engine.Scene is Level level)
        {
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                level.Add(new global::Celeste.Cutscenes.Cs17Epilogue(player));
            }
        }
    }
}