#pragma warning disable CS0436

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DZ;
using MonoMod.Utils;

namespace DZ;

/// <summary>
/// ╔══════════════════════════════════════════════════════════════════════╗
/// ║  SubChapterManager — EXPERIMENTAL / TEST ONLY                       ║
/// ╚══════════════════════════════════════════════════════════════════════╝
///
/// Implements a "sub-chapter" system: a single parent checkpoint can host
/// between 5 and 20 independent collab-style maps, each playable as a
/// self-contained sub-level. This is conceptually similar to how
/// CollabUtils2 handles lobby-to-map transitions, but operates entirely
/// within the AreaModeExtender framework rather than requiring a separate
/// mod dependency.
///
/// ┌──────────────────────────────────────────────────────────────────────┐
/// │  RESEARCH: How the Celeste mod community achieves similar things    │
/// │                                                                      │
/// │  1. VANILLA CELESTE — AreaData.Mode + ModeProperties.Checkpoints    │
/// │     • Each chapter has up to 3 modes (A/B/C), each with an array    │
/// │       of CheckpointData defining spawn rooms within that side.      │
/// │     • Vanilla caps at 3 modes (AreaMode enum: Normal/BSide/CSide). │
/// │     • Checkpoints are just named rooms — the engine handles respawn │
/// │       and session state per checkpoint automatically.               │
/// │                                                                      │
/// │  2. ALTSIDES HELPER (ASH) — l-Luna/AltSidesHelper                  │
/// │     • Registers alt-sides as standalone AreaData entries, then      │
/// │       patches OuiChapterPanel to show extra cassette tabs.          │
/// │     • Uses YAML metadata (mapname.altsideshelper.meta.yaml) with    │
/// │       IsAltSide=true and For=parent_SID to link child→parent.      │
/// │     • Supports arbitrary side labels, custom heart icons/colors,    │
/// │       and per-side unlock triggers or cassette collectibles.        │
/// │     • DZ bridges ASH via AltSidesHelperBridge (reflection  │
/// │       so no hard compile-time dependency).                          │
/// │                                                                      │
/// │  3. COLLABUTILS2 — EverestAPI/CelesteCollabUtils2                   │
/// │     • Structures collabs as: Maps/CollabName/0-Lobbies/lobby.bin    │
/// │       with sub-folders per lobby containing individual map .bins.   │
/// │     • A ChapterPanelTrigger entity in the lobby opens the standard  │
/// │       chapter panel for any target map, with custom author/credits. │
/// │     • Uses "Return to Lobby" system: pause menu → back to lobby.   │
/// │     • Mini Hearts gate progress; Silver Berries add challenge.      │
/// │     • CollabUtils2CollabID.txt marks the mod as a collab so         │
/// │       Everest treats all lobby entries as pre-unlocked chapters.    │
/// │     • Lazy loading defers map parsing until player enters.          │
/// │                                                                      │
/// │  4. CELESTERANDOMIZER — rhelmot/CelesteRandomizer                   │
/// │     • Treats each room as a "building block" and procedurally       │
/// │       stitches them together based on transition compatibility.     │
/// │     • Dynamically creates AreaData at runtime, similar to           │
/// │       DZ's PCGAreaRegistrar.                               │
/// │                                                                      │
/// │  5. DZ (THIS MOD) — AreaModeExtender                       │
/// │     • Extends Mode[] beyond vanilla's 3 to support D-Side (3) and  │
/// │       DX-Side (4), patching AreaStats.Clone, Session.ctor, and     │
/// │       SaveData.AfterInitialize to handle the extra slots safely.   │
/// │     • AltSidesHelperBridge detects ASH ownership and suppresses    │
/// │       double-injection of Mode[3] when ASH already claims it.      │
/// │     • PCGAreaRegistrar dynamically injects generated .bin maps as   │
/// │       playable AreaData entries, CelesteRandomizer-style.           │
/// │                                                                      │
/// │  NEW: SUB-CHAPTER SYSTEM                                            │
/// │     • A parent chapter checkpoint acts as a "lobby" that can host   │
/// │       5–20 independent collab-style sub-maps.                       │
/// │     • Each sub-chapter is a separate .bin with its own rooms,       │
/// │       entities, and difficulty, but appears under ONE checkpoint    │
/// │       in the parent chapter's panel.                                │
/// │     • On completion, the player returns to the parent checkpoint.   │
/// │     • Progress is tracked per-sub-chapter in DZ save data. │
/// │     • This avoids bloating the chapter select screen with dozens    │
/// │       of entries (the CollabUtils2 problem for small collabs).      │
/// └──────────────────────────────────────────────────────────────────────┘
///
/// <para><b>Architecture:</b></para>
/// <list type="bullet">
///   <item>SubChapterDef — metadata for a single sub-chapter (SID, display name, difficulty, author)</item>
///   <item>SubChapterGroup — a named group of 5–20 sub-chapters under one parent checkpoint</item>
///   <item>SubChapterManager — registry, lifecycle hooks, save tracking, and warp helpers</item>
///   <item>SubChapterPanelTrigger — in-game entity (like CollabUtils2's ChapterPanelTrigger)
///         that opens the sub-chapter selection UI when the player interacts</item>
/// </list>
/// </summary>
public static class SubChapterManager
{
    private const string LogTag = "SubChapterManager";

    // ── Constraints ─────────────────────────────────────────────────────
    public const int MIN_SUB_CHAPTERS = 5;
    public const int MAX_SUB_CHAPTERS = 20;

    // ── State ───────────────────────────────────────────────────────────
    private static bool _loaded;
    private static readonly Dictionary<string, SubChapterGroup> _groups = new(StringComparer.OrdinalIgnoreCase);

    // ── Data Types ──────────────────────────────────────────────────────

    /// <summary>
    /// A single sub-chapter: an independent map living under a parent checkpoint.
    /// Analogous to one map entry inside a CollabUtils2 lobby folder.
    /// </summary>
    public class SubChapterDef
    {
        /// <summary>Unique identifier (e.g. "DZ/ASide/01_City_Sub01")</summary>
        public string SID { get; set; }

        /// <summary>Display name shown in the sub-chapter selection panel</summary>
        public string DisplayName { get; set; }

        /// <summary>Map author credit</summary>
        public string Author { get; set; }

        /// <summary>Difficulty rating 1–5 (shown as skull icons)</summary>
        public int Difficulty { get; set; } = 2;

        /// <summary>
        /// Path to the .bin file, relative to the mod's Maps/ folder.
        /// Null if this sub-chapter reuses rooms embedded in the parent map.
        /// </summary>
        public string MapBinPath { get; set; }

        /// <summary>
        /// If true, the sub-chapter is a standalone .bin that gets registered
        /// as a hidden AreaData entry (like CollabUtils2 collab maps).
        /// If false, the sub-chapter is a named room range within the parent map.
        /// </summary>
        public bool IsStandalone { get; set; } = true;

        /// <summary>
        /// For non-standalone sub-chapters: the first room name in the parent map.
        /// The engine teleports the player to this room when entering.
        /// </summary>
        public string EntryRoom { get; set; }

        /// <summary>Icon path for the sub-chapter card (GUI atlas path)</summary>
        public string IconPath { get; set; }

        /// <summary>Optional: music event override for this sub-chapter</summary>
        public string MusicEvent { get; set; }

        /// <summary>Optional: heart gem ID to track collection</summary>
        public string HeartGemId { get; set; }
    }

    /// <summary>
    /// A group of sub-chapters living under one parent checkpoint.
    /// Analogous to a CollabUtils2 lobby + its map folder.
    /// </summary>
    public class SubChapterGroup
    {
        /// <summary>Unique group key (e.g. "Ch01_CollabPack")</summary>
        public string GroupKey { get; set; }

        /// <summary>Display name (shown as the checkpoint name in the parent chapter)</summary>
        public string DisplayName { get; set; }

        /// <summary>The parent chapter's A-Side SID</summary>
        public string ParentSID { get; set; }

        /// <summary>
        /// Which mode index this group lives under (0=A, 1=B, etc.).
        /// Typically 0 for collab packs integrated into the A-Side.
        /// </summary>
        public int ParentModeIndex { get; set; }

        /// <summary>The sub-chapters in this group (5–20)</summary>
        public List<SubChapterDef> SubChapters { get; set; } = new();

        /// <summary>
        /// Room name in the parent map that acts as the "lobby" for this group.
        /// Returning from a sub-chapter warps the player here.
        /// </summary>
        public string LobbyRoom { get; set; }

        /// <summary>
        /// Number of sub-chapters that must be completed to "clear" this group.
        /// 0 = all required; N = only N needed (like CollabUtils2 mini hearts).
        /// </summary>
        public int RequiredCompletions { get; set; }
    }

    // ── Lifecycle ───────────────────────────────────────────────────────

    public static void Load()
    {
        if (_loaded) return;
        _loaded = true;

        On.Celeste.LevelExit.ctor += OnLevelExitCtor;
        On.Celeste.LevelEnter.Go += OnLevelEnterGo;

        Logger.Log(LogLevel.Info, LogTag, "SubChapterManager loaded (TEST MODE)");
    }

    public static void Unload()
    {
        if (!_loaded) return;
        _loaded = false;

        On.Celeste.LevelExit.ctor -= OnLevelExitCtor;
        On.Celeste.LevelEnter.Go -= OnLevelEnterGo;

        _groups.Clear();

        Logger.Log(LogLevel.Info, LogTag, "SubChapterManager unloaded");
    }

    // ── Registration ────────────────────────────────────────────────────

    /// <summary>
    /// Registers a sub-chapter group. Validates constraints (5–20 sub-chapters).
    /// Call during or after AreaData.Load.
    /// </summary>
    public static bool RegisterGroup(SubChapterGroup group)
    {
        if (group == null)
        {
            Logger.Log(LogLevel.Warn, LogTag, "Cannot register null group");
            return false;
        }

        if (string.IsNullOrWhiteSpace(group.GroupKey))
        {
            Logger.Log(LogLevel.Warn, LogTag, "Group must have a GroupKey");
            return false;
        }

        int count = group.SubChapters?.Count ?? 0;
        if (count < MIN_SUB_CHAPTERS || count > MAX_SUB_CHAPTERS)
        {
            Logger.Log(LogLevel.Warn, LogTag,
                $"Group '{group.GroupKey}' has {count} sub-chapters; " +
                $"must be between {MIN_SUB_CHAPTERS} and {MAX_SUB_CHAPTERS}");
            return false;
        }

        _groups[group.GroupKey] = group;

        // Register standalone sub-chapter .bins as hidden AreaData entries
        foreach (var sub in group.SubChapters.Where(s => s.IsStandalone))
        {
            RegisterStandaloneSubChapter(sub, group);
        }

        Logger.Log(LogLevel.Info, LogTag,
            $"Registered group '{group.GroupKey}' with {count} sub-chapters " +
            $"(parent: {group.ParentSID})");
        return true;
    }

    /// <summary>
    /// Unregisters a sub-chapter group by key.
    /// </summary>
    public static bool UnregisterGroup(string groupKey)
    {
        return _groups.Remove(groupKey);
    }

    /// <summary>Returns all registered groups.</summary>
    public static IReadOnlyDictionary<string, SubChapterGroup> GetAllGroups() => _groups;

    /// <summary>Returns a specific group by key.</summary>
    public static SubChapterGroup GetGroup(string groupKey)
    {
        return _groups.TryGetValue(groupKey, out var g) ? g : null;
    }

    /// <summary>
    /// Finds which group (if any) a sub-chapter SID belongs to.
    /// </summary>
    public static (SubChapterGroup group, SubChapterDef subChapter)? FindSubChapterBySID(string sid)
    {
        if (string.IsNullOrWhiteSpace(sid))
            return null;

        foreach (var kvp in _groups)
        {
            var sub = kvp.Value.SubChapters?.FirstOrDefault(
                s => string.Equals(s.SID, sid, StringComparison.OrdinalIgnoreCase));
            if (sub != null)
                return (kvp.Value, sub);
        }

        return null;
    }

    // ── Save Data Tracking ──────────────────────────────────────────────

    /// <summary>
    /// Returns true if the given sub-chapter has been completed in the current save.
    /// Uses DZ's extended save data (achievement system).
    /// </summary>
    public static bool IsSubChapterCompleted(string subChapterSID)
    {
        string key = $"subchapter_completed_{subChapterSID}";
        return DZModule.SaveData?.HasAchievement(key) == true;
    }

    /// <summary>
    /// Marks a sub-chapter as completed in the current save.
    /// </summary>
    public static void MarkSubChapterCompleted(string subChapterSID)
    {
        string key = $"subchapter_completed_{subChapterSID}";
        DZModule.SaveData?.UnlockAchievement(key);
        Logger.Log(LogLevel.Info, LogTag, $"Sub-chapter completed: {subChapterSID}");
    }

    /// <summary>
    /// Returns the number of completed sub-chapters in a group.
    /// </summary>
    public static int GetCompletedCount(string groupKey)
    {
        var group = GetGroup(groupKey);
        if (group?.SubChapters == null)
            return 0;

        return group.SubChapters.Count(s => IsSubChapterCompleted(s.SID));
    }

    /// <summary>
    /// Returns true if the group's completion requirement is met.
    /// </summary>
    public static bool IsGroupCleared(string groupKey)
    {
        var group = GetGroup(groupKey);
        if (group == null)
            return false;

        int completed = GetCompletedCount(groupKey);
        int required = group.RequiredCompletions > 0
            ? group.RequiredCompletions
            : group.SubChapters.Count;

        return completed >= required;
    }

    // ── Hooks ───────────────────────────────────────────────────────────

    /// <summary>
    /// On level completion: if the completed map is a standalone sub-chapter,
    /// mark it as done and schedule a return to the parent lobby room.
    /// </summary>
    private static void OnLevelExitCtor(On.Celeste.LevelExit.orig_ctor orig,
        LevelExit self, LevelExit.Mode mode, Session session, HiresSnow snow)
    {
        orig(self, mode, session, snow);

        if (mode != LevelExit.Mode.Completed || session == null)
            return;

        AreaData area = AreaData.Get(session.Area);
        if (area == null)
            return;

        var match = FindSubChapterBySID(area.SID);
        if (match == null)
            return;

        var (group, subChapter) = match.Value;

        // Mark completed
        MarkSubChapterCompleted(subChapter.SID);

        // Collect heart gem if defined
        if (!string.IsNullOrWhiteSpace(subChapter.HeartGemId))
            DZModule.SaveData?.CollectHeartGem(subChapter.HeartGemId);

        // Schedule return to parent lobby room
        if (!string.IsNullOrWhiteSpace(group.LobbyRoom) &&
            !string.IsNullOrWhiteSpace(group.ParentSID))
        {
            ScheduleReturnToLobby(group.ParentSID, group.ParentModeIndex, group.LobbyRoom);
        }

        Logger.Log(LogLevel.Info, LogTag,
            $"Sub-chapter '{subChapter.DisplayName}' completed " +
            $"({GetCompletedCount(group.GroupKey)}/{group.SubChapters.Count} in '{group.GroupKey}')");
    }

    /// <summary>
    /// On level enter: if entering a sub-chapter's parent map with a pending
    /// lobby return, override the spawn room.
    /// </summary>
    private static void OnLevelEnterGo(On.Celeste.LevelEnter.orig_Go orig,
        Session session, bool fromSaveData)
    {
        if (_pendingReturn != null && session != null)
        {
            AreaData area = AreaData.Get(session.Area);
            if (area != null &&
                string.Equals(area.SID, _pendingReturn.Value.parentSID, StringComparison.OrdinalIgnoreCase) &&
                (int)session.Area.Mode == _pendingReturn.Value.modeIndex)
            {
                // Override the starting room to the lobby
                session.Level = _pendingReturn.Value.lobbyRoom;
                session.RespawnPoint = null; // Let the engine find the spawn
                _pendingReturn = null;

                Logger.Log(LogLevel.Info, LogTag,
                    $"Returning to lobby room '{session.Level}' in '{area.SID}'");
            }
        }

        orig(session, fromSaveData);
    }

    // ── Internal Helpers ────────────────────────────────────────────────

    private static (string parentSID, int modeIndex, string lobbyRoom)? _pendingReturn;

    private static void ScheduleReturnToLobby(string parentSID, int modeIndex, string lobbyRoom)
    {
        _pendingReturn = (parentSID, modeIndex, lobbyRoom);
        Logger.Log(LogLevel.Debug, LogTag,
            $"Scheduled return to lobby: {parentSID} mode {modeIndex} room '{lobbyRoom}'");
    }

    /// <summary>
    /// Registers a standalone sub-chapter .bin as a hidden AreaData entry.
    /// This mirrors CollabUtils2's approach: each map in a lobby folder
    /// becomes its own AreaData, but is hidden from the chapter select screen.
    ///
    /// The approach used here is similar to PCGAreaRegistrar but marks the
    /// entry as an interlude (hidden from chapter select) and tags it with
    /// sub-chapter metadata for the parent group.
    /// </summary>
    private static void RegisterStandaloneSubChapter(SubChapterDef sub, SubChapterGroup group)
    {
        if (string.IsNullOrWhiteSpace(sub.SID))
            return;

        // Check if already registered by Everest's normal .bin scanning
        try
        {
            if (AreaData.Get(sub.SID) != null)
            {
                Logger.Log(LogLevel.Verbose, LogTag,
                    $"Sub-chapter '{sub.SID}' already registered by Everest");
                TagSubChapterAreaData(sub, group);
                return;
            }
        }
        catch
        {
            // Not found — we'll register it dynamically if a .bin path is provided
        }

        if (string.IsNullOrWhiteSpace(sub.MapBinPath))
        {
            Logger.Log(LogLevel.Warn, LogTag,
                $"Sub-chapter '{sub.SID}' is standalone but has no MapBinPath — skipping registration");
            return;
        }

        // Dynamic registration would happen here for maps not already scanned
        // by Everest. For this test implementation, we rely on the .bin being in
        // the Maps/ folder so Everest picks it up automatically.
        Logger.Log(LogLevel.Info, LogTag,
            $"Sub-chapter '{sub.SID}' should be placed in Maps/ for Everest auto-scan " +
            $"(bin: {sub.MapBinPath})");
    }

    /// <summary>
    /// Tags an already-loaded AreaData with sub-chapter metadata so other
    /// systems (chapter panel, save data) can identify it as part of a group.
    /// </summary>
    private static void TagSubChapterAreaData(SubChapterDef sub, SubChapterGroup group)
    {
        try
        {
            AreaData area = AreaData.Get(sub.SID);
            if (area == null) return;

            DynamicData dyn = DynamicData.For(area);
            dyn.Set("SubChapterGroupKey", group.GroupKey);
            dyn.Set("SubChapterParentSID", group.ParentSID);
            dyn.Set("SubChapterIsHidden", true);

            // Hide from chapter select by marking as interlude
            // (CollabUtils2 uses a similar approach for collab maps)
            area.Interlude_Safe = true;

            Logger.Log(LogLevel.Verbose, LogTag,
                $"Tagged '{sub.SID}' as sub-chapter of group '{group.GroupKey}'");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, LogTag,
                $"Failed to tag sub-chapter '{sub.SID}': {ex.Message}");
        }
    }

    // ── Warp Helper ─────────────────────────────────────────────────────

    /// <summary>
    /// Warps the player from the current level into a sub-chapter.
    /// Similar to CollabUtils2's ChapterPanelTrigger → LevelLoader flow.
    /// </summary>
    public static void WarpToSubChapter(Level fromLevel, SubChapterDef sub)
    {
        if (fromLevel == null || sub == null)
            return;

        if (sub.IsStandalone)
        {
            // Standalone: create a new session for the sub-chapter's AreaData
            try
            {
                AreaData subArea = AreaData.Get(sub.SID);
                if (subArea == null)
                {
                    Logger.Log(LogLevel.Warn, LogTag,
                        $"Cannot warp to sub-chapter '{sub.SID}': AreaData not found");
                    return;
                }

                var session = new Session(new AreaKey(subArea.ID));
                session.StartedFromBeginning = true;
                session.FirstLevel = true;

                Engine.Scene = new LevelLoader(session);

                Logger.Log(LogLevel.Info, LogTag,
                    $"Warping to standalone sub-chapter '{sub.DisplayName}' ({sub.SID})");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LogTag,
                    $"Warp to sub-chapter failed: {ex.Message}");
            }
        }
        else
        {
            // Embedded: teleport within the current map to the entry room
            if (string.IsNullOrWhiteSpace(sub.EntryRoom))
            {
                Logger.Log(LogLevel.Warn, LogTag,
                    $"Cannot warp to embedded sub-chapter '{sub.SID}': no EntryRoom set");
                return;
            }

            fromLevel.Session.Level = sub.EntryRoom;
            fromLevel.Session.RespawnPoint = null;
            fromLevel.Reload();

            Logger.Log(LogLevel.Info, LogTag,
                $"Warping to embedded sub-chapter room '{sub.EntryRoom}'");
        }
    }

    // ── Test Registration ───────────────────────────────────────────────

    /// <summary>
    /// Creates and registers a test group for development purposes.
    /// Call via console command: DZ_test_subchapters
    /// </summary>
    public static void RegisterTestGroup()
    {
        var group = new SubChapterGroup
        {
            GroupKey = "TestCollabPack",
            DisplayName = "Test Collab Pack",
            ParentSID = AreaModeExtender.BuildASideSID("01_City"),
            ParentModeIndex = AreaModeExtender.MODE_NORMAL,
            LobbyRoom = "a-00",
            RequiredCompletions = 3, // Only need 3 of 5 to clear
            SubChapters = new List<SubChapterDef>()
        };

        // Generate 5 test sub-chapter definitions
        for (int i = 1; i <= 5; i++)
        {
            group.SubChapters.Add(new SubChapterDef
            {
                SID = $"DZ/ASide/01_City_SubTest{i:D2}",
                DisplayName = $"Test Sub-Chapter {i}",
                Author = "DZ Test",
                Difficulty = Math.Clamp(i, 1, 5),
                IsStandalone = false, // Embedded — uses rooms in the parent map
                EntryRoom = $"subtest-{i:D2}",
                HeartGemId = $"test_sub_{i}_heart"
            });
        }

        if (RegisterGroup(group))
        {
            Logger.Log(LogLevel.Info, LogTag,
                "Test sub-chapter group registered. " +
                "Use 'DZ_test_subchapter_status' to check progress.");
        }
    }

    /// <summary>
    /// Logs the status of all registered sub-chapter groups.
    /// Console command: DZ_test_subchapter_status
    /// </summary>
    public static void LogStatus()
    {
        if (_groups.Count == 0)
        {
            Logger.Log(LogLevel.Info, LogTag, "No sub-chapter groups registered.");
            Engine.Commands?.Log("No sub-chapter groups registered.");
            return;
        }

        foreach (var kvp in _groups)
        {
            var g = kvp.Value;
            int completed = GetCompletedCount(g.GroupKey);
            bool cleared = IsGroupCleared(g.GroupKey);

            string line = $"[{g.GroupKey}] '{g.DisplayName}' — " +
                          $"{completed}/{g.SubChapters.Count} completed " +
                          $"(need {(g.RequiredCompletions > 0 ? g.RequiredCompletions : g.SubChapters.Count)}) " +
                          $"{(cleared ? "✓ CLEARED" : "")}";

            Logger.Log(LogLevel.Info, LogTag, line);
            Engine.Commands?.Log(line);

            foreach (var sub in g.SubChapters)
            {
                bool done = IsSubChapterCompleted(sub.SID);
                string subLine = $"  [{(done ? "X" : " ")}] {sub.DisplayName} " +
                                 $"(diff:{sub.Difficulty}) by {sub.Author}";
                Logger.Log(LogLevel.Info, LogTag, subLine);
                Engine.Commands?.Log(subLine);
            }
        }
    }

    // ── Console Commands ────────────────────────────────────────────────

    [Command("DZ_test_subchapters", "Register a test sub-chapter group (5 embedded sub-chapters)")]
    private static void Cmd_RegisterTestGroup()
    {
        RegisterTestGroup();
        Engine.Commands?.Log("Test sub-chapter group 'TestCollabPack' registered.");
        Engine.Commands?.Log("Use 'DZ_test_subchapter_status' to view progress.");
    }

    [Command("DZ_test_subchapter_status", "Show status of all sub-chapter groups")]
    private static void Cmd_Status()
    {
        LogStatus();
    }

    [Command("DZ_test_subchapter_complete", "Mark a test sub-chapter as completed. Usage: DZ_test_subchapter_complete <1-5>")]
    private static void Cmd_Complete(int index = 1)
    {
        if (index < 1 || index > 20)
        {
            Engine.Commands?.Log("Index must be 1–20");
            return;
        }

        string sid = $"DZ/ASide/01_City_SubTest{index:D2}";
        MarkSubChapterCompleted(sid);
        Engine.Commands?.Log($"Marked sub-chapter {index} ({sid}) as completed.");

        var group = GetGroup("TestCollabPack");
        if (group != null)
        {
            int done = GetCompletedCount("TestCollabPack");
            Engine.Commands?.Log($"Progress: {done}/{group.SubChapters.Count} " +
                                 $"(cleared: {IsGroupCleared("TestCollabPack")})");
        }
    }

    [Command("DZ_test_subchapter_reset", "Reset all test sub-chapter completion flags")]
    private static void Cmd_Reset()
    {
        var group = GetGroup("TestCollabPack");
        if (group == null)
        {
            Engine.Commands?.Log("No test group registered. Run 'DZ_test_subchapters' first.");
            return;
        }

        // Note: We can't directly delete achievements, so we log a warning.
        // In production this would clear the save flags.
        Engine.Commands?.Log("Sub-chapter completion flags are stored in save data.");
        Engine.Commands?.Log("To fully reset, use 'DZ_save_repair' or start a new save file.");
    }
}
