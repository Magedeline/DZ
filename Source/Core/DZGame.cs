using System;
using System.IO;

namespace DZ.Core;

/// <summary>
/// Stub for the standalone fangame's global game class.
/// The original DZGame managed FMOD audio, bank paths, and game state.
/// In the DZ mod these entities are not instantiated at runtime; this stub
/// exists solely so the standalone source files compile. All audio calls are
/// no-ops; AudioBankPath returns an empty string.
/// </summary>
public static class DZGame
{
    /// <summary>Stub audio API. All calls are no-ops.</summary>
    public static class Audio
    {
        public static void PlaySfx(string eventPath) { }
        public static void PlayMusic(string eventPath, bool fadeIn = false) { }
        public static void StopMusic() { }
        public static void StopSfx(string eventPath) { }
    }

    /// <summary>Path to FMOD bank files (stub: empty).</summary>
    public static string AudioBankPath { get; set; } = string.Empty;

    /// <summary>Initialize game state (stub: no-op).</summary>
    public static void Initialize() { }

    /// <summary>Reload game state for hot reload (stub: no-op).</summary>
    public static void Reload() { }

    /// <summary>Shutdown game state (stub: no-op).</summary>
    public static void Shutdown() { }
}
