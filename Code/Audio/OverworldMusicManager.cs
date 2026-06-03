using Monocle;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// Audio manager for KIRBY_CELESTE.
/// Everest's IngestBank system automatically loads FMOD banks from Audio/ folder.
/// This class just provides logging and verification.
/// </summary>
public static class OverworldMusicManager
{
    private static bool _initialized = false;

    public static void LoadBanks()
    {
        if (_initialized) return;
        _initialized = true;

        Logger.Log(LogLevel.Info, "MaggyHelper", "✓ Audio system initialized (Everest IngestBank handles FMOD banks)");
    }

    public static void RetryBankLoadIfNeeded()
    {
        // Everest handles everything, nothing to retry
    }

    public static void UnloadBanks()
    {
        _initialized = false;
        Logger.Log(LogLevel.Info, "MaggyHelper", "Audio system unloaded");
    }
}
