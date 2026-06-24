#nullable enable
using Celeste.Mod;
using Monocle;

namespace Celeste;

/// <summary>
/// Helper methods for working with MaggyMapMetadata.
/// Provides integration with Celeste's Session, AreaKey, and other systems.
/// </summary>
internal static class MaggyMapMetadataHelper
{
    /// <summary>Get metadata for the current session's area</summary>
    public static MaggyMapMetadata? GetCurrentSessionMetadata()
    {
        if (Engine.Scene is not Level level)
            return null;

        return GetBySid(level.Session.Area.GetSID());
    }

    /// <summary>Get metadata by AreaKey</summary>
    public static MaggyMapMetadata? GetByAreaKey(AreaKey area)
    {
        return GetBySid(area.GetSID());
    }

    /// <summary>Get metadata by SID</summary>
    public static MaggyMapMetadata? GetBySid(string sid)
    {
        return MaggyMapMetadataRegistry.GetBySid(sid);
    }

    /// <summary>Get metadata by base key (e.g., "00_Prologue")</summary>
    public static MaggyMapMetadata? GetByBaseKey(string baseKey)
    {
        return MaggyMapMetadataRegistry.GetByBaseKey(baseKey);
    }

    /// <summary>Get all metadata for a specific side (A, B, C, D)</summary>
    public static List<MaggyMapMetadata> GetBySide(string side)
    {
        return MaggyMapMetadataRegistry.GetBySide(side);
    }

    /// <summary>Get all main chapter entries</summary>
    public static List<MaggyMapMetadata> GetMainChapterEntries()
    {
        return MaggyMapMetadataRegistry.GetMainChapterEntries();
    }

    /// <summary>Get all maps that should show in chapter select</summary>
    public static List<MaggyMapMetadata> GetChapterSelectMaps()
    {
        return MaggyMapMetadataRegistry.GetChapterSelectMaps();
    }

    /// <summary>Check if a SID exists in the registry</summary>
    public static bool HasSid(string sid)
    {
        return MaggyMapMetadataRegistry.HasSid(sid);
    }

    /// <summary>Get the total count of registered metadata entries</summary>
    public static int Count => MaggyMapMetadataRegistry.Count;

    /// <summary>Get all registered SIDs</summary>
    public static IEnumerable<string> GetAllSids()
    {
        return MaggyMapMetadataRegistry.GetAllSids();
    }

    /// <summary>Get the B-Side SID for a given base key</summary>
    public static string? GetBSideSid(string baseKey)
    {
        var metadata = GetByBaseKey(baseKey);
        if (metadata == null || !metadata.HasBSide)
            return null;

        return metadata.GetSideVariantSid("B");
    }

    /// <summary>Get the C-Side SID for a given base key</summary>
    public static string? GetCSideSid(string baseKey)
    {
        var metadata = GetByBaseKey(baseKey);
        if (metadata == null || !metadata.HasCSide)
            return null;

        return metadata.GetSideVariantSid("C");
    }

    /// <summary>Get the D-Side SID for a given base key</summary>
    public static string? GetDSideSid(string baseKey)
    {
        var metadata = GetByBaseKey(baseKey);
        if (metadata == null || !metadata.HasDSide)
            return null;

        return metadata.GetSideVariantSid("D");
    }

    /// <summary>Get all available sides for a base key</summary>
    public static List<string> GetAvailableSides(string baseKey)
    {
        var metadata = GetByBaseKey(baseKey);
        if (metadata == null)
            return new List<string>();

        var sides = new List<string> { "A" };
        if (metadata.HasBSide) sides.Add("B");
        if (metadata.HasCSide) sides.Add("C");
        if (metadata.HasDSide) sides.Add("D");

        return sides;
    }

    /// <summary>Sync metadata with AreaMapData chapter definitions</summary>
    public static void SyncWithAreaMapData()
    {
        var mainEntries = GetMainChapterEntries();
        Logger.Log(LogLevel.Info, "MaggyHelper", $"Syncing {mainEntries.Count} main chapter entries with AreaMapData");

        foreach (var entry in mainEntries)
        {
            // Log each entry for debugging
            Logger.Log(LogLevel.Debug, "MaggyHelper", 
                $"  - {entry.Sid}: Side={entry.Side}, MainEntry={entry.IsMainChapterEntry}, ShowInSelect={entry.ShowInChapterSelect}");
        }
    }

    /// <summary>Reload all metadata from disk</summary>
    public static void Reload()
    {
        MaggyMapMetadataRegistry.Reload();
        Logger.Log(LogLevel.Info, "MaggyHelper", "MaggyMapMetadata reloaded from disk");
    }
}
