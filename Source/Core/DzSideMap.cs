namespace DZ;

/// <summary>
/// Canonical enum for the four DZ map sides.
/// <para>
/// Mapping (single source of truth):
/// <list type="table">
///   <listheader><term>Side</term><term>Mode (int)</term><term>Folder</term><term>Label</term></listheader>
///   <item><term>A</term><term>0</term><term>"0"</term><term>"A"</term></item>
///   <item><term>B</term><term>1</term><term>"1"</term><term>"B"</term></item>
///   <item><term>C</term><term>2</term><term>"2"</term><term>"C"</term></item>
///   <item><term>D</term><term>3</term><term>"3"</term><term>"D"</term></item>
/// </list>
/// </para>
/// </summary>
public enum DzSide
{
    A = 0,
    B = 1,
    C = 2,
    D = 3
}

/// <summary>
/// Single source of truth for DZ side/mode/folder/label/SID conversions.
/// <para>
/// All callers that need to convert between an integer mode index, a
/// <see cref="DzSide"/> value, a map-folder string ("0"–"3"), a
/// human-readable label ("A"–"D"), or a SID should go through this
/// class rather than embedding the mapping inline.
/// </para>
/// <para>
/// <b>Ownership note (2026-08-05):</b> <c>AreaModeExtender</c> and
/// <c>AreaMapData</c> both contain overlapping side/folder/label logic.
/// A future PR should migrate their internal mapping calls here. For
/// now this class is additive; the existing methods in
/// <c>AreaModeExtender</c> are NOT removed and continue to delegate to
/// or mirror this class.
/// </para>
/// </summary>
public static class DzSideMap
{
    // ── canonical constants ────────────────────────────────────────────────

    /// <summary>Total number of sides (A/B/C/D = 4).</summary>
    public const int SideCount = 4;

    /// <summary>The mod's SID prefix, shared with <see cref="AreaModeExtender.MAP_PREFIX"/>.</summary>
    public const string MapPrefix = "DZ";

    // ── internal tables ───────────────────────────────────────────────────

    // Indexed by (int)DzSide
    private static readonly string[] _folders = { "0", "1", "2", "3" };
    private static readonly string[] _labels  = { "A", "B", "C", "D" };
    private static readonly char[]   _chars   = { 'A', 'B', 'C', 'D' };

    // ── DzSide ↔ mode index ───────────────────────────────────────────────

    /// <summary>Returns the <see cref="DzSide"/> for a raw mode integer (0–3).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mode"/> is outside 0–3.</exception>
    public static DzSide FromMode(int mode)
    {
        if (mode < 0 || mode >= SideCount)
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "DZ mode must be 0–3 (A–D).");
        return (DzSide)mode;
    }

    /// <summary>
    /// Returns the <see cref="DzSide"/> for a raw mode integer, clamping to A
    /// when the value is out of range and logging a warning.
    /// </summary>
    public static DzSide FromModeSafe(int mode)
    {
        if (mode < 0 || mode >= SideCount)
        {
            Logger.Log(LogLevel.Warn, "DZ/DzSideMap",
                $"Unknown mode index {mode}; expected 0–3. Falling back to A-Side.");
            return DzSide.A;
        }
        return (DzSide)mode;
    }

    /// <summary>Returns the integer mode index (0–3) for a <see cref="DzSide"/>.</summary>
    public static int ToMode(DzSide side) => (int)side;

    // ── DzSide ↔ folder ───────────────────────────────────────────────────

    /// <summary>Returns the map-folder string ("0"–"3") for a <see cref="DzSide"/>.</summary>
    public static string ToFolder(DzSide side) => _folders[(int)side];

    /// <summary>Returns the map-folder string for a raw mode integer.</summary>
    public static string FolderFromMode(int mode) => ToFolder(FromModeSafe(mode));

    // ── DzSide ↔ label ────────────────────────────────────────────────────

    /// <summary>Returns the human-readable single-character label ("A"/"B"/"C"/"D").</summary>
    public static string ToLabel(DzSide side) => _labels[(int)side];

    /// <summary>Returns the human-readable label for a raw mode integer.</summary>
    public static string LabelFromMode(int mode) => ToLabel(FromModeSafe(mode));

    // ── DzSide ↔ char ─────────────────────────────────────────────────────

    /// <summary>Returns the side character ('A'/'B'/'C'/'D') for a <see cref="DzSide"/>.</summary>
    public static char ToChar(DzSide side) => _chars[(int)side];

    /// <summary>Returns the side character for a raw mode integer.</summary>
    public static char CharFromMode(int mode) => ToChar(FromModeSafe(mode));

    // ── SID construction ──────────────────────────────────────────────────

    /// <summary>
    /// Builds a canonical DZ SID of the form <c>DZ/{folder}/{mapName}</c>.
    /// <para>Example: <c>BuildSid(DzSide.D, "01_City")</c> → <c>"DZ/3/01_City"</c>.</para>
    /// </summary>
    public static string BuildSid(DzSide side, string mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName))
            return MapPrefix;
        return $"{MapPrefix}/{ToFolder(side)}/{mapName}";
    }

    /// <summary>Builds a canonical DZ SID using a raw mode integer.</summary>
    public static string BuildSidFromMode(int mode, string mapName)
        => BuildSid(FromModeSafe(mode), mapName);

    // ── SID parsing ───────────────────────────────────────────────────────

    /// <summary>
    /// Parses a DZ SID of the form <c>DZ/{folder}/{mapName}</c> and returns the
    /// <see cref="DzSide"/> and bare map name.
    /// </summary>
    /// <returns><c>true</c> if the SID is a recognised DZ SID; <c>false</c> otherwise.</returns>
    public static bool TryParseSid(string sid, out DzSide side, out string mapName)
    {
        side    = DzSide.A;
        mapName = string.Empty;

        if (string.IsNullOrEmpty(sid))
            return false;

        // Expected format: "DZ/{folder}/{mapName}"
        if (!sid.StartsWith(MapPrefix + "/", StringComparison.OrdinalIgnoreCase))
            return false;

        var rest = sid.Substring(MapPrefix.Length + 1); // "{folder}/{mapName}"
        var slash = rest.IndexOf('/');
        if (slash <= 0)
            return false;

        var folder = rest.Substring(0, slash);
        mapName    = rest.Substring(slash + 1);

        for (int i = 0; i < _folders.Length; i++)
        {
            if (string.Equals(_folders[i], folder, StringComparison.OrdinalIgnoreCase))
            {
                side = (DzSide)i;
                return true;
            }
        }

        return false;
    }

    // ── deterministic self-check ──────────────────────────────────────────

    /// <summary>
    /// Runs deterministic self-checks of the mapping tables. Call from the
    /// sprite/editor contract validator or any dev build diagnostic to verify
    /// no table has drifted out of sync.
    /// </summary>
    /// <returns>A list of failure messages; empty on success.</returns>
    public static IReadOnlyList<string> SelfCheck()
    {
        var failures = new List<string>();

        var expectedFolders = new[] { "0", "1", "2", "3" };
        var expectedLabels  = new[] { "A", "B", "C", "D" };
        var expectedChars   = new[] { 'A', 'B', 'C', 'D' };

        for (int i = 0; i < SideCount; i++)
        {
            var side = (DzSide)i;

            if (ToMode(side) != i)
                failures.Add($"DzSideMap: ToMode({side}) = {ToMode(side)}, expected {i}");

            if (ToFolder(side) != expectedFolders[i])
                failures.Add($"DzSideMap: ToFolder({side}) = \"{ToFolder(side)}\", expected \"{expectedFolders[i]}\"");

            if (ToLabel(side) != expectedLabels[i])
                failures.Add($"DzSideMap: ToLabel({side}) = \"{ToLabel(side)}\", expected \"{expectedLabels[i]}\"");

            if (ToChar(side) != expectedChars[i])
                failures.Add($"DzSideMap: ToChar({side}) = '{ToChar(side)}', expected '{expectedChars[i]}'");

            // Round-trip: mode → side → mode
            var roundTrip = ToMode(FromMode(i));
            if (roundTrip != i)
                failures.Add($"DzSideMap: round-trip FromMode({i}) → ToMode gave {roundTrip}");

            // SID build + parse round-trip
            var testMap = $"01_Test";
            var builtSid = BuildSid(side, testMap);
            if (!TryParseSid(builtSid, out var parsedSide, out var parsedMap))
            {
                failures.Add($"DzSideMap: TryParseSid(\"{builtSid}\") returned false");
            }
            else
            {
                if (parsedSide != side)
                    failures.Add($"DzSideMap: TryParseSid(\"{builtSid}\").side = {parsedSide}, expected {side}");
                if (parsedMap != testMap)
                    failures.Add($"DzSideMap: TryParseSid(\"{builtSid}\").mapName = \"{parsedMap}\", expected \"{testMap}\"");
            }
        }

        // Verify CharFromMode(3) == 'D' — this is the specific bug that was present in
        // TapeManager.getCurrentSide() which defaulted mode 3 to 'A'.
        var dChar = CharFromMode(3);
        if (dChar != 'D')
            failures.Add($"DzSideMap: CharFromMode(3) = '{dChar}', expected 'D' (D-Side bug regression)");

        return failures;
    }
}
