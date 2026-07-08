// ModInterop exports for the DZ mod.
// Note: ModInterop attributes are resolved at compile-time by MonoMod.
// Other mods should use DZInterop helper class for safe access.

using Celeste.Mod.Meta;

namespace Celeste.Mod.DZ;

/// <summary>
/// ModInterop exports for the DZ mod, allowing other mods to call DZ functionality.
/// Other mods should use the DZInterop helper class instead of calling these directly.
/// </summary>
public static class DZExports
{
    // ── Credits & Epilogue ────────────────────────────────────────────────
    
    /// <summary>Launch Part 1 credits sequence.</summary>
    public static void LaunchPart1Credits() => DZModule.LaunchPart1Credits();

    /// <summary>Launch Part 2 credits sequence.</summary>
    public static void LaunchPart2Credits() => DZModule.LaunchPart2Credits();

    /// <summary>Get the SID for Chapter 17 Epilogue.</summary>
    public static string GetChapter17EpilogueSid() => DZModule.Chapter17EpilogueSid;

    /// <summary>Mark Chapter 17 Epilogue as completed in save data.</summary>
    public static void MarkChapter17EpilogueCompleted() => DZModule.MarkChapter17EpilogueCompleted();

    /// <summary>Check if Chapter 17 Epilogue has been completed.</summary>
    public static bool IsChapter17EpilogueCompleted() => DZModule.IsChapter17EpilogueCompleted();

    /// <summary>Launch the Chapter 17 Epilogue cutscene.</summary>
    public static void LaunchChapter17Epilogue() => DZModule.LaunchChapter17Epilogue();

    // ── Cheats & Unlocks ──────────────────────────────────────────────────
    
    /// <summary>Trigger the unlock everything cheat (unlocks all chapters).</summary>
    public static void TriggerUnlockEverythingCheat() => DZModule.TriggerUnlockEverythingCheat();

    // ── Settings Access ───────────────────────────────────────────────────
    
    /// <summary>Get the current DZ module settings.</summary>
    public static DZModuleSettings GetSettings() => DZModule.Settings;

    /// <summary>Get the current DZ module session.</summary>
    public static DZModuleSession GetSession() => DZModule.Session;

    /// <summary>Get the current DZ module save data.</summary>
    public static DZModuleSaveData GetSaveData() => DZModule.SaveData;

    // ── OverworldHelper Integration ────────────────────────────────────────────
    
    /// <summary>Subscribe to area change events.</summary>
    public static void SubscribeToAreaChanged(Action<AreaKey> callback) => OverworldHelperExports.SubscribeToAreaChanged(callback);

    /// <summary>Unsubscribe from area change events.</summary>
    public static void UnsubscribeFromAreaChanged(Action<AreaKey> callback) => OverworldHelperExports.UnsubscribeFromAreaChanged(callback);

    /// <summary>Subscribe to overworld creation events.</summary>
    public static void SubscribeToOverworldCreated(Action<Overworld> callback) => OverworldHelperExports.SubscribeToOverworldCreated(callback);

    /// <summary>Unsubscribe from overworld creation events.</summary>
    public static void UnsubscribeFromOverworldCreated(Action<Overworld> callback) => OverworldHelperExports.UnsubscribeFromOverworldCreated(callback);

    /// <summary>Get the current overworld instance.</summary>
    public static Overworld GetCurrentOverworld() => OverworldHelperExports.GetOverworld();

    /// <summary>Get custom configuration for a specific area.</summary>
    public static MapMeta GetConfigFromArea(AreaKey area, Type type) => OverworldHelperExports.GetConfigFromArea(area, type);

    /// <summary>Get custom configuration for a specific area (string-based).</summary>
    public static MapMeta GetConfigFromString(string area, Type type) => OverworldHelperExports.GetConfigFromString(area, type);
}
