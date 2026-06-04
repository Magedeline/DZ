using System;
using Monocle;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// Audio manager for KIRBY_CELESTE.
/// Banks loaded from Audio/ folder are auto-registered by Everest's IngestBank system.
/// No manual loading required—Everest handles FMOD bank initialization automatically.
/// </summary>
public static class OverworldMusicManager
{
    private static bool _initialized = false;

    public static void LoadBanks()
    {
        if (_initialized) return;
        _initialized = true;
        Logger.Log(LogLevel.Info, "MaggyHelper", "✓ Audio system ready (Everest IngestBank manages FMOD banks)");
    }

    public static void RetryBankLoadIfNeeded()
    {
        // Everest's IngestBank system handles all bank loading automatically
    }

    public static void UnloadBanks()
    {
        _initialized = false;
        Logger.Log(LogLevel.Info, "MaggyHelper", "Audio system unloaded");
    }
}
