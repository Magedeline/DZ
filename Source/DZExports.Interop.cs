using System;
using System.Reflection;

namespace Celeste.Mod.DZ;

/// <summary>
/// Helper class for other mods to access DZ exports via ModInterop.
/// Usage: Call DZInterop.Initialize() once during mod load, then use the static properties.
/// </summary>
public static class DZInterop
{
    private static bool _initialized;
    private static Type _dzExportsType;
    private static MethodInfo _launchPart1CreditsMethod;
    private static MethodInfo _launchPart2CreditsMethod;
    private static MethodInfo _getChapter17EpilogueSidMethod;
    private static MethodInfo _markChapter17EpilogueCompletedMethod;
    private static MethodInfo _isChapter17EpilogueCompletedMethod;
    private static MethodInfo _launchChapter17EpilogueMethod;
    private static MethodInfo _triggerUnlockEverythingCheatMethod;
    private static MethodInfo _getSettingsMethod;
    private static MethodInfo _getSessionMethod;
    private static MethodInfo _getSaveDataMethod;

    /// <summary>
    /// Initialize DZ interop. Call this once during your mod's Load() method.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized)
            return;

        try
        {
            // Get the DZExports type from the DZ assembly
            var dzAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "DZ");

            if (dzAssembly == null)
            {
                Logger.Log(LogLevel.Warn, "DZInterop", "DZ assembly not found. ModInterop will not be available.");
                return;
            }

            _dzExportsType = dzAssembly.GetType("Celeste.Mod.DZ.DZExports");
            if (_dzExportsType == null)
            {
                Logger.Log(LogLevel.Warn, "DZInterop", "DZExports type not found in DZ assembly.");
                return;
            }

            // Cache all method references
            _launchPart1CreditsMethod = _dzExportsType.GetMethod(nameof(LaunchPart1Credits));
            _launchPart2CreditsMethod = _dzExportsType.GetMethod(nameof(LaunchPart2Credits));
            _getChapter17EpilogueSidMethod = _dzExportsType.GetMethod(nameof(GetChapter17EpilogueSid));
            _markChapter17EpilogueCompletedMethod = _dzExportsType.GetMethod(nameof(MarkChapter17EpilogueCompleted));
            _isChapter17EpilogueCompletedMethod = _dzExportsType.GetMethod(nameof(IsChapter17EpilogueCompleted));
            _launchChapter17EpilogueMethod = _dzExportsType.GetMethod(nameof(LaunchChapter17Epilogue));
            _triggerUnlockEverythingCheatMethod = _dzExportsType.GetMethod(nameof(TriggerUnlockEverythingCheat));
            _getSettingsMethod = _dzExportsType.GetMethod(nameof(GetSettings));
            _getSessionMethod = _dzExportsType.GetMethod(nameof(GetSession));
            _getSaveDataMethod = _dzExportsType.GetMethod(nameof(GetSaveData));

            _initialized = true;
            Logger.Log(LogLevel.Info, "DZInterop", "DZ ModInterop initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZInterop", $"Failed to initialize DZ ModInterop: {ex.Message}");
        }
    }

    /// <summary>Check if DZ interop is initialized and available.</summary>
    public static bool IsAvailable => _initialized && _dzExportsType != null;

    // ── Credits & Epilogue ────────────────────────────────────────────────

    /// <summary>Launch Part 1 credits sequence.</summary>
    public static void LaunchPart1Credits()
    {
        if (!IsAvailable) return;
        _launchPart1CreditsMethod?.Invoke(null, null);
    }

    /// <summary>Launch Part 2 credits sequence.</summary>
    public static void LaunchPart2Credits()
    {
        if (!IsAvailable) return;
        _launchPart2CreditsMethod?.Invoke(null, null);
    }

    /// <summary>Get the SID for Chapter 17 Epilogue.</summary>
    public static string GetChapter17EpilogueSid()
    {
        if (!IsAvailable) return null;
        return _getChapter17EpilogueSidMethod?.Invoke(null, null) as string;
    }

    /// <summary>Mark Chapter 17 Epilogue as completed in save data.</summary>
    public static void MarkChapter17EpilogueCompleted()
    {
        if (!IsAvailable) return;
        _markChapter17EpilogueCompletedMethod?.Invoke(null, null);
    }

    /// <summary>Check if Chapter 17 Epilogue has been completed.</summary>
    public static bool IsChapter17EpilogueCompleted()
    {
        if (!IsAvailable) return false;
        return _isChapter17EpilogueCompletedMethod?.Invoke(null, null) is bool result && result;
    }

    /// <summary>Launch the Chapter 17 Epilogue cutscene.</summary>
    public static void LaunchChapter17Epilogue()
    {
        if (!IsAvailable) return;
        _launchChapter17EpilogueMethod?.Invoke(null, null);
    }

    // ── Cheats & Unlocks ──────────────────────────────────────────────────

    /// <summary>Trigger the unlock everything cheat (unlocks all chapters).</summary>
    public static void TriggerUnlockEverythingCheat()
    {
        if (!IsAvailable) return;
        _triggerUnlockEverythingCheatMethod?.Invoke(null, null);
    }

    // ── Settings Access ───────────────────────────────────────────────────

    /// <summary>Get the current DZ module settings.</summary>
    public static object GetSettings()
    {
        if (!IsAvailable) return null;
        return _getSettingsMethod?.Invoke(null, null);
    }

    /// <summary>Get the current DZ module session.</summary>
    public static object GetSession()
    {
        if (!IsAvailable) return null;
        return _getSessionMethod?.Invoke(null, null);
    }

    /// <summary>Get the current DZ module save data.</summary>
    public static object GetSaveData()
    {
        if (!IsAvailable) return null;
        return _getSaveDataMethod?.Invoke(null, null);
    }
}
