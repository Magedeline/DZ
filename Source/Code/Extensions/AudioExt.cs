using System.Collections.Generic;
using Celeste.Mod;
using FMOD;
using FMOD.Studio;

namespace Celeste.Extensions;

/// <summary>
/// Thin convenience wrappers around the internal <see cref="Audio"/> API,
/// exposing bank management and GUID ingestion helpers to the rest of the mod.
/// </summary>
public static class AudioExt
{
    /// <summary>All currently-loaded FMOD banks, keyed by name.</summary>
    public static Dictionary<string, Bank> Banks => Audio.Banks.Banks;

    /// <summary>
    /// Checks if the given FMOD result is <see cref="RESULT.OK"/>. Throws otherwise.
    /// </summary>
    public static void CheckFMOD(this RESULT result)
    {
        Audio.CheckFmod(result);
    }

    /// <inheritdoc cref="Audio.IngestBank(ModAsset)" />
    public static Bank IngestBank(ModAsset asset)
    {
        return Audio.IngestBank(asset);
    }

    /// <inheritdoc cref="Audio.IngestGUIDs(ModAsset)" />
    public static void IngestGUIDs(ModAsset asset)
    {
        Audio.IngestGUIDs(asset);
    }
}
