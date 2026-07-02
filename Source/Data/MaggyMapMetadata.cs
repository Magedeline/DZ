#nullable enable
namespace DZ;

/// <summary>
/// Metadata for DZ map files (.DZ.meta.yaml)
/// Contains chapter selection and map configuration data.
/// </summary>
internal class DZMapMetadata
{
    /// <summary>Schema version identifier</summary>
    public string Schema { get; set; } = "DZ-map-meta-v1";

    /// <summary>Full SID of the map (e.g., "DZ/0/00_Prologue")</summary>
    public string Sid { get; set; } = string.Empty;

    /// <summary>Base map key without side prefix (e.g., "00_Prologue")</summary>
    public string BaseKey { get; set; } = string.Empty;

    /// <summary>Side identifier (A, B, C, D, etc.)</summary>
    public string Side { get; set; } = "A";

    /// <summary>Mode index for the map</summary>
    public int ModeIndex { get; set; } = 0;

    /// <summary>Whether this is the main entry point for the chapter</summary>
    public bool IsMainChapterEntry { get; set; } = true;

    /// <summary>SID of the main chapter this map belongs to</summary>
    public string MainChapterSid { get; set; } = string.Empty;

    /// <summary>Whether to show this map in chapter select</summary>
    public bool ShowInChapterSelect { get; set; } = true;

    /// <summary>Optional display name override</summary>
    public string? DisplayName { get; set; }

    /// <summary>Optional chapter number override</summary>
    public int? ChapterNumber { get; set; }

    /// <summary>Whether this map has a B-Side variant</summary>
    public bool Has1 { get; set; } = false;

    /// <summary>Whether this map has a C-Side variant</summary>
    public bool Has2 { get; set; } = false;

    /// <summary>Whether this map has a D-Side variant</summary>
    public bool HasDSide { get; set; } = false;

    /// <summary>Music events to play on chapter completion by side</summary>
    public List<string>? MusicBySide { get; set; }

    /// <summary>Title text to display on chapter completion by side</summary>
    public Dictionary<string, string>? Title { get; set; }

    /// <summary>Get the SID for a specific side variant</summary>
    public string GetSideVariantSid(string side)
    {
        return $"{Sid.Substring(0, Sid.LastIndexOf('/') + 1)}{side}/{BaseKey}";
    }
}
