namespace Celeste;

/// <summary>
/// Registry for MaggyHelper map metadata files (.maggyhelper.meta.yaml)
/// Loads and provides access to map configuration data stored alongside .bin files.
/// </summary>
internal class MaggyMapMetadataRegistry : BaseMetadataRegistry<MaggyMapMetadata, MaggyMapMetadataRegistry>
{
    /// <summary>Human-readable name for logging</summary>
    protected override string RegistryName => "MaggyMapMetadata";

    /// <summary>Get the directory path for map metadata files</summary>
    protected override string GetRegistryDirectory(string modRoot)
    {
        return Path.Combine(modRoot, "Maps");
    }

    /// <summary>Called after each metadata item is deserialized</summary>
    protected override void OnItemLoaded(MaggyMapMetadata item)
    {
        if (string.IsNullOrEmpty(item.Sid))
        {
            LogWarn($"Skipping metadata entry with empty SID");
            return;
        }

        // Store by SID for lookup
        Items[item.Sid] = item;

        // Also store by base key for easier chapter-level lookups
        if (!string.IsNullOrEmpty(item.BaseKey))
        {
            string baseKey = item.BaseKey;
            if (!Items.ContainsKey(baseKey))
            {
                Items[baseKey] = item;
            }
        }
    }

    /// <summary>Get metadata by SID</summary>
    public static MaggyMapMetadata? GetBySid(string sid)
    {
        return Items.TryGetValue(sid, out var metadata) ? metadata : null;
    }

    /// <summary>Get metadata by base key (e.g., "00_Prologue")</summary>
    public static MaggyMapMetadata? GetByBaseKey(string baseKey)
    {
        return Items.TryGetValue(baseKey, out var metadata) ? metadata : null;
    }

    /// <summary>Get all metadata for a specific side (A, B, C, D)</summary>
    public static List<MaggyMapMetadata> GetBySide(string side)
    {
        return Items.Values
            .Where(m => m.Side.Equals(side, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>Get all main chapter entries</summary>
    public static List<MaggyMapMetadata> GetMainChapterEntries()
    {
        return Items.Values
            .Where(m => m.IsMainChapterEntry)
            .ToList();
    }

    /// <summary>Get all maps that should show in chapter select</summary>
    public static List<MaggyMapMetadata> GetChapterSelectMaps()
    {
        return Items.Values
            .Where(m => m.ShowInChapterSelect)
            .ToList();
    }

    /// <summary>Check if a SID exists in the registry</summary>
    public static bool HasSid(string sid)
    {
        return Items.ContainsKey(sid);
    }

    /// <summary>Get all registered SIDs</summary>
    public static IEnumerable<string> GetAllSids()
    {
        return Items.Keys;
    }

    /// <summary>Get the total count of registered metadata entries</summary>
    public static int Count => Items.Count;
}
