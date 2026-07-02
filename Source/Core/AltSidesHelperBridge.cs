#pragma warning disable CS0436

using System;
using System.Reflection;
using MonoMod.Utils;

namespace DZ;

/// <summary>
/// Bridge between DZ's internal D-Side system (AreaModeExtender / AreaMapData)
/// and AltSidesHelper (ASH).
///
/// The two systems approach D-Sides differently:
///  - AreaModeExtender stuffs a D-Side into Mode[3] of the A-Side's AreaData so
///    vanilla chapter-panel code can iterate them.
///  - AltSidesHelper registers the D-Side as its own standalone AreaData entry,
///    then patches the A-Side panel to show an extra cassette tab.
///
/// Without a bridge both systems run in parallel, resulting in:
///  • Double D-Side tabs (one from AME, one from ASH)
///  • AME's Mode[3] MapData referencing a path that ASH has already claimed
///  • IsSideUnlocked fighting ASH's own unlock state
///
/// This bridge:
///  1. Detects whether ASH is present at runtime (graceful no-op when absent).
///  2. On AreaData.Load, suppresses AME's Mode[3] injection for chapters that ASH
///     has already claimed (IsAltSide=true meta present on the D-Side map).
///  3. Forwards ASH's unlock signal to DZModule.SaveData so the rest
///     of the codebase (IsSideUnlocked, SideLockDisplaySystem, etc.) reflects the
///     correct state without needing to know about ASH directly.
///  4. After an ASH-routed D-Side completes, marks the extended heart-gem so
///     ChapterProgressionManager can unlock the next side correctly.
/// </summary>
public static class AltSidesHelperBridge
{
    // ── State ────────────────────────────────────────────────────────────────

    private static bool _loaded;
    private static bool? _ashPresent;

    /// <summary>Set of A-Side SIDs whose D-Side is owned by AltSidesHelper.</summary>
    private static readonly HashSet<string> AshOwned0SIDs =
        new(StringComparer.OrdinalIgnoreCase);

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public static void Load()
    {
        if (_loaded) return;
        _loaded = true;

        On.Celeste.LevelExit.ctor += OnLevelExitCtor;

        Logger.Log(LogLevel.Info, "DZ", "AltSidesHelperBridge loaded");
    }

    public static void Unload()
    {
        if (!_loaded) return;
        _loaded = false;

        On.Celeste.LevelExit.ctor -= OnLevelExitCtor;

        AshOwned0SIDs.Clear();
        _ashPresent = null;

        Logger.Log(LogLevel.Info, "DZ", "AltSidesHelperBridge unloaded");
    }

    /// <summary>
    /// Called directly by AreaModeExtender.OnAreaDataLoad after all AME work is
    /// done, guaranteeing we run after both Everest and AltSidesHelper have
    /// already processed AreaData.Load.
    /// </summary>
    public static void PostAreaDataLoad()
    {
        if (!IsAshPresent())
            return;

        AshOwned0SIDs.Clear();
        ScanAndBuildOwnershipMap();
        SuppressDoubleInjection();

        Logger.Log(LogLevel.Info, "DZ",
            $"AltSidesHelperBridge: ASH owns {AshOwned0SIDs.Count} chapter(s): " +
            string.Join(", ", AshOwned0SIDs));
    }

    // ── Public helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns true when AltSidesHelper is installed and has registered a D-Side
    /// for the given A-Side SID.  Safe to call at any time after AreaData.Load.
    /// </summary>
    public static bool IsAshOwned(string 0SID)
        => AshOwned0SIDs.Contains(0SID);

    /// <summary>
    /// Returns true when AltSidesHelper is installed and has registered a D-Side
    /// for the chapter that <paramref name="area"/> belongs to.
    /// </summary>
    public static bool IsAshOwned(AreaData area)
    {
        if (area == null) return false;

        // A-Side entry: direct lookup (SID is path-based, e.g. DZ/0/01_City).
        if (AshOwned0SIDs.Contains(area.SID))
            return true;

        // D-Side standalone entry: look up the A-Side from ASH metadata.
        if (TryGetAshParentSID(area, out string parentSID))
            return AshOwned0SIDs.Contains(parentSID);

        return false;
    }

    /// <summary>
    /// True when AltSidesHelper is present in the mod list.
    /// Result is cached after the first call.
    /// </summary>
    public static bool IsAshPresent()
    {
        _ashPresent ??= IsModLoaded("AltSidesHelper");
        return _ashPresent.Value;
    }

    // ── AreaData.Load hook removed — see PostAreaDataLoad() ─────────────────

    /// <summary>
    /// Walk every loaded AreaData and find D-Side (2 folder) entries that
    /// carry AltSidesHelper's IsAltSide=true metadata.  Their declared parent
    /// A-Side SID is added to <see cref="AshOwned0SIDs"/>.
    /// </summary>
    private static void ScanAndBuildOwnershipMap()
    {
        foreach (AreaData area in AreaData.Areas)
        {
            if (!AreaModeExtender.IsOurMap(area))
                continue;

            if (!AreaModeExtender.TryParseMainSideSID(area.SID, out _, out string sideFolder))
                continue;

            if (!sideFolder.Equals("2", StringComparison.OrdinalIgnoreCase))
                continue;

            // Check ASH metadata on this standalone D-Side entry.
            if (!TryGetAshParentSID(area, out string parentSID))
                continue;

            if (!string.IsNullOrWhiteSpace(parentSID))
                AshOwned0SIDs.Add(parentSID);
        }
    }

    /// <summary>
    /// For every A-Side that ASH owns, remove Mode[3] (D-Side) that
    /// AreaModeExtender may have already injected, so ASH's own cassette tab
    /// is the only one shown.
    /// </summary>
    private static void SuppressDoubleInjection()
    {
        if (AshOwned0SIDs.Count == 0)
            return;

        foreach (AreaData area in AreaData.Areas)
        {
            if (!AreaModeExtender.IsOurMap(area))
                continue;

            if (!AreaModeExtender.TryParseMainSideSID(area.SID, out _, out string sideFolder))
                continue;

            if (!sideFolder.Equals("0", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!AshOwned0SIDs.Contains(area.SID))
                continue;

            if (area.Mode == null || area.Mode.Length <= AreaModeExtender.MODE_2)
                continue;

            // Trim Mode[] back to 3 entries (A/B/C) — ASH owns the 4th tab.
            int newLength = AreaModeExtender.MODE_2; // == 3
            var trimmed = new ModeProperties[newLength];
            Array.Copy(area.Mode, trimmed, Math.Min(area.Mode.Length, newLength));
            area.Mode = trimmed;

            Logger.Log(LogLevel.Debug, "DZ",
                $"AltSidesHelperBridge: Removed AME Mode[3] from '{area.SID}' (ASH owns D-Side).");
        }
    }

    // ── LevelExit hook — sync ASH completion → DZ save data ────────

    private static void OnLevelExitCtor(On.Celeste.LevelExit.orig_ctor orig,
        LevelExit self, LevelExit.Mode exitMode, Session session, HiresSnow snow)
    {
        orig(self, exitMode, session, snow);

        if (exitMode != LevelExit.Mode.Completed || session == null)
            return;

        if (!IsAshPresent())
            return;

        AreaData area = AreaData.Get(session.Area);
        if (!AreaModeExtender.IsOurMap(area))
            return;

        // Only handle ASH-registered D-Side completions.
        if (!AreaModeExtender.TryParseMainSideSID(area.SID, out _, out string sideFolder))
            return;

        if (!sideFolder.Equals("2", StringComparison.OrdinalIgnoreCase))
            return;

        if (!TryGetAshParentSID(area, out string parentSID) ||
            !AshOwned0SIDs.Contains(parentSID))
            return;

        // Record the D-Side heart-gem in DZ's extended save so that
        // IsSideUnlocked and ChapterProgressionManager work correctly.
        string heartId = $"{area.SID}_{AreaModeExtender.GetModeName(AreaModeExtender.MODE_2)}";
        DZModule.SaveData?.CollectHeartGem(heartId);

        // Queue the chapter panel to reopen on the D-Side tab when the overworld
        // loads, so the player isn't dropped back on the A-Side tab.
        AreaData parentArea = AreaData.Get(parentSID);
        if (parentArea != null)
            AreaModeExtender.SetPending2Return(parentArea.ID);

        Logger.Log(LogLevel.Info, "DZ",
            $"AltSidesHelperBridge: Recorded ASH D-Side completion for '{area.SID}' " +
            $"(parent: '{parentSID}').");
    }

    // ── ASH metadata reflection helpers ─────────────────────────────────────

    /// <summary>
    /// Reads the AltSidesHelper metadata on <paramref name="area"/> and returns
    /// the declared parent SID if <c>IsAltSide == true</c>.
    /// Uses pure reflection so we never take a hard compile-time dependency on ASH.
    /// </summary>
    private static bool TryGetAshParentSID(AreaData area, out string parentSID)
    {
        parentSID = null;

        if (area == null)
            return false;

        try
        {
            // ASH attaches its metadata to AreaData via Everest's EverestModuleMetadata
            // pipeline.  It stores it as AreaData's Mod extra data under the key
            // "AltSidesHelper" or directly in area.Meta via a MapMeta sub-object.
            // The most reliable path: read the YAML-loaded MapMeta that ASH injects.

            // Path 1: Everest's mod-meta via DynamicData (covers most ASH versions).
            DynamicData areaDyn = DynamicData.For(area);

            // ASH adds an "AltSideData" field/property to the AreaData's Meta object.
            object meta = TryDynGet(areaDyn, "Meta");
            if (meta != null)
            {
                DynamicData metaDyn = DynamicData.For(meta);
                object altSideData = TryDynGet(metaDyn, "AltSideData");
                if (altSideData != null)
                {
                    DynamicData asdDyn = DynamicData.For(altSideData);
                    bool isAlt = TryDynGetBool(asdDyn, "IsAltSide");
                    if (isAlt)
                    {
                        parentSID = TryDynGetString(asdDyn, "For");
                        return !string.IsNullOrWhiteSpace(parentSID);
                    }
                }
            }

            // Path 2: ASH >= 1.7 may store the data directly on the AreaData object.
            object directAltData = TryDynGet(areaDyn, "AltSideData");
            if (directAltData != null)
            {
                DynamicData asdDyn = DynamicData.For(directAltData);
                bool isAlt = TryDynGetBool(asdDyn, "IsAltSide");
                if (isAlt)
                {
                    parentSID = TryDynGetString(asdDyn, "For");
                    return !string.IsNullOrWhiteSpace(parentSID);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"AltSidesHelperBridge: Exception reading ASH meta for '{area?.SID}': {ex.Message}");
        }

        return false;
    }

    // ── Reflection micro-utilities ───────────────────────────────────────────

    private static object TryDynGet(DynamicData dyn, string key)
    {
        try
        {
            if (dyn.TryGet(key, out object val))
                return val;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Verbose, "DZ/AltSidesHelperBridge", $"TryDynGet('{key}') failed: {ex.Message}");
        }
        return null;
    }

    private static bool TryDynGetBool(DynamicData dyn, string key)
    {
        object val = TryDynGet(dyn, key);
        return val is bool b && b;
    }

    private static string TryDynGetString(DynamicData dyn, string key)
    {
        object val = TryDynGet(dyn, key);
        return val as string;
    }

    private static bool IsModLoaded(string modName)
    {
        try
        {
            foreach (EverestModule mod in Everest.Modules)
            {
                if (string.Equals(mod.Metadata?.Name, modName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ/AltSidesHelperBridge", $"IsModLoaded('{modName}') failed: {ex.Message}");
        }
        return false;
    }
}
