namespace DZ;

/// <summary>
/// Structured logging helper for side-selection diagnostics.
/// <para>
/// All methods are no-ops unless <c>DZModuleSettings.EnableSideDiagnostics</c> is
/// <c>true</c>, so there is zero runtime cost when the setting is off.  Do NOT
/// call these methods from hot inner loops; they are intended for lifecycle /
/// selection / collection transition points only.
/// </para>
/// <para>
/// <b>Enabling diagnostics:</b>
/// Open the in-game mod settings menu (or edit saves/modsettings-DZ.yaml),
/// set <c>EnableSideDiagnostics: true</c>, then reproduce the area/side
/// selection path you want to inspect.  All entries are logged under the
/// category prefix <c>DZ/SideDiag</c>.
/// </para>
/// </summary>
public static class SideDiagnostics
{
    private const string LogCategory = "DZ/SideDiag";

    /// <summary>Returns <c>true</c> when diagnostics are currently enabled.</summary>
    public static bool IsEnabled =>
        Celeste.Mod.DZ.DZModule.Instance != null && Celeste.Mod.DZ.DZModule.Settings.EnableSideDiagnostics;

    // ── lifecycle boundary helpers ────────────────────────────────────────

    /// <summary>
    /// Log the area SID and <see cref="AreaKey.Mode"/> at a level/area entry
    /// boundary (e.g. level load, chapter panel reset).
    /// </summary>
    public static void LogAreaEntry(string context, string sid, int mode)
    {
        if (!IsEnabled) return;
        var side = DzSideMap.FromModeSafe(mode);
        Logger.Log(LogLevel.Info, LogCategory,
            $"[{context}] area={sid} mode={mode} side={DzSideMap.ToLabel(side)} folder={DzSideMap.ToFolder(side)}");
    }

    /// <summary>
    /// Log mode-array inspection data (length and which slots are non-null).
    /// Useful for diagnosing "missing D-Side" issues caused by a truncated
    /// <c>AreaStats.Modes</c> array.
    /// </summary>
    public static void LogModeArray(string context, string sid, int[] modesLength, bool[] nonNull)
    {
        if (!IsEnabled) return;
        var slots = string.Join(",", Enumerable.Range(0, modesLength.Length)
            .Select(i => nonNull[i] ? i.ToString() : $"({i}:null)"));
        Logger.Log(LogLevel.Info, LogCategory,
            $"[{context}] area={sid} mode-array length={modesLength.Length} slots=[{slots}]");
    }

    /// <summary>
    /// Log the mode-array length from a save's <c>AreaStats</c> object, plus the
    /// length currently expected for the area.
    /// </summary>
    public static void LogSaveAreaStats(string context, string sid, int saveModesLength, int expectedModes)
    {
        if (!IsEnabled) return;
        var status = saveModesLength >= expectedModes ? "OK" : "SHORT";
        Logger.Log(LogLevel.Info, LogCategory,
            $"[{context}] area={sid} save-modes={saveModesLength} expected={expectedModes} [{status}]");
    }

    /// <summary>
    /// Log the currently selected chapter-panel option/mode (OuiChapterPanel
    /// option index and associated mode index).
    /// </summary>
    public static void LogPanelOption(string context, string sid, int optionIndex, int optionMode)
    {
        if (!IsEnabled) return;
        var side = DzSideMap.FromModeSafe(optionMode);
        Logger.Log(LogLevel.Info, LogCategory,
            $"[{context}] area={sid} panel-option={optionIndex} option-mode={optionMode} side={DzSideMap.ToLabel(side)}");
    }

    /// <summary>
    /// Log a collectible transition (cassette/tape collect, heart gem collect,
    /// berry collect) with side context.
    /// </summary>
    public static void LogCollectibleTransition(string context, string collectibleType, string sid, int mode)
    {
        if (!IsEnabled) return;
        var side = DzSideMap.FromModeSafe(mode);
        Logger.Log(LogLevel.Info, LogCategory,
            $"[{context}] collect={collectibleType} area={sid} mode={mode} side={DzSideMap.ToLabel(side)}");
    }

    /// <summary>
    /// Log a free-form diagnostic message at Info level.  Use sparingly and
    /// only at genuine lifecycle boundaries, not per-frame.
    /// </summary>
    public static void Log(string context, string message)
    {
        if (!IsEnabled) return;
        Logger.Log(LogLevel.Info, LogCategory, $"[{context}] {message}");
    }
}
