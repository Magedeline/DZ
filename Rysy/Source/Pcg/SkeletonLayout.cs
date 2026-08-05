namespace DZ.RysyPlugin.Pcg;

/// <summary>Axis-aligned room rectangle in pixel space, matching Celeste's room grid.</summary>
public readonly record struct SkeletonRect(int Left, int Top, int Width, int Height) {
    public int Right => Left + Width;
    public int Bottom => Top + Height;
}

public sealed class SkeletonSlot {
    public required SkeletonRect Bounds { get; init; }
    public int ParentIndex { get; init; } = -1;
    public List<int> Neighbours { get; } = [];
}

public readonly record struct SkeletonDirection(int Dx, int Dy);

/// <summary>
/// Multi-room skeleton layout: non-overlapping rooms connected edge-to-edge, ported from
/// celeste_skeleton.lua. Produces empty rooms only -- pair with a tile generator
/// (<c>GenerateRoom</c>, MdMC/WFC) to fill them afterward.
/// </summary>
public static class SkeletonLayout {
    private const int Tile = 8;

    private static readonly SkeletonDirection[] Directions = [
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
    ];

    private static int SnapToTile(int px) => (int) Math.Floor(px / (double) Tile) * Tile;

    /// <summary>Interior overlap test (touching edges do not count as overlapping).</summary>
    public static bool InteriorsOverlap(SkeletonRect a, SkeletonRect b)
        => a.Left < b.Right && b.Left < a.Right && a.Top < b.Bottom && b.Top < a.Bottom;

    /// <summary>Whether two rects share a flush edge with at least <paramref name="minWidthTiles"/> of overlap.</summary>
    public static bool HasAdequateExitWidth(SkeletonRect a, SkeletonRect b, int minWidthTiles) {
        var minWidthPx = minWidthTiles * Tile;
        if (a.Bottom == b.Top || b.Bottom == a.Top) {
            var overlapX = Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left);
            return overlapX >= minWidthPx;
        }
        if (a.Right == b.Left || b.Right == a.Left) {
            var overlapY = Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top);
            return overlapY >= minWidthPx;
        }
        return false;
    }

    /// <summary>As <see cref="HasAdequateExitWidth"/> with a fixed 2-tile minimum, for loopback candidates.</summary>
    private static bool ShareExit(SkeletonRect a, SkeletonRect b) => HasAdequateExitWidth(a, b, 2);

    /// <summary>
    /// Grows a connected layout of <paramref name="count"/> non-overlapping rooms, starting
    /// from one room at the origin and repeatedly attaching a new room to a random existing
    /// one in a random direction. A room that fails to place after
    /// <paramref name="maxRetries"/> attempts is skipped, so the result may have fewer than
    /// <paramref name="count"/> rooms.
    /// </summary>
    public static List<SkeletonSlot> BuildLayout(int count, int minW, int maxW, int minH, int maxH,
        int maxRetries, Random rng, int minExitWidthTiles) {
        var slots = new List<SkeletonSlot>();

        var rw = rng.NextRangeInclusive(minW, maxW) * Tile;
        var rh = rng.NextRangeInclusive(minH, maxH) * Tile;
        slots.Add(new SkeletonSlot { Bounds = new SkeletonRect(0, 0, rw, rh) });

        for (var i = 1; i < count; i++) {
            for (var attempt = 0; attempt < maxRetries; attempt++) {
                var parentIdx = rng.Next(slots.Count);
                var parent = slots[parentIdx].Bounds;
                var dir = Directions[rng.Next(Directions.Length)];

                var newW = rng.NextRangeInclusive(minW, maxW) * Tile;
                var newH = rng.NextRangeInclusive(minH, maxH) * Tile;

                int nx, ny;
                if (dir.Dx != 0) {
                    nx = dir.Dx > 0 ? parent.Right : parent.Left - newW;
                    var overlapH = Math.Min(parent.Height, newH);
                    ny = parent.Top + rng.Next(Math.Max(1, overlapH - Tile * 2));
                } else {
                    ny = dir.Dy > 0 ? parent.Bottom : parent.Top - newH;
                    var overlapW = Math.Min(parent.Width, newW);
                    nx = parent.Left + rng.Next(Math.Max(1, overlapW - Tile * 2));
                }

                var candidate = new SkeletonRect(SnapToTile(nx), SnapToTile(ny), newW, newH);

                if (slots.Any(s => InteriorsOverlap(candidate, s.Bounds)))
                    continue;

                var slot = new SkeletonSlot { Bounds = candidate, ParentIndex = parentIdx };
                var idx = slots.Count;
                slots.Add(slot);

                for (var j = 0; j < idx; j++) {
                    // A minExitWidthTiles-adequate edge is a real connection; anything
                    // narrower is just incidental adjacency (see Playability.AirBudget --
                    // Madeline needs real doorway width to pass through).
                    if (!HasAdequateExitWidth(candidate, slots[j].Bounds, minExitWidthTiles))
                        continue;
                    slot.Neighbours.Add(j);
                    slots[j].Neighbours.Add(idx);
                }

                // The parent is always adjacent by construction, even if narrower than the
                // exit-width threshold above -- make sure the graph reflects that.
                if (!slot.Neighbours.Contains(parentIdx)) {
                    slot.Neighbours.Add(parentIdx);
                    slots[parentIdx].Neighbours.Add(idx);
                }
                break;
            }
        }

        return slots;
    }

    private static int ManhattanFromRoot(SkeletonSlot slot, SkeletonSlot root) {
        var acx = slot.Bounds.Left + slot.Bounds.Width / 2;
        var acy = slot.Bounds.Top + slot.Bounds.Height / 2;
        var rcx = root.Bounds.Left + root.Bounds.Width / 2;
        var rcy = root.Bounds.Top + root.Bounds.Height / 2;
        return Math.Abs(acx - rcx) + Math.Abs(acy - rcy);
    }

    /// <summary>Index of the room farthest (Manhattan, room-centre to room-centre) from the first room.</summary>
    public static int PickEndRoom(IReadOnlyList<SkeletonSlot> slots) {
        int endIdx = 0, endDist = 0;
        for (var i = 1; i < slots.Count; i++) {
            var d = ManhattanFromRoot(slots[i], slots[0]);
            if (d > endDist) {
                endDist = d;
                endIdx = i;
            }
        }
        return endIdx;
    }

    /// <summary>BFS from room 0 over the neighbour graph; returns indices never reached.</summary>
    public static List<int> FindUnreachableRooms(IReadOnlyList<SkeletonSlot> slots) {
        if (slots.Count == 0)
            return [];

        var visited = new bool[slots.Count];
        var q = new Queue<int>();
        q.Enqueue(0);
        visited[0] = true;

        while (q.Count > 0) {
            var idx = q.Dequeue();
            foreach (var n in slots[idx].Neighbours) {
                if (visited[n])
                    continue;
                visited[n] = true;
                q.Enqueue(n);
            }
        }

        var unreachable = new List<int>();
        for (var i = 0; i < slots.Count; i++) {
            if (!visited[i])
                unreachable.Add(i);
        }
        return unreachable;
    }

    /// <summary>
    /// Adds up to <paramref name="maxLoops"/> extra edges between rooms that share a flush,
    /// adequately-wide border but are not already connected, making the layout less linear.
    /// </summary>
    public static void AddLoopbackConnections(IReadOnlyList<SkeletonSlot> slots, Random rng, int maxLoops) {
        var attempts = 0;
        var added = 0;
        var i = 1;
        while (i < slots.Count && added < maxLoops && attempts < slots.Count * 2) {
            attempts++;
            var candidates = new List<int>();
            for (var j = 0; j < i; j++) {
                if (!slots[i].Neighbours.Contains(j) && ShareExit(slots[i].Bounds, slots[j].Bounds))
                    candidates.Add(j);
            }
            if (candidates.Count > 0) {
                var target = candidates[rng.Next(candidates.Count)];
                slots[i].Neighbours.Add(target);
                slots[target].Neighbours.Add(i);
                added++;
            }
            i++;
        }
    }
}
