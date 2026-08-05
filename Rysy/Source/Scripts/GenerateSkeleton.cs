using DZ.RysyPlugin.Pcg;
using Rysy;
using Rysy.History;
using Rysy.Scripting;
using Rysy.Shared;

namespace DZ.RysyPlugin.Scripts;

/// <summary>
/// Lays out several new, empty, non-overlapping rooms connected edge-to-edge, ported from
/// celeste_skeleton.lua. The first room gets the player spawn, the room farthest from it
/// (Manhattan distance between room centres) gets a golden berry. Run GenerateRoom on each
/// resulting room afterward to fill its tiles.
///
/// Unlike GenerateRoom this creates rooms rather than editing one, so all the work happens
/// in Prerun (CallRun is off) -- matching how ReplaceEntities.cs, the other script in this
/// codebase whose effect isn't "edit the target room", is structured.
/// </summary>
public sealed class GenerateSkeleton : Script {
    private const string LogTag = "DZ.Pcg";
    private const int Tile = 8;

    public override string Name => "Generate Skeleton";

    public override string? Tooltip =>
        "Lays out several new, non-overlapping rooms connected edge-to-edge. First room = start " +
        "(player spawn), farthest room = end (golden berry). Run Generate Room on each afterward to fill tiles.";

    public override bool CallRun => false;

    public override FieldList? Parameters => new() {
        ["roomCount"] = Fields.Int(12),
        ["minRoomWidthTiles"] = Fields.Int(30),
        ["maxRoomWidthTiles"] = Fields.Int(50),
        ["minRoomHeightTiles"] = Fields.Int(16),
        ["maxRoomHeightTiles"] = Fields.Int(28),
        // -1 picks a fresh seed per run.
        ["seed"] = Fields.Int(-1),
        ["maxRetries"] = Fields.Int(50),
        ["namePrefix"] = Fields.String("lvl_"),
        // Ensures connections between rooms are wide enough for Madeline to pass through
        // (3 tiles = 24px).
        ["minExitWidthTiles"] = Fields.Int(3),
        ["validateConnectivity"] = Fields.Bool(true),
        // Adds extra connections so the layout isn't purely a tree.
        ["ensureLoopbacks"] = Fields.Bool(false),
        // Not in the original Lua: offsets the whole layout clear of the current map's
        // existing rooms instead of always starting at (0,0), where it would otherwise
        // overlap whatever is already there on any map that isn't empty.
        ["avoidExistingRooms"] = Fields.Bool(true),
    };

    public override IHistoryAction? Prerun(ScriptArgs args) {
        var count = Math.Max(2, args.Get<int>("roomCount"));
        var minW = Math.Max(5, args.Get<int>("minRoomWidthTiles"));
        var maxW = Math.Max(minW, args.Get<int>("maxRoomWidthTiles"));
        var minH = Math.Max(5, args.Get<int>("minRoomHeightTiles"));
        var maxH = Math.Max(minH, args.Get<int>("maxRoomHeightTiles"));
        var seed = args.Get<int>("seed");
        var maxRetries = Math.Max(1, args.Get<int>("maxRetries"));
        var prefix = args.Get<string>("namePrefix") is { Length: > 0 } p ? p : "lvl_";
        var minExitWidth = Math.Max(2, args.Get<int>("minExitWidthTiles"));
        var validateConn = args.Get<bool>("validateConnectivity");
        var ensureLoops = args.Get<bool>("ensureLoopbacks");
        var avoidExisting = args.Get<bool>("avoidExistingRooms");

        var map = FindMap(args);
        if (map is null) {
            Logger.Write(LogTag, LogLevel.Error, "No map is open; can't generate a skeleton.");
            return null;
        }

        var rng = seed == -1 ? new Random() : new Random(seed);
        var slots = SkeletonLayout.BuildLayout(count, minW, maxW, minH, maxH, maxRetries, rng, minExitWidth);
        if (slots.Count == 0)
            return null;

        if (ensureLoops)
            SkeletonLayout.AddLoopbackConnections(slots, rng, slots.Count / 3);

        if (validateConn) {
            var unreachable = SkeletonLayout.FindUnreachableRooms(slots);
            if (unreachable.Count > 0)
                Logger.Write(LogTag, LogLevel.Warning, $"Layout has {unreachable.Count} unreachable room(s).");
        }

        var endIdx = SkeletonLayout.PickEndRoom(slots);

        var (offsetX, offsetY) = avoidExisting ? FindClearOffset(map, slots) : (0, 0);
        var usedNames = new HashSet<string>(map.Rooms.Select(r => r.Name), StringComparer.Ordinal);

        var actions = new List<IHistoryAction>();
        for (var i = 0; i < slots.Count; i++) {
            var slot = slots[i];
            var name = UniqueName(prefix, i, usedNames);

            var room = new Room(map, slot.Bounds.Width, slot.Bounds.Height) {
                Name = name,
                X = slot.Bounds.Left + offsetX,
                Y = slot.Bounds.Top + offsetY,
            };

            if (i == 0)
                RoomPlacement.AddEntity(room, "player", new(Tile * 2, slot.Bounds.Height - Tile * 2));
            if (i == endIdx)
                RoomPlacement.AddEntity(room, "goldenBerry",
                    new(slot.Bounds.Width / 2f, slot.Bounds.Height / 2f - Tile * 2));

            actions.Add(new AddRoomAction(room));
        }

        Logger.Write(LogTag, LogLevel.Info,
            $"Skeleton: generated {slots.Count} room(s) (start=room 0, end=room {endIdx}).");

        return actions.MergeActions();
    }

    /// <summary>
    /// Prefers the room's own Map, then the editor's current map -- mirrors
    /// GenerateRoom.FindSiblingRooms, since ScriptArgs.EditorState is never assigned by
    /// ScriptTool.RunScript and reading it throws.
    /// </summary>
    private static Map? FindMap(ScriptArgs args) {
        if (args.Rooms is { Count: > 0 } rooms && rooms[0].Map is { } fromRoom)
            return fromRoom;
        try {
            return EditorState.Current?.Map;
        } catch (Exception) {
            return null;
        }
    }

    /// <summary>
    /// Offset that places the whole new layout to the right of the map's current bounding
    /// box, with a one-room gap, so it doesn't land on top of existing rooms. (0, 0) if the
    /// map has no rooms yet.
    /// </summary>
    private static (int X, int Y) FindClearOffset(Map map, IReadOnlyList<SkeletonSlot> slots) {
        if (map.Rooms.Count == 0)
            return (0, 0);

        var bounds = map.GetBounds();
        var minSlotX = slots.Min(s => s.Bounds.Left);
        return (bounds.Right - minSlotX + Tile * 4, bounds.Top);
    }

    private static string UniqueName(string prefix, int index, HashSet<string> used) {
        var name = $"{prefix}{index}";
        if (used.Add(name))
            return name;
        // Prefix collided with an existing room from a prior skeleton run; disambiguate
        // rather than silently merging into that room's history entry.
        for (var suffix = 1; ; suffix++) {
            var candidate = $"{name}_{suffix}";
            if (used.Add(candidate))
                return candidate;
        }
    }
}
