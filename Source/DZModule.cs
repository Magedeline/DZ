using System;
using MonoMod.ModInterop;
using DZ;

namespace Celeste.Mod.DZ;

public class DZModule : EverestModule {
    public static DZModule Instance { get; private set; }

    public override Type SettingsType => typeof(DZModuleSettings);
    public static DZModuleSettings Settings => (DZModuleSettings) Instance._Settings;

    public static SpriteBank SpriteBank => new SpriteBank(GFX.Game, "DZ/Sprites");

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
    }

    public override void Unload() {
        AudioReplacer.Unload();
    }

    /// <summary>
    /// Launch part 1 credits.
    /// </summary>
    public static void LaunchPart1Credits()
    {
        // Stub implementation - does nothing
    }

    /// <summary>
    /// Launch part 2 credits.
    /// </summary>
    public static void LaunchPart2Credits()
    {
        // Stub implementation - does nothing
    }

    /// <summary>
    /// Trigger the unlock everything cheat.
    /// </summary>
    public static void TriggerUnlockEverythingCheat()
    {
        // Stub implementation - does nothing
    }

    /// <summary>
    /// Gets the SID for chapter 17 epilogue.
    /// </summary>
    public static string Chapter17EpilogueSid => "DZ/17_Epilogue";

    /// <summary>
    /// Marks chapter 17 epilogue as completed.
    /// </summary>
    public static void MarkChapter17EpilogueCompleted()
    {
        // Stub implementation
    }

    /// <summary>
    /// Checks if chapter 17 epilogue is completed.
    /// </summary>
    public static bool IsChapter17EpilogueCompleted()
    {
        return false;
    }

    /// <summary>
    /// Launches chapter 17 epilogue.
    /// </summary>
    public static void LaunchChapter17Epilogue()
    {
        // Stub implementation
    }
}